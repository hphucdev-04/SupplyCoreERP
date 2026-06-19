const fs = require('fs');
const file = 'C:\\\\Users\\\\TSP\\\\.understand-anything\\\\repo\\\\understand-anything-plugin\\\\packages\\\\dashboard\\\\src\\\\App.tsx';
const content = fs.readFileSync(file, 'utf8');
const lines = content.split('\n');
for (let i = 260; i < 320; i++) {
  console.log(`${i+1}: ${lines[i]}`);
}
