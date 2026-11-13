// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options;

public abstract class ResilienceManagementOperationOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.OperationId)]
    public string? OperationId { get; set; }
}
