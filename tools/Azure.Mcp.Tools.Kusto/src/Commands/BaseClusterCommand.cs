// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Kusto.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Option;

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
            var clusterUri = commandResult.GetValueOrDefault<string>(KustoOptionDefinitions.ClusterUri.Name);
            if (!string.IsNullOrEmpty(clusterUri))
            {
                // If clusterUri is provided, subscription becomes optional
                return;
            }

            var clusterName = commandResult.GetValueOrDefault<string>(KustoOptionDefinitions.Cluster.Name);

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
