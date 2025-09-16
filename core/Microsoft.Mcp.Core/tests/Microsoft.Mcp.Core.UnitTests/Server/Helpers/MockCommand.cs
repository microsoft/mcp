// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.UnitTests.Server.Helpers;

public class MockCommand : IBaseCommand
{
    private readonly Command _command;

    public MockCommand(string name, bool isReadonly = false)
    {
        Name = name;
        Description = $"{name}_description";
        Title = $"Title_{Description}";
        Metadata = new ToolMetadata { ReadOnly = isReadonly };
        _command = new Command(Name, Description);
    }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Title { get; set; }

    public ToolMetadata Metadata { get; set; }

    public void AddOption(string optionName)
    {
        var option = new Option<string>(optionName);
        option.Description = $"{optionName}_description";

        _command.Options.Add(option);
    }

    public Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        return Task.FromResult(new CommandResponse() { Status = 200 });
    }

    public Command GetCommand() => _command;

    public ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        throw new NotImplementedException();
    }
}

[HiddenCommand]
public class MockHiddenCommand : MockCommand
{
    public MockHiddenCommand(string name, bool isReadonly = false)
        : base(name, isReadonly)
    {
    }
}
