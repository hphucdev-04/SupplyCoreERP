const fs = require('fs');
const graph = JSON.parse(fs.readFileSync('D:\\\\ProjectOwner\\\\SupplyCoreERP\\\\.understand-anything\\\\knowledge-graph.json', 'utf8'));
const matchingNodes = graph.nodes.filter(n => n.id.toLowerCase().includes('mcpclientservice'));
console.log('Nodes matching McpClientService:');
console.log(JSON.stringify(matchingNodes, null, 2));

const containingFile = graph.nodes.filter(n => n.id.toLowerCase().includes('mcp/mcpclientservice.cs'));
console.log('File node matching:');
console.log(JSON.stringify(containingFile, null, 2));
