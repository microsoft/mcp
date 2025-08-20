#!/usr/bin/env node

const path = require('path')
const fs = require('fs')
const childProcess = require('child_process')

// Check if DEBUG environment variable is set
const isDebugMode = process.env.DEBUG && (
  process.env.DEBUG.toLowerCase() === 'true' ||
  process.env.DEBUG === '*'
)

// Helper function for debug logging
function debugLog(...args) {
  if (isDebugMode) {
    console.error(...args)
  }
}

/**
 * Run the platform-specific executable with the given arguments
 * @param {string[]} args Arguments to pass to the executable
 * @returns {Promise<number>} Returns a Promise that resolves to the exit code
 */
function runExecutable(args = []) {
  debugLog('\nPlatform package starting')
  debugLog('All args:')
  args.forEach((val, index) => {
    debugLog(`${index}: ${val}`)
  })

  try {
    const packageJson = require('./package.json')
    const execName = Object.values(packageJson.bin)[0]
    debugLog('Found executable in package.json:', execName)

    // The platform-specific executable should be in the same folder
    const execPath = path.join(__dirname, execName)
    debugLog('Executable path:', execPath)

    if (!fs.existsSync(execPath)) {
      console.error(`Executable "${execPath}" not found.`)
      return Promise.resolve(1)
    }

    debugLog(`Starting ${execPath}`) // Only logs in debug mode

    const child = childProcess.spawn(execPath, args, {
      stdio: 'inherit',
      shell: false
    })

    return new Promise((resolve) => {
      child.on('error', (err) => {
        console.error(`Error executing package: ${err.message}`)
        resolve(1)
      })

      child.on('exit', (code) => {
        resolve(code || 0)
      })
    })
  } catch (err) {
    console.error(`Error running executable: ${err.message}`)
    return Promise.resolve(1)
  }
}

// Check if this file is being run directly
if (require.main === module) {
  // Run the executable with command line args and exit with its code
  runExecutable(process.argv.slice(2))
    .then(code => process.exit(code))
} else {
  // Export the function for consumers to use
  module.exports = { runExecutable }
}
