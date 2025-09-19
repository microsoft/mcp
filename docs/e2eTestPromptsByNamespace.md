# Azure MCP End-to-End Test Prompts by Namespace

This document maps each test prompt in e2eTestPrompts to its corresponding tool's *top-level* Azure MCP area/namespace.

Columns:
- Tool Name: The root tool area (namespace) to invoke. Each of these corresponds to an underlying set of subcommands/tools.
- Test Prompt: A natural language user query that should map to that namespace.

> NOTE: Some namespaces expose multiple subcommands (get, list, query, etc.). Disambiguation among subcommands happens
> after the namespace is chosen; here we focus only on correct namespace routing.

## Azure AI Foundry

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_foundry | List all knowledge indexes in my AI Foundry project |
| azmcp_foundry | Show me the knowledge indexes in my AI Foundry project |
| azmcp_foundry | Show me the schema for knowledge index \<index-name> in my AI Foundry project |
| azmcp_foundry | Get the schema configuration for knowledge index \<index-name> |
| azmcp_foundry | Deploy a GPT4o instance on my resource \<resource-name> |
| azmcp_foundry | List all AI Foundry model deployments |
| azmcp_foundry | Show me all AI Foundry model deployments |
| azmcp_foundry | List all AI Foundry models |
| azmcp_foundry | Show me the available AI Foundry models |

## Azure AI Search

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_search | Show me the details of the index \<index-name> in Cognitive Search service \<service-name> |
| azmcp_search | List all indexes in the Cognitive Search service \<service-name> |
| azmcp_search | Show me the indexes in the Cognitive Search service \<service-name> |
| azmcp_search | Search for instances of \<search_term> in the index \<index-name> in Cognitive Search service \<service-name> |
| azmcp_search | List all Cognitive Search services in my subscription |
| azmcp_search | Show me the Cognitive Search services in my subscription |
| azmcp_search | Show me my Cognitive Search services |

## Azure App Configuration

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_appconfig | List all App Configuration stores in my subscription |
| azmcp_appconfig | Show me the App Configuration stores in my subscription |
| azmcp_appconfig | Show me my App Configuration stores |
| azmcp_appconfig | Delete the key \<key_name> in App Configuration store \<app_config_store_name> |
| azmcp_appconfig | List all key-value settings in App Configuration store \<app_config_store_name> |
| azmcp_appconfig | Show me the key-value settings in App Configuration store \<app_config_store_name> |
| azmcp_appconfig | Lock the key \<key_name> in App Configuration store \<app_config_store_name> |
| azmcp_appconfig | Set the key \<key_name> in App Configuration store \<app_config_store_name> to \<value> |
| azmcp_appconfig | Show the content for the key \<key_name> in App Configuration store \<app_config_store_name> |
| azmcp_appconfig | Unlock the key \<key_name> in App Configuration store \<app_config_store_name> |

## Azure App Lens

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_applens | Please help me diagnose issues with my app using app lens |
| azmcp_applens | Use app lens to check why my app is slow? |
| azmcp_applens | What does app lens say is wrong with my service? |

## Azure Container Registry (ACR)

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_acr | List all Azure Container Registries in my subscription |
| azmcp_acr | Show me my Azure Container Registries |
| azmcp_acr | Show me the container registries in my subscription |
| azmcp_acr | List container registries in resource group \<resource_group_name> |
| azmcp_acr | Show me the container registries in resource group \<resource_group_name> |
| azmcp_acr | List all container registry repositories in my subscription |
| azmcp_acr | Show me my container registry repositories |
| azmcp_acr | List repositories in the container registry \<registry_name> |
| azmcp_acr | Show me the repositories in the container registry \<registry_name> |

## Azure Cosmos DB

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_cosmos | List all cosmosdb accounts in my subscription |
| azmcp_cosmos | Show me my cosmosdb accounts |
| azmcp_cosmos | Show me the cosmosdb accounts in my subscription |
| azmcp_cosmos | Show me the items that contain the word \<search_term> in the container \<container_name> in the database \<database_name> for the cosmosdb account \<account_name> |
| azmcp_cosmos | List all the containers in the database \<database_name> for the cosmosdb account \<account_name> |
| azmcp_cosmos | Show me the containers in the database \<database_name> for the cosmosdb account \<account_name> |
| azmcp_cosmos | List all the databases in the cosmosdb account \<account_name> |
| azmcp_cosmos | Show me the databases in the cosmosdb account \<account_name> |

## Azure Data Explorer (Kusto)

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_kusto | Show me the details of the Data Explorer cluster \<cluster_name> |
| azmcp_kusto | List all Data Explorer clusters in my subscription |
| azmcp_kusto | Show me my Data Explorer clusters |
| azmcp_kusto | Show me the Data Explorer clusters in my subscription |
| azmcp_kusto | List all databases in the Data Explorer cluster \<cluster_name> |
| azmcp_kusto | Show me the databases in the Data Explorer cluster \<cluster_name> |
| azmcp_kusto | Show me all items that contain the word \<search_term> in the Data Explorer table \<table_name> in cluster \<cluster_name> |
| azmcp_kusto | Show me a data sample from the Data Explorer table \<table_name> in cluster \<cluster_name> |
| azmcp_kusto | List all tables in the Data Explorer database \<database_name> in cluster \<cluster_name> |
| azmcp_kusto | Show me the tables in the Data Explorer database \<database_name> in cluster \<cluster_name> |
| azmcp_kusto | Show me the schema for table \<table_name> in the Data Explorer database \<database_name> in cluster \<cluster_name> |

## Azure Database for MySQL

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_mysql | List all MySQL databases in server \<server> |
| azmcp_mysql | Show me the MySQL databases in server \<server> |
| azmcp_mysql | Show me all items that contain the word \<search_term> in the MySQL database \<database> in server \<server> |
| azmcp_mysql | Show me the configuration of MySQL server \<server> |
| azmcp_mysql | List all MySQL servers in my subscription |
| azmcp_mysql | Show me my MySQL servers |
| azmcp_mysql | Show me the MySQL servers in my subscription |
| azmcp_mysql | Show me the value of connection timeout in seconds in my MySQL server \<server> |
| azmcp_mysql | Set connection timeout to 20 seconds for my MySQL server \<server> |
| azmcp_mysql | List all tables in the MySQL database \<database> in server \<server> |
| azmcp_mysql | Show me the tables in the MySQL database \<database> in server \<server> |
| azmcp_mysql | Show me the schema of table \<table> in the MySQL database \<database> in server \<server> |

## Azure Database for PostgreSQL

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_postgres | List all PostgreSQL databases in server \<server> |
| azmcp_postgres | Show me the PostgreSQL databases in server \<server> |
| azmcp_postgres | Show me all items that contain the word \<search_term> in the PostgreSQL database \<database> in server \<server> |
| azmcp_postgres | Show me the configuration of PostgreSQL server \<server> |
| azmcp_postgres | List all PostgreSQL servers in my subscription |
| azmcp_postgres | Show me my PostgreSQL servers |
| azmcp_postgres | Show me the PostgreSQL servers in my subscription |
| azmcp_postgres | Show me if the parameter my PostgreSQL server \<server> has replication enabled |
| azmcp_postgres | Enable replication for my PostgreSQL server \<server> |
| azmcp_postgres | List all tables in the PostgreSQL database \<database> in server \<server> |
| azmcp_postgres | Show me the tables in the PostgreSQL database \<database> in server \<server> |
| azmcp_postgres | Show me the schema of table \<table> in the PostgreSQL database \<database> in server \<server> |

## Azure Deploy

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_deploy | Show me the log of the application deployed by azd |
| azmcp_deploy | Generate the azure architecture diagram for this application |
| azmcp_deploy | Show me the rules to generate bicep scripts |
| azmcp_deploy | How can I create a CI/CD pipeline to deploy this app to Azure? |
| azmcp_deploy | Create a plan to deploy this application to azure |

## Azure Event Grid

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_eventgrid | List all Event Grid topics in my subscription |
| azmcp_eventgrid | Show me the Event Grid topics in my subscription |
| azmcp_eventgrid | List all Event Grid topics in subscription \<subscription> |
| azmcp_eventgrid | List all Event Grid topics in resource group \<resource_group_name> in subscription \<subscription> |
| azmcp_eventgrid | Show me all Event Grid subscriptions for topic \<topic_name> |
| azmcp_eventgrid | List Event Grid subscriptions for topic \<topic_name> in subscription \<subscription> |
| azmcp_eventgrid | List Event Grid subscriptions for topic \<topic_name> in resource group \<resource_group_name> |
| azmcp_eventgrid | Show all Event Grid subscriptions in my subscription |
| azmcp_eventgrid | List all Event Grid subscriptions in subscription \<subscription> |
| azmcp_eventgrid | Show Event Grid subscriptions in resource group \<resource_group_name> in subscription \<subscription> |
| azmcp_eventgrid | List Event Grid subscriptions for subscription \<subscription> in location \<location> |

## Azure Function App

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_functionapp | Describe the function app \<function_app_name> in resource group \<resource_group_name> |
| azmcp_functionapp | Get configuration for function app \<function_app_name> |
| azmcp_functionapp | Get function app status for \<function_app_name> |
| azmcp_functionapp | Get information about my function app \<function_app_name> in \<resource_group_name> |
| azmcp_functionapp | Retrieve host name and status of function app \<function_app_name> |
| azmcp_functionapp | Show function app details for \<function_app_name> in \<resource_group_name> |
| azmcp_functionapp | Show me the details for the function app \<function_app_name> |
| azmcp_functionapp | Show plan and region for function app \<function_app_name> |
| azmcp_functionapp | What is the status of function app \<function_app_name>? |
| azmcp_functionapp | List all function apps in my subscription |
| azmcp_functionapp | Show me my Azure function apps |
| azmcp_functionapp | What function apps do I have? |

## Azure Key Vault

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_keyvault | Create a new certificate called \<certificate_name> in the key vault \<key_vault_account_name> |
| azmcp_keyvault | Show me the certificate \<certificate_name> in the key vault \<key_vault_account_name> |
| azmcp_keyvault | Show me the details of the certificate \<certificate_name> in the key vault \<key_vault_account_name> |
| azmcp_keyvault | Import the certificate in file \<file_path> into the key vault \<key_vault_account_name> |
| azmcp_keyvault | Import a certificate into the key vault \<key_vault_account_name> using the name \<certificate_name> |
| azmcp_keyvault | List all certificates in the key vault \<key_vault_account_name> |
| azmcp_keyvault | Show me the certificates in the key vault \<key_vault_account_name> |
| azmcp_keyvault | Create a new key called \<key_name> with the RSA type in the key vault \<key_vault_account_name> |
| azmcp_keyvault | List all keys in the key vault \<key_vault_account_name> |
| azmcp_keyvault | Show me the keys in the key vault \<key_vault_account_name> |
| azmcp_keyvault | Create a new secret called \<secret_name> with value \<secret_value> in the key vault \<key_vault_account_name> |
| azmcp_keyvault | List all secrets in the key vault \<key_vault_account_name> |
| azmcp_keyvault | Show me the secrets in the key vault \<key_vault_account_name> |

## Azure Kubernetes Service (AKS)

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_aks | Get the configuration of AKS cluster \<cluster-name> |
| azmcp_aks | Show me the details of AKS cluster \<cluster-name> in resource group \<resource-group> |
| azmcp_aks | Show me the network configuration for AKS cluster \<cluster-name> |
| azmcp_aks | What are the details of my AKS cluster \<cluster-name> in \<resource-group>? |
| azmcp_aks | List all AKS clusters in my subscription |
| azmcp_aks | Show me my Azure Kubernetes Service clusters |
| azmcp_aks | What AKS clusters do I have? |
| azmcp_aks | Get details for nodepool \<nodepool-name> in AKS cluster \<cluster-name> in \<resource-group> |
| azmcp_aks | Show me the configuration for nodepool \<nodepool-name> in AKS cluster \<cluster-name> in resource group \<resource-group> |
| azmcp_aks | What is the setup of nodepool \<nodepool-name> for AKS cluster \<cluster-name> in \<resource-group>? |
| azmcp_aks | List nodepools for AKS cluster \<cluster-name> in \<resource-group> |
| azmcp_aks | Show me the nodepool list for AKS cluster \<cluster-name> in \<resource-group> |
| azmcp_aks | What nodepools do I have for AKS cluster \<cluster-name> in \<resource-group> |

## Azure Load Testing

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_loadtesting | Create a basic URL test using the following endpoint URL \<test-url> that runs for 30 minutes with 45 virtual users. The test name is \<sample-name> with the test id \<test-id> and the load testing resource is \<load-test-resource> in the resource group \<resource-group> in my subscription |
| azmcp_loadtesting | Get the load test with id \<test-id> in the load test resource \<test-resource> in resource group \<resource-group> |
| azmcp_loadtesting | Create a load test resource \<load-test-resource-name> in the resource group \<resource-group> in my subscription |
| azmcp_loadtesting | List all load testing resources in the resource group \<resource-group> in my subscription |
| azmcp_loadtesting | Create a test run using the id \<testrun-id> for test \<test-id> in the load testing resource \<load-testing-resource> in resource group \<resource-group>. Use the name of test run \<display-name> and description as \<description> |
| azmcp_loadtesting | Get the load test run with id \<testrun-id> in the load test resource \<test-resource> in resource group \<resource-group> |
| azmcp_loadtesting | Get all the load test runs for the test with id \<test-id> in the load test resource \<test-resource> in resource group \<resource-group> |
| azmcp_loadtesting | Update a test run display name as \<display-name> for the id \<testrun-id> for test \<test-id> in the load testing resource \<load-testing-resource> in resource group \<resource-group>. |

## Azure Managed Grafana

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_grafana | List all Azure Managed Grafana in one subscription |

## Azure Managed Lustre

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_azuremanagedlustre | List the Azure Managed Lustre filesystems in my subscription \<subscription_name> |
| azmcp_azuremanagedlustre | List the Azure Managed Lustre filesystems in my resource group \<resource_group_name> |
| azmcp_azuremanagedlustre | Tell me how many IP addresses I need for \<filesystem_size> of \<amlfs_sku> |
| azmcp_azuremanagedlustre | List the Azure Managed Lustre SKUs available in \<location> |

## Azure Marketplace

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_marketplace | Get details about marketplace product \<product_name> |
| azmcp_marketplace | Search for Microsoft products in the marketplace |
| azmcp_marketplace | Show me marketplace products from publisher \<publisher_name> |

## Azure MCP Best Practices

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_bestpractices | Get the latest Azure code generation best practices |
| azmcp_bestpractices | Get the latest Azure deployment best practices |
| azmcp_bestpractices | Get the latest Azure best practices |
| azmcp_bestpractices | Get the latest Azure Functions code generation best practices |
| azmcp_bestpractices | Get the latest Azure Functions deployment best practices |
| azmcp_bestpractices | Get the latest Azure Functions best practices |
| azmcp_bestpractices | Get the latest Azure Static Web Apps best practices |
| azmcp_bestpractices | What are azure function best practices? |
| azmcp_bestpractices | Create the plan for creating a simple HTTP-triggered function app in javascript that returns a random compliment from a predefined list in a JSON response. And deploy it to azure eventually. But don't create any code until I confirm. |
| azmcp_bestpractices | Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm. |

## Azure Monitor

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_monitor | Show me the health status of entity \<entity_id> in the Log Analytics workspace \<workspace_name> |
| azmcp_monitor | Get metric definitions for \<resource_type> \<resource_name> from the namespace |
| azmcp_monitor | Show me all available metrics and their definitions for storage account \<account_name> |
| azmcp_monitor | What metric definitions are available for the Application Insights resource \<resource_name> |
| azmcp_monitor | Analyze the performance trends and response times for Application Insights resource \<resource_name> over the last \<time_period> |
| azmcp_monitor | Check the availability metrics for my Application Insights resource \<resource_name> for the last \<time_period> |
| azmcp_monitor | Get the \<aggregation_type> \<metric_name> metric for \<resource_type> \<resource_name> over the last \<time_period> with intervals |
| azmcp_monitor | Investigate error rates and failed requests for Application Insights resource \<resource_name> for the last \<time_period> |
| azmcp_monitor | Query the \<metric_name> metric for \<resource_type> \<resource_name> for the last \<time_period> |
| azmcp_monitor | What's the request per second rate for my Application Insights resource \<resource_name> over the last \<time_period> |
| azmcp_monitor | Show me the logs for the past hour for the resource \<resource_name> in the Log Analytics workspace \<workspace_name> |
| azmcp_monitor | List all tables in the Log Analytics workspace \<workspace_name> |
| azmcp_monitor | Show me the tables in the Log Analytics workspace \<workspace_name> |
| azmcp_monitor | List all available table types in the Log Analytics workspace \<workspace_name> |
| azmcp_monitor | Show me the available table types in the Log Analytics workspace \<workspace_name> |
| azmcp_monitor | List all Log Analytics workspaces in my subscription |
| azmcp_monitor | Show me my Log Analytics workspaces |
| azmcp_monitor | Show me the Log Analytics workspaces in my subscription |
| azmcp_monitor | Show me the logs for the past hour in the Log Analytics workspace \<workspace_name> |

## Azure Quick Review CLI

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_extension | Check my Azure subscription for any compliance issues or recommendations |
| azmcp_extension | Provide compliance recommendations for my current Azure subscription |
| azmcp_extension | Scan my Azure subscription for compliance recommendations |

## Azure Native ISV (Datadog)

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_datadog | List all monitored resources in the Datadog resource \<resource_name> |
| azmcp_datadog | Show me the monitored resources in the Datadog resource \<resource_name> |

## Azure Quota

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_quota | Show me the available regions for these resource types \<resource_types> |
| azmcp_quota | Check usage information for \<resource_type> in region \<region> |

## Azure RBAC

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_role | List all available role assignments in my subscription |
| azmcp_role | Show me the available role assignments in my subscription |

## Azure Redis

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_redis | List all access policies in the Redis Cache \<cache_name> |
| azmcp_redis | Show me the access policies in the Redis Cache \<cache_name> |
| azmcp_redis | List all Redis Caches in my subscription |
| azmcp_redis | Show me my Redis Caches |
| azmcp_redis | Show me the Redis Caches in my subscription |
| azmcp_redis | List all databases in the Redis Cluster \<cluster_name> |
| azmcp_redis | Show me the databases in the Redis Cluster \<cluster_name> |
| azmcp_redis | List all Redis Clusters in my subscription |
| azmcp_redis | Show me my Redis Clusters |
| azmcp_redis | Show me the Redis Clusters in my subscription |

## Azure Resource Group

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_group | List all resource groups in my subscription |
| azmcp_group | Show me my resource groups |
| azmcp_group | Show me the resource groups in my subscription |

## Azure Resource Health

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_resourcehealth | Get the availability status for resource \<resource_name> |
| azmcp_resourcehealth | Show me the health status of the storage account \<storage_account_name> |
| azmcp_resourcehealth | What is the availability status of virtual machine \<vm_name> in resource group \<resource_group_name>? |
| azmcp_resourcehealth | List availability status for all resources in my subscription |
| azmcp_resourcehealth | Show me the health status of all my Azure resources |
| azmcp_resourcehealth | What resources in resource group \<resource_group_name> have health issues? |
| azmcp_resourcehealth | List all service health events in my subscription |
| azmcp_resourcehealth | Show me Azure service health events for subscription \<subscription_id> |
| azmcp_resourcehealth | What service issues have occurred in the last 30 days? |
| azmcp_resourcehealth | List active service health events in my subscription |
| azmcp_resourcehealth | Show me planned maintenance events for my Azure services |

## Azure Service Bus

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_servicebus | Show me the details of service bus \<service_bus_name> queue \<queue_name> |
| azmcp_servicebus | Show me the details of service bus \<service_bus_name> topic \<topic_name> |
| azmcp_servicebus | Show me the details of service bus \<service_bus_name> subscription \<subscription_name> |

## Azure SQL Database

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_sql | Create a new SQL database named \<database_name> in server \<server_name> |
| azmcp_sql | Create a SQL database \<database_name> with Basic tier in server \<server_name> |
| azmcp_sql | Create a new database called \<database_name> on SQL server \<server_name> in resource group \<resource_group_name> |
| azmcp_sql | Delete the SQL database \<database_name> from server \<server_name> |
| azmcp_sql | Remove database \<database_name> from SQL server \<server_name> in resource group \<resource_group_name> |
| azmcp_sql | Delete the database called \<database_name> on server \<server_name> |
| azmcp_sql | List all databases in the Azure SQL server \<server_name> |
| azmcp_sql | Show me all the databases configuration details in the Azure SQL server \<server_name> |
| azmcp_sql | Get the configuration details for the SQL database \<database_name> on server \<server_name> |
| azmcp_sql | Show me the details of SQL database \<database_name> in server \<server_name> |
| azmcp_sql | List all elastic pools in SQL server \<server_name> |
| azmcp_sql | Show me the elastic pools configured for SQL server \<server_name> |
| azmcp_sql | What elastic pools are available in my SQL server \<server_name>? |
| azmcp_sql | Create a new Azure SQL server named \<server_name> in resource group \<resource_group_name> |
| azmcp_sql | Create an Azure SQL server with name \<server_name> in location \<location> with admin user \<admin_user> |
| azmcp_sql | Set up a new SQL server called \<server_name> in my resource group \<resource_group_name> |
| azmcp_sql | Delete the Azure SQL server \<server_name> from resource group \<resource_group_name> |
| azmcp_sql | Remove the SQL server \<server_name> from my subscription |
| azmcp_sql | Delete SQL server \<server_name> permanently |
| azmcp_sql | List Microsoft Entra ID administrators for SQL server \<server_name> |
| azmcp_sql | Show me the Entra ID administrators configured for SQL server \<server_name> |
| azmcp_sql | What Microsoft Entra ID administrators are set up for my SQL server \<server_name>? |
| azmcp_sql | Create a firewall rule for my Azure SQL server \<server_name> |
| azmcp_sql | Add a firewall rule to allow access from IP range \<start_ip> to \<end_ip> for SQL server \<server_name> |
| azmcp_sql | Create a new firewall rule named \<rule_name> for SQL server \<server_name> |
| azmcp_sql | Delete a firewall rule from my Azure SQL server \<server_name> |
| azmcp_sql | Remove the firewall rule \<rule_name> from SQL server \<server_name> |
| azmcp_sql | Delete firewall rule \<rule_name> for SQL server \<server_name> |
| azmcp_sql | List all firewall rules for SQL server \<server_name> |
| azmcp_sql | Show me the firewall rules for SQL server \<server_name> |
| azmcp_sql | What firewall rules are configured for my SQL server \<server_name>? |
| azmcp_sql | Show me the details of Azure SQL server \<server_name> in resource group \<resource_group_name> |
| azmcp_sql | Get the configuration details for SQL server \<server_name> |
| azmcp_sql | Display the properties of SQL server \<server_name> |

## Azure Storage

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_storage | Create a new storage account called testaccount123 in East US region |
| azmcp_storage | Create a storage account with premium performance and LRS replication |
| azmcp_storage | Create a new storage account with Data Lake Storage Gen2 enabled |
| azmcp_storage | Show me the details for my storage account \<account> |
| azmcp_storage | Get details about the storage account \<account> |
| azmcp_storage | List all storage accounts in my subscription including their location and SKU |
| azmcp_storage | Show me my storage accounts with whether hierarchical namespace (HNS) is enabled |
| azmcp_storage | Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings |
| azmcp_storage | Set access tier to Cool for multiple blobs in the container \<container> in the storage account \<account> |
| azmcp_storage | Change the access tier to Archive for blobs file1.txt and file2.txt in the container \<container> in the storage account \<account> |
| azmcp_storage | Create the storage container mycontainer in storage account \<account> |
| azmcp_storage | Create the container using blob public access in storage account \<account> |
| azmcp_storage | Create a new blob container named documents with container public access in storage account \<account> |
| azmcp_storage | Show me the properties of the storage container \<container> in the storage account \<account> |
| azmcp_storage | List all blob containers in the storage account \<account> |
| azmcp_storage | Show me the containers in the storage account \<account> |
| azmcp_storage | Show me the properties for blob \<blob> in container \<container> in storage account \<account> |
| azmcp_storage | Get the details about blob \<blob> in the container \<container> in storage account \<account> |
| azmcp_storage | List all blobs in the blob container \<container> in the storage account \<account> |
| azmcp_storage | Show me the blobs in the blob container \<container> in the storage account \<account> |
| azmcp_storage | Upload file \<local-file-path> to storage blob \<blob> in container \<container> in storage account \<account> |
| azmcp_storage | Create a new directory at the path \<directory_path> in Data Lake in the storage account \<account> |
| azmcp_storage | List all paths in the Data Lake file system \<file_system> in the storage account \<account> |
| azmcp_storage | Show me the paths in the Data Lake file system \<file_system> in the storage account \<account> |
| azmcp_storage | Recursively list all paths in the Data Lake file system \<file_system> in the storage account \<account> filtered by \<filter_path> |
| azmcp_storage | Send a message "Hello, World!" to the queue \<queue> in storage account \<account> |
| azmcp_storage | Send a message with TTL of 3600 seconds to the queue \<queue> in storage account \<account> |
| azmcp_storage | Add a message to the queue \<queue> in storage account \<account> with visibility timeout of 30 seconds |
| azmcp_storage | List all files and directories in the File Share \<share> in the storage account \<account> |
| azmcp_storage | Show me the files in the File Share \<share> directory \<directory_path> in the storage account \<account> |
| azmcp_storage | List files with prefix 'report' in the File Share \<share> in the storage account \<account> |
| azmcp_storage | List all tables in the storage account \<account> |
| azmcp_storage | Show me the tables in the storage account \<account> |

## Azure Subscription

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_subscription | List all subscriptions for my account |
| azmcp_subscription | Show me my subscriptions |
| azmcp_subscription | What is my current subscription? |
| azmcp_subscription | What subscriptions do I have? |

## Azure Terraform Best Practices

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_azureterraformbestpractices | Fetch the Azure Terraform best practices |
| azmcp_azureterraformbestpractices | Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault |

## Azure Virtual Desktop

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_virtualdesktop | List all host pools in my subscription |
| azmcp_virtualdesktop | List all session hosts in host pool \<hostpool_name> |
| azmcp_virtualdesktop | List all user sessions on session host \<sessionhost_name> in host pool \<hostpool_name> |

## Azure Workbooks

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_workbooks | Create a new workbook named \<workbook_name> |
| azmcp_workbooks | Delete the workbook with resource ID \<workbook_resource_id> |
| azmcp_workbooks | List all workbooks in my resource group \<resource_group_name> |
| azmcp_workbooks | What workbooks do I have in resource group \<resource_group_name>? |
| azmcp_workbooks | Get information about the workbook with resource ID \<workbook_resource_id> |
| azmcp_workbooks | Show me the workbook with display name \<workbook_display_name> |
| azmcp_workbooks | Update the workbook \<workbook_resource_id> with a new text step |

## Bicep

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_bicepschema | How can I use Bicep to create an Azure OpenAI service? |

## Cloud Architect

| Tool Name | Test Prompt |
| --------- | ----------- |
| azmcp_cloudarchitect | Please help me design an architecture for a large-scale file upload, storage, and retrieval service |
| azmcp_cloudarchitect | Help me create a cloud service that will serve as ATM for users |
| azmcp_cloudarchitect | I want to design a cloud app for ordering groceries |
| azmcp_cloudarchitect | How can I design a cloud service in Azure that will store and present videos for users? |