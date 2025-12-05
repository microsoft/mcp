// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.DeviceRegistry.Options.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Azure.ResourceManager.DeviceRegistry;

namespace Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;

public sealed class NamespaceGetCommand(ILogger<NamespaceGetCommand> logger) : BaseDeviceRegistryCommand<NamespaceGetOptions>
{
    private const string CommandTitle = "Get Device Registry Namespace";
    private readonly ILogger<NamespaceGetCommand> _logger = logger;

    public override string Id => "7a4e8b2c-5d9f-4c1a-8e6f-2b5c9d7a3e1f";

    public override string Name => "get";

    public override string Description =>
        """
        Gets detailed information about a specific Device Registry namespace. Returns namespace details including name, location, provisioning state, resource ID, and properties.
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
        command.Options.Add(new Option<string>(
            "--name"
        )
        {
            Description = "The name of the Device Registry namespace to retrieve",
            Required = true
        });
    }

    protected override NamespaceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>("--name");
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
            var namespaceResource = await deviceRegistryService.GetNamespaceAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var namespaceInfo = new NamespaceInfo(
                namespaceResource.Data.Name,
                namespaceResource.Data.Id.ToString(),
                namespaceResource.Data.Location.ToString(),
                namespaceResource.Data.Properties?.ProvisioningState?.ToString(),
                namespaceResource.Data.Properties?.Uuid,
                namespaceResource.Data.Tags.ToDictionary(t => t.Key, t => t.Value)
            );

            context.Response.Results = ResponseResult.Create(
                new NamespaceGetCommandResult(namespaceInfo),
                DeviceRegistryGetJsonContext.Default.NamespaceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Device Registry namespace. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Name: {Name}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.Name, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record NamespaceGetCommandResult(
        [property: JsonPropertyName("namespace")] NamespaceInfo Namespace);

    internal record NamespaceInfo(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("location")] string Location,
        [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
        [property: JsonPropertyName("uuid")] string? Uuid,
        [property: JsonPropertyName("tags")] Dictionary<string, string>? Tags);
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(NamespaceGetCommand.NamespaceGetCommandResult))]
internal partial class DeviceRegistryGetJsonContext : JsonSerializerContext
{
}
