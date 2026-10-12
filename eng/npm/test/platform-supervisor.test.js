// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Event-level internal supervisor contracts; real processes are tested separately.
const { test } = require('node:test')
const assert = require('node:assert/strict')
const { EventEmitter } = require('node:events')
const fs = require('node:fs')
const path = require('node:path')
const vm = require('node:vm')
const source = path.join(__dirname, '..', 'platform', 'supervisor.js')

function fixture() {
  const host = new EventEmitter()
  const child = new EventEmitter()
  child.pid = 321
  const exits = []; const kills = []; const calls = []; const messages = []
  Object.assign(host, {
    pid: 320, connected: true, send(message, callback) { messages.push(message); callback?.() }, argv: ['node', 'supervisor.js', '/server', '--arg', 'two words'],
    exit(code) { exits.push(code) }, kill(pid, signal) { kills.push({ pid, signal }) }
  })
  vm.runInNewContext(fs.readFileSync(source, 'utf8'), {
    process: host, console: { error() {} },
    require(key) {
      assert.equal(key, 'child_process')
      return { spawn(...args) { calls.push(args); return child } }
    }
  }, { filename: source })
  return { host, child, exits, kills, calls, messages }
}

for (const signal of ['SIGINT', 'SIGTERM', 'SIGHUP', 'SIGQUIT']) {
  test(`supervisor does not forward ${signal} a second time`, () => {
    const f = fixture()
    f.host.emit(signal)
    assert.deepEqual(f.kills, [])
    assert.deepEqual(f.exits, [])
  })
}

test('parent disconnect kills only the supervisor-owned group', () => {
  const f = fixture()
  f.host.emit('disconnect')
  assert.deepEqual(f.kills, [{ pid: -320, signal: 'SIGKILL' }])
})

for (const [code, expected] of [[0, 0], [42, 42], [null, 1]]) {
  test(`native child exit ${code} returns ${expected}`, () => {
    const f = fixture()
    f.child.emit('exit', code, code === null ? 'SIGTERM' : null)
    assert.equal(f.messages[0].type, 'exit')
    assert.equal(f.messages[0].code, expected)
    assert.deepEqual(f.kills, [{ pid: -320, signal: 'SIGKILL' }])
    assert.deepEqual(f.exits, [])
  })
}

test('native spawn failure returns failure', () => {
  const f = fixture()
  f.child.pid = undefined
  f.child.emit('error', new Error('spawn failed'))
  assert.deepEqual(f.exits, [1])
})

test('supervisor gives the native child only inherited standard streams', () => {
  const f = fixture()
  assert.equal(f.calls[0][0], '/server')
  assert.deepEqual(Array.from(f.calls[0][1]), ['--arg', 'two words'])
  assert.equal(f.calls[0][2].stdio, 'inherit')
  assert.equal(f.calls[0][2].shell, false)
  assert.equal(f.calls[0][2].detached, undefined)
})

test('supervisor reports readiness only after the native executable spawns', () => {
  const f = fixture()
  assert.equal(f.messages.length, 0)
  f.child.emit('spawn')
  assert.equal(f.messages.length, 1)
  assert.equal(f.messages[0].type, 'ready')
})

test('parent loss before spawn readiness terminates the owned group', () => {
  const f = fixture()
  f.host.connected = false
  f.child.emit('spawn')
  assert.equal(f.messages.length, 0)
  assert.deepEqual(f.kills, [{ pid: -320, signal: 'SIGKILL' }])
})

test('failed readiness delivery terminates the owned group', () => {
  const f = fixture()
  f.host.send = (message, callback) => callback(new Error('IPC disconnected'))
  f.child.emit('spawn')
  assert.deepEqual(f.kills, [{ pid: -320, signal: 'SIGKILL' }])
})

test('failed native status delivery still cleans up the owned group', () => {
  const f = fixture()
  f.host.send = (message, callback) => callback(new Error('IPC disconnected'))
  f.child.emit('exit', 0, null)
  assert.deepEqual(f.kills, [{ pid: -320, signal: 'SIGKILL' }])
})
