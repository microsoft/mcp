// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.Logging;

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

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    private static readonly string[] AllowedSkus = [
        "AMLFS-Durable-Premium-40",
        "AMLFS-Durable-Premium-125",
        "AMLFS-Durable-Premium-250",
        "AMLFS-Durable-Premium-500"
    ];

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureManagedLustreOptionDefinitions.SkuOption);
        command.Options.Add(AzureManagedLustreOptionDefinitions.SizeOption);
    }

    protected override FileSystemSubnetSizeOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Sku = parseResult.GetValueOrDefault<string>(AzureManagedLustreOptionDefinitions.SkuOption.Name);
        options.Size = parseResult.GetValueOrDefault<int>(AzureManagedLustreOptionDefinitions.SizeOption.Name);
        return options;
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = base.Validate(commandResult, commandResponse);

        if (result.IsValid)
        {
            if (commandResult.TryGetValue(AzureManagedLustreOptionDefinitions.SkuOption, out var skuName)
                && !string.IsNullOrWhiteSpace(skuName)
                && !AllowedSkus.Contains(skuName))
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
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;

        var options = BindOptions(parseResult);

        try
        {
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
