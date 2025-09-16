// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.LoadTesting;

public class LoadTestingSetup : IAreaSetup
{
    public string Name => "loadtesting";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILoadTestingService, LoadTestingService>();

        services.AddSingleton<TestResourceListCommand>();
        services.AddSingleton<TestResourceCreateCommand>();

        services.AddSingleton<TestGetCommand>();
        services.AddSingleton<TestCreateCommand>();

        services.AddSingleton<TestRunGetCommand>();
        services.AddSingleton<TestRunListCommand>();
        services.AddSingleton<TestRunCreateCommand>();
        services.AddSingleton<TestRunUpdateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Load Testing command group
        var service = new CommandGroup(
            Name,
            "Load Testing operations - Commands for managing Azure Load Testing resources, test configurations, and test runs. Includes operations for creating and managing load test resources, configuring test scripts, executing performance tests, and monitoring test results.");

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
        testResource.AddCommand("list", serviceProvider.GetRequiredService<TestResourceListCommand>());
        testResource.AddCommand("create", serviceProvider.GetRequiredService<TestResourceCreateCommand>());

        // Register commands for Load Test
        test.AddCommand("get", serviceProvider.GetRequiredService<TestGetCommand>());
        test.AddCommand("create", serviceProvider.GetRequiredService<TestCreateCommand>());

        // Register commands for Load Test Run
        testRun.AddCommand("get", serviceProvider.GetRequiredService<TestRunGetCommand>());
        testRun.AddCommand("list", serviceProvider.GetRequiredService<TestRunListCommand>());
        testRun.AddCommand("create", serviceProvider.GetRequiredService<TestRunCreateCommand>());
        testRun.AddCommand("update", serviceProvider.GetRequiredService<TestRunUpdateCommand>());

        return service;
    }
}
