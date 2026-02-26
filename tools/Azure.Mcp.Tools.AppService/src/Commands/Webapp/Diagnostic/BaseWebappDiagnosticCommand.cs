// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;

public abstract class BaseWebappDiagnosticCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(bool diagnosticCategoryRequired)
    : SubscriptionCommand<TOptions>
    where TOptions : BaseWebappDiagnosticOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(AppServiceOptionDefinitions.AppServiceName.AsRequired());
        if (diagnosticCategoryRequired)
        {
            command.Options.Add(AppServiceOptionDefinitions.DiagnosticCategory.AsRequired());
        }
        else
        {
            command.Options.Add(AppServiceOptionDefinitions.DiagnosticCategory);
        }
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.AppName = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.AppServiceName.Name);
        options.DiagnosticCategory = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.DiagnosticCategory.Name);
        return options;
    }
}
