// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Event-level lifecycle cases; actual POSIX processes are tested separately.
const { test } = require('node:test')
const assert = require('node:assert/strict')
const { EventEmitter } = require('node:events')
const fs = require('node:fs')
const path = require('node:path')
const vm = require('node:vm')
const source = path.join(__dirname, '..', 'platform', 'index.js')

function fixture(platform = 'linux', tty = false, announceReady = true, autoClose = true) {
  const host = new EventEmitter()
  Object.assign(host, { platform, env: {}, stdin: { isTTY: tty }, stdout: {}, stderr: {} })
  const children = []; const diagnostics = []; const spawnCalls = []
  host.kill = (pid, signal) => {
    assert.ok(pid < 0, 'must signal only a child-owned group')
    const child = children.find(c => c.pid === -pid)
    assert.ok(child)
    child.signals.push(signal)
    return true
  }
  const module = { exports: {} }
  function requireStub(name) {
    if (name === './package.json') return { bin: { test: 'server' } }
    if (name === 'fs') return { existsSync: () => true }
    if (name === 'child_process') { const makeChild = (...args) => {
      const child = new EventEmitter()
      const emit = child.emit.bind(child)
      child.emit = (event, ...args) => {
        if (autoClose && event === 'exit' && typeof args[0] === 'number' && platform !== 'win32' && !tty) {
          emit('message', { type: 'exit', code: args[0] })
        }
        const result = emit(event, ...args)
        if (autoClose && event === 'exit') emit('close', ...args)
        return result
      }
      child.pid = 100 + children.length
      child.signals = []
      child.exitCode = null
      child.signalCode = null
      child.kill = signal => { child.signals.push(signal); return true }
      children.push(child); spawnCalls.push(args)
      return child
    }; return { spawn: makeChild, fork: makeChild } }
    return require(name)
  }
  vm.runInNewContext(fs.readFileSync(source, 'utf8'), {
    require: requireStub, module, __dirname: '/test', process: host,
    console: { error: (...args) => diagnostics.push(args) }
  }, { filename: source })
  return { host, children, diagnostics, spawnCalls, run(...args) {
    const done = module.exports.runExecutable(...args)
    if (announceReady) children.at(-1)?.emit('message', { type: 'ready' })
    return done
  } }
}

for (const signal of ['SIGTERM', 'SIGINT']) {
  test(`forwards ${signal}, waits for cleanup, and removes listeners`, async () => {
    const f = fixture()
    const existing = () => {}
    f.host.on(signal, existing)
    const done = f.run(['server', 'start'])
    let settled = false
    done.then(() => { settled = true })
    f.host.emit(signal)
    await Promise.resolve()
    assert.deepEqual(f.children[0].signals, [signal])
    assert.equal(settled, false, 'forwarding is not completion')
    f.children[0].emit('exit', 42, null)
    assert.equal(await done, 42)
    assert.deepEqual(f.host.listeners(signal), [existing])
    assert.equal(f.host.listenerCount(signal === 'SIGTERM' ? 'SIGINT' : 'SIGTERM'), 0)
    assert.equal(f.diagnostics.length, 0)
  })
}

test('normal exits remove both listeners', async () => {
  const f = fixture()
  const done = f.run()
  f.children[0].emit('exit', 0, null)
  assert.equal(await done, 0)
  assert.equal(f.host.listenerCount('SIGTERM'), 0)
  assert.equal(f.host.listenerCount('SIGINT'), 0)
})

test('signal-terminated child is never reported as success', async () => {
  const f = fixture()
  const done = f.run()
  f.children[0].emit('exit', null, 'SIGTERM')
  assert.notEqual(await done, 0)
  assert.equal(f.host.listenerCount('SIGTERM'), 0)
})

test('spawn failure cleans temporary signal listeners', async () => {
  const f = fixture()
  const done = f.run()
  f.children[0].pid = undefined
  f.children[0].emit('error', new Error('spawn failed'))
  assert.equal(await done, 1)
  assert.equal(f.host.listenerCount('SIGINT'), 0)
  assert.equal(f.host.listenerCount('SIGTERM'), 0)
})

test('failed signal delivery does not abandon a running child', async () => {
  const f = fixture()
  const done = f.run()
  let settled = false
  done.then(() => { settled = true })
  f.host.kill = () => { throw Object.assign(new Error('denied'), { code: 'EPERM' }) }
  f.host.emit('SIGTERM')
  await Promise.resolve()
  assert.equal(settled, false)
  assert.equal(f.host.listenerCount('SIGTERM'), 1)
  f.children[0].emit('exit', 7, null)
  assert.equal(await done, 7)
  assert.equal(f.host.listenerCount('SIGTERM'), 0)
})

test('late signals during exit are harmless and preserve exit status', async () => {
  const f = fixture()
  const done = f.run()
  f.host.kill = () => { throw Object.assign(new Error('gone'), { code: 'ESRCH' }) }
  f.host.emit('SIGTERM')
  f.children[0].emit('exit', 9, null)
  assert.equal(await done, 9)
  assert.equal(f.host.listenerCount('SIGTERM'), 0)
})

test('sequential calls do not accumulate listeners', async () => {
  const f = fixture()
  for (let i = 0; i < 25; i++) {
    const done = f.run()
    assert.equal(f.host.listenerCount('SIGTERM'), 1)
    f.children[i].emit('exit', 0, null)
    assert.equal(await done, 0)
    assert.equal(f.host.listenerCount('SIGTERM'), 0)
    assert.equal(f.host.listenerCount('SIGINT'), 0)
  }
})

test('concurrent calls remove only their own listeners', async () => {
  const f = fixture()
  const a = f.run(); const b = f.run()
  assert.equal(f.host.listenerCount('SIGTERM'), 2)
  f.children[0].emit('exit', 0, null)
  await a
  assert.equal(f.host.listenerCount('SIGTERM'), 1)
  f.host.emit('SIGTERM')
  assert.deepEqual(f.children[0].signals, [])
  assert.deepEqual(f.children[1].signals, ['SIGTERM'])
  f.children[1].emit('exit', 0, null)
  await b
  assert.equal(f.host.listenerCount('SIGTERM'), 0)
})

test('leaves Windows signal subscriptions unchanged (simulated platform)', async () => {
  const f = fixture('win32')
  const done = f.run()
  assert.equal(f.host.listenerCount('SIGTERM'), 0)
  assert.equal(f.host.listenerCount('SIGINT'), 0)
  f.children[0].emit('exit', 0, null)
  assert.equal(await done, 0)
})

test('keeps inherited stdio and shell-free invocation', async () => {
  const f = fixture()
  const args = ['with spaces', '$HOME', '', '--']
  const done = f.run(args)
  assert.deepEqual(Array.from(f.spawnCalls[0][1]).slice(1), args)
  assert.deepEqual(Array.from(f.spawnCalls[0][2].stdio), ['inherit', 'inherit', 'inherit', 'ipc'])
  assert.deepEqual(Array.from(f.spawnCalls[0][2].execArgv), [])
  assert.equal(f.spawnCalls[0][2].detached, true)
  f.children[0].emit('exit', 0, null)
  await done
})

for (const signal of ['SIGINT', 'SIGTERM', 'SIGHUP', 'SIGQUIT']) {
  test(`terminal path adds no ${signal} forwarding (simulated terminal)`, async () => {
    const f = fixture('linux', true)
    const done = f.run()
    assert.equal(f.spawnCalls[0][2].detached, undefined)
    assert.equal(f.host.listenerCount(signal), 0)
    f.host.emit(signal)
    assert.deepEqual(f.children[0].signals, [])
    f.children[0].emit('exit', 0, null)
    await done
  })
}

test('does not forward to a group after the child has an exit status', async () => {
  const f = fixture()
  const done = f.run()
  f.children[0].exitCode = 0
  f.host.emit('SIGTERM')
  assert.deepEqual(f.children[0].signals, [])
  f.children[0].emit('exit', 0, null)
  assert.equal(await done, 0)
})

test('shutdown during supervisor startup waits for the native spawn handshake', async () => {
  const f = fixture('linux', false, false)
  const done = f.run()
  f.host.emit('SIGTERM')
  f.host.emit('SIGINT')
  assert.deepEqual(f.children[0].signals, [])
  f.children[0].emit('message', { type: 'unrelated' })
  assert.deepEqual(f.children[0].signals, [])
  f.children[0].emit('message', { type: 'ready' })
  assert.deepEqual(f.children[0].signals, ['SIGTERM', 'SIGINT'])
  f.children[0].emit('message', { type: 'ready' })
  assert.deepEqual(f.children[0].signals, ['SIGTERM', 'SIGINT'])
  f.children[0].emit('exit', null, 'SIGTERM')
  assert.equal(await done, 1)
  assert.equal(f.children[0].listenerCount('message'), 0)
})

test('spawn failure discards pending startup signals and the readiness listener', async () => {
  const f = fixture('linux', false, false)
  const done = f.run()
  f.host.emit('SIGTERM')
  f.children[0].pid = undefined
  f.children[0].emit('error', new Error('spawn failed'))
  assert.equal(await done, 1)
  assert.equal(f.children[0].listenerCount('message'), 0)
  assert.equal(f.host.listenerCount('SIGTERM'), 0)
  f.children[0].emit('message', { type: 'ready' })
  assert.deepEqual(f.children[0].signals, [])
})

test('drains native exit status after supervisor exit before resolving', async () => {
  const f = fixture('linux', false, true, false)
  const done = f.run()
  let settled = false
  done.then(() => { settled = true })
  f.children[0].emit('exit', null, 'SIGKILL')
  await Promise.resolve()
  assert.equal(settled, false)
  f.children[0].emit('message', { type: 'exit', code: 42 })
  f.children[0].emit('close', null, 'SIGKILL')
  assert.equal(await done, 42)
  assert.equal(f.host.listenerCount('SIGTERM'), 0)
  assert.equal(f.children[0].listenerCount('message'), 0)
})

test('invalid supervisor status cannot turn a killed child into success', async () => {
  const f = fixture()
  const done = f.run()
  for (const code of [null, '0', -1, 256]) f.children[0].emit('message', { type: 'exit', code })
  f.children[0].emit('exit', null, 'SIGKILL')
  assert.equal(await done, 1)
})

test('supervisor exit zero without native status is a failure', async () => {
  const f = fixture('linux', false, true, false)
  const done = f.run()
  f.children[0].emit('exit', 0, null)
  f.children[0].emit('close', 0, null)
  assert.equal(await done, 1)
})
