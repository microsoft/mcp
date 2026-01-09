// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Options;

public static class LoadTestingOptionDefinitions
{
    public const string TestResourceName = "test-resource-name";
    public const string TestRunId = "testrun-id";
    public const string TestId = "test-id";
    public const string DisplayNameOption = "display-name";
    public const string DescriptionOption = "description";
    public const string OldTestRunIdOption = "old-testrun-id";
    public const string VirtualUsersOption = "virtual-users";
    public const string DurationOption = "duration";
    public const string RampUpTimeOption = "ramp-up-time";
    public const string EndpointOption = "endpoint";

    public static readonly Option<string> TestResource = new($"--{TestResourceName}")
    {
        Description = "The name of the load test resource for which you want to fetch the details.",
        Required = false
    };

    public static readonly Option<string> TestRun = new($"--{TestRunId}")
    {
        Description = "The ID of the load test run for which you want to fetch the details.",
        Required = false
    };

    public static readonly Option<string> Test = new($"--{TestId}")
    {
        Description = "The ID of the load test for which you want to fetch the details.",
        Required = true
    };

    public static readonly Option<string> DisplayName = new($"--{DisplayNameOption}")
    {
        Description = "The display name for the load test run. This is a user-friendly name to identify the test run.",
        Required = false
    };

    public static readonly Option<string> Description = new($"--{DescriptionOption}")
    {
        Description = "The description for the load test run. This provides additional context about the test run.",
        Required = false
    };

    public static readonly Option<string> OldTestRunId = new($"--{OldTestRunIdOption}")
    {
        Description = "The ID of an existing test run to update. If provided, the command will trigger a rerun of the given test run id.",
        Required = false
    };

    public static readonly Option<int> VirtualUsers = new($"--{VirtualUsersOption}")
    {
        Description = "Virtual users is a measure of load that is simulated to test the HTTP endpoint. (Default - 50)",
        Required = false
    };

    public static readonly Option<int> Duration = new($"--{DurationOption}")
    {
        Description = "This is the duration for which the load is simulated against the endpoint. Enter decimals for fractional minutes (e.g., 1.5 for 1 minute and 30 seconds). Default is 20 mins",
        Required = false
    };

    public static readonly Option<int> RampUpTime = new($"--{RampUpTimeOption}")
    {
        Description = "The ramp-up time is the time it takes for the system to ramp-up to the total load specified. Enter decimals for fractional minutes (e.g., 1.5 for 1 minute and 30 seconds). Default is 1 min",
        Required = false
    };

    public static readonly Option<string> Endpoint = new($"--{EndpointOption}")
    {
        Description = "The endpoint URL to be tested. This is the URL of the HTTP endpoint that will be subjected to load testing.",
        Required = false
    };
}
