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
    Id = "eacaf2d9-6983-41dd-bd91-b0502d9ca2d6",
    Name = "create",
    Title = "Create Azure Migrate Business Case",
    Description = "Starts an Azure Migrate business case for the signed-in user from their offline collector ZIP and returns at once. " +
        "Requires 'assessment-name', 'target-region' (an Azure region name such as 'eastus') and 'inventory-sas-uri' (a read-only https " +
        "SAS URL of the collector ZIP); ask the user for any that are missing and do not invent them. Only USD is supported. Optional " +
        "settings (scaling factor, savings option, Azure Hybrid Benefit, network and labor cost percentages, growth rate, migration " +
        "completion by year) use the business case defaults when left out; pass them only when the user asks for them. Returns 'status', " +
        "'stage', 'nextStatusParameters' and 'instructions'. Follow 'instructions' and pass 'nextStatusParameters' unchanged to the " +
        "business case status tool. Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CreateBusinessCaseCommand(IAssessmentCreateClient createClient, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<CreateBusinessCaseOptions, BusinessCaseCommandResult>
{
    internal const string Currency = "USD";
    internal const double DefaultScalingFactor = 1.3;
    internal const string DefaultSavingsOption = "RI3Year";
    internal const string DefaultAzureHybridBenefit = "Yes";

    private readonly IAssessmentCreateClient _createClient = createClient;
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
        string? name = options.AssessmentName?.Trim();
        if (!CreateAssessmentCommand.IsValidAssessmentName(name))
        {
            return BusinessCaseStages.Invalid(
                "'assessment-name' is required: 1-64 characters, starting with a letter or digit, using only letters, digits, '-' or '_'.", name);
        }

        string? region = CreateAssessmentCommand.NormalizeRegion(options.TargetRegion);
        if (region is null)
        {
            return BusinessCaseStages.Invalid("'target-region' is required and must be an Azure region name such as 'eastus'.", name);
        }

        if (!string.IsNullOrWhiteSpace(options.Currency) && !string.Equals(options.Currency.Trim(), Currency, StringComparison.OrdinalIgnoreCase))
        {
            return BusinessCaseStages.Invalid("'currency' must be 'USD': the business case supports only USD for now.", name);
        }

        string? inventorySasUri = options.InventorySasUri?.Trim();
        if (!CreateAssessmentCommand.IsValidInventorySasUri(inventorySasUri))
        {
            return BusinessCaseStages.Invalid("'inventory-sas-uri' is required and must be an https read-only SAS URL of the collector ZIP.", name);
        }

        (AssessmentSettings? assessmentSettings, string? assessmentError) = CreateAssessmentCommand.BuildSettings(new CreateAssessmentOptions
        {
            ScalingFactor = options.ScalingFactor ?? DefaultScalingFactor,
            SavingsOption = string.IsNullOrWhiteSpace(options.SavingsOption) ? DefaultSavingsOption : options.SavingsOption,
            AzureHybridBenefit = string.IsNullOrWhiteSpace(options.AzureHybridBenefit) ? DefaultAzureHybridBenefit : options.AzureHybridBenefit,
            LinuxAzureHybridBenefit = string.IsNullOrWhiteSpace(options.LinuxAzureHybridBenefit) ? DefaultAzureHybridBenefit : options.LinuxAzureHybridBenefit,
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

        AssessmentRequest request = new(name!, WorkspaceId: null, inventorySasUri!, region, Currency, assessmentSettings);
        AssessmentOperation operation = await _createClient.CreateAssessmentAsync(request, token, cancellationToken);
        if (operation.HttpStatus is not (>= 200 and < 300) || operation.OperationId is null)
        {
            return new(true, null, BusinessCaseStages.Failed, true, BusinessCaseStages.Assessment, 1, BusinessCaseStages.Total, name,
                operation.OperationId?.ToString("D"), null, null, "assessmentCreate", operation.HttpStatus, operation.ErrorCode,
                operation.ErrorMessage ?? $"Assessment create failed (HTTP {operation.HttpStatus}).", null, null, null,
                "Tell the user the business case could not be started because creating its assessment failed, and quote 'errorMessage'.");
        }

        Guid operationId = operation.OperationId.Value;
        return new(true, null, BusinessCaseStages.Running, false, BusinessCaseStages.Assessment, 1, BusinessCaseStages.Total, name,
            operationId.ToString("D"), null, null, null, operation.HttpStatus, null, null, null, operation.RetryAfterSeconds ?? 30,
            BusinessCaseNextStatusParameters.Create(name, operationId, inventorySasUri!, haSettings!),
            "Tell the user the business case has started (stage 1 of 2: assessment) and can take several minutes. " +
            BusinessCaseStages.CheckAgainInstructions);
    }
}
