// Tests for eng/npm/wrapper/index.js. Run with:
//   node --test eng/npm/test/wrapper.test.js
//
// These live outside eng/npm/wrapper because Pack-Npm.ps1 copies that whole
// directory into the published package.
//
// The wrapper does all of its work when it loads and then exits, so each test
// runs a copy of it as a child process. A stub `npm` first on PATH records how
// it was called and "installs" a fake platform package, so nothing touches the
// network or the real npm cache.

const { test } = require('node:test')
const assert = require('node:assert')
const { spawn, spawnSync } = require('node:child_process')
const fs = require('node:fs')
const os = require('node:os')
const path = require('node:path')

const wrapperSource = path.join(__dirname, '..', 'wrapper', 'index.js')
const packageName = '@test/mcp'
const packageVersion = '1.2.3'
const platformPackageName = `${packageName}-${os.platform()}-${os.arch()}`

// Stands in for eng/npm/platform/index.js: instead of starting the server
// binary, it writes its arguments to stdout so tests can check that stdout
// carries nothing else.
const platformIndexSource = `exports.runExecutable = async (args) => {
  process.stdout.write('ran ' + JSON.stringify(args))
  return 0
}
`

function writePlatformPackage(dir) {
  fs.mkdirSync(dir, { recursive: true })
  fs.writeFileSync(path.join(dir, 'package.json'), JSON.stringify({
    name: platformPackageName,
    version: packageVersion,
    main: './index.js'
  }))
  fs.writeFileSync(path.join(dir, 'index.js'), platformIndexSource)
}

const npmStubSource = `const fs = require('fs')
const path = require('path')

const args = process.argv.slice(2)
fs.appendFileSync(process.env.FAKE_NPM_LOG, JSON.stringify({ args, cwd: process.cwd() }) + '\\n')

// Real npm prints progress and summaries to stdout; the wrapper must keep it
// off its own stdout.
process.stdout.write('added 1 package in 1s\\n')

const failOnceMarker = process.env.FAKE_NPM_FAIL_ONCE
if (process.env.FAKE_NPM_FAIL || (failOnceMarker && !fs.existsSync(failOnceMarker))) {
  if (failOnceMarker) {
    fs.writeFileSync(failOnceMarker, '')
  }
  process.stderr.write('npm error network request failed\\n')
  process.exit(1)
}

const spec = args[1]
const name = spec.slice(0, spec.lastIndexOf('@'))
const prefix = path.resolve(args[args.indexOf('--prefix') + 1])
const dir = path.join(prefix, 'node_modules', name)

// Like npm's reify, move the existing package out of the way before writing
// the new one. A slow install widens that window, which is what concurrent
// installs into one prefix trip over.
fs.rmSync(dir, { recursive: true, force: true })
const delayMs = Number(process.env.FAKE_NPM_DELAY_MS || 0)
if (delayMs) {
  Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, delayMs)
}

fs.mkdirSync(dir, { recursive: true })
fs.writeFileSync(path.join(dir, 'package.json'), JSON.stringify({ name, main: './index.js' }))
fs.writeFileSync(path.join(dir, 'index.js'), ${JSON.stringify(platformIndexSource)})
`

// Lays out a caller project and an installed copy of the wrapper under a fresh
// temp root:
//
//   <root>/project/package.json          cwd the wrapper is spawned from
//   <root>/node_modules/@test/mcp/       the wrapper, as npm would install it
//   <root>/bin/npm                       the npm stub
//   <root>/home/                         HOME and per-user cache dirs
function createFixture(t) {
  const root = fs.realpathSync(fs.mkdtempSync(path.join(os.tmpdir(), 'mcp-wrapper-test-')))

  const project = path.join(root, 'project')
  const projectManifest = JSON.stringify({ name: 'project', version: '1.0.0', dependencies: {} }, null, 2)
  fs.mkdirSync(project)
  fs.writeFileSync(path.join(project, 'package.json'), projectManifest)

  const wrapperDir = path.join(root, 'node_modules', ...packageName.split('/'))
  const home = path.join(root, 'home')
  fs.mkdirSync(wrapperDir, { recursive: true })
  fs.mkdirSync(home)
  t.after(() => {
    // Some tests make these read-only.
    fs.chmodSync(wrapperDir, 0o755)
    fs.chmodSync(home, 0o755)
    fs.rmSync(root, { recursive: true, force: true })
  })
  fs.copyFileSync(wrapperSource, path.join(wrapperDir, 'index.js'))
  fs.writeFileSync(path.join(wrapperDir, 'package.json'), JSON.stringify({
    name: packageName,
    version: packageVersion,
    engines: { node: '>=22.0.0' }
  }))

  const bin = path.join(root, 'bin')
  fs.mkdirSync(bin)
  fs.writeFileSync(path.join(bin, 'npm.js'), npmStubSource)
  if (process.platform === 'win32') {
    fs.writeFileSync(path.join(bin, 'npm.cmd'), `@"${process.execPath}" "%~dp0npm.js" %*\r\n`)
  } else {
    fs.writeFileSync(path.join(bin, 'npm'), `#!/bin/sh\nexec "${process.execPath}" "${path.join(bin, 'npm.js')}" "$@"\n`, { mode: 0o755 })
  }

  const env = { ...process.env }
  const pathKey = Object.keys(env).find(key => key.toUpperCase() === 'PATH') || 'PATH'
  env[pathKey] = bin + path.delimiter + env[pathKey]
  env.HOME = home
  env.USERPROFILE = home
  env.LOCALAPPDATA = path.join(home, 'AppData', 'Local')
  env.XDG_CACHE_HOME = path.join(home, '.cache')
  env.FAKE_NPM_LOG = path.join(root, 'npm.log')
  delete env.NODE_PATH
  delete env.NODE_OPTIONS
  delete env.DEBUG

  const cacheBase = {
    win32: env.LOCALAPPDATA,
    darwin: path.join(home, 'Library', 'Caches')
  }[process.platform] || env.XDG_CACHE_HOME

  return {
    root,
    project,
    projectManifest,
    wrapperDir,
    home,
    env,
    privateDir: path.join(wrapperDir, '.platform'),
    cacheDir: path.join(cacheBase, 'microsoft-mcp', platformPackageName, packageVersion)
  }
}

function wrapperArgs(fixture) {
  return [path.join(fixture.wrapperDir, 'index.js'), 'server', 'start']
}

function runWrapper(fixture, extraEnv = {}) {
  const result = spawnSync(process.execPath, wrapperArgs(fixture), {
    cwd: fixture.project,
    env: { ...fixture.env, ...extraEnv },
    encoding: 'utf8',
    timeout: 30000
  })
  assert.ifError(result.error)
  return result
}

function runWrapperAsync(fixture, extraEnv = {}) {
  return new Promise((resolve, reject) => {
    const child = spawn(process.execPath, wrapperArgs(fixture), {
      cwd: fixture.project,
      env: { ...fixture.env, ...extraEnv }
    })
    let stdout = ''
    let stderr = ''
    child.stdout.on('data', chunk => { stdout += chunk })
    child.stderr.on('data', chunk => { stderr += chunk })
    child.on('error', reject)
    child.on('close', status => resolve({ status, stdout, stderr }))
  })
}

function npmCalls(fixture) {
  if (!fs.existsSync(fixture.env.FAKE_NPM_LOG)) {
    return []
  }
  return fs.readFileSync(fixture.env.FAKE_NPM_LOG, 'utf8').trim().split('\n').map(line => JSON.parse(line))
}

// The directory npm was told to install into, resolved against the cwd it ran in.
function installPrefix(call) {
  return path.resolve(call.cwd, call.args[call.args.indexOf('--prefix') + 1])
}

// A process ID that is no longer running.
function deadPid() {
  return spawnSync(process.execPath, ['-e', '']).pid
}

// Replaces os.homedir() in the wrapper with one that throws, as it does when
// HOME is unset and the user has no passwd entry.
function noHomeDirEnv(fixture) {
  const preload = path.join(fixture.root, 'no-home-dir.js')
  fs.writeFileSync(preload, `require('os').homedir = () => { throw new Error('ENOENT: no home directory') }\n`)
  return { NODE_OPTIONS: `--require ${JSON.stringify(preload)}` }
}

const permissionsSkip = process.platform === 'win32'
  ? 'directory permissions are not enforced this way on Windows'
  : process.getuid() === 0 && 'root ignores directory permissions'

const expectedStdout = 'ran ["server","start"]'

test('does not run npm when the platform package is installed normally', (t) => {
  const fixture = createFixture(t)
  writePlatformPackage(path.join(fixture.root, 'node_modules', ...platformPackageName.split('/')))

  const result = runWrapper(fixture)

  assert.strictEqual(result.status, 0, result.stderr)
  assert.strictEqual(result.stdout, expectedStdout)
  assert.deepStrictEqual(npmCalls(fixture), [])
  assert.ok(!fs.existsSync(fixture.privateDir), 'should not create the private install dir')
})

test('does not need a home directory when the platform package is installed normally', (t) => {
  const fixture = createFixture(t)
  writePlatformPackage(path.join(fixture.root, 'node_modules', ...platformPackageName.split('/')))

  const result = runWrapper(fixture, noHomeDirEnv(fixture))

  assert.strictEqual(result.status, 0, result.stderr)
  assert.strictEqual(result.stdout, expectedStdout)
})

test('installs a missing platform package privately, not into the caller\'s project', (t) => {
  const fixture = createFixture(t)

  const result = runWrapper(fixture)

  assert.strictEqual(result.status, 0, result.stderr)
  // npm's own stdout must not reach ours: under `server start` it is the
  // JSON-RPC stream.
  assert.strictEqual(result.stdout, expectedStdout)

  const calls = npmCalls(fixture)
  assert.strictEqual(calls.length, 1)
  const { args, cwd } = calls[0]
  assert.strictEqual(args[0], 'install')
  assert.strictEqual(args[1], `${platformPackageName}@${packageVersion}`)
  for (const flag of ['--no-save', '--no-audit', '--no-fund']) {
    assert.ok(args.includes(flag), `expected ${flag} in ${JSON.stringify(args)}`)
  }
  assert.strictEqual(installPrefix(calls[0]), fixture.privateDir)
  assert.strictEqual(cwd, fixture.privateDir)

  assert.ok(fs.existsSync(path.join(fixture.privateDir, 'node_modules', ...platformPackageName.split('/'), 'index.js')))
  assert.ok(!fs.existsSync(path.join(fixture.privateDir, '.install.lock')), 'should release the install lock')
  assert.strictEqual(fs.readFileSync(path.join(fixture.project, 'package.json'), 'utf8'), fixture.projectManifest)
  assert.ok(!fs.existsSync(path.join(fixture.project, 'package-lock.json')), 'should not write a lockfile into the project')
  assert.ok(!fs.existsSync(path.join(fixture.project, 'node_modules')), 'should not install into the project')
})

test('installs into the wrapper directory without a home directory', (t) => {
  const fixture = createFixture(t)

  const result = runWrapper(fixture, noHomeDirEnv(fixture))

  assert.strictEqual(result.status, 0, result.stderr)
  assert.strictEqual(result.stdout, expectedStdout)
  const calls = npmCalls(fixture)
  assert.strictEqual(calls.length, 1)
  assert.strictEqual(installPrefix(calls[0]), fixture.privateDir)
})

test('reuses an earlier private install instead of running npm again', (t) => {
  const fixture = createFixture(t)

  assert.strictEqual(runWrapper(fixture).status, 0)
  const second = runWrapper(fixture)

  assert.strictEqual(second.status, 0, second.stderr)
  assert.strictEqual(second.stdout, expectedStdout)
  assert.doesNotMatch(second.stderr, /Installing missing platform package/)
  assert.strictEqual(npmCalls(fixture).length, 1)
})

test('reinstalls over a half-finished private install', (t) => {
  const fixture = createFixture(t)
  // A package.json without its index.js, as an interrupted install could leave.
  const partial = path.join(fixture.privateDir, 'node_modules', ...platformPackageName.split('/'))
  fs.mkdirSync(partial, { recursive: true })
  fs.writeFileSync(path.join(partial, 'package.json'), JSON.stringify({ name: platformPackageName, main: './index.js' }))

  const result = runWrapper(fixture)

  assert.strictEqual(result.status, 0, result.stderr)
  assert.strictEqual(result.stdout, expectedStdout)
  assert.strictEqual(npmCalls(fixture).length, 1)
})

test('installs once when several launches start at the same time', async (t) => {
  const fixture = createFixture(t)

  const results = await Promise.all(
    Array.from({ length: 4 }, () => runWrapperAsync(fixture, { FAKE_NPM_DELAY_MS: '500' }))
  )

  for (const result of results) {
    assert.strictEqual(result.status, 0, result.stderr)
    assert.strictEqual(result.stdout, expectedStdout)
  }
  assert.strictEqual(npmCalls(fixture).length, 1)
  assert.ok(!fs.existsSync(path.join(fixture.privateDir, '.install.lock')), 'should release the install lock')
})

test('takes over an install lock left by a process that died', (t) => {
  const fixture = createFixture(t)
  fs.mkdirSync(fixture.privateDir, { recursive: true })
  fs.writeFileSync(path.join(fixture.privateDir, '.install.lock'), String(deadPid()))

  const result = runWrapper(fixture)

  assert.strictEqual(result.status, 0, result.stderr)
  assert.strictEqual(result.stdout, expectedStdout)
  assert.strictEqual(npmCalls(fixture).length, 1)
  assert.ok(!fs.existsSync(path.join(fixture.privateDir, '.install.lock')), 'should release the install lock')
})

test('falls back to a per-user cache when the wrapper directory is not writable', { skip: permissionsSkip }, (t) => {
  const fixture = createFixture(t)
  // Like a root-owned global install, or a read-only filesystem.
  fs.chmodSync(fixture.wrapperDir, 0o555)

  const first = runWrapper(fixture)

  assert.strictEqual(first.status, 0, first.stderr)
  assert.strictEqual(first.stdout, expectedStdout)
  const calls = npmCalls(fixture)
  assert.strictEqual(calls.length, 1)
  assert.strictEqual(installPrefix(calls[0]), fixture.cacheDir)
  assert.ok(!fs.existsSync(fixture.privateDir), 'should not have created the private install dir')

  const second = runWrapper(fixture)

  assert.strictEqual(second.status, 0, second.stderr)
  assert.strictEqual(second.stdout, expectedStdout)
  assert.strictEqual(npmCalls(fixture).length, 1, 'should reuse the cached install')
})

test('reports troubleshooting steps when no install directory is writable', { skip: permissionsSkip }, (t) => {
  const fixture = createFixture(t)
  fs.chmodSync(fixture.wrapperDir, 0o555)
  fs.chmodSync(fixture.home, 0o555)

  const result = runWrapper(fixture)

  assert.strictEqual(result.status, 1)
  assert.strictEqual(result.stdout, '')
  assert.match(result.stderr, /No writable directory to install into/)
  assert.deepStrictEqual(npmCalls(fixture), [])
})

test('recovers when the first npm attempt fails', (t) => {
  const fixture = createFixture(t)

  const result = runWrapper(fixture, { FAKE_NPM_FAIL_ONCE: path.join(fixture.root, 'failed-once') })

  assert.strictEqual(result.status, 0, result.stderr)
  assert.strictEqual(result.stdout, expectedStdout)
  const calls = npmCalls(fixture)
  assert.strictEqual(calls.length, 2)
  assert.ok(calls[1].args.includes('--prefer-online'))
  assert.strictEqual(installPrefix(calls[1]), fixture.privateDir)
})

test('reports troubleshooting steps and exits non-zero when npm fails', (t) => {
  const fixture = createFixture(t)

  const result = runWrapper(fixture, { FAKE_NPM_FAIL: '1' })

  assert.strictEqual(result.status, 1)
  assert.strictEqual(result.stdout, '')
  assert.match(result.stderr, /Troubleshooting steps/)
  assert.match(result.stderr, new RegExp(`You are running ${process.version.replace(/\./g, '\\.')}`))
  assert.ok(!fs.existsSync(path.join(fixture.privateDir, '.install.lock')), 'should release the install lock')

  const calls = npmCalls(fixture)
  assert.strictEqual(calls.length, 2, 'should retry once')
  assert.ok(calls[1].args.includes('--prefer-online'))
})
