# npm launcher shutdown on POSIX non-terminal hosts

This change applies only on non-Windows hosts when none of stdin, stdout and
stderr is a terminal. Windows and any-terminal launches retain the existing
direct spawn path with no new signal subscriptions or process-group changes.

## One signal path

For the non-terminal path the wrapper starts a small Node supervisor using
`fork`, an IPC channel and `detached: true`. The supervisor leads a new session
and process group and starts the native executable without detaching it again.
Standard streams are inherited; the IPC channel is separate from protocol I/O.

The outer wrapper forwards SIGINT, SIGTERM, SIGHUP and SIGQUIT to that owned
group. The supervisor does not forward those signals again and waits for the
native child's exit. Both a PID-only request and a request to the wrapper's
original group thus take one forwarding path to the native process and helpers
that stay in its group. Distinct subsequent caller signals are not debounced.
Normal numeric exit statuses, including zero, are preserved; a missing numeric
status is reported as failure. Temporary wrapper listeners are removed on exit.

The supervisor announces readiness over IPC after the native executable has
spawned. Shutdown requests received during startup are queued until that
handshake. This prevents a request from being ignored before the native child
exists, or from terminating the supervisor before it installs its handlers.
Readiness messages do not use stdout and cannot contaminate the MCP protocol.

When the native executable exits, the supervisor sends its numeric result over
IPC, then stops the entire owned group with SIGKILL, including itself. This also
closes helpers that the native executable left running. The outer wrapper waits
for the supervisor's `close` event so queued IPC status is drained before it
returns the native result. A killed supervisor without a native status remains
a failure; it cannot accidentally turn into a successful exit.

## Abrupt loss of the outer wrapper

Simply detaching the native child would introduce a new orphan risk when the
outer wrapper is killed. The supervisor monitors its private parent IPC channel.
On disconnect it sends SIGKILL to its own group, including itself. It does not
pretend that JavaScript can catch SIGKILL in the already terminated parent.

The native executable inherits only standard descriptors, not the IPC channel.
The parent-loss behavior is tested with stdin deliberately kept open, by killing
both the wrapper PID and its original group and checking that a real loopback
TCP listener disappears. A separate startup regression holds native spawn,
kills the outer wrapper, then releases spawn and checks that inherited streams
close and any listener created during that race is gone.

## Deliberate boundaries and costs

This path adds one Node process to each non-terminal invocation. It owns the
group and detects abrupt parent loss without a native helper dependency. The
extra startup and resident-memory cost should be considered when running many
server instances or short CLI invocations.

Graceful shutdown still waits for the native child. There is no invented forced
shutdown timer. Terminal-only PID signalling remains unchanged; terminal jobs
normally receive foreground-group signals. A helper that creates a new session
can escape the owned group. Forcible loss of the supervisor itself, arbitrary
process-tree damage, permission changes and full PID 1/init supervision are not
covered. Native product smoke checks use published binaries with the modified
launcher. They validate protocol initialization and EOF without calling service
tools, and do not replace platform signal tests or a build of current C# source.

## Targeted tests

```sh
node --test eng/npm/test/platform.test.js eng/npm/test/platform-lifecycle.test.js eng/npm/test/platform-signals.test.js eng/npm/test/platform-supervisor.test.js
```

The signal tests use real launcher and Node fixture processes; lifecycle tests
also use labelled event-level doubles. The accompanying audit kit contains a
real Linux PTY comparison for terminal input, group preservation and Ctrl-C.
The process tests also compare complete stdout and stderr payloads larger than
a pipe buffer after native exit, through both wrapper and platform entry points.
These POSIX tests are skipped on Windows; those skips are not signal validation.
