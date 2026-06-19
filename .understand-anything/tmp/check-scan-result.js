const fs = require('fs');
const scan = JSON.parse(fs.readFileSync('D:\\\\ProjectOwner\\\\SupplyCoreERP\\\\.understand-anything\\\\intermediate\\\\scan-result.json', 'utf8'));
const matchingFiles = scan.files.filter(f => f.path.toLowerCase().includes('mcpclientservice.cs'));
console.log('Files in scan-result.json matching McpClientService.cs:');
console.log(JSON.stringify(matchingFiles, null, 2));
