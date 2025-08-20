// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBestPractices.Options;

public static class BestPracticesOptionDefinitions
{
    public const string ResourceName = "resource";
    public const string ActionName = "action";

    public static readonly Option<string> Resource = new(
        $"--{ResourceName}",
        "The Azure resource type for which to get best practices. Options: 'general' (general Azure), 'azurefunctions' (Azure Functions), 'static-web-app' (Azure Static Web Apps)."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Action = new(
        $"--{ActionName}",
        "The action type for the best practices. Options: 'all', 'code-generation', 'deployment'. Note: 'static-web-app' resource only supports 'all'."
    )
    {
        IsRequired = true
    };
}
