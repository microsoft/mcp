// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.DeviceRegistry.Models;
using Azure.Mcp.Tools.DeviceRegistry.Options.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;

[CommandMetadata(
    Id = "9c42f93b-2d4e-4fb3-b98b-2ef119b46c94",
    Name = "list",
    Title = "List Device Registry Namespaces",
    Description = """
        Lists Azure Device Registry namespaces in a subscription or resource group. Returns namespace details including
        name, location, provisioning state, and UUID. If a resource group is specified, only namespaces within that
        resource group are returned. Otherwise, all namespaces in the subscription are listed.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class NamespaceListCommand(ILogger<NamespaceListCommand> logger, IDeviceRegistryService deviceRegistryService)
    : BaseDeviceRegistryCommand<NamespaceListOptions>()
{
    private readonly ILogger<NamespaceListCommand> _logger = logger;
    private readonly IDeviceRegistryService _deviceRegistryService = deviceRegistryService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override NamespaceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var namespaces = await _deviceRegistryService.ListNamespacesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(namespaces?.Results ?? [], namespaces?.AreResultsTruncated ?? false),
                DeviceRegistryJsonContext.Default.NamespaceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error listing Device Registry namespaces. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}.",
                options.Subscription,
                options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource not found. Verify the subscription and resource group exist and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record NamespaceListCommandResult(
        List<DeviceRegistryNamespaceInfo> Namespaces,
        bool AreResultsTruncated);
}
