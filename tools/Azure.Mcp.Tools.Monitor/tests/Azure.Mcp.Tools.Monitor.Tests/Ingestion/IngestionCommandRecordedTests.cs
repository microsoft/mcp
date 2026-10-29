// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Ingestion;

public sealed class IngestionCommandRecordedTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string ValidLogData = """
        [
          {
            "TimeGenerated": "2026-01-01T00:00:00Z",
            "FixtureId": "mcp-ingestion-live-1",
            "Message": "first ingestion record",
            "Count": 7,
            "Enabled": true,
            "OptionalValue": null
          },
          {
            "TimeGenerated": "2026-01-01T00:00:00Z",
            "FixtureId": "mcp-ingestion-live-2",
            "Message": "second ingestion record",
            "Count": 8,
            "Enabled": false,
            "OptionalValue": "present"
          }
        ]
        """;

    // Schema validation needs the original column names, not the proxy's default name replacement.
    public override List<string> DisabledDefaultSanitizers =>
        [.. base.DisabledDefaultSanitizers, "AZSDK3493"];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new(new BodyKeySanitizerBody("$.name")),
        new(new BodyKeySanitizerBody("$..displayName")),
        new(new BodyKeySanitizerBody("$..tags.Owners")),
        new(new BodyKeySanitizerBody("$..workspaceId")
        {
            Value = "00000000-0000-0000-0000-000000000000"
        })
    ];

    // Resource IDs in ARM responses can include a user-derived resource group prefix.
    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        new(new()
        {
            Regex = "resource[Gg]roups/[^/\"\\\\?]+",
            Value = "resourceGroups/Sanitized"
        })
    ];

    // Deployment-output variables retain the real rule name while ARM request URIs are sanitized.
    public override List<UriRegexSanitizer> UriRegexSanitizers { get; } =
    [
        new(new UriRegexSanitizerBody
        {
            Regex = "/dataCollectionRules/[^/?]+",
            Value = "/dataCollectionRules/Sanitized"
        })
    ];

    [Fact]
    public async Task Should_Upload_Custom_Logs()
    {
        // Keep upload timestamps within the ingestion window and stable during playback.
        var timestamp = RegisterOrRetrieveVariable(
            "ingestionTimestamp",
            DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));
        var arguments = CreateUploadArguments(ValidLogData.Replace("2026-01-01T00:00:00Z", timestamp, StringComparison.Ordinal));

        var result = await CallToolAsync("monitor_ingestion_upload", arguments);

        Assert.Equal("Succeeded", result.AssertProperty("status").GetString());
        Assert.Equal(2, result.AssertProperty("recordCount").GetInt32());
        Assert.Equal(arguments["data-collection-rule"], result.AssertProperty("dataCollectionRule").GetString());
        Assert.Equal(arguments["stream"], result.AssertProperty("stream").GetString());
    }

    [Theory]
    [InlineData("not json", "not valid JSON")]
    [InlineData("{}", "JSON array")]
    [InlineData("[]", "no records")]
    [InlineData("[1]", "JSON object")]
    public async Task Should_Reject_Invalid_Upload_Payload(string logData, string expectedError)
    {
        var response = await CallToolAsync(
            "monitor_ingestion_upload",
            CreateUploadArguments(logData),
            resultProcessor: element => element);

        AssertError(response, expectedError);
    }

    [Fact]
    public async Task Should_Report_Upload_Service_Error_For_Undeclared_Stream()
    {
        var arguments = CreateUploadArguments(ValidLogData);
        arguments["stream"] = "Custom-Undeclared_CL";

        var response = await CallToolAsync(
            "monitor_ingestion_upload",
            arguments,
            resultProcessor: element => element);

        AssertError(response, "InvalidStream");
    }

    [Theory]
    [InlineData("logSearchBasicTableName")]
    [InlineData("logSearchAuxiliaryTableName")]
    public async Task Should_Validate_Custom_Logs_Against_Stream_Schema(string tableOutput)
    {
        var arguments = CreateValidationArguments(ValidLogData);
        arguments["stream"] = $"Custom-{GetDeploymentOutput(tableOutput)}";

        var result = await CallToolAsync("monitor_ingestion_validate", arguments);

        AssertValidationResult(result, arguments, isValid: true, recordCount: 2);
        Assert.Empty(result.AssertProperty("errors").EnumerateArray());
        Assert.Empty(result.AssertProperty("warnings").EnumerateArray());
    }

    [Fact]
    public async Task Should_Report_Schema_Type_Error()
    {
        var arguments = CreateValidationArguments(
            ValidLogData.Replace("\"Count\": 8", "\"Count\": \"not a number\"", StringComparison.Ordinal));

        var result = await CallToolAsync("monitor_ingestion_validate", arguments);

        AssertValidationResult(result, arguments, isValid: false, recordCount: 2);
        var error = Assert.Single(result.AssertProperty("errors").EnumerateArray());
        Assert.Equal(1, error.AssertProperty("recordIndex").GetInt32());
        Assert.Equal("Count", error.AssertProperty("column").GetString());
        Assert.Contains("long", error.AssertProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Empty(result.AssertProperty("warnings").EnumerateArray());
    }

    [Fact]
    public async Task Should_Warn_About_Undeclared_And_Missing_Columns()
    {
        var arguments = CreateValidationArguments(
            """[{"TimeGenerated":"2026-01-01T00:00:00Z","FixtureId":"mcp-ingestion-warning","Message":"warning fixture","Count":7,"Enabled":true,"ExtraColumn":"dropped"}]""");

        var result = await CallToolAsync("monitor_ingestion_validate", arguments);

        AssertValidationResult(result, arguments, isValid: true, recordCount: 1);
        Assert.Empty(result.AssertProperty("errors").EnumerateArray());
        var warnings = result.AssertProperty("warnings").EnumerateArray().ToArray();
        Assert.Equal(2, warnings.Length);
        var undeclared = Assert.Single(warnings, warning => warning.AssertProperty("column").GetString() == "ExtraColumn");
        Assert.Equal(0, undeclared.AssertProperty("recordIndex").GetInt32());
        Assert.Contains("not declared", undeclared.AssertProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        var missing = Assert.Single(warnings, warning => warning.AssertProperty("column").GetString() == "OptionalValue");
        Assert.Equal(JsonValueKind.Null, missing.AssertProperty("recordIndex").ValueKind);
        Assert.Contains("not present", missing.AssertProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("not json", "not valid JSON")]
    [InlineData("{}", "JSON array")]
    [InlineData("[]", "no records")]
    public async Task Should_Report_Invalid_Validation_Payload(string logData, string expectedError)
    {
        var arguments = CreateValidationArguments(logData);

        var result = await CallToolAsync("monitor_ingestion_validate", arguments);

        AssertValidationResult(result, arguments, isValid: false, recordCount: 0);
        var error = Assert.Single(result.AssertProperty("errors").EnumerateArray());
        Assert.Contains(expectedError, error.AssertProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_Require_Stream_For_Multi_Stream_Rule()
    {
        var arguments = CreateValidationArguments(ValidLogData);
        arguments.Remove("stream");

        var response = await CallToolAsync(
            "monitor_ingestion_validate",
            arguments,
            resultProcessor: element => element);

        AssertError(response, "multiple streams");
        AssertError(response, "--stream");
    }

    [Fact]
    public async Task Should_Reject_Undeclared_Validation_Stream()
    {
        var arguments = CreateValidationArguments(ValidLogData);
        arguments["stream"] = "Custom-Undeclared_CL";

        var response = await CallToolAsync(
            "monitor_ingestion_validate",
            arguments,
            resultProcessor: element => element);

        AssertError(response, "not declared");
        AssertError(response, "Custom-Undeclared_CL");
    }

    private Dictionary<string, object?> CreateUploadArguments(string logData) => new()
    {
        ["ingestion-endpoint"] = GetDeploymentOutput("logSearchIngestionEndpoint"),
        ["data-collection-rule"] = GetDeploymentOutput("logSearchDcrImmutableId"),
        ["stream"] = $"Custom-{GetDeploymentOutput("logSearchBasicTableName")}",
        ["log-data"] = logData,
        ["tenant"] = Settings.TenantId
    };

    private Dictionary<string, object?> CreateValidationArguments(string logData) => new()
    {
        ["subscription"] = Settings.SubscriptionId,
        ["resource-group"] = Settings.ResourceGroupName,
        ["data-collection-rule"] = GetDeploymentOutput("logSearchDcrName"),
        ["stream"] = $"Custom-{GetDeploymentOutput("logSearchBasicTableName")}",
        ["log-data"] = logData,
        ["tenant"] = Settings.TenantId
    };

    private string GetDeploymentOutput(string name) =>
        RegisterOrRetrieveDeploymentOutputVariable(name, name);

    private static void AssertValidationResult(
        JsonElement? result,
        Dictionary<string, object?> arguments,
        bool isValid,
        int recordCount)
    {
        Assert.Equal(isValid, result.AssertProperty("isValid").GetBoolean());
        Assert.True(result.AssertProperty("schemaChecked").GetBoolean());
        Assert.Equal(recordCount, result.AssertProperty("recordCount").GetInt32());
        Assert.Equal(arguments["data-collection-rule"], result.AssertProperty("dataCollectionRule").GetString());
        Assert.Equal(arguments["stream"], result.AssertProperty("stream").GetString());
    }

    private static void AssertError(JsonElement? response, string expectedMessage)
    {
        Assert.True(response.AssertProperty("status").GetInt32() >= 400);
        Assert.Contains(expectedMessage, response.AssertProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
    }
}
