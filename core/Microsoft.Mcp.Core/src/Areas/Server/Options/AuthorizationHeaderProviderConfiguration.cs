// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Options;

internal sealed class AuthorizationHeaderProviderConfiguration()
{
    public AcquireTokenConfiguration? AcquireTokenOptions { get; set; }
    public string? BaseUrl { get; set; }
    public string? HttpMethod { get; set; }
    public string? ProtocolScheme { get; set; }
    public string? RelativePath { get; set; }
    public bool? RequestAppToken { get; set; }
}
