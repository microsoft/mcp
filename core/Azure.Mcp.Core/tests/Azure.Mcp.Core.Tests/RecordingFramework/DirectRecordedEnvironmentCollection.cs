// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace Azure.Mcp.Core.Tests.RecordingFramework;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DirectRecordedEnvironmentCollection
{
    public const string Name = "Direct recorded test environment";
}
