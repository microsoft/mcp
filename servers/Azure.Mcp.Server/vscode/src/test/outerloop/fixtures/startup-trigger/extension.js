// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Test-only extension. Two responsibilities:
//
//   1. Register an MCP server definition provider via the public
//      `vscode.lm.registerMcpServerDefinitionProvider` API. This is a
//      sanity check that VS Code stable accepts our server schema (cmd,
//      args, env). The proxy script we hand it is wired up but VS Code
//      will only spawn it if a chat client actually invokes a tool from
//      that server, which doesn't happen headlessly in CI.
//
//   2. Independently of (1), spawn `npx @azure/mcp@latest server start`
//      from the extension host itself, drive the JSON-RPC handshake as
//      a stdio MCP client, call the keyvault secret get tool with bogus
//      args, and capture the resulting `elicitation/create` request
//      from the server into a log file. The Playwright spec asserts on
//      that log.
//
// This proves: VS Code 1.118+ runs the extension host correctly, accepts
// our MCP server contribution, and the latest published Azure MCP server
// still emits the destructive-tool elicitation prompt verbatim.
//
// Env vars (set by the spec on _electron.launch):
//   - MCP_TEST_PROXY_SCRIPT: absolute path to mcpProxy.js (handed to the
//                            provider; not actually invoked here).
//   - MCP_TEST_PROXY_LOG:    absolute path of the log file. Both the
//                            provider's proxy and the in-extension client
//                            share this same log; the spec greps it.
//   - MCP_TEST_NODE_PATH:    absolute path to node.exe (process.execPath).

'use strict';

const vscode = require('vscode');
const { spawn } = require('node:child_process');
const fs = require('node:fs');
const readline = require('node:readline');

const SERVER_LABEL = 'azure-mcp-latest';
const TARGET_TOOL_PATTERN = /keyvault.*secret.*get/i;

function consoleLog(message) {
    console.log(`[mcpStartupTrigger] ${message}`);
}

async function activate(context) {
    consoleLog('activated');

    const proxyScript = process.env.MCP_TEST_PROXY_SCRIPT;
    const proxyLog = process.env.MCP_TEST_PROXY_LOG;
    const nodePath = process.env.MCP_TEST_NODE_PATH;

    if (!proxyScript || !proxyLog || !nodePath) {
        consoleLog(`missing env vars: MCP_TEST_PROXY_SCRIPT=${proxyScript} MCP_TEST_PROXY_LOG=${proxyLog} MCP_TEST_NODE_PATH=${nodePath}`);
        return;
    }

    // Open the shared log so both the provider/proxy path and the in-extension
    // client path append to the same file.
    const logStream = fs.createWriteStream(proxyLog, { flags: 'a' });
    const fileLog = (tag, line) => {
        try {
            logStream.write(`[${new Date().toISOString()}] ${tag} ${line}\n`);
        } catch { /* best effort */ }
    };
    context.subscriptions.push({ dispose: () => { try { logStream.end(); } catch { } } });

    fileLog('EXT', `activate proxyScript=${proxyScript} nodePath=${nodePath}`);

    // ---- (1) MCP server definition provider registration --------------------
    try {
        const StdioCtor = vscode.lm?.McpStdioServerDefinition || vscode.McpStdioServerDefinition;
        if (StdioCtor) {
            const emitter = new vscode.EventEmitter();
            context.subscriptions.push(emitter);
            const provider = {
                onDidChangeMcpServerDefinitions: emitter.event,
                provideMcpServerDefinitions: () => {
                    const def = new StdioCtor(
                        SERVER_LABEL,
                        nodePath,
                        [proxyScript],
                        { MCP_PROXY_LOG: proxyLog }
                    );
                    consoleLog(`provideMcpServerDefinitions returning: ${SERVER_LABEL}`);
                    fileLog('EXT', `provideMcpServerDefinitions returning ${SERVER_LABEL}`);
                    return [def];
                },
                resolveMcpServerDefinition: async (server) => server
            };
            const reg = vscode.lm?.registerMcpServerDefinitionProvider?.('azure-mcp-test-provider', provider);
            if (reg) {
                context.subscriptions.push(reg);
                consoleLog('registered MCP server definition provider');
                fileLog('EXT', 'provider registered');
            } else {
                consoleLog('vscode.lm.registerMcpServerDefinitionProvider not available');
                fileLog('EXT', 'vscode.lm.registerMcpServerDefinitionProvider not available');
            }
        } else {
            fileLog('EXT', 'McpStdioServerDefinition not available');
        }
    } catch (err) {
        consoleLog(`provider registration failed: ${err && err.message}`);
        fileLog('EXT', `provider registration failed: ${err && err.message}`);
    }

    // ---- (2) In-extension MCP client driving the elicitation flow -----------
    // Run async; failures are logged but do not block extension activation.
    driveMcpClient(fileLog).catch(err => {
        consoleLog(`driveMcpClient failed: ${err && err.stack || err}`);
        fileLog('EXT', `driveMcpClient failed: ${err && err.message}`);
    });
}

async function driveMcpClient(fileLog) {
    const isWindows = process.platform === 'win32';
    fileLog('CLIENT', 'spawning npx -y @azure/mcp@latest server start --mode all');
    const child = spawn('npx', ['-y', '@azure/mcp@latest', 'server', 'start', '--mode', 'all'], {
        stdio: ['pipe', 'pipe', 'pipe'],
        shell: isWindows
    });

    child.on('error', err => fileLog('CLIENT', `child error: ${err.message}`));
    child.on('exit', (code, signal) => fileLog('CLIENT', `child exited code=${code} signal=${signal}`));

    const errLines = readline.createInterface({ input: child.stderr });
    errLines.on('line', line => fileLog('STDERR', line));

    const send = (obj) => {
        const line = JSON.stringify(obj);
        fileLog('C->S', line);
        child.stdin.write(line + '\n');
    };

    // Pending request id -> resolver
    const pending = new Map();
    let nextId = 1;
    const request = (method, params) => {
        const id = nextId++;
        return new Promise((resolve, reject) => {
            pending.set(id, { resolve, reject });
            send({ jsonrpc: '2.0', id, method, params });
        });
    };
    const notify = (method, params) => send({ jsonrpc: '2.0', method, params });

    let elicitationSeen = false;

    const outLines = readline.createInterface({ input: child.stdout });
    outLines.on('line', line => {
        fileLog('S->C', line);
        let parsed;
        try { parsed = JSON.parse(line); } catch { return; }
        // Server-initiated request (e.g., elicitation/create).
        if (parsed.method && parsed.id !== undefined && parsed.id !== null && !pending.has(parsed.id)) {
            if (parsed.method === 'elicitation/create') {
                elicitationSeen = true;
                fileLog('ELICITATION', line);
                send({ jsonrpc: '2.0', id: parsed.id, result: { action: 'cancel' } });
            } else {
                // Reply with method-not-found so the server isn't blocked.
                send({
                    jsonrpc: '2.0',
                    id: parsed.id,
                    error: { code: -32601, message: `method ${parsed.method} not handled by test client` }
                });
            }
            return;
        }
        // Response to one of our requests.
        if (parsed.id !== undefined && pending.has(parsed.id)) {
            const { resolve, reject } = pending.get(parsed.id);
            pending.delete(parsed.id);
            if (parsed.error) reject(new Error(JSON.stringify(parsed.error)));
            else resolve(parsed.result);
        }
    });

    // 1. initialize
    fileLog('CLIENT', 'sending initialize');
    await request('initialize', {
        protocolVersion: '2025-06-18',
        capabilities: { elicitation: {} },
        clientInfo: { name: 'mcp-vscode-outerloop-test', version: '1.0.0' }
    });
    // 2. initialized notification
    notify('notifications/initialized', {});
    // 3. tools/list
    fileLog('CLIENT', 'sending tools/list');
    const toolsList = await request('tools/list', {});
    const tools = (toolsList && toolsList.tools) || [];
    const match = tools.map(t => t && t.name).find(n => typeof n === 'string' && TARGET_TOOL_PATTERN.test(n));
    fileLog('CLIENT', `tools=${tools.length} match=${match || '<none>'}`);
    if (!match) {
        fileLog('CLIENT', 'no matching tool, aborting');
        return;
    }
    // 4. tools/call - this should trigger elicitation/create from the server.
    fileLog('CLIENT', `sending tools/call name=${match}`);
    try {
        const callResult = await request('tools/call', {
            name: match,
            arguments: {
                subscription: 'test-subscription',
                vault: 'test-vault',
                secret: 'test-secret'
            }
        });
        fileLog('CLIENT', `tools/call result: ${JSON.stringify(callResult).slice(0, 500)}`);
    } catch (err) {
        fileLog('CLIENT', `tools/call rejected: ${err && err.message}`);
    }
    fileLog('CLIENT', `done elicitationSeen=${elicitationSeen}`);
}

function deactivate() { }

module.exports = { activate, deactivate };
