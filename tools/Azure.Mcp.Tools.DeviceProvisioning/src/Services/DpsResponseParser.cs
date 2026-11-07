// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.DeviceProvisioning.Models;

namespace Azure.Mcp.Tools.DeviceProvisioning.Services;

internal static class DpsResponseParser
{
    public static DpsEnrollmentSnapshot ParseEnrollment(string json, string targetId)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var attestation = GetObject(root, "attestation");
        var x509 = GetObject(attestation, "x509");

        return new DpsEnrollmentSnapshot(
            targetId,
            GetString(root, "provisioningStatus", "enabled"),
            GetString(attestation, "type"),
            GetString(root, "allocationPolicy"),
            GetString(root, "iotHubHostName"),
            GetStringList(root, "iotHubs"),
            GetString(GetObject(root, "customAllocationDefinition"), "webhookUrl"),
            GetBoolean(GetObject(root, "reprovisionPolicy"), "updateHubAssignment"),
            GetBoolean(GetObject(root, "reprovisionPolicy"), "migrateDeviceData"),
            HasNonNullProperty(root, "initialTwin"),
            GetString(GetObject(attestation, "tpm"), "endorsementKey"),
            GetCertificateThumbprints(GetObject(x509, "clientCertificates")),
            GetCertificateThumbprints(GetObject(x509, "signingCertificates")),
            GetCertificateAuthorityReferences(x509));
    }

    public static DpsRegistrationStateSnapshot ParseRegistrationState(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return new DpsRegistrationStateSnapshot(
            GetString(root, "registrationId"),
            GetString(root, "assignedHub"),
            GetString(root, "deviceId"),
            GetString(root, "status"),
            GetString(root, "substatus"),
            GetInt32(root, "errorCode"),
            GetString(root, "errorMessage"),
            GetDateTimeOffset(root, "createdDateTimeUtc"),
            GetDateTimeOffset(root, "lastUpdatedDateTimeUtc"));
    }

    private static JsonElement GetObject(JsonElement parent, string propertyName)
    {
        return parent.ValueKind == JsonValueKind.Object &&
               parent.TryGetProperty(propertyName, out var value) &&
               value.ValueKind == JsonValueKind.Object
            ? value
            : default;
    }

    private static string GetString(
        JsonElement parent,
        string propertyName,
        string defaultValue = "")
    {
        return parent.ValueKind == JsonValueKind.Object &&
               parent.TryGetProperty(propertyName, out var value) &&
               value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? defaultValue
            : defaultValue;
    }

    private static bool? GetBoolean(JsonElement parent, string propertyName)
    {
        return parent.ValueKind == JsonValueKind.Object &&
               parent.TryGetProperty(propertyName, out var value) &&
               value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;
    }

    private static int? GetInt32(JsonElement parent, string propertyName)
    {
        return parent.ValueKind == JsonValueKind.Object &&
               parent.TryGetProperty(propertyName, out var value) &&
               value.TryGetInt32(out var result)
            ? result
            : null;
    }

    private static DateTimeOffset? GetDateTimeOffset(JsonElement parent, string propertyName)
    {
        return parent.ValueKind == JsonValueKind.Object &&
               parent.TryGetProperty(propertyName, out var value) &&
               value.ValueKind == JsonValueKind.String &&
               value.TryGetDateTimeOffset(out var result)
            ? result
            : null;
    }

    private static List<string> GetStringList(JsonElement parent, string propertyName)
    {
        if (parent.ValueKind != JsonValueKind.Object ||
            !parent.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return
        [
            .. value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item!)
        ];
    }

    private static List<string> GetCertificateThumbprints(JsonElement certificates)
    {
        var thumbprints = new List<string>();
        AddCertificateThumbprint(certificates, "primary", thumbprints);
        AddCertificateThumbprint(certificates, "secondary", thumbprints);
        return thumbprints;
    }

    private static void AddCertificateThumbprint(
        JsonElement certificates,
        string certificateName,
        List<string> thumbprints)
    {
        var certificate = GetObject(certificates, certificateName);
        var info = GetObject(certificate, "info");
        var thumbprint = GetString(info, "sha256Thumbprint");
        if (!string.IsNullOrWhiteSpace(thumbprint))
        {
            thumbprints.Add(NormalizeThumbprint(thumbprint));
        }
    }

    private static List<DpsCertificateAuthoritySnapshot> GetCertificateAuthorityReferences(
        JsonElement x509)
    {
        var references = GetObject(x509, "caReferences");
        var result = new List<DpsCertificateAuthoritySnapshot>();
        AddCertificateAuthorityReference(references, "primary", result);
        AddCertificateAuthorityReference(references, "secondary", result);
        return result;
    }

    private static void AddCertificateAuthorityReference(
        JsonElement references,
        string role,
        List<DpsCertificateAuthoritySnapshot> result)
    {
        var name = GetString(references, role);
        if (!string.IsNullOrWhiteSpace(name))
        {
            result.Add(new DpsCertificateAuthoritySnapshot(
                role,
                name,
                "unresolved",
                Certificate: null));
        }
    }

    private static bool HasNonNullProperty(JsonElement parent, string propertyName)
    {
        return parent.ValueKind == JsonValueKind.Object &&
               parent.TryGetProperty(propertyName, out var value) &&
               value.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined;
    }

    internal static string NormalizeThumbprint(string thumbprint) =>
        thumbprint.Replace(":", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant();
}
