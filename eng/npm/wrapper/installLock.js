// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const fs = require('fs')
const path = require('path')
const { randomUUID } = require('crypto')

const lockName = '.install.lock'
const pollMs = 250
const waitMs = 3 * 60 * 1000
const ownerPattern = /^owner-([1-9][0-9]*)-[0-9a-f-]{36}$/

function sleep(ms) {
  Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, ms)
}

function ownerIsDead(pid) {
  if (!Number.isSafeInteger(pid) || pid <= 0) return false
  try {
    process.kill(pid, 0)
    return false
  } catch (err) {
    // EPERM is not evidence that a process has died. Never steal a live lock
    // because of its age; a reused PID can instead cause a bounded timeout.
    return err.code === 'ESRCH'
  }
}

function removeEmptyDirectory(lockPath) {
  try {
    fs.rmdirSync(lockPath)
  } catch (err) {
    // A successor always publishes a NONEMPTY directory. Do not recursively
    // remove it: rmdir must fail if a new owner won the race.
    if (!['ENOENT', 'ENOTEMPTY', 'EEXIST', 'ENOTDIR'].includes(err.code)) throw err
  }
}

function removeOwner(lockPath, owner) {
  try {
    // The unique marker identifies the observed generation, not just its PID.
    fs.unlinkSync(path.join(lockPath, owner))
  } catch (err) {
    if (err.code !== 'ENOENT' && err.code !== 'ENOTDIR') throw err
  }
  removeEmptyDirectory(lockPath)
}

function removeDeadOwner(lockPath) {
  try {
    const stat = fs.lstatSync(lockPath)
    if (stat.isDirectory()) {
      const owners = fs.readdirSync(lockPath)
      if (owners.length === 0) {
        removeEmptyDirectory(lockPath)
      } else if (owners.length === 1) {
        const match = ownerPattern.exec(owners[0])
        if (match && ownerIsDead(Number(match[1]))) removeOwner(lockPath, owners[0])
      }
      return
    }

    if (stat.isFile()) {
      // Migrate an old PID-only lock only when its owner is definitely dead.
      // unlink cannot remove the directory published by another new wrapper.
      const pid = Number(fs.readFileSync(lockPath, 'utf8'))
      if (ownerIsDead(pid)) fs.unlinkSync(lockPath)
    }
  } catch (err) {
    if (['ENOENT', 'ENOTDIR', 'EISDIR'].includes(err.code)) return
    // Windows can report EPERM when unlink races with directory publication.
    if (err.code === 'EPERM') {
      try {
        if (fs.lstatSync(lockPath).isDirectory()) return
      } catch (statErr) {
        if (statErr.code === 'ENOENT') return
      }
    }
    throw err
  }
}

function isInstallInProgress(installDir) {
  try {
    fs.lstatSync(path.join(installDir, lockName))
    // Even a stale/unknown lock requires the locked path before loading a
    // potentially incomplete package. Errors other than absence fail closed.
    return true
  } catch (err) {
    // A non-directory install path cannot contain a lock; let the wrapper
    // fall back to its per-user cache just as it does for a missing path.
    if (err.code === 'ENOENT' || err.code === 'ENOTDIR') return false
    // An unreadable prefix may still have a valid per-user cache alternative.
    // Skip unlocked loading here rather than aborting the fallback search.
    if (err.code === 'EACCES' || err.code === 'EPERM') return true
    throw err
  }
}

/** Acquire an install lock and return an idempotent release function.
 *
 * Publish a prepared, nonempty directory with rename. Both acquisition and
 * stale cleanup preserve the invariant that a live owner's directory is never
 * empty. This is a local-filesystem, same-host protocol, not a distributed lock.
 * timeoutMs bounds waiting; it is NOT a lease that permits stealing a live lock.
 */
function acquireInstallLock(installDir, debugLog = () => {}, timeoutMs = waitMs) {
  const lockPath = path.join(installDir, lockName)
  const candidate = fs.mkdtempSync(path.join(installDir, '.install-candidate-'))
  const owner = `owner-${process.pid}-${randomUUID()}`
  const deadline = performance.now() + timeoutMs
  let published = false
  let announced = false
  try {
    fs.writeFileSync(path.join(candidate, owner), '', { flag: 'wx' })
    for (;;) {
      // Windows can rename a directory over an existing FILE. Inspect legacy
      // locks before publication so a live or ambiguous PID is never replaced.
      // New wrappers publish only nonempty directories; running old and new
      // lock protocols concurrently remains outside this protocol's guarantee.
      let legacyLock = false
      try {
        legacyLock = fs.lstatSync(lockPath).isFile()
      } catch (err) {
        if (err.code !== 'ENOENT') throw err
      }
      if (!legacyLock) {
        try {
          fs.renameSync(candidate, lockPath)
          published = true
          let released = false
          return () => {
            if (released) return
            removeOwner(lockPath, owner)
            released = true
          }
        } catch (err) {
          // POSIX refuses to replace a nonempty directory; Windows may report
          // access denied for the same collision. Other errors propagate.
          if (!['EEXIST', 'ENOTEMPTY', 'EISDIR', 'ENOTDIR', 'EPERM', 'EACCES'].includes(err.code)) throw err
          // The incumbent may release after rename fails but before inspection.
          // Retry within the same bounded deadline, including Windows EPERM.
        }
      }

      removeDeadOwner(lockPath)
      if (performance.now() >= deadline) {
        throw new Error(`Timed out waiting for platform package installation in ${installDir}`)
      }
      if (!announced) {
        debugLog(`Waiting for another process to finish installing into ${installDir}`)
        announced = true
      }
      sleep(Math.min(pollMs, Math.max(0, deadline - performance.now())))
    }
  } finally {
    // This path is private to this attempt, never the shared lock path.
    if (!published) fs.rmSync(candidate, { recursive: true, force: true })
  }
}

module.exports = { acquireInstallLock, isInstallInProgress }
