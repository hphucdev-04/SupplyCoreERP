const fs = require('fs');
const path = require('path');

const projectRoot = "D:\\ProjectOwner\\SupplyCoreERP";
const analysisPath = path.join(projectRoot, '.understand-anything', 'intermediate', 'domain-analysis.json');
const destPath = path.join(projectRoot, '.understand-anything', 'domain-graph.json');
const contextPath = path.join(projectRoot, '.understand-anything', 'tmp', 'domain-graph-context.json');

try {
  console.log("Loading domain-analysis.json...");
  if (!fs.existsSync(analysisPath)) {
    throw new Error(`File not found: ${analysisPath}`);
  }
  const graph = JSON.parse(fs.readFileSync(analysisPath, 'utf8'));

  const issues = [];
  const warnings = [];

  // Check required root arrays
  if (!Array.isArray(graph.nodes)) {
    issues.push("nodes must be an array");
    graph.nodes = [];
  }
  if (!Array.isArray(graph.edges)) {
    issues.push("edges must be an array");
    graph.edges = [];
  }

  const nodeIds = new Set();
  const seenIds = new Map();

  // Validate nodes
  graph.nodes.forEach((node, i) => {
    if (!node.id) {
      issues.push(`Node[${i}] missing id`);
      return;
    }
    
    // Check type
    if (!['domain', 'flow', 'step'].includes(node.type)) {
      issues.push(`Node[${i}] '${node.id}' has invalid type: ${node.type}`);
    }

    // Check name, summary, tags, complexity
    if (!node.name) issues.push(`Node '${node.id}' missing name`);
    if (!node.summary) issues.push(`Node '${node.id}' missing summary`);
    if (!node.tags || !Array.isArray(node.tags) || node.tags.length === 0) {
      issues.push(`Node '${node.id}' missing or empty tags`);
      node.tags = node.tags || ["untagged"];
    }
    if (!['simple', 'moderate', 'complex'].includes(node.complexity)) {
      warnings.push(`Node '${node.id}' has invalid complexity '${node.complexity}', defaulting to 'moderate'`);
      node.complexity = 'moderate';
    }

    // ID formatting: prefix check
    if (node.type === 'domain' && !node.id.startsWith('domain:')) issues.push(`Domain ID '${node.id}' must start with 'domain:'`);
    if (node.type === 'flow' && !node.id.startsWith('flow:')) issues.push(`Flow ID '${node.id}' must start with 'flow:'`);
    if (node.type === 'step' && !node.id.startsWith('step:')) issues.push(`Step ID '${node.id}' must start with 'step:'`);

    // Duplicate check
    if (seenIds.has(node.id)) {
      issues.push(`Duplicate node ID '${node.id}' at index ${seenIds.get(node.id)} and ${i}`);
    } else {
      seenIds.set(node.id, i);
      nodeIds.add(node.id);
    }
  });

  // Validate edges
  const validEdges = [];
  graph.edges.forEach((edge, i) => {
    if (!edge.source || !edge.target || !edge.type) {
      issues.push(`Edge[${i}] missing source, target, or type`);
      return;
    }

    if (!nodeIds.has(edge.source)) {
      issues.push(`Edge[${i}] source '${edge.source}' not found in nodes`);
      return;
    }
    if (!nodeIds.has(edge.target)) {
      issues.push(`Edge[${i}] target '${edge.target}' not found in nodes`);
      return;
    }

    if (edge.source === edge.target) {
      issues.push(`Edge[${i}] self-referencing edge: '${edge.source}' -> '${edge.target}'`);
      return;
    }

    if (typeof edge.weight !== 'number' || edge.weight < 0.0 || edge.weight > 1.0) {
      warnings.push(`Edge[${i}] has invalid weight ${edge.weight}, defaulting to 0.5`);
      edge.weight = 0.5;
    }

    validEdges.push(edge);
  });
  graph.edges = validEdges;

  // Output validation results
  console.log("Validation complete.");
  console.log(`- Total Issues found: ${issues.length}`);
  console.log(`- Total Warnings found: ${warnings.length}`);
  
  if (issues.length > 0) {
    console.error("Critical Issues:");
    issues.forEach(iss => console.error(`  [ERROR] ${iss}`));
    console.log("Attempting to write graph anyway with best effort...");
  }

  if (warnings.length > 0) {
    console.log("Warnings:");
    warnings.forEach(warn => console.log(`  [WARN] ${warn}`));
  }

  // Save to domain-graph.json
  fs.writeFileSync(destPath, JSON.stringify(graph, null, 2));
  console.log(`Saved validated domain graph to: ${destPath}`);

  // Cleanup intermediate files
  if (fs.existsSync(analysisPath)) {
    fs.unlinkSync(analysisPath);
    console.log(`Deleted intermediate file: ${analysisPath}`);
  }
  if (fs.existsSync(contextPath)) {
    fs.unlinkSync(contextPath);
    console.log(`Deleted intermediate file: ${contextPath}`);
  }

} catch (err) {
  console.error("Error during validation and save:", err);
  process.exit(1);
}
