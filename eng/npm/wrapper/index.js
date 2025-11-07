#!/usr/bin/env node

const os = require('os')
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

    // Try to install the platform package
    try {
      execSync(`npm install ${platformPackageName}@${packageVersion}`, {
        stdio: ['inherit', 'inherit', 'pipe'], // Only pipe stderr to capture errors
        timeout: 60000 // 60 second timeout
      })
    } catch (npmErr) {
      // If npm install fails, try with --no-save and different install strategies
      debugLog(`npm install failed, trying alternative installation methods: ${npmErr.message}`)

      // Try with --no-save and --prefer-online
      execSync(`npm install ${platformPackageName}@${packageVersion} --no-save --prefer-online`, {
        stdio: ['inherit', 'inherit', 'pipe'],
        timeout: 60000
      })
    }

    // Clear module cache and try to require again after installation
    Object.keys(require.cache).forEach(key => {
      if (key.includes(platformPackageName)) {
        delete require.cache[key]
      }
    })

    platformPackage = require(platformPackageName)

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
    console.error(`\n5. Check your internet connection and try again`)
    console.error(`\n6. If the issue persists, please report it at:`)
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
