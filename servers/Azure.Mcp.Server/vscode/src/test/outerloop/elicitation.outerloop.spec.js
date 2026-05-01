// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

'use strict';

const fs = require('node:fs/promises');
const os = require('node:os');
const path = require('node:path');

const { test, expect, _electron } = require('@playwright/test');
const { downloadAndUnzipVSCode } = require('@vscode/test-electron');

const MCP_SERVER_NAME = 'azure-mcp-latest';
const SECURITY_WARNING_TEXT = 'may expose secrets or sensitive information';
const TRIGGER_EXTENSION_SOURCE = path.join(__dirname, 'fixtures', 'trigger-extension');

test.describe('VS Code MCP elicitation outerloop', () => {
    test.describe.configure({ timeout: 10 * 60 * 1000 });

    test('installs latest Azure MCP server and shows elicitation UI for a real sensitive tool', async ({}, testInfo) => {
        await clearVsCodeDownloadCache();
        const vscodeExecutablePath = await downloadAndUnzipVSCode('stable');

        const tempRoot = await fs.mkdtemp(path.join(os.tmpdir(), 'mcp-vscode-outerloop-'));
        const workspacePath = path.join(tempRoot, 'workspace');
        const vscodeDir = path.join(workspacePath, '.vscode');
        const userDataDir = path.join(tempRoot, 'user-data');
        const extensionsDir = path.join(tempRoot, 'extensions');
        const artifactsDir = testInfo.outputPath('artifacts');

        await fs.mkdir(vscodeDir, { recursive: true });
        await fs.mkdir(userDataDir, { recursive: true });
        await fs.mkdir(extensionsDir, { recursive: true });
        await fs.mkdir(artifactsDir, { recursive: true });

        // Install the test-only trigger extension that programmatically calls
        // a sensitive MCP tool, which causes VS Code to show the elicitation UI.
        await installTriggerExtension(extensionsDir);

        const mcpSettings = {
            servers: {
                [MCP_SERVER_NAME]: {
                    command: 'npx',
                    args: ['-y', '@azure/mcp@latest', 'server', 'start', '--mode', 'all']
                }
            }
        };

        const workspaceSettings = {
            'chat.mcp.autostart': 'all'
        };

        await fs.writeFile(path.join(vscodeDir, 'mcp.json'), JSON.stringify(mcpSettings, null, 2), 'utf8');
        await fs.writeFile(path.join(vscodeDir, 'settings.json'), JSON.stringify(workspaceSettings, null, 2), 'utf8');

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

            // The trigger extension activates on startup and repeatedly tries to
            // invoke a sensitive MCP tool. As soon as VS Code finishes registering
            // the keyvault_secret_get tool, the invocation surfaces the elicitation
            // SECURITY WARNING card. Just wait for that text in any frame.
            await waitForElicitationText(window, SECURITY_WARNING_TEXT, 360000);
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

// Copies our test-only trigger extension into the VS Code extensions dir so it
// is loaded on startup. The extension calls vscode.lm.invokeTool() against a
// sensitive MCP tool, which causes VS Code to render the elicitation UI.
async function installTriggerExtension(extensionsDir) {
    const targetDir = path.join(extensionsDir, 'azure-mcp-tests.mcp-elicitation-trigger-0.0.1');
    await copyDir(TRIGGER_EXTENSION_SOURCE, targetDir);
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

// Polls the main frame and every (potentially nested) iframe for the elicitation
// SECURITY WARNING text. VS Code renders the elicitation card inside a webview, so a
// plain window.getByText() against the main frame never finds it.
async function waitForElicitationText(window, needle, timeoutMs) {
    const lowerNeedle = needle.toLowerCase();
    const deadline = Date.now() + timeoutMs;
    let lastError;

    while (Date.now() < deadline) {
        try {
            for (const frame of window.frames()) {
                let text;
                try {
                    text = await frame.evaluate(() => document.body ? document.body.innerText : '');
                } catch {
                    continue;
                }

                if (text && text.toLowerCase().includes(lowerNeedle)) {
                    return frame;
                }
            }
        } catch (err) {
            lastError = err;
        }

        await window.waitForTimeout(1000);
    }

    throw new Error(
        `Timed out after ${timeoutMs}ms waiting for elicitation text "${needle}" in any frame.` +
        (lastError ? ` Last error: ${lastError.message}` : '')
    );
}
