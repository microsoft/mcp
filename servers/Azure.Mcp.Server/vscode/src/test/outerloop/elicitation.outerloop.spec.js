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
        const tempRoot = await fs.mkdtemp(path.join(os.tmpdir(), 'mcp-vscode-outerloop-'));

        // Scope the @vscode/test-electron download cache to this test's temp
        // directory so we don't touch shared caches under cwd or the user's
        // home directory. Set MCP_OUTERLOOP_CLEAR_SHARED_VSCODE_CACHE=1 to
        // opt in to the legacy behavior of also clearing those shared caches.
        const vscodeCachePath = path.join(tempRoot, 'vscode-test-cache');
        await fs.mkdir(vscodeCachePath, { recursive: true });
        if (process.env.MCP_OUTERLOOP_CLEAR_SHARED_VSCODE_CACHE === '1') {
            await clearSharedVsCodeDownloadCache();
        }
        // Use VS Code Insiders by default so the test doesn't fight with a
        // user's currently-running stable install (which can share an update
        // lock and cause "Code is currently being updated" failures on the
        // exact same build hash). Override with MCP_OUTERLOOP_VSCODE_VERSION
        // (e.g., 'stable', 'insiders', or a specific version like '1.118.1').
        const vscodeVersion = process.env.MCP_OUTERLOOP_VSCODE_VERSION || 'insiders';
        const vscodeExecutablePath = await downloadAndUnzipVSCode({
            version: vscodeVersion,
            cachePath: vscodeCachePath
        });

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

        // Install the test-only startup-trigger extension. It registers an
        // mcpServerDefinitionProvider via vscode.lm.registerMcpServerDefinitionProvider
        // (the supported public API in stable 1.118+) and then calls
        // workbench.mcp.startServer to spawn it.
        await installFixtureExtension(STARTUP_TRIGGER_SOURCE, extensionsDir, 'mcp-startup-trigger-0.0.1');

        const userSettingsDir = path.join(userDataDir, 'User');
        await fs.mkdir(userSettingsDir, { recursive: true });
        await fs.writeFile(
            path.join(userSettingsDir, 'settings.json'),
            JSON.stringify({
                'chat.mcp.enabled': true,
                'chat.mcp.access': 'any',
                'chat.mcp.autostart': 'newAndOutdated'
            }, null, 2),
            'utf8'
        );

        const app = await _electron.launch({
            executablePath: vscodeExecutablePath,
            args: [
                workspacePath,
                '--skip-welcome',
                '--skip-release-notes',
                '--disable-workspace-trust',
                '--no-sandbox',
                `--user-data-dir=${userDataDir}`,
                `--extensions-dir=${extensionsDir}`
            ],
            env: {
                ...process.env,
                ELECTRON_ENABLE_LOGGING: '1',
                MCP_TEST_PROXY_SCRIPT: PROXY_SCRIPT,
                MCP_TEST_PROXY_LOG: proxyLogPath,
                MCP_TEST_NODE_PATH: process.execPath
            }
        });

        // Tee Electron main-process stdout/stderr into the artifacts dir so we
        // can see why VS Code might fail to open a window in CI.
        try {
            const electronProc = app.process();
            const stdoutLog = fsSync.createWriteStream(path.join(artifactsDir, 'vscode-stdout.log'), { flags: 'a' });
            const stderrLog = fsSync.createWriteStream(path.join(artifactsDir, 'vscode-stderr.log'), { flags: 'a' });
            electronProc.stdout?.on('data', chunk => {
                const text = chunk.toString();
                stdoutLog.write(text);
                console.log(`[vscode stdout] ${text.trimEnd()}`);
            });
            electronProc.stderr?.on('data', chunk => {
                const text = chunk.toString();
                stderrLog.write(text);
                console.log(`[vscode stderr] ${text.trimEnd()}`);
            });
            electronProc.on('exit', (code, signal) =>
                console.log(`[vscode] electron process exited code=${code} signal=${signal}`)
            );
        } catch (procErr) {
            console.log(`[outerloop] failed to attach electron process listeners: ${procErr && procErr.message}`);
        }

        let window;
        let tracingStarted = false;
        let testFailed = false;
        try {
            window = await waitForWorkbenchWindow(app, 180000, artifactsDir);

            window.on('console', msg => console.log(`[vscode console] ${msg.type()}: ${msg.text()}`));
            window.on('pageerror', err => console.log(`[vscode pageerror] ${err.message}`));

            await window.context().tracing.start({ screenshots: true, snapshots: true, sources: true });
            tracingStarted = true;

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
            testFailed = true;
            if (window) {
                try {
                    await window.screenshot({ path: path.join(artifactsDir, 'failure.png'), fullPage: true });
                    const html = await window.content();
                    await fs.writeFile(path.join(artifactsDir, 'failure.html'), html, 'utf8');
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
            if (tracingStarted && window) {
                try {
                    await window.context().tracing.stop({
                        path: testFailed ? path.join(artifactsDir, 'trace.zip') : undefined
                    });
                } catch (traceErr) {
                    console.log(`[outerloop] Failed to stop tracing: ${traceErr && traceErr.message}`);
                }
            }
            await app.close();
            // On Windows the OS may still hold handles to the just-downloaded
            // VS Code archive for a moment after Electron exits. Retry the
            // recursive remove so we don't fail an otherwise-passing test.
            await fs.rm(tempRoot, { recursive: true, force: true, maxRetries: 10, retryDelay: 250 })
                .catch(err => console.log(`[outerloop] failed to remove tempRoot ${tempRoot}: ${err && err.message}`));
        }
    });
});

async function clearSharedVsCodeDownloadCache() {
    const cacheCandidates = [
        path.join(process.cwd(), '.vscode-test'),
        path.join(os.homedir(), '.vscode-test'),
        path.join(os.tmpdir(), 'vscode-test')
    ];

    await Promise.all(cacheCandidates.map(async cachePath => {
        await fs.rm(cachePath, { recursive: true, force: true });
    }));
}

async function waitForWorkbenchWindow(app, timeoutMs = 180000, artifactsDir) {
    const deadline = Date.now() + timeoutMs;
    // The workbench shell is identified by .monaco-workbench. Older/newer builds
    // also expose .part.editor or the body[data-vscode-window-kind="main"], so
    // accept any of them.
    const workbenchSelector = '.monaco-workbench, body[data-vscode-window-kind="main"]';
    let lastSeenWindowCount = -1;

    while (Date.now() < deadline) {
        const windows = app.windows();
        if (windows.length !== lastSeenWindowCount) {
            console.log(`[outerloop] waitForWorkbenchWindow: windows=${windows.length}`);
            lastSeenWindowCount = windows.length;
        }
        for (const candidate of windows) {
            try {
                const workbench = candidate.locator(workbenchSelector).first();
                await workbench.waitFor({ state: 'visible', timeout: 2000 });
                return candidate;
            } catch {
                // Not the workbench window yet (could be the shared/loader window).
            }
        }
        await app.waitForEvent('window', { timeout: 2000 }).catch(() => undefined);
    }

    // Capture diagnostics for every window we can see before failing.
    if (artifactsDir) {
        const windows = app.windows();
        console.log(`[outerloop] waitForWorkbenchWindow timing out with ${windows.length} window(s)`);
        for (let i = 0; i < windows.length; i++) {
            const w = windows[i];
            try {
                const url = w.url();
                const title = await w.title().catch(() => '<title unavailable>');
                console.log(`[outerloop] window[${i}] url=${url} title=${title}`);
                await w.screenshot({
                    path: path.join(artifactsDir, `timeout-window-${i}.png`),
                    fullPage: true
                }).catch(() => undefined);
                const html = await w.content().catch(() => '<content unavailable>');
                await fs.writeFile(
                    path.join(artifactsDir, `timeout-window-${i}.html`),
                    html,
                    'utf8'
                ).catch(() => undefined);
            } catch (err) {
                console.log(`[outerloop] window[${i}] capture failed: ${err && err.message}`);
            }
        }
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
