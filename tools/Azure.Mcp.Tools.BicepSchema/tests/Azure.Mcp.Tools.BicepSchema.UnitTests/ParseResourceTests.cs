// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.BicepSchema.Services.Support;
using Xunit;

namespace Azure.Mcp.Tools.BicepSchema.UnitTests;

public class ParseResourceTests
{
    [Theory]
    [InlineData("Microsoft.Compute/virtualMachines", "Microsoft.Compute", "virtualMachines", null)]
    [InlineData("Microsoft.Compute.Better/virtualMachines/virtualCpus", "Microsoft.Compute.Better", "virtualMachines/virtualCpus", null)]
    [InlineData("Microsoft.Compute.Better/virtualMachines/virtualCpus/brains", "Microsoft.Compute.Better", "virtualMachines/virtualCpus/brains", null)]
    [InlineData("Microsoft.Compute/virtualMachines@2024-10-10", "Microsoft.Compute", "virtualMachines", "2024-10-10")]
    [InlineData("Microsoft.Compute.Better/virtualMachines/virtualCpus/brains@2024-10-10-preview", "Microsoft.Compute.Better", "virtualMachines/virtualCpus/brains", "2024-10-10-preview")]
    public void ParseResourceType(string resourceType, string expectedProvider, string expectedName, string? expectedApiVersion)
    {
        (string provider, string resourceName, string? apiVersion) = ResourceParser.ParseResourceType(resourceType);

        Assert.Equal(expectedProvider, provider);
        Assert.Equal(expectedName, resourceName);
        Assert.Equal(expectedApiVersion, apiVersion);
    }
}
