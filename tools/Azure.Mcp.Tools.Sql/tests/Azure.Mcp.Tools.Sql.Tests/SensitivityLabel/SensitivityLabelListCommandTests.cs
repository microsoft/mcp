// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Sql.Commands;
using Azure.Mcp.Tools.Sql.Commands.SensitivityLabel;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Mcp.Core.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.Tests.SensitivityLabel;

public class SensitivityLabelListCommandTests : SubscriptionCommandUnitTestsBase<SensitivityLabelListCommand, ISqlService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server server --database db", true)]
    [InlineData("--subscription sub --resource-group rg --server server", false)] // Missing database
    [InlineData("--subscription sub --resource-group rg --database db", false)] // Missing server
    [InlineData("--subscription sub --server server --database db", false)] // Missing resource group
    [InlineData("--resource-group rg --server server --database db", false)] // Missing subscription
    [InlineData("", false)] // Missing all required parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.ListSensitivityLabelsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSensitivityLabelsSuccessfully()
    {
        // Arrange
        var labels = new List<SqlDatabaseSensitivityLabel>
        {
            new("dbo", "Customers", "Email", "Confidential - GDPR", "331f0b13-76b5-2f1b-a77b-def5a73c73c2",
                "Contact Info", "5c503e21-22c6-81fa-620b-f369b8ec38d1", "Medium"),
            new("dbo", "Customers", "CreditCard", "Highly Confidential", "1866ca45-1973-4c28-9d12-04d407f147ad",
                "Credit Card", "d22fa6e9-5ee4-3bde-4c2b-a409604c4646", "Critical")
        };

        Service.ListSensitivityLabelsAsync(
            "testserver",
            "testdb",
            "testrg",
            "testsub",
            Arg.Any<CancellationToken>())
            .Returns(labels);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--database", "testdb");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        var result = ValidateAndDeserializeResponse(response, SqlJsonContext.Default.SensitivityLabelListResult);
        Assert.Equal(2, result.SensitivityLabels.Count);
        Assert.Equal("Email", result.SensitivityLabels[0].ColumnName);
        Assert.Equal("Confidential - GDPR", result.SensitivityLabels[0].LabelName);
        Assert.Equal("Critical", result.SensitivityLabels[1].Rank);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyListWhenNoLabels()
    {
        // Arrange
        Service.ListSensitivityLabelsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--database", "testdb");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListSensitivityLabelsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--database", "testdb");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404Error()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.NotFound, "Database not found");
        Service.ListSensitivityLabelsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(requestException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--database", "testdb");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("SQL database or server not found", response.Message);
        Assert.Null(response.Results);
        Assert.DoesNotContain("Database not found", JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse));
    }

    [Fact]
    public async Task ExecuteAsync_Handles403ErrorWithoutBackendDetails()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.Forbidden, "secret-backend-detail");
        Service.ListSensitivityLabelsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(requestException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--database", "testdb");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.DoesNotContain("secret-backend-detail", response.Message);
        Assert.Null(response.Results);
        Assert.DoesNotContain("secret-backend-detail", JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse));
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        Service.ListSensitivityLabelsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await ExecuteCommandAsync(
            "--subscription", "testsub",
            "--resource-group", "testrg",
            "--server", "testserver",
            "--database", "testdb");

        // Assert
        await Service.Received(1).ListSensitivityLabelsAsync(
            "testserver",
            "testdb",
            "testrg",
            "testsub",
            Arg.Any<CancellationToken>());
    }
}
