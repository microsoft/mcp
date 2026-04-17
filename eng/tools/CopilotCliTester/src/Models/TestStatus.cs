// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace CopilotCliTester.Models;

[JsonConverter(typeof(JsonStringEnumConverter<TestStatus>))]
internal enum TestStatus 
{
    Fail,
    Pass,
    Error
}
