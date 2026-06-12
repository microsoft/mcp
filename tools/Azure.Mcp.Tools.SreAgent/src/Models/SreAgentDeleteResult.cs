// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed record SreAgentDeleteResult(string Name, string ResourceType, bool Deleted);
