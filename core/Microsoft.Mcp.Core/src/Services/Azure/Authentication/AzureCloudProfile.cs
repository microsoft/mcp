// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager;

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

internal sealed record AzureCloudProfile(
    Uri AuthorityHost,
    ArmEnvironment ArmEnvironment,
    AzureCloudConfiguration.AzureCloud CloudType,
    CloudServiceConfiguration? LogAnalytics,
    KustoCloudConfiguration? Kusto);
