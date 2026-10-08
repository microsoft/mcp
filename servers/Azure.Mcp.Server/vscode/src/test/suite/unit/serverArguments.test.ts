// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import 'mocha';
import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';
import { buildServerArguments } from '../../../serverArguments';

suite('Azure MCP Extension - server arguments', () => {
    test('requires user-level opt-in for SSRF overrides', () => {
        const properties = readExtensionManifest().contributes.configuration.properties;
        const setting = properties['azureMcp.dangerouslyDisableSsrfProtectionsByNamespace'];

        assert.strictEqual(setting.scope, 'machine');
        assert.deepStrictEqual(setting.default, []);
    });

    test('matches enabled service choices and descriptions, with an ALL warning', () => {
        const manifest = readExtensionManifest();
        const properties = manifest.contributes.configuration.properties;
        const enabledServices = properties['azureMcp.enabledServices'].items;
        const ssrfOverrides = properties['azureMcp.dangerouslyDisableSsrfProtectionsByNamespace'].items;

        assert.deepStrictEqual(ssrfOverrides.enum, [...enabledServices.enum, 'ALL']);
        assert.deepStrictEqual(ssrfOverrides.markdownEnumDescriptions, [
            ...enabledServices.markdownEnumDescriptions,
            '**DANGER:** Disables SSRF protections for every Azure MCP tool namespace.'
        ]);
        assert.strictEqual(enabledServices.markdownEnumDescriptions.length, enabledServices.enum.length);
        assert.strictEqual(ssrfOverrides.markdownEnumDescriptions.length, ssrfOverrides.enum.length);
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
                scope?: string;
                default?: string[];
                items: {
                    enum: string[];
                    markdownEnumDescriptions: string[];
                };
            }>;
        };
    };
}
