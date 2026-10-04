import http from 'node:http';
import {createReadStream} from 'node:fs';
import {stat} from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import path from 'node:path';
const root = path.dirname(fileURLToPath(import.meta.url));
const port = Number(process.argv[2] ?? 8765);
const files = new Map([
  ['/', ['index.html','text/html; charset=utf-8']],
  ['/index.html',['index.html','text/html; charset=utf-8']],
  ['/style.css',['style.css','text/css; charset=utf-8']],
  ['/game.mjs',['game.mjs','text/javascript; charset=utf-8']],
  ['/engine.mjs',['engine.mjs','text/javascript; charset=utf-8']],
  ['/dictionary-worker.mjs',['dictionary-worker.mjs','text/javascript; charset=utf-8']],
  ['/wordlist.txt', ['../Assets/Resources/wordlist.txt','text/plain; charset=utf-8']]
]);
const server = http.createServer(async (req, res) => {
  const url = new URL(req.url, 'http://127.0.0.1');
  if (url.pathname === '/health') { res.writeHead(200, {'Content-Type':'application/json'}); res.end(JSON.stringify({app:'LexiGrid preview',version:1})); return; }
  if (req.method !== 'GET' && req.method !== 'HEAD') { res.writeHead(405); res.end(); return; }
  const route = files.get(url.pathname);
  if (!route) { res.writeHead(404); res.end('Not found'); return; }
  const file = path.resolve(root, route[0]);
  try {
    const info = await stat(file);
    res.writeHead(200, {'Content-Type':route[1], 'Content-Length':info.size, 'Cache-Control':'no-cache','X-Content-Type-Options':'nosniff','Content-Security-Policy':"default-src 'self'; script-src 'self'; style-src 'self'; worker-src 'self'; connect-src 'self'; img-src 'self' data:; frame-ancestors 'self'"});
    if (req.method === 'HEAD') res.end(); else createReadStream(file).pipe(res);
  } catch { res.writeHead(500); res.end('A required project file is missing'); }
});
server.on('error', error => { console.error(error.message); process.exit(1); });
server.listen(port, '127.0.0.1', () => console.log(`LexiGrid: http://127.0.0.1:${port}`));
