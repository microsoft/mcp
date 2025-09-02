// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;

public sealed class FileSystemGetSkuInfoCommand(ILogger<FileSystemGetSkuInfoCommand> logger)
    : BaseAzureManagedLustreCommand<FileSystemGetSkuInfoOptions>(logger)
{
    private const string CommandTitle = "Get AMLFS SKU information";

    public override string Name => "get-sku-info";

    public override string Description =>
        """
        Retrieves available Azure Managed Lustre in all the regions or in a specific regions. Use to discover capabilities, SKU names and zone support.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    private static readonly Option<string> _locationOption = AzureManagedLustreOptionDefinitions.LocationOption;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        _locationOption.IsRequired = false;
        command.AddOption(_locationOption);
    }

    protected override FileSystemGetSkuInfoOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueForOption(_locationOption);
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
                new FileSystemGetSkuInfoResult(skus),
                AzureManagedLustreJsonContext.Default.FileSystemGetSkuInfoResult) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AMLFS SKU info. Options: {@Options}", options);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record FileSystemGetSkuInfoResult(List<Models.AzureManagedLustreSkuInfo> Skus);
}
