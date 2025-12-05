// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Dps.Models;
using Azure.Mcp.Tools.Dps.Options;
using Azure.Mcp.Tools.Dps.Options.Instance;
using Azure.Mcp.Tools.Dps.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Dps.Commands.Instance;

/// <summary>
/// Command to create a Device Provisioning Service instance.
/// </summary>
public sealed class InstanceCreateCommand(ILogger<InstanceCreateCommand> logger) : SubscriptionCommand<InstanceCreateOptions>()
{
    private const string CommandTitle = "Create Device Provisioning Service Instance";
    private readonly ILogger<InstanceCreateCommand> _logger = logger;

    public override string Id => "4d9e0f3a-5c2b-4d6e-9f8a-1b3c4e5d6f7a";

    public override string Name => "create";

    public override string Description =>
        """
        Creates a Device Provisioning Service (DPS) instance in an Azure subscription. Returns instance details including name, location, SKU, and provisioning state. Requires a resource group and location. Optionally, you can link an IoT Hub during creation by providing its connection string and location.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(DpsOptionDefinitions.InstanceName);
        command.Options.Add(DpsOptionDefinitions.Location);
        command.Options.Add(DpsOptionDefinitions.Sku);
        command.Options.Add(DpsOptionDefinitions.Capacity);
        command.Options.Add(DpsOptionDefinitions.AllocationPolicy);
        command.Options.Add(DpsOptionDefinitions.LinkedHubConnectionString);
        command.Options.Add(DpsOptionDefinitions.LinkedHubLocation);
    }

    protected override InstanceCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.InstanceName = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.InstanceName.Name);
        options.Location = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.Location.Name);
        options.Sku = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.Sku.Name);
        options.Capacity = parseResult.GetValueOrDefault<int?>(DpsOptionDefinitions.Capacity.Name);
        options.AllocationPolicy = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.AllocationPolicy.Name);
        options.LinkedHubConnectionString = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.LinkedHubConnectionString.Name);
        options.LinkedHubLocation = parseResult.GetValueOrDefault<string>(DpsOptionDefinitions.LinkedHubLocation.Name);
        return options;
    }

    /// <summary>
    /// Test helper method to expose BindOptions for unit testing.
    /// </summary>
    internal InstanceCreateOptions TestBindOptions(ParseResult parseResult) => BindOptions(parseResult);

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Get the DPS service from DI
            var dpsService = context.GetService<IDpsService>();

            // Call service to create DPS instance
            var instance = await dpsService.CreateInstanceAsync(
                options.InstanceName!,
                options.ResourceGroup!,
                options.Location!,
                options.Subscription!,
                options.Sku,
                options.Capacity,
                options.AllocationPolicy,
                options.LinkedHubConnectionString,
                options.LinkedHubLocation,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            // Set results
            context.Response.Results = ResponseResult.Create(
                new InstanceCreateCommandResult(instance),
                DpsJsonContext.Default.InstanceCreateCommandResult);
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error creating DPS instance. InstanceName: {InstanceName}, ResourceGroup: {ResourceGroup}, Location: {Location}, Options: {@Options}",
                options.InstanceName, options.ResourceGroup, options.Location, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "DPS instance name already exists. Choose a different name.",
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the DPS instance. Ensure you have appropriate RBAC permissions. Details: {reqEx.Message}",
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group not found. Verify the resource group exists and you have access.",
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Invalid request. Verify the parameters are correct. Details: {reqEx.Message}",
        Azure.Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        ArgumentException argEx =>
            argEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        Azure.Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    /// <summary>
    /// Result record for instance create command.
    /// </summary>
    internal record InstanceCreateCommandResult([property: JsonPropertyName("instance")] DpsInstanceResult Instance);
}
