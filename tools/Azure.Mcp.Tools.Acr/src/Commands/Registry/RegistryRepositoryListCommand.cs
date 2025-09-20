// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Acr.Options;
using Azure.Mcp.Tools.Acr.Options.Registry;
using Azure.Mcp.Tools.Acr.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Acr.Commands.Registry;

public sealed class RegistryRepositoryListCommand(ILogger<RegistryRepositoryListCommand> logger)
    : BaseAcrCommand<RegistryRepositoryListOptions>
{
    private const string CommandTitle = "List Container Registry Repositories";
    private readonly ILogger<RegistryRepositoryListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List repositories in Azure Container Registries. By default, lists repositories for all registries in the subscription.
        You can narrow the scope using --resource-group and/or --registry to list repositories for a specific registry only.
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
        command.Options.Add(AcrOptionDefinitions.Registry);
    }

    protected override RegistryRepositoryListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Registry ??= parseResult.GetValueOrDefault<string>(AcrOptionDefinitions.Registry.Name);
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
            var service = context.GetService<IAcrService>();
            var map = await service.ListRegistryRepositories(
                options.Subscription!,
                options.ResourceGroup,
                options.Registry,
                options.Tenant,
                options.RetryPolicy);

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

    internal record RegistryRepositoryListCommandResult(Dictionary<string, List<string>> RepositoriesByRegistry);
}
