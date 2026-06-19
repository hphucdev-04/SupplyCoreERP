const fs = require('fs');
const file = 'C:\\\\Users\\\\TSP\\\\.understand-anything\\\\repo\\\\understand-anything-plugin\\\\packages\\\\dashboard\\\\src\\\\store.ts';
const content = fs.readFileSync(file, 'utf8');
const lines = content.split('\n');
lines.forEach((line, i) => {
  if (line.toLowerCase().includes('layer')) {
    console.log(`${i+1}: ${line}`);
  }
});
