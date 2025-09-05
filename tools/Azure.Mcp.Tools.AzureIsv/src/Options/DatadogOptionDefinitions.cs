// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureIsv.Options;

public static class DatadogOptionDefinitions
{
    public const string DatadogResourceParam = "datadog-resource";

    public static readonly Option<string> DatadogResourceName = new(
        $"--{DatadogResourceParam}"
    )
    {
        Description = "The name of the Datadog resource to use. This is the unique name you chose for your Datadog resource in Azure.",
        Required = true
    };
}
