// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Metadata for custom-cloud Azure Data Explorer data-plane access.
/// </summary>
public sealed record CustomCloudKustoMetadata(string? EndpointSuffix, string? Audience);
