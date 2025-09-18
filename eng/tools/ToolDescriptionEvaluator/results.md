# Tool Selection Analysis Setup

**Setup completed:** 2025-09-17 17:35:16  
**Tool count:** 136  
**Database setup time:** 4.8300256s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-17 17:35:16  
**Tool count:** 136  

## Table of Contents

- [Test 1: azmcp_foundry_knowledge_index_list](#test-1)
- [Test 2: azmcp_foundry_knowledge_index_list](#test-2)
- [Test 3: azmcp_foundry_knowledge_index_schema](#test-3)
- [Test 4: azmcp_foundry_knowledge_index_schema](#test-4)
- [Test 5: azmcp_foundry_models_deploy](#test-5)
- [Test 6: azmcp_foundry_models_deployments_list](#test-6)
- [Test 7: azmcp_foundry_models_deployments_list](#test-7)
- [Test 8: azmcp_foundry_models_list](#test-8)
- [Test 9: azmcp_foundry_models_list](#test-9)
- [Test 10: azmcp_search_index_get](#test-10)
- [Test 11: azmcp_search_index_get](#test-11)
- [Test 12: azmcp_search_index_get](#test-12)
- [Test 13: azmcp_search_index_query](#test-13)
- [Test 14: azmcp_search_service_list](#test-14)
- [Test 15: azmcp_search_service_list](#test-15)
- [Test 16: azmcp_search_service_list](#test-16)
- [Test 17: azmcp_appconfig_account_list](#test-17)
- [Test 18: azmcp_appconfig_account_list](#test-18)
- [Test 19: azmcp_appconfig_account_list](#test-19)
- [Test 20: azmcp_appconfig_kv_delete](#test-20)
- [Test 21: azmcp_appconfig_kv_list](#test-21)
- [Test 22: azmcp_appconfig_kv_list](#test-22)
- [Test 23: azmcp_appconfig_kv_lock_set](#test-23)
- [Test 24: azmcp_appconfig_kv_lock_set](#test-24)
- [Test 25: azmcp_appconfig_kv_set](#test-25)
- [Test 26: azmcp_appconfig_kv_show](#test-26)
- [Test 27: azmcp_applens_resource_diagnose](#test-27)
- [Test 28: azmcp_applens_resource_diagnose](#test-28)
- [Test 29: azmcp_applens_resource_diagnose](#test-29)
- [Test 30: azmcp_acr_registry_list](#test-30)
- [Test 31: azmcp_acr_registry_list](#test-31)
- [Test 32: azmcp_acr_registry_list](#test-32)
- [Test 33: azmcp_acr_registry_list](#test-33)
- [Test 34: azmcp_acr_registry_list](#test-34)
- [Test 35: azmcp_acr_registry_repository_list](#test-35)
- [Test 36: azmcp_acr_registry_repository_list](#test-36)
- [Test 37: azmcp_acr_registry_repository_list](#test-37)
- [Test 38: azmcp_acr_registry_repository_list](#test-38)
- [Test 39: azmcp_cosmos_account_list](#test-39)
- [Test 40: azmcp_cosmos_account_list](#test-40)
- [Test 41: azmcp_cosmos_account_list](#test-41)
- [Test 42: azmcp_cosmos_database_container_item_query](#test-42)
- [Test 43: azmcp_cosmos_database_container_list](#test-43)
- [Test 44: azmcp_cosmos_database_container_list](#test-44)
- [Test 45: azmcp_cosmos_database_list](#test-45)
- [Test 46: azmcp_cosmos_database_list](#test-46)
- [Test 47: azmcp_kusto_cluster_get](#test-47)
- [Test 48: azmcp_kusto_cluster_list](#test-48)
- [Test 49: azmcp_kusto_cluster_list](#test-49)
- [Test 50: azmcp_kusto_cluster_list](#test-50)
- [Test 51: azmcp_kusto_database_list](#test-51)
- [Test 52: azmcp_kusto_database_list](#test-52)
- [Test 53: azmcp_kusto_query](#test-53)
- [Test 54: azmcp_kusto_sample](#test-54)
- [Test 55: azmcp_kusto_table_list](#test-55)
- [Test 56: azmcp_kusto_table_list](#test-56)
- [Test 57: azmcp_kusto_table_schema](#test-57)
- [Test 58: azmcp_mysql_database_list](#test-58)
- [Test 59: azmcp_mysql_database_list](#test-59)
- [Test 60: azmcp_mysql_database_query](#test-60)
- [Test 61: azmcp_mysql_server_config_get](#test-61)
- [Test 62: azmcp_mysql_server_list](#test-62)
- [Test 63: azmcp_mysql_server_list](#test-63)
- [Test 64: azmcp_mysql_server_list](#test-64)
- [Test 65: azmcp_mysql_server_param_get](#test-65)
- [Test 66: azmcp_mysql_server_param_set](#test-66)
- [Test 67: azmcp_mysql_table_list](#test-67)
- [Test 68: azmcp_mysql_table_list](#test-68)
- [Test 69: azmcp_mysql_table_schema_get](#test-69)
- [Test 70: azmcp_postgres_database_list](#test-70)
- [Test 71: azmcp_postgres_database_list](#test-71)
- [Test 72: azmcp_postgres_database_query](#test-72)
- [Test 73: azmcp_postgres_server_config_get](#test-73)
- [Test 74: azmcp_postgres_server_list](#test-74)
- [Test 75: azmcp_postgres_server_list](#test-75)
- [Test 76: azmcp_postgres_server_list](#test-76)
- [Test 77: azmcp_postgres_server_param](#test-77)
- [Test 78: azmcp_postgres_server_param_set](#test-78)
- [Test 79: azmcp_postgres_table_list](#test-79)
- [Test 80: azmcp_postgres_table_list](#test-80)
- [Test 81: azmcp_postgres_table_schema_get](#test-81)
- [Test 82: azmcp_deploy_app_logs_get](#test-82)
- [Test 83: azmcp_deploy_architecture_diagram_generate](#test-83)
- [Test 84: azmcp_deploy_iac_rules_get](#test-84)
- [Test 85: azmcp_deploy_pipeline_guidance_get](#test-85)
- [Test 86: azmcp_deploy_plan_get](#test-86)
- [Test 87: azmcp_eventgrid_topic_list](#test-87)
- [Test 88: azmcp_eventgrid_topic_list](#test-88)
- [Test 89: azmcp_eventgrid_topic_list](#test-89)
- [Test 90: azmcp_eventgrid_topic_list](#test-90)
- [Test 91: azmcp_functionapp_get](#test-91)
- [Test 92: azmcp_functionapp_get](#test-92)
- [Test 93: azmcp_functionapp_get](#test-93)
- [Test 94: azmcp_functionapp_get](#test-94)
- [Test 95: azmcp_functionapp_get](#test-95)
- [Test 96: azmcp_functionapp_get](#test-96)
- [Test 97: azmcp_functionapp_get](#test-97)
- [Test 98: azmcp_functionapp_get](#test-98)
- [Test 99: azmcp_functionapp_get](#test-99)
- [Test 100: azmcp_functionapp_get](#test-100)
- [Test 101: azmcp_functionapp_get](#test-101)
- [Test 102: azmcp_functionapp_get](#test-102)
- [Test 103: azmcp_keyvault_certificate_create](#test-103)
- [Test 104: azmcp_keyvault_certificate_get](#test-104)
- [Test 105: azmcp_keyvault_certificate_get](#test-105)
- [Test 106: azmcp_keyvault_certificate_import](#test-106)
- [Test 107: azmcp_keyvault_certificate_import](#test-107)
- [Test 108: azmcp_keyvault_certificate_list](#test-108)
- [Test 109: azmcp_keyvault_certificate_list](#test-109)
- [Test 110: azmcp_keyvault_key_create](#test-110)
- [Test 111: azmcp_keyvault_key_list](#test-111)
- [Test 112: azmcp_keyvault_key_list](#test-112)
- [Test 113: azmcp_keyvault_secret_create](#test-113)
- [Test 114: azmcp_keyvault_secret_list](#test-114)
- [Test 115: azmcp_keyvault_secret_list](#test-115)
- [Test 116: azmcp_aks_cluster_get](#test-116)
- [Test 117: azmcp_aks_cluster_get](#test-117)
- [Test 118: azmcp_aks_cluster_get](#test-118)
- [Test 119: azmcp_aks_cluster_get](#test-119)
- [Test 120: azmcp_aks_cluster_list](#test-120)
- [Test 121: azmcp_aks_cluster_list](#test-121)
- [Test 122: azmcp_aks_cluster_list](#test-122)
- [Test 123: azmcp_aks_nodepool_get](#test-123)
- [Test 124: azmcp_aks_nodepool_get](#test-124)
- [Test 125: azmcp_aks_nodepool_get](#test-125)
- [Test 126: azmcp_aks_nodepool_list](#test-126)
- [Test 127: azmcp_aks_nodepool_list](#test-127)
- [Test 128: azmcp_aks_nodepool_list](#test-128)
- [Test 129: azmcp_loadtesting_test_create](#test-129)
- [Test 130: azmcp_loadtesting_test_get](#test-130)
- [Test 131: azmcp_loadtesting_testresource_create](#test-131)
- [Test 132: azmcp_loadtesting_testresource_list](#test-132)
- [Test 133: azmcp_loadtesting_testrun_create](#test-133)
- [Test 134: azmcp_loadtesting_testrun_get](#test-134)
- [Test 135: azmcp_loadtesting_testrun_list](#test-135)
- [Test 136: azmcp_loadtesting_testrun_update](#test-136)
- [Test 137: azmcp_grafana_list](#test-137)
- [Test 138: azmcp_azuremanagedlustre_filesystem_list](#test-138)
- [Test 139: azmcp_azuremanagedlustre_filesystem_list](#test-139)
- [Test 140: azmcp_azuremanagedlustre_filesystem_required-subnet-size](#test-140)
- [Test 141: azmcp_azuremanagedlustre_filesystem_sku_get](#test-141)
- [Test 142: azmcp_marketplace_product_get](#test-142)
- [Test 143: azmcp_marketplace_product_list](#test-143)
- [Test 144: azmcp_marketplace_product_list](#test-144)
- [Test 145: azmcp_bestpractices_get](#test-145)
- [Test 146: azmcp_bestpractices_get](#test-146)
- [Test 147: azmcp_bestpractices_get](#test-147)
- [Test 148: azmcp_bestpractices_get](#test-148)
- [Test 149: azmcp_bestpractices_get](#test-149)
- [Test 150: azmcp_bestpractices_get](#test-150)
- [Test 151: azmcp_bestpractices_get](#test-151)
- [Test 152: azmcp_bestpractices_get](#test-152)
- [Test 153: azmcp_bestpractices_get](#test-153)
- [Test 154: azmcp_bestpractices_get](#test-154)
- [Test 155: azmcp_monitor_healthmodels_entity_gethealth](#test-155)
- [Test 156: azmcp_monitor_metrics_definitions](#test-156)
- [Test 157: azmcp_monitor_metrics_definitions](#test-157)
- [Test 158: azmcp_monitor_metrics_definitions](#test-158)
- [Test 159: azmcp_monitor_metrics_query](#test-159)
- [Test 160: azmcp_monitor_metrics_query](#test-160)
- [Test 161: azmcp_monitor_metrics_query](#test-161)
- [Test 162: azmcp_monitor_metrics_query](#test-162)
- [Test 163: azmcp_monitor_metrics_query](#test-163)
- [Test 164: azmcp_monitor_metrics_query](#test-164)
- [Test 165: azmcp_monitor_resource_log_query](#test-165)
- [Test 166: azmcp_monitor_table_list](#test-166)
- [Test 167: azmcp_monitor_table_list](#test-167)
- [Test 168: azmcp_monitor_table_type_list](#test-168)
- [Test 169: azmcp_monitor_table_type_list](#test-169)
- [Test 170: azmcp_monitor_workspace_list](#test-170)
- [Test 171: azmcp_monitor_workspace_list](#test-171)
- [Test 172: azmcp_monitor_workspace_list](#test-172)
- [Test 173: azmcp_monitor_workspace_log_query](#test-173)
- [Test 174: azmcp_datadog_monitoredresources_list](#test-174)
- [Test 175: azmcp_datadog_monitoredresources_list](#test-175)
- [Test 176: azmcp_extension_azqr](#test-176)
- [Test 177: azmcp_extension_azqr](#test-177)
- [Test 178: azmcp_extension_azqr](#test-178)
- [Test 179: azmcp_quota_region_availability_list](#test-179)
- [Test 180: azmcp_quota_usage_check](#test-180)
- [Test 181: azmcp_role_assignment_list](#test-181)
- [Test 182: azmcp_role_assignment_list](#test-182)
- [Test 183: azmcp_redis_cache_accesspolicy_list](#test-183)
- [Test 184: azmcp_redis_cache_accesspolicy_list](#test-184)
- [Test 185: azmcp_redis_cache_list](#test-185)
- [Test 186: azmcp_redis_cache_list](#test-186)
- [Test 187: azmcp_redis_cache_list](#test-187)
- [Test 188: azmcp_redis_cluster_database_list](#test-188)
- [Test 189: azmcp_redis_cluster_database_list](#test-189)
- [Test 190: azmcp_redis_cluster_list](#test-190)
- [Test 191: azmcp_redis_cluster_list](#test-191)
- [Test 192: azmcp_redis_cluster_list](#test-192)
- [Test 193: azmcp_group_list](#test-193)
- [Test 194: azmcp_group_list](#test-194)
- [Test 195: azmcp_group_list](#test-195)
- [Test 196: azmcp_resourcehealth_availability-status_get](#test-196)
- [Test 197: azmcp_resourcehealth_availability-status_get](#test-197)
- [Test 198: azmcp_resourcehealth_availability-status_get](#test-198)
- [Test 199: azmcp_resourcehealth_availability-status_list](#test-199)
- [Test 200: azmcp_resourcehealth_availability-status_list](#test-200)
- [Test 201: azmcp_resourcehealth_availability-status_list](#test-201)
- [Test 202: azmcp_resourcehealth_service-health-events_list](#test-202)
- [Test 203: azmcp_resourcehealth_service-health-events_list](#test-203)
- [Test 204: azmcp_resourcehealth_service-health-events_list](#test-204)
- [Test 205: azmcp_resourcehealth_service-health-events_list](#test-205)
- [Test 206: azmcp_resourcehealth_service-health-events_list](#test-206)
- [Test 207: azmcp_servicebus_queue_details](#test-207)
- [Test 208: azmcp_servicebus_topic_details](#test-208)
- [Test 209: azmcp_servicebus_topic_subscription_details](#test-209)
- [Test 210: azmcp_sql_db_list](#test-210)
- [Test 211: azmcp_sql_db_list](#test-211)
- [Test 212: azmcp_sql_db_show](#test-212)
- [Test 213: azmcp_sql_db_show](#test-213)
- [Test 214: azmcp_sql_elastic-pool_list](#test-214)
- [Test 215: azmcp_sql_elastic-pool_list](#test-215)
- [Test 216: azmcp_sql_elastic-pool_list](#test-216)
- [Test 217: azmcp_sql_server_create](#test-217)
- [Test 218: azmcp_sql_server_create](#test-218)
- [Test 219: azmcp_sql_server_create](#test-219)
- [Test 220: azmcp_sql_server_delete](#test-220)
- [Test 221: azmcp_sql_server_delete](#test-221)
- [Test 222: azmcp_sql_server_delete](#test-222)
- [Test 223: azmcp_sql_server_entra-admin_list](#test-223)
- [Test 224: azmcp_sql_server_entra-admin_list](#test-224)
- [Test 225: azmcp_sql_server_entra-admin_list](#test-225)
- [Test 226: azmcp_sql_server_firewall-rule_create](#test-226)
- [Test 227: azmcp_sql_server_firewall-rule_create](#test-227)
- [Test 228: azmcp_sql_server_firewall-rule_create](#test-228)
- [Test 229: azmcp_sql_server_firewall-rule_delete](#test-229)
- [Test 230: azmcp_sql_server_firewall-rule_delete](#test-230)
- [Test 231: azmcp_sql_server_firewall-rule_delete](#test-231)
- [Test 232: azmcp_sql_server_firewall-rule_list](#test-232)
- [Test 233: azmcp_sql_server_firewall-rule_list](#test-233)
- [Test 234: azmcp_sql_server_firewall-rule_list](#test-234)
- [Test 235: azmcp_sql_server_show](#test-235)
- [Test 236: azmcp_sql_server_show](#test-236)
- [Test 237: azmcp_sql_server_show](#test-237)
- [Test 238: azmcp_storage_account_create](#test-238)
- [Test 239: azmcp_storage_account_create](#test-239)
- [Test 240: azmcp_storage_account_create](#test-240)
- [Test 241: azmcp_storage_account_get](#test-241)
- [Test 242: azmcp_storage_account_get](#test-242)
- [Test 243: azmcp_storage_account_get](#test-243)
- [Test 244: azmcp_storage_account_get](#test-244)
- [Test 245: azmcp_storage_account_get](#test-245)
- [Test 246: azmcp_storage_blob_batch_set-tier](#test-246)
- [Test 247: azmcp_storage_blob_batch_set-tier](#test-247)
- [Test 248: azmcp_storage_blob_container_create](#test-248)
- [Test 249: azmcp_storage_blob_container_create](#test-249)
- [Test 250: azmcp_storage_blob_container_create](#test-250)
- [Test 251: azmcp_storage_blob_container_get](#test-251)
- [Test 252: azmcp_storage_blob_container_get](#test-252)
- [Test 253: azmcp_storage_blob_container_get](#test-253)
- [Test 254: azmcp_storage_blob_get](#test-254)
- [Test 255: azmcp_storage_blob_get](#test-255)
- [Test 256: azmcp_storage_blob_get](#test-256)
- [Test 257: azmcp_storage_blob_get](#test-257)
- [Test 258: azmcp_storage_blob_upload](#test-258)
- [Test 259: azmcp_storage_datalake_directory_create](#test-259)
- [Test 260: azmcp_storage_datalake_file-system_list-paths](#test-260)
- [Test 261: azmcp_storage_datalake_file-system_list-paths](#test-261)
- [Test 262: azmcp_storage_datalake_file-system_list-paths](#test-262)
- [Test 263: azmcp_storage_queue_message_send](#test-263)
- [Test 264: azmcp_storage_queue_message_send](#test-264)
- [Test 265: azmcp_storage_queue_message_send](#test-265)
- [Test 266: azmcp_storage_share_file_list](#test-266)
- [Test 267: azmcp_storage_share_file_list](#test-267)
- [Test 268: azmcp_storage_share_file_list](#test-268)
- [Test 269: azmcp_storage_table_list](#test-269)
- [Test 270: azmcp_storage_table_list](#test-270)
- [Test 271: azmcp_subscription_list](#test-271)
- [Test 272: azmcp_subscription_list](#test-272)
- [Test 273: azmcp_subscription_list](#test-273)
- [Test 274: azmcp_subscription_list](#test-274)
- [Test 275: azmcp_azureterraformbestpractices_get](#test-275)
- [Test 276: azmcp_azureterraformbestpractices_get](#test-276)
- [Test 277: azmcp_virtualdesktop_hostpool_list](#test-277)
- [Test 278: azmcp_virtualdesktop_hostpool_sessionhost_list](#test-278)
- [Test 279: azmcp_virtualdesktop_hostpool_sessionhost_usersession-list](#test-279)
- [Test 280: azmcp_workbooks_create](#test-280)
- [Test 281: azmcp_workbooks_delete](#test-281)
- [Test 282: azmcp_workbooks_list](#test-282)
- [Test 283: azmcp_workbooks_list](#test-283)
- [Test 284: azmcp_workbooks_show](#test-284)
- [Test 285: azmcp_workbooks_show](#test-285)
- [Test 286: azmcp_workbooks_update](#test-286)
- [Test 287: azmcp_bicepschema_get](#test-287)
- [Test 288: azmcp_cloudarchitect_design](#test-288)
- [Test 289: azmcp_cloudarchitect_design](#test-289)
- [Test 290: azmcp_cloudarchitect_design](#test-290)
- [Test 291: azmcp_cloudarchitect_design](#test-291)

---

## Test 1

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695202 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.526528 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.433117 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.422779 | `azmcp_search_index_get` | ❌ |
| 5 | 0.412895 | `azmcp_search_service_list` | ❌ |
| 6 | 0.349507 | `azmcp_search_index_query` | ❌ |
| 7 | 0.329681 | `azmcp_foundry_models_deploy` | ❌ |
| 8 | 0.310470 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.309513 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.296877 | `azmcp_grafana_list` | ❌ |
| 11 | 0.291635 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.286074 | `azmcp_monitor_table_type_list` | ❌ |
| 13 | 0.279840 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.270212 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.270162 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.267906 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.265680 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.264056 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.262242 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.261138 | `azmcp_kusto_database_list` | ❌ |

---

## Test 2

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603396 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489311 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.396819 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.374704 | `azmcp_search_index_get` | ❌ |
| 5 | 0.350751 | `azmcp_search_service_list` | ❌ |
| 6 | 0.341865 | `azmcp_search_index_query` | ❌ |
| 7 | 0.317997 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.310576 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.278147 | `azmcp_foundry_models_deployments_list` | ❌ |
| 10 | 0.276839 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.272237 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.256208 | `azmcp_grafana_list` | ❌ |
| 13 | 0.249946 | `azmcp_get_bestpractices_get` | ❌ |
| 14 | 0.249587 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.232882 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.225181 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.224194 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.223814 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.218011 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.214893 | `azmcp_monitor_table_type_list` | ❌ |

---

## Test 3

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672577 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.564860 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.424581 | `azmcp_search_index_get` | ❌ |
| 4 | 0.375275 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.363951 | `azmcp_kusto_table_schema` | ❌ |
| 6 | 0.358315 | `azmcp_postgres_table_schema_get` | ❌ |
| 7 | 0.349967 | `azmcp_search_index_query` | ❌ |
| 8 | 0.347762 | `azmcp_foundry_models_list` | ❌ |
| 9 | 0.346329 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.326807 | `azmcp_search_service_list` | ❌ |
| 11 | 0.297822 | `azmcp_foundry_models_deploy` | ❌ |
| 12 | 0.295847 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.285897 | `azmcp_monitor_table_type_list` | ❌ |
| 14 | 0.277468 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.271427 | `azmcp_cloudarchitect_design` | ❌ |
| 16 | 0.266288 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.259298 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.253702 | `azmcp_grafana_list` | ❌ |
| 19 | 0.252091 | `azmcp_foundry_models_deployments_list` | ❌ |
| 20 | 0.238262 | `azmcp_storage_table_list` | ❌ |

---

## Test 4

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650269 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432758 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.415963 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.408316 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.398186 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.380040 | `azmcp_search_index_get` | ❌ |
| 7 | 0.352243 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.318649 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.311623 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.309927 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.286991 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.271701 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.271128 | `azmcp_aks_cluster_get` | ❌ |
| 14 | 0.262783 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.257402 | `azmcp_mysql_table_list` | ❌ |
| 16 | 0.256303 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.249010 | `azmcp_search_index_query` | ❌ |
| 18 | 0.246815 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.242191 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.239938 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 5

**Expected Tool:** `azmcp_foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.313400 | `azmcp_foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.282464 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.274011 | `azmcp_deploy_plan_get` | ❌ |
| 4 | 0.269513 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.268967 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.234071 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.222504 | `azmcp_grafana_list` | ❌ |
| 8 | 0.222478 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.221635 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.217001 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.215293 | `azmcp_loadtesting_testrun_create` | ❌ |
| 12 | 0.209865 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.209020 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 14 | 0.208124 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.207601 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.204420 | `azmcp_postgres_server_param_set` | ❌ |
| 17 | 0.195615 | `azmcp_workbooks_list` | ❌ |
| 18 | 0.192373 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.190106 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.189256 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 6

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559509 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.549636 | `azmcp_foundry_models_list` | ❌ |
| 3 | 0.533239 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.448711 | `azmcp_search_service_list` | ❌ |
| 5 | 0.434472 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 6 | 0.368174 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.334867 | `azmcp_grafana_list` | ❌ |
| 8 | 0.332002 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.328253 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 10 | 0.326752 | `azmcp_search_index_get` | ❌ |
| 11 | 0.320998 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.318854 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.310280 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.308008 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.302262 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.301302 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.300357 | `azmcp_search_index_query` | ❌ |
| 18 | 0.289448 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.288248 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.285916 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 7

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518221 | `azmcp_foundry_models_list` | ❌ |
| 2 | 0.503424 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.488885 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 4 | 0.401016 | `azmcp_search_service_list` | ❌ |
| 5 | 0.396422 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 6 | 0.328814 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.311230 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 8 | 0.305997 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.301514 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.298821 | `azmcp_search_index_query` | ❌ |
| 11 | 0.291256 | `azmcp_search_index_get` | ❌ |
| 12 | 0.286814 | `azmcp_grafana_list` | ❌ |
| 13 | 0.282504 | `azmcp_cloudarchitect_design` | ❌ |
| 14 | 0.269912 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.254926 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.250392 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.246893 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.243133 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.236572 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.234075 | `azmcp_redis_cache_list` | ❌ |

---

## Test 8

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560022 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.401146 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.387861 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.386180 | `azmcp_search_service_list` | ❌ |
| 5 | 0.346909 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.298648 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.290447 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 8 | 0.285437 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.277883 | `azmcp_grafana_list` | ❌ |
| 10 | 0.275316 | `azmcp_search_index_get` | ❌ |
| 11 | 0.273026 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.265730 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.255790 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.255760 | `azmcp_search_index_query` | ❌ |
| 15 | 0.252297 | `azmcp_postgres_database_list` | ❌ |
| 16 | 0.248620 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.248405 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.245193 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.235676 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.231110 | `azmcp_monitor_metrics_definitions` | ❌ |

---

## Test 9

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574818 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.430513 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.388967 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.356898 | `azmcp_foundry_models_deployments_list` | ❌ |
| 5 | 0.339069 | `azmcp_search_service_list` | ❌ |
| 6 | 0.299150 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 7 | 0.283250 | `azmcp_search_index_query` | ❌ |
| 8 | 0.274019 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.266936 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.261834 | `azmcp_search_index_get` | ❌ |
| 11 | 0.260143 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.245943 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.244697 | `azmcp_monitor_table_type_list` | ❌ |
| 14 | 0.243617 | `azmcp_deploy_plan_get` | ❌ |
| 15 | 0.240256 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.234050 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.211456 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.205424 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.200059 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.199386 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 10

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681052 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544557 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.490625 | `azmcp_search_service_list` | ❌ |
| 4 | 0.466005 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.459609 | `azmcp_search_index_query` | ❌ |
| 6 | 0.392754 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.388183 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.372382 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.370915 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.358384 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.356755 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.356252 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.354845 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.352762 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.351083 | `azmcp_sql_server_show` | ❌ |
| 16 | 0.343186 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.336902 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.333641 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.330038 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.329368 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 11

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640256 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.620140 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561856 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.480817 | `azmcp_search_index_query` | ❌ |
| 5 | 0.445467 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.439452 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.416404 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.409307 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.406485 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.397423 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.392400 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.382802 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.378750 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.378297 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.375372 | `azmcp_foundry_models_deployments_list` | ❌ |
| 16 | 0.371098 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.369526 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.368931 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367804 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.362619 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 12

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620759 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562775 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561154 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.471415 | `azmcp_search_index_query` | ❌ |
| 5 | 0.463972 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.401807 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.382692 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.372639 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.370331 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.367868 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.353840 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.351788 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.351161 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.350044 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.347566 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.346994 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.341728 | `azmcp_foundry_models_list` | ❌ |
| 18 | 0.335748 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.332289 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.331260 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522826 | `azmcp_search_index_get` | ❌ |
| 2 | 0.515870 | `azmcp_search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497467 | `azmcp_search_service_list` | ❌ |
| 4 | 0.373917 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.372940 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.327095 | `azmcp_kusto_query` | ❌ |
| 7 | 0.322358 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.311044 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.306415 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.305939 | `azmcp_marketplace_product_list` | ❌ |
| 11 | 0.295413 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.290306 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.288242 | `azmcp_foundry_models_deployments_list` | ❌ |
| 14 | 0.287501 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.283532 | `azmcp_foundry_models_list` | ❌ |
| 16 | 0.269913 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.260161 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.254226 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.247670 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.244844 | `azmcp_kusto_sample` | ❌ |

---

## Test 14

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.505971 | `azmcp_search_index_get` | ❌ |
| 3 | 0.500455 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.494272 | `azmcp_monitor_workspace_list` | ❌ |
| 5 | 0.493011 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.492228 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.486066 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.482464 | `azmcp_grafana_list` | ❌ |
| 9 | 0.477459 | `azmcp_subscription_list` | ❌ |
| 10 | 0.470384 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.470055 | `azmcp_marketplace_product_list` | ❌ |
| 12 | 0.454460 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.451893 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.443495 | `azmcp_search_index_query` | ❌ |
| 15 | 0.427817 | `azmcp_group_list` | ❌ |
| 16 | 0.425463 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.418396 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.418007 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.417472 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.414984 | `azmcp_foundry_models_list` | ❌ |

---

## Test 15

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.479898 | `azmcp_search_index_get` | ❌ |
| 3 | 0.453489 | `azmcp_marketplace_product_list` | ❌ |
| 4 | 0.448446 | `azmcp_search_index_query` | ❌ |
| 5 | 0.425939 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.419493 | `azmcp_marketplace_product_get` | ❌ |
| 7 | 0.412158 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.408456 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.400229 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.399822 | `azmcp_grafana_list` | ❌ |
| 11 | 0.397883 | `azmcp_foundry_models_deployments_list` | ❌ |
| 12 | 0.393632 | `azmcp_subscription_list` | ❌ |
| 13 | 0.390660 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.390559 | `azmcp_foundry_models_list` | ❌ |
| 15 | 0.384559 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.379805 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 17 | 0.376963 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.376089 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.373463 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.363444 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 16

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553025 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.436230 | `azmcp_search_index_get` | ❌ |
| 3 | 0.404758 | `azmcp_search_index_query` | ❌ |
| 4 | 0.344699 | `azmcp_foundry_models_deployments_list` | ❌ |
| 5 | 0.336174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.322580 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 7 | 0.322540 | `azmcp_foundry_models_list` | ❌ |
| 8 | 0.300427 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.292677 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.290214 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.289466 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.283366 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.282198 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 14 | 0.281672 | `azmcp_get_bestpractices_get` | ❌ |
| 15 | 0.281134 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.278574 | `azmcp_cloudarchitect_design` | ❌ |
| 17 | 0.278531 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.277693 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.275013 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.274081 | `azmcp_monitor_table_type_list` | ❌ |

---

## Test 17

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
| 6 | 0.442214 | `azmcp_grafana_list` | ❌ |
| 7 | 0.441656 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.433594 | `azmcp_eventgrid_topic_list` | ❌ |
| 9 | 0.432238 | `azmcp_search_service_list` | ❌ |
| 10 | 0.427647 | `azmcp_subscription_list` | ❌ |
| 11 | 0.427460 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.420794 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.412274 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.408599 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.404636 | `azmcp_storage_table_list` | ❌ |
| 16 | 0.398419 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.387414 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.387179 | `azmcp_appconfig_kv_delete` | ❌ |
| 19 | 0.385938 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.380818 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 18

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
| 6 | 0.359567 | `azmcp_postgres_server_config_get` | ❌ |
| 7 | 0.356514 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.355830 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.354747 | `azmcp_appconfig_kv_delete` | ❌ |
| 10 | 0.348603 | `azmcp_appconfig_kv_set` | ❌ |
| 11 | 0.344550 | `azmcp_marketplace_product_get` | ❌ |
| 12 | 0.341263 | `azmcp_grafana_list` | ❌ |
| 13 | 0.340731 | `azmcp_eventgrid_topic_list` | ❌ |
| 14 | 0.332824 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.325886 | `azmcp_subscription_list` | ❌ |
| 16 | 0.325232 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.318639 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.310432 | `azmcp_search_service_list` | ❌ |
| 19 | 0.296589 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.292788 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 19

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
| 7 | 0.289682 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 8 | 0.282153 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.272373 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.255774 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.239130 | `azmcp_loadtesting_testrun_list` | ❌ |
| 12 | 0.236404 | `azmcp_deploy_app_logs_get` | ❌ |
| 13 | 0.234890 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.233357 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.231649 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.230963 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.228042 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.221645 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.221405 | `azmcp_postgres_database_list` | ❌ |
| 20 | 0.219535 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618276 | `azmcp_appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.486631 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.424344 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.399569 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.392016 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.268822 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.261524 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.248791 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.240483 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.218487 | `azmcp_mysql_server_param_set` | ❌ |
| 12 | 0.218373 | `azmcp_sql_server_delete` | ❌ |
| 13 | 0.214449 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.196121 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 15 | 0.194831 | `azmcp_postgres_server_config_get` | ❌ |
| 16 | 0.175403 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.173143 | `azmcp_postgres_server_param_set` | ❌ |
| 18 | 0.166763 | `azmcp_storage_account_get` | ❌ |
| 19 | 0.165140 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.141099 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 21

**Expected Tool:** `azmcp_appconfig_kv_list`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.730852 | `azmcp_appconfig_kv_list` | ✅ **EXPECTED** |
| 2 | 0.595054 | `azmcp_appconfig_kv_show` | ❌ |
| 3 | 0.557810 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.530884 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.464635 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.439089 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.377534 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.374522 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.338195 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.333355 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.327550 | `azmcp_loadtesting_testrun_list` | ❌ |
| 12 | 0.323615 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.321277 | `azmcp_keyvault_certificate_list` | ❌ |
| 14 | 0.317744 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.296084 | `azmcp_postgres_table_list` | ❌ |
| 16 | 0.292091 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.279679 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.275469 | `azmcp_mysql_server_param_set` | ❌ |
| 19 | 0.267026 | `azmcp_postgres_database_list` | ❌ |
| 20 | 0.264802 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 22

**Expected Tool:** `azmcp_appconfig_kv_list`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682275 | `azmcp_appconfig_kv_list` | ✅ **EXPECTED** |
| 2 | 0.606545 | `azmcp_appconfig_kv_show` | ❌ |
| 3 | 0.522426 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.512945 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.468503 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.457866 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.370520 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.356793 | `azmcp_mysql_server_param_get` | ❌ |
| 9 | 0.317662 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.314774 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.304557 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.294898 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.288088 | `azmcp_mysql_server_param_set` | ❌ |
| 14 | 0.278909 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.264526 | `azmcp_aks_nodepool_get` | ❌ |
| 16 | 0.258688 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.249931 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.243655 | `azmcp_postgres_server_param_set` | ❌ |
| 19 | 0.238151 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.236361 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 23

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591237 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.508804 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.445551 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.431516 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.423650 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.373656 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.253705 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.250729 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.238544 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.238242 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.211331 | `azmcp_postgres_server_config_get` | ❌ |
| 12 | 0.208119 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.186880 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.163738 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.158946 | `azmcp_mysql_server_param_get` | ❌ |
| 16 | 0.154529 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.144377 | `azmcp_servicebus_queue_details` | ❌ |
| 18 | 0.139871 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.123282 | `azmcp_storage_account_create` | ❌ |
| 20 | 0.119701 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 24

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555699 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.541557 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.476497 | `azmcp_appconfig_kv_delete` | ❌ |
| 4 | 0.435759 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.425488 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.409406 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.267551 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.259626 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.252818 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.237098 | `azmcp_mysql_server_param_set` | ❌ |
| 11 | 0.229474 | `azmcp_keyvault_secret_list` | ❌ |
| 12 | 0.225350 | `azmcp_postgres_server_config_get` | ❌ |
| 13 | 0.190136 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.185141 | `azmcp_postgres_server_param_set` | ❌ |
| 15 | 0.179797 | `azmcp_mysql_server_param_get` | ❌ |
| 16 | 0.171375 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.159767 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.150336 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.143564 | `azmcp_servicebus_queue_details` | ❌ |
| 20 | 0.135572 | `azmcp_workbooks_delete` | ❌ |

---

## Test 25

**Expected Tool:** `azmcp_appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609635 | `azmcp_appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536497 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 3 | 0.518499 | `azmcp_appconfig_kv_list` | ❌ |
| 4 | 0.507170 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.505571 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.377919 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.360015 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.346927 | `azmcp_postgres_server_param_set` | ❌ |
| 9 | 0.311433 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.305254 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.221138 | `azmcp_loadtesting_test_create` | ❌ |
| 12 | 0.220113 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.213592 | `azmcp_mysql_server_param_get` | ❌ |
| 14 | 0.208947 | `azmcp_postgres_server_config_get` | ❌ |
| 15 | 0.193989 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.167006 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.164376 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.155719 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 19 | 0.147883 | `azmcp_storage_queue_message_send` | ❌ |
| 20 | 0.137964 | `azmcp_storage_account_create` | ❌ |

---

## Test 26

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
| 6 | 0.416264 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.301947 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.291447 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.269387 | `azmcp_loadtesting_test_get` | ❌ |
| 10 | 0.260220 | `azmcp_keyvault_secret_list` | ❌ |
| 11 | 0.259549 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.257940 | `azmcp_mysql_server_param_get` | ❌ |
| 13 | 0.251822 | `azmcp_loadtesting_testrun_list` | ❌ |
| 14 | 0.229242 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.226217 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.217856 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.206401 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.205556 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.201872 | `azmcp_mysql_server_param_set` | ❌ |
| 20 | 0.186734 | `azmcp_sql_server_show` | ❌ |

---

## Test 27

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.355635 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.329345 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.257790 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.216077 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.206477 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.205255 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.177942 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 9 | 0.170352 | `azmcp_deploy_iac_rules_get` | ❌ |
| 10 | 0.169553 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.169415 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.163768 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.148018 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.141970 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.133096 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.128768 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.125735 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.120066 | `azmcp_mysql_table_schema_get` | ❌ |
| 19 | 0.116209 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.111730 | `azmcp_redis_cache_list` | ❌ |

---

## Test 28

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.318608 | `azmcp_deploy_app_logs_get` | ❌ |
| 2 | 0.302557 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.255570 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.225972 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.222226 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.200402 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.186927 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.172691 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.163364 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.157877 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.152905 | `azmcp_deploy_iac_rules_get` | ❌ |
| 12 | 0.150964 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.150313 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.144054 | `azmcp_mysql_server_param_get` | ❌ |
| 15 | 0.133109 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.125941 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.118881 | `azmcp_mysql_table_schema_get` | ❌ |
| 18 | 0.112992 | `azmcp_monitor_workspace_log_query` | ❌ |
| 19 | 0.107063 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.099087 | `azmcp_search_index_get` | ❌ |

---

## Test 29

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.256325 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 2 | 0.250546 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.215890 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.199067 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.188245 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.179320 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.178879 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.159064 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.158352 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.156599 | `azmcp_search_service_list` | ❌ |
| 11 | 0.156168 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.153379 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.151702 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.146689 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.139604 | `azmcp_postgres_server_param_get` | ❌ |
| 16 | 0.137230 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.130326 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.129424 | `azmcp_mysql_server_param_get` | ❌ |
| 19 | 0.126169 | `azmcp_search_index_get` | ❌ |
| 20 | 0.113627 | `azmcp_postgres_server_list` | ❌ |

---

## Test 30

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.541506 | `azmcp_search_service_list` | ❌ |
| 4 | 0.527457 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.515897 | `azmcp_subscription_list` | ❌ |
| 6 | 0.514293 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.509386 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.503032 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.490776 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.487556 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.483500 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.482499 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.482236 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.481761 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.480869 | `azmcp_group_list` | ❌ |
| 16 | 0.469958 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.462353 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.460523 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.460343 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.456503 | `azmcp_mysql_server_list` | ❌ |

---

## Test 31

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.450287 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.382728 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.372153 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.370858 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.364918 | `azmcp_search_service_list` | ❌ |
| 9 | 0.359177 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.356414 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.354526 | `azmcp_storage_blob_container_create` | ❌ |
| 12 | 0.353251 | `azmcp_subscription_list` | ❌ |
| 13 | 0.352818 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.349526 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.349291 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.348080 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.344749 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.344071 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.339252 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.336892 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 32

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637130 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563476 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.474000 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471804 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.463743 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.459880 | `azmcp_search_service_list` | ❌ |
| 7 | 0.452938 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.451253 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.443939 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.440409 | `azmcp_subscription_list` | ❌ |
| 11 | 0.435835 | `azmcp_grafana_list` | ❌ |
| 12 | 0.435706 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.431745 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.430867 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.430309 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.409093 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.404664 | `azmcp_group_list` | ❌ |
| 18 | 0.398557 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.395721 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.386496 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 33

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `azmcp_acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476015 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.454929 | `azmcp_group_list` | ❌ |
| 5 | 0.454003 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.446008 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.428000 | `azmcp_workbooks_list` | ❌ |
| 8 | 0.423541 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421030 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.411316 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.409133 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.404427 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.388773 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.371025 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.370359 | `azmcp_redis_cluster_database_list` | ❌ |
| 16 | 0.366482 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.356119 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.354145 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.352336 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.351949 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 34

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.468028 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.449649 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `azmcp_group_list` | ❌ |
| 6 | 0.416354 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.413975 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.406554 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.403623 | `azmcp_storage_blob_container_get` | ❌ |
| 10 | 0.400209 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.389603 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.378353 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.369912 | `azmcp_sql_elastic-pool_list` | ❌ |
| 14 | 0.369779 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.367734 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.362040 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.354807 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.351410 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.344148 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.343572 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 35

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.510435 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.495567 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.492550 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.475629 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.466001 | `azmcp_search_service_list` | ❌ |
| 8 | 0.461777 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.461368 | `azmcp_grafana_list` | ❌ |
| 10 | 0.456838 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.449239 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.448228 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.440019 | `azmcp_subscription_list` | ❌ |
| 14 | 0.438218 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.437551 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.430939 | `azmcp_group_list` | ❌ |
| 17 | 0.423301 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.414463 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.405472 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.390890 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 36

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546334 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.407973 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.400145 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.339307 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.326631 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.308650 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.306649 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.302635 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.300174 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.296073 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.293421 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.292155 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.290148 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.289864 | `azmcp_search_service_list` | ❌ |
| 16 | 0.283716 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.283390 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.282582 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.276498 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.273167 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 37

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.433927 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.388490 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.370375 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.359617 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.356901 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.355328 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.351007 | `azmcp_redis_cluster_database_list` | ❌ |
| 10 | 0.347437 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.347084 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.346850 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.340014 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.338404 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.337543 | `azmcp_keyvault_certificate_list` | ❌ |
| 16 | 0.332849 | `azmcp_keyvault_key_list` | ❌ |
| 17 | 0.332785 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.332704 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.332572 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.330046 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 38

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501842 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.418623 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.374628 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.359922 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.341511 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.335467 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.333318 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.324104 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.318706 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.316614 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.315414 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.314959 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.311692 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.309627 | `azmcp_search_service_list` | ❌ |
| 16 | 0.306052 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.304725 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.303931 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.300101 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.299629 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 39

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668480 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.615268 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.588682 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.587802 | `azmcp_subscription_list` | ❌ |
| 6 | 0.560795 | `azmcp_search_service_list` | ❌ |
| 7 | 0.538321 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.528963 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.516914 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.502428 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.502199 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.499161 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.497679 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.486978 | `azmcp_group_list` | ❌ |
| 15 | 0.483046 | `azmcp_grafana_list` | ❌ |
| 16 | 0.474934 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.473625 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.459502 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.459002 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.457497 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 40

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665447 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605357 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.571613 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.486033 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.467671 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.436319 | `azmcp_subscription_list` | ❌ |
| 7 | 0.431496 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 8 | 0.428477 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.427709 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.408659 | `azmcp_search_service_list` | ❌ |
| 11 | 0.397575 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.390141 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.389842 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.386108 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.383985 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.381323 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.379496 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.373667 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.358398 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.353926 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 41

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605125 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.566249 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.546426 | `azmcp_subscription_list` | ❌ |
| 5 | 0.535227 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.530175 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.527812 | `azmcp_search_service_list` | ❌ |
| 8 | 0.488006 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.466414 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.457584 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.456219 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.455017 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.453626 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.441136 | `azmcp_grafana_list` | ❌ |
| 15 | 0.438277 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.437740 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.437026 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.434623 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.433094 | `azmcp_postgres_server_list` | ❌ |
| 20 | 0.430336 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 42

**Expected Tool:** `azmcp_cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.605253 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.566854 | `azmcp_cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 3 | 0.477874 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.447757 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.445640 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.429363 | `azmcp_search_service_list` | ❌ |
| 7 | 0.399756 | `azmcp_search_index_query` | ❌ |
| 8 | 0.384335 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.378151 | `azmcp_kusto_query` | ❌ |
| 10 | 0.374844 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.372689 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.366503 | `azmcp_search_index_get` | ❌ |
| 13 | 0.358903 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.351331 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.340982 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.338041 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.334389 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.333252 | `azmcp_marketplace_product_list` | ❌ |
| 19 | 0.331041 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.308694 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 43

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852832 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681044 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.630659 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.581593 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.535260 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.527459 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.486357 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.448957 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.447538 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.439770 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.427614 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.424294 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.422159 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.421535 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.420253 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.411663 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.392929 | `azmcp_postgres_database_list` | ❌ |
| 18 | 0.378191 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.372115 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.368426 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 44

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789395 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.614220 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.562062 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.537286 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.521532 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 6 | 0.471018 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.449120 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.411703 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.398064 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.397969 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.397755 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.395513 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.392763 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.386806 | `azmcp_redis_cluster_database_list` | ❌ |
| 15 | 0.356317 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.355640 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.345665 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.325994 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.319603 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.318540 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 45

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668515 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.665298 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.573703 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.571319 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.555134 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.548066 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.526046 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.501477 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.471453 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.459194 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.450854 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.442540 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.418871 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.407722 | `azmcp_search_service_list` | ❌ |
| 16 | 0.406805 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.405791 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.397641 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.389032 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.387534 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 46

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.614572 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.538479 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.524838 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.505363 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.498206 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.497414 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.449759 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.447875 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.437993 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.408605 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.402767 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.396280 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.383928 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.379009 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.348999 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.344447 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.342424 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.339516 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 47

**Expected Tool:** `azmcp_kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482059 | `azmcp_kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.465370 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.457669 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.416762 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.378455 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.362958 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.361772 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.353792 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.351393 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.344871 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.344590 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.333244 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.332639 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.326472 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.326306 | `azmcp_search_index_get` | ❌ |
| 16 | 0.326052 | `azmcp_aks_nodepool_list` | ❌ |
| 17 | 0.319764 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.318755 | `azmcp_kusto_query` | ❌ |
| 19 | 0.318082 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.314617 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 48

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.651218 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.644037 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.549094 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.536049 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.509397 | `azmcp_grafana_list` | ❌ |
| 6 | 0.505912 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.492107 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.491278 | `azmcp_search_service_list` | ❌ |
| 9 | 0.487583 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.486395 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.460255 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.458754 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.451500 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.428236 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.427695 | `azmcp_subscription_list` | ❌ |
| 16 | 0.411791 | `azmcp_group_list` | ❌ |
| 17 | 0.410016 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.407832 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.403488 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.399251 | `azmcp_monitor_table_list` | ❌ |

---

## Test 49

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.437363 | `azmcp_redis_cluster_list` | ❌ |
| 2 | 0.391087 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.386126 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 4 | 0.359551 | `azmcp_kusto_database_list` | ❌ |
| 5 | 0.341628 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.338217 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.315113 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.303083 | `azmcp_grafana_list` | ❌ |
| 9 | 0.292838 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.287768 | `azmcp_kusto_sample` | ❌ |
| 11 | 0.285603 | `azmcp_kusto_query` | ❌ |
| 12 | 0.283331 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.279848 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.277014 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.275559 | `azmcp_mysql_database_query` | ❌ |
| 16 | 0.270931 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.265906 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.264112 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.264035 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.263226 | `azmcp_quota_usage_check` | ❌ |

---

## Test 50

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584053 | `azmcp_redis_cluster_list` | ❌ |
| 2 | 0.549797 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 3 | 0.471120 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.469663 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.464294 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.462945 | `azmcp_grafana_list` | ❌ |
| 7 | 0.446124 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.440326 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.434016 | `azmcp_search_service_list` | ❌ |
| 10 | 0.432048 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.396253 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.392541 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.386776 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.380006 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.377490 | `azmcp_kusto_query` | ❌ |
| 16 | 0.371012 | `azmcp_subscription_list` | ❌ |
| 17 | 0.368890 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.366262 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.365972 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.365323 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 51

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
| 12 | 0.404095 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.396061 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.379966 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.375535 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.363663 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.350214 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.334270 | `azmcp_grafana_list` | ❌ |
| 19 | 0.320622 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.318850 | `azmcp_kusto_query` | ❌ |

---

## Test 52

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
| 11 | 0.368012 | `azmcp_postgres_table_list` | ❌ |
| 12 | 0.362905 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.359378 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.344011 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.338777 | `azmcp_storage_table_list` | ❌ |
| 16 | 0.336104 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.334803 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.310919 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.309809 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.305756 | `azmcp_kusto_query` | ❌ |

---

## Test 53

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.381352 | `azmcp_kusto_query` | ✅ **EXPECTED** |
| 2 | 0.363594 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.363252 | `azmcp_kusto_sample` | ❌ |
| 4 | 0.349147 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.345798 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.334762 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.328646 | `azmcp_search_service_list` | ❌ |
| 8 | 0.328162 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.324763 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.319112 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.318883 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.314961 | `azmcp_monitor_table_type_list` | ❌ |
| 13 | 0.314919 | `azmcp_search_index_query` | ❌ |
| 14 | 0.308113 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.304014 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.302894 | `azmcp_postgres_table_list` | ❌ |
| 17 | 0.292086 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.264026 | `azmcp_grafana_list` | ❌ |
| 19 | 0.263049 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.259156 | `azmcp_marketplace_product_list` | ❌ |

---

## Test 54

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
| 11 | 0.341674 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.337281 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.329933 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.319239 | `azmcp_kusto_query` | ❌ |
| 15 | 0.318189 | `azmcp_postgres_table_list` | ❌ |
| 16 | 0.310201 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.285941 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.268160 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.249424 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.242112 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 55

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591668 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585237 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.556724 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.550007 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.521516 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.520802 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.517077 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.475496 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.464341 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.449655 | `azmcp_kusto_table_schema` | ❌ |
| 11 | 0.436518 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.433775 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.429278 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.412275 | `azmcp_kusto_sample` | ❌ |
| 15 | 0.410425 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.400099 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.380671 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.337427 | `azmcp_kusto_query` | ❌ |
| 19 | 0.330068 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.329669 | `azmcp_grafana_list` | ❌ |

---

## Test 56

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549884 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.524691 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.523432 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.494108 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.490717 | `azmcp_monitor_table_list` | ❌ |
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
| 19 | 0.314691 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.300285 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 57

**Expected Tool:** `azmcp_kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588152 | `azmcp_kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564311 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.527917 | `azmcp_mysql_table_schema_get` | ❌ |
| 4 | 0.445191 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.437466 | `azmcp_kusto_table_list` | ❌ |
| 6 | 0.432585 | `azmcp_kusto_sample` | ❌ |
| 7 | 0.413686 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.398632 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.387660 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.366346 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.366081 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.358088 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.357533 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.345263 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.343568 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.314616 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.298243 | `azmcp_kusto_query` | ❌ |
| 18 | 0.294840 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.282712 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.275795 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 58

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634056 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.623421 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534457 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.498918 | `azmcp_mysql_server_list` | ❌ |
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
| 20 | 0.266148 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 59

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588121 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.574089 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.483855 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.463244 | `azmcp_mysql_server_list` | ❌ |
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

## Test 60

**Expected Tool:** `azmcp_mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476423 | `azmcp_mysql_table_list` | ❌ |
| 2 | 0.455770 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.433392 | `azmcp_mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419859 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.409445 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.393876 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.345179 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.328744 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.320053 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.298681 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.291451 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.285803 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.279005 | `azmcp_kusto_query` | ❌ |
| 14 | 0.278067 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.264434 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.257657 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.237932 | `azmcp_marketplace_product_list` | ❌ |
| 18 | 0.230415 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.226519 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.225958 | `azmcp_grafana_list` | ❌ |

---

## Test 61

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
| 6 | 0.413226 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.398405 | `azmcp_sql_server_show` | ❌ |
| 8 | 0.391644 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.376750 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.374870 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.267903 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.252810 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.238583 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.232680 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.224212 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.215307 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.198967 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.180063 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.169461 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.168310 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 62

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678473 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.558177 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.554818 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.501199 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.482079 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.467807 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.458406 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.457318 | `azmcp_grafana_list` | ❌ |
| 9 | 0.451969 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.431642 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.431126 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.422584 | `azmcp_search_service_list` | ❌ |
| 13 | 0.416796 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.410134 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.403845 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.379322 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.377511 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.374451 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.365458 | `azmcp_group_list` | ❌ |
| 20 | 0.354490 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 63

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478518 | `azmcp_mysql_database_list` | ❌ |
| 2 | 0.474586 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
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
| 15 | 0.244750 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.235455 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.232383 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.224586 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.218116 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.216149 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 64

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.534266 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530210 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.464360 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.456616 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.441836 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.431914 | `azmcp_grafana_list` | ❌ |
| 8 | 0.419663 | `azmcp_search_service_list` | ❌ |
| 9 | 0.416021 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.412407 | `azmcp_mysql_database_query` | ❌ |
| 11 | 0.408235 | `azmcp_mysql_table_schema_get` | ❌ |
| 12 | 0.406576 | `azmcp_mysql_server_param_get` | ❌ |
| 13 | 0.399358 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.376596 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.375684 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.364016 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.356691 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.341439 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.341087 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.334813 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 65

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
| 8 | 0.292546 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.285663 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.285645 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.183735 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.160082 | `azmcp_appconfig_kv_list` | ❌ |
| 13 | 0.146290 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.137499 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.124274 | `azmcp_grafana_list` | ❌ |
| 16 | 0.120498 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 17 | 0.118505 | `azmcp_loadtesting_testrun_list` | ❌ |
| 18 | 0.117704 | `azmcp_applens_resource_diagnose` | ❌ |
| 19 | 0.117324 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.115886 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 66

**Expected Tool:** `azmcp_mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.390762 | `azmcp_mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `azmcp_mysql_server_param_get` | ❌ |
| 3 | 0.307496 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.298911 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.254180 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.253189 | `azmcp_mysql_table_schema_get` | ❌ |
| 7 | 0.246424 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.246019 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.238742 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.236453 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.112499 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.105825 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.094606 | `azmcp_loadtesting_testrun_update` | ❌ |
| 14 | 0.090695 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.090334 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.089483 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.088097 | `azmcp_loadtesting_test_create` | ❌ |
| 18 | 0.086308 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 19 | 0.082240 | `azmcp_loadtesting_testrun_create` | ❌ |
| 20 | 0.082129 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 67

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
| 6 | 0.447284 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.442053 | `azmcp_kusto_table_list` | ❌ |
| 8 | 0.431034 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.429975 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.418619 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.410273 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.401216 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.361477 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.335069 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.308385 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.268415 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.260118 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.253046 | `azmcp_grafana_list` | ❌ |
| 19 | 0.241263 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.239226 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 68

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
| 7 | 0.419905 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.403265 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.391670 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.385166 | `azmcp_postgres_table_schema_get` | ❌ |
| 11 | 0.382169 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.349435 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.342926 | `azmcp_kusto_table_schema` | ❌ |
| 14 | 0.319674 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.303999 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.281571 | `azmcp_kusto_sample` | ❌ |
| 17 | 0.236723 | `azmcp_grafana_list` | ❌ |
| 18 | 0.231173 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.214496 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.209943 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 69

**Expected Tool:** `azmcp_mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630623 | `azmcp_mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558306 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.545025 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.482505 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.457739 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.443955 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.407451 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.398102 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.372911 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.348909 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.347368 | `azmcp_postgres_server_config_get` | ❌ |
| 12 | 0.324675 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.307950 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.271938 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.268273 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.243861 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.239328 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.202788 | `azmcp_bicepschema_get` | ❌ |
| 19 | 0.194220 | `azmcp_grafana_list` | ❌ |
| 20 | 0.186518 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 70

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

## Test 71

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760033 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589784 | `azmcp_postgres_server_list` | ❌ |
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
| 17 | 0.235404 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.227995 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.223442 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.222503 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 72

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
| 17 | 0.236681 | `azmcp_marketplace_product_list` | ❌ |
| 18 | 0.236363 | `azmcp_grafana_list` | ❌ |
| 19 | 0.218677 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.217855 | `azmcp_kusto_sample` | ❌ |

---

## Test 73

**Expected Tool:** `azmcp_postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `azmcp_postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.599471 | `azmcp_postgres_server_param_get` | ❌ |
| 3 | 0.535229 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.535050 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.518574 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.463172 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.431476 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.394675 | `azmcp_postgres_database_query` | ❌ |
| 9 | 0.356774 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.337899 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.269224 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.233426 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.222849 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.220186 | `azmcp_loadtesting_test_get` | ❌ |
| 15 | 0.208314 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.189446 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.177778 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.175244 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.168322 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.160792 | `azmcp_grafana_list` | ❌ |

---

## Test 74

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
| 10 | 0.435298 | `azmcp_search_service_list` | ❌ |
| 11 | 0.416315 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.408673 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.406617 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.399056 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.397428 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.389191 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.373699 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.365995 | `azmcp_group_list` | ❌ |
| 19 | 0.362900 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.351894 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 75

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.607062 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.576348 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.522995 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.506171 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.409406 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.400088 | `azmcp_postgres_server_param_set` | ❌ |
| 8 | 0.372955 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.336934 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.336270 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.274763 | `azmcp_grafana_list` | ❌ |
| 12 | 0.260533 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.253264 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.245276 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.241835 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.239500 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.229842 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.227547 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.225295 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.219273 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 76

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
| 8 | 0.421577 | `azmcp_search_service_list` | ❌ |
| 9 | 0.414695 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.410719 | `azmcp_postgres_database_query` | ❌ |
| 11 | 0.403538 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.399866 | `azmcp_postgres_table_schema_get` | ❌ |
| 13 | 0.376954 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.362650 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.362557 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.360521 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.358408 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.338815 | `azmcp_marketplace_product_list` | ❌ |
| 19 | 0.334680 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.334101 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 77

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
| 17 | 0.145056 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.142387 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.140139 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.140104 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 78

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
| 10 | 0.227740 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.133385 | `azmcp_kusto_sample` | ❌ |
| 12 | 0.127120 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.123491 | `azmcp_kusto_table_schema` | ❌ |
| 14 | 0.118089 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.115151 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.113841 | `azmcp_grafana_list` | ❌ |
| 17 | 0.112605 | `azmcp_deploy_plan_get` | ❌ |
| 18 | 0.108485 | `azmcp_kusto_table_list` | ❌ |
| 19 | 0.102847 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.102139 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 79

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789883 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.750580 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.574931 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.519820 | `azmcp_postgres_table_schema_get` | ❌ |
| 5 | 0.501400 | `azmcp_postgres_server_config_get` | ❌ |
| 6 | 0.477689 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.449190 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.432813 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.430171 | `azmcp_postgres_server_param_get` | ❌ |
| 10 | 0.396688 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.394396 | `azmcp_monitor_table_list` | ❌ |
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

## Test 80

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

## Test 81

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
| 14 | 0.253767 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.253353 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.239225 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.212206 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.201673 | `azmcp_grafana_list` | ❌ |
| 19 | 0.185124 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.183782 | `azmcp_bicepschema_get` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686701 | `azmcp_deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.404891 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.392565 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.389603 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.354432 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.342594 | `azmcp_monitor_resource_log_query` | ❌ |
| 8 | 0.334992 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.334522 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.327028 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.325553 | `azmcp_extension_azqr` | ❌ |
| 12 | 0.320572 | `azmcp_aks_nodepool_get` | ❌ |
| 13 | 0.314965 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.314219 | `azmcp_functionapp_get` | ❌ |
| 15 | 0.307291 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.297642 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 17 | 0.288973 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.284916 | `azmcp_search_service_list` | ❌ |
| 19 | 0.284418 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.283342 | `azmcp_mysql_server_config_get` | ❌ |

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
| 6 | 0.430764 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.417333 | `azmcp_get_bestpractices_get` | ❌ |
| 8 | 0.371127 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.343117 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.322230 | `azmcp_extension_azqr` | ❌ |
| 11 | 0.316752 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.284401 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.264933 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.264060 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.263521 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.255084 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.250629 | `azmcp_search_service_list` | ❌ |
| 18 | 0.247818 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.244757 | `azmcp_subscription_list` | ❌ |
| 20 | 0.244423 | `azmcp_storage_account_get` | ❌ |

---

## Test 84

**Expected Tool:** `azmcp_deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529092 | `azmcp_deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.404829 | `azmcp_bicepschema_get` | ❌ |
| 3 | 0.391965 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.383210 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.341436 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.304788 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.278653 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.266851 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.266629 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.252977 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.236341 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.223979 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.219521 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.206928 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.202239 | `azmcp_mysql_table_schema_get` | ❌ |
| 16 | 0.201288 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.195422 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.191094 | `azmcp_storage_share_file_list` | ❌ |
| 19 | 0.188615 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.177819 | `azmcp_storage_blob_get` | ❌ |

---

## Test 85

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `azmcp_deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.448917 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.382240 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.375202 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.373363 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.350101 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.338439 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.322906 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.289166 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.240757 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.230063 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.222417 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.212237 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.211103 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.206262 | `azmcp_storage_queue_message_send` | ❌ |
| 17 | 0.203987 | `azmcp_sql_server_delete` | ❌ |
| 18 | 0.198696 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.195915 | `azmcp_workbooks_delete` | ❌ |
| 20 | 0.190899 | `azmcp_search_index_query` | ❌ |

---

## Test 86

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688055 | `azmcp_deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.432825 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.425393 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.421744 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.413718 | `azmcp_loadtesting_test_create` | ❌ |
| 9 | 0.393518 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.365874 | `azmcp_foundry_models_deploy` | ❌ |
| 11 | 0.312840 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.300643 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.299552 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.296623 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.277064 | `azmcp_workbooks_delete` | ❌ |
| 16 | 0.258195 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.252696 | `azmcp_workbooks_create` | ❌ |
| 18 | 0.249598 | `azmcp_storage_blob_container_create` | ❌ |
| 19 | 0.247818 | `azmcp_sql_server_delete` | ❌ |
| 20 | 0.247257 | `azmcp_storage_blob_upload` | ❌ |

---

## Test 87

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759178 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.545540 | `azmcp_search_service_list` | ❌ |
| 3 | 0.514189 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.496629 | `azmcp_subscription_list` | ❌ |
| 5 | 0.496002 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.492690 | `azmcp_group_list` | ❌ |
| 7 | 0.485584 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.484509 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.475667 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.475056 | `azmcp_monitor_workspace_list` | ❌ |
| 11 | 0.472764 | `azmcp_grafana_list` | ❌ |
| 12 | 0.470300 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.460569 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.442229 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.440619 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.439820 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.438287 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.422415 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.409123 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.407838 | `azmcp_acr_registry_list` | ❌ |

---

## Test 88

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691068 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.478333 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 3 | 0.475119 | `azmcp_search_service_list` | ❌ |
| 4 | 0.450712 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.441607 | `azmcp_kusto_cluster_list` | ❌ |
| 6 | 0.437153 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.431250 | `azmcp_subscription_list` | ❌ |
| 8 | 0.430494 | `azmcp_grafana_list` | ❌ |
| 9 | 0.428437 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.424907 | `azmcp_monitor_workspace_list` | ❌ |
| 11 | 0.420072 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.419125 | `azmcp_group_list` | ❌ |
| 13 | 0.408708 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.399253 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.396758 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.391338 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.381698 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.381664 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.374650 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.371656 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 89

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759396 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.526595 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.514248 | `azmcp_search_service_list` | ❌ |
| 4 | 0.495814 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.494153 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.481357 | `azmcp_group_list` | ❌ |
| 7 | 0.481065 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.476906 | `azmcp_subscription_list` | ❌ |
| 9 | 0.476808 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.471888 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.468200 | `azmcp_grafana_list` | ❌ |
| 12 | 0.466774 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.445991 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.429646 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.428727 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.428427 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.421431 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.417876 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.403613 | `azmcp_marketplace_product_list` | ❌ |
| 20 | 0.392039 | `azmcp_kusto_database_list` | ❌ |

---

## Test 90

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659232 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.609175 | `azmcp_group_list` | ❌ |
| 3 | 0.514613 | `azmcp_workbooks_list` | ❌ |
| 4 | 0.505966 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.484746 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.475467 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.464233 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.460455 | `azmcp_search_service_list` | ❌ |
| 9 | 0.456540 | `azmcp_grafana_list` | ❌ |
| 10 | 0.455379 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 11 | 0.452988 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.448196 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.442914 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.442259 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.432333 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.423027 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.416777 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.411811 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.407927 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.396990 | `azmcp_functionapp_get` | ❌ |

---

## Test 91

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.448179 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.390048 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.380314 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.379655 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.376542 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.373215 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.347628 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.347347 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.342763 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.341455 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.341448 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.338591 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.333091 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.328326 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.327808 | `azmcp_foundry_models_deployments_list` | ❌ |
| 17 | 0.323953 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.322437 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.317412 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.314172 | `azmcp_storage_account_get` | ❌ |

---

## Test 92

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607276 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447400 | `azmcp_mysql_server_config_get` | ❌ |
| 3 | 0.424693 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.422336 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.407133 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.397977 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.392852 | `azmcp_appconfig_kv_list` | ❌ |
| 8 | 0.384151 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.383957 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.369436 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.367183 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.363405 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.361753 | `azmcp_deploy_plan_get` | ❌ |
| 14 | 0.353601 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.342398 | `azmcp_postgres_server_config_get` | ❌ |
| 16 | 0.321697 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.315513 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.314100 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.312611 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.297223 | `azmcp_storage_blob_get` | ❌ |

---

## Test 93

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.460102 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.420189 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.390708 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.334473 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.322197 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.319431 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.317583 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.317359 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.312732 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.311384 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.309942 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.305419 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.297747 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.297135 | `azmcp_aks_nodepool_get` | ❌ |
| 16 | 0.295538 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.295174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.290156 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.281564 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.277653 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 94

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690933 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433989 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.432317 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.424646 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.419375 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.416967 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.396163 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.390827 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.389322 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.387898 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.383191 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.378811 | `azmcp_get_bestpractices_get` | ❌ |
| 13 | 0.376019 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.375267 | `azmcp_workbooks_show` | ❌ |
| 15 | 0.368506 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.366961 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.359784 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.348610 | `azmcp_foundry_models_deployments_list` | ❌ |
| 19 | 0.346255 | `azmcp_group_list` | ❌ |
| 20 | 0.341609 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 95

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.443459 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.441394 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.391480 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.383917 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.355527 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.353616 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.351217 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.349540 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.347266 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.344702 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.342868 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.337247 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.333000 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.331796 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.325680 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.320825 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.319736 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.318174 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.305803 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 96

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.445142 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.368188 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.366279 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.365569 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.363324 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.358624 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352754 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.350985 | `azmcp_aks_cluster_get` | ❌ |
| 10 | 0.350178 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.349596 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.349013 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.336938 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.335848 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.325909 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.323655 | `azmcp_foundry_models_deployments_list` | ❌ |
| 17 | 0.323377 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.322984 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.320487 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.317752 | `azmcp_sql_server_show` | ❌ |

---

## Test 97

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433959 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.388678 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.370793 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.368420 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.368018 | `azmcp_loadtesting_testrun_get` | ❌ |
| 7 | 0.367589 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.355956 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.355282 | `azmcp_search_index_get` | ❌ |
| 10 | 0.349891 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.349476 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.346974 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.344067 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.343381 | `azmcp_get_bestpractices_get` | ❌ |
| 15 | 0.342238 | `azmcp_servicebus_queue_details` | ❌ |
| 16 | 0.338127 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.337614 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.334256 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.326091 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.323978 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 98

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554980 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426703 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.418362 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.408011 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.381629 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.364785 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.350663 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.335606 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.325271 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.321466 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.318517 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.304263 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.303123 | `azmcp_loadtesting_test_create` | ❌ |
| 14 | 0.301769 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.301244 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 16 | 0.291130 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.281401 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.277967 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.276628 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 20 | 0.267170 | `azmcp_search_service_list` | ❌ |

---

## Test 99

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565797 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.440329 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.422774 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.384159 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.342552 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.333621 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.319464 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.318076 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.310635 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.298434 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.297073 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.292793 | `azmcp_cloudarchitect_design` | ❌ |
| 13 | 0.291911 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.283654 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.272348 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.270845 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.267009 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.266527 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.258431 | `azmcp_search_service_list` | ❌ |
| 20 | 0.249136 | `azmcp_storage_table_list` | ❌ |

---

## Test 100

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.559382 | `azmcp_search_service_list` | ❌ |
| 3 | 0.516618 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.516217 | `azmcp_appconfig_account_list` | ❌ |
| 5 | 0.485278 | `azmcp_subscription_list` | ❌ |
| 6 | 0.474425 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.465575 | `azmcp_group_list` | ❌ |
| 8 | 0.464534 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.455819 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.455389 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.451430 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.445099 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.442614 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.433245 | `azmcp_eventgrid_topic_list` | ❌ |
| 15 | 0.432144 | `azmcp_grafana_list` | ❌ |
| 16 | 0.431611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.415840 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.414796 | `azmcp_foundry_models_deployments_list` | ❌ |
| 19 | 0.413034 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.411904 | `azmcp_sql_db_list` | ❌ |

---

## Test 101

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.452132 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.412646 | `azmcp_search_service_list` | ❌ |
| 4 | 0.411323 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.385832 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.374655 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.372790 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.370393 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.369589 | `azmcp_subscription_list` | ❌ |
| 10 | 0.368004 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.358720 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.357329 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.347887 | `azmcp_mysql_database_list` | ❌ |
| 14 | 0.347802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.341159 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.339873 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.334019 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.333136 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.327870 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.326628 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 102

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433675 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.348106 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.284362 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.281676 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.249658 | `azmcp_appconfig_account_list` | ❌ |
| 6 | 0.244782 | `azmcp_appconfig_kv_list` | ❌ |
| 7 | 0.240729 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.239514 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.217775 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.208396 | `azmcp_foundry_models_list` | ❌ |
| 11 | 0.207392 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.197655 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.195857 | `azmcp_role_assignment_list` | ❌ |
| 14 | 0.194503 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.186328 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.184120 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.184051 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.182124 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.179069 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.178961 | `azmcp_search_service_list` | ❌ |

---

## Test 103

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.740306 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.596373 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.590531 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.575960 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.543043 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.526698 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.434721 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.414022 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.372045 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.330026 | `azmcp_appconfig_kv_set` | ❌ |
| 11 | 0.308666 | `azmcp_loadtesting_test_create` | ❌ |
| 12 | 0.300979 | `azmcp_storage_datalake_directory_create` | ❌ |
| 13 | 0.296644 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.285184 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.267718 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.236919 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.233821 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.223034 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.219482 | `azmcp_subscription_list` | ❌ |
| 20 | 0.217086 | `azmcp_search_service_list` | ❌ |

---

## Test 104

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627979 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.624457 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.564977 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.539554 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.493516 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.475385 | `azmcp_keyvault_secret_list` | ❌ |
| 7 | 0.423974 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.418861 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.390699 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.359750 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.346167 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.319163 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.317177 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.293431 | `azmcp_subscription_list` | ❌ |
| 15 | 0.289685 | `azmcp_search_service_list` | ❌ |
| 16 | 0.279695 | `azmcp_search_index_get` | ❌ |
| 17 | 0.276581 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.275920 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 19 | 0.271911 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.269735 | `azmcp_sql_db_show` | ❌ |

---

## Test 105

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662324 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.606534 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.540155 | `azmcp_keyvault_certificate_import` | ❌ |
| 4 | 0.535132 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.499333 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.482380 | `azmcp_keyvault_secret_list` | ❌ |
| 7 | 0.459167 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.419077 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.416122 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.412434 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.411136 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.368360 | `azmcp_search_index_get` | ❌ |
| 13 | 0.365386 | `azmcp_sql_db_show` | ❌ |
| 14 | 0.363560 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.350930 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.332770 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.331645 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.315096 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.305818 | `azmcp_subscription_list` | ❌ |
| 20 | 0.301710 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 106

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649993 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.521180 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.469706 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.467097 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.426359 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.398035 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.364892 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.337966 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.272263 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 10 | 0.267356 | `azmcp_appconfig_kv_set` | ❌ |
| 11 | 0.248212 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.240328 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 13 | 0.228508 | `azmcp_workbooks_delete` | ❌ |
| 14 | 0.222971 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.205023 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.200472 | `azmcp_storage_datalake_directory_create` | ❌ |
| 17 | 0.199045 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.181816 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.175113 | `azmcp_storage_share_file_list` | ❌ |
| 20 | 0.174606 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 107

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649676 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.629905 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.527468 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.525743 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.492128 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.472232 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.399885 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.377865 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.287107 | `azmcp_appconfig_kv_set` | ❌ |
| 10 | 0.271739 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 11 | 0.256832 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.250432 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.234377 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.233767 | `azmcp_workbooks_delete` | ❌ |
| 15 | 0.211454 | `azmcp_storage_datalake_directory_create` | ❌ |
| 16 | 0.211350 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.209234 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.203658 | `azmcp_sql_server_create` | ❌ |
| 19 | 0.197598 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.196937 | `azmcp_workbooks_create` | ❌ |

---

## Test 108

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.762015 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.637465 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.608799 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.566460 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.539592 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.484660 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.478100 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.453226 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.431201 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.429531 | `azmcp_storage_table_list` | ❌ |
| 11 | 0.424714 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.408093 | `azmcp_subscription_list` | ❌ |
| 13 | 0.394434 | `azmcp_search_service_list` | ❌ |
| 14 | 0.393940 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.363512 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.362873 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.358938 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.350862 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.339860 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.336779 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 109

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660576 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.570282 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.540133 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.516722 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.509087 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.483404 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.420506 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.397031 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.396455 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.390169 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.382082 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.372424 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.362835 | `azmcp_subscription_list` | ❌ |
| 14 | 0.355698 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.344466 | `azmcp_search_service_list` | ❌ |
| 16 | 0.323177 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.316157 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 18 | 0.309942 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.305651 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.295917 | `azmcp_quota_usage_check` | ❌ |

---

## Test 110

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677107 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.569250 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.555828 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.465770 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.417395 | `azmcp_keyvault_certificate_list` | ❌ |
| 6 | 0.413161 | `azmcp_keyvault_secret_list` | ❌ |
| 7 | 0.412581 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.397141 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.389769 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.372042 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.335194 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 12 | 0.287035 | `azmcp_storage_datalake_directory_create` | ❌ |
| 13 | 0.283851 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.276139 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.261794 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.252181 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.231837 | `azmcp_storage_queue_message_send` | ❌ |
| 18 | 0.230284 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.223726 | `azmcp_storage_blob_container_create` | ❌ |
| 20 | 0.215888 | `azmcp_subscription_list` | ❌ |

---

## Test 111

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737183 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.650155 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.631528 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.498767 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.473916 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.468044 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.467554 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.455805 | `azmcp_keyvault_certificate_get` | ❌ |
| 9 | 0.443785 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.439167 | `azmcp_appconfig_kv_list` | ❌ |
| 11 | 0.430322 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.428290 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.426986 | `azmcp_subscription_list` | ❌ |
| 14 | 0.408341 | `azmcp_search_service_list` | ❌ |
| 15 | 0.388016 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.378819 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 17 | 0.373903 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.368258 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.354970 | `azmcp_monitor_table_list` | ❌ |
| 20 | 0.353714 | `azmcp_redis_cache_list` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609507 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.535381 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.520010 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.479818 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.462603 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.429515 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.421475 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.412599 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.408423 | `azmcp_keyvault_certificate_import` | ❌ |
| 10 | 0.406776 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.405205 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.375139 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.357334 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.353428 | `azmcp_subscription_list` | ❌ |
| 15 | 0.327200 | `azmcp_search_service_list` | ❌ |
| 16 | 0.324789 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 17 | 0.316124 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.308976 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.306567 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.297022 | `azmcp_search_index_get` | ❌ |

---

## Test 113

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.767805 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.614547 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.572487 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.516568 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.461516 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.417642 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.411768 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.391171 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.387606 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 10 | 0.384490 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.370140 | `azmcp_keyvault_certificate_get` | ❌ |
| 12 | 0.321445 | `azmcp_storage_datalake_directory_create` | ❌ |
| 13 | 0.288149 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.288126 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.287283 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.285395 | `azmcp_storage_queue_message_send` | ❌ |
| 17 | 0.246179 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.243816 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.236560 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.218716 | `azmcp_sql_server_firewall-rule_create` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747343 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.617127 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.569911 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.519133 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.455500 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.433185 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.417973 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.414310 | `azmcp_keyvault_certificate_get` | ❌ |
| 9 | 0.410496 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.410468 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.392374 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.391122 | `azmcp_subscription_list` | ❌ |
| 13 | 0.388773 | `azmcp_search_service_list` | ❌ |
| 14 | 0.387663 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.367470 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.340472 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.337595 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.334206 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.331203 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.329507 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |

---

## Test 115

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615400 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.520715 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.502403 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.467743 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.456355 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.412129 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.410957 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.409126 | `azmcp_keyvault_certificate_import` | ❌ |
| 9 | 0.401434 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.385852 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.381603 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.371660 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.345302 | `azmcp_subscription_list` | ❌ |
| 14 | 0.344339 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.328354 | `azmcp_search_service_list` | ❌ |
| 16 | 0.315115 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 17 | 0.305225 | `azmcp_search_index_get` | ❌ |
| 18 | 0.303769 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.299023 | `azmcp_storage_account_create` | ❌ |
| 20 | 0.294614 | `azmcp_mysql_server_list` | ❌ |

---

## Test 116

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661280 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.611431 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.579676 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.540766 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.481416 | `azmcp_mysql_server_config_get` | ❌ |
| 6 | 0.463591 | `azmcp_kusto_cluster_get` | ❌ |
| 7 | 0.463065 | `azmcp_loadtesting_test_get` | ❌ |
| 8 | 0.430976 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.419629 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.399345 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.391924 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.390959 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.390819 | `azmcp_appconfig_kv_list` | ❌ |
| 14 | 0.390141 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.371630 | `azmcp_mysql_server_param_get` | ❌ |
| 16 | 0.370291 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.367841 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.360930 | `azmcp_storage_blob_get` | ❌ |
| 19 | 0.350240 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.340096 | `azmcp_mysql_server_list` | ❌ |

---

## Test 117

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.667153 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.589101 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.545820 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.530314 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.507985 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.461466 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.448796 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.428449 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.422993 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.413625 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.408421 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.396636 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.396256 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.385261 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.384654 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.382948 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.377793 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.366088 | `azmcp_search_index_get` | ❌ |
| 19 | 0.362332 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.359093 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567982 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563029 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.493940 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.486040 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.380301 | `azmcp_mysql_server_config_get` | ❌ |
| 6 | 0.368538 | `azmcp_kusto_cluster_get` | ❌ |
| 7 | 0.342696 | `azmcp_loadtesting_test_get` | ❌ |
| 8 | 0.340294 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.334923 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.334860 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.329324 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.315228 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.314513 | `azmcp_appconfig_kv_list` | ❌ |
| 14 | 0.309738 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.299046 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.296593 | `azmcp_postgres_server_config_get` | ❌ |
| 17 | 0.289342 | `azmcp_mysql_server_param_get` | ❌ |
| 18 | 0.275751 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.273195 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.265830 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 119

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661810 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.578662 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.563549 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.534089 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.503953 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.434587 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.433913 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.419339 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.418519 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.417836 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.405658 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.405015 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.402335 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.399512 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.391717 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.384782 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.376853 | `azmcp_search_index_get` | ❌ |
| 18 | 0.372812 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367547 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.359877 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801067 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.690255 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.599940 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.594509 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.562043 | `azmcp_search_service_list` | ❌ |
| 6 | 0.561039 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.543684 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.515922 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.509202 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.502341 | `azmcp_subscription_list` | ❌ |
| 11 | 0.498286 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 12 | 0.498121 | `azmcp_group_list` | ❌ |
| 13 | 0.495977 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.486141 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.483658 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.482355 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.481469 | `azmcp_grafana_list` | ❌ |
| 18 | 0.452958 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 19 | 0.452681 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.445271 | `azmcp_storage_table_list` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608056 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.536863 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.500890 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.492910 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.455228 | `azmcp_search_service_list` | ❌ |
| 6 | 0.446270 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.416475 | `azmcp_aks_nodepool_get` | ❌ |
| 8 | 0.409417 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.408385 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.392997 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.376362 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.371809 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.371535 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.370963 | `azmcp_search_index_get` | ❌ |
| 15 | 0.370237 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.363662 | `azmcp_subscription_list` | ❌ |
| 17 | 0.361928 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.358420 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.356926 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.356016 | `azmcp_storage_account_get` | ❌ |

---

## Test 122

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623896 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.538749 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.530148 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.466749 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.449602 | `azmcp_kusto_cluster_list` | ❌ |
| 6 | 0.416564 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.392083 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.378826 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.377567 | `azmcp_acr_registry_repository_list` | ❌ |
| 10 | 0.374585 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.364022 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.353365 | `azmcp_search_service_list` | ❌ |
| 13 | 0.345290 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.345196 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.341581 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.337354 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.317977 | `azmcp_sql_elastic-pool_list` | ❌ |
| 18 | 0.317238 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.312051 | `azmcp_subscription_list` | ❌ |
| 20 | 0.311971 | `azmcp_quota_usage_check` | ❌ |

---

## Test 123

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.753822 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.699118 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.597677 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.498436 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.482676 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.468425 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.463200 | `azmcp_sql_elastic-pool_list` | ❌ |
| 8 | 0.435039 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.414822 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 10 | 0.401693 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.399174 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.383607 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 13 | 0.382395 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.380163 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.378369 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.378270 | `azmcp_search_index_get` | ❌ |
| 17 | 0.370390 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.362454 | `azmcp_loadtesting_test_get` | ❌ |
| 19 | 0.357019 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.343424 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 124

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678158 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.640097 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.481668 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.458596 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.446021 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.440182 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.389989 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 8 | 0.384600 | `azmcp_loadtesting_test_get` | ❌ |
| 9 | 0.367456 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.365231 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.357721 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.350998 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.350992 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 14 | 0.344818 | `azmcp_sql_db_show` | ❌ |
| 15 | 0.343805 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.342564 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.338364 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.329963 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.322685 | `azmcp_appconfig_kv_show` | ❌ |
| 20 | 0.321672 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 125

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599506 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.582325 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.412841 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.391590 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.385173 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 6 | 0.383045 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.346262 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 8 | 0.338624 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.323027 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.320733 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.314439 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.306921 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.306579 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.300123 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.298866 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.289422 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.287084 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.283171 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.276058 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.266184 | `azmcp_sql_db_show` | ❌ |

---

## Test 126

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694117 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.615516 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.531972 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.506624 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.487707 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.461548 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.446699 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.440645 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.438636 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.435177 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.431369 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.418681 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 13 | 0.413085 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.404890 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.399249 | `azmcp_acr_registry_list` | ❌ |
| 16 | 0.393850 | `azmcp_group_list` | ❌ |
| 17 | 0.391869 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.389070 | `azmcp_redis_cluster_database_list` | ❌ |
| 19 | 0.385781 | `azmcp_workbooks_list` | ❌ |
| 20 | 0.379549 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 127

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712299 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.644451 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.547444 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.510269 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509732 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 6 | 0.498016 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.447545 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.441510 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.441482 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.433138 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.430830 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.430739 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.408990 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 14 | 0.408569 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.407619 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.390197 | `azmcp_redis_cluster_database_list` | ❌ |
| 17 | 0.388906 | `azmcp_group_list` | ❌ |
| 18 | 0.383234 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.382434 | `azmcp_search_service_list` | ❌ |
| 20 | 0.378671 | `azmcp_kusto_database_list` | ❌ |

---

## Test 128

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623139 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.580535 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.453744 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.443902 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.425448 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.409341 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.386949 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.378905 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.368944 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.363290 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.359493 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 12 | 0.356345 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.356139 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.354542 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.329036 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.324551 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.324257 | `azmcp_deploy_plan_get` | ❌ |
| 18 | 0.323568 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.322487 | `azmcp_foundry_models_deployments_list` | ❌ |
| 20 | 0.319684 | `azmcp_redis_cluster_database_list` | ❌ |

---

## Test 129

**Expected Tool:** `azmcp_loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585388 | `azmcp_loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.531362 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.508690 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.413857 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.394664 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.390081 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.346526 | `azmcp_loadtesting_testrun_update` | ❌ |
| 8 | 0.338668 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.338173 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.337311 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.323519 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.310455 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.310144 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.299149 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.296991 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.291016 | `azmcp_storage_queue_message_send` | ❌ |
| 17 | 0.290957 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.288940 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.280434 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.267790 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 130

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642509 | `azmcp_loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.608781 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.574473 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.534314 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.473513 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.470051 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.437142 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.404501 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.397434 | `azmcp_group_list` | ❌ |
| 10 | 0.379328 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.373328 | `azmcp_loadtesting_testrun_update` | ❌ |
| 12 | 0.369982 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.365471 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.360811 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.347074 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.341239 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.329326 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.322733 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.305900 | `azmcp_storage_account_create` | ❌ |
| 20 | 0.304215 | `azmcp_monitor_workspace_log_query` | ❌ |

---

## Test 131

**Expected Tool:** `azmcp_loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.717578 | `azmcp_loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.596828 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.514437 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.476662 | `azmcp_loadtesting_testrun_create` | ❌ |
| 5 | 0.443116 | `azmcp_loadtesting_test_get` | ❌ |
| 6 | 0.442167 | `azmcp_workbooks_create` | ❌ |
| 7 | 0.416885 | `azmcp_group_list` | ❌ |
| 8 | 0.407752 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.394967 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.382774 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.370093 | `azmcp_loadtesting_testrun_get` | ❌ |
| 12 | 0.369409 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.356801 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.350916 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.342213 | `azmcp_redis_cluster_list` | ❌ |
| 16 | 0.341251 | `azmcp_grafana_list` | ❌ |
| 17 | 0.335696 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.326618 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.326596 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.311892 | `azmcp_mysql_server_list` | ❌ |

---

## Test 132

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
| 8 | 0.511607 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.506184 | `azmcp_loadtesting_test_get` | ❌ |
| 10 | 0.487330 | `azmcp_grafana_list` | ❌ |
| 11 | 0.483681 | `azmcp_loadtesting_testrun_list` | ❌ |
| 12 | 0.473444 | `azmcp_search_service_list` | ❌ |
| 13 | 0.473287 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.470899 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.463466 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.458800 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.452190 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.447138 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.433793 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.426880 | `azmcp_sql_db_list` | ❌ |

---

## Test 133

**Expected Tool:** `azmcp_loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621803 | `azmcp_loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.592805 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.540789 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.530882 | `azmcp_loadtesting_testrun_update` | ❌ |
| 5 | 0.488142 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.469444 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.418431 | `azmcp_loadtesting_testrun_list` | ❌ |
| 8 | 0.411627 | `azmcp_loadtesting_testresource_list` | ❌ |
| 9 | 0.402120 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.383719 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.331209 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.325463 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.323772 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.314636 | `azmcp_storage_datalake_directory_create` | ❌ |
| 15 | 0.306420 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.272151 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.267552 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.266839 | `azmcp_storage_queue_message_send` | ❌ |
| 19 | 0.262686 | `azmcp_storage_blob_container_create` | ❌ |
| 20 | 0.253973 | `azmcp_monitor_workspace_log_query` | ❌ |

---

## Test 134

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625224 | `azmcp_loadtesting_test_get` | ❌ |
| 2 | 0.602985 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.568789 | `azmcp_loadtesting_testrun_get` | ✅ **EXPECTED** |
| 4 | 0.562111 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.535963 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.497031 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.434602 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.415771 | `azmcp_loadtesting_testrun_update` | ❌ |
| 9 | 0.397895 | `azmcp_group_list` | ❌ |
| 10 | 0.394651 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.370202 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.366550 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.356279 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.352883 | `azmcp_workbooks_show` | ❌ |
| 15 | 0.347044 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.329389 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 17 | 0.328891 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.315656 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.314380 | `azmcp_storage_account_create` | ❌ |
| 20 | 0.298958 | `azmcp_monitor_workspace_log_query` | ❌ |

---

## Test 135

**Expected Tool:** `azmcp_loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615977 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.606058 | `azmcp_loadtesting_test_get` | ❌ |
| 3 | 0.569145 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.565093 | `azmcp_loadtesting_testrun_list` | ✅ **EXPECTED** |
| 5 | 0.535207 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.492700 | `azmcp_loadtesting_testrun_create` | ❌ |
| 7 | 0.432149 | `azmcp_group_list` | ❌ |
| 8 | 0.416453 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.410933 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.406508 | `azmcp_loadtesting_test_create` | ❌ |
| 11 | 0.395915 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.392066 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.391147 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.356833 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.342588 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.340618 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.329464 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.328011 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.323927 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.323187 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659812 | `azmcp_loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.509199 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.454745 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.443828 | `azmcp_loadtesting_test_get` | ❌ |
| 5 | 0.422036 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.399536 | `azmcp_loadtesting_test_create` | ❌ |
| 7 | 0.384654 | `azmcp_loadtesting_testresource_list` | ❌ |
| 8 | 0.384237 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.320124 | `azmcp_workbooks_update` | ❌ |
| 10 | 0.300024 | `azmcp_workbooks_create` | ❌ |
| 11 | 0.268172 | `azmcp_workbooks_show` | ❌ |
| 12 | 0.267137 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.255408 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.253250 | `azmcp_functionapp_get` | ❌ |
| 15 | 0.250017 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.240916 | `azmcp_workbooks_delete` | ❌ |
| 17 | 0.232572 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.227941 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 19 | 0.227913 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.227194 | `azmcp_servicebus_topic_subscription_details` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578892 | `azmcp_grafana_list` | ✅ **EXPECTED** |
| 2 | 0.551851 | `azmcp_search_service_list` | ❌ |
| 3 | 0.513028 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.505836 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.498077 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.493645 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.492724 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.492212 | `azmcp_subscription_list` | ❌ |
| 9 | 0.491740 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.489846 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.482789 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.479611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.459137 | `azmcp_eventgrid_topic_list` | ❌ |
| 14 | 0.457845 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.447752 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.441315 | `azmcp_group_list` | ❌ |
| 17 | 0.440393 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.436802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.431917 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.422236 | `azmcp_acr_registry_list` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.516886 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.513156 | `azmcp_search_service_list` | ❌ |
| 5 | 0.510513 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 6 | 0.507980 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.500508 | `azmcp_subscription_list` | ❌ |
| 8 | 0.499290 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.495957 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.480850 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.477164 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.472811 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.460936 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.460346 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.451887 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.450971 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.447269 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.445430 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.442505 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.438952 | `azmcp_grafana_list` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743903 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.613217 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.519986 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.514120 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.492114 | `azmcp_acr_registry_repository_list` | ❌ |
| 6 | 0.475557 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 7 | 0.466545 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.452905 | `azmcp_acr_registry_list` | ❌ |
| 9 | 0.443767 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.441644 | `azmcp_group_list` | ❌ |
| 11 | 0.433933 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.412748 | `azmcp_search_service_list` | ❌ |
| 13 | 0.412709 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.410028 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.409044 | `azmcp_sql_elastic-pool_list` | ❌ |
| 16 | 0.407705 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.402926 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.398168 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.397223 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.393822 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_required-subnet-size`  
**Prompt:** Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646978 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ✅ **EXPECTED** |
| 2 | 0.450342 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.327359 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.235376 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.218167 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 6 | 0.205268 | `azmcp_storage_share_file_list` | ❌ |
| 7 | 0.204654 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.204313 | `azmcp_aks_nodepool_get` | ❌ |
| 9 | 0.203596 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.198992 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.192371 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.188378 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 13 | 0.186379 | `azmcp_storage_blob_get` | ❌ |
| 14 | 0.176407 | `azmcp_marketplace_product_get` | ❌ |
| 15 | 0.175883 | `azmcp_postgres_server_param_get` | ❌ |
| 16 | 0.174849 | `azmcp_aks_nodepool_list` | ❌ |
| 17 | 0.172920 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 18 | 0.169792 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 19 | 0.166628 | `azmcp_applens_resource_diagnose` | ❌ |
| 20 | 0.165173 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836071 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ✅ **EXPECTED** |
| 2 | 0.626238 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.453801 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.444792 | `azmcp_search_service_list` | ❌ |
| 5 | 0.438893 | `azmcp_quota_region_availability_list` | ❌ |
| 6 | 0.414696 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.411881 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 8 | 0.411221 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.410516 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 10 | 0.405913 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.403218 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.402635 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.401697 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.401538 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.399919 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.398684 | `azmcp_subscription_list` | ❌ |
| 17 | 0.395033 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.392601 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.392146 | `azmcp_marketplace_product_list` | ❌ |
| 20 | 0.388896 | `azmcp_monitor_metrics_definitions` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570145 | `azmcp_marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.477522 | `azmcp_marketplace_product_list` | ❌ |
| 3 | 0.353256 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.330935 | `azmcp_servicebus_queue_details` | ❌ |
| 5 | 0.324083 | `azmcp_search_index_get` | ❌ |
| 6 | 0.323704 | `azmcp_servicebus_topic_details` | ❌ |
| 7 | 0.317373 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.302241 | `azmcp_aks_cluster_get` | ❌ |
| 9 | 0.294798 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.289354 | `azmcp_workbooks_show` | ❌ |
| 11 | 0.285577 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.283554 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.276842 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.274403 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.269243 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.268104 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.266271 | `azmcp_foundry_models_list` | ❌ |
| 18 | 0.259116 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.257285 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.254318 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527078 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443133 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.343549 | `azmcp_search_service_list` | ❌ |
| 4 | 0.330500 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.328676 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 6 | 0.324866 | `azmcp_search_index_query` | ❌ |
| 7 | 0.290877 | `azmcp_get_bestpractices_get` | ❌ |
| 8 | 0.290185 | `azmcp_search_index_get` | ❌ |
| 9 | 0.287924 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.263954 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.263529 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.258244 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.254438 | `azmcp_applens_resource_diagnose` | ❌ |
| 14 | 0.251532 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.250343 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.248822 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 17 | 0.245634 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.245271 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.241894 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.232832 | `azmcp_redis_cache_list` | ❌ |

---

## Test 144

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461616 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385167 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.308769 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.260387 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.259270 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.238760 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.238238 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.237988 | `azmcp_grafana_list` | ❌ |
| 9 | 0.226689 | `azmcp_search_service_list` | ❌ |
| 10 | 0.221138 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.204870 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.204011 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.203695 | `azmcp_eventgrid_topic_list` | ❌ |
| 14 | 0.202641 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.202430 | `azmcp_appconfig_kv_list` | ❌ |
| 16 | 0.201780 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.187594 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.185326 | `azmcp_subscription_list` | ❌ |
| 19 | 0.181325 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.176224 | `azmcp_monitor_table_list` | ❌ |

---

## Test 145

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.635406 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.531728 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490235 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.447777 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.438801 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.354191 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.353355 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.351664 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.345046 | `azmcp_bicepschema_get` | ❌ |
| 12 | 0.322785 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.312391 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.312284 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.290398 | `azmcp_search_service_list` | ❌ |
| 16 | 0.282195 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.276297 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.273591 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.273557 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.271770 | `azmcp_storage_blob_get` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.548542 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.541091 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.516852 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.516443 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.424443 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.424017 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.409787 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.392171 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369205 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.356238 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.342488 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.306627 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.304620 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.304195 | `azmcp_search_service_list` | ❌ |
| 16 | 0.302423 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.302015 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.291071 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.290283 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.290179 | `azmcp_storage_blob_get` | ❌ |

---

## Test 147

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.594323 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.518643 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.465573 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.454158 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.430630 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.399433 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.392767 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.384118 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.380286 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.375863 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.362669 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.335296 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.330487 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.329342 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.322718 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.322570 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.316805 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.314841 | `azmcp_search_service_list` | ❌ |
| 20 | 0.314123 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 148

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.570488 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.522998 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.445382 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.400447 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.381822 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.368157 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.367714 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.339658 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.317494 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.292977 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.284879 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.278941 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.275342 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.256382 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.246412 | `azmcp_storage_queue_message_send` | ❌ |
| 18 | 0.241745 | `azmcp_search_index_query` | ❌ |
| 19 | 0.239443 | `azmcp_storage_blob_get` | ❌ |
| 20 | 0.239436 | `azmcp_search_service_list` | ❌ |

---

## Test 149

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581850 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.497350 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495659 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.486886 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.474511 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.439182 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.412001 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.399571 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.377790 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.373497 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.323164 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.317931 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.303572 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290695 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.277947 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.276228 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.270375 | `azmcp_search_service_list` | ❌ |
| 18 | 0.269415 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.269357 | `azmcp_storage_blob_container_create` | ❌ |
| 20 | 0.265176 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.532790 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.487322 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.458060 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.413150 | `azmcp_functionapp_get` | ❌ |
| 6 | 0.395940 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.394761 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.394214 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.375723 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.363596 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.332626 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.332015 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.307885 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290894 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.289643 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.289326 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.284975 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.284215 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.278669 | `azmcp_storage_queue_message_send` | ❌ |
| 20 | 0.275538 | `azmcp_search_index_query` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.513262 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.505123 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.405143 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.401209 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.398226 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.389556 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.334624 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.315627 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.312250 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.292282 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.283198 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.275830 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.258767 | `azmcp_search_index_query` | ❌ |
| 16 | 0.256751 | `azmcp_search_service_list` | ❌ |
| 17 | 0.254638 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.251387 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.249439 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.243086 | `azmcp_mysql_database_query` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.500368 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.472112 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.433134 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.385965 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.381179 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.374702 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.368831 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.358703 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.337023 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.293848 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.288873 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.282013 | `azmcp_storage_queue_message_send` | ❌ |
| 14 | 0.259723 | `azmcp_mysql_database_query` | ❌ |
| 15 | 0.253236 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.251235 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 17 | 0.249981 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.246347 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.240292 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.231234 | `azmcp_search_index_query` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a simple HTTP-triggered function app in javascript that returns a random compliment from a predefined list in a JSON response. And deploy it to azure eventually. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429170 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.408233 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.380754 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.377184 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.352369 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.345059 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.319970 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.311848 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.301028 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.299149 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.235579 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.232320 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.218912 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.216209 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.210908 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.203401 | `azmcp_search_index_query` | ❌ |
| 17 | 0.202251 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.197959 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.188682 | `azmcp_storage_queue_message_send` | ❌ |
| 20 | 0.186444 | `azmcp_sql_server_create` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497389 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.493053 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.405272 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.395675 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.385415 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.374258 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.354645 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.348026 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.300077 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.284329 | `azmcp_storage_blob_container_create` | ❌ |
| 11 | 0.266936 | `azmcp_foundry_models_deploy` | ❌ |
| 12 | 0.243440 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.234609 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.222205 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.218758 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.210706 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.209149 | `azmcp_workbooks_create` | ❌ |
| 18 | 0.208844 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.207144 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.195372 | `azmcp_sql_server_create` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498345 | `azmcp_monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.472094 | `azmcp_monitor_workspace_list` | ❌ |
| 3 | 0.468205 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.467848 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.463168 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.436971 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.418755 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.413358 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.401596 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.385416 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.380121 | `azmcp_grafana_list` | ❌ |
| 12 | 0.358786 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.342873 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.338992 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.333342 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.316587 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.315629 | `azmcp_search_service_list` | ❌ |
| 18 | 0.314296 | `azmcp_applens_resource_diagnose` | ❌ |
| 19 | 0.305738 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.297767 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 156

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592640 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424003 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.332356 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.315519 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.315310 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.311108 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 7 | 0.305464 | `azmcp_servicebus_queue_details` | ❌ |
| 8 | 0.304735 | `azmcp_grafana_list` | ❌ |
| 9 | 0.303453 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.298853 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.294124 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.287300 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.284519 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.283102 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.282547 | `azmcp_mysql_table_schema_get` | ❌ |
| 16 | 0.281090 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.277566 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.274784 | `azmcp_loadtesting_test_get` | ❌ |
| 19 | 0.262141 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.256957 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 157

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589859 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.587736 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.551156 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.542805 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.473421 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.472677 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.459829 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.439032 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.437739 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.431109 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.417098 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 12 | 0.414488 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.411580 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.403921 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.401826 | `azmcp_monitor_metrics_query` | ❌ |
| 16 | 0.397526 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.391340 | `azmcp_monitor_table_type_list` | ❌ |
| 18 | 0.390422 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.383412 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.378217 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633173 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495435 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.382374 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.380460 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.370848 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.359089 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.353264 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.344326 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.341713 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.337875 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.329534 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.324002 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.317475 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.302823 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.301967 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.300496 | `azmcp_search_service_list` | ❌ |
| 17 | 0.299337 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.291565 | `azmcp_cloudarchitect_design` | ❌ |
| 19 | 0.291260 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.287764 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 159

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555559 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.447639 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.447192 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.433802 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.422368 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.416135 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.409106 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.388146 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.380049 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.356527 | `azmcp_functionapp_get` | ❌ |
| 11 | 0.350100 | `azmcp_loadtesting_testrun_list` | ❌ |
| 12 | 0.341820 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.339752 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.335406 | `azmcp_monitor_metrics_definitions` | ❌ |
| 15 | 0.329472 | `azmcp_loadtesting_testresource_create` | ❌ |
| 16 | 0.326927 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.326796 | `azmcp_workbooks_show` | ❌ |
| 18 | 0.320831 | `azmcp_search_index_query` | ❌ |
| 19 | 0.307751 | `azmcp_search_service_list` | ❌ |
| 20 | 0.303766 | `azmcp_search_index_get` | ❌ |

---

## Test 160

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557875 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.508674 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.460611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.438233 | `azmcp_monitor_metrics_definitions` | ❌ |
| 6 | 0.392094 | `azmcp_monitor_resource_log_query` | ❌ |
| 7 | 0.391670 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.372998 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.368589 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.339388 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.336627 | `azmcp_loadtesting_testrun_get` | ❌ |
| 12 | 0.326899 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.326643 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.321538 | `azmcp_search_service_list` | ❌ |
| 15 | 0.318196 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.317565 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.303909 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.303638 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.296819 | `azmcp_search_index_query` | ❌ |
| 20 | 0.292994 | `azmcp_mysql_server_list` | ❌ |

---

## Test 161

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461266 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390029 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.306338 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.304372 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.301811 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.289461 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.275443 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.267683 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.267390 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 10 | 0.265702 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.263777 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.263325 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.259193 | `azmcp_grafana_list` | ❌ |
| 14 | 0.253542 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.248754 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.247872 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.247663 | `azmcp_applens_resource_diagnose` | ❌ |
| 18 | 0.245715 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.240441 | `azmcp_workbooks_list` | ❌ |
| 20 | 0.231174 | `azmcp_storage_blob_get` | ❌ |

---

## Test 162

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.491850 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.417008 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.415966 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.406200 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.398988 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.397335 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.366959 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.362030 | `azmcp_loadtesting_testrun_get` | ❌ |
| 9 | 0.359340 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.331730 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.316302 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.315326 | `azmcp_functionapp_get` | ❌ |
| 13 | 0.311842 | `azmcp_search_index_query` | ❌ |
| 14 | 0.308767 | `azmcp_monitor_metrics_definitions` | ❌ |
| 15 | 0.295918 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.293608 | `azmcp_search_service_list` | ❌ |
| 17 | 0.293312 | `azmcp_loadtesting_testresource_create` | ❌ |
| 18 | 0.287528 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.287126 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.272646 | `azmcp_search_index_get` | ❌ |

---

## Test 163

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525320 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.384482 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.376657 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.367167 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.299448 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.293034 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 7 | 0.290156 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.277697 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 9 | 0.272349 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.267076 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.266376 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.265480 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.262699 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.261986 | `azmcp_grafana_list` | ❌ |
| 15 | 0.261656 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.252301 | `azmcp_servicebus_queue_details` | ❌ |
| 17 | 0.251638 | `azmcp_search_index_query` | ❌ |
| 18 | 0.250995 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.246502 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.244147 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 164

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480436 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.381961 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.363411 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.359285 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.350523 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.350491 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.331215 | `azmcp_loadtesting_testresource_list` | ❌ |
| 8 | 0.330074 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.328839 | `azmcp_monitor_metrics_definitions` | ❌ |
| 10 | 0.324932 | `azmcp_search_index_query` | ❌ |
| 11 | 0.319343 | `azmcp_loadtesting_testresource_create` | ❌ |
| 12 | 0.317459 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.292195 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.290762 | `azmcp_search_service_list` | ❌ |
| 15 | 0.282267 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.278490 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.277213 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.276999 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.276023 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.265303 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 165

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594061 | `azmcp_monitor_workspace_log_query` | ❌ |
| 2 | 0.580123 | `azmcp_monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.472095 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.470074 | `azmcp_monitor_metrics_query` | ❌ |
| 5 | 0.443531 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.443012 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.392427 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.390147 | `azmcp_grafana_list` | ❌ |
| 9 | 0.366115 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.359081 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.352857 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.345377 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.344785 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.337853 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.333608 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.320761 | `azmcp_loadtesting_testrun_get` | ❌ |
| 17 | 0.307259 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.307133 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.305186 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.302797 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 166

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851134 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725777 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.620507 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.586681 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.534886 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.511101 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.502152 | `azmcp_grafana_list` | ❌ |
| 8 | 0.488548 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.443831 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.420419 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.419873 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.413918 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.409230 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.400127 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.397410 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.375217 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.374952 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.366056 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.365787 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.365504 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 167

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798459 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.599916 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.532887 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.497065 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.487237 | `azmcp_grafana_list` | ❌ |
| 7 | 0.466630 | `azmcp_kusto_table_list` | ❌ |
| 8 | 0.449407 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.427408 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.413678 | `azmcp_monitor_resource_log_query` | ❌ |
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

## Test 168

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765702 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.569921 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.525469 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.504683 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.477280 | `azmcp_grafana_list` | ❌ |
| 7 | 0.447435 | `azmcp_kusto_table_list` | ❌ |
| 8 | 0.445347 | `azmcp_mysql_table_schema_get` | ❌ |
| 9 | 0.418517 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.416351 | `azmcp_kusto_table_schema` | ❌ |
| 11 | 0.412293 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.404852 | `azmcp_monitor_workspace_log_query` | ❌ |
| 13 | 0.404192 | `azmcp_monitor_metrics_definitions` | ❌ |
| 14 | 0.395123 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 15 | 0.383606 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.380581 | `azmcp_kusto_sample` | ❌ |
| 17 | 0.369890 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.361819 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.354757 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.351333 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 169

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843139 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736837 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.576731 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.502461 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.481189 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.475734 | `azmcp_grafana_list` | ❌ |
| 7 | 0.451212 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.427934 | `azmcp_kusto_table_schema` | ❌ |
| 9 | 0.427153 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.421484 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.406242 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.391308 | `azmcp_kusto_sample` | ❌ |
| 13 | 0.387591 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.384679 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.376246 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.370860 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.367591 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.348357 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.340101 | `azmcp_foundry_models_list` | ❌ |
| 20 | 0.339804 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 170

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `azmcp_grafana_list` | ❌ |
| 3 | 0.660135 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.600802 | `azmcp_search_service_list` | ❌ |
| 5 | 0.583213 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.530433 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.517493 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.513663 | `azmcp_aks_cluster_list` | ❌ |
| 9 | 0.500768 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.494595 | `azmcp_group_list` | ❌ |
| 11 | 0.493745 | `azmcp_subscription_list` | ❌ |
| 12 | 0.487565 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.475212 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.471758 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.470266 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.467655 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.466748 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.448201 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.444214 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.439425 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 171

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.585436 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.531083 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474745 | `azmcp_monitor_workspace_log_query` | ❌ |
| 6 | 0.459841 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.444207 | `azmcp_search_service_list` | ❌ |
| 8 | 0.386422 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.383596 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.383041 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 11 | 0.380891 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.379597 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.373786 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.371395 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.363287 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.358029 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.354811 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.354276 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.352809 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.350239 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `azmcp_grafana_list` | ❌ |
| 3 | 0.580261 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.521317 | `azmcp_monitor_table_type_list` | ❌ |
| 5 | 0.521277 | `azmcp_search_service_list` | ❌ |
| 6 | 0.463378 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.453702 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.439297 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435475 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.428945 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.427183 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.422653 | `azmcp_subscription_list` | ❌ |
| 13 | 0.422379 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.413155 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.411648 | `azmcp_acr_registry_list` | ❌ |
| 16 | 0.411448 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.410082 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.404177 | `azmcp_group_list` | ❌ |
| 19 | 0.402600 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.395576 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 173

**Expected Tool:** `azmcp_monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591648 | `azmcp_monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.494715 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.485984 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.484159 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.483323 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.427241 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.375429 | `azmcp_monitor_metrics_query` | ❌ |
| 8 | 0.365704 | `azmcp_grafana_list` | ❌ |
| 9 | 0.330182 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.322875 | `azmcp_workbooks_delete` | ❌ |
| 11 | 0.322408 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 12 | 0.315638 | `azmcp_search_service_list` | ❌ |
| 13 | 0.309411 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.301564 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.299830 | `azmcp_applens_resource_diagnose` | ❌ |
| 16 | 0.292088 | `azmcp_loadtesting_testrun_list` | ❌ |
| 17 | 0.291669 | `azmcp_kusto_query` | ❌ |
| 18 | 0.288698 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.286717 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.283294 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 174

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668828 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.434813 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.413241 | `azmcp_monitor_metrics_query` | ❌ |
| 4 | 0.408658 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.401731 | `azmcp_grafana_list` | ❌ |
| 6 | 0.393318 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.386685 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.369805 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.364360 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.356643 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.355415 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.345409 | `azmcp_postgres_database_list` | ❌ |
| 13 | 0.345298 | `azmcp_group_list` | ❌ |
| 14 | 0.330769 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.327205 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.318192 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.317478 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.306977 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.304096 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.302405 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 175

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443837 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.393227 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.374071 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.371017 | `azmcp_grafana_list` | ❌ |
| 6 | 0.370681 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.359274 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.350656 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.343214 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.342468 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.337109 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.320510 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.319895 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.302947 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.300733 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.285253 | `azmcp_group_list` | ❌ |
| 17 | 0.285004 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.274589 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.272689 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.271043 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 176

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `azmcp_quota_usage_check` | ❌ |
| 2 | 0.497434 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.481143 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.476826 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.451690 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.440399 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.438386 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.434685 | `azmcp_search_service_list` | ❌ |
| 9 | 0.431096 | `azmcp_deploy_iac_rules_get` | ❌ |
| 10 | 0.423206 | `azmcp_subscription_list` | ❌ |
| 11 | 0.422293 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.417076 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 13 | 0.408023 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.406591 | `azmcp_deploy_plan_get` | ❌ |
| 15 | 0.400363 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.391633 | `azmcp_marketplace_product_get` | ❌ |
| 17 | 0.388980 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.383400 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.381209 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.354341 | `azmcp_redis_cache_list` | ❌ |

---

## Test 177

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.488377 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.473365 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.462743 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 6 | 0.452232 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.448086 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.442021 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.439040 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.426161 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.385771 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.382677 | `azmcp_search_service_list` | ❌ |
| 13 | 0.375693 | `azmcp_subscription_list` | ❌ |
| 14 | 0.375071 | `azmcp_marketplace_product_get` | ❌ |
| 15 | 0.365859 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.365824 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.360612 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.349469 | `azmcp_storage_account_get` | ❌ |
| 19 | 0.341827 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.334327 | `azmcp_mysql_server_list` | ❌ |

---

## Test 178

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.504673 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.494872 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.487387 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.481698 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.464304 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.463564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 9 | 0.463172 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.452811 | `azmcp_search_service_list` | ❌ |
| 11 | 0.433938 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.423397 | `azmcp_subscription_list` | ❌ |
| 13 | 0.417356 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.403533 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 15 | 0.398621 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.391476 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.380267 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.376262 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.374279 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.373844 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 179

**Expected Tool:** `azmcp_quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590878 | `azmcp_quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413274 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.372940 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369855 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.361386 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.349685 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.348742 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.337839 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.331145 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.331074 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.328408 | `azmcp_grafana_list` | ❌ |
| 12 | 0.325796 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.313240 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.310624 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.307143 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.290125 | `azmcp_group_list` | ❌ |
| 17 | 0.287104 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.278534 | `azmcp_search_service_list` | ❌ |
| 19 | 0.265092 | `azmcp_monitor_metrics_query` | ❌ |
| 20 | 0.263276 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 180

**Expected Tool:** `azmcp_quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609244 | `azmcp_quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `azmcp_quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383929 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.379029 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.365684 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.358215 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.351637 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.345156 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.342266 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.342232 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.338636 | `azmcp_grafana_list` | ❌ |
| 13 | 0.331262 | `azmcp_monitor_metrics_definitions` | ❌ |
| 14 | 0.322571 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.321805 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.312566 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.305083 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.304570 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.300419 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.299859 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 181

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645258 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.483988 | `azmcp_group_list` | ❌ |
| 3 | 0.483076 | `azmcp_subscription_list` | ❌ |
| 4 | 0.478700 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474795 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.471364 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.468597 | `azmcp_search_service_list` | ❌ |
| 8 | 0.460029 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.452819 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.446246 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 11 | 0.430667 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.427950 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.426624 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.425029 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.403310 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.398446 | `azmcp_eventgrid_topic_list` | ❌ |
| 17 | 0.397565 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.396961 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.374732 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.374646 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 182

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609704 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.456956 | `azmcp_grafana_list` | ❌ |
| 3 | 0.436630 | `azmcp_subscription_list` | ❌ |
| 4 | 0.435642 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.435155 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.431865 | `azmcp_search_service_list` | ❌ |
| 7 | 0.428663 | `azmcp_group_list` | ❌ |
| 8 | 0.428370 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.421627 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.420804 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.410235 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 12 | 0.406766 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.395445 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.388812 | `azmcp_marketplace_product_get` | ❌ |
| 15 | 0.386800 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.383635 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.373204 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.368511 | `azmcp_loadtesting_testresource_list` | ❌ |
| 19 | 0.363678 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.361677 | `azmcp_marketplace_product_list` | ❌ |

---

## Test 183

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757060 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.565047 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.445073 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.377563 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.322930 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.312428 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.307404 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.303736 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.303531 | `azmcp_appconfig_kv_list` | ❌ |
| 10 | 0.300024 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.298380 | `azmcp_keyvault_certificate_list` | ❌ |
| 12 | 0.296627 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.286490 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.285063 | `azmcp_search_service_list` | ❌ |
| 15 | 0.284898 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.284304 | `azmcp_grafana_list` | ❌ |
| 17 | 0.283818 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.280740 | `azmcp_loadtesting_testrun_list` | ❌ |
| 19 | 0.277576 | `azmcp_subscription_list` | ❌ |
| 20 | 0.274897 | `azmcp_role_assignment_list` | ❌ |

---

## Test 184

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713835 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.523152 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.412377 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.338859 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.286321 | `azmcp_appconfig_kv_list` | ❌ |
| 6 | 0.283725 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.280245 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.266409 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.264484 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.262084 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.258045 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.257957 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.257447 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.257151 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.249585 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.247853 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.246871 | `azmcp_grafana_list` | ❌ |
| 18 | 0.246847 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.240600 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.237037 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 185

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.764063 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.653924 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.501842 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.495048 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.472307 | `azmcp_grafana_list` | ❌ |
| 6 | 0.466141 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.464785 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.431968 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.431715 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.423066 | `azmcp_subscription_list` | ❌ |
| 11 | 0.414865 | `azmcp_search_service_list` | ❌ |
| 12 | 0.396295 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.381343 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.380443 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.373395 | `azmcp_group_list` | ❌ |
| 16 | 0.373274 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.368719 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.367794 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.367496 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.361464 | `azmcp_acr_registry_list` | ❌ |

---

## Test 186

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537885 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.450374 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 3 | 0.441104 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.401235 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.302323 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.283598 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.275986 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.265858 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.262106 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.257556 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.252070 | `azmcp_grafana_list` | ❌ |
| 12 | 0.246445 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.236096 | `azmcp_postgres_table_list` | ❌ |
| 14 | 0.233781 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.231294 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.225079 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.224084 | `azmcp_loadtesting_testrun_list` | ❌ |
| 18 | 0.217990 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.212420 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.210134 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 187

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692209 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.595721 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.461559 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.434924 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.427325 | `azmcp_grafana_list` | ❌ |
| 6 | 0.399303 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.383383 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.382294 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.361735 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.353419 | `azmcp_search_service_list` | ❌ |
| 11 | 0.353389 | `azmcp_subscription_list` | ❌ |
| 12 | 0.340764 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.327206 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.315565 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.310802 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.308807 | `azmcp_eventgrid_topic_list` | ❌ |
| 17 | 0.306356 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.305932 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.304064 | `azmcp_group_list` | ❌ |
| 20 | 0.303249 | `azmcp_storage_table_list` | ❌ |

---

## Test 188

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752920 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
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
| 12 | 0.395418 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.390449 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.385544 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.379937 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.376262 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.366236 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.328453 | `azmcp_aks_nodepool_list` | ❌ |
| 19 | 0.328081 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.324867 | `azmcp_grafana_list` | ❌ |

---

## Test 189

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
| 10 | 0.397284 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.369086 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.353712 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.351025 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.349880 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.343245 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 16 | 0.325416 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.318982 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.302228 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.294393 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.292180 | `azmcp_grafana_list` | ❌ |

---

## Test 190

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812960 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.679028 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.672104 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.588847 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.569222 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.554299 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.527406 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.503279 | `azmcp_grafana_list` | ❌ |
| 9 | 0.467957 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.462558 | `azmcp_search_service_list` | ❌ |
| 11 | 0.457790 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.455614 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.445496 | `azmcp_group_list` | ❌ |
| 14 | 0.445406 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.443534 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.442822 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 17 | 0.436397 | `azmcp_subscription_list` | ❌ |
| 18 | 0.419136 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.419075 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.411121 | `azmcp_mysql_server_list` | ❌ |

---

## Test 191

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591594 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.514374 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.467519 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.403282 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.385045 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 6 | 0.368011 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.337910 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.329389 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.322157 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.321180 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.305629 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.301603 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.295045 | `azmcp_grafana_list` | ❌ |
| 14 | 0.291684 | `azmcp_postgres_database_list` | ❌ |
| 15 | 0.288103 | `azmcp_aks_nodepool_list` | ❌ |
| 16 | 0.272503 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.261138 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.260993 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.259662 | `azmcp_postgres_server_config_get` | ❌ |
| 20 | 0.252053 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 192

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
| 6 | 0.491262 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.456252 | `azmcp_grafana_list` | ❌ |
| 8 | 0.446556 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.440660 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.400189 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 11 | 0.398399 | `azmcp_search_service_list` | ❌ |
| 12 | 0.394530 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.394483 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.389814 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.372221 | `azmcp_group_list` | ❌ |
| 16 | 0.370370 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.369831 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.368926 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.367955 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.362596 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 193

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
| 9 | 0.518520 | `azmcp_acr_registry_list` | ❌ |
| 10 | 0.517060 | `azmcp_loadtesting_testresource_list` | ❌ |
| 11 | 0.509454 | `azmcp_search_service_list` | ❌ |
| 12 | 0.500858 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.491176 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.490734 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.486716 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.479529 | `azmcp_subscription_list` | ❌ |
| 17 | 0.477800 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.477023 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.472171 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.432152 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 194

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.463685 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.462391 | `azmcp_mysql_server_list` | ❌ |
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
| 15 | 0.351404 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.350999 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.345595 | `azmcp_redis_cluster_database_list` | ❌ |
| 18 | 0.328487 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.326141 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.325359 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 195

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665772 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.531920 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.523088 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.522911 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.518543 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.515905 | `azmcp_grafana_list` | ❌ |
| 8 | 0.492945 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.487780 | `azmcp_acr_registry_list` | ❌ |
| 10 | 0.475708 | `azmcp_search_service_list` | ❌ |
| 11 | 0.470658 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.464637 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.460412 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.454711 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.454439 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.437392 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.435312 | `azmcp_subscription_list` | ❌ |
| 18 | 0.432994 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.429798 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.429564 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 196

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630647 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377586 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.349981 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.331563 | `azmcp_monitor_metrics_definitions` | ❌ |
| 6 | 0.330187 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.327691 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.325794 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.324331 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.322117 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.311577 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.308238 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.306616 | `azmcp_grafana_list` | ❌ |
| 14 | 0.292084 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.290698 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.287239 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 17 | 0.286287 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.285945 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.284990 | `azmcp_applens_resource_diagnose` | ❌ |
| 20 | 0.281679 | `azmcp_storage_account_get` | ❌ |

---

## Test 197

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549306 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.510357 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.492853 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.490090 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 5 | 0.466885 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.455902 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.412609 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.411283 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.405847 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.403899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.375351 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.368594 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 13 | 0.368262 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.349407 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.347885 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.337143 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 17 | 0.336357 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.321704 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.318472 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.311399 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 198

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577417 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 2 | 0.570719 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424888 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.393435 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.386467 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.373938 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.355487 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.352244 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.342314 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.337466 | `azmcp_aks_nodepool_get` | ❌ |
| 11 | 0.327279 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.321504 | `azmcp_group_list` | ❌ |
| 13 | 0.318422 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.318336 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.316340 | `azmcp_sql_server_show` | ❌ |
| 16 | 0.307190 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.307104 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.299338 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.298602 | `azmcp_monitor_metrics_definitions` | ❌ |
| 20 | 0.293375 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 199

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.587330 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.578620 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.563455 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.548549 | `azmcp_grafana_list` | ❌ |
| 6 | 0.540583 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.534299 | `azmcp_search_service_list` | ❌ |
| 8 | 0.531356 | `azmcp_quota_region_availability_list` | ❌ |
| 9 | 0.530985 | `azmcp_group_list` | ❌ |
| 10 | 0.507740 | `azmcp_monitor_workspace_list` | ❌ |
| 11 | 0.496651 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.491394 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.491258 | `azmcp_subscription_list` | ❌ |
| 14 | 0.484221 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.482623 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.476832 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.465422 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.462565 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.459718 | `azmcp_workbooks_list` | ❌ |
| 20 | 0.457237 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 200

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645036 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.587168 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.508248 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.473929 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.462129 | `azmcp_search_service_list` | ❌ |
| 6 | 0.441453 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.441452 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.430571 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 9 | 0.418993 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.409367 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.407189 | `azmcp_storage_blob_container_get` | ❌ |
| 12 | 0.406697 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.406458 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.405795 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.403364 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.401286 | `azmcp_monitor_metrics_query` | ❌ |
| 17 | 0.387809 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.381149 | `azmcp_get_bestpractices_get` | ❌ |
| 19 | 0.380845 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 20 | 0.379965 | `azmcp_azureterraformbestpractices_get` | ❌ |

---

## Test 201

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.543421 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.427639 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.420567 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.420388 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.411111 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.411059 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.374184 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.370961 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.363808 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.360039 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.358871 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.350454 | `azmcp_group_list` | ❌ |
| 14 | 0.349273 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.334774 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.330185 | `azmcp_extension_azqr` | ❌ |
| 17 | 0.320138 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.319481 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.309414 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.308680 | `azmcp_grafana_list` | ❌ |

---

## Test 202

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.719917 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `azmcp_search_service_list` | ❌ |
| 3 | 0.518372 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.503744 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.470139 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.456526 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.454448 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.446515 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 9 | 0.438813 | `azmcp_subscription_list` | ❌ |
| 10 | 0.427154 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.426698 | `azmcp_grafana_list` | ❌ |
| 12 | 0.419828 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.419011 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.416883 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.411902 | `azmcp_group_list` | ❌ |
| 16 | 0.407099 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.385382 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.378841 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.360279 | `azmcp_marketplace_product_list` | ❌ |
| 20 | 0.357116 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 203

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726947 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.513815 | `azmcp_search_service_list` | ❌ |
| 3 | 0.491121 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.484385 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.474835 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.459865 | `azmcp_subscription_list` | ❌ |
| 7 | 0.431455 | `azmcp_marketplace_product_get` | ❌ |
| 8 | 0.425644 | `azmcp_quota_region_availability_list` | ❌ |
| 9 | 0.411892 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 10 | 0.410579 | `azmcp_marketplace_product_list` | ❌ |
| 11 | 0.409027 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.404636 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.395147 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.390855 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.390483 | `azmcp_group_list` | ❌ |
| 16 | 0.390381 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.390329 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.386331 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.385710 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.384613 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 204

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.301604 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.270290 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.251870 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.216847 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211842 | `azmcp_search_service_list` | ❌ |
| 6 | 0.191890 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.189628 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.188665 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.187819 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.185941 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.174872 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.170157 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.169947 | `azmcp_servicebus_queue_details` | ❌ |
| 14 | 0.164622 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.163022 | `azmcp_monitor_workspace_log_query` | ❌ |
| 16 | 0.155791 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.155444 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.148855 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.147315 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.147023 | `azmcp_grafana_list` | ❌ |

---

## Test 205

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711107 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.520197 | `azmcp_search_service_list` | ❌ |
| 3 | 0.502064 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.487327 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.453380 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.451351 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.439658 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.436070 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.411793 | `azmcp_grafana_list` | ❌ |
| 10 | 0.408792 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.407696 | `azmcp_subscription_list` | ❌ |
| 12 | 0.406949 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.404980 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.391992 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.379017 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.371279 | `azmcp_group_list` | ❌ |
| 17 | 0.368866 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.367388 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.357139 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.356002 | `azmcp_marketplace_product_list` | ❌ |

---

## Test 206

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527706 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.437868 | `azmcp_search_service_list` | ❌ |
| 3 | 0.402493 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.400175 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.397735 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.382901 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.382581 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.375034 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.372005 | `azmcp_monitor_metrics_query` | ❌ |
| 10 | 0.363470 | `azmcp_get_bestpractices_get` | ❌ |
| 11 | 0.362214 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.360562 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.357531 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.341495 | `azmcp_foundry_models_deployments_list` | ❌ |
| 15 | 0.340315 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.338062 | `azmcp_search_index_get` | ❌ |
| 17 | 0.333382 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.333173 | `azmcp_subscription_list` | ❌ |
| 19 | 0.332391 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.331594 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 207

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `azmcp_servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.400870 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.382417 | `azmcp_storage_queue_message_send` | ❌ |
| 5 | 0.376027 | `azmcp_aks_cluster_get` | ❌ |
| 6 | 0.360755 | `azmcp_storage_blob_container_get` | ❌ |
| 7 | 0.352789 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.352705 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.351081 | `azmcp_search_index_get` | ❌ |
| 10 | 0.342349 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.337239 | `azmcp_sql_db_show` | ❌ |
| 12 | 0.332541 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.327611 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.323281 | `azmcp_marketplace_product_get` | ❌ |
| 15 | 0.322914 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.310612 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.309215 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.296380 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.290453 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.279367 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 208

**Expected Tool:** `azmcp_servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591649 | `azmcp_servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571860 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.494885 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.483976 | `azmcp_servicebus_queue_details` | ❌ |
| 5 | 0.365658 | `azmcp_search_index_get` | ❌ |
| 6 | 0.362127 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.352485 | `azmcp_marketplace_product_get` | ❌ |
| 8 | 0.341289 | `azmcp_loadtesting_testrun_get` | ❌ |
| 9 | 0.340036 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.337675 | `azmcp_storage_blob_get` | ❌ |
| 11 | 0.335522 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.333396 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.330814 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.326077 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.324869 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.317497 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.306388 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.297323 | `azmcp_grafana_list` | ❌ |
| 19 | 0.290383 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.287440 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 209

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `azmcp_servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.494515 | `azmcp_servicebus_queue_details` | ❌ |
| 3 | 0.457036 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.444604 | `azmcp_marketplace_product_get` | ❌ |
| 5 | 0.443973 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.429458 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.426768 | `azmcp_kusto_cluster_get` | ❌ |
| 8 | 0.421009 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.411293 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.409614 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.405380 | `azmcp_search_service_list` | ❌ |
| 12 | 0.404740 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.395789 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.395176 | `azmcp_grafana_list` | ❌ |
| 15 | 0.388049 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.382225 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.369986 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.369116 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.368155 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.367649 | `azmcp_group_list` | ❌ |

---

## Test 210

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639694 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.609178 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.602889 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.532407 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.529058 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.527896 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.486638 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.476350 | `azmcp_sql_server_delete` | ❌ |
| 10 | 0.475733 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.474927 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.466130 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.457314 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.441355 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.440528 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.400489 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.395036 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.394543 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.380402 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.367404 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 211

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.609322 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.557353 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.553488 | `azmcp_mysql_server_config_get` | ❌ |
| 5 | 0.524274 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.471862 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.461650 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.458741 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.445291 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.443384 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.439752 | `azmcp_sql_server_delete` | ❌ |
| 12 | 0.387645 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.380428 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.357318 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.354580 | `azmcp_aks_nodepool_list` | ❌ |
| 16 | 0.349880 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.347075 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.342792 | `azmcp_appconfig_kv_list` | ❌ |
| 19 | 0.342517 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.341681 | `azmcp_kusto_table_list` | ❌ |

---

## Test 212

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610991 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.593150 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.530422 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.528136 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465693 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.446682 | `azmcp_postgres_server_param_get` | ❌ |
| 7 | 0.438925 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.398181 | `azmcp_mysql_table_schema_get` | ❌ |
| 9 | 0.397510 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.374999 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.371413 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.325945 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.325788 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.320677 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.297839 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.294987 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.281546 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.279952 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 19 | 0.273351 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.273315 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 213

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530095 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503681 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.440073 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.438622 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.432919 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.421862 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.400963 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.398714 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.375668 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.361500 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.344694 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.337996 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.323587 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.300629 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.300133 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.296827 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.291629 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.285619 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.268405 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.265545 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 214

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678124 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.498367 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.479044 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.473539 | `azmcp_aks_nodepool_list` | ❌ |
| 6 | 0.454426 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.450777 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.441264 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 9 | 0.434570 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.431174 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.429007 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 12 | 0.416273 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.394548 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.394338 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.370652 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.363579 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.357347 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.352050 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.351647 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.349490 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 215

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606425 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.457164 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.438522 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.432816 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.429793 | `azmcp_aks_nodepool_get` | ❌ |
| 7 | 0.423047 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.419753 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.400026 | `azmcp_mysql_server_param_get` | ❌ |
| 10 | 0.383940 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.378556 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.372423 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.335615 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.333099 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.319836 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.317886 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.304601 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.304317 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.298907 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.298264 | `azmcp_acr_registry_list` | ❌ |

---

## Test 216

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592709 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420325 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.402616 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.397670 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.397640 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.386833 | `azmcp_aks_nodepool_list` | ❌ |
| 7 | 0.378527 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.365129 | `azmcp_aks_nodepool_get` | ❌ |
| 9 | 0.357516 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.350723 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 11 | 0.344799 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.344468 | `azmcp_mysql_server_param_get` | ❌ |
| 13 | 0.342703 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.321778 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.298933 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.292566 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.284157 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.281680 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.272025 | `azmcp_monitor_metrics_definitions` | ❌ |
| 20 | 0.259325 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 217

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682605 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.537173 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.482102 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.473675 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.464987 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.451815 | `azmcp_loadtesting_testresource_create` | ❌ |
| 7 | 0.449920 | `azmcp_sql_server_show` | ❌ |
| 8 | 0.449757 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.418811 | `azmcp_sql_elastic-pool_list` | ❌ |
| 10 | 0.416802 | `azmcp_workbooks_create` | ❌ |
| 11 | 0.402511 | `azmcp_keyvault_secret_create` | ❌ |
| 12 | 0.400940 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.397363 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.353383 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 15 | 0.335788 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.332831 | `azmcp_extension_azqr` | ❌ |
| 17 | 0.326862 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.323405 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.319939 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.317946 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 218

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618309 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.472463 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.434810 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.396073 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.369890 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.368537 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.368115 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.360875 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.354438 | `azmcp_sql_elastic-pool_list` | ❌ |
| 10 | 0.352219 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.349584 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 12 | 0.349312 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.337557 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.324021 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 15 | 0.316772 | `azmcp_loadtesting_test_create` | ❌ |
| 16 | 0.315987 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.301131 | `azmcp_deploy_plan_get` | ❌ |
| 18 | 0.300788 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.297351 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.278419 | `azmcp_azureterraformbestpractices_get` | ❌ |

---

## Test 219

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589818 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.469425 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.442934 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.423887 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.421502 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.417608 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.416146 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.389609 | `azmcp_sql_elastic-pool_list` | ❌ |
| 9 | 0.385267 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.369631 | `azmcp_workbooks_create` | ❌ |
| 11 | 0.341142 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 12 | 0.332965 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.317538 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.312657 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.303168 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.300992 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.298321 | `azmcp_group_list` | ❌ |
| 18 | 0.288584 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.284845 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.277880 | `azmcp_acr_registry_list` | ❌ |

---

## Test 220

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702352 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.495550 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.483132 | `azmcp_workbooks_delete` | ❌ |
| 4 | 0.470205 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.449008 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.448515 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.438950 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.417035 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 9 | 0.402684 | `azmcp_sql_elastic-pool_list` | ❌ |
| 10 | 0.346442 | `azmcp_functionapp_get` | ❌ |
| 11 | 0.333270 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.327099 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.323460 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.317588 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.317257 | `azmcp_group_list` | ❌ |
| 16 | 0.307426 | `azmcp_appconfig_kv_delete` | ❌ |
| 17 | 0.305136 | `azmcp_monitor_metrics_query` | ❌ |
| 18 | 0.290106 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.273131 | `azmcp_loadtesting_testresource_create` | ❌ |
| 20 | 0.268748 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 221

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429140 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393885 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.376660 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.309280 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 5 | 0.306368 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.299760 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.295963 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.295073 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 9 | 0.276930 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.263894 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.258333 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.235107 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.234779 | `azmcp_appconfig_kv_delete` | ❌ |
| 14 | 0.234376 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.226945 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.225579 | `azmcp_grafana_list` | ❌ |
| 17 | 0.219702 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.210483 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.207513 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.207236 | `azmcp_marketplace_product_list` | ❌ |

---

## Test 222

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527930 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.362389 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 3 | 0.341503 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.315820 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.274818 | `azmcp_sql_server_create` | ❌ |
| 6 | 0.262381 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.261688 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 8 | 0.254391 | `azmcp_appconfig_kv_delete` | ❌ |
| 9 | 0.247364 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.237815 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.235278 | `azmcp_mysql_database_query` | ❌ |
| 12 | 0.168042 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.164350 | `azmcp_loadtesting_testrun_update` | ❌ |
| 14 | 0.159907 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.156253 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.148272 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.146243 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.142127 | `azmcp_kusto_query` | ❌ |
| 19 | 0.140290 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.125251 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 223

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783479 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.456051 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.401908 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 4 | 0.376055 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.365636 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.352607 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.344454 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.343559 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.329397 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.328737 | `azmcp_role_assignment_list` | ❌ |
| 11 | 0.280450 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.258095 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.249297 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.249153 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.246563 | `azmcp_keyvault_secret_list` | ❌ |
| 16 | 0.245267 | `azmcp_group_list` | ❌ |
| 17 | 0.238154 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.234681 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.233337 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.227804 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 224

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713306 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.413144 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.315966 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.311085 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.304891 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 6 | 0.303560 | `azmcp_postgres_server_config_get` | ❌ |
| 7 | 0.289764 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.287372 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.283806 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.273940 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.214529 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.197679 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.194313 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.193050 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.191538 | `azmcp_appconfig_kv_list` | ❌ |
| 16 | 0.188120 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.183184 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.182237 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.180494 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.178625 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 225

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646419 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.356025 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.307823 | `azmcp_sql_server_create` | ❌ |
| 4 | 0.253610 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.236850 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.236050 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.228196 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.222003 | `azmcp_sql_elastic-pool_list` | ❌ |
| 9 | 0.221683 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.220421 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 11 | 0.212644 | `azmcp_cloudarchitect_design` | ❌ |
| 12 | 0.200387 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.189941 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.188287 | `azmcp_deploy_plan_get` | ❌ |
| 15 | 0.180995 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.180556 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.174553 | `azmcp_deploy_iac_rules_get` | ❌ |
| 18 | 0.169345 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.167872 | `azmcp_get_bestpractices_get` | ❌ |
| 20 | 0.165162 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 226

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635467 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532712 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448822 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.423223 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.403858 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.335670 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.318374 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.313690 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.311149 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.308626 | `azmcp_sql_db_show` | ❌ |
| 12 | 0.302973 | `azmcp_mysql_server_config_get` | ❌ |
| 13 | 0.296495 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.290296 | `azmcp_deploy_iac_rules_get` | ❌ |
| 15 | 0.288030 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.265059 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 17 | 0.260209 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.253771 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.242716 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.241513 | `azmcp_get_bestpractices_get` | ❌ |

---

## Test 227

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670181 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533515 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503636 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.294977 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.287378 | `azmcp_sql_server_create` | ❌ |
| 6 | 0.271174 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.252956 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 8 | 0.248832 | `azmcp_postgres_server_param_set` | ❌ |
| 9 | 0.230709 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.226436 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.222105 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 12 | 0.179146 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.174948 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.174548 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.166810 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.158162 | `azmcp_keyvault_certificate_create` | ❌ |
| 17 | 0.156741 | `azmcp_keyvault_key_create` | ❌ |
| 18 | 0.149856 | `azmcp_kusto_query` | ❌ |
| 19 | 0.143646 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.140507 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 228

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685144 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574360 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539644 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428919 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.356377 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.339834 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.321907 | `azmcp_keyvault_secret_create` | ❌ |
| 8 | 0.302226 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.284505 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.281010 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.270367 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 12 | 0.259457 | `azmcp_mysql_server_param_set` | ❌ |
| 13 | 0.257525 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.248922 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.221031 | `azmcp_deploy_iac_rules_get` | ❌ |
| 16 | 0.219209 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.209355 | `azmcp_loadtesting_testrun_create` | ❌ |
| 18 | 0.207350 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.197089 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.196521 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 229

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.543857 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.540333 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.527546 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.418504 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.410574 | `azmcp_workbooks_delete` | ❌ |
| 7 | 0.341915 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.325375 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.320916 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.315063 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.312054 | `azmcp_appconfig_kv_delete` | ❌ |
| 12 | 0.263959 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.245270 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 14 | 0.241564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 15 | 0.235226 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.231494 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.225236 | `azmcp_keyvault_certificate_get` | ❌ |
| 18 | 0.225227 | `azmcp_kusto_query` | ❌ |
| 19 | 0.222917 | `azmcp_monitor_metrics_query` | ❌ |
| 20 | 0.222048 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 230

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670179 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574340 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.530419 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.398706 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.310449 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.259110 | `azmcp_workbooks_delete` | ❌ |
| 7 | 0.254974 | `azmcp_appconfig_kv_delete` | ❌ |
| 8 | 0.251005 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 9 | 0.231881 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.231117 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.230343 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.182013 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.166475 | `azmcp_loadtesting_testrun_update` | ❌ |
| 14 | 0.158025 | `azmcp_kusto_query` | ❌ |
| 15 | 0.156028 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.152458 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.152084 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 18 | 0.149578 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.145688 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.143785 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 231

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671211 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601231 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.577330 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.441605 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.367883 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.293303 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.291409 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 8 | 0.284391 | `azmcp_workbooks_delete` | ❌ |
| 9 | 0.275957 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.267631 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.252095 | `azmcp_appconfig_kv_delete` | ❌ |
| 12 | 0.222155 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.204194 | `azmcp_loadtesting_testrun_update` | ❌ |
| 14 | 0.185585 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.185007 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.183545 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.181757 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 18 | 0.180405 | `azmcp_kusto_query` | ❌ |
| 19 | 0.179886 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.178835 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 232

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729372 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549667 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.392512 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 6 | 0.385148 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.359228 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.356700 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.355202 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.350241 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.304958 | `azmcp_keyvault_secret_list` | ❌ |
| 12 | 0.278098 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.277411 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.276828 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.270667 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.263181 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.256310 | `azmcp_aks_nodepool_list` | ❌ |
| 18 | 0.253852 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.248780 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 20 | 0.242359 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 233

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630731 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.524126 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410680 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.316854 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 6 | 0.312035 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.298995 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.294466 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.293371 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.292608 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.225372 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.210531 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.206761 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.206476 | `azmcp_deploy_iac_rules_get` | ❌ |
| 15 | 0.206114 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.197711 | `azmcp_kusto_sample` | ❌ |
| 17 | 0.195864 | `azmcp_aks_nodepool_list` | ❌ |
| 18 | 0.191177 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.189871 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.189786 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 234

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630546 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532454 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473501 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412957 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.308004 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 6 | 0.305701 | `azmcp_mysql_server_param_get` | ❌ |
| 7 | 0.304314 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.282537 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.277628 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.273281 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.202425 | `azmcp_deploy_iac_rules_get` | ❌ |
| 12 | 0.200326 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.191165 | `azmcp_cloudarchitect_design` | ❌ |
| 14 | 0.177454 | `azmcp_loadtesting_test_get` | ❌ |
| 15 | 0.176225 | `azmcp_get_bestpractices_get` | ❌ |
| 16 | 0.173184 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.172370 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.171465 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 19 | 0.171335 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.167153 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 235

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `azmcp_sql_db_show` | ❌ |
| 2 | 0.595184 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.559893 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.540218 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.491401 | `azmcp_sql_server_create` | ❌ |
| 6 | 0.488317 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.481847 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.480067 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.478713 | `azmcp_sql_elastic-pool_list` | ❌ |
| 10 | 0.450375 | `azmcp_aks_cluster_get` | ❌ |
| 11 | 0.445602 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.445391 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.437447 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.424890 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.410380 | `azmcp_group_list` | ❌ |
| 16 | 0.400396 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.393946 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.389203 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.385318 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.383563 | `azmcp_acr_registry_list` | ❌ |

---

## Test 236

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.538034 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.471541 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.445430 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.443977 | `azmcp_mysql_server_param_get` | ❌ |
| 7 | 0.422646 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.413964 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 9 | 0.406630 | `azmcp_loadtesting_test_get` | ❌ |
| 10 | 0.400827 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.379960 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.359926 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.349963 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.316818 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.314864 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.308718 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.299904 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.298409 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.295903 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.284481 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 237

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.380021 | `azmcp_postgres_server_param_get` | ❌ |
| 4 | 0.372194 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370539 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.368788 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.367031 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.363268 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.357960 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.352375 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.288829 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.276327 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.271945 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.268920 | `azmcp_loadtesting_testrun_get` | ❌ |
| 15 | 0.257258 | `azmcp_appconfig_kv_list` | ❌ |
| 16 | 0.253925 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.246261 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.240682 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.237909 | `azmcp_keyvault_certificate_get` | ❌ |
| 20 | 0.236912 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 238

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.418473 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.394650 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.391586 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.374006 | `azmcp_loadtesting_test_create` | ❌ |
| 6 | 0.355049 | `azmcp_loadtesting_testresource_create` | ❌ |
| 7 | 0.351903 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.325925 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.323501 | `azmcp_appconfig_kv_set` | ❌ |
| 10 | 0.319843 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.314854 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.311275 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.305141 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.300188 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.298887 | `azmcp_storage_datalake_directory_create` | ❌ |
| 16 | 0.297236 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.289742 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.286778 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.277805 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.267474 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.400151 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.387071 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.382836 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.376270 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.361412 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.344343 | `azmcp_loadtesting_testresource_create` | ❌ |
| 8 | 0.340337 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.329099 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.324346 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.319383 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 12 | 0.310931 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.310707 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.310332 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.284467 | `azmcp_deploy_plan_get` | ❌ |
| 16 | 0.284385 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.283067 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 18 | 0.280443 | `azmcp_keyvault_certificate_create` | ❌ |
| 19 | 0.280192 | `azmcp_cloudarchitect_design` | ❌ |
| 20 | 0.279131 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 240

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589002 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.464796 | `azmcp_storage_blob_container_create` | ❌ |
| 3 | 0.444359 | `azmcp_storage_datalake_directory_create` | ❌ |
| 4 | 0.437040 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.407411 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.386262 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.385096 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 8 | 0.383927 | `azmcp_loadtesting_testresource_create` | ❌ |
| 9 | 0.383895 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.382274 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.380638 | `azmcp_loadtesting_test_create` | ❌ |
| 12 | 0.380342 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.372681 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.372360 | `azmcp_keyvault_certificate_create` | ❌ |
| 15 | 0.366696 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.363721 | `azmcp_workbooks_create` | ❌ |
| 17 | 0.360940 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.359330 | `azmcp_keyvault_secret_create` | ❌ |
| 19 | 0.321846 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.309241 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 241

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655152 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.603853 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.507639 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.504386 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.483435 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.442858 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.439236 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.431020 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.403478 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.397051 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.395698 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.388772 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.373840 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.373146 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.371705 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 16 | 0.368567 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.366978 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.366644 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.356973 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.353037 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 242

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676876 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.612889 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.518215 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.515153 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.483947 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.415410 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.411808 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.401802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.380012 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.375790 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.373938 | `azmcp_aks_cluster_get` | ❌ |
| 12 | 0.369755 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.368207 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.367985 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.362607 | `azmcp_aks_nodepool_get` | ❌ |
| 16 | 0.362602 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.362249 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.355093 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.354841 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.351052 | `azmcp_functionapp_get` | ❌ |

---

## Test 243

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664087 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.581393 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.557015 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.536909 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.535616 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.501053 | `azmcp_subscription_list` | ❌ |
| 7 | 0.496371 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.493246 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.484236 | `azmcp_storage_blob_container_get` | ❌ |
| 10 | 0.484163 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.473387 | `azmcp_search_service_list` | ❌ |
| 12 | 0.458793 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.454195 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.447991 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.445545 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.441838 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.432645 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.416387 | `azmcp_group_list` | ❌ |
| 19 | 0.414108 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.412679 | `azmcp_grafana_list` | ❌ |

---

## Test 244

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499302 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.461284 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.455450 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.450677 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.421642 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.409063 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 7 | 0.379853 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.378256 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.375553 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.367906 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.366021 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.362252 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.360571 | `azmcp_storage_blob_get` | ❌ |
| 14 | 0.347173 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.346039 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.344771 | `azmcp_search_service_list` | ❌ |
| 17 | 0.335306 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.330363 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.322148 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.312384 | `azmcp_acr_registry_list` | ❌ |

---

## Test 245

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557142 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.499153 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.473598 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.461641 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.453807 | `azmcp_subscription_list` | ❌ |
| 6 | 0.436170 | `azmcp_search_service_list` | ❌ |
| 7 | 0.432855 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.425048 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.418403 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.415843 | `azmcp_storage_blob_get` | ❌ |
| 11 | 0.415080 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.383856 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 13 | 0.382504 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.379856 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.376660 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.374635 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.359998 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.359053 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.356611 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.353273 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 246

**Expected Tool:** `azmcp_storage_blob_batch_set-tier`  
**Prompt:** Set access tier to Cool for multiple blobs in the container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620756 | `azmcp_storage_blob_batch_set-tier` | ✅ **EXPECTED** |
| 2 | 0.465722 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.409043 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.408384 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.378380 | `azmcp_cosmos_database_container_list` | ❌ |
| 6 | 0.369393 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.348748 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.324757 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.305741 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.304711 | `azmcp_storage_blob_upload` | ❌ |
| 11 | 0.295668 | `azmcp_storage_queue_message_send` | ❌ |
| 12 | 0.295532 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.295133 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.289458 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 15 | 0.286940 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.285276 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.271887 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.270526 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.265600 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.251602 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 247

**Expected Tool:** `azmcp_storage_blob_batch_set-tier`  
**Prompt:** Change the access tier to Archive for blobs file1.txt and file2.txt in the container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527994 | `azmcp_storage_blob_batch_set-tier` | ✅ **EXPECTED** |
| 2 | 0.422467 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.364946 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.364070 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.360883 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.351892 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.344188 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.341857 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.325105 | `azmcp_storage_blob_upload` | ❌ |
| 10 | 0.311146 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.301044 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.295798 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 13 | 0.282429 | `azmcp_storage_queue_message_send` | ❌ |
| 14 | 0.280305 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.272376 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 16 | 0.271310 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.267309 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.253981 | `azmcp_appconfig_kv_set` | ❌ |
| 19 | 0.246907 | `azmcp_deploy_iac_rules_get` | ❌ |
| 20 | 0.245593 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 248

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563469 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.524779 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.508053 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.447784 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.403407 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.387848 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.335039 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 8 | 0.331449 | `azmcp_storage_blob_get` | ❌ |
| 9 | 0.326352 | `azmcp_appconfig_kv_set` | ❌ |
| 10 | 0.323215 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.322465 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.320470 | `azmcp_storage_datalake_directory_create` | ❌ |
| 13 | 0.318875 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.305666 | `azmcp_keyvault_certificate_create` | ❌ |
| 15 | 0.297912 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.297384 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.292093 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.291137 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.281807 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.280865 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512762 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.500625 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.470927 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415378 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.414820 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.368859 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.361494 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 8 | 0.354862 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.334039 | `azmcp_storage_blob_upload` | ❌ |
| 10 | 0.320173 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.309739 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.296899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.285153 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.278047 | `azmcp_keyvault_secret_create` | ❌ |
| 15 | 0.275334 | `azmcp_keyvault_key_create` | ❌ |
| 16 | 0.275240 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.270168 | `azmcp_appconfig_kv_set` | ❌ |
| 18 | 0.269625 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.268922 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.256525 | `azmcp_sql_server_create` | ❌ |

---

## Test 250

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463198 | `azmcp_storage_account_create` | ❌ |
| 2 | 0.455376 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.451750 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 4 | 0.435099 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.388450 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.378021 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.375383 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.366330 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.351724 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 10 | 0.329038 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.322364 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.309104 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.287885 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.280773 | `azmcp_keyvault_certificate_create` | ❌ |
| 15 | 0.277049 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.276533 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.275617 | `azmcp_keyvault_secret_create` | ❌ |
| 18 | 0.269719 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.266791 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.265205 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 251

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665176 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.559177 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.523289 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.518764 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.496187 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.479946 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.461577 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.421964 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.421220 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.384585 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.377009 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.376988 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 13 | 0.367759 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.359218 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.354913 | `azmcp_sql_server_show` | ❌ |
| 16 | 0.353561 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.335739 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.334806 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.332134 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.327271 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 252

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613933 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.605437 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 3 | 0.530702 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.521995 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.479014 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.471385 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.453044 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.409820 | `azmcp_acr_registry_repository_list` | ❌ |
| 9 | 0.404640 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.394247 | `azmcp_storage_blob_container_create` | ❌ |
| 11 | 0.386144 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.367215 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.367036 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.359465 | `azmcp_search_service_list` | ❌ |
| 15 | 0.359399 | `azmcp_subscription_list` | ❌ |
| 16 | 0.358630 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 17 | 0.356400 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.351601 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.351458 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.348288 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 253

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625166 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.526462 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.511261 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.439698 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.437887 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.429767 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.418454 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.405678 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.390261 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.386750 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 12 | 0.384096 | `azmcp_acr_registry_repository_list` | ❌ |
| 13 | 0.355955 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.354374 | `azmcp_search_service_list` | ❌ |
| 15 | 0.352491 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.348138 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.346936 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.345644 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.340630 | `azmcp_subscription_list` | ❌ |
| 20 | 0.340150 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 254

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613311 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.586719 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.484012 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.478144 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.434968 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.432072 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.420947 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.408819 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.386665 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.359859 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.349768 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.345963 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.343645 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.338437 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.334104 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.331079 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.323281 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.318630 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.304098 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.303794 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 255

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662106 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.661919 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.537535 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.460661 | `azmcp_storage_blob_container_create` | ❌ |
| 5 | 0.457038 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.453696 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.388809 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.370177 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.360712 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.360470 | `azmcp_aks_cluster_get` | ❌ |
| 11 | 0.358376 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.353412 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.353131 | `azmcp_workbooks_show` | ❌ |
| 14 | 0.352671 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.348551 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.342979 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.337010 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.334138 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.319525 | `azmcp_keyvault_certificate_get` | ❌ |
| 20 | 0.319393 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 256

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592723 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.579070 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.568421 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 4 | 0.507970 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.465942 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.452160 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.415853 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.413336 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.400483 | `azmcp_acr_registry_repository_list` | ❌ |
| 10 | 0.394852 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.382903 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 12 | 0.379847 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.379099 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.369535 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.363904 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 16 | 0.361689 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.359099 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.348799 | `azmcp_subscription_list` | ❌ |
| 19 | 0.340190 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.331545 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 257

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570353 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.549442 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.533515 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.456071 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.449128 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.434015 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.397367 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.395810 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.385243 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.362337 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.359473 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 12 | 0.353799 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.345263 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.342766 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.342653 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 16 | 0.339846 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.336142 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.314069 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.306951 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.300295 | `azmcp_acr_registry_list` | ❌ |

---

## Test 258

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566301 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.403471 | `azmcp_storage_blob_get` | ❌ |
| 3 | 0.397762 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.382251 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.377488 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.351989 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.327516 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.324082 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.307523 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.298071 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 11 | 0.297254 | `azmcp_storage_queue_message_send` | ❌ |
| 12 | 0.294825 | `azmcp_keyvault_certificate_import` | ❌ |
| 13 | 0.291654 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 14 | 0.284994 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.273658 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.273463 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.272307 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 18 | 0.257792 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.253428 | `azmcp_appconfig_kv_show` | ❌ |
| 20 | 0.239567 | `azmcp_foundry_models_deploy` | ❌ |

---

## Test 259

**Expected Tool:** `azmcp_storage_datalake_directory_create`  
**Prompt:** Create a new directory at the path <directory_path> in Data Lake in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647078 | `azmcp_storage_datalake_directory_create` | ✅ **EXPECTED** |
| 2 | 0.481507 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 3 | 0.442414 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.348428 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.340437 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.339889 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.333776 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.313913 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.306845 | `azmcp_storage_blob_container_get` | ❌ |
| 10 | 0.303932 | `azmcp_storage_table_list` | ❌ |
| 11 | 0.302849 | `azmcp_loadtesting_testresource_create` | ❌ |
| 12 | 0.297012 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.295247 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.281849 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.281674 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.276608 | `azmcp_appconfig_kv_set` | ❌ |
| 17 | 0.272610 | `azmcp_workbooks_create` | ❌ |
| 18 | 0.249193 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.240515 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.236486 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 260

**Expected Tool:** `azmcp_storage_datalake_file-system_list-paths`  
**Prompt:** List all paths in the Data Lake file system <file_system> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.767960 | `azmcp_storage_datalake_file-system_list-paths` | ✅ **EXPECTED** |
| 2 | 0.506115 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.481743 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.451626 | `azmcp_storage_datalake_directory_create` | ❌ |
| 5 | 0.432222 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.420884 | `azmcp_storage_share_file_list` | ❌ |
| 7 | 0.419381 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.414952 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.402145 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.390655 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.384290 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.374713 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.357960 | `azmcp_monitor_table_type_list` | ❌ |
| 14 | 0.352558 | `azmcp_search_service_list` | ❌ |
| 15 | 0.349357 | `azmcp_subscription_list` | ❌ |
| 16 | 0.346694 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.344288 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.337104 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.333592 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.331526 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 261

**Expected Tool:** `azmcp_storage_datalake_file-system_list-paths`  
**Prompt:** Show me the paths in the Data Lake file system <file_system> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727344 | `azmcp_storage_datalake_file-system_list-paths` | ✅ **EXPECTED** |
| 2 | 0.502902 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.433622 | `azmcp_storage_datalake_directory_create` | ❌ |
| 4 | 0.432085 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.426861 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.399930 | `azmcp_storage_blob_container_get` | ❌ |
| 7 | 0.384157 | `azmcp_storage_share_file_list` | ❌ |
| 8 | 0.372453 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.347625 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.345916 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.344289 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.335052 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.329465 | `azmcp_storage_blob_get` | ❌ |
| 14 | 0.327591 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.325117 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.304870 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.304577 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.304566 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.289587 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.288593 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 262

**Expected Tool:** `azmcp_storage_datalake_file-system_list-paths`  
**Prompt:** Recursively list all paths in the Data Lake file system <file_system> in the storage account <account> filtered by <filter_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685275 | `azmcp_storage_datalake_file-system_list-paths` | ✅ **EXPECTED** |
| 2 | 0.465172 | `azmcp_storage_share_file_list` | ❌ |
| 3 | 0.431509 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 4 | 0.418276 | `azmcp_storage_datalake_directory_create` | ❌ |
| 5 | 0.394481 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.372137 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.363932 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.358319 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.343394 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.337297 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.335031 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.334195 | `azmcp_acr_registry_repository_list` | ❌ |
| 13 | 0.323581 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.322311 | `azmcp_search_service_list` | ❌ |
| 15 | 0.318156 | `azmcp_subscription_list` | ❌ |
| 16 | 0.317347 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.314467 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.310225 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.300135 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.294831 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 263

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Send a message "Hello, World!" to the queue <queue> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558401 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.410972 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.375068 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.373072 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.344373 | `azmcp_servicebus_queue_details` | ❌ |
| 6 | 0.335989 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.328105 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.325517 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.324933 | `azmcp_storage_blob_container_get` | ❌ |
| 10 | 0.321736 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.317420 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.305274 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.300578 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 14 | 0.285295 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.279181 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 16 | 0.278031 | `azmcp_monitor_workspace_log_query` | ❌ |
| 17 | 0.272609 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.262059 | `azmcp_storage_blob_upload` | ❌ |
| 19 | 0.258161 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.252408 | `azmcp_kusto_query` | ❌ |

---

## Test 264

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Send a message with TTL of 3600 seconds to the queue <queue> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642240 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.383344 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.373050 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.357015 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.347683 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.334576 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.325269 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.317420 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.315898 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.315019 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.312453 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.310258 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.282610 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.282534 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 15 | 0.277782 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.273178 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.271393 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.257323 | `azmcp_keyvault_secret_create` | ❌ |
| 19 | 0.239636 | `azmcp_kusto_query` | ❌ |
| 20 | 0.237026 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 265

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Add a message to the queue <queue> in storage account <account> with visibility timeout of 30 seconds  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595220 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.360533 | `azmcp_servicebus_queue_details` | ❌ |
| 3 | 0.338698 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.325197 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.322682 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.313484 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.312466 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.297591 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.293557 | `azmcp_storage_blob_upload` | ❌ |
| 10 | 0.278482 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 11 | 0.274397 | `azmcp_keyvault_secret_create` | ❌ |
| 12 | 0.270962 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.270456 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.266298 | `azmcp_monitor_workspace_log_query` | ❌ |
| 15 | 0.261954 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.257594 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.247123 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.245778 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.241013 | `azmcp_keyvault_key_create` | ❌ |
| 20 | 0.218569 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 266

**Expected Tool:** `azmcp_storage_share_file_list`  
**Prompt:** List all files and directories in the File Share <share> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640440 | `azmcp_storage_share_file_list` | ✅ **EXPECTED** |
| 2 | 0.539851 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.522644 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 4 | 0.500965 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.491152 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.458759 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.433528 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.416549 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.404225 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.397963 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.391675 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.390282 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.385269 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.382087 | `azmcp_search_service_list` | ❌ |
| 15 | 0.373056 | `azmcp_keyvault_certificate_list` | ❌ |
| 16 | 0.372921 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.366481 | `azmcp_subscription_list` | ❌ |
| 18 | 0.360454 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.353596 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.337938 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 267

**Expected Tool:** `azmcp_storage_share_file_list`  
**Prompt:** Show me the files in the File Share <share> directory <directory_path> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552006 | `azmcp_storage_share_file_list` | ✅ **EXPECTED** |
| 2 | 0.511236 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 3 | 0.452272 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.443743 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.425236 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.405964 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.380180 | `azmcp_storage_datalake_directory_create` | ❌ |
| 8 | 0.351906 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.351055 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.341853 | `azmcp_storage_blob_get` | ❌ |
| 11 | 0.341352 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.331565 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.328387 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.320405 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.317899 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.315352 | `azmcp_keyvault_key_list` | ❌ |
| 17 | 0.304034 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.303900 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.301881 | `azmcp_search_service_list` | ❌ |
| 20 | 0.301062 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 268

**Expected Tool:** `azmcp_storage_share_file_list`  
**Prompt:** List files with prefix 'report' in the File Share <share> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602213 | `azmcp_storage_share_file_list` | ✅ **EXPECTED** |
| 2 | 0.449412 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.446161 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 4 | 0.436632 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.423868 | `azmcp_extension_azqr` | ❌ |
| 6 | 0.422668 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.411646 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.378092 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.374980 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.369171 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.364292 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.360947 | `azmcp_search_service_list` | ❌ |
| 13 | 0.352130 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.344004 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.339261 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.336352 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.332926 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.319827 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.319475 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.318786 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 269

**Expected Tool:** `azmcp_storage_table_list`  
**Prompt:** List all tables in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.787243 | `azmcp_storage_table_list` | ✅ **EXPECTED** |
| 2 | 0.574921 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.552523 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.514042 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.510657 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.505290 | `azmcp_storage_blob_container_get` | ❌ |
| 7 | 0.503638 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.498180 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.497572 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.491995 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.486049 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.430612 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.421849 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.421152 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 15 | 0.407089 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.404701 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.393501 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.362913 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.360786 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.358239 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 270

**Expected Tool:** `azmcp_storage_table_list`  
**Prompt:** Show me the tables in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738095 | `azmcp_storage_table_list` | ✅ **EXPECTED** |
| 2 | 0.521785 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.520811 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.519070 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.514313 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.480680 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.479470 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.470860 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.462051 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.447645 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.441119 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.434084 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 13 | 0.428607 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.423663 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.420393 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.380764 | `azmcp_kusto_table_schema` | ❌ |
| 17 | 0.368053 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.365922 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.362253 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.356792 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 271

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576116 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.473852 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471653 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.452471 | `azmcp_search_service_list` | ❌ |
| 6 | 0.450973 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.445724 | `azmcp_grafana_list` | ❌ |
| 8 | 0.436338 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.431337 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.430279 | `azmcp_group_list` | ❌ |
| 11 | 0.422723 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.406935 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.394953 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.388737 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.380636 | `azmcp_marketplace_product_list` | ❌ |
| 16 | 0.367761 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.366860 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.355344 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.348524 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.344901 | `azmcp_servicebus_topic_subscription_details` | ❌ |

---

## Test 272

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405701 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.381238 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.351864 | `azmcp_grafana_list` | ❌ |
| 4 | 0.350951 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.341813 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.334744 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.328109 | `azmcp_search_service_list` | ❌ |
| 8 | 0.315604 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.308874 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.303528 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.303367 | `azmcp_marketplace_product_list` | ❌ |
| 12 | 0.297209 | `azmcp_group_list` | ❌ |
| 13 | 0.296282 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.295180 | `azmcp_marketplace_product_get` | ❌ |
| 15 | 0.285434 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 16 | 0.275417 | `azmcp_loadtesting_testresource_list` | ❌ |
| 17 | 0.274876 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.269922 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.256330 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.244501 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 273

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.319985 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.315547 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.286711 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.282645 | `azmcp_grafana_list` | ❌ |
| 5 | 0.279702 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.278798 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.273758 | `azmcp_marketplace_product_list` | ❌ |
| 8 | 0.256358 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.254815 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 10 | 0.252879 | `azmcp_eventgrid_topic_list` | ❌ |
| 11 | 0.252504 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.251683 | `azmcp_search_service_list` | ❌ |
| 13 | 0.251368 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.233143 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.230822 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.230571 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.227020 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.226446 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.222799 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.211120 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 274

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.403225 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.354504 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.342318 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.340339 | `azmcp_grafana_list` | ❌ |
| 5 | 0.336798 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.311939 | `azmcp_search_service_list` | ❌ |
| 7 | 0.311109 | `azmcp_marketplace_product_list` | ❌ |
| 8 | 0.305150 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.304965 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.302271 | `azmcp_eventgrid_topic_list` | ❌ |
| 11 | 0.300478 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.294080 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.291826 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.282326 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.281294 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.274224 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.269869 | `azmcp_group_list` | ❌ |
| 18 | 0.258468 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.258410 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.236600 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 275

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625270 | `azmcp_deploy_iac_rules_get` | ❌ |
| 3 | 0.605048 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.482936 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466199 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.431102 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.389080 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.386480 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.372596 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369184 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.362323 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.354086 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.339022 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.333210 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.312592 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.310274 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.305259 | `azmcp_mysql_database_query` | ❌ |
| 18 | 0.303849 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.302307 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.301536 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 276

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.510005 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.444297 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.439871 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.439535 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.428888 | `azmcp_keyvault_certificate_get` | ❌ |
| 8 | 0.389515 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.381306 | `azmcp_keyvault_certificate_create` | ❌ |
| 10 | 0.379881 | `azmcp_keyvault_certificate_import` | ❌ |
| 11 | 0.304912 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.304137 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.300776 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.292743 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.281261 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.279035 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.278638 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.277623 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.274572 | `azmcp_subscription_list` | ❌ |
| 20 | 0.274176 | `azmcp_search_service_list` | ❌ |

---

## Test 277

**Expected Tool:** `azmcp_virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711969 | `azmcp_virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659763 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.566615 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.548888 | `azmcp_search_service_list` | ❌ |
| 5 | 0.536542 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.535739 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 7 | 0.527948 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.527095 | `azmcp_aks_nodepool_list` | ❌ |
| 9 | 0.525796 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.525637 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.506608 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.505069 | `azmcp_subscription_list` | ❌ |
| 13 | 0.496298 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.495490 | `azmcp_grafana_list` | ❌ |
| 15 | 0.492515 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.476718 | `azmcp_group_list` | ❌ |
| 17 | 0.465569 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.463073 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.460388 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.459250 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 278

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714468 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.439611 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.402909 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.393721 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.364696 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.362307 | `azmcp_search_service_list` | ❌ |
| 9 | 0.344853 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.337530 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.335295 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.333517 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.332928 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.330896 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.328626 | `azmcp_keyvault_key_list` | ❌ |
| 16 | 0.321713 | `azmcp_subscription_list` | ❌ |
| 17 | 0.312156 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.311262 | `azmcp_grafana_list` | ❌ |
| 19 | 0.308168 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.302706 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 279

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812524 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.658078 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.500203 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.355514 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.336464 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.327562 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.322277 | `azmcp_subscription_list` | ❌ |
| 8 | 0.321688 | `azmcp_search_service_list` | ❌ |
| 9 | 0.315736 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.315384 | `azmcp_loadtesting_testrun_list` | ❌ |
| 11 | 0.306931 | `azmcp_aks_nodepool_get` | ❌ |
| 12 | 0.304557 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.303854 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.302954 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.299006 | `azmcp_keyvault_secret_list` | ❌ |
| 16 | 0.295551 | `azmcp_grafana_list` | ❌ |
| 17 | 0.285139 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.277483 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.275758 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.275022 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 280

**Expected Tool:** `azmcp_workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552212 | `azmcp_workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.433162 | `azmcp_workbooks_update` | ❌ |
| 3 | 0.361364 | `azmcp_workbooks_delete` | ❌ |
| 4 | 0.361215 | `azmcp_workbooks_show` | ❌ |
| 5 | 0.328113 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.239813 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.217945 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.214812 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.188137 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.172751 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.169440 | `azmcp_grafana_list` | ❌ |
| 12 | 0.153950 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.148897 | `azmcp_loadtesting_test_create` | ❌ |
| 14 | 0.147365 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.143713 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.142879 | `azmcp_storage_datalake_directory_create` | ❌ |
| 17 | 0.130524 | `azmcp_loadtesting_testrun_create` | ❌ |
| 18 | 0.130339 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.116803 | `azmcp_loadtesting_testrun_update` | ❌ |
| 20 | 0.113882 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 281

**Expected Tool:** `azmcp_workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621310 | `azmcp_workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.518630 | `azmcp_workbooks_show` | ❌ |
| 3 | 0.432454 | `azmcp_workbooks_create` | ❌ |
| 4 | 0.425569 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.390355 | `azmcp_workbooks_update` | ❌ |
| 6 | 0.273939 | `azmcp_grafana_list` | ❌ |
| 7 | 0.256795 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 8 | 0.242993 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.198585 | `azmcp_appconfig_kv_delete` | ❌ |
| 10 | 0.190455 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.186665 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.181661 | `azmcp_monitor_workspace_log_query` | ❌ |
| 13 | 0.155271 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.148882 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.145141 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.134979 | `azmcp_loadtesting_testrun_update` | ❌ |
| 17 | 0.132504 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.131813 | `azmcp_group_list` | ❌ |
| 19 | 0.122450 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.119436 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 282

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772430 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.532565 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.516739 | `azmcp_grafana_list` | ❌ |
| 5 | 0.488599 | `azmcp_group_list` | ❌ |
| 6 | 0.487920 | `azmcp_workbooks_delete` | ❌ |
| 7 | 0.459976 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.454209 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.439944 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.428781 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.416566 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.413409 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.405963 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.405064 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.399758 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.365302 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.362740 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.356739 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.352940 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.349674 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 283

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570260 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.539957 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.485504 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.472378 | `azmcp_grafana_list` | ❌ |
| 6 | 0.428025 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.425426 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.422785 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421646 | `azmcp_group_list` | ❌ |
| 10 | 0.412390 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.392371 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.380992 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.371128 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.363744 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.362606 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.350839 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.338334 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.337787 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.334580 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.302515 | `azmcp_monitor_metrics_definitions` | ❌ |

---

## Test 284

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.697517 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498356 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.494680 | `azmcp_workbooks_list` | ❌ |
| 4 | 0.452335 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.419050 | `azmcp_workbooks_update` | ❌ |
| 6 | 0.353506 | `azmcp_grafana_list` | ❌ |
| 7 | 0.277851 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.264685 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.256716 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.250055 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.236784 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.230519 | `azmcp_monitor_metrics_definitions` | ❌ |
| 13 | 0.230473 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.227527 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.225290 | `azmcp_loadtesting_test_get` | ❌ |
| 16 | 0.218990 | `azmcp_loadtesting_testresource_list` | ❌ |
| 17 | 0.207727 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.197263 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 19 | 0.195427 | `azmcp_group_list` | ❌ |
| 20 | 0.189848 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 285

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Show me the workbook with display name <workbook_display_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469476 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.455158 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.437638 | `azmcp_workbooks_update` | ❌ |
| 4 | 0.424338 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.366057 | `azmcp_workbooks_delete` | ❌ |
| 6 | 0.292898 | `azmcp_grafana_list` | ❌ |
| 7 | 0.266530 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.239907 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.227383 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.176481 | `azmcp_role_assignment_list` | ❌ |
| 11 | 0.175814 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.174513 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.174123 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.168191 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.165774 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.154760 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.152535 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.149678 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.148994 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.146054 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 286

**Expected Tool:** `azmcp_workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469915 | `azmcp_workbooks_update` | ✅ **EXPECTED** |
| 2 | 0.382651 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.362354 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.349689 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.276727 | `azmcp_loadtesting_testrun_update` | ❌ |
| 6 | 0.262874 | `azmcp_workbooks_list` | ❌ |
| 7 | 0.170118 | `azmcp_grafana_list` | ❌ |
| 8 | 0.148730 | `azmcp_mysql_server_param_set` | ❌ |
| 9 | 0.142404 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 10 | 0.142186 | `azmcp_loadtesting_testrun_create` | ❌ |
| 11 | 0.138354 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.136105 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.131007 | `azmcp_postgres_database_query` | ❌ |
| 14 | 0.129973 | `azmcp_postgres_server_param_set` | ❌ |
| 15 | 0.129660 | `azmcp_deploy_iac_rules_get` | ❌ |
| 16 | 0.126312 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.123282 | `azmcp_monitor_workspace_log_query` | ❌ |
| 18 | 0.111100 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 19 | 0.105705 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.097698 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 287

**Expected Tool:** `azmcp_bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485889 | `azmcp_deploy_iac_rules_get` | ❌ |
| 2 | 0.448373 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.440302 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.432773 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.432409 | `azmcp_bicepschema_get` | ✅ **EXPECTED** |
| 6 | 0.400985 | `azmcp_foundry_models_deploy` | ❌ |
| 7 | 0.398046 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.391625 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.382229 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.372097 | `azmcp_search_service_list` | ❌ |
| 11 | 0.344809 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.325716 | `azmcp_search_index_query` | ❌ |
| 13 | 0.324857 | `azmcp_search_index_get` | ❌ |
| 14 | 0.303183 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.291291 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.281487 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.279983 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.274770 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.270724 | `azmcp_storage_blob_container_create` | ❌ |
| 20 | 0.268139 | `azmcp_workbooks_create` | ❌ |

---

## Test 288

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.349336 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290902 | `azmcp_storage_blob_upload` | ❌ |
| 3 | 0.254991 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.221349 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.217623 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.216162 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 7 | 0.195530 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 8 | 0.191641 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.191096 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.178245 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.175833 | `azmcp_deploy_iac_rules_get` | ❌ |
| 12 | 0.159251 | `azmcp_storage_share_file_list` | ❌ |
| 13 | 0.154832 | `azmcp_storage_queue_message_send` | ❌ |
| 14 | 0.136707 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.135768 | `azmcp_get_bestpractices_get` | ❌ |
| 16 | 0.135426 | `azmcp_storage_datalake_directory_create` | ❌ |
| 17 | 0.132826 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.130037 | `azmcp_foundry_models_deploy` | ❌ |
| 19 | 0.127003 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 20 | 0.118383 | `azmcp_quota_usage_check` | ❌ |

---

## Test 289

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.290305 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.267727 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258197 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.225655 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.215780 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.207367 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.195407 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.189280 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.179187 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.168903 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 11 | 0.163950 | `azmcp_storage_blob_container_create` | ❌ |
| 12 | 0.163732 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.162192 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.160770 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.154937 | `azmcp_foundry_models_deploy` | ❌ |
| 16 | 0.154261 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.148089 | `azmcp_storage_queue_message_send` | ❌ |
| 18 | 0.145166 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.139769 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.128921 | `azmcp_sql_server_show` | ❌ |

---

## Test 290

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.299640 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242581 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.218064 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.213173 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.179199 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.169691 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.164328 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.156441 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.156119 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.154403 | `azmcp_storage_queue_message_send` | ❌ |
| 13 | 0.140250 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.138067 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.132355 | `azmcp_mysql_database_query` | ❌ |
| 16 | 0.130132 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.123936 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.119586 | `azmcp_workbooks_create` | ❌ |
| 19 | 0.114994 | `azmcp_mysql_table_schema_get` | ❌ |
| 20 | 0.111424 | `azmcp_sql_db_list` | ❌ |

---

## Test 291

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.420259 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.352797 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.323920 | `azmcp_storage_blob_upload` | ❌ |
| 5 | 0.310615 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.306967 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.304499 | `azmcp_storage_queue_message_send` | ❌ |
| 8 | 0.304209 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 9 | 0.300338 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.299412 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 11 | 0.298989 | `azmcp_get_bestpractices_get` | ❌ |
| 12 | 0.293806 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.292455 | `azmcp_applens_resource_diagnose` | ❌ |
| 14 | 0.291878 | `azmcp_deploy_iac_rules_get` | ❌ |
| 15 | 0.282267 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.275832 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.275550 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.272671 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.261446 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.259814 | `azmcp_search_service_list` | ❌ |

---

## Summary

**Total Prompts Tested:** 291  
**Execution Time:** 69.2597762s  

### Success Rate Metrics

**Top Choice Success:** 83.5% (243/291 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 4.8% (14/291 tests)  
**🎯 High Confidence (≥0.7):** 22.0% (64/291 tests)  
**✅ Good Confidence (≥0.6):** 58.1% (169/291 tests)  
**👍 Fair Confidence (≥0.5):** 83.2% (242/291 tests)  
**👌 Acceptable Confidence (≥0.4):** 92.1% (268/291 tests)  
**❌ Low Confidence (<0.4):** 7.9% (23/291 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 4.8% (14/291 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 22.0% (64/291 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 56.0% (163/291 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 75.3% (219/291 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 80.4% (234/291 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing adequately but has room for improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

