#!/usr/bin/env node

const fs = require('fs')
const os = require('os')
const path = require('path')
const packageJson = require('./package.json')

// Check if DEBUG environment variable is set
const isDebugMode = process.env.DEBUG && (
  process.env.DEBUG.toLowerCase() === 'true' ||
  process.env.DEBUG.includes('mcp') ||
  process.env.DEBUG === '*'
)

// Helper function for debug logging
function debugLog(...args) {
  if (isDebugMode) {
    console.error(...args)
  }
}

debugLog('\nWrapper package starting')
debugLog('All args:')
process.argv.forEach((val, index) => {
  debugLog(`${index}: ${val}`)
})

const platform = os.platform()
const arch = os.arch()

const packageName = packageJson.name
const packageVersion = packageJson.version
const platformPackageName = `${packageName}-${platform}-${arch}`

// Where the auto-install fallback below puts the platform package, in order of
// preference. It never goes in the caller's project: cwd is inherited from
// whoever spawned us - for an MCP client that is an unrelated user project -
// and a bare `npm install` resolves the nearest package.json from there and
// saves the platform package into it.
//
// The first choice lives inside this package, so it is tied to this version
// and goes away on uninstall. It is not writable for root-owned global installs
// or read-only filesystems, so the second is a per-user cache keyed by package
// and version. The OS temp directory is deliberately not used: on a shared
// machine another user could plant a package there for us to load.
//
// Neither directory has a package.json of its own: pointing npm at this
// package's manifest is a no-op, because the platform package is declared
// there as an optionalDependency that npm is already skipping (that skip is
// usually why we are in this code path at all).
//
// This only runs once the normal require has failed, so a working install never
// touches the home directory: os.homedir() throws when HOME is unset and the
// user has no passwd entry.
function getInstallDirs() {
  const dirs = [path.join(__dirname, '.platform')]
  try {
    dirs.push(path.join(userCacheDir(), 'microsoft-mcp', platformPackageName, packageVersion))
  } catch (err) {
    debugLog(`No per-user cache directory available: ${err.message}`)
  }
  return dirs
}

function userCacheDir() {
  if (platform === 'win32') {
    return process.env.LOCALAPPDATA || path.join(os.homedir(), 'AppData', 'Local')
  }
  if (platform === 'darwin') {
    return path.join(os.homedir(), 'Library', 'Caches')
  }
  // The XDG spec says to ignore a relative XDG_CACHE_HOME. Honoring one would
  // put the cache under the caller's working directory.
  const xdgCacheHome = process.env.XDG_CACHE_HOME
  return xdgCacheHome && path.isAbsolute(xdgCacheHome) ? xdgCacheHome : path.join(os.homedir(), '.cache')
}

function requireFromInstallDir(installDir) {
  return require(path.join(installDir, 'node_modules', platformPackageName))
}

function isWritableDir(dir) {
  try {
    fs.mkdirSync(dir, { recursive: true })
    fs.accessSync(dir, fs.constants.W_OK)
    return true
  } catch (err) {
    debugLog(`Cannot install platform package into ${dir}: ${err.message}`)
    return false
  }
}

// MCP clients often start several copies of a server at once, and they all
// share one install directory. Concurrent npm installs into one prefix undo
// each other's work (one moves away the package another has just finished), so
// a lock file lets one process install while the others wait and then load
// what it installed.
const installLockName = '.install.lock'
// Longer than both npm attempts together. The PID check below catches a dead
// holder sooner; this is the backstop for a PID that has been reused.
const installLockStaleMs = 3 * 60 * 1000
const installLockPollMs = 250

// A holder that is killed - say, by an MCP client giving up on a slow first
// start - never releases its lock, so check whether it is still running rather
// than waiting the lock out.
function isStaleLock(lockPath) {
  if (Date.now() - fs.statSync(lockPath).mtimeMs > installLockStaleMs) {
    return true
  }
  const pid = Number(fs.readFileSync(lockPath, 'utf8'))
  if (!pid) {
    return false // created, but the holder has not written its PID yet
  }
  try {
    process.kill(pid, 0)
    return false
  } catch (err) {
    return err.code === 'ESRCH'
  }
}

function isInstallInProgress(dir) {
  try {
    return !isStaleLock(path.join(dir, installLockName))
  } catch {
    return false // no lock
  }
}

function sleep(ms) {
  Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, ms)
}

function acquireInstallLock(installDir) {
  const lockPath = path.join(installDir, installLockName)
  let announced = false
  for (;;) {
    try {
      fs.writeFileSync(lockPath, String(process.pid), { flag: 'wx' })
      return lockPath
    } catch (err) {
      if (err.code !== 'EEXIST') {
        throw err
      }
    }

    try {
      if (isStaleLock(lockPath)) {
        debugLog(`Removing stale install lock ${lockPath}`)
        fs.rmSync(lockPath, { force: true })
        continue
      }
    } catch (err) {
      if (err.code !== 'ENOENT') {
        throw err
      }
      continue // released between our attempt and the check
    }

    if (!announced) {
      debugLog(`Waiting for another process to finish installing into ${installDir}`)
      announced = true
    }
    sleep(installLockPollMs)
  }
}

// Call with the install lock held.
function loadOrInstall(installDir) {
  // Another process may have installed it while we waited for the lock.
  try {
    const installed = requireFromInstallDir(installDir)
    debugLog(`Loaded ${platformPackageName} installed into ${installDir} by another process`)
    return installed
  } catch {
    // Not installed yet
  }

  const { execSync } = require('child_process')

  console.error(`Installing missing platform package: ${platformPackageName}`)
  debugLog(`Installing ${platformPackageName} into ${installDir}`)

  // stdout is not inherited: for `server start` it carries the JSON-RPC
  // protocol stream, and npm output written there would corrupt it.
  const installOptions = {
    cwd: installDir,
    stdio: ['ignore', 'pipe', 'pipe'],
    timeout: 60000 // 60 second timeout
  }
  // --prefix is relative to cwd so that no path goes through the shell, which
  // would expand any `$` or backtick in it.
  const installCommand = `npm install ${platformPackageName}@${packageVersion} --no-save --no-audit --no-fund --prefix .`

  // Try to install the platform package
  try {
    execSync(installCommand, installOptions)
  } catch (npmErr) {
    // If npm install fails, try again against the registry
    debugLog(`npm install failed, trying alternative installation methods: ${npmErr.message}`)

    execSync(`${installCommand} --prefer-online`, installOptions)
  }

  // Clear module cache and try to require again after installation
  Object.keys(require.cache).forEach(key => {
    if (key.includes(platformPackageName)) {
      delete require.cache[key]
    }
  })

  const installed = requireFromInstallDir(installDir)
  console.error(`✅ Successfully installed and loaded ${platformPackageName}`)
  return installed
}

// Try to load the platform package
let platformPackage
let loadErr
let installDirs
try {
  debugLog(`Attempting to require platform package: ${platformPackageName}`)
  platformPackage = require(platformPackageName)
} catch (err) {
  loadErr = err
  debugLog(`Failed to require ${platformPackageName}: ${err.message}`)

  // Node never searches the install directories on its own, so an earlier
  // auto-install is only found by looking there. Without this, every run would
  // reinstall. A failed require (not installed, or a half-finished install)
  // just falls through to installing again, and a directory another process is
  // still installing into is left to the locked path below.
  installDirs = getInstallDirs()
  for (const dir of installDirs) {
    if (isInstallInProgress(dir)) {
      debugLog(`Skipping ${dir}: another process is installing into it`)
      continue
    }
    try {
      platformPackage = requireFromInstallDir(dir)
      debugLog(`Loaded previously installed ${platformPackageName} from ${dir}`)
      break
    } catch (cachedErr) {
      debugLog(`No usable ${platformPackageName} in ${dir}: ${cachedErr.message}`)
    }
  }
}

if (!platformPackage) {
  // Try to automatically install the missing platform package
  let installDir
  try {
    installDir = installDirs.find(isWritableDir)
    if (!installDir) {
      throw new Error(`No writable directory to install into (tried ${installDirs.join(', ')})`)
    }

    const lockPath = acquireInstallLock(installDir)
    try {
      platformPackage = loadOrInstall(installDir)
    } finally {
      fs.rmSync(lockPath, { force: true })
    }
  } catch (installErr) {
    debugLog(`Auto-install failed: ${installErr.message}`)

    console.error(`\n❌ Failed to load platform specific package '${platformPackageName}'`)
    console.error(`\n🔍 Troubleshooting steps:`)
    console.error(`\n1. Clear npm cache:`)
    console.error(`   npm cache clean --force`)
    console.error(`\n2. If installing as a global tool, uninstall and reinstall:`)
    console.error(`   npm uninstall -g ${packageName}`)
    console.error(`   npm install -g ${packageName}`)
    console.error(`\n3. If using npx, clear the npx cache and try again:`)
    console.error(`   npx -y clear-npx-cache`)
    console.error(`   npx -y ${packageName}@latest --version`)
    console.error(`\n4. Install the platform package manually to see npm's full output:`)
    console.error(`   npm install ${platformPackageName}@${packageVersion} --no-save --prefix "${installDir || installDirs[0]}"`)
    console.error(`\n5. Check your Node version. npm silently skips the optional platform`)
    console.error(`   package when Node does not satisfy engines (${(packageJson.engines || {}).node}).`)
    console.error(`   You are running ${process.version}.`)
    console.error(`\n6. Check your internet connection and try again`)
    console.error(`\n7. If the issue persists, please report it at:`)
    console.error(`   https://github.com/microsoft/mcp/issues`)
    console.error(`\nOriginal error: ${loadErr.message}`)
    console.error(`Install error: ${installErr.message}`)
    process.exit(1)
  }
}

platformPackage.runExecutable(process.argv.slice(2))
  .then((code) => {
    debugLog(`Process exited with code: ${code}`)
    process.exit(code)
  })
  .catch((err) => {
    console.error(`Error: ${err.message}`)
    process.exit(1)
  })
