// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Kusto.Commands;

public sealed class ClusterGetCommand(ILogger<ClusterGetCommand> logger) : BaseClusterCommand<ClusterGetOptions>
{
    private const string CommandTitle = "Get Kusto Cluster Details";
    private readonly ILogger<ClusterGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Get details for a specific Kusto cluster. Requires `subscription` and `cluster`.
        The response includes the `clusterUri` property for use in subsequent commands.
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var kusto = context.GetService<IKustoService>();
            var cluster = await kusto.GetCluster(
                options.Subscription!,
                options.ClusterName!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = cluster is null ?
                null : ResponseResult.Create(new(cluster), KustoJsonContext.Default.ClusterGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred getting Kusto cluster details. Cluster: {Cluster}.", options.ClusterName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ClusterGetCommandResult(KustoClusterResourceProxy Cluster);
}
