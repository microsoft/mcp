// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

internal sealed record RecursiveSelectOutputSchemaSample(string Name, RecursiveSelectOutputSchemaSample[] Children);
