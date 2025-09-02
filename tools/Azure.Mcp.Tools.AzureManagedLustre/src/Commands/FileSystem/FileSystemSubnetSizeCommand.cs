// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;

public sealed class FileSystemSubnetSizeCommand(ILogger<FileSystemSubnetSizeCommand> logger)
    : BaseAzureManagedLustreCommand<FileSystemSubnetSizeOptions>(logger)
{
    private const string CommandTitle = "Calculate AMLFS Subnet Size required number of IP Addresses";

    public override string Name => "required-subnet-size";

    public override string Description =>
        """
        Calculates the required subnet size for an Azure Managed Lustre file system given a SKU and size. Use to plan network deployment for AMLFS. Returns the number of required IPs.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    private static readonly string[] AllowedSkus = [
        "AMLFS-Durable-Premium-40",
        "AMLFS-Durable-Premium-125",
        "AMLFS-Durable-Premium-250",
        "AMLFS-Durable-Premium-500"
    ];

    private readonly Option<string> _skuOption = AzureManagedLustreOptionDefinitions.SkuOption;
    private static readonly Option<int> _sizeOption = AzureManagedLustreOptionDefinitions.SizeOption;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_skuOption);
        command.AddOption(_sizeOption);
    }

    protected override FileSystemSubnetSizeOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Sku = parseResult.GetValueForOption(_skuOption);
        options.Size = parseResult.GetValueForOption(_sizeOption);
        return options;
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = base.Validate(commandResult, commandResponse);

        if (result.IsValid)
        {
            string skuName = commandResult.GetValueForOption(_skuOption)!;


            if (!AllowedSkus.Contains(skuName))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Invalid SKU '{skuName}'. Allowed values: {string.Join(", ", AllowedSkus)}";

                if (commandResponse != null)
                {
                    commandResponse.Status = 400;
                    commandResponse.Message = result.ErrorMessage!;
                }
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
            var result = await svc.GetRequiredAmlFSSubnetsSize(
                options.Subscription!,
                options.Sku!, options.Size,
                options.Tenant,
                options.RetryPolicy
                );
            context.Response.Results = ResponseResult.Create(new FileSystemSubnetSizeResult(result), AzureManagedLustreJsonContext.Default.FileSystemSubnetSizeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating AMLFS subnet size. Options: {@Options}", options);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record FileSystemSubnetSizeResult(int numberOfRequiredIPs);
}
