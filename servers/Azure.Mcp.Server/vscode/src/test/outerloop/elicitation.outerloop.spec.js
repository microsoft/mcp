// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Outerloop test: launches the latest stable VS Code, configures it with
// an mcp.json that points at our stdio proxy (./mcpProxy.js). The proxy
// spawns the real Azure MCP server (`npx @azure/mcp@latest`) and tees all
// JSON-RPC traffic to a log file. After VS Code completes its initialize
// handshake the proxy injects a tools/call for keyvault_secret_get; the
// server responds with an elicitation/create request whose message
// contains the SECURITY WARNING text we assert on.
//
// What this guards against (per maintainer intent: "vscode 没有 break 我们的
// elicitation behavior"):
//   * VS Code stable parses mcp.json and spawns the configured command.
//   * VS Code's MCP client completes the initialize / tools/list handshake.
//   * The Azure MCP server emits an elicitation/create request with the
//     expected SECURITY WARNING text when a tool annotated as `secret`
//     is called.
//
// What this does NOT cover: rendering of the elicitation card. That UI
// lives in GitHub Copilot Chat, which is not available in headless CI.

'use strict';

const fs = require('node:fs/promises');
const fsSync = require('node:fs');
const os = require('node:os');
const path = require('node:path');

const { test, expect, _electron } = require('@playwright/test');
const { downloadAndUnzipVSCode } = require('@vscode/test-electron');

const MCP_SERVER_NAME = 'azure-mcp-latest';
const SECURITY_WARNING_TEXT = 'may expose secrets or sensitive information';
const PROXY_SCRIPT = path.join(__dirname, 'mcpProxy.js');
const STARTUP_TRIGGER_SOURCE = path.join(__dirname, 'fixtures', 'startup-trigger');

test.describe('VS Code MCP elicitation outerloop', () => {
    test.describe.configure({ timeout: 10 * 60 * 1000 });

    test('VS Code spawns Azure MCP server and elicitation/create flows back with the SECURITY WARNING', async ({}, testInfo) => {
        await clearVsCodeDownloadCache();
        const vscodeExecutablePath = await downloadAndUnzipVSCode('stable');

        const tempRoot = await fs.mkdtemp(path.join(os.tmpdir(), 'mcp-vscode-outerloop-'));
        const workspacePath = path.join(tempRoot, 'workspace');
        const vscodeDir = path.join(workspacePath, '.vscode');
        const userDataDir = path.join(tempRoot, 'user-data');
        const extensionsDir = path.join(tempRoot, 'extensions');
        const artifactsDir = testInfo.outputPath('artifacts');
        const proxyLogPath = path.join(artifactsDir, 'mcp-proxy.log');

        await fs.mkdir(vscodeDir, { recursive: true });
        await fs.mkdir(userDataDir, { recursive: true });
        await fs.mkdir(extensionsDir, { recursive: true });
        await fs.mkdir(artifactsDir, { recursive: true });
        await fs.writeFile(proxyLogPath, '', 'utf8');

        // Install the test-only startup-trigger extension so workbench.mcp.startServer
        // is invoked from the Extension Host (CI has no Copilot Chat to lazily start it).
        await installFixtureExtension(STARTUP_TRIGGER_SOURCE, extensionsDir, 'mcp-startup-trigger-0.0.1');

        // Some VS Code builds gate MCP discovery behind settings; flip every plausible
        // toggle on, in user settings, so the workspace mcp.json is honored.
        const userSettingsDir = path.join(userDataDir, 'User');
        await fs.mkdir(userSettingsDir, { recursive: true });
        await fs.writeFile(
            path.join(userSettingsDir, 'settings.json'),
            JSON.stringify({
                'chat.mcp.enabled': true,
                'chat.mcp.discovery.enabled': true,
                'chat.mcp.autostart': 'all'
            }, null, 2),
            'utf8'
        );

        const mcpSettings = {
            servers: {
                [MCP_SERVER_NAME]: {
                    command: process.execPath,
                    args: [PROXY_SCRIPT],
                    env: { MCP_PROXY_LOG: proxyLogPath }
                }
            }
        };

        await fs.writeFile(
            path.join(vscodeDir, 'mcp.json'),
            JSON.stringify(mcpSettings, null, 2),
            'utf8'
        );

        const app = await _electron.launch({
            executablePath: vscodeExecutablePath,
            args: [
                workspacePath,
                '--skip-welcome',
                '--skip-release-notes',
                '--disable-workspace-trust',
                `--user-data-dir=${userDataDir}`,
                `--extensions-dir=${extensionsDir}`
            ]
        });

        let window;
        try {
            window = await waitForWorkbenchWindow(app);

            window.on('console', msg => console.log(`[vscode console] ${msg.type()}: ${msg.text()}`));
            window.on('pageerror', err => console.log(`[vscode pageerror] ${err.message}`));

            await window.context().tracing.start({ screenshots: true, snapshots: true, sources: true });

            // The startup-trigger extension fires workbench.mcp.startServer from
            // onStartupFinished. VS Code prompts the user to trust the MCP server
            // before spawning it; in CI nobody is there to click "Allow", so we
            // run a background loop that auto-accepts any such prompt.
            const dismissTrustPrompts = autoAcceptTrustPrompts(window, artifactsDir);
            try {
                await waitForLogContains(proxyLogPath, SECURITY_WARNING_TEXT, 6 * 60 * 1000);
            } finally {
                dismissTrustPrompts.stop();
            }
        } catch (err) {
            if (window) {
                try {
                    await window.screenshot({ path: path.join(artifactsDir, 'failure.png'), fullPage: true });
                    const html = await window.content();
                    await fs.writeFile(path.join(artifactsDir, 'failure.html'), html, 'utf8');
                    await window.context().tracing.stop({ path: path.join(artifactsDir, 'trace.zip') });
                } catch (captureErr) {
                    console.log(`[outerloop] Failed to capture diagnostics: ${captureErr.message}`);
                }
            }
            // Always copy the proxy log alongside other artifacts (it lives there already,
            // but log a tail to stdout for quick CI visibility).
            try {
                const tail = await readTail(proxyLogPath, 8000);
                console.log(`[outerloop] mcp-proxy.log tail:\n${tail}`);
            } catch { /* ignore */ }
            throw err;
        } finally {
            await app.close();
        }
    });
});

async function clearVsCodeDownloadCache() {
    const cacheCandidates = [
        path.join(process.cwd(), '.vscode-test'),
        path.join(os.homedir(), '.vscode-test'),
        path.join(os.tmpdir(), 'vscode-test')
    ];

    await Promise.all(cacheCandidates.map(async cachePath => {
        await fs.rm(cachePath, { recursive: true, force: true });
    }));
}

async function waitForWorkbenchWindow(app, timeoutMs = 120000) {
    const deadline = Date.now() + timeoutMs;

    while (Date.now() < deadline) {
        const windows = app.windows();
        for (const candidate of windows) {
            try {
                const workbench = candidate.locator('.monaco-workbench');
                await workbench.waitFor({ state: 'visible', timeout: 2000 });
                return candidate;
            } catch {
                // Not the workbench window yet (could be the shared/loader window).
            }
        }
        await app.waitForEvent('window', { timeout: 2000 }).catch(() => undefined);
    }

    throw new Error(`Timed out waiting for VS Code workbench window after ${timeoutMs}ms`);
}

async function installFixtureExtension(sourceDir, extensionsDir, targetName) {
    const targetDir = path.join(extensionsDir, `azure-mcp-tests.${targetName}`);
    await copyDir(sourceDir, targetDir);
}

async function copyDir(src, dest) {
    await fs.mkdir(dest, { recursive: true });
    const entries = await fs.readdir(src, { withFileTypes: true });
    for (const entry of entries) {
        const srcPath = path.join(src, entry.name);
        const destPath = path.join(dest, entry.name);
        if (entry.isDirectory()) {
            await copyDir(srcPath, destPath);
        } else if (entry.isFile()) {
            await fs.copyFile(srcPath, destPath);
        }
    }
}

async function waitForLogContains(logPath, needle, timeoutMs) {
    const lowerNeedle = needle.toLowerCase();
    const deadline = Date.now() + timeoutMs;

    while (Date.now() < deadline) {
        try {
            const contents = fsSync.readFileSync(logPath, 'utf8');
            if (contents.toLowerCase().includes(lowerNeedle)) {
                return;
            }
        } catch { /* file may not exist yet */ }
        await sleep(1000);
    }

    let tail = '';
    try { tail = await readTail(logPath, 4000); } catch { /* ignore */ }
    throw new Error(
        `Timed out after ${timeoutMs}ms waiting for "${needle}" in ${logPath}.\n` +
        `Log tail:\n${tail}`
    );
}

async function readTail(filePath, maxBytes) {
    const data = await fs.readFile(filePath, 'utf8');
    if (data.length <= maxBytes) return data;
    return data.slice(data.length - maxBytes);
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

// VS Code prompts the user to trust an MCP server before spawning it. In headless
// CI we can't click "Allow", so this helper polls the workbench every second for
// any quick-pick whose entries include a permissive option (Allow / Trust / Yes /
// Continue) or any modal dialog with the same, and clicks/enters it. We also
// snapshot screenshots periodically so we can see what the workbench looked like
// when we attempted to dismiss prompts.
function autoAcceptTrustPrompts(window, artifactsDir) {
    let stopped = false;
    let snapshotCounter = 0;

    const matchers = [
        /\ballow\b/i,
        /\btrust\b/i,
        /\byes\b/i,
        /\bcontinue\b/i,
        /\bproceed\b/i,
        /\bstart\b/i
    ];

    const loop = (async () => {
        while (!stopped) {
            try {
                // Periodic screenshots: every 10s.
                if (snapshotCounter % 10 === 0) {
                    try {
                        await window.screenshot({
                            path: path.join(artifactsDir, `state-${String(snapshotCounter).padStart(3, '0')}s.png`),
                            fullPage: true
                        });
                    } catch { /* ignore */ }
                }
                snapshotCounter += 1;

                // 1) Quick-pick / quick-input widget.
                const quickInput = window.locator('.quick-input-widget:visible').first();
                if (await quickInput.count() > 0 && await quickInput.isVisible().catch(() => false)) {
                    const rows = quickInput.locator('.quick-input-list .monaco-list-row');
                    const count = await rows.count().catch(() => 0);
                    let clicked = false;
                    for (let i = 0; i < count; i++) {
                        const text = (await rows.nth(i).innerText().catch(() => '')) || '';
                        if (matchers.some(re => re.test(text))) {
                            console.log(`[outerloop] auto-accept quick-pick option: ${text.replace(/\s+/g, ' ').trim()}`);
                            await rows.nth(i).click({ timeout: 5000 }).catch(() => undefined);
                            clicked = true;
                            break;
                        }
                    }
                    if (!clicked) {
                        // Press Enter on the default highlighted option as a fallback.
                        await window.keyboard.press('Enter').catch(() => undefined);
                    }
                }

                // 2) Modal dialog (dialog-shadow).
                const dialog = window.locator('.monaco-dialog-box, .dialog-shadow .dialog-modal-block').first();
                if (await dialog.count() > 0 && await dialog.isVisible().catch(() => false)) {
                    const buttons = dialog.locator('button, .monaco-button');
                    const bcount = await buttons.count().catch(() => 0);
                    for (let i = 0; i < bcount; i++) {
                        const text = (await buttons.nth(i).innerText().catch(() => '')) || '';
                        if (matchers.some(re => re.test(text))) {
                            console.log(`[outerloop] auto-accept dialog button: ${text.replace(/\s+/g, ' ').trim()}`);
                            await buttons.nth(i).click({ timeout: 5000 }).catch(() => undefined);
                            break;
                        }
                    }
                }

                // 3) Toast notification with action buttons.
                const toastButtons = window.locator('.notification-toast-container button.monaco-button');
                const tcount = await toastButtons.count().catch(() => 0);
                for (let i = 0; i < tcount; i++) {
                    const text = (await toastButtons.nth(i).innerText().catch(() => '')) || '';
                    if (matchers.some(re => re.test(text))) {
                        console.log(`[outerloop] auto-accept notification button: ${text.replace(/\s+/g, ' ').trim()}`);
                        await toastButtons.nth(i).click({ timeout: 5000 }).catch(() => undefined);
                        break;
                    }
                }
            } catch (err) {
                console.log(`[outerloop] autoAcceptTrustPrompts iteration error: ${err && err.message}`);
            }

            await sleep(1000);
        }
    })();

    return {
        stop() {
            stopped = true;
            return loop;
        }
    };
}
