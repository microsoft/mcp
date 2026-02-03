# Azure VM Create Command Design Document

## Overview

This document outlines the design for the `azmcp compute vm create` command, including mandatory parameters, smart defaults, validation rules, and implementation guidance based on analysis of Azure CLI, Google Cloud MCP, and AWS MCP patterns.

## Command Specification

```
azmcp compute vm create
```

**Tool Metadata:**
- `Destructive: true` - Creates billable resources
- `Idempotent: false` - Running twice creates duplicate VMs
- `ReadOnly: false` - Modifies Azure state

---

## Parameters

### Required Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `name` | string | Name of the virtual machine (1-64 chars, alphanumeric, hyphens, underscores) |
| `resourceGroup` | string | Name of the resource group |
| `image` | string | OS image reference (alias, URN, or resource ID) |

### Optional Parameters with Smart Defaults

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `subscription` | string | Current subscription | Target subscription (ID or name) |
| `location` | string | Resource group location | Azure region for the VM |
| `size` | string | `Standard_D2s_v5` | VM size (SKU) |
| `adminUsername` | string | - | Admin account username (required if no SSH key) |
| `adminPassword` | string | - | Admin password (Windows or Linux password auth) |
| `sshKeyValue` | string | - | SSH public key value (Linux only) |
| `authenticationType` | string | `ssh` (Linux) / `password` (Windows) | Authentication method |
| `vnet` | string | Auto-created `{name}VNet` | Existing VNet name or resource ID |
| `subnet` | string | Auto-created `{name}Subnet` | Existing subnet name |
| `publicIpAddress` | string | Auto-created `{name}PublicIP` | Public IP resource (empty string = none) |
| `nsg` | string | Auto-created `{name}NSG` | Network security group |
| `osDiskType` | string | `Premium_LRS` | OS disk storage type |
| `osDiskSizeGb` | int | Image default | OS disk size in GB |
| `tags` | object | - | Resource tags as key-value pairs |

---

## Smart Defaults

### VM Size Default: `Standard_D2s_v5`

**Rationale:**
- Azure CLI is migrating from `Standard_DS1_v2` to `Standard_D2s_v5`
- D-series v5 offers better price/performance ratio
- 2 vCPUs, 8 GB RAM - suitable for most workloads
- Premium storage capable
- Available in all regions

**Comparison with Other Providers:**
| Provider | Default Size | Specs |
|----------|-------------|-------|
| Azure MCP | `Standard_D2s_v5` | 2 vCPU, 8 GB RAM |
| Google Cloud MCP | `n1-standard-1` | 1 vCPU, 3.75 GB RAM |
| AWS (no default) | User must specify | - |

### Location Default: Resource Group Location

**Rationale:**
- Consistent with Azure CLI behavior
- Reduces network latency between resources
- Simplifies resource organization

**Implementation:**
```csharp
var location = options.Location
    ?? await GetResourceGroupLocation(options.ResourceGroup, options.Subscription);
```

### Authentication Defaults

**Linux VMs:**
- Default: `ssh` authentication
- Requires `sshKeyValue` parameter
- Falls back to `password` if `adminPassword` provided without SSH key

**Windows VMs:**
- Default: `password` authentication
- Requires `adminUsername` and `adminPassword`

**Decision: SSH Key Not Auto-Generated**
Unlike Azure CLI which can generate SSH keys locally, MCP runs remotely and cannot securely store private keys. Users must provide their own SSH public key.

### Network Auto-Creation Defaults

When no existing network resources are specified, the command creates:

| Resource | Naming Pattern | Default Configuration |
|----------|---------------|----------------------|
| Virtual Network | `{vmName}VNet` | Address prefix: `10.0.0.0/16` |
| Subnet | `{vmName}Subnet` | Address prefix: `10.0.0.0/24` |
| Public IP | `{vmName}PublicIP` | SKU: Standard, Dynamic allocation |
| NSG | `{vmName}NSG` | Allow SSH (22) for Linux, RDP (3389) for Windows |
| NIC | `{vmName}VMNic` | Connected to subnet, NSG, and public IP |

**Network Reuse Logic:**
1. If VNet specified → Use existing, find/create subnet
2. If subnet specified → Use existing with its VNet
3. If nothing specified → Create new VNet with subnet

### OS Disk Defaults

| Setting | Default | Rationale |
|---------|---------|-----------|
| Type | `Premium_LRS` | Best performance for production workloads |
| Caching | `ReadWrite` | Optimal for OS disks |
| Size | Image default | Usually 30-128 GB depending on image |
| Delete option | `Delete` | Clean up with VM |

---

## Image Aliases

Support common image aliases for convenience:

| Alias | Publisher | Offer | SKU |
|-------|-----------|-------|-----|
| `Ubuntu2204` | Canonical | 0001-com-ubuntu-server-jammy | 22_04-lts-gen2 |
| `Ubuntu2404` | Canonical | ubuntu-24_04-lts | server |
| `Win2022Datacenter` | MicrosoftWindowsServer | WindowsServer | 2022-datacenter-g2 |
| `Win2019Datacenter` | MicrosoftWindowsServer | WindowsServer | 2019-Datacenter |
| `RHEL9` | RedHat | RHEL | 9-lvm-gen2 |
| `Debian12` | Debian | debian-12 | 12-gen2 |

**Image Resolution Order:**
1. Check if input is a known alias → Resolve to URN
2. Check if input is a URN (publisher:offer:sku:version) → Use directly
3. Check if input is a resource ID → Use as custom image
4. Return error with helpful message

---

## Validation Rules

### VM Name Validation
- Length: 1-64 characters (Windows), 1-64 characters (Linux)
- Allowed: alphanumeric, hyphens, underscores
- Cannot start or end with hyphen
- Must be unique within resource group

### Username Validation
- Length: 1-20 characters (Windows), 1-64 characters (Linux)
- Cannot be: `admin`, `administrator`, `root`, `guest`, `test`
- Cannot start with number or special character
- Linux: lowercase recommended

### Password Validation
- Length: 12-123 characters
- Must contain 3 of 4: lowercase, uppercase, digit, special character
- Cannot contain username
- Cannot be common passwords

---

## Response Model

```json
{
  "id": "/subscriptions/.../resourceGroups/.../providers/Microsoft.Compute/virtualMachines/myvm",
  "name": "myvm",
  "location": "eastus2",
  "properties": {
    "provisioningState": "Succeeded",
    "vmId": "12345678-1234-1234-1234-123456789abc",
    "hardwareProfile": {
      "vmSize": "Standard_D2s_v5"
    },
    "osProfile": {
      "computerName": "myvm",
      "adminUsername": "azureuser"
    },
    "networkProfile": {
      "networkInterfaces": [
        {
          "id": "/subscriptions/.../networkInterfaces/myvmVMNic"
        }
      ]
    }
  },
  "createdResources": [
    {
      "type": "Microsoft.Network/virtualNetworks",
      "name": "myvmVNet"
    },
    {
      "type": "Microsoft.Network/publicIPAddresses",
      "name": "myvmPublicIP"
    }
  ]
}
```

---

## Error Handling

| Error Condition | Status Code | Message |
|-----------------|-------------|---------|
| VM name already exists | 409 | "A VM named '{name}' already exists in resource group '{rg}'" |
| Invalid image reference | 400 | "Image '{image}' not found. Use format 'publisher:offer:sku:version' or a known alias" |
| Quota exceeded | 403 | "Core quota exceeded for size '{size}' in region '{location}'" |
| Invalid VM size | 400 | "VM size '{size}' is not available in region '{location}'" |
| Missing authentication | 400 | "Linux VMs require either 'sshKeyValue' or 'adminPassword'" |
| Password validation failed | 400 | "Password does not meet complexity requirements" |

---

## Implementation Phases

### Phase 1: MVP
- Required parameters: `name`, `resourceGroup`, `image`
- Smart defaults for size, location, authentication
- Network auto-creation with default naming
- Support for image aliases
- Basic validation

### Phase 2: Enhanced
- Zones support for availability zones
- Tags parameter
- OS disk customization (size, type)
- Existing network resource references
- Data disk attachment

### Phase 3: Advanced
- Availability set support
- Managed identity configuration
- Accelerated networking
- Proximity placement groups
- Spot/low-priority instances
- Custom data / cloud-init scripts

---

## Comparison with Other Providers

### Google Cloud MCP `create_instance`

```
Required: project, zone, instance_name
Defaults: machine_type=n1-standard-1, image=debian-12
```

**Key Differences:**
- Google defaults the image; Azure requires explicit image selection
- Google requires zone; Azure uses resource group location
- Google auto-creates default network; Azure creates named resources

### AWS MCP (via Cloud Control API)

```
Required: ImageId, InstanceType, SubnetId
No smart defaults - all parameters explicit
```

**Key Differences:**
- AWS requires explicit network configuration
- AWS has no image aliases in MCP
- AWS doesn't create dependent resources automatically

### Azure MCP Design Advantages

1. **Guided Experience**: Image required but aliases simplify selection
2. **Network Automation**: Creates VNet/Subnet/NSG/PublicIP with sensible names
3. **Cost Awareness**: No hidden default image that might incur licensing costs
4. **Discoverability**: Clear parameter names and validation messages

---

## Example Usage

### Minimal (Linux with SSH)
```
azmcp compute vm create
  --name mylinuxvm
  --resource-group myRG
  --image Ubuntu2204
  --ssh-key-value "ssh-rsa AAAA..."
```

### Minimal (Windows)
```
azmcp compute vm create
  --name mywinvm
  --resource-group myRG
  --image Win2022Datacenter
  --admin-username azureuser
  --admin-password "SecureP@ssw0rd123"
```

### With Custom Size and Location
```
azmcp compute vm create
  --name myvm
  --resource-group myRG
  --image Ubuntu2204
  --size Standard_D4s_v5
  --location westus2
  --ssh-key-value "ssh-rsa AAAA..."
```

### Using Existing Network
```
azmcp compute vm create
  --name myvm
  --resource-group myRG
  --image Ubuntu2204
  --vnet existingVNet
  --subnet existingSubnet
  --public-ip-address ""
  --ssh-key-value "ssh-rsa AAAA..."
```

---

## References

- [Azure CLI az vm create](https://learn.microsoft.com/en-us/cli/azure/vm?view=azure-cli-latest#az-vm-create)
- [Google Cloud MCP create_instance](https://docs.cloud.google.com/compute/docs/reference/mcp/tools/create_instance)
- [Azure VM Sizes](https://learn.microsoft.com/en-us/azure/virtual-machines/sizes)
- [Azure VM Image Reference](https://learn.microsoft.com/en-us/azure/virtual-machines/linux/cli-ps-findimage)
