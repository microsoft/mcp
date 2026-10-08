// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Core.Commands;

public abstract class FabricCoreCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>()
    : AuthenticatedCommand<TOptions, TResult>
    where TOptions : class
{
    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        context.Response.Results = null;
    }

    // Every operation must supply safe guidance rather than inherit raw exception messages.
    protected abstract override string GetErrorMessage(Exception ex);

    protected string GetValidationErrorMessage(CommandValidationException exception, string fallback)
    {
        if (exception.MissingOptions is not { Count: > 0 } missingOptions)
        {
            return fallback;
        }

        var knownOptions = GetCommand().Options
            .Where(option => option.Required && missingOptions.Contains(option.Name, StringComparer.Ordinal))
            .Select(static option => option.Name)
            .ToArray();

        return knownOptions.Length > 0
            ? $"Missing Required options: {string.Join(", ", knownOptions)}"
            : fallback;
    }
}
