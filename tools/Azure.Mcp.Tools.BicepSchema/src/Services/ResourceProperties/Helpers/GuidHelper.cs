// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;

namespace Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Helpers;
public static class GuidHelper
{
    public static Guid GenerateDeterministicGuid(params string[] strings)
    {
        string concatenatedString = string.Join("", strings);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(concatenatedString));
        var guid = new Guid([.. hash.Take(16)]);

        return guid;
    }
}
