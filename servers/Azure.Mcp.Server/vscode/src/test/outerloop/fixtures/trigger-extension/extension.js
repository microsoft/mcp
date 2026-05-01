// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Test-only extension. Repeatedly tries to invoke a sensitive MCP tool
// (keyvault_secret_get) until VS Code has finished discovering MCP servers
// and tools. Invoking a tool with `secret: true` causes VS Code to render
// the elicitation SECURITY WARNING card, which is what the outerloop
// Playwright test asserts on.

const vscode = require('vscode');

// Possible tool ids depending on how VS Code maps MCP tool names. We try
// each on every poll iteration.
const CANDIDATE_TOOL_NAMES = [
    'keyvault_secret_get',
    'azure-mcp-latest_keyvault_secret_get',
    'mcp_azure-mcp-latest_keyvault_secret_get'
];

const POLL_INTERVAL_MS = 2000;
const MAX_DURATION_MS = 5 * 60 * 1000;

async function tryInvoke(toolName) {
    try {
        // The tool will eventually be registered after the MCP server starts.
        // Calling invokeTool with required input fields triggers VS Code's
        // elicitation UI for tools annotated with secret/destructive.
        await vscode.lm.invokeTool(toolName, {
            input: {
                subscription: 'test-subscription',
                vault: 'test-vault',
                secret: 'test-secret'
            },
            toolInvocationToken: undefined
        });
        return true;
    } catch (err) {
        // Tool not yet available, validation failed before elicitation, or
        // the user/test rejected it. Either way, log and keep polling — the
        // act of *attempting* the call is what surfaces the elicitation UI.
        console.log(`[mcpElicitationTrigger] invokeTool(${toolName}) failed: ${err && err.message}`);
        return false;
    }
}

async function pollLoop() {
    const deadline = Date.now() + MAX_DURATION_MS;
    while (Date.now() < deadline) {
        const tools = (vscode.lm && vscode.lm.tools) ? vscode.lm.tools : [];
        const matched = tools
            .map(t => t && t.name)
            .filter(n => typeof n === 'string' && n.toLowerCase().includes('keyvault') && n.toLowerCase().includes('secret') && n.toLowerCase().includes('get'));

        const targets = matched.length > 0 ? matched : CANDIDATE_TOOL_NAMES;
        for (const name of targets) {
            await tryInvoke(name);
        }

        await new Promise(resolve => setTimeout(resolve, POLL_INTERVAL_MS));
    }
}

function activate(context) {
    console.log('[mcpElicitationTrigger] activated');
    context.subscriptions.push(
        vscode.commands.registerCommand('mcpElicitationTrigger.run', () => pollLoop())
    );
    // Fire and forget. Failures are logged inside tryInvoke.
    pollLoop();
}

function deactivate() { }

module.exports = { activate, deactivate };
