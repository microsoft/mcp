// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Extension.Models;

namespace Azure.Mcp.Tools.Extension.Services;

public interface ICliInstallService
{
    Task<HttpResponseMessage> GetCliInstallInstructions(CliInstallType cliType, CancellationToken cancellationToken);
}
