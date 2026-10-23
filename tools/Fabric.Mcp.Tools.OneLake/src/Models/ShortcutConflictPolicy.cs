// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.OneLake.Models;

public enum ShortcutConflictPolicy
{
    Abort,
    CreateOrOverwrite,
    OverwriteOnly,
    GenerateUniqueName
}
