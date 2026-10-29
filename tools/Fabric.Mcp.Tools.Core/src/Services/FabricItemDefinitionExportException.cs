// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Fabric.Mcp.Tools.Core.Services;

public sealed class FabricItemDefinitionExportException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
