// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record FabricOperationIssue(string Code, string Message, int? HttpStatus = null);
