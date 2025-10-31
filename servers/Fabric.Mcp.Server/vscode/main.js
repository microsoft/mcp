// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

"use strict";

/* eslint-disable no-undef */ // Ignore the fact that the engine (which is VSCode) is unknown by the linter

// This is the extension entrypoint, which imports extension.bundle.js, the actual extension code.
// This is in a separate file so we can properly measure extension.bundle.js load time.

const loadStats = {
    loadStartTime: Date.now(),
    loadEndTime: undefined
};

Object.defineProperty(exports, "__esModule", { value: true });

// eslint-disable-next-line @typescript-eslint/no-var-requires
const extension = require("./dist/extension");

async function activate(ctx) {
    return await extension.activate(ctx, loadStats);
}

async function deactivate(ctx) {
    return await extension.deactivate(ctx);
}

exports.activate = activate;
exports.deactivate = deactivate;

/* eslint-enable no-undef */
