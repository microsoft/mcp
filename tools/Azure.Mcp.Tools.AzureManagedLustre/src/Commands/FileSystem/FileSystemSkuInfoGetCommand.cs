// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;

public sealed class FileSystemSkuInfoGetCommand(ILogger<FileSystemSkuInfoGetCommand> logger)
    : BaseAzureManagedLustreCommand<FileSystemSkuInfoGetOptions>(logger)
{
    private const string CommandTitle = "Get AMLFS SKU information";

    public override string Name => "sku-info-get";

    public override string Description =>
        """
        Retrieves the available Azure Managed Lustre SKU, including increments, bandwidth, scale targets and zonal support. If a location is specified, the results will be filtered to that location.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    private static readonly Option<string> _optionalLocationOption = AzureManagedLustreOptionDefinitions.OptionalLocationOption;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        _optionalLocationOption.IsRequired = false;
        command.AddOption(_optionalLocationOption);
    }

    protected override FileSystemSkuInfoGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueForOption(_optionalLocationOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
                return context.Response;

            var service = context.GetService<IAzureManagedLustreService>();
            var skus = await service.GetSkuInfoAsync(options.Subscription!, options.Tenant, options.Location, options.RetryPolicy);

            context.Response.Results = skus.Count > 0 ? ResponseResult.Create(
                new FileSystemSkuInfoGetResult(skus),
                AzureManagedLustreJsonContext.Default.FileSystemSkuInfoGetResult) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AMLFS SKU info. Options: {@Options}", options);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record FileSystemSkuInfoGetResult(List<Models.AzureManagedLustreSkuInfo> Skus);
}
