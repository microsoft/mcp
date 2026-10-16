// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Real wrapper + surviving npm-stub processes; no registry or Azure calls.
// The controller kills only its own wrapper PID while its installer is paused.
const { test } = require('node:test')
const assert = require('node:assert/strict')
const { spawn } = require('node:child_process')
const { setTimeout: delay } = require('node:timers/promises')
const fs = require('node:fs')
const os = require('node:os')
const path = require('node:path')
const source = path.join(__dirname, '..', 'wrapper')
const skip = process.platform !== 'linux' && 'Linux-only real orphan-process regression'

async function until(fn, label, timeout = 5000) {
  const end = Date.now() + timeout
  while (Date.now() < end) {
    if (fn()) return
    await delay(10)
  }
  throw new Error(`Timed out: ${label}`)
}

test('a surviving old installer cannot overwrite the newly published cache', { skip, timeout: 15000 }, async t => {
  const root = fs.realpathSync(fs.mkdtempSync(path.join(os.tmpdir(), 'mcp-orphan-install-')))
  const wrapper = path.join(root, 'wrapper')
  fs.cpSync(source, wrapper, { recursive: true })
  const name = `@test/mcp-${process.platform}-${process.arch}`
  fs.writeFileSync(path.join(wrapper, 'package.json'), JSON.stringify({ name: '@test/mcp', version: '1.2.3' }))
  const bin = path.join(root, 'bin')
  fs.mkdirSync(bin)
  const script = path.join(bin, 'npm.js')
  fs.writeFileSync(script, `
const fs = require('fs')
const path = require('path')
const root = process.env.ORPHAN_FIXTURE
const id = process.env.INSTALL_ID
const file = x => path.join(root, x)
const args = process.argv.slice(2)
if (args[0] !== 'install' || !args.includes('--prefix')) throw new Error('bad fixture invocation')
const dest = path.join(process.cwd(), 'node_modules', ${JSON.stringify(name)})
fs.mkdirSync(dest, { recursive: true })
fs.writeFileSync(path.join(dest, 'package.json'), JSON.stringify({ main: './index.js' }))
fs.writeFileSync(file(id + '.started'), JSON.stringify({ pid: process.pid, cwd: process.cwd() }))
if (id === 'orphan') {
  const deadline = Date.now() + 10000
  while (!fs.existsSync(file('release-orphan'))) {
    if (Date.now() > deadline) process.exit(91)
    Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, 10)
  }
}
fs.writeFileSync(path.join(dest, 'index.js'), 'exports.runExecutable = async () => { process.stdout.write(' + JSON.stringify(id) + '); return 0 }')
fs.writeFileSync(file(id + '.finished'), '')
`)
  fs.writeFileSync(path.join(bin, 'npm'), `#!/bin/sh\nexec "${process.execPath}" "${script}" "$@"\n`, { mode: 0o755 })
  const env = { ...process.env, ORPHAN_FIXTURE: root }
  for (const key of ['NODE_OPTIONS', 'NODE_PATH', 'DEBUG']) delete env[key]
  env.PATH = bin + path.delimiter + process.env.PATH
  env.HOME = path.join(root, 'home')
  env.XDG_CACHE_HOME = path.join(root, 'home', '.cache')
  const runs = []
  function launch(id) {
    const child = spawn(process.execPath, [path.join(wrapper, 'index.js'), 'server', 'start'], {
      cwd: root, env: { ...env, INSTALL_ID: id }, stdio: ['ignore', 'pipe', 'pipe'], detached: true
    })
    const run = { child, stdout: '', stderr: '', result: null }
    child.stdout.on('data', b => { run.stdout += b })
    child.stderr.on('data', b => { run.stderr += b })
    run.done = new Promise((resolve, reject) => {
      child.on('error', reject)
      child.on('close', (code, signal) => { run.result = { code, signal }; resolve(run.result) })
    })
    runs.push(run)
    return run
  }
  t.after(async () => {
    fs.writeFileSync(path.join(root, 'release-orphan'), '')
    if (fs.existsSync(path.join(root, 'orphan.started'))) {
      await until(() => fs.existsSync(path.join(root, 'orphan.finished')), 'orphan stub exits').catch(() => {})
    }
    // All groups here were created exclusively by this fixture. A killed
    // wrapper may still have a live installer in its original group.
    for (const run of runs) {
      try { process.kill(-run.child.pid, 'SIGKILL') } catch (err) { if (err.code !== 'ESRCH') throw err }
      await run.done
    }
    fs.rmSync(root, { recursive: true, force: true })
  })
  const a = launch('orphan')
  await until(() => fs.existsSync(path.join(root, 'orphan.started')), 'first installer paused')
  const oldInstaller = JSON.parse(fs.readFileSync(path.join(root, 'orphan.started'), 'utf8'))
  a.child.kill('SIGKILL')
  assert.equal((await a.done).signal, 'SIGKILL')
  assert.doesNotThrow(() => process.kill(oldInstaller.pid, 0), 'installer must really survive its wrapper')
  const b = launch('winner')
  assert.equal((await b.done).code, 0, b.stderr)
  assert.equal(b.stdout, 'winner')
  const newInstaller = JSON.parse(fs.readFileSync(path.join(root, 'winner.started'), 'utf8'))
  fs.writeFileSync(path.join(root, 'release-orphan'), '')
  await until(() => fs.existsSync(path.join(root, 'orphan.finished')), 'late write completes')
  const c = launch('cache-hit')
  assert.equal((await c.done).code, 0, c.stderr)
  t.diagnostic(JSON.stringify({ oldPrefix: oldInstaller.cwd, newPrefix: newInstaller.cwd, cachedResult: c.stdout }))
  assert.equal(c.stdout, 'winner', 'a late orphan write must not replace the finished cache')
  assert.notEqual(oldInstaller.cwd, newInstaller.cwd, 'different attempts must own different prefixes')
  assert.ok(!fs.existsSync(path.join(root, 'cache-hit.started')), 'a cache hit must not reinstall')
})
