// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words Retriable

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Safe failure metadata, excluding backend messages, parameters, resources, and nested details.</summary>
public sealed record FabricJobFailure(string? ErrorCode = null, Guid? RequestId = null, bool? IsRetriable = null);
