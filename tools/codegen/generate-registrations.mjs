#!/usr/bin/env node
// Generates IAreaRegistration files from tools-list JSON + source scanning.
// Usage: node generate-registrations.mjs <tools-list.json> <repo-root>

import { readFileSync, writeFileSync, readdirSync, statSync } from 'fs';
import { join, dirname, relative } from 'path';

const toolsListPath = process.argv[2] || 'C:/temp/tools-list.json';
const repoRoot = process.argv[3] || 'C:/repos/microsoft-mcp';

// ============================================================
// 1. Load tools-list JSON
// ============================================================
const toolsData = JSON.parse(readFileSync(toolsListPath, 'utf8'));
const tools = toolsData.results;

// Known inherited option names (from GlobalCommand / SubscriptionCommand)
const GLOBAL_OPTIONS = new Set([
  '--tenant', '--auth-method',
  '--retry-delay', '--retry-max-delay', '--retry-max-retries', '--retry-mode', '--retry-network-timeout'
]);
const SUBSCRIPTION_OPTIONS = new Set([...GLOBAL_OPTIONS, '--subscription']);

function classifyKind(options) {
  const optNames = new Set(options.map(o => o.name));
  if (optNames.has('--subscription')) return 'Subscription';
  if (optNames.has('--tenant')) return 'Global';
  return 'Basic';
}

function getCustomOptions(options) {
  return options.filter(o => !SUBSCRIPTION_OPTIONS.has(o.name));
}

// ============================================================
// 2. Build command tree from tools-list
// ============================================================
function buildTree() {
  const tree = {};
  for (const tool of tools) {
    const parts = tool.command.split(' ');
    const area = parts[0];
    if (!tree[area]) tree[area] = { commands: [], subgroups: {} };
    let node = tree[area];
    for (let i = 1; i < parts.length - 1; i++) {
      if (!node.subgroups[parts[i]]) node.subgroups[parts[i]] = { commands: [], subgroups: {} };
      node = node.subgroups[parts[i]];
    }
    node.commands.push(tool);
  }
  return tree;
}

const commandTree = buildTree();

// ============================================================
// 3. Scan source for Setup files and command classes
// ============================================================
function findFiles(dir, pattern) {
  const results = [];
  try {
    for (const entry of readdirSync(dir)) {
      const full = join(dir, entry);
      try {
        const stat = statSync(full);
        if (stat.isDirectory() && !entry.startsWith('.') && entry !== 'node_modules' && entry !== 'bin' && entry !== 'obj') {
          results.push(...findFiles(full, pattern));
        } else if (pattern.test(entry)) {
          results.push(full);
        }
      } catch {}
    }
  } catch {}
  return results;
}

// Find all Setup files
const setupFiles = findFiles(join(repoRoot, 'tools'), /Setup\.cs$/)
  .concat(findFiles(join(repoRoot, 'core'), /Setup\.cs$/));

// Find all Command files
const commandFiles = findFiles(join(repoRoot, 'tools'), /Command\.cs$/)
  .concat(findFiles(join(repoRoot, 'core'), /Command\.cs$/));

// Parse setup files for metadata
function parseSetupFile(path) {
  const content = readFileSync(path, 'utf8');
  const nameMatch = content.match(/Name\s*=>\s*"([^"]+)"/);
  const titleMatch = content.match(/Title\s*=>\s*"([^"]+)"/);
  const categoryMatch = content.match(/Category\s*=>\s*CommandCategory\.(\w+)/);
  const namespaceMatch = content.match(/namespace\s+([\w.]+)/);
  const classMatch = content.match(/class\s+(\w+)\s*:\s*IAreaSetup/);

  // Extract ConfigureServices body
  const csMatch = content.match(/(?:void\s+ConfigureServices|void\s+IAreaSetup\.ConfigureServices)\s*\(\s*IServiceCollection\s+\w+\s*\)\s*\{([\s\S]*?)\n\s*\}/);

  // Extract using statements
  const usings = [...content.matchAll(/using\s+([\w.]+);/g)].map(m => m[1]);

  // Extract subgroup descriptions from RegisterCommands
  const subgroupDescs = {};
  const sgMatches = [...content.matchAll(/new\s+CommandGroup\s*\(\s*"([^"]+)"\s*,\s*(?:"((?:[^"\\]|\\.)*)"|"""([\s\S]*?)""")/g)];
  for (const m of sgMatches) {
    subgroupDescs[m[1]] = (m[2] || m[3] || '').replace(/\s+/g, ' ').trim();
  }

  // Extract root group description
  const rootDescMatch = content.match(/new\s+CommandGroup\s*\(\s*Name\s*,\s*(?:"((?:[^"\\]|\\.)*)"|"""([\s\S]*?)"""|@"((?:[^"]|"")*)"|(\$"""[\s\S]*?"""))/);
  let rootDesc = '';
  if (rootDescMatch) {
    rootDesc = (rootDescMatch[1] || rootDescMatch[2] || rootDescMatch[3] || rootDescMatch[4] || '').replace(/\s+/g, ' ').trim();
  }

  return {
    path,
    name: nameMatch?.[1],
    title: titleMatch?.[1],
    category: categoryMatch?.[1] || 'AzureServices',
    namespace: namespaceMatch?.[1],
    className: classMatch?.[1],
    configureServicesBody: csMatch?.[1]?.trim() || '',
    usings,
    subgroupDescs,
    rootDesc
  };
}

// Parse command files for handler info
function parseCommandFile(path) {
  const content = readFileSync(path, 'utf8');
  // Match primary constructor pattern: class ClassName(...) : BaseClass<T>
  // Also match traditional pattern: class ClassName : BaseClass<T>
  const classMatch = content.match(/(?:sealed\s+)?class\s+(\w+)(?:<[^>]+>)?\s*(?:\([^)]*\)\s*)?:\s*(\w+)(?:<(\w+)>)?/);
  if (!classMatch) return null;

  const className = classMatch[1];
  const baseClass = classMatch[2];
  const namespaceMatch = content.match(/namespace\s+([\w.]+)/);

  // Determine kind from base class
  let kind = 'Basic';
  if (baseClass.includes('Subscription')) {
    kind = 'Subscription';
  } else if (baseClass.includes('Global')) {
    kind = 'Global';
  } else if (baseClass === 'BaseCommand') {
    kind = 'Basic';
  } else {
    // Custom base class - need to resolve later
    kind = 'Unknown';
  }

  // Check for hidden attribute
  const hidden = content.includes('[HiddenCommand]') || /Hidden\s*=\s*true/.test(content);

  // Extract Name
  const nameMatch = content.match(/override\s+string\s+Name\s*=>\s*"([^"]+)"/);
  // Extract Title
  const titleMatch = content.match(/override\s+string\s+Title\s*=>\s*"([^"]+)"/);

  return {
    path,
    className,
    baseClass,
    kind,
    hidden,
    namespace: namespaceMatch?.[1],
    name: nameMatch?.[1],
    title: titleMatch?.[1]
  };
}

const setups = setupFiles.map(parseSetupFile).filter(s => s.name);
const commands = commandFiles.map(parseCommandFile).filter(c => c);

// Index commands by class name
const commandsByName = {};
for (const cmd of commands) {
  commandsByName[cmd.className] = cmd;
}

// Resolve unknown kinds by checking base class chains
for (const cmd of commands) {
  if (cmd.kind === 'Unknown') {
    // Look up the base class
    const base = commandsByName[cmd.baseClass];
    if (base) {
      cmd.kind = base.kind;
    }
  }
}
// Second pass for deeper chains
for (const cmd of commands) {
  if (cmd.kind === 'Unknown') {
    const base = commandsByName[cmd.baseClass];
    if (base && base.kind !== 'Unknown') {
      cmd.kind = base.kind;
    }
  }
}

// ============================================================
// 4. Map tools-list commands to handler class names
// ============================================================
function findHandlerForTool(tool, setupInfo) {
  const parts = tool.command.split(' ');
  const cmdName = parts[parts.length - 1];
  const setupDir = dirname(dirname(setupInfo.path)); // project root

  // Read setup file to find which class is registered for this command name
  const content = readFileSync(setupInfo.path, 'utf8');

  // Pattern: serviceProvider.GetRequiredService<ClassName>();
  const reqMatches = [...content.matchAll(/GetRequiredService<(\w+)>\(\)/g)];
  for (const m of reqMatches) {
    // Prefer command in same project, fallback to global index
    const projectMatch = commands.find(c => c.className === m[1] && c.path.startsWith(setupDir));
    const cmdClass = projectMatch || commandsByName[m[1]];
    if (cmdClass && cmdClass.name === cmdName) {
      return cmdClass;
    }
  }

  // Fallback: search in project directory
  const projectCmds = commands.filter(c => c.path.startsWith(setupDir));
  const namePascal = cmdName.split('-').map(p => p[0].toUpperCase() + p.slice(1)).join('');
  const byClassName = projectCmds.filter(c =>
    c.className.toLowerCase() === (namePascal + 'command').toLowerCase()
  );
  if (byClassName.length === 1) return byClassName[0];

  // Try partial match
  const partials = projectCmds.filter(c =>
    c.className.toLowerCase().endsWith(namePascal.toLowerCase() + 'command') && c.name === cmdName
  );
  if (partials.length === 1) return partials[0];

  return null;
}

// ============================================================
// 5. Generate Registration files
// ============================================================
function escapeDescriptorString(s, indent) {
  if (!s) return '""';
  // Normalize: collapse all whitespace (newlines, multiple spaces) into single spaces
  const normalized = s.replace(/\s+/g, ' ').trim();
  // Escape any quotes and backslashes for a regular C# string
  const escaped = normalized.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
  return `"${escaped}"`;
}

function boolLower(v) { return v ? 'true' : 'false'; }

function generateOptionDescriptor(opt, indent) {
  const spaces = ' '.repeat(indent);
  const name = opt.name.replace(/^--/, '');
  const lines = [`${spaces}new OptionDescriptor`];
  lines.push(`${spaces}{`);
  lines.push(`${spaces}    Name = "${name}",`);
  lines.push(`${spaces}    Description = ${escapeDescriptorString(opt.description, indent + 4)},`);
  lines.push(`${spaces}    TypeName = "string",`);
  if (opt.required) lines.push(`${spaces}    Required = true,`);
  lines.push(`${spaces}}`);
  return lines.join('\r\n');
}

function generateCommandDescriptor(tool, handlerClass, indent) {
  const spaces = ' '.repeat(indent);
  let kind = 'Basic';
  if (handlerClass && handlerClass.kind !== 'Unknown') {
    kind = handlerClass.kind;
  } else {
    kind = classifyKind(tool.option || []);
  }
  const customOpts = getCustomOptions(tool.option || []);
  const hidden = handlerClass?.hidden || false;

  const meta = tool.metadata || {};
  const lines = [`${spaces}new CommandDescriptor`];
  lines.push(`${spaces}{`);
  lines.push(`${spaces}    Id = "${tool.id}",`);
  lines.push(`${spaces}    Name = "${tool.name}",`);

  // Description
  const desc = (tool.description || '').replace(/\r\n/g, '\n').replace(/\n/g, ' ').replace(/\s+/g, ' ').trim();
  lines.push(`${spaces}    Description = ${escapeDescriptorString(desc, indent + 4)},`);

  // Title is required - use handler's title, or generate from name
  const title = handlerClass?.title || tool.name.split('-').map(p => p[0].toUpperCase() + p.slice(1)).join(' ');
  lines.push(`${spaces}    Title = "${title}",`);
  if (hidden) {
    lines.push(`${spaces}    Hidden = true,`);
  }
  lines.push(`${spaces}    Annotations = new ToolAnnotations`);
  lines.push(`${spaces}    {`);
  lines.push(`${spaces}        Destructive = ${boolLower(meta.destructive?.value ?? false)},`);
  lines.push(`${spaces}        Idempotent = ${boolLower(meta.idempotent?.value ?? false)},`);
  lines.push(`${spaces}        OpenWorld = ${boolLower(meta.openWorld?.value ?? false)},`);
  lines.push(`${spaces}        ReadOnly = ${boolLower(meta.readOnly?.value ?? true)},`);
  lines.push(`${spaces}        LocalRequired = ${boolLower(meta.localRequired?.value ?? false)},`);
  lines.push(`${spaces}        Secret = ${boolLower(meta.secret?.value ?? false)},`);
  lines.push(`${spaces}    },`);

  if (customOpts.length === 0) {
    lines.push(`${spaces}    Options = [],`);
  } else {
    lines.push(`${spaces}    Options =`);
    lines.push(`${spaces}    [`);
    for (const opt of customOpts) {
      lines.push(generateOptionDescriptor(opt, indent + 8) + ',');
    }
    lines.push(`${spaces}    ],`);
  }

  lines.push(`${spaces}    Kind = CommandKind.${kind},`);
  const handlerName = handlerClass?.className || 'UNKNOWN';
  lines.push(`${spaces}    HandlerType = nameof(${handlerName})`);
  lines.push(`${spaces}}`);
  return lines.join('\r\n');
}

function generateSubgroupDescriptor(name, node, setupInfo, indent) {
  const spaces = ' '.repeat(indent);
  const desc = setupInfo.subgroupDescs[name] || `${name} operations.`;

  const lines = [`${spaces}new CommandGroupDescriptor`];
  lines.push(`${spaces}{`);
  lines.push(`${spaces}    Name = "${name}",`);
  lines.push(`${spaces}    Description = ${escapeDescriptorString(desc, indent + 4)},`);

  if (node.commands.length > 0) {
    lines.push(`${spaces}    Commands =`);
    lines.push(`${spaces}    [`);
    for (const tool of node.commands) {
      const handler = findHandlerForTool(tool, setupInfo);
      lines.push(generateCommandDescriptor(tool, handler, indent + 8) + ',');
    }
    lines.push(`${spaces}    ],`);
  }

  const sgNames = Object.keys(node.subgroups);
  if (sgNames.length > 0) {
    lines.push(`${spaces}    SubGroups =`);
    lines.push(`${spaces}    [`);
    for (const sg of sgNames) {
      lines.push(generateSubgroupDescriptor(sg, node.subgroups[sg], setupInfo, indent + 8) + ',');
    }
    lines.push(`${spaces}    ],`);
  }

  lines.push(`${spaces}}`);
  return lines.join('\r\n');
}

function generateRegistration(setupInfo) {
  const areaTree = commandTree[setupInfo.name];
  if (!areaTree) {
    console.error(`  WARN: No tools found for area '${setupInfo.name}' in tools-list. Skipping.`);
    return null;
  }

  const regClassName = setupInfo.className.replace('Setup', 'Registration');

  // Collect all handler classes for this area
  const handlerClasses = [];
  function collectHandlers(node) {
    for (const tool of node.commands) {
      const h = findHandlerForTool(tool, setupInfo);
      if (h) handlerClasses.push(h);
      else console.error(`  WARN: No handler found for '${tool.command}'`);
    }
    for (const sg of Object.values(node.subgroups)) {
      collectHandlers(sg);
    }
  }
  collectHandlers(areaTree);

  // Deduplicate handlers by class name
  const seenHandlers = new Set();
  const uniqueHandlers = [];
  for (const h of handlerClasses) {
    if (!seenHandlers.has(h.className)) {
      seenHandlers.add(h.className);
      uniqueHandlers.push(h);
    }
  }

  // Build usings - start with all usings from setup file, add handler namespaces
  const coreUsings = new Set([
    'Microsoft.Extensions.DependencyInjection',
    'Microsoft.Mcp.Core.Areas',
    'Microsoft.Mcp.Core.Commands',
    'Microsoft.Mcp.Core.Commands.Descriptors'
  ]);

  // Add all usings from the setup file (services, commands, etc.)
  for (const u of setupInfo.usings) {
    coreUsings.add(u);
  }

  // Add handler namespaces (may differ from setup usings if commands are in sub-namespaces)
  for (const h of uniqueHandlers) {
    if (h.namespace) coreUsings.add(h.namespace);
  }

  // Remove self-namespace
  coreUsings.delete(setupInfo.namespace);

  const sortedUsings = [...coreUsings].sort();

  // Generate file
  const lines = [];
  lines.push('// Copyright (c) Microsoft Corporation.');
  lines.push('// Licensed under the MIT License.');
  lines.push('');
  lines.push('#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member');
  lines.push('');
  for (const u of sortedUsings) {
    lines.push(`using ${u};`);
  }
  lines.push('');
  lines.push(`namespace ${setupInfo.namespace};`);
  lines.push('');
  lines.push(`public sealed class ${regClassName} : IAreaRegistration`);
  lines.push('{');
  lines.push(`    public static string AreaName => "${setupInfo.name}";`);
  lines.push('');
  lines.push(`    public static string AreaTitle => "${setupInfo.title}";`);
  lines.push('');
  lines.push(`    public static CommandCategory Category => CommandCategory.${setupInfo.category};`);
  lines.push('');

  // GetCommandDescriptors
  lines.push('    public static CommandGroupDescriptor GetCommandDescriptors() => new()');
  lines.push('    {');
  lines.push('        Name = AreaName,');
  lines.push(`        Description = ${escapeDescriptorString(setupInfo.rootDesc || `${setupInfo.title} operations.`, 8)},`);
  lines.push('        Title = AreaTitle,');

  if (areaTree.commands.length > 0) {
    lines.push('        Commands =');
    lines.push('        [');
    for (const tool of areaTree.commands) {
      const handler = findHandlerForTool(tool, setupInfo);
      lines.push(generateCommandDescriptor(tool, handler, 12) + ',');
    }
    lines.push('        ],');
  }

  const sgNames = Object.keys(areaTree.subgroups);
  if (sgNames.length > 0) {
    lines.push('        SubGroups =');
    lines.push('        [');
    for (const sg of sgNames) {
      lines.push(generateSubgroupDescriptor(sg, areaTree.subgroups[sg], setupInfo, 12) + ',');
    }
    lines.push('        ],');
  }

  lines.push('    };');
  lines.push('');

  // RegisterServices
  lines.push('    public static void RegisterServices(IServiceCollection services)');
  lines.push('    {');
  if (setupInfo.configureServicesBody) {
    for (const line of setupInfo.configureServicesBody.split('\n')) {
      const trimmed = line.trim();
      if (trimmed) lines.push(`        ${trimmed}`);
    }
  }
  lines.push('    }');
  lines.push('');

  // ResolveHandler
  lines.push('    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>');
  lines.push('        handlerTypeName switch');
  lines.push('        {');
  for (const h of uniqueHandlers) {
    lines.push(`            nameof(${h.className}) => serviceProvider.GetRequiredService<${h.className}>(),`);
  }
  lines.push(`            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in ${setupInfo.name} area.")`);
  lines.push('        };');

  lines.push('}');

  return {
    content: lines.join('\r\n') + '\r\n',
    fileName: regClassName + '.cs',
    dir: dirname(setupInfo.path),
    className: regClassName,
    setupClassName: setupInfo.className,
    namespace: setupInfo.namespace,
    handlers: handlerClasses
  };
}

// ============================================================
// 6. Run generation
// ============================================================
console.log(`Found ${tools.length} tools in tools-list.json`);
console.log(`Found ${setups.length} Setup files`);
console.log(`Found ${commands.length} Command files`);
console.log('');

const results = [];
const skipped = [];

for (const setup of setups) {
  // Skip GroupSetup (already converted) and ServerSetup/ToolsSetup (special - CLI only, not in tools-list)
  if (['GroupSetup', 'ServerSetup', 'ToolsSetup'].includes(setup.className)) {
    console.log(`SKIP: ${setup.className} (special/already converted)`);
    skipped.push(setup);
    continue;
  }

  console.log(`Generating for ${setup.className} (area: ${setup.name})...`);
  const result = generateRegistration(setup);
  if (result) {
    const outPath = join(result.dir, result.fileName);
    writeFileSync(outPath, result.content, 'utf8');
    console.log(`  -> ${relative(repoRoot, outPath)}`);
    results.push(result);
  }
}

console.log('');
console.log(`Generated ${results.length} Registration files.`);
console.log(`Skipped ${skipped.length} special setups.`);

// Output summary for Program.cs wiring
console.log('\n=== Program.cs AreaRegistrationInfo entries ===');
for (const r of results) {
  console.log(`            AreaRegistrationInfo.Create<${r.namespace}.${r.className}>(),`);
}

// Output warnings summary
const unknownHandlers = results.filter(r => r.content.includes('nameof(UNKNOWN)'));
if (unknownHandlers.length > 0) {
  console.log('\n=== Files with UNKNOWN handlers (need manual fix) ===');
  for (const r of unknownHandlers) {
    console.log(`  ${r.fileName}`);
  }
}
