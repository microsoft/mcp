// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppConfig.Options.KeyValue;

public sealed class KeyValueDeleteOptions : ISubscriptionOption
{
    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option(AppConfigOptionDescriptions.Account)]
    public required string Account { get; set; }

    [Option(AppConfigOptionDescriptions.Key)]
    public required string Key { get; set; }

    [Option(AppConfigOptionDescriptions.Label)]
    public string? Label { get; set; }
}
