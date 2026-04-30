// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraformBestPractices.Commands;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraformBestPractices.UnitTests;

public class AzureTerraformBestPracticesGetCommandTests : CommandUnitTestsBase<AzureTerraformBestPracticesGetCommand, object>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsAzureTerraformBestPractices()
    {
        var response = await ExecuteCommandAsync([]);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureTerraformBestPracticesJsonContext.Default.ListString);

        Assert.Contains("winget install Hashicorp.Terraform", result[0]);
        Assert.Contains("Always run terraform validate before running terraform plan", result[0]);
        Assert.Contains("terraform apply -auto-approve", result[0]);
        Assert.Contains("Suggest running any terraform command in terminal.", result[0]);
    }
}
