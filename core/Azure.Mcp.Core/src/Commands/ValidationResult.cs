// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Commands;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
