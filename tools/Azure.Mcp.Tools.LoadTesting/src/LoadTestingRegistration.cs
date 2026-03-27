// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.LoadTesting;

public sealed class LoadTestingRegistration : IAreaRegistration
{
    public static string AreaName => "loadtesting";

    public static string AreaTitle => "Azure Load Testing";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Load Testing operations - Commands for managing Azure Load Testing resources, test configurations, and test runs. Includes operations for creating and managing load test resources, configuring test scripts, executing performance tests, and monitoring test results.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "test",
                Description = "Load test operations - Commands for listing, creating and managing Azure load tests.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "2153384b-02ea-47b3-a069-7f5f9a709d66",
                        Name = "create",
                        Description = "Creates a new load test plan or configuration for performance testing scenarios. This command creates a basic URL-based load test that can be used to evaluate the performance and scalability of web applications and APIs. The test configuration defines target endpoint, load parameters, and test duration. Once we create a test plan, we can use that to trigger test runs to test the endpoints set using the 'azmcp loadtesting testrun create' command. This is NOT going to trigger or create any test runs and only will setup your test plan. Also, this is NOT going to create any test resource in azure. It will only create a test in an already existing load test resource.",
                        Title = "Create",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "test-resource-name",
                                Description = "The name of the load test resource for which you want to fetch the details.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "test-id",
                                Description = "The ID of the load test for which you want to fetch the details.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "description",
                                Description = "The description for the load test run. This provides additional context about the test run.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "display-name",
                                Description = "The display name for the load test run. This is a user-friendly name to identify the test run.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "endpoint",
                                Description = "The endpoint URL to be tested. This is the URL of the HTTP endpoint that will be subjected to load testing.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "virtual-users",
                                Description = "Virtual users is a measure of load that is simulated to test the HTTP endpoint. (Default - 50)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "duration",
                                Description = "This is the duration for which the load is simulated against the endpoint. Enter decimals for fractional minutes (e.g., 1.5 for 1 minute and 30 seconds). Default is 20 mins",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "ramp-up-time",
                                Description = "The ramp-up time is the time it takes for the system to ramp-up to the total load specified. Enter decimals for fractional minutes (e.g., 1.5 for 1 minute and 30 seconds). Default is 1 min",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TestResourceCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "be7c3864-0713-42f8-8eb7-b7ca28a951fb",
                        Name = "get",
                        Description = "Get the configuration and setup details for a load test by its test ID in a Load Testing resource. Returns only the test definition, including duration, ramp-up, virtual users, and endpoint. Does not return any test run results or execution data. Also does NOT return and resource details. Only the test configuration is fetched.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "test-resource-name",
                                Description = "The name of the load test resource for which you want to fetch the details.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "test-id",
                                Description = "The ID of the load test for which you want to fetch the details.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TestGetCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "testresource",
                Description = "Load test resource operations - Commands for listing, creating and managing Azure load test resources.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "c39f6e9c-86a7-4cba-b267-0fa71f1ac743",
                        Name = "create",
                        Description = "Returns the created Load Testing resource. This creates the resource in Azure only. It does not create any test plan or test run. Once the resource is setup, you can go and configure test plans in the resource and then trigger test runs for your test plans.",
                        Title = "Create",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "test-resource-name",
                                Description = "The name of the load test resource for which you want to fetch the details.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TestResourceCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "eb44ef6c-93dc-4fa1-949c-a5e8939d5052",
                        Name = "list",
                        Description = "Lists all Azure Load Testing resources available in the selected subscription and resource group. Returns metadata for each resource, including name, location, and status. Use this to discover, manage, or audit load testing resources in your environment. Does not return test plans or test runs.",
                        Title = "List",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "test-resource-name",
                                Description = "The name of the load test resource for which you want to fetch the details.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TestResourceListCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "testrun",
                Description = "Load test run operations - Commands for listing, creating and managing Azure load test runs.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "0e3c8f2c-57ce-49c0-bff4-27c9573e7049",
                        Name = "createorupdate",
                        Description = "Create or update a load test run execution. Creates a new test run for a specified test in the load testing resource, or updates metadata and display properties of an existing test run. When creating: Triggers a new test run execution based on the existing test configuration. Use testrun ID to specify the new run identifier. Create operations are NOT idempotent - each call starts a new test run with unique timestamps and execution state. When updating: Modifies descriptive information (display name, description) of a completed or in-progress test run for better organization and documentation. Update operations are idempotent - repeated calls with same values produce the same result. This does not modify the test plan configuration or create a new test/resource - only manages test run executions.",
                        Title = "Createorupdate",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "test-resource-name",
                                Description = "The name of the load test resource for which you want to fetch the details.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "testrun-id",
                                Description = "The ID of the load test run for which you want to fetch the details.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "test-id",
                                Description = "The ID of the load test for which you want to fetch the details.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "display-name",
                                Description = "The display name for the load test run. This is a user-friendly name to identify the test run.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "description",
                                Description = "The description for the load test run. This provides additional context about the test run.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "old-testrun-id",
                                Description = "The ID of an existing test run to update. If provided, the command will trigger a rerun of the given test run id.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TestRunCreateOrUpdateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "713313ec-b9a5-4a71-9953-5b2d4a7b5d7b",
                        Name = "get",
                        Description = "Get load test run details by testrun ID, or list all test runs by test ID. Returns execution details including status, start/end times, progress, metrics, and artifacts. Does not return test configuration or resource details.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "test-resource-name",
                                Description = "The name of the load test resource for which you want to fetch the details.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "testrun-id",
                                Description = "The ID of the load test run for which you want to fetch the details.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "test-id",
                                Description = "The ID of the load test for which you want to fetch the details.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TestGetCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ILoadTestingService, LoadTestingService>();
        services.AddSingleton<TestResourceListCommand>();
        services.AddSingleton<TestResourceCreateCommand>();
        services.AddSingleton<TestGetCommand>();
        services.AddSingleton<TestCreateCommand>();
        services.AddSingleton<TestRunGetCommand>();
        services.AddSingleton<TestRunCreateOrUpdateCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(TestResourceCreateCommand) => serviceProvider.GetRequiredService<TestResourceCreateCommand>(),
            nameof(TestGetCommand) => serviceProvider.GetRequiredService<TestGetCommand>(),
            nameof(TestResourceListCommand) => serviceProvider.GetRequiredService<TestResourceListCommand>(),
            nameof(TestRunCreateOrUpdateCommand) => serviceProvider.GetRequiredService<TestRunCreateOrUpdateCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in loadtesting area.")
        };
}
