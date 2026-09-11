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
const installDirs = [
  path.join(__dirname, '.platform'),
  path.join(userCacheDir(), 'microsoft-mcp', platformPackageName, packageVersion)
]

function userCacheDir() {
  if (platform === 'win32') {
    return process.env.LOCALAPPDATA || path.join(os.homedir(), 'AppData', 'Local')
  }
  if (platform === 'darwin') {
    return path.join(os.homedir(), 'Library', 'Caches')
  }
  return process.env.XDG_CACHE_HOME || path.join(os.homedir(), '.cache')
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

// Try to load the platform package
let platformPackage
let loadErr
try {
  debugLog(`Attempting to require platform package: ${platformPackageName}`)
  platformPackage = require(platformPackageName)
} catch (err) {
  loadErr = err
  debugLog(`Failed to require ${platformPackageName}: ${err.message}`)

  // Node never searches the install directories on its own, so an earlier
  // auto-install is only found by looking there. Without this, every run would
  // reinstall. A failed require (not installed, or a half-finished install)
  // just falls through to installing again.
  for (const dir of installDirs) {
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
  try {
    const { execSync } = require('child_process')

    console.error(`Installing missing platform package: ${platformPackageName}`)

    const installDir = installDirs.find(isWritableDir)
    if (!installDir) {
      throw new Error(`No writable directory to install into (tried ${installDirs.join(', ')})`)
    }
    debugLog(`Installing ${platformPackageName} into ${installDir}`)

    // stdout is not inherited: for `server start` it carries the JSON-RPC
    // protocol stream, and npm output written there would corrupt it.
    const installOptions = {
      cwd: installDir,
      stdio: ['ignore', 'pipe', 'pipe'],
      timeout: 60000 // 60 second timeout
    }
    const installCommand = `npm install ${platformPackageName}@${packageVersion} --no-save --no-audit --no-fund --prefix "${installDir}"`

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

    platformPackage = requireFromInstallDir(installDir)

    console.error(`✅ Successfully installed and loaded ${platformPackageName}`)

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
    console.error(`\n4. Manually install the platform package to check compatibility:`)
    console.error(`   npm install ${platformPackageName}@latest`)
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
