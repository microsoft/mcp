// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Options;

internal sealed class AcquireTokenConfiguration()
{
    public string? AuthenticationOptionsName { get; set; }
    public string? Claims { get; set; }
    public Guid? CorrelationId { get; set; }
    public Dictionary<string, string>? ExtraHeadersParameters { get; set; }
    public Dictionary<string, object>? ExtraParameters { get; set; }
    public Dictionary<string, string>? ExtraQueryParameters { get; set; }
    public string? FmiPath { get; set; }
    public bool? ForceRefresh { get; set; }
    public string? LongRunningWebApiSessionKey { get; set; }
    public ManagedIdentityConfiguration? ManagedIdentity { get; set; }
    public string? PopClaim { get; set; }
    public string? PopPublicKey { get; set; }
    public string? Tenant { get; set; }
    public string? UserFlow { get; set; }
}
