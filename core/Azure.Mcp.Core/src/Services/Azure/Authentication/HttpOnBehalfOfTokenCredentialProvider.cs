using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

public class HttpOnBehalfOfTokenCredentialProvider : IAzureTokenCredentialProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<HttpOnBehalfOfTokenCredentialProvider> _logger;

    public HttpOnBehalfOfTokenCredentialProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<HttpOnBehalfOfTokenCredentialProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<TokenCredential> GetTokenAsync(string? tenant, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext is not HttpContext httpContext)
        {
            throw new InvalidOperationException("There is no ongoing HTTP request.");
        }

        if (tenant is not null)
        {
            if (httpContext.User.FindFirst("tid")?.Value is string tidClaim
                && tidClaim != tenant)
            {
                _logger.LogWarning(
                    "The requested token tenant '{GetTokenTenant}' does not match the tenant of the authenticated user '{TidClaim}'. Going to throw.",
                    tenant,
                    tidClaim);

                throw new InvalidOperationException(
                    $"The requested token tenant '{tenant}' does not match the tenant of the authenticated user '{tidClaim}'.");
            }
        }

        // MicrosoftIdentityTokenCredential is registered as scoped, so we
        // can get it from the request services to ensure we get the right instance.
        MicrosoftIdentityTokenCredential tokenCredential = httpContext
            .RequestServices
            .GetRequiredService<MicrosoftIdentityTokenCredential>();
        return Task.FromResult<TokenCredential>(tokenCredential);
    }
}

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="HttpOnBehalfOfTokenCredentialProvider"/> as a
    /// <see cref="IAzureTokenCredentialProvider"/> with lifetime <see cref="ServiceLifetime.Singleton"/>
    /// into the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddHttpOnBehalfOfTokenCredentialProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Dependencies - directly in constructor.
        services.AddHttpContextAccessor();

        // Dependencies - indirectly required to get MicrosoftIdentityTokenCredential.
        services.AddMicrosoftIdentityWebAppAuthentication(configuration)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();
        services.AddMicrosoftIdentityAzureTokenCredential();

        // Register the OBO token provider as a singleton service per the interface contract.
        return services.AddSingleton<IAzureTokenCredentialProvider, HttpOnBehalfOfTokenCredentialProvider>();
    }
}
