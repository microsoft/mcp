// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Test-only extension. Periodically:
//   1. Logs everything we can see (lm.tools, mcp-related commands).
//   2. Tries to nudge MCP server discovery via every plausible command.
//   3. Once a keyvault_secret_get-style tool is contributed, invokes it.
//      Tools annotated as secret/destructive cause VS Code to render the
//      elicitation SECURITY WARNING card, which is what the outerloop
//      Playwright test asserts on.

const vscode = require('vscode');

const POLL_INTERVAL_MS = 3000;
const MAX_DURATION_MS = 8 * 60 * 1000;

const NUDGE_COMMANDS = [
    'workbench.action.chat.open',
    'workbench.action.chat.newChat',
    'workbench.mcp.startServer',
    'workbench.mcp.listServer',
    'workbench.mcp.refreshServers',
    'mcp.startServer',
    'mcp.listServer',
    'mcp.refreshServers',
    'chat.mcp.start'
];

function log(message) {
    console.log(`[mcpElicitationTrigger] ${message}`);
}

async function listMcpCommands() {
    try {
        const all = await vscode.commands.getCommands(true);
        return all.filter(c => /mcp/i.test(c));
    } catch (err) {
        log(`getCommands failed: ${err && err.message}`);
        return [];
    }
}

function listTools() {
    const tools = (vscode.lm && Array.isArray(vscode.lm.tools)) ? vscode.lm.tools : [];
    return tools.map(t => (t && t.name) || '<unnamed>');
}

async function tryInvoke(toolName) {
    try {
        await vscode.lm.invokeTool(toolName, {
            input: {
                subscription: 'test-subscription',
                vault: 'test-vault',
                secret: 'test-secret'
            },
            toolInvocationToken: undefined
        });
        log(`invokeTool(${toolName}) returned (unexpected; elicitation should have surfaced)`);
        return true;
    } catch (err) {
        log(`invokeTool(${toolName}) failed: ${err && err.message}`);
        return false;
    }
}

async function pollLoop() {
    const deadline = Date.now() + MAX_DURATION_MS;
    let mcpCommandsLogged = false;

    while (Date.now() < deadline) {
        if (!mcpCommandsLogged) {
            const mcpCommands = await listMcpCommands();
            log(`mcp-related commands: ${JSON.stringify(mcpCommands)}`);
            mcpCommandsLogged = true;

            // Fire all plausible nudge commands once. Most will throw because
            // they don't exist; that's fine.
            for (const cmd of NUDGE_COMMANDS) {
                try {
                    await vscode.commands.executeCommand(cmd);
                    log(`nudge: executed ${cmd}`);
                } catch (err) {
                    log(`nudge: ${cmd} failed: ${err && err.message}`);
                }
            }
        }

        const toolNames = listTools();
        log(`lm.tools (${toolNames.length}): ${JSON.stringify(toolNames)}`);

        const candidates = toolNames.filter(n =>
            typeof n === 'string' &&
            n.toLowerCase().includes('keyvault') &&
            n.toLowerCase().includes('secret') &&
            n.toLowerCase().includes('get')
        );

        for (const name of candidates) {
            await tryInvoke(name);
        }

        await new Promise(resolve => setTimeout(resolve, POLL_INTERVAL_MS));
    }

    log('pollLoop deadline reached');
}

function activate(context) {
    log('activated');
    context.subscriptions.push(
        vscode.commands.registerCommand('mcpElicitationTrigger.run', () => pollLoop())
    );
    pollLoop();
}

function deactivate() { }

module.exports = { activate, deactivate };
