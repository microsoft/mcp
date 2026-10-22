// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Regression tests for MCP child process lifecycle, error handling, stdout
// draining, and startup-failure reporting in both mcpProxy.js and extension.js,
// plus prompt-failure assertions in waitForLogContains.

'use strict';

const isPlaywright = Boolean(process.env.PLAYWRIGHT_TEST);

// Hook Module._load so extension.js can be loaded and tested without requiring
// the VS Code extension host runtime.
const Module = require('node:module');
const originalLoad = Module._load;
Module._load = function (request, parent, isMain) {
    if (request === 'vscode') {
        return {
            lm: {},
            EventEmitter: class { constructor() { this.event = () => {}; } }
        };
    }
    return originalLoad.apply(this, arguments);
};

// Dual-runner compatibility: Use @playwright/test if executed within Playwright
// (both collection/listing and execution), or standard node:test when executed via `node --test`.
let testRunner, describeBlock, beforeEachHook, afterEachHook;

if (isPlaywright) {
    const pw = require('@playwright/test');
    testRunner = pw.test;
    describeBlock = pw.test.describe;
    beforeEachHook = pw.test.beforeEach;
    afterEachHook = pw.test.afterEach;
} else {
    const nodeTest = require('node:test');
    testRunner = nodeTest.it;
    describeBlock = nodeTest.describe;
    beforeEachHook = nodeTest.beforeEach;
    afterEachHook = nodeTest.afterEach;
}

const assert = require('node:assert/strict');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { spawn } = require('node:child_process');

const PROXY_SCRIPT = path.join(__dirname, 'mcpProxy.js');
const { driveMcpClient } = require('./fixtures/startup-trigger/extension.js');
const { waitForLogContains } = require('./logWatcher');

let testTempDir;
const originalEnv = { ...process.env };

beforeEachHook(() => {
    testTempDir = fs.mkdtempSync(path.join(os.tmpdir(), 'mcp-lifecycle-test-'));
});

afterEachHook(() => {
    process.env = { ...originalEnv };
    if (testTempDir && fs.existsSync(testTempDir)) {
        fs.rmSync(testTempDir, { recursive: true, force: true });
    }
});

function createMockExecutable(dir, name, jsSource) {
    const isWin = process.platform === 'win32';
    if (isWin) {
        const jsPath = path.join(dir, `${name}.js`);
        fs.writeFileSync(jsPath, jsSource, 'utf8');
        const cmdPath = path.join(dir, `${name}.cmd`);
        fs.writeFileSync(cmdPath, `@echo off\r\n"${process.execPath}" "${jsPath}" %*\r\n`, 'utf8');
        return cmdPath;
    } else {
        const scriptPath = path.join(dir, name);
        const content = `#!${process.execPath}\n${jsSource}`;
        fs.writeFileSync(scriptPath, content, { mode: 0o755 });
        return scriptPath;
    }
}

function createUnexecutableFile(dir, name) {
    const filePath = path.join(dir, name);
    fs.writeFileSync(filePath, 'not an executable', { mode: 0o644 });
    return filePath;
}

function runProxy(envOverrides = {}, stdinData = null) {
    return new Promise((resolve) => {
        const proxy = spawn(process.execPath, [PROXY_SCRIPT], {
            env: {
                ...process.env,
                ...envOverrides
            },
            stdio: ['pipe', 'pipe', 'pipe']
        });

        let stdout = '';
        let stderr = '';
        proxy.stdout.on('data', d => { stdout += d; });
        proxy.stderr.on('data', d => { stderr += d; });

        if (stdinData) {
            proxy.stdin.write(stdinData);
        }

        proxy.on('close', (code, signal) => {
            const logPath = envOverrides.MCP_PROXY_LOG;
            let logContent = '';
            if (logPath && fs.existsSync(logPath)) {
                logContent = fs.readFileSync(logPath, 'utf8');
            }
            resolve({ code, signal, stdout, stderr, logContent });
        });
    });
}

describeBlock('mcpProxy process lifecycle & failure handling', () => {
    testRunner('fails with exit code 2 when MCP_SERVER_EXECUTABLE env var is unset', async () => {
        const logPath = path.join(testTempDir, 'proxy.log');
        const env = { ...process.env, MCP_PROXY_LOG: logPath };
        delete env.MCP_SERVER_EXECUTABLE;

        const result = await runProxy(env);
        assert.equal(result.code, 2);
        assert.match(result.stderr, /MCP_SERVER_EXECUTABLE env var is required/);
    });

    testRunner('fails with exit code 2 when MCP_SERVER_EXECUTABLE does not exist', async () => {
        const logPath = path.join(testTempDir, 'proxy.log');
        const missingExe = path.join(testTempDir, 'nonexistent-azmcp');

        const result = await runProxy({
            MCP_PROXY_LOG: logPath,
            MCP_SERVER_EXECUTABLE: missingExe
        });
        assert.equal(result.code, 2);
        assert.match(result.stderr, /MCP_SERVER_EXECUTABLE not found at/);
    });

    testRunner('handles child process spawn error, logs STARTUP_FAILURE, and exits with code 1', async () => {
        const logPath = path.join(testTempDir, 'proxy.log');
        const unexecutable = createUnexecutableFile(testTempDir, 'unexecutable_server');

        const result = await runProxy({
            MCP_PROXY_LOG: logPath,
            MCP_SERVER_EXECUTABLE: unexecutable
        });

        assert.equal(result.code, 1);
        assert.match(result.logContent, /STARTUP_FAILURE/);
        assert.match(result.logContent, /error=/);
        assert.match(result.logContent, new RegExp(`executable='${unexecutable.replace(/\\/g, '\\\\')}'`));
    });

    testRunner('logs STARTUP_FAILURE and propagates non-zero exit code when child exits unsuccessfully before elicitation', async () => {
        const logPath = path.join(testTempDir, 'proxy.log');
        const mockExe = createMockExecutable(testTempDir, 'early_exit_nonzero', 'process.exit(42);\n');

        const result = await runProxy({
            MCP_PROXY_LOG: logPath,
            MCP_SERVER_EXECUTABLE: mockExe
        });

        assert.equal(result.code, 42);
        assert.match(result.logContent, /STARTUP_FAILURE/);
        assert.match(result.logContent, /code=42/);
    });

    testRunner('logs STARTUP_FAILURE and exits with code 1 when child exits with code 0 before elicitation', async () => {
        const logPath = path.join(testTempDir, 'proxy.log');
        const mockExe = createMockExecutable(testTempDir, 'early_exit_zero', 'process.exit(0);\n');

        const result = await runProxy({
            MCP_PROXY_LOG: logPath,
            MCP_SERVER_EXECUTABLE: mockExe
        });

        // Must not be treated as a successful test!
        assert.equal(result.code, 1);
        assert.match(result.logContent, /STARTUP_FAILURE/);
        assert.match(result.logContent, /code=0/);
    });

    testRunner('drains buffered stdout before close and exits with code 0 when elicitation arrives right before exit', async () => {
        const logPath = path.join(testTempDir, 'proxy.log');
        const serverSource = `
const readline = require('node:readline');
const rl = readline.createInterface({ input: process.stdin });
rl.on('line', line => {
    try {
        const msg = JSON.parse(line);
        if (msg.method === 'tools/list') {
            process.stdout.write(JSON.stringify({
                jsonrpc: '2.0',
                id: msg.id,
                result: { tools: [{ name: 'keyvault_secret_get' }] }
            }) + '\\n');
        } else if (msg.method === 'tools/call') {
            process.stdout.write(JSON.stringify({
                jsonrpc: '2.0',
                id: 88888,
                method: 'elicitation/create',
                params: { message: 'may expose secrets or sensitive information' }
            }) + '\\n');
            process.exit(0);
        }
    } catch {}
});
`;
        const mockExe = createMockExecutable(testTempDir, 'elicitation_then_exit', serverSource);

        const initNotification = JSON.stringify({
            jsonrpc: '2.0',
            method: 'notifications/initialized',
            params: {}
        }) + '\n';

        const result = await runProxy(
            {
                MCP_PROXY_LOG: logPath,
                MCP_SERVER_EXECUTABLE: mockExe
            },
            initNotification
        );

        assert.equal(result.code, 0);
        assert.match(result.logContent, / ELICITATION /);
        assert.ok(!result.logContent.includes('STARTUP_FAILURE'), 'STARTUP_FAILURE must not be logged when elicitation was seen');
    });

    testRunner('deduplicates STARTUP_FAILURE so it appears exactly once across error and close events', async () => {
        const logPath = path.join(testTempDir, 'proxy.log');
        const unexecutable = createUnexecutableFile(testTempDir, 'unexecutable_server_dedup');

        const result = await runProxy({
            MCP_PROXY_LOG: logPath,
            MCP_SERVER_EXECUTABLE: unexecutable
        });

        assert.equal(result.code, 1);
        const matches = result.logContent.match(/STARTUP_FAILURE/g) || [];
        assert.equal(matches.length, 1, `Expected exactly 1 STARTUP_FAILURE in log, found ${matches.length}`);
    });
});

describeBlock('extension driveMcpClient lifecycle & failure handling', () => {
    testRunner('rejects and logs when MCP_SERVER_EXECUTABLE is not set', async () => {
        delete process.env.MCP_SERVER_EXECUTABLE;
        const logs = [];
        const fileLog = (tag, line) => logs.push({ tag, line });

        await assert.rejects(
            () => driveMcpClient(fileLog),
            /MCP_SERVER_EXECUTABLE env var is required/
        );
        assert.ok(logs.some(l => l.line.includes('MCP_SERVER_EXECUTABLE env var is required')));
    });

    testRunner('rejects and logs when MCP_SERVER_EXECUTABLE does not exist', async () => {
        const missingExe = path.join(testTempDir, 'nonexistent-azmcp');
        process.env.MCP_SERVER_EXECUTABLE = missingExe;
        const logs = [];
        const fileLog = (tag, line) => logs.push({ tag, line });

        await assert.rejects(
            () => driveMcpClient(fileLog),
            /MCP_SERVER_EXECUTABLE not found at/
        );
        assert.ok(logs.some(l => l.line.includes('MCP_SERVER_EXECUTABLE not found at')));
    });

    testRunner('handles child process spawn error, logs STARTUP_FAILURE, and rejects pending requests', async () => {
        const unexecutable = createUnexecutableFile(testTempDir, 'unexecutable_client');
        process.env.MCP_SERVER_EXECUTABLE = unexecutable;
        const logs = [];
        const fileLog = (tag, line) => logs.push({ tag, line });

        await assert.rejects(
            () => driveMcpClient(fileLog),
            /MCP server startup failure/
        );

        assert.ok(logs.some(l => l.line.includes('STARTUP_FAILURE') && l.line.includes('error=')));
    });

    testRunner('logs STARTUP_FAILURE when child exits unsuccessfully before elicitation', async () => {
        const mockExe = createMockExecutable(testTempDir, 'client_early_exit_nonzero', 'process.exit(42);\n');
        process.env.MCP_SERVER_EXECUTABLE = mockExe;
        const logs = [];
        const fileLog = (tag, line) => logs.push({ tag, line });

        await assert.rejects(
            () => driveMcpClient(fileLog),
            /MCP server startup failure/
        );

        assert.ok(logs.some(l => l.line.includes('STARTUP_FAILURE') && l.line.includes('unexpected exit code=42')));
    });

    testRunner('logs STARTUP_FAILURE when child exits with code 0 before elicitation', async () => {
        const mockExe = createMockExecutable(testTempDir, 'client_early_exit_zero', 'process.exit(0);\n');
        process.env.MCP_SERVER_EXECUTABLE = mockExe;
        const logs = [];
        const fileLog = (tag, line) => logs.push({ tag, line });

        await assert.rejects(
            () => driveMcpClient(fileLog),
            /MCP server startup failure/
        );

        assert.ok(logs.some(l => l.line.includes('STARTUP_FAILURE') && l.line.includes('unexpected exit code=0')));
    });

    testRunner('processes buffered output before deciding elicitation and completes cleanly without STARTUP_FAILURE', async () => {
        const serverSource = `
const readline = require('node:readline');
const rl = readline.createInterface({ input: process.stdin });
rl.on('line', line => {
    try {
        const msg = JSON.parse(line);
        if (msg.method === 'initialize') {
            process.stdout.write(JSON.stringify({ jsonrpc: '2.0', id: msg.id, result: { protocolVersion: '2025-06-18' } }) + '\\n');
        } else if (msg.method === 'tools/list') {
            process.stdout.write(JSON.stringify({ jsonrpc: '2.0', id: msg.id, result: { tools: [{ name: 'keyvault_secret_get' }] } }) + '\\n');
        } else if (msg.method === 'tools/call') {
            process.stdout.write(JSON.stringify({ jsonrpc: '2.0', id: 88888, method: 'elicitation/create', params: { message: 'may expose secrets or sensitive information' } }) + '\\n');
            process.exit(0);
        }
    } catch {}
});
`;
        const mockExe = createMockExecutable(testTempDir, 'client_elicitation_drain', serverSource);
        process.env.MCP_SERVER_EXECUTABLE = mockExe;
        const logs = [];
        const fileLog = (tag, line) => logs.push({ tag, line });

        await driveMcpClient(fileLog);

        assert.ok(logs.some(l => l.tag === 'ELICITATION'), 'Elicitation should be observed');
        assert.ok(!logs.some(l => l.line.includes('STARTUP_FAILURE')), 'STARTUP_FAILURE must not be logged');
        assert.ok(logs.some(l => l.line.includes('done elicitationSeen=true')), 'Client should complete successfully');
    });

    testRunner('deduplicates STARTUP_FAILURE so it appears exactly once across error and close events', async () => {
        const unexecutable = createUnexecutableFile(testTempDir, 'client_dedup');
        process.env.MCP_SERVER_EXECUTABLE = unexecutable;
        const logs = [];
        const fileLog = (tag, line) => logs.push({ tag, line });

        await assert.rejects(
            () => driveMcpClient(fileLog),
            /MCP server startup failure/
        );

        const failures = logs.filter(l => l.line.includes('STARTUP_FAILURE'));
        assert.equal(failures.length, 1, `Expected exactly 1 STARTUP_FAILURE log, found ${failures.length}`);
    });
});

describeBlock('waitForLogContains prompt-failure assertion', () => {
    testRunner('fails promptly on STARTUP_FAILURE rather than waiting for the full timeout', async () => {
        const logPath = path.join(testTempDir, 'startup_fail.log');
        fs.writeFileSync(logPath, '[2026-10-18T12:00:00Z] PROXY STARTUP_FAILURE executable=\'/bin/azmcp\' error=spawn EACCES\n');

        const startTime = Date.now();
        await assert.rejects(
            () => waitForLogContains(logPath, 'may expose secrets', 30000),
            /MCP server startup failure detected in proxy log before "may expose secrets" was seen/
        );
        const elapsed = Date.now() - startTime;
        assert.ok(elapsed < 2000, `Expected failure within 2000ms, took ${elapsed}ms`);
    });

    testRunner('resolves successfully when needle is present in log', async () => {
        const logPath = path.join(testTempDir, 'success.log');
        fs.writeFileSync(logPath, '[2026-10-18T12:00:00Z] ELICITATION may expose secrets or sensitive information\n');

        await waitForLogContains(logPath, 'may expose secrets', 5000);
    });

    testRunner('times out when needle is not found and no failure is reported', async () => {
        const logPath = path.join(testTempDir, 'timeout.log');
        fs.writeFileSync(logPath, '[2026-10-18T12:00:00Z] PROXY normal operation\n');

        await assert.rejects(
            () => waitForLogContains(logPath, 'may expose secrets', 1500),
            /Timed out after 1500ms/
        );
    });
});
