// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public record OnboardingRegistrationState(
    string RegistrationId,
    string AssignedHub,
    string DeviceId,
    string Status,
    string Substatus,
    int? ErrorCode,
    string ErrorMessage,
    DateTimeOffset? CreatedDateTimeUtc,
    DateTimeOffset? LastUpdatedDateTimeUtc);
