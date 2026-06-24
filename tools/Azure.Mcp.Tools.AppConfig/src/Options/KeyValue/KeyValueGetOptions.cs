// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppConfig.Options.KeyValue;

public class KeyValueGetOptions : ISubscriptionOption
{
    [Option(Description = AppConfigOptionDescriptions.Account)]
    public required string Account { get; set; }

    [Option(Description = AppConfigOptionDescriptions.Key)]
    public string? Key { get; set; }

    [Option(Description = AppConfigOptionDescriptions.Label)]
    public string? Label { get; set; }

    [Option(Description = "Specifies the key filter, if any, to be used when retrieving key-values. The filter can be an exact match, for example a filter of 'foo' would get all key-values with a key of 'foo', or the filter can include a '*' character at the end of the string for wildcard searches (e.g., 'App*'). If omitted all keys will be retrieved.")]
    public string? KeyFilter { get; set; }

    [Option(Description = "Specifies the label filter, if any, to be used when retrieving key-values. The filter can be an exact match, for example a filter of 'foo' would get all key-values with a label of 'foo', or the filter can include a '*' character at the end of the string for wildcard searches (e.g., 'Prod*'). This filter is case-sensitive. If omitted, all labels will be retrieved.")]
    public string? LabelFilter { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
