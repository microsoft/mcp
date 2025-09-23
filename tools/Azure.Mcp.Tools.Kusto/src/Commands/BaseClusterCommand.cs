// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Kusto.Options;

namespace Azure.Mcp.Tools.Kusto.Commands;

public abstract class BaseClusterCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseClusterOptions, new()
{
    protected static bool UseClusterUri(BaseClusterOptions options) => !string.IsNullOrEmpty(options.ClusterUri);

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KustoOptionDefinitions.ClusterUri);
        command.Options.Add(KustoOptionDefinitions.Cluster);
    }

    public override ValidationResult Validate(CommandResult parseResult, CommandResponse? commandResponse = null)
    {
        var validationResult = new ValidationResult { IsValid = true };

        parseResult.TryGetValue(KustoOptionDefinitions.ClusterUri, out string? clusterUri);
        parseResult.TryGetValue(KustoOptionDefinitions.Cluster, out string? clusterName);
        parseResult.TryGetValue(OptionDefinitions.Common.Subscription, out string? subscription);

        if (!string.IsNullOrEmpty(clusterUri))
        {
            // If clusterUri is provided, subscription becomes optional
            return validationResult;
        }
        else
        {
            // clusterUri not provided, require both subscription and clusterName
            if (string.IsNullOrEmpty(subscription) || string.IsNullOrEmpty(clusterName))
            {
                validationResult.IsValid = false;
                validationResult.ErrorMessage = $"Either {KustoOptionDefinitions.ClusterUri.Name} must be provided, or both {OptionDefinitions.Common.Subscription.Name} and {KustoOptionDefinitions.Cluster.Name} must be provided.";

                if (commandResponse != null)
                {
                    commandResponse.Status = HttpStatusCode.BadRequest;
                    commandResponse.Message = validationResult.ErrorMessage;
                }
            }
        }

        if (validationResult.IsValid)
            return base.Validate(parseResult, commandResponse);

        return validationResult;
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ClusterUri = parseResult.GetValueOrDefault<string>(KustoOptionDefinitions.ClusterUri.Name);
        options.ClusterName = parseResult.GetValueOrDefault<string>(KustoOptionDefinitions.Cluster.Name);

        return options;
    }
}
