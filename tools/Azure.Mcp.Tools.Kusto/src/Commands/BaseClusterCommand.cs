// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Kusto.Options;

namespace Azure.Mcp.Tools.Kusto.Commands;

public abstract class BaseClusterCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : GlobalCommand<TOptions> where TOptions : BaseClusterOptions, new()
{
    protected static bool UseClusterUri(BaseClusterOptions options) => !string.IsNullOrEmpty(options.ClusterUri);

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.Subscription);
        command.Options.Add(KustoOptionDefinitions.ClusterUri);
        command.Options.Add(KustoOptionDefinitions.Cluster);
        command.Validators.Add(commandResult =>
        {
            if (commandResult.TryGetValue(KustoOptionDefinitions.ClusterUri, out string? clusterUri) && !string.IsNullOrEmpty(clusterUri))
            {
                // If clusterUri is provided, subscription becomes optional
                return;
            }

            commandResult.TryGetValue(KustoOptionDefinitions.Cluster, out string? clusterName);

            // clusterUri not provided, require both subscription and clusterName
            if (string.IsNullOrEmpty(clusterName) || !CommandHelper.HasSubscriptionAvailable(commandResult))
            {
                commandResult.AddError($"Either {KustoOptionDefinitions.ClusterUri.Name} must be provided, or both {OptionDefinitions.Common.Subscription.Name} and {KustoOptionDefinitions.Cluster.Name} must be provided.");
            }
        });
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Subscription = CommandHelper.GetSubscription(parseResult);
        options.ClusterUri = parseResult.GetValueOrDefault<string>(KustoOptionDefinitions.ClusterUri.Name);
        options.ClusterName = parseResult.GetValueOrDefault<string>(KustoOptionDefinitions.Cluster.Name);

        return options;
    }
}
