// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.DeviceRegistry.Options.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Azure.ResourceManager.DeviceRegistry;

namespace Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;

public sealed class NamespaceListCommand(ILogger<NamespaceListCommand> logger) : BaseDeviceRegistryCommand<NamespaceListOptions>
{
    private const string CommandTitle = "List Device Registry Namespaces";
    private readonly ILogger<NamespaceListCommand> _logger = logger;

    public override string Id => "8f3d2a5c-4b1e-4c9a-8d7f-2e5b6c9a4d3f";

    public override string Name => "list";

    public override string Description =>
        """
        Lists all Device Registry namespaces in a subscription or within a specific resource group. Returns namespace details including name, location, provisioning state, and properties.
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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup);
    }

    protected override NamespaceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            var deviceRegistryService = context.GetService<IDeviceRegistryService>();
            var namespaces = await deviceRegistryService.GetNamespacesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var namespaceInfos = namespaces.Select(ns => new NamespaceInfo(
                ns.Data.Name,
                ns.Data.Id.ToString(),
                ns.Data.Location.ToString(),
                ns.Data.Properties?.ProvisioningState?.ToString()
            )).ToList();

            context.Response.Results = ResponseResult.Create(
                new NamespaceListCommandResult(namespaceInfos),
                DeviceRegistryJsonContext.Default.NamespaceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Device Registry namespaces. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record NamespaceListCommandResult(
        [property: JsonPropertyName("namespaces")] List<NamespaceInfo> Namespaces);

    internal record NamespaceInfo(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("location")] string Location,
        [property: JsonPropertyName("provisioningState")] string? ProvisioningState);
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(NamespaceListCommand.NamespaceListCommandResult))]
internal partial class DeviceRegistryJsonContext : JsonSerializerContext
{
}
