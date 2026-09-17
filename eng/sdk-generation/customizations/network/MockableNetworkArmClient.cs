// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.TypeSpec.Generator.Customizations;

namespace Azure.ResourceManager.Network.Mocking;

[CodeGenSuppress("GetNetworkInterfaceResource", typeof(ResourceIdentifier))]
[CodeGenSuppress("GetPublicIPAddressResource", typeof(ResourceIdentifier))]
public partial class MockableNetworkArmClient
{
}
