// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Deploy.Options.App;

public sealed class LogsGetOptions : ISubscriptionOption
{
    [Option(Description = "The full path of the workspace folder.")]
    public required string WorkspaceFolder { get; set; }

    [Option(Description = "The name of the environment created by azd (AZURE_ENV_NAME) during `azd init` or `azd up`. If not provided in context, try to find it in the .azure directory in the workspace or use 'azd env list'.")]
    public required string AzdEnvName { get; set; }

    [Option(Description = "The maximum row number of logs to retrieve. Use this to get a specific number of logs or to avoid the retrieved logs from reaching token limit. Default is 200.", DefaultValue = 200)]
    public int? Limit { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
