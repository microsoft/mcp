// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;

namespace Azure.Mcp.Tests.Helpers;

/// <summary>
/// Creates <see cref="IAzureService"/> substitutes that retain real <see cref="EndpointValidator"/> authorization.
/// </summary>
public static class AzureServiceTestHelpers
{
    /// <summary>
    /// Configures an Azure service substitute to apply the supplied ARM transport and environment.
    /// </summary>
    /// <param name="service">The Azure service substitute.</param>
    /// <param name="httpClient">The HTTP client used by ARM requests.</param>
    /// <param name="armEnvironment">The ARM cloud environment.</param>
    public static void ConfigureArmClientOptions(
        IAzureService service,
        HttpClient httpClient,
        ArmEnvironment armEnvironment)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(httpClient);

        service.When(value => value.ConfigureArmClientOptions(Arg.Any<ArmClientOptions>()))
            .Do(call =>
            {
                ArmClientOptions options = call.Arg<ArmClientOptions>();
                options.Transport = new HttpClientTransport(httpClient);
                options.Environment = armEnvironment;
            });
    }

    /// <summary>
    /// Creates a substitute with real validation and an independently owned immutable <see cref="SsrfProtectionPolicy"/>.
    /// </summary>
    /// <param name="ssrfProtectionPolicy">The policy to exercise, or <see langword="null"/> for no overrides.</param>
    /// <param name="contextAccessor">
    /// The accessor to scope in tests, or <see langword="null"/> for an independent accessor with no active context.
    /// </param>
    /// <param name="armEnvironment">
    /// The configured test cloud, or <see langword="null"/> for <see cref="ArmEnvironment.AzurePublicCloud"/>.
    /// </param>
    /// <returns>
    /// An <see cref="IAzureService"/> substitute whose other Azure operations can be configured by the test.
    /// </returns>
    public static IAzureService CreateAzureService(
        SsrfProtectionPolicy? ssrfProtectionPolicy = null,
        ICommandContextAccessor? contextAccessor = null,
        ArmEnvironment? armEnvironment = null)
    {
        IAzureService service = Substitute.For<IAzureService>();
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(armEnvironment ?? ArmEnvironment.AzurePublicCloud);
        service.CloudConfiguration.Returns(cloudConfiguration);
        EndpointValidator validator = new(
            ssrfProtectionPolicy ?? new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance,
            contextAccessor ?? new CommandContextAccessor());
        service.WhenForAnyArgs(value => value.ValidateAzureServiceEndpoint(
            string.Empty, string.Empty))
            .Do(call => validator.ValidateAzureServiceEndpoint(
                call.ArgAt<string>(0), call.ArgAt<string>(1), service.CloudConfiguration.ArmEnvironment));
        service.WhenForAnyArgs(value => value.ValidatePublicTargetUrl(string.Empty))
            .Do(call => validator.ValidatePublicTargetUrl(call.ArgAt<string>(0)));
        return service;
    }
}
