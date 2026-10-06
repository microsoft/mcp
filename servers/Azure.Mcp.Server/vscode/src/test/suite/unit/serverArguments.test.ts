// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import 'mocha';
import * as assert from 'assert';
import { buildServerArguments } from '../../../serverArguments';

suite('Azure MCP Extension - server arguments', () => {
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
