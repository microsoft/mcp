// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;

public sealed class SubnetSizeValidateCommand(ILogger<SubnetSizeValidateCommand> logger)
    : BaseManagedLustreCommand<SubnetSizeValidateOptions>(logger)
{
    private const string CommandTitle = "Validate AMLFS subnet against SKU and size";

    public override string Name => "validate";

    public override string Description =>
        "Validates that the provided subnet can host an Azure Managed Lustre filesystem for the given SKU and size.";

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
        command.Options.Add(ManagedLustreOptionDefinitions.SubnetIdOption);
        command.Options.Add(ManagedLustreOptionDefinitions.LocationOption);
        command.Validators.Add(commandResult =>
            {
                var sku = commandResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.SkuOption);
                if (!string.IsNullOrWhiteSpace(sku) && !AllowedSkus.Contains(sku))
                {
                    commandResult.AddError($"Invalid SKU '{sku}'. Allowed values: {string.Join(", ", AllowedSkus)}");
                }
            }
        );
    }

    protected override SubnetSizeValidateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Sku = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.SkuOption.Name);
        options.Size = parseResult.GetValueOrDefault<int>(ManagedLustreOptionDefinitions.SizeOption.Name);
        options.SubnetId = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.SubnetIdOption.Name);
        options.Location = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.LocationOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
                return context.Response;

            var options = BindOptions(parseResult);
            var svc = context.GetService<IManagedLustreService>();
            var subnetIsValid = await svc.CheckAmlFSSubnetAsync(
                                options.Subscription!,
                                options.Sku!,
                                options.Size,
                                options.SubnetId!,
                                options.Location!,
                                options.Tenant,
                                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new FileSystemCheckSubnetResult(subnetIsValid), ManagedLustreJsonContext.Default.FileSystemCheckSubnetResult);
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
