## Wrapper package

The azmcp cli is invocable through the same `npx -yes @azure/mcp` on Windows, Mac and Linux, x64 and arm. This requires building different platform and cpu architecture specific executables, with each compiling to > 70MB.

The `@azure/mcp` package contains just `index.js`. `index.js` is responsible for detecting the platform and cpu it's running on, loading the platform specific package, then passing all of its cli args to the platform package's `index.js` file.

The `index.js` file is set as the package's `bin` entry with the key `azmcp`.  This allows it to be the entrypoint for `npx` calls as well as placing the command `azmcp` in the users path if they globally install `@azure/mcp`.

## Platform packages

To ensure that the appropriate binaries are distributed to each platform, we use a cross-platform wrapper package, `@azure/mcp`, that takes optional dependencies on 5 platform specific packages: `@azure/mcp-win32-x64`, `@azure/mcp-darwin-arm64`, etc.

The platform packages contain an `index.js` as well as all of the .NET binaries for the platform.  The `index.js` in a platform package reads its own `package.json` to discover the file name for its executable, then it calls that executable with all of the passed in args.

The platform's executable is set as the package's `bin` entry with a platform specific key like `azmcp-linux-x64`.  This means that when you `npx` invoke a platform specific package, `npx` will directly call the platform binary. It also places the platform specific command in the users path if the platform package is globally installed.
