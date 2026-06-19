// Wraps the marked-rendered HTML fragment in a print-styled HTML document.
const fs = require('fs');
const dir = '/Users/manuel/Documents/PROJECTS/RadioSignalsAnalysis';
const body = fs.readFileSync(dir + '/elaborat.content.html', 'utf8');

const css = `
@page { size: A4; margin: 20mm 18mm; }
* { box-sizing: border-box; }
body { font-family: -apple-system, "Segoe UI", "Helvetica Neue", Arial, sans-serif;
       font-size: 11pt; line-height: 1.5; color: #1a1a1a; }
h1 { font-size: 21pt; margin: 0.2em 0 0.4em; }
h2 { font-size: 15pt; border-bottom: 1px solid #ddd; padding-bottom: 4px; margin-top: 1.5em; }
h3 { font-size: 12.5pt; margin-top: 1.1em; }
p, li { orphans: 2; widows: 2; }
code { font-family: "SF Mono", Menlo, Consolas, monospace; font-size: 9.5pt;
       background: #f4f4f4; padding: 1px 4px; border-radius: 3px; }
pre { background: #f6f8fa; padding: 10px 12px; border-radius: 6px; overflow-x: auto;
      page-break-inside: avoid; border: 1px solid #e5e5e5; }
pre code { background: none; padding: 0; font-size: 8.7pt; line-height: 1.4; white-space: pre-wrap; }
table { border-collapse: collapse; width: 100%; font-size: 10pt; margin: 0.6em 0; page-break-inside: avoid; }
th, td { border: 1px solid #ccc; padding: 5px 8px; text-align: left; vertical-align: top; }
th { background: #f0f0f0; }
blockquote { border-left: 4px solid #c8c8c8; margin: 0.8em 0; padding: 2px 14px;
             color: #555; background: #fafafa; }
a { color: #0b5cad; text-decoration: none; word-break: break-word; }
img { max-width: 100%; }
hr { border: none; border-top: 1px solid #ddd; margin: 1.4em 0; }
`;

const html = `<!DOCTYPE html><html lang="mk"><head><meta charset="utf-8"><title>RadioSignals — Елаборат</title><style>${css}</style></head><body>${body}</body></html>`;
fs.writeFileSync(dir + '/elaborat.html', html);
console.log('wrote elaborat.html (' + html.length + ' bytes)');
