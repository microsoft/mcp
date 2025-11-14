# Recorded Testing in `microsoft/mcp`

## Context

This repository ships CLI tools. Specifically, multiple combinations of `tools` assembled into `mcp servers` that are effectively standalone CLI tools themselves. Developers in this repo write test classes that inherit from [`CommandsTestBase`](https://github.com/microsoft/mcp/blob/main/core/Azure.Mcp.Core/tests/Azure.Mcp.Tests/Client/CommandTestsBase.cs) to exercise this functionality against live azure resources.

## How does this repo record tests?

This repository relies upon a record/playback solution called [test-proxy]() that was created for the rest of the `azure-sdk-for-X` language repos.

- Azure/azure-sdk-for-js
- Azure/azure-sdk-for-net
- Azure/azure-sdk-for-python

To name a few. This server is a fully crossplatform, language-agnostic record/playback reverse-proxy that also offers customization for secret-sanitization, assets storage, and many other features necessary for this project.

Due to the methodology of `CommandsTestBase`, MCP code itself is running _separately_ from the actually running livetest project. To handle recording in a separate process, the `HttpClientService` from `Azure.Mcp.Core` was enhanced with `#IF DEBUG`-only code to redirect any request from a client created using `HttpClientService.CreateClient()`.

### On the mcp server side

During **only** `Debug` builds any mcp server produced from this repo and using `CreateClient()` as part of its client init will automagically redirect traffic to a reverse proxy target indicated by `TEST_PROXY_URL`.

### On the test side

Devs will reparent their currently-parented-to-`CommandTestsBase` to `RecordedCommandTestsBase`. This class will handle starting

## Converting a `LiveTest` to a `Recorded` test

## The sanitization/playback loop

The general process for a conversion is as follows.

1. Ensure that

- If a `.livesettings` file exists at root of your `tool`, continue.
- Update the `TestMode` setting within the `.livesettings.json` alongside each livetests csproj to `Record`.
  - Invoke the livetests.
- Use `.proxy/Azure.Sdk.Tools.TestProxy(.exe) config locate -a path/to/assets.json` to locate the recordings directory
  - Review recordings for secrets.
- Temporarily rename the `livesettings.json` to a different name to force `playback` mode. I use `DISABLED.livesettings.json`.
- Invoke the livetests again. If they all pass, and secret review was clean, push.
    - `.proxy/Azure.Sdk.Tools.TestPRoxy(.exe) push -a path/to/assets.json`
    - If they _do not_ pass, examine each individual test error. There should be some sort of mismatch or the like in the test-proxy error.
- Commit the `assets.json` updated with the new tag.

