// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Client for the external API that starts an Azure Catalyst machine assessment from a collector ZIP. Creating an assessment is
/// asynchronous: the call returns an operation that is checked with <see cref="IAssessmentOperationStatusClient"/>.
/// </summary>
public interface IAssessmentCreateClient
{
    /// <summary>
    /// Starts the assessment and returns its operation.
    /// </summary>
    Task<AssessmentOperation> CreateAssessmentAsync(AssessmentRequest request, string token, CancellationToken cancellationToken);
}

/// <summary>
/// Inputs of the assessment create call. <see cref="InventorySasUri"/> is a read SAS URL, so it is left out of <see cref="ToString"/>.
/// </summary>
public sealed record AssessmentRequest(
    string AssessmentName,
    string? WorkspaceId,
    string InventorySasUri,
    string TargetRegion,
    string Currency,
    AssessmentSettings? Settings = null)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"AssessmentRequest {{ AssessmentName = {AssessmentName}, WorkspaceId = {WorkspaceId}, TargetRegion = {TargetRegion}, Currency = {Currency}, Settings = {Settings} }}";
}

/// <summary>
/// Optional assessment settings supplied by the user. A null value is not sent, so the assessment uses its default.
/// </summary>
public sealed record AssessmentSettings
{
    /// <summary>Gets the scaling factor, or null for the default.</summary>
    public double? ScalingFactor { get; init; }
    /// <summary>Gets the storage scaling factor, or null for the default.</summary>
    public double? StorageScalingFactor { get; init; }
    /// <summary>Gets the discount percentage, or null for the default.</summary>
    public double? DiscountPercentage { get; init; }
    /// <summary>Gets the environment type, or null for the default.</summary>
    public string? EnvironmentType { get; init; }
    /// <summary>Gets the licensing program, or null for the default.</summary>
    public string? LicensingProgram { get; init; }
    /// <summary>Gets the billing subscription id, or null for the default.</summary>
    public string? BillingSubscriptionId { get; init; }
    /// <summary>Gets the savings option, or null for the default.</summary>
    public string? SavingsOption { get; init; }
    /// <summary>Gets the azure offer code, or null for the default.</summary>
    public string? AzureOfferCode { get; init; }
    /// <summary>Gets the security offering type, or null for the default.</summary>
    public string? SecurityOfferingType { get; init; }
    /// <summary>Gets the disk types, or null for the default.</summary>
    public string[]? DiskTypes { get; init; }
    /// <summary>Gets the pricing tier, or null for the default.</summary>
    public string? PricingTier { get; init; }
    /// <summary>Gets the storage redundancy, or null for the default.</summary>
    public string? StorageRedundancy { get; init; }
    /// <summary>Gets the azure hybrid benefit, or null for the default.</summary>
    public string? AzureHybridBenefit { get; init; }
    /// <summary>Gets the linux azure hybrid benefit, or null for the default.</summary>
    public string? LinuxAzureHybridBenefit { get; init; }
    /// <summary>Gets the vm families, or null for the default.</summary>
    public string[]? VmFamilies { get; init; }
    /// <summary>Gets the uptime days per month, or null for the default.</summary>
    public int? UptimeDaysPerMonth { get; init; }
    /// <summary>Gets the uptime hours per day, or null for the default.</summary>
    public int? UptimeHoursPerDay { get; init; }
    /// <summary>Gets the vm security options, or null for the default.</summary>
    public string[]? VmSecurityOptions { get; init; }
    /// <summary>Gets the management services, or null for the default.</summary>
    public string[]? ManagementServices { get; init; }
}
