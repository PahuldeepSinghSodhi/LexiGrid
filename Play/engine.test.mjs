import test from 'node:test';
import assert from 'node:assert/strict';
import {readFile} from 'node:fs/promises';
import {Trie,MinHeap,baseScore,timeMultiplier,finalScore,adjacent,generateBoard,findWords,visitPath,ComboTracker} from './engine.mjs';

test('Tap selected tail removes it, and tapping an earlier tile removes its suffix',() => {
  assert.deepEqual(visitPath([0,1,2],2,4,true),[0,1]);
  assert.deepEqual(visitPath([0,1,2],1,4,true),[0]);
  assert.deepEqual(visitPath([0,1,2],0,4,true),[]);
});
test('Retracing removes multiple letters, preserves anchor and allows corrected continuation',() => {
  let path=visitPath([0,1,2,6,5],2,4); assert.deepEqual(path,[0,1,2]);
  path=visitPath(path,1,4); assert.deepEqual(path,[0,1]);
  assert.deepEqual(visitPath(path,5,4),[0,1,5]);
  assert.deepEqual(visitPath(path,1,4),path);
});
test('Disconnected tap starts fresh; disconnected drag and invalid indices do not corrupt path',() => {
  assert.deepEqual(visitPath([0,1],15,4,true),[15]);
  assert.deepEqual(visitPath([0,1],15,4),[0,1]);
  for (const index of [-1,16,NaN,1.5]) assert.deepEqual(visitPath([0,1],index,4,true),[0,1]);
});
test('Each non-overlapping pair of quick valid words earns exactly two seconds',() => {
  const combo=new ComboTracker(); assert.deepEqual([10,14.9,15,16].map(t=>combo.accept(t,30)),[0,2,0,2]);
});
test('Five-second boundary restarts pairing and reset breaks invalid or duplicate streaks',() => {
  const combo=new ComboTracker(); combo.accept(0,30); assert.equal(combo.accept(5,25),0);
  assert.equal(combo.accept(9.99,20),2); combo.accept(10,20); combo.reset(); assert.equal(combo.accept(11,19),0);
});
test('Expired round cannot be revived and a new round cannot inherit a pending combo',() => {
  const combo=new ComboTracker(); combo.accept(0,1); assert.equal(combo.accept(.5,0),0);
  assert.equal(combo.accept(1,-1),0); combo.reset(); assert.equal(combo.accept(2,30),0);
});

test('Trie normalizes case, shares prefixes, and ignores duplicate words',() => {
  const trie = new Trie(4); trie.insert('cat'); trie.insert('CARD'); trie.insert('car'); trie.insert('CAT');
  assert.equal(trie.wordCount,3); assert.ok(trie.search('CaT')); assert.ok(trie.startsWith('CA'));
  assert.equal(trie.search('ca'),false); assert.equal(trie.startsWith('zz'),false); assert.equal(trie.search(''),false);
});
test('Transferred Trie representation preserves exact and prefix lookups',() => {
  const original = new Trie(); original.insert('hello'); const copy = Trie.unpack(structuredClone(original.pack()));
  assert.ok(copy.search('HELLO')); assert.ok(copy.startsWith('hel')); assert.equal(copy.wordCount,1);
});
test('Base scores follow original C# scoring bands',() => {
  assert.deepEqual([-1,0,2,3,4,5,6,7,10].map(baseScore),[0,0,0,100,400,800,1200,2000,2000]);
});
test('Speed boundaries are strict and final scores are integers',() => {
  assert.deepEqual([4.99,5,9.99,10].map(timeMultiplier),[1.5,1.25,1.25,1]);
  assert.equal(finalScore(3,2),150); assert.equal(finalScore(5,5),1000); assert.equal(finalScore(2,0),0);
});
test('MinHeap keeps the best ten and rejects ties at capacity',() => {
  const heap = new MinHeap(); for (let score=1;score<=30;score++) heap.tryInsertIfBetter({score});
  assert.equal(heap.items.length,10); assert.equal(heap.peek().score,21); assert.equal(heap.tryInsertIfBetter({score:21}),false);
  assert.deepEqual(heap.sorted().map(e=>e.score),[30,29,28,27,26,25,24,23,22,21]);
});
test('MinHeap extraction works for a single item and empty heap',() => {
  const heap = new MinHeap([{score:4}]); assert.equal(heap.extractMin().score,4); assert.throws(()=>heap.extractMin());
  assert.equal(heap.tryInsertIfBetter({score:1},0),false);
});
test('Adjacency allows diagonals and forbids wraparound or same tile',() => {
  assert.ok(adjacent(0,5,4)); assert.equal(adjacent(3,4,4),false); assert.equal(adjacent(0,0,4),false); assert.equal(adjacent(0,15,4),false);
});
test('Board solver finds words without reusing tiles',() => {
  const trie=new Trie(); trie.insert('cat'); trie.insert('tact');
  const words=findWords(['C','A','X','T'],2,trie,100);
  assert.ok(words.has('CAT')); assert.equal(words.has('TACT'),false);
  assert.equal(new Set(words.get('CAT')).size,3);
});
test('Generation has a guaranteed fallback after exhausted attempts',() => {
  const trie = new Trie(); trie.insert('CAT');
  for (const size of [3,4,5]) { const board = generateBoard(size,trie,()=>0,0); assert.equal(board.length,size*size); assert.ok(findWords(board,size,trie).has('CAT')); }
});
test('Generation rejects empty or incompatible dictionary',() => {
  assert.throws(()=>generateBoard(3,new Trie())); const trie = new Trie(); trie.insert('abcdefghijklmnop');
  assert.throws(()=>generateBoard(3,trie,()=>0,0));
});
test('Full project dictionary loads and all three board sizes are playable',async () => {
  const words=(await readFile(new URL('../Assets/Resources/wordlist.txt',import.meta.url),'utf8')).split(/[\r\n]+/);
  const trie = new Trie(); for (const word of words) if (word.length>=3 && /^[a-z]+$/i.test(word)) trie.insert(word);
  assert.equal(trie.wordCount,358612); assert.ok(trie.search('cat')); assert.ok(trie.startsWith('dic'));
  for (const size of [3,4,5]) for (let attempt=0;attempt<5;attempt++) assert.ok(findWords(generateBoard(size,trie),size,trie).size>0);
});
