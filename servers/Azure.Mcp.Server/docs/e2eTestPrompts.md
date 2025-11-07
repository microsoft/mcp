# Azure MCP End-to-End Test Prompts

This file contains prompts used for end-to-end testing to ensure each tool is invoked properly by MCP clients. The tables are organized by Azure MCP Server areas in alphabetical order, with Tool Names sorted alphabetically within each table.

## Azure AI Search

| Tool Name | Test Prompt |
|:----------|:----------|
| search_knowledge_base_get | List all knowledge bases in the Azure AI Search service <service-name> |
| search_knowledge_base_get | Show me the knowledge bases in the Azure AI Search service <service-name> |
| search_knowledge_base_get | List all knowledge bases in the search service <service-name> |
| search_knowledge_base_get | Show me the knowledge bases in the search service <service-name> |
| search_knowledge_base_get | Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name> |
| search_knowledge_base_get | Show me the knowledge base <agent-name> in search service <service-name> |
| search_knowledge_base_retrieve | Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query> |
| search_knowledge_base_retrieve | Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query> |
| search_knowledge_base_retrieve | Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query> |
| search_knowledge_base_retrieve | Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query> |
| search_knowledge_base_retrieve | Query knowledge base <agent-name> in search service <service-name> about <query> |
| search_knowledge_base_retrieve | Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query> |
| search_knowledge_base_retrieve | What does knowledge base <agent-name> in search service <service-name> know about <query> |
| search_knowledge_base_retrieve | Find information about <query> using knowledge base <agent-name> in search service <service-name> |
| search_knowledge_source_get | List all knowledge sources in the Azure AI Search service <service-name> |
| search_knowledge_source_get | Show me the knowledge sources in the Azure AI Search service <service-name> |
| search_knowledge_source_get | List all knowledge sources in the search service <service-name> |
| search_knowledge_source_get | Show me the knowledge sources in the search service <service-name> |
| search_knowledge_source_get | Get the details of knowledge source <source-name> in the Azure AI Search service <service-name> |
| search_knowledge_source_get | Show me the knowledge source <source-name> in search service <service-name> |
| search_index_get | Show me the details of the index \<index-name> in Cognitive Search service \<service-name> |
| search_index_get | List all indexes in the Cognitive Search service \<service-name> |
| search_index_get | Show me the indexes in the Cognitive Search service \<service-name> |
| search_index_query | Search for instances of \<search_term> in the index \<index-name> in Cognitive Search service \<service-name> |
| search_service_list | List all Cognitive Search services in my subscription |
| search_service_list | Show me the Cognitive Search services in my subscription |
| search_service_list | Show me my Cognitive Search services |

## Azure AI Services Speech

| Tool Name | Test Prompt |
|:----------|:----------|
| speech_stt_recognize | Convert this audio file to text using Azure Speech Services |
| speech_stt_recognize | Recognize speech from my audio file with language detection |
| speech_stt_recognize | Transcribe speech from audio file <file_path> with profanity filtering |
| speech_stt_recognize | Convert speech to text from audio file <file_path> using endpoint <endpoint> |
| speech_stt_recognize | Transcribe the audio file <file_path> in Spanish language |
| speech_stt_recognize | Convert speech to text with detailed output format from audio file <file_path> |
| speech_stt_recognize | Recognize speech from <file_path> with phrase hints for better accuracy |
| speech_stt_recognize | Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning" |
| speech_stt_recognize | Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API" |
| speech_stt_recognize | Transcribe audio with raw profanity output from file <file_path> |
| speech_tts_synthesize | Convert text to speech and save to output.wav |
| speech_tts_synthesize | Synthesize speech from "Hello, welcome to Azure" and save to welcome.wav |
| speech_tts_synthesize | Generate speech audio from text "Hello world" using Azure Speech Services |
| speech_tts_synthesize | Convert text to speech with Spanish language and save to spanish-audio.wav |
| speech_tts_synthesize | Synthesize speech with voice en-US-JennyNeural from text "Azure AI Services" |
| speech_tts_synthesize | Create MP3 audio file from text "Welcome to Azure" with high quality format |
| speech_tts_synthesize | Generate speech with custom voice model using endpoint ID <endpoint-id> |
| speech_tts_synthesize | Convert text to OGG/Opus format audio file |
| speech_tts_synthesize | Synthesize long text content to audio file with streaming |
| speech_tts_synthesize | Create audio file from text in French language with appropriate voice |

## Azure App Configuration

| Tool Name | Test Prompt |
|:----------|:----------|
| appconfig_account_list | List all App Configuration stores in my subscription |
| appconfig_account_list | Show me the App Configuration stores in my subscription |
| appconfig_account_list | Show me my App Configuration stores |
| appconfig_kv_delete | Delete the key <key_name> in App Configuration store <app_config_store_name> |
| appconfig_kv_get | List all key-value settings in App Configuration store <app_config_store_name> |
| appconfig_kv_get | Show me the key-value settings in App Configuration store <app_config_store_name> |
| appconfig_kv_get | List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name> |
| appconfig_kv_get | Show the content for the key <key_name> in App Configuration store <app_config_store_name> |
| appconfig_kv_lock_set | Lock the key <key_name> in App Configuration store <app_config_store_name> |
| appconfig_kv_lock_set | Unlock the key <key_name> in App Configuration store <app_config_store_name> |
| appconfig_kv_set | Set the key <key_name> in App Configuration store <app_config_store_name> to \<value> |

## Azure App Lens

| Tool Name | Test Prompt |
|:----------|:----------|
| applens_resource_diagnose | Please help me diagnose issues with my app using app lens |
| applens_resource_diagnose | Use app lens to check why my app is slow? |
| applens_resource_diagnose | What does app lens say is wrong with my service? |

## Azure App Service

| Tool Name | Test Prompt |
|:----------|:----------|
| appservice_database_add | Add database connection <connection_string> to my app service <app_name> for database <database_name> in resource group <resource_group> |
| appservice_database_add | Configure SQL Server database <database_name> for app service <app_name> with connection string <connection_string> in resource group <resource_group> |
| appservice_database_add | Add MySQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group> |
| appservice_database_add | Add PostgreSQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group> |
| appservice_database_add | Connect CosmosDB database <database_name> using connection string <connection_string> to app service <app_name> in resource group <resource_group> |
| appservice_database_add | Add database connection <connection_string> for database <database_name> on server <database_server> to app service <app_name> in resource group <resource_group> |
| appservice_database_add | Add database connection string for <database_name> to app service <app_name> using connection string <connection_string> in resource group <resource_group> |
| appservice_database_add | Connect database <database_name> to my app service <app_name> using connection string <connection_string> in resource group <resource_group> |
| appservice_database_add | Set up database <database_name> for app service <app_name> with connection string <connection_string> under resource group <resource_group> |
| appservice_database_add | Configure database <database_name> for app service <app_name> with the connection string <connection_string> in resource group <resource_group> |

## Azure Application Insights

| Tool Name | Test Prompt |
|:----------|:----------|
| applicationinsights_recommendation_list | List code optimization recommendations across my Application Insights components |
| applicationinsights_recommendation_list | Show me code optimization recommendations for all Application Insights resources in my subscription |
| applicationinsights_recommendation_list | List profiler recommendations for Application Insights in resource group <resource_group_name> |
| applicationinsights_recommendation_list | Show me performance improvement recommendations from Application Insights |

## Azure CLI

| Tool Name | Test Prompt |
|:----------|:----------|
| extension_cli_generate | Create a Storage account with name <storage_account_name> using Azure CLI |
| extension_cli_generate | List all virtual machines in my subscription using Azure CLI |
| extension_cli_generate | Show me the details of the storage account <account_name> with Azure CLI commands |
| extension_cli_install | <Ask the MCP host to uninstall az cli on your machine and run test prompts for extension_cli_generate> |
| extension_cli_install | How to install azd |
| extension_cli_install | What is Azure Functions Core tools and how to install it |

## Azure Container Registry (ACR)

| Tool Name | Test Prompt |
|:----------|:----------|
| acr_registry_list | List all Azure Container Registries in my subscription |
| acr_registry_list | Show me my Azure Container Registries |
| acr_registry_list | Show me the container registries in my subscription |
| acr_registry_list | List container registries in resource group <resource_group_name> |
| acr_registry_list | Show me the container registries in resource group <resource_group_name> |
| acr_registry_repository_list | List all container registry repositories in my subscription |
| acr_registry_repository_list | Show me my container registry repositories |
| acr_registry_repository_list | List repositories in the container registry <registry_name> |
| acr_registry_repository_list | Show me the repositories in the container registry <registry_name> |

## Azure Communication Services

| Tool Name | Test Prompt |
|:----------|:----------|
| communication_email_send | Send an email to <email-address> with subject <subject> |
| communication_email_send | Send an email from my communication service to <email-address> |
| communication_email_send | Send HTML-formatted email to <email-address> with subject <subject> |
| communication_email_send | Send email with CC to <email-address-1> and <email-address-2> |
| communication_email_send | Send email to multiple recipients: <email-address-1>, <email-address-2> |
| communication_email_send | Send email with reply-to address set to <email-address> |
| communication_email_send | Send email with custom sender name <sender-name> |
| communication_email_send | Send an email with BCC recipients |
| communication_sms_send | Send an SMS message to <phone-number> saying "Hello" |
| communication_sms_send | Send SMS to <phone-number-2> from <phone-number-1> with message "Test message" |
| communication_sms_send | Send SMS to multiple recipients: <phone-number-1>, <phone-number-2> |
| communication_sms_send | Send SMS with delivery reporting enabled |
| communication_sms_send | Send SMS message with custom tracking tag "campaign1" |
| communication_sms_send | Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification" |
| communication_sms_send | Send SMS from my communication service to <phone-number-1> |
| communication_sms_send | Send an SMS with delivery receipt tracking |

## Azure Confidential Ledger

| Tool Name | Test Prompt |
|:----------|:----------|
| confidentialledger_entries_append | Append an entry to my ledger <ledger_name> with data {"key": "value"} |
| confidentialledger_entries_append | Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"} |
| confidentialledger_entries_append | Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id> |
| confidentialledger_entries_append | Create an immutable ledger entry in <ledger_name> with content {"audit": "log"} |
| confidentialledger_entries_append | Write an entry to confidential ledger <ledger_name> |
| confidentialledger_entries_get | Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name> |
| confidentialledger_entries_get | Get transaction <transaction_id> from ledger <ledger_name> |

## Azure Cosmos DB

| Tool Name | Test Prompt |
|:----------|:----------|
| cosmos_account_list | List all cosmosdb accounts in my subscription |
| cosmos_account_list | Show me my cosmosdb accounts |
| cosmos_account_list | Show me the cosmosdb accounts in my subscription |
| cosmos_database_container_item_query | Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name> |
| cosmos_database_container_list | List all the containers in the database <database_name> for the cosmosdb account <account_name> |
| cosmos_database_container_list | Show me the containers in the database <database_name> for the cosmosdb account <account_name> |
| cosmos_database_list | List all the databases in the cosmosdb account <account_name> |
| cosmos_database_list | Show me the databases in the cosmosdb account <account_name> |

## Azure Data Explorer

| Tool Name | Test Prompt |
|:----------|:----------|
| kusto_cluster_get | Show me the details of the Data Explorer cluster <cluster_name> |
| kusto_cluster_list | List all Data Explorer clusters in my subscription |
| kusto_cluster_list | Show me my Data Explorer clusters |
| kusto_cluster_list | Show me the Data Explorer clusters in my subscription |
| kusto_database_list | List all databases in the Data Explorer cluster <cluster_name> |
| kusto_database_list | Show me the databases in the Data Explorer cluster <cluster_name> |
| kusto_query | Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name> |
| kusto_sample | Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name> |
| kusto_table_list | List all tables in the Data Explorer database <database_name> in cluster <cluster_name> |
| kusto_table_list | Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name> |
| kusto_table_schema | Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name> |

## Azure Database for MySQL

| Tool Name | Test Prompt |
|:----------|:----------|
| mysql_database_list | List all MySQL databases in server \<server> |
| mysql_database_list | Show me the MySQL databases in server \<server> |
| mysql_database_query | Show me all items that contain the word \<search_term> in the MySQL database \<database> in server \<server> |
| mysql_server_config_get | Show me the configuration of MySQL server \<server> |
| mysql_server_list | List all MySQL servers in my subscription |
| mysql_server_list | Show me my MySQL servers |
| mysql_server_list | Show me the MySQL servers in my subscription |
| mysql_server_param_get | Show me the value of connection timeout in seconds in my MySQL server \<server>  |
| mysql_server_param_set | Set connection timeout to 20 seconds for my MySQL server \<server> |
| mysql_table_list | List all tables in the MySQL database \<database> in server \<server> |
| mysql_table_list | Show me the tables in the MySQL database \<database> in server \<server> |
| mysql_table_schema_get | Show me the schema of table \<table> in the MySQL database \<database> in server \<server> |

## Azure Database for PostgreSQL

| Tool Name | Test Prompt |
|:----------|:----------|
| postgres_database_list | List all PostgreSQL databases in server \<server> |
| postgres_database_list | Show me the PostgreSQL databases in server \<server> |
| postgres_database_query | Show me all items that contain the word \<search_term> in the PostgreSQL database \<database> in server \<server> |
| postgres_server_config_get | Show me the configuration of PostgreSQL server \<server> |
| postgres_server_list | List all PostgreSQL servers in my subscription |
| postgres_server_list | Show me my PostgreSQL servers |
| postgres_server_list | Show me the PostgreSQL servers in my subscription |
| postgres_server_param_get | Show me if the parameter my PostgreSQL server \<server> has replication enabled |
| postgres_server_param_set | Enable replication for my PostgreSQL server \<server> |
| postgres_table_list | List all tables in the PostgreSQL database \<database> in server \<server> |
| postgres_table_list | Show me the tables in the PostgreSQL database \<database> in server \<server> |
| postgres_table_schema_get | Show me the schema of table \<table> in the PostgreSQL database \<database> in server \<server> |

## Azure Deploy

| Tool Name | Test Prompt |
|:----------|:----------|
| deploy_app_logs_get | Show me the log of the application deployed by azd  |
| deploy_architecture_diagram_generate | Generate the azure architecture diagram for this application |
| deploy_iac_rules_get | Show me the rules to generate bicep scripts  |
| deploy_pipeline_guidance_get | How can I create a CI/CD pipeline to deploy this app to Azure? |
| deploy_plan_get | Create a plan to deploy this application to azure |

## Azure Event Grid

| Tool Name | Test Prompt |
|:----------|:----------|
| eventgrid_events_publish | Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data> |
| eventgrid_events_publish | Publish event to my Event Grid topic <topic_name> with the following events <event_data> |
| eventgrid_events_publish | Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data> |
| eventgrid_topic_list | List all Event Grid topics in my subscription |
| eventgrid_topic_list | Show me the Event Grid topics in my subscription |
| eventgrid_topic_list | List all Event Grid topics in subscription <subscription> |
| eventgrid_topic_list | List all Event Grid topics in resource group <resource_group_name> in subscription <subscription> |
| eventgrid_subscription_list | Show me all Event Grid subscriptions for topic <topic_name> |
| eventgrid_subscription_list | List Event Grid subscriptions for topic <topic_name> in subscription <subscription> |
| eventgrid_subscription_list | List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name> |
| eventgrid_subscription_list | Show all Event Grid subscriptions in my subscription |
| eventgrid_subscription_list | List all Event Grid subscriptions in subscription <subscription> |
| eventgrid_subscription_list | Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription> |
| eventgrid_subscription_list | List Event Grid subscriptions for subscription <subscription> in location <location> |

## Azure Event Hubs

| Tool Name | Test Prompt |
|:----------|:----------|
| eventhubs_eventhub_consumergroup_delete | Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name> |
| eventhubs_eventhub_consumergroup_get | List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name> |
| eventhubs_eventhub_consumergroup_get | Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name> |
| eventhubs_eventhub_consumergroup_update | Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name> |
| eventhubs_eventhub_consumergroup_update | Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name> |
| eventhubs_eventhub_delete | Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name> |
| eventhubs_eventhub_get | List all Event Hubs in my namespace <namespace_name> |
| eventhubs_eventhub_get | Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name> |
| eventhubs_eventhub_update | Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name> |
| eventhubs_eventhub_update | Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name> |
| eventhubs_namespace_delete | Delete my namespace <namespace_name> in my resource group <resource_group_name> |
| eventhubs_namespace_get | List all Event Hubs namespaces in my subscription |
| eventhubs_namespace_get | Get the details of my namespace <namespace_name> in my resource group <resource_group_name> |
| eventhubs_namespace_update | Create an new namespace <namespace_name> in my resource group <resource_group_name> |
| eventhubs_namespace_update | Update my namespace <namespace_name> in my resource group <resource_group_name>|

## Azure Function App

| Tool Name | Test Prompt |
|:----------|:----------|
| functionapp_get | Describe the function app <function_app_name> in resource group <resource_group_name> |
| functionapp_get | Get configuration for function app <function_app_name> |
| functionapp_get | Get function app status for <function_app_name> |
| functionapp_get | Get information about my function app <function_app_name> in <resource_group_name> |
| functionapp_get | Retrieve host name and status of function app <function_app_name> |
| functionapp_get | Show function app details for <function_app_name> in <resource_group_name> |
| functionapp_get | Show me the details for the function app <function_app_name> |
| functionapp_get | Show plan and region for function app <function_app_name> |
| functionapp_get | What is the status of function app <function_app_name>? |
| functionapp_get | List all function apps in my subscription |
| functionapp_get | Show me my Azure function apps |
| functionapp_get | What function apps do I have? |

## Azure Key Vault

| Tool Name | Test Prompt |
|:----------|:----------|
| keyvault_admin_settings_get | Get the account settings for my key vault <key_vault_account_name> |
| keyvault_admin_settings_get | Show me the account settings for managed HSM keyvault <key_vault_account_name> |
| keyvault_admin_settings_get | What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name> |
| keyvault_certificate_create | Create a new certificate called <certificate_name> in the key vault <key_vault_account_name> |
| keyvault_certificate_create | Generate a certificate named <certificate_name> in key vault <key_vault_account_name> |
| keyvault_certificate_create | Request creation of certificate <certificate_name> in the key vault <key_vault_account_name> |
| keyvault_certificate_create | Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name> |
| keyvault_certificate_create | Issue a certificate <certificate_name> in key vault <key_vault_account_name> |
| keyvault_certificate_get | Show me the certificate <certificate_name> in the key vault <key_vault_account_name> |
| keyvault_certificate_get | Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name> |
| keyvault_certificate_get | Get the certificate <certificate_name> from vault <key_vault_account_name> |
| keyvault_certificate_get | Display the certificate details for <certificate_name> in vault <key_vault_account_name> |
| keyvault_certificate_get | Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name> |
| keyvault_certificate_import | Import the certificate in file <file_path> into the key vault <key_vault_account_name> |
| keyvault_certificate_import | Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name> |
| keyvault_certificate_import | Upload certificate file <file_path> to key vault <key_vault_account_name> |
| keyvault_certificate_import | Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name> |
| keyvault_certificate_import | Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name> |
| keyvault_certificate_list | List all certificates in the key vault <key_vault_account_name> |
| keyvault_certificate_list | Show me the certificates in the key vault <key_vault_account_name> |
| keyvault_certificate_list | What certificates are in the key vault <key_vault_account_name>? |
| keyvault_certificate_list | List certificate names in vault <key_vault_account_name> |
| keyvault_certificate_list | Enumerate certificates in key vault <key_vault_account_name> |
| keyvault_certificate_list | Show certificate names in the key vault <key_vault_account_name> |
| keyvault_key_create | Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name> |
| keyvault_key_create | Generate a key <key_name> with type <key_type> in vault <key_vault_account_name> |
| keyvault_key_create | Create an oct key in the vault <key_vault_account_name> |
| keyvault_key_create | Create an RSA key in the vault <key_vault_account_name> with name <key_name> |
| keyvault_key_create | Create an EC key with name <key_name> in the vault <key_vault_account_name> |
| keyvault_key_get | Show me the key <key_name> in the key vault <key_vault_account_name> |
| keyvault_key_get | Show me the details of the key <key_name> in the key vault <key_vault_account_name> |
| keyvault_key_get | Get the key <key_name> from vault <key_vault_account_name> |
| keyvault_key_get | Display the key details for <key_name> in vault <key_vault_account_name> |
| keyvault_key_get | Retrieve key metadata for <key_name> in vault <key_vault_account_name> |
| keyvault_key_list | List all keys in the key vault <key_vault_account_name> |
| keyvault_key_list | Show me the keys in the key vault <key_vault_account_name> |
| keyvault_key_list | What keys are in the key vault <key_vault_account_name>? |
| keyvault_key_list | List key names in vault <key_vault_account_name> |
| keyvault_key_list | Enumerate keys in key vault <key_vault_account_name> |
| keyvault_key_list | Show key names in the key vault <key_vault_account_name> |
| keyvault_secret_create | Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name> |
| keyvault_secret_create | Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name> |
| keyvault_secret_create | Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name> |
| keyvault_secret_create | Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name> |
| keyvault_secret_create | Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name> |
| keyvault_secret_get | Show me the secret <secret_name> in the key vault <key_vault_account_name> |
| keyvault_secret_get | Show me the details of the secret <secret_name> in the key vault <key_vault_account_name> |
| keyvault_secret_get | Get the secret <secret_name> from vault <key_vault_account_name> |
| keyvault_secret_get | Display the secret details for <secret_name> in vault <key_vault_account_name> |
| keyvault_secret_get | Retrieve secret metadata for <secret_name> in vault <key_vault_account_name> |
| keyvault_secret_list | List all secrets in the key vault <key_vault_account_name> |
| keyvault_secret_list | Show me the secrets in the key vault <key_vault_account_name> |
| keyvault_secret_list | What secrets are in the key vault <key_vault_account_name>? |
| keyvault_secret_list | List secrets names in vault <key_vault_account_name> |
| keyvault_secret_list | Enumerate secrets in key vault <key_vault_account_name> |
| keyvault_secret_list | Show secrets names in the key vault <key_vault_account_name> |

## Azure Kubernetes Service (AKS)

| Tool Name | Test Prompt |
|:----------|:----------|
| aks_cluster_get | Get the configuration of AKS cluster \<cluster-name> |
| aks_cluster_get | Show me the details of AKS cluster \<cluster-name> in resource group \<resource-group> |
| aks_cluster_get | Show me the network configuration for AKS cluster \<cluster-name> |
| aks_cluster_get | What are the details of my AKS cluster \<cluster-name> in \<resource-group>? |
| aks_cluster_get | List all AKS clusters in my subscription |
| aks_cluster_get | Show me my Azure Kubernetes Service clusters |
| aks_cluster_get | What AKS clusters do I have? |
| aks_nodepool_get | Get details for nodepool \<nodepool-name> in AKS cluster \<cluster-name> in \<resource-group> |
| aks_nodepool_get | Show me the configuration for nodepool \<nodepool-name> in AKS cluster \<cluster-name> in resource group \<resource-group> |
| aks_nodepool_get | What is the setup of nodepool \<nodepool-name> for AKS cluster \<cluster-name> in \<resource-group>? |
| aks_nodepool_get | List nodepools for AKS cluster \<cluster-name> in \<resource-group> |
| aks_nodepool_get | Show me the nodepool list for AKS cluster \<cluster-name> in \<resource-group> |
| aks_nodepool_get | What nodepools do I have for AKS cluster \<cluster-name> in \<resource-group> |

## Azure Load Testing

| Tool Name | Test Prompt |
|:----------|:----------|
| loadtesting_test_create | Create a basic URL test using the following endpoint URL \<test-url> that runs for 30 minutes with 45 virtual users. The test name is \<sample-name> with the test id \<test-id> and the load testing resource is \<load-test-resource> in the resource group \<resource-group> in my subscription |
| loadtesting_test_get | Get the load test with id \<test-id> in the load test resource \<test-resource> in resource group \<resource-group> |
| loadtesting_testresource_create | Create a load test resource \<load-test-resource-name> in the resource group \<resource-group> in my subscription |
| loadtesting_testresource_list | List all load testing resources in the resource group \<resource-group> in my subscription |
| loadtesting_testrun_create | Create a test run using the id \<testrun-id> for test \<test-id> in the load testing resource \<load-testing-resource> in resource group \<resource-group>. Use the name of test run \<display-name> and description as \<description> |
| loadtesting_testrun_get | Get the load test run with id \<testrun-id> in the load test resource \<test-resource> in resource group \<resource-group> |
| loadtesting_testrun_list |  Get all the load test runs for the test with id \<test-id> in the load test resource \<test-resource> in resource group \<resource-group> |
| loadtesting_testrun_update | Update a test run display name as \<display-name> for the id \<testrun-id> for test \<test-id> in the load testing resource \<load-testing-resource> in resource group \<resource-group>.|

## Azure Managed Grafana

| Tool Name | Test Prompt |
|:----------|:----------|
| grafana_list | List all Azure Managed Grafana in one subscription |

## Azure Managed Lustre

| Tool Name | Test Prompt |
|:----------|:----------|
| managedlustre_fs_create | Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time> |
| managedlustre_fs_list | List the Azure Managed Lustre filesystems in my subscription <subscription_name> |
| managedlustre_fs_list | List the Azure Managed Lustre filesystems in my resource group <resource_group_name> |
| managedlustre_fs_sku_get | List the Azure Managed Lustre SKUs available in location <location> |
| managedlustre_fs_autoexport-job_create | Create an autoexport job for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> |
| managedlustre_fs_subnetsize_ask | Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku> |
| managedlustre_fs_subnetsize_validate | Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku> |
| managedlustre_fs_update | Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time> |

## Azure Marketplace

| Tool Name | Test Prompt |
|:----------|:----------|
| marketplace_product_get | Get details about marketplace product <product_name> |
| marketplace_product_list | Search for Microsoft products in the marketplace |
| marketplace_product_list | Show me marketplace products from publisher <publisher_name> |

## Azure MCP Best Practices

| Tool Name | Test Prompt |
|:----------|:----------|
| get_bestpractices_get | Get the latest Azure code generation best practices |
| get_bestpractices_get | Get the latest Azure deployment best practices |
| get_bestpractices_get | Get the latest Azure best practices |
| get_bestpractices_get | Get the latest Azure Functions code generation best practices |
| get_bestpractices_get | Get the latest Azure Functions deployment best practices |
| get_bestpractices_get | Get the latest Azure Functions best practices |
| get_bestpractices_get | Get the latest Azure Static Web Apps best practices |
| get_bestpractices_get | What are azure function best practices? |
| get_bestpractices_get | configure azure mcp in coding agent for my repo |
| get_bestpractices_ai_app | Get best practices for building AI applications in Azure |
| get_bestpractices_ai_app | Show me the best practices for Microsoft Foundry agents code generation |
| get_bestpractices_ai_app | Get guidance for building agents with Microsoft Foundry |
| get_bestpractices_ai_app | Create an AI app that helps me to manage travel queries. |
| get_bestpractices_ai_app | Create an AI app that helps me to manage travel queries in Microsoft Foundry |
## Azure Monitor

| Tool Name | Test Prompt |
|:----------|:----------|
| monitor_activitylog_list | List the activity logs of the last month for <resource_name> |
| monitor_healthmodels_entity_get | Show me the health status of entity <entity_id> using the health model <health_model_name> |
| monitor_metrics_definitions | Get metric definitions for <resource_type> <resource_name> from the namespace |
| monitor_metrics_definitions | Show me all available metrics and their definitions for storage account <account_name> |
| monitor_metrics_definitions | What metric definitions are available for the Application Insights resource <resource_name> |
| monitor_metrics_query | Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period> |
| monitor_metrics_query | Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period> |
| monitor_metrics_query | Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals |
| monitor_metrics_query | Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period> |
| monitor_metrics_query | Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period> |
| monitor_metrics_query | What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period> |
| monitor_resource_log_query | Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name> |
| monitor_table_list | List all tables in the Log Analytics workspace <workspace_name> |
| monitor_table_list | Show me the tables in the Log Analytics workspace <workspace_name> |
| monitor_table_type_list | List all available table types in the Log Analytics workspace <workspace_name> |
| monitor_table_type_list | Show me the available table types in the Log Analytics workspace <workspace_name> |
| monitor_webtests_create | Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component> |
| monitor_webtests_get | Get Web Test details for <webtest_resource_name> in my subscription in <resource_group> |
| monitor_webtests_list | List all Web Test resources in my subscription |
| monitor_webtests_list | List all Web Test resources in my subscription in <resource_group> |
| monitor_webtests_update | Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component> |
| monitor_workspace_list | List all Log Analytics workspaces in my subscription |
| monitor_workspace_list | Show me my Log Analytics workspaces |
| monitor_workspace_list | Show me the Log Analytics workspaces in my subscription |
| monitor_workspace_log_query | Show me the logs for the past hour in the Log Analytics workspace <workspace_name> |

## Azure Native ISV

| Tool Name | Test Prompt |
|:----------|:----------|
| datadog_monitoredresources_list | List all monitored resources in the Datadog resource <resource_name> |
| datadog_monitoredresources_list | Show me the monitored resources in the Datadog resource <resource_name> |

## Azure Quick Review CLI

| Tool Name | Test Prompt |
|:----------|:----------|
| extension_azqr | Check my Azure subscription for any compliance issues or recommendations |
| extension_azqr | Provide compliance recommendations for my current Azure subscription |
| extension_azqr | Scan my Azure subscription for compliance recommendations |

## Azure Quota

| Tool Name | Test Prompt |
|:----------|:----------|
| quota_region_availability_list | Show me the available regions for these resource types <resource_types> |
| quota_usage_check | Check usage information for <resource_type> in region <region> |

## Azure RBAC

| Tool Name | Test Prompt |
|:----------|:----------|
| role_assignment_list | List all available role assignments in my subscription |
| role_assignment_list | Show me the available role assignments in my subscription |

## Azure Redis

| Tool Name | Test Prompt |
|:----------|:----------|
| redis_create | Create a new Redis resource named <resource_name> with SKU <sku_name> in resource group <resource_group_name> |
| redis_create | Create a new Redis resource for me |
| redis_create | Create a Redis cache named <resource_name> with SKU <sku_name> in resource group <resource_group_name> |
| redis_create | Create a new Redis cluster with name <resource_name>, SKU <sku_name> |
| redis_list | List all Redis resources in my subscription |
| redis_list | Show me my Redis resources |
| redis_list | Show me the Redis resources in my subscription |
| redis_list | Show me my Redis caches |
| redis_list | Get Redis clusters |

## Azure Resource Group

| Tool Name | Test Prompt |
|:----------|:----------|
| group_list | List all resource groups in my subscription |
| group_list | Show me my resource groups |
| group_list | Show me the resource groups in my subscription |

## Azure Resource Health

| Tool Name | Test Prompt |
|:----------|:----------|
| resourcehealth_availability-status_get | Get the availability status for resource <resource_name> |
| resourcehealth_availability-status_get | Show me the health status of the storage account <storage_account_name> |
| resourcehealth_availability-status_get | What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>? |
| resourcehealth_availability-status_list | List availability status for all resources in my subscription |
| resourcehealth_availability-status_list | Show me the health status of all my Azure resources |
| resourcehealth_availability-status_list | What resources in resource group <resource_group_name> have health issues? |
| resourcehealth_health-events_list | List all service health events in my subscription |
| resourcehealth_health-events_list | Show me Azure service health events for subscription <subscription_id> |
| resourcehealth_health-events_list | What service issues have occurred in the last 30 days? |
| resourcehealth_health-events_list | List active service health events in my subscription |
| resourcehealth_health-events_list | Show me planned maintenance events for my Azure services |

## Azure Service Bus

| Tool Name | Test Prompt |
|:----------|:----------|
| servicebus_queue_details | Show me the details of service bus <service_bus_name> queue <queue_name> |
| servicebus_topic_details | Show me the details of service bus <service_bus_name> topic <topic_name> |
| servicebus_topic_subscription_details | Show me the details of service bus <service_bus_name> subscription <subscription_name> |

## Azure SignalR

| Tool Name | Test Prompt |
|:----------|:----------|
| signalr_runtime_get | Show me the details of SignalR <signalr_name> |
| signalr_runtime_get | Show me the network information of SignalR runtime <signalr_name> |
| signalr_runtime_get | Describe the SignalR runtime <signalr_name> in resource group <resource_group_name> |
| signalr_runtime_get | Get information about my SignalR runtime <signalr_name> in <resource_group_name> |
| signalr_runtime_get | Show all the SignalRs information in <resource_group_name> |
| signalr_runtime_get | List all SignalRs in my subscription |

## Azure SQL Database

| Tool Name | Test Prompt |
|:----------|:----------|
| sql_db_create | Create a new SQL database named <database_name> in server <server_name> |
| sql_db_create | Create a SQL database <database_name> with Basic tier in server <server_name> |
| sql_db_create | Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name> |
| sql_db_delete | Delete the SQL database <database_name> from server <server_name> |
| sql_db_delete | Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name> |
| sql_db_delete | Delete the database called <database_name> on server <server_name> |
| sql_db_list | List all databases in the Azure SQL server <server_name> |
| sql_db_list | Show me all the databases configuration details in the Azure SQL server <server_name> |
| sql_db_rename | Rename the SQL database <database_name> on server <server_name> to <new_database_name> |
| sql_db_rename | Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name> |
| sql_db_show | Get the configuration details for the SQL database <database_name> on server <server_name> |
| sql_db_show | Show me the details of SQL database <database_name> in server <server_name> |
| sql_db_update | Update the performance tier of SQL database <database_name> on server <server_name> |
| sql_db_update | Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU |

## Azure SQL Elastic Pool Operations

| Tool Name | Test Prompt |
|:----------|:----------|
| sql_elastic-pool_list | List all elastic pools in SQL server <server_name> |
| sql_elastic-pool_list | Show me the elastic pools configured for SQL server <server_name> |
| sql_elastic-pool_list | What elastic pools are available in my SQL server <server_name>? |

## Azure SQL Server Operations

| Tool Name | Test Prompt |
|:----------|:----------|
| sql_server_create | Create a new Azure SQL server named <server_name> in resource group <resource_group_name> |
| sql_server_create | Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user> |
| sql_server_create | Set up a new SQL server called <server_name> in my resource group <resource_group_name> |
| sql_server_delete | Delete the Azure SQL server <server_name> from resource group <resource_group_name> |
| sql_server_delete | Remove the SQL server <server_name> from my subscription |
| sql_server_delete | Delete SQL server <server_name> permanently |
| sql_server_entra-admin_list | List Microsoft Entra ID administrators for SQL server <server_name> |
| sql_server_entra-admin_list | Show me the Entra ID administrators configured for SQL server <server_name> |
| sql_server_entra-admin_list | What Microsoft Entra ID administrators are set up for my SQL server <server_name>? |
| sql_server_firewall-rule_create | Create a firewall rule for my Azure SQL server <server_name> |
| sql_server_firewall-rule_create | Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name> |
| sql_server_firewall-rule_create | Create a new firewall rule named <rule_name> for SQL server <server_name> |
| sql_server_firewall-rule_delete | Delete a firewall rule from my Azure SQL server <server_name> |
| sql_server_firewall-rule_delete | Remove the firewall rule <rule_name> from SQL server <server_name> |
| sql_server_firewall-rule_delete | Delete firewall rule <rule_name> for SQL server <server_name> |
| sql_server_firewall-rule_list | List all firewall rules for SQL server <server_name> |
| sql_server_firewall-rule_list | Show me the firewall rules for SQL server <server_name> |
| sql_server_firewall-rule_list | What firewall rules are configured for my SQL server <server_name>? |
| sql_server_list | List all Azure SQL servers in resource group <resource_group_name> |
| sql_server_list | Show me every SQL server available in resource group <resource_group_name> |
| sql_server_show | Show me the details of Azure SQL server <server_name> in resource group <resource_group_name> |
| sql_server_show | Get the configuration details for SQL server <server_name> |
| sql_server_show | Display the properties of SQL server <server_name> |

## Azure Storage

| Tool Name | Test Prompt |
|:----------|:----------|
| storage_account_create | Create a new storage account called testaccount123 in East US region |
| storage_account_create | Create a storage account with premium performance and LRS replication |
| storage_account_create | Create a new storage account with Data Lake Storage Gen2 enabled |
| storage_account_get | Show me the details for my storage account <account> |
| storage_account_get | Get details about the storage account <account> |
| storage_account_get | List all storage accounts in my subscription including their location and SKU |
| storage_account_get | Show me my storage accounts with whether hierarchical namespace (HNS) is enabled |
| storage_account_get | Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings |
| storage_blob_container_create | Create the storage container mycontainer in storage account <account> |
| storage_blob_container_create | Create the container using blob public access in storage account <account> |
| storage_blob_container_create | Create a new blob container named documents with container public access in storage account <account> |
| storage_blob_container_get | Show me the properties of the storage container <container> in the storage account <account> |
| storage_blob_container_get | List all blob containers in the storage account <account> |
| storage_blob_container_get | Show me the containers in the storage account <account> |
| storage_blob_get | Show me the properties for blob <blob> in container <container> in storage account <account> |
| storage_blob_get | Get the details about blob <blob> in the container <container> in storage account <account> |
| storage_blob_get | List all blobs in the blob container <container> in the storage account <account> |
| storage_blob_get | Show me the blobs in the blob container <container> in the storage account <account> |
| storage_blob_upload | Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account> |

## Azure Subscription Management

| Tool Name | Test Prompt |
|:----------|:----------|
| subscription_list | List all subscriptions for my account |
| subscription_list | Show me my subscriptions |
| subscription_list | What is my current subscription? |
| subscription_list | What subscriptions do I have? |

## Azure Terraform Best Practices

| Tool Name | Test Prompt |
|:----------|:----------|
| azureterraformbestpractices_get | Fetch the Azure Terraform best practices |
| azureterraformbestpractices_get | Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault |

## Azure Virtual Desktop

| Tool Name | Test Prompt |
|:----------|:----------|
| virtualdesktop_hostpool_list | List all host pools in my subscription |
| virtualdesktop_hostpool_host_list | List all session hosts in host pool <hostpool_name> |
| virtualdesktop_hostpool_host_user-list | List all user sessions on session host <sessionhost_name> in host pool <hostpool_name> |

## Azure Workbooks

| Tool Name | Test Prompt |
|:----------|:----------|
| workbooks_create | Create a new workbook named <workbook_name> |
| workbooks_delete | Delete the workbook with resource ID <workbook_resource_id> |
| workbooks_list | List all workbooks in my resource group <resource_group_name> |
| workbooks_list | What workbooks do I have in resource group <resource_group_name>? |
| workbooks_show | Get information about the workbook with resource ID <workbook_resource_id> |
| workbooks_show | Show me the workbook with resource ID <workbook_resource_id> |
| workbooks_update | Update the workbook <workbook_resource_id> with a new text step |

## Bicep

| Tool Name | Test Prompt |
|:----------|:----------|
| bicepschema_get | How can I use Bicep to create an Azure OpenAI service? |

## Cloud Architect

| Tool Name | Test Prompt |
|:----------|:----------|
| cloudarchitect_design | Please help me design an architecture for a large-scale file upload, storage, and retrieval service |
| cloudarchitect_design | Help me design an Azure cloud service that will serve as an ATM for users |
| cloudarchitect_design | I want to design a cloud app for ordering groceries |
| cloudarchitect_design | How can I design a cloud service in Azure that will store and present videos for users? |

## Microsoft Foundry

| Tool Name | Test Prompt |
|:----------|:----------|
| foundry_agents_connect | Query an agent in my Microsoft Foundry resource |
| foundry_agents_create | Create a new Microsoft Foundry agent using instructions in the active editor |
| foundry_agents_evaluate | Evaluate the full query and response I got from my agent for task_adherence |
| foundry_agents_get-sdk-sample | Create a CLI app that can talk to a Microsoft Foundry Agent using Python SDK |
| foundry_agents_list | List all agents in my Microsoft Foundry resource |
| foundry_agents_list | Show me the available agents in my Microsoft Foundry resource |
| foundry_agents_query-and-evaluate | Query and evaluate an agent in my Microsoft Foundry resource for task_adherence |
| foundry_knowledge_index_list | List all knowledge indexes in my Microsoft Foundry project |
| foundry_knowledge_index_list | Show me the knowledge indexes in my Microsoft Foundry project |
| foundry_knowledge_index_schema | Show me the schema for knowledge index \<index-name> in my Microsoft Foundry resource |
| foundry_knowledge_index_schema | Get the schema configuration for knowledge index \<index-name> |
| foundry_models_deploy | Deploy a GPT4o instance on my resource \<resource-name> |
| foundry_models_deployments_list | List all Microsoft Foundry model deployments |
| foundry_models_deployments_list | Show me all Microsoft Foundry model deployments |
| foundry_models_list | List all Microsoft Foundry models |
| foundry_models_list | Show me the available Microsoft Foundry models |
| foundry_openai_chat-completions-create | Create a chat completion with the message "Hello, how are you today?" using my Microsoft Foundry resource |
| foundry_openai_create-completion | Create a completion with the prompt "What is Azure?" using my Microsoft Foundry resource |
| foundry_openai_embeddings-create | Generate embeddings for the text "Azure OpenAI Service" using my Microsoft Foundry resource |
| foundry_openai_embeddings-create | Create vector embeddings for my text using my Microsoft Foundry resource |
| foundry_openai_models-list | List all available OpenAI models in my Microsoft Foundry resource |
| foundry_openai_models-list | Show me the OpenAI model deployments in my Microsoft Foundry resource |
| foundry_resource_get | List all Microsoft Foundry resources in my subscription |
| foundry_resource_get | Show me the Microsoft Foundry resources in resource group <resource_group_name> |
| foundry_resource_get | Get details for Microsoft Foundry resource <resource_name> in resource group <resource_group_name> |
| foundry_threads_create | Create a Microsoft Foundry thread to hold the conversation |
| foundry_threads_get-messages | Show me the messages in the Microsoft Foundry thread with id <thread_id> |
| foundry_threads_list | List my Microsoft Foundry threads |
