// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Acr.Options.Registry;
using Azure.Mcp.Tools.Acr.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Acr.Commands.Registry;

public sealed class RegistryListCommand(ILogger<RegistryListCommand> logger) : BaseAcrCommand<RegistryListOptions>
{
    private const string CommandTitle = "List Container Registries";
    private readonly ILogger<RegistryListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        $"""
        List Azure Container Registries in a subscription. Optionally filter by resource group. Each registry result
        includes: name, location, loginServer, skuName, skuTier. If no registries are found the tool returns null results
        (consistent with other list commands).
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
            var acrService = context.GetService<IAcrService>();
            var registries = await acrService.ListRegistries(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(registries ?? []), AcrJsonContext.Default.RegistryListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing container registries. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record RegistryListCommandResult(List<Models.AcrRegistryInfo> Registries);
}
