// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareCheckNameAvailabilityCommand(ILogger<FileShareCheckNameAvailabilityCommand> logger)
    : BaseFileSharesCommand<FileShareCheckNameAvailabilityOptions>
{
    private const string CommandTitle = "Check File Share Name Availability";
    private readonly ILogger<FileShareCheckNameAvailabilityCommand> _logger = logger;

    public override string Id => "j5e6f7a8-b9c0-41d2-e3f4-a5b6c7d8e9f0";

    public override string Name => "checkname";

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

    public override string Description =>
        "Check if a file share name is available in a location.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FileSharesOptionDefinitions.Location.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShareName.AsRequired());
    }

    protected override FileShareCheckNameAvailabilityOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Location.Name);
        options.Name = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.FileShareName.Name);
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
            var fileSharesService = context.GetService<IFileSharesService>();
            var availability = await fileSharesService.CheckNameAvailability(
                options.Subscription!,
                options.Location!,
                options.Name!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(availability, FileSharesJsonContext.Default.NameAvailabilityResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred checking name availability for {Name}.", options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
