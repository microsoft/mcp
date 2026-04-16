// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.BicepSchema.Commands;
using Azure.Mcp.Tools.BicepSchema.Services;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.BicepSchema.UnitTests;

public class BicepSchemaGetCommandTests : CommandUnitTestsBase<BicepSchemaGetCommand, IBicepSchemaService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsSchema_WhenResourceTypeExists()
    {
        var response = await ExecuteCommandAsync("--resource-type", "Microsoft.Sql/servers/databases/schemas");
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, BicepSchemaJsonContext.Default.BicepSchemaGetCommandResult);
        var name = result?.BicepSchemaResult.FirstOrDefault()?.Name;

        Assert.Contains("Microsoft.Sql/servers/databases/schemas@2023-08-01", name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenResourceTypeDoesNotExist()
    {
        var response = await ExecuteCommandAsync("--resource-type", "Microsoft.Unknown/virtualRandom");
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        Assert.Contains("Resource type Microsoft.Unknown/virtualRandom not found.", response.Message);
    }
}
