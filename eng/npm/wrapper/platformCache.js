// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const fs = require('fs')
const path = require('path')
const { randomUUID } = require('crypto')
const { execSync } = require('child_process')

const pointerName = '.installed-platform.json'
const generationPattern = /^\.install-[a-zA-Z0-9]+$/

function requireGeneration(installDir, directory, packageName) {
  const installed = require(path.join(installDir, directory, 'node_modules', packageName))
  if (typeof installed?.runExecutable !== 'function') {
    throw new Error('Installed platform package does not export runExecutable')
  }
  return installed
}

function loadPlatformPackage(installDir, packageName, packageVersion) {
  let pointer
  try {
    pointer = JSON.parse(fs.readFileSync(path.join(installDir, pointerName), 'utf8'))
  } catch (err) {
    if (err.code !== 'ENOENT') throw err
    // Existing, complete installations remain usable without migration.
    return requireGeneration(installDir, '', packageName)
  }

  if (pointer?.packageName !== packageName || pointer?.version !== packageVersion ||
      typeof pointer?.directory !== 'string' || !generationPattern.test(pointer.directory)) {
    throw new Error('Invalid platform cache reference')
  }
  return requireGeneration(installDir, pointer.directory, packageName)
}

function publishGeneration(installDir, directory, packageName, packageVersion) {
  const temporary = path.join(installDir, `.installed-platform-${randomUUID()}.tmp`)
  try {
    fs.writeFileSync(temporary, JSON.stringify({ packageName, version: packageVersion, directory }), { flag: 'wx' })
    // Only the small reference is replaced; installed directories are never
    // moved or deleted underneath a module or a running native executable.
    fs.renameSync(temporary, path.join(installDir, pointerName))
  } finally {
    fs.rmSync(temporary, { force: true })
  }
}

// Call under the install lock. An orphaned npm process can outlive the lock
// holder, so every attempt also needs a private destination. Only a successful
// command with a loadable entrypoint is published. Failure (including timeout)
// leaves that attempt unpublished; it is never reused by a retry or recovery.
function installPlatformPackage(installDir, packageName, packageVersion, debugLog = () => {}) {
  const command = `npm install ${packageName}@${packageVersion} --no-save --no-audit --no-fund --prefix .`
  for (let attempt = 0; attempt < 2; attempt++) {
    const generation = fs.mkdtempSync(path.join(installDir, '.install-'))
    try {
      execSync(command + (attempt === 1 ? ' --prefer-online' : ''), {
        cwd: generation,
        stdio: ['ignore', 'pipe', 'pipe'],
        timeout: 60000
      })
    } catch (err) {
      // Do not delete the directory here: npm or one of its children may still
      // be writing after the shell times out. Never publish a failed attempt.
      if (attempt === 1) throw err
      debugLog(`npm install failed; retrying in a new directory: ${err.message}`)
      continue
    }

    const directory = path.basename(generation)
    const installed = requireGeneration(installDir, directory, packageName)
    publishGeneration(installDir, directory, packageName, packageVersion)
    return installed
  }
}

module.exports = { loadPlatformPackage, installPlatformPackage }
