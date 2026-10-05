// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.Error;

/// <summary>
/// Parsed ARM OData error. Nested details and additionalInfo are intentionally not modeled.
/// </summary>
public sealed class InfraIqArmODataError
{
    public string? Code { get; set; }

    public string? Message { get; set; }

    public string? Target { get; set; }
}
