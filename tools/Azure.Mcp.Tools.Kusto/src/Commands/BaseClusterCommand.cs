// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Kusto.Options;

namespace Azure.Mcp.Tools.Kusto.Commands;

public abstract class BaseClusterCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseClusterOptions, new()
{
    protected readonly Option<string> _clusterNameOption = KustoOptionDefinitions.Cluster;
    protected readonly Option<string> _clusterUriOption = KustoOptionDefinitions.ClusterUri;

    protected static bool UseClusterUri(BaseClusterOptions options) => !string.IsNullOrEmpty(options.ClusterUri);

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_clusterUriOption);
        command.Options.Add(_clusterNameOption);
    }

    public override ValidationResult Validate(CommandResult parseResult, CommandResponse? commandResponse = null)
    {
        var validationResult = new ValidationResult { IsValid = true };

        parseResult.TryGetValue(_clusterUriOption, out string? clusterUri);
        parseResult.TryGetValue(_clusterNameOption, out string? clusterName);
        parseResult.TryGetValue(_subscriptionOption, out string? subscription);

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
                validationResult.ErrorMessage = $"Either {_clusterUriOption.Name} must be provided, or both {_subscriptionOption.Name} and {_clusterNameOption.Name} must be provided.";

                if (commandResponse != null)
                {
                    commandResponse.Status = 400;
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
        options.ClusterUri = parseResult.GetValueOrDefault(_clusterUriOption);
        options.ClusterName = parseResult.GetValueOrDefault(_clusterNameOption);

        return options;
    }
}
