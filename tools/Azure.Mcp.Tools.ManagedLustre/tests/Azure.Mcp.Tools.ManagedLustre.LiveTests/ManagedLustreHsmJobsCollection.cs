// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace Azure.Mcp.Tools.ManagedLustre.LiveTests;

/// <summary>
/// Collection definition to ensure HSM job tests run sequentially.
/// Autoimport and autoexport jobs cannot run simultaneously on the same filesystem.
/// </summary>
[CollectionDefinition("ManagedLustre HSM Jobs", DisableParallelization = true)]
public class ManagedLustreHsmJobsCollection
{
}
