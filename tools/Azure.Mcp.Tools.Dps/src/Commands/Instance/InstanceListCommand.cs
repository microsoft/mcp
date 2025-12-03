// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Dps.Models;
using Azure.Mcp.Tools.Dps.Options.Instance;
using Azure.Mcp.Tools.Dps.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Dps.Commands.Instance;

/// <summary>
/// Command to list Device Provisioning Service instances.
/// </summary>
public sealed class InstanceListCommand(ILogger<InstanceListCommand> logger) : SubscriptionCommand<InstanceListOptions>()
{
    private const string CommandTitle = "List Device Provisioning Service Instances";
    private readonly ILogger<InstanceListCommand> _logger = logger;

    public override string Id => "3c8f9e2d-4b1a-4c5e-8d6f-9a7b3c2e1f0d";

    public override string Name => "list";

    public override string Description =>
        """
        Lists Device Provisioning Service (DPS) instances in an Azure subscription. Returns instance name, location, SKU, provisioning state, ID scope, and allocation policy. Optionally filter by resource group.
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
        command.Options.Add(Azure.Mcp.Core.Models.Option.OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override InstanceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(Azure.Mcp.Core.Models.Option.OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }

    /// <summary>
    /// Test helper method to expose BindOptions for unit testing.
    /// </summary>
    internal InstanceListOptions TestBindOptions(ParseResult parseResult) => BindOptions(parseResult);

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

            // Call service operation with required parameters
            var instances = await dpsService.ListInstancesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            // Set results
            context.Response.Results = ResponseResult.Create(
                new InstanceListCommandResult(instances ?? []),
                DpsJsonContext.Default.InstanceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing DPS instances. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Subscription or resource group not found. Verify the subscription exists and you have access.",
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Ensure you have appropriate RBAC permissions. Details: {reqEx.Message}",
        Azure.Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        Azure.Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,
        _ => base.GetStatusCode(ex)
    };

    /// <summary>
    /// Result record for instance list command.
    /// </summary>
    internal record InstanceListCommandResult([property: JsonPropertyName("instances")] List<DpsInstanceInfo> Instances);
}
