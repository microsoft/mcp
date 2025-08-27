// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using NSubstitute;
using NSubstitute.Core;
using Microsoft.Mcp.Core.Models.Command;
using System.CommandLine.Parsing;
using System.CommandLine;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server;

internal class CommandFactoryHelpers
{
    //public static ICommandFactory CreateCommandFactory(IServiceProvider? serviceProvider = default)
    //{
    //    IServiceProvider services = serviceProvider ?? new ServiceCollection()
    //        .AddLogging()
    //        .BuildServiceProvider();

    //    var factory = Substitute.For<ICommandFactory>();

    //    factory.AllCommands.Returns(new Di)

    //    var logger = services.GetRequiredService<ILogger<ICommandFactory>>();
    //    var telemetryService = services.GetService<ITelemetryService>() ?? new NoOpTelemetryService();


    //    var commandFactory = new CommandFactory(services, areaSetups, telemetryService, logger);

    //    return commandFactory;
    //}

    //public class MockCommand : IBaseCommand
    //{
    //    public MockCommand(string name, string description)
    //    {
    //        Name = name;
    //        Description = description;
    //        Title = $"{name}_{description}";
    //        Metadata = new ToolMetadata();
    //    }

    //    public string Name { get; set; }

    //    public string Description { get; set; }

    //    public string Title { get; set; }

    //    public ToolMetadata Metadata { get; set; }

    //    public Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    //    {

    //    }

    //    public Command GetCommand()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class NoOpTelemetryService : ITelemetryService
    {
        public ValueTask<Activity?> StartActivity(string activityName)
        {
            return ValueTask.FromResult<Activity?>(null);
        }

        public ValueTask<Activity?> StartActivity(string activityName, Implementation? clientInfo)
        {
            return ValueTask.FromResult<Activity?>(null);
        }

        public void Dispose()
        {
        }
    }
}
