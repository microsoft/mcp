// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FunctionApp.Options.FunctionApp;

public sealed class FunctionAppCreateContainerAppOptions : ISubscriptionOption
{
    [Option(Description = FunctionAppOptionDescriptions.FunctionApp)]
    public required string FunctionApp { get; set; }

    [Option(Description = FunctionAppOptionDescriptions.Location)]
    public required string Location { get; set; }

    [Option(Description = FunctionAppOptionDescriptions.Runtime)]
    public string? Runtime { get; set; }

    [Option(Description = FunctionAppOptionDescriptions.RuntimeVersion)]
    public string? RuntimeVersion { get; set; }

    [Option(Description = FunctionAppOptionDescriptions.StorageAccount)]
    public string? StorageAccount { get; set; }

    [Option(Description = FunctionAppOptionDescriptions.StorageAuthMode)]
    public string? StorageAuthMode { get; set; }

    [Option(Description = "The name of the Container Apps managed environment that hosts the Function App. It is created in the resource group if it does not exist; if omitted, an environment named '<function-app>-env' is created.")]
    public string? ContainerAppsEnvironment { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
