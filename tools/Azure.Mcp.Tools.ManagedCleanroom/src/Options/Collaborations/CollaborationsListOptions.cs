// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;

public class CollaborationsListOptions
{
    [Option(ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option("When true, returns only active collaborations (email-only lookup). When omitted, returns all collaborations.")]
    public bool? ActiveOnly { get; set; }

    [Option(ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool AllowUntrustedCert { get; set; }

    [Option(ManagedCleanroomOptionDescriptions.TokenScope)]
    public string? TokenScope { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}

