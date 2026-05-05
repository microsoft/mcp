// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Discovery tests that exercise <see cref="OptionSchemaGenerator"/> against every command
/// registered in the real command factory. Catches at build time any option type that the
/// underlying <c>JsonSchemaExporter</c> cannot represent — the same class of failure that
/// otherwise only surfaces at server startup when <c>Tool.InputSchema</c> rejects the value.
/// </summary>
public class InputSchemaDiscoveryTests
{
    [Fact]
    public void AllCommands_ProduceValidInputSchemas()
    {
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        var visibleCommands = Microsoft.Mcp.Core.Commands.CommandFactory
            .GetVisibleCommands(commandFactory.AllCommands)
            .ToList();

        Assert.NotEmpty(visibleCommands);

        var failures = new List<string>();

        foreach (var (commandName, command) in visibleCommands)
        {
            var options = command.GetCommand().Options.ToList();

            JsonObject schema;
            try
            {
                schema = OptionSchemaGenerator.CreateInputSchema(options);
            }
            catch (Exception ex)
            {
                failures.Add($"{commandName}: schema generation threw {ex.GetType().Name}: {ex.Message}");
                continue;
            }

            if ((string?)schema["type"] != "object")
            {
                failures.Add($"{commandName}: root schema 'type' was not 'object'.");
            }

            if (schema["properties"] is not JsonObject)
            {
                failures.Add($"{commandName}: root schema is missing a 'properties' object.");
            }

            if (schema["additionalProperties"] is not JsonValue value || value.GetValue<bool>() != false)
            {
                failures.Add($"{commandName}: root schema must set 'additionalProperties' to false.");
            }
        }

        Assert.True(failures.Count == 0, "One or more commands produced invalid input schemas:\n" + string.Join("\n", failures));
    }
}
