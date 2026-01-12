// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

/// <summary>
/// Checks if a file share name is available.
/// </summary>
public sealed class FileShareCheckNameAvailabilityCommand(ILogger<FileShareCheckNameAvailabilityCommand> logger, IFileSharesService fileSharesService)
    : BaseFileSharesCommand<FileShareCheckNameAvailabilityOptions>(logger, fileSharesService)
{
    public override string Id => "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d";
    public override string Name => "check-name-availability";
    public override string Description => "Check if a file share name is available";
    public override string Title => "Check File Share Name Availability";

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
        command.Options.Add(FileSharesOptionDefinitions.FileShare.Name.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShare.Location.AsRequired());
    }

    protected override FileShareCheckNameAvailabilityOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.FileShare.Name.Name);
        options.Location = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.FileShare.Location.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        var options = BindOptions(parseResult);

        try
        {
            _logger.LogInformation(
                "Checking name availability for file share {FileShareName} in location {Location}",
                options.FileShareName,
                options.Location);

            var availabilityResult = await _fileSharesService.CheckNameAvailabilityAsync(
                options.Subscription!,
                options.FileShareName!,
                options.Location!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new FileShareCheckNameAvailabilityCommandResult(availabilityResult.IsAvailable, availabilityResult.Reason, availabilityResult.Message);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareCheckNameAvailabilityCommandResult);

            _logger.LogInformation(
                "Name availability check completed. File share name {FileShareName} is {Status}",
                options.FileShareName,
                availabilityResult.IsAvailable ? "available" : "not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file share name availability. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileShareCheckNameAvailabilityCommandResult(bool IsAvailable, string? Reason, string? Message);
}
