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

// Try to load the platform package
let platformPackage
try {
  debugLog(`Attempting to require platform package: ${platformPackageName}`)
  platformPackage = require(platformPackageName)
} catch (err) {
  debugLog(`Failed to require ${platformPackageName}, attempting auto-install: ${err.message}`)

  // Try to automatically install the missing platform package
  try {
    const { execSync } = require('child_process')

    console.error(`Installing missing platform package: ${platformPackageName}`)

    // Install into a private directory owned by this package, never the
    // caller's. cwd is inherited from whoever spawned us - for an MCP client
    // that is an unrelated user project - and a bare `npm install` resolves the
    // nearest package.json from there and saves the platform package into it.
    //
    // The directory deliberately has no package.json of its own: pointing npm
    // at this package's manifest is a no-op, because the platform package is
    // declared there as an optionalDependency that npm is already skipping
    // (that skip is usually why we are in this code path at all).
    const installDir = path.join(__dirname, '.platform')
    fs.mkdirSync(installDir, { recursive: true })

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

    platformPackage = require(path.join(installDir, 'node_modules', platformPackageName))

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
    console.error(`\nOriginal error: ${err.message}`)
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
