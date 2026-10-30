// Tests for the published platform launcher. Run from the repository root:
//   node --test eng/npm/test/platform.test.js
// A real Node executable stands in for the bundled server; no Azure resources,
// package installation, network access, or additional dependencies are needed.

const { test, after } = require('node:test')
const assert = require('node:assert/strict')
const { spawnSync } = require('node:child_process')
const fs = require('node:fs')
const os = require('node:os')
const path = require('node:path')

const source = path.join(__dirname, '..', 'platform', 'index.js')
// Link the Windows executable when possible; copy once across volumes, then
// hard-link fixtures on the temporary volume.
// Repeated large binary copies otherwise compete with real process tests and
// trigger fresh antivirus scans for each individual exit-code assertion.
let windowsExecutable
if (process.platform === 'win32') {
  const shared = fs.mkdtempSync(path.join(os.tmpdir(), 'mcp-platform-node-'))
  windowsExecutable = path.join(shared, 'node.exe')
  try {
    fs.linkSync(process.execPath, windowsExecutable)
  } catch (err) {
    if (!['EXDEV', 'EPERM', 'EACCES', 'ENOTSUP'].includes(err.code)) throw err
    fs.copyFileSync(process.execPath, windowsExecutable)
  }
  after(() => fs.rmSync(shared, { recursive: true, force: true }))
}

function createFixture(t, privateExecutable = false) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), 'mcp-platform-test-'))
  t.after(() => fs.rmSync(root, { recursive: true, force: true }))
  // Reuse the installed Node binary through a relative bin path on Windows
  // when both paths are on one volume. Exit/argument assertions do not need a
  // fresh executable copy. Tests that remove the executable get a private link.
  const reuseNode = process.platform === 'win32' && !privateExecutable &&
    path.parse(root).root.toLowerCase() === path.parse(process.execPath).root.toLowerCase()
  const executable = reuseNode ? path.relative(root, process.execPath) :
    (process.platform === 'win32' ? 'test-server.exe' : 'test-server')
  const executablePath = reuseNode ? process.execPath : path.join(root, executable)
  const launcher = path.join(root, 'index.js')
  fs.copyFileSync(source, launcher)
  const supervisor = path.join(path.dirname(source), 'supervisor.js')
  if (fs.existsSync(supervisor)) fs.copyFileSync(supervisor, path.join(root, 'supervisor.js'))
  fs.writeFileSync(path.join(root, 'package.json'), JSON.stringify({
    name: '@test/mcp-platform',
    version: '1.0.0',
    bin: { testServer: executable }
  }))
  // Windows symlinks can require elevated privileges. A copy also works
  // when the installed Node executable is on a different filesystem.
  if (reuseNode) {
    // Already present; never mutate the real Node executable.
  } else if (process.platform === 'win32') {
    fs.linkSync(windowsExecutable, executablePath)
  } else {
    fs.symlinkSync(process.execPath, executablePath)
  }
  const env = { ...process.env }
  delete env.NODE_OPTIONS
  delete env.NODE_PATH
  delete env.DEBUG
  return { root, executablePath, launcher, env }
}

function run(fixture, args, asModule = false) {
  const moduleScript = `require(${JSON.stringify(fixture.launcher)})
    .runExecutable(${JSON.stringify(args)}).then(code => process.exit(code))`
  const result = spawnSync(process.execPath,
    asModule ? ['-e', moduleScript] : [fixture.launcher, ...args], {
      cwd: fixture.root,
      env: fixture.env,
      encoding: 'utf8',
      // Windows executable fixture startup can take longer on local machines.
      // Match the existing wrapper suite's 30-second bound there; assertions
      // still require the exact native exit status and protocol output.
      timeout: process.platform === 'win32' ? 30000 : 10000
    })
  assert.ifError(result.error)
  assert.equal(result.signal, null, 'the launcher should return an exit status')
  return result
}

for (const code of [0, 1, 7, 42, 255]) {
  test(`preserves normal exit code ${code}`, (t) => {
    const fixture = createFixture(t)
    const result = run(fixture, ['-e', `process.exit(${code})`])
    assert.equal(result.status, code, result.stderr)
  })
}

for (const signal of ['SIGTERM', 'SIGINT', 'SIGKILL']) {
  for (const asModule of [false, true]) {
    test(`reports ${signal} as failure (${asModule ? 'module' : 'CLI'})`, {
      skip: process.platform === 'win32' && 'POSIX signal termination semantics'
    }, (t) => {
      const fixture = createFixture(t)
      const result = run(fixture, ['-e', `process.kill(process.pid, '${signal}')`], asModule)
      assert.notEqual(result.status, 0, 'signal termination must not be reported as success')
    })
  }
}

test('passes arguments without shell expansion and keeps stdout/stderr separate', (t) => {
  const fixture = createFixture(t)
  const args = ['with spaces', '$HOME', 'a;b', '\u4e2d\u6587', '']
  const result = run(fixture, ['-e', `
    const fs = require('fs')
    fs.writeSync(1, JSON.stringify(process.argv.slice(1)))
    fs.writeSync(2, 'diagnostic')
  `, '--', ...args])
  assert.equal(result.status, 0, result.stderr)
  assert.equal(result.stdout, JSON.stringify(args))
  assert.equal(result.stderr, 'diagnostic')
})

test('returns a failure when the executable is missing', (t) => {
  const fixture = createFixture(t, true)
  fs.unlinkSync(fixture.executablePath)
  const result = run(fixture, [])
  assert.equal(result.status, 1)
  assert.match(result.stderr, /not found/)
})

test('returns a failure on a spawn permission error', {
  skip: process.platform === 'win32' && 'POSIX executable permissions'
}, (t) => {
  const fixture = createFixture(t)
  fs.unlinkSync(fixture.executablePath)
  fs.writeFileSync(fixture.executablePath, 'not executable', { mode: 0o644 })
  const result = run(fixture, [])
  assert.equal(result.status, 1)
  assert.match(result.stderr, /Error executing package/)
})
