// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Options;

namespace Azure.Mcp.Core.Configuration;

[OptionsValidator]
public partial class AzureMcpServerConfigurationValidator : IValidateOptions<AzureMcpServerConfiguration>
{
}
