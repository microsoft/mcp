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

        var testResourceList = serviceProvider.GetRequiredService<TestResourceListCommand>();
        testResource.AddCommand(testResourceList.Name, testResourceList);
        var testResourceCreate = serviceProvider.GetRequiredService<TestResourceCreateCommand>();
        testResource.AddCommand(testResourceCreate.Name, testResourceCreate);

        // Register commands for Load Test
        var testGet = serviceProvider.GetRequiredService<TestGetCommand>();
        test.AddCommand(testGet.Name, testGet);
        var testCreate = serviceProvider.GetRequiredService<TestCreateCommand>();
        test.AddCommand(testCreate.Name, testCreate);

        // Register commands for Load Test Run
        var testRunGet = serviceProvider.GetRequiredService<TestRunGetCommand>();
        testRun.AddCommand(testRunGet.Name, testRunGet);
        var testRunList = serviceProvider.GetRequiredService<TestRunListCommand>();
        testRun.AddCommand(testRunList.Name, testRunList);
        var testRunCreate = serviceProvider.GetRequiredService<TestRunCreateCommand>();
        testRun.AddCommand(testRunCreate.Name, testRunCreate);
        var testRunUpdate = serviceProvider.GetRequiredService<TestRunUpdateCommand>();
        testRun.AddCommand(testRunUpdate.Name, testRunUpdate);

        return service;
    }
}
