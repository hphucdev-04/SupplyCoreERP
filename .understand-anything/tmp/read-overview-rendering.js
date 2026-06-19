const fs = require('fs');
const file = 'C:\\\\Users\\\\TSP\\\\.understand-anything\\\\repo\\\\understand-anything-plugin\\\\packages\\\\dashboard\\\\src\\\\components\\\\GraphView.tsx';
const content = fs.readFileSync(file, 'utf8');
const lines = content.split('\n');
for (let i = 205; i < 295; i++) {
  console.log(`${i+1}: ${lines[i]}`);
}
