// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License

using System.Runtime.InteropServices;
using Azure.Mcp.Tools.Extension.Models;

namespace Azure.Mcp.Tools.Extension.Services;

internal class CliInstallService(IHttpClientFactory httpClientFactory) : ICliInstallService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<HttpResponseMessage> GetCliInstallInstructions(CliInstallType cliType, CancellationToken cancellationToken)
    {
        string osStr;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            osStr = "windows";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            osStr = "linux";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            osStr = "macOS";
        }
        else
        {
            throw new ArgumentException($"Unsupported OS type {RuntimeInformation.OSDescription}. Supported OS are: windows, macOS, linux");
        }

        string instructionsUrl = cliType switch
        {
            CliInstallType.Az => $"https://raw.githubusercontent.com/microsoft/GitHub-Copilot-for-Azure/refs/heads/main/docs/cli-install/{osStr}/az.md",
            CliInstallType.Azd => $"https://raw.githubusercontent.com/microsoft/GitHub-Copilot-for-Azure/refs/heads/main/docs/cli-install/{osStr}/azd.md",
            CliInstallType.Func => $"https://raw.githubusercontent.com/microsoft/GitHub-Copilot-for-Azure/refs/heads/main/docs/cli-install/{osStr}/func.md",
            _ => throw new ArgumentOutOfRangeException(nameof(cliType), cliType, null)
        };

        using HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new(instructionsUrl)
        };
        return await _httpClientFactory.CreateClient().SendAsync(requestMessage, cancellationToken);
    }
}
