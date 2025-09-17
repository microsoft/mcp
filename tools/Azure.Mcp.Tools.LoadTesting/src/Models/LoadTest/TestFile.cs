// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest
{
    public class TestFile
    {
        /// <summary>
        /// Gets or sets the name of the test file.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the type of the test file (e.g., "TEST_SCRIPT", "USER_PROPERTIES", "ADDITIONAL_ARTIFACTS", "ZIPPED_ARTIFACTS", "URL_TEST_CONFIG").
        /// </summary>
        public string? FileType { get; set; }

        /// <summary>
        /// Gets or sets the size of the test file in bytes.
        /// </summary>
        public long? FileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the test file was uploaded.
        /// </summary>
        public DateTimeOffset? UploadedAt { get; set; }

        /// <summary>
        /// Gets or sets the validation status of the test file.
        /// </summary>
        public string? ValidationStatus { get; set; }
    }
}
