using Azure.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Dps.Options.EnrollmentGroup;

/// <summary>
/// Options for creating an enrollment group.
/// </summary>
public class EnrollmentGroupCreateOptions : Azure.Mcp.Core.Options.GlobalOptions
{
    public string DpsHostName { get; set; } = string.Empty;
    public string EnrollmentGroupId { get; set; } = string.Empty;
    public string AttestationType { get; set; } = string.Empty; // e.g., "symmetricKey", "x509"
    public string? PrimaryKey { get; set; }
    public string? SecondaryKey { get; set; }
    public string? Certificate { get; set; }
    public string? ProvisioningStatus { get; set; } // e.g., "enabled", "disabled"
}
