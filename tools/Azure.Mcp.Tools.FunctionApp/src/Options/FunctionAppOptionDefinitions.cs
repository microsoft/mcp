// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FunctionApp.Options;

public static class FunctionAppOptionDefinitions
{
    public const string FunctionAppName = "function-app";

    public static readonly Option<string> FunctionApp = new(
        $"--{FunctionAppName}"
    )
    {
        Description = "The name of the Function App.",
        Required = true
    };
}
