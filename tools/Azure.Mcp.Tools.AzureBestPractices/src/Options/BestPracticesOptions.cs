// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBestPractices.Options;

public sealed class BestPracticesOptions
{
    [Option(Description = "The Azure resource type for which to get best practices. Options: 'general' (general Azure), 'azurefunctions' (Azure Functions), 'static-web-app' (Azure Static Web Apps), 'coding-agent' (Coding Agent).")]
    public required string Resource { get; set; }

    [Option(Description = "The action type for the best practices. Options: 'all', 'code-generation', 'deployment'. Note: 'static-web-app' and 'coding-agent' resources only supports 'all'.")]
    public required string Action { get; set; }
}
