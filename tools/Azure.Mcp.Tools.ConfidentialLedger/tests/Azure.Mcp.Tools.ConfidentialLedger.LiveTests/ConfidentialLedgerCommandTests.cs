// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.ConfidentialLedger.LiveTests;

public class ConfidentialLedgerCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_append_entry_successfully()
    {
        // Arrange
        var ledgerName = Settings.DeploymentOutputs["CONFIDENTIAL_LEDGER_NAME"];
        var testContent = $$"""
            {
                "message": "Live test entry",
                "timestamp": "{{DateTime.UtcNow:o}}",
                "testRun": "{{Guid.NewGuid()}}"
            }
            """;

        // Act
        var result = await CallToolAsync(
            "azmcp_confidentialledger_entries_append",
            new()
            {
                { "ledger", ledgerName },
                { "content", testContent }
            });

        // Assert
        Assert.NotNull(result);

        var transactionId = result.Value.AssertProperty("transactionId");
        Assert.Equal(JsonValueKind.String, transactionId.ValueKind);
        Assert.NotNull(transactionId.GetString());
        Assert.NotEmpty(transactionId.GetString()!);

        // Transaction ID should be in format "major.minor" (e.g., "2.199")
        var transactionIdStr = transactionId.GetString();
        Assert.Contains(".", transactionIdStr);

        var state = result.Value.AssertProperty("state");
        Assert.Equal(JsonValueKind.String, state.ValueKind);
        Assert.Equal("Committed", state.GetString());

        Output.WriteLine($"Successfully appended entry with transaction ID: {transactionIdStr}");
    }

    [Fact]
    public async Task Should_append_entry_with_collection_id()
    {
        // Arrange
        var ledgerName = Settings.DeploymentOutputs["CONFIDENTIAL_LEDGER_NAME"];
        var collectionId = "test-collection";
        var testContent = $$"""
            {
                "collectionTest": true,
                "timestamp": "{{DateTime.UtcNow:o}}"
            }
            """;

        // Act
        var result = await CallToolAsync(
            "azmcp_confidentialledger_entries_append",
            new()
            {
                { "ledger", ledgerName },
                { "content", testContent },
                { "collectionId", collectionId }
            });

        // Assert
        Assert.NotNull(result);

        var transactionId = result.Value.AssertProperty("transactionId");
        Assert.Equal(JsonValueKind.String, transactionId.ValueKind);
        Assert.NotNull(transactionId.GetString());
        Assert.NotEmpty(transactionId.GetString()!);

        var state = result.Value.AssertProperty("state");
        Assert.Equal("Committed", state.GetString());

        Output.WriteLine($"Successfully appended entry to collection '{collectionId}' with transaction ID: {transactionId.GetString()}");
    }
}
