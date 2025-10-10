// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Extension.Services;

public interface ICliInstallService
{
    public Task<HttpResponseMessage> GetCliInstallInstructions(string cliType);
}
