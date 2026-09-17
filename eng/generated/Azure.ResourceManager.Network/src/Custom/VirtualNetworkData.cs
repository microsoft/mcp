// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.ResourceManager.Network;

public partial class VirtualNetworkData
{
    public IList<string> AddressPrefixes
    {
        get
        {
            AddressSpace ??= new Models.VirtualNetworkAddressSpace();
            return AddressSpace.AddressPrefixes;
        }
    }
}
