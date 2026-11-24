// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Azure.Mcp.Tools.AzureMigrate.Options.Assessment;
using Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;
using Microsoft.AspNetCore.Http;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;

/// <summary>
/// POC: starts a business case for the signed-in user. It only creates the assessment and returns at once; the status command then
/// moves the business case forward on each call (see <see cref="GetBusinessCaseStatusCommand"/>). Nothing is stored on the server.
/// </summary>
[CommandMetadata(
    Id = CommandId,
    Name = "create",
    Title = "Create Azure Catalyst Business Case",
    Description = "Creates an Azure Catalyst business case for the signed-in user from their inventory: an assessment of their servers " +
        "for Azure, then cost savings, payback, ROI and Azure versus on-premises cost per migration strategy, and returns at once. " +
        "When: the user asks to create, build or run an assessment, a business case, an ROI or cost savings analysis, an ROI summary, " +
        "or executive artifacts (an executive summary of the migration). Use this tool for all of these. " +
        "How: needs 'target-region' (an Azure region name such as 'eastus') and the inventory: pass 'session-id' " +
        "(the 'sessionId' of the user's inventory, from the inventory tools) to use a fresh link to that inventory's offline zip, or " +
        "'inventory-sas-uri' only when the user gives a read-only https SAS URL of a collector ZIP; when you take it from the chat, " +
        "        tell the user you are using the inventory SAS URL from the chat. Before calling, ask the user for any of these " +
                "they have not given (target region, which inventory) and wait for the answer; do not invent or assume them. " +
                ReplyFormat.AskRegion + "Never ask " +
                "for a business case name: pass 'assessment-name' only when the user gives one, otherwise leave it out and a name is " +
                "generated. Only USD is supported. Optional settings use their defaults when left out; pass them only when the " +
        "user asks for them and do not ask for them otherwise: scaling factor (1.3), storage scaling factor (1), savings option " +
        "(RI3Year), Azure Hybrid Benefit for Windows and Linux (Yes), discount (0%), environment type (Production), licensing program " +
        "(Retail), Azure offer (pay as you go), Defender for Cloud (No), disk types (StandardSSD), pricing tier (Standard), storage " +
        "redundancy (locally redundant), VM families (all), uptime (31 days, 24 hours), VM security types (none), management services " +
        "(none), network cost (5%), IaaS, PaaS and management labor cost remaining (75%, 60%, 60%), infrastructure growth (5% a year) " +
        "and migration completion by year (0, 20, 50, 100%). " +
        "Returns: 'status', 'stage', 'nextStatusParameters' and 'instructions', which lists the settings used. " +
        "Next: success means the business case has started, not finished. Follow 'instructions': they say either to start tracking " +
        "it right away, in the same reply, with the business case status tool using 'nextStatusParameters' unchanged until it " +
        "completes (without waiting for the user to ask, calling the status tool directly yourself, never from a script or code), " +
        "or that the business case card tracks it for the user. The status tool's 'result' holds the ROI report. " +
        "Do not mention tool or parameter names to the user. " + ReplyFormat.ToolRule,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CreateBusinessCaseCommand(
    IAssessmentCreateClient createClient, IInventoryExportClient exportClient, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<CreateBusinessCaseOptions, BusinessCaseCommandResult>
{
    internal const string CommandId = "eacaf2d9-6983-41dd-bd91-b0502d9ca2d6";
    internal const string Currency = "USD";
    internal const double DefaultScalingFactor = 1.3;
    internal const string DefaultSavingsOption = "RI3Year";
    internal const string DefaultAzureHybridBenefit = "Yes";

    private readonly IAssessmentCreateClient _createClient = createClient;
    private readonly IInventoryExportClient _exportClient = exportClient;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CreateBusinessCaseOptions options, CancellationToken cancellationToken)
    {
        BusinessCaseCommandResult result = await RunAsync(options, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.BusinessCaseCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    internal async Task<BusinessCaseCommandResult> RunAsync(CreateBusinessCaseOptions options, CancellationToken cancellationToken)
    {
        string? name = string.IsNullOrWhiteSpace(options.AssessmentName) ? null : options.AssessmentName.Trim();
        if (name is not null && !AssessmentValidation.IsValidAssessmentName(name))
        {
            return BusinessCaseStages.Invalid(
                "'assessment-name' must be 1-64 characters, starting with a letter or digit, using only letters, digits, '-' or '_'.", name);
        }

        string? region = AssessmentValidation.NormalizeRegion(options.TargetRegion);
        if (region is null)
        {
            return BusinessCaseStages.Invalid("'target-region' is required and must be an Azure region name such as 'eastus'.", name);
        }

        name ??= GenerateName(region, DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(options.Currency) && !string.Equals(options.Currency.Trim(), Currency, StringComparison.OrdinalIgnoreCase))
        {
            return BusinessCaseStages.Invalid("'currency' must be 'USD': the business case supports only USD for now.", name);
        }

        string? inventorySasUri = options.InventorySasUri?.Trim();
        string? sessionId = options.SessionId?.Trim();
        if (string.IsNullOrEmpty(inventorySasUri))
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return BusinessCaseStages.Invalid(
                    "An inventory is required: pass 'session-id' of the user's inventory or 'inventory-sas-uri' of a collector ZIP.", name);
            }

            if (!DsBackdoorClient.IsValidSessionId(sessionId))
            {
                return BusinessCaseStages.Invalid("'session-id' must be 1-20 characters using only letters, digits, '-' or '_'.", name);
            }
        }
        else if (!AssessmentValidation.IsValidInventorySasUri(inventorySasUri))
        {
            return BusinessCaseStages.Invalid("'inventory-sas-uri' must be an https read-only SAS URL of the collector ZIP.", name);
        }

        (AssessmentSettings? assessmentSettings, string? assessmentError) = AssessmentValidation.BuildSettings(new CreateAssessmentOptions
        {
            ScalingFactor = options.ScalingFactor ?? DefaultScalingFactor,
            SavingsOption = string.IsNullOrWhiteSpace(options.SavingsOption) ? DefaultSavingsOption : options.SavingsOption,
            AzureHybridBenefit = string.IsNullOrWhiteSpace(options.AzureHybridBenefit) ? DefaultAzureHybridBenefit : options.AzureHybridBenefit,
            LinuxAzureHybridBenefit = string.IsNullOrWhiteSpace(options.LinuxAzureHybridBenefit) ? DefaultAzureHybridBenefit : options.LinuxAzureHybridBenefit,
            StorageScalingFactor = options.StorageScalingFactor,
            DiscountPercentage = options.DiscountPercentage,
            EnvironmentType = options.EnvironmentType,
            LicensingProgram = options.LicensingProgram,
            BillingSubscriptionId = options.BillingSubscriptionId,
            AzureOfferCode = options.AzureOfferCode,
            SecurityOfferingType = options.SecurityOfferingType,
            DiskTypes = options.DiskTypes,
            PricingTier = options.PricingTier,
            StorageRedundancy = options.StorageRedundancy,
            VmFamilies = options.VmFamilies,
            UptimeDaysPerMonth = options.UptimeDaysPerMonth,
            UptimeHoursPerDay = options.UptimeHoursPerDay,
            VmSecurityOptions = options.VmSecurityOptions,
            ManagementServices = options.ManagementServices,
        });
        (BusinessCaseHaSettings? haSettings, string? haError) = BusinessCaseHaSettings.Build(options);
        if (assessmentError is not null || haError is not null)
        {
            return BusinessCaseStages.Invalid(string.Join(" ", new[] { assessmentError, haError }.Where(e => e is not null)), name);
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return BusinessCaseStages.Invalid(authError, name);
        }

        string inventorySource = string.IsNullOrEmpty(inventorySasUri)
            ? $"a fresh export of the user's inventory session '{sessionId}'"
            : "the inventory SAS URL the user gave in the chat";

        if (string.IsNullOrEmpty(inventorySasUri))
        {
            InventoryExportResult export = await _exportClient.ExportOfflineAsync(sessionId!, token, cancellationToken);
            if (!AssessmentValidation.IsValidInventorySasUri(export.Uri))
            {
                return new(true, null, BusinessCaseStages.Failed, true, BusinessCaseStages.Assessment, 1, BusinessCaseStages.Total, name,
                    null, null, null, "inventoryExport", export.HttpStatus, null,
                    export.Error ?? "The inventory export did not return a valid link.", null, null, null,
                    ReplyFormat.Failure("Business case not started: the inventory's offline zip could not be exported.",
                        "check the inventory status (it may still be running, have failed or been archived) and try again once it " +
                        "is ready, or give a read-only SAS URL of a collector ZIP instead."));
            }

            inventorySasUri = export.Uri;
        }

        AssessmentRequest request = new(name!, WorkspaceId: null, inventorySasUri!, region, Currency, assessmentSettings);
        AssessmentOperation operation = await _createClient.CreateAssessmentAsync(request, token, cancellationToken);
        if (operation.HttpStatus is not (>= 200 and < 300) || operation.OperationId is null)
        {
            return new(true, null, BusinessCaseStages.Failed, true, BusinessCaseStages.Assessment, 1, BusinessCaseStages.Total, name,
                operation.OperationId?.ToString("D"), null, null, "assessmentCreate", operation.HttpStatus, operation.ErrorCode,
                operation.ErrorMessage ?? $"Assessment create failed (HTTP {operation.HttpStatus}).", null, null, null,
                ReplyFormat.Failure("Business case not started: its assessment could not be created.",
                    "fix the input named in the cause if there is one and try again; otherwise offer to try again."));
        }

        Guid operationId = operation.OperationId.Value;
        return new(true, null, BusinessCaseStages.Running, false, BusinessCaseStages.Assessment, 1, BusinessCaseStages.Total, name,
            operationId.ToString("D"), null, null, null, operation.HttpStatus, null, null, null, operation.RetryAfterSeconds ?? 30,
            BusinessCaseNextStatusParameters.Create(name, operationId, inventorySasUri!, haSettings!),
            ReplyFormat.Reply(
                $"Business case '{name}' started. It usually takes 2 to 5 minutes: first assessing the servers, then building the ROI report.",
                "the inventory used (" + inventorySource + ") and the region, plus any setting the user asked for. The full " +
                "list, to answer questions only and not to show: " + DescribeSettings(region, assessmentSettings!, haSettings!) + ".",
                "the ROI summary and report follow here when it is ready.") + " " +
            (BusinessCaseReportWidget.IsEnabled
                ? BusinessCaseStages.WaitInstructions
                : "Then, right away in this same reply, start tracking the business case status with exactly the values in " +
                  "'nextStatusParameters'; do not wait for the user to ask, do not change or invent any of them, and keep checking until " +
                  "it completes."));
    }

    /// <summary>Builds a valid business case name from the region and UTC time, for when the user gives none.</summary>
    internal static string GenerateName(string region, DateTime utcNow) =>
        $"bc-{region}-{utcNow.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture)}";

    /// <summary>Lists every business case setting with the value used, defaults included, for the user.</summary>
    internal static string DescribeSettings(string region, AssessmentSettings s, BusinessCaseHaSettings ha)
    {
        static string List(string[]? values, string fallback) => values is { Length: > 0 } ? string.Join(", ", values) : fallback;

        return string.Join("; ",
            $"target region {region}",
            $"currency {Currency}",
            $"scaling factor {s.ScalingFactor ?? DefaultScalingFactor}",
            $"storage scaling factor {s.StorageScalingFactor ?? 1}",
            $"savings option {s.SavingsOption ?? DefaultSavingsOption}",
            $"Azure Hybrid Benefit Windows {s.AzureHybridBenefit ?? DefaultAzureHybridBenefit}, Linux {s.LinuxAzureHybridBenefit ?? DefaultAzureHybridBenefit}",
            $"discount {s.DiscountPercentage ?? 0}%",
            $"environment type {s.EnvironmentType ?? "Production"}",
            $"licensing program {s.LicensingProgram ?? "Retail"}" + (s.BillingSubscriptionId is null ? string.Empty : $" (billing subscription {s.BillingSubscriptionId})"),
            $"Azure offer {s.AzureOfferCode ?? "MSAZR0003P (pay as you go)"}",
            $"Defender for Cloud {(s.SecurityOfferingType == "MDC" ? "Yes" : "No")}",
            $"disk types {List(s.DiskTypes, "StandardSSD")}",
            $"pricing tier {s.PricingTier ?? "Standard"}",
            $"storage redundancy {s.StorageRedundancy ?? "LocallyRedundant"}",
            $"VM families {List(s.VmFamilies, "all")}",
            $"uptime {s.UptimeDaysPerMonth ?? 31} days a month, {s.UptimeHoursPerDay ?? 24} hours a day",
            $"VM security types {List(s.VmSecurityOptions, "none")}",
            $"management services {List(s.ManagementServices, "none")}",
            $"network cost {ha.NetworkCostPercentage}%",
            $"labor cost remaining IaaS {ha.IaasLaborCostPercentage}%, PaaS {ha.PaasLaborCostPercentage}%, management {ha.ManagementLaborCostPercentage}%",
            $"infrastructure growth {ha.InfrastructureGrowthRate}% a year",
            $"migration completion by year {string.Join(", ", ha.MigrationCompletionByYear)}%");
    }
}
