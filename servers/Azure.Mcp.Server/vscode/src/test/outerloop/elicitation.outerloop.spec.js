// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

'use strict';

const fs = require('node:fs/promises');
const os = require('node:os');
const path = require('node:path');

const { test, expect, _electron } = require('@playwright/test');
const { downloadAndUnzipVSCode } = require('@vscode/test-electron');

const MCP_SERVER_NAME = 'azure-mcp-latest';
const TOOL_NAME = 'keyvault secret get';
const SECURITY_WARNING_TEXT = 'may expose secrets or sensitive information';

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

            // Give VS Code a moment to discover the MCP server config and start npx download.
            await window.waitForTimeout(5000);

            await runCommand(window, 'MCP: Run Tool');
            await selectFromQuickInput(window, MCP_SERVER_NAME);
            await selectFromQuickInput(window, TOOL_NAME);
            await fillCurrentQuickInput(window, 'test-subscription');
            await fillCurrentQuickInput(window, 'test-vault');

            // The elicitation card is rendered inside a webview iframe (see screenshot in PR).
            // Search across the main frame and every iframe (including nested) until the
            // SECURITY WARNING text appears.
            await waitForElicitationText(window, SECURITY_WARNING_TEXT, 180000);
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

async function runCommand(window, commandName) {
    const commandPaletteShortcut = process.platform === 'darwin' ? 'Meta+Shift+P' : 'Control+Shift+P';
    await window.keyboard.press(commandPaletteShortcut);
    await fillActiveQuickInput(window, commandName);
}

async function selectFromQuickInput(window, value) {
    await fillActiveQuickInput(window, value);
}

async function fillCurrentQuickInput(window, value) {
    await fillActiveQuickInput(window, value);
}

async function fillActiveQuickInput(window, value) {
    const widget = window.locator('.quick-input-widget:visible').first();
    await widget.waitFor({ state: 'visible', timeout: 30000 });

    const input = widget.locator('input.input, input').first();
    await expect(input).toBeVisible({ timeout: 30000 });
    await input.fill('');
    await input.type(value, { delay: 10 });
    await window.keyboard.press('Enter');
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
