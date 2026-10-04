import {Trie, MinHeap, visitPath, ComboTracker, generateBoard, finalScore, timeMultiplier} from './engine.mjs';
const $ = selector => document.querySelector(selector);
const all = selector => [...document.querySelectorAll(selector)];
function load(key, fallback) { try { return JSON.parse(localStorage.getItem(key)) ?? fallback; } catch { return fallback; } }
function save(key, value) { try { localStorage.setItem(key, JSON.stringify(value)); return true; } catch { return false; } }
let settings = load('lexigrid.preview.settings', {size:4,duration:60});
if (![3,4,5].includes(settings.size)) settings.size = 4;
if (![30,60,90,120].includes(settings.duration)) settings.duration = 60;
const storedScores = load('lexigrid.preview.scores', []);
const leaderboard = new MinHeap();
for (const entry of Array.isArray(storedScores) ? storedScores : []) if (Number.isFinite(entry?.score) && entry.score > 0 && typeof entry.date === 'string') leaderboard.tryInsertIfBetter(entry);
let trie, screen = 'menu', board = [], path = [], score = 0, found = new Set(), gesture = null;
let startTime = 0, deadline = 0, lastWordTime = 0, frame = 0;
const combo = new ComboTracker();
const feedback = $('#current-word'), boardElement = $('#board');
function show(name) {
  for (const element of all('.screen')) element.hidden = element.id !== name;
  screen = name; if (name !== 'game') clearSelection();
  if (name === 'menu') updateBest();
}
function updateBest() {
  const best = leaderboard.sorted()[0]?.score ?? 0;
  $('#personal-best').textContent = best ? `Personal best: ${best.toLocaleString()} points` : 'A fresh board. A fresh start.';
}
function drawSettings() {
  for (const button of all('[data-size]')) button.setAttribute('aria-pressed', String(Number(button.dataset.size) === settings.size));
  for (const button of all('[data-duration]')) button.setAttribute('aria-pressed', String(Number(button.dataset.duration) === settings.duration));
}
function setMessage(message, invalid = false) { feedback.textContent = message; feedback.classList.toggle('invalid', invalid); }
function releasePointer() {
  const pointer = gesture?.id; gesture = null;
  if (pointer !== undefined && boardElement.hasPointerCapture(pointer)) boardElement.releasePointerCapture(pointer);
}
function clearSelection(resetMessage = true) {
  path = []; releasePointer();
  for (const tile of all('.tile')) { tile.classList.remove('selected','invalid'); tile.setAttribute('aria-pressed','false'); }
  $('#path-line polyline').setAttribute('points','');
  if (resetMessage) setMessage('Drag across letters to build a word');
}
function updatePath() {
  const word = path.map(index => board[index]).join(''), valid = trie.startsWith(word);
  setMessage(word || 'Drag across letters to build a word', !!word && !valid);
  for (const tile of all('.tile')) {
    const selected = path.includes(Number(tile.dataset.index));
    tile.classList.toggle('selected',selected); tile.classList.toggle('invalid',selected && !valid); tile.setAttribute('aria-pressed',String(selected));
  }
  const wrap = $('.board-wrap').getBoundingClientRect();
  const points = path.map(index => { const r = boardElement.children[index].getBoundingClientRect(); return `${r.left + r.width / 2 - wrap.left},${r.top + r.height / 2 - wrap.top}`; });
  $('#path-line polyline').setAttribute('points',points.join(' '));
}
function addTile(index, tap = false) {
  if (screen !== 'game' || performance.now() >= deadline || index < 0 || index >= board.length) return;
  path = visitPath(path, index, settings.size, tap); updatePath();
}
function resetCombo() { combo.reset(); $('#combo-status').textContent = 'Find two words under 5 seconds apart for +2s.'; }
function reject(message) { resetCombo(); setMessage(message, true); }
function undo() {
  releasePointer(); path = path.slice(0,-1); updatePath();
}
function submit() {
  if (screen !== 'game') return;
  if (performance.now() >= deadline) { endRound(); return; }
  const word = path.map(index => board[index]).join(''); clearSelection();
  if (!word) return;
  if (word.length < 3) { reject('Use at least 3 letters'); return; }
  if (!trie.search(word)) { reject('Not in the dictionary. Try another path.'); return; }
  if (found.has(word)) { reject('Already found this round'); return; }
  const now = performance.now(), elapsed = (now - lastWordTime) / 1000, points = finalScore(word.length, elapsed);
  const bonus = combo.accept(now / 1000, (deadline - now) / 1000);
  deadline += bonus * 1000; renderTimer(now);
  $('#combo-status').textContent = bonus ? 'Two-word combo! +2 seconds added. Start your next pair.' : '1 of 2 — find another word within 5 seconds for +2s.';
  score += points; found.add(word); lastWordTime = now;
  $('#score').textContent = score.toLocaleString(); $('#word-count').textContent = found.size;
  $('#history-empty').hidden = true;
  const row = document.createElement('li'), wordLabel = document.createElement('strong'), pointsLabel = document.createElement('span');
  wordLabel.textContent = word; pointsLabel.textContent = `+${points.toLocaleString()}`; row.append(wordLabel,pointsLabel); $('#word-history').prepend(row);
  setMessage(`${word}  +${points.toLocaleString()}${bonus ? '  ·  +2s combo!' : timeMultiplier(elapsed) > 1 ? '  ·  Speed bonus!' : ''}`);
}
function renderTimer(now = performance.now()) {
  const remaining = Math.max(0,(deadline - now) / 1000), whole = Math.ceil(remaining);
  $('#timer').textContent = `${String(Math.floor(whole / 60)).padStart(2,'0')}:${String(whole % 60).padStart(2,'0')}`;
  $('#timer').classList.toggle('urgent', remaining < 10);
  $('#timer-fill').style.width = `${Math.min(100,remaining / settings.duration * 100)}%`;
  if (combo.pending !== null && now / 1000 - combo.pending >= 5) resetCombo();
  return remaining;
}
function tick() {
  if (screen !== 'game') return;
  const remaining = renderTimer();
  if (remaining <= 0) { endRound(); return; }
  frame = requestAnimationFrame(tick);
}
function play() {
  if (!trie) return;
  cancelAnimationFrame(frame); clearSelection();
  try { board = generateBoard(settings.size,trie); } catch (error) { $('#load-status').textContent = error.message; show('menu'); return; }
  score = 0; found = new Set(); resetCombo(); $('#score').textContent = '0'; $('#word-count').textContent = '0';
  $('#word-history').replaceChildren(); $('#history-empty').hidden = false;
  boardElement.style.setProperty('--size', settings.size); boardElement.replaceChildren();
  for (let index = 0; index < board.length; index++) {
    const tile = document.createElement('button'); tile.className = 'tile'; tile.textContent = board[index]; tile.dataset.index = index;
    tile.setAttribute('aria-label',`${board[index]}, row ${Math.floor(index/settings.size)+1}, column ${index%settings.size+1}`); tile.setAttribute('aria-pressed','false');
    tile.addEventListener('click', event => { if (event.detail === 0) addTile(index,true); }); boardElement.append(tile);
  }
  show('game'); startTime = lastWordTime = performance.now(); deadline = startTime + settings.duration * 1000; tick();
}
function endRound() {
  if (screen !== 'game') return;
  cancelAnimationFrame(frame); clearSelection();
  const previousBest = leaderboard.sorted()[0]?.score ?? 0;
  if (score > 0) { leaderboard.tryInsertIfBetter({name:'Player',score,date:new Date().toISOString()}); save('lexigrid.preview.scores',leaderboard.items); }
  $('#final-score').textContent = score.toLocaleString(); $('#final-words').textContent = `${found.size} ${found.size === 1 ? 'word' : 'words'} discovered`;
  $('#new-best').hidden = score <= previousBest; show('results');
}
function drawLeaderboard() {
  const ranks = $('#ranks'); ranks.replaceChildren(); const entries = leaderboard.sorted(); $('#ranks-empty').hidden = entries.length > 0;
  entries.forEach((entry,index) => { const row = document.createElement('li'); for (const value of [`#${index+1}`,'Player',entry.score.toLocaleString()]) { const cell = document.createElement('span'); cell.textContent = value; row.append(cell); } ranks.append(row); });
}
document.addEventListener('click', event => {
  const button = event.target.closest('[data-action]'); if (!button) return;
  const action = button.dataset.action;
  if (action === 'play') { play(); return; }
  if (action === 'leave') { endRound(); return; }
  if (screen === 'game') endRound();
  if (action === 'leaderboard') drawLeaderboard(); if (action === 'settings') drawSettings(); show(action);
});
for (const button of all('[data-size]')) button.addEventListener('click',() => { settings.size = Number(button.dataset.size); save('lexigrid.preview.settings',settings); drawSettings(); });
for (const button of all('[data-duration]')) button.addEventListener('click',() => { settings.duration = Number(button.dataset.duration); save('lexigrid.preview.settings',settings); drawSettings(); });
boardElement.addEventListener('pointerdown', event => {
  if (screen !== 'game' || gesture || !event.isPrimary || (event.pointerType === 'mouse' && event.button !== 0)) return;
  const tile = event.target.closest('.tile'); if (!tile) return;
  const index = Number(tile.dataset.index);
  gesture = {id:event.pointerId,index,x:event.clientX,y:event.clientY,selected:path.includes(index),moved:false,last:index};
  if (!gesture.selected) addTile(index,true);
  boardElement.setPointerCapture(event.pointerId); event.preventDefault();
});
boardElement.addEventListener('pointermove', event => {
  if (!gesture || event.pointerId !== gesture.id) return;
  if (event.pointerType === 'mouse' && event.buttons === 0) { clearSelection(); return; }
  if (!gesture.moved && Math.hypot(event.clientX-gesture.x,event.clientY-gesture.y) < 6) return;
  if (!gesture.moved) { gesture.moved = true; if (gesture.selected) addTile(gesture.index); }
  const tile = document.elementFromPoint(event.clientX,event.clientY)?.closest('.tile');
  if (!tile || !boardElement.contains(tile)) return;
  const index = Number(tile.dataset.index);
  if (index !== gesture.last) { gesture.last = index; addTile(index); }
});
window.addEventListener('pointerup', event => {
  if (!gesture || event.pointerId !== gesture.id) return;
  const finished = gesture; releasePointer();
  if (finished.moved) submit();
  else if (finished.selected) addTile(finished.index,true);
});
boardElement.addEventListener('pointercancel',event => { if (gesture?.id === event.pointerId) clearSelection(); });
boardElement.addEventListener('lostpointercapture',event => { if (gesture?.id === event.pointerId) clearSelection(); });
window.addEventListener('blur',() => { if (gesture) clearSelection(); });
document.addEventListener('visibilitychange',() => { if (document.hidden && gesture) clearSelection(); });
$('#submit-word').addEventListener('click',submit); $('#clear-word').addEventListener('click',() => clearSelection());
$('#undo-word').addEventListener('click',undo);
document.addEventListener('keydown',event => {
  if (screen !== 'game') return;
  if (event.key === 'Escape') clearSelection();
  if (event.key === 'Backspace') { event.preventDefault(); undo(); }
  if (event.key === 'Enter' && (event.target.closest('.tile') || event.target === document.body)) { event.preventDefault(); submit(); }
});
window.addEventListener('resize',() => { if (path.length) updatePath(); });
const demo = $('.demo-grid');
for (const [index,letter] of [...'LEXIGRIDWORDPLAY'].entries()) { const element = document.createElement('span'); element.textContent = letter; if (index < 4) element.className = 'accent'; demo.append(element); }
updateBest(); drawSettings();
const worker = new Worker('./dictionary-worker.mjs',{type:'module'});
worker.onmessage = ({data}) => {
  if (data.type === 'progress') { $('#loading-bar').style.width = `${Math.round(data.value * 100)}%`; $('#load-status').textContent = `Preparing dictionary… ${Math.round(data.value * 100)}%`; }
  if (data.type === 'ready') {
    trie = Trie.unpack(data.data); $('#loading-bar').style.width = '100%'; $('#play-button').disabled = false;
    $('#play-button').replaceChildren(document.createTextNode('Play a round')); const arrow = document.createElement('span'); arrow.textContent = '↗'; $('#play-button').append(arrow);
    $('#load-status').textContent = 'Ready when you are. Your settings are remembered.';
    $('#dictionary-status').textContent = `${trie.wordCount.toLocaleString()} dictionary words · On your device`; worker.terminate();
  }
  if (data.type === 'error') { $('#load-status').textContent = data.message; $('#load-status').setAttribute('role','alert'); worker.terminate(); }
};
worker.onerror = () => { $('#load-status').textContent = 'Dictionary loading failed. Reload this page to retry.'; $('#load-status').setAttribute('role','alert'); };
worker.postMessage('load');
