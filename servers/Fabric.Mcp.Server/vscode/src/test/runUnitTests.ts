// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { runExtensionTests } from "./runExtensionTests";

async function main() {
    try {
        await runExtensionTests('suite', 'unitTests');
    } catch (err) {
        console.error('Failed to run tests: ', err);
        process.exit(1);
    }
}

void main();
