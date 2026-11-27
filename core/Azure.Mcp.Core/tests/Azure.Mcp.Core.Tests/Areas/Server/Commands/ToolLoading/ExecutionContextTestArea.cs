// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

internal sealed class ExecutionContextTestArea(
    string name,
    IBaseCommand command,
    CommandRegistration? registration = null) : IAreaSetup
{
    public string Name => name;
    public string Title => name;

    public void ConfigureServices(IServiceCollection services)
    {
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var group = new CommandGroup(name, "Execution context test tools");
        if (registration is null)
        {
            group.AddCommand(command);
        }
        else
        {
            group.AddCommand(command.Name, registration);
        }
        return group;
    }
}
