// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as path from 'path';

import { runTests, downloadAndUnzipVSCode } from '@vscode/test-electron';

export async function runExtensionTests(...testPath: string[] ) {
    try {
        const vsCodePath = await downloadAndUnzipVSCode('stable');

        // The folder containing the Extension Manifest package.json
        const extensionDevelopmentPath = path.resolve(__dirname, '..', '..');

        // The path to test runner
        const extensionTestsPath = path.resolve(__dirname, ...testPath);

        await runTests({
            vscodeExecutablePath: vsCodePath,
            extensionDevelopmentPath: extensionDevelopmentPath, 
            extensionTestsPath: extensionTestsPath,
            extensionTestsEnv: {
                DEBUGTELEMETRY: 'true',
            }
        });
    } catch (err) {
        console.error('Failed to run tests: ', err);
        process.exit(1);
    }
}
