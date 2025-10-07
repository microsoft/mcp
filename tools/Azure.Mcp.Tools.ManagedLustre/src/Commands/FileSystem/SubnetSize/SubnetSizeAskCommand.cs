// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;

public sealed class SubnetSizeAskCommand(ILogger<SubnetSizeAskCommand> logger)
    : BaseManagedLustreCommand<SubnetSizeAskOptions>(logger)
{
    private const string CommandTitle = "Calculate AMLFS Subnet Size required number of IP Addresses";

    public override string Name => "ask";

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
        command.Options.Add(ManagedLustreOptionDefinitions.SkuOption);
        command.Options.Add(ManagedLustreOptionDefinitions.SizeOption);
        command.Validators.Add(commandResult =>
        {
            if (commandResult.TryGetValue(ManagedLustreOptionDefinitions.SkuOption, out var skuName)
                && !string.IsNullOrWhiteSpace(skuName)
                && !AllowedSkus.Contains(skuName))
            {
                commandResult.AddError($"Invalid SKU '{skuName}'. Allowed values: {string.Join(", ", AllowedSkus)}");
            }
        });
    }

    protected override SubnetSizeAskOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Sku = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.SkuOption.Name);
        options.Size = parseResult.GetValueOrDefault<int>(ManagedLustreOptionDefinitions.SizeOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;

        var options = BindOptions(parseResult);

        try
        {
            var svc = context.GetService<IManagedLustreService>();
            var result = await svc.GetRequiredAmlFSSubnetsSize(
                options.Subscription!,
                options.Sku!, options.Size,
                options.Tenant,
                options.RetryPolicy
                );
            context.Response.Results = ResponseResult.Create(new(result), ManagedLustreJsonContext.Default.FileSystemSubnetSizeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating AMLFS subnet size. Options: {@Options}", options);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record FileSystemSubnetSizeResult(int NumberOfRequiredIPs);
}
