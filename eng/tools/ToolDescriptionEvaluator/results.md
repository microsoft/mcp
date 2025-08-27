# Tool Selection Analysis Setup

**Setup completed:** 2025-08-27 15:41:05  
**Tool count:** 131  
**Database setup time:** 1.9952344s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-08-27 15:41:05  
**Tool count:** 131  

## Table of Contents

- [Test 1: azmcp_foundry_knowledge_index_list](#test-1)
- [Test 2: azmcp_foundry_knowledge_index_list](#test-2)
- [Test 3: azmcp_foundry_models_deploy](#test-3)
- [Test 4: azmcp_foundry_models_deployments_list](#test-4)
- [Test 5: azmcp_foundry_models_deployments_list](#test-5)
- [Test 6: azmcp_foundry_models_list](#test-6)
- [Test 7: azmcp_foundry_models_list](#test-7)
- [Test 8: azmcp_search_index_describe](#test-8)
- [Test 9: azmcp_search_index_list](#test-9)
- [Test 10: azmcp_search_index_list](#test-10)
- [Test 11: azmcp_search_index_query](#test-11)
- [Test 12: azmcp_search_service_list](#test-12)
- [Test 13: azmcp_search_service_list](#test-13)
- [Test 14: azmcp_search_service_list](#test-14)
- [Test 15: azmcp_appconfig_account_list](#test-15)
- [Test 16: azmcp_appconfig_account_list](#test-16)
- [Test 17: azmcp_appconfig_account_list](#test-17)
- [Test 18: azmcp_appconfig_kv_delete](#test-18)
- [Test 19: azmcp_appconfig_kv_list](#test-19)
- [Test 20: azmcp_appconfig_kv_list](#test-20)
- [Test 21: azmcp_appconfig_kv_lock](#test-21)
- [Test 22: azmcp_appconfig_kv_set](#test-22)
- [Test 23: azmcp_appconfig_kv_show](#test-23)
- [Test 24: azmcp_appconfig_kv_unlock](#test-24)
- [Test 25: azmcp_extension_az](#test-25)
- [Test 26: azmcp_extension_az](#test-26)
- [Test 27: azmcp_extension_az](#test-27)
- [Test 28: azmcp_acr_registry_list](#test-28)
- [Test 29: azmcp_acr_registry_list](#test-29)
- [Test 30: azmcp_acr_registry_list](#test-30)
- [Test 31: azmcp_acr_registry_list](#test-31)
- [Test 32: azmcp_acr_registry_list](#test-32)
- [Test 33: azmcp_acr_registry_repository_list](#test-33)
- [Test 34: azmcp_acr_registry_repository_list](#test-34)
- [Test 35: azmcp_acr_registry_repository_list](#test-35)
- [Test 36: azmcp_acr_registry_repository_list](#test-36)
- [Test 37: azmcp_cosmos_account_list](#test-37)
- [Test 38: azmcp_cosmos_account_list](#test-38)
- [Test 39: azmcp_cosmos_account_list](#test-39)
- [Test 40: azmcp_cosmos_database_container_item_query](#test-40)
- [Test 41: azmcp_cosmos_database_container_list](#test-41)
- [Test 42: azmcp_cosmos_database_container_list](#test-42)
- [Test 43: azmcp_cosmos_database_list](#test-43)
- [Test 44: azmcp_cosmos_database_list](#test-44)
- [Test 45: azmcp_kusto_cluster_get](#test-45)
- [Test 46: azmcp_kusto_cluster_list](#test-46)
- [Test 47: azmcp_kusto_cluster_list](#test-47)
- [Test 48: azmcp_kusto_cluster_list](#test-48)
- [Test 49: azmcp_kusto_database_list](#test-49)
- [Test 50: azmcp_kusto_database_list](#test-50)
- [Test 51: azmcp_kusto_query](#test-51)
- [Test 52: azmcp_kusto_sample](#test-52)
- [Test 53: azmcp_kusto_table_list](#test-53)
- [Test 54: azmcp_kusto_table_list](#test-54)
- [Test 55: azmcp_kusto_table_schema](#test-55)
- [Test 56: azmcp_mysql_database_list](#test-56)
- [Test 57: azmcp_mysql_database_list](#test-57)
- [Test 58: azmcp_mysql_database_query](#test-58)
- [Test 59: azmcp_mysql_server_config_get](#test-59)
- [Test 60: azmcp_mysql_server_list](#test-60)
- [Test 61: azmcp_mysql_server_list](#test-61)
- [Test 62: azmcp_mysql_server_list](#test-62)
- [Test 63: azmcp_mysql_server_param_get](#test-63)
- [Test 64: azmcp_mysql_server_param_set](#test-64)
- [Test 65: azmcp_mysql_table_list](#test-65)
- [Test 66: azmcp_mysql_table_list](#test-66)
- [Test 67: azmcp_mysql_table_schema_get](#test-67)
- [Test 68: azmcp_postgres_database_list](#test-68)
- [Test 69: azmcp_postgres_database_list](#test-69)
- [Test 70: azmcp_postgres_database_query](#test-70)
- [Test 71: azmcp_postgres_server_config_get](#test-71)
- [Test 72: azmcp_postgres_server_list](#test-72)
- [Test 73: azmcp_postgres_server_list](#test-73)
- [Test 74: azmcp_postgres_server_list](#test-74)
- [Test 75: azmcp_postgres_server_param](#test-75)
- [Test 76: azmcp_postgres_server_param_set](#test-76)
- [Test 77: azmcp_postgres_table_list](#test-77)
- [Test 78: azmcp_postgres_table_list](#test-78)
- [Test 79: azmcp_postgres_table_schema_get](#test-79)
- [Test 80: azmcp_extension_azd](#test-80)
- [Test 81: azmcp_extension_azd](#test-81)
- [Test 82: azmcp_deploy_app_logs_get](#test-82)
- [Test 83: azmcp_deploy_architecture_diagram_generate](#test-83)
- [Test 84: azmcp_deploy_iac_rules_get](#test-84)
- [Test 85: azmcp_deploy_pipeline_guidance_get](#test-85)
- [Test 86: azmcp_deploy_plan_get](#test-86)
- [Test 87: azmcp_functionapp_get](#test-87)
- [Test 88: azmcp_functionapp_get](#test-88)
- [Test 89: azmcp_functionapp_get](#test-89)
- [Test 90: azmcp_functionapp_get](#test-90)
- [Test 91: azmcp_functionapp_get](#test-91)
- [Test 92: azmcp_functionapp_get](#test-92)
- [Test 93: azmcp_functionapp_get](#test-93)
- [Test 94: azmcp_functionapp_get](#test-94)
- [Test 95: azmcp_functionapp_get](#test-95)
- [Test 96: azmcp_functionapp_list](#test-96)
- [Test 97: azmcp_functionapp_list](#test-97)
- [Test 98: azmcp_functionapp_list](#test-98)
- [Test 99: azmcp_keyvault_certificate_create](#test-99)
- [Test 100: azmcp_keyvault_certificate_get](#test-100)
- [Test 101: azmcp_keyvault_certificate_get](#test-101)
- [Test 102: azmcp_keyvault_certificate_import](#test-102)
- [Test 103: azmcp_keyvault_certificate_import](#test-103)
- [Test 104: azmcp_keyvault_certificate_list](#test-104)
- [Test 105: azmcp_keyvault_certificate_list](#test-105)
- [Test 106: azmcp_keyvault_key_create](#test-106)
- [Test 107: azmcp_keyvault_key_list](#test-107)
- [Test 108: azmcp_keyvault_key_list](#test-108)
- [Test 109: azmcp_keyvault_secret_create](#test-109)
- [Test 110: azmcp_keyvault_secret_list](#test-110)
- [Test 111: azmcp_keyvault_secret_list](#test-111)
- [Test 112: azmcp_aks_cluster_get](#test-112)
- [Test 113: azmcp_aks_cluster_get](#test-113)
- [Test 114: azmcp_aks_cluster_get](#test-114)
- [Test 115: azmcp_aks_cluster_get](#test-115)
- [Test 116: azmcp_aks_cluster_list](#test-116)
- [Test 117: azmcp_aks_cluster_list](#test-117)
- [Test 118: azmcp_aks_cluster_list](#test-118)
- [Test 119: azmcp_loadtesting_test_create](#test-119)
- [Test 120: azmcp_loadtesting_test_get](#test-120)
- [Test 121: azmcp_loadtesting_testresource_create](#test-121)
- [Test 122: azmcp_loadtesting_testresource_list](#test-122)
- [Test 123: azmcp_loadtesting_testrun_create](#test-123)
- [Test 124: azmcp_loadtesting_testrun_get](#test-124)
- [Test 125: azmcp_loadtesting_testrun_list](#test-125)
- [Test 126: azmcp_loadtesting_testrun_update](#test-126)
- [Test 127: azmcp_grafana_list](#test-127)
- [Test 128: azmcp_azuremanagedlustre_filesystem_list](#test-128)
- [Test 129: azmcp_azuremanagedlustre_filesystem_list](#test-129)
- [Test 130: azmcp_azuremanagedlustre_filesystem_required-subnet-size](#test-130)
- [Test 131: azmcp_marketplace_product_get](#test-131)
- [Test 132: azmcp_bestpractices_get](#test-132)
- [Test 133: azmcp_bestpractices_get](#test-133)
- [Test 134: azmcp_bestpractices_get](#test-134)
- [Test 135: azmcp_bestpractices_get](#test-135)
- [Test 136: azmcp_bestpractices_get](#test-136)
- [Test 137: azmcp_bestpractices_get](#test-137)
- [Test 138: azmcp_bestpractices_get](#test-138)
- [Test 139: azmcp_bestpractices_get](#test-139)
- [Test 140: azmcp_bestpractices_get](#test-140)
- [Test 141: azmcp_bestpractices_get](#test-141)
- [Test 142: azmcp_monitor_healthmodels_entity_gethealth](#test-142)
- [Test 143: azmcp_monitor_metrics_definitions](#test-143)
- [Test 144: azmcp_monitor_metrics_definitions](#test-144)
- [Test 145: azmcp_monitor_metrics_definitions](#test-145)
- [Test 146: azmcp_monitor_metrics_query](#test-146)
- [Test 147: azmcp_monitor_metrics_query](#test-147)
- [Test 148: azmcp_monitor_metrics_query](#test-148)
- [Test 149: azmcp_monitor_metrics_query](#test-149)
- [Test 150: azmcp_monitor_metrics_query](#test-150)
- [Test 151: azmcp_monitor_metrics_query](#test-151)
- [Test 152: azmcp_monitor_resource_log_query](#test-152)
- [Test 153: azmcp_monitor_table_list](#test-153)
- [Test 154: azmcp_monitor_table_list](#test-154)
- [Test 155: azmcp_monitor_table_type_list](#test-155)
- [Test 156: azmcp_monitor_table_type_list](#test-156)
- [Test 157: azmcp_monitor_workspace_list](#test-157)
- [Test 158: azmcp_monitor_workspace_list](#test-158)
- [Test 159: azmcp_monitor_workspace_list](#test-159)
- [Test 160: azmcp_monitor_workspace_log_query](#test-160)
- [Test 161: azmcp_datadog_monitoredresources_list](#test-161)
- [Test 162: azmcp_datadog_monitoredresources_list](#test-162)
- [Test 163: azmcp_extension_azqr](#test-163)
- [Test 164: azmcp_extension_azqr](#test-164)
- [Test 165: azmcp_extension_azqr](#test-165)
- [Test 166: azmcp_quota_region_availability_list](#test-166)
- [Test 167: azmcp_quota_usage_check](#test-167)
- [Test 168: azmcp_role_assignment_list](#test-168)
- [Test 169: azmcp_role_assignment_list](#test-169)
- [Test 170: azmcp_redis_cache_accesspolicy_list](#test-170)
- [Test 171: azmcp_redis_cache_accesspolicy_list](#test-171)
- [Test 172: azmcp_redis_cache_list](#test-172)
- [Test 173: azmcp_redis_cache_list](#test-173)
- [Test 174: azmcp_redis_cache_list](#test-174)
- [Test 175: azmcp_redis_cluster_database_list](#test-175)
- [Test 176: azmcp_redis_cluster_database_list](#test-176)
- [Test 177: azmcp_redis_cluster_list](#test-177)
- [Test 178: azmcp_redis_cluster_list](#test-178)
- [Test 179: azmcp_redis_cluster_list](#test-179)
- [Test 180: azmcp_group_list](#test-180)
- [Test 181: azmcp_group_list](#test-181)
- [Test 182: azmcp_group_list](#test-182)
- [Test 183: azmcp_resourcehealth_availability-status_get](#test-183)
- [Test 184: azmcp_resourcehealth_availability-status_get](#test-184)
- [Test 185: azmcp_resourcehealth_availability-status_get](#test-185)
- [Test 186: azmcp_resourcehealth_availability-status_list](#test-186)
- [Test 187: azmcp_resourcehealth_availability-status_list](#test-187)
- [Test 188: azmcp_resourcehealth_availability-status_list](#test-188)
- [Test 189: azmcp_servicebus_queue_details](#test-189)
- [Test 190: azmcp_servicebus_topic_details](#test-190)
- [Test 191: azmcp_servicebus_topic_subscription_details](#test-191)
- [Test 192: azmcp_sql_db_list](#test-192)
- [Test 193: azmcp_sql_db_list](#test-193)
- [Test 194: azmcp_sql_db_show](#test-194)
- [Test 195: azmcp_sql_db_show](#test-195)
- [Test 196: azmcp_sql_elastic-pool_list](#test-196)
- [Test 197: azmcp_sql_elastic-pool_list](#test-197)
- [Test 198: azmcp_sql_elastic-pool_list](#test-198)
- [Test 199: azmcp_sql_server_entra-admin_list](#test-199)
- [Test 200: azmcp_sql_server_entra-admin_list](#test-200)
- [Test 201: azmcp_sql_server_entra-admin_list](#test-201)
- [Test 202: azmcp_sql_server_firewall-rule_list](#test-202)
- [Test 203: azmcp_sql_server_firewall-rule_list](#test-203)
- [Test 204: azmcp_sql_server_firewall-rule_list](#test-204)
- [Test 205: azmcp_storage_account_create](#test-205)
- [Test 206: azmcp_storage_account_create](#test-206)
- [Test 207: azmcp_storage_account_create](#test-207)
- [Test 208: azmcp_storage_account_details](#test-208)
- [Test 209: azmcp_storage_account_details](#test-209)
- [Test 210: azmcp_storage_account_list](#test-210)
- [Test 211: azmcp_storage_account_list](#test-211)
- [Test 212: azmcp_storage_account_list](#test-212)
- [Test 213: azmcp_storage_blob_batch_set-tier](#test-213)
- [Test 214: azmcp_storage_blob_batch_set-tier](#test-214)
- [Test 215: azmcp_storage_blob_container_create](#test-215)
- [Test 216: azmcp_storage_blob_container_create](#test-216)
- [Test 217: azmcp_storage_blob_container_create](#test-217)
- [Test 218: azmcp_storage_blob_container_details](#test-218)
- [Test 219: azmcp_storage_blob_container_list](#test-219)
- [Test 220: azmcp_storage_blob_container_list](#test-220)
- [Test 221: azmcp_storage_blob_details](#test-221)
- [Test 222: azmcp_storage_blob_details](#test-222)
- [Test 223: azmcp_storage_blob_list](#test-223)
- [Test 224: azmcp_storage_blob_list](#test-224)
- [Test 225: azmcp_storage_blob_upload](#test-225)
- [Test 226: azmcp_storage_blob_upload](#test-226)
- [Test 227: azmcp_storage_blob_upload](#test-227)
- [Test 228: azmcp_storage_datalake_directory_create](#test-228)
- [Test 229: azmcp_storage_datalake_file-system_list-paths](#test-229)
- [Test 230: azmcp_storage_datalake_file-system_list-paths](#test-230)
- [Test 231: azmcp_storage_datalake_file-system_list-paths](#test-231)
- [Test 232: azmcp_storage_queue_message_send](#test-232)
- [Test 233: azmcp_storage_queue_message_send](#test-233)
- [Test 234: azmcp_storage_queue_message_send](#test-234)
- [Test 235: azmcp_storage_share_file_list](#test-235)
- [Test 236: azmcp_storage_share_file_list](#test-236)
- [Test 237: azmcp_storage_share_file_list](#test-237)
- [Test 238: azmcp_storage_table_list](#test-238)
- [Test 239: azmcp_storage_table_list](#test-239)
- [Test 240: azmcp_subscription_list](#test-240)
- [Test 241: azmcp_subscription_list](#test-241)
- [Test 242: azmcp_subscription_list](#test-242)
- [Test 243: azmcp_subscription_list](#test-243)
- [Test 244: azmcp_azureterraformbestpractices_get](#test-244)
- [Test 245: azmcp_azureterraformbestpractices_get](#test-245)
- [Test 246: azmcp_virtualdesktop_hostpool_list](#test-246)
- [Test 247: azmcp_virtualdesktop_hostpool_sessionhost_list](#test-247)
- [Test 248: azmcp_virtualdesktop_hostpool_sessionhost_usersession-list](#test-248)
- [Test 249: azmcp_workbooks_create](#test-249)
- [Test 250: azmcp_workbooks_delete](#test-250)
- [Test 251: azmcp_workbooks_list](#test-251)
- [Test 252: azmcp_workbooks_list](#test-252)
- [Test 253: azmcp_workbooks_show](#test-253)
- [Test 254: azmcp_workbooks_show](#test-254)
- [Test 255: azmcp_workbooks_update](#test-255)
- [Test 256: azmcp_bicepschema_get](#test-256)
- [Test 257: azmcp_cloudarchitect_design](#test-257)
- [Test 258: azmcp_cloudarchitect_design](#test-258)
- [Test 259: azmcp_cloudarchitect_design](#test-259)
- [Test 260: azmcp_cloudarchitect_design](#test-260)

---

## Test 1

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695201 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.493943 | `azmcp_search_index_list` | ❌ |
| 3 | 0.433117 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.382476 | `azmcp_search_service_list` | ❌ |
| 5 | 0.345294 | `azmcp_search_index_describe` | ❌ |
| 6 | 0.329682 | `azmcp_foundry_models_deploy` | ❌ |
| 7 | 0.310401 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.309681 | `azmcp_monitor_table_list` | ❌ |
| 9 | 0.296877 | `azmcp_grafana_list` | ❌ |
| 10 | 0.291635 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.286620 | `azmcp_search_index_query` | ❌ |
| 12 | 0.286074 | `azmcp_monitor_table_type_list` | ❌ |
| 13 | 0.279802 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.270212 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.270162 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.267906 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.265579 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.265213 | `azmcp_loadtesting_testrun_list` | ❌ |
| 19 | 0.264056 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.261138 | `azmcp_kusto_database_list` | ❌ |

---

## Test 2

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603396 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.417630 | `azmcp_search_index_list` | ❌ |
| 3 | 0.396819 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.326253 | `azmcp_search_service_list` | ❌ |
| 5 | 0.324781 | `azmcp_search_index_describe` | ❌ |
| 6 | 0.317997 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.310576 | `azmcp_foundry_models_deploy` | ❌ |
| 8 | 0.278068 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.276839 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.272237 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.263706 | `azmcp_search_index_query` | ❌ |
| 12 | 0.260150 | `azmcp_bestpractices_get` | ❌ |
| 13 | 0.256208 | `azmcp_grafana_list` | ❌ |
| 14 | 0.249587 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.248887 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.232951 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.225181 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.224194 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.223705 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.218010 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 3

**Expected Tool:** `azmcp_foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.313400 | `azmcp_foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.282364 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.274011 | `azmcp_deploy_plan_get` | ❌ |
| 4 | 0.269513 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.268967 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.234071 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.222504 | `azmcp_grafana_list` | ❌ |
| 8 | 0.222478 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.221635 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.215293 | `azmcp_loadtesting_testrun_create` | ❌ |
| 11 | 0.215164 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.210645 | `azmcp_functionapp_get` | ❌ |
| 13 | 0.209865 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.208124 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.207601 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.204420 | `azmcp_postgres_server_param_set` | ❌ |
| 17 | 0.200541 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 18 | 0.195615 | `azmcp_workbooks_list` | ❌ |
| 19 | 0.190106 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.189255 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 4

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559459 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.549636 | `azmcp_foundry_models_list` | ❌ |
| 3 | 0.533239 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.434472 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.404693 | `azmcp_search_service_list` | ❌ |
| 6 | 0.387176 | `azmcp_search_index_list` | ❌ |
| 7 | 0.368173 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.334867 | `azmcp_grafana_list` | ❌ |
| 9 | 0.331872 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.320998 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.318854 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.312247 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.310280 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.306178 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.302262 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.301302 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.289448 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.288248 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.285916 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.284901 | `azmcp_monitor_table_list` | ❌ |

---

## Test 5

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518221 | `azmcp_foundry_models_list` | ❌ |
| 2 | 0.503424 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.488823 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 4 | 0.396422 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.360908 | `azmcp_search_service_list` | ❌ |
| 6 | 0.337032 | `azmcp_search_index_list` | ❌ |
| 7 | 0.328814 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.305997 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.301514 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.286814 | `azmcp_grafana_list` | ❌ |
| 11 | 0.282504 | `azmcp_cloudarchitect_design` | ❌ |
| 12 | 0.275928 | `azmcp_bestpractices_get` | ❌ |
| 13 | 0.269806 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.254926 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.250392 | `azmcp_redis_cluster_list` | ❌ |
| 16 | 0.246893 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.243133 | `azmcp_monitor_table_type_list` | ❌ |
| 18 | 0.238612 | `azmcp_search_index_describe` | ❌ |
| 19 | 0.236572 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.234075 | `azmcp_redis_cache_list` | ❌ |

---

## Test 6

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560022 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.401146 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.387861 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.355031 | `azmcp_search_service_list` | ❌ |
| 5 | 0.346852 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.337429 | `azmcp_search_index_list` | ❌ |
| 7 | 0.298648 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.285437 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.277883 | `azmcp_grafana_list` | ❌ |
| 10 | 0.273055 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.265730 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.255669 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.252297 | `azmcp_postgres_database_list` | ❌ |
| 14 | 0.248620 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.248405 | `azmcp_mysql_table_list` | ❌ |
| 16 | 0.247709 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.245193 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.245005 | `azmcp_loadtesting_testrun_list` | ❌ |
| 19 | 0.231110 | `azmcp_monitor_metrics_definitions` | ❌ |
| 20 | 0.229647 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 7

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574818 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.430513 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.388967 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.356809 | `azmcp_foundry_models_deployments_list` | ❌ |
| 5 | 0.309590 | `azmcp_search_service_list` | ❌ |
| 6 | 0.287991 | `azmcp_search_index_list` | ❌ |
| 7 | 0.274019 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.266937 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.247745 | `azmcp_bestpractices_get` | ❌ |
| 10 | 0.245943 | `azmcp_quota_region_availability_list` | ❌ |
| 11 | 0.244697 | `azmcp_monitor_table_type_list` | ❌ |
| 12 | 0.243617 | `azmcp_deploy_plan_get` | ❌ |
| 13 | 0.240256 | `azmcp_monitor_metrics_definitions` | ❌ |
| 14 | 0.237407 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 15 | 0.233924 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.212188 | `azmcp_search_index_describe` | ❌ |
| 17 | 0.211456 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.205424 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.203036 | `azmcp_search_index_query` | ❌ |
| 20 | 0.200059 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 8

**Expected Tool:** `azmcp_search_index_describe`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618178 | `azmcp_search_index_list` | ❌ |
| 2 | 0.597678 | `azmcp_search_index_describe` | ✅ **EXPECTED** |
| 3 | 0.466005 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.465274 | `azmcp_search_index_query` | ❌ |
| 5 | 0.436730 | `azmcp_search_service_list` | ❌ |
| 6 | 0.393556 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.389306 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.370915 | `azmcp_mysql_table_schema_get` | ❌ |
| 9 | 0.358315 | `azmcp_kusto_cluster_get` | ❌ |
| 10 | 0.356252 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.343852 | `azmcp_storage_blob_container_details` | ❌ |
| 12 | 0.338129 | `azmcp_storage_account_details` | ❌ |
| 13 | 0.336903 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.333641 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.330038 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.327096 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.325015 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.320820 | `azmcp_foundry_models_deployments_list` | ❌ |
| 19 | 0.317639 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.316039 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 9

**Expected Tool:** `azmcp_search_index_list`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.796644 | `azmcp_search_index_list` | ✅ **EXPECTED** |
| 2 | 0.561856 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.561102 | `azmcp_search_service_list` | ❌ |
| 4 | 0.518943 | `azmcp_search_index_describe` | ❌ |
| 5 | 0.455689 | `azmcp_search_index_query` | ❌ |
| 6 | 0.439636 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.416404 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.409307 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.406485 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.397423 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.392400 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.382791 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.378750 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.378297 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.375344 | `azmcp_foundry_models_deployments_list` | ❌ |
| 16 | 0.371099 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.369526 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.368931 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.362619 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.361922 | `azmcp_foundry_models_list` | ❌ |

---

## Test 10

**Expected Tool:** `azmcp_search_index_list`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750053 | `azmcp_search_index_list` | ✅ **EXPECTED** |
| 2 | 0.561168 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.512511 | `azmcp_search_index_describe` | ❌ |
| 4 | 0.497661 | `azmcp_search_service_list` | ❌ |
| 5 | 0.443796 | `azmcp_search_index_query` | ❌ |
| 6 | 0.401982 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.382733 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.372723 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.370378 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.367953 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.353901 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.351841 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.351216 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.350082 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.347629 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.341792 | `azmcp_foundry_models_list` | ❌ |
| 17 | 0.335800 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.332368 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.331285 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.330444 | `azmcp_kusto_table_list` | ❌ |

---

## Test 11

**Expected Tool:** `azmcp_search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552944 | `azmcp_search_index_list` | ❌ |
| 2 | 0.522558 | `azmcp_search_index_query` | ✅ **EXPECTED** |
| 3 | 0.492637 | `azmcp_search_index_describe` | ❌ |
| 4 | 0.463356 | `azmcp_search_service_list` | ❌ |
| 5 | 0.373917 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 6 | 0.327095 | `azmcp_kusto_query` | ❌ |
| 7 | 0.322009 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.311044 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.298106 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.290809 | `azmcp_monitor_metrics_query` | ❌ |
| 11 | 0.288226 | `azmcp_foundry_models_deployments_list` | ❌ |
| 12 | 0.287493 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.283532 | `azmcp_foundry_models_list` | ❌ |
| 14 | 0.270026 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.260161 | `azmcp_mysql_table_list` | ❌ |
| 16 | 0.254226 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.244844 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.243984 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.235536 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.232713 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 12

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.745450 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.608251 | `azmcp_search_index_list` | ❌ |
| 3 | 0.500455 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.494272 | `azmcp_monitor_workspace_list` | ❌ |
| 5 | 0.493011 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.492228 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.486066 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.482464 | `azmcp_grafana_list` | ❌ |
| 9 | 0.477471 | `azmcp_subscription_list` | ❌ |
| 10 | 0.470384 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.467656 | `azmcp_functionapp_list` | ❌ |
| 12 | 0.454400 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.451645 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.447601 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.427817 | `azmcp_group_list` | ❌ |
| 16 | 0.425463 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.418396 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.417472 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.414984 | `azmcp_foundry_models_list` | ❌ |
| 20 | 0.409175 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644213 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.525315 | `azmcp_search_index_list` | ❌ |
| 3 | 0.425939 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.412158 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.408456 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.400229 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.399822 | `azmcp_grafana_list` | ❌ |
| 8 | 0.397810 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.393708 | `azmcp_subscription_list` | ❌ |
| 10 | 0.390559 | `azmcp_foundry_models_list` | ❌ |
| 11 | 0.384559 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.382097 | `azmcp_functionapp_list` | ❌ |
| 13 | 0.379805 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 14 | 0.376962 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.376950 | `azmcp_search_index_describe` | ❌ |
| 16 | 0.376089 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.374692 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.363619 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.362366 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.360230 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 14

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488099 | `azmcp_search_index_list` | ❌ |
| 2 | 0.482308 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 3 | 0.351773 | `azmcp_search_index_describe` | ❌ |
| 4 | 0.344619 | `azmcp_foundry_models_deployments_list` | ❌ |
| 5 | 0.336174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.329777 | `azmcp_search_index_query` | ❌ |
| 7 | 0.322580 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 8 | 0.322540 | `azmcp_foundry_models_list` | ❌ |
| 9 | 0.292675 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.290214 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.288563 | `azmcp_bestpractices_get` | ❌ |
| 12 | 0.283366 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.281134 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.278574 | `azmcp_cloudarchitect_design` | ❌ |
| 15 | 0.278531 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.276660 | `azmcp_extension_az` | ❌ |
| 17 | 0.274081 | `azmcp_monitor_table_type_list` | ❌ |
| 18 | 0.272275 | `azmcp_monitor_table_list` | ❌ |
| 19 | 0.270642 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.266987 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 15

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.635561 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.492146 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.491380 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.473554 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.460609 | `azmcp_functionapp_list` | ❌ |
| 7 | 0.460242 | `azmcp_storage_account_list` | ❌ |
| 8 | 0.442214 | `azmcp_grafana_list` | ❌ |
| 9 | 0.441656 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.429305 | `azmcp_search_service_list` | ❌ |
| 11 | 0.427658 | `azmcp_subscription_list` | ❌ |
| 12 | 0.427460 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.420794 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.408599 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.404636 | `azmcp_storage_table_list` | ❌ |
| 16 | 0.399781 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.387414 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.387179 | `azmcp_appconfig_kv_delete` | ❌ |
| 19 | 0.385938 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.380818 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 16

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634978 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.533437 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.425610 | `azmcp_appconfig_kv_show` | ❌ |
| 4 | 0.372456 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.368731 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.368072 | `azmcp_functionapp_list` | ❌ |
| 7 | 0.359567 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.356514 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.354747 | `azmcp_appconfig_kv_delete` | ❌ |
| 10 | 0.348603 | `azmcp_appconfig_kv_set` | ❌ |
| 11 | 0.341263 | `azmcp_grafana_list` | ❌ |
| 12 | 0.332824 | `azmcp_mysql_server_config_get` | ❌ |
| 13 | 0.330809 | `azmcp_storage_account_list` | ❌ |
| 14 | 0.325885 | `azmcp_subscription_list` | ❌ |
| 15 | 0.321968 | `azmcp_appconfig_kv_unlock` | ❌ |
| 16 | 0.320605 | `azmcp_marketplace_product_get` | ❌ |
| 17 | 0.318639 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.317667 | `azmcp_search_service_list` | ❌ |
| 19 | 0.296589 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.292788 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 17

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565435 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.564705 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.414689 | `azmcp_appconfig_kv_show` | ❌ |
| 4 | 0.355916 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.348661 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.327234 | `azmcp_appconfig_kv_set` | ❌ |
| 7 | 0.308131 | `azmcp_appconfig_kv_unlock` | ❌ |
| 8 | 0.302405 | `azmcp_appconfig_kv_lock` | ❌ |
| 9 | 0.282153 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.255774 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.244080 | `azmcp_loadtesting_testrun_list` | ❌ |
| 12 | 0.241260 | `azmcp_storage_blob_container_details` | ❌ |
| 13 | 0.237881 | `azmcp_loadtesting_test_get` | ❌ |
| 14 | 0.236404 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.235204 | `azmcp_storage_account_details` | ❌ |
| 16 | 0.233357 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.231649 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.230137 | `azmcp_storage_blob_container_list` | ❌ |
| 19 | 0.228042 | `azmcp_mysql_server_param_set` | ❌ |
| 20 | 0.221405 | `azmcp_postgres_database_list` | ❌ |

---

## Test 18

**Expected Tool:** `azmcp_appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618277 | `azmcp_appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.486631 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.444881 | `azmcp_appconfig_kv_unlock` | ❌ |
| 4 | 0.443998 | `azmcp_appconfig_kv_lock` | ❌ |
| 5 | 0.424344 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.399569 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.392016 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.264810 | `azmcp_workbooks_delete` | ❌ |
| 9 | 0.262117 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.248752 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.240599 | `azmcp_keyvault_secret_create` | ❌ |
| 12 | 0.218487 | `azmcp_mysql_server_param_set` | ❌ |
| 13 | 0.194831 | `azmcp_postgres_server_config_get` | ❌ |
| 14 | 0.175403 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.173143 | `azmcp_postgres_server_param_set` | ❌ |
| 16 | 0.165140 | `azmcp_mysql_server_param_get` | ❌ |
| 17 | 0.155805 | `azmcp_storage_account_details` | ❌ |
| 18 | 0.141099 | `azmcp_postgres_server_param_get` | ❌ |
| 19 | 0.137771 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 20 | 0.135822 | `azmcp_redis_cache_list` | ❌ |

---

## Test 19

**Expected Tool:** `azmcp_appconfig_kv_list`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.730852 | `azmcp_appconfig_kv_list` | ✅ **EXPECTED** |
| 2 | 0.595054 | `azmcp_appconfig_kv_show` | ❌ |
| 3 | 0.557810 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.530884 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.482784 | `azmcp_appconfig_kv_unlock` | ❌ |
| 6 | 0.464635 | `azmcp_appconfig_kv_delete` | ❌ |
| 7 | 0.438315 | `azmcp_appconfig_kv_lock` | ❌ |
| 8 | 0.377534 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.374460 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.338195 | `azmcp_keyvault_secret_list` | ❌ |
| 11 | 0.333355 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.329461 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.317744 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.296908 | `azmcp_storage_account_details` | ❌ |
| 15 | 0.296084 | `azmcp_postgres_table_list` | ❌ |
| 16 | 0.292091 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.279679 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.276807 | `azmcp_storage_blob_container_list` | ❌ |
| 19 | 0.275469 | `azmcp_mysql_server_param_set` | ❌ |
| 20 | 0.267026 | `azmcp_postgres_database_list` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_appconfig_kv_list`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682275 | `azmcp_appconfig_kv_list` | ✅ **EXPECTED** |
| 2 | 0.606545 | `azmcp_appconfig_kv_show` | ❌ |
| 3 | 0.522426 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.512945 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.490384 | `azmcp_appconfig_kv_unlock` | ❌ |
| 6 | 0.468503 | `azmcp_appconfig_kv_delete` | ❌ |
| 7 | 0.458896 | `azmcp_appconfig_kv_lock` | ❌ |
| 8 | 0.370520 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.356793 | `azmcp_mysql_server_param_get` | ❌ |
| 10 | 0.317662 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.316879 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.296442 | `azmcp_storage_account_details` | ❌ |
| 13 | 0.294807 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.289896 | `azmcp_storage_blob_container_details` | ❌ |
| 15 | 0.288088 | `azmcp_mysql_server_param_set` | ❌ |
| 16 | 0.282379 | `azmcp_loadtesting_testrun_list` | ❌ |
| 17 | 0.258688 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.247736 | `azmcp_storage_blob_details` | ❌ |
| 19 | 0.243655 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.236389 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 21

**Expected Tool:** `azmcp_appconfig_kv_lock`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646614 | `azmcp_appconfig_kv_lock` | ✅ **EXPECTED** |
| 2 | 0.518065 | `azmcp_appconfig_kv_unlock` | ❌ |
| 3 | 0.508804 | `azmcp_appconfig_kv_list` | ❌ |
| 4 | 0.445551 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.431516 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.423650 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.373656 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.253705 | `azmcp_mysql_server_param_set` | ❌ |
| 9 | 0.251298 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.238611 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.238242 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.211331 | `azmcp_postgres_server_config_get` | ❌ |
| 13 | 0.208057 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.161398 | `azmcp_storage_blob_container_details` | ❌ |
| 15 | 0.158946 | `azmcp_mysql_server_param_get` | ❌ |
| 16 | 0.154529 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.150689 | `azmcp_storage_account_details` | ❌ |
| 18 | 0.149535 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.144377 | `azmcp_servicebus_queue_details` | ❌ |
| 20 | 0.141401 | `azmcp_storage_blob_batch_set-tier` | ❌ |

---

## Test 22

**Expected Tool:** `azmcp_appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609567 | `azmcp_appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.541867 | `azmcp_appconfig_kv_lock` | ❌ |
| 3 | 0.518461 | `azmcp_appconfig_kv_list` | ❌ |
| 4 | 0.511238 | `azmcp_appconfig_kv_unlock` | ❌ |
| 5 | 0.507090 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.505547 | `azmcp_appconfig_kv_delete` | ❌ |
| 7 | 0.377906 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.359969 | `azmcp_mysql_server_param_set` | ❌ |
| 9 | 0.346908 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.311453 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.305903 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.221149 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.213567 | `azmcp_mysql_server_param_get` | ❌ |
| 14 | 0.208990 | `azmcp_postgres_server_config_get` | ❌ |
| 15 | 0.182017 | `azmcp_storage_account_details` | ❌ |
| 16 | 0.171495 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 17 | 0.166929 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.164399 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.162793 | `azmcp_storage_account_create` | ❌ |
| 20 | 0.143895 | `azmcp_storage_queue_message_send` | ❌ |

---

## Test 23

**Expected Tool:** `azmcp_appconfig_kv_show`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603216 | `azmcp_appconfig_kv_list` | ❌ |
| 2 | 0.561508 | `azmcp_appconfig_kv_show` | ✅ **EXPECTED** |
| 3 | 0.448912 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.441713 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.437432 | `azmcp_appconfig_account_list` | ❌ |
| 6 | 0.433846 | `azmcp_appconfig_kv_lock` | ❌ |
| 7 | 0.427588 | `azmcp_appconfig_kv_unlock` | ❌ |
| 8 | 0.301859 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.291448 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.276985 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.260220 | `azmcp_keyvault_secret_list` | ❌ |
| 12 | 0.257940 | `azmcp_mysql_server_param_get` | ❌ |
| 13 | 0.239998 | `azmcp_storage_account_details` | ❌ |
| 14 | 0.239977 | `azmcp_storage_blob_container_details` | ❌ |
| 15 | 0.229242 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.217856 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.206401 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.205556 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.203816 | `azmcp_storage_blob_container_list` | ❌ |
| 20 | 0.201872 | `azmcp_mysql_server_param_set` | ❌ |

---

## Test 24

**Expected Tool:** `azmcp_appconfig_kv_unlock`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603597 | `azmcp_appconfig_kv_unlock` | ✅ **EXPECTED** |
| 2 | 0.552244 | `azmcp_appconfig_kv_lock` | ❌ |
| 3 | 0.541557 | `azmcp_appconfig_kv_list` | ❌ |
| 4 | 0.476496 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.435759 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.425488 | `azmcp_appconfig_kv_set` | ❌ |
| 7 | 0.409406 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.268062 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.259561 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.252923 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.237098 | `azmcp_mysql_server_param_set` | ❌ |
| 12 | 0.225350 | `azmcp_postgres_server_config_get` | ❌ |
| 13 | 0.185141 | `azmcp_postgres_server_param_set` | ❌ |
| 14 | 0.179797 | `azmcp_mysql_server_param_get` | ❌ |
| 15 | 0.175358 | `azmcp_storage_blob_container_details` | ❌ |
| 16 | 0.171375 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.169443 | `azmcp_storage_account_details` | ❌ |
| 18 | 0.159767 | `azmcp_postgres_server_param_get` | ❌ |
| 19 | 0.143564 | `azmcp_servicebus_queue_details` | ❌ |
| 20 | 0.135802 | `azmcp_storage_account_create` | ❌ |

---

## Test 25

**Expected Tool:** `azmcp_extension_az`  
**Prompt:** Create a Storage account with name <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593510 | `azmcp_storage_account_create` | ❌ |
| 2 | 0.467048 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.455521 | `azmcp_storage_account_details` | ❌ |
| 4 | 0.447463 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.429618 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.402922 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.386765 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.384937 | `azmcp_storage_blob_list` | ❌ |
| 9 | 0.374481 | `azmcp_keyvault_certificate_create` | ❌ |
| 10 | 0.352805 | `azmcp_appconfig_kv_set` | ❌ |
| 11 | 0.337708 | `azmcp_storage_datalake_directory_create` | ❌ |
| 12 | 0.334515 | `azmcp_storage_blob_container_details` | ❌ |
| 13 | 0.333770 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.329895 | `azmcp_loadtesting_testresource_create` | ❌ |
| 15 | 0.327875 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.325736 | `azmcp_loadtesting_test_create` | ❌ |
| 17 | 0.318516 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.311829 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.303766 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.303151 | `azmcp_appconfig_kv_lock` | ❌ |

---

## Test 26

**Expected Tool:** `azmcp_extension_az`  
**Prompt:** List all virtual machines in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577373 | `azmcp_search_service_list` | ❌ |
| 2 | 0.531767 | `azmcp_subscription_list` | ❌ |
| 3 | 0.530948 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.500615 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.499251 | `azmcp_kusto_cluster_list` | ❌ |
| 6 | 0.496186 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.491307 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 8 | 0.484074 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.482576 | `azmcp_grafana_list` | ❌ |
| 10 | 0.477567 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.473774 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.473437 | `azmcp_functionapp_list` | ❌ |
| 13 | 0.468411 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 14 | 0.456914 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.454007 | `azmcp_group_list` | ❌ |
| 16 | 0.435557 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.430029 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.411045 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.409699 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.407261 | `azmcp_acr_registry_list` | ❌ |

---

## Test 27

**Expected Tool:** `azmcp_extension_az`  
**Prompt:** Show me the details of the storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.632334 | `azmcp_storage_account_details` | ❌ |
| 2 | 0.545329 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.518748 | `azmcp_storage_blob_container_details` | ❌ |
| 4 | 0.516679 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.509806 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.473642 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.455133 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.433899 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.433255 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.417590 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.411378 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.379322 | `azmcp_functionapp_get` | ❌ |
| 13 | 0.377441 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.371852 | `azmcp_sql_db_show` | ❌ |
| 15 | 0.367600 | `azmcp_aks_cluster_get` | ❌ |
| 16 | 0.364783 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.360310 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.347005 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.337702 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.326852 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 28

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.532560 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.528620 | `azmcp_search_service_list` | ❌ |
| 5 | 0.527318 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.515937 | `azmcp_subscription_list` | ❌ |
| 7 | 0.514293 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.509386 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.505220 | `azmcp_storage_account_list` | ❌ |
| 10 | 0.503032 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.490776 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.483500 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.482499 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.482236 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.481761 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.480869 | `azmcp_group_list` | ❌ |
| 17 | 0.473475 | `azmcp_functionapp_list` | ❌ |
| 18 | 0.469958 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.466412 | `azmcp_storage_blob_list` | ❌ |
| 20 | 0.462353 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 29

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.452146 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.415552 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.382807 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.374549 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.372153 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.370858 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.366017 | `azmcp_storage_blob_container_details` | ❌ |
| 10 | 0.359177 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.357314 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.353379 | `azmcp_subscription_list` | ❌ |
| 13 | 0.349526 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.349291 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.344750 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.344071 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.339252 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.338380 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.336892 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.333732 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 30

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637130 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563476 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.474000 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471804 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.463742 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.463435 | `azmcp_search_service_list` | ❌ |
| 7 | 0.463004 | `azmcp_storage_blob_container_list` | ❌ |
| 8 | 0.452938 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.451253 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.443939 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.440464 | `azmcp_subscription_list` | ❌ |
| 12 | 0.435835 | `azmcp_grafana_list` | ❌ |
| 13 | 0.433942 | `azmcp_storage_account_list` | ❌ |
| 14 | 0.431745 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.430857 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.430308 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.409093 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.404664 | `azmcp_group_list` | ❌ |
| 19 | 0.398556 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.395721 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 31

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `azmcp_acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476138 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.458137 | `azmcp_storage_blob_container_list` | ❌ |
| 5 | 0.454929 | `azmcp_group_list` | ❌ |
| 6 | 0.454003 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.446008 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.428000 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.423541 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.421030 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.411316 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.409133 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.403791 | `azmcp_storage_blob_list` | ❌ |
| 14 | 0.388773 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.372510 | `azmcp_sql_elastic-pool_list` | ❌ |
| 16 | 0.370359 | `azmcp_redis_cluster_database_list` | ❌ |
| 17 | 0.356119 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.354145 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.352336 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.351839 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 32

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.468163 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.449649 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `azmcp_group_list` | ❌ |
| 6 | 0.445516 | `azmcp_storage_blob_container_list` | ❌ |
| 7 | 0.416353 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.413975 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.406554 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.400209 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.389603 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.385074 | `azmcp_storage_blob_list` | ❌ |
| 13 | 0.378353 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.373837 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.371881 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.369779 | `azmcp_mysql_database_list` | ❌ |
| 17 | 0.354807 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.351411 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.344148 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.343055 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 33

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.510435 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.495567 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.494112 | `azmcp_storage_blob_container_list` | ❌ |
| 6 | 0.492550 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.480676 | `azmcp_search_service_list` | ❌ |
| 8 | 0.475629 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.462702 | `azmcp_storage_account_list` | ❌ |
| 10 | 0.461777 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.461369 | `azmcp_grafana_list` | ❌ |
| 12 | 0.456838 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.449239 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.448228 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.440083 | `azmcp_subscription_list` | ❌ |
| 16 | 0.438043 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.437933 | `azmcp_storage_blob_list` | ❌ |
| 18 | 0.430939 | `azmcp_group_list` | ❌ |
| 19 | 0.423301 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.414463 | `azmcp_kusto_database_list` | ❌ |

---

## Test 34

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546333 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.441221 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.407973 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.378587 | `azmcp_storage_blob_list` | ❌ |
| 6 | 0.377692 | `azmcp_storage_blob_container_details` | ❌ |
| 7 | 0.339307 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.326684 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.308650 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.307942 | `azmcp_storage_blob_container_create` | ❌ |
| 11 | 0.302635 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.300174 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.294200 | `azmcp_storage_account_list` | ❌ |
| 14 | 0.293421 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.292155 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.290148 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.283716 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.283390 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.276498 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.273350 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 35

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.454071 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.433927 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.415194 | `azmcp_storage_blob_list` | ❌ |
| 6 | 0.370375 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.359617 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.356947 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.355328 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.351007 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.347437 | `azmcp_postgres_database_list` | ❌ |
| 12 | 0.347084 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.346850 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.341254 | `azmcp_storage_blob_container_details` | ❌ |
| 15 | 0.340014 | `azmcp_redis_cluster_list` | ❌ |
| 16 | 0.338404 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.337543 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.332856 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.332785 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.330046 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 36

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501842 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.428026 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.418623 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.375343 | `azmcp_storage_blob_list` | ❌ |
| 6 | 0.359922 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.341511 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.335519 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.333318 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.329818 | `azmcp_storage_blob_container_details` | ❌ |
| 11 | 0.324104 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.318706 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.316614 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.315414 | `azmcp_redis_cluster_database_list` | ❌ |
| 15 | 0.314960 | `azmcp_storage_table_list` | ❌ |
| 16 | 0.311692 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.304725 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.303931 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.300101 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.299629 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 37

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818391 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668485 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.615281 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.612482 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.588661 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.587691 | `azmcp_subscription_list` | ❌ |
| 7 | 0.557847 | `azmcp_search_service_list` | ❌ |
| 8 | 0.528996 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.524170 | `azmcp_storage_blob_container_list` | ❌ |
| 10 | 0.516899 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.514446 | `azmcp_functionapp_list` | ❌ |
| 12 | 0.502412 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.502139 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.499113 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.497653 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.487000 | `azmcp_group_list` | ❌ |
| 17 | 0.483033 | `azmcp_grafana_list` | ❌ |
| 18 | 0.474953 | `azmcp_postgres_server_list` | ❌ |
| 19 | 0.474276 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.459839 | `azmcp_storage_blob_list` | ❌ |

---

## Test 38

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665447 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605357 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.571613 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.467671 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.463920 | `azmcp_storage_blob_container_list` | ❌ |
| 6 | 0.450833 | `azmcp_storage_account_list` | ❌ |
| 7 | 0.443455 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.436283 | `azmcp_subscription_list` | ❌ |
| 9 | 0.431496 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.427709 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.397574 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.390141 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.389815 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.389414 | `azmcp_storage_blob_list` | ❌ |
| 15 | 0.386108 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.383985 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.381323 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.379496 | `azmcp_appconfig_kv_show` | ❌ |
| 19 | 0.358376 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.355795 | `azmcp_functionapp_list` | ❌ |

---

## Test 39

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605125 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.566249 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.553900 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.546327 | `azmcp_subscription_list` | ❌ |
| 6 | 0.535227 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.513709 | `azmcp_search_service_list` | ❌ |
| 8 | 0.488006 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.477268 | `azmcp_storage_blob_container_list` | ❌ |
| 10 | 0.466414 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.461998 | `azmcp_functionapp_list` | ❌ |
| 12 | 0.457584 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.456219 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.455017 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.453626 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.443558 | `azmcp_storage_account_details` | ❌ |
| 17 | 0.441136 | `azmcp_grafana_list` | ❌ |
| 18 | 0.438277 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.437026 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.434623 | `azmcp_mysql_database_list` | ❌ |

---

## Test 40

**Expected Tool:** `azmcp_cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.605253 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.566854 | `azmcp_cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 3 | 0.477874 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.453776 | `azmcp_storage_blob_container_list` | ❌ |
| 5 | 0.447757 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.398979 | `azmcp_search_service_list` | ❌ |
| 7 | 0.396946 | `azmcp_storage_blob_list` | ❌ |
| 8 | 0.386227 | `azmcp_search_index_list` | ❌ |
| 9 | 0.384335 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.378151 | `azmcp_kusto_query` | ❌ |
| 11 | 0.374844 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.372689 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.358879 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.351331 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.345303 | `azmcp_storage_blob_container_details` | ❌ |
| 16 | 0.341090 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.334389 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.331041 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.308694 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.302962 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 41

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852832 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681044 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.674778 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.630659 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.545064 | `azmcp_storage_blob_list` | ❌ |
| 6 | 0.535260 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.527459 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 8 | 0.486357 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.448957 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.447539 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.447491 | `azmcp_storage_account_list` | ❌ |
| 12 | 0.439770 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.427614 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.424294 | `azmcp_redis_cluster_database_list` | ❌ |
| 15 | 0.422151 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.421534 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.411757 | `azmcp_monitor_table_list` | ❌ |
| 18 | 0.378191 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.372115 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.368473 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 42

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789395 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.614220 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.592393 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.562062 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.521532 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 6 | 0.471019 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.455106 | `azmcp_storage_blob_list` | ❌ |
| 8 | 0.449120 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.411703 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.398064 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.397755 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.395513 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.394078 | `azmcp_storage_account_details` | ❌ |
| 14 | 0.392764 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.386806 | `azmcp_redis_cluster_database_list` | ❌ |
| 16 | 0.368313 | `azmcp_storage_account_list` | ❌ |
| 17 | 0.355640 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.325994 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.319603 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.318540 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 43

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668515 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.665298 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.573704 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.571319 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.555134 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.548066 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.526046 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.501477 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.491347 | `azmcp_storage_blob_container_list` | ❌ |
| 11 | 0.471453 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.460736 | `azmcp_storage_account_list` | ❌ |
| 13 | 0.459194 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.450955 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.442540 | `azmcp_mysql_table_list` | ❌ |
| 16 | 0.427443 | `azmcp_storage_blob_list` | ❌ |
| 17 | 0.405825 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.397642 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.389032 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.387534 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 44

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.614572 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.538479 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.524837 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.505363 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.498206 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.497414 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.449759 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.447875 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.445891 | `azmcp_storage_blob_container_list` | ❌ |
| 12 | 0.437993 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.408605 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.404251 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.396342 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.378945 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.348999 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.344442 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.342424 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.339516 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 45

**Expected Tool:** `azmcp_kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482148 | `azmcp_kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.464523 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.457669 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.416762 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.364174 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.364126 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.344871 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.344590 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.333244 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.332639 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.331424 | `azmcp_storage_blob_container_details` | ❌ |
| 12 | 0.326472 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.318754 | `azmcp_kusto_query` | ❌ |
| 14 | 0.318082 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.314617 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.304275 | `azmcp_mysql_database_query` | ❌ |
| 17 | 0.301024 | `azmcp_grafana_list` | ❌ |
| 18 | 0.300008 | `azmcp_kusto_table_list` | ❌ |
| 19 | 0.291566 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.289673 | `azmcp_storage_account_details` | ❌ |

---

## Test 46

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.651218 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.644037 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.549093 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.535622 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.509396 | `azmcp_grafana_list` | ❌ |
| 6 | 0.505912 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.492107 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.487882 | `azmcp_search_service_list` | ❌ |
| 9 | 0.487583 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.486159 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.460255 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.458754 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.451500 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.428236 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.427759 | `azmcp_subscription_list` | ❌ |
| 16 | 0.411791 | `azmcp_group_list` | ❌ |
| 17 | 0.407832 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.404929 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.404569 | `azmcp_storage_account_list` | ❌ |
| 20 | 0.395458 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 47

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.437363 | `azmcp_redis_cluster_list` | ❌ |
| 2 | 0.391087 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.386126 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 4 | 0.359551 | `azmcp_kusto_database_list` | ❌ |
| 5 | 0.341784 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.338709 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.314734 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.303083 | `azmcp_grafana_list` | ❌ |
| 9 | 0.292838 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.287768 | `azmcp_kusto_sample` | ❌ |
| 11 | 0.285603 | `azmcp_kusto_query` | ❌ |
| 12 | 0.283331 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.279848 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.277014 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.275559 | `azmcp_mysql_database_query` | ❌ |
| 16 | 0.270915 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.265903 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.264112 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.264035 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.263226 | `azmcp_quota_usage_check` | ❌ |

---

## Test 48

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584053 | `azmcp_redis_cluster_list` | ❌ |
| 2 | 0.549797 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 3 | 0.471078 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.469570 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.464294 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.462945 | `azmcp_grafana_list` | ❌ |
| 7 | 0.446124 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.440326 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.432048 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.421307 | `azmcp_search_service_list` | ❌ |
| 11 | 0.396253 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.392541 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.386776 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.380006 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.377490 | `azmcp_kusto_query` | ❌ |
| 16 | 0.371088 | `azmcp_subscription_list` | ❌ |
| 17 | 0.368890 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.366262 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.365972 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.365323 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 49

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628129 | `azmcp_redis_cluster_database_list` | ❌ |
| 2 | 0.610646 | `azmcp_kusto_database_list` | ✅ **EXPECTED** |
| 3 | 0.553218 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.549673 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.517039 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.474354 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.461496 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.459180 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.434330 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.431669 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.419528 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.404115 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.396060 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.379966 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.375535 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.363663 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.349647 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.334270 | `azmcp_grafana_list` | ❌ |
| 19 | 0.320622 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.318850 | `azmcp_kusto_query` | ❌ |

---

## Test 50

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.597975 | `azmcp_redis_cluster_database_list` | ❌ |
| 2 | 0.558503 | `azmcp_kusto_database_list` | ✅ **EXPECTED** |
| 3 | 0.497144 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.491400 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.486732 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.440064 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.427251 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.422588 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.391411 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.383664 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.368013 | `azmcp_postgres_table_list` | ❌ |
| 12 | 0.362905 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.359378 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.343985 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.338777 | `azmcp_storage_table_list` | ❌ |
| 16 | 0.336104 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.334803 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.310700 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.309809 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.305756 | `azmcp_kusto_query` | ❌ |

---

## Test 51

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.381352 | `azmcp_kusto_query` | ✅ **EXPECTED** |
| 2 | 0.363594 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.363252 | `azmcp_kusto_sample` | ❌ |
| 4 | 0.349193 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.345799 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.334762 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.328163 | `azmcp_mysql_database_query` | ❌ |
| 8 | 0.324763 | `azmcp_mysql_table_schema_get` | ❌ |
| 9 | 0.319112 | `azmcp_redis_cluster_database_list` | ❌ |
| 10 | 0.318883 | `azmcp_kusto_table_schema` | ❌ |
| 11 | 0.314961 | `azmcp_monitor_table_type_list` | ❌ |
| 12 | 0.308113 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.304014 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.302894 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.300427 | `azmcp_search_service_list` | ❌ |
| 16 | 0.300418 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.292087 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.264026 | `azmcp_grafana_list` | ❌ |
| 19 | 0.263085 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.257192 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 52

**Expected Tool:** `azmcp_kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537154 | `azmcp_kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.419463 | `azmcp_kusto_table_schema` | ❌ |
| 3 | 0.391595 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.391423 | `azmcp_kusto_table_list` | ❌ |
| 5 | 0.380691 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.377056 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.364611 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.364361 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.361845 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.343671 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.341678 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.337281 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.329933 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.319239 | `azmcp_kusto_query` | ❌ |
| 15 | 0.318189 | `azmcp_postgres_table_list` | ❌ |
| 16 | 0.310196 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.285941 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.267689 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.250074 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.242112 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 53

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591668 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585237 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.556724 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.550069 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.521516 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.520802 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.517077 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.475496 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.464341 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.449656 | `azmcp_kusto_table_schema` | ❌ |
| 11 | 0.436518 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.433775 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.429278 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.412275 | `azmcp_kusto_sample` | ❌ |
| 15 | 0.410425 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.400099 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.380671 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.337427 | `azmcp_kusto_query` | ❌ |
| 19 | 0.330100 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.329669 | `azmcp_grafana_list` | ❌ |

---

## Test 54

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549885 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.524691 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.523432 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.494108 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.490742 | `azmcp_monitor_table_list` | ❌ |
| 6 | 0.475412 | `azmcp_kusto_database_list` | ❌ |
| 7 | 0.466302 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.466212 | `azmcp_kusto_table_schema` | ❌ |
| 9 | 0.431964 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.425623 | `azmcp_kusto_sample` | ❌ |
| 11 | 0.421413 | `azmcp_postgres_database_list` | ❌ |
| 12 | 0.418153 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.415682 | `azmcp_mysql_database_list` | ❌ |
| 14 | 0.403445 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.391081 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.367187 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.348891 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.330383 | `azmcp_kusto_query` | ❌ |
| 19 | 0.314766 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.300347 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 55

**Expected Tool:** `azmcp_kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588151 | `azmcp_kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564311 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.527917 | `azmcp_mysql_table_schema_get` | ❌ |
| 4 | 0.445190 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.437466 | `azmcp_kusto_table_list` | ❌ |
| 6 | 0.432585 | `azmcp_kusto_sample` | ❌ |
| 7 | 0.413770 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.398632 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.387660 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.366346 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.366081 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.358088 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.357533 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.345263 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.314580 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.298243 | `azmcp_kusto_query` | ❌ |
| 17 | 0.294840 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.282712 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.275795 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.273998 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 56

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634056 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.623421 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534457 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.498903 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.490148 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.472745 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.462034 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.453687 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.430335 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.428203 | `azmcp_mysql_database_query` | ❌ |
| 11 | 0.421794 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.406803 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.338476 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.327614 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.317875 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.284786 | `azmcp_grafana_list` | ❌ |
| 17 | 0.278428 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.270842 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.268856 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.266185 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 57

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588122 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.574089 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.483855 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.463241 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.448169 | `azmcp_redis_cluster_database_list` | ❌ |
| 6 | 0.444547 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.415119 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.405492 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.404871 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.384974 | `azmcp_postgres_table_list` | ❌ |
| 11 | 0.384778 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.380422 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.297709 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.290592 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.259334 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.247558 | `azmcp_grafana_list` | ❌ |
| 17 | 0.239544 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.236450 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.236206 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.235967 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 58

**Expected Tool:** `azmcp_mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478118 | `azmcp_mysql_table_list` | ❌ |
| 2 | 0.457382 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.434733 | `azmcp_mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.421667 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.411614 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.395366 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.345922 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.330483 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.321017 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.300160 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.291514 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.286469 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.279117 | `azmcp_kusto_query` | ❌ |
| 14 | 0.278860 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.264423 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.257524 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.230263 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.226807 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.225229 | `azmcp_grafana_list` | ❌ |
| 20 | 0.198900 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 59

**Expected Tool:** `azmcp_mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531887 | `azmcp_postgres_server_config_get` | ❌ |
| 2 | 0.489816 | `azmcp_mysql_server_config_get` | ✅ **EXPECTED** |
| 3 | 0.485952 | `azmcp_mysql_server_param_set` | ❌ |
| 4 | 0.476863 | `azmcp_mysql_server_param_get` | ❌ |
| 5 | 0.426507 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.413270 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.391644 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.376750 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.374870 | `azmcp_postgres_server_param_get` | ❌ |
| 10 | 0.356337 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.267903 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.251260 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.238583 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.233245 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.224212 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.198877 | `azmcp_aks_cluster_get` | ❌ |
| 17 | 0.185106 | `azmcp_appconfig_kv_unlock` | ❌ |
| 18 | 0.180063 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.171489 | `azmcp_appconfig_kv_lock` | ❌ |
| 20 | 0.171382 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 60

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678472 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.558177 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.554750 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.501199 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.482079 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.467807 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.458406 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.457318 | `azmcp_grafana_list` | ❌ |
| 9 | 0.451969 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.431642 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.431126 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.423031 | `azmcp_search_service_list` | ❌ |
| 13 | 0.416796 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.410134 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.403577 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.379322 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.377511 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.374451 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.365458 | `azmcp_group_list` | ❌ |
| 20 | 0.354490 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 61

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478518 | `azmcp_mysql_database_list` | ❌ |
| 2 | 0.474512 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.412380 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.389993 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.377048 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.372766 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.363906 | `azmcp_mysql_server_param_get` | ❌ |
| 9 | 0.355142 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.337771 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.281558 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.251411 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.248026 | `azmcp_grafana_list` | ❌ |
| 14 | 0.248003 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.245417 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.235455 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.232383 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.224586 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.218115 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.216149 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 62

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.534194 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530210 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.464360 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.456616 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.441837 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.431914 | `azmcp_grafana_list` | ❌ |
| 8 | 0.416021 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.412407 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.410413 | `azmcp_search_service_list` | ❌ |
| 11 | 0.408235 | `azmcp_mysql_table_schema_get` | ❌ |
| 12 | 0.406576 | `azmcp_mysql_server_param_get` | ❌ |
| 13 | 0.399358 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.376596 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.375883 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.364016 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.356691 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.341439 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.341087 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.334813 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 63

**Expected Tool:** `azmcp_mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `azmcp_mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.407671 | `azmcp_mysql_server_param_set` | ❌ |
| 3 | 0.333841 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.313150 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.310834 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.300031 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.296654 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.292563 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.285663 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.285645 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.183735 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.160082 | `azmcp_appconfig_kv_list` | ❌ |
| 13 | 0.148022 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.139462 | `azmcp_appconfig_kv_unlock` | ❌ |
| 15 | 0.137158 | `azmcp_monitor_metrics_query` | ❌ |
| 16 | 0.124274 | `azmcp_grafana_list` | ❌ |
| 17 | 0.119862 | `azmcp_loadtesting_testrun_list` | ❌ |
| 18 | 0.117324 | `azmcp_appconfig_kv_set` | ❌ |
| 19 | 0.116181 | `azmcp_appconfig_kv_lock` | ❌ |
| 20 | 0.115886 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 64

**Expected Tool:** `azmcp_mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.390761 | `azmcp_mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `azmcp_mysql_server_param_get` | ❌ |
| 3 | 0.307496 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.298911 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.254205 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.253189 | `azmcp_mysql_table_schema_get` | ❌ |
| 7 | 0.246424 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.246019 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.238742 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.236453 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.112499 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.105281 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.105050 | `azmcp_appconfig_kv_unlock` | ❌ |
| 14 | 0.094606 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.093966 | `azmcp_extension_az` | ❌ |
| 16 | 0.090695 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.090334 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.089483 | `azmcp_appconfig_kv_show` | ❌ |
| 19 | 0.088097 | `azmcp_loadtesting_test_create` | ❌ |
| 20 | 0.086501 | `azmcp_appconfig_kv_lock` | ❌ |

---

## Test 65

**Expected Tool:** `azmcp_mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633448 | `azmcp_mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573844 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.550898 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.546963 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.475178 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.447282 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.442053 | `azmcp_kusto_table_list` | ❌ |
| 8 | 0.431034 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.429975 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.418810 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.410273 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.401217 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.361477 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.335069 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.308385 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.268415 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.260118 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.253046 | `azmcp_grafana_list` | ❌ |
| 19 | 0.241294 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.239226 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 66

**Expected Tool:** `azmcp_mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609131 | `azmcp_mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.526236 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.525709 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.507258 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.498050 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.439004 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.419901 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.403265 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.391670 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.385166 | `azmcp_postgres_table_schema_get` | ❌ |
| 11 | 0.382292 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.349434 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.342926 | `azmcp_kusto_table_schema` | ❌ |
| 14 | 0.319674 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.303999 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.281571 | `azmcp_kusto_sample` | ❌ |
| 17 | 0.236723 | `azmcp_grafana_list` | ❌ |
| 18 | 0.231173 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.214496 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.209943 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 67

**Expected Tool:** `azmcp_mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630646 | `azmcp_mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558304 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.545030 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.482513 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.457748 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.443929 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.407432 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.398085 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.372895 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.348889 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.347306 | `azmcp_postgres_server_config_get` | ❌ |
| 12 | 0.324655 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.307920 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.271949 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.243853 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.239319 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.202801 | `azmcp_bicepschema_get` | ❌ |
| 18 | 0.194173 | `azmcp_grafana_list` | ❌ |
| 19 | 0.186513 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.186054 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 68

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815617 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.644014 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.622790 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.542685 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.490904 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.471672 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.453436 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.444410 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.435828 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.418348 | `azmcp_postgres_database_query` | ❌ |
| 11 | 0.414679 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.407877 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.319946 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.293787 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.292441 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.289334 | `azmcp_grafana_list` | ❌ |
| 17 | 0.252438 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.249563 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.245546 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.245456 | `azmcp_group_list` | ❌ |

---

## Test 69

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760033 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589783 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.585891 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.552660 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.495629 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.452128 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.433860 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.430589 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.426839 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.416937 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.385475 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.365997 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.281529 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.261442 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.257971 | `azmcp_grafana_list` | ❌ |
| 16 | 0.247726 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.235403 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.227995 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.223442 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.222503 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 70

**Expected Tool:** `azmcp_postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546211 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.503267 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.466599 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.415817 | `azmcp_postgres_database_query` | ✅ **EXPECTED** |
| 5 | 0.403969 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.403924 | `azmcp_postgres_server_config_get` | ❌ |
| 7 | 0.380446 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.361081 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.354323 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.341271 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.264914 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.262356 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.262160 | `azmcp_kusto_query` | ❌ |
| 14 | 0.254174 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.248628 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.244295 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.236363 | `azmcp_grafana_list` | ❌ |
| 18 | 0.218677 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.217855 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.189002 | `azmcp_foundry_models_list` | ❌ |

---

## Test 71

**Expected Tool:** `azmcp_postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `azmcp_postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.599471 | `azmcp_postgres_server_param_get` | ❌ |
| 3 | 0.535229 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.535049 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.518574 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.463172 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.431476 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.394675 | `azmcp_postgres_database_query` | ❌ |
| 9 | 0.337899 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.315825 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.269224 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.235724 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.222849 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.222599 | `azmcp_loadtesting_test_get` | ❌ |
| 15 | 0.208314 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.177778 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.174936 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.168322 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.160792 | `azmcp_grafana_list` | ❌ |
| 20 | 0.158279 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 72

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900023 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640733 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.565914 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.538997 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.507621 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.483663 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.472458 | `azmcp_grafana_list` | ❌ |
| 8 | 0.453841 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.446509 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.430475 | `azmcp_search_service_list` | ❌ |
| 11 | 0.416298 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.408673 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.406617 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.398485 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.397428 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.389191 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.373699 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.365995 | `azmcp_group_list` | ❌ |
| 19 | 0.351894 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.346981 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 73

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.607062 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.576349 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.522996 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.506171 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.409406 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.400088 | `azmcp_postgres_server_param_set` | ❌ |
| 8 | 0.372955 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.336934 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.336264 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.274763 | `azmcp_grafana_list` | ❌ |
| 12 | 0.260533 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.253264 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.245527 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.241835 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.239500 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.229842 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.227547 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.225295 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.219274 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 74

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579232 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.531804 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.514445 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.505869 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.452608 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.444127 | `azmcp_grafana_list` | ❌ |
| 8 | 0.414695 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.411590 | `azmcp_search_service_list` | ❌ |
| 10 | 0.410719 | `azmcp_postgres_database_query` | ❌ |
| 11 | 0.403538 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.399866 | `azmcp_postgres_table_schema_get` | ❌ |
| 13 | 0.376954 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.362650 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.362557 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.360396 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.358409 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.334679 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.334101 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.325681 | `azmcp_group_list` | ❌ |

---

## Test 75

**Expected Tool:** `azmcp_postgres_server_param`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594753 | `azmcp_postgres_server_param_get` | ❌ |
| 2 | 0.539671 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.489693 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.480826 | `azmcp_postgres_server_param_set` | ❌ |
| 5 | 0.451871 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.357606 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.343799 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.330875 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.305351 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.295439 | `azmcp_mysql_server_param_set` | ❌ |
| 11 | 0.185273 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.174107 | `azmcp_grafana_list` | ❌ |
| 13 | 0.169190 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.166286 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.158090 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.155785 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.145346 | `azmcp_loadtesting_testrun_list` | ❌ |
| 18 | 0.145056 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.143280 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.140139 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 76

**Expected Tool:** `azmcp_postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488474 | `azmcp_postgres_server_config_get` | ❌ |
| 2 | 0.469794 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.464562 | `azmcp_postgres_server_param_set` | ✅ **EXPECTED** |
| 4 | 0.447011 | `azmcp_postgres_server_param_get` | ❌ |
| 5 | 0.440760 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.354049 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.341624 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.317484 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.241642 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.227753 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.133385 | `azmcp_kusto_sample` | ❌ |
| 12 | 0.127120 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.123491 | `azmcp_kusto_table_schema` | ❌ |
| 14 | 0.118089 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.114978 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.113841 | `azmcp_grafana_list` | ❌ |
| 17 | 0.112605 | `azmcp_deploy_plan_get` | ❌ |
| 18 | 0.108485 | `azmcp_kusto_table_list` | ❌ |
| 19 | 0.102847 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.102298 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 77

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789883 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.750580 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.574930 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.519820 | `azmcp_postgres_table_schema_get` | ❌ |
| 5 | 0.501400 | `azmcp_postgres_server_config_get` | ❌ |
| 6 | 0.477688 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.449190 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.432813 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.430171 | `azmcp_postgres_server_param_get` | ❌ |
| 10 | 0.396688 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.394552 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.373673 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.352211 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.308203 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.299785 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.257808 | `azmcp_grafana_list` | ❌ |
| 17 | 0.256245 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.249162 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.236931 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.229889 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 78

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736083 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.690112 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.558357 | `azmcp_postgres_table_schema_get` | ❌ |
| 4 | 0.543331 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.521570 | `azmcp_postgres_server_config_get` | ❌ |
| 6 | 0.464929 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.457757 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.447184 | `azmcp_postgres_server_param_get` | ❌ |
| 9 | 0.390392 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.383179 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.371151 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.334843 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.315781 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.307262 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.272906 | `azmcp_kusto_sample` | ❌ |
| 16 | 0.266178 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.243542 | `azmcp_grafana_list` | ❌ |
| 18 | 0.207521 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.205697 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.202950 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 79

**Expected Tool:** `azmcp_postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714877 | `azmcp_postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597846 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.574230 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.508082 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.480733 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.475665 | `azmcp_kusto_table_schema` | ❌ |
| 7 | 0.443816 | `azmcp_postgres_server_param_get` | ❌ |
| 8 | 0.442553 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.427530 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.406761 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.362687 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.322766 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.303748 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.253353 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.239225 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.212206 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.201673 | `azmcp_grafana_list` | ❌ |
| 18 | 0.185124 | `azmcp_appconfig_kv_list` | ❌ |
| 19 | 0.183782 | `azmcp_bicepschema_get` | ❌ |
| 20 | 0.167346 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 80

**Expected Tool:** `azmcp_extension_azd`  
**Prompt:** Create a To-Do list web application that uses NodeJS and MongoDB  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.241366 | `azmcp_extension_az` | ❌ |
| 2 | 0.203706 | `azmcp_deploy_iac_rules_get` | ❌ |
| 3 | 0.200024 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.196585 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.190019 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.187620 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.186051 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.181543 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.179933 | `azmcp_bestpractices_get` | ❌ |
| 10 | 0.177946 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.173269 | `azmcp_extension_azd` | ✅ **EXPECTED** |
| 12 | 0.165761 | `azmcp_postgres_table_list` | ❌ |
| 13 | 0.148122 | `azmcp_postgres_database_list` | ❌ |
| 14 | 0.145027 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.143918 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.140478 | `azmcp_storage_share_file_list` | ❌ |
| 17 | 0.138464 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.137936 | `azmcp_postgres_database_query` | ❌ |
| 19 | 0.137912 | `azmcp_mysql_database_query` | ❌ |
| 20 | 0.129433 | `azmcp_sql_db_list` | ❌ |

---

## Test 81

**Expected Tool:** `azmcp_extension_azd`  
**Prompt:** Deploy my web application to Azure App Service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544719 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.489853 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.441305 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.437357 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.421067 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.404718 | `azmcp_bestpractices_get` | ❌ |
| 7 | 0.394023 | `azmcp_deploy_iac_rules_get` | ❌ |
| 8 | 0.364145 | `azmcp_extension_azd` | ✅ **EXPECTED** |
| 9 | 0.361097 | `azmcp_foundry_models_deployments_list` | ❌ |
| 10 | 0.356426 | `azmcp_extension_az` | ❌ |
| 11 | 0.308550 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.299738 | `azmcp_search_index_list` | ❌ |
| 13 | 0.297374 | `azmcp_workbooks_delete` | ❌ |
| 14 | 0.275067 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.273949 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.273452 | `azmcp_search_service_list` | ❌ |
| 17 | 0.260381 | `azmcp_storage_queue_message_send` | ❌ |
| 18 | 0.259116 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 19 | 0.249133 | `azmcp_storage_account_details` | ❌ |
| 20 | 0.244902 | `azmcp_sql_db_list` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686701 | `azmcp_deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.486709 | `azmcp_extension_azd` | ❌ |
| 3 | 0.471692 | `azmcp_deploy_plan_get` | ❌ |
| 4 | 0.404890 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.392565 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.389603 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.341180 | `azmcp_monitor_resource_log_query` | ❌ |
| 8 | 0.340723 | `azmcp_extension_az` | ❌ |
| 9 | 0.334992 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.334572 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.327028 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.325553 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.315158 | `azmcp_functionapp_get` | ❌ |
| 14 | 0.307291 | `azmcp_sql_db_show` | ❌ |
| 15 | 0.299854 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.288973 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.284418 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.283342 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.281706 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.275999 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 83

**Expected Tool:** `azmcp_deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680640 | `azmcp_deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562521 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.505052 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.497193 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.435921 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.431654 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.423230 | `azmcp_bestpractices_get` | ❌ |
| 8 | 0.371127 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.343117 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.323166 | `azmcp_extension_az` | ❌ |
| 11 | 0.322230 | `azmcp_extension_azqr` | ❌ |
| 12 | 0.284401 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.264049 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.263521 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.263337 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.255084 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.244720 | `azmcp_subscription_list` | ❌ |
| 18 | 0.244196 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.242778 | `azmcp_storage_account_details` | ❌ |
| 20 | 0.239647 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 84

**Expected Tool:** `azmcp_deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529092 | `azmcp_deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.415286 | `azmcp_extension_az` | ❌ |
| 3 | 0.404829 | `azmcp_bicepschema_get` | ❌ |
| 4 | 0.399397 | `azmcp_bestpractices_get` | ❌ |
| 5 | 0.385283 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 6 | 0.341436 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 7 | 0.304788 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.278653 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.266851 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.265367 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 11 | 0.243314 | `azmcp_extension_azd` | ❌ |
| 12 | 0.219521 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.206983 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.206902 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 15 | 0.202660 | `azmcp_storage_blob_details` | ❌ |
| 16 | 0.202239 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.201288 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.195422 | `azmcp_mysql_table_list` | ❌ |
| 19 | 0.191094 | `azmcp_storage_share_file_list` | ❌ |
| 20 | 0.188615 | `azmcp_role_assignment_list` | ❌ |

---

## Test 85

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `azmcp_deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.448918 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.386921 | `azmcp_extension_az` | ❌ |
| 5 | 0.386704 | `azmcp_bestpractices_get` | ❌ |
| 6 | 0.375202 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.373363 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.351749 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.346892 | `azmcp_extension_azd` | ❌ |
| 10 | 0.338440 | `azmcp_foundry_models_deploy` | ❌ |
| 11 | 0.230335 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.230063 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.202583 | `azmcp_storage_queue_message_send` | ❌ |
| 14 | 0.198738 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.194866 | `azmcp_workbooks_delete` | ❌ |
| 16 | 0.192621 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.190369 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 18 | 0.163702 | `azmcp_storage_datalake_directory_create` | ❌ |
| 19 | 0.163398 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.161544 | `azmcp_workbooks_create` | ❌ |

---

## Test 86

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688096 | `azmcp_deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587885 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499406 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.498583 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.437539 | `azmcp_bestpractices_get` | ❌ |
| 6 | 0.430367 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.421809 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.416291 | `azmcp_extension_az` | ❌ |
| 9 | 0.413728 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.393549 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.312900 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.300708 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.279571 | `azmcp_workbooks_delete` | ❌ |
| 14 | 0.278052 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.252693 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.243502 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 17 | 0.242538 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.242049 | `azmcp_storage_account_details` | ❌ |
| 19 | 0.240951 | `azmcp_mysql_table_schema_get` | ❌ |
| 20 | 0.239462 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 87

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665665 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.571436 | `azmcp_functionapp_list` | ❌ |
| 3 | 0.448179 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.390091 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.379655 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.375647 | `azmcp_bestpractices_get` | ❌ |
| 7 | 0.373215 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.347628 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.347347 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.342763 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.341448 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.341263 | `azmcp_extension_azd` | ❌ |
| 13 | 0.341117 | `azmcp_extension_az` | ❌ |
| 14 | 0.338591 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.336261 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.323953 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.322437 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.317649 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.311100 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.303835 | `azmcp_workbooks_delete` | ❌ |

---

## Test 88

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620797 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.571758 | `azmcp_functionapp_list` | ❌ |
| 3 | 0.447400 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.424693 | `azmcp_appconfig_account_list` | ❌ |
| 5 | 0.422336 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.407133 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.400206 | `azmcp_loadtesting_test_get` | ❌ |
| 8 | 0.392852 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.386036 | `azmcp_bestpractices_get` | ❌ |
| 10 | 0.367183 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.363406 | `azmcp_loadtesting_test_create` | ❌ |
| 12 | 0.362460 | `azmcp_storage_account_details` | ❌ |
| 13 | 0.361753 | `azmcp_deploy_plan_get` | ❌ |
| 14 | 0.342398 | `azmcp_postgres_server_config_get` | ❌ |
| 15 | 0.321697 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.312611 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.310132 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.294528 | `azmcp_storage_blob_details` | ❌ |
| 19 | 0.291382 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.289833 | `azmcp_mysql_server_list` | ❌ |

---

## Test 89

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630260 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.599171 | `azmcp_functionapp_list` | ❌ |
| 3 | 0.456176 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.420189 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.390708 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.333897 | `azmcp_bestpractices_get` | ❌ |
| 7 | 0.322086 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.320055 | `azmcp_aks_cluster_get` | ❌ |
| 9 | 0.317583 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.317485 | `azmcp_loadtesting_testrun_get` | ❌ |
| 11 | 0.317367 | `azmcp_storage_account_details` | ❌ |
| 12 | 0.311384 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.297747 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.295549 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.295174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.290156 | `azmcp_servicebus_queue_details` | ❌ |
| 17 | 0.287686 | `azmcp_storage_blob_container_details` | ❌ |
| 18 | 0.277653 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.276452 | `azmcp_storage_blob_container_list` | ❌ |
| 20 | 0.271707 | `azmcp_mysql_server_param_get` | ❌ |

---

## Test 90

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.703066 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.592001 | `azmcp_functionapp_list` | ❌ |
| 3 | 0.433989 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.432317 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.424646 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.416988 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.411685 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 8 | 0.396236 | `azmcp_storage_account_details` | ❌ |
| 9 | 0.389322 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.379156 | `azmcp_bestpractices_get` | ❌ |
| 11 | 0.376019 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.375307 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.368506 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.366961 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.360644 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.360165 | `azmcp_aks_cluster_get` | ❌ |
| 17 | 0.348506 | `azmcp_foundry_models_deployments_list` | ❌ |
| 18 | 0.346255 | `azmcp_group_list` | ❌ |
| 19 | 0.343892 | `azmcp_workbooks_list` | ❌ |
| 20 | 0.332477 | `azmcp_extension_azqr` | ❌ |

---

## Test 91

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611202 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.575736 | `azmcp_functionapp_list` | ❌ |
| 3 | 0.443459 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.438507 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.383917 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.355524 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.355468 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 8 | 0.351673 | `azmcp_bestpractices_get` | ❌ |
| 9 | 0.347266 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.346738 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 11 | 0.345205 | `azmcp_storage_account_details` | ❌ |
| 12 | 0.337247 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.333000 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.325680 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.320825 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.318174 | `azmcp_deploy_plan_get` | ❌ |
| 17 | 0.317825 | `azmcp_extension_az` | ❌ |
| 18 | 0.317679 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.313963 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 20 | 0.309525 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 92

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676375 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.584110 | `azmcp_functionapp_list` | ❌ |
| 3 | 0.445142 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.368188 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.366279 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.364992 | `azmcp_bestpractices_get` | ❌ |
| 7 | 0.363387 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.358624 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.352754 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.351460 | `azmcp_aks_cluster_get` | ❌ |
| 11 | 0.342238 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.336938 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.335848 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.331111 | `azmcp_loadtesting_testrun_get` | ❌ |
| 15 | 0.325942 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.323519 | `azmcp_foundry_models_deployments_list` | ❌ |
| 17 | 0.323377 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.312201 | `azmcp_workbooks_list` | ❌ |
| 19 | 0.309621 | `azmcp_storage_account_details` | ❌ |
| 20 | 0.300841 | `azmcp_role_assignment_list` | ❌ |

---

## Test 93

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.616800 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.579598 | `azmcp_functionapp_list` | ❌ |
| 3 | 0.433958 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.376891 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.367552 | `azmcp_aks_cluster_get` | ❌ |
| 6 | 0.363432 | `azmcp_storage_account_details` | ❌ |
| 7 | 0.355956 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.349891 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.347295 | `azmcp_bestpractices_get` | ❌ |
| 10 | 0.346974 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.344067 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.342238 | `azmcp_servicebus_queue_details` | ❌ |
| 13 | 0.334256 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.330450 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.326091 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.319965 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 17 | 0.318309 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.306350 | `azmcp_storage_blob_container_details` | ❌ |
| 19 | 0.302236 | `azmcp_workbooks_show` | ❌ |
| 20 | 0.301782 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 94

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559764 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.556322 | `azmcp_functionapp_list` | ❌ |
| 3 | 0.426703 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.418362 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.408011 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.381629 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.370786 | `azmcp_bestpractices_get` | ❌ |
| 8 | 0.350663 | `azmcp_quota_region_availability_list` | ❌ |
| 9 | 0.335606 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.318517 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.307063 | `azmcp_extension_az` | ❌ |
| 12 | 0.304263 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.303123 | `azmcp_loadtesting_test_create` | ❌ |
| 14 | 0.301769 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.297752 | `azmcp_storage_account_details` | ❌ |
| 16 | 0.291130 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.281401 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.275577 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 19 | 0.274960 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.265729 | `azmcp_role_assignment_list` | ❌ |

---

## Test 95

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550600 | `azmcp_functionapp_list` | ❌ |
| 2 | 0.548008 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 3 | 0.440773 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.422774 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.384159 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.345108 | `azmcp_bestpractices_get` | ❌ |
| 7 | 0.333621 | `azmcp_quota_usage_check` | ❌ |
| 8 | 0.319464 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.310636 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.298267 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.297073 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.296174 | `azmcp_extension_az` | ❌ |
| 13 | 0.292793 | `azmcp_cloudarchitect_design` | ❌ |
| 14 | 0.274341 | `azmcp_storage_account_details` | ❌ |
| 15 | 0.270851 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.257282 | `azmcp_storage_blob_container_details` | ❌ |
| 17 | 0.249136 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.248352 | `azmcp_storage_blob_container_list` | ❌ |
| 19 | 0.243422 | `azmcp_storage_account_list` | ❌ |
| 20 | 0.241875 | `azmcp_sql_db_list` | ❌ |

---

## Test 96

**Expected Tool:** `azmcp_functionapp_list`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.782233 | `azmcp_functionapp_list` | ✅ **EXPECTED** |
| 2 | 0.547255 | `azmcp_search_service_list` | ❌ |
| 3 | 0.525885 | `azmcp_functionapp_get` | ❌ |
| 4 | 0.516618 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.516217 | `azmcp_appconfig_account_list` | ❌ |
| 6 | 0.493701 | `azmcp_storage_account_list` | ❌ |
| 7 | 0.485259 | `azmcp_subscription_list` | ❌ |
| 8 | 0.474425 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.465575 | `azmcp_group_list` | ❌ |
| 10 | 0.464534 | `azmcp_monitor_workspace_list` | ❌ |
| 11 | 0.455413 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.455388 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.451429 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.445099 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.442614 | `azmcp_redis_cluster_list` | ❌ |
| 16 | 0.432144 | `azmcp_grafana_list` | ❌ |
| 17 | 0.431611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.415840 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.414687 | `azmcp_foundry_models_deployments_list` | ❌ |
| 20 | 0.411904 | `azmcp_sql_db_list` | ❌ |

---

## Test 97

**Expected Tool:** `azmcp_functionapp_list`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610306 | `azmcp_functionapp_list` | ✅ **EXPECTED** |
| 2 | 0.515504 | `azmcp_functionapp_get` | ❌ |
| 3 | 0.452132 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.417717 | `azmcp_bestpractices_get` | ❌ |
| 5 | 0.385705 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.374655 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.372790 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.370376 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.369681 | `azmcp_subscription_list` | ❌ |
| 10 | 0.368018 | `azmcp_extension_az` | ❌ |
| 11 | 0.368004 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.359788 | `azmcp_search_service_list` | ❌ |
| 13 | 0.358720 | `azmcp_deploy_plan_get` | ❌ |
| 14 | 0.357329 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.351646 | `azmcp_storage_blob_container_list` | ❌ |
| 16 | 0.347887 | `azmcp_mysql_database_list` | ❌ |
| 17 | 0.334137 | `azmcp_storage_account_list` | ❌ |
| 18 | 0.334019 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.333136 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.327870 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 98

**Expected Tool:** `azmcp_functionapp_list`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.440842 | `azmcp_functionapp_list` | ✅ **EXPECTED** |
| 2 | 0.391843 | `azmcp_functionapp_get` | ❌ |
| 3 | 0.348106 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.286743 | `azmcp_bestpractices_get` | ❌ |
| 5 | 0.256927 | `azmcp_extension_az` | ❌ |
| 6 | 0.249658 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.244782 | `azmcp_appconfig_kv_list` | ❌ |
| 8 | 0.240729 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.239349 | `azmcp_foundry_models_deployments_list` | ❌ |
| 10 | 0.235352 | `azmcp_extension_azd` | ❌ |
| 11 | 0.207391 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.197623 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.195857 | `azmcp_role_assignment_list` | ❌ |
| 14 | 0.194503 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.185096 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.184120 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.184051 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.182124 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.179069 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.177017 | `azmcp_storage_share_file_list` | ❌ |

---

## Test 99

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.740327 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.595854 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.590476 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.575960 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.542585 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.526698 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.434682 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.414022 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.358279 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.330026 | `azmcp_appconfig_kv_set` | ❌ |
| 11 | 0.308667 | `azmcp_loadtesting_test_create` | ❌ |
| 12 | 0.300980 | `azmcp_storage_datalake_directory_create` | ❌ |
| 13 | 0.285184 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.254339 | `azmcp_storage_account_details` | ❌ |
| 15 | 0.237031 | `azmcp_storage_blob_container_list` | ❌ |
| 16 | 0.233821 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.229694 | `azmcp_storage_account_list` | ❌ |
| 18 | 0.219479 | `azmcp_subscription_list` | ❌ |
| 19 | 0.213420 | `azmcp_storage_queue_message_send` | ❌ |
| 20 | 0.210729 | `azmcp_search_service_list` | ❌ |

---

## Test 100

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627951 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.624457 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.565005 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.539554 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.493432 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.475385 | `azmcp_keyvault_secret_list` | ❌ |
| 7 | 0.423728 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.418788 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.390699 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.346167 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.341449 | `azmcp_storage_account_details` | ❌ |
| 12 | 0.317177 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.316884 | `azmcp_storage_blob_container_list` | ❌ |
| 14 | 0.298804 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.293421 | `azmcp_subscription_list` | ❌ |
| 16 | 0.280510 | `azmcp_storage_blob_list` | ❌ |
| 17 | 0.276581 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.274949 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 19 | 0.273791 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.271911 | `azmcp_quota_usage_check` | ❌ |

---

## Test 101

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662551 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.606534 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.540155 | `azmcp_keyvault_certificate_import` | ❌ |
| 4 | 0.535157 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.499272 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.482380 | `azmcp_keyvault_secret_list` | ❌ |
| 7 | 0.432557 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.415722 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.412404 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.411136 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.365386 | `azmcp_sql_db_show` | ❌ |
| 12 | 0.363228 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.332770 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.315986 | `azmcp_storage_blob_container_list` | ❌ |
| 15 | 0.315096 | `azmcp_storage_table_list` | ❌ |
| 16 | 0.305778 | `azmcp_subscription_list` | ❌ |
| 17 | 0.304492 | `azmcp_storage_blob_container_details` | ❌ |
| 18 | 0.301710 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.300375 | `azmcp_storage_account_list` | ❌ |
| 20 | 0.299789 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 102

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649993 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.521183 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.469585 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.467097 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.426600 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.398099 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.364868 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.337967 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.269560 | `azmcp_appconfig_kv_lock` | ❌ |
| 10 | 0.267356 | `azmcp_appconfig_kv_set` | ❌ |
| 11 | 0.239740 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 12 | 0.238854 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.224373 | `azmcp_workbooks_delete` | ❌ |
| 14 | 0.214526 | `azmcp_storage_account_details` | ❌ |
| 15 | 0.208793 | `azmcp_storage_blob_container_list` | ❌ |
| 16 | 0.200472 | `azmcp_storage_datalake_directory_create` | ❌ |
| 17 | 0.199045 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.195575 | `azmcp_storage_account_list` | ❌ |
| 19 | 0.185744 | `azmcp_storage_blob_list` | ❌ |
| 20 | 0.182875 | `azmcp_storage_account_create` | ❌ |

---

## Test 103

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649676 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.629902 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.527468 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.525268 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.491898 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.472199 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.399857 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.377865 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.287107 | `azmcp_appconfig_kv_set` | ❌ |
| 10 | 0.265369 | `azmcp_appconfig_kv_lock` | ❌ |
| 11 | 0.243505 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.238392 | `azmcp_storage_account_details` | ❌ |
| 13 | 0.234376 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.227232 | `azmcp_workbooks_delete` | ❌ |
| 15 | 0.225028 | `azmcp_storage_blob_container_list` | ❌ |
| 16 | 0.211454 | `azmcp_storage_datalake_directory_create` | ❌ |
| 17 | 0.211315 | `azmcp_storage_account_list` | ❌ |
| 18 | 0.203150 | `azmcp_storage_blob_upload` | ❌ |
| 19 | 0.197598 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.196937 | `azmcp_workbooks_create` | ❌ |

---

## Test 104

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.762015 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.637437 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.608799 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.566292 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.539624 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.484660 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.478100 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.453226 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.433468 | `azmcp_storage_blob_container_list` | ❌ |
| 10 | 0.431201 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.429531 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.428838 | `azmcp_storage_account_list` | ❌ |
| 13 | 0.424379 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.408270 | `azmcp_storage_blob_list` | ❌ |
| 15 | 0.408042 | `azmcp_subscription_list` | ❌ |
| 16 | 0.373773 | `azmcp_search_index_list` | ❌ |
| 17 | 0.368478 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.366071 | `azmcp_storage_account_details` | ❌ |
| 19 | 0.358938 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.357371 | `azmcp_search_service_list` | ❌ |

---

## Test 105

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660576 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.570278 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.540050 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.516722 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.509123 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.483404 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.420506 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.396055 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.390099 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.382082 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.375543 | `azmcp_storage_blob_container_list` | ❌ |
| 12 | 0.373188 | `azmcp_storage_account_details` | ❌ |
| 13 | 0.372424 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.365489 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.362782 | `azmcp_subscription_list` | ❌ |
| 16 | 0.336468 | `azmcp_storage_blob_list` | ❌ |
| 17 | 0.323177 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.317493 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.317235 | `azmcp_search_index_list` | ❌ |
| 20 | 0.315760 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |

---

## Test 106

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676352 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.569255 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.555829 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.465742 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.417395 | `azmcp_keyvault_certificate_list` | ❌ |
| 6 | 0.413161 | `azmcp_keyvault_secret_list` | ❌ |
| 7 | 0.412581 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.397141 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.389973 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.369034 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.340767 | `azmcp_appconfig_kv_lock` | ❌ |
| 12 | 0.287036 | `azmcp_storage_datalake_directory_create` | ❌ |
| 13 | 0.282541 | `azmcp_storage_account_details` | ❌ |
| 14 | 0.266251 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.261794 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.254412 | `azmcp_storage_blob_container_list` | ❌ |
| 17 | 0.252181 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.229735 | `azmcp_storage_queue_message_send` | ❌ |
| 19 | 0.227048 | `azmcp_storage_blob_list` | ❌ |
| 20 | 0.215837 | `azmcp_subscription_list` | ❌ |

---

## Test 107

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737135 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.650155 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.631528 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.498767 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.473916 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.468044 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.467326 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.463027 | `azmcp_storage_account_list` | ❌ |
| 9 | 0.460037 | `azmcp_storage_blob_container_list` | ❌ |
| 10 | 0.456153 | `azmcp_keyvault_certificate_get` | ❌ |
| 11 | 0.443785 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.439167 | `azmcp_appconfig_kv_list` | ❌ |
| 13 | 0.436496 | `azmcp_storage_blob_list` | ❌ |
| 14 | 0.428162 | `azmcp_keyvault_secret_create` | ❌ |
| 15 | 0.426909 | `azmcp_subscription_list` | ❌ |
| 16 | 0.403964 | `azmcp_storage_account_details` | ❌ |
| 17 | 0.402742 | `azmcp_search_index_list` | ❌ |
| 18 | 0.380480 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 19 | 0.378059 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.376452 | `azmcp_search_service_list` | ❌ |

---

## Test 108

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609392 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.535381 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.520010 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.480436 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.462249 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.429417 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.421475 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.412607 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.408423 | `azmcp_keyvault_certificate_import` | ❌ |
| 10 | 0.405205 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.382971 | `azmcp_storage_account_details` | ❌ |
| 12 | 0.378650 | `azmcp_storage_blob_container_list` | ❌ |
| 13 | 0.375139 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.369700 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.353390 | `azmcp_subscription_list` | ❌ |
| 16 | 0.338893 | `azmcp_storage_blob_list` | ❌ |
| 17 | 0.325023 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 18 | 0.323400 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.320761 | `azmcp_search_index_list` | ❌ |
| 20 | 0.306567 | `azmcp_role_assignment_list` | ❌ |

---

## Test 109

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.767756 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.613514 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.572297 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.516457 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.461437 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.417525 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.411481 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.388618 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.384262 | `azmcp_keyvault_certificate_list` | ❌ |
| 10 | 0.373932 | `azmcp_appconfig_kv_lock` | ❌ |
| 11 | 0.369981 | `azmcp_keyvault_certificate_get` | ❌ |
| 12 | 0.321535 | `azmcp_storage_datalake_directory_create` | ❌ |
| 13 | 0.288055 | `azmcp_storage_account_details` | ❌ |
| 14 | 0.287066 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.282860 | `azmcp_storage_queue_message_send` | ❌ |
| 16 | 0.248014 | `azmcp_storage_blob_container_list` | ❌ |
| 17 | 0.246321 | `azmcp_storage_account_list` | ❌ |
| 18 | 0.236457 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.209815 | `azmcp_subscription_list` | ❌ |
| 20 | 0.208111 | `azmcp_storage_blob_upload` | ❌ |

---

## Test 110

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747343 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.617131 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.569911 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.519048 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.455500 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.433185 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.424399 | `azmcp_storage_blob_container_list` | ❌ |
| 8 | 0.417973 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.415705 | `azmcp_storage_account_list` | ❌ |
| 10 | 0.414583 | `azmcp_keyvault_certificate_get` | ❌ |
| 11 | 0.410496 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.409822 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.400508 | `azmcp_storage_blob_list` | ❌ |
| 14 | 0.392378 | `azmcp_keyvault_certificate_create` | ❌ |
| 15 | 0.391082 | `azmcp_subscription_list` | ❌ |
| 16 | 0.364601 | `azmcp_storage_account_details` | ❌ |
| 17 | 0.355446 | `azmcp_search_index_list` | ❌ |
| 18 | 0.347416 | `azmcp_search_service_list` | ❌ |
| 19 | 0.341082 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.340472 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 111

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615400 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.520654 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.502346 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.467743 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.456807 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.411604 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.410957 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.409126 | `azmcp_keyvault_certificate_import` | ❌ |
| 9 | 0.395481 | `azmcp_storage_account_details` | ❌ |
| 10 | 0.385852 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.381612 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.361162 | `azmcp_storage_blob_container_list` | ❌ |
| 13 | 0.345256 | `azmcp_subscription_list` | ❌ |
| 14 | 0.344339 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.337698 | `azmcp_storage_account_list` | ❌ |
| 16 | 0.326380 | `azmcp_storage_blob_list` | ❌ |
| 17 | 0.315151 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 18 | 0.303769 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.300775 | `azmcp_storage_blob_container_details` | ❌ |
| 20 | 0.299295 | `azmcp_search_index_list` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660869 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.612708 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.481416 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.463682 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.456804 | `azmcp_loadtesting_test_get` | ❌ |
| 6 | 0.430975 | `azmcp_postgres_server_config_get` | ❌ |
| 7 | 0.392990 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.391924 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.390959 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.390819 | `azmcp_appconfig_kv_list` | ❌ |
| 11 | 0.390141 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.380541 | `azmcp_functionapp_get` | ❌ |
| 13 | 0.371630 | `azmcp_mysql_server_param_get` | ❌ |
| 14 | 0.367841 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.353948 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.350240 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.340171 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.339882 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.337661 | `azmcp_sql_elastic-pool_list` | ❌ |
| 20 | 0.336742 | `azmcp_storage_blob_container_details` | ❌ |

---

## Test 113

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666849 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.589954 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.508226 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.461466 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.449531 | `azmcp_functionapp_get` | ❌ |
| 6 | 0.448796 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.422993 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.413780 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.408420 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.396636 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.385261 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.384654 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.371570 | `azmcp_group_list` | ❌ |
| 14 | 0.365232 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.362332 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.356690 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.355049 | `azmcp_storage_account_details` | ❌ |
| 18 | 0.354600 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.353625 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.350559 | `azmcp_workbooks_list` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567273 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.565008 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.380301 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.368584 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.340349 | `azmcp_loadtesting_test_get` | ❌ |
| 6 | 0.340293 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.334923 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.334860 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.314513 | `azmcp_appconfig_kv_list` | ❌ |
| 10 | 0.309738 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.299098 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.298637 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.296592 | `azmcp_postgres_server_config_get` | ❌ |
| 14 | 0.295183 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.289342 | `azmcp_mysql_server_param_get` | ❌ |
| 16 | 0.283065 | `azmcp_storage_account_details` | ❌ |
| 17 | 0.275751 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.273195 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.267611 | `azmcp_sql_elastic-pool_list` | ❌ |
| 20 | 0.265086 | `azmcp_sql_db_list` | ❌ |

---

## Test 115

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661426 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.579767 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.503925 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.446330 | `azmcp_functionapp_get` | ❌ |
| 5 | 0.433913 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.419338 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.418518 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.417836 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.402432 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.390071 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.384782 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.382392 | `azmcp_storage_blob_container_details` | ❌ |
| 13 | 0.380074 | `azmcp_storage_account_details` | ❌ |
| 14 | 0.372812 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.367547 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.360459 | `azmcp_sql_elastic-pool_list` | ❌ |
| 17 | 0.359877 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.357011 | `azmcp_loadtesting_test_get` | ❌ |
| 19 | 0.354685 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.353462 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 116

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.800445 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.690255 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.599940 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.560861 | `azmcp_aks_cluster_get` | ❌ |
| 5 | 0.549327 | `azmcp_search_service_list` | ❌ |
| 6 | 0.543684 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.515922 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.509202 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.505702 | `azmcp_functionapp_list` | ❌ |
| 10 | 0.502401 | `azmcp_subscription_list` | ❌ |
| 11 | 0.498121 | `azmcp_group_list` | ❌ |
| 12 | 0.495977 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.486591 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 14 | 0.486237 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.486142 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.483592 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.482355 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.481469 | `azmcp_grafana_list` | ❌ |
| 19 | 0.452681 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.445271 | `azmcp_storage_table_list` | ❌ |

---

## Test 117

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608043 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.536412 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.492910 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.446270 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.409711 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.408385 | `azmcp_kusto_database_list` | ❌ |
| 7 | 0.393026 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.388143 | `azmcp_search_service_list` | ❌ |
| 9 | 0.387737 | `azmcp_search_index_list` | ❌ |
| 10 | 0.371809 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.371535 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.370237 | `azmcp_acr_registry_repository_list` | ❌ |
| 13 | 0.363804 | `azmcp_subscription_list` | ❌ |
| 14 | 0.362727 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.361928 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.360526 | `azmcp_storage_blob_container_list` | ❌ |
| 17 | 0.360053 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.356926 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.355864 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.349446 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624216 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.530023 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.449602 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.416564 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.392083 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.378826 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.377567 | `azmcp_acr_registry_repository_list` | ❌ |
| 8 | 0.374635 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.364022 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.345290 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.345241 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.342303 | `azmcp_extension_az` | ❌ |
| 13 | 0.341581 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.335444 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.334494 | `azmcp_acr_registry_list` | ❌ |
| 16 | 0.322075 | `azmcp_sql_elastic-pool_list` | ❌ |
| 17 | 0.317238 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.314872 | `azmcp_storage_blob_container_list` | ❌ |
| 19 | 0.312354 | `azmcp_subscription_list` | ❌ |
| 20 | 0.311971 | `azmcp_quota_usage_check` | ❌ |

---

## Test 119

**Expected Tool:** `azmcp_loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585388 | `azmcp_loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.531362 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.508690 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.413857 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.402698 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.399602 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.346526 | `azmcp_loadtesting_testrun_update` | ❌ |
| 8 | 0.342853 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.336713 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.323398 | `azmcp_monitor_workspace_log_query` | ❌ |
| 11 | 0.310466 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.310144 | `azmcp_workbooks_create` | ❌ |
| 13 | 0.299453 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.296991 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.290957 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.289956 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.288940 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.285555 | `azmcp_storage_queue_message_send` | ❌ |
| 19 | 0.273887 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.255511 | `azmcp_search_service_list` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643973 | `azmcp_loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.608802 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.574557 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.540879 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.473675 | `azmcp_loadtesting_testrun_list` | ❌ |
| 6 | 0.473321 | `azmcp_loadtesting_testrun_create` | ❌ |
| 7 | 0.437058 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.406483 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.397160 | `azmcp_group_list` | ❌ |
| 10 | 0.379262 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.373218 | `azmcp_loadtesting_testrun_update` | ❌ |
| 12 | 0.369694 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.365293 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.363159 | `azmcp_functionapp_get` | ❌ |
| 15 | 0.346393 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.341168 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.328990 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.322893 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.298666 | `azmcp_monitor_workspace_log_query` | ❌ |
| 20 | 0.296556 | `azmcp_workbooks_delete` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.717505 | `azmcp_loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.596282 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.514163 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.476321 | `azmcp_loadtesting_testrun_create` | ❌ |
| 5 | 0.446841 | `azmcp_loadtesting_test_get` | ❌ |
| 6 | 0.442769 | `azmcp_workbooks_create` | ❌ |
| 7 | 0.417412 | `azmcp_group_list` | ❌ |
| 8 | 0.395350 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.383376 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.375392 | `azmcp_loadtesting_testrun_get` | ❌ |
| 11 | 0.371653 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.370161 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.350771 | `azmcp_loadtesting_testrun_update` | ❌ |
| 14 | 0.342433 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.341147 | `azmcp_grafana_list` | ❌ |
| 16 | 0.335739 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.328741 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.326783 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.312734 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.306241 | `azmcp_quota_usage_check` | ❌ |

---

## Test 122

**Expected Tool:** `azmcp_loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738027 | `azmcp_loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.591851 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.577408 | `azmcp_group_list` | ❌ |
| 4 | 0.565565 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.561516 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.526662 | `azmcp_workbooks_list` | ❌ |
| 7 | 0.515624 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.512935 | `azmcp_loadtesting_test_get` | ❌ |
| 9 | 0.511607 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.488178 | `azmcp_loadtesting_testrun_list` | ❌ |
| 11 | 0.487330 | `azmcp_grafana_list` | ❌ |
| 12 | 0.473404 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.470899 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.467689 | `azmcp_loadtesting_testrun_get` | ❌ |
| 15 | 0.458800 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.454667 | `azmcp_search_service_list` | ❌ |
| 17 | 0.452190 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.447138 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.437348 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.426880 | `azmcp_sql_db_list` | ❌ |

---

## Test 123

**Expected Tool:** `azmcp_loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621803 | `azmcp_loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.592805 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.540789 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.530882 | `azmcp_loadtesting_testrun_update` | ❌ |
| 5 | 0.489907 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.472404 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.413872 | `azmcp_loadtesting_testrun_list` | ❌ |
| 8 | 0.411627 | `azmcp_loadtesting_testresource_list` | ❌ |
| 9 | 0.402120 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.353102 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.331019 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.325480 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.314636 | `azmcp_storage_datalake_directory_create` | ❌ |
| 14 | 0.308941 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.272151 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.267551 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.263746 | `azmcp_storage_queue_message_send` | ❌ |
| 18 | 0.250958 | `azmcp_monitor_workspace_log_query` | ❌ |
| 19 | 0.249723 | `azmcp_workbooks_show` | ❌ |
| 20 | 0.242768 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 124

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626778 | `azmcp_loadtesting_test_get` | ❌ |
| 2 | 0.603066 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.572731 | `azmcp_loadtesting_testrun_get` | ✅ **EXPECTED** |
| 4 | 0.561944 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.535183 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.499389 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.434255 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.415438 | `azmcp_loadtesting_testrun_update` | ❌ |
| 9 | 0.397875 | `azmcp_group_list` | ❌ |
| 10 | 0.397070 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.373582 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.366532 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.356307 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.353051 | `azmcp_workbooks_show` | ❌ |
| 15 | 0.346995 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.329148 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 17 | 0.328853 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.315577 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.293719 | `azmcp_monitor_workspace_log_query` | ❌ |
| 20 | 0.291211 | `azmcp_mysql_server_list` | ❌ |

---

## Test 125

**Expected Tool:** `azmcp_loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615977 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.607935 | `azmcp_loadtesting_test_get` | ❌ |
| 3 | 0.573158 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.568920 | `azmcp_loadtesting_testrun_list` | ✅ **EXPECTED** |
| 5 | 0.535207 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.492700 | `azmcp_loadtesting_testrun_create` | ❌ |
| 7 | 0.432149 | `azmcp_group_list` | ❌ |
| 8 | 0.417762 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.410933 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.406508 | `azmcp_loadtesting_test_create` | ❌ |
| 11 | 0.395915 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.392066 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.391147 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.356833 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.341385 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.340695 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.329464 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.328011 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.326457 | `azmcp_sql_elastic-pool_list` | ❌ |
| 20 | 0.323927 | `azmcp_redis_cache_list` | ❌ |

---

## Test 126

**Expected Tool:** `azmcp_loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659812 | `azmcp_loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.509199 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.455629 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.446611 | `azmcp_loadtesting_test_get` | ❌ |
| 5 | 0.422036 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.399536 | `azmcp_loadtesting_test_create` | ❌ |
| 7 | 0.384654 | `azmcp_loadtesting_testresource_list` | ❌ |
| 8 | 0.383635 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.320124 | `azmcp_workbooks_update` | ❌ |
| 10 | 0.300023 | `azmcp_workbooks_create` | ❌ |
| 11 | 0.272693 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.268219 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.267137 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.255408 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.251868 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.237372 | `azmcp_workbooks_delete` | ❌ |
| 17 | 0.232572 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.227194 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 19 | 0.226593 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.224638 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 127

**Expected Tool:** `azmcp_grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578892 | `azmcp_grafana_list` | ✅ **EXPECTED** |
| 2 | 0.544665 | `azmcp_search_service_list` | ❌ |
| 3 | 0.513028 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.505836 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.498077 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.497103 | `azmcp_functionapp_list` | ❌ |
| 7 | 0.493645 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.492724 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.492210 | `azmcp_subscription_list` | ❌ |
| 10 | 0.491522 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.489846 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.482789 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.479611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.452683 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.447727 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.441762 | `azmcp_storage_account_list` | ❌ |
| 17 | 0.441315 | `azmcp_group_list` | ❌ |
| 18 | 0.440392 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.436802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.422236 | `azmcp_acr_registry_list` | ❌ |

---

## Test 128

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.525557 | `azmcp_search_service_list` | ❌ |
| 3 | 0.516886 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.511954 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 5 | 0.508849 | `azmcp_storage_account_list` | ❌ |
| 6 | 0.507981 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.500471 | `azmcp_subscription_list` | ❌ |
| 8 | 0.499290 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.497156 | `azmcp_functionapp_list` | ❌ |
| 10 | 0.495957 | `azmcp_storage_table_list` | ❌ |
| 11 | 0.480850 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.476772 | `azmcp_aks_cluster_list` | ❌ |
| 13 | 0.472811 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.460936 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.460346 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.450971 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.447269 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.445430 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.438952 | `azmcp_grafana_list` | ❌ |
| 20 | 0.437939 | `azmcp_postgres_server_list` | ❌ |

---

## Test 129

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743903 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.519986 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.514088 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.492115 | `azmcp_acr_registry_repository_list` | ❌ |
| 5 | 0.476591 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 6 | 0.466545 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.452905 | `azmcp_acr_registry_list` | ❌ |
| 8 | 0.443767 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.441644 | `azmcp_group_list` | ❌ |
| 10 | 0.433933 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.416503 | `azmcp_storage_blob_container_list` | ❌ |
| 12 | 0.414548 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.413758 | `azmcp_sql_elastic-pool_list` | ❌ |
| 14 | 0.412709 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.412032 | `azmcp_storage_account_list` | ❌ |
| 16 | 0.411023 | `azmcp_functionapp_list` | ❌ |
| 17 | 0.402926 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.401591 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.398168 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.393822 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 130

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_required-subnet-size`  
**Prompt:** Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646978 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ✅ **EXPECTED** |
| 2 | 0.450342 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.245377 | `azmcp_extension_az` | ❌ |
| 4 | 0.235376 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.229262 | `azmcp_storage_blob_container_details` | ❌ |
| 6 | 0.218638 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 7 | 0.205268 | `azmcp_storage_share_file_list` | ❌ |
| 8 | 0.204496 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.203596 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.192371 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.186337 | `azmcp_storage_account_details` | ❌ |
| 12 | 0.179759 | `azmcp_storage_account_list` | ❌ |
| 13 | 0.177017 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 14 | 0.176750 | `azmcp_storage_blob_details` | ❌ |
| 15 | 0.169792 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.165332 | `azmcp_aks_cluster_get` | ❌ |
| 17 | 0.165173 | `azmcp_deploy_plan_get` | ❌ |
| 18 | 0.162216 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.153870 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.151555 | `azmcp_grafana_list` | ❌ |

---

## Test 131

**Expected Tool:** `azmcp_marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528228 | `azmcp_marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.353256 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.330935 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.323704 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.322443 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.302335 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.300683 | `azmcp_storage_blob_container_details` | ❌ |
| 8 | 0.289417 | `azmcp_workbooks_show` | ❌ |
| 9 | 0.281400 | `azmcp_storage_account_details` | ❌ |
| 10 | 0.276826 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.274403 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.269243 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.266271 | `azmcp_foundry_models_list` | ❌ |
| 14 | 0.264527 | `azmcp_search_index_describe` | ❌ |
| 15 | 0.255636 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.252041 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.251291 | `azmcp_mysql_table_schema_get` | ❌ |
| 18 | 0.248779 | `azmcp_grafana_list` | ❌ |
| 19 | 0.245820 | `azmcp_appconfig_kv_show` | ❌ |
| 20 | 0.235780 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 132

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653993 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.642495 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.531727 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490235 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.447777 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.438801 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.435166 | `azmcp_extension_az` | ❌ |
| 9 | 0.372867 | `azmcp_extension_azd` | ❌ |
| 10 | 0.353355 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.351664 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.321323 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.312391 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.289967 | `azmcp_storage_account_details` | ❌ |
| 15 | 0.266579 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 16 | 0.261539 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.259833 | `azmcp_subscription_list` | ❌ |
| 18 | 0.258970 | `azmcp_mysql_table_schema_get` | ❌ |
| 19 | 0.258775 | `azmcp_search_service_list` | ❌ |
| 20 | 0.258646 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 133

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.601339 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.558518 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.541091 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.516852 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.516443 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.452068 | `azmcp_extension_az` | ❌ |
| 7 | 0.424443 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.423950 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.409787 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.392171 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.358593 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.342487 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.306627 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.304620 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.302423 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.292995 | `azmcp_storage_account_details` | ❌ |
| 17 | 0.288043 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.279585 | `azmcp_subscription_list` | ❌ |
| 19 | 0.278876 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 20 | 0.277791 | `azmcp_search_service_list` | ❌ |

---

## Test 134

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624808 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.605663 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.518643 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.465572 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.455995 | `azmcp_extension_az` | ❌ |
| 6 | 0.454158 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.430630 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.399433 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.384057 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.380286 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.375863 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.362465 | `azmcp_extension_azd` | ❌ |
| 13 | 0.329342 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.329314 | `azmcp_storage_account_details` | ❌ |
| 15 | 0.316805 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.314123 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.301857 | `azmcp_subscription_list` | ❌ |
| 18 | 0.293443 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.287118 | `azmcp_search_service_list` | ❌ |
| 20 | 0.283591 | `azmcp_storage_blob_batch_set-tier` | ❌ |

---

## Test 135

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636218 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.577119 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.522998 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.445382 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.416803 | `azmcp_extension_az` | ❌ |
| 7 | 0.400447 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.393359 | `azmcp_functionapp_list` | ❌ |
| 9 | 0.381822 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.368157 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.317494 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.278941 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.269946 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.256382 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.253379 | `azmcp_storage_blob_upload` | ❌ |
| 16 | 0.243542 | `azmcp_storage_queue_message_send` | ❌ |
| 17 | 0.238484 | `azmcp_storage_account_details` | ❌ |
| 18 | 0.234246 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 19 | 0.232404 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.230334 | `azmcp_storage_blob_details` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587478 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.497350 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.496388 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.495659 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.474511 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.439058 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.431008 | `azmcp_extension_az` | ❌ |
| 8 | 0.423958 | `azmcp_functionapp_list` | ❌ |
| 9 | 0.412001 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.381090 | `azmcp_functionapp_get` | ❌ |
| 11 | 0.321678 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.317931 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.290695 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.277946 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.265176 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.263968 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.263566 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.259070 | `azmcp_storage_queue_message_send` | ❌ |
| 19 | 0.251130 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 20 | 0.246787 | `azmcp_storage_account_details` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.616294 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.543471 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.487322 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.458060 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.444295 | `azmcp_extension_az` | ❌ |
| 6 | 0.433095 | `azmcp_functionapp_list` | ❌ |
| 7 | 0.395940 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.394762 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.394214 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.390919 | `azmcp_functionapp_get` | ❌ |
| 11 | 0.332015 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.328838 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.289326 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.284215 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.275641 | `azmcp_storage_queue_message_send` | ❌ |
| 16 | 0.267667 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.263108 | `azmcp_storage_account_details` | ❌ |
| 18 | 0.261619 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.258034 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.254664 | `azmcp_mysql_database_query` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560174 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.521483 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.505123 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.405993 | `azmcp_extension_az` | ❌ |
| 6 | 0.405143 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.401209 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.398226 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.389556 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.355985 | `azmcp_extension_azd` | ❌ |
| 11 | 0.312174 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.283198 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.263368 | `azmcp_storage_account_details` | ❌ |
| 14 | 0.259417 | `azmcp_storage_blob_upload` | ❌ |
| 15 | 0.256045 | `azmcp_storage_blob_details` | ❌ |
| 16 | 0.249439 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.243086 | `azmcp_mysql_database_query` | ❌ |
| 18 | 0.237289 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.225940 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.224676 | `azmcp_mysql_server_list` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589817 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.508285 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.487728 | `azmcp_extension_az` | ❌ |
| 4 | 0.472112 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.433134 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.394086 | `azmcp_functionapp_list` | ❌ |
| 7 | 0.385965 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.368831 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.358703 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.355752 | `azmcp_functionapp_get` | ❌ |
| 11 | 0.293848 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.278572 | `azmcp_storage_queue_message_send` | ❌ |
| 13 | 0.261465 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.259723 | `azmcp_mysql_database_query` | ❌ |
| 15 | 0.249260 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.248175 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.248003 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.246921 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 19 | 0.231120 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.223124 | `azmcp_storage_account_details` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a simple HTTP-triggered function app in javascript that returns a random compliment from a predefined list in a JSON response. And deploy it to azure eventually. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429170 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.408233 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.406619 | `azmcp_extension_az` | ❌ |
| 4 | 0.387498 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 5 | 0.380754 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.352369 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.345059 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.319970 | `azmcp_loadtesting_test_create` | ❌ |
| 9 | 0.316210 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 10 | 0.299148 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.232320 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.218912 | `azmcp_workbooks_create` | ❌ |
| 13 | 0.217972 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.210908 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.210532 | `azmcp_storage_blob_details` | ❌ |
| 16 | 0.201471 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.197959 | `azmcp_mysql_database_query` | ❌ |
| 18 | 0.190147 | `azmcp_storage_account_details` | ❌ |
| 19 | 0.185991 | `azmcp_storage_queue_message_send` | ❌ |
| 20 | 0.174633 | `azmcp_subscription_list` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497276 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.493182 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.408474 | `azmcp_extension_az` | ❌ |
| 4 | 0.405146 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.395623 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.391252 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 7 | 0.374154 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.358213 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.348171 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.304256 | `azmcp_extension_azd` | ❌ |
| 11 | 0.243575 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.230280 | `azmcp_storage_blob_container_create` | ❌ |
| 13 | 0.220657 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.220579 | `azmcp_storage_blob_container_list` | ❌ |
| 15 | 0.218621 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.209213 | `azmcp_workbooks_create` | ❌ |
| 17 | 0.208849 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.207259 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.203732 | `azmcp_storage_blob_list` | ❌ |
| 20 | 0.191395 | `azmcp_storage_blob_upload` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476307 | `azmcp_monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.472094 | `azmcp_monitor_workspace_list` | ❌ |
| 3 | 0.468064 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.464012 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.459778 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.436971 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.418755 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.413357 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.404230 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.380121 | `azmcp_grafana_list` | ❌ |
| 11 | 0.358432 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.339320 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.337603 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.316657 | `azmcp_workbooks_show` | ❌ |
| 15 | 0.314731 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.305738 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.298822 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.296719 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.292690 | `azmcp_workbooks_delete` | ❌ |
| 20 | 0.279273 | `azmcp_kusto_query` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592640 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424141 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.332356 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.315519 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.315310 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.311108 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 7 | 0.305464 | `azmcp_servicebus_queue_details` | ❌ |
| 8 | 0.304735 | `azmcp_grafana_list` | ❌ |
| 9 | 0.303453 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.297379 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.294124 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.293189 | `azmcp_search_index_describe` | ❌ |
| 13 | 0.284519 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.283102 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.282547 | `azmcp_mysql_table_schema_get` | ❌ |
| 16 | 0.277663 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.277566 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.270098 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 19 | 0.249144 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.248987 | `azmcp_bicepschema_get` | ❌ |

---

## Test 144

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611603 | `azmcp_storage_account_details` | ❌ |
| 2 | 0.587736 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.557820 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.547214 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.542805 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.517515 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.511937 | `azmcp_storage_blob_container_details` | ❌ |
| 8 | 0.473421 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.459829 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.431109 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.417098 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 12 | 0.414488 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.412347 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.403921 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.401901 | `azmcp_monitor_metrics_query` | ❌ |
| 16 | 0.397526 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.393422 | `azmcp_storage_blob_details` | ❌ |
| 18 | 0.390422 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.378187 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.359476 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 145

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633173 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495513 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.380252 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.370848 | `azmcp_monitor_table_type_list` | ❌ |
| 5 | 0.359089 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.353264 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.344326 | `azmcp_quota_usage_check` | ❌ |
| 8 | 0.337907 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.329534 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.324002 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.308315 | `azmcp_monitor_workspace_log_query` | ❌ |
| 12 | 0.303296 | `azmcp_search_index_list` | ❌ |
| 13 | 0.302722 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.301831 | `azmcp_workbooks_show` | ❌ |
| 15 | 0.299167 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.291565 | `azmcp_cloudarchitect_design` | ❌ |
| 17 | 0.291260 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.286293 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.286161 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.283538 | `azmcp_search_index_describe` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555377 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.445146 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.439684 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.417973 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.409107 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.404582 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.388205 | `azmcp_quota_usage_check` | ❌ |
| 8 | 0.380075 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.354218 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.341791 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.340642 | `azmcp_loadtesting_testrun_list` | ❌ |
| 12 | 0.339771 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.335430 | `azmcp_monitor_metrics_definitions` | ❌ |
| 14 | 0.329460 | `azmcp_loadtesting_testresource_create` | ❌ |
| 15 | 0.328475 | `azmcp_loadtesting_test_get` | ❌ |
| 16 | 0.326875 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.291424 | `azmcp_search_index_list` | ❌ |
| 18 | 0.289449 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.287813 | `azmcp_storage_blob_details` | ❌ |
| 20 | 0.285117 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 147

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557830 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.504303 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.460611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.438233 | `azmcp_monitor_metrics_definitions` | ❌ |
| 6 | 0.389650 | `azmcp_monitor_resource_log_query` | ❌ |
| 7 | 0.372998 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.356326 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.341525 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.339388 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.326899 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.322916 | `azmcp_functionapp_get` | ❌ |
| 13 | 0.318196 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.311770 | `azmcp_search_index_list` | ❌ |
| 15 | 0.303909 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.302312 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.295333 | `azmcp_functionapp_list` | ❌ |
| 18 | 0.292911 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.292483 | `azmcp_search_service_list` | ❌ |
| 20 | 0.289116 | `azmcp_mysql_server_param_get` | ❌ |

---

## Test 148

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461249 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390029 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.306338 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.299972 | `azmcp_monitor_resource_log_query` | ❌ |
| 5 | 0.298432 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.279638 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.275443 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.267682 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.265702 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.263777 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.263325 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.259193 | `azmcp_grafana_list` | ❌ |
| 13 | 0.258070 | `azmcp_storage_blob_details` | ❌ |
| 14 | 0.249889 | `azmcp_loadtesting_test_get` | ❌ |
| 15 | 0.249506 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 16 | 0.248754 | `azmcp_loadtesting_testresource_list` | ❌ |
| 17 | 0.245735 | `azmcp_workbooks_show` | ❌ |
| 18 | 0.244710 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.241022 | `azmcp_storage_blob_container_details` | ❌ |
| 20 | 0.236117 | `azmcp_functionapp_get` | ❌ |

---

## Test 149

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492138 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.413804 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.411016 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.398988 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.397335 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.368342 | `azmcp_loadtesting_testrun_get` | ❌ |
| 7 | 0.359340 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.354940 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.316302 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.308767 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.305818 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.295918 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.293311 | `azmcp_loadtesting_testresource_create` | ❌ |
| 14 | 0.287528 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.287126 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.284552 | `azmcp_functionapp_list` | ❌ |
| 17 | 0.280087 | `azmcp_search_index_list` | ❌ |
| 18 | 0.272721 | `azmcp_search_service_list` | ❌ |
| 19 | 0.271392 | `azmcp_workbooks_show` | ❌ |
| 20 | 0.259245 | `azmcp_monitor_table_type_list` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525557 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.384524 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.373897 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.362120 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.299399 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.293162 | `azmcp_loadtesting_testrun_get` | ❌ |
| 7 | 0.287714 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 8 | 0.281050 | `azmcp_search_index_query` | ❌ |
| 9 | 0.272320 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.267039 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.266440 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.266224 | `azmcp_storage_blob_details` | ❌ |
| 13 | 0.264100 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 14 | 0.262741 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.261993 | `azmcp_grafana_list` | ❌ |
| 16 | 0.256939 | `azmcp_loadtesting_testrun_list` | ❌ |
| 17 | 0.252276 | `azmcp_servicebus_queue_details` | ❌ |
| 18 | 0.246567 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.246175 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.244134 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480140 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.378128 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.363412 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.348716 | `azmcp_monitor_resource_log_query` | ❌ |
| 5 | 0.341334 | `azmcp_monitor_workspace_log_query` | ❌ |
| 6 | 0.331215 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.330074 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.328838 | `azmcp_monitor_metrics_definitions` | ❌ |
| 9 | 0.327098 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.319343 | `azmcp_loadtesting_testresource_create` | ❌ |
| 11 | 0.292195 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.278492 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.277129 | `azmcp_loadtesting_test_get` | ❌ |
| 14 | 0.276023 | `azmcp_mysql_server_param_get` | ❌ |
| 15 | 0.273665 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.272239 | `azmcp_functionapp_list` | ❌ |
| 17 | 0.266918 | `azmcp_search_index_list` | ❌ |
| 18 | 0.265303 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.254630 | `azmcp_search_service_list` | ❌ |
| 20 | 0.243449 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584895 | `azmcp_monitor_workspace_log_query` | ❌ |
| 2 | 0.577580 | `azmcp_monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.472053 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.469718 | `azmcp_monitor_metrics_query` | ❌ |
| 5 | 0.443445 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.442847 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.392381 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.390052 | `azmcp_grafana_list` | ❌ |
| 9 | 0.361106 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.359011 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.352808 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.345337 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.333495 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.328126 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.320743 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.307848 | `azmcp_aks_cluster_get` | ❌ |
| 17 | 0.307121 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.306879 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.302664 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.299868 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851018 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.586691 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.534829 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.511123 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.502075 | `azmcp_grafana_list` | ❌ |
| 8 | 0.488557 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.436216 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.420394 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.419859 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.413834 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.409643 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.405953 | `azmcp_search_index_list` | ❌ |
| 15 | 0.397408 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.375176 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.374930 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.366099 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.365781 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.365538 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798332 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.599917 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.532887 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.497065 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.487237 | `azmcp_grafana_list` | ❌ |
| 7 | 0.466630 | `azmcp_kusto_table_list` | ❌ |
| 8 | 0.441635 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.427408 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.413632 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.411590 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.403863 | `azmcp_deploy_app_logs_get` | ❌ |
| 13 | 0.398753 | `azmcp_mysql_table_schema_get` | ❌ |
| 14 | 0.389881 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.376474 | `azmcp_kusto_sample` | ❌ |
| 16 | 0.376338 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.370624 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.347853 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.343837 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.332323 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765635 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.569921 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.525469 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.504683 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.477280 | `azmcp_grafana_list` | ❌ |
| 7 | 0.447435 | `azmcp_kusto_table_list` | ❌ |
| 8 | 0.445347 | `azmcp_mysql_table_schema_get` | ❌ |
| 9 | 0.418517 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.416351 | `azmcp_kusto_table_schema` | ❌ |
| 11 | 0.412293 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.404192 | `azmcp_monitor_metrics_definitions` | ❌ |
| 13 | 0.394213 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.394068 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 15 | 0.383606 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.380581 | `azmcp_kusto_sample` | ❌ |
| 17 | 0.369889 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.361820 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.354757 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.347919 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 156

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843138 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736724 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.576731 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.502460 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.481189 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.475734 | `azmcp_grafana_list` | ❌ |
| 7 | 0.451212 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.427934 | `azmcp_kusto_table_schema` | ❌ |
| 9 | 0.421484 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.416739 | `azmcp_monitor_workspace_log_query` | ❌ |
| 11 | 0.406242 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.391308 | `azmcp_kusto_sample` | ❌ |
| 13 | 0.385866 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.384346 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.376246 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.370860 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.367591 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.348357 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.340101 | `azmcp_foundry_models_list` | ❌ |
| 20 | 0.339804 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 157

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `azmcp_grafana_list` | ❌ |
| 3 | 0.660038 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.588276 | `azmcp_search_service_list` | ❌ |
| 5 | 0.583213 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.530433 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.517493 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.513041 | `azmcp_aks_cluster_list` | ❌ |
| 9 | 0.507465 | `azmcp_storage_account_list` | ❌ |
| 10 | 0.500768 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.494595 | `azmcp_group_list` | ❌ |
| 12 | 0.493730 | `azmcp_subscription_list` | ❌ |
| 13 | 0.487576 | `azmcp_functionapp_list` | ❌ |
| 14 | 0.487565 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.471758 | `azmcp_redis_cluster_list` | ❌ |
| 16 | 0.470266 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.467655 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.466748 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.448201 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.444214 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.585314 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.531083 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `azmcp_grafana_list` | ❌ |
| 5 | 0.462960 | `azmcp_monitor_workspace_log_query` | ❌ |
| 6 | 0.459841 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.398741 | `azmcp_search_service_list` | ❌ |
| 8 | 0.386422 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.384081 | `azmcp_search_index_list` | ❌ |
| 10 | 0.384002 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.381847 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 12 | 0.381694 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.379597 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.373786 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.371395 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.358029 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.354811 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.354276 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.352809 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.350239 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 159

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `azmcp_grafana_list` | ❌ |
| 3 | 0.580088 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.521316 | `azmcp_monitor_table_type_list` | ❌ |
| 5 | 0.500498 | `azmcp_search_service_list` | ❌ |
| 6 | 0.453702 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.449975 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.439297 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435475 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.428945 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.427230 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.422707 | `azmcp_subscription_list` | ❌ |
| 13 | 0.422379 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.422208 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.413155 | `azmcp_storage_table_list` | ❌ |
| 16 | 0.411648 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.411448 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.410082 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.404177 | `azmcp_group_list` | ❌ |
| 20 | 0.395576 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 160

**Expected Tool:** `azmcp_monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581663 | `azmcp_monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.492927 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.485793 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.484159 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.483323 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.427241 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.374939 | `azmcp_monitor_metrics_query` | ❌ |
| 8 | 0.365704 | `azmcp_grafana_list` | ❌ |
| 9 | 0.322408 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 10 | 0.322001 | `azmcp_search_index_list` | ❌ |
| 11 | 0.318833 | `azmcp_workbooks_delete` | ❌ |
| 12 | 0.309810 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.301564 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.300988 | `azmcp_search_service_list` | ❌ |
| 15 | 0.292300 | `azmcp_extension_az` | ❌ |
| 16 | 0.291669 | `azmcp_kusto_query` | ❌ |
| 17 | 0.288505 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.287253 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.284162 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.283294 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 161

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668827 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.434813 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.413173 | `azmcp_monitor_metrics_query` | ❌ |
| 4 | 0.408658 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.401731 | `azmcp_grafana_list` | ❌ |
| 6 | 0.393318 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.386685 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.369805 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.364360 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.356609 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.355415 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.345409 | `azmcp_postgres_database_list` | ❌ |
| 13 | 0.345298 | `azmcp_group_list` | ❌ |
| 14 | 0.330769 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.327205 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.318192 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.317478 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.306977 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.304097 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.302405 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 162

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443481 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.393227 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.374071 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.371017 | `azmcp_grafana_list` | ❌ |
| 6 | 0.370681 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.359274 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.350656 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.343214 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.342468 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.337094 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.319895 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.316979 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.302947 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.299934 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.285253 | `azmcp_group_list` | ❌ |
| 17 | 0.285004 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.274589 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.274464 | `azmcp_loadtesting_testrun_get` | ❌ |
| 20 | 0.270840 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 163

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `azmcp_quota_usage_check` | ❌ |
| 2 | 0.490730 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.476826 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 4 | 0.456851 | `azmcp_bestpractices_get` | ❌ |
| 5 | 0.442625 | `azmcp_extension_az` | ❌ |
| 6 | 0.440399 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.438387 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.431096 | `azmcp_deploy_iac_rules_get` | ❌ |
| 9 | 0.427495 | `azmcp_search_service_list` | ❌ |
| 10 | 0.423237 | `azmcp_subscription_list` | ❌ |
| 11 | 0.420585 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.408023 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.406591 | `azmcp_deploy_plan_get` | ❌ |
| 14 | 0.400363 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.388980 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.383400 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.380257 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.354341 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.351428 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.349661 | `azmcp_mysql_server_list` | ❌ |

---

## Test 164

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542880 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.496391 | `azmcp_bestpractices_get` | ❌ |
| 3 | 0.488377 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.474017 | `azmcp_extension_az` | ❌ |
| 5 | 0.473365 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.462743 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 7 | 0.448085 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.442021 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.439040 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.426161 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.385771 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.382470 | `azmcp_search_service_list` | ❌ |
| 13 | 0.375770 | `azmcp_subscription_list` | ❌ |
| 14 | 0.365824 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.359062 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.341827 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.338388 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.334341 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.333625 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.316612 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 165

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545136 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.504673 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.494872 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.491052 | `azmcp_bestpractices_get` | ❌ |
| 6 | 0.472526 | `azmcp_extension_az` | ❌ |
| 7 | 0.464304 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.463564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 9 | 0.463172 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.450091 | `azmcp_search_service_list` | ❌ |
| 11 | 0.433938 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.423512 | `azmcp_subscription_list` | ❌ |
| 13 | 0.417356 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.403533 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 15 | 0.398621 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.391476 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.374302 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.371590 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.370619 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.369448 | `azmcp_redis_cache_list` | ❌ |

---

## Test 166

**Expected Tool:** `azmcp_quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590878 | `azmcp_quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413274 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.372940 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.361386 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.349685 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.348742 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.337839 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.331159 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.331074 | `azmcp_monitor_metrics_definitions` | ❌ |
| 10 | 0.328408 | `azmcp_grafana_list` | ❌ |
| 11 | 0.325796 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.313240 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.310326 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.307143 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.290125 | `azmcp_group_list` | ❌ |
| 16 | 0.287104 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.271127 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.265329 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.264358 | `azmcp_postgres_server_list` | ❌ |
| 20 | 0.246956 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 167

**Expected Tool:** `azmcp_quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609244 | `azmcp_quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `azmcp_quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.380135 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.379029 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.365684 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.358215 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.345160 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.342231 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.338636 | `azmcp_grafana_list` | ❌ |
| 11 | 0.331262 | `azmcp_monitor_metrics_definitions` | ❌ |
| 12 | 0.330188 | `azmcp_storage_blob_container_details` | ❌ |
| 13 | 0.322571 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.321714 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.309805 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.305628 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.300710 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.300362 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.280386 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.279857 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 168

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.487393 | `azmcp_search_service_list` | ❌ |
| 3 | 0.483988 | `azmcp_group_list` | ❌ |
| 4 | 0.483125 | `azmcp_subscription_list` | ❌ |
| 5 | 0.478700 | `azmcp_grafana_list` | ❌ |
| 6 | 0.474796 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.471364 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.460029 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.452819 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.449008 | `azmcp_storage_account_list` | ❌ |
| 11 | 0.446372 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 12 | 0.441104 | `azmcp_functionapp_list` | ❌ |
| 13 | 0.430667 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.427950 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.426624 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.403310 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.397565 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.396564 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.374732 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.365611 | `azmcp_acr_registry_list` | ❌ |

---

## Test 169

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609705 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.456956 | `azmcp_grafana_list` | ❌ |
| 3 | 0.436747 | `azmcp_subscription_list` | ❌ |
| 4 | 0.435642 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.435287 | `azmcp_search_service_list` | ❌ |
| 6 | 0.435155 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.428663 | `azmcp_group_list` | ❌ |
| 8 | 0.428370 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.421627 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.420804 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.410380 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 12 | 0.406766 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.395445 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.387877 | `azmcp_functionapp_list` | ❌ |
| 15 | 0.386800 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.383635 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.373204 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.368511 | `azmcp_loadtesting_testresource_list` | ❌ |
| 19 | 0.353406 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.351866 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 170

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757057 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.565047 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.445073 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.377563 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.322930 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.312428 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.307404 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.303736 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.303531 | `azmcp_appconfig_kv_list` | ❌ |
| 10 | 0.303216 | `azmcp_storage_blob_container_details` | ❌ |
| 11 | 0.300024 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.298847 | `azmcp_storage_account_list` | ❌ |
| 13 | 0.298380 | `azmcp_keyvault_certificate_list` | ❌ |
| 14 | 0.296657 | `azmcp_keyvault_key_list` | ❌ |
| 15 | 0.289203 | `azmcp_storage_blob_container_list` | ❌ |
| 16 | 0.286490 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.284898 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.284304 | `azmcp_grafana_list` | ❌ |
| 19 | 0.284115 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.283849 | `azmcp_mysql_server_list` | ❌ |

---

## Test 171

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713839 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.523153 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.412377 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.338859 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.315295 | `azmcp_storage_blob_container_details` | ❌ |
| 6 | 0.286321 | `azmcp_appconfig_kv_list` | ❌ |
| 7 | 0.283725 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.280245 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.264504 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.258045 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.257957 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.257447 | `azmcp_mysql_server_config_get` | ❌ |
| 13 | 0.257151 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.253484 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.253169 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.249917 | `azmcp_extension_az` | ❌ |
| 17 | 0.247853 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.246871 | `azmcp_grafana_list` | ❌ |
| 19 | 0.246847 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.240600 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.764063 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.653924 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.501880 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.495048 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.472307 | `azmcp_grafana_list` | ❌ |
| 6 | 0.466141 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.464785 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.433313 | `azmcp_search_service_list` | ❌ |
| 9 | 0.431968 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.431715 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.423145 | `azmcp_subscription_list` | ❌ |
| 12 | 0.396295 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.393829 | `azmcp_storage_account_list` | ❌ |
| 14 | 0.381343 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.380844 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.373395 | `azmcp_group_list` | ❌ |
| 17 | 0.373274 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.368719 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.361464 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.354948 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 173

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537885 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.450387 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 3 | 0.441104 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.401235 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.302323 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.283598 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.276005 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.265858 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.262106 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.257556 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.252070 | `azmcp_grafana_list` | ❌ |
| 12 | 0.246445 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.236096 | `azmcp_postgres_table_list` | ❌ |
| 14 | 0.233781 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.231390 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.231294 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.225079 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.217990 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.212420 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.211175 | `azmcp_extension_az` | ❌ |

---

## Test 174

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692210 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.595721 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.461603 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.434924 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.427325 | `azmcp_grafana_list` | ❌ |
| 6 | 0.399303 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.383383 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.382294 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.368549 | `azmcp_search_service_list` | ❌ |
| 10 | 0.361735 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.353487 | `azmcp_subscription_list` | ❌ |
| 12 | 0.340764 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.327206 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.319474 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.316444 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.310802 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.306356 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.305932 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.304064 | `azmcp_group_list` | ❌ |
| 20 | 0.300259 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 175

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752919 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.603780 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.594994 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.548268 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.538403 | `azmcp_cosmos_database_list` | ❌ |
| 6 | 0.520914 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.471359 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.458244 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.456133 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.449548 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.419621 | `azmcp_postgres_table_list` | ❌ |
| 12 | 0.395416 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.390449 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.385544 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.379937 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.375714 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.366236 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.328081 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.324867 | `azmcp_grafana_list` | ❌ |
| 20 | 0.317852 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 176

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** Show me the databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.721506 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.562860 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.537788 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.490987 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.481618 | `azmcp_cosmos_database_list` | ❌ |
| 6 | 0.480274 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.434940 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.414701 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.408379 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.397285 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.369076 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.353712 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.351025 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.349880 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.343275 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 16 | 0.325303 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.318982 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.302228 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.294393 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.292180 | `azmcp_grafana_list` | ❌ |

---

## Test 177

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812960 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.679028 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.672104 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.588847 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.568902 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.554298 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.527406 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.503279 | `azmcp_grafana_list` | ❌ |
| 9 | 0.467957 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.463770 | `azmcp_search_service_list` | ❌ |
| 11 | 0.457600 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.455613 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.445496 | `azmcp_group_list` | ❌ |
| 14 | 0.445406 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.442886 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 16 | 0.439387 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.436494 | `azmcp_subscription_list` | ❌ |
| 18 | 0.423101 | `azmcp_storage_account_list` | ❌ |
| 19 | 0.419137 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.419075 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 178

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591593 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.514375 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.467519 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.403281 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.385069 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 6 | 0.368698 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.337915 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.329389 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.322157 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.321180 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.305874 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.301294 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.295045 | `azmcp_grafana_list` | ❌ |
| 14 | 0.291684 | `azmcp_postgres_database_list` | ❌ |
| 15 | 0.272504 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.261138 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.260993 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.259662 | `azmcp_postgres_server_config_get` | ❌ |
| 19 | 0.257012 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.252053 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 179

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me the Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.744239 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.607511 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.580866 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.518857 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.494170 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.491556 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.456252 | `azmcp_grafana_list` | ❌ |
| 8 | 0.446568 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.440660 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.400256 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 11 | 0.394530 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.394483 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.393490 | `azmcp_search_service_list` | ❌ |
| 14 | 0.389814 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.372221 | `azmcp_group_list` | ❌ |
| 16 | 0.370345 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.368926 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.367955 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.367096 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.362596 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 180

**Expected Tool:** `azmcp_group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755935 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.566552 | `azmcp_workbooks_list` | ❌ |
| 3 | 0.552633 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.546156 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.545480 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.542878 | `azmcp_grafana_list` | ❌ |
| 7 | 0.530600 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.524796 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.524265 | `azmcp_search_service_list` | ❌ |
| 10 | 0.518520 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.517060 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.500858 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.491176 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.486716 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.485348 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.479545 | `azmcp_subscription_list` | ❌ |
| 17 | 0.477887 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.476662 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.472171 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.460186 | `azmcp_functionapp_list` | ❌ |

---

## Test 181

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.463685 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.462472 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.459304 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.453960 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.429014 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.426935 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.407817 | `azmcp_grafana_list` | ❌ |
| 9 | 0.396822 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.391278 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.383058 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.379927 | `azmcp_acr_registry_repository_list` | ❌ |
| 13 | 0.373796 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.366273 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.360235 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.350999 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.345595 | `azmcp_redis_cluster_database_list` | ❌ |
| 18 | 0.328487 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.326534 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.325359 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 182

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665771 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.531920 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.523088 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.522911 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.518543 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.515905 | `azmcp_grafana_list` | ❌ |
| 8 | 0.492945 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.487780 | `azmcp_acr_registry_list` | ❌ |
| 10 | 0.475313 | `azmcp_search_service_list` | ❌ |
| 11 | 0.470658 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.464637 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.460412 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.454794 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.451877 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.437342 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.435360 | `azmcp_subscription_list` | ❌ |
| 18 | 0.432994 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.429798 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.429564 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 183

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629120 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377586 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.349980 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.334270 | `azmcp_functionapp_get` | ❌ |
| 6 | 0.331563 | `azmcp_monitor_metrics_definitions` | ❌ |
| 7 | 0.330170 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.327691 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.324331 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.322117 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.316198 | `azmcp_storage_blob_container_details` | ❌ |
| 12 | 0.311644 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.308238 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.306616 | `azmcp_grafana_list` | ❌ |
| 15 | 0.290757 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.286482 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 17 | 0.283436 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.281508 | `azmcp_workbooks_list` | ❌ |
| 19 | 0.272387 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.272207 | `azmcp_group_list` | ❌ |

---

## Test 184

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546066 | `azmcp_storage_account_details` | ❌ |
| 2 | 0.537060 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.522807 | `azmcp_storage_account_list` | ❌ |
| 4 | 0.492853 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.489273 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 6 | 0.483366 | `azmcp_storage_blob_container_details` | ❌ |
| 7 | 0.470855 | `azmcp_storage_blob_list` | ❌ |
| 8 | 0.466885 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.413656 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.411283 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.405847 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.403899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.375351 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.368262 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.349407 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.321704 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.319566 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.311399 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.306746 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.304562 | `azmcp_functionapp_list` | ❌ |

---

## Test 185

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 2 | 0.573290 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424960 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.393479 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.386598 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.373883 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.371898 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.348148 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 9 | 0.321304 | `azmcp_group_list` | ❌ |
| 10 | 0.318379 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.318319 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.307076 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.304604 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.302450 | `azmcp_functionapp_list` | ❌ |
| 15 | 0.300392 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 16 | 0.299346 | `azmcp_monitor_metrics_query` | ❌ |
| 17 | 0.298723 | `azmcp_monitor_metrics_definitions` | ❌ |
| 18 | 0.294197 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.289170 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.283955 | `azmcp_acr_registry_list` | ❌ |

---

## Test 186

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.587526 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.578620 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.563455 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.548549 | `azmcp_grafana_list` | ❌ |
| 6 | 0.540583 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.531356 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.530985 | `azmcp_group_list` | ❌ |
| 9 | 0.530242 | `azmcp_search_service_list` | ❌ |
| 10 | 0.508580 | `azmcp_storage_account_list` | ❌ |
| 11 | 0.507740 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.496651 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.491394 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.491359 | `azmcp_subscription_list` | ❌ |
| 15 | 0.484221 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.482623 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.476832 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.471663 | `azmcp_functionapp_list` | ❌ |
| 19 | 0.465850 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.457237 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 187

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.582272 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.508252 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.473905 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.441533 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.409363 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.406709 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.406408 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.405790 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.405059 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.401056 | `azmcp_functionapp_list` | ❌ |
| 12 | 0.400948 | `azmcp_search_service_list` | ❌ |
| 13 | 0.400553 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.400427 | `azmcp_extension_az` | ❌ |
| 15 | 0.400033 | `azmcp_subscription_list` | ❌ |
| 16 | 0.399782 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.397368 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.387835 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.386810 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 20 | 0.386281 | `azmcp_bestpractices_get` | ❌ |

---

## Test 188

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.536904 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.427638 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.420501 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.411111 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.374184 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.370961 | `azmcp_loadtesting_testresource_list` | ❌ |
| 8 | 0.363808 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.360039 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.350454 | `azmcp_group_list` | ❌ |
| 11 | 0.348923 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.334774 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.332889 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 14 | 0.330185 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.328560 | `azmcp_extension_az` | ❌ |
| 16 | 0.321393 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.319481 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.317434 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.316540 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.309414 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 189

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `azmcp_servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.400870 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.380512 | `azmcp_storage_queue_message_send` | ❌ |
| 5 | 0.375386 | `azmcp_aks_cluster_get` | ❌ |
| 6 | 0.352180 | `azmcp_storage_blob_container_details` | ❌ |
| 7 | 0.338738 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.337239 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.323046 | `azmcp_kusto_cluster_get` | ❌ |
| 10 | 0.310924 | `azmcp_search_index_list` | ❌ |
| 11 | 0.310612 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.308567 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.306552 | `azmcp_storage_account_details` | ❌ |
| 14 | 0.302027 | `azmcp_functionapp_get` | ❌ |
| 15 | 0.301249 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.298514 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.279044 | `azmcp_functionapp_list` | ❌ |
| 18 | 0.278090 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.271662 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.266686 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 190

**Expected Tool:** `azmcp_servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591649 | `azmcp_servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571861 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.483976 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.361354 | `azmcp_aks_cluster_get` | ❌ |
| 5 | 0.347044 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.340036 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.335558 | `azmcp_kusto_cluster_get` | ❌ |
| 8 | 0.324869 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.319285 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.315561 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.308061 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.306601 | `azmcp_search_index_list` | ❌ |
| 13 | 0.305931 | `azmcp_storage_blob_container_details` | ❌ |
| 14 | 0.303980 | `azmcp_search_service_list` | ❌ |
| 15 | 0.303663 | `azmcp_storage_table_list` | ❌ |
| 16 | 0.297323 | `azmcp_grafana_list` | ❌ |
| 17 | 0.295563 | `azmcp_functionapp_list` | ❌ |
| 18 | 0.294371 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.290383 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.278717 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 191

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `azmcp_servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.494515 | `azmcp_servicebus_queue_details` | ❌ |
| 3 | 0.457036 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.449818 | `azmcp_search_service_list` | ❌ |
| 5 | 0.429458 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.426573 | `azmcp_kusto_cluster_get` | ❌ |
| 7 | 0.421009 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.411027 | `azmcp_aks_cluster_list` | ❌ |
| 9 | 0.406135 | `azmcp_functionapp_list` | ❌ |
| 10 | 0.404739 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.396053 | `azmcp_marketplace_product_get` | ❌ |
| 12 | 0.395176 | `azmcp_grafana_list` | ❌ |
| 13 | 0.388049 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.385222 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.374364 | `azmcp_subscription_list` | ❌ |
| 16 | 0.369986 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.368411 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.368155 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367649 | `azmcp_group_list` | ❌ |
| 20 | 0.358070 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 192

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643255 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639732 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.609157 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.602959 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.529086 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.527947 | `azmcp_kusto_database_list` | ❌ |
| 7 | 0.486695 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.482770 | `azmcp_sql_elastic-pool_list` | ❌ |
| 9 | 0.474945 | `azmcp_redis_cluster_database_list` | ❌ |
| 10 | 0.466165 | `azmcp_storage_table_list` | ❌ |
| 11 | 0.464589 | `azmcp_sql_db_show` | ❌ |
| 12 | 0.457373 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.457253 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.441383 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.440568 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.400486 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.395098 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.394563 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.382766 | `azmcp_functionapp_list` | ❌ |
| 20 | 0.380416 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 193

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609322 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.557353 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.553488 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.524274 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.471862 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.461650 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.458742 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.454316 | `azmcp_sql_elastic-pool_list` | ❌ |
| 9 | 0.445291 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.433140 | `azmcp_mysql_table_schema_get` | ❌ |
| 11 | 0.425707 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.387645 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.380428 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.359065 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.349880 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.347075 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.345262 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.342792 | `azmcp_appconfig_kv_list` | ❌ |
| 19 | 0.342284 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.341681 | `azmcp_kusto_table_list` | ❌ |

---

## Test 194

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593150 | `azmcp_postgres_server_config_get` | ❌ |
| 2 | 0.530422 | `azmcp_mysql_server_config_get` | ❌ |
| 3 | 0.528136 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 4 | 0.465693 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.446682 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.438925 | `azmcp_mysql_server_param_get` | ❌ |
| 7 | 0.398181 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.397510 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.374073 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.371766 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.354111 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 12 | 0.325945 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.320054 | `azmcp_aks_cluster_get` | ❌ |
| 14 | 0.304960 | `azmcp_functionapp_get` | ❌ |
| 15 | 0.297839 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.294987 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.273566 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.273315 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.263979 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.260930 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 195

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530095 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.440073 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.438622 | `azmcp_mysql_table_schema_get` | ❌ |
| 4 | 0.432919 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.421862 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.400963 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.398714 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.375668 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.361500 | `azmcp_redis_cluster_database_list` | ❌ |
| 10 | 0.357597 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.344694 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.337996 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.323587 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.300133 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.299814 | `azmcp_aks_cluster_get` | ❌ |
| 16 | 0.296827 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.294910 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.285843 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.268498 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.261790 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 196

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686435 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.498367 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.458302 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.454426 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.450792 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.434570 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.431871 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 9 | 0.431174 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.416312 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.414738 | `azmcp_postgres_database_list` | ❌ |
| 12 | 0.394337 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.370652 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.363579 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.357347 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.351647 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.351058 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.349479 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.348568 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.346054 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 197

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.616579 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.457163 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.432816 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.423047 | `azmcp_mysql_server_config_get` | ❌ |
| 5 | 0.419802 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.400026 | `azmcp_mysql_server_param_get` | ❌ |
| 7 | 0.389072 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 8 | 0.385834 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 9 | 0.378556 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.372423 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.335615 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.333099 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.319924 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.304600 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.304317 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.298907 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.298264 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.297857 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.293905 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.277055 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 198

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602478 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420325 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.402608 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.397670 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.378527 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.367742 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.357516 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.344799 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.344468 | `azmcp_mysql_server_param_get` | ❌ |
| 10 | 0.322365 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 11 | 0.321778 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.298933 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.292566 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.284157 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.281680 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.272025 | `azmcp_monitor_metrics_definitions` | ❌ |
| 17 | 0.259325 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.256675 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.252882 | `azmcp_foundry_models_deployments_list` | ❌ |
| 20 | 0.249936 | `azmcp_extension_az` | ❌ |

---

## Test 199

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.788356 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.407432 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.376055 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.365636 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.352607 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.344513 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.343559 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.328968 | `azmcp_sql_elastic-pool_list` | ❌ |
| 9 | 0.328737 | `azmcp_role_assignment_list` | ❌ |
| 10 | 0.312627 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.280450 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.258095 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.249297 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.249153 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.246563 | `azmcp_keyvault_secret_list` | ❌ |
| 16 | 0.245267 | `azmcp_group_list` | ❌ |
| 17 | 0.238150 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.234681 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.233337 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.232633 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 200

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718251 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.315966 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.311085 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.309059 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.303560 | `azmcp_postgres_server_config_get` | ❌ |
| 6 | 0.287372 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.283806 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.273998 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.268897 | `azmcp_sql_elastic-pool_list` | ❌ |
| 10 | 0.266264 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.214529 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.197679 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.194313 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.193050 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.191538 | `azmcp_appconfig_kv_list` | ❌ |
| 16 | 0.188120 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.186088 | `azmcp_loadtesting_testrun_list` | ❌ |
| 18 | 0.183184 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 19 | 0.182322 | `azmcp_extension_az` | ❌ |
| 20 | 0.182237 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 201

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.651306 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.253610 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.244772 | `azmcp_extension_az` | ❌ |
| 4 | 0.236850 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.236089 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.229560 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.228093 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 8 | 0.221683 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.217899 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.217698 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.212644 | `azmcp_cloudarchitect_design` | ❌ |
| 12 | 0.205654 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.189941 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.188287 | `azmcp_deploy_plan_get` | ❌ |
| 15 | 0.180995 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.180555 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.174553 | `azmcp_deploy_iac_rules_get` | ❌ |
| 18 | 0.173222 | `azmcp_bestpractices_get` | ❌ |
| 19 | 0.169345 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.165162 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 202

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732275 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.397092 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 3 | 0.385148 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.359228 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.356706 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.355203 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.350241 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.347004 | `azmcp_sql_elastic-pool_list` | ❌ |
| 9 | 0.327808 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.325404 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.304958 | `azmcp_keyvault_secret_list` | ❌ |
| 12 | 0.278098 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.277844 | `azmcp_functionapp_list` | ❌ |
| 14 | 0.277410 | `azmcp_keyvault_key_list` | ❌ |
| 15 | 0.276828 | `azmcp_keyvault_certificate_list` | ❌ |
| 16 | 0.270667 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.263181 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.259932 | `azmcp_extension_az` | ❌ |
| 19 | 0.253852 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.248780 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 203

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.631499 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.321414 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 3 | 0.312035 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.298995 | `azmcp_mysql_server_param_get` | ❌ |
| 5 | 0.294466 | `azmcp_mysql_server_config_get` | ❌ |
| 6 | 0.293358 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.290374 | `azmcp_extension_az` | ❌ |
| 8 | 0.290235 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.289766 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.287747 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.280898 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.225372 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.210531 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.206761 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.206476 | `azmcp_deploy_iac_rules_get` | ❌ |
| 16 | 0.206114 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.197711 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.194886 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 19 | 0.189871 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.189786 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 204

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633622 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.311867 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 3 | 0.305701 | `azmcp_mysql_server_param_get` | ❌ |
| 4 | 0.304314 | `azmcp_mysql_server_config_get` | ❌ |
| 5 | 0.299474 | `azmcp_extension_az` | ❌ |
| 6 | 0.277628 | `azmcp_postgres_server_config_get` | ❌ |
| 7 | 0.273256 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.262028 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.261404 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.261123 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.258402 | `azmcp_sql_elastic-pool_list` | ❌ |
| 12 | 0.202425 | `azmcp_deploy_iac_rules_get` | ❌ |
| 13 | 0.200326 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.191165 | `azmcp_cloudarchitect_design` | ❌ |
| 15 | 0.186128 | `azmcp_loadtesting_test_get` | ❌ |
| 16 | 0.181118 | `azmcp_bestpractices_get` | ❌ |
| 17 | 0.179898 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 18 | 0.171335 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.167153 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.162949 | `azmcp_functionapp_list` | ❌ |

---

## Test 205

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.523668 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.428712 | `azmcp_storage_account_details` | ❌ |
| 3 | 0.414467 | `azmcp_storage_account_list` | ❌ |
| 4 | 0.407725 | `azmcp_storage_blob_container_list` | ❌ |
| 5 | 0.391586 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.374006 | `azmcp_loadtesting_test_create` | ❌ |
| 7 | 0.355049 | `azmcp_loadtesting_testresource_create` | ❌ |
| 8 | 0.329396 | `azmcp_storage_blob_list` | ❌ |
| 9 | 0.328695 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 10 | 0.325832 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.323501 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.319843 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.315241 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.311850 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.305188 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.298887 | `azmcp_storage_datalake_directory_create` | ❌ |
| 17 | 0.297236 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.289742 | `azmcp_appconfig_kv_show` | ❌ |
| 19 | 0.277805 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.267474 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 206

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488836 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.407891 | `azmcp_storage_account_list` | ❌ |
| 3 | 0.402128 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 4 | 0.401456 | `azmcp_storage_account_details` | ❌ |
| 5 | 0.382836 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.365288 | `azmcp_storage_blob_container_list` | ❌ |
| 7 | 0.361412 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.351340 | `azmcp_storage_blob_container_details` | ❌ |
| 9 | 0.344343 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.329099 | `azmcp_loadtesting_test_create` | ❌ |
| 11 | 0.319617 | `azmcp_storage_blob_list` | ❌ |
| 12 | 0.315053 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.310332 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.302787 | `azmcp_extension_az` | ❌ |
| 15 | 0.284467 | `azmcp_deploy_plan_get` | ❌ |
| 16 | 0.284385 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.283067 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 18 | 0.281142 | `azmcp_appconfig_kv_lock` | ❌ |
| 19 | 0.280404 | `azmcp_keyvault_certificate_create` | ❌ |
| 20 | 0.280192 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 207

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587691 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.453252 | `azmcp_storage_account_details` | ❌ |
| 3 | 0.444359 | `azmcp_storage_datalake_directory_create` | ❌ |
| 4 | 0.427132 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.426585 | `azmcp_storage_blob_container_list` | ❌ |
| 6 | 0.393086 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.386262 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.384865 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 9 | 0.383927 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.383328 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 11 | 0.382274 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.380638 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.380503 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.379380 | `azmcp_storage_blob_container_details` | ❌ |
| 15 | 0.372357 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.366696 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.359261 | `azmcp_keyvault_secret_create` | ❌ |
| 18 | 0.321846 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.309241 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.302875 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 208

**Expected Tool:** `azmcp_storage_account_details`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639321 | `azmcp_storage_account_details` | ✅ **EXPECTED** |
| 2 | 0.539057 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.532291 | `azmcp_storage_blob_container_details` | ❌ |
| 4 | 0.507678 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.504386 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.476782 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.448674 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.442858 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.439236 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.431020 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.403478 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.397051 | `azmcp_mysql_server_config_get` | ❌ |
| 13 | 0.395698 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.388425 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.371133 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 16 | 0.367049 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.361551 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.356973 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.353431 | `azmcp_loadtesting_testrun_get` | ❌ |
| 20 | 0.341278 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 209

**Expected Tool:** `azmcp_storage_account_details`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669635 | `azmcp_storage_account_details` | ✅ **EXPECTED** |
| 2 | 0.549844 | `azmcp_storage_blob_container_details` | ❌ |
| 3 | 0.522945 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.501487 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.483947 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.481395 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.455377 | `azmcp_storage_blob_list` | ❌ |
| 8 | 0.415410 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.411808 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.401802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.375790 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.373470 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.369755 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.368023 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.367316 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.365451 | `azmcp_storage_blob_details` | ❌ |
| 17 | 0.362602 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.340406 | `azmcp_appconfig_kv_lock` | ❌ |
| 19 | 0.337077 | `azmcp_loadtesting_testrun_get` | ❌ |
| 20 | 0.330558 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 210

**Expected Tool:** `azmcp_storage_account_list`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.703877 | `azmcp_storage_account_list` | ✅ **EXPECTED** |
| 2 | 0.581393 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.576347 | `azmcp_storage_account_details` | ❌ |
| 4 | 0.544687 | `azmcp_storage_blob_container_list` | ❌ |
| 5 | 0.536909 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.501088 | `azmcp_subscription_list` | ❌ |
| 7 | 0.498502 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.496371 | `azmcp_quota_region_availability_list` | ❌ |
| 9 | 0.493312 | `azmcp_storage_blob_list` | ❌ |
| 10 | 0.493246 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.484163 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.471507 | `azmcp_search_service_list` | ❌ |
| 13 | 0.459550 | `azmcp_functionapp_list` | ❌ |
| 14 | 0.458793 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.454195 | `azmcp_acr_registry_list` | ❌ |
| 16 | 0.448903 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.432645 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.416387 | `azmcp_group_list` | ❌ |
| 19 | 0.412679 | `azmcp_grafana_list` | ❌ |
| 20 | 0.393518 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 211

**Expected Tool:** `azmcp_storage_account_list`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571003 | `azmcp_storage_account_list` | ✅ **EXPECTED** |
| 2 | 0.498231 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.461284 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 4 | 0.450677 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.448981 | `azmcp_storage_account_details` | ❌ |
| 6 | 0.421642 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.421051 | `azmcp_storage_blob_container_details` | ❌ |
| 8 | 0.417652 | `azmcp_storage_blob_list` | ❌ |
| 9 | 0.409447 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 10 | 0.379853 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.375553 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.367906 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.366021 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.347173 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.346039 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.335306 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.331743 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.327814 | `azmcp_functionapp_list` | ❌ |
| 19 | 0.322108 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.312384 | `azmcp_acr_registry_list` | ❌ |

---

## Test 212

**Expected Tool:** `azmcp_storage_account_list`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608046 | `azmcp_storage_account_list` | ✅ **EXPECTED** |
| 2 | 0.502780 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.499153 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.484101 | `azmcp_storage_account_details` | ❌ |
| 5 | 0.478874 | `azmcp_storage_blob_list` | ❌ |
| 6 | 0.473598 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.453933 | `azmcp_subscription_list` | ❌ |
| 8 | 0.439843 | `azmcp_storage_blob_container_details` | ❌ |
| 9 | 0.432854 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.425048 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.418264 | `azmcp_search_service_list` | ❌ |
| 12 | 0.415080 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.384505 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.384035 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.383014 | `azmcp_functionapp_list` | ❌ |
| 16 | 0.376660 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.359998 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.359053 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.353273 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.341127 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 213

**Expected Tool:** `azmcp_storage_blob_batch_set-tier`  
**Prompt:** Set access tier to Cool for multiple blobs in the container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680512 | `azmcp_storage_blob_batch_set-tier` | ✅ **EXPECTED** |
| 2 | 0.481334 | `azmcp_storage_blob_list` | ❌ |
| 3 | 0.459489 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.431856 | `azmcp_storage_blob_container_details` | ❌ |
| 5 | 0.422570 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.378380 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.370056 | `azmcp_storage_account_list` | ❌ |
| 8 | 0.352885 | `azmcp_storage_account_details` | ❌ |
| 9 | 0.330453 | `azmcp_storage_blob_upload` | ❌ |
| 10 | 0.325500 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.324757 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.305741 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.297254 | `azmcp_appconfig_kv_lock` | ❌ |
| 14 | 0.295717 | `azmcp_appconfig_kv_unlock` | ❌ |
| 15 | 0.295532 | `azmcp_appconfig_kv_set` | ❌ |
| 16 | 0.295133 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.286940 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.285276 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.271887 | `azmcp_appconfig_kv_show` | ❌ |
| 20 | 0.255608 | `azmcp_extension_az` | ❌ |

---

## Test 214

**Expected Tool:** `azmcp_storage_blob_batch_set-tier`  
**Prompt:** Change the access tier to Archive for blobs file1.txt and file2.txt in the container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.616053 | `azmcp_storage_blob_batch_set-tier` | ✅ **EXPECTED** |
| 2 | 0.436141 | `azmcp_storage_blob_list` | ❌ |
| 3 | 0.426142 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.407878 | `azmcp_storage_blob_container_details` | ❌ |
| 5 | 0.364903 | `azmcp_storage_account_list` | ❌ |
| 6 | 0.360883 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.351892 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.351742 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.346809 | `azmcp_storage_account_details` | ❌ |
| 10 | 0.341857 | `azmcp_storage_table_list` | ❌ |
| 11 | 0.324606 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.301044 | `azmcp_acr_registry_repository_list` | ❌ |
| 13 | 0.294760 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.293979 | `azmcp_appconfig_kv_lock` | ❌ |
| 15 | 0.280305 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.279325 | `azmcp_extension_az` | ❌ |
| 17 | 0.276719 | `azmcp_appconfig_kv_unlock` | ❌ |
| 18 | 0.267309 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.253981 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.246907 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 215

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570598 | `azmcp_storage_blob_container_list` | ❌ |
| 2 | 0.512580 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.479936 | `azmcp_storage_blob_list` | ❌ |
| 4 | 0.451589 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 5 | 0.447784 | `azmcp_cosmos_database_container_list` | ❌ |
| 6 | 0.439592 | `azmcp_storage_blob_container_details` | ❌ |
| 7 | 0.420577 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.398696 | `azmcp_storage_account_list` | ❌ |
| 9 | 0.387848 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.335039 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.326352 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.323214 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.320470 | `azmcp_storage_datalake_directory_create` | ❌ |
| 14 | 0.318855 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.312374 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 16 | 0.305680 | `azmcp_keyvault_certificate_create` | ❌ |
| 17 | 0.297912 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 18 | 0.297384 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.292093 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.291137 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 216

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584046 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.515864 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.509983 | `azmcp_storage_blob_list` | ❌ |
| 4 | 0.467593 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.458805 | `azmcp_storage_blob_container_details` | ❌ |
| 6 | 0.415378 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.392842 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 8 | 0.381938 | `azmcp_storage_account_list` | ❌ |
| 9 | 0.380296 | `azmcp_storage_account_details` | ❌ |
| 10 | 0.355806 | `azmcp_storage_blob_upload` | ❌ |
| 11 | 0.354861 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.320173 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 13 | 0.309739 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.296899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.285153 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.277972 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.275240 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.275199 | `azmcp_keyvault_key_create` | ❌ |
| 19 | 0.270167 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.269625 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 217

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512912 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.477598 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.471483 | `azmcp_storage_blob_list` | ❌ |
| 4 | 0.435099 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.434202 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.410487 | `azmcp_storage_blob_container_details` | ❌ |
| 7 | 0.380062 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.378021 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.375383 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.361007 | `azmcp_storage_account_list` | ❌ |
| 11 | 0.355181 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 12 | 0.333699 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.329038 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.322364 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.280806 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.276533 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.275559 | `azmcp_keyvault_secret_create` | ❌ |
| 18 | 0.269719 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.266791 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.265228 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 218

**Expected Tool:** `azmcp_storage_blob_container_details`  
**Prompt:** Show me the properties of the storage container files in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648757 | `azmcp_storage_blob_container_list` | ❌ |
| 2 | 0.638696 | `azmcp_storage_blob_container_details` | ✅ **EXPECTED** |
| 3 | 0.590054 | `azmcp_storage_account_details` | ❌ |
| 4 | 0.582066 | `azmcp_storage_blob_list` | ❌ |
| 5 | 0.519124 | `azmcp_storage_account_list` | ❌ |
| 6 | 0.518490 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.496349 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.467995 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.447238 | `azmcp_storage_blob_details` | ❌ |
| 10 | 0.445063 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.433692 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 12 | 0.419634 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.418341 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.387558 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.365807 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.357446 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.355076 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.342947 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.335755 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.334143 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 219

**Expected Tool:** `azmcp_storage_blob_container_list`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728948 | `azmcp_storage_blob_container_list` | ✅ **EXPECTED** |
| 2 | 0.708548 | `azmcp_storage_blob_list` | ❌ |
| 3 | 0.613933 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.557079 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.530702 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.471385 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.466796 | `azmcp_storage_blob_container_details` | ❌ |
| 8 | 0.456826 | `azmcp_storage_account_details` | ❌ |
| 9 | 0.453044 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.431093 | `azmcp_storage_blob_container_create` | ❌ |
| 11 | 0.409820 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.401486 | `azmcp_storage_blob_details` | ❌ |
| 13 | 0.386144 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.369372 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.368419 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 16 | 0.367207 | `azmcp_keyvault_key_list` | ❌ |
| 17 | 0.356400 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.351601 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.351458 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.348288 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 220

**Expected Tool:** `azmcp_storage_blob_container_list`  
**Prompt:** Show me the blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669501 | `azmcp_storage_blob_container_list` | ✅ **EXPECTED** |
| 2 | 0.639897 | `azmcp_storage_blob_list` | ❌ |
| 3 | 0.561214 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.513201 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.496616 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.473405 | `azmcp_storage_account_details` | ❌ |
| 7 | 0.459196 | `azmcp_storage_blob_container_details` | ❌ |
| 8 | 0.446475 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.430796 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.417912 | `azmcp_storage_blob_details` | ❌ |
| 11 | 0.406810 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.399338 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.392982 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.373746 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 15 | 0.372190 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.340048 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.338712 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.332977 | `azmcp_appconfig_kv_show` | ❌ |
| 19 | 0.321478 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.321463 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 221

**Expected Tool:** `azmcp_storage_blob_details`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629565 | `azmcp_storage_blob_list` | ❌ |
| 2 | 0.609436 | `azmcp_storage_blob_container_details` | ❌ |
| 3 | 0.579203 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.573316 | `azmcp_storage_blob_details` | ✅ **EXPECTED** |
| 5 | 0.509477 | `azmcp_storage_account_details` | ❌ |
| 6 | 0.477946 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.463019 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.436018 | `azmcp_storage_account_list` | ❌ |
| 9 | 0.431571 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.420748 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.386482 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.370995 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.364055 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.359392 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.349565 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.323065 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.318346 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.303596 | `azmcp_appconfig_kv_list` | ❌ |
| 19 | 0.287289 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.283098 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 222

**Expected Tool:** `azmcp_storage_blob_details`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620291 | `azmcp_storage_blob_list` | ❌ |
| 2 | 0.604540 | `azmcp_storage_blob_container_details` | ❌ |
| 3 | 0.566941 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.548693 | `azmcp_storage_account_details` | ❌ |
| 5 | 0.503906 | `azmcp_storage_blob_details` | ✅ **EXPECTED** |
| 6 | 0.469307 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.453696 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.434361 | `azmcp_storage_blob_upload` | ❌ |
| 9 | 0.414040 | `azmcp_storage_account_list` | ❌ |
| 10 | 0.406597 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.388809 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.370177 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.360712 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.359655 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.353461 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.348551 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.327201 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.320142 | `azmcp_keyvault_certificate_get` | ❌ |
| 19 | 0.319393 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.313425 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 223

**Expected Tool:** `azmcp_storage_blob_list`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.767056 | `azmcp_storage_blob_list` | ✅ **EXPECTED** |
| 2 | 0.686405 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.579070 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.532659 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.507970 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.469189 | `azmcp_storage_blob_container_details` | ❌ |
| 7 | 0.454766 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.452160 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.434416 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.415853 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.411817 | `azmcp_storage_blob_details` | ❌ |
| 12 | 0.400483 | `azmcp_acr_registry_repository_list` | ❌ |
| 13 | 0.384284 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.379851 | `azmcp_keyvault_key_list` | ❌ |
| 15 | 0.379099 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.372423 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 17 | 0.369535 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.361689 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.359099 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.331545 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 224

**Expected Tool:** `azmcp_storage_blob_list`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671820 | `azmcp_storage_blob_list` | ✅ **EXPECTED** |
| 2 | 0.617974 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.533515 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.470516 | `azmcp_storage_blob_container_details` | ❌ |
| 5 | 0.456071 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.450705 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.448536 | `azmcp_storage_account_list` | ❌ |
| 8 | 0.447308 | `azmcp_storage_account_details` | ❌ |
| 9 | 0.430213 | `azmcp_storage_blob_details` | ❌ |
| 10 | 0.395809 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.385242 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.363261 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.362337 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.361996 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 15 | 0.353799 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.345263 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.342766 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.339846 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.300295 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.291436 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 225

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591091 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.481573 | `azmcp_storage_blob_list` | ❌ |
| 3 | 0.436185 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.373767 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 5 | 0.368768 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.360680 | `azmcp_storage_account_details` | ❌ |
| 7 | 0.347016 | `azmcp_storage_blob_container_details` | ❌ |
| 8 | 0.333446 | `azmcp_storage_account_list` | ❌ |
| 9 | 0.328962 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.327416 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.324049 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.311732 | `azmcp_storage_blob_details` | ❌ |
| 13 | 0.294744 | `azmcp_keyvault_certificate_import` | ❌ |
| 14 | 0.284896 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.277659 | `azmcp_appconfig_kv_lock` | ❌ |
| 16 | 0.273638 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.273513 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.257845 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.253430 | `azmcp_appconfig_kv_show` | ❌ |
| 20 | 0.239522 | `azmcp_foundry_models_deploy` | ❌ |

---

## Test 226

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload the file <local-file-path> overwriting blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636116 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.446813 | `azmcp_storage_blob_list` | ❌ |
| 3 | 0.396643 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.387138 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 5 | 0.335857 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.328860 | `azmcp_storage_blob_container_details` | ❌ |
| 7 | 0.328190 | `azmcp_storage_blob_details` | ❌ |
| 8 | 0.326193 | `azmcp_storage_account_details` | ❌ |
| 9 | 0.315356 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.307996 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.294974 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.289572 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.283145 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.280881 | `azmcp_storage_account_list` | ❌ |
| 15 | 0.262470 | `azmcp_appconfig_kv_lock` | ❌ |
| 16 | 0.253320 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.248237 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.241726 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.231355 | `azmcp_appconfig_kv_unlock` | ❌ |
| 20 | 0.223204 | `azmcp_appconfig_kv_delete` | ❌ |

---

## Test 227

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Overwrite <blob> with <local-file-name> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.561515 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.455562 | `azmcp_storage_blob_list` | ❌ |
| 3 | 0.417312 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.385132 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 5 | 0.356701 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.354464 | `azmcp_storage_blob_container_details` | ❌ |
| 7 | 0.337105 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.327976 | `azmcp_storage_blob_details` | ❌ |
| 9 | 0.317867 | `azmcp_storage_account_details` | ❌ |
| 10 | 0.317821 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.298033 | `azmcp_appconfig_kv_lock` | ❌ |
| 12 | 0.285479 | `azmcp_workbooks_delete` | ❌ |
| 13 | 0.284797 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.274924 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.258484 | `azmcp_appconfig_kv_unlock` | ❌ |
| 16 | 0.252844 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.250215 | `azmcp_keyvault_certificate_import` | ❌ |
| 18 | 0.244022 | `azmcp_appconfig_kv_delete` | ❌ |
| 19 | 0.243240 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.227445 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 228

**Expected Tool:** `azmcp_storage_datalake_directory_create`  
**Prompt:** Create a new directory at the path <directory_path> in Data Lake in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647078 | `azmcp_storage_datalake_directory_create` | ✅ **EXPECTED** |
| 2 | 0.477075 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 3 | 0.441547 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.348401 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.340453 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.337665 | `azmcp_storage_blob_container_list` | ❌ |
| 7 | 0.333862 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.326548 | `azmcp_storage_account_details` | ❌ |
| 9 | 0.303932 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.302849 | `azmcp_loadtesting_testresource_create` | ❌ |
| 11 | 0.297012 | `azmcp_loadtesting_test_create` | ❌ |
| 12 | 0.287500 | `azmcp_storage_account_list` | ❌ |
| 13 | 0.281674 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.278405 | `azmcp_storage_blob_list` | ❌ |
| 15 | 0.278175 | `azmcp_storage_blob_upload` | ❌ |
| 16 | 0.276608 | `azmcp_appconfig_kv_set` | ❌ |
| 17 | 0.272610 | `azmcp_workbooks_create` | ❌ |
| 18 | 0.249193 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.240515 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.236486 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 229

**Expected Tool:** `azmcp_storage_datalake_file-system_list-paths`  
**Prompt:** List all paths in the Data Lake file system <file_system> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769814 | `azmcp_storage_datalake_file-system_list-paths` | ✅ **EXPECTED** |
| 2 | 0.506115 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.485536 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.481743 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.467351 | `azmcp_storage_account_list` | ❌ |
| 6 | 0.463837 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.451626 | `azmcp_storage_datalake_directory_create` | ❌ |
| 8 | 0.420884 | `azmcp_storage_share_file_list` | ❌ |
| 9 | 0.419381 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.403391 | `azmcp_storage_account_details` | ❌ |
| 11 | 0.402145 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.390655 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.384327 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.374721 | `azmcp_keyvault_key_list` | ❌ |
| 15 | 0.357960 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.346694 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.344646 | `azmcp_functionapp_list` | ❌ |
| 18 | 0.344288 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.337104 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.333592 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 230

**Expected Tool:** `azmcp_storage_datalake_file-system_list-paths`  
**Prompt:** Show me the paths in the Data Lake file system <file_system> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727958 | `azmcp_storage_datalake_file-system_list-paths` | ✅ **EXPECTED** |
| 2 | 0.502758 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.439610 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.433508 | `azmcp_storage_datalake_directory_create` | ❌ |
| 5 | 0.431934 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.411061 | `azmcp_storage_account_list` | ❌ |
| 7 | 0.403353 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.402364 | `azmcp_storage_blob_list` | ❌ |
| 9 | 0.383952 | `azmcp_storage_share_file_list` | ❌ |
| 10 | 0.372366 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.347561 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.345807 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.344736 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.337201 | `azmcp_storage_blob_container_details` | ❌ |
| 15 | 0.304731 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.304377 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.304375 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.289433 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.288460 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.280376 | `azmcp_functionapp_list` | ❌ |

---

## Test 231

**Expected Tool:** `azmcp_storage_datalake_file-system_list-paths`  
**Prompt:** Recursively list all paths in the Data Lake file system <file_system> in the storage account <account> filtered by <filter_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686433 | `azmcp_storage_datalake_file-system_list-paths` | ✅ **EXPECTED** |
| 2 | 0.465035 | `azmcp_storage_share_file_list` | ❌ |
| 3 | 0.431574 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 4 | 0.418122 | `azmcp_storage_datalake_directory_create` | ❌ |
| 5 | 0.416095 | `azmcp_storage_blob_container_list` | ❌ |
| 6 | 0.397478 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.396289 | `azmcp_storage_account_list` | ❌ |
| 8 | 0.394484 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.358330 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.343305 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.341769 | `azmcp_storage_account_details` | ❌ |
| 12 | 0.337207 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.336951 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.333941 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.328840 | `azmcp_functionapp_list` | ❌ |
| 16 | 0.323352 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.318016 | `azmcp_subscription_list` | ❌ |
| 18 | 0.314395 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.299804 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.294644 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 232

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Send a message "Hello, World!" to the queue <queue> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.556787 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.412449 | `azmcp_storage_account_list` | ❌ |
| 3 | 0.410972 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.400887 | `azmcp_storage_blob_container_list` | ❌ |
| 5 | 0.391050 | `azmcp_storage_account_details` | ❌ |
| 6 | 0.371142 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.348555 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.344373 | `azmcp_servicebus_queue_details` | ❌ |
| 9 | 0.335989 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.328105 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.325517 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.321736 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.318513 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.307333 | `azmcp_appconfig_kv_lock` | ❌ |
| 15 | 0.305274 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.285295 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.283394 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 18 | 0.276137 | `azmcp_appconfig_kv_unlock` | ❌ |
| 19 | 0.272609 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.258161 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 233

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Send a message with TTL of 3600 seconds to the queue <queue> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639672 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.383344 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.373050 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.370761 | `azmcp_storage_blob_container_list` | ❌ |
| 5 | 0.367030 | `azmcp_storage_account_list` | ❌ |
| 6 | 0.363729 | `azmcp_storage_account_details` | ❌ |
| 7 | 0.355961 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 8 | 0.345384 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.334481 | `azmcp_storage_blob_list` | ❌ |
| 10 | 0.327302 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.315019 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.310258 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.294922 | `azmcp_appconfig_kv_lock` | ❌ |
| 14 | 0.282610 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.277782 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.273178 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.271393 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.261862 | `azmcp_appconfig_kv_unlock` | ❌ |
| 19 | 0.257186 | `azmcp_keyvault_secret_create` | ❌ |
| 20 | 0.239636 | `azmcp_kusto_query` | ❌ |

---

## Test 234

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Add a message to the queue <queue> in storage account <account> with visibility timeout of 30 seconds  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592887 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.360490 | `azmcp_servicebus_queue_details` | ❌ |
| 3 | 0.329155 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.325329 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.322619 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.319410 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.315008 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.314567 | `azmcp_storage_account_list` | ❌ |
| 9 | 0.292951 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 10 | 0.292303 | `azmcp_storage_blob_list` | ❌ |
| 11 | 0.289323 | `azmcp_appconfig_kv_lock` | ❌ |
| 12 | 0.274459 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.270407 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.270314 | `azmcp_storage_blob_upload` | ❌ |
| 15 | 0.262052 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.257421 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.247023 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.246957 | `azmcp_appconfig_kv_unlock` | ❌ |
| 19 | 0.245623 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.241542 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 235

**Expected Tool:** `azmcp_storage_share_file_list`  
**Prompt:** List all files and directories in the File Share <share> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640440 | `azmcp_storage_share_file_list` | ✅ **EXPECTED** |
| 2 | 0.570872 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.539851 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.524910 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.522873 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 6 | 0.516653 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.474154 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.458759 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.433528 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.416549 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.400965 | `azmcp_storage_blob_container_details` | ❌ |
| 12 | 0.397963 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.390273 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.385269 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.373056 | `azmcp_keyvault_certificate_list` | ❌ |
| 16 | 0.372921 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.366460 | `azmcp_subscription_list` | ❌ |
| 18 | 0.360293 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.353596 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.341755 | `azmcp_functionapp_list` | ❌ |

---

## Test 236

**Expected Tool:** `azmcp_storage_share_file_list`  
**Prompt:** Show me the files in the File Share <share> directory <directory_path> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551793 | `azmcp_storage_share_file_list` | ✅ **EXPECTED** |
| 2 | 0.509406 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 3 | 0.470199 | `azmcp_storage_blob_container_list` | ❌ |
| 4 | 0.452409 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.431859 | `azmcp_storage_account_list` | ❌ |
| 6 | 0.421886 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.418334 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.405834 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.379973 | `azmcp_storage_datalake_directory_create` | ❌ |
| 10 | 0.351796 | `azmcp_storage_blob_container_details` | ❌ |
| 11 | 0.351680 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.341401 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.331211 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.328371 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.320263 | `azmcp_keyvault_secret_list` | ❌ |
| 16 | 0.317861 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.315168 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.303839 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.303719 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.301089 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 237

**Expected Tool:** `azmcp_storage_share_file_list`  
**Prompt:** List files with prefix 'report' in the File Share <share> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602213 | `azmcp_storage_share_file_list` | ✅ **EXPECTED** |
| 2 | 0.452409 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.449412 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.447430 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.446095 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 6 | 0.423868 | `azmcp_extension_azqr` | ❌ |
| 7 | 0.422668 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.412008 | `azmcp_storage_blob_list` | ❌ |
| 9 | 0.390975 | `azmcp_storage_account_details` | ❌ |
| 10 | 0.378092 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.374350 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.369171 | `azmcp_acr_registry_repository_list` | ❌ |
| 13 | 0.364292 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.343990 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.339261 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.336352 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.332926 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.319827 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.319475 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.314995 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 238

**Expected Tool:** `azmcp_storage_table_list`  
**Prompt:** List all tables in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.787243 | `azmcp_storage_table_list` | ✅ **EXPECTED** |
| 2 | 0.613700 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.574975 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.570307 | `azmcp_storage_account_list` | ❌ |
| 5 | 0.552523 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.546453 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.514042 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.503638 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.498181 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.497572 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.491995 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.486049 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.481347 | `azmcp_storage_account_details` | ❌ |
| 14 | 0.430612 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.421849 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.404701 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.393482 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.362914 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.360786 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.358239 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_storage_table_list`  
**Prompt:** Show me the tables in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738095 | `azmcp_storage_table_list` | ✅ **EXPECTED** |
| 2 | 0.589625 | `azmcp_storage_blob_container_list` | ❌ |
| 3 | 0.536928 | `azmcp_storage_account_list` | ❌ |
| 4 | 0.521787 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.520811 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.512064 | `azmcp_storage_blob_list` | ❌ |
| 7 | 0.495640 | `azmcp_storage_account_details` | ❌ |
| 8 | 0.480680 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.479470 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.470860 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.462051 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.447645 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.441119 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.434030 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 15 | 0.423663 | `azmcp_postgres_table_list` | ❌ |
| 16 | 0.380764 | `azmcp_kusto_table_schema` | ❌ |
| 17 | 0.367981 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.365922 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.362253 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.355013 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 240

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576055 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.473852 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471653 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.470819 | `azmcp_search_service_list` | ❌ |
| 6 | 0.462509 | `azmcp_storage_account_list` | ❌ |
| 7 | 0.450973 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.445724 | `azmcp_grafana_list` | ❌ |
| 9 | 0.436338 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.431337 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.430280 | `azmcp_group_list` | ❌ |
| 12 | 0.406935 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.395830 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.393357 | `azmcp_functionapp_list` | ❌ |
| 15 | 0.388737 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.366860 | `azmcp_loadtesting_testresource_list` | ❌ |
| 17 | 0.354245 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.350655 | `azmcp_storage_blob_container_list` | ❌ |
| 19 | 0.348524 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.345154 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 241

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405723 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.381238 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.351864 | `azmcp_grafana_list` | ❌ |
| 4 | 0.350951 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.344860 | `azmcp_search_service_list` | ❌ |
| 6 | 0.341813 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.315604 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.308874 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.303528 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.297209 | `azmcp_group_list` | ❌ |
| 11 | 0.296282 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.285434 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.275585 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.275417 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.272790 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.271295 | `azmcp_storage_account_list` | ❌ |
| 17 | 0.258047 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.256330 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.254883 | `azmcp_functionapp_list` | ❌ |
| 20 | 0.244501 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 242

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.319958 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.298352 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.286711 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.285063 | `azmcp_search_service_list` | ❌ |
| 5 | 0.282645 | `azmcp_grafana_list` | ❌ |
| 6 | 0.279702 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.278798 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.256358 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.254815 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 10 | 0.252504 | `azmcp_loadtesting_testresource_list` | ❌ |
| 11 | 0.233143 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.230571 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.230324 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.227020 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.226446 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.222799 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.219535 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.216460 | `azmcp_group_list` | ❌ |
| 19 | 0.211120 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.210310 | `azmcp_storage_account_list` | ❌ |

---

## Test 243

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.403229 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.354504 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.342318 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.340339 | `azmcp_grafana_list` | ❌ |
| 5 | 0.336798 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.332531 | `azmcp_search_service_list` | ❌ |
| 7 | 0.304965 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.300478 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 9 | 0.294080 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.291826 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.285774 | `azmcp_marketplace_product_get` | ❌ |
| 12 | 0.282326 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.281294 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.269869 | `azmcp_group_list` | ❌ |
| 15 | 0.259135 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.258410 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.257203 | `azmcp_functionapp_list` | ❌ |
| 18 | 0.253091 | `azmcp_storage_account_list` | ❌ |
| 19 | 0.236600 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.233362 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 244

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692578 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625270 | `azmcp_deploy_iac_rules_get` | ❌ |
| 3 | 0.604991 | `azmcp_bestpractices_get` | ❌ |
| 4 | 0.482936 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466199 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.459270 | `azmcp_extension_az` | ❌ |
| 7 | 0.431102 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.389080 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.386480 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.372596 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.354838 | `azmcp_bicepschema_get` | ❌ |
| 12 | 0.354086 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.339035 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.332182 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.312592 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.310275 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.306705 | `azmcp_storage_account_details` | ❌ |
| 18 | 0.305259 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.303849 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.291466 | `azmcp_subscription_list` | ❌ |

---

## Test 245

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584112 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.518352 | `azmcp_bestpractices_get` | ❌ |
| 3 | 0.510004 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.444297 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.439871 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.439607 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.429323 | `azmcp_keyvault_certificate_get` | ❌ |
| 8 | 0.406432 | `azmcp_extension_az` | ❌ |
| 9 | 0.389450 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.381339 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.304912 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.304137 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.300776 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.292804 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.290398 | `azmcp_storage_account_details` | ❌ |
| 16 | 0.278638 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.274806 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.274538 | `azmcp_subscription_list` | ❌ |
| 19 | 0.267574 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.266936 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 246

**Expected Tool:** `azmcp_virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713689 | `azmcp_virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.658080 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.566615 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.557529 | `azmcp_search_service_list` | ❌ |
| 5 | 0.536542 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.535739 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 7 | 0.528358 | `azmcp_sql_elastic-pool_list` | ❌ |
| 8 | 0.527948 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.525443 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.506608 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.505116 | `azmcp_subscription_list` | ❌ |
| 12 | 0.496297 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.495490 | `azmcp_grafana_list` | ❌ |
| 14 | 0.492515 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.476718 | `azmcp_group_list` | ❌ |
| 16 | 0.474623 | `azmcp_functionapp_list` | ❌ |
| 17 | 0.460388 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.459250 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.456279 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.431475 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 247

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736121 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714469 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.590273 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.397910 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.364696 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.344934 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.337530 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.335295 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.333517 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.332928 | `azmcp_keyvault_secret_list` | ❌ |
| 11 | 0.331141 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.329287 | `azmcp_search_service_list` | ❌ |
| 13 | 0.328623 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.321841 | `azmcp_subscription_list` | ❌ |
| 15 | 0.312156 | `azmcp_keyvault_certificate_list` | ❌ |
| 16 | 0.311262 | `azmcp_grafana_list` | ❌ |
| 17 | 0.308168 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.302706 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.291590 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.291489 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 248

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812890 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.664960 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.512404 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.336675 | `azmcp_monitor_workspace_list` | ❌ |
| 5 | 0.329418 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.322480 | `azmcp_subscription_list` | ❌ |
| 7 | 0.316573 | `azmcp_loadtesting_testrun_list` | ❌ |
| 8 | 0.315695 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.304322 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.303668 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.302921 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.298796 | `azmcp_keyvault_secret_list` | ❌ |
| 13 | 0.295712 | `azmcp_grafana_list` | ❌ |
| 14 | 0.295224 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.285353 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.278030 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.275584 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.275027 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.274540 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.273772 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552212 | `azmcp_workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.433162 | `azmcp_workbooks_update` | ❌ |
| 3 | 0.365579 | `azmcp_workbooks_delete` | ❌ |
| 4 | 0.361273 | `azmcp_workbooks_show` | ❌ |
| 5 | 0.328113 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.239727 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.217264 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.214818 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.188137 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.173344 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.172541 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.169440 | `azmcp_grafana_list` | ❌ |
| 13 | 0.148897 | `azmcp_loadtesting_test_create` | ❌ |
| 14 | 0.147365 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.142879 | `azmcp_storage_datalake_directory_create` | ❌ |
| 16 | 0.138518 | `azmcp_monitor_table_type_list` | ❌ |
| 17 | 0.130524 | `azmcp_loadtesting_testrun_create` | ❌ |
| 18 | 0.130339 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.116803 | `azmcp_loadtesting_testrun_update` | ❌ |
| 20 | 0.113882 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 250

**Expected Tool:** `azmcp_workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624673 | `azmcp_workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.518706 | `azmcp_workbooks_show` | ❌ |
| 3 | 0.432454 | `azmcp_workbooks_create` | ❌ |
| 4 | 0.425569 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.390355 | `azmcp_workbooks_update` | ❌ |
| 6 | 0.273939 | `azmcp_grafana_list` | ❌ |
| 7 | 0.198585 | `azmcp_appconfig_kv_delete` | ❌ |
| 8 | 0.193240 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.186665 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.186572 | `azmcp_monitor_workspace_log_query` | ❌ |
| 11 | 0.157219 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.155100 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.151308 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.148882 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.145141 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.134979 | `azmcp_loadtesting_testrun_update` | ❌ |
| 17 | 0.132504 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.131813 | `azmcp_group_list` | ❌ |
| 19 | 0.126089 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 20 | 0.122647 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 251

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772431 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.532603 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.516739 | `azmcp_grafana_list` | ❌ |
| 5 | 0.490216 | `azmcp_workbooks_delete` | ❌ |
| 6 | 0.488600 | `azmcp_group_list` | ❌ |
| 7 | 0.459976 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.454210 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.439945 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.428858 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.416443 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.413409 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.405963 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.405064 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.399758 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.366218 | `azmcp_functionapp_list` | ❌ |
| 17 | 0.365302 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.362740 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.358874 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.352940 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 252

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570259 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.539999 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.488377 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.472378 | `azmcp_grafana_list` | ❌ |
| 6 | 0.428025 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.425426 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.422785 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421646 | `azmcp_group_list` | ❌ |
| 10 | 0.412464 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.392371 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.380991 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.371128 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.363744 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.362416 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.356910 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.350839 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.338334 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.334580 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.322180 | `azmcp_functionapp_list` | ❌ |

---

## Test 253

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.697474 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498390 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.494708 | `azmcp_workbooks_list` | ❌ |
| 4 | 0.457252 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.419105 | `azmcp_workbooks_update` | ❌ |
| 6 | 0.353546 | `azmcp_grafana_list` | ❌ |
| 7 | 0.277807 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.256678 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.242738 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.238877 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.235477 | `azmcp_marketplace_product_get` | ❌ |
| 12 | 0.230558 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.230516 | `azmcp_monitor_metrics_definitions` | ❌ |
| 14 | 0.227558 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.226385 | `azmcp_loadtesting_test_get` | ❌ |
| 16 | 0.218999 | `azmcp_loadtesting_testresource_list` | ❌ |
| 17 | 0.207693 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.195848 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.195466 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 20 | 0.195373 | `azmcp_group_list` | ❌ |

---

## Test 254

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Show me the workbook with display name <workbook_display_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469435 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.455158 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.437638 | `azmcp_workbooks_update` | ❌ |
| 4 | 0.424338 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.371623 | `azmcp_workbooks_delete` | ❌ |
| 6 | 0.292898 | `azmcp_grafana_list` | ❌ |
| 7 | 0.266324 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.239907 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.227383 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.176481 | `azmcp_role_assignment_list` | ❌ |
| 11 | 0.175814 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.174513 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.174123 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.168191 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.165774 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.154760 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.149678 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.146054 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.143754 | `azmcp_loadtesting_testrun_get` | ❌ |
| 20 | 0.141559 | `azmcp_foundry_models_list` | ❌ |

---

## Test 255

**Expected Tool:** `azmcp_workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469915 | `azmcp_workbooks_update` | ✅ **EXPECTED** |
| 2 | 0.382651 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.362429 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.351698 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.276727 | `azmcp_loadtesting_testrun_update` | ❌ |
| 6 | 0.262873 | `azmcp_workbooks_list` | ❌ |
| 7 | 0.170118 | `azmcp_grafana_list` | ❌ |
| 8 | 0.155789 | `azmcp_storage_blob_upload` | ❌ |
| 9 | 0.148730 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.145379 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 11 | 0.144812 | `azmcp_extension_az` | ❌ |
| 12 | 0.142404 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 13 | 0.142186 | `azmcp_loadtesting_testrun_create` | ❌ |
| 14 | 0.138354 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.136105 | `azmcp_loadtesting_testresource_create` | ❌ |
| 16 | 0.131007 | `azmcp_postgres_database_query` | ❌ |
| 17 | 0.129973 | `azmcp_postgres_server_param_set` | ❌ |
| 18 | 0.129660 | `azmcp_deploy_iac_rules_get` | ❌ |
| 19 | 0.124925 | `azmcp_appconfig_kv_unlock` | ❌ |
| 20 | 0.115996 | `azmcp_appconfig_kv_lock` | ❌ |

---

## Test 256

**Expected Tool:** `azmcp_bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485889 | `azmcp_deploy_iac_rules_get` | ❌ |
| 2 | 0.451493 | `azmcp_bestpractices_get` | ❌ |
| 3 | 0.440302 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.432773 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.432409 | `azmcp_bicepschema_get` | ✅ **EXPECTED** |
| 6 | 0.401162 | `azmcp_extension_az` | ❌ |
| 7 | 0.400985 | `azmcp_foundry_models_deploy` | ❌ |
| 8 | 0.398046 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.390547 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 10 | 0.382229 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.363171 | `azmcp_search_index_list` | ❌ |
| 12 | 0.345030 | `azmcp_search_service_list` | ❌ |
| 13 | 0.335700 | `azmcp_search_index_query` | ❌ |
| 14 | 0.303518 | `azmcp_search_index_describe` | ❌ |
| 15 | 0.303183 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.291268 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 17 | 0.281478 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.280207 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.275781 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.268139 | `azmcp_workbooks_create` | ❌ |

---

## Test 257

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.349336 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.295312 | `azmcp_storage_blob_upload` | ❌ |
| 3 | 0.254991 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.250641 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 5 | 0.221349 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.217623 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.216162 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 8 | 0.212008 | `azmcp_storage_blob_container_details` | ❌ |
| 9 | 0.211877 | `azmcp_extension_az` | ❌ |
| 10 | 0.178245 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.175833 | `azmcp_deploy_iac_rules_get` | ❌ |
| 12 | 0.174777 | `azmcp_storage_blob_details` | ❌ |
| 13 | 0.159251 | `azmcp_storage_share_file_list` | ❌ |
| 14 | 0.155284 | `azmcp_storage_queue_message_send` | ❌ |
| 15 | 0.136237 | `azmcp_bestpractices_get` | ❌ |
| 16 | 0.136163 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.135545 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.135426 | `azmcp_storage_datalake_directory_create` | ❌ |
| 19 | 0.130037 | `azmcp_foundry_models_deploy` | ❌ |
| 20 | 0.126092 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |

---

## Test 258

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.290270 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.283710 | `azmcp_extension_az` | ❌ |
| 3 | 0.267683 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.258160 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.226551 | `azmcp_bestpractices_get` | ❌ |
| 6 | 0.225622 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.207352 | `azmcp_deploy_iac_rules_get` | ❌ |
| 8 | 0.179120 | `azmcp_loadtesting_testresource_create` | ❌ |
| 9 | 0.171988 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.170600 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 11 | 0.163694 | `azmcp_mysql_database_query` | ❌ |
| 12 | 0.162220 | `azmcp_extension_azd` | ❌ |
| 13 | 0.160743 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.154229 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.148123 | `azmcp_storage_queue_message_send` | ❌ |
| 16 | 0.146303 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 17 | 0.145124 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.138580 | `azmcp_storage_blob_container_create` | ❌ |
| 19 | 0.130673 | `azmcp_search_index_query` | ❌ |
| 20 | 0.124738 | `azmcp_search_index_list` | ❌ |

---

## Test 259

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.299640 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.245097 | `azmcp_extension_az` | ❌ |
| 5 | 0.242581 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.218064 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.213481 | `azmcp_bestpractices_get` | ❌ |
| 8 | 0.179199 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.164283 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.159293 | `azmcp_marketplace_product_get` | ❌ |
| 11 | 0.156565 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.153082 | `azmcp_storage_queue_message_send` | ❌ |
| 13 | 0.140353 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 14 | 0.137104 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.134492 | `azmcp_storage_blob_upload` | ❌ |
| 16 | 0.132355 | `azmcp_mysql_database_query` | ❌ |
| 17 | 0.130552 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.130132 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.119586 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.114994 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 260

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.420259 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.366584 | `azmcp_extension_az` | ❌ |
| 4 | 0.352797 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.343739 | `azmcp_storage_blob_upload` | ❌ |
| 6 | 0.310615 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.303258 | `azmcp_storage_queue_message_send` | ❌ |
| 8 | 0.300898 | `azmcp_bestpractices_get` | ❌ |
| 9 | 0.297027 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 10 | 0.293806 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.292363 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.291879 | `azmcp_deploy_iac_rules_get` | ❌ |
| 13 | 0.272671 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.266682 | `azmcp_storage_blob_list` | ❌ |
| 15 | 0.261446 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.261401 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 17 | 0.258294 | `azmcp_storage_account_details` | ❌ |
| 18 | 0.257883 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.255675 | `azmcp_storage_blob_container_list` | ❌ |
| 20 | 0.255019 | `azmcp_storage_table_list` | ❌ |

---

## Summary

**Total Prompts Tested:** 260  
**Execution Time:** 43.2256371s  

### Success Rate Metrics

**Top Choice Success:** 83.5% (217/260 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 5.0% (13/260 tests)  
**🎯 High Confidence (≥0.7):** 23.8% (62/260 tests)  
**✅ Good Confidence (≥0.6):** 60.0% (156/260 tests)  
**👍 Fair Confidence (≥0.5):** 84.2% (219/260 tests)  
**👌 Acceptable Confidence (≥0.4):** 93.8% (244/260 tests)  
**❌ Low Confidence (<0.4):** 6.2% (16/260 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 5.0% (13/260 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 23.8% (62/260 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 58.5% (152/260 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 75.8% (197/260 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 80.8% (210/260 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing adequately but has room for improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

