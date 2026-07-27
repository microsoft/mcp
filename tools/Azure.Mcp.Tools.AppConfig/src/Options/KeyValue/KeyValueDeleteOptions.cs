// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppConfig.Options.KeyValue;

public sealed class KeyValueDeleteOptions : ISubscriptionOption
{
    [Option(Description = AppConfigOptionDescriptions.Account)]
    public required string Account { get; set; }

    [Option(Description = AppConfigOptionDescriptions.Key)]
    public required string Key { get; set; }

    [Option(Description = AppConfigOptionDescriptions.Label)]
    public string? Label { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
