// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Shared logging and log-polling utilities for outerloop tests.
// Pure helper module with no test runner declarations or side effects.

'use strict';

const fs = require('node:fs/promises');
const fsSync = require('node:fs');

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

async function readTail(filePath, maxBytes) {
    const data = await fs.readFile(filePath, 'utf8');
    if (data.length <= maxBytes) return data;
    return data.slice(data.length - maxBytes);
}

async function waitForLogContains(logPath, needle, timeoutMs) {
    const lowerNeedle = needle.toLowerCase();
    const FAILURE_SENTINEL = 'STARTUP_FAILURE';
    const deadline = Date.now() + timeoutMs;

    while (Date.now() < deadline) {
        try {
            const contents = fsSync.readFileSync(logPath, 'utf8');
            if (contents.toLowerCase().includes(lowerNeedle)) {
                return;
            }
            // Fail immediately if the proxy reported a startup failure rather
            // than waiting for the full timeout.
            if (contents.includes(FAILURE_SENTINEL)) {
                let tail = '';
                try { tail = await readTail(logPath, 4000); } catch { /* ignore */ }
                throw new Error(
                    `MCP server startup failure detected in proxy log before "${needle}" was seen.\n` +
                    `Log tail:\n${tail}`
                );
            }
        } catch (err) {
            // Re-throw errors we generated above; swallow OS read errors
            // (file may not exist yet when the test first runs).
            if (err.message && (err.message.startsWith('MCP server startup failure') || err.message.startsWith('Timed out'))) {
                throw err;
            }
        }
        await sleep(1000);
    }

    let tail = '';
    try { tail = await readTail(logPath, 4000); } catch { /* ignore */ }
    throw new Error(
        `Timed out after ${timeoutMs}ms waiting for "${needle}" in ${logPath}.\n` +
        `Log tail:\n${tail}`
    );
}

module.exports = {
    waitForLogContains,
    readTail,
    sleep
};
