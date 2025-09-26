// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Aks.Options;
using Azure.Mcp.Tools.Aks.Options.Cluster;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Aks.Commands.Cluster;

public sealed class ClusterGetCommand(ILogger<ClusterGetCommand> logger) : BaseAksCommand<ClusterGetOptions>
{
    private const string CommandTitle = "Get Azure Kubernetes Service (AKS) Cluster Details";
    private readonly ILogger<ClusterGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Get or list Azure Kubernetes Service (AKS) clusters. If a specific cluster name is provided, that cluster will
        be retrieved. Otherwise, all clusters will be listed in the specified subscription. Returns detailed cluster
        information including configuration, network settings, and status.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup);
        command.Options.Add(AksOptionDefinitions.Cluster);
        command.Validators.Add(commandResults =>
        {
            var clusterName = commandResults.GetValueOrDefault(AksOptionDefinitions.Cluster);
            if (!string.IsNullOrEmpty(clusterName))
            {
                var resourceGroup = commandResults.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);
                if (string.IsNullOrEmpty(resourceGroup))
                {
                    commandResults.AddError("When specifying a cluster name, the --resource-group option is required.");
                }
            }
        });
    }

    protected override ClusterGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ClusterName = parseResult.GetValueOrDefault<string>(AksOptionDefinitions.Cluster.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var aksService = context.GetService<IAksService>();
            var clusters = await aksService.GetClusters(
                options.Subscription!,
                options.ClusterName,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(clusters ?? []), AksJsonContext.Default.ClusterGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting AKS cluster. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ClusterName: {ClusterName}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.ClusterName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ClusterGetCommandResult(List<Models.Cluster> Clusters);
}
