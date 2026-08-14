// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed record InstallationMethod(
    string Platform,
    string Method,
    string Command,
    bool ManagesPath);
