// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using VallyEvaluator.Models;

namespace VallyEvaluator;

public class BuildInfo
{
    public BuildInfoData Data { get; private set; }

    public BuildInfo(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Build info file not found: {filePath}");
        }

        var jsonContent = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize(jsonContent, VallyJsonContext.Default.BuildInfoData);

        Data = data ?? throw new InvalidOperationException($"Failed to deserialize build info from: {filePath}");
    }
}
