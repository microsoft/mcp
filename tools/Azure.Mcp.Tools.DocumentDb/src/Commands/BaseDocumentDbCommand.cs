// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.DocumentDb.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.DocumentDb.Commands;

public abstract class BaseDocumentDbCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseDocumentDbCommand<TOptions>> logger)
    : GlobalCommand<TOptions> where TOptions : BaseDocumentDbOptions, new()
{
    protected readonly ILogger<BaseDocumentDbCommand<TOptions>> _logger = logger;
}
