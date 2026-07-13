// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;

public class CollaborationsListOptions : BaseManagedCleanroomDataPlaneOptions
{
    [Option(Description = "When true, returns only active collaborations (email-only lookup). When omitted, returns all collaborations.")]
    public bool? ActiveOnly { get; set; }
}

