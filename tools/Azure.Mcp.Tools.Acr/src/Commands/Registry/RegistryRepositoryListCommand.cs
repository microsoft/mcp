// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Acr.Options.Registry;
using Azure.Mcp.Tools.Acr.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Acr.Commands.Registry;

[CommandMetadata(
    Id = "adc6eb20-ad98-4624-954d-61581f6fbca9",
    Name = "list",
    Title = "List Container Registry Repositories",
    Description = """
        List repositories in Azure Container Registries. By default, lists repositories for all registries in the subscription.
        You can narrow the scope using --resource-group and/or --registry to list repositories for a specific registry only.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RegistryRepositoryListCommand(ILogger<RegistryRepositoryListCommand> logger, IAcrService acrService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RegistryRepositoryListOptions, RegistryRepositoryListCommand.RegistryRepositoryListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<RegistryRepositoryListCommand> _logger = logger;
    private readonly IAcrService _acrService = acrService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RegistryRepositoryListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var map = await _acrService.ListRegistryRepositories(
                options.Subscription!,
                options.ResourceGroup,
                options.Registry,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(map ?? []), AcrJsonContext.Default.RegistryRepositoryListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing ACR repositories. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Registry: {Registry}",
                options.Subscription, options.ResourceGroup, options.Registry);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record RegistryRepositoryListCommandResult(Dictionary<string, List<string>> RepositoriesByRegistry);
}
