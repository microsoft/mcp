# Manual Testing Guide

This guide describes how to manually test the Azure MCP Server end-to-end to verify that tools are invoked correctly by MCP clients.

## Testing Environment Setup

| Setting | Value |
|---------|-------|
| **OS** | Windows (unless otherwise specified) |
| **VS Code** | Latest stable version |
| **Mode** | Agent |
| **Model** | Claude Sonnet 4.5 (or latest recommended) |
| **Azure MCP version** | Latest version (unless otherwise specified) |

### Tool Selection

Configure the following tools depending on the server mode being tested:

**With Virtual Tool On:**
- Built-in tools
- Extension: GitHub Copilot for Azure
- MCP Server: Azure MCP
- GitHub MCP Server

**With Virtual Tool Off:**
- Built-in tools
- Extension: GitHub Copilot for Azure
- MCP Server: Azure MCP

## Server Setup

Refer to the instructions in [CONTRIBUTING.md — Server Modes](../CONTRIBUTING.md#server-modes) on how to configure the Azure MCP Server with the desired mode.

### Consolidated Mode

To test in consolidated mode, add the following setting to your VS Code `settings.json` and verify that you can see the consolidated tools:

```json
{
    "azureMcp.serverMode": "consolidated"
}
```

## Tests to Execute

### All or Namespace Mode

Run through all the test prompts listed in [e2eTestPrompts.md](../servers/Azure.Mcp.Server/docs/e2eTestPrompts.md).

For each prompt:

1. Enter the prompt in GitHub Copilot Chat (Agent mode).
2. Verify that the **expected tool** is invoked (check the tool name in the Copilot response).
3. Verify that the tool **executes without error** and returns a valid response.
4. Note any cases where the wrong tool is invoked or the tool fails.

### Consolidated Mode

Run through all the test prompts listed in [consolidated-prompts.json](../eng/tools/ToolDescriptionEvaluator/prompts/consolidated-prompts.json).

This file contains the mapping between tool names and their corresponding test prompts. For each entry:

1. Enter the test prompt in GitHub Copilot Chat (Agent mode).
2. Verify that the **mapped consolidated tool** is invoked.
3. Verify that the tool **executes without error**.

## Validation Criteria

| Criteria | Expected Result |
|----------|----------------|
| Correct tool invoked | The tool listed in the test prompt table is the one selected by the MCP client |
| No execution errors | The tool runs to completion without returning an error |
| Valid response | The response contains meaningful data relevant to the prompt |

## Reporting Results

When reporting manual test results, include:

- **Environment**: OS, VS Code version, Azure MCP version, server mode
- **Pass/Fail**: Whether the expected tool was invoked and ran without error
- **Tool mismatch**: If a different tool was invoked, note which tool was selected instead
- **Errors**: Full error messages for any failures
