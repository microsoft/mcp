# npm fallback installation: locks and published generations

The normal optional-dependency loading path is unchanged. This document concerns
only the fallback used when the wrapper cannot load its platform package.

## Ownership

`.install.lock` is published as a prepared, nonempty directory with a unique
`owner-<pid>-<uuid>` marker. Stale cleanup and release remove only the marker they
observed and then attempt a non-recursive `rmdir`. They must never recursively
remove or rename the shared lock directory.

A live owner is not displaced based on lock age. Waiting is bounded; a timeout is
an error, not permission to steal a lock. Complete legacy installations are
reused. A legacy PID lock is migrated only if that PID is definitely dead.
Ambiguous legacy lock files fail closed. Mixed old/new running implementations
and shared network filesystems are not supported by this proposed lock protocol.

## Installation isolation

A dead wrapper does not prove that its npm subprocess has stopped. Therefore,
each attempt, including a retry, installs under its own `.install-<random>`
directory within the private prefix. It never writes directly into another
attempt's directory or into an already published generation.

After npm exits successfully and the package exports a loadable `runExecutable`,
a temporary JSON reference is renamed to `.installed-platform.json`. The
reference records the package name, version and a restricted relative directory
name. Readers require the module from that stable directory. Publication does
not move an installed directory or remove a directory still used by a process.
This is atomic visibility on the tested local filesystem, not an fsync-based
power-loss durability guarantee or a content-integrity/security attestation.

Failed attempts remain unpublished. They are not immediately deleted because a
timed-out shell or dead wrapper may leave a writer running. A retry always uses a
new directory. When recovering after a wrapper death, two npm processes may
briefly run concurrently, but they cannot share a writable prefix.

## Tradeoffs

Legacy PID-only files are inspected before attempting publication. On Windows,
renaming a directory over a file can succeed, so a rename collision alone cannot
protect a live legacy owner. Only a definitely dead legacy PID is removed.
Concurrent use of old file-lock publishers and new directory-lock publishers is
outside this protocol's guarantee. Publication collisions, including permission
errors when an incumbent releases before inspection, retry within the bounded
deadline. Persistent publication permission errors therefore report a timeout.

Abandoned directories consume space; this patch intentionally does not add an
unsafe automatic garbage collector. Cleanup is an offline maintenance operation:
stop the relevant wrappers and installers before removing a private cache prefix.
A successful npm command is assumed to have completed installation; deliberately
lifecycle scripts started in the background that continue modifying a published directory are
not made safe by this wrapper. PID namespaces and machines sharing a cache are
outside this same-host protocol.

## Targeted tests

```sh
node --test eng/npm/test/install-lock.test.js eng/npm/test/wrapper.test.js eng/npm/test/wrapper-lock-race.test.js eng/npm/test/platform-cache.test.js eng/npm/test/wrapper-orphan-install.test.js
```

The orphan regression is Linux-only and uses real processes with a local npm
stub. Other tests separately inject command failures to inspect unpublished
attempts. The accompanying audit kit includes a real npm CLI check against a
loopback-only, synthetic registry. None of these are Azure service tests.
