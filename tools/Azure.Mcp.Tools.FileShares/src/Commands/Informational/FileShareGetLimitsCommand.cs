// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Commands.Informational;

public sealed class FileShareGetLimitsCommand(ILogger<FileShareGetLimitsCommand> logger)
    : BaseFileSharesCommand<FileShareGetLimitsCommand.FileShareGetLimitsOptions>
{
    private const string CommandTitle = "Get File Share Limits";
    private readonly ILogger<FileShareGetLimitsCommand> _logger = logger;

    public override string Id => "a1b2c3d4-e5f6-47a8-b9c0-d1e2f3a4b5c6";

    public override string Name => "get-limits";

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

    public override string Description => "Get File Shares service limits and quotas";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
    }

    protected override FileShareGetLimitsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
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
            var limits = new FileShareLimits
            {
                MaxStorageAccountsPerSubscription = 250,
                MaxFileSharesPerStorageAccount = 5000,
                MaxFileShareSizeGB = 102400,
                MaxFileSize = "4 TB",
                MaxDirectoryDepth = 2048,
                MaxFilenameLength = 255,
                MaxPathLength = 260
            };

            var result = new FileShareGetLimitsCommandResult(limits);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareGetLimitsCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting File Shares limits. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class FileShareGetLimitsOptions : BaseFileSharesOptions
    {
    }

    internal record FileShareGetLimitsCommandResult(FileShareLimits Limits);
}

/// <summary>
/// Model representing File Shares service limits.
/// </summary>
public sealed record FileShareLimits
{
    [JsonPropertyName("maxStorageAccountsPerSubscription")]
    public int MaxStorageAccountsPerSubscription { get; set; }

    [JsonPropertyName("maxFileSharesPerStorageAccount")]
    public int MaxFileSharesPerStorageAccount { get; set; }

    [JsonPropertyName("maxFileShareSizeGB")]
    public int MaxFileShareSizeGB { get; set; }

    [JsonPropertyName("maxFileSize")]
    public string MaxFileSize { get; set; } = string.Empty;

    [JsonPropertyName("maxDirectoryDepth")]
    public int MaxDirectoryDepth { get; set; }

    [JsonPropertyName("maxFilenameLength")]
    public int MaxFilenameLength { get; set; }

    [JsonPropertyName("maxPathLength")]
    public int MaxPathLength { get; set; }
}
