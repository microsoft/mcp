// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Policy.Models;
using Azure.Mcp.Tools.Policy.Options;
using Azure.Mcp.Tools.Policy.Options.Assignment;
using Azure.Mcp.Tools.Policy.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Policy.Commands.Assignment;

public sealed class PolicyAssignmentListCommand(ILogger<PolicyAssignmentListCommand> logger)
    : SubscriptionCommand<PolicyAssignmentListOptions>
{
    private const string CommandTitle = "List Policy Assignments";
    private readonly ILogger<PolicyAssignmentListCommand> _logger = logger;

    public override string Id => "b7c4d3e2-0f1a-4b8c-9d6e-5a7b8c9d0e1f";

    public override string Name => "list";

    public override string Description =>
        """
        List policy assignments in a subscription or scope. This command retrieves all Azure Policy
        assignments, including their definitions, enforcement modes, parameters, and metadata. You can
        optionally filter by scope to list assignments at a specific resource group, resource, or
        management group level.
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
        command.Options.Add(PolicyOptionDefinitions.Scope.AsOptional());
    }

    protected override PolicyAssignmentListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Scope = parseResult.GetValueOrDefault<string>(PolicyOptionDefinitions.Scope.Name);
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
            var policyService = context.GetService<IPolicyService>();
            var assignments = await policyService.ListPolicyAssignmentsAsync(
                options.Subscription!,
                options.Scope,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new PolicyAssignmentListCommandResult(assignments ?? []),
                PolicyJsonContext.Default.PolicyAssignmentListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing policy assignments in subscription '{Subscription}' with scope '{Scope}'. Options: {@Options}",
                options.Subscription, options.Scope ?? "all", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed. Ensure you have the 'Reader' role or higher on the subscription or scope. Details: {reqEx.Message}",
        Azure.Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        _ => base.GetErrorMessage(ex)
    };

    protected override System.Net.HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => (System.Net.HttpStatusCode)reqEx.Status,
        Azure.Identity.AuthenticationFailedException => System.Net.HttpStatusCode.Unauthorized,
        _ => base.GetStatusCode(ex)
    };

    public record PolicyAssignmentListCommandResult(List<PolicyAssignment> Assignments);
}
