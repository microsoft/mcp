// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;
using Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;

/// <summary>
/// Shows the ROI report (ROI.md) of a finished business case. The report card is attached to this tool rather than to the status
/// tool, so the client renders it once instead of on every status poll.
/// </summary>
[CommandMetadata(
    Id = CommandId,
    Name = "report",
    Title = "Show Azure Catalyst Business Case Report",
    Description = "Shows the ROI report of a finished Azure Catalyst business case in a report card. " +
        "When: once, right after the business case status tool returns 'status' Succeeded with a 'result' link, or when the user asks to " +
        "see the report again. Never call it while the business case is running. " +
        "How: pass 'result' of the succeeded status result unchanged as 'report-uri', and its 'assessmentName'. " +
        "Returns: 'report' (the ROI report Markdown), 'result' (the read link) and 'instructions'. " +
        "Next: follow 'instructions'. Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ShowBusinessCaseReportCommand(IHaSessionClient haClient)
    : BaseCommand<ShowBusinessCaseReportOptions, BusinessCaseCommandResult>
{
    internal const string CommandId = "3f0c9a52-6b1e-4d27-9a84-5c7e2b1d8f63";

    private readonly IHaSessionClient _haClient = haClient;

    /// <summary>Returns true for an https link to Azure Blob Storage, the only place the ROI report is exported to.</summary>
    internal static bool IsValidReportUri(string? uri) =>
        Uri.TryCreate(uri, UriKind.Absolute, out Uri? parsed)
        && parsed.Scheme == Uri.UriSchemeHttps
        && parsed.Host.EndsWith(".blob.core.windows.net", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ShowBusinessCaseReportOptions options, CancellationToken cancellationToken)
    {
        BusinessCaseCommandResult result = await RunAsync(options, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.BusinessCaseCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    internal async Task<BusinessCaseCommandResult> RunAsync(ShowBusinessCaseReportOptions options, CancellationToken cancellationToken)
    {
        string? name = string.IsNullOrWhiteSpace(options.AssessmentName) ? null : options.AssessmentName.Trim();
        string? reportUri = options.ReportUri?.Trim();
        if (!IsValidReportUri(reportUri))
        {
            return BusinessCaseStages.Invalid(
                "'report-uri' is required and must be 'result' of the succeeded business case status result, unchanged.", name);
        }

        string? markdown = await _haClient.DownloadReportAsync(reportUri!, cancellationToken);
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return Result(name, BusinessCaseStages.Failed, reportUri!,
                "Tell the user the ROI report could not be shown, possibly because its link has expired. Show 'result' as a clickable " +
                "Markdown link exactly as given, and say they can ask for the business case status again for a new link.");
        }

        return Result(name, BusinessCaseStages.Succeeded, reportUri!,
            "The ROI report is shown to the user in the report card; do not paste 'report'. Summarise its headline figures from 'report' " +
            "in a few bullets (annual savings, ROI, NPV and payback per migration strategy, and the number of servers), quoting the " +
            "figures exactly. Then add a clickable Markdown link to 'result' exactly as given, for example " +
            "[Download the ROI report (ROI.md)](<result>); never retype, shorten or show the URL on its own, and say the link expires.")
            with { Report = markdown };
    }

    private static BusinessCaseCommandResult Result(string? name, string status, string reportUri, string instructions) =>
        new(true, null, status, true, BusinessCaseStages.HaSkill, BusinessCaseStages.StageNumber(BusinessCaseStages.HaSkill),
            BusinessCaseStages.Total, name, null, null, null, status == BusinessCaseStages.Failed ? "reportDownload" : null, null, null,
            null, reportUri, null, null, instructions);
}
