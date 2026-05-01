// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Test-only extension. Forces VS Code to spawn the MCP server configured
// in the workspace mcp.json, since headless CI has no Copilot Chat to
// trigger lazy startup. We invoke every plausible MCP-start command, with
// and without the server-id argument. Whichever exists wins; the others
// are no-ops.

const vscode = require('vscode');

const SERVER_ID = 'azure-mcp-latest';

const COMMAND_VARIANTS = [
    ['workbench.mcp.startServer'],
    ['workbench.mcp.startServer', SERVER_ID],
    ['mcp.startServer'],
    ['mcp.startServer', SERVER_ID],
    ['workbench.mcp.listServer'],
    ['workbench.mcp.refreshServers'],
    ['mcp.refreshServers']
];

function log(message) {
    console.log(`[mcpStartupTrigger] ${message}`);
}

async function activate() {
    log('activated');
    try {
        const all = await vscode.commands.getCommands(true);
        log(`mcp commands: ${JSON.stringify(all.filter(c => /mcp/i.test(c)))}`);
    } catch (err) {
        log(`getCommands failed: ${err && err.message}`);
    }

    for (const variant of COMMAND_VARIANTS) {
        try {
            await vscode.commands.executeCommand(...variant);
            log(`executed: ${JSON.stringify(variant)}`);
        } catch (err) {
            log(`failed: ${JSON.stringify(variant)} -> ${err && err.message}`);
        }
    }

    // Re-trigger after a short delay in case the first attempt happened
    // before mcp.json discovery completed.
    setTimeout(async () => {
        for (const variant of COMMAND_VARIANTS) {
            try {
                await vscode.commands.executeCommand(...variant);
                log(`retry executed: ${JSON.stringify(variant)}`);
            } catch (err) {
                log(`retry failed: ${JSON.stringify(variant)} -> ${err && err.message}`);
            }
        }
    }, 10000);
}

function deactivate() { }

module.exports = { activate, deactivate };
