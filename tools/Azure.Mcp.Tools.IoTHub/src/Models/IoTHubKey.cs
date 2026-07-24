// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

public record IoTHubKey(
    string KeyName,
    string PrimaryKey,
    string SecondaryKey,
    string Rights);
