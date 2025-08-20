// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
public class TestScriptFileInfo
{
    /// <summary>
    /// Gets or sets the URL where the test script file can be accessed.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the test script file.
    /// </summary>
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the test script file (e.g., "JMX_FILE", "USER_PROPERTIES").
    /// </summary>
    [JsonPropertyName("fileType")]
    public string? FileType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the file URL expires.
    /// </summary>
    [JsonPropertyName("expireDateTime")]
    public DateTimeOffset? ExpireDateTime { get; set; }

    /// <summary>
    /// Gets or sets the validation status of the test script file.
    /// </summary>
    [JsonPropertyName("validationStatus")]
    public string? ValidationStatus { get; set; } = string.Empty;
}
