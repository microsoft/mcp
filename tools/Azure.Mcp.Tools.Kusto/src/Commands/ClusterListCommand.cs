// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Kusto.Commands;

[CommandMetadata(
    Id = "2cff1548-40c9-48ea-8548-6bfa91f2ea85",
    Name = "list",
    Title = "List Kusto Clusters",
    Description = "List/enumerate all Azure Data Explorer/Kusto/KQL clusters in a subscription.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ClusterListCommand(ILogger<ClusterListCommand> logger, IKustoService kustoService) : SubscriptionCommand<ClusterListOptions>()
{
    private readonly ILogger<ClusterListCommand> _logger = logger;
    private readonly IKustoService _kustoService = kustoService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override ClusterListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var clusterNames = await _kustoService.ListClustersAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(clusterNames?.Results ?? [], clusterNames?.AreResultsTruncated ?? false), KustoJsonContext.Default.ClusterListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing Kusto clusters. Subscription: {Subscription}.", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ClusterListCommandResult(List<string> Clusters, bool AreResultsTruncated);
}
