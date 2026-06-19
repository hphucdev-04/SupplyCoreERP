const fs = require('fs');
const graph = JSON.parse(fs.readFileSync('D:\\\\ProjectOwner\\\\SupplyCoreERP\\\\.understand-anything\\\\knowledge-graph.json', 'utf8'));

const files = [
  'src/SupplyCoreERP.Mcp.Client/AgentProviders/GeminiProvider.cs',
  'src/SupplyCoreERP.Mcp.Client/Mcp/McpClientService.cs'
];

files.forEach(targetFile => {
  const node = graph.nodes.find(n => n.filePath === targetFile);
  console.log(`\n========================================`);
  console.log(`Target Node for: ${targetFile}`);
  console.log(`========================================`);
  console.log(JSON.stringify(node, null, 2));

  if (node) {
    const incomingEdges = graph.edges.filter(e => e.target === node.id);
    const outgoingEdges = graph.edges.filter(e => e.source === node.id);

    console.log('\nIncoming Edges:');
    console.log(JSON.stringify(incomingEdges, null, 2));

    console.log('\nOutgoing Edges:');
    console.log(JSON.stringify(outgoingEdges, null, 2));

    // Find the layer
    const layer = graph.layers.find(l => l.nodeIds.includes(node.id));
    console.log('\nLayer:');
    console.log(JSON.stringify(layer, null, 2));
  }
});
