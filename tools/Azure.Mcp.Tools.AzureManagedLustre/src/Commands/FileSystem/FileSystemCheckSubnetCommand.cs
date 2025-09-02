// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;

public sealed class FileSystemCheckSubnetCommand(ILogger<FileSystemCheckSubnetCommand> logger)
    : BaseAzureManagedLustreCommand<FileSystemCheckSubnetOptions>(logger)
{
    private const string CommandTitle = "Validate AMLFS subnet against SKU and size";

    public override string Name => "check-subnet-size";

    public override string Description =>
        "Validates that the provided subnet can host an Azure Managed Lustre filesystem for the given SKU and size.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    private static readonly string[] AllowedSkus = [
        "AMLFS-Durable-Premium-40",
        "AMLFS-Durable-Premium-125",
        "AMLFS-Durable-Premium-250",
        "AMLFS-Durable-Premium-500"
    ];

    private readonly Option<string> _skuOption = AzureManagedLustreOptionDefinitions.SkuOption;
    private readonly Option<int> _sizeOption = AzureManagedLustreOptionDefinitions.SizeOption;
    private readonly Option<string> _subnetIdOption = AzureManagedLustreOptionDefinitions.SubnetIdOption;
    private readonly Option<string> _locationOption = AzureManagedLustreOptionDefinitions.LocationOption;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_skuOption);
        command.AddOption(_sizeOption);
        command.AddOption(_subnetIdOption);
        command.AddOption(_locationOption);
    }

    protected override FileSystemCheckSubnetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Sku = parseResult.GetValueForOption(_skuOption);
        options.Size = parseResult.GetValueForOption(_sizeOption);
        options.SubnetId = parseResult.GetValueForOption(_subnetIdOption);
        options.Location = parseResult.GetValueForOption(_locationOption);
        return options;
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = base.Validate(commandResult, commandResponse);
        if (!result.IsValid)
            return result;

        var sku = commandResult.GetValueForOption(_skuOption);
        if (!string.IsNullOrWhiteSpace(sku) && !AllowedSkus.Contains(sku))
        {
            result.IsValid = false;
            result.ErrorMessage = $"Invalid SKU '{sku}'. Allowed values: {string.Join(", ", AllowedSkus)}";
            if (commandResponse != null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = result.ErrorMessage;
            }
        }

        return result;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
                return context.Response;

            var svc = context.GetService<IAzureManagedLustreService>();
            await svc.CheckAmlFSSubnetAsync(
                options.Subscription!,
                options.Sku!,
                options.Size,
                options.SubnetId!,
                options.Location,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new FileSystemCheckSubnetResult(true), AzureManagedLustreJsonContext.Default.FileSystemCheckSubnetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating AMLFS subnet. Options: {@Options}", options);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record FileSystemCheckSubnetResult(bool Valid);
}
