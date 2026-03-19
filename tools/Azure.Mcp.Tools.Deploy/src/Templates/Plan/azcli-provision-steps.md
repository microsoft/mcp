1. Env setup for Azure CLI:
    1. Install Azure CLI if not installed.
    2. Ensure there is a default subscription set. If provided, override the default subscription with the provided subscription ID.
    3. Subscription ID: Use default subscription
    (ONLY WHEN the project is using passwordless connection to mysql-flexible/postgres-flexible/sql) 4. Install Service Connector Azure CLI extension: az extension add --name serviceconnector-passwordless --upgrade
2. Provision Azure Infrastructure with Azure CLI:
    1. Provisioning tool: azcli. Expected files: {{IaCType}}.
    2. Grab current subscriptionID, call tool quota_region_availability_list and quota_usage_check to get available region and SKUs for all needed Azure resource types used in this project.
    3. If expected files exist, please follow the below steps:
        - Existing file paths: <filePaths>
        - Check if the Azure resources in the existing files match what we required in the project. If not, add steps to update the existing files with missing Azure resource.
    4. If expected files do not exist, please follow the below steps:
        - Generate the {{IaCType}} files in infra/ folder. Call tool deploy_iac_rules_get to get IaC rules
    5. Validate all generated files to ensure they are runnable and free of syntax errors:
        - Call 'get_errors' on all generated files and iterate until all errors are resolved.
    6. Provision resources with Az CLI `az deployment sub create`(targetScope = 'subscription') or `az deployment group create`(targetScope = 'resourceGroup').
    (ONLY WHEN the project is using passwordless connection to mysql-flexible/postgres-flexible/sql) 7. Post-provisioning steps ONLY WHEN the project is using passwordless connection to mysql-flexible/postgres-flexible/sql:
        - Build connection between the database and {{AzureComputeHost}} with Managed Identity.
        - Create the connection using Service Connector: az {{TargetAppCommandTitle}} connection create mysql-flexible/postgres-flexible/sql --connection connectionname --user-identity client-id=XX subs-id=XX --source-id <azure resource id> --tg <db resource group> --server servername --database dbname -y.
        - Check the connection with 'az {{TargetAppCommandTitle}} connection show -g AppRG -n MyApp --connection MyConnection -o json'.
        - Check the 'configurations' property in the output. Each name means an env variable name and you should use it in your application to connect to the database.
