const fs = require('fs');
const path = require('path');

const inputPath = process.argv[2];
const outputPath = process.argv[3];

if (!inputPath || !outputPath) {
  console.error("Usage: node ua-tour-analyze.js <inputPath> <outputPath>");
  process.exit(1);
}

try {
  const data = JSON.parse(fs.readFileSync(inputPath, 'utf8'));
  const nodes = data.nodes || [];
  const edges = data.edges || [];
  const layers = data.layers || [];

  // Compute Fan-In and Fan-Out
  const fanInMap = {};
  const fanOutMap = {};

  nodes.forEach(n => {
    fanInMap[n.id] = 0;
    fanOutMap[n.id] = 0;
  });

  edges.forEach(e => {
    if (fanInMap[e.target] !== undefined) fanInMap[e.target]++;
    if (fanOutMap[e.source] !== undefined) fanOutMap[e.source]++;
  });

  const fanIn = Object.keys(fanInMap).map(id => ({ id, count: fanInMap[id] }))
    .sort((a, b) => b.count - a.count)
    .slice(0, 20);

  const fanOut = Object.keys(fanOutMap).map(id => ({ id, count: fanOutMap[id] }))
    .sort((a, b) => b.count - a.count)
    .slice(0, 20);

  // Entry Point Candidates
  // main, index, server, Program.cs, or README.md
  const entryPointKeywords = ['main', 'index', 'server', 'program', 'readme', 'app.module', 'app-routing'];
  const entryPoints = nodes.filter(n => {
    const nameLower = n.name ? n.name.toLowerCase() : '';
    const filePathLower = n.filePath ? n.filePath.toLowerCase() : '';
    return entryPointKeywords.some(kw => nameLower.includes(kw) || filePathLower.includes(kw));
  }).map(n => ({ id: n.id, name: n.name || '', type: n.type || '', filePath: n.filePath || '' }));

  // Find top code entry point candidate
  let topCodeEntryPoint = null;
  const preferredCode = ['program.cs', 'main.ts', 'server.ts', 'index.ts', 'index.js'];
  for (const pref of preferredCode) {
    const found = entryPoints.find(ep => ep.filePath && ep.filePath.toLowerCase().endsWith(pref));
    if (found) {
      topCodeEntryPoint = found.id;
      break;
    }
  }
  if (!topCodeEntryPoint) {
    const codeEP = entryPoints.find(ep => ep.type === 'file' && ep.filePath && !ep.filePath.toLowerCase().endsWith('.md') && !ep.filePath.toLowerCase().endsWith('.json'));
    if (codeEP) topCodeEntryPoint = codeEP.id;
  }
  if (!topCodeEntryPoint && nodes.length > 0) {
    topCodeEntryPoint = nodes[0].id;
  }

  // Dependency Chains (BFS) from topCodeEntryPoint
  const dependencyChains = [];
  if (topCodeEntryPoint) {
    const adj = {};
    nodes.forEach(n => { adj[n.id] = []; });
    edges.forEach(e => {
      if ((e.type === 'imports' || e.type === 'calls') && adj[e.source]) {
        adj[e.source].push(e.target);
      }
    });

    const visited = new Set();
    const queue = [{ id: topCodeEntryPoint, depth: 0, path: [topCodeEntryPoint] }];
    visited.add(topCodeEntryPoint);

    while (queue.length > 0 && dependencyChains.length < 50) {
      const curr = queue.shift();
      dependencyChains.push(curr);

      const neighbors = adj[curr.id] || [];
      for (const nbr of neighbors) {
        if (!visited.has(nbr)) {
          visited.add(nbr);
          queue.push({
            id: nbr,
            depth: curr.depth + 1,
            path: [...curr.path, nbr]
          });
        }
      }
    }
  }

  // Non-Code File Inventory
  const nonCodeFiles = {
    documentation: [],
    infrastructure: [],
    data: [],
    config: []
  };

  nodes.forEach(n => {
    const fp = n.filePath ? n.filePath.toLowerCase() : '';
    if (n.type !== 'file') {
      if (n.type === 'document' || fp.endsWith('.md')) {
        nonCodeFiles.documentation.push({ id: n.id, name: n.name || '', filePath: n.filePath || '' });
      } else if (n.type === 'service' || fp.includes('docker') || fp.includes('dockerfile')) {
        nonCodeFiles.infrastructure.push({ id: n.id, name: n.name || '', filePath: n.filePath || '' });
      } else if (n.type === 'config' || fp.endsWith('.json') || fp.endsWith('.yaml') || fp.endsWith('.yml')) {
        nonCodeFiles.config.push({ id: n.id, name: n.name || '', filePath: n.filePath || '' });
      }
    } else {
      if (fp.endsWith('.md') || fp.includes('/docs/') || fp.includes('\\docs\\')) {
        nonCodeFiles.documentation.push({ id: n.id, name: n.name || '', filePath: n.filePath || '' });
      } else if (fp.includes('docker') || fp.includes('k8s') || fp.includes('deploy') || fp.includes('nginx') || fp.endsWith('.sh') || fp.endsWith('.ps1')) {
        nonCodeFiles.infrastructure.push({ id: n.id, name: n.name || '', filePath: n.filePath || '' });
      } else if (fp.endsWith('.json') || fp.endsWith('.config') || fp.endsWith('.yml') || fp.endsWith('.yaml') || fp.endsWith('.xml') || fp.endsWith('.toml')) {
        nonCodeFiles.config.push({ id: n.id, name: n.name || '', filePath: n.filePath || '' });
      } else if (fp.endsWith('.sql') || fp.endsWith('.csv') || fp.includes('migration') || fp.includes('seed') || fp.includes('data')) {
        nonCodeFiles.data.push({ id: n.id, name: n.name || '', filePath: n.filePath || '' });
      }
    }
  });

  // Tightly Coupled Clusters
  const mutualConnections = [];
  const checked = new Set();
  const graph = {};
  nodes.forEach(n => { graph[n.id] = new Set(); });
  edges.forEach(e => {
    if (graph[e.source] && graph[e.target]) {
      graph[e.source].add(e.target);
    }
  });

  nodes.forEach(n1 => {
    nodes.forEach(n2 => {
      if (n1.id !== n2.id) {
        const pairKey = [n1.id, n2.id].sort().join('||');
        if (!checked.has(pairKey)) {
          checked.add(pairKey);
          const hasEdge1 = graph[n1.id].has(n2.id);
          const hasEdge2 = graph[n2.id].has(n1.id);
          if (hasEdge1 && hasEdge2) {
            mutualConnections.push([n1.id, n2.id]);
          }
        }
      }
    });
  });

  const tightlyCoupledClusters = mutualConnections.slice(0, 15).map(cluster => ({
    nodeIds: cluster,
    strength: 'mutual'
  }));

  const nodeSummaryIndex = {};
  nodes.forEach(n => {
    nodeSummaryIndex[n.id] = {
      name: n.name || '',
      type: n.type || '',
      filePath: n.filePath || '',
      summary: n.summary || ''
    };
  });

  const results = {
    fanIn,
    fanOut,
    entryPoints,
    dependencyChains,
    nonCodeFiles,
    tightlyCoupledClusters,
    layers,
    nodeSummaryIndex
  };

  fs.writeFileSync(outputPath, JSON.stringify(results, null, 2), 'utf8');
  console.log("Analysis completed successfully.");
  process.exit(0);

} catch (err) {
  console.error("Fatal error during graph analysis:", err);
  process.exit(1);
}
