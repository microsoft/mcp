// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import 'mocha';
import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';

suite('Fabric MCP Extension - configuration', () => {
    const manifestPath = path.resolve(__dirname, '..', '..', '..', '..', 'package.json');
    const settings = JSON.parse(fs.readFileSync(manifestPath, 'utf8')).contributes.configuration.properties;
    const enabledServices = settings['fabricMcp.enabledServices'];

    test('offers all current Fabric namespaces including Jobs', () => {
        assert.deepStrictEqual(enabledServices.items.enum, ['core', 'jobs', 'docs', 'onelake', 'datafactory']);
        assert.strictEqual(enabledServices.uniqueItems, true);
    });

    test('describes each namespace in the same order as its choice', () => {
        const descriptions = enabledServices.items.markdownEnumDescriptions;
        const labels = ['Core', 'Jobs', 'Documentation', 'OneLake', 'Data Factory'];

        assert.strictEqual(descriptions.length, labels.length);
        labels.forEach((label, index) => {
            assert.ok(descriptions[index].startsWith(label), `Missing description for ${label}`);
        });
        assert.ok(descriptions[1].includes('Submit one on-demand item job, request cancellation, read/list job instances, or get/list/create/update/delete item schedules'));
        assert.ok(descriptions[1].includes('acceptance, creation/update, deletion receipts, and retry guidance'));
        assert.ok(descriptions[1].includes('no polling or automatic retries'));
        assert.ok(descriptions[1].includes('Each listing returns one page; request continuation explicitly'));
    });

    test('keeps an empty namespace selection available to enable all toolsets', () => {
        assert.strictEqual(enabledServices.type, 'array');
        assert.strictEqual(enabledServices.minItems ?? 0, 0);
        assert.deepStrictEqual(enabledServices.default ?? [], []);
        assert.ok(enabledServices.markdownDescription.includes('Leave empty to enable all'));
    });

    test('preserves server mode and read-only choices and defaults', () => {
        const serverMode = settings['fabricMcp.serverMode'];
        const readOnly = settings['fabricMcp.readOnly'];

        assert.deepStrictEqual(serverMode.enum, ['single', 'namespace', 'all']);
        assert.strictEqual(serverMode.default, 'all');
        assert.strictEqual(readOnly.type, 'boolean');
        assert.strictEqual(readOnly.default, false);
    });
});
