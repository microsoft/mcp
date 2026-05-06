// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Sql.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Sql.Commands;

public abstract class BaseElasticPoolCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(ILogger<BaseSqlCommand<TOptions, TResult>> logger)
    : BaseSqlCommand<TOptions, TResult>(logger) where TOptions : BaseElasticPoolOptions, new()
{
}
