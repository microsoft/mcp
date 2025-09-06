// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';

export function activate(context: vscode.ExtensionContext) {
    const didChangeEmitter = new vscode.EventEmitter<void>();

    // Function to determine binary path based on configuration
    function getBinaryPath(): string {
        let binary = '';
        const arch = process.arch;
        if (process.platform === 'win32') {
            if (arch === 'x64' || arch === 'arm64') {
                binary = 'azmcp.exe';
            } else {
                throw new Error('Unsupported Windows architecture: ' + arch);
            }
        } else if (process.platform === 'darwin' || process.platform === 'linux') {
            if (arch === 'x64' || arch === 'arm64') {
                binary = 'azmcp';
            } else {
                throw new Error(`Unsupported ${process.platform} architecture: ${arch}`);
            }
        } else {
            throw new Error('Unsupported platform: ' + process.platform);
        }

        // Check if user wants to use native binary
        const config = vscode.workspace.getConfiguration('azureMcp');
        const useNativeBinary = config.get<boolean>('useNativeBinary') === true;

        let binPath: string;
        if (useNativeBinary) {
            // Use native binary from server/native folder
            binPath = path.join(context.extensionPath, 'server', 'native', binary);
            if (!fs.existsSync(binPath)) {
                // Fallback to regular server folder if native binary doesn't exist
                console.warn(`Native binary not found at ${binPath}, falling back to regular binary`);
                binPath = path.join(context.extensionPath, 'server', binary);
            }
        } else {
            // Use regular binary from server folder
            binPath = path.join(context.extensionPath, 'server', binary);
        }

        if (!fs.existsSync(binPath)) {
            console.warn("Azure MCP Server binary not found.");
        }

        // Ensure executable permission on macOS and Linux
        if ((process.platform === 'linux' || process.platform === 'darwin') && fs.existsSync(binPath)) {
            try {
                fs.chmodSync(binPath, 0o755);
            } catch (e) {
                console.warn(`Failed to set executable permission on ${binPath}: ${e}`);
            }
        }

        return binPath;
    }

    context.subscriptions.push(
        vscode.lm.registerMcpServerDefinitionProvider('azureMcpProvider', {
            onDidChangeMcpServerDefinitions: didChangeEmitter.event,
            provideMcpServerDefinitions: async () => {
                // Get the binary path based on current configuration
                const binPath = getBinaryPath();

                // Read enabled MCP services from user/workspace settings
                const config = vscode.workspace.getConfiguration('azureMcp');
                // Example: ["storage", "keyvault", ...]
                const enabledServices: string[] | undefined = config.get('enabledServices');
                const args = ['server', 'start'];

                // Server Mode (single | namespace | all). Default 'namespace'.
                const mode = config.get<string>('serverMode') || 'namespace';
                if (mode) {
                    args.push('--mode', mode);
                }

                // Namespaces filter
                if (enabledServices && Array.isArray(enabledServices) && enabledServices.length > 0) {
                    for (const svc of enabledServices) {
                        args.push('--namespace', svc);
                    }
                }

                // Read-only flag
                const readOnly = config.get<boolean>('readOnly') === true;
                if (readOnly) {
                    args.push('--read-only');
                }


                // Honor VS Code telemetry settings
                // Only set AZURE_MCP_COLLECT_TELEMETRY if telemetry is disabled
                const env: Record<string, string | number | null> = {};
                if (!vscode.env.isTelemetryEnabled) {
                    env.AZURE_MCP_COLLECT_TELEMETRY = 'false';
                }

                return [
                    new vscode.McpStdioServerDefinition(
                        'Azure MCP',
                        binPath,
                        args,
                        env
                    )
                ];
            },
            resolveMcpServerDefinition: async (server: vscode.McpServerDefinition) => {
                // Optionally prompt for secrets or do other setup here
                return server;
            }
        })
    );

    // Listen for changes to azureMcp.enabledServices and re-register MCP server
    context.subscriptions.push(
        vscode.workspace.onDidChangeConfiguration((event: vscode.ConfigurationChangeEvent) => {
            if (
                event.affectsConfiguration('azureMcp.enabledServices') ||
                event.affectsConfiguration('azureMcp.serverMode') ||
                event.affectsConfiguration('azureMcp.readOnly') ||
                event.affectsConfiguration('azureMcp.useNativeBinary')
            ) {
                didChangeEmitter.fire();
            }
        })
    );

    // Listen for changes to VS Code telemetry settings and re-register MCP server
    context.subscriptions.push(
        vscode.env.onDidChangeTelemetryEnabled(() => {
            void vscode.window.showInformationMessage(
                'VS Code telemetry setting changed. Re-registering the MCP server.'
            );
            didChangeEmitter.fire();
        })
    );
}

export function deactivate() {
    // No process management needed; VS Code will handle server lifecycle
}
