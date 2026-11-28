// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// These fixtures use REAL launcher/Node processes, not a native azmcp binary.
// Keep stdin open when testing PID-only signals to distinguish shutdown from EOF.
const { test } = require('node:test')
const assert = require('node:assert/strict')
const fs = require('node:fs')
const os = require('node:os')
const path = require('node:path')
const net = require('node:net')
const { spawn } = require('node:child_process')
const { setTimeout: delay } = require('node:timers/promises')
const sourceRoot = path.join(__dirname, '..')
const skip = process.platform === 'win32' && 'POSIX process-group/signal tests'

async function until(fn, description, timeout = 3000) {
  const end = Date.now() + timeout
  while (Date.now() < end) {
    const value = fn()
    if (value) return value
    await delay(10)
  }
  throw new Error(`Timed out: ${description}`)
}
function fixture(t) {
  const root = fs.realpathSync(fs.mkdtempSync(path.join(os.tmpdir(), 'mcp-signal-test-')))
  const wrapper = path.join(root, 'wrapper')
  fs.cpSync(path.join(sourceRoot, 'wrapper'), wrapper, { recursive: true })
  fs.writeFileSync(path.join(wrapper, 'package.json'), JSON.stringify({ name: '@test/mcp', version: '1.2.3' }))
  const platform = path.join(wrapper, 'node_modules', `@test/mcp-${process.platform}-${process.arch}`)
  fs.mkdirSync(platform, { recursive: true })
  fs.copyFileSync(path.join(sourceRoot, 'platform', 'index.js'), path.join(platform, 'index.js'))
  const supervisor = path.join(sourceRoot, 'platform', 'supervisor.js')
  if (fs.existsSync(supervisor)) fs.copyFileSync(supervisor, path.join(platform, 'supervisor.js'))
  fs.symlinkSync(process.execPath, path.join(platform, 'server'))
  fs.writeFileSync(path.join(platform, 'package.json'), JSON.stringify({ main: 'index.js', bin: { server: 'server' } }))
  const env = { ...process.env, TEST_ROOT: root }
  delete env.NODE_OPTIONS; delete env.NODE_PATH; delete env.DEBUG
  const runs = []
  t.after(async () => {
    const readyPath = path.join(root, 'ready')
    if (runs.some(run => !run.closed) && fs.existsSync(readyPath)) {
      const { pid } = JSON.parse(fs.readFileSync(readyPath, 'utf8'))
      // This PID is recorded by this fixture, not discovered from system-wide
      // process searches. The new launcher may give it its own group.
      try { process.kill(-pid, 'SIGKILL') } catch (err) { if (err.code !== 'ESRCH') throw err }
      try { process.kill(pid, 'SIGKILL') } catch (err) { if (err.code !== 'ESRCH') throw err }
    }
    const helperPath = path.join(root, 'helper-ready')
    if (runs.some(run => !run.closed) && fs.existsSync(helperPath)) {
      const { pid } = JSON.parse(fs.readFileSync(helperPath, 'utf8'))
      try { process.kill(pid, 'SIGKILL') } catch (err) { if (err.code !== 'ESRCH') throw err }
    }
    for (const run of runs) {
      // The launcher is the leader of a group created only by this fixture.
      // Kill the group only while this run is unfinished, to avoid PID reuse.
      if (!run.closed) {
        try { process.kill(-run.child.pid, 'SIGKILL') } catch (err) { if (err.code !== 'ESRCH') throw err }
      }
      await run.done
    }
    fs.rmSync(root, { recursive: true, force: true })
  })
  function launch(script, entry = 'wrapper') {
    const filename = path.join(root, 'server.js')
    fs.writeFileSync(filename, script)
    const entryPath = path.join(entry === 'wrapper' ? wrapper : platform, 'index.js')
    const child = spawn(process.execPath, [entryPath, filename], {
      env, cwd: root, detached: true, stdio: ['pipe', 'pipe', 'pipe']
    })
    const run = { child, closed: false, stdout: '', stderr: '' }
    child.stdout.on('data', b => { run.stdout += b })
    child.stderr.on('data', b => { run.stderr += b })
    run.done = new Promise((resolve, reject) => {
      child.on('error', reject)
      child.on('close', (code, signal) => { run.closed = true; resolve({ code, signal }) })
    })
    runs.push(run)
    return run
  }
  return { root, launch, file: name => path.join(root, name),
    pauseNativeSpawn() {
      const filename = path.join(platform, 'supervisor.js')
      const original = fs.readFileSync(filename, 'utf8')
      fs.writeFileSync(filename, `
const scheduledFs = require('fs')
const scheduledPath = require('path')
const scheduledChildProcess = require('child_process')
const scheduledSpawn = scheduledChildProcess.spawn
scheduledChildProcess.spawn = (...args) => {
  scheduledFs.writeFileSync(scheduledPath.join(process.env.TEST_ROOT, 'before-native-spawn'), '')
  const deadline = Date.now() + 5000
  while (!scheduledFs.existsSync(scheduledPath.join(process.env.TEST_ROOT, 'allow-native-spawn'))) {
    if (Date.now() > deadline) throw new Error('native spawn scheduling timeout')
    Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, 10)
  }
  return scheduledSpawn(...args)
}
` + original)
    }
  }
}
function serverScript(mode, status = 42) {
  return `
const fs = require('fs')
const path = require('path')
const write = (name, data = '') => fs.writeFileSync(path.join(process.env.TEST_ROOT, name), data)
let closing = false
const stop = reason => {
  if (closing) return
  closing = true
  write('received', reason)
  setTimeout(() => {
    const finish = () => { write('cleaned'); process.exit(${status}) }
    ${mode === 'tcp' ? 'server.close(finish)' : 'finish()'}
  }, 80)
}
${mode === 'default' ? '' : "process.on('SIGTERM', () => stop('SIGTERM')); process.on('SIGINT', () => stop('SIGINT'))"}
${mode === 'eof' ? "process.stdin.resume(); process.stdin.on('end', () => stop('EOF'))" : ''}
${mode === 'tcp' ? "const server = require('net').createServer(socket => socket.end()); server.listen(0, '127.0.0.1', () => write('ready', JSON.stringify({ pid: process.pid, port: server.address().port })))" : "write('ready', JSON.stringify({ pid: process.pid })); setInterval(() => {}, 1000)"}
`
}
function canConnect(port) {
  return new Promise(resolve => {
    const socket = net.connect({ host: '127.0.0.1', port })
    socket.setTimeout(1000)
    socket.on('connect', () => { socket.destroy(); resolve(true) })
    socket.on('error', () => resolve(false))
    socket.on('timeout', () => { socket.destroy(); resolve(false) })
  })
}

for (const entry of ['wrapper', 'platform']) {
  for (const signal of ['SIGTERM', 'SIGINT']) {
    test(`PID-only ${signal} reaches child and waits for cleanup (${entry})`, { skip, timeout: 8000 }, async t => {
      const f = fixture(t)
      const run = f.launch(serverScript('timer'), entry)
      await until(() => fs.existsSync(f.file('ready')), 'child ready')
      run.child.kill(signal)
      await until(() => fs.existsSync(f.file('received')), 'child receives targeted signal')
      assert.equal(fs.readFileSync(f.file('received'), 'utf8'), signal)
      const result = await run.done
      assert.equal(result.code, 42, run.stderr)
      assert.equal(result.signal, null)
      assert.ok(fs.existsSync(f.file('cleaned')), 'launcher must await cleanup')
      assert.equal(run.stdout, '')
      assert.equal(run.stderr, '')
    })
  }
}

test('default child signal termination returns failure rather than success', { skip, timeout: 8000 }, async t => {
  const f = fixture(t)
  const run = f.launch(serverScript('default'))
  await until(() => fs.existsSync(f.file('ready')), 'child ready')
  run.child.kill('SIGTERM')
  const result = await Promise.race([run.done, delay(3000).then(() => { throw new Error('child did not terminate') })])
  assert.equal(result.code, 1, run.stderr)
  assert.equal(result.signal, null)
})

test('signal delivery to the whole fixture group still permits graceful cleanup', { skip, timeout: 8000 }, async t => {
  const f = fixture(t)
  const run = f.launch(serverScript('timer', 0))
  await until(() => fs.existsSync(f.file('ready')), 'child ready')
  process.kill(-run.child.pid, 'SIGTERM')
  const result = await run.done
  assert.ok(fs.existsSync(f.file('cleaned')))
  assert.equal(result.code, 0, run.stderr)
})

test('stdin EOF shutdown still works without any signal', { skip, timeout: 8000 }, async t => {
  const f = fixture(t)
  const run = f.launch(serverScript('eof', 0))
  await until(() => fs.existsSync(f.file('ready')), 'child ready')
  run.child.stdin.end()
  const result = await run.done
  assert.equal(result.code, 0, run.stderr)
  assert.equal(fs.readFileSync(f.file('received'), 'utf8'), 'EOF')
  assert.ok(fs.existsSync(f.file('cleaned')))
})

test('PID-only termination closes the child loopback TCP listener', { skip, timeout: 8000 }, async t => {
  const f = fixture(t)
  const run = f.launch(serverScript('tcp', 0))
  await until(() => fs.existsSync(f.file('ready')), 'child ready')
  const { port } = JSON.parse(fs.readFileSync(f.file('ready'), 'utf8'))
  assert.equal(await canConnect(port), true)
  run.child.kill('SIGTERM')
  await until(() => fs.existsSync(f.file('received')), 'TCP child receives signal')
  const result = await run.done
  assert.equal(result.code, 0, run.stderr)
  assert.equal(await canConnect(port), false, 'no listener may remain after launcher exit')
  assert.ok(fs.existsSync(f.file('cleaned')))
})

// A second signal has a real cost here; unlike serverScript's idempotent stop,
// this fixture exits early on repetition so accidental double delivery fails.
function countingServer() {
  return `
const fs = require('fs')
const path = require('path')
const file = name => path.join(process.env.TEST_ROOT, name)
let count = 0
for (const signal of ['SIGINT', 'SIGTERM', 'SIGHUP', 'SIGQUIT']) {
  process.on(signal, () => {
    fs.appendFileSync(file('events'), JSON.stringify({ signal, count: ++count }) + '\\n')
    if (count > 1) process.exit(77)
    setTimeout(() => { fs.writeFileSync(file('cleaned'), ''); process.exit(0) }, 300)
  })
}
fs.writeFileSync(file('ready'), JSON.stringify({ pid: process.pid }))
setInterval(() => {}, 1000)
`
}

for (const signal of ['SIGTERM', 'SIGINT', 'SIGHUP', 'SIGQUIT']) {
  for (const target of ['pid', 'group']) {
    test(`one ${signal} to wrapper ${target} reaches child exactly once`, { skip, timeout: 8000 }, async t => {
      const f = fixture(t)
      const run = f.launch(countingServer())
      await until(() => fs.existsSync(f.file('ready')), 'counting child ready')
      process.kill(target === 'group' ? -run.child.pid : run.child.pid, signal)
      const result = await run.done
      const events = fs.readFileSync(f.file('events'), 'utf8').trim().split('\n').map(JSON.parse)
      assert.equal(events.length, 1, JSON.stringify(events))
      assert.equal(events[0].signal, signal)
      assert.ok(fs.existsSync(f.file('cleaned')), 'duplicate signals must not abort cleanup')
      assert.equal(result.code, 0, run.stderr)
      assert.equal(run.stdout, '')
      assert.equal(run.stderr, '')
    })
  }
}

test('two distinct caller signals are not hidden by a debounce', { skip, timeout: 8000 }, async t => {
  const f = fixture(t)
  const run = f.launch(countingServer())
  await until(() => fs.existsSync(f.file('ready')), 'counting child ready')
  run.child.kill('SIGTERM')
  await until(() => fs.existsSync(f.file('events')), 'first signal handled')
  run.child.kill('SIGTERM')
  const result = await run.done
  const events = fs.readFileSync(f.file('events'), 'utf8').trim().split('\n').map(JSON.parse)
  assert.equal(events.length, 2)
  assert.equal(result.code, 77)
  assert.ok(!fs.existsSync(f.file('cleaned')))
})

test('a PID-only stop also reaches a helper in the owned child group', { skip, timeout: 8000 }, async t => {
  const f = fixture(t)
  const leaf = `
const fs = require('fs')
const path = require('path')
const file = name => path.join(process.env.TEST_ROOT, name)
process.on('SIGTERM', () => { fs.writeFileSync(file('leaf-stopped'), ''); process.exit(0) })
fs.writeFileSync(file('leaf-ready'), String(process.pid))
setInterval(() => {}, 1000)
`
  const run = f.launch(`
const fs = require('fs')
const path = require('path')
const file = name => path.join(process.env.TEST_ROOT, name)
let stopping = false
let leafDone = false
const leaf = require('child_process').spawn(process.execPath, ['-e', ${JSON.stringify(leaf)}], { stdio: 'inherit' })
const finish = () => {
  if (stopping && leafDone) { fs.writeFileSync(file('cleaned'), ''); process.exit(0) }
}
leaf.on('exit', () => { leafDone = true; finish() })
process.on('SIGTERM', () => { stopping = true; finish() })
fs.writeFileSync(file('ready'), JSON.stringify({ pid: process.pid }))
`)
  await until(() => fs.existsSync(f.file('ready')) && fs.existsSync(f.file('leaf-ready')), 'both child and helper ready')
  run.child.kill('SIGTERM')
  await until(() => fs.existsSync(f.file('leaf-stopped')), 'owned-group helper stops')
  const result = await run.done
  assert.equal(result.code, 0, run.stderr)
  assert.ok(fs.existsSync(f.file('cleaned')))
})

for (const target of ['pid', 'group']) {
  test(`SIGKILL of wrapper ${target} does not leave its isolated server listening`, { skip, timeout: 8000 }, async t => {
    const f = fixture(t)
    const run = f.launch(serverScript('tcp', 0))
    await until(() => fs.existsSync(f.file('ready')), 'TCP child ready')
    const { port } = JSON.parse(fs.readFileSync(f.file('ready'), 'utf8'))
    assert.equal(await canConnect(port), true)
    // stdin is deliberately open. EOF cannot account for this shutdown.
    process.kill(target === 'group' ? -run.child.pid : run.child.pid, 'SIGKILL')
    const end = Date.now() + 3000
    while (Date.now() < end && await canConnect(port)) await delay(10)
    assert.equal(await canConnect(port), false, 'parent loss must close the isolated server')
    const result = await run.done
    assert.equal(result.signal, 'SIGKILL')
    assert.equal(result.code, null)
    assert.equal(run.stdout, '')
    assert.equal(run.stderr, '')
  })
}

test('a stop before native spawn is not swallowed by the supervisor', { skip, timeout: 8000 }, async t => {
  const f = fixture(t)
  f.pauseNativeSpawn()
  const run = f.launch(serverScript('tcp', 0))
  await until(() => fs.existsSync(f.file('before-native-spawn')), 'supervisor handlers installed before native spawn')
  run.child.kill('SIGTERM')
  // Hold the native spawn until the outer launcher has handled the request.
  // The readiness regression has an event-level counterpart without timing.
  await delay(100)
  fs.writeFileSync(f.file('allow-native-spawn'), '')
  const result = await Promise.race([run.done, delay(3000).then(() => { throw new Error('startup stop was lost') })])
  assert.notEqual(result.code, 0, run.stderr)
  assert.equal(result.signal, null)
  assert.equal(run.stdout, '')
})

test('native exit preserves its code and stops a helper left listening in the owned group', { skip, timeout: 8000 }, async t => {
  const f = fixture(t)
  const helper = `
const fs = require('fs')
const path = require('path')
const server = require('net').createServer(socket => socket.end())
server.listen(0, '127.0.0.1', () => fs.writeFileSync(path.join(process.env.TEST_ROOT, 'helper-ready'),
  JSON.stringify({ pid: process.pid, port: server.address().port })))
`
  const run = f.launch(`
const fs = require('fs')
const path = require('path')
require('child_process').spawn(process.execPath, ['-e', ${JSON.stringify(helper)}], { stdio: 'inherit' })
const timer = setInterval(() => {
  if (fs.existsSync(path.join(process.env.TEST_ROOT, 'helper-ready'))) {
    clearInterval(timer)
    process.exit(42)
  }
}, 10)
`)
  await until(() => fs.existsSync(f.file('helper-ready')), 'helper listener starts')
  const { port } = JSON.parse(fs.readFileSync(f.file('helper-ready'), 'utf8'))
  const result = await Promise.race([run.done, delay(3000).then(() => { throw new Error('helper outlived native exit') })])
  assert.equal(result.code, 42, run.stderr)
  assert.equal(await canConnect(port), false)
  assert.equal(run.stdout, '')
})

test('parent loss before native spawn does not leave a server holding inherited streams', { skip, timeout: 8000 }, async t => {
  const f = fixture(t)
  f.pauseNativeSpawn()
  const run = f.launch(serverScript('tcp', 0))
  await until(() => fs.existsSync(f.file('before-native-spawn')), 'supervisor waiting before spawn')
  const exited = new Promise(resolve => run.child.once('exit', resolve))
  run.child.kill('SIGKILL')
  await exited
  // The outer launcher is gone while the supervisor is still starting. Its
  // disconnect callback must clean up even if native spawn wins that race.
  fs.writeFileSync(f.file('allow-native-spawn'), '')
  const result = await Promise.race([run.done, delay(3000).then(() => {
    throw new Error('startup orphan kept inherited streams open')
  })])
  assert.equal(result.code, null)
  assert.equal(result.signal, 'SIGKILL')
  if (fs.existsSync(f.file('ready'))) {
    const { port } = JSON.parse(fs.readFileSync(f.file('ready'), 'utf8'))
    assert.equal(await canConnect(port), false, 'startup orphan must not remain listening')
  }
  assert.equal(run.stdout, '')
  assert.equal(run.stderr, '')
})

for (const entry of ['wrapper', 'platform']) {
  test(`native exit preserves large stdout and stderr payloads (${entry})`, { skip, timeout: 8000 }, async t => {
    const f = fixture(t)
    const stdout = 'protocol-output\n'.repeat(20000)
    const stderr = 'diagnostic-output\n'.repeat(20000)
    const run = f.launch(`
process.stdout.write(${JSON.stringify(stdout)}, err => {
  if (err) throw err
  process.stderr.write(${JSON.stringify(stderr)}, err => {
    if (err) throw err
    process.exit(42)
  })
})
`, entry)
    const result = await run.done
    assert.equal(result.code, 42)
    assert.equal(result.signal, null)
    assert.equal(run.stdout, stdout)
    assert.equal(run.stderr, stderr)
  })
}
