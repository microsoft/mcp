// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ConfidentialLedger.Models;
using Azure.Mcp.Tools.ConfidentialLedger.Options;

namespace Azure.Mcp.Tools.ConfidentialLedger;

[JsonSerializable(typeof(AppendEntryOptions))]
[JsonSerializable(typeof(AppendEntryResult))]
[JsonSerializable(typeof(AppendEntryRequest))]
public partial class ConfidentialLedgerJsonContext : JsonSerializerContext
{
}
