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
using Azure.Mcp.Tools.Policy.Options.Assignment;
using Azure.Mcp.Tools.Policy.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Policy.Commands.Assignment;

public sealed class PolicyAssignmentGetCommand(ILogger<PolicyAssignmentGetCommand> logger)
    : SubscriptionCommand<PolicyAssignmentGetOptions>
{
    private const string CommandTitle = "Get Policy Assignment";
    private readonly ILogger<PolicyAssignmentGetCommand> _logger = logger;

    public override string Id => "a8f3e2b1-9c4d-4a5e-b6f7-8c9d0e1f2a3b";

    public override string Name => "get";

    public override string Description =>
        """
        Get a policy assignment by name and scope. This command retrieves the details of a specific
        Azure Policy assignment including its definition, enforcement mode, parameters, and metadata.
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
        command.Options.Add(PolicyOptionDefinitions.AssignmentName);
        command.Options.Add(PolicyOptionDefinitions.Scope);
    }

    protected override PolicyAssignmentGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.AssignmentName = parseResult.GetValueOrDefault<string>(PolicyOptionDefinitions.AssignmentName.Name);
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
            var assignment = await policyService.GetPolicyAssignmentAsync(
                options.AssignmentName!,
                options.Scope!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            if (assignment == null)
            {
                context.Response.Status = HttpStatusCode.NotFound;
                context.Response.Message = $"Policy assignment '{options.AssignmentName}' not found in scope '{options.Scope}'.";
                return context.Response;
            }

            context.Response.Results = ResponseResult.Create(
                new PolicyAssignmentGetCommandResult(assignment),
                PolicyJsonContext.Default.PolicyAssignmentGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting policy assignment '{AssignmentName}' in scope '{Scope}'. Options: {@Options}",
                options.AssignmentName, options.Scope, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "Policy assignment not found. Verify the assignment name and scope are correct.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed. Ensure you have the 'Reader' role or higher on the specified scope. Details: {reqEx.Message}",
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

    public record PolicyAssignmentGetCommandResult(PolicyAssignment Assignment);
}
