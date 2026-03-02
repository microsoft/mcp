// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CopilotCliTester.Models;

[JsonSerializable(typeof(TestPrompt))]
[JsonSerializable(typeof(TestPrompt[]))]
[JsonSerializable(typeof(TestResult))]
[JsonSerializable(typeof(TestResult[]))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(object))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
internal partial class JsonContext : JsonSerializerContext;