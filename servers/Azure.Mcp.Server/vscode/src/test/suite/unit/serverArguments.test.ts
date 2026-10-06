// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import 'mocha';
import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';
import { buildServerArguments } from '../../../serverArguments';

suite('Azure MCP Extension - server arguments', () => {
    test('offers every enabled service namespace plus ALL for SSRF overrides', () => {
        const manifest = readExtensionManifest();
        const properties = manifest.contributes.configuration.properties;
        const enabledNamespaces = properties['azureMcp.enabledServices'].items.enum;
        const dangerouslyDisabledNamespaces =
            properties['azureMcp.dangerouslyDisableSsrfProtectionsByNamespace'].items.enum;

        assert.deepStrictEqual(dangerouslyDisabledNamespaces, [...enabledNamespaces, 'ALL']);
    });

    test('keeps SSRF protections enabled by default', () => {
        const args = buildServerArguments(createConfiguration({}));

        assert.deepStrictEqual(args, ['server', 'start', '--mode', 'namespace']);
    });

    test('forwards each namespace with SSRF protections disabled', () => {
        const args = buildServerArguments(createConfiguration({
            serverMode: 'all',
            enabledServices: ['storage', 'keyvault'],
            readOnly: true,
            dangerouslyDisableSsrfProtectionsByNamespace: ['storage', 'ALL'],
            dangerouslyWriteSupportLogsToDir: 'C:\\logs'
        }));

        assert.deepStrictEqual(args, [
            'server',
            'start',
            '--mode',
            'all',
            '--namespace',
            'storage',
            '--namespace',
            'keyvault',
            '--read-only',
            '--dangerously-disable-ssrf-protections-by-namespace',
            'storage',
            '--dangerously-disable-ssrf-protections-by-namespace',
            'ALL',
            '--dangerously-write-support-logs-to-dir',
            'C:\\logs'
        ]);
    });
});

function createConfiguration(values: Readonly<Record<string, unknown>>) {
    return {
        get<T>(section: string): T | undefined {
            return values[section] as T | undefined;
        }
    };
}

function readExtensionManifest(): ExtensionManifest {
    const manifestPath = path.resolve(__dirname, '..', '..', '..', '..', 'package.json');
    return JSON.parse(fs.readFileSync(manifestPath, 'utf8')) as ExtensionManifest;
}

interface ExtensionManifest {
    contributes: {
        configuration: {
            properties: Record<string, {
                items: {
                    enum: string[];
                };
            }>;
        };
    };
}
