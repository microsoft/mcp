// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Services;

/// <summary>Why a Fabric or Power BI REST request did not produce a usable JSON response.</summary>
internal enum ApiRequestFailure
{
    ServiceError,
    Redirect,
    UnsupportedMediaType,
    ResponseTooLarge,
    InvalidJson
}
