// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as vscode from 'vscode';

export async function delay(ms: number, token?: vscode.CancellationToken): Promise<void> {
    return new Promise<void>((resolve, reject) => {
        let cancellationListener: vscode.Disposable;

        const timeout = setTimeout(() => {
            cancellationListener?.dispose();
            resolve();
        }, ms);

        if (token) {
            cancellationListener = token.onCancellationRequested(() => {
                cancellationListener.dispose();
                clearTimeout(timeout);
                reject();
            });
        }
    });
}
