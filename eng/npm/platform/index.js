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

    // Non-interactive hosts may signal either the wrapper PID or its group.
    // A separate child group gives both requests one forwarding path. Leave
    // terminal jobs in their original group so input and job control work.
    const forwardSignals = process.platform !== 'win32' &&
      ![process.stdin, process.stdout, process.stderr].some(stream => stream?.isTTY)
    const child = forwardSignals
      ? childProcess.fork(path.join(__dirname, 'supervisor.js'), [execPath, ...args], {
          stdio: ['inherit', 'inherit', 'inherit', 'ipc'],
          execArgv: [],
          detached: true
        })
      : childProcess.spawn(execPath, args, {
          stdio: 'inherit',
          shell: false
        })

    return new Promise((resolve) => {
      const signalHandlers = new Map()
      const pendingSignals = []
      let supervisorReady = false
      let supervisedExitCode
      let finished = false
      const forward = (signal) => {
        if (finished || child.pid === undefined || child.exitCode !== null || child.signalCode !== null) return
        try {
          process.kill(-child.pid, signal)
        } catch (err) {
          if (err.code !== 'ESRCH') console.error(`Error forwarding ${signal}: ${err.message}`)
        }
      }
      const handleSupervisorMessage = (message) => {
        if (finished) return
        if (message?.type === 'exit' && Number.isInteger(message.code) && message.code >= 0 && message.code <= 255) {
          supervisedExitCode = message.code
          return
        }
        if (supervisorReady || message?.type !== 'ready') return
        supervisorReady = true
        for (const signal of pendingSignals.splice(0)) forward(signal)
      }
      const finish = (code) => {
        if (finished) return
        finished = true
        for (const [signal, handler] of signalHandlers) {
          process.removeListener(signal, handler)
        }
        child.removeListener('message', handleSupervisorMessage)
        pendingSignals.length = 0
        resolve(code)
      }

      child.on('error', (err) => {
        console.error(`Error executing package: ${err.message}`)
        // A failed spawn has no PID. A later error (for example a failed kill)
        // must not make the wrapper exit while its child is still running.
        if (child.pid === undefined) finish(1)
      })

      child.once('exit', (code) => {
        if (!forwardSignals) finish(code ?? 1)
      })
      if (forwardSignals) {
        // IPC can still contain the native exit status when 'exit' fires.
        // 'close' follows channel drainage; the supervisor kills its own
        // group after sending that status, so helpers cannot outlive it.
        child.once('close', () => finish(supervisedExitCode ?? 1))
      }

      // Keep the child referenced and await its exit. Forward to the owned
      // group so helpers that remain in that group receive the request too.
      // Do not infer that the child has stopped just because kill succeeded.
      if (forwardSignals) {
        // Wait until the supervisor has installed its handlers AND spawned
        // the executable. A signal during Node startup must not kill the
        // supervisor first or be swallowed before its native child exists.
        child.on('message', handleSupervisorMessage)
        for (const signal of ['SIGINT', 'SIGTERM', 'SIGHUP', 'SIGQUIT']) {
          const handler = () => {
            if (child.pid === undefined || child.exitCode !== null || child.signalCode !== null) return
            if (supervisorReady) forward(signal)
            else pendingSignals.push(signal)
          }
          signalHandlers.set(signal, handler)
          process.on(signal, handler)
        }
      }
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
