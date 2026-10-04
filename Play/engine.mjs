// Browser preview of the original C# rules. No Unity runtime or cloud service is required.
export class Trie {
  constructor(capacity = 262144) {
    this.first = new Uint32Array(capacity);
    this.next = new Uint32Array(capacity);
    this.code = new Uint8Array(capacity);
    this.end = new Uint8Array(capacity);
    this.nodes = 1; this.wordCount = 0;
  }
  static unpack(data) { const trie = Object.create(Trie.prototype); Object.assign(trie, data); return trie; }
  pack() { return {first: this.first, next: this.next, code: this.code, end: this.end, nodes: this.nodes, wordCount: this.wordCount}; }
  grow() {
    for (const key of ['first', 'next', 'code', 'end']) {
      const old = this[key], replacement = new old.constructor(old.length * 2);
      replacement.set(old); this[key] = replacement;
    }
  }
  step(node, letter) {
    const code = typeof letter === 'number' ? letter : letter.toLowerCase().charCodeAt(0);
    for (let child = this.first[node]; child; child = this.next[child]) if (this.code[child] === code) return child;
    return 0;
  }
  insert(word) {
    if (!word) return;
    let node = 0;
    for (const letter of word.toLowerCase()) {
      let child = this.step(node, letter);
      if (!child) {
        if (this.nodes === this.first.length) this.grow();
        child = this.nodes++; this.code[child] = letter.charCodeAt(0);
        this.next[child] = this.first[node]; this.first[node] = child;
      }
      node = child;
    }
    if (!this.end[node]) { this.end[node] = 1; this.wordCount++; }
  }
  lookup(word) {
    if (!word) return -1;
    let node = 0;
    for (const letter of word.toLowerCase()) { node = this.step(node, letter); if (!node) return -1; }
    return node;
  }
  search(word) { const node = this.lookup(word); return node >= 0 && this.end[node] === 1; }
  startsWith(word) { return this.lookup(word) >= 0; }
}

export class MinHeap {
  constructor(items = []) { this.items = []; for (const item of items) this.insert(item); }
  peek() { return this.items[0]; }
  insert(item) {
    const a = this.items; a.push(item); let i = a.length - 1;
    while (i > 0) { const p = (i - 1) >> 1; if (a[p].score <= a[i].score) break; [a[p], a[i]] = [a[i], a[p]]; i = p; }
  }
  extractMin() {
    const a = this.items; if (!a.length) throw new Error('Heap is empty');
    const first = a[0], last = a.pop(); if (!a.length) return first;
    a[0] = last; let i = 0;
    for (;;) {
      let small = i; const l = i * 2 + 1, r = l + 1;
      if (l < a.length && a[l].score < a[small].score) small = l;
      if (r < a.length && a[r].score < a[small].score) small = r;
      if (small === i) break;
      [a[small], a[i]] = [a[i], a[small]]; i = small;
    }
    return first;
  }
  tryInsertIfBetter(item, maximum = 10) {
    if (maximum <= 0) return false;
    if (this.items.length < maximum) { this.insert(item); return true; }
    if (item.score <= this.peek().score) return false;
    this.extractMin(); this.insert(item); return true;
  }
  sorted() { return [...this.items].sort((a, b) => b.score - a.score); }
}
export function baseScore(length) { return length < 3 ? 0 : ({3:100,4:400,5:800,6:1200}[length] ?? 2000); }
export function timeMultiplier(seconds) { return seconds < 5 ? 1.5 : seconds < 10 ? 1.25 : 1; }
export function finalScore(length, seconds) { return Math.trunc(baseScore(length) * timeMultiplier(seconds)); }
export function adjacent(a, b, size) {
  const dr = Math.abs(Math.floor(a / size) - Math.floor(b / size)), dc = Math.abs(a % size - b % size);
  return dr <= 1 && dc <= 1 && a !== b;
}
// Tap removes the touched tile and its suffix; retracing keeps the touched anchor.
export function visitPath(path, index, size, tap = false) {
  if (!Number.isInteger(index) || index < 0 || index >= size * size) return path;
  const position = path.indexOf(index);
  if (position >= 0) return path.slice(0, position + (tap ? 0 : 1));
  if (path.length && !adjacent(path.at(-1), index, size)) return tap ? [index] : path;
  return [...path, index];
}
export class ComboTracker {
  constructor() { this.reset(); }
  reset() { this.pending = null; }
  accept(now, remaining) {
    if (remaining <= 0) { this.reset(); return 0; }
    if (this.pending !== null && now >= this.pending && now - this.pending < 5) {
      this.reset(); return 2;
    }
    this.pending = now; return 0;
  }
}
const frequencies = [['E',12.7],['T',9.1],['A',8.2],['O',7.5],['I',7],['N',6.7],['S',6.3],['H',6.1],['R',6],['D',4.3],['L',4],['C',2.8],['U',2.8],['M',2.4],['W',2.4],['F',2.2],['G',2],['Y',2],['P',1.9],['B',1.5],['V',1],['K',.8],['J',.15],['X',.15],['Q',.1],['Z',.07]];
const totalWeight = frequencies.reduce((sum, entry) => sum + entry[1], 0);
export function randomLetter(random = Math.random) {
  let value = random() * totalWeight;
  for (const [letter, weight] of frequencies) { value -= weight; if (value <= 0) return letter; }
  return 'Z';
}
export function findWords(board, size, trie, limit = 1) {
  const answers = new Map(), visited = new Uint8Array(board.length), neighbours = board.map((_, i) => board.map((_, j) => j).filter(j => adjacent(i, j, size)));
  function visit(index, node, word, path) {
    if (answers.size >= limit) return;
    const child = trie.step(node, board[index]); if (!child) return;
    visited[index] = 1; const value = word + board[index], route = [...path, index];
    if (value.length >= 3 && trie.end[child] && !answers.has(value)) answers.set(value, route);
    if (answers.size < limit) for (const next of neighbours[index]) if (!visited[next]) visit(next, child, value, route);
    visited[index] = 0;
  }
  for (let i = 0; i < board.length && answers.size < limit; i++) visit(i, 0, '', []);
  return answers;
}
export function generateBoard(size, trie, random = Math.random, maxAttempts = 100) {
  if (!trie.wordCount) throw new Error('The dictionary is empty');
  for (let i = 0; i < maxAttempts; i++) {
    const board = Array.from({length: size * size}, () => randomLetter(random));
    if (findWords(board, size, trie).size) return board;
  }
  // Mirror the improved C# generator's guaranteed word placement after retry exhaustion.
  const board = Array.from({length: size * size}, () => randomLetter(random));
  let selected = '';
  function find(node, prefix) {
    if (prefix.length >= 3 && trie.end[node]) { selected = prefix; return true; }
    if (prefix.length >= board.length) return false;
    for (let child = trie.first[node]; child; child = trie.next[child]) if (find(child, prefix + String.fromCharCode(trie.code[child]))) return true;
    return false;
  }
  if (!find(0, '')) throw new Error('No dictionary word fits this board');
  for (let i = 0; i < selected.length; i++) {
    const row = Math.floor(i / size), col = row % 2 ? size - 1 - i % size : i % size;
    board[row * size + col] = selected[i].toUpperCase();
  }
  return board;
}
