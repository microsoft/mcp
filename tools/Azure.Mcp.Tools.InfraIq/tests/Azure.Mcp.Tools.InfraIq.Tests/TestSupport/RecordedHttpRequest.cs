// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Tests.TestSupport;

internal sealed record RecordedHttpRequest(
    HttpMethod Method,
    Uri? Uri,
    string? AuthorizationScheme,
    string? AuthorizationParameter,
    Dictionary<string, string> Headers,
    string? Body);
