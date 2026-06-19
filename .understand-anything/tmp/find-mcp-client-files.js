const fs = require('fs');
const path = require('path');

function searchDir(dir) {
  const files = fs.readdirSync(dir);
  for (const file of files) {
    const fullPath = path.join(dir, file);
    const stat = fs.statSync(fullPath);
    if (stat.isDirectory()) {
      if (file !== 'node_modules' && file !== '.git' && file !== 'bin' && file !== 'obj') {
        searchDir(fullPath);
      }
    } else {
      if (file.toLowerCase().includes('mcp')) {
        console.log(`Found file: ${fullPath}`);
      }
    }
  }
}

searchDir('D:\\\\ProjectOwner\\\\SupplyCoreERP');
