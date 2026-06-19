const fs = require('fs');
const file = 'C:\\\\Users\\\\TSP\\\\.understand-anything\\\\repo\\\\understand-anything-plugin\\\\packages\\\\core\\\\src\\\\types.ts';
const content = fs.readFileSync(file, 'utf8');
const lines = content.split('\n');
for (let i = 59; i < 79; i++) {
  console.log(`${i+1}: ${lines[i]}`);
}
