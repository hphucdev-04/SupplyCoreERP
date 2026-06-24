const fs = require('fs');
const file = 'C:\\\\Users\\\\TSP\\\\.understand-anything\\\\repo\\\\understand-anything-plugin\\\\packages\\\\core\\\\src\\\\types.ts';
const content = fs.readFileSync(file, 'utf8');
const lines = content.split('\n');
lines.forEach((line, i) => {
  if (line.includes('interface Layer') || (i > 0 && lines[i-1].includes('interface Layer')) || (i > 1 && lines[i-2].includes('interface Layer')) || (i > 2 && lines[i-3].includes('interface Layer'))) {
    console.log(`${i+1}: ${line}`);
  }
});
