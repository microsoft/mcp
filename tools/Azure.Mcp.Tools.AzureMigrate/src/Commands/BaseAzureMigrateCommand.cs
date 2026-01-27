// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureMigrate.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Commands;

/// <summary>
/// Base command for all Azure Migrate commands.
/// </summary>
public abstract class BaseAzureMigrateCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : GlobalCommand<TOptions> where TOptions : BaseAzureMigrateOptions, new();
