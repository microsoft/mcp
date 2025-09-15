// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;

public sealed class SkuGetCommand(ILogger<SkuGetCommand> logger)
    : BaseAzureManagedLustreCommand<SkuGetOptions>(logger)
{
    private const string CommandTitle = "Get AMLFS SKU information";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves the available Azure Managed Lustre SKU, including increments, bandwidth, scale targets and zonal support. If a location is specified, the results will be filtered to that location.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    private static readonly Option<string> _optionalLocationOption = AzureManagedLustreOptionDefinitions.OptionalLocationOption;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_optionalLocationOption);
    }

    protected override SkuGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValue(_optionalLocationOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
                return context.Response;

            var options = BindOptions(parseResult);
            var service = context.GetService<IAzureManagedLustreService>();
            var skus = await service.SkuGetInfoAsync(options.Subscription!, options.Tenant, options.Location, options.RetryPolicy);

            context.Response.Results = skus.Count > 0 ? ResponseResult.Create(
                new SkuGetResult(skus),
                AzureManagedLustreJsonContext.Default.SkuGetResult) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AMLFS SKU info.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record SkuGetResult(List<Models.AzureManagedLustreSkuInfo> Skus);
}
