// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ResilienceManagementLiveTestCollection() : ICollectionFixture<ResilienceManagementTestCleanupFixture>
{
    public const string Name = "ResilienceManagementLiveTests";
}