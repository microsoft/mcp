// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License

using System.Runtime.InteropServices;

namespace Azure.Mcp.Tools.Extension.Services;

internal class CliInstallService(IHttpClientFactory httpClientFactory) : ICliInstallService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<HttpResponseMessage> GetCliInstallInstructions(string cliType, CancellationToken cancellationToken)
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
            Constants.AzureCliType => $"https://raw.githubusercontent.com/microsoft/GitHub-Copilot-for-Azure/refs/heads/main/docs/cli-install/{osStr}/az.md",
            Constants.AzureDeveloperCliType => $"https://raw.githubusercontent.com/microsoft/GitHub-Copilot-for-Azure/refs/heads/main/docs/cli-install/{osStr}/azd.md",
            Constants.AzureFunctionsCoreToolsCliType => $"https://raw.githubusercontent.com/microsoft/GitHub-Copilot-for-Azure/refs/heads/main/docs/cli-install/{osStr}/func.md",
            _ => throw new ArgumentException($"Invalid CLI type: {cliType}.")
        };

        using HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new(instructionsUrl)
        };
        return await _httpClientFactory.CreateClient().SendAsync(requestMessage, cancellationToken);
    }
}
