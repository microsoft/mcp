// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.BicepSchema.Options;

namespace Azure.Mcp.Tools.BicepSchema.Commands;

public abstract class BaseBicepSchemaCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions> : GlobalCommand<TOptions>
    where TOptions : BaseBicepSchemaOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(BicepSchemaOptionDefinitions.ResourceTypeName);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceType = parseResult.GetValueOrDefault<string>(BicepSchemaOptionDefinitions.ResourceTypeName.Name);
        return options;
    }
}
