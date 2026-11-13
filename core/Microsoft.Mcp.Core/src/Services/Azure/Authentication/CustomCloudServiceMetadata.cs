// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Metadata for a custom-cloud service endpoint.
/// </summary>
public sealed record CustomCloudServiceMetadata(string? Endpoint, string? Audience);
