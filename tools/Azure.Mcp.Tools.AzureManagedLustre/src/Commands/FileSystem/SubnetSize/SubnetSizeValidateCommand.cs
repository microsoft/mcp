// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;

public sealed class SubnetSizeValidateCommand(ILogger<SubnetSizeValidateCommand> logger)
    : BaseAzureManagedLustreCommand<SubnetSizeValidateOptions>(logger)
{
    private const string CommandTitle = "Validate AMLFS subnet against SKU and size";

    public override string Name => "validate";

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
        command.Options.Add(_skuOption);
        command.Options.Add(_sizeOption);
        command.Options.Add(_subnetIdOption);
        command.Options.Add(_locationOption);
    }

    protected override SubnetSizeValidateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Sku = parseResult.GetValue(_skuOption);
        options.Size = parseResult.GetValue(_sizeOption);
        options.SubnetId = parseResult.GetValue(_subnetIdOption);
        options.Location = parseResult.GetValue(_locationOption);
        return options;
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = base.Validate(commandResult, commandResponse);
        if (!result.IsValid)
            return result;

        var sku = commandResult.GetValue(_skuOption);
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
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
                return context.Response;

            var options = BindOptions(parseResult);
            var svc = context.GetService<IAzureManagedLustreService>();
            var subnetIsValid = await svc.CheckAmlFSSubnetAsync(
                                options.Subscription!,
                                options.Sku!,
                                options.Size,
                                options.SubnetId!,
                                options.Location!,
                                options.Tenant,
                                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new FileSystemCheckSubnetResult(subnetIsValid), AzureManagedLustreJsonContext.Default.FileSystemCheckSubnetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating AMLFS subnet.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record FileSystemCheckSubnetResult(bool Valid);
}
