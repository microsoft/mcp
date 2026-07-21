// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Extension.Services;

public interface ICliInstallService
{
    Task<HttpResponseMessage> GetCliInstallInstructions(string cliType, CancellationToken cancellationToken);
}
