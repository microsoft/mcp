# Azure MCP End-to-End Test Prompts

This file contains prompts used for end-to-end testing to ensure each tool is invoked properly by MCP clients. The tables are organized by Azure MCP Server areas in alphabetical order, with Tool Names sorted alphabetically within each table.

## Azure AI Foundry

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_foundry_agents_connect | Query an agent in my AI foundry project |
| azmcp_foundry_agents_evaluate | Evaluate the full query and response I got from my agent for task_adherence |
| azmcp_foundry_agents_query-and-evaluate | Query and evaluate an agent in my AI Foundry project for task_adherence |
| azmcp_foundry_knowledge_index_list | List all knowledge indexes in my AI Foundry project |
| azmcp_foundry_knowledge_index_list | Show me the knowledge indexes in my AI Foundry project |
| azmcp_foundry_knowledge_index_schema | Show me the schema for knowledge index \<index-name> in my AI Foundry project |
| azmcp_foundry_knowledge_index_schema | Get the schema configuration for knowledge index \<index-name> |
| azmcp_foundry_models_deploy | Deploy a GPT4o instance on my resource \<resource-name> |
| azmcp_foundry_models_deployments_list | List all AI Foundry model deployments |
| azmcp_foundry_models_deployments_list | Show me all AI Foundry model deployments |
| azmcp_foundry_models_list | List all AI Foundry models |
| azmcp_foundry_models_list | Show me the available AI Foundry models |
| azmcp_foundry_openai_create-completion | Create a completion with the prompt "What is Azure?"|

## Azure AI Search

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_search_index_get | Show me the details of the index \<index-name> in Cognitive Search service \<service-name> |
| azmcp_search_index_get | List all indexes in the Cognitive Search service \<service-name> |
| azmcp_search_index_get | Show me the indexes in the Cognitive Search service \<service-name> |
| azmcp_search_index_query | Search for instances of \<search_term> in the index \<index-name> in Cognitive Search service \<service-name> |
| azmcp_search_service_list | List all Cognitive Search services in my subscription |
| azmcp_search_service_list | Show me the Cognitive Search services in my subscription |
| azmcp_search_service_list | Show me my Cognitive Search services |

## Azure AI Services Speech

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_speech_stt_recognize | Convert this audio file to text using Azure Speech Services |
| azmcp_speech_stt_recognize | Recognize speech from my audio file with language detection |
| azmcp_speech_stt_recognize | Transcribe speech from audio file <file_path> with profanity filtering |
| azmcp_speech_stt_recognize | Convert speech to text from audio file <file_path> using endpoint <endpoint> |
| azmcp_speech_stt_recognize | Transcribe the audio file <file_path> in Spanish language |
| azmcp_speech_stt_recognize | Convert speech to text with detailed output format from audio file <file_path> |
| azmcp_speech_stt_recognize | Recognize speech from <file_path> with phrase hints for better accuracy |
| azmcp_speech_stt_recognize | Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning" |
| azmcp_speech_stt_recognize | Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API" |
| azmcp_speech_stt_recognize | Transcribe audio with raw profanity output from file <file_path> |

## Azure App Configuration

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_appconfig_account_list | List all App Configuration stores in my subscription |
| azmcp_appconfig_account_list | Show me the App Configuration stores in my subscription |
| azmcp_appconfig_account_list | Show me my App Configuration stores |
| azmcp_appconfig_kv_delete | Delete the key <key_name> in App Configuration store <app_config_store_name> |
| azmcp_appconfig_kv_get | List all key-value settings in App Configuration store <app_config_store_name> |
| azmcp_appconfig_kv_get | Show me the key-value settings in App Configuration store <app_config_store_name> |
| azmcp_appconfig_kv_get | List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name> |
| azmcp_appconfig_kv_get | Show the content for the key <key_name> in App Configuration store <app_config_store_name> |
| azmcp_appconfig_kv_lock_set | Lock the key <key_name> in App Configuration store <app_config_store_name> |
| azmcp_appconfig_kv_lock_set | Unlock the key <key_name> in App Configuration store <app_config_store_name> |
| azmcp_appconfig_kv_set | Set the key <key_name> in App Configuration store <app_config_store_name> to \<value> |

## Azure App Lens

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_applens_resource_diagnose | Please help me diagnose issues with my app using app lens |
| azmcp_applens_resource_diagnose | Use app lens to check why my app is slow? |
| azmcp_applens_resource_diagnose | What does app lens say is wrong with my service? |
## Azure App Service

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_appservice_database_add | Add a database connection to my app service <app_name> in resource group <resource_group> |
| azmcp_appservice_database_add | Configure a SQL Server database for app service <app_name> |
| azmcp_appservice_database_add | Add a MySQL database to app service <app_name> |
| azmcp_appservice_database_add | Add a PostgreSQL database to app service <app_name> |
| azmcp_appservice_database_add | Add a CosmosDB database to app service <app_name> |
| azmcp_appservice_database_add | Add database <database_name> on server <database_server> to app service <app_name> |
| azmcp_appservice_database_add | Set connection string for database <database_name> in app service <app_name> |
| azmcp_appservice_database_add | Configure tenant <tenant> for database <database_name> in app service <app_name> |
| azmcp_appservice_database_add | Add database <database_name> with retry policy to app service <app_name> |

## Azure Application Insights

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_applicationinsights_recommendation_list | List code optimization recommendations across my Application Insights components |
| azmcp_applicationinsights_recommendation_list | Show me code optimization recommendations for all Application Insights resources in my subscription |
| azmcp_applicationinsights_recommendation_list | List profiler recommendations for Application Insights in resource group <resource_group_name> |
| azmcp_applicationinsights_recommendation_list | Show me performance improvement recommendations from Application Insights |

## Azure Container Registry (ACR)

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_acr_registry_list | List all Azure Container Registries in my subscription |
| azmcp_acr_registry_list | Show me my Azure Container Registries |
| azmcp_acr_registry_list | Show me the container registries in my subscription |
| azmcp_acr_registry_list | List container registries in resource group <resource_group_name> |
| azmcp_acr_registry_list | Show me the container registries in resource group <resource_group_name> |
| azmcp_acr_registry_repository_list | List all container registry repositories in my subscription |
| azmcp_acr_registry_repository_list | Show me my container registry repositories |
| azmcp_acr_registry_repository_list | List repositories in the container registry <registry_name> |
| azmcp_acr_registry_repository_list | Show me the repositories in the container registry <registry_name> |

## Azure Communication Services

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_communication_sms_send | Send an SMS message to +1234567890 saying "Hello" |
| azmcp_communication_sms_send | Send SMS to +1234567890 from +1234567891 with message "Test message" |
| azmcp_communication_sms_send | Send SMS to multiple recipients: +1234567890, +1234567891 |
| azmcp_communication_sms_send | Send SMS with delivery reporting enabled |
| azmcp_communication_sms_send | Send SMS message with custom tracking tag "campaign1" |
| azmcp_communication_sms_send | Send broadcast SMS to +1234567890 and +1234567891 saying "Urgent notification" |
| azmcp_communication_sms_send | Send SMS from my communication service to +1234567890 |
| azmcp_communication_sms_send | Send an SMS with delivery receipt tracking |

## Azure Cosmos DB

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_cosmos_account_list | List all cosmosdb accounts in my subscription |
| azmcp_cosmos_account_list | Show me my cosmosdb accounts |
| azmcp_cosmos_account_list | Show me the cosmosdb accounts in my subscription |
| azmcp_cosmos_database_container_item_query | Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name> |
| azmcp_cosmos_database_container_list | List all the containers in the database <database_name> for the cosmosdb account <account_name> |
| azmcp_cosmos_database_container_list | Show me the containers in the database <database_name> for the cosmosdb account <account_name> |
| azmcp_cosmos_database_list | List all the databases in the cosmosdb account <account_name> |
| azmcp_cosmos_database_list | Show me the databases in the cosmosdb account <account_name> |

## Azure Data Explorer

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_kusto_cluster_get | Show me the details of the Data Explorer cluster <cluster_name> |
| azmcp_kusto_cluster_list | List all Data Explorer clusters in my subscription |
| azmcp_kusto_cluster_list | Show me my Data Explorer clusters |
| azmcp_kusto_cluster_list | Show me the Data Explorer clusters in my subscription |
| azmcp_kusto_database_list | List all databases in the Data Explorer cluster <cluster_name> |
| azmcp_kusto_database_list | Show me the databases in the Data Explorer cluster <cluster_name> |
| azmcp_kusto_query | Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name> |
| azmcp_kusto_sample | Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name> |
| azmcp_kusto_table_list | List all tables in the Data Explorer database <database_name> in cluster <cluster_name> |
| azmcp_kusto_table_list | Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name> |
| azmcp_kusto_table_schema | Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name> |

## Azure Database for MySQL

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_mysql_database_list | List all MySQL databases in server \<server> |
| azmcp_mysql_database_list | Show me the MySQL databases in server \<server> |
| azmcp_mysql_database_query | Show me all items that contain the word \<search_term> in the MySQL database \<database> in server \<server> |
| azmcp_mysql_server_config_get | Show me the configuration of MySQL server \<server> |
| azmcp_mysql_server_list | List all MySQL servers in my subscription |
| azmcp_mysql_server_list | Show me my MySQL servers |
| azmcp_mysql_server_list | Show me the MySQL servers in my subscription |
| azmcp_mysql_server_param_get | Show me the value of connection timeout in seconds in my MySQL server \<server>  |
| azmcp_mysql_server_param_set | Set connection timeout to 20 seconds for my MySQL server \<server> |
| azmcp_mysql_table_list | List all tables in the MySQL database \<database> in server \<server> |
| azmcp_mysql_table_list | Show me the tables in the MySQL database \<database> in server \<server> |
| azmcp_mysql_table_schema_get | Show me the schema of table \<table> in the MySQL database \<database> in server \<server> |

## Azure Database for PostgreSQL

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_postgres_database_list | List all PostgreSQL databases in server \<server> |
| azmcp_postgres_database_list | Show me the PostgreSQL databases in server \<server> |
| azmcp_postgres_database_query | Show me all items that contain the word \<search_term> in the PostgreSQL database \<database> in server \<server> |
| azmcp_postgres_server_config_get | Show me the configuration of PostgreSQL server \<server> |
| azmcp_postgres_server_list | List all PostgreSQL servers in my subscription |
| azmcp_postgres_server_list | Show me my PostgreSQL servers |
| azmcp_postgres_server_list | Show me the PostgreSQL servers in my subscription |
| azmcp_postgres_server_param_get | Show me if the parameter my PostgreSQL server \<server> has replication enabled |
| azmcp_postgres_server_param_set | Enable replication for my PostgreSQL server \<server> |
| azmcp_postgres_table_list | List all tables in the PostgreSQL database \<database> in server \<server> |
| azmcp_postgres_table_list | Show me the tables in the PostgreSQL database \<database> in server \<server> |
| azmcp_postgres_table_schema_get | Show me the schema of table \<table> in the PostgreSQL database \<database> in server \<server> |

## Azure Deploy

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_deploy_app_logs_get | Show me the log of the application deployed by azd  |
| azmcp_deploy_architecture_diagram_generate | Generate the azure architecture diagram for this application |
| azmcp_deploy_iac_rules_get | Show me the rules to generate bicep scripts  |
| azmcp_deploy_pipeline_guidance_get | How can I create a CI/CD pipeline to deploy this app to Azure? |
| azmcp_deploy_plan_get | Create a plan to deploy this application to azure |

## Azure Event Grid

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_eventgrid_events_publish | Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data> |
| azmcp_eventgrid_events_publish | Publish event to my Event Grid topic <topic_name> with the following events <event_data> |
| azmcp_eventgrid_events_publish | Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data> |
| azmcp_eventgrid_topic_list | List all Event Grid topics in my subscription |
| azmcp_eventgrid_topic_list | Show me the Event Grid topics in my subscription |
| azmcp_eventgrid_topic_list | List all Event Grid topics in subscription <subscription> |
| azmcp_eventgrid_topic_list | List all Event Grid topics in resource group <resource_group_name> in subscription <subscription> |
| azmcp_eventgrid_subscription_list | Show me all Event Grid subscriptions for topic <topic_name> |
| azmcp_eventgrid_subscription_list | List Event Grid subscriptions for topic <topic_name> in subscription <subscription> |
| azmcp_eventgrid_subscription_list | List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name> |
| azmcp_eventgrid_subscription_list | Show all Event Grid subscriptions in my subscription |
| azmcp_eventgrid_subscription_list | List all Event Grid subscriptions in subscription <subscription> |
| azmcp_eventgrid_subscription_list | Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription> |
| azmcp_eventgrid_subscription_list | List Event Grid subscriptions for subscription <subscription> in location <location> |

## Azure Event Hubs

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_eventhubs_namespace_get | List all Event Hubs namespaces in my subscription |
| azmcp_eventhubs_namespace_get | Get the details of my namespace <namespace_name> in my resource group <resource_group_name> |

## Azure Function App

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_functionapp_get | Describe the function app <function_app_name> in resource group <resource_group_name> |
| azmcp_functionapp_get | Get configuration for function app <function_app_name> |
| azmcp_functionapp_get | Get function app status for <function_app_name> |
| azmcp_functionapp_get | Get information about my function app <function_app_name> in <resource_group_name> |
| azmcp_functionapp_get | Retrieve host name and status of function app <function_app_name> |
| azmcp_functionapp_get | Show function app details for <function_app_name> in <resource_group_name> |
| azmcp_functionapp_get | Show me the details for the function app <function_app_name> |
| azmcp_functionapp_get | Show plan and region for function app <function_app_name> |
| azmcp_functionapp_get | What is the status of function app <function_app_name>? |
| azmcp_functionapp_get | List all function apps in my subscription |
| azmcp_functionapp_get | Show me my Azure function apps |
| azmcp_functionapp_get | What function apps do I have? |

## Azure Key Vault

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_keyvault_admin_settings_get | Get the account settings for my key vault <key_vault_account_name> |
| azmcp_keyvault_admin_settings_get | Show me the account settings for managed HSM keyvault <key_vault_account_name> |
| azmcp_keyvault_admin_settings_get | What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name> |
| azmcp_keyvault_certificate_create | Create a new certificate called <certificate_name> in the key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_create | Generate a certificate named <certificate_name> in key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_create | Request creation of certificate <certificate_name> in the key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_create | Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name> |
| azmcp_keyvault_certificate_create | Issue a certificate <certificate_name> in key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_get | Show me the certificate <certificate_name> in the key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_get | Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_get | Get the certificate <certificate_name> from vault <key_vault_account_name> |
| azmcp_keyvault_certificate_get | Display the certificate details for <certificate_name> in vault <key_vault_account_name> |
| azmcp_keyvault_certificate_get | Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name> |
| azmcp_keyvault_certificate_import | Import the certificate in file <file_path> into the key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_import | Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name> |
| azmcp_keyvault_certificate_import | Upload certificate file <file_path> to key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_import | Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name> |
| azmcp_keyvault_certificate_import | Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name> |
| azmcp_keyvault_certificate_list | List all certificates in the key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_list | Show me the certificates in the key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_list | What certificates are in the key vault <key_vault_account_name>? |
| azmcp_keyvault_certificate_list | List certificate names in vault <key_vault_account_name> |
| azmcp_keyvault_certificate_list | Enumerate certificates in key vault <key_vault_account_name> |
| azmcp_keyvault_certificate_list | Show certificate names in the key vault <key_vault_account_name> |
| azmcp_keyvault_key_create | Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name> |
| azmcp_keyvault_key_create | Generate a key <key_name> with type <key_type> in vault <key_vault_account_name> |
| azmcp_keyvault_key_create | Create an oct key in the vault <key_vault_account_name> |
| azmcp_keyvault_key_create | Create an RSA key in the vault <key_vault_account_name> with name <key_name> |
| azmcp_keyvault_key_create | Create an EC key with name <key_name> in the vault <key_vault_account_name> |
| azmcp_keyvault_key_get | Show me the key <key_name> in the key vault <key_vault_account_name> |
| azmcp_keyvault_key_get | Show me the details of the key <key_name> in the key vault <key_vault_account_name> |
| azmcp_keyvault_key_get | Get the key <key_name> from vault <key_vault_account_name> |
| azmcp_keyvault_key_get | Display the key details for <key_name> in vault <key_vault_account_name> |
| azmcp_keyvault_key_get | Retrieve key metadata for <key_name> in vault <key_vault_account_name> |
| azmcp_keyvault_key_list | List all keys in the key vault <key_vault_account_name> |
| azmcp_keyvault_key_list | Show me the keys in the key vault <key_vault_account_name> |
| azmcp_keyvault_key_list | What keys are in the key vault <key_vault_account_name>? |
| azmcp_keyvault_key_list | List key names in vault <key_vault_account_name> |
| azmcp_keyvault_key_list | Enumerate keys in key vault <key_vault_account_name> |
| azmcp_keyvault_key_list | Show key names in the key vault <key_vault_account_name> |
| azmcp_keyvault_secret_create | Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name> |
| azmcp_keyvault_secret_create | Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name> |
| azmcp_keyvault_secret_create | Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name> |
| azmcp_keyvault_secret_create | Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name> |
| azmcp_keyvault_secret_create | Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name> |
| azmcp_keyvault_secret_get | Show me the secret <secret_name> in the key vault <key_vault_account_name> |
| azmcp_keyvault_secret_get | Show me the details of the secret <secret_name> in the key vault <key_vault_account_name> |
| azmcp_keyvault_secret_get | Get the secret <secret_name> from vault <key_vault_account_name> |
| azmcp_keyvault_secret_get | Display the secret details for <secret_name> in vault <key_vault_account_name> |
| azmcp_keyvault_secret_get | Retrieve secret metadata for <secret_name> in vault <key_vault_account_name> |
| azmcp_keyvault_secret_list | List all secrets in the key vault <key_vault_account_name> |
| azmcp_keyvault_secret_list | Show me the secrets in the key vault <key_vault_account_name> |
| azmcp_keyvault_secret_list | What secrets are in the key vault <key_vault_account_name>? |
| azmcp_keyvault_secret_list | List secrets names in vault <key_vault_account_name> |
| azmcp_keyvault_secret_list | Enumerate secrets in key vault <key_vault_account_name> |
| azmcp_keyvault_secret_list | Show secrets names in the key vault <key_vault_account_name> |

## Azure Kubernetes Service (AKS)

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_aks_cluster_get | Get the configuration of AKS cluster \<cluster-name> |
| azmcp_aks_cluster_get | Show me the details of AKS cluster \<cluster-name> in resource group \<resource-group> |
| azmcp_aks_cluster_get | Show me the network configuration for AKS cluster \<cluster-name> |
| azmcp_aks_cluster_get | What are the details of my AKS cluster \<cluster-name> in \<resource-group>? |
| azmcp_aks_cluster_list | List all AKS clusters in my subscription |
| azmcp_aks_cluster_list | Show me my Azure Kubernetes Service clusters |
| azmcp_aks_cluster_list | What AKS clusters do I have? |
| azmcp_aks_nodepool_get | Get details for nodepool \<nodepool-name> in AKS cluster \<cluster-name> in \<resource-group> |
| azmcp_aks_nodepool_get | Show me the configuration for nodepool \<nodepool-name> in AKS cluster \<cluster-name> in resource group \<resource-group> |
| azmcp_aks_nodepool_get | What is the setup of nodepool \<nodepool-name> for AKS cluster \<cluster-name> in \<resource-group>? |
| azmcp_aks_nodepool_list | List nodepools for AKS cluster \<cluster-name> in \<resource-group> |
| azmcp_aks_nodepool_list | Show me the nodepool list for AKS cluster \<cluster-name> in \<resource-group> |
| azmcp_aks_nodepool_list | What nodepools do I have for AKS cluster \<cluster-name> in \<resource-group> |

## Azure Load Testing

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_loadtesting_test_create | Create a basic URL test using the following endpoint URL \<test-url> that runs for 30 minutes with 45 virtual users. The test name is \<sample-name> with the test id \<test-id> and the load testing resource is \<load-test-resource> in the resource group \<resource-group> in my subscription |
| azmcp_loadtesting_test_get | Get the load test with id \<test-id> in the load test resource \<test-resource> in resource group \<resource-group> |
| azmcp_loadtesting_testresource_create | Create a load test resource \<load-test-resource-name> in the resource group \<resource-group> in my subscription |
| azmcp_loadtesting_testresource_list | List all load testing resources in the resource group \<resource-group> in my subscription |
| azmcp_loadtesting_testrun_create | Create a test run using the id \<testrun-id> for test \<test-id> in the load testing resource \<load-testing-resource> in resource group \<resource-group>. Use the name of test run \<display-name> and description as \<description> |
| azmcp_loadtesting_testrun_get | Get the load test run with id \<testrun-id> in the load test resource \<test-resource> in resource group \<resource-group> |
| azmcp_loadtesting_testrun_list |  Get all the load test runs for the test with id \<test-id> in the load test resource \<test-resource> in resource group \<resource-group> |
| azmcp_loadtesting_testrun_update | Update a test run display name as \<display-name> for the id \<testrun-id> for test \<test-id> in the load testing resource \<load-testing-resource> in resource group \<resource-group>.|

## Azure Managed Grafana

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_grafana_list | List all Azure Managed Grafana in one subscription |

## Azure Managed Lustre

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_azuremanagedlustre_filesystem_list | List the Azure Managed Lustre filesystems in my subscription <subscription_name> |
| azmcp_azuremanagedlustre_filesystem_list | List the Azure Managed Lustre filesystems in my resource group <resource_group_name> |
| azmcp_azuremanagedlustre_filesystem_required-subnet-size | Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku> |
| azmcp_azuremanagedlustre_filesystem_sku_get | List the Azure Managed Lustre SKUs available in <location> |

## Azure Marketplace

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_marketplace_product_get | Get details about marketplace product <product_name> |
| azmcp_marketplace_product_list | Search for Microsoft products in the marketplace |
| azmcp_marketplace_product_list | Show me marketplace products from publisher <publisher_name> |

## Azure MCP Best Practices

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_get_bestpractices_get | Get the latest Azure code generation best practices |
| azmcp_get_bestpractices_get | Get the latest Azure deployment best practices |
| azmcp_get_bestpractices_get | Get the latest Azure best practices |
| azmcp_get_bestpractices_get | Get the latest Azure Functions code generation best practices |
| azmcp_get_bestpractices_get | Get the latest Azure Functions deployment best practices|
| azmcp_get_bestpractices_get | Get the latest Azure Functions best practices |
| azmcp_get_bestpractices_get | Get the latest Azure Static Web Apps best practices |
| azmcp_get_bestpractices_get | What are azure function best practices? |

## Azure Monitor

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_monitor_healthmodels_entity_gethealth | Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name> |
| azmcp_monitor_metrics_definitions | Get metric definitions for <resource_type> <resource_name> from the namespace |
| azmcp_monitor_metrics_definitions | Show me all available metrics and their definitions for storage account <account_name> |
| azmcp_monitor_metrics_definitions | What metric definitions are available for the Application Insights resource <resource_name> |
| azmcp_monitor_metrics_query | Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period> |
| azmcp_monitor_metrics_query | Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period> |
| azmcp_monitor_metrics_query | Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals |
| azmcp_monitor_metrics_query | Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period> |
| azmcp_monitor_metrics_query | Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period> |
| azmcp_monitor_metrics_query | What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period> |
| azmcp_monitor_resource_log_query | Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name> |
| azmcp_monitor_table_list | List all tables in the Log Analytics workspace <workspace_name> |
| azmcp_monitor_table_list | Show me the tables in the Log Analytics workspace <workspace_name> |
| azmcp_monitor_table_type_list | List all available table types in the Log Analytics workspace <workspace_name> |
| azmcp_monitor_table_type_list | Show me the available table types in the Log Analytics workspace <workspace_name> |
| azmcp_monitor_workspace_list | List all Log Analytics workspaces in my subscription |
| azmcp_monitor_workspace_list | Show me my Log Analytics workspaces |
| azmcp_monitor_workspace_list | Show me the Log Analytics workspaces in my subscription |
| azmcp_monitor_workspace_log_query | Show me the logs for the past hour in the Log Analytics workspace <workspace_name> |

## Azure Native ISV

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_datadog_monitoredresources_list | List all monitored resources in the Datadog resource <resource_name> |
| azmcp_datadog_monitoredresources_list | Show me the monitored resources in the Datadog resource <resource_name> |

## Azure Quick Review CLI

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_extension_azqr | Check my Azure subscription for any compliance issues or recommendations |
| azmcp_extension_azqr | Provide compliance recommendations for my current Azure subscription |
| azmcp_extension_azqr | Scan my Azure subscription for compliance recommendations |

## Azure Quota

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_quota_region_availability_list | Show me the available regions for these resource types <resource_types> |
| azmcp_quota_usage_check | Check usage information for <resource_type> in region <region> |

## Azure RBAC

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_role_assignment_list | List all available role assignments in my subscription |
| azmcp_role_assignment_list | Show me the available role assignments in my subscription |

## Azure Redis

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_redis_cache_accesspolicy_list | List all access policies in the Redis Cache <cache_name> |
| azmcp_redis_cache_accesspolicy_list | Show me the access policies in the Redis Cache <cache_name> |
| azmcp_redis_cache_list | List all Redis Caches in my subscription |
| azmcp_redis_cache_list | Show me my Redis Caches |
| azmcp_redis_cache_list | Show me the Redis Caches in my subscription |
| azmcp_redis_cluster_database_list | List all databases in the Redis Cluster <cluster_name> |
| azmcp_redis_cluster_database_list | Show me the databases in the Redis Cluster <cluster_name> |
| azmcp_redis_cluster_list | List all Redis Clusters in my subscription |
| azmcp_redis_cluster_list | Show me my Redis Clusters |
| azmcp_redis_cluster_list | Show me the Redis Clusters in my subscription |

## Azure Resource Group

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_group_list | List all resource groups in my subscription |
| azmcp_group_list | Show me my resource groups |
| azmcp_group_list | Show me the resource groups in my subscription |

## Azure Resource Health

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_resourcehealth_availability-status_get | Get the availability status for resource <resource_name> |
| azmcp_resourcehealth_availability-status_get | Show me the health status of the storage account <storage_account_name> |
| azmcp_resourcehealth_availability-status_get | What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>? |
| azmcp_resourcehealth_availability-status_list | List availability status for all resources in my subscription |
| azmcp_resourcehealth_availability-status_list | Show me the health status of all my Azure resources |
| azmcp_resourcehealth_availability-status_list | What resources in resource group <resource_group_name> have health issues? |
| azmcp_resourcehealth_service-health-events_list | List all service health events in my subscription |
| azmcp_resourcehealth_service-health-events_list | Show me Azure service health events for subscription <subscription_id> |
| azmcp_resourcehealth_service-health-events_list | What service issues have occurred in the last 30 days? |
| azmcp_resourcehealth_service-health-events_list | List active service health events in my subscription |
| azmcp_resourcehealth_service-health-events_list | Show me planned maintenance events for my Azure services |

## Azure Service Bus

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_servicebus_queue_details | Show me the details of service bus <service_bus_name> queue <queue_name> |
| azmcp_servicebus_topic_details | Show me the details of service bus <service_bus_name> topic <topic_name> |
| azmcp_servicebus_topic_subscription_details | Show me the details of service bus <service_bus_name> subscription <subscription_name> |

## Azure SQL Database

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_sql_db_create | Create a new SQL database named <database_name> in server <server_name> |
| azmcp_sql_db_create | Create a SQL database <database_name> with Basic tier in server <server_name> |
| azmcp_sql_db_create | Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name> |
| azmcp_sql_db_delete | Delete the SQL database <database_name> from server <server_name> |
| azmcp_sql_db_delete | Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name> |
| azmcp_sql_db_delete | Delete the database called <database_name> on server <server_name> |
| azmcp_sql_db_list | List all databases in the Azure SQL server <server_name> |
| azmcp_sql_db_list | Show me all the databases configuration details in the Azure SQL server <server_name> |
| azmcp_sql_db_rename | Rename the SQL database <database_name> on server <server_name> to <new_database_name> |
| azmcp_sql_db_rename | Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name> |
| azmcp_sql_db_show | Get the configuration details for the SQL database <database_name> on server <server_name> |
| azmcp_sql_db_show | Show me the details of SQL database <database_name> in server <server_name> |
| azmcp_sql_db_update | Update the performance tier of SQL database <database_name> on server <server_name> |
| azmcp_sql_db_update | Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU |

## Azure SQL Elastic Pool Operations

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_sql_elastic-pool_list | List all elastic pools in SQL server <server_name> |
| azmcp_sql_elastic-pool_list | Show me the elastic pools configured for SQL server <server_name> |
| azmcp_sql_elastic-pool_list | What elastic pools are available in my SQL server <server_name>? |

## Azure SQL Server Operations

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_sql_server_create | Create a new Azure SQL server named <server_name> in resource group <resource_group_name> |
| azmcp_sql_server_create | Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user> |
| azmcp_sql_server_create | Set up a new SQL server called <server_name> in my resource group <resource_group_name> |
| azmcp_sql_server_delete | Delete the Azure SQL server <server_name> from resource group <resource_group_name> |
| azmcp_sql_server_delete | Remove the SQL server <server_name> from my subscription |
| azmcp_sql_server_delete | Delete SQL server <server_name> permanently |
| azmcp_sql_server_entra-admin_list | List Microsoft Entra ID administrators for SQL server <server_name> |
| azmcp_sql_server_entra-admin_list | Show me the Entra ID administrators configured for SQL server <server_name> |
| azmcp_sql_server_entra-admin_list | What Microsoft Entra ID administrators are set up for my SQL server <server_name>? |
| azmcp_sql_server_firewall-rule_create | Create a firewall rule for my Azure SQL server <server_name> |
| azmcp_sql_server_firewall-rule_create | Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name> |
| azmcp_sql_server_firewall-rule_create | Create a new firewall rule named <rule_name> for SQL server <server_name> |
| azmcp_sql_server_firewall-rule_delete | Delete a firewall rule from my Azure SQL server <server_name> |
| azmcp_sql_server_firewall-rule_delete | Remove the firewall rule <rule_name> from SQL server <server_name> |
| azmcp_sql_server_firewall-rule_delete | Delete firewall rule <rule_name> for SQL server <server_name> |
| azmcp_sql_server_firewall-rule_list | List all firewall rules for SQL server <server_name> |
| azmcp_sql_server_firewall-rule_list | Show me the firewall rules for SQL server <server_name> |
| azmcp_sql_server_firewall-rule_list | What firewall rules are configured for my SQL server <server_name>? |
| azmcp_sql_server_list | List all Azure SQL servers in resource group <resource_group_name> |
| azmcp_sql_server_list | Show me every SQL server available in resource group <resource_group_name> |
| azmcp_sql_server_show | Show me the details of Azure SQL server <server_name> in resource group <resource_group_name> |
| azmcp_sql_server_show | Get the configuration details for SQL server <server_name> |
| azmcp_sql_server_show | Display the properties of SQL server <server_name> |

## Azure Storage

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_storage_account_create | Create a new storage account called testaccount123 in East US region |
| azmcp_storage_account_create | Create a storage account with premium performance and LRS replication |
| azmcp_storage_account_create | Create a new storage account with Data Lake Storage Gen2 enabled |
| azmcp_storage_account_get | Show me the details for my storage account <account> |
| azmcp_storage_account_get | Get details about the storage account <account> |
| azmcp_storage_account_get | List all storage accounts in my subscription including their location and SKU |
| azmcp_storage_account_get | Show me my storage accounts with whether hierarchical namespace (HNS) is enabled |
| azmcp_storage_account_get | Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings |
| azmcp_storage_blob_container_create | Create the storage container mycontainer in storage account <account> |
| azmcp_storage_blob_container_create | Create the container using blob public access in storage account <account> |
| azmcp_storage_blob_container_create | Create a new blob container named documents with container public access in storage account <account> |
| azmcp_storage_blob_container_get | Show me the properties of the storage container <container> in the storage account <account> |
| azmcp_storage_blob_container_get | List all blob containers in the storage account <account> |
| azmcp_storage_blob_container_get | Show me the containers in the storage account <account> |
| azmcp_storage_blob_get | Show me the properties for blob <blob> in container <container> in storage account <account> |
| azmcp_storage_blob_get | Get the details about blob <blob> in the container <container> in storage account <account> |
| azmcp_storage_blob_get | List all blobs in the blob container <container> in the storage account <account> |
| azmcp_storage_blob_get | Show me the blobs in the blob container <container> in the storage account <account> |
| azmcp_storage_blob_upload | Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account> |

## Azure Subscription Management

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_subscription_list | List all subscriptions for my account |
| azmcp_subscription_list | Show me my subscriptions |
| azmcp_subscription_list | What is my current subscription? |
| azmcp_subscription_list | What subscriptions do I have? |

## Azure Terraform Best Practices

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_azureterraformbestpractices_get | Fetch the Azure Terraform best practices |
| azmcp_azureterraformbestpractices_get | Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault |

## Azure Virtual Desktop

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_virtualdesktop_hostpool_list | List all host pools in my subscription |
| azmcp_virtualdesktop_hostpool_sessionhost_list | List all session hosts in host pool <hostpool_name> |
| azmcp_virtualdesktop_hostpool_sessionhost_usersession-list | List all user sessions on session host <sessionhost_name> in host pool <hostpool_name> |

## Azure Workbooks

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_workbooks_create | Create a new workbook named <workbook_name> |
| azmcp_workbooks_delete | Delete the workbook with resource ID <workbook_resource_id> |
| azmcp_workbooks_list | List all workbooks in my resource group <resource_group_name> |
| azmcp_workbooks_list | What workbooks do I have in resource group <resource_group_name>? |
| azmcp_workbooks_show | Get information about the workbook with resource ID <workbook_resource_id> |
| azmcp_workbooks_show | Show me the workbook with display name <workbook_display_name> |
| azmcp_workbooks_update | Update the workbook <workbook_resource_id> with a new text step |

## Bicep

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_bicepschema_get | How can I use Bicep to create an Azure OpenAI service? |

## Cloud Architect

| Tool Name | Test Prompt |
|:----------|:----------|
| azmcp_cloudarchitect_design | Please help me design an architecture for a large-scale file upload, storage, and retrieval service |
| azmcp_cloudarchitect_design | Help me create a cloud service that will serve as ATM for users |
| azmcp_cloudarchitect_design | I want to design a cloud app for ordering groceries |
| azmcp_cloudarchitect_design | How can I design a cloud service in Azure that will store and present videos for users? |
