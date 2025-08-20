// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Workbooks.Options;

namespace Azure.Mcp.Tools.Workbooks.Commands;

public abstract class BaseWorkbooksCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : GlobalCommand<T>
    where T : BaseWorkbooksOptions, new();
