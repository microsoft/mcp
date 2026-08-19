# Azure MCP End-to-End Test Prompts

This file contains prompts used for end-to-end testing to ensure each tool is invoked properly by MCP clients. The tables are organized by Azure MCP Server areas in alphabetical order, with Tool Names sorted alphabetically within each table.

The `Interaction` column describes whether a prompt can invoke its tool immediately:

- `none`: The prompt contains enough information for direct tool invocation.
- `clarification-required`: The user must provide missing command parameters.
- `context-required`: The user must provide an attachment, project, or other external context. This includes the deployment of Azure resources.

## Azure Advisor

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| advisor_metadata_get | Get the Advisor metadata for recommendation type id <recommendation-type-id> | none |
| advisor_metadata_get | What does Advisor recommendation type <recommendation-type-id> mean? | none |
| advisor_metadata_get | Show me the catalog details for Advisor recommendation type <recommendation-type-id> | none |
| advisor_metadata_get | Get the German (de) metadata for Advisor recommendation type <recommendation-type-id> | none |
| advisor_metadata_get | What is the impact and category of Advisor recommendation type <recommendation-type-id>? | none |
| advisor_metadata_get | Show the remediation actions for Advisor recommendation type <recommendation-type-id> | none |
| advisor_metadata_get | When does Advisor recommendation type <recommendation-type-id> retire? | none |
| advisor_metadata_list | List the Advisor recommendation metadata catalog | none |
| advisor_metadata_list | Before I deploy any virtual machines, what kinds of recommendations could Advisor produce for them? | none |
| advisor_metadata_list | List high-impact Advisor metadata for microsoft.sql/servers/databases | none |
| advisor_metadata_list | Show the German metadata catalog for Advisor recommendations | none |
| advisor_metadata_list | Which Advisor recommendation types include service-retirement details? | none |
| advisor_metadata_list | List Advisor metadata in the ServiceUpgradeAndRetirement subcategory | none |
| advisor_metadata_list | Find the Advisor service-retirement metadata with tracking ID QNY1-HB8 | none |
| advisor_metadata_list | Show Advisor service retirements on or after March 31, 2026 | none |
| advisor_recommendation_apply | Apply Advisor recommendations to this ARM template | context-required |
| advisor_recommendation_apply | Apply Advisor recommendations to this Terraform file for Storage Account | context-required |
| advisor_recommendation_list | List all recommendations in my subscription | none |
| advisor_recommendation_list | Show me Advisor recommendations in the subscription <subscription> | none |
| advisor_recommendation_list | List all Advisor recommendations in the subscription <subscription> | none |
| advisor_recommendation_list | Show me high-impact Security recommendations in subscription <subscription> | none |
| advisor_recommendation_list | List Cost recommendations for storage accounts in subscription <subscription> | none |
| advisor_recommendation_list | Find Advisor recommendations mentioning "right-size" in subscription <subscription> | none |
| advisor_recommendation_list | Show me the top 10 Advisor recommendations in subscription <subscription> | none |
| advisor_recommendation_summary | Summarize the key themes from my Advisor recommendations in subscription <subscription> | none |
| advisor_recommendation_summary | Summarize Advisor recommendations in subscription <subscription> by category | none |
| advisor_recommendation_summary | Show the top 10 most common Advisor recommendations in subscription <subscription> | none |
| advisor_recommendation_summary | Group Advisor recommendations by impact in subscription <subscription> | none |
| advisor_recommendation_summary | Which resource types have the most high-impact recommendations in subscription <subscription>? | none |
| advisor_recommendation_summary | Summarize high-impact Security recommendations by resource-type in subscription <subscription> | none |
| advisor_recommendation_summary | Group Cost recommendations for storage accounts by impact in subscription <subscription> | none |
| advisor_recommendation_summary | Summarize Advisor recommendations mentioning "encryption" by category in subscription <subscription> | none |

## Azure AI Search

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| search_knowledge_base_get | List all knowledge bases in the Azure AI Search service <service-name> | none |
| search_knowledge_base_get | Show me the knowledge bases in the Azure AI Search service <service-name> | none |
| search_knowledge_base_get | List all knowledge bases in the search service <service-name> | none |
| search_knowledge_base_get | Show me the knowledge bases in the search service <service-name> | none |
| search_knowledge_base_get | Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name> | none |
| search_knowledge_base_get | Show me the knowledge base <agent-name> in search service <service-name> | none |
| search_knowledge_base_retrieve | Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query> | none |
| search_knowledge_base_retrieve | Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query> | none |
| search_knowledge_base_retrieve | Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query> | none |
| search_knowledge_base_retrieve | Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query> | none |
| search_knowledge_base_retrieve | Query knowledge base <agent-name> in search service <service-name> about <query> | none |
| search_knowledge_base_retrieve | Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query> | none |
| search_knowledge_base_retrieve | What does knowledge base <agent-name> in search service <service-name> know about <query> | none |
| search_knowledge_base_retrieve | Find information about <query> using knowledge base <agent-name> in search service <service-name> | none |
| search_knowledge_source_get | List all knowledge sources in the Azure AI Search service <service-name> | none |
| search_knowledge_source_get | Show me the knowledge sources in the Azure AI Search service <service-name> | none |
| search_knowledge_source_get | List all knowledge sources in the search service <service-name> | none |
| search_knowledge_source_get | Show me the knowledge sources in the search service <service-name> | none |
| search_knowledge_source_get | Get the details of knowledge source <source-name> in the Azure AI Search service <service-name> | none |
| search_knowledge_source_get | Show me the knowledge source <source-name> in search service <service-name> | none |
| search_index_get | Show me the details of the index \<index-name> in Cognitive Search service \<service-name> | none |
| search_index_get | List all indexes in the Cognitive Search service \<service-name> | none |
| search_index_get | Show me the indexes in the Cognitive Search service \<service-name> | none |
| search_index_query | Search for instances of \<search_term> in the index \<index-name> in Cognitive Search service \<service-name> | none |
| search_service_list | List all Cognitive Search services in my subscription | none |
| search_service_list | Show me the Cognitive Search services in my subscription | none |
| search_service_list | Show me my Cognitive Search services | none |

## Azure AI Services Speech

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| speech_stt_recognize | Convert this audio file to text using Azure Speech Services | context-required |
| speech_stt_recognize | Recognize speech from my audio file with language detection | context-required |
| speech_stt_recognize | Transcribe speech from audio file <file_path> with profanity filtering | context-required |
| speech_stt_recognize | Convert speech to text from audio file <file_path> using endpoint <endpoint> | context-required |
| speech_stt_recognize | Transcribe the audio file <file_path> in Spanish language | context-required |
| speech_stt_recognize | Convert speech to text with detailed output format from audio file <file_path> | context-required |
| speech_stt_recognize | Recognize speech from <file_path> with phrase hints for better accuracy | context-required |
| speech_stt_recognize | Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning" | clarification-required |
| speech_stt_recognize | Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API" | clarification-required |
| speech_stt_recognize | Transcribe audio with raw profanity output from file <file_path> | context-required |
| speech_tts_synthesize | Convert text to speech and save to output.wav | clarification-required |
| speech_tts_synthesize | Synthesize speech from "Hello, welcome to Azure" and save to welcome.wav | none |
| speech_tts_synthesize | Generate speech audio from text "Hello world" using Azure Speech Services | none |
| speech_tts_synthesize | Convert text to speech with Spanish language and save to spanish-audio.wav | clarification-required |
| speech_tts_synthesize | Synthesize speech with voice en-US-JennyNeural from text "Azure AI Services" | none |
| speech_tts_synthesize | Create MP3 audio file from text "Welcome to Azure" with high quality format | none |
| speech_tts_synthesize | Generate speech with custom voice model using endpoint ID <endpoint-id> | clarification-required |
| speech_tts_synthesize | Convert text to OGG/Opus format audio file | clarification-required |
| speech_tts_synthesize | Synthesize long text content to audio file with streaming | clarification-required |
| speech_tts_synthesize | Create audio file from text in French language with appropriate voice | clarification-required |

## Azure App Configuration

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| appconfig_account_list | List all App Configuration stores in my subscription | none |
| appconfig_account_list | Show me the App Configuration stores in my subscription | none |
| appconfig_account_list | Show me my App Configuration stores | none |
| appconfig_kv_delete | Delete the key <key_name> in App Configuration store <app_config_store_name> | none |
| appconfig_kv_get | List all key-value settings in App Configuration store <app_config_store_name> | none |
| appconfig_kv_get | Show me the key-value settings in App Configuration store <app_config_store_name> | none |
| appconfig_kv_get | List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name> | none |
| appconfig_kv_get | Show the content for the key <key_name> in App Configuration store <app_config_store_name> | none |
| appconfig_kv_lock_set | Lock the key <key_name> in App Configuration store <app_config_store_name> | none |
| appconfig_kv_lock_set | Unlock the key <key_name> in App Configuration store <app_config_store_name> | none |
| appconfig_kv_set | Set the key <key_name> in App Configuration store <app_config_store_name> to \<value> | none |

## Azure App Lens

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| applens_resource_diagnose | Please help me diagnose issues with my app using app lens | clarification-required |
| applens_resource_diagnose | Use app lens to check why my app is slow? | clarification-required |
| applens_resource_diagnose | What does app lens say is wrong with my service? | clarification-required |

## Azure App Service

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| appservice_database_add | Add database connection <connection_string> to my app service <app_name> for database <database_name> in resource group <resource_group> | none |
| appservice_database_add | Configure SQL Server database <database_name> for app service <app_name> with connection string <connection_string> in resource group <resource_group> | none |
| appservice_database_add | Add MySQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group> | none |
| appservice_database_add | Add PostgreSQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group> | none |
| appservice_database_add | Connect CosmosDB database <database_name> using connection string <connection_string> to app service <app_name> in resource group <resource_group> | none |
| appservice_database_add | Add database connection <connection_string> for database <database_name> on server <database_server> to app service <app_name> in resource group <resource_group> | none |
| appservice_database_add | Add database connection string for <database_name> to app service <app_name> using connection string <connection_string> in resource group <resource_group> | none |
| appservice_database_add | Connect database <database_name> to my app service <app_name> using connection string <connection_string> in resource group <resource_group> | none |
| appservice_database_add | Set up database <database_name> for app service <app_name> with connection string <connection_string> under resource group <resource_group> | none |
| appservice_database_add | Configure database <database_name> for app service <app_name> with the connection string <connection_string> in resource group <resource_group> | none |
| appservice_webapp_diagnostic_diagnose | Use Azure App Service diagnostics to diagnose web app <webapp> in <resource_group> with detector <detector_name> | none |
| appservice_webapp_diagnostic_diagnose | Use Azure App Service diagnostics to diagnose web app <webapp> in <resource_group> with detector <detector_name> between <start_time> and <end_time> with interval <interval> | none |
| appservice_webapp_diagnostic_list | List the Azure App Service diagnostic detectors for web app <webapp> in <resource_group> | none |
| appservice_webapp_change-state | Start the web app <app> in <resource_group> | none |
| appservice_webapp_change-state | Stop the web app <app> in <resource_group> | none |
| appservice_webapp_change-state | Restart the web app <app> in <resource_group> | none |
| appservice_webapp_change-state | Soft restart the web app <app> in <resource_group> waiting for restart to complete | none |
| appservice_webapp_get | List the web apps in my subscription | none |
| appservice_webapp_get | Show me the web apps in my <resource_group> resource group | none |
| appservice_webapp_get | Get the details for web app <webapp> in <resource_group> | none |
| appservice_webapp_deployment_get | List the deployments for web app <webapp> in <resource_group> | none |
| appservice_webapp_deployment_get | Get the deployment <deployment-id> for web app <webapp> in <resource_group> | none |
| appservice_webapp_settings_get-appsettings | List the application settings for web app <webapp> in <resource_group> | none |
| appservice_webapp_settings_get-appsettings | Get the application settings for web app <webapp> in <resource_group> | none |
| appservice_webapp_settings_update-appsettings | Add application setting <setting-name> with <setting-value> to web app <webapp> in <resource_group> | none |
| appservice_webapp_settings_update-appsettings | Set application setting <setting-name> with <setting-value> to web app <webapp> in <resource_group> | none |
| appservice_webapp_settings_update-appsettings | Delete application setting <setting-name> from web app <webapp> in <resource_group> | none |

## Azure Application Insights

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| applicationinsights_recommendation_list | List code optimization recommendations across my Application Insights components | none |
| applicationinsights_recommendation_list | Show me code optimization recommendations for all Application Insights resources in my subscription | none |
| applicationinsights_recommendation_list | List profiler recommendations for Application Insights in resource group <resource_group_name> | none |
| applicationinsights_recommendation_list | List Application Insights code optimization recommendations for improving application performance | none |

## Azure Backup

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| azurebackup_backup_status | Check backup status for resource <resource_id> in location <location> | none |
| azurebackup_backup_status | What is the backup status of <resource_id> in location <location> in my subscription? | none |
| azurebackup_disasterrecovery_enable-crr | Enable cross-region restore on vault <vault_name> in resource group <resource_group> | none |
| azurebackup_disasterrecovery_enable-crr | Turn on cross-region restore for vault <vault_name> under resource group <resource_group> | none |
| azurebackup_governance_find-unprotected | Find unprotected resources of type <resource_type> in my subscription | none |
| azurebackup_governance_find-unprotected | Show me Azure resources that are not backed up for resource type <resource_type> | none |
| azurebackup_governance_find-unprotected | Find unprotected SQL databases and file shares discovered by backup vaults in my subscription | none |
| azurebackup_governance_find-unprotected | Find all resources and sub-resources in resource group <resource_group> that are not protected by Azure Backup | none |
| azurebackup_governance_immutability | Configure immutability state on vault <vault_name> in resource group <resource_group> | clarification-required |
| azurebackup_governance_immutability | Set immutability to Enabled on vault <vault_name> in resource group <resource_group> | none |
| azurebackup_governance_soft-delete | Configure soft delete on Azure Backup vault <vault_name> in resource group <resource_group> | clarification-required |
| azurebackup_governance_soft-delete | Set soft delete state to AlwaysOn for vault <vault_name> in resource group <resource_group> | none |
| azurebackup_job_get | Get backup job <job_id> from vault <vault_name> in resource group <resource_group> | none |
| azurebackup_job_get | Show me the status of backup job <job_id> in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_policy_create | Create a backup policy named <policy_name> for AzureIaasVM in vault <vault_name> in resource group <resource_group> | none |
| azurebackup_policy_create | Set up a new backup policy called <policy_name> for AzureFileShare workload in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_policy_create | Create an Enhanced VM backup policy <policy_name> with hourly schedule every 4 hours starting 08:00 for 12 hours in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_policy_create | Create a weekly VM policy <policy_name> on Mondays at 03:00 with 8 weekly, 12 monthly, 5 yearly retention and archive after 90 days in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_policy_create | Create a SQL backup policy <policy_name> with daily full at 02:00, differential on Wednesdays, and 60-minute log frequency in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_policy_create | Create an Azure Disk backup policy <policy_name> with daily, weekly, and monthly retention tiers and vault tier copy enabled in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_policy_update | Update backup policy <policy_name> in vault <vault_name> in resource group <resource_group> to change the schedule time to 04:00 | none |
| azurebackup_policy_update | Modify the daily retention to 60 days for backup policy <policy_name> in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_policy_get | Get backup policy <policy_name> from vault <vault_name> in resource group <resource_group> | none |
| azurebackup_policy_get | Show me the details of backup policy <policy_name> in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_protectableitem_list | List protectable items in vault <vault_name> in resource group <resource_group> |none |
| azurebackup_protectableitem_list | Show me all items that can be backed up in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_protecteditem_get | Get protected item details for <item_name> in vault <vault_name> and resource group <resource_group> | none |
| azurebackup_protecteditem_get | Show backup status of protected item <item_name> in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_protecteditem_protect | Enable backup protection for <item_name> using policy <policy_name> in vault <vault_name> and resource group <resource_group> | none |
| azurebackup_protecteditem_protect | Start protecting my Azure VM by enabling backup on <item_name> in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_protecteditem_undelete | Restore a soft-deleted backup item for datasource <datasource_id> in vault <vault_name> and resource group <resource_group> | none |
| azurebackup_protecteditem_undelete | Undelete the accidentally deleted backup for VM <datasource_id> in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_recoverypoint_get | Get recovery points for protected item <item_name> in vault <vault_name> and resource group <resource_group> | none |
| azurebackup_recoverypoint_get | List available recovery points for <item_name> in vault <vault_name> under resource group <resource_group> | none |
| azurebackup_security_configure-encryption | Configure customer-managed key encryption on vault <vault_name> in resource group <resource_group> using key <key_name> from key vault <key_vault_uri> with system-assigned identity | none |
| azurebackup_security_configure-encryption | Enable CMK encryption on vault <vault_name> using user-assigned identity <identity_id> and key <key_name> from <key_vault_uri> | none |
| azurebackup_security_configure-encryption | Set up customer-managed encryption for backup vault <vault_name> in <resource_group> | none |
| azurebackup_security_configure-mua | Enable multi-user authorization on vault <vault_name> in resource group <resource_group> with resource guard <resource_guard_id> | none |
| azurebackup_security_configure-mua | Disable MUA on vault <vault_name> in resource group <resource_group> | none |
| azurebackup_vault_create | Create a Recovery Services vault named <vault_name> in resource group <resource_group> in region <location> with vault-type 'rsv' | none |
| azurebackup_vault_create | Set up a new backup vault called <vault_name> in <location> under resource group <resource_group> with vault-type 'dpp' | none |
| azurebackup_vault_get | Get details of Recovery Services vault <vault_name> in resource group <resource_group> | none |
| azurebackup_vault_get | Show me information about Azure Backup vault <vault_name> in resource group <resource_group> | none |
| azurebackup_vault_update | Update Azure Backup vault <vault_name> in resource group <resource_group> to enable soft delete | none |
| azurebackup_vault_update | Change the identity type of Azure Backup vault <vault_name> in resource group <resource_group> to SystemAssigned | none |

## Azure CLI

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| extension_cli_generate | What's the Azure CLI command for getting a storage account's details? | none |
| extension_cli_generate | List all virtual machines in my subscription using Azure CLI | none |
| extension_cli_generate | Show me the details of the storage account <account_name> using Azure CLI commands | none |
| extension_cli_install | \<Ask the MCP host to uninstall az cli on your machine and run test prompts for extension_cli_generate> | context-required |
| extension_cli_install | How to install azd | none |
| extension_cli_install | What is Azure Functions Core tools and how to install it | none |

## Azure Container Apps

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| containerapps_list | List all Azure Container Apps in my subscription | none |
| containerapps_list | Show me my Azure Container Apps | none |
| containerapps_list | List container apps in resource group <resource_group_name> | none |
| containerapps_list | Show me the container apps in resource group <resource_group_name> | none |

## Azure Container Registry (ACR)

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| acr_registry_list | List all Azure Container Registries in my subscription | none |
| acr_registry_list | Show me my Azure Container Registries | none |
| acr_registry_list | Show me the container registries in my subscription | none |
| acr_registry_list | List container registries in resource group <resource_group_name> | none |
| acr_registry_list | Show me the container registries in resource group <resource_group_name> | none |
| acr_registry_repository_list | List all container registry repositories in my subscription | none |
| acr_registry_repository_list | Show me my container registry repositories | none |
| acr_registry_repository_list | List repositories in the container registry <registry_name> | none |
| acr_registry_repository_list | Show me the repositories in the container registry <registry_name> | none |

## Azure Communication Services

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| communication_email_send | Send an email to <email-address> with subject <subject> | clarification-required |
| communication_email_send | Send an email from my communication service to <email-address> | clarification-required |
| communication_email_send | Send HTML-formatted email to <email-address> with subject <subject> | clarification-required |
| communication_email_send | Send email with CC to <email-address-1> and <email-address-2> | clarification-required |
| communication_email_send | Send email to multiple recipients: <email-address-1>, <email-address-2> | clarification-required |
| communication_email_send | Send email with reply-to address set to <email-address> | clarification-required |
| communication_email_send | Send email with custom sender name <sender-name> | clarification-required |
| communication_email_send | Send an email with BCC recipients | clarification-required |
| communication_sms_send | Send an SMS message to <phone-number> saying "Hello" | clarification-required |
| communication_sms_send | Send SMS to <phone-number-2> from <phone-number-1> with message "Test message" | clarification-required |
| communication_sms_send | Send SMS to multiple recipients: <phone-number-1>, <phone-number-2> | clarification-required |
| communication_sms_send | Send SMS with delivery reporting enabled | clarification-required |
| communication_sms_send | Send SMS message with custom tracking tag "campaign1" | clarification-required |
| communication_sms_send | Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification" | clarification-required |
| communication_sms_send | Send SMS from my communication service to <phone-number-1> | clarification-required |
| communication_sms_send | Send an SMS with delivery receipt tracking | clarification-required |

## Azure Compute

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| compute_vm_create | Create a new Linux VM named <vm-name> in resource group <resource-group-name> | none |
| compute_vm_create | Create a virtual machine with Standard_D2s_v5 size in <resource-group-name> | clarification-required |
| compute_vm_create | Create a Windows VM with password authentication in resource group <resource-group-name> | none |
| compute_vm_create | Create VM <vm-name> in <location> with SSH key authentication | none |
| compute_vm_create | Deploy a new VM with a 128GB Premium SSD OS disk in resource group <resource-group-name> | none |
| compute_vm_create | Create a VM with Standard_E4s_v3 size and no public IP in <resource-group-name> | none |
| compute_vm_get | List all virtual machines in my subscription | none |
| compute_vm_get | Show me all VMs in my subscription | none |
| compute_vm_get | What virtual machines do I have? | none |
| compute_vm_get | Get all virtual machines in resource group <resource-group-name> | none |
| compute_vm_get | Show me VMs in resource group <resource-group-name> | none |
| compute_vm_get | List the Azure virtual machines in resource group <resource-group-name> | none |
| compute_vm_get | Get details for virtual machine <vm-name> in resource group <resource-group-name> | none |
| compute_vm_get | Show me virtual machine <vm-name> in resource group <resource-group-name> | none |
| compute_vm_get | What are the details of VM <vm-name> in resource group <resource-group-name>? | none |
| compute_vm_get | Get virtual machine <vm-name> with instance view in resource group <resource-group-name> | none |
| compute_vm_get | Show me VM <vm-name> with runtime status in resource group <resource-group-name> | none |
| compute_vm_get | What is the power state of virtual machine <vm-name> in resource group <resource-group-name>? | none |
| compute_vm_get | Get VM <vm-name> status and provisioning state in resource group <resource-group-name> | none |
| compute_vm_get | Show me the current status of VM <vm-name> | none |
| compute_vm_update | Add tags to VM <vm-name> in resource group <resource-group-name> | clarification-required |
| compute_vm_update | Set the environment=production tag on Azure virtual machine <vm-name> | none |
| compute_vm_update | Update VM <vm-name> to enable boot diagnostics in resource group <resource-group-name> | none |
| compute_vm_update | Change the size of VM <vm-name> to Standard_D4s_v3 | none |
| compute_vm_delete | Delete VM <vm-name> in resource group <resource-group-name> | none |
| compute_vm_delete | Remove virtual machine <vm-name> from resource group <resource-group-name> | none |
| compute_vm_delete | Destroy VM <vm-name> in resource group <resource-group-name> | none |
| compute_vm_delete | Force delete VM <vm-name> in resource group <resource-group-name> using force-deletion | none |
| compute_vm_power-state | Power on and start VM <vm-name> in resource group <resource-group-name> | none |
| compute_vm_power-state | Stop the running virtual machine <vm-name> and power it off in resource group <resource-group-name> | none |
| compute_vm_power-state | Deallocate VM <vm-name> in resource group <resource-group-name> to release compute resources while keeping the VM | none |
| compute_vm_power-state | Restart VM <vm-name> in resource group <resource-group-name> | none |
| compute_vm_power-state | Stop VM <vm-name> in resource group <resource-group-name> and skip the OS shutdown | none |
| compute_vm_power-state | Start VM <vm-name> in resource group <resource-group-name> without waiting for completion | none |
| compute_vm_power-state | Power off and shut down VM <vm-name> in resource group <resource-group-name> | none |
| compute_vm_power-state | Deallocate and power off VM <vm-name> to stop billing for compute resources while preserving the VM | none |
| compute_vmss_create | Create a virtual machine scale set named <vmss-name> in resource group <resource-group-name> | none |
| compute_vmss_create | Create a VMSS with 3 instances in <resource-group-name> | clarification-required |
| compute_vmss_create | Deploy a virtual machine scale set with Rolling upgrade policy and 5 instances | clarification-required |
| compute_vmss_create | Create Linux VMSS with SSH authentication in <resource-group-name> | none |
| compute_vmss_get | List all virtual machine scale sets in my subscription | none |
| compute_vmss_get | List virtual machine scale sets in resource group <resource-group-name> | none |
| compute_vmss_get | What scale sets are in resource group <resource-group-name>? | none |
| compute_vmss_get | Get details for virtual machine scale set <vmss-name> in resource group <resource-group-name> | none |
| compute_vmss_get | Show me VMSS <vmss-name> in resource group <resource-group-name> | none |
| compute_vmss_get | Show me instance <instance-id> of VMSS <vmss-name> in resource group <resource-group-name> | none |
| compute_vmss_get | What is the status of instance <instance-id> in scale set <vmss-name>? | none |
| compute_vmss_update | Update the capacity of scale set <vmss-name> to 10 | none |
| compute_vmss_update | Enable automatic OS upgrades on VMSS <vmss-name> | none |
| compute_vmss_update | Change upgrade policy to Rolling for <vmss-name> | none |
| compute_vmss_update | Add tags to scale set <vmss-name> in resource group <resource-group-name> | clarification-required |
| compute_vmss_delete | Delete scale set <vmss-name> in resource group <resource-group-name> | none |
| compute_vmss_delete | Remove VMSS <vmss-name> from resource group <resource-group-name> | none |
| compute_vmss_delete | Destroy virtual machine scale set <vmss-name> in resource group <resource-group-name> | none |
| compute_vmss_delete | Force delete VMSS <vmss-name> in resource group <resource-group-name> using force-deletion | none |
| compute_disk_get | List all managed disks in my subscription | none |
| compute_disk_get | Show me all disks in resource group <resource-group> | none |
| compute_disk_get | Get details of disk <disk-name> in resource group <resource-group> | none |
| compute_disk_get | Show me the disk sizes in resource group <resource-group> | none |
| compute_disk_get | What managed disks are available? | none |
| compute_disk_get | Get information about disk <disk-name> | none |
| compute_disk_create | Create a 128 GB managed disk named <disk-name> in resource group <resource-group> | none |
| compute_disk_create | Create a new Premium_LRS disk called <disk-name> in resource group <resource-group> with 256 GB | none |
| compute_disk_create | Create a managed disk <disk-name> in resource group <resource-group> in eastus | none |
| compute_disk_create | Create a disk from snapshot <snapshot-resource-id> in resource group <resource-group> | none |
| compute_disk_create | Create a managed disk <disk-name> in resource group <resource-group> from blob <blob-uri> | none |
| compute_disk_create | Create a 64 GB Standard_LRS Linux disk named <disk-name> in resource group <resource-group> in zone 1 | none |
| compute_disk_create | Create a managed disk <disk-name> in resource group <resource-group> with tags env=prod team=infra | none |
| compute_disk_create | Create a 128 GB Premium_LRS disk named <disk-name> in resource group <resource-group> with performance tier P30 | none |
| compute_disk_create | Create a disk <disk-name> in resource group <resource-group> with customer-managed encryption using disk encryption set <disk-encryption-set-id> | none |
| compute_disk_create | Create a managed disk from gallery image version <image-version-resource-id> in resource group <resource-group> | none |
| compute_disk_create | Create a data disk from LUN 0 of gallery image version <image-version-resource-id> in resource group <resource-group> | none |
| compute_disk_create | Create a disk ready for upload named <disk-name> in resource group <resource-group> with upload size 20972032 bytes | none |
| compute_disk_create | Create an Azure managed disk named <disk-name> in resource group <resource-group> as a Trusted Launch upload disk with UploadWithSecurityData type and TrustedLaunch security type | none |
| compute_disk_create | Create an UltraSSD_LRS disk named <disk-name> in resource group <resource-group> with 256 GB, 10000 IOPS, and 500 MBps throughput | none |
| compute_disk_create | Create a shared managed disk named <disk-name> in resource group <resource-group> with 512 GB and max shares set to 3 | none |
| compute_disk_create | Create a managed disk <disk-name> in resource group <resource-group> with network access policy DenyAll and disk access <disk-access-resource-id> | none |
| compute_disk_create | Create a 128 GB managed disk named <disk-name> in resource group <resource-group> with on-demand bursting enabled | none |
| compute_disk_create | Create a managed disk <disk-name> in resource group <resource-group> with encryption type EncryptionAtRestWithPlatformAndCustomerKeys | none |
| compute_disk_create | Create a V2 hypervisor generation disk named <disk-name> in resource group <resource-group> with 128 GB | none |
| compute_disk_delete | Delete the managed disk <disk-name> in resource group <resource-group> | none |
| compute_disk_delete | Remove managed disk <disk-name> from resource group <resource-group> | none |
| compute_disk_delete | Delete disk <disk-name> in resource group <resource-group> in my subscription | none |
| compute_disk_update | Update disk <disk-name> in resource group <resource-group> to 256 GB | none |
| compute_disk_update | Change the SKU of disk <disk-name> to Premium_LRS | none |
| compute_disk_update | Resize disk <disk-name> in resource group <resource-group> to 512 GB | none |
| compute_disk_update | Update disk <disk-name> to enable bursting | none |
| compute_disk_update | Set the max shares on disk <disk-name> in resource group <resource-group> to 2 | none |
| compute_disk_update | Change the network access policy of disk <disk-name> to DenyAll | none |
| compute_disk_update | Update disk <disk-name> in resource group <resource-group> with tags env=staging | none |
| compute_disk_update | Set the IOPS limit on ultra disk <disk-name> in resource group <resource-group> to 10000 | none |
| compute_disk_update | Update the throughput of disk <disk-name> in resource group <resource-group> to 500 MBps | none |
| compute_disk_update | Change the performance tier of disk <disk-name> in resource group <resource-group> to P40 | none |
| compute_disk_update | Update disk <disk-name> in resource group <resource-group> to use disk encryption set <disk-encryption-set-id> | none |
| compute_disk_update | Change the encryption type of disk <disk-name> in resource group <resource-group> to EncryptionAtRestWithPlatformAndCustomerKeys | none |
| compute_disk_update | Set disk access on disk <disk-name> in resource group <resource-group> to <disk-access-resource-id> with network access policy AllowPrivate | none |
| compute_disk_update | Update disk <disk-name> to Standard_LRS SKU with 512 GB size and tags env=dev | none |

## Azure Confidential Ledger

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| confidentialledger_entries_append | Append an entry to my ledger <ledger_name> with data {"key": "value"} | none |
| confidentialledger_entries_append | Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"} | none |
| confidentialledger_entries_append | Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id> | none |
| confidentialledger_entries_append | Create an immutable ledger entry in <ledger_name> with content {"audit": "log"} | none |
| confidentialledger_entries_append | Write an entry to confidential ledger <ledger_name> | none |
| confidentialledger_entries_get | Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name> | none |
| confidentialledger_entries_get | Get transaction <transaction_id> from ledger <ledger_name> | none |

## Azure Cosmos DB

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| cosmos_list | List all cosmosdb accounts in my subscription | none |
| cosmos_list | Show me my cosmosdb accounts | none |
| cosmos_list | Show me the cosmosdb accounts in my subscription | none |
| cosmos_list | List all the cosmosdb accounts in resource group <resource_group> | none |
| cosmos_list | List all the databases in the cosmosdb account <account_name> | none |
| cosmos_list | List all the databases in the cosmosdb account <account_name> in resource group <resource_group> | none |
| cosmos_list | Show me the databases in the cosmosdb account <account_name> | none |
| cosmos_list | List all the containers in the database <database_name> for the cosmosdb account <account_name> | none |
| cosmos_list | Show me the containers in the database <database_name> for the cosmosdb account <account_name> | none |
| cosmos_database_container_item_query | Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name> | none |
| cosmos_database_container_item_get | Get the document with id <document_id> from container <container_name> in database <database_name> of the cosmosdb account <account_name> | none |
| cosmos_database_container_item_get | Find the document <document_id> in container <container_name> from database <database_name> of the cosmosdb account <account_name> using partition key <partition_key> | none |
| cosmos_database_container_item_list-recent | Show me the 15 most recent documents in container <container_name> of database <database_name> in cosmosdb account <account_name> | none |
| cosmos_database_container_item_list-recent | Get the latest documents from <container_name> in <database_name> for cosmosdb account <account_name> | none |
| cosmos_database_container_item_text-search | Search documents in container <container_name> from database <database_name> of the cosmosdb account <account_name> where <search_property> contains "<search_phrase>" | none |
| cosmos_database_container_item_text-search | Run a full-text search for the word "<search_phrase>" against property <search_property> in container <container_name> of database <database_name> for cosmosdb account <account_name> | none |
| cosmos_database_container_item_vector-search | Find documents similar to "<text_to_search>" in container <container_name> of database <database_name> in cosmosdb account <account_name> using vector property <vector_property> with Azure OpenAI endpoint <endpoint> and deployment <deployment> | context-required |
| cosmos_database_container_item_vector-search | Show me the top <count> documents in container <container_name> of database <database_name> for cosmosdb account <account_name> most similar to "<text_to_search>" using vector property <vector_property>, embedding deployment <deployment> at endpoint <endpoint> with <embedding_dimensions> dimensions, and project only <properties_to_select> | context-required |
| cosmos_database_container_schema_infer | Infer the schema of container <container_name> in database <database_name> for cosmosdb account <account_name> | none |
| cosmos_database_container_schema_infer | Sample <sample_size> documents from container <container_name> in database <database_name> of the cosmosdb account <account_name> and tell me the property names and types | none |

## Azure Data Explorer

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| kusto_cluster_get | Show me the details of the Data Explorer cluster <cluster_name> | none |
| kusto_cluster_list | List all Data Explorer clusters in my subscription | none |
| kusto_cluster_list | Show me my Data Explorer clusters | none |
| kusto_cluster_list | Show me the Data Explorer clusters in my subscription | none |
| kusto_database_list | List all databases in the Data Explorer cluster <cluster_name> | none |
| kusto_database_list | Show me the databases in the Data Explorer cluster <cluster_name> | none |
| kusto_query | Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name> | none |
| kusto_sample | Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name> | none |
| kusto_table_list | List all tables in the Data Explorer database <database_name> in cluster <cluster_name> | none |
| kusto_table_list | Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name> | none |
| kusto_table_schema | Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name> | none |

## Azure Database for MySQL

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| mysql_list | List all MySQL servers in my subscription | none |
| mysql_list | Show me my MySQL servers | none |
| mysql_list | Show me the MySQL servers in my subscription | none |
| mysql_list | List all MySQL databases in server \<server> | none |
| mysql_list | Show me the MySQL databases in server \<server> | none |
| mysql_list | List all tables in the MySQL database \<database> in server \<server> | none |
| mysql_list | Show me the tables in the MySQL database \<database> in server \<server> | none |
| mysql_database_query | Show me all items that contain the word \<search_term> in the MySQL database \<database> in server \<server> | none |
| mysql_server_config_get | Show me the configuration of MySQL server \<server> | none |
| mysql_server_param_get | Show me the value of connection timeout in seconds in my MySQL server \<server> | none |
| mysql_server_param_set | Set connection timeout to 20 seconds for my MySQL server \<server> | none |
| mysql_table_schema_get | Show me the schema of table \<table> in the MySQL database \<database> in server \<server> | none |

## Azure Database for PostgreSQL

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| postgres_list | List all PostgreSQL servers in my subscription | none |
| postgres_list | Show me my PostgreSQL servers | none |
| postgres_list | Show me the PostgreSQL servers in my subscription | none |
| postgres_list | List all PostgreSQL databases in server \<server> | none |
| postgres_list | Show me the PostgreSQL databases in server \<server> | none |
| postgres_list | List all tables in the PostgreSQL database \<database> in server \<server> | none |
| postgres_list | Show me the tables in the PostgreSQL database \<database> in server \<server> | none |
| postgres_list | List all tables in the \<schema> schema of the PostgreSQL database \<database> in server \<server> | none |
| postgres_database_query | Show me all items that contain the word \<search_term> in the PostgreSQL database \<database> in server \<server> | none |
| postgres_server_config_get | Show me the configuration of PostgreSQL server \<server> | none |
| postgres_server_param_get | Show me if the parameter my PostgreSQL server \<server> has replication enabled | none |
| postgres_server_param_set | Enable replication for my PostgreSQL server \<server> | none |
| postgres_table_schema_get | Show me the schema of table \<table> in the PostgreSQL database \<database> in server \<server> | none |

## Azure Deploy

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| deploy_app_logs_get | Show me the log of the application deployed by azd | context-required |
| deploy_architecture_diagram_generate | Generate the Azure architecture diagram for this application | context-required |
| deploy_iac_rules_get | Give me the IaC rules for deploying this application to Azure Container Apps using Azure CLI and Bicep | none |
| deploy_pipeline_guidance_get | Generate a CI/CD pipeline using GitHub Actions workflow to deploy my application to Azure with best practices | context-required |
| deploy_plan_get | Generate an Azure deployment plan for this project using Azure CLI and IaC templates | context-required |

## Azure Device Registry

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| deviceregistry_namespace_list | List all Device Registry namespaces in my subscription | none |
| deviceregistry_namespace_list | Show me the Device Registry namespaces in subscription <subscription> | none |
| deviceregistry_namespace_list | List Device Registry namespaces in resource group <resource_group_name> | none |
| deviceregistry_namespace_list | What Device Registry namespaces do I have in my Azure subscription? | none |

## Azure Event Grid

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| eventgrid_events_publish | Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data> | none |
| eventgrid_events_publish | Publish event to my Event Grid topic <topic_name> with the following events <event_data> | none |
| eventgrid_events_publish | Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data> | none |
| eventgrid_topic_list | List all Event Grid topics in my subscription | none |
| eventgrid_topic_list | Show me the Event Grid topics in my subscription | none |
| eventgrid_topic_list | List all Event Grid topics in subscription <subscription> | none |
| eventgrid_topic_list | List all Event Grid topics in resource group <resource_group_name> in subscription <subscription> | none |
| eventgrid_subscription_list | Show me all Event Grid subscriptions for topic <topic_name> | none |
| eventgrid_subscription_list | List Event Grid subscriptions for topic <topic_name> in subscription <subscription> | none |
| eventgrid_subscription_list | List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name> | none |
| eventgrid_subscription_list | Show all Event Grid subscriptions in my subscription | none |
| eventgrid_subscription_list | List all Event Grid subscriptions in subscription <subscription> | none |
| eventgrid_subscription_list | Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription> | none |
| eventgrid_subscription_list | List Event Grid subscriptions for subscription <subscription> in location <location> | none |

## Azure Event Hubs

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| eventhubs_eventhub_consumergroup_delete | Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name> | none |
| eventhubs_eventhub_consumergroup_get | List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name> | none |
| eventhubs_eventhub_consumergroup_get | Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name> | none |
| eventhubs_eventhub_consumergroup_update | Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name> | none |
| eventhubs_eventhub_consumergroup_update | Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name> | clarification-required |
| eventhubs_eventhub_delete | Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name> | none |
| eventhubs_eventhub_get | List all Event Hubs in my namespace <namespace_name> | none |
| eventhubs_eventhub_get | Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name> | none |
| eventhubs_eventhub_update | Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name> | none |
| eventhubs_eventhub_update | Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name> | clarification-required |
| eventhubs_namespace_delete | Delete Azure Event Hubs namespace <namespace_name> in resource group <resource_group_name> | none |
| eventhubs_namespace_get | List all Event Hubs namespaces in my subscription | none |
| eventhubs_namespace_get | Get the details of my namespace <namespace_name> in my resource group <resource_group_name> | none |
| eventhubs_namespace_update | Create a new Azure Event Hubs namespace <namespace_name> in resource group <resource_group_name> | none |
| eventhubs_namespace_update | Update my namespace <namespace_name> in my resource group <resource_group_name> | clarification-required |

## Azure File Shares

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| fileshares_fileshare_create | Create a new file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_create | Create file share <file_share_name> in resource group <resource_group_name> with 100 GB storage | none |
| fileshares_fileshare_create | Create a file share named <file_share_name> in location <location> with resource group <resource_group_name> | none |
| fileshares_fileshare_create | Set up a new file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_delete | Delete the file share <file_share_name> from resource group <resource_group_name> | none |
| fileshares_fileshare_delete | Remove file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_get | List all file shares in my subscription | none |
| fileshares_fileshare_get | Show me the file shares in resource group <resource_group_name> | none |
| fileshares_fileshare_get | Get details of file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_get | Show me the file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_get | What file shares exist in resource group <resource_group_name>? | none |
| fileshares_limits | Get the file share limits for subscription <subscription> in location <location> | none |
| fileshares_limits | Get the Azure Files share service limits in my subscription for location <location> | none |
| fileshares_limits | Show me the file share service limits in location <location> | none |
| fileshares_fileshare_check-name-availability | Check if file share name <file_share_name> is available in <location> in subscription <subscription> | none |
| fileshares_fileshare_check-name-availability | Is the file share name <file_share_name> available in <location>? | none |
| fileshares_fileshare_check-name-availability | Verify availability of file share name <file_share_name> in <location> | none |
| fileshares_rec | Get Azure Files provisioning recommendations for file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_rec | Show me provisioning recommendations for file share <file_share_name> | none |
| fileshares_rec | Get the Azure Files recommended provisioning settings for file share <file_share_name> | none |
| fileshares_fileshare_snapshot_create | Create a snapshot of file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_snapshot_create | Create a snapshot for file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_snapshot_create | Take a snapshot of file share <file_share_name> | none |
| fileshares_fileshare_snapshot_delete | Delete the snapshot <snapshot_id> from file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_snapshot_delete | Remove snapshot <snapshot_id> from file share <file_share_name> | none |
| fileshares_fileshare_snapshot_get | List all snapshots for file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_snapshot_get | Show me the snapshots of file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_snapshot_get | Get snapshot <snapshot_id> for file share <file_share_name> | clarification-required |
| fileshares_fileshare_snapshot_update | Update the snapshot <snapshot_id> of file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_snapshot_update | Update metadata for snapshot <snapshot_id> of file share <file_share_name> | clarification-required |
| fileshares_fileshare_peconnection_get | List all private endpoint connections for file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_peconnection_get | Show me the private endpoint connections for file share <file_share_name> | none |
| fileshares_fileshare_peconnection_get | Get private endpoint connection <connection_name> for file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_peconnection_get | What private endpoint connections exist for file share <file_share_name>? | none |
| fileshares_fileshare_peconnection_update | Approve the private endpoint connection <connection_name> for file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_peconnection_update | Reject private endpoint connection <connection_name> for file share <file_share_name> | clarification-required |
| fileshares_fileshare_peconnection_update | Update private endpoint connection <connection_name> status to Approved for file share <file_share_name> | none |
| fileshares_fileshare_peconnection_update | Change the status of private endpoint connection <connection_name> to Rejected | clarification-required |
| fileshares_fileshare_update | Update file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_fileshare_update | Update the provisioned storage for file share <file_share_name> to 200 GB | none |
| fileshares_fileshare_update | Modify file share <file_share_name> in resource group <resource_group_name> with new settings | clarification-required |
| fileshares_usage | Get Azure Files usage data for file share <file_share_name> in resource group <resource_group_name> | none |
| fileshares_usage | Show me the usage statistics for file share <file_share_name> | none |
| fileshares_usage | Get the current Azure Files usage for file share <file_share_name> | none |

## Azure Function App

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| functionapp_get | Describe the function app <function_app_name> in resource group <resource_group_name> | none |
| functionapp_get | Get configuration for function app <function_app_name> | none |
| functionapp_get | Get function app status for <function_app_name> | none |
| functionapp_get | Get information about my function app <function_app_name> in <resource_group_name> | none |
| functionapp_get | Retrieve host name and status of function app <function_app_name> | none |
| functionapp_get | Show function app details for <function_app_name> in <resource_group_name> | none |
| functionapp_get | Show me the details for the function app <function_app_name> | none |
| functionapp_get | Show plan and region for function app <function_app_name> | none |
| functionapp_get | What is the status of function app <function_app_name>? | none |
| functionapp_get | List all function apps in my subscription | none |
| functionapp_get | Show me my Azure function apps | none |
| functionapp_get | What function apps do I have? | none |

## Azure Functions Templates

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| functions_language_list | Check the available languages that Azure Functions supports. | none |
| functions_language_list | Use Azure Functions language discovery to list the supported languages and compare them | none |
| functions_language_list | Use Azure Functions runtime discovery to list the available runtime versions | none |
| functions_project_get | Set up a new Azure Functions project in Python | none |
| functions_project_get | Generate the project files for a TypeScript Azure Functions app | none |
| functions_project_get | Use an Azure Functions project template to create boilerplate for a Java app using JDK 21 | none |
| functions_template_get | Get the available triggers and bindings for C# Azure Functions. | none |
| functions_template_get | Show me all the Python Azure Function templates | none |
| functions_template_get | Create a Timer trigger function in C# that runs every 5 minutes | none |
| functions_template_get | Show me a Cosmos DB trigger with an output binding in Java | none |
| functions_template_get | Generate a MCP Tool trigger in TypeScript for Node.js 22 | none |

## Azure Insights

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| insights_get | Generate insights from my current subscription | none |
| insights_get | Summarize what's deployed across my Azure environment and highlight notable patterns | none |
| insights_get | Analyze my tenant and give me insights about the overall infrastructure | none |
| insights_get | Use Azure Insights to analyze my existing Azure environment and summarize its deployed resources | none |
| insights_get | Analyze subscription <subscription_id> for architectural patterns | none |
| insights_get | Analyze my Azure infrastructure and surface patterns to help me plan my next project | none |
| insights_get | Generate insights about my Azure environment to help me plan a new data analytics platform | none |
| insights_get | What insights can you derive about my subscription to help me plan a containerized microservices workload on AKS? | none |

## Azure IoT Hub

| Tool Name | Test Prompt |
|:----------|:----------|
| iothub_device_list | List devices in IoT Hub <hub_name> in resource group <resource_group_name> |
| iothub_device_list | Show all devices registered to IoT Hub <hub_name> |
| iothub_device_list | Get the device registry for IoT Hub <hub_name> in subscription <subscription_id> |
| iothub_hub_get | Get details for IoT Hub <hub_name> in resource group <resource_group_name> |
| iothub_hub_get | Show IoT Hub <hub_name> in resource group <resource_group_name> for subscription <subscription_id> |
| iothub_hub_get | Retrieve IoT Hub <hub_name> metadata from resource group <resource_group_name> |

## Azure Key Vault

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| keyvault_admin_settings_get | Get the account settings for my key vault <key_vault_account_name> | none |
| keyvault_admin_settings_get | Show me the account settings for managed HSM keyvault <key_vault_account_name> | none |
| keyvault_admin_settings_get | Get the value of the <setting_name> administrative setting in Key Vault <key_vault_account_name> | none |
| keyvault_certificate_create | Create a new certificate called <certificate_name> in the key vault <key_vault_account_name> | none |
| keyvault_certificate_create | Generate a certificate named <certificate_name> in key vault <key_vault_account_name> | none |
| keyvault_certificate_create | Request creation of certificate <certificate_name> in the key vault <key_vault_account_name> | none |
| keyvault_certificate_create | Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name> | none |
| keyvault_certificate_create | Issue a certificate <certificate_name> in key vault <key_vault_account_name> | none |
| keyvault_certificate_get | Show me the certificate <certificate_name> in the key vault <key_vault_account_name> | none |
| keyvault_certificate_get | Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name> | none |
| keyvault_certificate_get | Get the certificate <certificate_name> from vault <key_vault_account_name> | none |
| keyvault_certificate_get | Display the certificate details for <certificate_name> in vault <key_vault_account_name> | none |
| keyvault_certificate_get | Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name> | none |
| keyvault_certificate_import | Import the certificate in file <file_path> into the key vault <key_vault_account_name> | context-required |
| keyvault_certificate_import | Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name> | clarification-required |
| keyvault_certificate_import | Upload certificate file <file_path> to key vault <key_vault_account_name> | context-required |
| keyvault_certificate_import | Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name> | context-required |
| keyvault_certificate_import | Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name> | context-required |
| keyvault_certificate_get | List all certificates in the key vault <key_vault_account_name> | none |
| keyvault_certificate_get | Show me the certificates in the key vault <key_vault_account_name> | none |
| keyvault_certificate_get | What certificates are in the key vault <key_vault_account_name>? | none |
| keyvault_certificate_get | List certificate names in vault <key_vault_account_name> | none |
| keyvault_certificate_get | Enumerate certificates in key vault <key_vault_account_name> | none |
| keyvault_certificate_get | Show certificate names in the key vault <key_vault_account_name> | none |
| keyvault_key_create | Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name> | none |
| keyvault_key_create | Generate a key <key_name> with type <key_type> in vault <key_vault_account_name> | none |
| keyvault_key_create | Create an oct key in the vault <key_vault_account_name> | none |
| keyvault_key_create | Create an RSA key in the vault <key_vault_account_name> with name <key_name> | none |
| keyvault_key_create | Create an EC key with name <key_name> in the vault <key_vault_account_name> | none |
| keyvault_key_get | Show me the key <key_name> in the key vault <key_vault_account_name> | none |
| keyvault_key_get | Show me the details of the key <key_name> in the key vault <key_vault_account_name> | none |
| keyvault_key_get | Get the key <key_name> from vault <key_vault_account_name> | none |
| keyvault_key_get | Display the key details for <key_name> in vault <key_vault_account_name> | none |
| keyvault_key_get | Retrieve key metadata for <key_name> in vault <key_vault_account_name> | none |
| keyvault_key_get | List all keys in the key vault <key_vault_account_name> | none |
| keyvault_key_get | Show me the keys in the key vault <key_vault_account_name> | none |
| keyvault_key_get | What keys are in the key vault <key_vault_account_name>? | none |
| keyvault_key_get | List key names in vault <key_vault_account_name> | none |
| keyvault_key_get | Enumerate keys in key vault <key_vault_account_name> | none |
| keyvault_key_get | Show key names in the key vault <key_vault_account_name> | none |
| keyvault_secret_create | Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name> | none |
| keyvault_secret_create | Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name> | none |
| keyvault_secret_create | Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name> | none |
| keyvault_secret_create | Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name> | none |
| keyvault_secret_create | Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name> | none |
| keyvault_secret_get | Show me the secret <secret_name> in the key vault <key_vault_account_name> | none |
| keyvault_secret_get | Show me the details of the secret <secret_name> in the key vault <key_vault_account_name> | none |
| keyvault_secret_get | Get the secret <secret_name> from vault <key_vault_account_name> | none |
| keyvault_secret_get | Display the secret details for <secret_name> in vault <key_vault_account_name> | none |
| keyvault_secret_get | Retrieve secret metadata for <secret_name> in vault <key_vault_account_name> | none |
| keyvault_secret_get | List all secrets in the key vault <key_vault_account_name> | none |
| keyvault_secret_get | Show me the secrets in the key vault <key_vault_account_name> | none |
| keyvault_secret_get | What secrets are in the key vault <key_vault_account_name>? | none |
| keyvault_secret_get | List secrets names in vault <key_vault_account_name> | none |
| keyvault_secret_get | Enumerate secrets in key vault <key_vault_account_name> | none |
| keyvault_secret_get | Show secrets names in the key vault <key_vault_account_name> | none |

## Azure Kubernetes Service (AKS)

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| aks_cluster_get | Get the configuration of AKS cluster \<cluster-name> | none |
| aks_cluster_get | Show me the details of AKS cluster \<cluster-name> in resource group \<resource-group> | none |
| aks_cluster_get | Show me the network configuration for AKS cluster \<cluster-name> | none |
| aks_cluster_get | What are the details of my AKS cluster \<cluster-name> in \<resource-group>? | none |
| aks_cluster_get | List all AKS clusters in my subscription | none |
| aks_cluster_get | Show me my Azure Kubernetes Service clusters | none |
| aks_cluster_get | What AKS clusters do I have? | none |
| aks_nodepool_get | Get details for nodepool \<nodepool-name> in AKS cluster \<cluster-name> in \<resource-group> | none |
| aks_nodepool_get | Show me the configuration for nodepool \<nodepool-name> in AKS cluster \<cluster-name> in resource group \<resource-group> | none |
| aks_nodepool_get | What is the setup of nodepool \<nodepool-name> for AKS cluster \<cluster-name> in \<resource-group>? | none |
| aks_nodepool_get | List nodepools for AKS cluster \<cluster-name> in \<resource-group> | none |
| aks_nodepool_get | Show me the nodepool list for AKS cluster \<cluster-name> in \<resource-group> | none |
| aks_nodepool_get | What nodepools do I have for AKS cluster \<cluster-name> in \<resource-group> | none |

## Azure Load Testing

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| loadtesting_test_create | Create a basic URL test using the following endpoint URL \<test-url> that runs for 30 minutes with 45 virtual users. The test name is \<sample-name> with the test id \<test-id> and the load testing resource is \<load-test-resource> in the resource group \<resource-group> in my subscription | none |
| loadtesting_test_get | Get the load test with id \<test-id> in the load test resource \<test-resource> in resource group \<resource-group> | none |
| loadtesting_testresource_create | Create a load test resource \<load-test-resource-name> in the resource group \<resource-group> in my subscription | none |
| loadtesting_testresource_list | List all load testing resources in the resource group \<resource-group> in my subscription | none |
| loadtesting_testrun_get | Get the load test run with id \<testrun-id> in the load test resource \<test-resource> in resource group \<resource-group> | none |
| loadtesting_testrun_get | Get all the load test runs for the test with id \<test-id> in the load test resource \<test-resource> in resource group \<resource-group> | none |
| loadtesting_testrun_createorupdate | Create a test run using the id \<testrun-id> for test \<test-id> in the load testing resource \<load-testing-resource> in resource group \<resource-group>. Use the name of test run \<display-name> and description as \<description> | none |
| loadtesting_testrun_createorupdate | Update a test run display name as \<display-name> for the id \<testrun-id> for test \<test-id> in the load testing resource \<load-testing-resource> in resource group \<resource-group>. | none |

## Azure Managed Grafana

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| grafana_list | List all Azure Managed Grafana in one subscription | none |

## Azure Managed Lustre

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| managedlustre_fs_create | Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time> | none |
| managedlustre_fs_list | List the Azure Managed Lustre filesystems in my subscription <subscription_name> | none |
| managedlustre_fs_list | List the Azure Managed Lustre filesystems in my resource group <resource_group_name> | none |
| managedlustre_fs_sku_get | List the Azure Managed Lustre SKUs available in location <location> | none |
| managedlustre_fs_blob_autoexport_create | Create an autoexport job for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_autoexport_cancel | Cancel the autoexport job <job_name> for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_autoexport_get | Get the details of autoexport job <job_name> for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_autoexport_get | Show the list of autoexport jobs for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_autoexport_delete | Delete the autoexport job <job_name> for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_autoimport_create | Create an autoimport job for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_autoimport_cancel | Cancel the autoimport job <job_name> for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_autoimport_delete | Delete the autoimport job <job_name> for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_autoimport_get | Get the details of autoimport job <job_name> for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_autoimport_get | Get the details of all the autoimport jobs for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_import_create | Create a one-time import job for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_import_get | Get the details of import job <job_name> for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_import_get | List all one-time import jobs for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_import_cancel | Cancel the one-time import job <job_name> for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_blob_import_delete | Delete the one-time import job <job_name> for the Azure Managed Lustre filesystem <filesystem_name> in resource group <resource_group_name> | none |
| managedlustre_fs_subnetsize_ask | Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku> | none |
| managedlustre_fs_subnetsize_validate | Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku> | none |
| managedlustre_fs_update | Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time> | none |

## Azure Marketplace

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| marketplace_product_get | Get details about marketplace product <product_name> | none |
| marketplace_product_list | Search for Microsoft products in the marketplace | none |
| marketplace_product_list | Show me marketplace products from publisher <publisher_name> | none |

## Azure MCP Best Practices

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| get_azure_bestpractices_get | Get the latest Azure code generation best practices | none |
| get_azure_bestpractices_get | Get the latest Azure deployment best practices | none |
| get_azure_bestpractices_get | Get the latest Azure best practices | none |
| get_azure_bestpractices_get | Get the latest Azure Functions code generation best practices | none |
| get_azure_bestpractices_get | Get the latest Azure Functions deployment best practices | none |
| get_azure_bestpractices_get | Get the latest Azure Functions best practices | none |
| get_azure_bestpractices_get | Get the latest Azure Static Web Apps best practices | none |
| get_azure_bestpractices_get | What are azure function best practices? | none |
| get_azure_bestpractices_get | configure azure mcp in coding agent for my repo | none |
| get_azure_bestpractices_ai_app | Get best practices for building AI applications in Azure | none |
| get_azure_bestpractices_ai_app | Show me the best practices for Microsoft Foundry agents code generation | none |
| get_azure_bestpractices_ai_app | Get guidance for building agents with Microsoft Foundry | none |
| get_azure_bestpractices_ai_app | Create an AI app that helps me to manage travel queries. | none |
| get_azure_bestpractices_ai_app | Create an AI app that helps me to manage travel queries in Microsoft Foundry | none |

## Azure Migrate

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| azuremigrate_platformlandingzone_getguidance | Get guidance for enabling DDoS protection in my Landing Zone | none |
| azuremigrate_platformlandingzone_getguidance | How do I turn off Bastion in my Platform Landing Zone? | none |
| azuremigrate_platformlandingzone_getguidance | Show me how to change IP address ranges in my Platform Landing Zone | none |
| azuremigrate_platformlandingzone_getguidance | Get guidance for implementing zero trust in my Platform Landing Zone | none |
| azuremigrate_platformlandingzone_getguidance | How can I disable a policy in my Platform Landing Zone? | none |
| azuremigrate_platformlandingzone_getguidance | Get guidance for changing resource naming patterns in my Landing Zone | none |
| azuremigrate_platformlandingzone_getguidance | Show me how to modify network topology in my Landing Zone | none |
| azuremigrate_platformlandingzone_getguidance | Get guidance for updating management groups in my Platform Landing Zone | none |
| azuremigrate_platformlandingzone_getguidance | Get Azure Migrate Platform Landing Zone guidance for configuring DDoS protection in an existing landing zone | none |
| azuremigrate_platformlandingzone_getguidance | List all available policies by archetype in my Landing Zone | none |
| azuremigrate_platformlandingzone_getguidance | Use Azure Migrate Platform Landing Zone guidance to find policy assignments related to storage encryption in the landing-zone policy catalog | none |
| azuremigrate_platformlandingzone_request | Check if a platform landing zone already exists for migrate project <migrate-project-name> in resource group <resource-group-name> | none |
| azuremigrate_platformlandingzone_request | Update the landing zone parameters for migrate project <migrate-project-name> in resource group <resource-group-name> | none |
| azuremigrate_platformlandingzone_request | Set up a single region landing zone with Azure Firewall for migrate project <migrate-project-name> | none |
| azuremigrate_platformlandingzone_request | Configure a multi-region landing zone with hub-spoke architecture for migrate project <migrate-project-name> in resource group <resource-group-name> | none |
| azuremigrate_platformlandingzone_request | Generate a platform landing zone for migrate project <migrate-project-name> in resource group <resource-group-name> | none |
| azuremigrate_platformlandingzone_request | Generate a platform landing zone | none |
| azuremigrate_platformlandingzone_request | Generate a platform landing zone and create a new migrate project with name <migrate-project-name> in resource group <resource-group-name> | none |
| azuremigrate_platformlandingzone_request | Start landing zone generation for migrate project <migrate-project-name> | none |
| azuremigrate_platformlandingzone_request | Download the generated landing zone for migrate project <migrate-project-name> in resource group <resource-group-name> | none |
| azuremigrate_platformlandingzone_request | Check parameter status for migrate project <migrate-project-name> in resource group <resource-group-name> | none |
| azuremigrate_platformlandingzone_request | Verify if all parameters are set for migrate project <migrate-project-name> | none |

## Azure Monitor

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| monitor_activitylog_list | List the activity logs of the last month for <resource_name> | none |
| monitor_healthmodels_get | Show me the health model <health_model_name> in resource group <resource_group> | none |
| monitor_healthmodels_list | List the Azure Monitor health models in my subscription | none |
| monitor_healthmodels_list | What health models are in resource group <resource_group>? | none |
| monitor_instrumentation_get-learning-resource | Get the onboarding learning resource at path <resource_path> | none |
| monitor_instrumentation_get-learning-resource | Show me the content of the Azure Monitor onboarding learning resource at path <resource_path> | none |
| monitor_instrumentation_get-learning-resource | Use Azure Monitor instrumentation onboarding to get the learning resource file at path <resource_path> | none |
| monitor_instrumentation_get-learning-resource | List all available Azure Monitor onboarding learning resources | none |
| monitor_instrumentation_get-learning-resource | Show me all learning resource paths for Azure Monitor instrumentation | none |
| monitor_instrumentation_get-learning-resource | What learning resources are available for Azure Monitor instrumentation onboarding? | none |
| monitor_instrumentation_orchestrator-next | After completing the previous Azure Monitor instrumentation step, get the next action for session <session_id> with completion note <completion_note> | none |
| monitor_instrumentation_orchestrator-next | Get the next Azure Monitor instrumentation onboarding action for session <session_id> after I completed <completion_note> | none |
| monitor_instrumentation_orchestrator-next | I finished the previous instrumentation step; return the next step for session <session_id> with note <completion_note> | none |
| monitor_instrumentation_orchestrator-start | Start Azure Monitor instrumentation orchestration for workspace <workspace_path> | none |
| monitor_instrumentation_orchestrator-start | Analyze workspace <workspace_path> and return the first Azure Monitor instrumentation step | none |
| monitor_instrumentation_orchestrator-start | Begin guided Azure Monitor onboarding for project at <workspace_path> and give me step one | none |
| monitor_instrumentation_send-brownfield-analysis | Send brownfield code analysis findings JSON <findings_json> to Azure Monitor instrumentation session <session_id> after analysis was requested | none |
| monitor_instrumentation_send-brownfield-analysis | Submit brownfield analysis findings <findings_json> to Azure Monitor instrumentation session <session_id> | none |
| monitor_instrumentation_send-brownfield-analysis | Send completed brownfield telemetry analysis <findings_json> to Azure Monitor instrumentation onboarding session <session_id> | none |
| monitor_instrumentation_send-enhancement-select | Submit enhancement selection keys <enhancement_keys> for Azure Monitor instrumentation session <session_id> after enhancement options are presented | none |
| monitor_instrumentation_send-enhancement-select | Continue instrumentation enhancement flow by sending selected keys <enhancement_keys> to session <session_id> | none |
| monitor_instrumentation_send-enhancement-select | Send chosen enhancement option keys <enhancement_keys> to Azure Monitor instrumentation onboarding session <session_id> | none |
| monitor_metrics_definitions | Get metric definitions for <resource_type> <resource_name> from the namespace | none |
| monitor_metrics_definitions | Show me all available metrics and their definitions for storage account <account_name> | none |
| monitor_metrics_definitions | What metric definitions are available for the Application Insights resource <resource_name> | none |
| monitor_metrics_query | Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period> | none |
| monitor_metrics_query | Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period> | none |
| monitor_metrics_query | Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals | none |
| monitor_metrics_query | Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period> | none |
| monitor_metrics_query | Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period> | none |
| monitor_metrics_query | What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period> | none |
| monitor_resource_log_query | Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name> | none |
| monitor_table_list | List all tables in the Log Analytics workspace <workspace_name> | none |
| monitor_table_list | Show me the tables in the Log Analytics workspace <workspace_name> | none |
| monitor_table_type_list | List all available table types in the Log Analytics workspace <workspace_name> | none |
| monitor_table_type_list | Show me the available table types in the Log Analytics workspace <workspace_name> | none |
| monitor_webtests_createorupdate | Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component> | none |
| monitor_webtests_createorupdate | Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component> | none |
| monitor_workspace_list | List all Log Analytics workspaces in my subscription | none |
| monitor_workspace_list | Show me my Log Analytics workspaces | none |
| monitor_workspace_list | Show me the Log Analytics workspaces in my subscription | none |
| monitor_workspace_log_query | Show me the logs for the past hour in the Log Analytics workspace <workspace_name> | none |

## Azure Native ISV

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| datadog_monitoredresources_list | List all monitored resources in the Datadog resource <resource_name> | none |
| datadog_monitoredresources_list | Show me the monitored resources in the Datadog resource <resource_name> | none |

## Azure Quick Review CLI

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| extension_azqr | Check my Azure subscription for any compliance issues or recommendations | none |
| extension_azqr | Provide compliance recommendations for my current Azure subscription | none |
| extension_azqr | Scan my Azure subscription for compliance recommendations | none |

## Azure Quota

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| quota_region_availability_list | Show me the available regions for these resource types <resource_types> | none |
| quota_usage_check | Check usage information for <resource_type> in region <region> | none |

## Azure RBAC

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| role_assignment_list | List all available role assignments in my subscription | none |
| role_assignment_list | Show me the available role assignments in my subscription | none |

## Azure Redis

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| redis_create | Create a new Redis resource named <resource_name> with SKU <sku_name> in resource group <resource_group_name> | none |
| redis_create | Create a new Redis resource for me | clarification-required |
| redis_create | Create a Redis cache named <resource_name> with SKU <sku_name> in resource group <resource_group_name> | none |
| redis_create | Create a new Redis cluster with name <resource_name>, SKU <sku_name> | clarification-required |
| redis_list | List all Redis resources in my subscription | none |
| redis_list | Show me my Redis resources | none |
| redis_list | Show me the Redis resources in my subscription | none |
| redis_list | Show me my Redis caches | none |
| redis_list | Get Redis clusters | none |

## Azure Resilience Management

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| resilience_drill_get | List all resilience drills in service group <service_group> | none |
| resilience_drill_get | Get the details of resilience drill <drill_name> in service group <service_group> | none |
| resilience_drill_resource_get | List all drill resources for resilience drill <drill_name> in service group <service_group> | none |
| resilience_drill_resource_get | List all drill targets for resilience drill <drill_name> in service group <service_group> | none |
| resilience_drill_resource_get | Show the resources targeted by resilience drill <drill_name> in service group <service_group> | none |
| resilience_drill_resource_get | Get the complete details of drill resource <resource_name> for resilience drill <drill_name> in service group <service_group> | none |
| resilience_drill_resource_get | Get drill target <resource_name> for resilience drill <drill_name> in service group <service_group> | none |
| resilience_drill_resource_get | Retrieve the ARM properties of drill resource <resource_name> for resilience drill <drill_name> in service group <service_group> | none |
| resilience_drill_run_get | List all runs of drill <drill_name> in service group <service_group> | none |
| resilience_drill_run_get | Get drill run <drill_run_name> for drill <drill_name> in service group <service_group> | none |
| resilience_drill_run_resource_get | List all resources of drill run <drill_run_name> for drill <drill_name> in service group <service_group> | none |
| resilience_drill_run_resource_get | Get resource <resource_name> from drill run <drill_run_name> for drill <drill_name> in service group <service_group> | none |
| resilience_goal_assignment_get | List all resilience goal assignments in service group <service_group> | none |
| resilience_goal_assignment_get | Get the details of goal assignment <goal_assignment_name> in service group <service_group> | none |
| resilience_goal_resource_get | List all resources (members) of goal assignment <goal_assignment_name> in service group <service_group> | none |
| resilience_goal_resource_get | Get the goal resource <resource_name> for goal assignment <goal_assignment_name> in service group <service_group> | none |
| resilience_goal_template_get | List all resilience goal templates in service group <service_group> | none |
| resilience_goal_template_get | Get the details of goal template <goal_template_name> in service group <service_group> | none |
| resilience_recovery_job_get | List all recovery jobs of recovery plan <recovery_plan_name> in service group <service_group> | none |
| resilience_recovery_job_get | Get the details of recovery job <recovery_job_name> for recovery plan <recovery_plan_name> in service group <service_group> | none |
| resilience_recovery_job_resource_get | List all resources (targets) of recovery job <recovery_job_name> for recovery plan <recovery_plan_name> in service group <service_group> | none |
| resilience_recovery_job_resource_get | Get the recovery job resource <resource_name> for recovery job <recovery_job_name> of recovery plan <recovery_plan_name> in service group <service_group> | none |
| resilience_recovery_plan_get | List all resilience recovery plans in service group <service_group> | none |
| resilience_recovery_plan_get | Get the details of recovery plan <recovery_plan_name> in service group <service_group> | none |
| resilience_recovery_plan_resource_get | List all resources (members) of recovery plan <recovery_plan_name> in service group <service_group> | none |
| resilience_recovery_plan_resource_get | Get the recovery resource <resource_name> for recovery plan <recovery_plan_name> in service group <service_group> | none |
| resilience_usageplan_create | Create a resilience usage plan <usage_plan_name> with plan type Basic in resource group <resource_group_name> | none |
| resilience_usageplan_create | Set up a Basic resilience usage plan named <usage_plan_name> in resource group <resource_group_name> | none |
| resilience_usageplan_create | Update resilience usage plan <usage_plan_name> in resource group <resource_group_name> to use the Basic plan type | none |
| resilience_usageplan_enrollment_create | Create a usage plan enrollment <enrollment_name> for usage plan <usage_plan_name> associated with service group <service_group> in resource group <resource_group_name> | none |
| resilience_usageplan_enrollment_create | Enroll service group <service_group> in usage plan <usage_plan_name> as <enrollment_name> in resource group <resource_group_name> | none |
| resilience_usageplan_enrollment_create | Update enrollment <enrollment_name> under usage plan <usage_plan_name> to use service group <service_group> in resource group <resource_group_name> | none |
| resilience_usageplan_enrollment_get | List all enrollments of usage plan <usage_plan_name> in resource group <resource_group_name> | none |
| resilience_usageplan_enrollment_get | Get the details of usage plan enrollment <enrollment_name> for usage plan <usage_plan_name> in resource group <resource_group_name> | none |
| resilience_usageplan_get | List all resilience usage plans in my subscription | none |
| resilience_usageplan_get | List all resilience usage plans in resource group <resource_group_name> | none |
| resilience_usageplan_get | Get the details of usage plan <usage_plan_name> in resource group <resource_group_name> | none |

## Azure Resource Group

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| group_list | List all resource groups in my subscription | none |
| group_list | Show me my resource groups | none |
| group_list | Show me the resource groups in my subscription | none |
| group_resource_list | List all resources in my resource group | none |
| group_resource_list | Show me what resources are in the resource group myRG | none |
| group_resource_list | What resources exist in resource group myRG? | none |

## Azure Resource Health

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| resourcehealth_availability-status_get | Get the availability status for resource <resource_name> | none |
| resourcehealth_availability-status_get | What is the Azure Resource Health availability status of the storage account <storage_account_name>? | none |
| resourcehealth_availability-status_get | What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>? | none |
| resourcehealth_availability-status_get | Get Azure Resource Health availability status for all resources in my subscription | none |
| resourcehealth_availability-status_get | Show me the health status of all my Azure resources | none |
| resourcehealth_availability-status_get | What resources in resource group <resource_group_name> have health issues? | none |
| resourcehealth_health-events_list | List all service health events in my subscription | none |
| resourcehealth_health-events_list | Show me Azure service health events for subscription <subscription_id> | none |
| resourcehealth_health-events_list | What service issues have occurred in the last 30 days? | none |
| resourcehealth_health-events_list | List active service health events in my subscription | none |
| resourcehealth_health-events_list | Show me planned maintenance events for my Azure services | none |

## Azure Policy
| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| policy_assignment_list | List Azure Policies in the subscription <subscription_id> | none |

## Azure Pricing

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| pricing_get | What is the price of Standard_D4s_v5 VMs? | none |
| pricing_get | What's the price difference between Premium_LRS and Standard_LRS storage? | clarification-required |
| pricing_get | Get consumption price for Standard_E8s_v5 in brazil | none |
| pricing_get | What is the price for Virtual Machines? | clarification-required |
| pricing_get | How much does a Standard_D4s_v5 VM cost per hour? | none |
| pricing_get | Which is cheaper- Standard_D4s_v5 in eastus vs westeurope | none |
| pricing_get | Get pricing where productName contains 'Premium' | none |
| pricing_get | How much does Hot access tier storage cost per GB in westeurope? | none |
| pricing_get | Show savings plan prices for Standard_E4s_v5 Linux VMs | none |
| pricing_get | Here's my Bicep template. Can you estimate the monthly cost of this deployment? <bicep_template> | clarification-required |

## Azure Service Bus

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| servicebus_queue_details | Show me the details of service bus <service_bus_name> queue <queue_name> | none |
| servicebus_topic_details | Show me the details of service bus <service_bus_name> topic <topic_name> | none |
| servicebus_topic_subscription_details | Show me the details of service bus <service_bus_name> subscription <subscription_name> | none |

## Azure Service Fabric

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| servicefabric_managedcluster_node_get | Get all nodes in Service Fabric managed cluster <cluster_name> in resource group <resource_group_name> | none |
| servicefabric_managedcluster_node_get | Show me the nodes and their status for Service Fabric managed cluster <cluster_name> | none |
| servicefabric_managedcluster_node_get | Get node <node_name> from Service Fabric managed cluster <cluster_name> | none |
| servicefabric_managedcluster_nodetype_restart | Restart nodes <node_name_1> and <node_name_2> in Service Fabric managed cluster <cluster_name> UD by UD | none |
| servicefabric_managedcluster_nodetype_restart | Restart node <node_name_1> in node type <node_type_name> on managed cluster <cluster_name> | none |

## Azure SignalR

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| signalr_runtime_get | Show me the details of SignalR <signalr_name> | none |
| signalr_runtime_get | Show me the network information of SignalR runtime <signalr_name> | none |
| signalr_runtime_get | Describe the SignalR runtime <signalr_name> in resource group <resource_group_name> | none |
| signalr_runtime_get | Get information about my SignalR runtime <signalr_name> in <resource_group_name> | none |
| signalr_runtime_get | Show all the SignalRs information in <resource_group_name> | none |
| signalr_runtime_get | List all SignalRs in my subscription | none |

## Azure SQL Database

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| sql_db_create | Create a new SQL database named <database_name> in server <server_name> | none |
| sql_db_create | Create a SQL database <database_name> with Basic tier in server <server_name> | none |
| sql_db_create | Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name> | none |
| sql_db_delete | Delete the SQL database <database_name> from server <server_name> | none |
| sql_db_delete | Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name> | none |
| sql_db_delete | Delete the database called <database_name> on server <server_name> | clarification-required |
| sql_db_get | List all Azure SQL databases in server <server_name> | none |
| sql_db_get | List all databases in the Azure SQL server <server_name> | none |
| sql_db_get | Show me the Azure SQL database <database_name> details in server <server_name> | none |
| sql_db_get | Show me the Azure SQL database <database_name> in server <server_name> | none |
| sql_db_rename | Rename the SQL database <database_name> on server <server_name> to <new_database_name> | none |
| sql_db_rename | Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name> | none |
| sql_db_update | Update the performance tier of SQL database <database_name> on server <server_name> | none |
| sql_db_update | Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU | none |

## Azure SQL Elastic Pool Operations

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| sql_elastic-pool_list | List all elastic pools in SQL server <server_name> | none |
| sql_elastic-pool_list | Show me the elastic pools configured for SQL server <server_name> | none |
| sql_elastic-pool_list | What elastic pools are available in my SQL server <server_name>? | none |

## Azure SQL Server Operations

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| sql_server_create | Create a new Azure SQL server named <server_name> in resource group <resource_group_name> | none |
| sql_server_create | Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user> | none |
| sql_server_create | Set up a new SQL server called <server_name> in my resource group <resource_group_name> | none |
| sql_server_delete | Delete the Azure SQL server <server_name> from resource group <resource_group_name> | none |
| sql_server_delete | Remove the SQL server <server_name> from my subscription | none |
| sql_server_delete | Delete SQL server <server_name> permanently | none |
| sql_server_entra-admin_list | List Microsoft Entra ID administrators for SQL server <server_name> | none |
| sql_server_entra-admin_list | Show me the Entra ID administrators configured for SQL server <server_name> | none |
| sql_server_entra-admin_list | What Microsoft Entra ID administrators are set up for my SQL server <server_name>? | none |
| sql_server_firewall-rule_create | Create a firewall rule for my Azure SQL server <server_name> | none |
| sql_server_firewall-rule_create | Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name> | none |
| sql_server_firewall-rule_create | Create a new firewall rule named <rule_name> for SQL server <server_name> | none |
| sql_server_firewall-rule_delete | Delete a firewall rule from my Azure SQL server <server_name> | none |
| sql_server_firewall-rule_delete | Remove the firewall rule <rule_name> from SQL server <server_name> | none |
| sql_server_firewall-rule_delete | Delete firewall rule <rule_name> for SQL server <server_name> | none |
| sql_server_firewall-rule_list | List all firewall rules for SQL server <server_name> | none |
| sql_server_firewall-rule_list | Show me the firewall rules for SQL server <server_name> | none |
| sql_server_firewall-rule_list | What firewall rules are configured for my SQL server <server_name>? | none |
| sql_server_get | List all Azure SQL servers in resource group <resource_group_name> | none |
| sql_server_get | Show me every Azure SQL server in resource group <resource_group_name> | none |
| sql_server_get | Show me the Azure SQL server <server_name> details | none |
| sql_server_get | Get Azure SQL server <server_name> info | none |
| sql_server_get | Display the properties of Azure SQL server <server_name> | none |

## Azure SRE Agent

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| sreagent_agents_list | List all Azure SRE Agent resources in my subscription | none |
| sreagent_agents_get | Show me the details of SRE Agent <agent_name> in resource group <resource_group> | none |
| sreagent_agents_create | Use SRE Agent sub-agent management to create sub-agent <name> on SRE Agent <agent_name> | none |
| sreagent_agents_delete | Use SRE Agent sub-agent management to delete sub-agent <name> from SRE Agent <agent_name> | none |
| sreagent_agents_tools_list | List the custom tools attached to SRE Agent <agent_name> | none |
| sreagent_agents_tools_get | Get the definition of custom tool <tool_name> from SRE Agent <agent_name> | none |
| sreagent_agents_tools_create | Create a custom tool called <tool_name> on SRE Agent <agent_name> | none |
| sreagent_skills_list | List all skills available on SRE Agent <agent_name> | none |
| sreagent_skills_create | Add a new skill called <skill_name> to SRE Agent <agent_name> | none |
| sreagent_skills_delete | Delete the skill <skill_name> from SRE Agent <agent_name> | none |
| sreagent_connectors_list | List the connectors configured on SRE Agent <agent_name> | none |
| sreagent_connectors_get | Show me the details of connector <connector_name> on SRE Agent <agent_name> | none |
| sreagent_connectors_create_kusto | Create a Kusto connector on SRE Agent <agent_name> | none |
| sreagent_connectors_create_mcp | Create an MCP connector on SRE Agent <agent_name> | none |
| sreagent_connectors_delete | Remove the connector <connector_name> from SRE Agent <agent_name> | none |
| sreagent_connectors_test | Test the connector <connector_name> on SRE Agent <agent_name> and list its tools | none |
| sreagent_hooks_list | List the hooks configured for SRE Agent <agent_name> | none |
| sreagent_hooks_get | Show me the details of hook <hook_name> on SRE Agent <agent_name> | none |
| sreagent_hooks_delete | Remove and permanently delete hook <hook_name> from SRE Agent <agent_name> | none |
| sreagent_hooks_thread_list | List the hook activation states for thread <thread_id> on SRE Agent <agent_name> | none |
| sreagent_hooks_thread_activate | Activate hook <hook_name> on thread <thread_id> of SRE Agent <agent_name> | none |
| sreagent_hooks_thread_deactivate | Deactivate hook <hook_name> on thread <thread_id> of SRE Agent <agent_name> | none |
| sreagent_threads_list | List the active threads on SRE Agent <agent_name> | none |
| sreagent_threads_get | Show me thread <thread_id> on SRE Agent <agent_name> | none |
| sreagent_threads_create | Start a new thread on SRE Agent <agent_name> | none |
| sreagent_threads_send_message | Send a message to thread <thread_id> on SRE Agent <agent_name> | none |
| sreagent_threads_investigate | Use SRE Agent <agent_name> to investigate this issue: <issue> | none |
| sreagent_threads_investigate_yolo | Investigate <issue> on SRE Agent <agent_name> in yolo mode, automatically granting all pending approvals without waiting | none |
| sreagent_threads_delete | Delete thread <thread_id> from SRE Agent <agent_name> | none |
| sreagent_scheduledtasks_list | List the scheduled tasks on SRE Agent <agent_name> | none |
| sreagent_scheduledtasks_get | Show me the scheduled task <task_id> on SRE Agent <agent_name> | none |
| sreagent_scheduledtasks_create | Schedule a recurring task on SRE Agent <agent_name> that runs every Monday | clarification-required |
| sreagent_scheduledtasks_pause | Pause the scheduled task <task_id> on SRE Agent <agent_name> | none |
| sreagent_scheduledtasks_resume | Resume the scheduled task <task_id> on SRE Agent <agent_name> | none |
| sreagent_scheduledtasks_delete | Delete the scheduled task <task_id> from SRE Agent <agent_name> | none |
| sreagent_incidents_active_list | List the active incidents on SRE Agent <agent_name> | none |
| sreagent_incidents_create | Create a new incident investigation for SRE Agent <agent_name> with title <title> | none |
| sreagent_incidents_plans_list | List the incident response plans configured on SRE Agent <agent_name> | none |
| sreagent_incidents_plans_create | Enable a new incident response plan on SRE Agent <agent_name> with alert filter <filter> and handler <handler> | none |
| sreagent_incidents_setup_pagerduty | Configure the PagerDuty incident-management integration for SRE Agent <agent_name> | none |
| sreagent_incidents_setup_servicenow | Configure the ServiceNow incident-management integration for SRE Agent <agent_name> | none |
| sreagent_workflows_generate | Generate a YAML workflow for a tool named <tool_name> | clarification-required |
| sreagent_workflows_validate | Validate the following SRE Agent workflow YAML | context-required |
| sreagent_workflows_apply | Apply the workflow YAML to SRE Agent <agent_name> | none |
| sreagent_docs_get | Show me the SRE Agent documentation for the topic <topic> | none |
| sreagent_docs_memories_list | Get a complete list of all indexed knowledge base documents stored in SRE Agent <agent_name> memory without filtering | none |
| sreagent_docs_memories_search | Search the SRE Agent knowledge base for <text> | none |
| sreagent_docs_memories_add | Add a document called <name> to the SRE Agent knowledge base | none |
| sreagent_docs_memories_delete | Confirm and delete knowledge base document <name> from SRE Agent <agent_name> | none |
| sreagent_docs_memories_reindex | Reindex the knowledge base documents for SRE Agent <agent_name> | none |
| sreagent_architecture_plan | Use SRE Agent architecture planning for these requirements: <requirements> | none |
| sreagent_commonprompts_list | List the common prompts on SRE Agent <agent_name> | none |
| sreagent_commonprompts_get | Show me the common prompt <prompt_name> on SRE Agent <agent_name> | none |
| sreagent_commonprompts_create | Create a common prompt called <prompt_name> on SRE Agent <agent_name> | none |
| sreagent_commonprompts_delete | Permanently remove and erase common prompt <prompt_name> from SRE Agent <agent_name> | none |

## Azure Storage

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| storage_account_create | Create a new storage account called testaccount123 in East US region | clarification-required |
| storage_account_create | Create a storage account with premium performance and LRS replication | clarification-required |
| storage_account_create | Create a new storage account with Data Lake Storage Gen2 enabled | clarification-required |
| storage_account_get | Show me the details for my storage account <account> | none |
| storage_account_get | Get details about the storage account <account> | none |
| storage_account_get | List all storage accounts in my subscription including their location and SKU | none |
| storage_account_get | Show me my storage accounts with whether hierarchical namespace (HNS) is enabled | none |
| storage_account_get | Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings | none |
| storage_blob_container_create | Create the storage container mycontainer in storage account <account> | none |
| storage_blob_container_get | Show me the properties of the storage container <container> in the storage account <account> | none |
| storage_blob_container_get | List all blob containers in the storage account <account> | none |
| storage_blob_container_get | List all blob containers in the storage account <account> with prefix <prefix> | none |
| storage_blob_container_get | Show me the containers in the storage account <account> | none |
| storage_blob_get | Show me the properties for blob <blob> in container <container> in storage account <account> | none |
| storage_blob_get | Get the details about blob <blob> in the container <container> in storage account <account> | none |
| storage_blob_get | List all blobs in the blob container <container> in the storage account <account> | none |
| storage_blob_get | List all blobs in the blob container <container> in the storage account <account> with prefix <prefix> | none |
| storage_blob_get | Show me the blobs in the blob container <container> in the storage account <account> | none |
| storage_blob_upload | Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account> | none |
| storage_table_list | List all tables in the storage account <account> | none |
| storage_table_list | Show me the tables in the storage account <account> | none |

## Azure Storage Sync

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| storagesync_service_create | Create a new Storage Sync Service named <service-name> in resource group <resource-group-name> at location <location> | none |
| storagesync_service_delete | Delete the Storage Sync Service <service-name> from resource group <resource-group-name> | none |
| storagesync_service_get | Get the details of Storage Sync Service <service-name> in resource group <resource-group-name> | none |
| storagesync_service_get | List all Storage Sync Services in resource group <resource-group-name> | none |
| storagesync_service_update | Update Storage Sync Service <service-name> with new tags | clarification-required |
| storagesync_registeredserver_get | Get the details of registered server <server-name> in service <service-name> | none |
| storagesync_registeredserver_get | List all registered servers in service <service-name> | none |
| storagesync_registeredserver_unregister | Unregister server <server-name> from service <service-name> | none |
| storagesync_registeredserver_update | Update registered server <server-name> configuration in service <service-name> | clarification-required |
| storagesync_syncgroup_create | Create a new sync group named <syncgroup-name> in service <service-name> | none |
| storagesync_syncgroup_delete | Delete the sync group <syncgroup-name> from service <service-name> | none |
| storagesync_syncgroup_get | Get the details of sync group <syncgroup-name> in service <service-name> | none |
| storagesync_cloudendpoint_changedetection | Trigger change detection on cloud endpoint <endpoint-name> in sync group <syncgroup-name> in service <service-name> for directory path <path> | none |
| storagesync_cloudendpoint_create | Create a new cloud endpoint named <endpoint-name> for Azure file share <share-name> in storage account <storage-account-name> | clarification-required |
| storagesync_cloudendpoint_delete | Delete the cloud endpoint <endpoint-name> from sync group <syncgroup-name> | clarification-required |
| storagesync_cloudendpoint_get | Get the details of cloud endpoint <endpoint-name> in sync group <syncgroup-name> | none |
| storagesync_cloudendpoint_get | List all cloud endpoints in sync group <syncgroup-name> | none |
| storagesync_serverendpoint_create | Create a new server endpoint on server <server-name> pointing to local path <local-path> in sync group <syncgroup-name> | none |
| storagesync_serverendpoint_delete | Delete the server endpoint <endpoint-name> from sync group <syncgroup-name> | none |
| storagesync_serverendpoint_get | Get the details of server endpoint <endpoint-name> in sync group <syncgroup-name> | none |
| storagesync_serverendpoint_get | List all server endpoints in sync group <syncgroup-name> | none |
| storagesync_serverendpoint_update | Update server endpoint <endpoint-name> with cloud tiering enabled and tiering policy in sync group <syncgroup-name> | none |

## Azure Subscription Management

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| subscription_list | List all subscriptions for my account | none |
| subscription_list | Show me my subscriptions | none |
| subscription_list | What is my current subscription? | none |
| subscription_list | What subscriptions do I have? | none |

## Azure Terraform Best Practices

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| azureterraformbestpractices_get | Fetch the Azure Terraform best practices | none |
| azureterraformbestpractices_get | Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault | none |

## Azure Terraform

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| azureterraform_azurerm_get | Get the documentation for azurerm_virtual_network | none |
| azureterraform_azurerm_get | Show me the Terraform provider arguments for azurerm_storage_account | none |
| azureterraform_azurerm_get | Get the data source documentation for azurerm_subscription | none |
| azureterraform_azurerm_get | Get the Terraform AzureRM provider documentation for the 'sku' argument of azurerm_storage_account | none |
| azureterraform_azapi_get | Get AzAPI Terraform provider documentation for Microsoft.Storage/storageAccounts | none |
| azureterraform_azapi_get | Get AzAPI docs for Microsoft.Network/virtualNetworks | none |
| azureterraform_azapi_get | Get AzAPI Terraform provider documentation for Microsoft.Compute/virtualMachines with API version 2024-07-01 | none |
| azureterraform_avm_list | List all available Azure Verified Modules | none |
| azureterraform_avm_list | Show me the available AVM modules for Terraform | none |
| azureterraform_avm_list | List the AVM pattern modules available for Terraform | none |
| azureterraform_avm_versions | Show all versions of avm-res-network-virtualnetwork | none |
| azureterraform_avm_versions | What versions are available for avm-res-storage-storageaccount? | none |
| azureterraform_avm_versions | Show all versions of the avm-ptn-aiml-ai-foundry pattern module | none |
| azureterraform_avm_get | Get the documentation for avm-res-storage-storageaccount version 0.1.0 | none |
| azureterraform_avm_get | Get the documentation for the latest version of Azure Verified Module avm-res-network-virtualnetwork | none |
| azureterraform_avm_get | Get the documentation for the avm-ptn-aiml-ai-foundry pattern module | none |
| azureterraform_aztfexport_resource | Export the resource /subscriptions/<subscription>/resourceGroups/<resource-group>/providers/Microsoft.Storage/storageAccounts/<account> to Terraform | none |
| azureterraform_aztfexport_resource | Generate an aztfexport command to export a single Azure resource to Terraform | clarification-required |
| azureterraform_aztfexport_resourcegroup | Export all resources in resource group my-rg to Terraform | none |
| azureterraform_aztfexport_resourcegroup | Export resource group my-rg to Terraform using the azapi provider | none |
| azureterraform_aztfexport_query | Generate an aztfexport command that uses a resource graph query to export all storage accounts in my subscription | none |
| azureterraform_aztfexport_query | Generate an aztfexport query command to export resources matching "type == 'Microsoft.Storage/storageAccounts'" | none |
| azureterraform_conftest_workspace | Validate Terraform files in ./my-terraform-folder against Azure security policies | context-required |
| azureterraform_conftest_workspace | Validate Terraform files in ./infra using the avmsec policy set | context-required |
| azureterraform_conftest_plan | Validate my Terraform plan file against Azure-Proactive-Resiliency-Library-v2 policies | context-required |
| azureterraform_conftest_plan | Validate a Terraform plan JSON file in ./plan-output against Azure policies with high severity filter | context-required |

## Azure Virtual Desktop

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| virtualdesktop_hostpool_list | List all host pools in my subscription | none |
| virtualdesktop_hostpool_host_list | List all session hosts in host pool <hostpool_name> | none |
| virtualdesktop_hostpool_host_user-list | List all user sessions on session host <sessionhost_name> in host pool <hostpool_name> | none |

## Azure Well-Architected Framework

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| wellarchitectedframework_serviceguide_get | List all services with Well-Architected Framework guidance | none |
| wellarchitectedframework_serviceguide_get | What services have architectural guidance? | none |
| wellarchitectedframework_serviceguide_get | Get Well-Architected Framework guidance for App Service | none |
| wellarchitectedframework_serviceguide_get | What's the waf guidance for a VM? | none |
| wellarchitectedframework_serviceguide_get | What's the architectural guidance for Azure Cosmos DB | none |

## Azure Workbooks

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| workbooks_create | Create a new workbook named <workbook_name> | none |
| workbooks_delete | Delete the workbook with resource ID <workbook_resource_id> | none |
| workbooks_list | List all workbooks in my resource group <resource_group_name> | none |
| workbooks_list | What workbooks do I have in resource group <resource_group_name>? | none |
| workbooks_show | Get information about the workbook with resource ID <workbook_resource_id> | none |
| workbooks_show | Show me the workbook with resource ID <workbook_resource_id> | none |
| workbooks_update | Update the workbook <workbook_resource_id> with a new text step | clarification-required |

## Bicep

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| bicepschema_get | How can I use Bicep to create an Azure OpenAI service? | none |

## Cloud Architect

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| cloudarchitect_design | Help me design an Azure cloud service that will serve as an ATM for users | none |
| cloudarchitect_design | I want to design a cloud app for ordering groceries | none |
| cloudarchitect_design | How can I design a cloud service in Azure that will store and present videos for users? | none |

## Microsoft Foundry Extensions

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| foundryextensions_knowledge_index_list | List all knowledge indexes in my Microsoft Foundry project | none |
| foundryextensions_knowledge_index_list | Show me the knowledge indexes in my Microsoft Foundry project | none |
| foundryextensions_knowledge_index_schema | Show me the schema for knowledge index \<index-name> in my Microsoft Foundry resource | none |
| foundryextensions_knowledge_index_schema | Get the schema configuration for knowledge index \<index-name> | none |
| foundryextensions_openai_chat-completions-create | Create a chat completion with the message "Hello, how are you today?" using my Microsoft Foundry resource | none |
| foundryextensions_openai_create-completion | Create a completion with the prompt "What is Azure?" using my Microsoft Foundry resource | none |
| foundryextensions_openai_embeddings-create | Generate embeddings for the text "Azure OpenAI Service" using my Microsoft Foundry resource | none |
| foundryextensions_openai_embeddings-create | Create vector embeddings for my text using my Microsoft Foundry resource | none |
| foundryextensions_openai_models-list | List all available OpenAI models in my Microsoft Foundry resource | none |
| foundryextensions_openai_models-list | Use Microsoft Foundry Extensions to list the OpenAI model deployments in my Microsoft Foundry resource | none |
| foundryextensions_resource_get | Use Microsoft Foundry Extensions to list all Microsoft Foundry resources in my subscription | none |
| foundryextensions_resource_get | Use Microsoft Foundry Extensions to list Microsoft Foundry resources in resource group <resource_group_name> | none |
| foundryextensions_resource_get | Use Microsoft Foundry Extensions to get details for resource <resource_name> in resource group <resource_group_name> | none |
