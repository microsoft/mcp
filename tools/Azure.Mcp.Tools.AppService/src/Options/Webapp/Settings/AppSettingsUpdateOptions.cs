// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Settings;

public sealed class AppSettingsUpdateOptions : ISubscriptionOption
{
    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option(AppServiceOptionDefinitions.App)]
    public required string App { get; set; }

    [Option("The name of the application setting.")]
    public required string SettingName { get; set; }

    [Option("The value of the application setting. Required for add and set update types.")]
    public string? SettingValue { get; set; }

    [Option("The type of update to perform on the application setting. Valid values are: add, set, delete.")]
    public required string SettingUpdateType { get; set; }
}
