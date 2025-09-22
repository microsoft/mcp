# Tool Selection Analysis Setup

**Setup completed:** 2025-09-22 14:01:52  
**Tool count:** 140  
**Database setup time:** 2.0399131s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-22 14:01:52  
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
- [Test 184: azmcp_postgres_server_param](#test-184)
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
| 1 | 0.743568 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.541506 | `azmcp_search_service_list` | ❌ |
| 4 | 0.527263 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.515977 | `azmcp_subscription_list` | ❌ |
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

## Test 2

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
| 10 | 0.356316 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.354277 | `azmcp_storage_blob_container_create` | ❌ |
| 12 | 0.353404 | `azmcp_subscription_list` | ❌ |
| 13 | 0.352818 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.349526 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.349291 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.348080 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.344750 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.344071 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.339252 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.336892 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 3

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
| 6 | 0.459880 | `azmcp_search_service_list` | ❌ |
| 7 | 0.452938 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.451253 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.443939 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.440533 | `azmcp_subscription_list` | ❌ |
| 11 | 0.435835 | `azmcp_grafana_list` | ❌ |
| 12 | 0.435706 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.431745 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.430722 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.430308 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.422368 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.409093 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.404664 | `azmcp_group_list` | ❌ |
| 19 | 0.398556 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.386495 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 4

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
| 14 | 0.374979 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.371025 | `azmcp_sql_elastic-pool_list` | ❌ |
| 16 | 0.370359 | `azmcp_redis_cluster_database_list` | ❌ |
| 17 | 0.366482 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.356119 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.354145 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.352209 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 5

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
| 6 | 0.416353 | `azmcp_cosmos_database_container_list` | ❌ |
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
| 17 | 0.354701 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.351411 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.344334 | `azmcp_eventgrid_subscription_list` | ❌ |
| 20 | 0.344148 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 6

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
| 9 | 0.461369 | `azmcp_grafana_list` | ❌ |
| 10 | 0.456838 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.449239 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.448228 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.440117 | `azmcp_subscription_list` | ❌ |
| 14 | 0.438019 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.437551 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.430939 | `azmcp_group_list` | ❌ |
| 17 | 0.423301 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.414463 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.405472 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.390890 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 7

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546333 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.407973 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.400145 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.339307 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.326631 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.308650 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.306442 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.302635 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.300174 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.296073 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.293421 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.292155 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.290148 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.289864 | `azmcp_search_service_list` | ❌ |
| 16 | 0.283716 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.283390 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.282581 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.276498 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.272962 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 8

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
| 14 | 0.338490 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.337543 | `azmcp_keyvault_certificate_list` | ❌ |
| 16 | 0.333079 | `azmcp_keyvault_key_list` | ❌ |
| 17 | 0.332785 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.332704 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.332572 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.330046 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 9

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
| 13 | 0.314960 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.311692 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.309627 | `azmcp_search_service_list` | ❌ |
| 16 | 0.306052 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.304725 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.303931 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.300101 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.299629 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 10

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660869 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.611424 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.579755 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.540767 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.481416 | `azmcp_mysql_server_config_get` | ❌ |
| 6 | 0.463682 | `azmcp_kusto_cluster_get` | ❌ |
| 7 | 0.463065 | `azmcp_loadtesting_test_get` | ❌ |
| 8 | 0.430975 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.419629 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.399345 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.391924 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.390959 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.390819 | `azmcp_appconfig_kv_list` | ❌ |
| 14 | 0.390141 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.371630 | `azmcp_mysql_server_param_get` | ❌ |
| 16 | 0.370291 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.369525 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.367841 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.360930 | `azmcp_storage_blob_get` | ❌ |
| 20 | 0.350240 | `azmcp_sql_db_show` | ❌ |

---

## Test 11

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666849 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.589024 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.545849 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.530314 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.508226 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.461466 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.448796 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.428449 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.422993 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.413625 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.408420 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
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

## Test 12

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567273 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563263 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.493940 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.486117 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.380301 | `azmcp_mysql_server_config_get` | ❌ |
| 6 | 0.368584 | `azmcp_kusto_cluster_get` | ❌ |
| 7 | 0.342696 | `azmcp_loadtesting_test_get` | ❌ |
| 8 | 0.340293 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.334923 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.334860 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.329324 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.315228 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.314513 | `azmcp_appconfig_kv_list` | ❌ |
| 14 | 0.309738 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.299047 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.296849 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.296592 | `azmcp_postgres_server_config_get` | ❌ |
| 18 | 0.289342 | `azmcp_mysql_server_param_get` | ❌ |
| 19 | 0.275751 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.273195 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661441 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.578731 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.563603 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.534094 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.503906 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.434665 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.433860 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.419470 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.418598 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.417892 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.405712 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.404999 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.402441 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.399511 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.391770 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.384817 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.376933 | `azmcp_search_index_get` | ❌ |
| 18 | 0.372852 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367570 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.359952 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 14

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801015 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.690255 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.599940 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.594509 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.562043 | `azmcp_search_service_list` | ❌ |
| 6 | 0.560861 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.543684 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.515922 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.509202 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.502428 | `azmcp_subscription_list` | ❌ |
| 11 | 0.498286 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 12 | 0.498121 | `azmcp_group_list` | ❌ |
| 13 | 0.495977 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.486142 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.483592 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.482355 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.481469 | `azmcp_grafana_list` | ❌ |
| 18 | 0.452959 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 19 | 0.452681 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.445271 | `azmcp_storage_table_list` | ❌ |

---

## Test 15

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608090 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.536412 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.500890 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.492910 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.455228 | `azmcp_search_service_list` | ❌ |
| 6 | 0.446270 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.416555 | `azmcp_aks_nodepool_get` | ❌ |
| 8 | 0.409711 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.408385 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.392997 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.376362 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.371809 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.371535 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.370963 | `azmcp_search_index_get` | ❌ |
| 15 | 0.370237 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.363825 | `azmcp_subscription_list` | ❌ |
| 17 | 0.361928 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.358420 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.356926 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.356016 | `azmcp_storage_account_get` | ❌ |

---

## Test 16

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624050 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.538749 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.530023 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.467075 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.449602 | `azmcp_kusto_cluster_list` | ❌ |
| 6 | 0.416564 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.392083 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.378826 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.377567 | `azmcp_acr_registry_repository_list` | ❌ |
| 10 | 0.374585 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.364022 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.353365 | `azmcp_search_service_list` | ❌ |
| 13 | 0.345290 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.345241 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.341581 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.337354 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.317977 | `azmcp_sql_elastic-pool_list` | ❌ |
| 18 | 0.317238 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.312380 | `azmcp_subscription_list` | ❌ |
| 20 | 0.311971 | `azmcp_quota_usage_check` | ❌ |

---

## Test 17

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.753870 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.699423 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.597308 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.498496 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.482683 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.468392 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.463192 | `azmcp_sql_elastic-pool_list` | ❌ |
| 8 | 0.434875 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.414751 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 10 | 0.401610 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.399215 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.383565 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 13 | 0.382352 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.380152 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.378264 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.378238 | `azmcp_search_index_get` | ❌ |
| 17 | 0.370172 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.362512 | `azmcp_loadtesting_test_get` | ❌ |
| 19 | 0.356766 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.343270 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 18

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678102 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.640096 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.481312 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.458596 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.445990 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.440182 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.389989 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 8 | 0.384600 | `azmcp_loadtesting_test_get` | ❌ |
| 9 | 0.367455 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.365231 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.357721 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.350998 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.350992 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 14 | 0.344818 | `azmcp_sql_db_show` | ❌ |
| 15 | 0.343726 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.342564 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.338364 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.329963 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.322685 | `azmcp_appconfig_kv_show` | ❌ |
| 20 | 0.321672 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 19

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599509 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.582325 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.412109 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.391743 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.385173 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 6 | 0.383045 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.346262 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 8 | 0.338624 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.323119 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.320733 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.314439 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.306678 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.306579 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.300123 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.298866 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.289422 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.287084 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.283171 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.276058 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.266184 | `azmcp_sql_db_show` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694091 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.615439 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.531869 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.506631 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.487684 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.461603 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.446778 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.440631 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.438612 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.435279 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.431546 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.418708 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 13 | 0.413137 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.404836 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.399316 | `azmcp_acr_registry_list` | ❌ |
| 16 | 0.393884 | `azmcp_group_list` | ❌ |
| 17 | 0.391849 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.389151 | `azmcp_redis_cluster_database_list` | ❌ |
| 19 | 0.385847 | `azmcp_workbooks_list` | ❌ |
| 20 | 0.379548 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 21

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712299 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.644495 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.547377 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.510269 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509732 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 6 | 0.497966 | `azmcp_aks_cluster_get` | ❌ |
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

## Test 22

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623138 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.580565 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.453802 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.443902 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.425448 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.409286 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.386949 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.378905 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.368944 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.363290 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.359493 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 12 | 0.356345 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.356139 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.354542 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.329036 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.324552 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.324311 | `azmcp_deploy_plan_get` | ❌ |
| 18 | 0.323568 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.322487 | `azmcp_foundry_models_deployments_list` | ❌ |
| 20 | 0.319684 | `azmcp_redis_cluster_database_list` | ❌ |

---

## Test 23

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
| 8 | 0.433600 | `azmcp_eventgrid_topic_list` | ❌ |
| 9 | 0.432238 | `azmcp_search_service_list` | ❌ |
| 10 | 0.429300 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.427687 | `azmcp_subscription_list` | ❌ |
| 12 | 0.427460 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.420794 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.412274 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.408599 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.404636 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.398346 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.387414 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.385938 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.380818 | `azmcp_quota_region_availability_list` | ❌ |

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
| 6 | 0.368731 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.359567 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.356514 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.355830 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.354747 | `azmcp_appconfig_kv_delete` | ❌ |
| 11 | 0.348603 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.344562 | `azmcp_marketplace_product_get` | ❌ |
| 13 | 0.341263 | `azmcp_grafana_list` | ❌ |
| 14 | 0.340756 | `azmcp_eventgrid_topic_list` | ❌ |
| 15 | 0.332824 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.325934 | `azmcp_subscription_list` | ❌ |
| 17 | 0.325232 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.310432 | `azmcp_search_service_list` | ❌ |
| 19 | 0.296589 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.292788 | `azmcp_monitor_workspace_list` | ❌ |

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
| 18 | 0.226029 | `azmcp_sql_db_update` | ❌ |
| 19 | 0.221645 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.221405 | `azmcp_postgres_database_list` | ❌ |

---

## Test 26

**Expected Tool:** `azmcp_appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618394 | `azmcp_appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.486817 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.424412 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.422801 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.399674 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.392152 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.268837 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.261981 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.248718 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.240471 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.218310 | `azmcp_mysql_server_param_set` | ❌ |
| 12 | 0.218244 | `azmcp_sql_server_delete` | ❌ |
| 13 | 0.214477 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.196054 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 15 | 0.194855 | `azmcp_postgres_server_config_get` | ❌ |
| 16 | 0.194795 | `azmcp_sql_db_delete` | ❌ |
| 17 | 0.183562 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.175244 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.173144 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.166785 | `azmcp_storage_account_get` | ❌ |

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
| 6 | 0.439089 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.377534 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.374642 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.338255 | `azmcp_keyvault_secret_list` | ❌ |
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
| 20 | 0.266351 | `azmcp_sql_db_update` | ❌ |

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
| 6 | 0.457866 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.370520 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.356793 | `azmcp_mysql_server_param_get` | ❌ |
| 9 | 0.317662 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.314774 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.304557 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.294872 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.288088 | `azmcp_mysql_server_param_set` | ❌ |
| 14 | 0.278909 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.269939 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.264553 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.258687 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.249931 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.243655 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.238151 | `azmcp_sql_server_show` | ❌ |

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
| 6 | 0.373656 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.253705 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.251298 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.238644 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.238242 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.211331 | `azmcp_postgres_server_config_get` | ❌ |
| 12 | 0.208002 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.186863 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.185698 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.163738 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.158946 | `azmcp_mysql_server_param_get` | ❌ |
| 17 | 0.154547 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.144421 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.139871 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.123282 | `azmcp_storage_account_create` | ❌ |

---

## Test 30

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555972 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.541688 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.476706 | `azmcp_appconfig_kv_delete` | ❌ |
| 4 | 0.436000 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.425707 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.409529 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.268370 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.259717 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.253166 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.237364 | `azmcp_mysql_server_param_set` | ❌ |
| 11 | 0.229723 | `azmcp_keyvault_secret_list` | ❌ |
| 12 | 0.225415 | `azmcp_postgres_server_config_get` | ❌ |
| 13 | 0.191169 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.190317 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.185172 | `azmcp_postgres_server_param_set` | ❌ |
| 16 | 0.180027 | `azmcp_mysql_server_param_get` | ❌ |
| 17 | 0.171735 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.159847 | `azmcp_postgres_server_param_get` | ❌ |
| 19 | 0.150513 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.143654 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 31

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
| 9 | 0.311494 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.305955 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.263390 | `azmcp_sql_db_update` | ❌ |
| 12 | 0.221138 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.219997 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.213592 | `azmcp_mysql_server_param_get` | ❌ |
| 15 | 0.208947 | `azmcp_postgres_server_config_get` | ❌ |
| 16 | 0.193989 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.167003 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.164376 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.155719 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 20 | 0.147883 | `azmcp_storage_queue_message_send` | ❌ |

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
| 6 | 0.416264 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.301796 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.291448 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.269387 | `azmcp_loadtesting_test_get` | ❌ |
| 10 | 0.260230 | `azmcp_keyvault_secret_list` | ❌ |
| 11 | 0.259549 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.257940 | `azmcp_mysql_server_param_get` | ❌ |
| 13 | 0.251822 | `azmcp_loadtesting_testrun_list` | ❌ |
| 14 | 0.229242 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.226217 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.217887 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.206401 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.205556 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.201872 | `azmcp_mysql_server_param_set` | ❌ |
| 20 | 0.186734 | `azmcp_sql_server_show` | ❌ |

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
| 6 | 0.206466 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.205255 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.177942 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 9 | 0.170352 | `azmcp_deploy_iac_rules_get` | ❌ |
| 10 | 0.169553 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.169415 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.163768 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.148018 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.133096 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.128768 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.125735 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.120066 | `azmcp_mysql_table_schema_get` | ❌ |
| 18 | 0.116209 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.111731 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.109169 | `azmcp_subscription_list` | ❌ |

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
| 6 | 0.200402 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.186927 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.172691 | `azmcp_bestpractices_get` | ❌ |
| 9 | 0.163364 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.157874 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.152905 | `azmcp_deploy_iac_rules_get` | ❌ |
| 12 | 0.150313 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.144054 | `azmcp_mysql_server_param_get` | ❌ |
| 14 | 0.133109 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.125941 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 16 | 0.118881 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.112992 | `azmcp_monitor_workspace_log_query` | ❌ |
| 18 | 0.107063 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.099087 | `azmcp_search_index_get` | ❌ |
| 20 | 0.098418 | `azmcp_mysql_server_list` | ❌ |

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
| 6 | 0.179320 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.178879 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.159064 | `azmcp_bestpractices_get` | ❌ |
| 9 | 0.158370 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.156599 | `azmcp_search_service_list` | ❌ |
| 11 | 0.156168 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.153379 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.151702 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.146689 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.139619 | `azmcp_postgres_server_param_get` | ❌ |
| 16 | 0.137230 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.130326 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.129424 | `azmcp_mysql_server_param_get` | ❌ |
| 19 | 0.126169 | `azmcp_search_index_get` | ❌ |
| 20 | 0.113627 | `azmcp_postgres_server_list` | ❌ |

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
| 6 | 0.507981 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.500514 | `azmcp_subscription_list` | ❌ |
| 8 | 0.499290 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.495957 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.480850 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.477131 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.472811 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.460936 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.460346 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.451887 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.450971 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.447269 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.445430 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.442506 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.438952 | `azmcp_grafana_list` | ❌ |

---

## Test 37

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743903 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.613217 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.519986 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.514120 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.492115 | `azmcp_acr_registry_repository_list` | ❌ |
| 6 | 0.475557 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 7 | 0.466545 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.452905 | `azmcp_acr_registry_list` | ❌ |
| 9 | 0.443767 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.441644 | `azmcp_group_list` | ❌ |
| 11 | 0.433933 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.412747 | `azmcp_search_service_list` | ❌ |
| 13 | 0.412709 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.410027 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.409044 | `azmcp_sql_elastic-pool_list` | ❌ |
| 16 | 0.407704 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.402926 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.398168 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.397222 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.393822 | `azmcp_cosmos_database_list` | ❌ |

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
| 6 | 0.205268 | `azmcp_storage_share_file_list` | ❌ |
| 7 | 0.204654 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.204365 | `azmcp_aks_nodepool_get` | ❌ |
| 9 | 0.203596 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.198992 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.192371 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.188378 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 13 | 0.186379 | `azmcp_storage_blob_get` | ❌ |
| 14 | 0.176424 | `azmcp_marketplace_product_get` | ❌ |
| 15 | 0.175962 | `azmcp_postgres_server_param_get` | ❌ |
| 16 | 0.174849 | `azmcp_aks_nodepool_list` | ❌ |
| 17 | 0.172920 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 18 | 0.169792 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 19 | 0.166628 | `azmcp_applens_resource_diagnose` | ❌ |
| 20 | 0.165332 | `azmcp_aks_cluster_get` | ❌ |

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
| 16 | 0.398806 | `azmcp_subscription_list` | ❌ |
| 17 | 0.395033 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.392643 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.392146 | `azmcp_marketplace_product_list` | ❌ |
| 20 | 0.391371 | `azmcp_eventgrid_subscription_list` | ❌ |

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
| 5 | 0.466242 | `azmcp_deploy_plan_get` | ❌ |
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
| 16 | 0.310275 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.305259 | `azmcp_mysql_database_query` | ❌ |
| 18 | 0.303849 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.302307 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.301536 | `azmcp_storage_blob_container_get` | ❌ |

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
| 5 | 0.439926 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.439532 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.428888 | `azmcp_keyvault_certificate_get` | ❌ |
| 8 | 0.389506 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.381339 | `azmcp_keyvault_certificate_create` | ❌ |
| 10 | 0.379881 | `azmcp_keyvault_certificate_import` | ❌ |
| 11 | 0.304912 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.304137 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.300776 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.292743 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.285517 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.281261 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.279035 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.278638 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.277623 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.274580 | `azmcp_subscription_list` | ❌ |

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
| 5 | 0.490280 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.447777 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.438801 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.354191 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.353355 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.351664 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.345046 | `azmcp_bicepschema_get` | ❌ |
| 12 | 0.322785 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.312391 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.312077 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.292039 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.290398 | `azmcp_search_service_list` | ❌ |
| 17 | 0.282195 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.276297 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.273591 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.273557 | `azmcp_storage_account_get` | ❌ |

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
| 4 | 0.516898 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.516443 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.424443 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.424017 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.409787 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.392171 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369205 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.356238 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.342487 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.306627 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.305711 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.304620 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.304195 | `azmcp_search_service_list` | ❌ |
| 17 | 0.302423 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.302015 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.296142 | `azmcp_sql_db_create` | ❌ |
| 20 | 0.291071 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

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
| 6 | 0.430663 | `azmcp_deploy_plan_get` | ❌ |
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
| 5 | 0.445434 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.400447 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.381822 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.368157 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.367714 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.339658 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.317494 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.292977 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.284617 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.278941 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.275342 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.256382 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.252529 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.246412 | `azmcp_storage_queue_message_send` | ❌ |
| 19 | 0.241745 | `azmcp_search_index_query` | ❌ |
| 20 | 0.239443 | `azmcp_storage_blob_get` | ❌ |

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
| 5 | 0.474566 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.439182 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.412001 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.399571 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.377790 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.373497 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.323164 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.317931 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.303572 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290695 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.277946 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.276228 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.275778 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.270375 | `azmcp_search_service_list` | ❌ |
| 19 | 0.269415 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.269109 | `azmcp_storage_blob_container_create` | ❌ |

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
| 6 | 0.395940 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.394762 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.394254 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.375723 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.363596 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.332626 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.332015 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.307885 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290894 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.289428 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.289326 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.284975 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.284215 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.278669 | `azmcp_storage_queue_message_send` | ❌ |
| 20 | 0.275538 | `azmcp_search_index_query` | ❌ |

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
| 6 | 0.401245 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.398226 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.389556 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.334624 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.315627 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.312250 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.292282 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.283198 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.275578 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.258767 | `azmcp_search_index_query` | ❌ |
| 16 | 0.256800 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.256751 | `azmcp_search_service_list` | ❌ |
| 18 | 0.254638 | `azmcp_storage_blob_get` | ❌ |
| 19 | 0.253397 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.251387 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

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
| 6 | 0.381179 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.374702 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.368899 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.358703 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.337024 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.293848 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.288873 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.282013 | `azmcp_storage_queue_message_send` | ❌ |
| 14 | 0.259723 | `azmcp_mysql_database_query` | ❌ |
| 15 | 0.253005 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.251235 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 17 | 0.246347 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.240292 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.231234 | `azmcp_search_index_query` | ❌ |
| 20 | 0.231120 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 50

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a simple HTTP-triggered function app in javascript that returns a random compliment from a predefined list in a JSON response. And deploy it to azure eventually. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429208 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.408233 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.380754 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.377184 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 5 | 0.352369 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.345059 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.319970 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.311848 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.301028 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.299148 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.235579 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.232320 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.218912 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.215940 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.210908 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.206254 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.203401 | `azmcp_search_index_query` | ❌ |
| 18 | 0.202251 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.197959 | `azmcp_mysql_database_query` | ❌ |
| 20 | 0.188682 | `azmcp_storage_queue_message_send` | ❌ |

---

## Test 51

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499423 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.494910 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.406991 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.397802 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.386276 | `azmcp_bestpractices_get` | ✅ **EXPECTED** |
| 6 | 0.375230 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.355851 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.349640 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.301486 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.284051 | `azmcp_storage_blob_container_create` | ❌ |
| 11 | 0.268361 | `azmcp_foundry_models_deploy` | ❌ |
| 12 | 0.249170 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.244796 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.235496 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.222663 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.219977 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.211042 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.209700 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.209522 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.208427 | `azmcp_storage_table_list` | ❌ |

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
| 4 | 0.432848 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.432409 | `azmcp_bicepschema_get` | ✅ **EXPECTED** |
| 6 | 0.400985 | `azmcp_foundry_models_deploy` | ❌ |
| 7 | 0.398046 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.391625 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.382229 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.372097 | `azmcp_search_service_list` | ❌ |
| 11 | 0.344809 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.325716 | `azmcp_search_index_query` | ❌ |
| 13 | 0.324857 | `azmcp_search_index_get` | ❌ |
| 14 | 0.317232 | `azmcp_sql_db_create` | ❌ |
| 15 | 0.303183 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.291291 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.281487 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.279983 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.274770 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.270531 | `azmcp_storage_blob_container_create` | ❌ |

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
| 6 | 0.216162 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 7 | 0.195530 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 8 | 0.191391 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.191096 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.178269 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.175833 | `azmcp_deploy_iac_rules_get` | ❌ |
| 12 | 0.159251 | `azmcp_storage_share_file_list` | ❌ |
| 13 | 0.154832 | `azmcp_storage_queue_message_send` | ❌ |
| 14 | 0.136707 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.135768 | `azmcp_bestpractices_get` | ❌ |
| 16 | 0.135426 | `azmcp_storage_datalake_directory_create` | ❌ |
| 17 | 0.132826 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.130037 | `azmcp_foundry_models_deploy` | ❌ |
| 19 | 0.127003 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 20 | 0.118383 | `azmcp_quota_usage_check` | ❌ |

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
| 4 | 0.225669 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.215748 | `azmcp_bestpractices_get` | ❌ |
| 6 | 0.207352 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.195387 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.189220 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.179120 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.168850 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 11 | 0.163694 | `azmcp_mysql_database_query` | ❌ |
| 12 | 0.163615 | `azmcp_storage_blob_container_create` | ❌ |
| 13 | 0.162137 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.160743 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.154895 | `azmcp_foundry_models_deploy` | ❌ |
| 16 | 0.154249 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.152324 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.148073 | `azmcp_storage_queue_message_send` | ❌ |
| 19 | 0.145124 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.139758 | `azmcp_storage_account_get` | ❌ |

---

## Test 55

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.298982 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271851 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265711 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242405 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.217586 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.212842 | `azmcp_bestpractices_get` | ❌ |
| 7 | 0.179685 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.169657 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.164767 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.156744 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.155958 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.155053 | `azmcp_storage_queue_message_send` | ❌ |
| 13 | 0.139903 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.138013 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.132118 | `azmcp_mysql_database_query` | ❌ |
| 16 | 0.131213 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.124363 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.119772 | `azmcp_workbooks_create` | ❌ |
| 19 | 0.114825 | `azmcp_mysql_table_schema_get` | ❌ |
| 20 | 0.112146 | `azmcp_sql_db_list` | ❌ |

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
| 5 | 0.310672 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.306967 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.304499 | `azmcp_storage_queue_message_send` | ❌ |
| 8 | 0.304209 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 9 | 0.300392 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.299412 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 11 | 0.298989 | `azmcp_bestpractices_get` | ❌ |
| 12 | 0.293806 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.292455 | `azmcp_applens_resource_diagnose` | ❌ |
| 14 | 0.291879 | `azmcp_deploy_iac_rules_get` | ❌ |
| 15 | 0.282267 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.275832 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.275550 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.272671 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.261446 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.259814 | `azmcp_search_service_list` | ❌ |

---

## Test 57

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818364 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668435 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.615207 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.588634 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.587709 | `azmcp_subscription_list` | ❌ |
| 6 | 0.560793 | `azmcp_search_service_list` | ❌ |
| 7 | 0.538317 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.528985 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.516913 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.502423 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.502135 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.499101 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.497682 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.486951 | `azmcp_group_list` | ❌ |
| 15 | 0.483043 | `azmcp_grafana_list` | ❌ |
| 16 | 0.474950 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.473530 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.459531 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.459009 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.457449 | `azmcp_datadog_monitoredresources_list` | ❌ |

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
| 6 | 0.436290 | `azmcp_subscription_list` | ❌ |
| 7 | 0.431496 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 8 | 0.428477 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.427709 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.408659 | `azmcp_search_service_list` | ❌ |
| 11 | 0.397574 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.390141 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.389842 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.386108 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.383985 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.381323 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.379496 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.373667 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.358593 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.353926 | `azmcp_keyvault_certificate_list` | ❌ |

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
| 4 | 0.546349 | `azmcp_subscription_list` | ❌ |
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
| 20 | 0.430276 | `azmcp_aks_cluster_list` | ❌ |

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
| 6 | 0.429363 | `azmcp_search_service_list` | ❌ |
| 7 | 0.399756 | `azmcp_search_index_query` | ❌ |
| 8 | 0.384335 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.378151 | `azmcp_kusto_query` | ❌ |
| 10 | 0.374844 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.372689 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.366503 | `azmcp_search_index_get` | ❌ |
| 13 | 0.358903 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.351331 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.340987 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.338041 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.334389 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.333252 | `azmcp_marketplace_product_list` | ❌ |
| 19 | 0.331041 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.325792 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 61

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852859 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681061 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.630694 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.581622 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.535218 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.527467 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.486411 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.448991 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.447566 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.439794 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.427628 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.424310 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.422194 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.421587 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.420254 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.411638 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.392961 | `azmcp_postgres_database_list` | ❌ |
| 18 | 0.378276 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.372172 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.368778 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 62

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
| 6 | 0.471019 | `azmcp_storage_table_list` | ❌ |
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
| 6 | 0.555134 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.548066 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.526046 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.501477 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.471453 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.459194 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.450794 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.442540 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.418871 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.407722 | `azmcp_search_service_list` | ❌ |
| 16 | 0.406805 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.406093 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.397642 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.389105 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.387534 | `azmcp_acr_registry_repository_list` | ❌ |

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
| 6 | 0.505363 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.498206 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.497414 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.449759 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.447875 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.437993 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.408605 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.402767 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.396200 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.383928 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.379009 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.348999 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.344602 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.342424 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.339516 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 65

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
| 10 | 0.356643 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.355333 | `azmcp_loadtesting_testresource_list` | ❌ |
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

## Test 66

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
| 9 | 0.343116 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.342468 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.337109 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.320510 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.319895 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.302947 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.300733 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.285505 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.285253 | `azmcp_group_list` | ❌ |
| 18 | 0.285004 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.280372 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.274589 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 67

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686701 | `azmcp_deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471726 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.404890 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.392565 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.389603 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.354432 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.342594 | `azmcp_monitor_resource_log_query` | ❌ |
| 8 | 0.334992 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.334522 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.327028 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.325553 | `azmcp_extension_azqr` | ❌ |
| 12 | 0.320419 | `azmcp_aks_nodepool_get` | ❌ |
| 13 | 0.314964 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.314890 | `azmcp_sql_db_create` | ❌ |
| 15 | 0.312836 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.307291 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.297642 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.288973 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.284916 | `azmcp_search_service_list` | ❌ |
| 20 | 0.284418 | `azmcp_sql_db_list` | ❌ |

---

## Test 68

**Expected Tool:** `azmcp_deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680640 | `azmcp_deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562538 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.505052 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.497193 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.435921 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.430764 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.417333 | `azmcp_bestpractices_get` | ❌ |
| 8 | 0.371127 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.343117 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.322230 | `azmcp_extension_azqr` | ❌ |
| 11 | 0.316752 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.284401 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.270093 | `azmcp_sql_db_create` | ❌ |
| 14 | 0.264933 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.264060 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.263521 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.255084 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.250629 | `azmcp_search_service_list` | ❌ |
| 19 | 0.248047 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.247818 | `azmcp_sql_server_show` | ❌ |

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
| 6 | 0.304816 | `azmcp_deploy_plan_get` | ❌ |
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
| 18 | 0.194571 | `azmcp_sql_db_create` | ❌ |
| 19 | 0.191094 | `azmcp_storage_share_file_list` | ❌ |
| 20 | 0.188615 | `azmcp_role_assignment_list` | ❌ |

---

## Test 70

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `azmcp_deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499297 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.448918 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.382240 | `azmcp_bestpractices_get` | ❌ |
| 5 | 0.375202 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.373363 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.350101 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.338440 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.322906 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.289166 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.262768 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.240757 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.234690 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.230063 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.222416 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.212123 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.211103 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.206262 | `azmcp_storage_queue_message_send` | ❌ |
| 19 | 0.203987 | `azmcp_sql_server_delete` | ❌ |
| 20 | 0.198696 | `azmcp_mysql_server_list` | ❌ |

---

## Test 71

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688109 | `azmcp_deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.432825 | `azmcp_bestpractices_get` | ❌ |
| 6 | 0.425393 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.421744 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.413718 | `azmcp_loadtesting_test_create` | ❌ |
| 9 | 0.393518 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.365875 | `azmcp_foundry_models_deploy` | ❌ |
| 11 | 0.344407 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.312839 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.300643 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.299552 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.296623 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.292780 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.277064 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.258195 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.252696 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.249358 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 72

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682907 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.636180 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.486216 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.480944 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.478217 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.457868 | `azmcp_search_service_list` | ❌ |
| 7 | 0.445125 | `azmcp_subscription_list` | ❌ |
| 8 | 0.435412 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.434659 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.422093 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.420907 | `azmcp_group_list` | ❌ |
| 12 | 0.417538 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.415223 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.408588 | `azmcp_grafana_list` | ❌ |
| 15 | 0.397665 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.392784 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.386186 | `azmcp_marketplace_product_list` | ❌ |
| 18 | 0.383422 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.382782 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.378136 | `azmcp_datadog_monitoredresources_list` | ❌ |

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
| 6 | 0.444774 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.443366 | `azmcp_subscription_list` | ❌ |
| 8 | 0.438131 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435639 | `azmcp_search_service_list` | ❌ |
| 10 | 0.434420 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.433482 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.431618 | `azmcp_grafana_list` | ❌ |
| 13 | 0.426989 | `azmcp_group_list` | ❌ |
| 14 | 0.419194 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.402159 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.398589 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.392822 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.389694 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.386880 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.384264 | `azmcp_marketplace_product_list` | ❌ |

---

## Test 74

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665867 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.663277 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.524919 | `azmcp_group_list` | ❌ |
| 4 | 0.488696 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484167 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 6 | 0.478967 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.474205 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.473708 | `azmcp_servicebus_topic_details` | ❌ |
| 9 | 0.465477 | `azmcp_acr_registry_list` | ❌ |
| 10 | 0.465098 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.462201 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.457114 | `azmcp_search_service_list` | ❌ |
| 13 | 0.452429 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.443225 | `azmcp_subscription_list` | ❌ |
| 15 | 0.436651 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.436075 | `azmcp_grafana_list` | ❌ |
| 17 | 0.434505 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.428724 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.412301 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.407234 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 75

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594210 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.593173 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.525064 | `azmcp_subscription_list` | ❌ |
| 4 | 0.518857 | `azmcp_search_service_list` | ❌ |
| 5 | 0.509007 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.490371 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.489687 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.480579 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.475701 | `azmcp_group_list` | ❌ |
| 10 | 0.475207 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.473274 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.467172 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.460683 | `azmcp_grafana_list` | ❌ |
| 14 | 0.451759 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.440660 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.439125 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.437213 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.422468 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.415047 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.410171 | `azmcp_acr_registry_list` | ❌ |

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
| 4 | 0.518195 | `azmcp_subscription_list` | ❌ |
| 5 | 0.510115 | `azmcp_group_list` | ❌ |
| 6 | 0.508443 | `azmcp_search_service_list` | ❌ |
| 7 | 0.494659 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.492726 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.485794 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.483521 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.481609 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.481450 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.473868 | `azmcp_grafana_list` | ❌ |
| 14 | 0.467622 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 15 | 0.453352 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.446402 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.428290 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.426897 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.424047 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.410745 | `azmcp_acr_registry_list` | ❌ |

---

## Test 77

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618614 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.557573 | `azmcp_group_list` | ❌ |
| 3 | 0.531273 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.504984 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.502308 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.487257 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.472283 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.467550 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.464545 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.459530 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.457119 | `azmcp_grafana_list` | ❌ |
| 12 | 0.440230 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.438344 | `azmcp_subscription_list` | ❌ |
| 14 | 0.438218 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.435258 | `azmcp_search_service_list` | ❌ |
| 16 | 0.431166 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.423121 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.415644 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.411672 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.411012 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 78

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652090 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.581674 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.480537 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.478459 | `azmcp_subscription_list` | ❌ |
| 5 | 0.476763 | `azmcp_search_service_list` | ❌ |
| 6 | 0.475482 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.471596 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.471384 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.466432 | `azmcp_group_list` | ❌ |
| 10 | 0.449893 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.449681 | `azmcp_grafana_list` | ❌ |
| 12 | 0.447080 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.446620 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.444645 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.437300 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.436793 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.436653 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.425231 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.423324 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.422680 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 79

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759182 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.610435 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.545540 | `azmcp_search_service_list` | ❌ |
| 4 | 0.514189 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.496588 | `azmcp_subscription_list` | ❌ |
| 6 | 0.496002 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 7 | 0.492690 | `azmcp_group_list` | ❌ |
| 8 | 0.485584 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.484509 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.475667 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.475056 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.472764 | `azmcp_grafana_list` | ❌ |
| 13 | 0.470300 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.460569 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.442229 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.440503 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.439820 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.438287 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.422414 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.409123 | `azmcp_kusto_database_list` | ❌ |

---

## Test 80

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691101 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.600983 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.478334 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.475119 | `azmcp_search_service_list` | ❌ |
| 5 | 0.450712 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.441607 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.437153 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.431319 | `azmcp_subscription_list` | ❌ |
| 9 | 0.430494 | `azmcp_grafana_list` | ❌ |
| 10 | 0.428437 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.424907 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.420072 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.419125 | `azmcp_group_list` | ❌ |
| 14 | 0.408708 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.399253 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.396758 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.391338 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.381680 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.381664 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.374650 | `azmcp_acr_registry_list` | ❌ |

---

## Test 81

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759409 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.632284 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.526595 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `azmcp_search_service_list` | ❌ |
| 5 | 0.495814 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.494153 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.481357 | `azmcp_group_list` | ❌ |
| 8 | 0.481065 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.476829 | `azmcp_subscription_list` | ❌ |
| 10 | 0.476808 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.471888 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.468200 | `azmcp_grafana_list` | ❌ |
| 13 | 0.466774 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.445991 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.429646 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.428727 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.428427 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.421430 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.417762 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.403614 | `azmcp_marketplace_product_list` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659188 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.614519 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.609175 | `azmcp_group_list` | ❌ |
| 4 | 0.514613 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.505966 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.484746 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.475467 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.464233 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.460456 | `azmcp_search_service_list` | ❌ |
| 10 | 0.456540 | `azmcp_grafana_list` | ❌ |
| 11 | 0.455379 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 12 | 0.452988 | `azmcp_acr_registry_list` | ❌ |
| 13 | 0.448196 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.442914 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.442259 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.432063 | `azmcp_loadtesting_testresource_list` | ❌ |
| 17 | 0.423027 | `azmcp_postgres_server_list` | ❌ |
| 18 | 0.416777 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.411811 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.407927 | `azmcp_cosmos_account_list` | ❌ |

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
| 6 | 0.440399 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.438387 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.434685 | `azmcp_search_service_list` | ❌ |
| 9 | 0.431096 | `azmcp_deploy_iac_rules_get` | ❌ |
| 10 | 0.423302 | `azmcp_subscription_list` | ❌ |
| 11 | 0.422293 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.417076 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 13 | 0.408023 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.406586 | `azmcp_deploy_plan_get` | ❌ |
| 15 | 0.400363 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.398671 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.391646 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.388980 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.381209 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.354341 | `azmcp_redis_cache_list` | ❌ |

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
| 6 | 0.452232 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.448076 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.442021 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.439040 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.426161 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.385771 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.382677 | `azmcp_search_service_list` | ❌ |
| 13 | 0.375818 | `azmcp_subscription_list` | ❌ |
| 14 | 0.375077 | `azmcp_marketplace_product_get` | ❌ |
| 15 | 0.365859 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.365824 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.360612 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.349469 | `azmcp_storage_account_get` | ❌ |
| 19 | 0.341827 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.334327 | `azmcp_mysql_server_list` | ❌ |

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
| 4 | 0.494880 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.487387 | `azmcp_bestpractices_get` | ❌ |
| 6 | 0.481698 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.464304 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.463564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 9 | 0.463172 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.452811 | `azmcp_search_service_list` | ❌ |
| 11 | 0.433938 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.423555 | `azmcp_subscription_list` | ❌ |
| 13 | 0.417356 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.403533 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 15 | 0.398621 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.391476 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.380268 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.376262 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.374279 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.373844 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 86

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695144 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.526435 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.433153 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.422764 | `azmcp_search_index_get` | ❌ |
| 5 | 0.412989 | `azmcp_search_service_list` | ❌ |
| 6 | 0.349551 | `azmcp_search_index_query` | ❌ |
| 7 | 0.329619 | `azmcp_foundry_models_deploy` | ❌ |
| 8 | 0.310546 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.309538 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.296924 | `azmcp_grafana_list` | ❌ |
| 11 | 0.291629 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.286166 | `azmcp_monitor_table_type_list` | ❌ |
| 13 | 0.280057 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.270265 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.270178 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.267932 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.265656 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.264048 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.262277 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.261139 | `azmcp_kusto_database_list` | ❌ |

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
| 6 | 0.341865 | `azmcp_search_index_query` | ❌ |
| 7 | 0.317997 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.310576 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.278147 | `azmcp_foundry_models_deployments_list` | ❌ |
| 10 | 0.276839 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.272237 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.256208 | `azmcp_grafana_list` | ❌ |
| 13 | 0.249946 | `azmcp_bestpractices_get` | ❌ |
| 14 | 0.249587 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.232831 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.225181 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.224194 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.223814 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.218010 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.214893 | `azmcp_monitor_table_type_list` | ❌ |

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
| 6 | 0.358315 | `azmcp_postgres_table_schema_get` | ❌ |
| 7 | 0.349967 | `azmcp_search_index_query` | ❌ |
| 8 | 0.347762 | `azmcp_foundry_models_list` | ❌ |
| 9 | 0.346348 | `azmcp_monitor_table_list` | ❌ |
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
| 6 | 0.380040 | `azmcp_search_index_get` | ❌ |
| 7 | 0.352243 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.318648 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.311669 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.309927 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.286991 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.271893 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.271701 | `azmcp_loadtesting_testrun_list` | ❌ |
| 14 | 0.262678 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.257402 | `azmcp_mysql_table_list` | ❌ |
| 16 | 0.256303 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.249010 | `azmcp_search_index_query` | ❌ |
| 18 | 0.246815 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.242191 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.239938 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 90

**Expected Tool:** `azmcp_foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.313400 | `azmcp_foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.282464 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.274033 | `azmcp_deploy_plan_get` | ❌ |
| 4 | 0.269513 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.268967 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.234071 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.222504 | `azmcp_grafana_list` | ❌ |
| 8 | 0.222478 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.221635 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.217001 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.215293 | `azmcp_loadtesting_testrun_create` | ❌ |
| 12 | 0.209865 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.208124 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.207601 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.204420 | `azmcp_postgres_server_param_set` | ❌ |
| 16 | 0.195615 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.192373 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.190106 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.189382 | `azmcp_postgres_server_param_get` | ❌ |
| 20 | 0.187987 | `azmcp_workbooks_delete` | ❌ |

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
| 6 | 0.368184 | `azmcp_deploy_plan_get` | ❌ |
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
| 6 | 0.328815 | `azmcp_deploy_plan_get` | ❌ |
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
| 6 | 0.298648 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.290447 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 8 | 0.285437 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.277883 | `azmcp_grafana_list` | ❌ |
| 10 | 0.275316 | `azmcp_search_index_get` | ❌ |
| 11 | 0.272954 | `azmcp_monitor_table_list` | ❌ |
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
| 6 | 0.299150 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 7 | 0.283250 | `azmcp_search_index_query` | ❌ |
| 8 | 0.274019 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.266937 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.261834 | `azmcp_search_index_get` | ❌ |
| 11 | 0.260144 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.245943 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.244697 | `azmcp_monitor_table_type_list` | ❌ |
| 14 | 0.243632 | `azmcp_deploy_plan_get` | ❌ |
| 15 | 0.240256 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.234050 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.211456 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.205424 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.200059 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.199386 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 95

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.448179 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.390048 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.380314 | `azmcp_bestpractices_get` | ❌ |
| 5 | 0.379655 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.376542 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.373215 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.347628 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.347347 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.342789 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.341455 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.341448 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.338591 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.333091 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.328326 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.327808 | `azmcp_foundry_models_deployments_list` | ❌ |
| 17 | 0.323953 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.322437 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.314172 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.311100 | `azmcp_workbooks_create` | ❌ |

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
| 6 | 0.397977 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.392852 | `azmcp_appconfig_kv_list` | ❌ |
| 8 | 0.384151 | `azmcp_bestpractices_get` | ❌ |
| 9 | 0.383957 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.369436 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.367183 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.363406 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.361776 | `azmcp_deploy_plan_get` | ❌ |
| 14 | 0.353601 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.352018 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.342398 | `azmcp_postgres_server_config_get` | ❌ |
| 17 | 0.321697 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.315513 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.314100 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.312611 | `azmcp_sql_db_list` | ❌ |

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
| 6 | 0.322197 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.320055 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.317583 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.317359 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.312732 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.311384 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.309942 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.305418 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.297747 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.297041 | `azmcp_aks_nodepool_get` | ❌ |
| 16 | 0.295538 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.295174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.290184 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.281564 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.277653 | `azmcp_mysql_server_config_get` | ❌ |

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
| 6 | 0.416967 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.396163 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.390827 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.389322 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.387898 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.383191 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.378811 | `azmcp_bestpractices_get` | ❌ |
| 13 | 0.376019 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.375267 | `azmcp_workbooks_show` | ❌ |
| 15 | 0.368506 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.366961 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.360165 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.348610 | `azmcp_foundry_models_deployments_list` | ❌ |
| 19 | 0.346255 | `azmcp_group_list` | ❌ |
| 20 | 0.341613 | `azmcp_marketplace_product_get` | ❌ |

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
| 6 | 0.355527 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.353617 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.351217 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.349540 | `azmcp_bestpractices_get` | ❌ |
| 10 | 0.347266 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.344702 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.342868 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.337247 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.333000 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.331796 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.325680 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.320825 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.319597 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.318223 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.305803 | `azmcp_appconfig_kv_show` | ❌ |

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
| 6 | 0.363324 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.358624 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352754 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.351460 | `azmcp_aks_cluster_get` | ❌ |
| 10 | 0.350178 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.349596 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.349013 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.336938 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.335848 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.326910 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.325909 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.323655 | `azmcp_foundry_models_deployments_list` | ❌ |
| 18 | 0.323377 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.320487 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.317752 | `azmcp_sql_server_show` | ❌ |

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
| 6 | 0.368018 | `azmcp_loadtesting_testrun_get` | ❌ |
| 7 | 0.367552 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.355956 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.355282 | `azmcp_search_index_get` | ❌ |
| 10 | 0.349891 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.349476 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.346974 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.344067 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.343381 | `azmcp_bestpractices_get` | ❌ |
| 15 | 0.342303 | `azmcp_servicebus_queue_details` | ❌ |
| 16 | 0.337969 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.337619 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.334256 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.326091 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.323978 | `azmcp_resourcehealth_availability-status_get` | ❌ |

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
| 4 | 0.408010 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.381629 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.364785 | `azmcp_bestpractices_get` | ❌ |
| 7 | 0.350663 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.335606 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.325271 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.321466 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.318517 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.307792 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.304263 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.303123 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.301769 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.291130 | `azmcp_storage_table_list` | ❌ |
| 17 | 0.281401 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.277967 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.276628 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 20 | 0.267170 | `azmcp_search_service_list` | ❌ |

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
| 6 | 0.333621 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.319464 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.318076 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.310636 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.298434 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.297126 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.292793 | `azmcp_cloudarchitect_design` | ❌ |
| 13 | 0.291911 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.283653 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.272348 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.270846 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.267009 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.266527 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.258431 | `azmcp_search_service_list` | ❌ |
| 20 | 0.249136 | `azmcp_storage_table_list` | ❌ |

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
| 5 | 0.485257 | `azmcp_subscription_list` | ❌ |
| 6 | 0.474425 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.465575 | `azmcp_group_list` | ❌ |
| 8 | 0.464534 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.455618 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.455388 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.451429 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.445099 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.442614 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.433540 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.433193 | `azmcp_eventgrid_topic_list` | ❌ |
| 16 | 0.432144 | `azmcp_grafana_list` | ❌ |
| 17 | 0.431611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.415840 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.413034 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.411904 | `azmcp_sql_db_list` | ❌ |

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
| 6 | 0.374655 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.372790 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.370393 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.369686 | `azmcp_subscription_list` | ❌ |
| 10 | 0.368004 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.358769 | `azmcp_deploy_plan_get` | ❌ |
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
| 6 | 0.244782 | `azmcp_appconfig_kv_list` | ❌ |
| 7 | 0.240729 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.239514 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.217775 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.208396 | `azmcp_foundry_models_list` | ❌ |
| 11 | 0.207391 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.197655 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.195857 | `azmcp_role_assignment_list` | ❌ |
| 14 | 0.194503 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.184120 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.184051 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.182124 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.179069 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.178961 | `azmcp_search_service_list` | ❌ |
| 20 | 0.177017 | `azmcp_storage_share_file_list` | ❌ |

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
| 6 | 0.493645 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.492724 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.492305 | `azmcp_subscription_list` | ❌ |
| 9 | 0.491687 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.489846 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.482789 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.479611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.459055 | `azmcp_eventgrid_topic_list` | ❌ |
| 14 | 0.457845 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.447752 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.441315 | `azmcp_group_list` | ❌ |
| 17 | 0.440392 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.436802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.431917 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.430389 | `azmcp_eventgrid_subscription_list` | ❌ |

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
| 6 | 0.542878 | `azmcp_grafana_list` | ❌ |
| 7 | 0.530600 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.524796 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.518520 | `azmcp_acr_registry_list` | ❌ |
| 10 | 0.516850 | `azmcp_loadtesting_testresource_list` | ❌ |
| 11 | 0.509454 | `azmcp_search_service_list` | ❌ |
| 12 | 0.500858 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.491176 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.490734 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.486716 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.480995 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.479588 | `azmcp_subscription_list` | ❌ |
| 18 | 0.477800 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.476840 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.472171 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 109

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
| 6 | 0.428927 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.426935 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.407817 | `azmcp_grafana_list` | ❌ |
| 9 | 0.396822 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.391278 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.383058 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.379927 | `azmcp_acr_registry_repository_list` | ❌ |
| 13 | 0.373796 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.373641 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.366273 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.351405 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.350999 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.345595 | `azmcp_redis_cluster_database_list` | ❌ |
| 19 | 0.328487 | `azmcp_loadtesting_testresource_create` | ❌ |
| 20 | 0.326117 | `azmcp_aks_cluster_list` | ❌ |

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
| 6 | 0.518357 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.515905 | `azmcp_grafana_list` | ❌ |
| 8 | 0.493291 | `azmcp_eventgrid_subscription_list` | ❌ |
| 9 | 0.492945 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.487780 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.475708 | `azmcp_search_service_list` | ❌ |
| 12 | 0.470658 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.464637 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.460412 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.454711 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.454439 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.437296 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.435421 | `azmcp_subscription_list` | ❌ |
| 19 | 0.432994 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.429798 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 111

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.740328 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.595857 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.590604 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.575953 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.543027 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.526693 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.434614 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.414118 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.372055 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.352962 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.330033 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.308676 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.300991 | `azmcp_storage_datalake_directory_create` | ❌ |
| 14 | 0.296656 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.285185 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.267726 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.237085 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.233830 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.223034 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.219468 | `azmcp_subscription_list` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627979 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.624457 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.565005 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.539554 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.493388 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.475412 | `azmcp_keyvault_secret_list` | ❌ |
| 7 | 0.423728 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.418955 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.390699 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.359751 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.346167 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.319164 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.317177 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.293451 | `azmcp_subscription_list` | ❌ |
| 15 | 0.289685 | `azmcp_search_service_list` | ❌ |
| 16 | 0.279695 | `azmcp_search_index_get` | ❌ |
| 17 | 0.276581 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.275920 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 19 | 0.271911 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.269735 | `azmcp_sql_db_show` | ❌ |

---

## Test 113

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662324 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.606534 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.540155 | `azmcp_keyvault_certificate_import` | ❌ |
| 4 | 0.535157 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.499229 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.482353 | `azmcp_keyvault_secret_list` | ❌ |
| 7 | 0.459167 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.419077 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.415722 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.412498 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.411136 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.368360 | `azmcp_search_index_get` | ❌ |
| 13 | 0.365386 | `azmcp_sql_db_show` | ❌ |
| 14 | 0.363228 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.350930 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.332770 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.331645 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.315096 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.305808 | `azmcp_subscription_list` | ❌ |
| 20 | 0.301768 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649993 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.521183 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.469706 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.467097 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.426600 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.398107 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.364713 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.338026 | `azmcp_keyvault_secret_list` | ❌ |
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
| 19 | 0.180219 | `azmcp_sql_db_create` | ❌ |
| 20 | 0.175113 | `azmcp_storage_share_file_list` | ❌ |

---

## Test 115

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649642 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.629850 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.527486 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.525754 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.491866 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.472321 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.399745 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.377944 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.287080 | `azmcp_appconfig_kv_set` | ❌ |
| 10 | 0.271724 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 11 | 0.259849 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.256768 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.250360 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.234343 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.233704 | `azmcp_workbooks_delete` | ❌ |
| 16 | 0.211389 | `azmcp_storage_datalake_directory_create` | ❌ |
| 17 | 0.211319 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.209178 | `azmcp_storage_blob_upload` | ❌ |
| 19 | 0.203635 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.197602 | `azmcp_sql_db_show` | ❌ |

---

## Test 116

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.762015 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.637579 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.608870 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.566460 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.539624 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.484660 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.478100 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.453226 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.431201 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.429531 | `azmcp_storage_table_list` | ❌ |
| 11 | 0.424379 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.408060 | `azmcp_subscription_list` | ❌ |
| 13 | 0.394434 | `azmcp_search_service_list` | ❌ |
| 14 | 0.393940 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.363512 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.362873 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.358938 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.350862 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.339860 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.336779 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 117

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660576 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.570282 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.540044 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.516750 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.509123 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.483404 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.420506 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.397031 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.396055 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.390244 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.382082 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.372424 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.362810 | `azmcp_subscription_list` | ❌ |
| 14 | 0.355698 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.344466 | `azmcp_search_service_list` | ❌ |
| 16 | 0.323177 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.316157 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 18 | 0.309942 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.305651 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.295917 | `azmcp_quota_usage_check` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676352 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.569285 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.555829 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.465658 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.417395 | `azmcp_keyvault_certificate_list` | ❌ |
| 6 | 0.413237 | `azmcp_keyvault_secret_list` | ❌ |
| 7 | 0.412581 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.397141 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.389769 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.372042 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.338097 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.335194 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 13 | 0.287036 | `azmcp_storage_datalake_directory_create` | ❌ |
| 14 | 0.283851 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.276139 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.261794 | `azmcp_workbooks_create` | ❌ |
| 17 | 0.252181 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.231837 | `azmcp_storage_queue_message_send` | ❌ |
| 19 | 0.230284 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.223719 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 119

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737314 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.650237 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.631528 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.498767 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.473916 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.468044 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.467326 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.455805 | `azmcp_keyvault_certificate_get` | ❌ |
| 9 | 0.443785 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.439167 | `azmcp_appconfig_kv_list` | ❌ |
| 11 | 0.430322 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.428311 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.426917 | `azmcp_subscription_list` | ❌ |
| 14 | 0.408341 | `azmcp_search_service_list` | ❌ |
| 15 | 0.388016 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.378819 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 17 | 0.373903 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.368258 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.355043 | `azmcp_monitor_table_list` | ❌ |
| 20 | 0.353714 | `azmcp_redis_cache_list` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609355 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.535390 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.520010 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.479818 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.462249 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.429555 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.421475 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.412607 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.408423 | `azmcp_keyvault_certificate_import` | ❌ |
| 10 | 0.406776 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.405205 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.375139 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.357334 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.353415 | `azmcp_subscription_list` | ❌ |
| 15 | 0.327200 | `azmcp_search_service_list` | ❌ |
| 16 | 0.324788 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 17 | 0.316124 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.308976 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.306567 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.297022 | `azmcp_search_index_get` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.768440 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.613567 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.572728 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.517160 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.461143 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.417557 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.411714 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.391231 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.387386 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 10 | 0.384578 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.370051 | `azmcp_keyvault_certificate_get` | ❌ |
| 12 | 0.357685 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.321954 | `azmcp_storage_datalake_directory_create` | ❌ |
| 14 | 0.288288 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.288115 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.287499 | `azmcp_workbooks_create` | ❌ |
| 17 | 0.285181 | `azmcp_storage_queue_message_send` | ❌ |
| 18 | 0.246265 | `azmcp_storage_blob_container_create` | ❌ |
| 19 | 0.243970 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.236947 | `azmcp_storage_table_list` | ❌ |

---

## Test 122

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747492 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.617297 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.569911 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.519101 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.455500 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.433185 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.417973 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.414310 | `azmcp_keyvault_certificate_get` | ❌ |
| 9 | 0.410496 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.409822 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.392378 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.391075 | `azmcp_subscription_list` | ❌ |
| 13 | 0.388773 | `azmcp_search_service_list` | ❌ |
| 14 | 0.387663 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.367470 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.340472 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.337595 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.334206 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.331203 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.329507 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |

---

## Test 123

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615466 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.520687 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.502404 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.467743 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.456355 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.411604 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.410957 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.409126 | `azmcp_keyvault_certificate_import` | ❌ |
| 9 | 0.401434 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.385852 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.381612 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.371660 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.345267 | `azmcp_subscription_list` | ❌ |
| 14 | 0.344339 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.328354 | `azmcp_search_service_list` | ❌ |
| 16 | 0.315114 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 17 | 0.305225 | `azmcp_search_index_get` | ❌ |
| 18 | 0.303769 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.299023 | `azmcp_storage_account_create` | ❌ |
| 20 | 0.294614 | `azmcp_mysql_server_list` | ❌ |

---

## Test 124

**Expected Tool:** `azmcp_kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482148 | `azmcp_kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.464523 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.457669 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.416762 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.378416 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.362870 | `azmcp_aks_cluster_list` | ❌ |
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
| 18 | 0.318754 | `azmcp_kusto_query` | ❌ |
| 19 | 0.318082 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.314617 | `azmcp_kusto_table_schema` | ❌ |

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
| 4 | 0.535927 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.509396 | `azmcp_grafana_list` | ❌ |
| 6 | 0.505912 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.492107 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.491278 | `azmcp_search_service_list` | ❌ |
| 9 | 0.487583 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.486159 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.460255 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.458754 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.451500 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.428236 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.427811 | `azmcp_subscription_list` | ❌ |
| 16 | 0.413325 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.411791 | `azmcp_group_list` | ❌ |
| 18 | 0.410016 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.407832 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.399179 | `azmcp_monitor_table_list` | ❌ |

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
| 6 | 0.338212 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.314734 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.303083 | `azmcp_grafana_list` | ❌ |
| 9 | 0.292838 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.287768 | `azmcp_kusto_sample` | ❌ |
| 11 | 0.285603 | `azmcp_kusto_query` | ❌ |
| 12 | 0.283331 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.279848 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.277014 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.275559 | `azmcp_mysql_database_query` | ❌ |
| 16 | 0.270779 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.265906 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.264112 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.264035 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.263226 | `azmcp_quota_usage_check` | ❌ |

---

## Test 127

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584053 | `azmcp_redis_cluster_list` | ❌ |
| 2 | 0.549797 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 3 | 0.471076 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.469570 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.464294 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.462945 | `azmcp_grafana_list` | ❌ |
| 7 | 0.446124 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.440326 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.434016 | `azmcp_search_service_list` | ❌ |
| 10 | 0.432048 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.408461 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.396253 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.392541 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.386776 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.380006 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.377490 | `azmcp_kusto_query` | ❌ |
| 17 | 0.371154 | `azmcp_subscription_list` | ❌ |
| 18 | 0.368890 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.365972 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.365323 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 128

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628199 | `azmcp_redis_cluster_database_list` | ❌ |
| 2 | 0.610586 | `azmcp_kusto_database_list` | ✅ **EXPECTED** |
| 3 | 0.552651 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.549575 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.516809 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.474140 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.461468 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.459574 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.433861 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.431796 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.419220 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.403668 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.396014 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.380098 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.375628 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.363598 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.350140 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.334426 | `azmcp_grafana_list` | ❌ |
| 19 | 0.320635 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.319263 | `azmcp_kusto_query` | ❌ |

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
| 6 | 0.440064 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.427251 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.422588 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.391411 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.383664 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.368013 | `azmcp_postgres_table_list` | ❌ |
| 12 | 0.362905 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.359249 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.344010 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.338777 | `azmcp_storage_table_list` | ❌ |
| 16 | 0.336104 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.334803 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.310774 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.309809 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.305756 | `azmcp_kusto_query` | ❌ |

---

## Test 130

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.381098 | `azmcp_kusto_query` | ✅ **EXPECTED** |
| 2 | 0.363896 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.362792 | `azmcp_kusto_sample` | ❌ |
| 4 | 0.348973 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.345284 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.334585 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.329048 | `azmcp_search_service_list` | ❌ |
| 8 | 0.327456 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.325624 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.319101 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.319022 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.315643 | `azmcp_search_index_query` | ❌ |
| 13 | 0.315473 | `azmcp_monitor_table_type_list` | ❌ |
| 14 | 0.307856 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.303747 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.303449 | `azmcp_search_index_get` | ❌ |
| 17 | 0.292270 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.279510 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.262850 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.262700 | `azmcp_grafana_list` | ❌ |

---

## Test 131

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
| 11 | 0.341644 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.337281 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.329933 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.319239 | `azmcp_kusto_query` | ❌ |
| 15 | 0.318189 | `azmcp_postgres_table_list` | ❌ |
| 16 | 0.310196 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.285941 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.267689 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.259027 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.249309 | `azmcp_aks_cluster_list` | ❌ |

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
| 4 | 0.549903 | `azmcp_monitor_table_list` | ❌ |
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
| 19 | 0.329931 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.329669 | `azmcp_grafana_list` | ❌ |

---

## Test 133

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549900 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.524619 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.523449 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.494062 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.490565 | `azmcp_monitor_table_list` | ❌ |
| 6 | 0.475358 | `azmcp_kusto_database_list` | ❌ |
| 7 | 0.466290 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.466220 | `azmcp_kusto_table_schema` | ❌ |
| 9 | 0.431955 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.425675 | `azmcp_kusto_sample` | ❌ |
| 11 | 0.421369 | `azmcp_postgres_database_list` | ❌ |
| 12 | 0.418059 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.415567 | `azmcp_mysql_database_list` | ❌ |
| 14 | 0.403482 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.391003 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.367246 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.348943 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.330413 | `azmcp_kusto_query` | ❌ |
| 19 | 0.314810 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.300104 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 134

**Expected Tool:** `azmcp_kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588251 | `azmcp_kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564356 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.528032 | `azmcp_mysql_table_schema_get` | ❌ |
| 4 | 0.445253 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.437494 | `azmcp_kusto_table_list` | ❌ |
| 6 | 0.432640 | `azmcp_kusto_sample` | ❌ |
| 7 | 0.413767 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.398727 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.387624 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.366387 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.366120 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.358184 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.357652 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.345364 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.343667 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.314675 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.298325 | `azmcp_kusto_query` | ❌ |
| 18 | 0.294896 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.282770 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.275790 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 135

**Expected Tool:** `azmcp_loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585388 | `azmcp_loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.531362 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.508690 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.413808 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.394664 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.390081 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.346617 | `azmcp_loadtesting_testrun_update` | ❌ |
| 8 | 0.338668 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.338173 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.337311 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.323519 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.310466 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.310144 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.296991 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.291016 | `azmcp_storage_queue_message_send` | ❌ |
| 16 | 0.290957 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.288940 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.280434 | `azmcp_sql_server_create` | ❌ |
| 19 | 0.267790 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.255857 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642152 | `azmcp_loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.608813 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.574614 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.533850 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.473198 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.469617 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.436944 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.404490 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.397376 | `azmcp_group_list` | ❌ |
| 10 | 0.379328 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.373143 | `azmcp_loadtesting_testrun_update` | ❌ |
| 12 | 0.369916 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.365515 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.347111 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.341209 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.329210 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.322654 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.305918 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.304224 | `azmcp_monitor_workspace_log_query` | ❌ |
| 20 | 0.298652 | `azmcp_workbooks_delete` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.717584 | `azmcp_loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.596022 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.514477 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.476678 | `azmcp_loadtesting_testrun_create` | ❌ |
| 5 | 0.442851 | `azmcp_workbooks_create` | ❌ |
| 6 | 0.442401 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.417245 | `azmcp_group_list` | ❌ |
| 8 | 0.408728 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.395340 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.383320 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.370066 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.369795 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.357074 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.351265 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.342539 | `azmcp_redis_cluster_list` | ❌ |
| 16 | 0.342088 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.335998 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.326776 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.312584 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.308073 | `azmcp_sql_db_create` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737902 | `azmcp_loadtesting_testresource_list` | ✅ **EXPECTED** |
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
| 12 | 0.475970 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.473444 | `azmcp_search_service_list` | ❌ |
| 14 | 0.473287 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.470899 | `azmcp_acr_registry_list` | ❌ |
| 16 | 0.463466 | `azmcp_loadtesting_testrun_get` | ❌ |
| 17 | 0.452190 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.447138 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.433793 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.426880 | `azmcp_sql_db_list` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621803 | `azmcp_loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.592805 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.540789 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.531104 | `azmcp_loadtesting_testrun_update` | ❌ |
| 5 | 0.488142 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.469444 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.418431 | `azmcp_loadtesting_testrun_list` | ❌ |
| 8 | 0.411611 | `azmcp_loadtesting_testresource_list` | ❌ |
| 9 | 0.402120 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.383719 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.362695 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.331019 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.325487 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.323772 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.314636 | `azmcp_storage_datalake_directory_create` | ❌ |
| 16 | 0.272151 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.267551 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.266839 | `azmcp_storage_queue_message_send` | ❌ |
| 19 | 0.262297 | `azmcp_storage_blob_container_create` | ❌ |
| 20 | 0.253973 | `azmcp_monitor_workspace_log_query` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625332 | `azmcp_loadtesting_test_get` | ❌ |
| 2 | 0.602981 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.568405 | `azmcp_loadtesting_testrun_get` | ✅ **EXPECTED** |
| 4 | 0.561944 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.535183 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.496655 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.434255 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.415545 | `azmcp_loadtesting_testrun_update` | ❌ |
| 9 | 0.397875 | `azmcp_group_list` | ❌ |
| 10 | 0.394683 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.366532 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 12 | 0.356307 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.352984 | `azmcp_workbooks_show` | ❌ |
| 14 | 0.346995 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.329537 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.328853 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.315577 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.314329 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.298953 | `azmcp_monitor_workspace_log_query` | ❌ |
| 20 | 0.291128 | `azmcp_mysql_server_list` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615900 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.606059 | `azmcp_loadtesting_test_get` | ❌ |
| 3 | 0.569175 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.565120 | `azmcp_loadtesting_testrun_list` | ✅ **EXPECTED** |
| 5 | 0.535205 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.492715 | `azmcp_loadtesting_testrun_create` | ❌ |
| 7 | 0.432137 | `azmcp_group_list` | ❌ |
| 8 | 0.416456 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.410933 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.406519 | `azmcp_loadtesting_test_create` | ❌ |
| 11 | 0.395914 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.391152 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.356828 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.342599 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.340623 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.329476 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.328007 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.323926 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.323183 | `azmcp_sql_elastic-pool_list` | ❌ |
| 20 | 0.318622 | `azmcp_mysql_server_list` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660175 | `azmcp_loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.509131 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.454715 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.443890 | `azmcp_loadtesting_test_get` | ❌ |
| 5 | 0.422172 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.399527 | `azmcp_loadtesting_test_create` | ❌ |
| 7 | 0.384887 | `azmcp_loadtesting_testresource_list` | ❌ |
| 8 | 0.384163 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.332616 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.320144 | `azmcp_workbooks_update` | ❌ |
| 11 | 0.300285 | `azmcp_workbooks_create` | ❌ |
| 12 | 0.268394 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.267228 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.255706 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.253509 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.241099 | `azmcp_workbooks_delete` | ❌ |
| 17 | 0.232816 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.228146 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.227990 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 20 | 0.227288 | `azmcp_servicebus_topic_subscription_details` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570139 | `azmcp_marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.477522 | `azmcp_marketplace_product_list` | ❌ |
| 3 | 0.353256 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.330999 | `azmcp_servicebus_queue_details` | ❌ |
| 5 | 0.324083 | `azmcp_search_index_get` | ❌ |
| 6 | 0.323704 | `azmcp_servicebus_topic_details` | ❌ |
| 7 | 0.317373 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.302335 | `azmcp_aks_cluster_get` | ❌ |
| 9 | 0.294798 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.289354 | `azmcp_workbooks_show` | ❌ |
| 11 | 0.285577 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.283554 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.276826 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.274403 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.269243 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.268104 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.266271 | `azmcp_foundry_models_list` | ❌ |
| 18 | 0.259116 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.257141 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.254318 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 144

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527077 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443121 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.343549 | `azmcp_search_service_list` | ❌ |
| 4 | 0.330500 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.328676 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 6 | 0.324866 | `azmcp_search_index_query` | ❌ |
| 7 | 0.290877 | `azmcp_bestpractices_get` | ❌ |
| 8 | 0.290185 | `azmcp_search_index_get` | ❌ |
| 9 | 0.287924 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.263954 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.263529 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.258243 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.254438 | `azmcp_applens_resource_diagnose` | ❌ |
| 14 | 0.251532 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.250343 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.248822 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 17 | 0.245634 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.245271 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.241894 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.232832 | `azmcp_redis_cache_list` | ❌ |

---

## Test 145

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461616 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385169 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.308769 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.260387 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.259270 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.238760 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.238238 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.237988 | `azmcp_grafana_list` | ❌ |
| 9 | 0.226689 | `azmcp_search_service_list` | ❌ |
| 10 | 0.221138 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.211192 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.204870 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.204011 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.203711 | `azmcp_eventgrid_topic_list` | ❌ |
| 15 | 0.202641 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.201780 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.187594 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.185471 | `azmcp_subscription_list` | ❌ |
| 19 | 0.181325 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.176205 | `azmcp_monitor_table_list` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498345 | `azmcp_monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.472094 | `azmcp_monitor_workspace_list` | ❌ |
| 3 | 0.468388 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.467848 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.463168 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.436971 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.418755 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.413357 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.401596 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.385416 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.380121 | `azmcp_grafana_list` | ❌ |
| 12 | 0.358432 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.342740 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.339320 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.333342 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.316587 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.315629 | `azmcp_search_service_list` | ❌ |
| 18 | 0.314731 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.314296 | `azmcp_applens_resource_diagnose` | ❌ |
| 20 | 0.305738 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 147

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
| 7 | 0.305533 | `azmcp_servicebus_queue_details` | ❌ |
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
| 6 | 0.472677 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.459829 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.439032 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.437739 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.431109 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.417098 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 12 | 0.414488 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.411580 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.403921 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.401901 | `azmcp_monitor_metrics_query` | ❌ |
| 16 | 0.397526 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.391340 | `azmcp_monitor_table_type_list` | ❌ |
| 18 | 0.390422 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.383450 | `azmcp_subscription_list` | ❌ |
| 20 | 0.378411 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 149

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633173 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495513 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.382374 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.380460 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.370848 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.359089 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.353264 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.344326 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.341713 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.337874 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.329400 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.324002 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.317475 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.302892 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.301966 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.300496 | `azmcp_search_service_list` | ❌ |
| 17 | 0.299337 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.291565 | `azmcp_cloudarchitect_design` | ❌ |
| 19 | 0.291260 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.282067 | `azmcp_search_index_get` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555377 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.447607 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.447192 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.433777 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.422404 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.416100 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.409107 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.388205 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.380075 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.356549 | `azmcp_functionapp_get` | ❌ |
| 11 | 0.350085 | `azmcp_loadtesting_testrun_list` | ❌ |
| 12 | 0.341791 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.339621 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.335430 | `azmcp_monitor_metrics_definitions` | ❌ |
| 15 | 0.326924 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.326802 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.320852 | `azmcp_search_index_query` | ❌ |
| 18 | 0.307782 | `azmcp_search_service_list` | ❌ |
| 19 | 0.303774 | `azmcp_search_index_get` | ❌ |
| 20 | 0.290515 | `azmcp_workbooks_delete` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557830 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
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
| 12 | 0.326759 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.326643 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.321538 | `azmcp_search_service_list` | ❌ |
| 15 | 0.318196 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.303909 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.303638 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.296819 | `azmcp_search_index_query` | ❌ |
| 19 | 0.292994 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.289116 | `azmcp_mysql_server_param_get` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461192 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390030 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.306441 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.304432 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.301766 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.289372 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.275511 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.267745 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.267460 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 10 | 0.265836 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.263843 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.263558 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.259244 | `azmcp_grafana_list` | ❌ |
| 14 | 0.253584 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.248723 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.247988 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.245716 | `azmcp_workbooks_show` | ❌ |
| 18 | 0.240480 | `azmcp_workbooks_list` | ❌ |
| 19 | 0.231273 | `azmcp_storage_blob_get` | ❌ |
| 20 | 0.229105 | `azmcp_mysql_server_list` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492138 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.417008 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.415966 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.406200 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.398988 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.397335 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.366959 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.362030 | `azmcp_loadtesting_testrun_get` | ❌ |
| 9 | 0.359340 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.331730 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.316201 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.315326 | `azmcp_functionapp_get` | ❌ |
| 13 | 0.311842 | `azmcp_search_index_query` | ❌ |
| 14 | 0.308767 | `azmcp_monitor_metrics_definitions` | ❌ |
| 15 | 0.295918 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.293608 | `azmcp_search_service_list` | ❌ |
| 17 | 0.293311 | `azmcp_loadtesting_testresource_create` | ❌ |
| 18 | 0.287528 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.272646 | `azmcp_search_index_get` | ❌ |
| 20 | 0.271333 | `azmcp_workbooks_show` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525463 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.384273 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.376526 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.367030 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.299315 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.292867 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 7 | 0.290112 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.277516 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 9 | 0.272356 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.266871 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.266320 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.265318 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.262524 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.261784 | `azmcp_grafana_list` | ❌ |
| 15 | 0.261640 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.252381 | `azmcp_servicebus_queue_details` | ❌ |
| 17 | 0.251626 | `azmcp_search_index_query` | ❌ |
| 18 | 0.251002 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.250551 | `azmcp_postgres_server_param_get` | ❌ |
| 20 | 0.246390 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480140 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.381961 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.363412 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.359285 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.350523 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.350491 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.331061 | `azmcp_loadtesting_testresource_list` | ❌ |
| 8 | 0.330074 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.328838 | `azmcp_monitor_metrics_definitions` | ❌ |
| 10 | 0.324932 | `azmcp_search_index_query` | ❌ |
| 11 | 0.319343 | `azmcp_loadtesting_testresource_create` | ❌ |
| 12 | 0.317459 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.292195 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.290762 | `azmcp_search_service_list` | ❌ |
| 15 | 0.282267 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.278491 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.277213 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.276999 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.276023 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.243449 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 156

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594068 | `azmcp_monitor_workspace_log_query` | ❌ |
| 2 | 0.580119 | `azmcp_monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.472064 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.469703 | `azmcp_monitor_metrics_query` | ❌ |
| 5 | 0.443468 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.443147 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.392377 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.390022 | `azmcp_grafana_list` | ❌ |
| 9 | 0.366124 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.359065 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.352821 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.345341 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.344781 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.337855 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.333531 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.330384 | `azmcp_workbooks_delete` | ❌ |
| 17 | 0.320690 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.307859 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.307107 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.306172 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 157

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851209 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.586691 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.534829 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.511123 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.502075 | `azmcp_grafana_list` | ❌ |
| 8 | 0.488557 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.443812 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.420394 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.419859 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.413834 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.409199 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.400092 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.397408 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.396780 | `azmcp_search_service_list` | ❌ |
| 17 | 0.375176 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.374930 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.366099 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.365781 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798563 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.599917 | `azmcp_monitor_workspace_list` | ❌ |
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
| 17 | 0.373359 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 18 | 0.370624 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.347853 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.343837 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 159

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765844 | `azmcp_monitor_table_list` | ❌ |
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
| 14 | 0.395124 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 15 | 0.383606 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.380581 | `azmcp_kusto_sample` | ❌ |
| 17 | 0.372490 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.369889 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.361820 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.354757 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 160

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843138 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736978 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.576731 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.502460 | `azmcp_storage_table_list` | ❌ |
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
| 16 | 0.372991 | `azmcp_postgres_table_list` | ❌ |
| 17 | 0.370860 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.367591 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.348357 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.340101 | `azmcp_foundry_models_list` | ❌ |

---

## Test 161

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `azmcp_grafana_list` | ❌ |
| 3 | 0.660257 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.600802 | `azmcp_search_service_list` | ❌ |
| 5 | 0.583213 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.530433 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.517493 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.513586 | `azmcp_aks_cluster_list` | ❌ |
| 9 | 0.500768 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.494595 | `azmcp_group_list` | ❌ |
| 11 | 0.493776 | `azmcp_subscription_list` | ❌ |
| 12 | 0.487565 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.475212 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.471758 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.470266 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.467655 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.466748 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.448201 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.444222 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.439354 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 162

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.585483 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.531083 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474745 | `azmcp_monitor_workspace_log_query` | ❌ |
| 6 | 0.459841 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.444207 | `azmcp_search_service_list` | ❌ |
| 8 | 0.386422 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.383692 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.383041 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 11 | 0.380891 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.379597 | `azmcp_storage_table_list` | ❌ |
| 13 | 0.373786 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.371395 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.363287 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.358029 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.354811 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.354276 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.353682 | `azmcp_subscription_list` | ❌ |
| 20 | 0.352809 | `azmcp_acr_registry_list` | ❌ |

---

## Test 163

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `azmcp_grafana_list` | ❌ |
| 3 | 0.580350 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.521316 | `azmcp_monitor_table_type_list` | ❌ |
| 5 | 0.521276 | `azmcp_search_service_list` | ❌ |
| 6 | 0.463378 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.453702 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.439297 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435475 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.428945 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.427257 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.422768 | `azmcp_subscription_list` | ❌ |
| 13 | 0.422398 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.413155 | `azmcp_storage_table_list` | ❌ |
| 15 | 0.411648 | `azmcp_acr_registry_list` | ❌ |
| 16 | 0.411448 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.410082 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.404177 | `azmcp_group_list` | ❌ |
| 19 | 0.402600 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.395576 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 164

**Expected Tool:** `azmcp_monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591648 | `azmcp_monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.494715 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.486209 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.484159 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.483323 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.427241 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.374939 | `azmcp_monitor_metrics_query` | ❌ |
| 8 | 0.365704 | `azmcp_grafana_list` | ❌ |
| 9 | 0.330182 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.322875 | `azmcp_workbooks_delete` | ❌ |
| 11 | 0.322408 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 12 | 0.315638 | `azmcp_search_service_list` | ❌ |
| 13 | 0.309411 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.301564 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.299830 | `azmcp_applens_resource_diagnose` | ❌ |
| 16 | 0.298195 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 17 | 0.292089 | `azmcp_loadtesting_testrun_list` | ❌ |
| 18 | 0.291669 | `azmcp_kusto_query` | ❌ |
| 19 | 0.288794 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.287253 | `azmcp_aks_cluster_get` | ❌ |

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
| 18 | 0.270941 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.268856 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.266441 | `azmcp_keyvault_key_list` | ❌ |

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

## Test 167

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
| 6 | 0.413226 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.398405 | `azmcp_sql_server_show` | ❌ |
| 8 | 0.391644 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.376750 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.374826 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.267903 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.252810 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.238583 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.232680 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.224212 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.215445 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.198877 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.180063 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.169466 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.168310 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 169

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678472 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.558177 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.554817 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
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
| 15 | 0.403782 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.379322 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.377511 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.374451 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.365458 | `azmcp_group_list` | ❌ |
| 20 | 0.354490 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 170

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
| 15 | 0.244727 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.235455 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.232383 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.224586 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.218115 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.216149 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 171

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
| 6 | 0.441837 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.431914 | `azmcp_grafana_list` | ❌ |
| 8 | 0.419663 | `azmcp_search_service_list` | ❌ |
| 9 | 0.416021 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.412407 | `azmcp_mysql_database_query` | ❌ |
| 11 | 0.408235 | `azmcp_mysql_table_schema_get` | ❌ |
| 12 | 0.406576 | `azmcp_mysql_server_param_get` | ❌ |
| 13 | 0.399358 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.376596 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.375645 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.364016 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.356691 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.341439 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.341087 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.340946 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `azmcp_mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.407671 | `azmcp_mysql_server_param_set` | ❌ |
| 3 | 0.333841 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.313150 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.310822 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.300031 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.296654 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.292546 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.285663 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.285645 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.183735 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.160082 | `azmcp_appconfig_kv_list` | ❌ |
| 13 | 0.146290 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.137158 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.124274 | `azmcp_grafana_list` | ❌ |
| 16 | 0.120498 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 17 | 0.118505 | `azmcp_loadtesting_testrun_list` | ❌ |
| 18 | 0.117704 | `azmcp_applens_resource_diagnose` | ❌ |
| 19 | 0.117324 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.115886 | `azmcp_cosmos_database_list` | ❌ |

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
| 4 | 0.298911 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.254180 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.253189 | `azmcp_mysql_table_schema_get` | ❌ |
| 7 | 0.246424 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.246019 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.238742 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.236482 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.112499 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.105281 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.094469 | `azmcp_loadtesting_testrun_update` | ❌ |
| 14 | 0.090695 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.090334 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.089483 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.088097 | `azmcp_loadtesting_test_create` | ❌ |
| 18 | 0.086308 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 19 | 0.082253 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.082240 | `azmcp_loadtesting_testrun_create` | ❌ |

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
| 6 | 0.447284 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.442053 | `azmcp_kusto_table_list` | ❌ |
| 8 | 0.431034 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.429975 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.418545 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.410273 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.401217 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.361477 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.335069 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.308385 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.268415 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.260118 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.253046 | `azmcp_grafana_list` | ❌ |
| 19 | 0.241559 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.239226 | `azmcp_appconfig_kv_list` | ❌ |

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
| 6 | 0.439004 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.419905 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.403265 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.391670 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.385166 | `azmcp_postgres_table_schema_get` | ❌ |
| 11 | 0.382065 | `azmcp_monitor_table_list` | ❌ |
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
| 5 | 0.490924 | `azmcp_postgres_server_param_get` | ❌ |
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
| 5 | 0.495664 | `azmcp_postgres_server_param_get` | ❌ |
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

## Test 179

**Expected Tool:** `azmcp_postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546211 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.503267 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.466599 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.415817 | `azmcp_postgres_database_query` | ✅ **EXPECTED** |
| 5 | 0.403965 | `azmcp_postgres_server_param_get` | ❌ |
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

## Test 180

**Expected Tool:** `azmcp_postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `azmcp_postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.599506 | `azmcp_postgres_server_param_get` | ❌ |
| 3 | 0.535229 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.535049 | `azmcp_postgres_database_list` | ❌ |
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
| 16 | 0.189471 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.184937 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.177778 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.174936 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.168322 | `azmcp_kusto_table_schema` | ❌ |

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
| 5 | 0.507696 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.483663 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.472458 | `azmcp_grafana_list` | ❌ |
| 8 | 0.453841 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.446509 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.435298 | `azmcp_search_service_list` | ❌ |
| 11 | 0.416315 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.408673 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.406617 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.398983 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.397428 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.389191 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.375198 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.373699 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.365995 | `azmcp_group_list` | ❌ |
| 20 | 0.362864 | `azmcp_eventgrid_topic_list` | ❌ |

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
| 5 | 0.506254 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.409406 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.400088 | `azmcp_postgres_server_param_set` | ❌ |
| 8 | 0.372955 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.336934 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.336270 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.274763 | `azmcp_grafana_list` | ❌ |
| 12 | 0.260533 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.253264 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.245213 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.241835 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.239500 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.229842 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.227547 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.225295 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.219274 | `azmcp_cosmos_database_container_list` | ❌ |

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
| 5 | 0.505969 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.452608 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.444127 | `azmcp_grafana_list` | ❌ |
| 8 | 0.421577 | `azmcp_search_service_list` | ❌ |
| 9 | 0.414695 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.410719 | `azmcp_postgres_database_query` | ❌ |
| 11 | 0.403538 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.399866 | `azmcp_postgres_table_schema_get` | ❌ |
| 13 | 0.376954 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.369239 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.362650 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.362557 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.360462 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.358409 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.338815 | `azmcp_marketplace_product_list` | ❌ |
| 20 | 0.334679 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 184

**Expected Tool:** `azmcp_postgres_server_param`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594719 | `azmcp_postgres_server_param_get` | ❌ |
| 2 | 0.539666 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.489649 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.480752 | `azmcp_postgres_server_param_set` | ❌ |
| 5 | 0.451869 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.357601 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.343875 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.330858 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.305262 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.295498 | `azmcp_mysql_server_param_set` | ❌ |
| 11 | 0.185325 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.182979 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.174083 | `azmcp_grafana_list` | ❌ |
| 14 | 0.169202 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.166351 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.158146 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.155821 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.145068 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.142327 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.140176 | `azmcp_cosmos_account_list` | ❌ |

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
| 4 | 0.447061 | `azmcp_postgres_server_param_get` | ❌ |
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
| 15 | 0.117122 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.114978 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.113841 | `azmcp_grafana_list` | ❌ |
| 18 | 0.112621 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.108485 | `azmcp_kusto_table_list` | ❌ |
| 20 | 0.102847 | `azmcp_extension_azqr` | ❌ |

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
| 6 | 0.477688 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.449190 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.432813 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.430190 | `azmcp_postgres_server_param_get` | ❌ |
| 10 | 0.396688 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.394313 | `azmcp_monitor_table_list` | ❌ |
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
| 6 | 0.464929 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.457757 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.447202 | `azmcp_postgres_server_param_get` | ❌ |
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

## Test 188

**Expected Tool:** `azmcp_postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714611 | `azmcp_postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597657 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.574267 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.507948 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.480575 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.475678 | `azmcp_kusto_table_schema` | ❌ |
| 7 | 0.443945 | `azmcp_postgres_server_param_get` | ❌ |
| 8 | 0.442620 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.427606 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.406792 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.362728 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.322995 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.303959 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.253660 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.253525 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.239399 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.212579 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.201721 | `azmcp_grafana_list` | ❌ |
| 19 | 0.184834 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.183496 | `azmcp_bicepschema_get` | ❌ |

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
| 6 | 0.349685 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.348742 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.337839 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.331145 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.331074 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.328408 | `azmcp_grafana_list` | ❌ |
| 12 | 0.325796 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.313208 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.310624 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.307143 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.291068 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.290125 | `azmcp_group_list` | ❌ |
| 18 | 0.287104 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.278534 | `azmcp_search_service_list` | ❌ |
| 20 | 0.265329 | `azmcp_monitor_metrics_query` | ❌ |

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
| 6 | 0.365684 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.358215 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.351637 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.345156 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.342266 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.342231 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.340100 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.338636 | `azmcp_grafana_list` | ❌ |
| 14 | 0.331262 | `azmcp_monitor_metrics_definitions` | ❌ |
| 15 | 0.322571 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.321805 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.312566 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.308289 | `azmcp_servicebus_topic_details` | ❌ |
| 19 | 0.305083 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.304570 | `azmcp_loadtesting_testrun_get` | ❌ |

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
| 6 | 0.312428 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.307509 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.303736 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.303531 | `azmcp_appconfig_kv_list` | ❌ |
| 10 | 0.300024 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.298380 | `azmcp_keyvault_certificate_list` | ❌ |
| 12 | 0.296953 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.286490 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.285062 | `azmcp_search_service_list` | ❌ |
| 15 | 0.284898 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.284304 | `azmcp_grafana_list` | ❌ |
| 17 | 0.283818 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.280741 | `azmcp_loadtesting_testrun_list` | ❌ |
| 19 | 0.277728 | `azmcp_subscription_list` | ❌ |
| 20 | 0.274897 | `azmcp_role_assignment_list` | ❌ |

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
| 16 | 0.248135 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.247899 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.246871 | `azmcp_grafana_list` | ❌ |
| 19 | 0.246847 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.240600 | `azmcp_datadog_monitoredresources_list` | ❌ |

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
| 6 | 0.466141 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.464785 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.431968 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.431715 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.423195 | `azmcp_subscription_list` | ❌ |
| 11 | 0.414865 | `azmcp_search_service_list` | ❌ |
| 12 | 0.396295 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.391605 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.381343 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.380470 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.373395 | `azmcp_group_list` | ❌ |
| 17 | 0.373274 | `azmcp_storage_table_list` | ❌ |
| 18 | 0.368719 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.367794 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.367510 | `azmcp_eventgrid_topic_list` | ❌ |

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
| 6 | 0.399303 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.383383 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.382294 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.363507 | `azmcp_eventgrid_subscription_list` | ❌ |
| 10 | 0.361735 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.353560 | `azmcp_subscription_list` | ❌ |
| 12 | 0.353419 | `azmcp_search_service_list` | ❌ |
| 13 | 0.340764 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.327236 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.315615 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.310802 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.308818 | `azmcp_eventgrid_topic_list` | ❌ |
| 18 | 0.306356 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.305932 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.303249 | `azmcp_storage_table_list` | ❌ |

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
| 16 | 0.376230 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.366236 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.328453 | `azmcp_aks_nodepool_list` | ❌ |
| 19 | 0.328081 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.324867 | `azmcp_grafana_list` | ❌ |

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
| 6 | 0.480274 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.434940 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.414701 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.408379 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.397285 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.369086 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.353712 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.351025 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.349880 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.343275 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 16 | 0.325385 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.318982 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.302228 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.294393 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.292180 | `azmcp_grafana_list` | ❌ |

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
| 5 | 0.569183 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.554298 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.527406 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.503279 | `azmcp_grafana_list` | ❌ |
| 9 | 0.467957 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.462558 | `azmcp_search_service_list` | ❌ |
| 11 | 0.457600 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.455613 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.445496 | `azmcp_group_list` | ❌ |
| 14 | 0.445406 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.443534 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.442886 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 17 | 0.436889 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.436540 | `azmcp_subscription_list` | ❌ |
| 19 | 0.419137 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.411121 | `azmcp_mysql_server_list` | ❌ |

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
| 6 | 0.368066 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.337910 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.329389 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.322157 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.321180 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.305874 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.301294 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.295045 | `azmcp_grafana_list` | ❌ |
| 14 | 0.291684 | `azmcp_postgres_database_list` | ❌ |
| 15 | 0.288103 | `azmcp_aks_nodepool_list` | ❌ |
| 16 | 0.272504 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.261138 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.260993 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.259662 | `azmcp_postgres_server_config_get` | ❌ |
| 20 | 0.252053 | `azmcp_resourcehealth_availability-status_list` | ❌ |

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
| 6 | 0.491257 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.456252 | `azmcp_grafana_list` | ❌ |
| 8 | 0.446568 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.440660 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.415482 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.400256 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 12 | 0.398399 | `azmcp_search_service_list` | ❌ |
| 13 | 0.394530 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.394483 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.389814 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.372221 | `azmcp_group_list` | ❌ |
| 17 | 0.370370 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.369831 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.368926 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.367955 | `azmcp_resourcehealth_availability-status_list` | ❌ |

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
| 6 | 0.330187 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.327691 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.325794 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.324331 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.322117 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.311644 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.308238 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.306616 | `azmcp_grafana_list` | ❌ |
| 14 | 0.292005 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.290698 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.287239 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 17 | 0.286287 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.285945 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.284990 | `azmcp_applens_resource_diagnose` | ❌ |
| 20 | 0.281679 | `azmcp_storage_account_get` | ❌ |

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
| 6 | 0.455902 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.412608 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.411283 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.405847 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.403899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.375351 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.368594 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 13 | 0.368262 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.349407 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.347885 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.347171 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.337143 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 18 | 0.336357 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.321704 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.318311 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 203

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 2 | 0.570884 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424939 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.393479 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.386598 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.373883 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.355414 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.352447 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.342229 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.337314 | `azmcp_aks_nodepool_get` | ❌ |
| 11 | 0.327197 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.321304 | `azmcp_group_list` | ❌ |
| 13 | 0.318379 | `azmcp_sql_db_list` | ❌ |
| 14 | 0.318319 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.316508 | `azmcp_sql_server_show` | ❌ |
| 16 | 0.307248 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.307076 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.299346 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.298723 | `azmcp_monitor_metrics_definitions` | ❌ |
| 20 | 0.294197 | `azmcp_aks_cluster_get` | ❌ |

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
| 6 | 0.540583 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.534299 | `azmcp_search_service_list` | ❌ |
| 8 | 0.531356 | `azmcp_quota_region_availability_list` | ❌ |
| 9 | 0.530985 | `azmcp_group_list` | ❌ |
| 10 | 0.507740 | `azmcp_monitor_workspace_list` | ❌ |
| 11 | 0.496651 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.491428 | `azmcp_subscription_list` | ❌ |
| 13 | 0.491394 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.488953 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.484176 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.482623 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.476832 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.465529 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.462516 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.459718 | `azmcp_workbooks_list` | ❌ |

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
| 6 | 0.441470 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.441430 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.430496 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 9 | 0.418944 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.409363 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.407124 | `azmcp_storage_blob_container_get` | ❌ |
| 12 | 0.406709 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.406408 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.405790 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.403600 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.400553 | `azmcp_monitor_metrics_query` | ❌ |
| 17 | 0.387835 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.381144 | `azmcp_bestpractices_get` | ❌ |
| 19 | 0.380761 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 20 | 0.379969 | `azmcp_azureterraformbestpractices_get` | ❌ |

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
| 5 | 0.420387 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.411111 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.411059 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.374184 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.370772 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.363808 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.360039 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.358871 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.350454 | `azmcp_group_list` | ❌ |
| 14 | 0.348923 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.336467 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.334774 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.330185 | `azmcp_extension_azqr` | ❌ |
| 18 | 0.320138 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.319481 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.317434 | `azmcp_quota_region_availability_list` | ❌ |

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
| 5 | 0.503791 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.470139 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.456526 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.454448 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.446515 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.438808 | `azmcp_subscription_list` | ❌ |
| 11 | 0.427135 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.426698 | `azmcp_grafana_list` | ❌ |
| 13 | 0.419828 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.419011 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.416883 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.411902 | `azmcp_group_list` | ❌ |
| 17 | 0.407099 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.385382 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.378841 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.360279 | `azmcp_marketplace_product_list` | ❌ |

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
| 6 | 0.474841 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.459839 | `azmcp_subscription_list` | ❌ |
| 8 | 0.431445 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.425644 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.411892 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.410579 | `azmcp_marketplace_product_list` | ❌ |
| 12 | 0.409002 | `azmcp_aks_cluster_list` | ❌ |
| 13 | 0.404636 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.395147 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.390652 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.390483 | `azmcp_group_list` | ❌ |
| 17 | 0.390381 | `azmcp_applens_resource_diagnose` | ❌ |
| 18 | 0.390329 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.386035 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.385710 | `azmcp_datadog_monitoredresources_list` | ❌ |

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
| 6 | 0.191890 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.189628 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.188665 | `azmcp_bestpractices_get` | ❌ |
| 9 | 0.187819 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.185941 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.174872 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.170157 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.170031 | `azmcp_servicebus_queue_details` | ❌ |
| 14 | 0.168759 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.164622 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.163022 | `azmcp_monitor_workspace_log_query` | ❌ |
| 17 | 0.155791 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.155509 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.150546 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 20 | 0.149112 | `azmcp_aks_cluster_get` | ❌ |

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
| 5 | 0.487384 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.453380 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 7 | 0.451351 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.439658 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.436070 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.411793 | `azmcp_grafana_list` | ❌ |
| 11 | 0.408792 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.407719 | `azmcp_subscription_list` | ❌ |
| 13 | 0.406949 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.404998 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.391992 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.379016 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.371279 | `azmcp_group_list` | ❌ |
| 18 | 0.368866 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.367379 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.357139 | `azmcp_appconfig_account_list` | ❌ |

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
| 6 | 0.382940 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.382581 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.375034 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.374714 | `azmcp_eventgrid_subscription_list` | ❌ |
| 10 | 0.371691 | `azmcp_monitor_metrics_query` | ❌ |
| 11 | 0.363470 | `azmcp_bestpractices_get` | ❌ |
| 12 | 0.362214 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.360562 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.357531 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.341495 | `azmcp_foundry_models_deployments_list` | ❌ |
| 16 | 0.338062 | `azmcp_search_index_get` | ❌ |
| 17 | 0.333382 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.333219 | `azmcp_subscription_list` | ❌ |
| 19 | 0.332392 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.324544 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 212

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.483988 | `azmcp_group_list` | ❌ |
| 3 | 0.483185 | `azmcp_subscription_list` | ❌ |
| 4 | 0.478700 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474796 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.471364 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.468596 | `azmcp_search_service_list` | ❌ |
| 8 | 0.460029 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.452819 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.446372 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 11 | 0.430667 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.427950 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.426624 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.425029 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.423371 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.403310 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.398447 | `azmcp_eventgrid_topic_list` | ❌ |
| 18 | 0.397565 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.396883 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.374727 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 213

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609705 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.456956 | `azmcp_grafana_list` | ❌ |
| 3 | 0.436827 | `azmcp_subscription_list` | ❌ |
| 4 | 0.435642 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.435155 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.431865 | `azmcp_search_service_list` | ❌ |
| 7 | 0.428663 | `azmcp_group_list` | ❌ |
| 8 | 0.428370 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.421627 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.420804 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.418755 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.410380 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 13 | 0.406766 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.395445 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.388811 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.386800 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.383635 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.373204 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.368530 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.363675 | `azmcp_eventgrid_topic_list` | ❌ |

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
| 6 | 0.393556 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.388183 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.372367 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.370915 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.358315 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.356755 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.356252 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.354845 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.352762 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.351083 | `azmcp_sql_server_show` | ❌ |
| 16 | 0.342943 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.336903 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.333641 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.330038 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.329368 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

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
| 6 | 0.439446 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.416404 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.409307 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.406485 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.397423 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.392400 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.382987 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.378750 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.378297 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.375372 | `azmcp_foundry_models_deployments_list` | ❌ |
| 16 | 0.371099 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.369526 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.368931 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367804 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.362728 | `azmcp_keyvault_secret_list` | ❌ |

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
| 6 | 0.401792 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.382692 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.372639 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.370330 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.367868 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.353839 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.351788 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.351161 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.350043 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.347566 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.346994 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.341728 | `azmcp_foundry_models_list` | ❌ |
| 18 | 0.335748 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.332289 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.331281 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 217

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
| 12 | 0.290809 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.288242 | `azmcp_foundry_models_deployments_list` | ❌ |
| 14 | 0.287501 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.283532 | `azmcp_foundry_models_list` | ❌ |
| 16 | 0.269980 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.260161 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.254226 | `azmcp_storage_table_list` | ❌ |
| 19 | 0.248402 | `azmcp_monitor_table_type_list` | ❌ |
| 20 | 0.247656 | `azmcp_marketplace_product_get` | ❌ |

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
| 6 | 0.492228 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.486066 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.482464 | `azmcp_grafana_list` | ❌ |
| 9 | 0.477512 | `azmcp_subscription_list` | ❌ |
| 10 | 0.470384 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.470055 | `azmcp_marketplace_product_list` | ❌ |
| 12 | 0.454460 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.451744 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.443495 | `azmcp_search_index_query` | ❌ |
| 15 | 0.434052 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.427817 | `azmcp_group_list` | ❌ |
| 17 | 0.425463 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.418396 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.417955 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.417472 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 219

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
| 6 | 0.419484 | `azmcp_marketplace_product_get` | ❌ |
| 7 | 0.412158 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.408456 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.400229 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.399822 | `azmcp_grafana_list` | ❌ |
| 11 | 0.397883 | `azmcp_foundry_models_deployments_list` | ❌ |
| 12 | 0.393772 | `azmcp_subscription_list` | ❌ |
| 13 | 0.392910 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.390660 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.390559 | `azmcp_foundry_models_list` | ❌ |
| 16 | 0.384559 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.379805 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 18 | 0.376962 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.376089 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.373463 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

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
| 6 | 0.322580 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 7 | 0.322540 | `azmcp_foundry_models_list` | ❌ |
| 8 | 0.300427 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.292677 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.290214 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.289466 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.283366 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.282198 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 14 | 0.281672 | `azmcp_bestpractices_get` | ❌ |
| 15 | 0.281134 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.278574 | `azmcp_cloudarchitect_design` | ❌ |
| 17 | 0.278531 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.277693 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.275013 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.274081 | `azmcp_monitor_table_type_list` | ❌ |

---

## Test 221

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642951 | `azmcp_servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.400870 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.382417 | `azmcp_storage_queue_message_send` | ❌ |
| 5 | 0.375386 | `azmcp_aks_cluster_get` | ❌ |
| 6 | 0.360754 | `azmcp_storage_blob_container_get` | ❌ |
| 7 | 0.352789 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.352705 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.351081 | `azmcp_search_index_get` | ❌ |
| 10 | 0.346285 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.342349 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.337239 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.332541 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.327389 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.323279 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.323046 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.310612 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.309214 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.296392 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.290424 | `azmcp_eventgrid_topic_list` | ❌ |

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
| 4 | 0.494879 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.484014 | `azmcp_servicebus_queue_details` | ❌ |
| 6 | 0.365658 | `azmcp_search_index_get` | ❌ |
| 7 | 0.361354 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.352494 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.341289 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.340036 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.337675 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.335558 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.333396 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.330814 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.326077 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.324869 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.317424 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.306388 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.297323 | `azmcp_grafana_list` | ❌ |
| 20 | 0.290383 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 223

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `azmcp_servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.527109 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.494551 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.457036 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.444606 | `azmcp_marketplace_product_get` | ❌ |
| 6 | 0.443962 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.429458 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.426573 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.421009 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.411293 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.409496 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.405380 | `azmcp_search_service_list` | ❌ |
| 13 | 0.404739 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.395789 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.395176 | `azmcp_grafana_list` | ❌ |
| 16 | 0.388049 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.382225 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.369986 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.368411 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.368155 | `azmcp_kusto_cluster_list` | ❌ |

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
| 6 | 0.355614 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.347128 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.346831 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.329705 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 10 | 0.327837 | `azmcp_sql_db_delete` | ❌ |
| 11 | 0.311794 | `azmcp_keyvault_secret_create` | ❌ |
| 12 | 0.301243 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.297803 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.279490 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.276192 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.272135 | `azmcp_keyvault_certificate_create` | ❌ |
| 17 | 0.254831 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.238999 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.231273 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.209994 | `azmcp_loadtesting_testresource_create` | ❌ |

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
| 6 | 0.384660 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.371559 | `azmcp_sql_server_show` | ❌ |
| 8 | 0.361051 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.343099 | `azmcp_sql_db_delete` | ❌ |
| 10 | 0.326611 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.324123 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.321837 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.317180 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.311150 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.304628 | `azmcp_keyvault_secret_create` | ❌ |
| 16 | 0.301487 | `azmcp_kusto_table_schema` | ❌ |
| 17 | 0.290173 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.286796 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.276688 | `azmcp_loadtesting_test_create` | ❌ |
| 20 | 0.257394 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 226

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545906 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.494377 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.473975 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.456262 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.455293 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.422871 | `azmcp_workbooks_create` | ❌ |
| 8 | 0.413309 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.398726 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.392814 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.390310 | `azmcp_keyvault_secret_create` | ❌ |
| 12 | 0.379238 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.378636 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.374962 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.369375 | `azmcp_sql_db_delete` | ❌ |
| 16 | 0.365919 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.358566 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.323537 | `azmcp_group_list` | ❌ |
| 19 | 0.319381 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.310776 | `azmcp_acr_registry_repository_list` | ❌ |

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
| 6 | 0.350121 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.345061 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.338052 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.337699 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.317215 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.286892 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.284794 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.278895 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.271009 | `azmcp_appconfig_kv_delete` | ❌ |
| 15 | 0.253808 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.243201 | `azmcp_kusto_table_schema` | ❌ |
| 17 | 0.235173 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.211680 | `azmcp_kusto_query` | ❌ |
| 19 | 0.183587 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.143366 | `azmcp_monitor_resource_log_query` | ❌ |

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
| 6 | 0.412704 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.392236 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.390334 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.381382 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.380074 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.370486 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.368841 | `azmcp_sql_elastic-pool_list` | ❌ |
| 13 | 0.367342 | `azmcp_redis_cluster_database_list` | ❌ |
| 14 | 0.345612 | `azmcp_group_list` | ❌ |
| 15 | 0.334517 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.332517 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.327408 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.310437 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.304849 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 20 | 0.301838 | `azmcp_functionapp_get` | ❌ |

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
| 6 | 0.314902 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.311506 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.310758 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.305059 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 10 | 0.295355 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.294781 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.288554 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.283955 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.258654 | `azmcp_appconfig_kv_delete` | ❌ |
| 15 | 0.246948 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.229764 | `azmcp_kusto_table_schema` | ❌ |
| 17 | 0.213266 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.187992 | `azmcp_kusto_query` | ❌ |
| 19 | 0.171883 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.151809 | `azmcp_cosmos_account_list` | ❌ |

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
| 17 | 0.395322 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.394623 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.380402 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.367404 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 231

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
| 8 | 0.458742 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.451453 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.445291 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.443384 | `azmcp_sql_elastic-pool_list` | ❌ |
| 12 | 0.387645 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.380428 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.357215 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.354581 | `azmcp_aks_nodepool_list` | ❌ |
| 16 | 0.349880 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.347075 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.342792 | `azmcp_appconfig_kv_list` | ❌ |
| 19 | 0.342284 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.341681 | `azmcp_kusto_table_list` | ❌ |

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
| 6 | 0.446633 | `azmcp_postgres_server_param_get` | ❌ |
| 7 | 0.438925 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.398181 | `azmcp_mysql_table_schema_get` | ❌ |
| 9 | 0.397510 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.396416 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.371413 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.325945 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.325690 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.320054 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.297839 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.294987 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.281546 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.279952 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 19 | 0.273566 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.273315 | `azmcp_cosmos_database_list` | ❌ |

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
| 6 | 0.421862 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.400963 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.398714 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.375668 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.361500 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.344694 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.337996 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.323587 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.300133 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.299814 | `azmcp_aks_cluster_get` | ❌ |
| 16 | 0.296827 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.291629 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.285843 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.268405 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.265483 | `azmcp_aks_nodepool_get` | ❌ |

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
| 6 | 0.371461 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.369822 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.368957 | `azmcp_mysql_server_param_set` | ❌ |
| 9 | 0.344860 | `azmcp_sql_db_delete` | ❌ |
| 10 | 0.334237 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.306013 | `azmcp_loadtesting_testrun_update` | ❌ |
| 12 | 0.273809 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.270134 | `azmcp_kusto_table_schema` | ❌ |
| 14 | 0.263397 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.250975 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.250753 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.240663 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.230967 | `azmcp_loadtesting_testrun_create` | ❌ |
| 19 | 0.223147 | `azmcp_loadtesting_test_create` | ❌ |
| 20 | 0.223086 | `azmcp_kusto_query` | ❌ |

---

## Test 235

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.401820 | `azmcp_sql_db_list` | ❌ |
| 2 | 0.394743 | `azmcp_sql_db_show` | ❌ |
| 3 | 0.389883 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 4 | 0.386595 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.381815 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.349174 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 7 | 0.342258 | `azmcp_sql_elastic-pool_list` | ❌ |
| 8 | 0.339034 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.333287 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.329714 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.325597 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.304346 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.296959 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 14 | 0.261126 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.257309 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.238510 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.232089 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.223103 | `azmcp_loadtesting_testrun_update` | ❌ |
| 19 | 0.221271 | `azmcp_kusto_query` | ❌ |
| 20 | 0.219568 | `azmcp_foundry_knowledge_index_schema` | ❌ |

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
| 6 | 0.454426 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.450777 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.441264 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 9 | 0.434570 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.431174 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.429007 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 12 | 0.416258 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.394537 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.394337 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.370652 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.363579 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.357347 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.352026 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.351647 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.349729 | `azmcp_keyvault_key_list` | ❌ |

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
| 6 | 0.429728 | `azmcp_aks_nodepool_get` | ❌ |
| 7 | 0.423047 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.419753 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.400026 | `azmcp_mysql_server_param_get` | ❌ |
| 10 | 0.383940 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.378556 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.373386 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.335615 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.333099 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.319856 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.317886 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.304600 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.304317 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.298907 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.298264 | `azmcp_acr_registry_list` | ❌ |

---

## Test 238

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
| 8 | 0.365113 | `azmcp_aks_nodepool_get` | ❌ |
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
| 20 | 0.259391 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682899 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.564199 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.537328 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.481795 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.473804 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.464978 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.451480 | `azmcp_loadtesting_testresource_create` | ❌ |
| 8 | 0.450184 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.449932 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.419453 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.416299 | `azmcp_workbooks_create` | ❌ |
| 12 | 0.402802 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.401683 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.397643 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.335379 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.332447 | `azmcp_extension_azqr` | ❌ |
| 17 | 0.326775 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.323920 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.319975 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.318138 | `azmcp_loadtesting_test_create` | ❌ |

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
| 6 | 0.369864 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.368115 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.367996 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.360875 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.354438 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.352234 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.349584 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 13 | 0.349312 | `azmcp_sql_db_list` | ❌ |
| 14 | 0.324021 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 15 | 0.316772 | `azmcp_loadtesting_test_create` | ❌ |
| 16 | 0.315987 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.301226 | `azmcp_deploy_plan_get` | ❌ |
| 18 | 0.300788 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.297351 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.278419 | `azmcp_azureterraformbestpractices_get` | ❌ |

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
| 4 | 0.442934 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.423887 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.421502 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.417608 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.416146 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.389609 | `azmcp_sql_elastic-pool_list` | ❌ |
| 10 | 0.385267 | `azmcp_loadtesting_testresource_create` | ❌ |
| 11 | 0.369631 | `azmcp_workbooks_create` | ❌ |
| 12 | 0.333007 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.317210 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.312657 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.303177 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.300992 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.298321 | `azmcp_group_list` | ❌ |
| 18 | 0.288584 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.284845 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.277880 | `azmcp_acr_registry_list` | ❌ |

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
| 6 | 0.449007 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.448514 | `azmcp_sql_server_show` | ❌ |
| 8 | 0.438950 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.417035 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 10 | 0.402684 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.346442 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.333269 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.323460 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.317588 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.317257 | `azmcp_group_list` | ❌ |
| 16 | 0.307426 | `azmcp_appconfig_kv_delete` | ❌ |
| 17 | 0.304909 | `azmcp_monitor_metrics_query` | ❌ |
| 18 | 0.292074 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.290106 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.273131 | `azmcp_loadtesting_testresource_create` | ❌ |

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
| 6 | 0.301933 | `azmcp_sql_db_delete` | ❌ |
| 7 | 0.299760 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.295963 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.295073 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 10 | 0.276930 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 11 | 0.258333 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.235107 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.234779 | `azmcp_appconfig_kv_delete` | ❌ |
| 14 | 0.234376 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.232903 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.226608 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.225579 | `azmcp_grafana_list` | ❌ |
| 18 | 0.219702 | `azmcp_kusto_table_list` | ❌ |
| 19 | 0.210483 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.207493 | `azmcp_marketplace_product_get` | ❌ |

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
| 6 | 0.274818 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.262381 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 8 | 0.261689 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 9 | 0.254391 | `azmcp_appconfig_kv_delete` | ❌ |
| 10 | 0.247364 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.237815 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.168042 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.164197 | `azmcp_loadtesting_testrun_update` | ❌ |
| 14 | 0.159907 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.156253 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.148272 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.146243 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.142127 | `azmcp_kusto_query` | ❌ |
| 19 | 0.140346 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.136157 | `azmcp_monitor_resource_log_query` | ❌ |

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
| 6 | 0.352607 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.344454 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.343559 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.329397 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.328737 | `azmcp_role_assignment_list` | ❌ |
| 11 | 0.280450 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.258095 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.250088 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.249297 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.249153 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.246677 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.245267 | `azmcp_group_list` | ❌ |
| 18 | 0.238204 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.234681 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.233337 | `azmcp_cosmos_database_container_list` | ❌ |

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
| 19 | 0.180468 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.178625 | `azmcp_loadtesting_testrun_list` | ❌ |

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
| 6 | 0.236050 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.230580 | `azmcp_sql_db_create` | ❌ |
| 8 | 0.228196 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.223111 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.222002 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.212644 | `azmcp_cloudarchitect_design` | ❌ |
| 12 | 0.200387 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.197760 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.189941 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.188300 | `azmcp_deploy_plan_get` | ❌ |
| 16 | 0.180995 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.180555 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 18 | 0.174553 | `azmcp_deploy_iac_rules_get` | ❌ |
| 19 | 0.169345 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.167872 | `azmcp_bestpractices_get` | ❌ |

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
| 6 | 0.423223 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.403858 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.335670 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.326425 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.318368 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.313690 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.311193 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.295941 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.290296 | `azmcp_deploy_iac_rules_get` | ❌ |
| 15 | 0.288030 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.265059 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 17 | 0.260209 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.253830 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.246996 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.242716 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670189 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533562 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.295018 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.287457 | `azmcp_sql_server_create` | ❌ |
| 6 | 0.271240 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.252970 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 8 | 0.248826 | `azmcp_postgres_server_param_set` | ❌ |
| 9 | 0.237646 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.230685 | `azmcp_mysql_server_param_set` | ❌ |
| 11 | 0.222068 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 12 | 0.179219 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.174851 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.174584 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.166723 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.161852 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.158176 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.156396 | `azmcp_keyvault_key_create` | ❌ |
| 19 | 0.149884 | `azmcp_kusto_query` | ❌ |
| 20 | 0.143603 | `azmcp_appconfig_kv_set` | ❌ |

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
| 6 | 0.356402 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.339841 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.321980 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.302214 | `azmcp_keyvault_certificate_create` | ❌ |
| 10 | 0.283923 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.281043 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.270399 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 13 | 0.259505 | `azmcp_mysql_server_param_set` | ❌ |
| 14 | 0.248939 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.221008 | `azmcp_deploy_iac_rules_get` | ❌ |
| 16 | 0.219182 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.209376 | `azmcp_loadtesting_testrun_create` | ❌ |
| 18 | 0.207284 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.197104 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.196512 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

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
| 6 | 0.418504 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.410574 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.341915 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.341838 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.341474 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.312054 | `azmcp_appconfig_kv_delete` | ❌ |
| 12 | 0.263959 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.248381 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.245270 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 15 | 0.241564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 16 | 0.235230 | `azmcp_keyvault_certificate_create` | ❌ |
| 17 | 0.231494 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.225236 | `azmcp_keyvault_certificate_get` | ❌ |
| 19 | 0.225227 | `azmcp_kusto_query` | ❌ |
| 20 | 0.222317 | `azmcp_monitor_metrics_query` | ❌ |

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
| 6 | 0.298395 | `azmcp_sql_db_delete` | ❌ |
| 7 | 0.259110 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.254974 | `azmcp_appconfig_kv_delete` | ❌ |
| 9 | 0.251005 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 10 | 0.231881 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.231126 | `azmcp_postgres_server_param_get` | ❌ |
| 12 | 0.182013 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.166323 | `azmcp_loadtesting_testrun_update` | ❌ |
| 14 | 0.158025 | `azmcp_kusto_query` | ❌ |
| 15 | 0.156028 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.152458 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.152084 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 18 | 0.150665 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.149628 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.145688 | `azmcp_loadtesting_test_get` | ❌ |

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
| 6 | 0.336482 | `azmcp_sql_db_delete` | ❌ |
| 7 | 0.293303 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.291409 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 9 | 0.286333 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.284391 | `azmcp_workbooks_delete` | ❌ |
| 11 | 0.252095 | `azmcp_appconfig_kv_delete` | ❌ |
| 12 | 0.222155 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.204033 | `azmcp_loadtesting_testrun_update` | ❌ |
| 14 | 0.193574 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.185585 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.185007 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.183545 | `azmcp_deploy_iac_rules_get` | ❌ |
| 18 | 0.181757 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 19 | 0.180404 | `azmcp_kusto_query` | ❌ |
| 20 | 0.179948 | `azmcp_keyvault_secret_list` | ❌ |

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
| 6 | 0.385148 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.359228 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.356700 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.355203 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.350241 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.305039 | `azmcp_keyvault_secret_list` | ❌ |
| 12 | 0.278098 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.277611 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.276828 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.270667 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.263181 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.256310 | `azmcp_aks_nodepool_list` | ❌ |
| 18 | 0.253852 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.248780 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 20 | 0.243907 | `azmcp_eventgrid_subscription_list` | ❌ |

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
| 6 | 0.312035 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.298995 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.294466 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.293371 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.292608 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.225372 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.212971 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.210887 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.210531 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.206823 | `azmcp_keyvault_secret_list` | ❌ |
| 16 | 0.206476 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.206114 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.197711 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.195864 | `azmcp_aks_nodepool_list` | ❌ |
| 20 | 0.191134 | `azmcp_aks_nodepool_get` | ❌ |

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
| 6 | 0.305701 | `azmcp_mysql_server_param_get` | ❌ |
| 7 | 0.304314 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.282538 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.277628 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.273282 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.202425 | `azmcp_deploy_iac_rules_get` | ❌ |
| 12 | 0.200326 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 13 | 0.191678 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.191165 | `azmcp_cloudarchitect_design` | ❌ |
| 15 | 0.189036 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.177454 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.176225 | `azmcp_bestpractices_get` | ❌ |
| 18 | 0.173184 | `azmcp_applens_resource_diagnose` | ❌ |
| 19 | 0.172350 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.171465 | `azmcp_azureterraformbestpractices_get` | ❌ |

---

## Test 257

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
| 10 | 0.450140 | `azmcp_aks_cluster_get` | ❌ |
| 11 | 0.445602 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.445391 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.437447 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.424890 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.410380 | `azmcp_group_list` | ❌ |
| 16 | 0.400207 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.394066 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.388694 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.385318 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.383563 | `azmcp_acr_registry_list` | ❌ |

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
| 5 | 0.445419 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.443977 | `azmcp_mysql_server_param_get` | ❌ |
| 7 | 0.422646 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.413964 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 9 | 0.406630 | `azmcp_loadtesting_test_get` | ❌ |
| 10 | 0.400827 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.379960 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.359439 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.349874 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.316818 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.314864 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.308718 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.300098 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.298409 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.295903 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.284481 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 259

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.380036 | `azmcp_postgres_server_param_get` | ❌ |
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
| 16 | 0.253956 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.246266 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.240682 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.237909 | `azmcp_keyvault_certificate_get` | ❌ |
| 20 | 0.236982 | `azmcp_keyvault_key_list` | ❌ |

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
| 6 | 0.355049 | `azmcp_loadtesting_testresource_create` | ❌ |
| 7 | 0.351902 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.325938 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.323501 | `azmcp_appconfig_kv_set` | ❌ |
| 10 | 0.319843 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.315241 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.311941 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.311275 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.305188 | `azmcp_keyvault_certificate_create` | ❌ |
| 15 | 0.300188 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.298887 | `azmcp_storage_datalake_directory_create` | ❌ |
| 17 | 0.297236 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.289742 | `azmcp_appconfig_kv_show` | ❌ |
| 19 | 0.286778 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.277805 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 261

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.400151 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.387071 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.382836 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.377221 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.376155 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.361412 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.344343 | `azmcp_loadtesting_testresource_create` | ❌ |
| 9 | 0.340337 | `azmcp_storage_blob_container_get` | ❌ |
| 10 | 0.329099 | `azmcp_loadtesting_test_create` | ❌ |
| 11 | 0.324346 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.319383 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 13 | 0.310931 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.310707 | `azmcp_storage_blob_upload` | ❌ |
| 15 | 0.310332 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.284483 | `azmcp_deploy_plan_get` | ❌ |
| 17 | 0.284385 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.283067 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.280404 | `azmcp_keyvault_certificate_create` | ❌ |
| 20 | 0.280192 | `azmcp_cloudarchitect_design` | ❌ |

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
| 6 | 0.407411 | `azmcp_storage_blob_container_get` | ❌ |
| 7 | 0.386262 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.385096 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 9 | 0.383927 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.383895 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.382274 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.380638 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.380503 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.372681 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.372357 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.366696 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.363721 | `azmcp_workbooks_create` | ❌ |
| 18 | 0.359316 | `azmcp_keyvault_secret_create` | ❌ |
| 19 | 0.324944 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.321883 | `azmcp_deploy_plan_get` | ❌ |

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
| 6 | 0.442858 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.439236 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.431020 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.403478 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.397051 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.395698 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.388425 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.373840 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.373146 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.371705 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 16 | 0.368567 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.367049 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.366454 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.360593 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.356973 | `azmcp_cosmos_database_list` | ❌ |

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
| 6 | 0.415410 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.411808 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.401802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.380012 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.375790 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.373470 | `azmcp_aks_cluster_get` | ❌ |
| 12 | 0.369755 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.368207 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.368023 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.362602 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.362437 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.362236 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.355178 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.354842 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.354534 | `azmcp_monitor_resource_log_query` | ❌ |

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
| 6 | 0.501172 | `azmcp_subscription_list` | ❌ |
| 7 | 0.496371 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.493246 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.484236 | `azmcp_storage_blob_container_get` | ❌ |
| 10 | 0.484163 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.473387 | `azmcp_search_service_list` | ❌ |
| 12 | 0.458793 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.454195 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.447977 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.445545 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.441838 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.440396 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.432645 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.416387 | `azmcp_group_list` | ❌ |
| 20 | 0.414108 | `azmcp_marketplace_product_get` | ❌ |

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
| 18 | 0.330250 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.322285 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.312384 | `azmcp_acr_registry_list` | ❌ |

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
| 5 | 0.454025 | `azmcp_subscription_list` | ❌ |
| 6 | 0.436170 | `azmcp_search_service_list` | ❌ |
| 7 | 0.432854 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.425048 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.418403 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.415843 | `azmcp_storage_blob_get` | ❌ |
| 11 | 0.415080 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.392617 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.383856 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.382593 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.379856 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.376660 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.374635 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.359998 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.359053 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.356552 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 268

**Expected Tool:** `azmcp_storage_blob_batch_set-tier`  
**Prompt:** Set access tier to Cool for multiple blobs in the container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620756 | `azmcp_storage_blob_batch_set-tier` | ✅ **EXPECTED** |
| 2 | 0.465722 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.408639 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.408384 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.378380 | `azmcp_cosmos_database_container_list` | ❌ |
| 6 | 0.369393 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.348748 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.324757 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.305741 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.304711 | `azmcp_storage_blob_upload` | ❌ |
| 11 | 0.295668 | `azmcp_storage_queue_message_send` | ❌ |
| 12 | 0.295532 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.295512 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.295133 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.289458 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 16 | 0.286940 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.285276 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.271887 | `azmcp_appconfig_kv_show` | ❌ |
| 19 | 0.265600 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.262595 | `azmcp_monitor_resource_log_query` | ❌ |

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
| 6 | 0.351892 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.343981 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.341857 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.325105 | `azmcp_storage_blob_upload` | ❌ |
| 10 | 0.311146 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.301044 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.300312 | `azmcp_sql_db_update` | ❌ |
| 13 | 0.295798 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.280305 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.272376 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 16 | 0.271310 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.267309 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.253981 | `azmcp_appconfig_kv_set` | ❌ |
| 19 | 0.251624 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.246907 | `azmcp_deploy_iac_rules_get` | ❌ |

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
| 6 | 0.387848 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.335039 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 8 | 0.331449 | `azmcp_storage_blob_get` | ❌ |
| 9 | 0.326352 | `azmcp_appconfig_kv_set` | ❌ |
| 10 | 0.324867 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.323238 | `azmcp_keyvault_secret_create` | ❌ |
| 12 | 0.322464 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.320470 | `azmcp_storage_datalake_directory_create` | ❌ |
| 14 | 0.318855 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.305680 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.297912 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.297384 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.292093 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.291137 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.281807 | `azmcp_sql_server_create` | ❌ |

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
| 6 | 0.368859 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.361494 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 8 | 0.354861 | `azmcp_storage_table_list` | ❌ |
| 9 | 0.334040 | `azmcp_storage_blob_upload` | ❌ |
| 10 | 0.320173 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.309739 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.296899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.296438 | `azmcp_sql_db_create` | ❌ |
| 14 | 0.285153 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.278052 | `azmcp_keyvault_secret_create` | ❌ |
| 16 | 0.275240 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.275199 | `azmcp_keyvault_key_create` | ❌ |
| 18 | 0.270167 | `azmcp_appconfig_kv_set` | ❌ |
| 19 | 0.269625 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.268922 | `azmcp_workbooks_create` | ❌ |

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
| 6 | 0.378021 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.375383 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.366330 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.351724 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 10 | 0.329038 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.322364 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.314128 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.309104 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.287885 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.280806 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.277049 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.276533 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.275575 | `azmcp_keyvault_secret_create` | ❌ |
| 19 | 0.269719 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.266791 | `azmcp_appconfig_kv_set` | ❌ |

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
| 17 | 0.350264 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.335739 | `azmcp_appconfig_kv_list` | ❌ |
| 19 | 0.334806 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.332134 | `azmcp_deploy_app_logs_get` | ❌ |

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
| 6 | 0.471385 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.453044 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.409820 | `azmcp_acr_registry_repository_list` | ❌ |
| 9 | 0.404640 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.393989 | `azmcp_storage_blob_container_create` | ❌ |
| 11 | 0.386144 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.367427 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.367036 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.359465 | `azmcp_search_service_list` | ❌ |
| 15 | 0.359378 | `azmcp_subscription_list` | ❌ |
| 16 | 0.358630 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 17 | 0.356400 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.351601 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.351566 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.348288 | `azmcp_appconfig_account_list` | ❌ |

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
| 6 | 0.437887 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.429767 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.418128 | `azmcp_storage_blob_container_create` | ❌ |
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
| 19 | 0.340643 | `azmcp_subscription_list` | ❌ |
| 20 | 0.340150 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

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
| 6 | 0.431571 | `azmcp_storage_table_list` | ❌ |
| 7 | 0.420748 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.408521 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.386482 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.359392 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.349565 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.345511 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.343238 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 14 | 0.338048 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.333887 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.330904 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.326504 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.323065 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.318346 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.303869 | `azmcp_aks_nodepool_get` | ❌ |

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
| 6 | 0.453696 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.388809 | `azmcp_storage_table_list` | ❌ |
| 8 | 0.370177 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.360712 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.359655 | `azmcp_aks_cluster_get` | ❌ |
| 11 | 0.358376 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.353461 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.353131 | `azmcp_workbooks_show` | ❌ |
| 14 | 0.352671 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.348551 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.342860 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.337010 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.334138 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.329754 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.319525 | `azmcp_keyvault_certificate_get` | ❌ |

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
| 6 | 0.452160 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.415853 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.413280 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.400483 | `azmcp_acr_registry_repository_list` | ❌ |
| 10 | 0.394852 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.382903 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 12 | 0.380052 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.379099 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.369639 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.363904 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 16 | 0.361689 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.359099 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.348791 | `azmcp_subscription_list` | ❌ |
| 19 | 0.340190 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.328193 | `azmcp_search_service_list` | ❌ |

---

## Test 279

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570349 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.549371 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.533522 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.456057 | `azmcp_storage_table_list` | ❌ |
| 5 | 0.449079 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.433879 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.397262 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.395794 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.385260 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.362258 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.359386 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 12 | 0.353754 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.345236 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.342696 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.342543 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 16 | 0.339810 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.336173 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.314055 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.308731 | `azmcp_storage_blob_upload` | ❌ |
| 20 | 0.306820 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 280

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566115 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.403093 | `azmcp_storage_blob_get` | ❌ |
| 3 | 0.397333 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.382088 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.376974 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.351761 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.327040 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.323952 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.307260 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.298029 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 11 | 0.297131 | `azmcp_storage_queue_message_send` | ❌ |
| 12 | 0.294668 | `azmcp_keyvault_certificate_import` | ❌ |
| 13 | 0.291484 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 14 | 0.284553 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.284413 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.273317 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.273253 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 18 | 0.272373 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 19 | 0.257796 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.253385 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 281

**Expected Tool:** `azmcp_storage_datalake_directory_create`  
**Prompt:** Create a new directory at the path <directory_path> in Data Lake in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647078 | `azmcp_storage_datalake_directory_create` | ✅ **EXPECTED** |
| 2 | 0.481507 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 3 | 0.442414 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.348448 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.340453 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.340046 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.333862 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.329189 | `azmcp_sql_db_create` | ❌ |
| 9 | 0.313913 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.306845 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.303932 | `azmcp_storage_table_list` | ❌ |
| 12 | 0.302849 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.297012 | `azmcp_loadtesting_test_create` | ❌ |
| 14 | 0.295247 | `azmcp_storage_blob_upload` | ❌ |
| 15 | 0.281849 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.281674 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.276608 | `azmcp_appconfig_kv_set` | ❌ |
| 18 | 0.249193 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.241212 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.240531 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 282

**Expected Tool:** `azmcp_storage_datalake_file-system_list-paths`  
**Prompt:** List all paths in the Data Lake file system <file_system> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.768097 | `azmcp_storage_datalake_file-system_list-paths` | ✅ **EXPECTED** |
| 2 | 0.506211 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.481716 | `azmcp_storage_table_list` | ❌ |
| 4 | 0.451751 | `azmcp_storage_datalake_directory_create` | ❌ |
| 5 | 0.432225 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.421096 | `azmcp_storage_share_file_list` | ❌ |
| 7 | 0.419344 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.415032 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.402172 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.390634 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.384491 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.374963 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.358062 | `azmcp_monitor_table_type_list` | ❌ |
| 14 | 0.352714 | `azmcp_search_service_list` | ❌ |
| 15 | 0.349364 | `azmcp_subscription_list` | ❌ |
| 16 | 0.346934 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.344399 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.337554 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.337221 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.333706 | `azmcp_acr_registry_repository_list` | ❌ |

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
| 6 | 0.399930 | `azmcp_storage_blob_container_get` | ❌ |
| 7 | 0.384157 | `azmcp_storage_share_file_list` | ❌ |
| 8 | 0.372453 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.347625 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.345916 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.344289 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.335052 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.329464 | `azmcp_storage_blob_get` | ❌ |
| 14 | 0.327668 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.325117 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.321697 | `azmcp_monitor_table_type_list` | ❌ |
| 17 | 0.304870 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.304588 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.304566 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.289587 | `azmcp_acr_registry_repository_list` | ❌ |

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
| 6 | 0.372101 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.363918 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.358303 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.343302 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.337285 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.335036 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.333908 | `azmcp_acr_registry_repository_list` | ❌ |
| 13 | 0.323351 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.322053 | `azmcp_search_service_list` | ❌ |
| 15 | 0.317872 | `azmcp_subscription_list` | ❌ |
| 16 | 0.317437 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.314512 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.314318 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.310101 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.299896 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 285

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Send a message "Hello, World!" to the queue <queue> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558401 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.410972 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.375068 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.373072 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.344491 | `azmcp_servicebus_queue_details` | ❌ |
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
| 19 | 0.260151 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.258161 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 286

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Send a message with TTL of 3600 seconds to the queue <queue> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642281 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.383347 | `azmcp_storage_table_list` | ❌ |
| 3 | 0.373094 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.357077 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.347722 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.334605 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.325315 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.317267 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.315919 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.314996 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.312411 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.310355 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.303960 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 14 | 0.282659 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.282606 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 16 | 0.277831 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.273189 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.271352 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.257351 | `azmcp_keyvault_secret_create` | ❌ |
| 20 | 0.248004 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 287

**Expected Tool:** `azmcp_storage_queue_message_send`  
**Prompt:** Add a message to the queue <queue> in storage account <account> with visibility timeout of 30 seconds  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595323 | `azmcp_storage_queue_message_send` | ✅ **EXPECTED** |
| 2 | 0.360641 | `azmcp_servicebus_queue_details` | ❌ |
| 3 | 0.338590 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.325347 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.322604 | `azmcp_storage_table_list` | ❌ |
| 6 | 0.313149 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.312357 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.297459 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.293460 | `azmcp_storage_blob_upload` | ❌ |
| 10 | 0.278704 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 11 | 0.274559 | `azmcp_keyvault_secret_create` | ❌ |
| 12 | 0.271013 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.270512 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.266491 | `azmcp_monitor_workspace_log_query` | ❌ |
| 15 | 0.262108 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.260463 | `azmcp_storage_blob_batch_set-tier` | ❌ |
| 17 | 0.257511 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.247138 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.245807 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.241490 | `azmcp_keyvault_key_create` | ❌ |

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
| 6 | 0.458759 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.433528 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.416549 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.404225 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.397963 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.391675 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.390560 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.385362 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.382087 | `azmcp_search_service_list` | ❌ |
| 15 | 0.373056 | `azmcp_keyvault_certificate_list` | ❌ |
| 16 | 0.372921 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.366463 | `azmcp_subscription_list` | ❌ |
| 18 | 0.360454 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.353892 | `azmcp_monitor_table_list` | ❌ |
| 20 | 0.353596 | `azmcp_appconfig_account_list` | ❌ |

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
| 6 | 0.405964 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.380180 | `azmcp_storage_datalake_directory_create` | ❌ |
| 8 | 0.351906 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.351055 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.341853 | `azmcp_storage_blob_get` | ❌ |
| 11 | 0.341352 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.331565 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.328388 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.320461 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.317899 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.315417 | `azmcp_keyvault_key_list` | ❌ |
| 17 | 0.304034 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.303900 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.301881 | `azmcp_search_service_list` | ❌ |
| 20 | 0.297519 | `azmcp_subscription_list` | ❌ |

---

## Test 290

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
| 15 | 0.343815 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.339261 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.336352 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.332926 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.319836 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.319475 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 291

**Expected Tool:** `azmcp_storage_table_list`  
**Prompt:** List all tables in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.787243 | `azmcp_storage_table_list` | ✅ **EXPECTED** |
| 2 | 0.574874 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.552523 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.514042 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.510657 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.505290 | `azmcp_storage_blob_container_get` | ❌ |
| 7 | 0.503638 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.498181 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.497572 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.491995 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.486049 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.430612 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.421849 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.421152 | `azmcp_storage_datalake_file-system_list-paths` | ❌ |
| 15 | 0.407089 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.404701 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.396428 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.393795 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.362914 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.360786 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 292

**Expected Tool:** `azmcp_storage_table_list`  
**Prompt:** Show me the tables in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738095 | `azmcp_storage_table_list` | ✅ **EXPECTED** |
| 2 | 0.521718 | `azmcp_monitor_table_list` | ❌ |
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
| 16 | 0.406204 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.380764 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.368191 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.365922 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.362253 | `azmcp_kusto_sample` | ❌ |

---

## Test 293

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576093 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.473852 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471653 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.467765 | `azmcp_eventgrid_subscription_list` | ❌ |
| 6 | 0.452471 | `azmcp_search_service_list` | ❌ |
| 7 | 0.450973 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.445724 | `azmcp_grafana_list` | ❌ |
| 9 | 0.436338 | `azmcp_storage_table_list` | ❌ |
| 10 | 0.431337 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.430280 | `azmcp_group_list` | ❌ |
| 12 | 0.422734 | `azmcp_eventgrid_topic_list` | ❌ |
| 13 | 0.406935 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.394917 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.388737 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.380636 | `azmcp_marketplace_product_list` | ❌ |
| 17 | 0.367761 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.366831 | `azmcp_loadtesting_testresource_list` | ❌ |
| 19 | 0.348524 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.344901 | `azmcp_servicebus_topic_subscription_details` | ❌ |

---

## Test 294

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405792 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.384208 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.381238 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.351864 | `azmcp_grafana_list` | ❌ |
| 5 | 0.350951 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.341813 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.334787 | `azmcp_eventgrid_topic_list` | ❌ |
| 8 | 0.328109 | `azmcp_search_service_list` | ❌ |
| 9 | 0.315604 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.308874 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.303528 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.303367 | `azmcp_marketplace_product_list` | ❌ |
| 13 | 0.297209 | `azmcp_group_list` | ❌ |
| 14 | 0.296282 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.295181 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.285434 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.275422 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.269922 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.256330 | `azmcp_storage_table_list` | ❌ |
| 20 | 0.244501 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 295

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.320025 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.315549 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.313335 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.286711 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.282645 | `azmcp_grafana_list` | ❌ |
| 6 | 0.279702 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.278798 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.273758 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.256358 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.254815 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.252938 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.252443 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.251683 | `azmcp_search_service_list` | ❌ |
| 14 | 0.251368 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.233143 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.230571 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.230324 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.227020 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.226446 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.211120 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 296

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.403291 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.375168 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.354504 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.342318 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.340339 | `azmcp_grafana_list` | ❌ |
| 6 | 0.336798 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.311939 | `azmcp_search_service_list` | ❌ |
| 8 | 0.311109 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.305161 | `azmcp_marketplace_product_get` | ❌ |
| 10 | 0.304965 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.302338 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.300478 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.294080 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.291826 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.282288 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.281294 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.274224 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.269869 | `azmcp_group_list` | ❌ |
| 19 | 0.258410 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.236600 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 297

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
| 9 | 0.525737 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.525637 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.506608 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.505149 | `azmcp_subscription_list` | ❌ |
| 13 | 0.496297 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.495490 | `azmcp_grafana_list` | ❌ |
| 15 | 0.492515 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.476718 | `azmcp_group_list` | ❌ |
| 17 | 0.465511 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.463011 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.461974 | `azmcp_eventgrid_subscription_list` | ❌ |
| 20 | 0.460388 | `azmcp_acr_registry_list` | ❌ |

---

## Test 298

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714469 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.439611 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.402858 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.393721 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.364696 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.362307 | `azmcp_search_service_list` | ❌ |
| 9 | 0.344853 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.337530 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.335295 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.333517 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.333003 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.330912 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.328734 | `azmcp_keyvault_key_list` | ❌ |
| 16 | 0.321826 | `azmcp_subscription_list` | ❌ |
| 17 | 0.312156 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.311262 | `azmcp_grafana_list` | ❌ |
| 19 | 0.308168 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.302706 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 299

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812659 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.659212 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501167 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.356479 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.336385 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.327423 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.324576 | `azmcp_subscription_list` | ❌ |
| 8 | 0.324289 | `azmcp_search_service_list` | ❌ |
| 9 | 0.316295 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.315778 | `azmcp_loadtesting_testrun_list` | ❌ |
| 11 | 0.307862 | `azmcp_aks_nodepool_get` | ❌ |
| 12 | 0.305405 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.305175 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.304414 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.300014 | `azmcp_keyvault_secret_list` | ❌ |
| 16 | 0.299477 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.295899 | `azmcp_grafana_list` | ❌ |
| 18 | 0.284934 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.278813 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.278222 | `azmcp_datadog_monitoredresources_list` | ❌ |

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
| 6 | 0.239865 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.217264 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.214818 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.188137 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.172748 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.169440 | `azmcp_grafana_list` | ❌ |
| 12 | 0.164006 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.153950 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.148897 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.147365 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.143713 | `azmcp_sql_server_create` | ❌ |
| 17 | 0.130524 | `azmcp_loadtesting_testrun_create` | ❌ |
| 18 | 0.130339 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.116860 | `azmcp_loadtesting_testrun_update` | ❌ |
| 20 | 0.113927 | `azmcp_deploy_plan_get` | ❌ |

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
| 6 | 0.273939 | `azmcp_grafana_list` | ❌ |
| 7 | 0.256795 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 8 | 0.248002 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.242993 | `azmcp_sql_server_delete` | ❌ |
| 10 | 0.198585 | `azmcp_appconfig_kv_delete` | ❌ |
| 11 | 0.190455 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.186665 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.181661 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.155100 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.148882 | `azmcp_extension_azqr` | ❌ |
| 16 | 0.145041 | `azmcp_loadtesting_testresource_list` | ❌ |
| 17 | 0.134990 | `azmcp_loadtesting_testrun_update` | ❌ |
| 18 | 0.132504 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.131813 | `azmcp_group_list` | ❌ |
| 20 | 0.122450 | `azmcp_loadtesting_test_get` | ❌ |

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
| 6 | 0.487920 | `azmcp_workbooks_delete` | ❌ |
| 7 | 0.459976 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.454210 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.439945 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.428781 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.416520 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.413409 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.405870 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.405064 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.399758 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.365302 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.362740 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.356739 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.352940 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.349674 | `azmcp_cosmos_account_list` | ❌ |

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
| 6 | 0.428025 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.425426 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.422785 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421646 | `azmcp_group_list` | ❌ |
| 10 | 0.412390 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.392290 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.380991 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.371128 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.363744 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.362629 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.350839 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.338334 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.337786 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.334580 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.329813 | `azmcp_eventgrid_subscription_list` | ❌ |

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
| 6 | 0.353546 | `azmcp_grafana_list` | ❌ |
| 7 | 0.277807 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.264640 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.256678 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.250024 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.236741 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.230558 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.230516 | `azmcp_monitor_metrics_definitions` | ❌ |
| 14 | 0.227558 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.225294 | `azmcp_loadtesting_test_get` | ❌ |
| 16 | 0.223667 | `azmcp_servicebus_topic_details` | ❌ |
| 17 | 0.218915 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.207693 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.197245 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 20 | 0.195373 | `azmcp_group_list` | ❌ |

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
| 6 | 0.292898 | `azmcp_grafana_list` | ❌ |
| 7 | 0.266450 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.239907 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.227383 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.176481 | `azmcp_role_assignment_list` | ❌ |
| 11 | 0.175814 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.174636 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.174123 | `azmcp_storage_table_list` | ❌ |
| 14 | 0.168191 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.165774 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.154760 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.152535 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.149678 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.148992 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.146054 | `azmcp_kusto_table_schema` | ❌ |

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
| 5 | 0.276751 | `azmcp_loadtesting_testrun_update` | ❌ |
| 6 | 0.262873 | `azmcp_workbooks_list` | ❌ |
| 7 | 0.174535 | `azmcp_sql_db_update` | ❌ |
| 8 | 0.170118 | `azmcp_grafana_list` | ❌ |
| 9 | 0.148730 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.142404 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.142186 | `azmcp_loadtesting_testrun_create` | ❌ |
| 12 | 0.138354 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.136105 | `azmcp_loadtesting_testresource_create` | ❌ |
| 14 | 0.131007 | `azmcp_postgres_database_query` | ❌ |
| 15 | 0.129973 | `azmcp_postgres_server_param_set` | ❌ |
| 16 | 0.129660 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.126312 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.111099 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 19 | 0.105705 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.100448 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Summary

**Total Prompts Tested:** 306  
**Execution Time:** 36.8302926s  

### Success Rate Metrics

**Top Choice Success:** 84.6% (259/306 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 4.6% (14/306 tests)  
**🎯 High Confidence (≥0.7):** 20.9% (64/306 tests)  
**✅ Good Confidence (≥0.6):** 59.2% (181/306 tests)  
**👍 Fair Confidence (≥0.5):** 85.3% (261/306 tests)  
**👌 Acceptable Confidence (≥0.4):** 94.8% (290/306 tests)  
**❌ Low Confidence (<0.4):** 5.2% (16/306 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 4.6% (14/306 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 20.9% (64/306 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 56.2% (172/306 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 76.8% (235/306 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 81.7% (250/306 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing adequately but has room for improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

