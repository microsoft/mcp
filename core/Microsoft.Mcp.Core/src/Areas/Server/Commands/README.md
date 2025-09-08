<!-- Copyright (c) Microsoft Corporation.
<!-- Licensed under the MIT License. -->

# Starting the Server in different modes
There are two ways to start the server in `STDIO` or `SSE` (Streaming HTTP) modes.

## STDIO

`dotnet build && npx @modelcontextprotocol/inspector ./bin/Debug/net9.0/azmcp.exe server start`

## SSE
`dotnet build && ./bin/Debug/net9.0/azmcp.exe server start --transport sse`

`npx @modelcontextprotocol/inspector`

The SSE URI would be `http://localhost:5008`

Then attach to the azmcp process in debugger.  The default port for SSE is 5008.

## To set timeout in mcp inspector

http://localhost:5173/?timeout=2000000000#resources
