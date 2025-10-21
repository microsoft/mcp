// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Fabric.Mcp.Tools.OneLake.Options;

public static class OneLakeOptionDefinitions
{
    public const string FormatName = "format";
    public static readonly Option<string> Format = new($"--{FormatName}")
    {
        Description = "Output format for OneLake API responses. Use 'json' for parsed workspace objects or 'xml' for raw API response. Supported values: 'json' (default), 'xml'."
    };
}