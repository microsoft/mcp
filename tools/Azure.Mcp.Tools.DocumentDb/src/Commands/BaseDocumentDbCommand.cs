// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.DocumentDb.Options;

namespace Azure.Mcp.Tools.DocumentDb.Commands;

public abstract class BaseDocumentDbCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : GlobalCommand<TOptions> where TOptions : BaseDocumentDbOptions, new()
{
}
