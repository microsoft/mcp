// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

internal sealed record SelectOutputSchemaCollection(
    [property: JsonPropertyName("result/items~")] SelectOutputSchemaSample[] Items);
