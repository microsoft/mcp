// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Options;
using Azure.Mcp.Tools.Authorization.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Authorization.Commands;

public sealed class RoleAssignmentListCommand(ILogger<RoleAssignmentListCommand> logger) : SubscriptionCommand<RoleAssignmentListOptions>
{
    private const string _commandTitle = "List Role Assignments";
    private readonly ILogger<RoleAssignmentListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List role assignments. This command retrieves and displays all Azure RBAC role assignments
        in the specified scope. Results include role definition IDs and principal IDs, returned as a JSON array.
        """;

    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Authorization.Scope);
    }

    protected override RoleAssignmentListOptions BindOptions(ParseResult parseResult)
    {
        var args = base.BindOptions(parseResult);
        args.Scope = parseResult.GetValueOrDefault<string>(OptionDefinitions.Authorization.Scope.Name);
        return args;
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
            var authService = context.GetService<IAuthorizationService>();
            var assignments = await authService.ListRoleAssignments(
                options.Scope,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(assignments ?? []), AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing role assignments.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record RoleAssignmentListCommandResult(List<RoleAssignment> Assignments);
}
