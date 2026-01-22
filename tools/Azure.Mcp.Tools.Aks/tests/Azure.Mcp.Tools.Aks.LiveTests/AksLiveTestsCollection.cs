// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace Azure.Mcp.Tools.Aks.LiveTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class AksLiveTestsCollection
{
    public const string Name = "Azure.Mcp.Tools.Aks.LiveTests.SerialCollection";
}
