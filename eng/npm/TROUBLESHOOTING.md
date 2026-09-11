# Wrapper package troubleshooting

To see debug text from the wrapper and platform packages, set the `DEBUG` environment variable to `true`:

```powershell
> $env:DEBUG='true'   #`export DEBUG=true` in bash
> npx --yes @azure/mcp@latest --version
```
```
Attempting to require platform package: @azure/mcp-linux-x64
All args:
0: /usr/bin/node
1: /home/user/.npm/_npx/541454632b79112e/node_modules/.bin/azmcp
2: --version
All args:
0: --version
Found executable in package.json: azmcp
Executable path: /home/user/.npm/_npx/541454632b79112e/node_modules/@azure/mcp-linux-x64/azmcp
Starting /home/user/.npm/_npx/541454632b79112e/node_modules/@azure/mcp-linux-x64/azmcp
0.0.6+90c3def5f15860420244db365c04eb302494da5b
Process exited with code: 0
```

This is useful if you want to see where the package is installed and how it's resolving the platform specific dependency.

# Platform package auto-install

The wrapper runs the server from a platform-specific package (for example `@azure/mcp-linux-x64`) that npm installs as an optional dependency. npm skips it silently in some cases, most commonly when the running Node version does not satisfy the package's `engines` field. When the wrapper can't find it, it runs `npm install --no-save` itself, into the first writable one of:

1. A `.platform` directory inside the wrapper package.
2. A per-user cache directory: `~/Library/Caches/microsoft-mcp/<package>/<version>` on macOS, `%LOCALAPPDATA%\microsoft-mcp\<package>\<version>` on Windows, and `$XDG_CACHE_HOME/microsoft-mcp/<package>/<version>` (default `~/.cache`) elsewhere.

It never installs into the current working directory. Later runs reuse the install, and with `DEBUG=true` the wrapper logs which directory it used. Delete that directory to force a fresh install.

# NPX Debugging

To debug javascript invoked during the npx call, you can start the run with the node option `--inspect-brk` enabled:
```
npx --node-options="--inspect-brk" @azure/mcp@latest 
```

This will start npx, but will wait for a debugger to attach before continuing.  
See [VSCode's node debugging documentation](https://code.visualstudio.com/docs/nodejs/nodejs-debugging#_attaching-to-nodejs) for details.
