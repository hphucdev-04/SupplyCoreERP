const fs = require('fs');
const file = 'C:\\\\Users\\\\TSP\\\\.understand-anything\\\\repo\\\\understand-anything-plugin\\\\packages\\\\dashboard\\\\src\\\\components\\\\GraphView.tsx';
const content = fs.readFileSync(file, 'utf8');
const lines = content.split('\n');
lines.forEach((line, i) => {
  if (line.toLowerCase().includes('layer') && line.length < 120) {
    console.log(`${i+1}: ${line}`);
  }
});
