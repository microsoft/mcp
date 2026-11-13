// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Validation;

/// <summary>Fixed input guidance that never echoes the rejected path, query, or body.</summary>
internal sealed class ApiRequestInputException(string message) : Exception(message);
