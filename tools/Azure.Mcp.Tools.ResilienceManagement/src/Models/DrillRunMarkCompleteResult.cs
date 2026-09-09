// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record DrillRunMarkCompleteResult(string OperationId, bool HasCompleted);
