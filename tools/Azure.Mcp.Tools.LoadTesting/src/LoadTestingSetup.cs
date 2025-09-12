// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.LoadTesting;

public class LoadTestingSetup : IAreaSetup
{
    public string Name => "loadtesting";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILoadTestingService, LoadTestingService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Load Testing command group
        var service = new CommandGroup(
            Name,
            "Load Testing operations - Commands for managing Azure Load Testing resources, test configurations, and test runs. Includes operations for creating and managing load test resources, configuring test scripts, executing performance tests, and monitoring test results.");
        rootGroup.AddSubGroup(service);

        // Create Load Test subgroups
        var testResource = new CommandGroup(
            "testresource",
            "Load test resource operations - Commands for listing, creating and managing Azure load test resources.");
        service.AddSubGroup(testResource);

        var test = new CommandGroup(
            "test",
            "Load test operations - Commands for listing, creating and managing Azure load tests.");
        service.AddSubGroup(test);

        var testRun = new CommandGroup(
            "testrun",
            "Load test run operations - Commands for listing, creating and managing Azure load test runs.");
        service.AddSubGroup(testRun);

        // Register commands for Load Test Resource
        testResource.AddCommand("list", new TestResourceListCommand(loggerFactory.CreateLogger<TestResourceListCommand>()));
        testResource.AddCommand("create", new TestResourceCreateCommand(loggerFactory.CreateLogger<TestResourceCreateCommand>()));

        // Register commands for Load Test
        test.AddCommand("get", new TestGetCommand(loggerFactory.CreateLogger<TestGetCommand>()));
        test.AddCommand("create", new TestCreateCommand(loggerFactory.CreateLogger<TestCreateCommand>()));

        // Register commands for Load Test Run
        testRun.AddCommand("get", new TestRunGetCommand(loggerFactory.CreateLogger<TestRunGetCommand>()));
        testRun.AddCommand("list", new TestRunListCommand(loggerFactory.CreateLogger<TestRunListCommand>()));
        testRun.AddCommand("create", new TestRunCreateCommand(loggerFactory.CreateLogger<TestRunCreateCommand>()));
        testRun.AddCommand("update", new TestRunUpdateCommand(loggerFactory.CreateLogger<TestRunUpdateCommand>()));
    }
}
