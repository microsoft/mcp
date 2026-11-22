// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Support.Commands.Ticket;
using Xunit;

namespace Azure.Mcp.Tools.Support.Tests.Ticket;

public class SupportTicketSecretScannerTests
{
    // Test values are assembled at runtime so no secret-shaped literals live in source.
    private static readonly string s_storageKey = new string('A', 40) + new string('b', 46) + "==";
    private static readonly string s_jwt = "eyJ" + new string('a', 20) + ".eyJ" + new string('b', 30) + "." + new string('c', 30);

    public static TheoryData<string, string> PositiveCases => new()
    {
        { "Conn: DefaultEndpointsProtocol=https;AccountName=acct;AccountKey=abcd1234efgh;EndpointSuffix=core.windows.net", SupportTicketSecretScanner.ConnectionStringCredential },
        { "Endpoint=sb://ns.servicebus.windows.net/;SharedAccessKeyName=root;SharedAccessKey=abcd1234efgh", SupportTicketSecretScanner.ConnectionStringCredential },
        { "Server=tcp:db;Database=app;User ID=admin;Password=Placeholder9!;", SupportTicketSecretScanner.ConnectionStringCredential },
        { "Server=db;Uid=admin;Pwd=hunter22;", SupportTicketSecretScanner.ConnectionStringCredential },
        { "URL https://acct.blob.core.windows.net/c/b?sv=2022-11-02&se=2030-01-01T00%3A00%3A00Z&sr=b&sp=r&sig=" + new string('x', 30) + "%3D", SupportTicketSecretScanner.SasToken },
        { "Token: " + s_jwt, SupportTicketSecretScanner.BearerToken },
        { "Header Authorization: Bearer " + new string('z', 40), SupportTicketSecretScanner.BearerToken },
        { "-----BEGIN " + "RSA PRIVATE KEY-----\n...\n-----END RSA PRIVATE KEY-----", SupportTicketSecretScanner.PrivateKey },
        { "-----BEGIN " + "PRIVATE KEY-----", SupportTicketSecretScanner.PrivateKey },
        { "key1 is " + s_storageKey + " for the account", SupportTicketSecretScanner.StorageAccountKey },
        { "token " + "ghp_" + new string('a', 36), SupportTicketSecretScanner.AccessToken },
        { "token " + "github_pat_" + new string('a', 30), SupportTicketSecretScanner.AccessToken },
        { "aws " + "AKIA" + new string('B', 16), SupportTicketSecretScanner.AccessToken },
        { "client secret " + "abc1" + "Q~" + new string('d', 34), SupportTicketSecretScanner.AccessToken },
    };

    [Theory]
    [MemberData(nameof(PositiveCases))]
    public void Scan_DetectsSecret(string text, string expectedCategory)
    {
        var categories = SupportTicketSecretScanner.Scan("title", text);

        Assert.Contains(expectedCategory, categories);
    }

    [Theory]
    [InlineData("VM not starting after resize. Correlation ID 3f2a1b4c-5d6e-7f80-91a2-b3c4d5e6f708 at 2024-05-01T10:00:00Z.")]
    [InlineData("Password reset for the portal account is failing with error 500.")]
    [InlineData("Set Password= in the app settings is not documented.")]
    [InlineData("Storage account key rotation fails; the AccountKey setting is missing.")]
    [InlineData("Blob URL https://acct.blob.core.windows.net/c/b returns 403 AuthorizationFailure.")]
    [InlineData("Resource /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1")]
    [InlineData("The design signature sig=abc is mentioned in docs.")]
    [InlineData("Bearer token authentication fails with 401.")]
    [InlineData("")]
    [InlineData(null)]
    public void Scan_IgnoresNonSecretText(string? text)
    {
        Assert.Empty(SupportTicketSecretScanner.Scan(text));
    }

    [Fact]
    public void Scan_ReportsEachCategoryOnceAcrossFields()
    {
        var categories = SupportTicketSecretScanner.Scan(
            "Pwd=hunter22",
            "AccountKey=abcd1234efgh and Password=another1 and " + s_jwt);

        Assert.Equal(2, categories.Count);
        Assert.Contains(SupportTicketSecretScanner.ConnectionStringCredential, categories);
        Assert.Contains(SupportTicketSecretScanner.BearerToken, categories);
    }
}
