const fs = require('fs');
const file = 'C:\\\\Users\\\\TSP\\\\.understand-anything\\\\repo\\\\understand-anything-plugin\\\\packages\\\\dashboard\\\\src\\\\components\\\\GraphView.tsx';
const content = fs.readFileSync(file, 'utf8');
const lines = content.split('\n');
// Let's print around line 1250-1380
for (let i = 1250; i < 1380; i++) {
  if (lines[i]) {
    console.log(`${i+1}: ${lines[i]}`);
  }
}
