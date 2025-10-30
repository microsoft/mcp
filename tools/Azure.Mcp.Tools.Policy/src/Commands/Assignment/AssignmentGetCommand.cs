// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Policy.Models;
using Azure.Mcp.Tools.Policy.Options;
using Azure.Mcp.Tools.Policy.Options.Assignment;
using Azure.Mcp.Tools.Policy.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Policy.Commands.Assignment;

public sealed class AssignmentGetCommand(ILogger<AssignmentGetCommand> logger)
    : SubscriptionCommand<AssignmentGetOptions>()
{
    private const string CommandTitle = "Get Policy Assignments";
    private readonly ILogger<AssignmentGetCommand> _logger = logger;

    public override string Id => "a7b3c4d5-e6f7-8g9h-0i1j-2k3l4m5n6o7p";

    public override string Name => "get";

    public override string Description =>
        """
        Returns policy assignments.
        Use the Azure CLI tool to find the 'policyAssignments' list parameter.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        OpenWorld = false,
        Idempotent = true,
        ReadOnly = true,
        Secret = false,
        LocalRequired = true  // Requires Azure CLI installed locally
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(PolicyOptionDefinitions.Assignment.AssignmentOption.AsOptional());
        command.Options.Add(PolicyOptionDefinitions.Assignment.PolicyAssignmentsOption.AsOptional());
    }

    protected override AssignmentGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Assignment = parseResult.GetValueOrDefault<string>(PolicyOptionDefinitions.Assignment.AssignmentName);
        options.policyAssignments = parseResult.GetValueOrDefault<PolicyAssignment[]>(PolicyOptionDefinitions.Assignment.PolicyAssignmentsName) ?? Array.Empty<PolicyAssignment>();
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
            var service = context.GetService<IPolicyService>();

            var result = new AssignmentGetCommandResult((options.policyAssignments ?? []).ToList());
            context.Response.Results = ResponseResult.Create(result, PolicyJsonContext.Default.AssignmentGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        InvalidOperationException => $"Failed to retrieve policy assignments. {ex.Message}",
        FileNotFoundException => "Azure CLI is not installed or not found in PATH. Please install Azure CLI to use this command.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        InvalidOperationException => HttpStatusCode.BadRequest,
        FileNotFoundException => HttpStatusCode.NotFound,
        _ => base.GetStatusCode(ex)
    };

    internal record AssignmentGetCommandResult(List<PolicyAssignment> Results);
}
