// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Internal POSIX, non-terminal launcher. Started with fork + detached + IPC.
// It owns the native child's group; stdout/stderr remain protocol-transparent.
const { spawn } = require('child_process')

if (typeof process.send !== 'function') {
  console.error('The platform supervisor requires a parent IPC channel.')
  process.exit(1)
}

// Group signals are already delivered directly to the native child. The
// supervisor must not forward them again or exit before the child finishes.
for (const signal of ['SIGINT', 'SIGTERM', 'SIGHUP', 'SIGQUIT']) {
  process.on(signal, () => {})
}

// Unlike a signal callback, IPC disconnect also detects SIGKILL of the outer
// wrapper. The channel is not inherited by the native executable. Terminate
// this entire owned group (including this supervisor) on parent loss.
function stopOwnedGroup() {
  try {
    process.kill(-process.pid, 'SIGKILL')
  } catch (err) {
    console.error(`Error stopping orphaned platform group: ${err.message}`)
    process.exit(1)
  }
}
process.on('disconnect', stopOwnedGroup)
if (!process.connected) stopOwnedGroup()

try {
  const child = spawn(process.argv[2], process.argv.slice(3), {
    stdio: 'inherit',
    shell: false
  })
  child.once('spawn', () => {
    if (!process.connected) return stopOwnedGroup()
    process.send({ type: 'ready' }, err => {
      if (err) stopOwnedGroup()
    })
  })
  child.on('error', err => {
    console.error(`Error executing package: ${err.message}`)
    if (child.pid === undefined) process.exit(1)
  })
  child.once('exit', code => {
    if (!process.connected) return stopOwnedGroup()
    // Send the native result before killing the whole owned group, including
    // this supervisor and any helpers left behind by a stopped native child.
    // The parent drains IPC before resolving its returned exit status.
    process.send({ type: 'exit', code: code ?? 1 }, () => stopOwnedGroup())
  })
} catch (err) {
  console.error(`Error executing package: ${err.message}`)
  process.exit(1)
}
