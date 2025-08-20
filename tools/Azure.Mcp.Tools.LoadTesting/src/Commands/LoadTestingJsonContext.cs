// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;

namespace Azure.Mcp.Tools.LoadTesting.Commands;

[JsonSerializable(typeof(TestResourceListCommand.TestResourceListCommandResult))]
[JsonSerializable(typeof(TestRunGetCommand.TestRunGetCommandResult))]
[JsonSerializable(typeof(TestRunCreateCommand.TestRunCreateCommandResult))]
[JsonSerializable(typeof(TestRunListCommand.TestRunListCommandResult))]
[JsonSerializable(typeof(TestRunUpdateCommand.TestRunUpdateCommandResult))]
[JsonSerializable(typeof(TestGetCommand.TestGetCommandResult))]
[JsonSerializable(typeof(TestResourceCreateCommand.TestResourceCreateCommandResult))]
[JsonSerializable(typeof(TestCreateCommand.TestCreateCommandResult))]
[JsonSerializable(typeof(TestRun))]
[JsonSerializable(typeof(TestRunRequest))]
[JsonSerializable(typeof(TestRequestPayload))]
[JsonSerializable(typeof(Test))]
internal sealed partial class LoadTestJsonContext : JsonSerializerContext;
