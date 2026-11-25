// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Services.Azure.Authentication;

public sealed class HttpOnBehalfOfTokenCredentialProviderTests
{
    private const string ScopeUriClaim = "http://schemas.microsoft.com/identity/claims/scope";

    public static TheoryData<Claim[], bool> PrincipalClaims => new()
    {
        { [new("roles", "Mcp.Tools.ReadWrite.All")], true },
        { [new(ClaimTypes.Role, "Mcp.Tools.ReadWrite.All")], true },
        { [new("idtyp", "app"), new("roles", "Mcp.Tools.ReadWrite.All")], true },
        { [new("scp", "Mcp.Tools.ReadWrite")], false },
        { [new(ScopeUriClaim, "Mcp.Tools.ReadWrite")], false },
        { [new("idtyp", "user"), new("scp", "Mcp.Tools.ReadWrite")], false },
        { [new("sub", "user-object-id")], false },
    };

    [Theory]
    [MemberData(nameof(PrincipalClaims))]
    public void IsAppOnlyToken_ClassifiesPrincipal(Claim[] claims, bool expected)
    {
        Assert.Equal(expected, HttpOnBehalfOfTokenCredentialProvider.IsAppOnlyToken(CreatePrincipal(claims)));
    }

    [Fact]
    public async Task GetTokenCredentialAsync_AppOnlyToken_ThrowsBeforeOnBehalfOfExchange()
    {
        var httpContext = new DefaultHttpContext
        {
            User = CreatePrincipal([new("roles", "Mcp.Tools.ReadWrite.All")])
        };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);
        var provider = new HttpOnBehalfOfTokenCredentialProvider(accessor, NullLogger<HttpOnBehalfOfTokenCredentialProvider>.Instance);

        var ex = await Assert.ThrowsAsync<OnBehalfOfUserTokenRequiredException>(
            () => provider.GetTokenCredentialAsync(null, TestContext.Current.CancellationToken));

        Assert.Contains("UseHostingEnvironmentIdentity", ex.Message);
    }

    private static ClaimsPrincipal CreatePrincipal(Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Bearer"));
}
