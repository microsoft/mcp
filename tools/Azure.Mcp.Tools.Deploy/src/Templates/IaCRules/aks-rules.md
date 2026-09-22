=== Additional requirements for Azure Kubernetes Service (AKS):
- **CRITICAL** PREREQUISITE: You MUST use a stable kubernetesVersion, get by `az aks get-versions --query 'values[?isDefault].version' --location <> -o tsv`.
- For SKU, set based on appmod-get-available-region-sku output to get available VM size in the region, e.g. Standard_D2ds_v5.
- Node pool names must be 6 characters or less (e.g., 'sys', 'user', 'win01').
{{AuthenticationRules}}
{{ToolSpecificRules}}
{{KeyvaultIntegrationRules}}
