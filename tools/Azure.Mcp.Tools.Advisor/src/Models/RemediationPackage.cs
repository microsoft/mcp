// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// ARM resource envelope returned by the proposed
/// <c>GET /providers/Microsoft.Advisor/remediationTypes/{recommendationTypeId}</c> operation.
/// </summary>
public sealed class RemediationPackage
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public RemediationProperties? Properties { get; set; }
}

/// <summary>
/// Remediation metadata, safety flags, inline artifacts, and human-readable methods for a recommendation type.
/// </summary>
public sealed class RemediationProperties
{
    public string? RecommendationTypeId { get; set; }
    public string? OutputType { get; set; }
    public bool? Destructive { get; set; }
    public bool? Reversible { get; set; }
    public bool? Grounded { get; set; }
    public string? Confidence { get; set; }
    public int? Version { get; set; }
    public List<RemediationArtifact>? Artifacts { get; set; }
    public List<RemediationMethod>? Methods { get; set; }
}

/// <summary>
/// A single inline, executable artifact (cli, powershell, bicep, or arm) with its content.
/// </summary>
public sealed class RemediationArtifact
{
    public string? ArtifactType { get; set; }
    public string? ContentType { get; set; }
    public string? Confidence { get; set; }
    public string? Content { get; set; }
}

/// <summary>
/// A human-readable remediation method (e.g. Azure CLI) with parameters, ordered steps, and verification.
/// </summary>
public sealed class RemediationMethod
{
    public string? Heading { get; set; }
    public string? Method { get; set; }
    public string? Relation { get; set; }
    public bool? Executable { get; set; }
    public List<RemediationParameter>? Parameters { get; set; }
    public List<RemediationStep>? Steps { get; set; }
    public string? Verification { get; set; }
}

/// <summary>
/// A parameter required by a remediation method.
/// </summary>
public sealed class RemediationParameter
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Example { get; set; }
    public bool? Required { get; set; }
}

/// <summary>
/// A single ordered step within a remediation method.
/// </summary>
public sealed class RemediationStep
{
    public int? Number { get; set; }
    public string? Text { get; set; }
    public string? Kind { get; set; }
    public string? Command { get; set; }
}
