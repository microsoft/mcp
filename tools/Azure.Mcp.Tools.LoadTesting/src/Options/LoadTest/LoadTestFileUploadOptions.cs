// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.LoadTesting.Options.LoadTest;

public class LoadTestFileUploadOptions : BaseLoadTestingOptions
{
    /// <summary>
    /// The ID of the load test.
    /// </summary>
    [JsonPropertyName(OptionDefinitions.LoadTesting.TestId)]
    public string? TestId { get; set; }

    /// <summary>
    /// The file name to upload.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// The local file path to upload.
    /// </summary>
    public string? LocalFilePath { get; set; }

    /// <summary>
    /// The file type (ex: "TEST_SCRIPT" for main test script, "USER_PROPERTIES", "ADDITIONAL_ARTIFACTS", "ZIPPED_ARTIFACTS", "URL_TEST_CONFIG" for supporting resources)
    /// </summary>
    public string? FileType { get; set; }
}
