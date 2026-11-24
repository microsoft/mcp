// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Commands;
using Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Services;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.BusinessCase;

public class BusinessCaseCommandTests
{
    private const string SasUri = "https://sa.blob.core.windows.net/zips/collector.zip?sp=r&sig=secret";
    private const string OutputUri = "https://sa.blob.core.windows.net/out/recommendations.json?sp=r&sig=out";
    private const string TcoUri = "https://sa.blob.core.windows.net/bcs/manifest.json?sp=r&sig=tco";
    private const string HaUri = "https://sa.blob.core.windows.net/ha/manifest.json?sp=r&sig=ha";

    private readonly Guid _asOperationId = Guid.NewGuid();
    private readonly IAssessmentService _create = Substitute.For<IAssessmentService>();
    private readonly IAssessmentService _asStatus = Substitute.For<IAssessmentService>();
    private readonly IBusinessCaseEvaluationService _evaluation = Substitute.For<IBusinessCaseEvaluationService>();
    private readonly IHaSessionService _ha = Substitute.For<IHaSessionService>();
    private readonly IInventoryExportService _export = Substitute.For<IInventoryExportService>();

    public BusinessCaseCommandTests() =>
        _ha.ExportArtifactsAsync(default!, default!, default).ReturnsForAnyArgs(new InventoryExportResult(404, null, "none"));

    [Fact]
    public async Task Create_StartsAssessmentWithBusinessCaseDefaultsAndReturnsNextStatusParameters()
    {
        _create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(202, _asOperationId, "Running", null, 10, null, null));

        var result = await CreateCommand().RunAsync(
            new() { AssessmentName = "bc-1", TargetRegion = "East US", InventorySasUri = SasUri }, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal("Running", result.Status);
        Assert.Equal("Assessment", result.Stage);
        Assert.Equal(2, result.TotalStages);
        Assert.Equal(10, result.RetryAfterSeconds);
        Assert.Equal(_asOperationId.ToString("D"), result.NextStatusParameters!.AsOperationId);
        Assert.Equal(new double[] { 0, 20, 50, 100 }, result.NextStatusParameters.MigrationCompletionByYear);
        Assert.Contains("scaling factor 1.3; storage scaling factor 1; savings option RI3Year", result.Instructions);
        Assert.Contains("the inventory SAS URL the user gave in the chat", result.Instructions);
        Assert.Contains("migration completion by year 0, 20, 50, 100%", result.Instructions);
        await _create.Received(1).CreateAssessmentAsync(
            Arg.Is<AssessmentRequest>(r => r.Currency == "USD" && r.TargetRegion == "eastus" && r.Settings!.ScalingFactor == 1.3
                && r.Settings.SavingsOption == "RI3Year" && r.Settings.AzureHybridBenefit == "Yes" && r.Settings.LinuxAzureHybridBenefit == "Yes"),
            "token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_PassesUserSettingsToTheAssessment()
    {
        _create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(202, _asOperationId, "Running", null, 10, null, null));

        var result = await CreateCommand().RunAsync(
            new()
            {
                AssessmentName = "bc-1", TargetRegion = "eastus", InventorySasUri = SasUri, DiscountPercentage = 10,
                EnvironmentType = "devtest", DiskTypes = ["Premium"], UptimeHoursPerDay = 12,
            },
            TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        await _create.Received(1).CreateAssessmentAsync(
            Arg.Is<AssessmentRequest>(r => r.Settings!.DiscountPercentage == 10 && r.Settings.EnvironmentType == "DevTest"
                && r.Settings.DiskTypes!.SequenceEqual(new[] { "Premium" }) && r.Settings.UptimeHoursPerDay == 12
                && r.Settings.ScalingFactor == 1.3 && r.Settings.SavingsOption == "RI3Year"),
            "token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WithoutName_GeneratesOneFromRegionAndTime()
    {
        _create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(202, _asOperationId, "Running", null, 10, null, null));

        var result = await CreateCommand().RunAsync(
            new() { TargetRegion = "Central India", InventorySasUri = SasUri }, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Matches(@"^bc-centralindia-\d{8}-\d{6}$", result.AssessmentName);
        Assert.True(AssessmentValidation.IsValidAssessmentName(result.AssessmentName));
        Assert.Equal(result.AssessmentName, result.NextStatusParameters!.AssessmentName);
        await _create.Received(1).CreateAssessmentAsync(
            Arg.Is<AssessmentRequest>(r => r.AssessmentName == result.AssessmentName), "token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WithSessionIdAndNoSas_ExportsTheInventoryAndUsesItsLink()
    {
        _export.ExportOfflineAsync("inv-1", "token", Arg.Any<CancellationToken>()).Returns(new InventoryExportResult(200, SasUri, null));
        _create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(202, _asOperationId, "Running", null, 10, null, null));

        var result = await CreateCommand().RunAsync(
            new() { AssessmentName = "bc-1", TargetRegion = "eastus", SessionId = "inv-1" }, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal("Running", result.Status);
        Assert.Equal(SasUri, result.NextStatusParameters!.InventorySasUri);
        Assert.Contains("a fresh export of the user's inventory session 'inv-1'", result.Instructions);
        await _create.Received(1).CreateAssessmentAsync(Arg.Is<AssessmentRequest>(r => r.InventorySasUri == SasUri), "token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_ExportFails_ReturnsFailedWithoutCallingTheAssessment()
    {
        _export.ExportOfflineAsync("inv-1", "token", Arg.Any<CancellationToken>()).Returns(new InventoryExportResult(400, null, "SessionState"));

        var result = await CreateCommand().RunAsync(
            new() { AssessmentName = "bc-1", TargetRegion = "eastus", SessionId = "inv-1" }, TestContext.Current.CancellationToken);

        Assert.Equal("Failed", result.Status);
        Assert.Contains("SessionState", result.ErrorMessage);
        Assert.Equal("inventoryExport", result.FailedStep);
        await _create.DidNotReceiveWithAnyArgs().CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("EUR", null, "'currency' must be 'USD'")]
    [InlineData(null, new double[] { 0, 50, 100 }, "'migration-completion-by-year' must have 4 values")]
    [InlineData(null, new double[] { 0, 50, 100, 101 }, "'migration-completion-by-year' must have 4 values")]
    public async Task Create_RejectsInvalidInputWithoutCallingTheAssessment(string? currency, double[]? ramp, string expectedError)
    {
        var result = await CreateCommand().RunAsync(
            new() { AssessmentName = "bc-1", TargetRegion = "eastus", InventorySasUri = SasUri, Currency = currency, MigrationCompletionByYear = ramp },
            TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Contains(expectedError, result.Error);
        await _create.DidNotReceiveWithAnyArgs().CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Status_AssessmentRunning_StaysInStageOne()
    {
        AssessmentStatus(new AssessmentOperation(200, _asOperationId, "Running", null, 15, null, null));

        var result = await StatusCommand().RunAsync(StatusOptions(), TestContext.Current.CancellationToken);

        Assert.Equal("Running", result.Status);
        Assert.Equal(1, result.StageNumber);
        Assert.Equal(15, result.RetryAfterSeconds);
        Assert.Null(result.NextStatusParameters!.DsSessionId);
        await _evaluation.DidNotReceiveWithAnyArgs().GetOnPremTcoAsync(default!, default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Status_AssessmentSucceeded_RunsEvaluationsAndStartsHaSkillInDerivedSession()
    {
        AssessmentSucceeded();
        Evaluations(new EvaluationResult(200, "Succeeded", TcoUri, null, null, null), new EvaluationResult(200, "Succeeded", HaUri, null, null, null));
        string sessionId = GetBusinessCaseStatusCommand.SessionIdFor(_asOperationId);
        string dsOperationId = Guid.NewGuid().ToString("D");
        _ha.StartInSessionAsync(default!, default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaSessionResult(null, 202, sessionId, false, dsOperationId, "req", null));

        var result = await StatusCommand().RunAsync(StatusOptions(o => o.NetworkCostPercentage = 7), TestContext.Current.CancellationToken);

        Assert.Equal("Running", result.Status);
        Assert.Equal("HaSkill", result.Stage);
        Assert.Equal(sessionId, result.NextStatusParameters!.DsSessionId);
        Assert.Equal(dsOperationId, result.NextStatusParameters.DsOperationId);
        Assert.Equal(7, result.NextStatusParameters.NetworkCostPercentage);
        Assert.Matches("^bc-[0-9a-f]{16}$", sessionId);
        await _evaluation.Received(1).GetOnPremTcoAsync("token", OutputUri, SasUri, Arg.Any<CancellationToken>());
        await _evaluation.Received(1).EvaluateHaAsync("token", OutputUri, TcoUri, Arg.Is<BusinessCaseHaSettings>(s => s.NetworkCostPercentage == 7),
            Arg.Any<CancellationToken>());
        await _ha.Received(1).StartInSessionAsync("token", new HaInput(SasUri, OutputUri, TcoUri, HaUri), sessionId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null, "Running", null)]
    [InlineData(503, "Running", null)]
    [InlineData(400, "Failed", "onPremTco")]
    public async Task Status_OnPremTcoError_IsRetriedOnlyWhenTransient(int? httpStatus, string expectedStatus, string? expectedFailedStep)
    {
        AssessmentSucceeded();
        Evaluations(new EvaluationResult(httpStatus, null, null, "InvalidInventory", "bad", null), null);

        var result = await StatusCommand().RunAsync(StatusOptions(), TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedFailedStep, result.FailedStep);
        Assert.Equal(expectedStatus == "Running", result.NextStatusParameters is not null);
        await _evaluation.DidNotReceiveWithAnyArgs().EvaluateHaAsync(default!, default!, default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Status_HaSkillSessionConflict_Fails()
    {
        AssessmentSucceeded();
        Evaluations(new EvaluationResult(200, "Succeeded", TcoUri, null, null, null), new EvaluationResult(200, "Succeeded", HaUri, null, null, null));
        _ha.StartInSessionAsync(default!, default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaSessionResult("createSession", 409, "bc-x", false, null, "req", "ActiveSessionAlreadyExists"));

        var result = await StatusCommand().RunAsync(StatusOptions(), TestContext.Current.CancellationToken);

        Assert.Equal("Failed", result.Status);
        Assert.Equal("createSession", result.FailedStep);
        Assert.Contains("another", result.Instructions);
    }

    [Theory]
    [InlineData("sessionBusy", 200, "Session 'bc-earlier' is Invoking.", "earlier ROI summary")]
    [InlineData("invokeSession", 400, "HTTP 400: InvalidSessionState", "being evaluated")]
    public async Task Status_HaSkillReusedSessionStillBusy_WaitsForIt(string failedStep, int httpStatus, string error, string instructions)
    {
        AssessmentSucceeded();
        Evaluations(new EvaluationResult(200, "Succeeded", TcoUri, null, null, null), new EvaluationResult(200, "Succeeded", HaUri, null, null, null));
        _ha.StartInSessionAsync(default!, default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaSessionResult(failedStep, httpStatus, "bc-earlier", true, null, "req", error));

        var result = await StatusCommand().RunAsync(StatusOptions(), TestContext.Current.CancellationToken);

        Assert.Equal("Running", result.Status);
        Assert.Null(result.FailedStep);
        Assert.NotNull(result.NextStatusParameters);
        Assert.Contains(instructions, result.Instructions);
    }

    [Fact]
    public async Task Status_FinalStageAlreadyStarted_ReturnsTheSameSessionWithoutStartingAgain()
    {
        AssessmentSucceeded();
        Evaluations(new EvaluationResult(200, "Succeeded", TcoUri, null, null, null), new EvaluationResult(200, "Succeeded", HaUri, null, null, null));
        string dsOperationId = Guid.NewGuid().ToString("D");
        _ha.StartInSessionAsync(default!, default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaSessionResult(null, 202, "bc-x", false, dsOperationId, "req", null));

        var first = await StatusCommand().RunAsync(StatusOptions(), TestContext.Current.CancellationToken);
        var second = await StatusCommand().RunAsync(StatusOptions(), TestContext.Current.CancellationToken);

        Assert.Equal(dsOperationId, first.NextStatusParameters!.DsOperationId);
        Assert.Equal(("bc-x", dsOperationId), (second.NextStatusParameters!.DsSessionId, second.NextStatusParameters.DsOperationId));
        await _ha.ReceivedWithAnyArgs(1).StartInSessionAsync(default!, default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Wait_ChecksUntilTheBusinessCaseFinishes()
    {
        Guid dsOperationId = Guid.NewGuid();
        _ha.GetOperationStatusAsync(default!, default!, default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaOperationStatus(200, "Running", null, null, null, null, null),
            new HaOperationStatus(200, "Failed", null, "HaFailed", "bad", null, null));
        WaitBusinessCaseCommand.CheckInterval = TimeSpan.Zero;

        var result = await new WaitBusinessCaseCommand(StatusCommand()).RunAsync(
            StatusOptions(o => { o.DsSessionId = "bc-x"; o.DsOperationId = dsOperationId.ToString("D"); }), TimeSpan.FromSeconds(10),
            TestContext.Current.CancellationToken);

        Assert.Equal("Failed", result.Status);
        Assert.True(result.IsCompleted);
        await _ha.ReceivedWithAnyArgs(2).GetOperationStatusAsync(default!, default!, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Wait_StillRunningWhenTimeIsUp_SaysToWaitAgain()
    {
        AssessmentStatus(new AssessmentOperation(200, _asOperationId, "Running", null, 15, null, null));

        var result = await new WaitBusinessCaseCommand(StatusCommand()).RunAsync(StatusOptions(), TimeSpan.Zero, TestContext.Current.CancellationToken);

        Assert.Equal("Running", result.Status);
        Assert.NotNull(result.NextStatusParameters);
        Assert.Equal(BusinessCaseStages.WaitAgainInstructions, result.Instructions);
    }

    [Theory]
    [InlineData(200, "Succeeded", "Succeeded", true)]
    [InlineData(200, "Failed", "Failed", true)]
    [InlineData(200, "Running", "Running", false)]
    [InlineData(503, null, "Running", false)]
    [InlineData(404, null, "Failed", true)]
    public async Task Status_WithDsIds_ReadsHaSkillOperationOnly(int httpStatus, string? dsStatus, string expectedStatus, bool expectedCompleted)
    {
        Guid dsOperationId = Guid.NewGuid();
        _ha.GetOperationStatusAsync(default!, default!, default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaOperationStatus(httpStatus, dsStatus, "{\"savings\":1}", null, null, null, null));
        _ha.ExportAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new InventoryExportResult(200, "https://acct.blob.core.windows.net/c/ROI.md?sv=1&sig=x", null));

        var result = await StatusCommand().RunAsync(
            StatusOptions(o => { o.DsSessionId = "bc-0123456789abcdef"; o.DsOperationId = dsOperationId.ToString(); }),
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedCompleted, result.IsCompleted);
        Assert.Equal("HaSkill", result.Stage);
        Assert.Equal(expectedStatus == "Succeeded" ? "https://acct.blob.core.windows.net/c/ROI.md?sv=1&sig=x" : null, result.Result);
        await _ha.Received(1).GetOperationStatusAsync("token", "bc-0123456789abcdef", dsOperationId, Arg.Any<CancellationToken>());
        await _asStatus.DidNotReceiveWithAnyArgs().GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Wait_Succeeded_SaysToShowTheReportCardWithoutTheReport()
    {
        const string uri = "https://acct.blob.core.windows.net/c/ROI.md?sv=1&sig=x";
        _ha.GetOperationStatusAsync(default!, default!, default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaOperationStatus(200, "Succeeded", null, null, null, null, null));
        _ha.ExportAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new InventoryExportResult(200, uri, null));
        _ha.DownloadReportAsync(uri, Arg.Any<CancellationToken>()).Returns("# ROI");

        var result = await new WaitBusinessCaseCommand(StatusCommand()).RunAsync(
            StatusOptions(o => { o.DsSessionId = "bc-0123456789abcdef"; o.DsOperationId = Guid.NewGuid().ToString(); }),
            TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        Assert.True(result.IsCompleted);
        Assert.Null(result.Report);
        Assert.Equal(BusinessCaseStages.ShowReportInstructions, result.Instructions);
    }

    [Fact]
    public async Task Status_Succeeded_ReturnsDownloadedReportMarkdown()
    {
        const string uri = "https://acct.blob.core.windows.net/c/ROI.md?sv=1&sig=x";
        const string zip = "https://acct.blob.core.windows.net/c/BusinessCase-internal.md?sv=1&sig=y";
        _ha.GetOperationStatusAsync(default!, default!, default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaOperationStatus(200, "Succeeded", null, null, null, null, null));
        _ha.ExportAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new InventoryExportResult(200, uri, null));
        _ha.ExportArtifactsAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new InventoryExportResult(200, zip, null));
        _ha.DownloadReportAsync(uri, Arg.Any<CancellationToken>()).Returns("# ROI\n| a | b |");

        var result = await StatusCommand().RunAsync(
            StatusOptions(o => { o.DsSessionId = "bc-0123456789abcdef"; o.DsOperationId = Guid.NewGuid().ToString(); }),
            TestContext.Current.CancellationToken);

        Assert.Equal("Succeeded", result.Status);
        Assert.Equal(uri, result.Result);
        Assert.Equal("# ROI\n| a | b |", result.Report);
        Assert.Equal(zip, result.Artifacts);
        Assert.Contains("'artifacts'", result.Instructions);
        Assert.Contains("full report is shown in the card", result.Instructions);
        Assert.Contains("'**Next step:**'", result.Instructions);
    }

    [Fact]
    public async Task Status_Succeeded_WhenCardFetchesReport_ReturnsLinksWithoutDownloading()
    {
        const string uri = "https://acct.blob.core.windows.net/c/ROI.md?sv=1&sig=x";
        const string zip = "https://acct.blob.core.windows.net/c/artifacts.zip?sv=1&sig=y";
        _ha.GetOperationStatusAsync(default!, default!, default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaOperationStatus(200, "Succeeded", null, null, null, null, null));
        _ha.ExportAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new InventoryExportResult(200, uri, null));
        _ha.ExportArtifactsAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new InventoryExportResult(200, zip, null));

        var command = new GetBusinessCaseStatusCommand(_asStatus, _evaluation, _ha, Accessor()) { CardFetchesReport = true };
        var result = await command.RunAsync(
            StatusOptions(o => { o.DsSessionId = "bc-0123456789abcdef"; o.DsOperationId = Guid.NewGuid().ToString(); }),
            TestContext.Current.CancellationToken);

        Assert.Equal("Succeeded", result.Status);
        Assert.Equal(uri, result.Result);
        Assert.Null(result.Report);
        Assert.Equal(zip, result.Artifacts);
        Assert.Contains("shown in the card above", result.Instructions);
        await _ha.DidNotReceiveWithAnyArgs().DownloadReportAsync(default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14 }, true)]
    [InlineData(new byte[] { 0x23, 0x20, 0x52, 0x4F, 0x49 }, false)]
    [InlineData(new byte[] { 0x50, 0x4B }, false)]
    public void IsZip_DetectsZipHeader(byte[] content, bool expected) =>
        Assert.Equal(expected, HaSessionService.IsZip(content));

    [Fact]
    public async Task Status_RequiresBothDsIds()
    {
        var result = await StatusCommand().RunAsync(StatusOptions(o => o.DsSessionId = "bc-1"), TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Contains("must be passed together", result.Error);
    }

    [Fact]
    public void NextStatusParameters_SerializeWithToolParameterNames()
    {
        var parameters = BusinessCaseNextStatusParameters.Create("bc-1", _asOperationId, SasUri, BusinessCaseHaSettings.Default, "bc-s", "op");
        var result = new BusinessCaseCommandResult(true, null, "Running", false, "HaSkill", 2, 2, "bc-1", null, null, null, null, null, null,
            null, null, 30, parameters, null);

        string json = JsonSerializer.Serialize(result, AzureMigrateJsonContext.Default.BusinessCaseCommandResult);

        Assert.Contains("\"nextStatusParameters\":{\"assessment-name\":\"bc-1\",\"as-operation-id\":", json);
        Assert.Contains("\"ds-session-id\":\"bc-s\"", json);
        Assert.Contains("\"migration-completion-by-year\":[0,20,50,100]", json);
    }

    [Fact]
    public void HaEvaluateBody_SendsSettingsAndYearRamp()
    {
        string body = BusinessCaseEvaluationService.BuildHaEvaluateBody(OutputUri, TcoUri, BusinessCaseHaSettings.Default);

        using JsonDocument doc = JsonDocument.Parse(body);
        JsonElement settings = doc.RootElement.GetProperty("settings");
        Assert.Equal(OutputUri, doc.RootElement.GetProperty("inputs").GetProperty("assessments").GetProperty("uri").GetString());
        Assert.Equal(TcoUri, doc.RootElement.GetProperty("inputs").GetProperty("onPremTco").GetProperty("uri").GetString());
        Assert.Equal(75, settings.GetProperty("iaasLaborCostPercentage").GetDouble());
        Assert.Equal(50, settings.GetProperty("perYearMigrationCompletionPercentage").GetProperty("Year2").GetDouble());
    }

    [Fact]
    public void ParseEvaluation_ReadsManifestAndError()
    {
        var ok = BusinessCaseEvaluationService.Parse(200, $"{{\"id\":\"x\",\"status\":\"Succeeded\",\"result\":{{\"manifestUri\":\"{TcoUri}\"}}}}", null);
        var error = BusinessCaseEvaluationService.Parse(400, "{\"error\":{\"code\":\"InvalidInventory\",\"message\":\"bad\"}}", null);

        Assert.Equal(("Succeeded", TcoUri), (ok.Status, ok.ManifestUri));
        Assert.Equal(("InvalidInventory", "bad"), (error.ErrorCode, error.ErrorMessage));
        Assert.DoesNotContain("sig=", ok.ToString());
    }

    [Theory]
    [InlineData(200, "{\"currency\":\"USD\",\"strategies\":[]}", "Succeeded", null)]
    [InlineData(503, null, "Running", null)]
    [InlineData(403, null, "Failed", "haResult")]
    public async Task Status_HaMocked_FinishesWithHaEvaluationSummary(int httpStatus, string? summary, string expectedStatus, string? expectedFailedStep)
    {
        AssessmentSucceeded();
        Evaluations(new EvaluationResult(200, "Succeeded", TcoUri, null, null, null), new EvaluationResult(200, "Succeeded", HaUri, null, null, null));
        _evaluation.GetHaSummaryAsync(default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaSummaryResult(httpStatus, summary, summary is null ? "error" : null));
        var command = new GetBusinessCaseStatusCommand(
            _asStatus, _evaluation, new MockHaSessionService(NullLogger<MockHaSessionService>.Instance), Accessor());

        var result = await command.RunAsync(StatusOptions(), TestContext.Current.CancellationToken);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedFailedStep, result.FailedStep);
        Assert.Equal(summary, result.Result);
        Assert.Null(result.DsSessionId);
        await _evaluation.Received(1).GetHaSummaryAsync(HaUri, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void BuildHaSummary_ReturnsBusinessCasePerStrategy()
    {
        const string manifest = """
            {"summary":{"currency":"USD","settingsUsed":[],"strategies":{
              "PaasPreferred":{"businessCase":{"status":"Complete","annualSavings":57273.1328125,"paybackPeriod":1,
                "totalAzureCost":{"compute":1,"total":123059.0859375},"totalOnPremCost":{"total":180332.21875},
                "yearOnYear":[{"year":"Year1","onPremisesCost":160274.0,"azureCost":138188.81,"savings":22085.19}]},
                "businessCaseUnavailableReason":null},
              "PaasCentric":{"businessCase":null,"businessCaseUnavailableReason":"Not computed."}},"diagnostics":{}}}
            """;

        string? summary = BusinessCaseEvaluationService.BuildHaSummary(manifest);

        Assert.Equal(
            "{\"currency\":\"USD\",\"strategies\":[{\"strategy\":\"PaasPreferred\",\"status\":\"Complete\",\"annualSavings\":57273.13," +
            "\"paybackPeriodYears\":1,\"azureTotalCost\":123059.09,\"onPremisesTotalCost\":180332.22,\"yearOnYear\":[{\"year\":\"Year1\"," +
            "\"onPremisesCost\":160274,\"azureCost\":138188.81,\"savings\":22085.19}]},{\"strategy\":\"PaasCentric\",\"unavailableReason\":\"Not computed.\"}]}",
            summary);
        Assert.Null(BusinessCaseEvaluationService.BuildHaSummary("{\"summary\":{}}"));
        Assert.Null(BusinessCaseEvaluationService.BuildHaSummary("not json"));
    }

    [Fact]
    public void HaInputPackage_IncludesBusinessCaseManifests()
    {
        byte[] zip = HaInputPackage.Build(new HaInput(SasUri, OutputUri, TcoUri, HaUri));

        using ZipArchive archive = new(new MemoryStream(zip));
        using JsonDocument manifest = JsonDocument.Parse(archive.Entries.Single().Open());
        JsonElement root = manifest.RootElement;
        Assert.Equal("bc-dataplane-links/1", root.GetProperty("schema").GetString());
        Assert.Equal(SasUri, root.GetProperty("inventory").GetProperty("uri").GetString());
        JsonElement assessment = root.GetProperty("assessments").EnumerateArray().Single();
        Assert.Equal("assessedMachines", assessment.GetProperty("type").GetString());
        Assert.Equal(OutputUri, assessment.GetProperty("uri").GetString());
        Assert.Equal(TcoUri, root.GetProperty("onPremTco").GetProperty("manifestUri").GetString());
        Assert.Equal(HaUri, root.GetProperty("ha").GetProperty("manifestUri").GetString());
        Assert.False(root.TryGetProperty("inventorySasUri", out _));
    }

    private void AssessmentStatus(AssessmentOperation operation) =>
        _asStatus.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(operation);

    private void AssessmentSucceeded() => AssessmentStatus(new AssessmentOperation(200, _asOperationId, "Succeeded", OutputUri, null, null, null));

    private void Evaluations(EvaluationResult tco, EvaluationResult? ha)
    {
        _evaluation.GetOnPremTcoAsync(default!, default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(tco);
        if (ha is not null)
        {
            _evaluation.EvaluateHaAsync(default!, default!, default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(ha);
        }
    }

    private GetBusinessCaseStatusOptions StatusOptions(Action<GetBusinessCaseStatusOptions>? configure = null)
    {
        GetBusinessCaseStatusOptions options = new() { AsOperationId = _asOperationId.ToString(), InventorySasUri = SasUri };
        configure?.Invoke(options);
        return options;
    }

    private static HttpContextAccessor Accessor()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer token";
        return new HttpContextAccessor { HttpContext = context };
    }

    private CreateBusinessCaseCommand CreateCommand() => new(_create, _export, Accessor());

    private GetBusinessCaseStatusCommand StatusCommand() => new(_asStatus, _evaluation, _ha, Accessor());
}
