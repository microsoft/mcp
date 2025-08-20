// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Sql.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands;

public abstract class BaseElasticPoolCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseSqlCommand<TOptions>> logger)
    : BaseSqlCommand<TOptions>(logger) where TOptions : BaseElasticPoolOptions, new()
{
}
