// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppConfig.Options.KeyValue.Lock;

public class KeyValueLockSetOptions : ISubscriptionOption
{
    [Option(Description = "Whether a key-value will be locked (set to read-only) or unlocked (read-only removed).")]
    public bool? Lock { get; set; }

    [Option(Description = "The name of the App Configuration store (e.g., my-appconfig).")]
    public required string Account { get; set; }

    [Option(Description = "The name of the key to access within the App Configuration store.")]
    public required string Key { get; set; }

    [Option(Description = "The label to apply to the configuration key. Labels are used to group and organize settings.")]
    public string? Label { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
