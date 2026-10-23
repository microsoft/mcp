// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Real filesystem and module loading; npm completion/errors are injected here.
const { test } = require('node:test')
const assert = require('node:assert/strict')
const fs = require('node:fs')
const os = require('node:os')
const path = require('node:path')
const vm = require('node:vm')
const helper = path.join(__dirname, '..', 'wrapper', 'platformCache.js')
const cache = require(helper)
const name = '@test/mcp-cache'
const version = '1.2.3'

function fixture(t) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'mcp-cache-test-'))
  t.after(() => fs.rmSync(dir, { recursive: true, force: true }))
  return dir
}
function writePackage(dir, tag) {
  const pkg = path.join(dir, 'node_modules', name)
  fs.mkdirSync(pkg, { recursive: true })
  fs.writeFileSync(path.join(pkg, 'package.json'), JSON.stringify({ main: './index.js' }))
  fs.writeFileSync(path.join(pkg, 'tag'), tag)
  fs.writeFileSync(path.join(pkg, 'index.js'), "exports.runExecutable = () => require('fs').readFileSync(require('path').join(__dirname, 'tag'), 'utf8')")
}
function instrument(execSync, fsOverrides = {}) {
  const module = { exports: {} }
  const injectedRequire = key => {
    if (key === 'child_process') return { execSync }
    if (key === 'fs') return { ...fs, ...fsOverrides }
    return require(key)
  }
  vm.runInNewContext(fs.readFileSync(helper, 'utf8'), { module, require: injectedRequire }, { filename: helper })
  return module.exports
}
function pointer(dir) {
  return JSON.parse(fs.readFileSync(path.join(dir, '.installed-platform.json'), 'utf8'))
}

test('loads a complete legacy cache without migration or npm', t => {
  const dir = fixture(t)
  writePackage(dir, 'legacy')
  assert.equal(cache.loadPlatformPackage(dir, name, version).runExecutable(), 'legacy')
  assert.ok(!fs.existsSync(path.join(dir, '.installed-platform.json')))
})

test('publishes only a successful loadable generation, leaving stdout isolated', t => {
  const dir = fixture(t)
  let destination
  const subject = instrument((command, options) => {
    assert.ok(command.includes('--no-save --no-audit --no-fund --prefix .'))
    assert.equal(path.dirname(options.cwd), dir)
    assert.match(path.basename(options.cwd), /^\.install-[a-zA-Z0-9]+$/)
    assert.equal(options.timeout, 60000)
    assert.deepEqual(Array.from(options.stdio), ['ignore', 'pipe', 'pipe'])
    assert.ok(!fs.existsSync(path.join(dir, '.installed-platform.json')))
    destination = options.cwd
    writePackage(destination, 'complete')
  })
  assert.equal(subject.installPlatformPackage(dir, name, version).runExecutable(), 'complete')
  assert.equal(path.join(dir, pointer(dir).directory), destination)
  assert.equal(cache.loadPlatformPackage(dir, name, version).runExecutable(), 'complete')
})

test('a failed attempt and retry never share a writable prefix', t => {
  const dir = fixture(t)
  const calls = []
  const subject = instrument((command, options) => {
    calls.push({ command, dir: options.cwd })
    writePackage(options.cwd, calls.length === 1 ? 'failed' : 'winner')
    if (calls.length === 1) throw Object.assign(new Error('timeout'), { code: 'ETIMEDOUT' })
  })
  const installed = subject.installPlatformPackage(dir, name, version)
  assert.equal(calls.length, 2)
  assert.notEqual(calls[0].dir, calls[1].dir)
  assert.ok(!calls[0].command.includes('--prefer-online'))
  assert.ok(calls[1].command.includes('--prefer-online'))
  assert.equal(installed.runExecutable(), 'winner')
  writePackage(calls[0].dir, 'late orphan write')
  assert.equal(installed.runExecutable(), 'winner')
  assert.equal(cache.loadPlatformPackage(dir, name, version).runExecutable(), 'winner')
})

test('two failed attempts leave no published reference', t => {
  const dir = fixture(t)
  const calls = []
  const subject = instrument((command, options) => {
    calls.push(options.cwd)
    writePackage(options.cwd, 'not ready')
    throw new Error('npm failed')
  })
  assert.throws(() => subject.installPlatformPackage(dir, name, version), /npm failed/)
  assert.equal(new Set(calls).size, 2)
  assert.ok(!fs.existsSync(path.join(dir, '.installed-platform.json')))
  assert.throws(() => cache.loadPlatformPackage(dir, name, version))
  assert.ok(calls.every(p => fs.existsSync(p)), 'failure directories may still have active writers')
})

test('a successful npm exit with an invalid entrypoint is not published', t => {
  const dir = fixture(t)
  const subject = instrument((command, options) => {
    writePackage(options.cwd, 'bad')
    fs.writeFileSync(path.join(options.cwd, 'node_modules', name, 'index.js'), 'module.exports = {}')
  })
  assert.throws(() => subject.installPlatformPackage(dir, name, version), /runExecutable/)
  assert.ok(!fs.existsSync(path.join(dir, '.installed-platform.json')))
})

test('publishing a successor does not move or remove a running generation', t => {
  const dir = fixture(t)
  let count = 0
  const subject = instrument((command, options) => writePackage(options.cwd, String(++count)))
  const first = subject.installPlatformPackage(dir, name, version)
  const firstDirectory = pointer(dir).directory
  const second = subject.installPlatformPackage(dir, name, version)
  assert.notEqual(pointer(dir).directory, firstDirectory)
  assert.equal(first.runExecutable(), '1', 'lazy relative reads must still work')
  assert.equal(second.runExecutable(), '2')
  assert.equal(cache.loadPlatformPackage(dir, name, version).runExecutable(), '2')
})

for (const directory of ['../outside', '/tmp/outside', '.install-a/../../outside', '.install-a\\..\\outside', '', null]) {
  test(`rejects invalid reference directory ${JSON.stringify(directory)}`, t => {
    const dir = fixture(t)
    fs.writeFileSync(path.join(dir, '.installed-platform.json'), JSON.stringify({ packageName: name, version, directory }))
    assert.throws(() => cache.loadPlatformPackage(dir, name, version), /Invalid platform cache/)
  })
}

for (const change of [{ packageName: 'wrong' }, { version: '9.9.9' }]) {
  test(`rejects a mismatched reference ${JSON.stringify(change)}`, t => {
    const dir = fixture(t)
    fs.writeFileSync(path.join(dir, '.installed-platform.json'), JSON.stringify({ packageName: name, version, directory: '.install-abc123', ...change }))
    assert.throws(() => cache.loadPlatformPackage(dir, name, version), /Invalid platform cache/)
  })
}

test('failed reference publication preserves the previous cache (injected I/O error)', t => {
  const dir = fixture(t)
  instrument((command, options) => writePackage(options.cwd, 'old')).installPlatformPackage(dir, name, version)
  const before = pointer(dir)
  const subject = instrument((command, options) => writePackage(options.cwd, 'new'), {
    renameSync() { throw Object.assign(new Error('publication failed'), { code: 'EIO' }) }
  })
  assert.throws(() => subject.installPlatformPackage(dir, name, version), /publication failed/)
  assert.deepEqual(pointer(dir), before)
  assert.equal(cache.loadPlatformPackage(dir, name, version).runExecutable(), 'old')
  assert.ok(!fs.readdirSync(dir).some(p => p.endsWith('.tmp')))
})
