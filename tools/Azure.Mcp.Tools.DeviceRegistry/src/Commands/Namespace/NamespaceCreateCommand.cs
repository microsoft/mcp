// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.DeviceRegistry.Options.Namespace;
using Azure.Mcp.Tools.DeviceRegistry.Services;
using Azure.ResourceManager.DeviceRegistry;

namespace Azure.Mcp.Tools.DeviceRegistry.Commands.Namespace;

public sealed class NamespaceCreateCommand(ILogger<NamespaceCreateCommand> logger) : BaseDeviceRegistryCommand<NamespaceCreateOptions>
{
    private const string CommandTitle = "Create Device Registry Namespace";
    private readonly ILogger<NamespaceCreateCommand> _logger = logger;

    public override string Id => "9c2b3f7a-6d8e-4a1c-9e5f-3b7c8d6a2e4f";

    public override string Name => "create";

    public override string Description =>
        """
        Creates a new Device Registry namespace in the specified resource group. A namespace provides a logical container for Device Registry resources.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
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
            Description = "The name of the Device Registry namespace to create",
            Required = true
        });
        command.Options.Add(new Option<string>(
            "--location"
        )
        {
            Description = "The Azure region where the namespace will be created (e.g., 'eastus', 'westus2')",
            Required = true
        });
        command.Options.Add(new Option<string[]>(
            "--tags"
        )
        {
            Description = "Tags for the namespace in key=value format. Can be specified multiple times.",
            Required = false,
            AllowMultipleArgumentsPerToken = true
        });
    }

    protected override NamespaceCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>("--name");
        options.Location = parseResult.GetValueOrDefault<string>("--location");
        
        var tags = parseResult.GetValueOrDefault<string[]>("--tags");
        if (tags != null && tags.Length > 0)
        {
            options.Tags = new Dictionary<string, string>();
            foreach (var tag in tags)
            {
                var parts = tag.Split('=', 2);
                if (parts.Length == 2)
                {
                    options.Tags[parts[0]] = parts[1];
                }
            }
        }
        
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
            var namespaceResource = await deviceRegistryService.CreateNamespaceAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.Location!,
                options.Tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var namespaceInfo = new NamespaceInfo(
                namespaceResource.Data.Name,
                namespaceResource.Data.Id.ToString(),
                namespaceResource.Data.Location.ToString(),
                namespaceResource.Data.Properties?.ProvisioningState?.ToString()
            );

            context.Response.Results = ResponseResult.Create(
                new NamespaceCreateCommandResult(namespaceInfo),
                DeviceRegistryCreateJsonContext.Default.NamespaceCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating Device Registry namespace. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Name: {Name}, Location: {Location}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.Name, options.Location, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record NamespaceCreateCommandResult(
        [property: JsonPropertyName("namespace")] NamespaceInfo Namespace);

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
[JsonSerializable(typeof(NamespaceCreateCommand.NamespaceCreateCommandResult))]
internal partial class DeviceRegistryCreateJsonContext : JsonSerializerContext
{
}
