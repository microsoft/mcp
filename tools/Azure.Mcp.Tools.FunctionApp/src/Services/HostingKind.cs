// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FunctionApp.Services;

/// <summary>
/// The hosting model used for a Function App.
/// </summary>
public enum HostingKind
{
    Consumption,
    FlexConsumption,
    Premium,
    AppService,
    ContainerApp
}
