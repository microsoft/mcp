// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;

namespace Azure.Mcp.Tools.Postgres.Tests.Services.Support;

internal static class PostgresTestAzureServiceExtensions
{
    internal static IAzureCloudConfiguration ConfigureCloud(
        this IAzureService azureService,
        ArmEnvironment armEnvironment)
    {
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(armEnvironment);
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        return cloudConfiguration;
    }
}
