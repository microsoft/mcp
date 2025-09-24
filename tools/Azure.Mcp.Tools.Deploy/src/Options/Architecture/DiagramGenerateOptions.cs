// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Deploy.Options.Architecture;

public class DiagramGenerateOptions : GlobalOptions
{
    [JsonPropertyName(CommandFactoryToolLoader.RawMcpToolInputOptionName)]
    public string? RawMcpToolInput { get; set; }
}
