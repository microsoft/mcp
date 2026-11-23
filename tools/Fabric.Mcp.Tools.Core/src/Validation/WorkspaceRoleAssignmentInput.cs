// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class WorkspaceRoleAssignmentInput
{
    internal const int MaxRequestBytes = FabricItemJsonInput.MaxObjectBytes;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static void ValidateRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new FabricItemInputException("--role is required and must be a nonempty string. No access level is inferred.");
        }
        try
        {
            if (role.Length > MaxRequestBytes || StrictUtf8.GetByteCount(role) > MaxRequestBytes)
            {
                throw new FabricItemInputException("--role exceeds the 1 MiB request budget.");
            }
        }
        catch (EncoderFallbackException)
        {
            throw new FabricItemInputException("--role must contain valid Unicode. No replacement access level is selected.");
        }
    }

    internal static void ValidateRequestSize(string body)
    {
        if (Encoding.UTF8.GetByteCount(body) > MaxRequestBytes)
        {
            throw new FabricItemInputException("The serialized role-assignment request must not exceed the 1 MiB (1,048,576 byte) tool budget.");
        }
    }
}
