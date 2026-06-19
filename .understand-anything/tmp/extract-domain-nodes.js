const fs = require('fs');
const path = require('path');

const projectRoot = "D:\\ProjectOwner\\SupplyCoreERP";
const graphPath = path.join(projectRoot, '.understand-anything', 'knowledge-graph.json');
const outPath = path.join(projectRoot, '.understand-anything', 'tmp', 'domain-graph-context.json');

try {
  console.log("Loading knowledge-graph.json...");
  const graph = JSON.parse(fs.readFileSync(graphPath, 'utf8'));

  // Keep only file-level nodes (or abstract/infra nodes)
  const fileTypes = new Set(['file', 'config', 'document', 'service', 'pipeline', 'table', 'schema', 'resource', 'endpoint']);
  const filteredNodes = graph.nodes
    .filter(n => fileTypes.has(n.type))
    .map(n => ({
      id: n.id,
      type: n.type,
      name: n.name,
      summary: n.summary,
      tags: n.tags,
      filePath: n.filePath
    }));

  // Keep only relevant edges
  const edgeTypesToKeep = new Set(['calls', 'inherits', 'implements', 'configures', 'deploys', 'tested_by', 'related', 'imports']);
  const filteredEdges = graph.edges
    .filter(e => {
      // Both endpoints must exist in our filtered list
      const sourceExists = filteredNodes.some(n => n.id === e.source);
      const targetExists = filteredNodes.some(n => n.id === e.target);
      return sourceExists && targetExists && edgeTypesToKeep.has(e.type);
    })
    .map(e => ({
      source: e.source,
      target: e.target,
      type: e.type
    }));

  const context = {
    project: graph.project,
    nodes: filteredNodes,
    edges: filteredEdges,
    layers: graph.layers.map(l => ({
      id: l.id,
      name: l.name,
      description: l.description,
      nodeIdsCount: l.nodeIds.length
    })),
    tour: graph.tour
  };

  fs.writeFileSync(outPath, JSON.stringify(context, null, 2));
  console.log(`SUCCESS: Wrote domain context to ${outPath}`);
  console.log(`- Nodes: ${filteredNodes.length}`);
  console.log(`- Edges: ${filteredEdges.length}`);
} catch (e) {
  console.error("Error extracting domain context:", e);
  process.exit(1);
}
