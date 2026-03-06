// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.DeviceRegistry.Models;
using Azure.Mcp.Tools.DeviceRegistry.Options.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;

public sealed class NamespaceListCommand(ILogger<NamespaceListCommand> logger)
    : BaseDeviceRegistryCommand<NamespaceListOptions>()
{
    private const string CommandTitle = "List Device Registry Namespaces";
    private readonly ILogger<NamespaceListCommand> _logger = logger;

    public override string Id => "9c42f93b-2d4e-4fb3-b98b-2ef119b46c94";

    public override string Name => "list";

    public override string Description =>
        """
        Lists Azure Device Registry namespaces in a subscription or resource group. Returns namespace details including
        name, location, provisioning state, and UUID. If a resource group is specified, only namespaces within that
        resource group are returned. Otherwise, all namespaces in the subscription are listed.
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
            var service = context.GetService<IDeviceRegistryService>();

            var namespaces = await service.ListNamespacesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new NamespaceListCommandResult(namespaces?.Results ?? [], namespaces?.AreResultsTruncated ?? false),
                DeviceRegistryJsonContext.Default.NamespaceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error listing Device Registry namespaces. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Subscription,
                options.ResourceGroup,
                options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource not found. Verify the subscription and resource group exist and you have access.",
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Details: {reqEx.Message}",
        Azure.RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record NamespaceListCommandResult(
        List<DeviceRegistryNamespaceInfo> Namespaces,
        bool AreResultsTruncated);
}
