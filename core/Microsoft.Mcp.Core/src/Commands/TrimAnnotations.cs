// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Azure.Mcp.Core.Commands;

public static class TrimAnnotations
{
    public const DynamicallyAccessedMemberTypes CommandAnnotations =
        DynamicallyAccessedMemberTypes.PublicProperties
        | DynamicallyAccessedMemberTypes.NonPublicProperties;
}
