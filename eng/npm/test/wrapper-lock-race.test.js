// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Real wrapper processes, real filesystem operations, and a local npm stub.
// The preload changes scheduling only. No registry, credentials or Azure calls.
const { test } = require('node:test')
const assert = require('node:assert/strict')
const { spawn, spawnSync } = require('node:child_process')
const { setTimeout: delay } = require('node:timers/promises')
const fs = require('node:fs')
const os = require('node:os')
const path = require('node:path')
const { randomUUID } = require('node:crypto')
const wrapperSource = path.join(__dirname, '..', 'wrapper')

async function until(fn, label, timeout = 7000) {
  const deadline = Date.now() + timeout
  while (Date.now() < deadline) {
    const value = fn()
    if (value) return value
    await delay(10)
  }
  throw new Error(`Timed out: ${label}`)
}
const preload = `
const fs = require('fs')
const path = require('path')
const marker = name => path.join(process.env.TEST_MARKERS, process.env.TEST_ID + '.' + name)
const lock = process.env.TEST_LOCK
let paused = false
const pause = () => {
  fs.writeFileSync(marker('before-delete'), '')
  const end = Date.now() + 10000
  while (!fs.existsSync(marker('allow-delete'))) {
    if (Date.now() > end) throw new Error('test scheduling barrier timed out')
    Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, 10)
  }
}
for (const method of ['rmSync', 'unlinkSync']) {
  const original = fs[method]
  fs[method] = function(file, ...args) {
    if (!paused && (file === lock || path.dirname(String(file)) === lock)) {
      paused = true
      pause()
      try { return original.call(fs, file, ...args) }
      finally { fs.writeFileSync(marker('after-delete'), '') }
    }
    return original.call(fs, file, ...args)
  }
}
const kill = process.kill.bind(process)
process.kill = function(pid, signal) {
  const result = kill(pid, signal)
  if (signal === 0 && process.env.TEST_LIVE_PID && pid === Number(process.env.TEST_LIVE_PID)) {
    fs.writeFileSync(marker('observed-live'), '')
  }
  return result
}
`
const npmStub = `
const fs = require('fs')
const path = require('path')
const markers = process.env.TEST_MARKERS
const id = process.env.TEST_ID
const args = process.argv.slice(2)
if (args[0] !== 'install' || !args.includes('--prefix')) throw new Error('unexpected npm command')
fs.appendFileSync(path.join(markers, 'installs.jsonl'), JSON.stringify({ id, cwd: process.cwd(), args }) + '\\n')
fs.writeFileSync(path.join(markers, 'active', id), '')
if (fs.readdirSync(path.join(markers, 'active')).length > 1) fs.writeFileSync(path.join(markers, 'overlap'), '')
fs.writeFileSync(path.join(markers, id + '.install-started'), '')
if (process.env.TEST_HOLD) {
  const deadline = Date.now() + 10000
  while (!fs.existsSync(path.join(markers, 'finish'))) {
    if (Date.now() > deadline) throw new Error('test install barrier timed out')
    Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, 10)
  }
} else {
  Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, 100)
}
const spec = args[1]
const name = spec.slice(0, spec.lastIndexOf('@'))
const prefix = path.resolve(args[args.indexOf('--prefix') + 1])
const dest = path.join(prefix, 'node_modules', name)
fs.mkdirSync(dest, { recursive: true })
fs.writeFileSync(path.join(dest, 'package.json'), JSON.stringify({ name, main: 'index.js' }))
fs.writeFileSync(path.join(dest, 'index.js'), 'exports.runExecutable = async () => { process.stdout.write("ran"); return 0 }\\n')
fs.unlinkSync(path.join(markers, 'active', id))
`
function fixture(t) {
  const root = fs.realpathSync(fs.mkdtempSync(path.join(os.tmpdir(), 'mcp-lock-race-')))
  const wrapper = path.join(root, 'wrapper')
  fs.cpSync(wrapperSource, wrapper, { recursive: true })
  fs.writeFileSync(path.join(wrapper, 'package.json'), JSON.stringify({ name: '@test/mcp', version: '1.2.3' }))
  const privateDir = path.join(wrapper, '.platform')
  fs.mkdirSync(privateDir)
  const markers = path.join(root, 'markers')
  fs.mkdirSync(path.join(markers, 'active'), { recursive: true })
  const bin = path.join(root, 'bin')
  fs.mkdirSync(bin)
  fs.writeFileSync(path.join(bin, 'npm.js'), npmStub)
  fs.writeFileSync(path.join(root, 'preload.js'), preload)
  if (process.platform === 'win32') {
    fs.writeFileSync(path.join(bin, 'npm.cmd'), `@"${process.execPath}" "%~dp0npm.js" %*\r\n`)
  } else {
    fs.writeFileSync(path.join(bin, 'npm'), `#!/bin/sh\nexec "${process.execPath}" "${path.join(bin, 'npm.js')}" "$@"\n`, { mode: 0o755 })
  }
  const env = { ...process.env }
  delete env.NODE_OPTIONS
  delete env.NODE_PATH
  delete env.DEBUG
  const key = Object.keys(env).find(k => k.toUpperCase() === 'PATH') || 'PATH'
  env[key] = bin + path.delimiter + env[key]
  Object.assign(env, {
    TEST_MARKERS: markers, TEST_LOCK: path.join(privateDir, '.install.lock'),
    HOME: path.join(root, 'home'), USERPROFILE: path.join(root, 'home'),
    LOCALAPPDATA: path.join(root, 'home', 'local'), XDG_CACHE_HOME: path.join(root, 'home', '.cache')
  })
  const processes = []
  t.after(async () => {
    fs.writeFileSync(path.join(markers, 'finish'), '')
    for (const p of processes) fs.writeFileSync(path.join(markers, p.id + '.allow-delete'), '')
    for (const p of processes) {
      try { await Promise.race([p.done, delay(2000).then(() => { throw new Error('cleanup timeout') })]) }
      catch {
        if (p.child.exitCode === null && p.child.signalCode === null) p.child.kill('SIGKILL')
      }
    }
    fs.rmSync(root, { recursive: true, force: true })
  })
  function launch(id, schedule = false, extra = {}) {
    let stderr = ''; let stdout = ''
    const args = schedule ? ['--require', path.join(root, 'preload.js')] : []
    args.push(path.join(wrapper, 'index.js'), 'server', 'start')
    const child = spawn(process.execPath, args, { cwd: root, env: { ...env, TEST_ID: id, ...extra } })
    child.stdout.on('data', b => { stdout += b })
    child.stderr.on('data', b => { stderr += b })
    const done = new Promise((resolve, reject) => {
      child.on('error', reject)
      child.on('close', (code, signal) => resolve({ code, signal, stdout, stderr }))
    })
    processes.push({ id, child, done })
    return { child, done }
  }
  return {
    root, privateDir, lock: env.TEST_LOCK, markers, launch,
    mark: name => path.join(markers, name),
    calls() {
      const log = path.join(markers, 'installs.jsonl')
      return fs.existsSync(log) ? fs.readFileSync(log, 'utf8').trim().split('\n').map(JSON.parse) : []
    }
  }
}
function deadPid() {
  const result = spawnSync(process.execPath, ['-e', ''], { timeout: 5000 })
  assert.ifError(result.error)
  return result.pid
}

for (const seed of ['fresh', 'legacy-stale', 'directory-stale']) {
  test(`serializes eight real wrapper launches (${seed})`, { timeout: 15000 }, async t => {
    const f = fixture(t)
    if (seed === 'legacy-stale') fs.writeFileSync(f.lock, String(deadPid()))
    if (seed === 'directory-stale') {
      fs.mkdirSync(f.lock)
      fs.writeFileSync(path.join(f.lock, `owner-${deadPid()}-${randomUUID()}`), '')
    }
    const runs = Array.from({ length: 8 }, (_, i) => f.launch('P' + i))
    const results = await Promise.all(runs.map(r => r.done))
    for (const r of results) {
      assert.equal(r.code, 0, r.stderr)
      assert.equal(r.stdout, 'ran')
    }
    assert.equal(f.calls().length, 1)
    assert.ok(!fs.existsSync(f.mark('overlap')))
    assert.ok(!fs.existsSync(f.lock))
  })
}

for (const seed of ['legacy', 'directory']) {
  test(`delayed stale ${seed} cleanup preserves the new owner's lock`, { timeout: 15000 }, async t => {
    const f = fixture(t)
    if (seed === 'legacy') fs.writeFileSync(f.lock, String(deadPid()))
    else {
      fs.mkdirSync(f.lock)
      fs.writeFileSync(path.join(f.lock, `owner-${deadPid()}-${randomUUID()}`), '')
    }
    const a = f.launch('A', true, { TEST_HOLD: '1' })
    const b = f.launch('B', true, { TEST_HOLD: '1', TEST_LIVE_PID: String(a.child.pid) })
    await until(() => ['A', 'B'].every(id => fs.existsSync(f.mark(id + '.before-delete'))), 'both stale observations')
    fs.writeFileSync(f.mark('A.allow-delete'), '')
    await until(() => fs.existsSync(f.mark('A.install-started')), 'A holds lock and starts install')
    fs.writeFileSync(f.mark('B.allow-delete'), '')
    await until(() => fs.existsSync(f.mark('B.after-delete')), 'B attempts its delayed deletion')
    await until(() => fs.existsSync(f.mark('B.observed-live')) || fs.existsSync(f.mark('B.install-started')), 'B sees active owner or incorrectly enters')
    const observedCalls = f.calls().length
    const overlap = fs.existsSync(f.mark('overlap'))
    fs.writeFileSync(f.mark('finish'), '')
    const results = await Promise.all([a.done, b.done])
    assert.equal(observedCalls, 1, 'only A may install before it releases the lock')
    assert.equal(overlap, false, 'npm install intervals must not overlap')
    assert.equal(f.calls().length, 1, 'B must reuse A\'s finished installation')
    for (const r of results) assert.equal(r.code, 0, r.stderr)
    assert.ok(!fs.existsSync(f.lock))
  })
}
