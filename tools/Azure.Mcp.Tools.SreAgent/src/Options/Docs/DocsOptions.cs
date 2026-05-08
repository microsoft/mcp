// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Docs;

public class DocsGetOptions : GlobalOptions
{
    public string? Topic { get; set; }
}

public class MemoryRemoteOptions : BaseSreAgentOptions;

public class MemoriesSearchOptions : BaseSreAgentOptions
{
    public string? Query { get; set; }
}

public class MemoriesAddOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public string? Content { get; set; }
}

public class MemoriesDeleteOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public bool Confirm { get; set; }
}
