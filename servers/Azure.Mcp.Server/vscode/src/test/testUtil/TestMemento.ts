// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import * as vscode from 'vscode';

export class TestMemento implements vscode.Memento {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    private readonly values: Map<string, any> = new Map<string, any>();

    keys(): readonly string[] {
        return Array.from(this.values.keys());
    }

    get<T>(key: string, defaultValue?: T): T | undefined {
        return this.values.get(key) ?? defaultValue;
    }

    update(key: string, value: unknown): Thenable<void> {
        this.values.set(key, value);
        return Promise.resolve();
    }
}
