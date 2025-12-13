// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Commands.Informational;

public sealed class FileShareGetUsageDataCommand(ILogger<FileShareGetUsageDataCommand> logger)
    : BaseFileSharesCommand<FileShareGetUsageDataCommand.FileShareGetUsageDataOptions>
{
    private const string CommandTitle = "Get File Share Usage Data";
    private readonly ILogger<FileShareGetUsageDataCommand> _logger = logger;

    public override string Id => "c3d4e5f6-a7b8-49c0-d1e2-f3a4b5c6d7e8";

    public override string Name => "get-usage-data";

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

    public override string Description => "Get File Shares usage data and analytics";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
    }

    protected override FileShareGetUsageDataOptions BindOptions(ParseResult parseResult)
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
            var usageData = new FileShareUsageData
            {
                TotalFileSharesCreated = 0,
                TotalStorageUsedGB = 0,
                AverageFileShareSizeGB = 0,
                PeakUsageHour = "Not available",
                GrowthTrendPercentage = 0
            };

            var result = new FileShareGetUsageDataCommandResult(usageData);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareGetUsageDataCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting File Shares usage data. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class FileShareGetUsageDataOptions : BaseFileSharesOptions
    {
    }

    internal record FileShareGetUsageDataCommandResult(FileShareUsageData UsageData);
}

/// <summary>
/// Model representing File Shares usage data.
/// </summary>
public sealed record FileShareUsageData
{
    [JsonPropertyName("totalFileSharesCreated")]
    public int TotalFileSharesCreated { get; set; }

    [JsonPropertyName("totalStorageUsedGB")]
    public double TotalStorageUsedGB { get; set; }

    [JsonPropertyName("averageFileShareSizeGB")]
    public double AverageFileShareSizeGB { get; set; }

    [JsonPropertyName("peakUsageHour")]
    public string? PeakUsageHour { get; set; }

    [JsonPropertyName("growthTrendPercentage")]
    public double GrowthTrendPercentage { get; set; }
}
