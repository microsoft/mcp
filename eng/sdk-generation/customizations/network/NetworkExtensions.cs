// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.TypeSpec.Generator.Customizations;

namespace Azure.ResourceManager.Network;

[CodeGenSuppress("GetNetworkInterfaceResource", typeof(ArmClient), typeof(ResourceIdentifier))]
[CodeGenSuppress("GetPublicIPAddressResource", typeof(ArmClient), typeof(ResourceIdentifier))]
public static partial class NetworkExtensions
{
}
