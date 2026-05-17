- DO NOT use auto scale ('auto_scaling_enabled').
- AKS terraform Example:
```
resource "azurerm_kubernetes_cluster" "aks" {
  name                = azurecaf_name.aks.result
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  kubernetes_version  = var.kubernetes_version    // Use a stable version from `az aks get-versions --query 'values[?isDefault].version' --location <> -o tsv`

  default_node_pool {
    name                = "default"
    node_count          = 2
    vm_size             = "Standard_D2s_v3"
    os_disk_size_gb     = 30
    vnet_subnet_id      = null
  }

  identity {
    type         = "SystemAssigned"  // Use SystemAssigned identity for AKS
  }

  network_profile {
    network_plugin    = "kubenet"
    load_balancer_sku = "standard"
  }

  key_vault_secrets_provider {  // Add if using Key Vault
    secret_rotation_enabled = true
  }

  tags = {
    Environment = var.environment
  }

  depends_on = [
  ]
}

# AcrPull role assignment for AKS identity
resource "azurerm_role_assignment" "aks_acr_pull" {
  principal_id                     = azurerm_kubernetes_cluster.aks.kubelet_identity[0].object_id
  role_definition_id               = "/subscriptions/{subscription_id}/providers/Microsoft.Authorization/roleDefinitions/7f951dda-4ed3-4680-a7ca-43fe172d538d"
  scope                            = azurerm_container_registry.acr.id
  skip_service_principal_aad_check = true
  depends_on = [ azurerm_kubernetes_cluster.aks ]  // Important
}
```

```
# Key Vault Secrets User role assignment for AKS managed identity
resource "azurerm_role_assignment" "secret_user" {
  scope                = azurerm_key_vault.keyvault.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_kubernetes_cluster.aks.key_vault_secrets_provider[0].secret_identity[0].object_id
  depends_on = [ azurerm_kubernetes_cluster.aks ]
}
```
