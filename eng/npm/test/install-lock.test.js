// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// node --test eng/npm/test/install-lock.test.js
const { test } = require('node:test')
const assert = require('node:assert/strict')
const fs = require('node:fs')
const os = require('node:os')
const path = require('node:path')
const vm = require('node:vm')
const { spawnSync } = require('node:child_process')
const { randomUUID } = require('node:crypto')
const modulePath = path.join(__dirname, '..', 'wrapper', 'installLock.js')
const locks = require(modulePath)

function fixture(t) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'mcp-lock-test-'))
  t.after(() => fs.rmSync(dir, { recursive: true, force: true }))
  return { dir, lock: path.join(dir, '.install.lock') }
}
function deadPid() {
  const result = spawnSync(process.execPath, ['-e', ''], { timeout: 5000 })
  assert.ifError(result.error)
  assert.equal(result.status, 0)
  return result.pid
}
function marker(pid) { return `owner-${pid}-${randomUUID()}` }
function directoryLock(lock, pid) {
  fs.mkdirSync(lock)
  const owner = marker(pid)
  fs.writeFileSync(path.join(lock, owner), '')
  return owner
}
function candidates(dir) { return fs.readdirSync(dir).filter(n => n.startsWith('.install-candidate-')) }
// Load the complete production helper. Scheduling hooks below call real fs
// operations; error-injection cases are explicitly labelled separately.
function instrument(fsOverrides = {}, kill = process.kill.bind(process)) {
  const module = { exports: {} }
  const sandbox = {
    module, performance, Atomics, Int32Array, SharedArrayBuffer,
    process: { pid: process.pid, kill },
    require(name) { return name === 'fs' ? { ...fs, ...fsOverrides } : require(name) }
  }
  vm.runInNewContext(fs.readFileSync(modulePath, 'utf8'), sandbox, { filename: modulePath })
  return module.exports
}

test('publishes a complete owner directory and releases idempotently', t => {
  const f = fixture(t)
  assert.equal(locks.isInstallInProgress(f.dir), false)
  const release = locks.acquireInstallLock(f.dir)
  assert.equal(locks.isInstallInProgress(f.dir), true)
  const owners = fs.readdirSync(f.lock)
  assert.equal(owners.length, 1)
  assert.match(owners[0], new RegExp(`^owner-${process.pid}-`))
  assert.deepEqual(candidates(f.dir), [])
  release()
  release()
  assert.equal(locks.isInstallInProgress(f.dir), false)
})

test('recovers a dead legacy PID-only file', t => {
  const f = fixture(t)
  fs.writeFileSync(f.lock, String(deadPid()))
  const release = locks.acquireInstallLock(f.dir)
  assert.ok(fs.statSync(f.lock).isDirectory())
  release()
})

test('recovers a dead generation directory', t => {
  const f = fixture(t)
  const old = directoryLock(f.lock, deadPid())
  const release = locks.acquireInstallLock(f.dir)
  assert.ok(!fs.readdirSync(f.lock).includes(old))
  release()
})

test('recovers an empty directory left during release', t => {
  const f = fixture(t)
  fs.mkdirSync(f.lock)
  locks.acquireInstallLock(f.dir)()
  assert.ok(!fs.existsSync(f.lock))
})

for (const format of ['legacy', 'directory']) {
  test(`never steals a live ${format} lock based on age`, t => {
    const f = fixture(t)
    if (format === 'legacy') fs.writeFileSync(f.lock, String(process.pid))
    else directoryLock(f.lock, process.pid)
    const oldTime = new Date(Date.now() - 60 * 60 * 1000)
    fs.utimesSync(f.lock, oldTime, oldTime)
    assert.throws(() => locks.acquireInstallLock(f.dir, undefined, 30), /Timed out/)
    assert.ok(fs.existsSync(f.lock))
    assert.deepEqual(candidates(f.dir), [])
  })
}

for (const value of ['', 'garbage', '0', '-1']) {
  test(`fails closed for ambiguous legacy owner ${JSON.stringify(value)}`, t => {
    const f = fixture(t)
    fs.writeFileSync(f.lock, value)
    assert.throws(() => locks.acquireInstallLock(f.dir, undefined, 20), /Timed out/)
    assert.equal(fs.readFileSync(f.lock, 'utf8'), value)
    assert.deepEqual(candidates(f.dir), [])
  })
}

test('fails closed for an unknown directory layout', t => {
  const f = fixture(t)
  fs.mkdirSync(f.lock)
  fs.writeFileSync(path.join(f.lock, 'unknown-owner'), '')
  assert.throws(() => locks.acquireInstallLock(f.dir, undefined, 20), /Timed out/)
  assert.deepEqual(fs.readdirSync(f.lock), ['unknown-owner'])
})

test('a delayed marker unlink cannot remove a replacement generation', t => {
  const f = fixture(t)
  const old = directoryLock(f.lock, deadPid())
  let successor
  let injected = false
  const hooked = instrument({
    unlinkSync(file) {
      if (!injected && file === path.join(f.lock, old)) {
        injected = true
        fs.unlinkSync(file)
        fs.rmdirSync(f.lock)
        successor = directoryLock(f.lock, process.pid)
      }
      return fs.unlinkSync(file)
    }
  })
  assert.throws(() => hooked.acquireInstallLock(f.dir, undefined, 30), /Timed out/)
  assert.ok(injected)
  assert.deepEqual(fs.readdirSync(f.lock), [successor])
})

test('a delayed rmdir cannot remove a replacement nonempty directory', t => {
  const f = fixture(t)
  directoryLock(f.lock, deadPid())
  let successor
  let injected = false
  const hooked = instrument({
    rmdirSync(file) {
      if (!injected && file === f.lock) {
        injected = true
        fs.rmdirSync(file)
        successor = directoryLock(f.lock, process.pid)
      }
      return fs.rmdirSync(file)
    }
  })
  assert.throws(() => hooked.acquireInstallLock(f.dir, undefined, 30), /Timed out/)
  assert.ok(injected)
  assert.deepEqual(fs.readdirSync(f.lock), [successor])
})

test('a delayed legacy unlink cannot delete a replacement directory', t => {
  const f = fixture(t)
  fs.writeFileSync(f.lock, String(deadPid()))
  let successor
  let injected = false
  const hooked = instrument({
    unlinkSync(file) {
      if (!injected && file === f.lock) {
        injected = true
        fs.unlinkSync(file)
        successor = directoryLock(f.lock, process.pid)
      }
      return fs.unlinkSync(file)
    }
  })
  assert.throws(() => hooked.acquireInstallLock(f.dir, undefined, 30), /Timed out/)
  assert.ok(injected)
  assert.deepEqual(fs.readdirSync(f.lock), [successor])
})

test('a delayed old release preserves a replacement generation', t => {
  const f = fixture(t)
  const release = locks.acquireInstallLock(f.dir)
  const old = fs.readdirSync(f.lock)[0]
  fs.unlinkSync(path.join(f.lock, old))
  fs.rmdirSync(f.lock)
  const successor = directoryLock(f.lock, process.pid)
  release()
  release()
  assert.deepEqual(fs.readdirSync(f.lock), [successor])
})

test('EPERM from the PID probe is not treated as a dead owner (injected error)', t => {
  const f = fixture(t)
  fs.writeFileSync(f.lock, String(deadPid()))
  const hooked = instrument({}, () => { throw Object.assign(new Error('denied'), { code: 'EPERM' }) })
  assert.throws(() => hooked.acquireInstallLock(f.dir, undefined, 20), /Timed out/)
  assert.ok(fs.existsSync(f.lock))
})

test('unrelated rename errors propagate and clean the private candidate (injected error)', t => {
  const f = fixture(t)
  const hooked = instrument({ renameSync() { throw Object.assign(new Error('I/O failure'), { code: 'EIO' }) } })
  assert.throws(() => hooked.acquireInstallLock(f.dir), /I\/O failure/)
  assert.deepEqual(candidates(f.dir), [])
})

test('release between failed publication and inspection is retried', t => {
  const f = fixture(t)
  const owner = directoryLock(f.lock, process.pid)
  let injected = false
  const hooked = instrument({
    renameSync(from, to) {
      try { return fs.renameSync(from, to) }
      catch (err) {
        if (!injected && to === f.lock) {
          injected = true
          fs.unlinkSync(path.join(f.lock, owner))
          fs.rmdirSync(f.lock)
        }
        throw err
      }
    }
  })
  hooked.acquireInstallLock(f.dir)()
  assert.ok(injected)
  assert.ok(!fs.existsSync(f.lock))
})


test('unreadable lock inspection skips unlocked loading without aborting fallback (injected error)', t => {
  const f = fixture(t)
  const hooked = instrument({ lstatSync() { throw Object.assign(new Error('denied'), { code: 'EACCES' }) } })
  assert.equal(hooked.isInstallInProgress(f.dir), true)
})

test('legacy locks are inspected before a rename that could replace a file (simulated Windows)', t => {
  const f = fixture(t)
  fs.writeFileSync(f.lock, String(process.pid))
  let attempted = false
  const hooked = instrument({
    renameSync(from, to) {
      attempted = true
      fs.unlinkSync(to)
      fs.renameSync(from, to)
    }
  })
  assert.throws(() => hooked.acquireInstallLock(f.dir, undefined, 20), /Timed out/)
  assert.equal(attempted, false)
  assert.equal(fs.readFileSync(f.lock, 'utf8'), String(process.pid))
})

test('Windows EPERM collision followed by owner release retries publication (injected scheduling)', t => {
  const f = fixture(t)
  const owner = directoryLock(f.lock, process.pid)
  let injected = false
  const hooked = instrument({
    renameSync(from, to) {
      if (!injected) {
        injected = true
        fs.unlinkSync(path.join(f.lock, owner))
        fs.rmdirSync(f.lock)
        throw Object.assign(new Error('incumbent released after collision'), { code: 'EPERM' })
      }
      return fs.renameSync(from, to)
    }
  })
  hooked.acquireInstallLock(f.dir)()
  assert.ok(injected)
  assert.ok(!fs.existsSync(f.lock))
})

test('persistent publication permission errors time out and clean only the private candidate', t => {
  const f = fixture(t)
  const hooked = instrument({
    renameSync() { throw Object.assign(new Error('denied'), { code: 'EACCES' }) }
  })
  assert.throws(() => hooked.acquireInstallLock(f.dir, undefined, 20), /Timed out/)
  assert.deepEqual(candidates(f.dir), [])
  assert.ok(!fs.existsSync(f.lock))
})
