// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.CommonPrompts;

public class CommonPromptsListOptions : BaseSreAgentOptions
{
    public string? Search { get; set; }
}

public class CommonPromptsGetOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
}

public class CommonPromptsCreateOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public string? Content { get; set; }
}

public class CommonPromptsDeleteOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public bool Confirm { get; set; }
}
