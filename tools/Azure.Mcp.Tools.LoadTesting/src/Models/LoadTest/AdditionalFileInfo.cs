// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
public class AdditionalFileInfo
{
    /// <summary>
    /// Gets or sets the URL where the additional file can be accessed or downloaded.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the additional file including its extension.
    /// This should be the original filename as uploaded to the load testing service.
    /// </summary>
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type or category of the additional file.
    /// This indicates how the file will be used during test execution.
    /// </summary>
    [JsonPropertyName("fileType")]
    public string? FileType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the file URL will expire.
    /// After this time, the URL will no longer be accessible. 
    /// </summary>
    [JsonPropertyName("expireDateTime")]
    public DateTimeOffset? ExpireDateTime { get; set; }

    /// <summary>
    /// Gets or sets the validation status of the additional file.
    /// This indicates whether the file has been successfully validated by the load testing service.\
    /// </summary>
    [JsonPropertyName("validationStatus")]
    public string? ValidationStatus { get; set; } = string.Empty;
}
