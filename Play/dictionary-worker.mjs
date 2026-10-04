import {Trie} from './engine.mjs';
self.onmessage = async () => {
  try {
    const response = await fetch('/wordlist.txt');
    if (!response.ok) throw new Error('Word list could not be loaded');
    const lines = (await response.text()).split(/[\r\n]+/), trie = new Trie();
    for (let i = 0; i < lines.length; i++) {
      const word = lines[i].trim();
      if (word.length >= 3 && /^[a-z]+$/i.test(word)) trie.insert(word);
      if (i % 10000 === 0) self.postMessage({type:'progress', value:i / lines.length});
    }
    const data = trie.pack();
    self.postMessage({type:'ready', data}, [data.first.buffer, data.next.buffer, data.code.buffer, data.end.buffer]);
  } catch (error) { self.postMessage({type:'error', message:error.message}); }
};
