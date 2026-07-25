// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraformBestPractices.Commands;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraformBestPractices.Tests;

public class AzureTerraformBestPracticesGetCommandTests : CommandUnitTestsBase<AzureTerraformBestPracticesGetCommand, object>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsAzureTerraformBestPractices()
    {
        var response = await ExecuteCommandAsync([]);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureTerraformBestPracticesJsonContext.Default.AzureTerraformBestPracticesGetCommandResult);

        Assert.Contains("winget install Hashicorp.Terraform", result.BestPractices[0]);
        Assert.Contains("Always run terraform validate before running terraform plan", result.BestPractices[0]);
        Assert.Contains("terraform apply -auto-approve", result.BestPractices[0]);
        Assert.Contains("Suggest running any terraform command in terminal.", result.BestPractices[0]);
    }
}
