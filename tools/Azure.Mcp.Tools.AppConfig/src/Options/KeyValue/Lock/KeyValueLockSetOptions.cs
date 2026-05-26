// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppConfig.Options.KeyValue.Lock;

public class KeyValueLockSetOptions : ISubscriptionOption
{
    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option("Whether a key-value will be locked (set to read-only) or unlocked (read-only removed).")]
    public bool? Lock { get; set; }

    [Option("The name of the App Configuration store (e.g., my-appconfig).")]
    public required string Account { get; set; }

    [Option("The name of the key to access within the App Configuration store.")]
    public required string Key { get; set; }

    [Option("The label to apply to the configuration key. Labels are used to group and organize settings.")]
    public string? Label { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
