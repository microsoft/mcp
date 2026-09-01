// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppService.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Settings;

public sealed class AppSettingsUpdateOptions : ISubscriptionOption
{
    [Option(Description = AppServiceOptionDefinitions.App)]
    public required string App { get; set; }

    [Option(Description = "The name of the application setting.")]
    public required string SettingName { get; set; }

    [Option(Description = "The value of the application setting. Required for add and set update types.")]
    public string? SettingValue { get; set; }

    [Option(Description = "The update operation to perform on the application setting.")]
    public required AppSettingUpdateType SettingUpdateType { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

}
