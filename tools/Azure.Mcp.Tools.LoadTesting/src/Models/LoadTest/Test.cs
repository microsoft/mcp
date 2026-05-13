// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;

public class Test
{
    /// <summary>
    /// Gets or sets the environment variables that will be available during test execution.
    /// These variables can be referenced in test scripts and are useful for setting locust tests.
    /// </summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = [];

    /// <summary>
    /// Gets or sets the load test configuration that defines the test execution parameters
    /// such as virtual users, duration, ramp-up time, and target endpoints.
    /// </summary>
    public LoadTestConfiguration LoadTestConfiguration { get; set; } = new();

    /// <summary>
    /// Gets or sets the input artifacts associated with the test, including test scripts,
    /// configuration files, and additional files required for test execution.
    /// </summary>
    public InputArtifacts InputArtifacts { get; set; } = new();

    /// <summary>
    /// Gets or sets the kind of load test. Typically indicates the test type or framework
    /// being used (e.g., "LoadTest" for standard load tests).
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether public IP addresses are disabled for the test.
    /// When true, the test will run without using public IP addresses, providing additional security.
    /// </summary>
    public bool PublicIPDisabled { get; set; }

    /// <summary>
    /// Gets or sets the type of managed identity used for metrics collection.
    /// This determines how the load testing service authenticates to collect and store metrics.
    /// </summary>
    public string MetricsReferenceIdentityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the load test.
    /// This ID is used to reference the test configuration in subsequent operations.
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a detailed description of the load test.
    /// This should explain what the test validates, its purpose, and any specific scenarios it covers.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable display name for the load test.
    /// This name is shown in the Azure portal and should be descriptive and easy to identify.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the test configuration was created.
    /// This timestamp is automatically set by the Azure Load Testing service.
    /// </summary>
    public DateTimeOffset CreatedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user or service principal that created the test configuration.
    /// This is typically an email address or service principal name.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the test configuration was last modified.
    /// This timestamp is automatically updated whenever the test configuration is changed.
    /// </summary>
    public DateTimeOffset LastModifiedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user or service principal that last modified the test configuration.
    /// This is typically an email address or service principal name.
    /// </summary>
    public string LastModifiedBy { get; set; } = string.Empty;
}
