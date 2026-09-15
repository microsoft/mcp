// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FunctionApp.Options.FunctionApp;

public sealed class FunctionAppCreateOptions : ISubscriptionOption
{
    [Option(Description = FunctionAppOptionDescriptions.FunctionApp)]
    public required string FunctionApp { get; set; }

    [Option(Description = FunctionAppOptionDescriptions.Location)]
    public required string Location { get; set; }

    [Option(Description = "The name of an existing App Service plan to host the Function App. If omitted, a plan named '<function-app>-plan' is created using --plan-type and --plan-sku.")]
    public string? AppServicePlan { get; set; }

    [Option(Description = "The hosting plan type to create when --app-service-plan is omitted. Valid values: consumption, flex, premium, appservice. Defaults to consumption.")]
    public string? PlanType { get; set; }

    [Option(Description = "The explicit App Service plan SKU to create (e.g., B1, S1, P1v3, EP1, FC1). Takes precedence over the SKU implied by --plan-type.")]
    public string? PlanSku { get; set; }

    [Option(Description = FunctionAppOptionDescriptions.Runtime)]
    public string? Runtime { get; set; }

    [Option(Description = FunctionAppOptionDescriptions.RuntimeVersion)]
    public string? RuntimeVersion { get; set; }

    [Option(Name = "os", Description = "The target operating system. Valid values: windows, linux. Defaults to windows unless the runtime or plan requires Linux (python, flex consumption).")]
    public string? OperatingSystem { get; set; }

    [Option(Description = FunctionAppOptionDescriptions.StorageAccount)]
    public string? StorageAccount { get; set; }

    [Option(Description = FunctionAppOptionDescriptions.StorageAuthMode)]
    public string? StorageAuthMode { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
