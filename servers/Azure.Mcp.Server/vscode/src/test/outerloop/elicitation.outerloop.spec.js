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

    test('installs latest Azure MCP server and shows elicitation UI for a real sensitive tool', async () => {
        await clearVsCodeDownloadCache();
        const vscodeExecutablePath = await downloadAndUnzipVSCode('stable');

        const tempRoot = await fs.mkdtemp(path.join(os.tmpdir(), 'mcp-vscode-outerloop-'));
        const workspacePath = path.join(tempRoot, 'workspace');
        const vscodeDir = path.join(workspacePath, '.vscode');
        const userDataDir = path.join(tempRoot, 'user-data');
        const extensionsDir = path.join(tempRoot, 'extensions');

        await fs.mkdir(vscodeDir, { recursive: true });
        await fs.mkdir(userDataDir, { recursive: true });
        await fs.mkdir(extensionsDir, { recursive: true });

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

        try {
            const window = await waitForWorkbenchWindow(app);

            await runCommand(window, 'MCP: Run Tool');
            await selectFromQuickInput(window, MCP_SERVER_NAME);
            await selectFromQuickInput(window, TOOL_NAME);
            await fillCurrentQuickInput(window, 'test-subscription');
            await fillCurrentQuickInput(window, 'test-vault');

            await expect(window.getByText(SECURITY_WARNING_TEXT)).toBeVisible({ timeout: 120000 });
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

    const commandInput = window.locator('input[aria-label*="Type the name of a command"]');
    await expect(commandInput).toBeVisible({ timeout: 30000 });
    await commandInput.fill(commandName);
    await window.keyboard.press('Enter');
}

async function selectFromQuickInput(window, value) {
    const quickInput = window.locator('input[aria-label*="Type to narrow down"]');
    await expect(quickInput).toBeVisible({ timeout: 30000 });
    await quickInput.fill(value);
    await window.keyboard.press('Enter');
}

async function fillCurrentQuickInput(window, value) {
    const input = window.locator('.quick-input-widget input').last();
    await expect(input).toBeVisible({ timeout: 30000 });
    await input.fill(value);
    await window.keyboard.press('Enter');
}
