## Wrapper package

The server cli is invocable through the same cross-platform "wrapper" package using `npx -yes @sample/mcp-server` on Windows, Mac and Linux, x64 and arm. This requires building different platform and cpu architecture specific executables, with each compiling to > 70MB.

The main package (without a platform suffix like `linux-x64`) contains just `index.js`. `index.js` is responsible for detecting the platform and cpu it's running on, loading the platform specific package, then passing all of its cli args to the platform package's `index.js` file.

The `index.js` file is set as the package's `bin` entry with the key like `mcpsrv`.  This allows it to be the entrypoint for `npx` calls as well as placing the command `mcpsrv` in the users path if they globally install the wrapper package (`npm install -g @sample/mcp-server`).

## Platform packages

To ensure that the appropriate binaries are distributed to each platform, the cross-platform wrapper package takes optional dependencies on platform specific packages: `@sample/mcp-server-win32-x64`, `@sample/mcp-server-darwin-arm64`, etc.

The platform packages contain an `index.js` as well as all of the .NET binaries for the platform.  The `index.js` in a platform package reads its own `package.json` to discover the file name for its executable, then it calls that executable with all of the passed in args.

The platform's executable is set as the package's `bin` entry with a platform specific key like `mcpsrv-linux-x64`.  This means that when you `npx` invoke a platform specific package, `npx` will directly call the platform binary. It also places the platform specific command in the users path if the platform package is globally installed (`npm install -g @sample/mcpsrv-linux-x64`).
