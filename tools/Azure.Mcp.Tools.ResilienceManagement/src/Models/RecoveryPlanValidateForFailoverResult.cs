// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanValidateForFailoverResult(
    string OperationId,
    IReadOnlyList<RecoveryPlanFailoverQualification> RecoveryResourceQualifications);
