// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.BicepSchema.Options;

namespace Azure.Mcp.Tools.BicepSchema.Commands;

public abstract class BaseBicepSchemaCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions> : GlobalCommand<TOptions>
    where TOptions : BaseBicepSchemaOptions, new()
{
    protected readonly Option<string> _resourceTypeName = BicepSchemaOptionDefinitions.ResourceTypeName;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_resourceTypeName);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceType = parseResult.GetValueForOption(_resourceTypeName);
        return options;
    }
}
