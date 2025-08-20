// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
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
        command.AddOption(_clusterUriOption);
        command.AddOption(_clusterNameOption);

        command.AddValidator(result =>
        {
            var validationResult = Validate(result);
            if (!validationResult.IsValid)
            {
                result.ErrorMessage = validationResult.ErrorMessage;
                return;
            }
        });
    }

    public override ValidationResult Validate(CommandResult parseResult, CommandResponse? commandResponse = null)
    {
        var validationResult = new ValidationResult { IsValid = true };
        var clusterUri = parseResult.GetValueForOption(_clusterUriOption);
        var clusterName = parseResult.GetValueForOption(_clusterNameOption);
        if (!string.IsNullOrEmpty(clusterUri))
        {
            // If clusterUri is provided, subscription becomes optional
            return validationResult;
        }
        else
        {
            var subscription = parseResult.GetValueForOption(_subscriptionOption);

            // clusterUri not provided, require both subscription and clusterName
            if (string.IsNullOrEmpty(subscription) || string.IsNullOrEmpty(clusterName))
            {
                validationResult.IsValid = false;
                validationResult.ErrorMessage = $"Either --{_clusterUriOption.Name} must be provided, or both --{_subscriptionOption.Name} and --{_clusterNameOption.Name} must be provided.";

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
        options.ClusterUri = parseResult.GetValueForOption(_clusterUriOption);
        options.ClusterName = parseResult.GetValueForOption(_clusterNameOption);

        return options;
    }
}
