// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Pool;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Pool;

public sealed class PoolUpdateCommand(ILogger<PoolUpdateCommand> logger) : SubscriptionCommand<PoolUpdateOptions>()
{
    private const string CommandTitle = "Update NetApp Files Capacity Pool";

    private readonly ILogger<PoolUpdateCommand> _logger = logger;

    public override string Id => "d5a9b3e7-6c4f-4d8a-b2e1-f7c8a0d3e5b9";

    public override string Name => "update";

    public override string Description =>
        """
        Updates an existing Azure NetApp Files capacity pool in a specified account and resource group, and returns the updated pool details including name, location, resource group, provisioning state, service level, size, QoS type, cool access, and encryption type. Supports updating size, QoS type, cool access, and tags. Requires account name, pool name, resource group, location, and subscription.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Pool.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.Size.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.QosType);
        command.Options.Add(NetAppFilesOptionDefinitions.CoolAccess);
        command.Options.Add(NetAppFilesOptionDefinitions.Tags.AsOptional());
    }

    protected override PoolUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        var size = parseResult.GetValueOrDefault<long>(NetAppFilesOptionDefinitions.Size.Name);
        options.Size = size != 0 ? size : null;
        options.QosType = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.QosType.Name);
        options.CoolAccess = parseResult.GetValueOrDefault<bool?>(NetAppFilesOptionDefinitions.CoolAccess.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Tags.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var netAppFilesService = context.GetService<INetAppFilesService>();

            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid tags JSON format: {ex.Message}", nameof(options.Tags));
                }
            }

            var pool = await netAppFilesService.UpdatePool(
                options.Account!,
                options.Pool!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.Size,
                options.QosType,
                options.CoolAccess,
                tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new PoolUpdateCommandResult(pool),
                NetAppFilesJsonContext.Default.PoolUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating NetApp Files capacity pool. Pool: {Pool}, Account: {Account}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Pool, options.Account, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A capacity pool with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the capacity pool. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Capacity pool, account, or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record PoolUpdateCommandResult([property: JsonPropertyName("pool")] CapacityPoolCreateResult Pool);
}
