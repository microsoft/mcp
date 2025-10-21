// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import 'mocha';
import * as assert from 'assert';
import * as sinon from 'sinon';
import * as vscode from 'vscode';
import * as path from 'path';
import { activate } from '../../../extension';

suite('Azure MCP Extension - activate', () => {
    let context: vscode.ExtensionContext;
    let registerStub: sinon.SinonStub;
    let originalPlatform: string;


    setup(() => {
        // Mock ExtensionContext
        context = {
            subscriptions: [],
            extensionPath: '/ext/path',
            // ...other properties can be added as needed
        } as unknown as vscode.ExtensionContext;

        // Mock the MCP API class to expose executablePath for assertions
        class MockMcpStdioServerDefinition {
            executablePath: string;
            constructor(id: string, executablePath: string, args: string[]) {
                this.executablePath = executablePath;
            }
        }
        // Assign the mock class to vscode after it is defined
        (vscode as unknown as { McpStdioServerDefinition: typeof MockMcpStdioServerDefinition }).McpStdioServerDefinition = MockMcpStdioServerDefinition;

        // Stub the MCP API on vscode.lm
        const fakeDisposable = { dispose: sinon.stub() };
        (vscode as unknown as { lm: { registerMcpServerDefinitionProvider: sinon.SinonStub } }).lm = {
            registerMcpServerDefinitionProvider: sinon.stub().returns(fakeDisposable)
        };
        registerStub = (vscode as unknown as { lm: { registerMcpServerDefinitionProvider: sinon.SinonStub } }).lm.registerMcpServerDefinitionProvider;

        // Stub EventEmitter
        // Stub EventEmitter
        sinon.stub(vscode, 'EventEmitter').callsFake(() => {
            return {
                event: ((listener: (e: unknown) => unknown, thisArgs?: unknown, disposables?: vscode.Disposable[]) => {
                    return { dispose: () => { } };
                }) as vscode.Event<unknown>,
                dispose: sinon.stub()
            } as Pick<vscode.EventEmitter<unknown>, 'event' | 'dispose'>;
        });
        // Save and override process.platform
        originalPlatform = process.platform;
    });

    teardown(() => {
        sinon.restore();
        delete ((vscode as unknown) as { lm?: unknown }).lm;
        Object.defineProperty(process, 'platform', { value: originalPlatform });
    });

    test('registers the MCP server provider and disposables', async () => {
        await activate(context);
        assert.ok(registerStub.calledOnce, 'registerMcpServerDefinitionProvider should be called');
        // Check that a disposable object was added to subscriptions
        assert.ok(context.subscriptions.some(s => typeof s === 'object' && typeof s.dispose === 'function'), 'Disposable should be added to subscriptions');
    });

    test('constructs correct server path for win32', async () => {
        Object.defineProperty(process, 'platform', { value: 'win32' });
        await activate(context);
        const call = registerStub.getCall(0);
        const provider = call.args[1];
        const defs = await provider.provideMcpServerDefinitions();
        assert.strictEqual(defs[0].executablePath, path.join('/ext/path', 'server', 'win-x64', 'azmcp.exe'));
    });

    test('constructs correct server path for darwin', async () => {
        Object.defineProperty(process, 'platform', { value: 'darwin' });
        await activate(context);
        const call = registerStub.getCall(0);
        const provider = call.args[1];
        const defs = await provider.provideMcpServerDefinitions();
        assert.strictEqual(defs[0].executablePath, path.join('/ext/path', 'server', 'osx-x64', 'azmcp'));
    });

    test('constructs correct server path for linux', async () => {
        Object.defineProperty(process, 'platform', { value: 'linux' });
        await activate(context);
        const call = registerStub.getCall(0);
        const provider = call.args[1];
        const defs = await provider.provideMcpServerDefinitions();
        assert.strictEqual(defs[0].executablePath, path.join('/ext/path', 'server', 'linux-x64', 'azmcp'));
    });
});
