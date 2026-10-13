// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using VallyEvaluator.Models;

namespace VallyEvaluator;

[JsonSerializable(typeof(BuildInfoData))]
[JsonSerializable(typeof(ServerInfo))]
[JsonSerializable(typeof(PlatformInfo))]
[JsonSerializable(typeof(PathToTest))]
public partial class VallyJsonContext : JsonSerializerContext
{
}
