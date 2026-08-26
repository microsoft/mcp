// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Mcp.Core.Options;

internal interface IOptionContainerMetadata
{
    string? Prefix { get; }

    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    Type ContainerType { get; }
}
