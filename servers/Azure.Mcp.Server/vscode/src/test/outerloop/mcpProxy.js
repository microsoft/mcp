// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// MCP stdio proxy used by the outerloop test.
//
// VS Code is configured (via mcp.json) to launch THIS script instead of
// the server executable directly. The proxy:
//
//   1. Spawns the real Azure MCP server.
//   2. Tees every JSON-RPC line in either direction to a log file
//      (path provided via MCP_PROXY_LOG env var). The Playwright test
//      reads this log to assert behavior.
//   3. Transparently forwards traffic between VS Code and the server,
//      so VS Code completes its normal initialize / tools/list flow.
//   4. After the VS Code "initialized" notification, parses the next
//      tools/list response coming back from the server, picks the
//      keyvault_secret_get tool, and injects a tools/call with a
//      synthetic id (999_999) directly into the server. The matching
//      response and the elicitation/create REQUEST that the server
//      sends back are intercepted (not forwarded to VS Code, to avoid
//      confusing it) and logged.
//
// Outcome: the log will contain a line tagged ELICITATION whose payload
// is the elicitation/create request from the server. The test asserts
// that payload contains "may expose secrets or sensitive information".

'use strict';

const { spawn } = require('node:child_process');
const fs = require('node:fs');
const readline = require('node:readline');

const LOG_PATH = process.env.MCP_PROXY_LOG;
if (!LOG_PATH) {
    process.stderr.write('mcpProxy: MCP_PROXY_LOG env var is required\n');
    process.exit(2);
}

const SERVER_EXECUTABLE = process.env.MCP_SERVER_EXECUTABLE;
if (!SERVER_EXECUTABLE) {
    process.stderr.write('mcpProxy: MCP_SERVER_EXECUTABLE env var is required\n');
    process.exit(2);
}
if (!fs.existsSync(SERVER_EXECUTABLE)) {
    process.stderr.write(`mcpProxy: MCP_SERVER_EXECUTABLE not found at '${SERVER_EXECUTABLE}'\n`);
    process.exit(2);
}

const INJECT_TOOLS_LIST_ID = 9_999_998;
const INJECT_TOOL_CALL_ID = 9_999_999;
const TARGET_TOOL_PATTERN = /keyvault.*secret.*get/i;

const logStream = fs.createWriteStream(LOG_PATH, { flags: 'a' });
function log(tag, line) {
    const entry = `[${new Date().toISOString()}] ${tag} ${line}\n`;
    try {
        if (!logStream.writableEnded) {
            logStream.write(entry);
        } else {
            fs.appendFileSync(LOG_PATH, entry);
        }
    } catch {
        try { fs.appendFileSync(LOG_PATH, entry); } catch { }
    }
}

let elicitationSeen = false;
let startupFailureReported = false;

function reportStartupFailure(details) {
    if (elicitationSeen || startupFailureReported) return;
    startupFailureReported = true;
    log('PROXY', `STARTUP_FAILURE executable='${SERVER_EXECUTABLE}' ${details}. Verify the binary is built for this platform and can be started.`);
}

let exiting = false;
function safeExit(exitCode) {
    if (exiting) return;
    exiting = true;
    const fallback = setTimeout(() => process.exit(exitCode), 1000);
    fallback.unref();
    try {
        logStream.end(() => {
            clearTimeout(fallback);
            process.exit(exitCode);
        });
    } catch {
        process.exit(exitCode);
    }
}

log('PROXY', `start pid=${process.pid} cwd=${process.cwd()}`);
log('PROXY', `spawning ${SERVER_EXECUTABLE} server start --mode all`);

// Test launch seam: if the configured executable is a JavaScript file (used by lifecycle unit tests),
// run it via Node.js so that Windows executes it without requiring shell: true.
const isJsScript = SERVER_EXECUTABLE.toLowerCase().endsWith('.js');
const spawnExe = isJsScript ? process.execPath : SERVER_EXECUTABLE;
const spawnArgs = isJsScript ? [SERVER_EXECUTABLE, 'server', 'start', '--mode', 'all'] : ['server', 'start', '--mode', 'all'];

const child = spawn(spawnExe, spawnArgs, {
    stdio: ['pipe', 'pipe', 'pipe']
});

let exitCode = null;
let exitSignal = null;

child.on('error', err => {
    log('PROXY', `child error: ${err.message}`);
    reportStartupFailure(`error=${err.message}`);
    safeExit(1);
});

child.on('exit', (code, signal) => {
    exitCode = code;
    exitSignal = signal;
    log('PROXY', `child exited code=${code} signal=${signal}`);
});

child.on('close', (code, signal) => {
    log('PROXY', `child closed code=${code} signal=${signal}`);
    const effectiveCode = (code !== null && code !== undefined) ? code : exitCode;
    const effectiveSignal = signal ?? exitSignal;
    if (!elicitationSeen) {
        reportStartupFailure(`code=${effectiveCode} signal=${effectiveSignal}`);
    }
    const finalCode = elicitationSeen ? (effectiveCode ?? 0) : (effectiveCode !== 0 && effectiveCode !== null && effectiveCode !== undefined ? effectiveCode : 1);
    safeExit(finalCode);
});

const childErrLines = readline.createInterface({ input: child.stderr });
childErrLines.on('line', line => log('STDERR', line));

let initializeNotificationSeen = false;
let toolsListInjected = false;
let toolCallInjected = false;
let resolvedToolName;


function writeToServer(obj) {
    const line = JSON.stringify(obj);
    log('PROXY->S', line);
    child.stdin.write(line + '\n');
}

// VS Code -> Server
const clientLines = readline.createInterface({ input: process.stdin });
clientLines.on('line', line => {
    log('C->S', line);

    let parsed;
    try { parsed = JSON.parse(line); } catch { /* non-JSON, just forward */ }

    // Forward unmodified.
    child.stdin.write(line + '\n');

    if (parsed && parsed.method === 'notifications/initialized') {
        initializeNotificationSeen = true;
        // Once VS Code has initialized, ask the server for its tool list
        // so we can discover the real keyvault secret get tool name.
        if (!toolsListInjected) {
            toolsListInjected = true;
            writeToServer({
                jsonrpc: '2.0',
                id: INJECT_TOOLS_LIST_ID,
                method: 'tools/list'
            });
        }
    }
});
clientLines.on('close', () => {
    log('PROXY', 'client stdin closed');
    child.stdin.end();
});

// Server -> VS Code
const serverLines = readline.createInterface({ input: child.stdout });
serverLines.on('line', line => {
    log('S->C', line);

    let parsed;
    try { parsed = JSON.parse(line); } catch { /* non-JSON */ }

    if (parsed && (parsed.id === INJECT_TOOLS_LIST_ID || parsed.id === INJECT_TOOL_CALL_ID)) {
        // Our injected request/response - don't forward to VS Code.
        log('INTERCEPT', line);

        if (parsed.id === INJECT_TOOLS_LIST_ID && parsed.result && Array.isArray(parsed.result.tools)) {
            const match = parsed.result.tools
                .map(t => t && t.name)
                .find(n => typeof n === 'string' && TARGET_TOOL_PATTERN.test(n));
            log('PROXY', `tools discovered: ${parsed.result.tools.length} match=${match || '<none>'}`);
            if (match && !toolCallInjected) {
                resolvedToolName = match;
                toolCallInjected = true;
                writeToServer({
                    jsonrpc: '2.0',
                    id: INJECT_TOOL_CALL_ID,
                    method: 'tools/call',
                    params: {
                        name: match,
                        arguments: {
                            subscription: 'test-subscription',
                            vault: 'test-vault',
                            secret: 'test-secret'
                        }
                    }
                });
            }
        }
        return;
    }

    if (parsed && parsed.method === 'elicitation/create') {
        // Server is asking the client for user consent. Log it (the test
        // asserts on this line) and immediately reply on behalf of the
        // injected call so the server isn't blocked. We assume this
        // elicitation was caused by our injected tools/call - if not,
        // VS Code may be confused, but in practice VS Code only issues
        // tools/call via Copilot Chat which isn't running in CI.
        elicitationSeen = true;
        log('ELICITATION', line);
        const response = {
            jsonrpc: '2.0',
            id: parsed.id,
            result: { action: 'cancel' }
        };
        writeToServer(response);
        return;
    }

    // Default: forward to VS Code.
    process.stdout.write(line + '\n');
});
serverLines.on('close', () => {
    log('PROXY', 'server stdout closed');
});

process.on('SIGTERM', () => { log('PROXY', 'SIGTERM'); child.kill('SIGTERM'); });
process.on('SIGINT', () => { log('PROXY', 'SIGINT'); child.kill('SIGINT'); });
