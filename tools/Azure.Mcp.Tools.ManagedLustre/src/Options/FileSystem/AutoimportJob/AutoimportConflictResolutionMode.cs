// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoimportJob;

public enum AutoimportConflictResolutionMode
{
    Fail,
    Skip,
    OverwriteIfDirty,
    OverwriteAlways,
}
