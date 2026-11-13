// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record CreateFolderRequest(string DisplayName, Guid? ParentFolderId = null);
