const fs = require('fs');
const path = require('path');

const scanFilesPath = path.join(__dirname, 'ua-scan-files.json');
const importMapInputPath = path.join(__dirname, 'ua-import-map-input.json');

try {
  const scanData = JSON.parse(fs.readFileSync(scanFilesPath, 'utf8'));
  const input = {
    projectRoot: path.resolve(__dirname, '..', '..'),
    files: scanData.files
  };
  fs.writeFileSync(importMapInputPath, JSON.stringify(input, null, 2), 'utf8');
  console.log('Successfully wrote ua-import-map-input.json');
} catch (error) {
  console.error('Error generating input json:', error);
  process.exit(1);
}
