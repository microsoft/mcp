using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Dps.Models
{
    /*
    {
        "enrollmentGroupId": "foo-bar",
        "attestation": {
            "type": "symmetricKey"
        },
        "capabilities": {
            "iotEdge": false
        },
        "etag": "IjA2MDA4ZTYyLTAwMDAtMzMwMC0wMDAwLTY5MzFmMjdhMDAwMCI=",
        "provisioningStatus": "enabled",
        "reprovisionPolicy": {
            "updateHubAssignment": true,
            "migrateDeviceData": true
        },
        "createdDateTimeUtc": "2025-12-04T20:43:38.6838432Z",
        "lastUpdatedDateTimeUtc": "2025-12-04T20:43:38.6838432Z",
        "allocationPolicy": "hashed",
        "iotHubs": [
            "iot-hub-cms4.azure-devices.net"
        ]
    }
    */

    public sealed class EnrollmentGroup
    {
        [JsonPropertyName("enrollmentGroupId")]
        public string EnrollmentGroupId { get; set; } = string.Empty;

        [JsonPropertyName("provisioningStatus")]
        public string ProvisioningStatus { get; set; } = string.Empty;

        [JsonPropertyName("createdDateTimeUtc")]
        public DateTime CreatedDateTimeUtc { get; set; }

        [JsonPropertyName("lastUpdatedDateTimeUtc")]
        public DateTime LastUpdatedDateTimeUtc { get; set; }

        [JsonPropertyName("allocationPolicy")]
        public string AllocationPolicy { get; set; } = string.Empty;

        [JsonPropertyName("iotHubs")]
        public List<string> IotHubs { get; set; } = [];

        [JsonPropertyName("attestation")]
        public Attestation Attestation { get; set; } = new();

        [JsonPropertyName("capabilities")]
        public Capabilities Capabilities { get; set; } = new();

        [JsonPropertyName("reprovisionPolicy")]
        public ReprovisionPolicy ReprovisionPolicy { get; set; } = new();

        [JsonPropertyName("credentialPolicyName")]
        public string CredentialPolicyName { get; set; } = string.Empty;

        [JsonPropertyName("etag")]
        public string Etag { get; set; } = string.Empty;
    }
    public sealed class Attestation
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "symmetricKey";

        public SymmetricKeyAttestation? SymmetricKey { get; set; }

        // For x509 attestation
        //[JsonPropertyName("certificate")]
        //public string? Certificate { get; set; }
    }

    public class SymmetricKeyAttestation
    {
        // For symmetricKey attestation
        [JsonPropertyName("primaryKey")]
        public string? PrimaryKey { get; set; }

        [JsonPropertyName("secondaryKey")]
        public string? SecondaryKey { get; set; }
    }

    public sealed class Capabilities
    {
        [JsonPropertyName("iotEdge")]
        public bool IotEdge { get; set; }
    }

    public sealed class ReprovisionPolicy
    {
        [JsonPropertyName("updateHubAssignment")]
        public bool UpdateHubAssignment { get; set; }

        [JsonPropertyName("migrateDeviceData")]
        public bool MigrateDeviceData { get; set; }
    }
}
