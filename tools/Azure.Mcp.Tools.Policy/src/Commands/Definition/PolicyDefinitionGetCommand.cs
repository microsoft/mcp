// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Policy.Models;
using Azure.Mcp.Tools.Policy.Options;
using Azure.Mcp.Tools.Policy.Options.Definition;
using Azure.Mcp.Tools.Policy.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Policy.Commands.Definition;

public sealed class PolicyDefinitionGetCommand(ILogger<PolicyDefinitionGetCommand> logger)
    : SubscriptionCommand<PolicyDefinitionGetOptions>
{
    private const string CommandTitle = "Get Policy Definition";
    private readonly ILogger<PolicyDefinitionGetCommand> _logger = logger;

    public override string Id => "d7e3f2a1-8c4b-4d5e-a6b7-9c8d0f1e2b3a";

    public override string Name => "get";

    public override string Description =>
        """
        Get a policy definition by name in a subscription or management group.

        This command retrieves detailed information about a specific policy definition,
        including its display name, description, policy type (BuiltIn, Custom, Static),
        mode (All, Indexed), policy rule, parameters, and metadata.

        Specify either --subscription for subscription-level definitions or --management-group
        for management group-level definitions.
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

        // Remove the subscription validator since we allow either subscription OR management group
        command.Validators.Clear();

        // Add custom validator for subscription OR management group
        command.Validators.Add(commandResult =>
        {
            var hasSubscription = commandResult.HasOptionResult(OptionDefinitions.Common.Subscription.Name) ||
                                  !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID"));
            var hasManagementGroup = commandResult.HasOptionResult(PolicyOptionDefinitions.ManagementGroup.Name);

            if (!hasSubscription && !hasManagementGroup)
            {
                commandResult.AddError("Either --subscription or --management-group must be specified.");
            }

            if (hasSubscription && hasManagementGroup)
            {
                commandResult.AddError("Cannot specify both --subscription and --management-group. Please specify only one.");
            }
        });

        command.Options.Add(PolicyOptionDefinitions.DefinitionName);
        command.Options.Add(PolicyOptionDefinitions.ManagementGroup.AsOptional());
    }

    protected override PolicyDefinitionGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DefinitionName = parseResult.GetValueOrDefault<string>(PolicyOptionDefinitions.DefinitionName.Name);
        options.ManagementGroup = parseResult.GetValueOrDefault<string>(PolicyOptionDefinitions.ManagementGroup.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Validate that either subscription or management group is provided
            if (string.IsNullOrEmpty(options.Subscription) && string.IsNullOrEmpty(options.ManagementGroup))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "Either --subscription or --management-group must be specified.";
                return context.Response;
            }

            // Validate that both are not provided
            if (!string.IsNullOrEmpty(options.Subscription) && !string.IsNullOrEmpty(options.ManagementGroup))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "Cannot specify both --subscription and --management-group. Please specify only one.";
                return context.Response;
            }

            var policyService = context.GetService<IPolicyService>();
            var definition = await policyService.GetPolicyDefinitionAsync(
                options.DefinitionName!,
                options.Subscription,
                options.ManagementGroup,
                options.Tenant,
                options.RetryPolicy);

            if (definition == null)
            {
                var scope = !string.IsNullOrEmpty(options.ManagementGroup)
                    ? $"management group '{options.ManagementGroup}'"
                    : $"subscription '{options.Subscription}'";
                context.Response.Status = HttpStatusCode.NotFound;
                context.Response.Message = $"Policy definition '{options.DefinitionName}' not found in {scope}.";
                return context.Response;
            }

            context.Response.Results = ResponseResult.Create(
                new PolicyDefinitionGetCommandResult(definition),
                PolicyJsonContext.Default.PolicyDefinitionGetCommandResult);
        }
        catch (Exception ex)
        {
            var scope = !string.IsNullOrEmpty(options.ManagementGroup)
                ? $"management group '{options.ManagementGroup}'"
                : $"subscription '{options.Subscription}'";
            _logger.LogError(ex,
                "Error getting policy definition '{DefinitionName}' in {Scope}. Options: {@Options}",
                options.DefinitionName, scope, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "Policy definition not found. Verify the definition name and that you have access.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the policy definition. Details: {reqEx.Message}",
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

    public record PolicyDefinitionGetCommandResult(PolicyDefinition? Definition);
}
