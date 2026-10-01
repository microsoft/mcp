// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Abstractions;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Areas.Server.Options;

public class MicrosoftIdentityApplicationOptionsBinderTests()
{
    [Fact]
    public void Bind_BindsApplicationOptions()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ClientId"] = "client-id",
            ["TenantId"] = "tenant-id",
            ["Instance"] = "https://login.microsoftonline.com/",
            ["Audiences:0"] = "api://audience",
            ["AllowWebApiToBeAuthorizedByACL"] = "true",
            ["SendX5C"] = "true"
        });
        var options = new MicrosoftIdentityApplicationOptions();

        MicrosoftIdentityApplicationOptionsBinder.Bind(configuration, options);

        Assert.Equal("client-id", options.ClientId);
        Assert.Equal("tenant-id", options.TenantId);
        Assert.Equal("https://login.microsoftonline.com/", options.Instance);
        Assert.Equal(["api://audience"], options.Audiences);
        Assert.True(options.AllowWebApiToBeAuthorizedByACL);
        Assert.True(options.SendX5C);
    }

    [Fact]
    public void Bind_BindsClientCredentialWithoutRuntimeOnlyProperties()
    {
        IConfiguration configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ClientCredentials:0:SourceType"] = "SignedAssertionFromManagedIdentity",
            ["ClientCredentials:0:ManagedIdentityClientId"] = "managed-identity-client-id",
            ["ClientCredentials:0:TokenExchangeUrl"] = "api://AzureADTokenExchange",
            ["ClientCredentials:0:UseBoundCredential"] = "true"
        });
        var options = new MicrosoftIdentityApplicationOptions();

        MicrosoftIdentityApplicationOptionsBinder.Bind(configuration, options);

        IEnumerable<CredentialDescription> credentials = Assert.IsAssignableFrom<IEnumerable<CredentialDescription>>(
            options.ClientCredentials);
        CredentialDescription credential = Assert.Single(credentials);
        Assert.Equal(CredentialSource.SignedAssertionFromManagedIdentity, credential.SourceType);
        Assert.Equal("managed-identity-client-id", credential.ManagedIdentityClientId);
        Assert.Equal("api://AzureADTokenExchange", credential.TokenExchangeUrl);
        Assert.True(credential.UseBoundCredential);
        Assert.Null(credential.Certificate);
        Assert.Null(credential.CachedValue);
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
}
