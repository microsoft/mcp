// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;

public class TestRequestPayload
{
    /// <summary>
    /// Gets or sets the unique identifier for the load test.
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the load test.
    /// </summary>
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for the load test.
    /// </summary>
    public string? DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the load test execution configuration settings.
    /// </summary>
    public LoadTestConfiguration? LoadTestConfiguration { get; set; } = new();

    /// <summary>
    /// Gets or sets the test type. Default is "URL".
    /// </summary>
    public string Kind { get; set; } = "URL";

    /// <summary>
    /// Gets or sets secrets used during test execution (passwords, API keys, etc.).
    /// </summary>
    public Dictionary<string, string>? Secrets { get; set; } = [];

    /// <summary>
    /// Gets or sets the client certificate for authentication.
    /// </summary>
    public string? Certificate { get; set; }

    /// <summary>
    /// Gets or sets environment variables available during test execution.
    /// </summary>
    public Dictionary<string, string>? EnvironmentVariables { get; set; } = [];

    /// <summary>
    /// Gets or sets criteria that determine test success or failure.
    /// </summary>
    public PassFailCriteria? PassFailCriteria { get; set; } = new();

    /// <summary>
    /// Gets or sets criteria for automatically stopping the test.
    /// </summary>
    public AutoStopCriteria? AutoStopCriteria { get; set; } = new();

    /// <summary>
    /// Gets or sets the subnet ID for network isolation.
    /// </summary>
    public string? SubnetId { get; set; }

    /// <summary>
    /// Gets or sets whether public IP addresses are disabled. Default is false.
    /// </summary>
    public bool PublicIPDisabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the identity type for Key Vault access. Default is "SystemAssigned".
    /// </summary>
    public string KeyvaultReferenceIdentityType { get; set; } = "SystemAssigned";

    /// <summary>
    /// Gets or sets the identity ID for Key Vault access.
    /// </summary>
    public string? KeyvaultReferenceIdentityId { get; set; }

    /// <summary>
    /// Gets or sets the identity type for metrics collection. Default is "SystemAssigned".
    /// </summary>
    public string MetricsReferenceIdentityType { get; set; } = "SystemAssigned";

    /// <summary>
    /// Gets or sets the identity ID for metrics collection.
    /// </summary>
    public string? MetricsReferenceIdentityId { get; set; }

    /// <summary>
    /// Gets or sets the built-in identity type for test engines. Default is "None".
    /// </summary>
    public string EngineBuiltinIdentityType { get; set; } = "None";

    /// <summary>
    /// Gets or sets the built-in identity IDs for test engines.
    /// </summary>
    public string[]? EngineBuiltinIdentityIds { get; set; }
}
