// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppConfig.Options.KeyValue;

public class KeyValueSetOptions : ISubscriptionOption
{
    [Option(Description = AppConfigOptionDescriptions.Account)]
    public required string Account { get; set; }

    [Option(Description = AppConfigOptionDescriptions.Key)]
    public required string Key { get; set; }

    [Option(Description = AppConfigOptionDescriptions.Label)]
    public string? Label { get; set; }

    [Option(Description = "The content type of the configuration value. This is used to indicate how the value should be interpreted or parsed.")]
    public string? ContentType { get; set; }

    [Option(Description = "The value to set for the configuration key.")]
    public required string Value { get; set; }

    [Option(Description = "The tags to associate with the configuration key. Tags should be in the format 'key=value'. Multiple tags can be specified.")]
    public string[]? Tags { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
