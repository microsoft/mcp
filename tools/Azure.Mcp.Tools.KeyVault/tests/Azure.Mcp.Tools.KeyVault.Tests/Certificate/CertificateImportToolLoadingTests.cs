// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.KeyVault.Services;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Host;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Azure.Mcp.Tools.KeyVault.Tests.Certificate;

public sealed class CertificateImportToolLoadingTests
{
    [Theory]
    [InlineData(false, "base64")]
    [InlineData(true, "base64")]
    [InlineData(false, "pem")]
    [InlineData(true, "pem")]
    [InlineData(false, "file")]
    [InlineData(true, "file")]
    public async Task HttpImport_AllowsInlineCertificatesButDeniesFiles(bool namespaceMode, string inputKind)
    {
        var configuration = OptionsFactory.Create(new ServerRuntimeConfiguration
        {
            Transport = "http",
            DangerouslyDisableElicitation = true
        });
        var reader = new KeyVaultService(Substitute.For<IAzureService>(), new FileSystem(configuration));
        var data = inputKind switch
        {
            "pem" => "-----BEGIN CERTIFICATE-----",
            "base64" => Convert.ToBase64String([1, 2, 3]),
            _ => "certificate.pfx"
        };
        byte[]? importedBytes = null;
        var service = Substitute.For<IKeyVaultService>();
        service.ImportCertificate("vault", "certificate", data, null, null, Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                importedBytes = await reader.ReadCertificateDataAsync(call.ArgAt<string>(2), call.ArgAt<CancellationToken>(5));
                return CertificateModelFactory.KeyVaultCertificateWithPolicy(
                    new CertificateProperties("certificate"),
                    keyId: new Uri("https://vault.vault.azure.net/keys/certificate/version"),
                    secretId: new Uri("https://vault.vault.azure.net/secrets/certificate/version"),
                    cer: [1, 2, 3],
                    policy: new CertificatePolicy("Self", "CN=test"));
            });
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<ITelemetryService, NoopTelemetryService>();
        new KeyVaultSetup().ConfigureServices(services);
        services.AddSingleton(service);
        using var provider = services.BuildServiceProvider();
        var factory = new CommandFactory(
            provider,
            [new KeyVaultSetup()],
            provider.GetRequiredService<ITelemetryService>(),
            OptionsFactory.Create(new McpServerConfiguration
            {
                Name = "test",
                ShortName = "test",
                DisplayName = "Test Server",
                Version = "1.0.0",
                Description = "Test Server",
                RootCommandGroupName = "azmcp"
            }),
            NullLogger<CommandFactory>.Instance);
        await using IToolLoader loader = namespaceMode
            ? new NamespaceToolLoader(new CommandContextAccessor(), factory, configuration, NullLogger<NamespaceToolLoader>.Instance)
            : new CommandFactoryToolLoader(new CommandContextAccessor(), factory, configuration, NullLogger<CommandFactoryToolLoader>.Instance);
        var parameters = new Dictionary<string, JsonElement>
        {
            ["vault"] = JsonSerializer.SerializeToElement("vault"),
            ["certificate"] = JsonSerializer.SerializeToElement("certificate"),
            ["certificate-data"] = JsonSerializer.SerializeToElement(data)
        };
        var arguments = namespaceMode
            ? new Dictionary<string, JsonElement>
            {
                ["intent"] = JsonSerializer.SerializeToElement("Import a certificate"),
                ["command"] = JsonSerializer.SerializeToElement("keyvault_certificate_import"),
                ["parameters"] = JsonSerializer.SerializeToElement(parameters)
            }
            : parameters;
        var request = McpTestUtilities.CreateToolCallRequest(new CallToolRequestParams
        {
            Name = namespaceMode ? "keyvault" : "keyvault_certificate_import",
            Arguments = arguments
        }, Substitute.For<McpServer>());

        var response = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        if (inputKind == "file")
        {
            Assert.True(response.IsError);
            Assert.Null(importedBytes);
            Assert.Contains("not allowed in HTTP mode", McpTestUtilities.GetFirstText(response.Content));
        }
        else
        {
            Assert.True(response.IsError != true, McpTestUtilities.GetFirstText(response.Content));
            Assert.Equal(inputKind == "pem" ? Encoding.UTF8.GetBytes(data) : [1, 2, 3], importedBytes);
        }
        await service.Received(1).ImportCertificate("vault", "certificate", data, null, null, Arg.Any<CancellationToken>());
    }
}
