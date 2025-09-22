# Tool Selection Analysis Setup

**Setup completed:** 2025-09-22 15:39:57  
**Tool count:** 140  
**Database setup time:** 1.6964240s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-22 15:39:57  
**Tool count:** 140  

## Table of Contents

- [Test 1: azmcp_acr_registry_list](#test-1)
- [Test 2: azmcp_acr_registry_list](#test-2)
- [Test 3: azmcp_acr_registry_list](#test-3)
- [Test 4: azmcp_acr_registry_list](#test-4)
- [Test 5: azmcp_acr_registry_list](#test-5)
- [Test 6: azmcp_acr_registry_repository_list](#test-6)
- [Test 7: azmcp_acr_registry_repository_list](#test-7)
- [Test 8: azmcp_acr_registry_repository_list](#test-8)
- [Test 9: azmcp_acr_registry_repository_list](#test-9)
- [Test 10: azmcp_aks_cluster_get](#test-10)
- [Test 11: azmcp_aks_cluster_get](#test-11)
- [Test 12: azmcp_aks_cluster_get](#test-12)
- [Test 13: azmcp_aks_cluster_get](#test-13)
- [Test 14: azmcp_aks_cluster_list](#test-14)
- [Test 15: azmcp_aks_cluster_list](#test-15)
- [Test 16: azmcp_aks_cluster_list](#test-16)
- [Test 17: azmcp_aks_nodepool_get](#test-17)
- [Test 18: azmcp_aks_nodepool_get](#test-18)
- [Test 19: azmcp_aks_nodepool_get](#test-19)
- [Test 20: azmcp_aks_nodepool_list](#test-20)
- [Test 21: azmcp_aks_nodepool_list](#test-21)
- [Test 22: azmcp_aks_nodepool_list](#test-22)
- [Test 23: azmcp_appconfig_account_list](#test-23)
- [Test 24: azmcp_appconfig_account_list](#test-24)
- [Test 25: azmcp_appconfig_account_list](#test-25)
- [Test 26: azmcp_appconfig_kv_delete](#test-26)
- [Test 27: azmcp_appconfig_kv_list](#test-27)
- [Test 28: azmcp_appconfig_kv_list](#test-28)
- [Test 29: azmcp_appconfig_kv_lock_set](#test-29)
- [Test 30: azmcp_appconfig_kv_lock_set](#test-30)
- [Test 31: azmcp_appconfig_kv_set](#test-31)
- [Test 32: azmcp_appconfig_kv_show](#test-32)
- [Test 33: azmcp_applens_resource_diagnose](#test-33)
- [Test 34: azmcp_applens_resource_diagnose](#test-34)
- [Test 35: azmcp_applens_resource_diagnose](#test-35)
- [Test 36: azmcp_azuremanagedlustre_filesystem_list](#test-36)
- [Test 37: azmcp_azuremanagedlustre_filesystem_list](#test-37)
- [Test 38: azmcp_azuremanagedlustre_filesystem_required-subnet-size](#test-38)
- [Test 39: azmcp_azuremanagedlustre_filesystem_sku_get](#test-39)
- [Test 40: azmcp_azureterraformbestpractices_get](#test-40)
- [Test 41: azmcp_azureterraformbestpractices_get](#test-41)
- [Test 42: azmcp_bestpractices_get](#test-42)
- [Test 43: azmcp_bestpractices_get](#test-43)
- [Test 44: azmcp_bestpractices_get](#test-44)
- [Test 45: azmcp_bestpractices_get](#test-45)
- [Test 46: azmcp_bestpractices_get](#test-46)
- [Test 47: azmcp_bestpractices_get](#test-47)
- [Test 48: azmcp_bestpractices_get](#test-48)
- [Test 49: azmcp_bestpractices_get](#test-49)
- [Test 50: azmcp_bestpractices_get](#test-50)
- [Test 51: azmcp_bestpractices_get](#test-51)
- [Test 52: azmcp_bicepschema_get](#test-52)
- [Test 53: azmcp_cloudarchitect_design](#test-53)
- [Test 54: azmcp_cloudarchitect_design](#test-54)
- [Test 55: azmcp_cloudarchitect_design](#test-55)
- [Test 56: azmcp_cloudarchitect_design](#test-56)
- [Test 57: azmcp_cosmos_account_list](#test-57)
- [Test 58: azmcp_cosmos_account_list](#test-58)
- [Test 59: azmcp_cosmos_account_list](#test-59)
- [Test 60: azmcp_cosmos_database_container_item_query](#test-60)
- [Test 61: azmcp_cosmos_database_container_list](#test-61)
- [Test 62: azmcp_cosmos_database_container_list](#test-62)
- [Test 63: azmcp_cosmos_database_list](#test-63)
- [Test 64: azmcp_cosmos_database_list](#test-64)
- [Test 65: azmcp_datadog_monitoredresources_list](#test-65)
- [Test 66: azmcp_datadog_monitoredresources_list](#test-66)
- [Test 67: azmcp_deploy_app_logs_get](#test-67)
- [Test 68: azmcp_deploy_architecture_diagram_generate](#test-68)
- [Test 69: azmcp_deploy_iac_rules_get](#test-69)
- [Test 70: azmcp_deploy_pipeline_guidance_get](#test-70)
- [Test 71: azmcp_deploy_plan_get](#test-71)
- [Test 72: azmcp_eventgrid_subscription_list](#test-72)
- [Test 73: azmcp_eventgrid_subscription_list](#test-73)
- [Test 74: azmcp_eventgrid_subscription_list](#test-74)
- [Test 75: azmcp_eventgrid_subscription_list](#test-75)
- [Test 76: azmcp_eventgrid_subscription_list](#test-76)
- [Test 77: azmcp_eventgrid_subscription_list](#test-77)
- [Test 78: azmcp_eventgrid_subscription_list](#test-78)
- [Test 79: azmcp_eventgrid_topic_list](#test-79)
- [Test 80: azmcp_eventgrid_topic_list](#test-80)
- [Test 81: azmcp_eventgrid_topic_list](#test-81)
- [Test 82: azmcp_eventgrid_topic_list](#test-82)
- [Test 83: azmcp_extension_azqr](#test-83)
- [Test 84: azmcp_extension_azqr](#test-84)
- [Test 85: azmcp_extension_azqr](#test-85)
- [Test 86: azmcp_foundry_knowledge_index_list](#test-86)
- [Test 87: azmcp_foundry_knowledge_index_list](#test-87)
- [Test 88: azmcp_foundry_knowledge_index_schema](#test-88)
- [Test 89: azmcp_foundry_knowledge_index_schema](#test-89)
- [Test 90: azmcp_foundry_models_deploy](#test-90)
- [Test 91: azmcp_foundry_models_deployments_list](#test-91)
- [Test 92: azmcp_foundry_models_deployments_list](#test-92)
- [Test 93: azmcp_foundry_models_list](#test-93)
- [Test 94: azmcp_foundry_models_list](#test-94)
- [Test 95: azmcp_functionapp_get](#test-95)
- [Test 96: azmcp_functionapp_get](#test-96)
- [Test 97: azmcp_functionapp_get](#test-97)
- [Test 98: azmcp_functionapp_get](#test-98)
- [Test 99: azmcp_functionapp_get](#test-99)
- [Test 100: azmcp_functionapp_get](#test-100)
- [Test 101: azmcp_functionapp_get](#test-101)
- [Test 102: azmcp_functionapp_get](#test-102)
- [Test 103: azmcp_functionapp_get](#test-103)
- [Test 104: azmcp_functionapp_get](#test-104)
- [Test 105: azmcp_functionapp_get](#test-105)
- [Test 106: azmcp_functionapp_get](#test-106)
- [Test 107: azmcp_grafana_list](#test-107)
- [Test 108: azmcp_group_list](#test-108)
- [Test 109: azmcp_group_list](#test-109)
- [Test 110: azmcp_group_list](#test-110)
- [Test 111: azmcp_keyvault_certificate_create](#test-111)
- [Test 112: azmcp_keyvault_certificate_get](#test-112)
- [Test 113: azmcp_keyvault_certificate_get](#test-113)
- [Test 114: azmcp_keyvault_certificate_import](#test-114)
- [Test 115: azmcp_keyvault_certificate_import](#test-115)
- [Test 116: azmcp_keyvault_certificate_list](#test-116)
- [Test 117: azmcp_keyvault_certificate_list](#test-117)
- [Test 118: azmcp_keyvault_key_create](#test-118)
- [Test 119: azmcp_keyvault_key_list](#test-119)
- [Test 120: azmcp_keyvault_key_list](#test-120)
- [Test 121: azmcp_keyvault_secret_create](#test-121)
- [Test 122: azmcp_keyvault_secret_list](#test-122)
- [Test 123: azmcp_keyvault_secret_list](#test-123)
- [Test 124: azmcp_kusto_cluster_get](#test-124)
- [Test 125: azmcp_kusto_cluster_list](#test-125)
- [Test 126: azmcp_kusto_cluster_list](#test-126)
- [Test 127: azmcp_kusto_cluster_list](#test-127)
- [Test 128: azmcp_kusto_database_list](#test-128)
- [Test 129: azmcp_kusto_database_list](#test-129)
- [Test 130: azmcp_kusto_query](#test-130)
- [Test 131: azmcp_kusto_sample](#test-131)
- [Test 132: azmcp_kusto_table_list](#test-132)
- [Test 133: azmcp_kusto_table_list](#test-133)
- [Test 134: azmcp_kusto_table_schema](#test-134)
- [Test 135: azmcp_loadtesting_test_create](#test-135)
- [Test 136: azmcp_loadtesting_test_get](#test-136)
- [Test 137: azmcp_loadtesting_testresource_create](#test-137)
- [Test 138: azmcp_loadtesting_testresource_list](#test-138)
- [Test 139: azmcp_loadtesting_testrun_create](#test-139)
- [Test 140: azmcp_loadtesting_testrun_get](#test-140)
- [Test 141: azmcp_loadtesting_testrun_list](#test-141)
- [Test 142: azmcp_loadtesting_testrun_update](#test-142)
- [Test 143: azmcp_marketplace_product_get](#test-143)
- [Test 144: azmcp_marketplace_product_list](#test-144)
- [Test 145: azmcp_marketplace_product_list](#test-145)
- [Test 146: azmcp_monitor_healthmodels_entity_gethealth](#test-146)
- [Test 147: azmcp_monitor_metrics_definitions](#test-147)
- [Test 148: azmcp_monitor_metrics_definitions](#test-148)
- [Test 149: azmcp_monitor_metrics_definitions](#test-149)
- [Test 150: azmcp_monitor_metrics_query](#test-150)
- [Test 151: azmcp_monitor_metrics_query](#test-151)
- [Test 152: azmcp_monitor_metrics_query](#test-152)
- [Test 153: azmcp_monitor_metrics_query](#test-153)
- [Test 154: azmcp_monitor_metrics_query](#test-154)
- [Test 155: azmcp_monitor_metrics_query](#test-155)
- [Test 156: azmcp_monitor_resource_log_query](#test-156)
- [Test 157: azmcp_monitor_table_list](#test-157)
- [Test 158: azmcp_monitor_table_list](#test-158)
- [Test 159: azmcp_monitor_table_type_list](#test-159)
- [Test 160: azmcp_monitor_table_type_list](#test-160)
- [Test 161: azmcp_monitor_workspace_list](#test-161)
- [Test 162: azmcp_monitor_workspace_list](#test-162)
- [Test 163: azmcp_monitor_workspace_list](#test-163)
- [Test 164: azmcp_monitor_workspace_log_query](#test-164)
- [Test 165: azmcp_mysql_database_list](#test-165)
- [Test 166: azmcp_mysql_database_list](#test-166)
- [Test 167: azmcp_mysql_database_query](#test-167)
- [Test 168: azmcp_mysql_server_config_get](#test-168)
- [Test 169: azmcp_mysql_server_list](#test-169)
- [Test 170: azmcp_mysql_server_list](#test-170)
- [Test 171: azmcp_mysql_server_list](#test-171)
- [Test 172: azmcp_mysql_server_param_get](#test-172)
- [Test 173: azmcp_mysql_server_param_set](#test-173)
- [Test 174: azmcp_mysql_table_list](#test-174)
- [Test 175: azmcp_mysql_table_list](#test-175)
- [Test 176: azmcp_mysql_table_schema_get](#test-176)
- [Test 177: azmcp_postgres_database_list](#test-177)
- [Test 178: azmcp_postgres_database_list](#test-178)
- [Test 179: azmcp_postgres_database_query](#test-179)
- [Test 180: azmcp_postgres_server_config_get](#test-180)
- [Test 181: azmcp_postgres_server_list](#test-181)
- [Test 182: azmcp_postgres_server_list](#test-182)
- [Test 183: azmcp_postgres_server_list](#test-183)
- [Test 184: azmcp_postgres_server_param_get](#test-184)
- [Test 185: azmcp_postgres_server_param_set](#test-185)
- [Test 186: azmcp_postgres_table_list](#test-186)
- [Test 187: azmcp_postgres_table_list](#test-187)
- [Test 188: azmcp_postgres_table_schema_get](#test-188)
- [Test 189: azmcp_quota_region_availability_list](#test-189)
- [Test 190: azmcp_quota_usage_check](#test-190)
- [Test 191: azmcp_redis_cache_accesspolicy_list](#test-191)
- [Test 192: azmcp_redis_cache_accesspolicy_list](#test-192)
- [Test 193: azmcp_redis_cache_list](#test-193)
- [Test 194: azmcp_redis_cache_list](#test-194)
- [Test 195: azmcp_redis_cache_list](#test-195)
- [Test 196: azmcp_redis_cluster_database_list](#test-196)
- [Test 197: azmcp_redis_cluster_database_list](#test-197)
- [Test 198: azmcp_redis_cluster_list](#test-198)
- [Test 199: azmcp_redis_cluster_list](#test-199)
- [Test 200: azmcp_redis_cluster_list](#test-200)
- [Test 201: azmcp_resourcehealth_availability-status_get](#test-201)
- [Test 202: azmcp_resourcehealth_availability-status_get](#test-202)
- [Test 203: azmcp_resourcehealth_availability-status_get](#test-203)
- [Test 204: azmcp_resourcehealth_availability-status_list](#test-204)
- [Test 205: azmcp_resourcehealth_availability-status_list](#test-205)
- [Test 206: azmcp_resourcehealth_availability-status_list](#test-206)
- [Test 207: azmcp_resourcehealth_service-health-events_list](#test-207)
- [Test 208: azmcp_resourcehealth_service-health-events_list](#test-208)
- [Test 209: azmcp_resourcehealth_service-health-events_list](#test-209)
- [Test 210: azmcp_resourcehealth_service-health-events_list](#test-210)
- [Test 211: azmcp_resourcehealth_service-health-events_list](#test-211)
- [Test 212: azmcp_role_assignment_list](#test-212)
- [Test 213: azmcp_role_assignment_list](#test-213)
- [Test 214: azmcp_search_index_get](#test-214)
- [Test 215: azmcp_search_index_get](#test-215)
- [Test 216: azmcp_search_index_get](#test-216)
- [Test 217: azmcp_search_index_query](#test-217)
- [Test 218: azmcp_search_service_list](#test-218)
- [Test 219: azmcp_search_service_list](#test-219)
- [Test 220: azmcp_search_service_list](#test-220)
- [Test 221: azmcp_servicebus_queue_details](#test-221)
- [Test 222: azmcp_servicebus_topic_details](#test-222)
- [Test 223: azmcp_servicebus_topic_subscription_details](#test-223)
- [Test 224: azmcp_sql_db_create](#test-224)
- [Test 225: azmcp_sql_db_create](#test-225)
- [Test 226: azmcp_sql_db_create](#test-226)
- [Test 227: azmcp_sql_db_delete](#test-227)
- [Test 228: azmcp_sql_db_delete](#test-228)
- [Test 229: azmcp_sql_db_delete](#test-229)
- [Test 230: azmcp_sql_db_list](#test-230)
- [Test 231: azmcp_sql_db_list](#test-231)
- [Test 232: azmcp_sql_db_show](#test-232)
- [Test 233: azmcp_sql_db_show](#test-233)
- [Test 234: azmcp_sql_db_update](#test-234)
- [Test 235: azmcp_sql_db_update](#test-235)
- [Test 236: azmcp_sql_elastic-pool_list](#test-236)
- [Test 237: azmcp_sql_elastic-pool_list](#test-237)
- [Test 238: azmcp_sql_elastic-pool_list](#test-238)
- [Test 239: azmcp_sql_server_create](#test-239)
- [Test 240: azmcp_sql_server_create](#test-240)
- [Test 241: azmcp_sql_server_create](#test-241)
- [Test 242: azmcp_sql_server_delete](#test-242)
- [Test 243: azmcp_sql_server_delete](#test-243)
- [Test 244: azmcp_sql_server_delete](#test-244)
- [Test 245: azmcp_sql_server_entra-admin_list](#test-245)
- [Test 246: azmcp_sql_server_entra-admin_list](#test-246)
- [Test 247: azmcp_sql_server_entra-admin_list](#test-247)
- [Test 248: azmcp_sql_server_firewall-rule_create](#test-248)
- [Test 249: azmcp_sql_server_firewall-rule_create](#test-249)
- [Test 250: azmcp_sql_server_firewall-rule_create](#test-250)
- [Test 251: azmcp_sql_server_firewall-rule_delete](#test-251)
- [Test 252: azmcp_sql_server_firewall-rule_delete](#test-252)
- [Test 253: azmcp_sql_server_firewall-rule_delete](#test-253)
- [Test 254: azmcp_sql_server_firewall-rule_list](#test-254)
- [Test 255: azmcp_sql_server_firewall-rule_list](#test-255)
- [Test 256: azmcp_sql_server_firewall-rule_list](#test-256)
- [Test 257: azmcp_sql_server_show](#test-257)
- [Test 258: azmcp_sql_server_show](#test-258)
- [Test 259: azmcp_sql_server_show](#test-259)
- [Test 260: azmcp_storage_account_create](#test-260)
- [Test 261: azmcp_storage_account_create](#test-261)
- [Test 262: azmcp_storage_account_create](#test-262)
- [Test 263: azmcp_storage_account_get](#test-263)
- [Test 264: azmcp_storage_account_get](#test-264)
- [Test 265: azmcp_storage_account_get](#test-265)
- [Test 266: azmcp_storage_account_get](#test-266)
- [Test 267: azmcp_storage_account_get](#test-267)
- [Test 268: azmcp_storage_blob_batch_set-tier](#test-268)
- [Test 269: azmcp_storage_blob_batch_set-tier](#test-269)
- [Test 270: azmcp_storage_blob_container_create](#test-270)
- [Test 271: azmcp_storage_blob_container_create](#test-271)
- [Test 272: azmcp_storage_blob_container_create](#test-272)
- [Test 273: azmcp_storage_blob_container_get](#test-273)
- [Test 274: azmcp_storage_blob_container_get](#test-274)
- [Test 275: azmcp_storage_blob_container_get](#test-275)
- [Test 276: azmcp_storage_blob_get](#test-276)
- [Test 277: azmcp_storage_blob_get](#test-277)
- [Test 278: azmcp_storage_blob_get](#test-278)
- [Test 279: azmcp_storage_blob_get](#test-279)
- [Test 280: azmcp_storage_blob_upload](#test-280)
- [Test 281: azmcp_storage_datalake_directory_create](#test-281)
- [Test 282: azmcp_storage_datalake_file-system_list-paths](#test-282)
- [Test 283: azmcp_storage_datalake_file-system_list-paths](#test-283)
- [Test 284: azmcp_storage_datalake_file-system_list-paths](#test-284)
- [Test 285: azmcp_storage_queue_message_send](#test-285)
- [Test 286: azmcp_storage_queue_message_send](#test-286)
- [Test 287: azmcp_storage_queue_message_send](#test-287)
- [Test 288: azmcp_storage_share_file_list](#test-288)
- [Test 289: azmcp_storage_share_file_list](#test-289)
- [Test 290: azmcp_storage_share_file_list](#test-290)
- [Test 291: azmcp_storage_table_list](#test-291)
- [Test 292: azmcp_storage_table_list](#test-292)
- [Test 293: azmcp_subscription_list](#test-293)
- [Test 294: azmcp_subscription_list](#test-294)
- [Test 295: azmcp_subscription_list](#test-295)
- [Test 296: azmcp_subscription_list](#test-296)
- [Test 297: azmcp_virtualdesktop_hostpool_list](#test-297)
- [Test 298: azmcp_virtualdesktop_hostpool_sessionhost_list](#test-298)
- [Test 299: azmcp_virtualdesktop_hostpool_sessionhost_usersession-list](#test-299)
- [Test 300: azmcp_workbooks_create](#test-300)
- [Test 301: azmcp_workbooks_delete](#test-301)
- [Test 302: azmcp_workbooks_list](#test-302)
- [Test 303: azmcp_workbooks_list](#test-303)
- [Test 304: azmcp_workbooks_show](#test-304)
- [Test 305: azmcp_workbooks_show](#test-305)
- [Test 306: azmcp_workbooks_update](#test-306)

---

## Test 1

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743538 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711544 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.541506 | `azmcp_search_service_list` | ❌ |
| 4 | 0.527464 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.516020 | `azmcp_subscription_list` | ❌ |

---

## Test 2

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585940 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563628 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.450287 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.382742 | `azmcp_mysql_server_list` | ❌ |

---

## Test 3

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637164 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563535 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.474000 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471804 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.463742 | `azmcp_postgres_server_list` | ❌ |

---

## Test 4

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654310 | `azmcp_acr_registry_repository_list` | ❌ |
| 2 | 0.633856 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476023 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.454929 | `azmcp_group_list` | ❌ |
| 5 | 0.454003 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 5

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639315 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637989 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.468034 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.449649 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `azmcp_group_list` | ❌ |

---

## Test 6

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626469 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617547 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.510435 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.495567 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.492550 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 7

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546249 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469285 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.407973 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.400145 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.339307 | `azmcp_mysql_database_list` | ❌ |

---

## Test 8

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674073 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541713 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.433927 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.388490 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.370375 | `azmcp_mysql_database_list` | ❌ |

---

## Test 9

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600573 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501800 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.418623 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.374628 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.359922 | `azmcp_mysql_database_list` | ❌ |

---

## Test 10

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661009 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.611387 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.579676 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.540767 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.481416 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 11

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666881 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.589055 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.545820 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.530314 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.508226 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 12

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567423 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563004 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.493940 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.486040 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.380301 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661426 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.578608 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.563549 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.534089 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.503925 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 14

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801054 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.690255 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.599940 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.594509 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.562043 | `azmcp_search_service_list` | ❌ |

---

## Test 15

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608059 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.536472 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.500890 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.492910 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.455228 | `azmcp_search_service_list` | ❌ |

---

## Test 16

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623859 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.538749 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.529985 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.466749 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.449602 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 17

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.753920 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.699423 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.597319 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.498546 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.482683 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 18

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678158 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.640096 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.481475 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.458596 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.445982 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 19

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599524 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.582330 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.412145 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.391546 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.385258 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694117 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.615516 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.531947 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.506624 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.487707 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 21

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712299 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.644451 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.547413 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.510269 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509732 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 22

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623160 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.580543 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.453658 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.443929 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.425537 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 23

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786343 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.635520 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.492243 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.491437 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.473652 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 24

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634978 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.533437 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.425610 | `azmcp_appconfig_kv_show` | ❌ |
| 4 | 0.379815 | `azmcp_eventgrid_subscription_list` | ❌ |
| 5 | 0.372456 | `azmcp_postgres_server_list` | ❌ |

---

## Test 25

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

---

## Test 26

**Expected Tool:** `azmcp_appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618277 | `azmcp_appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.486631 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.424344 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.399569 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 27

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

---

## Test 28

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

---

## Test 29

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

---

## Test 30

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555699 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.541557 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.476496 | `azmcp_appconfig_kv_delete` | ❌ |
| 4 | 0.435759 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.425488 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 31

**Expected Tool:** `azmcp_appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609817 | `azmcp_appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536639 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 3 | 0.518870 | `azmcp_appconfig_kv_list` | ❌ |
| 4 | 0.507232 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.505682 | `azmcp_appconfig_kv_delete` | ❌ |

---

## Test 32

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

---

## Test 33

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.355635 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.329345 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.257790 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.216077 | `azmcp_bestpractices_get` | ❌ |

---

## Test 34

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

---

## Test 35

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

---

## Test 36

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.516886 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.513156 | `azmcp_search_service_list` | ❌ |
| 5 | 0.510514 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |

---

## Test 37

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743917 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.613006 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.520222 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.514207 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.492108 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 38

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

---

## Test 39

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

---

## Test 40

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625270 | `azmcp_deploy_iac_rules_get` | ❌ |
| 3 | 0.605047 | `azmcp_bestpractices_get` | ❌ |
| 4 | 0.482936 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466199 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 41

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `azmcp_bestpractices_get` | ❌ |
| 3 | 0.510004 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.444297 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.440083 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 42

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.635406 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.531727 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490235 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 43

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.548542 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.541091 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.516852 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.516443 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 44

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.594323 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.518643 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.465572 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.454158 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 45

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.570488 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.522998 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.445382 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 46

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581850 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.497350 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495659 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.486886 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.474511 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 47

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.532790 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.487322 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.458060 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.413150 | `azmcp_functionapp_get` | ❌ |

---

## Test 48

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.513262 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.505123 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.405143 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 49

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.500368 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.472112 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.433134 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.385965 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 50

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a simple HTTP-triggered function app in javascript that returns a random compliment from a predefined list in a JSON response. And deploy it to azure eventually. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429170 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.408233 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.380754 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.377184 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 5 | 0.352369 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 51

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497276 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.493182 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.405146 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.395623 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.385140 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |

---

## Test 52

**Expected Tool:** `azmcp_bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485889 | `azmcp_deploy_iac_rules_get` | ❌ |
| 2 | 0.448373 | `azmcp_bestpractices_get` | ❌ |
| 3 | 0.440302 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.432773 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.432409 | `azmcp_bicepschema_get` | ✅ **EXPECTED** |

---

## Test 53

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

---

## Test 54

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.290270 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.267683 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258160 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.225622 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.215748 | `azmcp_bestpractices_get` | ❌ |

---

## Test 55

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

---

## Test 56

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

---

## Test 57

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668480 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.615268 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.588682 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.588050 | `azmcp_subscription_list` | ❌ |

---

## Test 58

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

---

## Test 59

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605125 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.566249 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.546591 | `azmcp_subscription_list` | ❌ |
| 5 | 0.535227 | `azmcp_storage_table_list` | ❌ |

---

## Test 60

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

---

## Test 61

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852831 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681046 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.630657 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.581591 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.535257 | `azmcp_storage_table_list` | ❌ |

---

## Test 62

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789415 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.614268 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.562124 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.537312 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.521507 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 63

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

---

## Test 64

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

---

## Test 65

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669338 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.435204 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.413185 | `azmcp_monitor_metrics_query` | ❌ |
| 4 | 0.409129 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.401915 | `azmcp_grafana_list` | ❌ |

---

## Test 66

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443868 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.393227 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.374071 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.371017 | `azmcp_grafana_list` | ❌ |

---

## Test 67

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686701 | `azmcp_deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.404890 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.392565 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.389603 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 68

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

---

## Test 69

**Expected Tool:** `azmcp_deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529092 | `azmcp_deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.404829 | `azmcp_bicepschema_get` | ❌ |
| 3 | 0.391965 | `azmcp_bestpractices_get` | ❌ |
| 4 | 0.383210 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.341436 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 70

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `azmcp_deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.448918 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.382240 | `azmcp_bestpractices_get` | ❌ |
| 5 | 0.375202 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 71

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688055 | `azmcp_deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.432825 | `azmcp_bestpractices_get` | ❌ |

---

## Test 72

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682900 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.636180 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.486216 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.480944 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.478217 | `azmcp_servicebus_topic_details` | ❌ |

---

## Test 73

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672482 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.655324 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.539977 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.498485 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.460145 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 74

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665867 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.663335 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.524919 | `azmcp_group_list` | ❌ |
| 4 | 0.488696 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484167 | `azmcp_servicebus_topic_subscription_details` | ❌ |

---

## Test 75

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594210 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.593171 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.525237 | `azmcp_subscription_list` | ❌ |
| 4 | 0.518857 | `azmcp_search_service_list` | ❌ |
| 5 | 0.509007 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 76

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604278 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.602603 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.535955 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.518121 | `azmcp_subscription_list` | ❌ |
| 5 | 0.510115 | `azmcp_group_list` | ❌ |

---

## Test 77

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618614 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.557573 | `azmcp_group_list` | ❌ |
| 3 | 0.531313 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.504984 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.502308 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 78

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652090 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.581728 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.480537 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.478218 | `azmcp_subscription_list` | ❌ |
| 5 | 0.476763 | `azmcp_search_service_list` | ❌ |

---

## Test 79

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759178 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.610435 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.545540 | `azmcp_search_service_list` | ❌ |
| 4 | 0.514189 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.496586 | `azmcp_subscription_list` | ❌ |

---

## Test 80

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691068 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.600983 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.478334 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.475119 | `azmcp_search_service_list` | ❌ |
| 5 | 0.450712 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 81

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759396 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.632284 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.526595 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `azmcp_search_service_list` | ❌ |
| 5 | 0.495814 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659417 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.615084 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.609343 | `azmcp_group_list` | ❌ |
| 4 | 0.514238 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.506181 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 83

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `azmcp_quota_usage_check` | ❌ |
| 2 | 0.497434 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.481143 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.476826 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.451690 | `azmcp_bestpractices_get` | ❌ |

---

## Test 84

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `azmcp_bestpractices_get` | ❌ |
| 3 | 0.488377 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.473365 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.462743 | `azmcp_extension_azqr` | ✅ **EXPECTED** |

---

## Test 85

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.504673 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.494872 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.487387 | `azmcp_bestpractices_get` | ❌ |

---

## Test 86

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695201 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.526528 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.433117 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.422779 | `azmcp_search_index_get` | ❌ |
| 5 | 0.412895 | `azmcp_search_service_list` | ❌ |

---

## Test 87

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

---

## Test 88

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

---

## Test 89

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650269 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432759 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.415963 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.408316 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.398186 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 90

**Expected Tool:** `azmcp_foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.313400 | `azmcp_foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.282481 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.274011 | `azmcp_deploy_plan_get` | ❌ |
| 4 | 0.269553 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.268967 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 91

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559508 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.549636 | `azmcp_foundry_models_list` | ❌ |
| 3 | 0.533239 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.448711 | `azmcp_search_service_list` | ❌ |
| 5 | 0.434472 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 92

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

---

## Test 93

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

---

## Test 94

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574818 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.430513 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.388967 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.356899 | `azmcp_foundry_models_deployments_list` | ❌ |
| 5 | 0.339069 | `azmcp_search_service_list` | ❌ |

---

## Test 95

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.448179 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.390059 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.380314 | `azmcp_bestpractices_get` | ❌ |
| 5 | 0.379655 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 96

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

---

## Test 97

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.460102 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.420189 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.390708 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.334473 | `azmcp_bestpractices_get` | ❌ |

---

## Test 98

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

---

## Test 99

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

---

## Test 100

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.445142 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.368188 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.366279 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.365569 | `azmcp_bestpractices_get` | ❌ |

---

## Test 101

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433958 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.388678 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.370793 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.368420 | `azmcp_storage_blob_get` | ❌ |

---

## Test 102

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

---

## Test 103

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565797 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.440329 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.422774 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.384159 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.342552 | `azmcp_bestpractices_get` | ❌ |

---

## Test 104

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.559382 | `azmcp_search_service_list` | ❌ |
| 3 | 0.516618 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.516217 | `azmcp_appconfig_account_list` | ❌ |
| 5 | 0.485403 | `azmcp_subscription_list` | ❌ |

---

## Test 105

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.452132 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.412646 | `azmcp_search_service_list` | ❌ |
| 4 | 0.411323 | `azmcp_bestpractices_get` | ❌ |
| 5 | 0.385832 | `azmcp_foundry_models_deployments_list` | ❌ |

---

## Test 106

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433674 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.348106 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.284362 | `azmcp_bestpractices_get` | ❌ |
| 4 | 0.281676 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.249658 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 107

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

---

## Test 108

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

---

## Test 109

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.463685 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.462383 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.459304 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.453960 | `azmcp_workbooks_list` | ❌ |

---

## Test 110

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

---

## Test 111

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.740327 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.595854 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.590531 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.575950 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.543057 | `azmcp_keyvault_certificate_get` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627967 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.624455 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.565005 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.539554 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.493432 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 113

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662317 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.606531 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.540155 | `azmcp_keyvault_certificate_import` | ❌ |
| 4 | 0.535157 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.499272 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649993 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.521183 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.469726 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.467090 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.426600 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 115

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649676 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.629902 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.527467 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.525759 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.491898 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 116

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.762001 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.637437 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.608976 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.566484 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.539624 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 117

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660568 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.570291 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.540050 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.516920 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.509123 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676352 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.569250 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.555829 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.465742 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.417376 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 119

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737135 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.650393 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.631519 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.498767 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.473916 | `azmcp_storage_table_list` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609392 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.535646 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.520008 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.479833 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.462249 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.768140 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.614040 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.572679 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.516660 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.461900 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 122

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747294 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.617131 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.569903 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.519133 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.455500 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 123

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615572 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.520654 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.502403 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.467740 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.456345 | `azmcp_keyvault_certificate_get` | ❌ |

---

## Test 124

**Expected Tool:** `azmcp_kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482148 | `azmcp_kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.464557 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.457669 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.416762 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.378455 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 125

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.651218 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.644037 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.549093 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.536043 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.509396 | `azmcp_grafana_list` | ❌ |

---

## Test 126

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

---

## Test 127

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584068 | `azmcp_redis_cluster_list` | ❌ |
| 2 | 0.549830 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 3 | 0.471142 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.469626 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.464375 | `azmcp_kusto_database_list` | ❌ |

---

## Test 128

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

---

## Test 129

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

---

## Test 130

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.380954 | `azmcp_kusto_query` | ✅ **EXPECTED** |
| 2 | 0.363598 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.363010 | `azmcp_kusto_sample` | ❌ |
| 4 | 0.348911 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.345737 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 131

**Expected Tool:** `azmcp_kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537154 | `azmcp_kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.419463 | `azmcp_kusto_table_schema` | ❌ |
| 3 | 0.391423 | `azmcp_kusto_table_list` | ❌ |
| 4 | 0.391248 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.380691 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 132

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

---

## Test 133

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549885 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.524691 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.523432 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.494108 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.490717 | `azmcp_monitor_table_list` | ❌ |

---

## Test 134

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

---

## Test 135

**Expected Tool:** `azmcp_loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585388 | `azmcp_loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.531935 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.508690 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.413857 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.394664 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642528 | `azmcp_loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.608922 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.574853 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.534232 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.473347 | `azmcp_loadtesting_testrun_create` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718065 | `azmcp_loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.596828 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.514437 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.476662 | `azmcp_loadtesting_testrun_create` | ❌ |
| 5 | 0.443117 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738027 | `azmcp_loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.592196 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.577408 | `azmcp_group_list` | ❌ |
| 4 | 0.565565 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.561516 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621803 | `azmcp_loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.593628 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.540789 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.530882 | `azmcp_loadtesting_testrun_update` | ❌ |
| 5 | 0.488142 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625244 | `azmcp_loadtesting_test_get` | ❌ |
| 2 | 0.603084 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.568330 | `azmcp_loadtesting_testrun_get` | ✅ **EXPECTED** |
| 4 | 0.562405 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.535180 | `azmcp_loadtesting_testrun_create` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615946 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.606032 | `azmcp_loadtesting_test_get` | ❌ |
| 3 | 0.569118 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.565077 | `azmcp_loadtesting_testrun_list` | ✅ **EXPECTED** |
| 5 | 0.535602 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659812 | `azmcp_loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.509199 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.454745 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.443828 | `azmcp_loadtesting_test_get` | ❌ |
| 5 | 0.422757 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570145 | `azmcp_marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.477592 | `azmcp_marketplace_product_list` | ❌ |
| 3 | 0.353256 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.330935 | `azmcp_servicebus_queue_details` | ❌ |
| 5 | 0.324083 | `azmcp_search_index_get` | ❌ |

---

## Test 144

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527074 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443133 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.343549 | `azmcp_search_service_list` | ❌ |
| 4 | 0.330500 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.328676 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 145

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461668 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385167 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.308769 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.260387 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.259270 | `azmcp_redis_cache_list` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498518 | `azmcp_monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.472064 | `azmcp_monitor_workspace_list` | ❌ |
| 3 | 0.468174 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.467867 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.463303 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 147

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592640 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424042 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.332356 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.315519 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.315310 | `azmcp_servicebus_topic_details` | ❌ |

---

## Test 148

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

---

## Test 149

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633173 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495465 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.382374 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.380460 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.370848 | `azmcp_monitor_table_type_list` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555601 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.447607 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.447192 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.433777 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.422404 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557886 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.508674 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.460611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.438233 | `azmcp_monitor_metrics_definitions` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461446 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390109 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.306453 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.304427 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.301883 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.491831 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.417008 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.415966 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.406200 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.398988 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525482 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.384577 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.376582 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.367139 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.299442 | `azmcp_quota_usage_check` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480493 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.381961 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.363412 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.359285 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.350523 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 156

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594054 | `azmcp_monitor_workspace_log_query` | ❌ |
| 2 | 0.580076 | `azmcp_monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.472051 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.470141 | `azmcp_monitor_metrics_query` | ❌ |
| 5 | 0.443506 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 157

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851075 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.586691 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.534829 | `azmcp_mysql_table_list` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798463 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701108 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.599917 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.532874 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.496997 | `azmcp_mysql_table_list` | ❌ |

---

## Test 159

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

---

## Test 160

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843138 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736837 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.576731 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.502460 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.481189 | `azmcp_mysql_table_list` | ❌ |

---

## Test 161

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

---

## Test 162

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

---

## Test 163

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `azmcp_grafana_list` | ❌ |
| 3 | 0.580261 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.521316 | `azmcp_monitor_table_type_list` | ❌ |
| 5 | 0.521276 | `azmcp_search_service_list` | ❌ |

---

## Test 164

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

---

## Test 165

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634056 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.623421 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534457 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.498910 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.490148 | `azmcp_sql_db_list` | ❌ |

---

## Test 166

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588122 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.574089 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.483855 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.463238 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.448169 | `azmcp_redis_cluster_database_list` | ❌ |

---

## Test 167

**Expected Tool:** `azmcp_mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476475 | `azmcp_mysql_table_list` | ❌ |
| 2 | 0.455841 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.433317 | `azmcp_mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419875 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.409475 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 168

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

---

## Test 169

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678472 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.558177 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.554806 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.501199 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.482079 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 170

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478518 | `azmcp_mysql_database_list` | ❌ |
| 2 | 0.474573 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.412380 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.389993 | `azmcp_postgres_database_list` | ❌ |

---

## Test 171

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.534257 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530210 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.464360 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.456616 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `azmcp_mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.407671 | `azmcp_mysql_server_param_set` | ❌ |
| 3 | 0.333924 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.313150 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.310834 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 173

**Expected Tool:** `azmcp_mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.390761 | `azmcp_mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `azmcp_mysql_server_param_get` | ❌ |
| 3 | 0.307496 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.299283 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.254163 | `azmcp_mysql_server_list` | ❌ |

---

## Test 174

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

---

## Test 175

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

---

## Test 176

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

---

## Test 177

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

---

## Test 178

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

---

## Test 179

**Expected Tool:** `azmcp_postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546211 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.503267 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.466599 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.415805 | `azmcp_postgres_database_query` | ✅ **EXPECTED** |
| 5 | 0.403969 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 180

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

---

## Test 181

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

---

## Test 182

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

---

## Test 183

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

---

## Test 184

**Expected Tool:** `azmcp_postgres_server_param_get`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594753 | `azmcp_postgres_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.539671 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.489693 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.480826 | `azmcp_postgres_server_param_set` | ❌ |
| 5 | 0.451871 | `azmcp_postgres_database_list` | ❌ |

---

## Test 185

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

---

## Test 186

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

---

## Test 187

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

---

## Test 188

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

---

## Test 189

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

---

## Test 190

**Expected Tool:** `azmcp_quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609244 | `azmcp_quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `azmcp_quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383928 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.379029 | `azmcp_redis_cache_list` | ❌ |

---

## Test 191

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

---

## Test 192

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713839 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.523153 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.412377 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.338859 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.286321 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 193

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

---

## Test 194

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

---

## Test 195

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

---

## Test 196

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

---

## Test 197

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

---

## Test 198

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812960 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.679028 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.672104 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.588847 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.569226 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 199

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

---

## Test 200

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

---

## Test 201

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630647 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377586 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.349980 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.331563 | `azmcp_monitor_metrics_definitions` | ❌ |

---

## Test 202

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

---

## Test 203

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 2 | 0.570884 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424938 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.393479 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.386598 | `azmcp_quota_usage_check` | ❌ |

---

## Test 204

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

---

## Test 205

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.587088 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.508252 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.473905 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.462125 | `azmcp_search_service_list` | ❌ |

---

## Test 206

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.543421 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.427638 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.420567 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.420385 | `azmcp_mysql_server_list` | ❌ |

---

## Test 207

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.719917 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `azmcp_search_service_list` | ❌ |
| 3 | 0.533215 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.518372 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.503744 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 208

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726947 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.513815 | `azmcp_search_service_list` | ❌ |
| 3 | 0.509817 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.491121 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484386 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 209

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

---

## Test 210

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711107 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.548286 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.520197 | `azmcp_search_service_list` | ❌ |
| 4 | 0.502064 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.487327 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 211

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

---

## Test 212

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.483988 | `azmcp_group_list` | ❌ |
| 3 | 0.483302 | `azmcp_subscription_list` | ❌ |
| 4 | 0.478700 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474796 | `azmcp_redis_cache_list` | ❌ |

---

## Test 213

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609705 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.456956 | `azmcp_grafana_list` | ❌ |
| 3 | 0.436776 | `azmcp_subscription_list` | ❌ |
| 4 | 0.435642 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.435155 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 214

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681052 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544557 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.490624 | `azmcp_search_service_list` | ❌ |
| 4 | 0.466005 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.459609 | `azmcp_search_index_query` | ❌ |

---

## Test 215

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

---

## Test 216

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620759 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562775 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561154 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.471416 | `azmcp_search_index_query` | ❌ |
| 5 | 0.463972 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 217

**Expected Tool:** `azmcp_search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522627 | `azmcp_search_index_get` | ❌ |
| 2 | 0.515940 | `azmcp_search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497514 | `azmcp_search_service_list` | ❌ |
| 4 | 0.374044 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.372909 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 218

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

---

## Test 219

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.479898 | `azmcp_search_index_get` | ❌ |
| 3 | 0.453454 | `azmcp_marketplace_product_list` | ❌ |
| 4 | 0.448446 | `azmcp_search_index_query` | ❌ |
| 5 | 0.425939 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 220

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

---

## Test 221

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `azmcp_servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.400870 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.382417 | `azmcp_storage_queue_message_send` | ❌ |
| 5 | 0.375447 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 222

**Expected Tool:** `azmcp_servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591649 | `azmcp_servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571861 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.498915 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.494885 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.483976 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 223

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `azmcp_servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.527109 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.494515 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.457036 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.444604 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 224

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470892 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.376833 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.359945 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.357421 | `azmcp_sql_db_list` | ❌ |

---

## Test 225

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459672 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.420843 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.396106 | `azmcp_sql_db_update` | ❌ |
| 5 | 0.395495 | `azmcp_sql_server_delete` | ❌ |

---

## Test 226

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603998 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545889 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.494309 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.473807 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.456392 | `azmcp_storage_account_create` | ❌ |

---

## Test 227

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520786 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.484026 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.386564 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.364776 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.351204 | `azmcp_postgres_database_list` | ❌ |

---

## Test 228

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579119 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.500756 | `azmcp_sql_db_show` | ❌ |
| 3 | 0.478729 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.466216 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 5 | 0.421365 | `azmcp_sql_db_create` | ❌ |

---

## Test 229

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.459422 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.427494 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.364494 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.355416 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.319617 | `azmcp_sql_db_show` | ❌ |

---

## Test 230

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639694 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.609178 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.602890 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.532407 | `azmcp_sql_server_show` | ❌ |

---

## Test 231

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617755 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.609394 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.557385 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.553495 | `azmcp_mysql_server_config_get` | ❌ |
| 5 | 0.524351 | `azmcp_sql_db_show` | ❌ |

---

## Test 232

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

---

## Test 233

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

---

## Test 234

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565229 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.427621 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.385817 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.377337 | `azmcp_storage_blob_batch_set-tier` | ❌ |

---

## Test 235

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.402138 | `azmcp_sql_db_list` | ❌ |
| 2 | 0.394998 | `azmcp_sql_db_show` | ❌ |
| 3 | 0.389603 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 4 | 0.386482 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.381735 | `azmcp_sql_db_create` | ❌ |

---

## Test 236

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

---

## Test 237

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606425 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.457163 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.438522 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.432816 | `azmcp_mysql_database_list` | ❌ |

---

## Test 238

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592709 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420325 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.402611 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.397670 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.397640 | `azmcp_sql_server_show` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682576 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563532 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.536357 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.481395 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.472979 | `azmcp_sql_db_show` | ❌ |

---

## Test 240

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618309 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510169 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.472463 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.434810 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.396073 | `azmcp_storage_account_create` | ❌ |

---

## Test 241

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589818 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.501403 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.469425 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.442915 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.423887 | `azmcp_sql_server_show` | ❌ |

---

## Test 242

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702353 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.495550 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.486195 | `azmcp_sql_db_delete` | ❌ |
| 4 | 0.483132 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.470205 | `azmcp_sql_db_show` | ❌ |

---

## Test 243

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

---

## Test 244

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527930 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.396541 | `azmcp_sql_db_delete` | ❌ |
| 3 | 0.362389 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.341503 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.315820 | `azmcp_workbooks_delete` | ❌ |

---

## Test 245

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

---

## Test 246

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

---

## Test 247

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

---

## Test 248

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635466 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532712 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448822 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.432802 | `azmcp_sql_db_create` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670225 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533583 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503661 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.295006 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.287457 | `azmcp_sql_server_create` | ❌ |

---

## Test 250

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685107 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574336 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539577 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428919 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.395165 | `azmcp_sql_db_create` | ❌ |

---

## Test 251

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.543857 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.540333 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.527546 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.436585 | `azmcp_sql_db_delete` | ❌ |

---

## Test 252

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

---

## Test 253

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671212 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601230 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.577330 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.441605 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.367883 | `azmcp_sql_server_show` | ❌ |

---

## Test 254

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

---

## Test 255

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

---

## Test 256

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

---

## Test 257

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `azmcp_sql_db_show` | ❌ |
| 2 | 0.595184 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.559879 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.540218 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.491401 | `azmcp_sql_server_create` | ❌ |

---

## Test 258

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

---

## Test 259

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

---

## Test 260

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.418472 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.394541 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.391586 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.374006 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 261

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500720 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.400100 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.387100 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.382780 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.377206 | `azmcp_sql_db_create` | ❌ |

---

## Test 262

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589003 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.464611 | `azmcp_storage_blob_container_create` | ❌ |
| 3 | 0.447156 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.444359 | `azmcp_storage_datalake_directory_create` | ❌ |
| 5 | 0.437040 | `azmcp_storage_account_get` | ❌ |

---

## Test 263

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655152 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.603853 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.507638 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.504386 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.483435 | `azmcp_storage_account_create` | ❌ |

---

## Test 264

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

---

## Test 265

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664087 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.581393 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.557016 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.536909 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.535616 | `azmcp_storage_account_create` | ❌ |

---

## Test 266

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499302 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.461284 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.455449 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.450677 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.421642 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 267

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557142 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.499153 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.473598 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.461641 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.454028 | `azmcp_subscription_list` | ❌ |

---

## Test 268

**Expected Tool:** `azmcp_storage_blob_batch_set-tier`  
**Prompt:** Set access tier to Cool for multiple blobs in the container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620659 | `azmcp_storage_blob_batch_set-tier` | ✅ **EXPECTED** |
| 2 | 0.465706 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.408688 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.408291 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.378335 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 269

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

---

## Test 270

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563396 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.524779 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.508053 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.447784 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.403407 | `azmcp_storage_account_get` | ❌ |

---

## Test 271

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512578 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.500624 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.470927 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415378 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.414820 | `azmcp_storage_blob_get` | ❌ |

---

## Test 272

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463198 | `azmcp_storage_account_create` | ❌ |
| 2 | 0.455375 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.451690 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 4 | 0.435099 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.388450 | `azmcp_storage_blob_get` | ❌ |

---

## Test 273

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665176 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.559177 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.523288 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.518763 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.496184 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 274

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

---

## Test 275

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

---

## Test 276

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613091 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.586289 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.483614 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.477946 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.434667 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 277

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662106 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.661919 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.537535 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.460657 | `azmcp_storage_blob_container_create` | ❌ |
| 5 | 0.457038 | `azmcp_storage_account_create` | ❌ |

---

## Test 278

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

---

## Test 279

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

---

## Test 280

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566213 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.403309 | `azmcp_storage_blob_get` | ❌ |
| 3 | 0.397499 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.382052 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.377014 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 281

**Expected Tool:** `azmcp_storage_datalake_directory_create`  
**Prompt:** Create a new directory at the path <directory_path> in Data Lake in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648330 | `azmcp_storage_datalake_directory_create` | ✅ **EXPECTED** |
| 2 | 0.482873 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 3 | 0.443022 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.349702 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.341619 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 282

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

---

## Test 283

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

---

## Test 284

**Expected Tool:** `azmcp_storage_datalake_file-system_list-paths`  
**Prompt:** Recursively list all paths in the Data Lake file system <file_system> in the storage account <account> filtered by <filter_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685260 | `azmcp_storage_datalake_file-system_list-paths` | ✅ **EXPECTED** |
| 2 | 0.465158 | `azmcp_storage_share_file_list` | ❌ |
| 3 | 0.431539 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 4 | 0.418199 | `azmcp_storage_datalake_directory_create` | ❌ |
| 5 | 0.394456 | `azmcp_storage_table_list` | ❌ |

---

## Test 285

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Send a message "Hello, World!" to the queue <queue> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558995 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.411169 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.375260 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.373398 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.344298 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 286

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

---

## Test 287

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Add a message to the queue <queue> in storage account <account> with visibility timeout of 30 seconds  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595279 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.360634 | `azmcp_servicebus_queue_details` | ❌ |
| 3 | 0.338574 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.325301 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.322605 | `azmcp_storage_table_list` | ❌ |

---

## Test 288

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

---

## Test 289

**Expected Tool:** `azmcp_storage_share_file_list`  
**Prompt:** Show me the files in the File Share <share> directory <directory_path> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552006 | `azmcp_storage_share_file_list` | ✅ **EXPECTED** |
| 2 | 0.511236 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 3 | 0.452271 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.443743 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.425236 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 290

**Expected Tool:** `azmcp_storage_share_file_list`  
**Prompt:** List files with prefix 'report' in the File Share <share> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602266 | `azmcp_storage_share_file_list` | ✅ **EXPECTED** |
| 2 | 0.449421 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.445997 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 4 | 0.436470 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.423719 | `azmcp_extension_azqr` | ❌ |

---

## Test 291

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

---

## Test 292

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

---

## Test 293

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576403 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.473852 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471653 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.467765 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 294

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405813 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.384208 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.381238 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.351864 | `azmcp_grafana_list` | ❌ |
| 5 | 0.350951 | `azmcp_redis_cache_list` | ❌ |

---

## Test 295

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.319893 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.315547 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.313335 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.286711 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.282645 | `azmcp_grafana_list` | ❌ |

---

## Test 296

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.403186 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.375168 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.354504 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.342318 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.340339 | `azmcp_grafana_list` | ❌ |

---

## Test 297

**Expected Tool:** `azmcp_virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711969 | `azmcp_virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659745 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.566615 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.548888 | `azmcp_search_service_list` | ❌ |
| 5 | 0.536542 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 298

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726982 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714469 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.439611 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.402909 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 299

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812659 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.659093 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501167 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.356479 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.336385 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 300

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

---

## Test 301

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

---

## Test 302

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772431 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.532565 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.516739 | `azmcp_grafana_list` | ❌ |
| 5 | 0.488600 | `azmcp_group_list` | ❌ |

---

## Test 303

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570259 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.539957 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.485504 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.472378 | `azmcp_grafana_list` | ❌ |

---

## Test 304

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.697539 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498390 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.494708 | `azmcp_workbooks_list` | ❌ |
| 4 | 0.452348 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.419105 | `azmcp_workbooks_update` | ❌ |

---

## Test 305

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

---

## Test 306

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

---

## Summary

**Total Prompts Tested:** 306  
**Execution Time:** 57.5946338s  

### Success Rate Metrics

**Top Choice Success:** 85.0% (260/306 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 4.6% (14/306 tests)  
**🎯 High Confidence (≥0.7):** 20.9% (64/306 tests)  
**✅ Good Confidence (≥0.6):** 59.2% (181/306 tests)  
**👍 Fair Confidence (≥0.5):** 85.6% (262/306 tests)  
**👌 Acceptable Confidence (≥0.4):** 95.1% (291/306 tests)  
**❌ Low Confidence (<0.4):** 4.9% (15/306 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 4.6% (14/306 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 20.9% (64/306 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 56.2% (172/306 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 77.1% (236/306 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 82.0% (251/306 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing adequately but has room for improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

