# Tool Selection Analysis Setup

**Setup completed:** 2025-09-26 09:36:25  
**Tool count:** 144  
**Database setup time:** 1.5962594s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-26 09:36:25  
**Tool count:** 144  

## Table of Contents

- [Test 1: azmcp_foundry_agents_connect](#test-1)
- [Test 2: azmcp_foundry_agents_evaluate](#test-2)
- [Test 3: azmcp_foundry_agents_query-and-evaluate](#test-3)
- [Test 4: azmcp_foundry_knowledge_index_list](#test-4)
- [Test 5: azmcp_foundry_knowledge_index_list](#test-5)
- [Test 6: azmcp_foundry_knowledge_index_schema](#test-6)
- [Test 7: azmcp_foundry_knowledge_index_schema](#test-7)
- [Test 8: azmcp_foundry_models_deploy](#test-8)
- [Test 9: azmcp_foundry_models_deployments_list](#test-9)
- [Test 10: azmcp_foundry_models_deployments_list](#test-10)
- [Test 11: azmcp_foundry_models_list](#test-11)
- [Test 12: azmcp_foundry_models_list](#test-12)
- [Test 13: azmcp_search_index_get](#test-13)
- [Test 14: azmcp_search_index_get](#test-14)
- [Test 15: azmcp_search_index_get](#test-15)
- [Test 16: azmcp_search_index_query](#test-16)
- [Test 17: azmcp_search_service_list](#test-17)
- [Test 18: azmcp_search_service_list](#test-18)
- [Test 19: azmcp_search_service_list](#test-19)
- [Test 20: azmcp_appconfig_account_list](#test-20)
- [Test 21: azmcp_appconfig_account_list](#test-21)
- [Test 22: azmcp_appconfig_account_list](#test-22)
- [Test 23: azmcp_appconfig_kv_delete](#test-23)
- [Test 24: azmcp_appconfig_kv_list](#test-24)
- [Test 25: azmcp_appconfig_kv_list](#test-25)
- [Test 26: azmcp_appconfig_kv_lock_set](#test-26)
- [Test 27: azmcp_appconfig_kv_lock_set](#test-27)
- [Test 28: azmcp_appconfig_kv_set](#test-28)
- [Test 29: azmcp_appconfig_kv_show](#test-29)
- [Test 30: azmcp_applens_resource_diagnose](#test-30)
- [Test 31: azmcp_applens_resource_diagnose](#test-31)
- [Test 32: azmcp_applens_resource_diagnose](#test-32)
- [Test 33: azmcp_appservice_database_add](#test-33)
- [Test 34: azmcp_appservice_database_add](#test-34)
- [Test 35: azmcp_appservice_database_add](#test-35)
- [Test 36: azmcp_appservice_database_add](#test-36)
- [Test 37: azmcp_appservice_database_add](#test-37)
- [Test 38: azmcp_appservice_database_add](#test-38)
- [Test 39: azmcp_appservice_database_add](#test-39)
- [Test 40: azmcp_appservice_database_add](#test-40)
- [Test 41: azmcp_appservice_database_add](#test-41)
- [Test 42: azmcp_applicationinsights_recommendation_list](#test-42)
- [Test 43: azmcp_applicationinsights_recommendation_list](#test-43)
- [Test 44: azmcp_applicationinsights_recommendation_list](#test-44)
- [Test 45: azmcp_applicationinsights_recommendation_list](#test-45)
- [Test 46: azmcp_acr_registry_list](#test-46)
- [Test 47: azmcp_acr_registry_list](#test-47)
- [Test 48: azmcp_acr_registry_list](#test-48)
- [Test 49: azmcp_acr_registry_list](#test-49)
- [Test 50: azmcp_acr_registry_list](#test-50)
- [Test 51: azmcp_acr_registry_repository_list](#test-51)
- [Test 52: azmcp_acr_registry_repository_list](#test-52)
- [Test 53: azmcp_acr_registry_repository_list](#test-53)
- [Test 54: azmcp_acr_registry_repository_list](#test-54)
- [Test 55: azmcp_cosmos_account_list](#test-55)
- [Test 56: azmcp_cosmos_account_list](#test-56)
- [Test 57: azmcp_cosmos_account_list](#test-57)
- [Test 58: azmcp_cosmos_database_container_item_query](#test-58)
- [Test 59: azmcp_cosmos_database_container_list](#test-59)
- [Test 60: azmcp_cosmos_database_container_list](#test-60)
- [Test 61: azmcp_cosmos_database_list](#test-61)
- [Test 62: azmcp_cosmos_database_list](#test-62)
- [Test 63: azmcp_kusto_cluster_get](#test-63)
- [Test 64: azmcp_kusto_cluster_list](#test-64)
- [Test 65: azmcp_kusto_cluster_list](#test-65)
- [Test 66: azmcp_kusto_cluster_list](#test-66)
- [Test 67: azmcp_kusto_database_list](#test-67)
- [Test 68: azmcp_kusto_database_list](#test-68)
- [Test 69: azmcp_kusto_query](#test-69)
- [Test 70: azmcp_kusto_sample](#test-70)
- [Test 71: azmcp_kusto_table_list](#test-71)
- [Test 72: azmcp_kusto_table_list](#test-72)
- [Test 73: azmcp_kusto_table_schema](#test-73)
- [Test 74: azmcp_mysql_database_list](#test-74)
- [Test 75: azmcp_mysql_database_list](#test-75)
- [Test 76: azmcp_mysql_database_query](#test-76)
- [Test 77: azmcp_mysql_server_config_get](#test-77)
- [Test 78: azmcp_mysql_server_list](#test-78)
- [Test 79: azmcp_mysql_server_list](#test-79)
- [Test 80: azmcp_mysql_server_list](#test-80)
- [Test 81: azmcp_mysql_server_param_get](#test-81)
- [Test 82: azmcp_mysql_server_param_set](#test-82)
- [Test 83: azmcp_mysql_table_list](#test-83)
- [Test 84: azmcp_mysql_table_list](#test-84)
- [Test 85: azmcp_mysql_table_schema_get](#test-85)
- [Test 86: azmcp_postgres_database_list](#test-86)
- [Test 87: azmcp_postgres_database_list](#test-87)
- [Test 88: azmcp_postgres_database_query](#test-88)
- [Test 89: azmcp_postgres_server_config_get](#test-89)
- [Test 90: azmcp_postgres_server_list](#test-90)
- [Test 91: azmcp_postgres_server_list](#test-91)
- [Test 92: azmcp_postgres_server_list](#test-92)
- [Test 93: azmcp_postgres_server_param](#test-93)
- [Test 94: azmcp_postgres_server_param_set](#test-94)
- [Test 95: azmcp_postgres_table_list](#test-95)
- [Test 96: azmcp_postgres_table_list](#test-96)
- [Test 97: azmcp_postgres_table_schema_get](#test-97)
- [Test 98: azmcp_deploy_app_logs_get](#test-98)
- [Test 99: azmcp_deploy_architecture_diagram_generate](#test-99)
- [Test 100: azmcp_deploy_iac_rules_get](#test-100)
- [Test 101: azmcp_deploy_pipeline_guidance_get](#test-101)
- [Test 102: azmcp_deploy_plan_get](#test-102)
- [Test 103: azmcp_eventgrid_topic_list](#test-103)
- [Test 104: azmcp_eventgrid_topic_list](#test-104)
- [Test 105: azmcp_eventgrid_topic_list](#test-105)
- [Test 106: azmcp_eventgrid_topic_list](#test-106)
- [Test 107: azmcp_eventgrid_subscription_list](#test-107)
- [Test 108: azmcp_eventgrid_subscription_list](#test-108)
- [Test 109: azmcp_eventgrid_subscription_list](#test-109)
- [Test 110: azmcp_eventgrid_subscription_list](#test-110)
- [Test 111: azmcp_eventgrid_subscription_list](#test-111)
- [Test 112: azmcp_eventgrid_subscription_list](#test-112)
- [Test 113: azmcp_eventgrid_subscription_list](#test-113)
- [Test 114: azmcp_eventgrid_events_publish](#test-114)
- [Test 115: azmcp_eventgrid_events_publish](#test-115)
- [Test 116: azmcp_eventgrid_events_publish](#test-116)
- [Test 117: azmcp_functionapp_get](#test-117)
- [Test 118: azmcp_functionapp_get](#test-118)
- [Test 119: azmcp_functionapp_get](#test-119)
- [Test 120: azmcp_functionapp_get](#test-120)
- [Test 121: azmcp_functionapp_get](#test-121)
- [Test 122: azmcp_functionapp_get](#test-122)
- [Test 123: azmcp_functionapp_get](#test-123)
- [Test 124: azmcp_functionapp_get](#test-124)
- [Test 125: azmcp_functionapp_get](#test-125)
- [Test 126: azmcp_functionapp_get](#test-126)
- [Test 127: azmcp_functionapp_get](#test-127)
- [Test 128: azmcp_functionapp_get](#test-128)
- [Test 129: azmcp_keyvault_admin_settings_get](#test-129)
- [Test 130: azmcp_keyvault_admin_settings_get](#test-130)
- [Test 131: azmcp_keyvault_admin_settings_get](#test-131)
- [Test 132: azmcp_keyvault_certificate_create](#test-132)
- [Test 133: azmcp_keyvault_certificate_get](#test-133)
- [Test 134: azmcp_keyvault_certificate_get](#test-134)
- [Test 135: azmcp_keyvault_certificate_import](#test-135)
- [Test 136: azmcp_keyvault_certificate_import](#test-136)
- [Test 137: azmcp_keyvault_certificate_list](#test-137)
- [Test 138: azmcp_keyvault_certificate_list](#test-138)
- [Test 139: azmcp_keyvault_key_create](#test-139)
- [Test 140: azmcp_keyvault_key_get](#test-140)
- [Test 141: azmcp_keyvault_key_get](#test-141)
- [Test 142: azmcp_keyvault_key_list](#test-142)
- [Test 143: azmcp_keyvault_key_list](#test-143)
- [Test 144: azmcp_keyvault_secret_create](#test-144)
- [Test 145: azmcp_keyvault_secret_get](#test-145)
- [Test 146: azmcp_keyvault_secret_get](#test-146)
- [Test 147: azmcp_keyvault_secret_list](#test-147)
- [Test 148: azmcp_keyvault_secret_list](#test-148)
- [Test 149: azmcp_aks_cluster_get](#test-149)
- [Test 150: azmcp_aks_cluster_get](#test-150)
- [Test 151: azmcp_aks_cluster_get](#test-151)
- [Test 152: azmcp_aks_cluster_get](#test-152)
- [Test 153: azmcp_aks_cluster_get](#test-153)
- [Test 154: azmcp_aks_cluster_get](#test-154)
- [Test 155: azmcp_aks_cluster_get](#test-155)
- [Test 156: azmcp_aks_nodepool_get](#test-156)
- [Test 157: azmcp_aks_nodepool_get](#test-157)
- [Test 158: azmcp_aks_nodepool_get](#test-158)
- [Test 159: azmcp_aks_nodepool_get](#test-159)
- [Test 160: azmcp_aks_nodepool_get](#test-160)
- [Test 161: azmcp_aks_nodepool_get](#test-161)
- [Test 162: azmcp_loadtesting_test_create](#test-162)
- [Test 163: azmcp_loadtesting_test_get](#test-163)
- [Test 164: azmcp_loadtesting_testresource_create](#test-164)
- [Test 165: azmcp_loadtesting_testresource_list](#test-165)
- [Test 166: azmcp_loadtesting_testrun_create](#test-166)
- [Test 167: azmcp_loadtesting_testrun_get](#test-167)
- [Test 168: azmcp_loadtesting_testrun_list](#test-168)
- [Test 169: azmcp_loadtesting_testrun_update](#test-169)
- [Test 170: azmcp_grafana_list](#test-170)
- [Test 171: azmcp_azuremanagedlustre_filesystem_list](#test-171)
- [Test 172: azmcp_azuremanagedlustre_filesystem_list](#test-172)
- [Test 173: azmcp_azuremanagedlustre_filesystem_required-subnet-size](#test-173)
- [Test 174: azmcp_azuremanagedlustre_filesystem_sku_get](#test-174)
- [Test 175: azmcp_marketplace_product_get](#test-175)
- [Test 176: azmcp_marketplace_product_list](#test-176)
- [Test 177: azmcp_marketplace_product_list](#test-177)
- [Test 178: azmcp_bestpractices_get](#test-178)
- [Test 179: azmcp_bestpractices_get](#test-179)
- [Test 180: azmcp_bestpractices_get](#test-180)
- [Test 181: azmcp_bestpractices_get](#test-181)
- [Test 182: azmcp_bestpractices_get](#test-182)
- [Test 183: azmcp_bestpractices_get](#test-183)
- [Test 184: azmcp_bestpractices_get](#test-184)
- [Test 185: azmcp_bestpractices_get](#test-185)
- [Test 186: azmcp_bestpractices_get](#test-186)
- [Test 187: azmcp_bestpractices_get](#test-187)
- [Test 188: azmcp_monitor_healthmodels_entity_gethealth](#test-188)
- [Test 189: azmcp_monitor_metrics_definitions](#test-189)
- [Test 190: azmcp_monitor_metrics_definitions](#test-190)
- [Test 191: azmcp_monitor_metrics_definitions](#test-191)
- [Test 192: azmcp_monitor_metrics_query](#test-192)
- [Test 193: azmcp_monitor_metrics_query](#test-193)
- [Test 194: azmcp_monitor_metrics_query](#test-194)
- [Test 195: azmcp_monitor_metrics_query](#test-195)
- [Test 196: azmcp_monitor_metrics_query](#test-196)
- [Test 197: azmcp_monitor_metrics_query](#test-197)
- [Test 198: azmcp_monitor_resource_log_query](#test-198)
- [Test 199: azmcp_monitor_table_list](#test-199)
- [Test 200: azmcp_monitor_table_list](#test-200)
- [Test 201: azmcp_monitor_table_type_list](#test-201)
- [Test 202: azmcp_monitor_table_type_list](#test-202)
- [Test 203: azmcp_monitor_workspace_list](#test-203)
- [Test 204: azmcp_monitor_workspace_list](#test-204)
- [Test 205: azmcp_monitor_workspace_list](#test-205)
- [Test 206: azmcp_monitor_workspace_log_query](#test-206)
- [Test 207: azmcp_datadog_monitoredresources_list](#test-207)
- [Test 208: azmcp_datadog_monitoredresources_list](#test-208)
- [Test 209: azmcp_extension_azqr](#test-209)
- [Test 210: azmcp_extension_azqr](#test-210)
- [Test 211: azmcp_extension_azqr](#test-211)
- [Test 212: azmcp_quota_region_availability_list](#test-212)
- [Test 213: azmcp_quota_usage_check](#test-213)
- [Test 214: azmcp_role_assignment_list](#test-214)
- [Test 215: azmcp_role_assignment_list](#test-215)
- [Test 216: azmcp_redis_cache_accesspolicy_list](#test-216)
- [Test 217: azmcp_redis_cache_accesspolicy_list](#test-217)
- [Test 218: azmcp_redis_cache_list](#test-218)
- [Test 219: azmcp_redis_cache_list](#test-219)
- [Test 220: azmcp_redis_cache_list](#test-220)
- [Test 221: azmcp_redis_cluster_database_list](#test-221)
- [Test 222: azmcp_redis_cluster_database_list](#test-222)
- [Test 223: azmcp_redis_cluster_list](#test-223)
- [Test 224: azmcp_redis_cluster_list](#test-224)
- [Test 225: azmcp_redis_cluster_list](#test-225)
- [Test 226: azmcp_group_list](#test-226)
- [Test 227: azmcp_group_list](#test-227)
- [Test 228: azmcp_group_list](#test-228)
- [Test 229: azmcp_resourcehealth_availability-status_get](#test-229)
- [Test 230: azmcp_resourcehealth_availability-status_get](#test-230)
- [Test 231: azmcp_resourcehealth_availability-status_get](#test-231)
- [Test 232: azmcp_resourcehealth_availability-status_list](#test-232)
- [Test 233: azmcp_resourcehealth_availability-status_list](#test-233)
- [Test 234: azmcp_resourcehealth_availability-status_list](#test-234)
- [Test 235: azmcp_resourcehealth_service-health-events_list](#test-235)
- [Test 236: azmcp_resourcehealth_service-health-events_list](#test-236)
- [Test 237: azmcp_resourcehealth_service-health-events_list](#test-237)
- [Test 238: azmcp_resourcehealth_service-health-events_list](#test-238)
- [Test 239: azmcp_resourcehealth_service-health-events_list](#test-239)
- [Test 240: azmcp_servicebus_queue_details](#test-240)
- [Test 241: azmcp_servicebus_topic_details](#test-241)
- [Test 242: azmcp_servicebus_topic_subscription_details](#test-242)
- [Test 243: azmcp_sql_db_create](#test-243)
- [Test 244: azmcp_sql_db_create](#test-244)
- [Test 245: azmcp_sql_db_create](#test-245)
- [Test 246: azmcp_sql_db_delete](#test-246)
- [Test 247: azmcp_sql_db_delete](#test-247)
- [Test 248: azmcp_sql_db_delete](#test-248)
- [Test 249: azmcp_sql_db_list](#test-249)
- [Test 250: azmcp_sql_db_list](#test-250)
- [Test 251: azmcp_sql_db_rename](#test-251)
- [Test 252: azmcp_sql_db_rename](#test-252)
- [Test 253: azmcp_sql_db_show](#test-253)
- [Test 254: azmcp_sql_db_show](#test-254)
- [Test 255: azmcp_sql_db_update](#test-255)
- [Test 256: azmcp_sql_db_update](#test-256)
- [Test 257: azmcp_sql_elastic-pool_list](#test-257)
- [Test 258: azmcp_sql_elastic-pool_list](#test-258)
- [Test 259: azmcp_sql_elastic-pool_list](#test-259)
- [Test 260: azmcp_sql_server_create](#test-260)
- [Test 261: azmcp_sql_server_create](#test-261)
- [Test 262: azmcp_sql_server_create](#test-262)
- [Test 263: azmcp_sql_server_delete](#test-263)
- [Test 264: azmcp_sql_server_delete](#test-264)
- [Test 265: azmcp_sql_server_delete](#test-265)
- [Test 266: azmcp_sql_server_entra-admin_list](#test-266)
- [Test 267: azmcp_sql_server_entra-admin_list](#test-267)
- [Test 268: azmcp_sql_server_entra-admin_list](#test-268)
- [Test 269: azmcp_sql_server_firewall-rule_create](#test-269)
- [Test 270: azmcp_sql_server_firewall-rule_create](#test-270)
- [Test 271: azmcp_sql_server_firewall-rule_create](#test-271)
- [Test 272: azmcp_sql_server_firewall-rule_delete](#test-272)
- [Test 273: azmcp_sql_server_firewall-rule_delete](#test-273)
- [Test 274: azmcp_sql_server_firewall-rule_delete](#test-274)
- [Test 275: azmcp_sql_server_firewall-rule_list](#test-275)
- [Test 276: azmcp_sql_server_firewall-rule_list](#test-276)
- [Test 277: azmcp_sql_server_firewall-rule_list](#test-277)
- [Test 278: azmcp_sql_server_list](#test-278)
- [Test 279: azmcp_sql_server_list](#test-279)
- [Test 280: azmcp_sql_server_show](#test-280)
- [Test 281: azmcp_sql_server_show](#test-281)
- [Test 282: azmcp_sql_server_show](#test-282)
- [Test 283: azmcp_storage_account_create](#test-283)
- [Test 284: azmcp_storage_account_create](#test-284)
- [Test 285: azmcp_storage_account_create](#test-285)
- [Test 286: azmcp_storage_account_get](#test-286)
- [Test 287: azmcp_storage_account_get](#test-287)
- [Test 288: azmcp_storage_account_get](#test-288)
- [Test 289: azmcp_storage_account_get](#test-289)
- [Test 290: azmcp_storage_account_get](#test-290)
- [Test 291: azmcp_storage_blob_container_create](#test-291)
- [Test 292: azmcp_storage_blob_container_create](#test-292)
- [Test 293: azmcp_storage_blob_container_create](#test-293)
- [Test 294: azmcp_storage_blob_container_get](#test-294)
- [Test 295: azmcp_storage_blob_container_get](#test-295)
- [Test 296: azmcp_storage_blob_container_get](#test-296)
- [Test 297: azmcp_storage_blob_get](#test-297)
- [Test 298: azmcp_storage_blob_get](#test-298)
- [Test 299: azmcp_storage_blob_get](#test-299)
- [Test 300: azmcp_storage_blob_get](#test-300)
- [Test 301: azmcp_storage_blob_upload](#test-301)
- [Test 302: azmcp_subscription_list](#test-302)
- [Test 303: azmcp_subscription_list](#test-303)
- [Test 304: azmcp_subscription_list](#test-304)
- [Test 305: azmcp_subscription_list](#test-305)
- [Test 306: azmcp_azureterraformbestpractices_get](#test-306)
- [Test 307: azmcp_azureterraformbestpractices_get](#test-307)
- [Test 308: azmcp_virtualdesktop_hostpool_list](#test-308)
- [Test 309: azmcp_virtualdesktop_hostpool_sessionhost_list](#test-309)
- [Test 310: azmcp_virtualdesktop_hostpool_sessionhost_usersession-list](#test-310)
- [Test 311: azmcp_workbooks_create](#test-311)
- [Test 312: azmcp_workbooks_delete](#test-312)
- [Test 313: azmcp_workbooks_list](#test-313)
- [Test 314: azmcp_workbooks_list](#test-314)
- [Test 315: azmcp_workbooks_show](#test-315)
- [Test 316: azmcp_workbooks_show](#test-316)
- [Test 317: azmcp_workbooks_update](#test-317)
- [Test 318: azmcp_bicepschema_get](#test-318)
- [Test 319: azmcp_cloudarchitect_design](#test-319)
- [Test 320: azmcp_cloudarchitect_design](#test-320)
- [Test 321: azmcp_cloudarchitect_design](#test-321)
- [Test 322: azmcp_cloudarchitect_design](#test-322)

---

## Test 1

**Expected Tool:** `azmcp_foundry_agents_connect`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603124 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.535808 | `azmcp_foundry_agents_connect` | ✅ **EXPECTED** |
| 3 | 0.494462 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.443005 | `azmcp_foundry_agents_evaluate` | ❌ |
| 5 | 0.379587 | `azmcp_search_index_query` | ❌ |
| 6 | 0.365856 | `azmcp_foundry_models_list` | ❌ |
| 7 | 0.355385 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 8 | 0.327613 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.319855 | `azmcp_foundry_models_deploy` | ❌ |
| 10 | 0.305581 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.297446 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 12 | 0.272398 | `azmcp_search_service_list` | ❌ |
| 13 | 0.243499 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.241302 | `azmcp_postgres_database_query` | ❌ |
| 15 | 0.232633 | `azmcp_search_index_get` | ❌ |
| 16 | 0.230797 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.226536 | `azmcp_monitor_workspace_log_query` | ❌ |
| 18 | 0.217753 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.211141 | `azmcp_mysql_database_query` | ❌ |
| 20 | 0.191244 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |

---

## Test 2

**Expected Tool:** `azmcp_foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544099 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469400 | `azmcp_foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.356494 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.280833 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.235412 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.233781 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.233359 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.232102 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.228550 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.224773 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.220966 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.218372 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.214474 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.210219 | `azmcp_search_index_query` | ❌ |
| 15 | 0.207708 | `azmcp_postgres_database_query` | ❌ |
| 16 | 0.207578 | `azmcp_loadtesting_testrun_list` | ❌ |
| 17 | 0.203902 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 18 | 0.194160 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.187851 | `azmcp_mysql_table_schema_schema` | ❌ |
| 20 | 0.183167 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 3

**Expected Tool:** `azmcp_foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my AI Foundry project for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580566 | `azmcp_foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 2 | 0.518643 | `azmcp_foundry_agents_evaluate` | ❌ |
| 3 | 0.471098 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.381887 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.315906 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.315347 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.308767 | `azmcp_foundry_models_deploy` | ❌ |
| 8 | 0.276459 | `azmcp_foundry_models_list` | ❌ |
| 9 | 0.253361 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 10 | 0.246328 | `azmcp_search_index_query` | ❌ |
| 11 | 0.231465 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.207747 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.188362 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.183868 | `azmcp_postgres_database_query` | ❌ |
| 15 | 0.179159 | `azmcp_search_service_list` | ❌ |
| 16 | 0.166181 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.163051 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 18 | 0.162163 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.153536 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.152762 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 4

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695202 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.532985 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.526528 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 4 | 0.433117 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.422758 | `azmcp_search_index_get` | ❌ |
| 6 | 0.412895 | `azmcp_search_service_list` | ❌ |
| 7 | 0.349507 | `azmcp_search_index_query` | ❌ |
| 8 | 0.329681 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.310470 | `azmcp_foundry_models_deployments_list` | ❌ |
| 10 | 0.309867 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.296877 | `azmcp_grafana_list` | ❌ |
| 12 | 0.291635 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.286150 | `azmcp_monitor_table_type_list` | ❌ |
| 14 | 0.279802 | `azmcp_keyvault_key_list` | ❌ |
| 15 | 0.270212 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.270162 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.267906 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.265680 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.264056 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.262242 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 5

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603396 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489311 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.473949 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.396819 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.374694 | `azmcp_search_index_get` | ❌ |
| 6 | 0.350751 | `azmcp_search_service_list` | ❌ |
| 7 | 0.341865 | `azmcp_search_index_query` | ❌ |
| 8 | 0.317997 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.310576 | `azmcp_foundry_models_deploy` | ❌ |
| 10 | 0.278147 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.276839 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.272237 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.256208 | `azmcp_grafana_list` | ❌ |
| 14 | 0.250335 | `azmcp_foundry_agents_connect` | ❌ |
| 15 | 0.233389 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.225181 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.224194 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.223814 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.223725 | `azmcp_monitor_metrics_definitions` | ❌ |
| 20 | 0.218011 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 6

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672577 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.564860 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.424557 | `azmcp_search_index_get` | ❌ |
| 4 | 0.397225 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.375275 | `azmcp_mysql_table_schema_schema` | ❌ |
| 6 | 0.363978 | `azmcp_kusto_table_schema` | ❌ |
| 7 | 0.358307 | `azmcp_postgres_table_schema_schema` | ❌ |
| 8 | 0.349967 | `azmcp_search_index_query` | ❌ |
| 9 | 0.347762 | `azmcp_foundry_models_list` | ❌ |
| 10 | 0.346933 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.326807 | `azmcp_search_service_list` | ❌ |
| 12 | 0.297822 | `azmcp_foundry_models_deploy` | ❌ |
| 13 | 0.295847 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.285931 | `azmcp_monitor_table_type_list` | ❌ |
| 15 | 0.276869 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.271427 | `azmcp_cloudarchitect_design` | ❌ |
| 17 | 0.266288 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.259298 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.253702 | `azmcp_grafana_list` | ❌ |
| 20 | 0.236805 | `azmcp_mysql_server_list` | ❌ |

---

## Test 7

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650269 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432777 | `azmcp_postgres_table_schema_schema` | ❌ |
| 3 | 0.415963 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.408356 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.398186 | `azmcp_mysql_table_schema_schema` | ❌ |
| 6 | 0.380030 | `azmcp_search_index_get` | ❌ |
| 7 | 0.352243 | `azmcp_postgres_server_config_config` | ❌ |
| 8 | 0.318537 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.312100 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.309927 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.286991 | `azmcp_mysql_server_config_config` | ❌ |
| 12 | 0.271701 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.257402 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.256359 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.249723 | `azmcp_aks_cluster_get` | ❌ |
| 16 | 0.249010 | `azmcp_search_index_query` | ❌ |
| 17 | 0.246861 | `azmcp_monitor_table_type_list` | ❌ |
| 18 | 0.242191 | `azmcp_mysql_server_param_param` | ❌ |
| 19 | 0.240003 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.239095 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 8

**Expected Tool:** `azmcp_foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.313217 | `azmcp_foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.282117 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.273661 | `azmcp_deploy_plan_get` | ❌ |
| 4 | 0.269548 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.268535 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.233994 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.222166 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.222142 | `azmcp_grafana_list` | ❌ |
| 9 | 0.221448 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.216957 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.216301 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 12 | 0.215502 | `azmcp_loadtesting_testrun_create` | ❌ |
| 13 | 0.209569 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.207907 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.207385 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.204420 | `azmcp_postgres_server_param_set` | ❌ |
| 17 | 0.195339 | `azmcp_workbooks_list` | ❌ |
| 18 | 0.192264 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.192190 | `azmcp_storage_account_create` | ❌ |
| 20 | 0.189998 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 9

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559509 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.549636 | `azmcp_foundry_models_list` | ❌ |
| 3 | 0.539695 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.533239 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.448711 | `azmcp_search_service_list` | ❌ |
| 6 | 0.434472 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 7 | 0.368467 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.334867 | `azmcp_grafana_list` | ❌ |
| 9 | 0.332002 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.328253 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 11 | 0.326660 | `azmcp_search_index_get` | ❌ |
| 12 | 0.320998 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.318854 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.309715 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.302336 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.301302 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.300357 | `azmcp_search_index_query` | ❌ |
| 18 | 0.289448 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.288248 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.285916 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 10

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518221 | `azmcp_foundry_models_list` | ❌ |
| 2 | 0.503424 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.488885 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 4 | 0.486395 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.401016 | `azmcp_search_service_list` | ❌ |
| 6 | 0.396422 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 7 | 0.328922 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.311230 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 9 | 0.305381 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.301514 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.298821 | `azmcp_search_index_query` | ❌ |
| 12 | 0.291181 | `azmcp_search_index_get` | ❌ |
| 13 | 0.286814 | `azmcp_grafana_list` | ❌ |
| 14 | 0.269912 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.254926 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.250392 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.246893 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.243193 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.236572 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.234075 | `azmcp_redis_cache_list` | ❌ |

---

## Test 11

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560022 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.491952 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.401146 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.387861 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.386180 | `azmcp_search_service_list` | ❌ |
| 6 | 0.346909 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.298694 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.290447 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 9 | 0.285437 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.277883 | `azmcp_grafana_list` | ❌ |
| 11 | 0.275228 | `azmcp_search_index_get` | ❌ |
| 12 | 0.273551 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.265730 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.255790 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.255760 | `azmcp_search_index_query` | ❌ |
| 16 | 0.252274 | `azmcp_postgres_database_list` | ❌ |
| 17 | 0.248620 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.248405 | `azmcp_mysql_table_list` | ❌ |
| 19 | 0.245193 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.235676 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 12

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574818 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.475138 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.430513 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.388967 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.356898 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.339069 | `azmcp_search_service_list` | ❌ |
| 7 | 0.299150 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 8 | 0.283250 | `azmcp_search_index_query` | ❌ |
| 9 | 0.279975 | `azmcp_foundry_agents_connect` | ❌ |
| 10 | 0.274019 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.266662 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.261777 | `azmcp_search_index_get` | ❌ |
| 13 | 0.260193 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.245943 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.244728 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.240277 | `azmcp_monitor_metrics_definitions` | ❌ |
| 17 | 0.234050 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.217331 | `azmcp_marketplace_product_list` | ❌ |
| 19 | 0.211456 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.207810 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681085 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544557 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.490625 | `azmcp_search_service_list` | ❌ |
| 4 | 0.466005 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.459609 | `azmcp_search_index_query` | ❌ |
| 6 | 0.388183 | `azmcp_loadtesting_testrun_get` | ❌ |
| 7 | 0.372330 | `azmcp_marketplace_product_get` | ❌ |
| 8 | 0.370915 | `azmcp_mysql_table_schema_schema` | ❌ |
| 9 | 0.358315 | `azmcp_kusto_cluster_get` | ❌ |
| 10 | 0.356793 | `azmcp_storage_blob_get` | ❌ |
| 11 | 0.356252 | `azmcp_sql_db_show` | ❌ |
| 12 | 0.354845 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.351730 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.351083 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.348263 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.337040 | `azmcp_keyvault_key_get` | ❌ |
| 17 | 0.333641 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.330046 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.329434 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.325015 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 14

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640215 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.620140 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561856 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.480817 | `azmcp_search_index_query` | ❌ |
| 5 | 0.453047 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.445467 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 7 | 0.439814 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.416404 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.409307 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.406531 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.397423 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.382791 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.378750 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.378297 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.375372 | `azmcp_foundry_models_deployments_list` | ❌ |
| 16 | 0.371098 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.369526 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.368931 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367804 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.367388 | `azmcp_redis_cache_list` | ❌ |

---

## Test 15

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620717 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562775 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561154 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.471415 | `azmcp_search_index_query` | ❌ |
| 5 | 0.463972 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.408569 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.402427 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.382742 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.372639 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.370331 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.367868 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.351788 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.351161 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.350044 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.347566 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.346994 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.341728 | `azmcp_foundry_models_list` | ❌ |
| 18 | 0.335748 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.332289 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.328039 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 16

**Expected Tool:** `azmcp_search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522765 | `azmcp_search_index_get` | ❌ |
| 2 | 0.515870 | `azmcp_search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497467 | `azmcp_search_service_list` | ❌ |
| 4 | 0.373917 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.372940 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.327095 | `azmcp_kusto_query` | ❌ |
| 7 | 0.322362 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.311044 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.306415 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.305939 | `azmcp_marketplace_product_list` | ❌ |
| 11 | 0.295413 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.291319 | `azmcp_foundry_agents_connect` | ❌ |
| 13 | 0.290809 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.288242 | `azmcp_foundry_models_deployments_list` | ❌ |
| 15 | 0.287501 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.283532 | `azmcp_foundry_models_list` | ❌ |
| 17 | 0.274984 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.270424 | `azmcp_monitor_table_list` | ❌ |
| 19 | 0.259765 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.244844 | `azmcp_kusto_sample` | ❌ |

---

## Test 17

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.520340 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.505875 | `azmcp_search_index_get` | ❌ |
| 4 | 0.500455 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.494272 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.493011 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.492228 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.486066 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.482464 | `azmcp_grafana_list` | ❌ |
| 10 | 0.477470 | `azmcp_subscription_list` | ❌ |
| 11 | 0.470384 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.470055 | `azmcp_marketplace_product_list` | ❌ |
| 13 | 0.454460 | `azmcp_foundry_models_deployments_list` | ❌ |
| 14 | 0.443495 | `azmcp_search_index_query` | ❌ |
| 15 | 0.431621 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.427923 | `azmcp_group_list` | ❌ |
| 17 | 0.425463 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.418042 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.417472 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.414984 | `azmcp_foundry_models_list` | ❌ |

---

## Test 18

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.479820 | `azmcp_search_index_get` | ❌ |
| 3 | 0.467337 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.453489 | `azmcp_marketplace_product_list` | ❌ |
| 5 | 0.448446 | `azmcp_search_index_query` | ❌ |
| 6 | 0.425939 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.419485 | `azmcp_marketplace_product_get` | ❌ |
| 8 | 0.412158 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.408456 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.400229 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.399822 | `azmcp_grafana_list` | ❌ |
| 12 | 0.397883 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.393795 | `azmcp_subscription_list` | ❌ |
| 14 | 0.390660 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.390559 | `azmcp_foundry_models_list` | ❌ |
| 16 | 0.389434 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.379805 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 18 | 0.376089 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.373489 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.360230 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 19

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553025 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.436160 | `azmcp_search_index_get` | ❌ |
| 3 | 0.417096 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.404758 | `azmcp_search_index_query` | ❌ |
| 5 | 0.344699 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.337215 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.322580 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 8 | 0.322540 | `azmcp_foundry_models_list` | ❌ |
| 9 | 0.300427 | `azmcp_marketplace_product_list` | ❌ |
| 10 | 0.292677 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.290214 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.289466 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 13 | 0.283366 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.282198 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.281672 | `azmcp_get_bestpractices_get` | ❌ |
| 16 | 0.281134 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.278574 | `azmcp_cloudarchitect_design` | ❌ |
| 18 | 0.278531 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.277732 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.275013 | `azmcp_sql_server_show` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.635612 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.492146 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.491380 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.473554 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.442214 | `azmcp_grafana_list` | ❌ |
| 7 | 0.441656 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.433637 | `azmcp_eventgrid_topic_list` | ❌ |
| 9 | 0.432238 | `azmcp_search_service_list` | ❌ |
| 10 | 0.427653 | `azmcp_subscription_list` | ❌ |
| 11 | 0.427413 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.423903 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.420794 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.412274 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.408599 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.389537 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.387414 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.385938 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.380818 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.370646 | `azmcp_postgres_server_config_config` | ❌ |

---

## Test 21

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634978 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.533500 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.425552 | `azmcp_appconfig_kv_show` | ❌ |
| 4 | 0.372683 | `azmcp_eventgrid_subscription_list` | ❌ |
| 5 | 0.372456 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.368731 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.359567 | `azmcp_postgres_server_config_config` | ❌ |
| 8 | 0.356514 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.355830 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.354747 | `azmcp_appconfig_kv_delete` | ❌ |
| 11 | 0.348603 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.344581 | `azmcp_marketplace_product_get` | ❌ |
| 13 | 0.341263 | `azmcp_grafana_list` | ❌ |
| 14 | 0.340769 | `azmcp_eventgrid_topic_list` | ❌ |
| 15 | 0.332824 | `azmcp_mysql_server_config_config` | ❌ |
| 16 | 0.325892 | `azmcp_subscription_list` | ❌ |
| 17 | 0.325530 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.318639 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.310432 | `azmcp_search_service_list` | ❌ |
| 20 | 0.292788 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 22

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565435 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.564746 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.414649 | `azmcp_appconfig_kv_show` | ❌ |
| 4 | 0.355916 | `azmcp_postgres_server_config_config` | ❌ |
| 5 | 0.348661 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.327234 | `azmcp_appconfig_kv_set` | ❌ |
| 7 | 0.289682 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 8 | 0.282153 | `azmcp_mysql_server_config_config` | ❌ |
| 9 | 0.272373 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.255774 | `azmcp_mysql_server_param_param` | ❌ |
| 11 | 0.250457 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.239130 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.236404 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.234890 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.233357 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.231649 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.228042 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.225910 | `azmcp_sql_db_update` | ❌ |
| 19 | 0.221645 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.221386 | `azmcp_postgres_database_list` | ❌ |

---

## Test 23

**Expected Tool:** `azmcp_appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618276 | `azmcp_appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.486587 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.424344 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.399542 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.392016 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.268822 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.262117 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.248752 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.240483 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.236177 | `azmcp_keyvault_key_get` | ❌ |
| 12 | 0.218487 | `azmcp_mysql_server_param_set` | ❌ |
| 13 | 0.218373 | `azmcp_sql_server_delete` | ❌ |
| 14 | 0.196121 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 15 | 0.194933 | `azmcp_sql_db_delete` | ❌ |
| 16 | 0.194831 | `azmcp_postgres_server_config_config` | ❌ |
| 17 | 0.183482 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.175403 | `azmcp_mysql_server_config_config` | ❌ |
| 19 | 0.173050 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.170178 | `azmcp_sql_db_rename` | ❌ |

---

## Test 24

**Expected Tool:** `azmcp_appconfig_kv_list`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.730827 | `azmcp_appconfig_kv_list` | ✅ **EXPECTED** |
| 2 | 0.595207 | `azmcp_appconfig_kv_show` | ❌ |
| 3 | 0.557907 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.531049 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.464582 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.438992 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.379951 | `azmcp_keyvault_admin_get` | ❌ |
| 8 | 0.377688 | `azmcp_postgres_server_config_config` | ❌ |
| 9 | 0.373892 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.337559 | `azmcp_keyvault_secret_list` | ❌ |
| 11 | 0.333387 | `azmcp_mysql_server_param_param` | ❌ |
| 12 | 0.327843 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.323691 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.317894 | `azmcp_mysql_server_config_config` | ❌ |
| 15 | 0.296258 | `azmcp_postgres_table_list` | ❌ |
| 16 | 0.292071 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.275558 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.266992 | `azmcp_postgres_database_list` | ❌ |
| 19 | 0.265991 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.264922 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 25

**Expected Tool:** `azmcp_appconfig_kv_list`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682207 | `azmcp_appconfig_kv_list` | ✅ **EXPECTED** |
| 2 | 0.606519 | `azmcp_appconfig_kv_show` | ❌ |
| 3 | 0.522426 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.512945 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.468503 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.457866 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.370520 | `azmcp_postgres_server_config_config` | ❌ |
| 8 | 0.362405 | `azmcp_keyvault_admin_get` | ❌ |
| 9 | 0.356793 | `azmcp_mysql_server_param_param` | ❌ |
| 10 | 0.317662 | `azmcp_mysql_server_config_config` | ❌ |
| 11 | 0.314774 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.304557 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.294807 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.288088 | `azmcp_mysql_server_param_set` | ❌ |
| 15 | 0.278909 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.269411 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.258688 | `azmcp_postgres_server_param_param` | ❌ |
| 18 | 0.249117 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.243859 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.238151 | `azmcp_sql_server_show` | ❌ |

---

## Test 26

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591237 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.508801 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.445551 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.431516 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.423635 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.373656 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.253705 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.251298 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.238544 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.238416 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.214150 | `azmcp_keyvault_key_get` | ❌ |
| 12 | 0.211331 | `azmcp_postgres_server_config_config` | ❌ |
| 13 | 0.210627 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.185382 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.163738 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.158946 | `azmcp_mysql_server_param_param` | ❌ |
| 17 | 0.154529 | `azmcp_postgres_server_param_param` | ❌ |
| 18 | 0.144377 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.139871 | `azmcp_mysql_server_config_config` | ❌ |
| 20 | 0.127535 | `azmcp_sql_db_rename` | ❌ |

---

## Test 27

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555699 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.541533 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.476497 | `azmcp_appconfig_kv_delete` | ❌ |
| 4 | 0.435748 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.425488 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.409406 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.272339 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.268062 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.259561 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.252818 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.237098 | `azmcp_mysql_server_param_set` | ❌ |
| 12 | 0.225350 | `azmcp_postgres_server_config_config` | ❌ |
| 13 | 0.190598 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.190136 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.185081 | `azmcp_postgres_server_param_set` | ❌ |
| 16 | 0.179797 | `azmcp_mysql_server_param_param` | ❌ |
| 17 | 0.171375 | `azmcp_mysql_server_config_config` | ❌ |
| 18 | 0.159767 | `azmcp_postgres_server_param_param` | ❌ |
| 19 | 0.149653 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.143564 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 28

**Expected Tool:** `azmcp_appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609635 | `azmcp_appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536497 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 3 | 0.518483 | `azmcp_appconfig_kv_list` | ❌ |
| 4 | 0.507151 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.505571 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.377919 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.360015 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.347009 | `azmcp_postgres_server_param_set` | ❌ |
| 9 | 0.311433 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.305955 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.276129 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.262919 | `azmcp_sql_db_update` | ❌ |
| 13 | 0.236837 | `azmcp_keyvault_secret_get` | ❌ |
| 14 | 0.213592 | `azmcp_mysql_server_param_param` | ❌ |
| 15 | 0.208947 | `azmcp_postgres_server_config_config` | ❌ |
| 16 | 0.193989 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.174508 | `azmcp_sql_db_rename` | ❌ |
| 18 | 0.167006 | `azmcp_postgres_server_param_param` | ❌ |
| 19 | 0.164376 | `azmcp_mysql_server_config_config` | ❌ |
| 20 | 0.137964 | `azmcp_storage_account_create` | ❌ |

---

## Test 29

**Expected Tool:** `azmcp_appconfig_kv_show`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603163 | `azmcp_appconfig_kv_list` | ❌ |
| 2 | 0.561544 | `azmcp_appconfig_kv_show` | ✅ **EXPECTED** |
| 3 | 0.449033 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.441708 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.437527 | `azmcp_appconfig_account_list` | ❌ |
| 6 | 0.416284 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.323452 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.301734 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.301200 | `azmcp_keyvault_secret_get` | ❌ |
| 10 | 0.291274 | `azmcp_postgres_server_config_config` | ❌ |
| 11 | 0.269453 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.259449 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.257662 | `azmcp_mysql_server_param_param` | ❌ |
| 14 | 0.229014 | `azmcp_mysql_server_config_config` | ❌ |
| 15 | 0.225063 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.217549 | `azmcp_postgres_server_param_param` | ❌ |
| 17 | 0.206318 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.201678 | `azmcp_mysql_server_param_set` | ❌ |
| 19 | 0.186511 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.185926 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 30

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.355635 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.329345 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.300703 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.257790 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.216077 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.206095 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.205255 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.193032 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 9 | 0.181187 | `azmcp_foundry_agents_evaluate` | ❌ |
| 10 | 0.178164 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.169553 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.163768 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.148018 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.141970 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.133096 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.128768 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.125735 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.120066 | `azmcp_mysql_table_schema_schema` | ❌ |
| 19 | 0.116209 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.111730 | `azmcp_redis_cache_list` | ❌ |

---

## Test 31

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.318608 | `azmcp_deploy_app_logs_get` | ❌ |
| 2 | 0.302557 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.255885 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.225972 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.222226 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.200402 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.199366 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.187043 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.172691 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.163364 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.162347 | `azmcp_foundry_agents_evaluate` | ❌ |
| 12 | 0.150964 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.150313 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.144054 | `azmcp_mysql_server_param_param` | ❌ |
| 15 | 0.133109 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.125941 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.118881 | `azmcp_mysql_table_schema_schema` | ❌ |
| 18 | 0.113012 | `azmcp_monitor_workspace_log_query` | ❌ |
| 19 | 0.107063 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.101816 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 32

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.257080 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 2 | 0.250546 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.215890 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.199067 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.188245 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.188040 | `azmcp_appservice_database_add` | ❌ |
| 7 | 0.179050 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.178879 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.159064 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.158196 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.156599 | `azmcp_search_service_list` | ❌ |
| 12 | 0.156168 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 13 | 0.153379 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.151665 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.146689 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.139604 | `azmcp_postgres_server_param_param` | ❌ |
| 17 | 0.130326 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.129424 | `azmcp_mysql_server_param_param` | ❌ |
| 19 | 0.126177 | `azmcp_search_index_get` | ❌ |
| 20 | 0.125602 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 33

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a database connection to my app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729071 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.398617 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.380126 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.368252 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.364437 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.361951 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.353953 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.348749 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.342711 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.342276 | `azmcp_sql_db_update` | ❌ |
| 11 | 0.334383 | `azmcp_sql_server_delete` | ❌ |
| 12 | 0.301680 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.298638 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.286125 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.281484 | `azmcp_loadtesting_testresource_create` | ❌ |
| 16 | 0.280123 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.272080 | `azmcp_keyvault_secret_create` | ❌ |
| 18 | 0.266255 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.264904 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.260527 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 34

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure a SQL Server database for app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612164 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.484217 | `azmcp_sql_db_update` | ❌ |
| 3 | 0.471103 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.454417 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.408878 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.405299 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.389143 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.381822 | `azmcp_mysql_server_config_config` | ❌ |
| 9 | 0.367325 | `azmcp_sql_server_delete` | ❌ |
| 10 | 0.366337 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.355360 | `azmcp_sql_server_list` | ❌ |
| 12 | 0.352234 | `azmcp_deploy_plan_get` | ❌ |
| 13 | 0.350562 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.340399 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.328997 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.322881 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.315986 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.304849 | `azmcp_loadtesting_test_create` | ❌ |
| 19 | 0.299644 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.295080 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 35

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a MySQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648464 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.418902 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.409593 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.397907 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.382602 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.351839 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.345861 | `azmcp_sql_db_update` | ❌ |
| 8 | 0.344869 | `azmcp_mysql_table_schema_schema` | ❌ |
| 9 | 0.335323 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.323158 | `azmcp_mysql_database_query` | ❌ |
| 11 | 0.320639 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.314492 | `azmcp_mysql_server_param_set` | ❌ |
| 13 | 0.297738 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.295428 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.279652 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.272652 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.272465 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.269851 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.269785 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.260792 | `azmcp_functionapp_get` | ❌ |

---

## Test 36

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a PostgreSQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579547 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449032 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.439503 | `azmcp_postgres_database_query` | ❌ |
| 4 | 0.409416 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.405339 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.399322 | `azmcp_postgres_server_param_set` | ❌ |
| 7 | 0.383424 | `azmcp_sql_db_create` | ❌ |
| 8 | 0.337025 | `azmcp_postgres_table_schema_schema` | ❌ |
| 9 | 0.331329 | `azmcp_sql_db_rename` | ❌ |
| 10 | 0.328746 | `azmcp_postgres_server_param_param` | ❌ |
| 11 | 0.305433 | `azmcp_sql_db_update` | ❌ |
| 12 | 0.289404 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.279683 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.258666 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.257753 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.254344 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.241610 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.234073 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.231834 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.223433 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 37

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a CosmosDB database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642912 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.476966 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.465566 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.431491 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.421263 | `azmcp_cosmos_database_container_list` | ❌ |
| 6 | 0.400244 | `azmcp_sql_db_update` | ❌ |
| 7 | 0.378272 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.374178 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.370033 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.362411 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.353048 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.352350 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.349430 | `azmcp_mysql_database_list` | ❌ |
| 14 | 0.326553 | `azmcp_sql_db_delete` | ❌ |
| 15 | 0.324949 | `azmcp_appconfig_kv_set` | ❌ |
| 16 | 0.314996 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.314468 | `azmcp_sql_server_delete` | ❌ |
| 18 | 0.314300 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.309056 | `azmcp_redis_cluster_database_list` | ❌ |
| 20 | 0.303033 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 38

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645385 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.489204 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.439947 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.423900 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.422245 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.394852 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.394440 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.381728 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.368741 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.360169 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.357287 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.349733 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.348345 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.348078 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.304408 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.281289 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.277271 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.274535 | `azmcp_appconfig_kv_set` | ❌ |
| 19 | 0.274222 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.266258 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 39

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665216 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.401714 | `azmcp_sql_db_rename` | ❌ |
| 3 | 0.371196 | `azmcp_sql_db_update` | ❌ |
| 4 | 0.369071 | `azmcp_sql_db_create` | ❌ |
| 5 | 0.332119 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.314270 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.312395 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.307420 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.304622 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.297194 | `azmcp_mysql_server_param_param` | ❌ |
| 11 | 0.294182 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.292606 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.285790 | `azmcp_postgres_server_param_set` | ❌ |
| 14 | 0.273579 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.269023 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.267621 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.267098 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.266587 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.260464 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.256168 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 40

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536761 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.408796 | `azmcp_sql_db_rename` | ❌ |
| 3 | 0.394572 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.391758 | `azmcp_sql_db_update` | ❌ |
| 5 | 0.318461 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.318263 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.305259 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.301240 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.298453 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.298122 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.297607 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.295912 | `azmcp_subscription_list` | ❌ |
| 13 | 0.294340 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.290182 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.280891 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.274754 | `azmcp_sql_db_delete` | ❌ |
| 17 | 0.274380 | `azmcp_sql_server_delete` | ❌ |
| 18 | 0.273681 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.272241 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.265452 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 41

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560268 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.426753 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.392376 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.361028 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.349556 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.346672 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.345100 | `azmcp_sql_db_update` | ❌ |
| 8 | 0.342276 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.339789 | `azmcp_sql_db_delete` | ❌ |
| 10 | 0.339459 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.330944 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.317003 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.292346 | `azmcp_sql_server_delete` | ❌ |
| 14 | 0.281774 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.277048 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.270323 | `azmcp_kusto_table_schema` | ❌ |
| 17 | 0.268258 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.263877 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.258869 | `azmcp_keyvault_certificate_create` | ❌ |
| 20 | 0.257068 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 42

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572473 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.445157 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.390478 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.385368 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.375286 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.357471 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.352902 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.345282 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.344858 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.330014 | `azmcp_search_service_list` | ❌ |
| 11 | 0.326046 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.297036 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.296190 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.268845 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.265961 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.263811 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.260352 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.258931 | `azmcp_monitor_table_list` | ❌ |
| 19 | 0.248396 | `azmcp_search_index_get` | ❌ |
| 20 | 0.245629 | `azmcp_redis_cache_list` | ❌ |

---

## Test 43

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696531 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.468384 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.452121 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.435241 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.424623 | `azmcp_search_service_list` | ❌ |
| 6 | 0.405506 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.404905 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.401105 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.393786 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.387112 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.380224 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.371626 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.367339 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.367243 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.362866 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.355398 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.339417 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.336775 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.334552 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.332236 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 44

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.479392 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.468847 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.467717 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.461695 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.451694 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.449821 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.446454 | `azmcp_search_service_list` | ❌ |
| 9 | 0.419715 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.417639 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.416057 | `azmcp_get_bestpractices_get` | ❌ |
| 12 | 0.415609 | `azmcp_monitor_metrics_definitions` | ❌ |
| 13 | 0.407442 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.401304 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.401135 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.398817 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.389849 | `azmcp_monitor_table_type_list` | ❌ |
| 18 | 0.388734 | `azmcp_group_list` | ❌ |
| 19 | 0.386954 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.385149 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 45

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.398251 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.383767 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.369053 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.366758 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.341619 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.325776 | `azmcp_deploy_iac_rules_get` | ❌ |
| 8 | 0.324433 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.321337 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.313491 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.287391 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.285234 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.262799 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.259246 | `azmcp_search_service_list` | ❌ |
| 15 | 0.254871 | `azmcp_search_index_query` | ❌ |
| 16 | 0.247065 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.233938 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.230120 | `azmcp_monitor_workspace_log_query` | ❌ |
| 19 | 0.229476 | `azmcp_mysql_server_config_config` | ❌ |
| 20 | 0.225298 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 46

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.541506 | `azmcp_search_service_list` | ❌ |
| 4 | 0.516129 | `azmcp_subscription_list` | ❌ |
| 5 | 0.514293 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.509386 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.503032 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.490776 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.487272 | `azmcp_storage_blob_container_get` | ❌ |
| 10 | 0.483500 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.482236 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.481761 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.480883 | `azmcp_group_list` | ❌ |
| 14 | 0.469958 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.466718 | `azmcp_aks_cluster_get` | ❌ |
| 16 | 0.462353 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.460523 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.460343 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.456503 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.454170 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 47

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.449620 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.382728 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.373107 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.372153 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.370858 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.364918 | `azmcp_search_service_list` | ❌ |
| 10 | 0.359177 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.354277 | `azmcp_storage_blob_container_create` | ❌ |
| 12 | 0.353778 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.353590 | `azmcp_subscription_list` | ❌ |
| 14 | 0.352818 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.349526 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.349291 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.348033 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.344749 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.344071 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.339252 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 48

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
| 10 | 0.440610 | `azmcp_subscription_list` | ❌ |
| 11 | 0.435835 | `azmcp_grafana_list` | ❌ |
| 12 | 0.435281 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.431745 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.430309 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.419749 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.404718 | `azmcp_group_list` | ❌ |
| 17 | 0.398557 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.395721 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.386496 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.364215 | `azmcp_mysql_server_list` | ❌ |

---

## Test 49

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `azmcp_acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476015 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.454931 | `azmcp_group_list` | ❌ |
| 5 | 0.454003 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.446008 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.428000 | `azmcp_workbooks_list` | ❌ |
| 8 | 0.423541 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421030 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.417327 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.411316 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.409133 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.403825 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.388773 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.378482 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.371025 | `azmcp_sql_elastic-pool_list` | ❌ |
| 17 | 0.370359 | `azmcp_redis_cluster_database_list` | ❌ |
| 18 | 0.356119 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.354145 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.352336 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 50

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.468028 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.449649 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.445716 | `azmcp_group_list` | ❌ |
| 6 | 0.416354 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.413975 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.413191 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.406554 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.403228 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.400209 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.389603 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.378353 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.369912 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.369779 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.367734 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.355657 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.354807 | `azmcp_loadtesting_testresource_list` | ❌ |
| 19 | 0.351410 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.350245 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 51

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
| 13 | 0.440212 | `azmcp_subscription_list` | ❌ |
| 14 | 0.437086 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.431019 | `azmcp_group_list` | ❌ |
| 16 | 0.414463 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.412547 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.405472 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.390890 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.377142 | `azmcp_mysql_database_list` | ❌ |

---

## Test 52

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546334 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.407973 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.399461 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.339307 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.326631 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.308650 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.306819 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.306442 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.302635 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.300174 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.295995 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.292155 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.290148 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.289864 | `azmcp_search_service_list` | ❌ |
| 16 | 0.283716 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.283390 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.282582 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.276498 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.272964 | `azmcp_redis_cluster_database_list` | ❌ |

---

## Test 53

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.433927 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.387931 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.370375 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.359617 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.356901 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.355328 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.351007 | `azmcp_redis_cluster_database_list` | ❌ |
| 10 | 0.347467 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.347084 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.340014 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.338553 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.337543 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.332857 | `azmcp_keyvault_key_list` | ❌ |
| 16 | 0.332785 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.332704 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.332572 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.330046 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.322287 | `azmcp_mysql_table_list` | ❌ |

---

## Test 54

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501842 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.418623 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.374025 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.359922 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.341511 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.335467 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.333318 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.324104 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.318706 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.316614 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.315414 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.311692 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.309627 | `azmcp_search_service_list` | ❌ |
| 15 | 0.306052 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.304725 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.303931 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.302428 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.300101 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.299303 | `azmcp_mysql_table_list` | ❌ |

---

## Test 55

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668480 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.615268 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.587838 | `azmcp_subscription_list` | ❌ |
| 5 | 0.560795 | `azmcp_search_service_list` | ❌ |
| 6 | 0.538321 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.528963 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.516914 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.502428 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.502199 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.499161 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.497679 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.487067 | `azmcp_group_list` | ❌ |
| 14 | 0.483046 | `azmcp_grafana_list` | ❌ |
| 15 | 0.474934 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.460181 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.459502 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.459002 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.457497 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.453975 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 56

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665447 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605357 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.571613 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.486033 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.436464 | `azmcp_subscription_list` | ❌ |
| 6 | 0.431496 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.428004 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.427709 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.408659 | `azmcp_search_service_list` | ❌ |
| 10 | 0.405748 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.397575 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.390141 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.389842 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.386108 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.383985 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.381323 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.379460 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.373667 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.367942 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.358376 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 57

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605125 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.566249 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.546497 | `azmcp_subscription_list` | ❌ |
| 5 | 0.530175 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.527812 | `azmcp_search_service_list` | ❌ |
| 7 | 0.488006 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.466414 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.457584 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.456219 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.455017 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.453626 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.441136 | `azmcp_grafana_list` | ❌ |
| 14 | 0.438277 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.437536 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.437026 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.434623 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.433094 | `azmcp_postgres_server_list` | ❌ |
| 19 | 0.428112 | `azmcp_group_list` | ❌ |
| 20 | 0.426516 | `azmcp_sql_db_list` | ❌ |

---

## Test 58

**Expected Tool:** `azmcp_cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.605253 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.566854 | `azmcp_cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 3 | 0.477874 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.447757 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.444835 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.429363 | `azmcp_search_service_list` | ❌ |
| 7 | 0.399756 | `azmcp_search_index_query` | ❌ |
| 8 | 0.378151 | `azmcp_kusto_query` | ❌ |
| 9 | 0.374844 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.372689 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.366467 | `azmcp_search_index_get` | ❌ |
| 12 | 0.358903 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.351331 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.341495 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.337927 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.335256 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.334389 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.331041 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.308694 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.302916 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 59

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852832 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681044 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.630659 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.581272 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.527459 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 6 | 0.486357 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.448957 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.447538 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.439770 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.427614 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.424294 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.422159 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.421535 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.420253 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.411949 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.392941 | `azmcp_postgres_database_list` | ❌ |
| 17 | 0.386670 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.383508 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.378191 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.372115 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 60

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789395 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.614220 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.562062 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.536876 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.521532 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 6 | 0.449120 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.411703 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.398064 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.397969 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.397755 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.395513 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.392763 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.386806 | `azmcp_redis_cluster_database_list` | ❌ |
| 14 | 0.356217 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.355640 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.345665 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.343034 | `azmcp_monitor_table_list` | ❌ |
| 18 | 0.325994 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.319603 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.318550 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 61

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815649 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668560 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.665337 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.573700 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.571319 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.548069 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.525965 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.501441 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.471462 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.459243 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.450838 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.442596 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.418896 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.407789 | `azmcp_search_service_list` | ❌ |
| 15 | 0.406856 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.405821 | `azmcp_keyvault_key_list` | ❌ |
| 17 | 0.401861 | `azmcp_subscription_list` | ❌ |
| 18 | 0.397663 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.388676 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.387539 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 62

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
| 6 | 0.498206 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.497414 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.449759 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.447877 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.437993 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.408605 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.402767 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.396398 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.383824 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.379009 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.369344 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.348999 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.344442 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.342424 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.339516 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 63

**Expected Tool:** `azmcp_kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482148 | `azmcp_kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.457669 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.416762 | `azmcp_redis_cluster_database_list` | ❌ |
| 4 | 0.380557 | `azmcp_aks_cluster_get` | ❌ |
| 5 | 0.361772 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.353792 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.351447 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.344871 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.344590 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.333244 | `azmcp_mysql_table_schema_schema` | ❌ |
| 11 | 0.332639 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.326472 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.326335 | `azmcp_search_index_get` | ❌ |
| 14 | 0.318755 | `azmcp_kusto_query` | ❌ |
| 15 | 0.318517 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.318082 | `azmcp_mysql_server_config_config` | ❌ |
| 17 | 0.314722 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.302084 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.301024 | `azmcp_grafana_list` | ❌ |
| 20 | 0.300008 | `azmcp_kusto_table_list` | ❌ |

---

## Test 64

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.651218 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.644037 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.549094 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.509397 | `azmcp_grafana_list` | ❌ |
| 5 | 0.505912 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.492107 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.491313 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.491278 | `azmcp_search_service_list` | ❌ |
| 9 | 0.487583 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.486159 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.460255 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.458754 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.451500 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.427859 | `azmcp_subscription_list` | ❌ |
| 15 | 0.420174 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.412631 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.411911 | `azmcp_group_list` | ❌ |
| 18 | 0.410016 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.399535 | `azmcp_monitor_table_list` | ❌ |
| 20 | 0.391278 | `azmcp_monitor_table_type_list` | ❌ |

---

## Test 65

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
| 6 | 0.336474 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.303083 | `azmcp_grafana_list` | ❌ |
| 8 | 0.292994 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.292838 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.287768 | `azmcp_kusto_sample` | ❌ |
| 11 | 0.285603 | `azmcp_kusto_query` | ❌ |
| 12 | 0.283331 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.279848 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.277014 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.275559 | `azmcp_mysql_database_query` | ❌ |
| 16 | 0.271243 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.265906 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.264134 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.264035 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.263226 | `azmcp_quota_usage_check` | ❌ |

---

## Test 66

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584053 | `azmcp_redis_cluster_list` | ❌ |
| 2 | 0.549797 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 3 | 0.469570 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.464294 | `azmcp_kusto_database_list` | ❌ |
| 5 | 0.462945 | `azmcp_grafana_list` | ❌ |
| 6 | 0.446124 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.443171 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.440326 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.434016 | `azmcp_search_service_list` | ❌ |
| 10 | 0.432048 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.406863 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.396253 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.392541 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.386776 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.380006 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.377490 | `azmcp_kusto_query` | ❌ |
| 17 | 0.371188 | `azmcp_subscription_list` | ❌ |
| 18 | 0.368890 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.365323 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.356138 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 67

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628129 | `azmcp_redis_cluster_database_list` | ❌ |
| 2 | 0.610646 | `azmcp_kusto_database_list` | ✅ **EXPECTED** |
| 3 | 0.553214 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.549673 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.517039 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.474354 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.461496 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.459180 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.434330 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.431669 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.419528 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.403955 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.396061 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.375535 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.363663 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.363266 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.340095 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.334270 | `azmcp_grafana_list` | ❌ |
| 19 | 0.320622 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.318850 | `azmcp_kusto_query` | ❌ |

---

## Test 68

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.597975 | `azmcp_redis_cluster_database_list` | ❌ |
| 2 | 0.558503 | `azmcp_kusto_database_list` | ✅ **EXPECTED** |
| 3 | 0.497144 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.491400 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.486699 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.440064 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.427251 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.422588 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.391411 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.383664 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.368012 | `azmcp_postgres_table_list` | ❌ |
| 12 | 0.362905 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.359471 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.344011 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.336375 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.336104 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.334889 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.318196 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.309809 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.305756 | `azmcp_kusto_query` | ❌ |

---

## Test 69

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.381352 | `azmcp_kusto_query` | ✅ **EXPECTED** |
| 2 | 0.363594 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.363252 | `azmcp_kusto_sample` | ❌ |
| 4 | 0.349286 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.345798 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.334762 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.328646 | `azmcp_search_service_list` | ❌ |
| 8 | 0.328162 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.324763 | `azmcp_mysql_table_schema_schema` | ❌ |
| 10 | 0.319112 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.318976 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.314988 | `azmcp_monitor_table_type_list` | ❌ |
| 13 | 0.314919 | `azmcp_search_index_query` | ❌ |
| 14 | 0.308113 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.304014 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.302894 | `azmcp_postgres_table_list` | ❌ |
| 17 | 0.292086 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.264026 | `azmcp_grafana_list` | ❌ |
| 19 | 0.263085 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.254418 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 70

**Expected Tool:** `azmcp_kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537154 | `azmcp_kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.419540 | `azmcp_kusto_table_schema` | ❌ |
| 3 | 0.391595 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.391423 | `azmcp_kusto_table_list` | ❌ |
| 5 | 0.380691 | `azmcp_mysql_table_schema_schema` | ❌ |
| 6 | 0.377056 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.364629 | `azmcp_postgres_table_schema_schema` | ❌ |
| 8 | 0.364361 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.361845 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.343675 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.341941 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.337281 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.319239 | `azmcp_kusto_query` | ❌ |
| 14 | 0.318189 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.310196 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.285941 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.282651 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.252004 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.242112 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.240744 | `azmcp_grafana_list` | ❌ |

---

## Test 71

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591668 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585237 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.556724 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.550038 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.521516 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.520802 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.475482 | `azmcp_postgres_database_list` | ❌ |
| 8 | 0.464359 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.449728 | `azmcp_kusto_table_schema` | ❌ |
| 10 | 0.436518 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.433775 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.429278 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.412275 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.410425 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.400099 | `azmcp_mysql_table_schema_schema` | ❌ |
| 16 | 0.384948 | `azmcp_postgres_table_schema_schema` | ❌ |
| 17 | 0.380671 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.337427 | `azmcp_kusto_query` | ❌ |
| 19 | 0.329669 | `azmcp_grafana_list` | ❌ |
| 20 | 0.317903 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 72

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549884 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.524691 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.523432 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.494108 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.490942 | `azmcp_monitor_table_list` | ❌ |
| 6 | 0.475412 | `azmcp_kusto_database_list` | ❌ |
| 7 | 0.466300 | `azmcp_kusto_table_schema` | ❌ |
| 8 | 0.431976 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.425623 | `azmcp_kusto_sample` | ❌ |
| 10 | 0.421371 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.418153 | `azmcp_mysql_table_schema_schema` | ❌ |
| 12 | 0.415682 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.403445 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.402688 | `azmcp_postgres_table_schema_schema` | ❌ |
| 15 | 0.391081 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.367187 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.348891 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.330383 | `azmcp_kusto_query` | ❌ |
| 19 | 0.314766 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.298584 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 73

**Expected Tool:** `azmcp_kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587981 | `azmcp_kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.563452 | `azmcp_postgres_table_schema_schema` | ❌ |
| 3 | 0.527350 | `azmcp_mysql_table_schema_schema` | ❌ |
| 4 | 0.444729 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.437292 | `azmcp_kusto_table_list` | ❌ |
| 6 | 0.432440 | `azmcp_kusto_sample` | ❌ |
| 7 | 0.413732 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.398346 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.386654 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.366382 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.366207 | `azmcp_monitor_table_type_list` | ❌ |
| 12 | 0.358120 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.345423 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.343767 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.339948 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.315377 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.298721 | `azmcp_kusto_query` | ❌ |
| 18 | 0.294768 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.283144 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.275850 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 74

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634060 | `azmcp_postgres_database_list` | ❌ |
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
| 12 | 0.406803 | `azmcp_mysql_table_schema_schema` | ❌ |
| 13 | 0.338476 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.327614 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.317875 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.284786 | `azmcp_grafana_list` | ❌ |
| 17 | 0.278428 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.270729 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.268856 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.266185 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 75

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588121 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.574057 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.483855 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.463244 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.448169 | `azmcp_redis_cluster_database_list` | ❌ |
| 6 | 0.444547 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.415119 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.405492 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.404871 | `azmcp_mysql_table_schema_schema` | ❌ |
| 10 | 0.384974 | `azmcp_postgres_table_list` | ❌ |
| 11 | 0.384778 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.380422 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.297709 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.290592 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.259334 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.251227 | `azmcp_appservice_database_add` | ❌ |
| 17 | 0.247558 | `azmcp_grafana_list` | ❌ |
| 18 | 0.239544 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.236450 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.236206 | `azmcp_acr_registry_list` | ❌ |

---

## Test 76

**Expected Tool:** `azmcp_mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476423 | `azmcp_mysql_table_list` | ❌ |
| 2 | 0.455770 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.433392 | `azmcp_mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419859 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.409445 | `azmcp_mysql_table_schema_schema` | ❌ |
| 6 | 0.393854 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.345179 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.328744 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.320053 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.298681 | `azmcp_mysql_server_param_param` | ❌ |
| 11 | 0.291451 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.285803 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.279005 | `azmcp_kusto_query` | ❌ |
| 14 | 0.278067 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.264434 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.257657 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.230415 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.226605 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.225958 | `azmcp_grafana_list` | ❌ |
| 20 | 0.198397 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 77

**Expected Tool:** `azmcp_mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531887 | `azmcp_postgres_server_config_config` | ❌ |
| 2 | 0.489816 | `azmcp_mysql_server_config_config` | ❌ |
| 3 | 0.485952 | `azmcp_mysql_server_param_set` | ❌ |
| 4 | 0.476863 | `azmcp_mysql_server_param_param` | ❌ |
| 5 | 0.426507 | `azmcp_mysql_table_schema_schema` | ❌ |
| 6 | 0.413226 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.398405 | `azmcp_sql_server_show` | ❌ |
| 8 | 0.391644 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.376750 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.374870 | `azmcp_postgres_server_param_param` | ❌ |
| 11 | 0.267912 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.252810 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.238601 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.232680 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.224212 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.214473 | `azmcp_appservice_database_add` | ❌ |
| 17 | 0.180063 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.166209 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.165443 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.164733 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 78

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
| 6 | 0.478541 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.467807 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.458406 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.457318 | `azmcp_grafana_list` | ❌ |
| 10 | 0.451938 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.431642 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.431126 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.422584 | `azmcp_search_service_list` | ❌ |
| 14 | 0.410134 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.379322 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.377511 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.374451 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.365605 | `azmcp_group_list` | ❌ |
| 19 | 0.354490 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.344742 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 79

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478518 | `azmcp_mysql_database_list` | ❌ |
| 2 | 0.474586 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.412380 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.389949 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.377048 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.372766 | `azmcp_mysql_table_schema_schema` | ❌ |
| 8 | 0.363906 | `azmcp_mysql_server_param_param` | ❌ |
| 9 | 0.355142 | `azmcp_postgres_server_config_config` | ❌ |
| 10 | 0.337771 | `azmcp_mysql_server_config_config` | ❌ |
| 11 | 0.281558 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.251411 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.248026 | `azmcp_grafana_list` | ❌ |
| 14 | 0.248003 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.241497 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.235455 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.232383 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.224586 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.223351 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.218116 | `azmcp_acr_registry_list` | ❌ |

---

## Test 80

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.534266 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530210 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.464360 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.458499 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.456616 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.441836 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.431914 | `azmcp_grafana_list` | ❌ |
| 9 | 0.419663 | `azmcp_search_service_list` | ❌ |
| 10 | 0.416021 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.412407 | `azmcp_mysql_database_query` | ❌ |
| 12 | 0.408235 | `azmcp_mysql_table_schema_schema` | ❌ |
| 13 | 0.399358 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.376596 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.364016 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.356691 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.341439 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.341087 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.337333 | `azmcp_eventgrid_subscription_list` | ❌ |
| 20 | 0.334813 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 81

**Expected Tool:** `azmcp_mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `azmcp_mysql_server_param_param` | ❌ |
| 2 | 0.407671 | `azmcp_mysql_server_param_set` | ❌ |
| 3 | 0.333841 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.313150 | `azmcp_mysql_table_schema_schema` | ❌ |
| 5 | 0.310834 | `azmcp_postgres_server_param_param` | ❌ |
| 6 | 0.300031 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.296654 | `azmcp_mysql_server_config_config` | ❌ |
| 8 | 0.292546 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.286073 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.285645 | `azmcp_postgres_server_config_config` | ❌ |
| 11 | 0.241196 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.183743 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.160053 | `azmcp_appconfig_kv_list` | ❌ |
| 14 | 0.150796 | `azmcp_keyvault_secret_get` | ❌ |
| 15 | 0.146290 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.138668 | `azmcp_keyvault_admin_get` | ❌ |
| 17 | 0.124274 | `azmcp_grafana_list` | ❌ |
| 18 | 0.121605 | `azmcp_foundry_agents_connect` | ❌ |
| 19 | 0.120498 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 20 | 0.118505 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.390762 | `azmcp_mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `azmcp_mysql_server_param_param` | ❌ |
| 3 | 0.307798 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.298911 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.277569 | `azmcp_appservice_database_add` | ❌ |
| 6 | 0.254180 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.253189 | `azmcp_mysql_table_schema_schema` | ❌ |
| 8 | 0.246424 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.246019 | `azmcp_mysql_server_config_config` | ❌ |
| 10 | 0.238742 | `azmcp_postgres_server_config_config` | ❌ |
| 11 | 0.236453 | `azmcp_postgres_server_param_param` | ❌ |
| 12 | 0.140552 | `azmcp_foundry_agents_connect` | ❌ |
| 13 | 0.112499 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.094606 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.091971 | `azmcp_keyvault_admin_get` | ❌ |
| 16 | 0.090695 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.090334 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.089514 | `azmcp_appconfig_kv_show` | ❌ |
| 19 | 0.088097 | `azmcp_loadtesting_test_create` | ❌ |
| 20 | 0.086308 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 83

**Expected Tool:** `azmcp_mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633448 | `azmcp_mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573844 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.550907 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.546963 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.475178 | `azmcp_mysql_table_schema_schema` | ❌ |
| 6 | 0.447284 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.442053 | `azmcp_kusto_table_list` | ❌ |
| 8 | 0.429975 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.419030 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.410273 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.401216 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.393205 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.361477 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.335069 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.308442 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.268415 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.260118 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.253046 | `azmcp_grafana_list` | ❌ |
| 19 | 0.241294 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.239223 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 84

**Expected Tool:** `azmcp_mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609131 | `azmcp_mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.526236 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.525709 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.507258 | `azmcp_mysql_table_schema_schema` | ❌ |
| 5 | 0.498004 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.439004 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.419905 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.403265 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.385207 | `azmcp_postgres_table_schema_schema` | ❌ |
| 10 | 0.382774 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.378011 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.349435 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.343015 | `azmcp_kusto_table_schema` | ❌ |
| 14 | 0.319674 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.303999 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.281571 | `azmcp_kusto_sample` | ❌ |
| 17 | 0.236723 | `azmcp_grafana_list` | ❌ |
| 18 | 0.231173 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.225827 | `azmcp_appservice_database_add` | ❌ |
| 20 | 0.214496 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 85

**Expected Tool:** `azmcp_mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630623 | `azmcp_mysql_table_schema_schema` | ❌ |
| 2 | 0.558332 | `azmcp_postgres_table_schema_schema` | ❌ |
| 3 | 0.545025 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.482619 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.457739 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.443955 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.407451 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.398067 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.372911 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.348909 | `azmcp_mysql_server_config_config` | ❌ |
| 11 | 0.347368 | `azmcp_postgres_server_config_config` | ❌ |
| 12 | 0.324675 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.307950 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.271938 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.268273 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.243861 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.239328 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.208768 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.202788 | `azmcp_bicepschema_get` | ❌ |
| 20 | 0.194220 | `azmcp_grafana_list` | ❌ |

---

## Test 86

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815618 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.644014 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.622790 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.542685 | `azmcp_postgres_server_config_config` | ❌ |
| 5 | 0.490904 | `azmcp_postgres_server_param_param` | ❌ |
| 6 | 0.471672 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.453436 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.444410 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.435828 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.418295 | `azmcp_postgres_database_query` | ❌ |
| 11 | 0.414726 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.407877 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.319946 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.293787 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.292441 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.289334 | `azmcp_grafana_list` | ❌ |
| 17 | 0.252438 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.249563 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.245546 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.245483 | `azmcp_group_list` | ❌ |

---

## Test 87

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760002 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589784 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.585891 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.552660 | `azmcp_postgres_server_config_config` | ❌ |
| 5 | 0.495629 | `azmcp_postgres_server_param_param` | ❌ |
| 6 | 0.452128 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.433860 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.430652 | `azmcp_postgres_table_schema_schema` | ❌ |
| 9 | 0.426783 | `azmcp_postgres_database_query` | ❌ |
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
| 20 | 0.222587 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 88

**Expected Tool:** `azmcp_postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546179 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.503267 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.466599 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.415767 | `azmcp_postgres_database_query` | ✅ **EXPECTED** |
| 5 | 0.403969 | `azmcp_postgres_server_param_param` | ❌ |
| 6 | 0.403924 | `azmcp_postgres_server_config_config` | ❌ |
| 7 | 0.380545 | `azmcp_postgres_table_schema_schema` | ❌ |
| 8 | 0.361081 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.354774 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.341271 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.264914 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.262356 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.262160 | `azmcp_kusto_query` | ❌ |
| 14 | 0.254174 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.248628 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.244295 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.236363 | `azmcp_grafana_list` | ❌ |
| 18 | 0.218763 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.217855 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.189002 | `azmcp_foundry_models_list` | ❌ |

---

## Test 89

**Expected Tool:** `azmcp_postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `azmcp_postgres_server_config_config` | ❌ |
| 2 | 0.599471 | `azmcp_postgres_server_param_param` | ❌ |
| 3 | 0.535692 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.535011 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.518574 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.463172 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.431536 | `azmcp_postgres_table_schema_schema` | ❌ |
| 8 | 0.394582 | `azmcp_postgres_database_query` | ❌ |
| 9 | 0.356774 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.337899 | `azmcp_mysql_server_config_config` | ❌ |
| 11 | 0.269256 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.233426 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.222849 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.220186 | `azmcp_loadtesting_test_get` | ❌ |
| 15 | 0.208324 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.185547 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.178187 | `azmcp_appservice_database_add` | ❌ |
| 18 | 0.177778 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.168411 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.160792 | `azmcp_grafana_list` | ❌ |

---

## Test 90

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900023 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640695 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.565914 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.538997 | `azmcp_postgres_server_config_config` | ❌ |
| 5 | 0.507621 | `azmcp_postgres_server_param_param` | ❌ |
| 6 | 0.483663 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.472458 | `azmcp_grafana_list` | ❌ |
| 8 | 0.457583 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.453841 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.446509 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.435298 | `azmcp_search_service_list` | ❌ |
| 12 | 0.416315 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.406617 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.397428 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.389191 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.373699 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.373641 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.366102 | `azmcp_group_list` | ❌ |
| 19 | 0.362956 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.351894 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 91

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.607016 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.576348 | `azmcp_postgres_server_config_config` | ❌ |
| 4 | 0.522995 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.506171 | `azmcp_postgres_server_param_param` | ❌ |
| 6 | 0.409363 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.400256 | `azmcp_postgres_server_param_set` | ❌ |
| 8 | 0.373006 | `azmcp_postgres_table_schema_schema` | ❌ |
| 9 | 0.336934 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.336270 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.274763 | `azmcp_grafana_list` | ❌ |
| 12 | 0.260533 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.253264 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.241835 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.239500 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.238588 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.229842 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.227547 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.225295 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.219273 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 92

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579179 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.531804 | `azmcp_postgres_server_config_config` | ❌ |
| 4 | 0.514445 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.505869 | `azmcp_postgres_server_param_param` | ❌ |
| 6 | 0.452608 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.444127 | `azmcp_grafana_list` | ❌ |
| 8 | 0.430033 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.421577 | `azmcp_search_service_list` | ❌ |
| 10 | 0.414695 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.410688 | `azmcp_postgres_database_query` | ❌ |
| 12 | 0.403538 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.376954 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.367001 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.362650 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.362557 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.358408 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.334680 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.334101 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.332073 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 93

**Expected Tool:** `azmcp_postgres_server_param`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594753 | `azmcp_postgres_server_param_param` | ❌ |
| 2 | 0.539671 | `azmcp_postgres_server_config_config` | ❌ |
| 3 | 0.489693 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.481196 | `azmcp_postgres_server_param_set` | ❌ |
| 5 | 0.451798 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.357606 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.343799 | `azmcp_mysql_server_param_param` | ❌ |
| 8 | 0.330904 | `azmcp_postgres_table_schema_schema` | ❌ |
| 9 | 0.305282 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.295439 | `azmcp_mysql_server_param_set` | ❌ |
| 11 | 0.185329 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.183436 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.174107 | `azmcp_grafana_list` | ❌ |
| 14 | 0.169190 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.166286 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.158090 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.155815 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.145056 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.141137 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 20 | 0.140195 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 94

**Expected Tool:** `azmcp_postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488474 | `azmcp_postgres_server_config_config` | ❌ |
| 2 | 0.469794 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.464876 | `azmcp_postgres_server_param_set` | ✅ **EXPECTED** |
| 4 | 0.447011 | `azmcp_postgres_server_param_param` | ❌ |
| 5 | 0.440722 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.354049 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.341539 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.317527 | `azmcp_postgres_table_schema_schema` | ❌ |
| 9 | 0.241642 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.227740 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.192554 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.133385 | `azmcp_kusto_sample` | ❌ |
| 13 | 0.127120 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.126424 | `azmcp_foundry_agents_evaluate` | ❌ |
| 15 | 0.123537 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.119027 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.118089 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.114978 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.113841 | `azmcp_grafana_list` | ❌ |
| 20 | 0.113259 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 95

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789883 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.750591 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.574931 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.519908 | `azmcp_postgres_table_schema_schema` | ❌ |
| 5 | 0.501400 | `azmcp_postgres_server_config_config` | ❌ |
| 6 | 0.477689 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.449146 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.432813 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.430171 | `azmcp_postgres_server_param_param` | ❌ |
| 10 | 0.396688 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.394477 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.373673 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.352211 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.308255 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.299785 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.257808 | `azmcp_grafana_list` | ❌ |
| 17 | 0.256245 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.249162 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.236948 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.229889 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 96

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736083 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.690065 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.558435 | `azmcp_postgres_table_schema_schema` | ❌ |
| 4 | 0.543331 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.521570 | `azmcp_postgres_server_config_config` | ❌ |
| 6 | 0.464882 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.457757 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.447184 | `azmcp_postgres_server_param_param` | ❌ |
| 9 | 0.390392 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.383179 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.371394 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.334928 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.315781 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.307262 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.272906 | `azmcp_kusto_sample` | ❌ |
| 16 | 0.266178 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.243542 | `azmcp_grafana_list` | ❌ |
| 18 | 0.207521 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.205679 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.202950 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 97

**Expected Tool:** `azmcp_postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714937 | `azmcp_postgres_table_schema_schema` | ❌ |
| 2 | 0.597846 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.574201 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.508082 | `azmcp_postgres_server_config_config` | ❌ |
| 5 | 0.480733 | `azmcp_mysql_table_schema_schema` | ❌ |
| 6 | 0.475769 | `azmcp_kusto_table_schema` | ❌ |
| 7 | 0.443816 | `azmcp_postgres_server_param_param` | ❌ |
| 8 | 0.442553 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.427478 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.406761 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.363340 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.322766 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.303748 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.253767 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.253353 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.239225 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.212206 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.201673 | `azmcp_grafana_list` | ❌ |
| 19 | 0.185109 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.184021 | `azmcp_appservice_database_add` | ❌ |

---

## Test 98

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686737 | `azmcp_deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471477 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.404882 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.392565 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.389163 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.354424 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.342643 | `azmcp_monitor_resource_log_query` | ❌ |
| 8 | 0.334992 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.334588 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.333624 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.327072 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.325559 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.314995 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.314889 | `azmcp_sql_db_create` | ❌ |
| 15 | 0.314233 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.312872 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.307314 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.297665 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.294663 | `azmcp_sql_server_list` | ❌ |
| 20 | 0.288981 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 99

**Expected Tool:** `azmcp_deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680035 | `azmcp_deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562585 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.505052 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.497362 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.435921 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.430764 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.417333 | `azmcp_get_bestpractices_get` | ❌ |
| 8 | 0.371127 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.343117 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.322230 | `azmcp_extension_azqr` | ❌ |
| 11 | 0.317906 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.284401 | `azmcp_mysql_table_schema_schema` | ❌ |
| 13 | 0.270092 | `azmcp_sql_db_create` | ❌ |
| 14 | 0.264933 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.264060 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.263521 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.255084 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.250629 | `azmcp_search_service_list` | ❌ |
| 19 | 0.248086 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.247818 | `azmcp_sql_server_show` | ❌ |

---

## Test 100

**Expected Tool:** `azmcp_deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529092 | `azmcp_deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.404829 | `azmcp_bicepschema_get` | ❌ |
| 3 | 0.391965 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.383210 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.341234 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.304570 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.278653 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.266629 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 9 | 0.266221 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.252977 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.236341 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.223979 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.219521 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.206928 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.202239 | `azmcp_mysql_table_schema_schema` | ❌ |
| 16 | 0.201288 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.195422 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.194571 | `azmcp_sql_db_create` | ❌ |
| 19 | 0.188615 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.177845 | `azmcp_storage_blob_get` | ❌ |

---

## Test 101

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638635 | `azmcp_deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499226 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.448917 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.382240 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.374817 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.373363 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.350101 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.338439 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.322906 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.297769 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.262768 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.240757 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.234494 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.230063 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.222459 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.212123 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.211103 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.203987 | `azmcp_sql_server_delete` | ❌ |
| 19 | 0.201214 | `azmcp_sql_db_rename` | ❌ |
| 20 | 0.198696 | `azmcp_mysql_server_list` | ❌ |

---

## Test 102

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688367 | `azmcp_deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587950 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.497470 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.432825 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.425393 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.421744 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.413718 | `azmcp_loadtesting_test_create` | ❌ |
| 9 | 0.393518 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.365874 | `azmcp_foundry_models_deploy` | ❌ |
| 11 | 0.344407 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.312840 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.300643 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.299552 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.296702 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.292585 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.277064 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.258195 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.252696 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.252241 | `azmcp_sql_db_rename` | ❌ |

---

## Test 103

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759251 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.610315 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.545540 | `azmcp_search_service_list` | ❌ |
| 4 | 0.514189 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.496614 | `azmcp_subscription_list` | ❌ |
| 6 | 0.496002 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 7 | 0.492754 | `azmcp_group_list` | ❌ |
| 8 | 0.485584 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.484509 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.480199 | `azmcp_eventgrid_events_publish` | ❌ |
| 11 | 0.475667 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.475056 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.472764 | `azmcp_grafana_list` | ❌ |
| 14 | 0.470300 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.442229 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.439820 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.438287 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.427536 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.422415 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.421784 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |

---

## Test 104

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691152 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.599956 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.478333 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.475119 | `azmcp_search_service_list` | ❌ |
| 5 | 0.463508 | `azmcp_eventgrid_events_publish` | ❌ |
| 6 | 0.450712 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.441607 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.437153 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.431346 | `azmcp_subscription_list` | ❌ |
| 10 | 0.430494 | `azmcp_grafana_list` | ❌ |
| 11 | 0.428437 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.424907 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.420072 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 14 | 0.419194 | `azmcp_group_list` | ❌ |
| 15 | 0.408708 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.399253 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.396758 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.390619 | `azmcp_servicebus_topic_details` | ❌ |
| 19 | 0.384757 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.381664 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 105

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759442 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.632794 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.526595 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `azmcp_search_service_list` | ❌ |
| 5 | 0.495814 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.494153 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.481439 | `azmcp_group_list` | ❌ |
| 8 | 0.481065 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.476808 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.476805 | `azmcp_subscription_list` | ❌ |
| 11 | 0.471888 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.468200 | `azmcp_grafana_list` | ❌ |
| 13 | 0.466774 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.464616 | `azmcp_eventgrid_events_publish` | ❌ |
| 15 | 0.445991 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.429646 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.428727 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.425386 | `azmcp_servicebus_topic_details` | ❌ |
| 19 | 0.421431 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.392039 | `azmcp_kusto_database_list` | ❌ |

---

## Test 106

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659252 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.618817 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.609175 | `azmcp_group_list` | ❌ |
| 4 | 0.514613 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.505966 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.491433 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.484746 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.475467 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.464233 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.460455 | `azmcp_search_service_list` | ❌ |
| 11 | 0.456540 | `azmcp_grafana_list` | ❌ |
| 12 | 0.455379 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.452988 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.448196 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.442914 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.442259 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.432333 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.423027 | `azmcp_postgres_server_list` | ❌ |
| 19 | 0.415440 | `azmcp_eventgrid_events_publish` | ❌ |
| 20 | 0.411811 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 107

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682970 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.637188 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.486216 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.486162 | `azmcp_eventgrid_events_publish` | ❌ |
| 5 | 0.480944 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.478026 | `azmcp_servicebus_topic_details` | ❌ |
| 7 | 0.457868 | `azmcp_search_service_list` | ❌ |
| 8 | 0.445138 | `azmcp_subscription_list` | ❌ |
| 9 | 0.435412 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.434659 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.422093 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.420976 | `azmcp_group_list` | ❌ |
| 13 | 0.417538 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.415223 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.408588 | `azmcp_grafana_list` | ❌ |
| 16 | 0.397665 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.392784 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.378136 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.376133 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.367406 | `azmcp_acr_registry_list` | ❌ |

---

## Test 108

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672522 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.656023 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.539977 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.498352 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.477876 | `azmcp_eventgrid_events_publish` | ❌ |
| 6 | 0.460145 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 7 | 0.444774 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.443349 | `azmcp_subscription_list` | ❌ |
| 9 | 0.438131 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.435639 | `azmcp_search_service_list` | ❌ |
| 11 | 0.434420 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.433482 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.431618 | `azmcp_grafana_list` | ❌ |
| 14 | 0.427056 | `azmcp_group_list` | ❌ |
| 15 | 0.419194 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.402159 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.398589 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.392822 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.386880 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.376197 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 109

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669236 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.663338 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.524933 | `azmcp_group_list` | ❌ |
| 4 | 0.488696 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484167 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 6 | 0.478967 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.474205 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.473544 | `azmcp_servicebus_topic_details` | ❌ |
| 9 | 0.470148 | `azmcp_eventgrid_events_publish` | ❌ |
| 10 | 0.465477 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.465098 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.462201 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.457114 | `azmcp_search_service_list` | ❌ |
| 14 | 0.452428 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.452191 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.443259 | `azmcp_subscription_list` | ❌ |
| 17 | 0.436651 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.436075 | `azmcp_grafana_list` | ❌ |
| 19 | 0.429208 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.412528 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 110

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593273 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.592262 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.525156 | `azmcp_subscription_list` | ❌ |
| 4 | 0.518858 | `azmcp_search_service_list` | ❌ |
| 5 | 0.509007 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.490371 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.489687 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.480579 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.475795 | `azmcp_group_list` | ❌ |
| 10 | 0.475207 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.473274 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.467172 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.460683 | `azmcp_grafana_list` | ❌ |
| 14 | 0.451759 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.439125 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.422468 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.415047 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.410171 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.403391 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.399352 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 111

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604322 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.600323 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.535955 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.518205 | `azmcp_subscription_list` | ❌ |
| 5 | 0.510216 | `azmcp_group_list` | ❌ |
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
| 16 | 0.428290 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.426897 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.411734 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.410745 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.387573 | `azmcp_kusto_database_list` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621513 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.557606 | `azmcp_group_list` | ❌ |
| 3 | 0.531331 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.504984 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.502308 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.487257 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.472283 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.467550 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.466908 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.464545 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.459530 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.457119 | `azmcp_grafana_list` | ❌ |
| 13 | 0.440510 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.438414 | `azmcp_subscription_list` | ❌ |
| 15 | 0.438218 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.435258 | `azmcp_search_service_list` | ❌ |
| 17 | 0.431166 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.423121 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.411672 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.411012 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 113

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653850 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.581690 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.480537 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.478495 | `azmcp_subscription_list` | ❌ |
| 5 | 0.476763 | `azmcp_search_service_list` | ❌ |
| 6 | 0.475482 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.471596 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.471384 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.466489 | `azmcp_group_list` | ❌ |
| 10 | 0.449893 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.449681 | `azmcp_grafana_list` | ❌ |
| 12 | 0.447080 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.446620 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.444645 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.437300 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.436793 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.436653 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.426251 | `azmcp_eventgrid_events_publish` | ❌ |
| 19 | 0.425231 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.420013 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755365 | `azmcp_eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.465489 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.412894 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.356875 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.345677 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.311108 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 7 | 0.309912 | `azmcp_kusto_table_schema` | ❌ |
| 8 | 0.268458 | `azmcp_workbooks_create` | ❌ |
| 9 | 0.266711 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.260034 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.257155 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.250633 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.247462 | `azmcp_mysql_table_schema_schema` | ❌ |
| 14 | 0.245718 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.242883 | `azmcp_keyvault_secret_create` | ❌ |
| 16 | 0.242594 | `azmcp_loadtesting_test_create` | ❌ |
| 17 | 0.239142 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.235830 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.233747 | `azmcp_sql_db_rename` | ❌ |
| 20 | 0.231667 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 115

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654648 | `azmcp_eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.515617 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.447461 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.391043 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.366337 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.315972 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 7 | 0.247158 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.246031 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.245854 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.244996 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.242968 | `azmcp_sql_db_update` | ❌ |
| 12 | 0.242591 | `azmcp_kusto_query` | ❌ |
| 13 | 0.240175 | `azmcp_sql_db_create` | ❌ |
| 14 | 0.237478 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.237082 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 16 | 0.235560 | `azmcp_subscription_list` | ❌ |
| 17 | 0.235304 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.234683 | `azmcp_keyvault_secret_create` | ❌ |
| 19 | 0.234358 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.229925 | `azmcp_sql_db_rename` | ❌ |

---

## Test 116

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600274 | `azmcp_eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.451234 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.428996 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.351749 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.349804 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.329602 | `azmcp_workbooks_create` | ❌ |
| 7 | 0.298300 | `azmcp_workbooks_list` | ❌ |
| 8 | 0.296131 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 9 | 0.292814 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.292618 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.291730 | `azmcp_sql_server_list` | ❌ |
| 12 | 0.289237 | `azmcp_group_list` | ❌ |
| 13 | 0.280315 | `azmcp_loadtesting_testresource_create` | ❌ |
| 14 | 0.279691 | `azmcp_functionapp_get` | ❌ |
| 15 | 0.264383 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.262213 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 17 | 0.260268 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.259705 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.250906 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.245914 | `azmcp_extension_azqr` | ❌ |

---

## Test 117

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659672 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447543 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.390394 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.380675 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.379349 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.376536 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.373586 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.353006 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.347135 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.346799 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.342613 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.341396 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.340800 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.338459 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.337480 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 16 | 0.333534 | `azmcp_extension_azqr` | ❌ |
| 17 | 0.328620 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.324011 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.322273 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.317116 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607226 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447400 | `azmcp_mysql_server_config_config` | ❌ |
| 3 | 0.424693 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.422336 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.407128 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.397977 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.392925 | `azmcp_appconfig_kv_list` | ❌ |
| 8 | 0.384151 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.383957 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.369436 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.367183 | `azmcp_mysql_server_param_param` | ❌ |
| 12 | 0.363405 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.361433 | `azmcp_deploy_plan_get` | ❌ |
| 14 | 0.353601 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.351942 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.342398 | `azmcp_postgres_server_config_config` | ❌ |
| 17 | 0.321697 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.315023 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.314100 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.312611 | `azmcp_sql_db_list` | ❌ |

---

## Test 119

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622427 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.460102 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.420189 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.390708 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.334473 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.322197 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.317583 | `azmcp_quota_usage_check` | ❌ |
| 8 | 0.317359 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.312732 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.311384 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.309942 | `azmcp_loadtesting_testrun_get` | ❌ |
| 12 | 0.305215 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.300530 | `azmcp_aks_cluster_get` | ❌ |
| 14 | 0.297747 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.295538 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.295392 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.295152 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 18 | 0.290156 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.281564 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.277653 | `azmcp_mysql_server_config_config` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691148 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433989 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.432317 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.424646 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.419375 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.416967 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.396163 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.390827 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.389322 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.387898 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.383857 | `azmcp_sql_server_list` | ❌ |
| 12 | 0.383191 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.378811 | `azmcp_get_bestpractices_get` | ❌ |
| 14 | 0.376019 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.375267 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.368506 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.353835 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.352505 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.348610 | `azmcp_foundry_models_deployments_list` | ❌ |
| 20 | 0.346266 | `azmcp_group_list` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592864 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
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
| 14 | 0.333000 | `azmcp_mysql_server_config_config` | ❌ |
| 15 | 0.331154 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.325417 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.320825 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.317600 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.315342 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.305800 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 122

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687787 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.445142 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.368188 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.366279 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.365569 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.363324 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.358418 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352754 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.350178 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.349596 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.349013 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.336938 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.335848 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.327763 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.325909 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.325899 | `azmcp_sql_server_list` | ❌ |
| 17 | 0.323655 | `azmcp_foundry_models_deployments_list` | ❌ |
| 18 | 0.323377 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.322984 | `azmcp_loadtesting_testrun_get` | ❌ |
| 20 | 0.319830 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 123

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645091 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433959 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.388678 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.369829 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.368425 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.368018 | `azmcp_loadtesting_testrun_get` | ❌ |
| 7 | 0.355956 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.355321 | `azmcp_search_index_get` | ❌ |
| 9 | 0.349891 | `azmcp_mysql_server_config_config` | ❌ |
| 10 | 0.349476 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.346959 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.343847 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.343381 | `azmcp_get_bestpractices_get` | ❌ |
| 14 | 0.342238 | `azmcp_servicebus_queue_details` | ❌ |
| 15 | 0.337577 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.334256 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.330450 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.326091 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.322671 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 20 | 0.321950 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 124

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555123 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426703 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.418362 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.407787 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.381067 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.364785 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.350663 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.335606 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.325271 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.321466 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.318517 | `azmcp_mysql_server_config_config` | ❌ |
| 12 | 0.313831 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.306310 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.304263 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.301769 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.281401 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.277967 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.267170 | `azmcp_search_service_list` | ❌ |
| 19 | 0.267141 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.266494 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 125

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565728 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.440329 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.422774 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.384159 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.342552 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.333621 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.319211 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.318076 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.310635 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.298434 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.296755 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.295694 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.292793 | `azmcp_cloudarchitect_design` | ❌ |
| 14 | 0.283654 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.272348 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.270845 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.267009 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.265973 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.258431 | `azmcp_search_service_list` | ❌ |
| 20 | 0.241875 | `azmcp_sql_db_list` | ❌ |

---

## Test 126

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646696 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.559382 | `azmcp_search_service_list` | ❌ |
| 3 | 0.516618 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.516217 | `azmcp_appconfig_account_list` | ❌ |
| 5 | 0.485322 | `azmcp_subscription_list` | ❌ |
| 6 | 0.474425 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.465690 | `azmcp_group_list` | ❌ |
| 8 | 0.464534 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.462296 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.455389 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.445099 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.442614 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.433274 | `azmcp_eventgrid_topic_list` | ❌ |
| 14 | 0.432144 | `azmcp_grafana_list` | ❌ |
| 15 | 0.431611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.429253 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.417070 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.415840 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.413034 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.411904 | `azmcp_sql_db_list` | ❌ |

---

## Test 127

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560085 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.452132 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.436167 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.412646 | `azmcp_search_service_list` | ❌ |
| 5 | 0.411323 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.385832 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.374655 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.372790 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.370393 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.369862 | `azmcp_subscription_list` | ❌ |
| 11 | 0.368007 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.358083 | `azmcp_deploy_plan_get` | ❌ |
| 13 | 0.357329 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.347887 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.347802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.339873 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.334019 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.333136 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.327870 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.327326 | `azmcp_sql_server_list` | ❌ |

---

## Test 128

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433598 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.348106 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.284362 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.281676 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.249658 | `azmcp_appconfig_account_list` | ❌ |
| 6 | 0.244888 | `azmcp_appconfig_kv_list` | ❌ |
| 7 | 0.240714 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.239514 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.217775 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.213974 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.207392 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.197655 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.195857 | `azmcp_role_assignment_list` | ❌ |
| 14 | 0.194503 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.186328 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.184120 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.184051 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.179069 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.178961 | `azmcp_search_service_list` | ❌ |
| 20 | 0.175295 | `azmcp_subscription_list` | ❌ |

---

## Test 129

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604780 | `azmcp_keyvault_admin_get` | ❌ |
| 2 | 0.542205 | `azmcp_appconfig_kv_show` | ❌ |
| 3 | 0.526236 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.520401 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.487928 | `azmcp_keyvault_secret_get` | ❌ |
| 6 | 0.466426 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.461201 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.452367 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.435720 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.423017 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.418397 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 12 | 0.406328 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.400540 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.367094 | `azmcp_mysql_server_param_param` | ❌ |
| 15 | 0.360747 | `azmcp_mysql_server_config_config` | ❌ |
| 16 | 0.355915 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.338699 | `azmcp_subscription_list` | ❌ |
| 18 | 0.323508 | `azmcp_search_index_get` | ❌ |
| 19 | 0.305008 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.296568 | `azmcp_storage_blob_get` | ❌ |

---

## Test 130

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671370 | `azmcp_keyvault_admin_get` | ❌ |
| 2 | 0.511478 | `azmcp_appconfig_kv_show` | ❌ |
| 3 | 0.453590 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.432814 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.420021 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.404666 | `azmcp_appconfig_kv_set` | ❌ |
| 7 | 0.400387 | `azmcp_keyvault_secret_get` | ❌ |
| 8 | 0.394832 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.389905 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.382164 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.379581 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.343204 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.331239 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.305474 | `azmcp_mysql_server_config_config` | ❌ |
| 15 | 0.301008 | `azmcp_mysql_server_param_param` | ❌ |
| 16 | 0.300465 | `azmcp_subscription_list` | ❌ |
| 17 | 0.288407 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.282658 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.279315 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.278782 | `azmcp_search_service_list` | ❌ |

---

## Test 131

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538250 | `azmcp_appconfig_kv_show` | ❌ |
| 2 | 0.505716 | `azmcp_keyvault_admin_get` | ❌ |
| 3 | 0.496489 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.445836 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.429768 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.420101 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.408155 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.391269 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.390408 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.384208 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.383307 | `azmcp_mysql_server_param_param` | ❌ |
| 12 | 0.379877 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.318317 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.303125 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.281027 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.273450 | `azmcp_mysql_server_config_config` | ❌ |
| 17 | 0.267424 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.264577 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.263270 | `azmcp_subscription_list` | ❌ |
| 20 | 0.248150 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 132

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.740326 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.595854 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.590531 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.575960 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.543219 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.526698 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.434682 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.416091 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.413986 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.380436 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.372045 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.352952 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.296674 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.285184 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.281804 | `azmcp_sql_db_rename` | ❌ |
| 16 | 0.267718 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.237081 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.222631 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.219634 | `azmcp_subscription_list` | ❌ |
| 20 | 0.217086 | `azmcp_search_service_list` | ❌ |

---

## Test 133

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628016 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.624457 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.565005 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.539554 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.515732 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.493432 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.483032 | `azmcp_keyvault_secret_get` | ❌ |
| 8 | 0.474864 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.423728 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.418861 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.359750 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.318291 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.293606 | `azmcp_subscription_list` | ❌ |
| 14 | 0.289685 | `azmcp_search_service_list` | ❌ |
| 15 | 0.279615 | `azmcp_search_index_get` | ❌ |
| 16 | 0.276581 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.271911 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.269735 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.266478 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.263141 | `azmcp_storage_account_create` | ❌ |

---

## Test 134

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662306 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.606534 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.574891 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.540155 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.535157 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.499272 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.481982 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.481388 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.459167 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.420223 | `azmcp_keyvault_admin_get` | ❌ |
| 11 | 0.417907 | `azmcp_storage_blob_container_get` | ❌ |
| 12 | 0.415722 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.368353 | `azmcp_search_index_get` | ❌ |
| 14 | 0.365386 | `azmcp_sql_db_show` | ❌ |
| 15 | 0.350912 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.332770 | `azmcp_mysql_server_config_config` | ❌ |
| 17 | 0.331645 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.317856 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.305937 | `azmcp_subscription_list` | ❌ |
| 20 | 0.301710 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 135

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649993 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.521183 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.469728 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.467097 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.426600 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.398035 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.386025 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.364868 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.354836 | `azmcp_keyvault_secret_get` | ❌ |
| 10 | 0.338062 | `azmcp_keyvault_secret_list` | ❌ |
| 11 | 0.248212 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.228508 | `azmcp_workbooks_delete` | ❌ |
| 13 | 0.222971 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.205023 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.181145 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.180219 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.180109 | `azmcp_sql_db_rename` | ❌ |
| 18 | 0.174606 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.170377 | `azmcp_subscription_list` | ❌ |
| 20 | 0.158491 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649676 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.629902 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.527468 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.525900 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.491898 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.472232 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.405961 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.399856 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.378111 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.371435 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.259893 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.259284 | `azmcp_sql_db_rename` | ❌ |
| 13 | 0.256832 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.250432 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.233767 | `azmcp_workbooks_delete` | ❌ |
| 16 | 0.210904 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.209234 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.203649 | `azmcp_sql_server_create` | ❌ |
| 19 | 0.197598 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.196937 | `azmcp_workbooks_create` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.762015 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.637437 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.608568 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.566405 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.539624 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.484660 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.484299 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.478100 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.453226 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.438227 | `azmcp_keyvault_admin_get` | ❌ |
| 11 | 0.408159 | `azmcp_subscription_list` | ❌ |
| 12 | 0.394434 | `azmcp_search_service_list` | ❌ |
| 13 | 0.393940 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.362996 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.362873 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.358938 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.350862 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.339860 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 19 | 0.336779 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 20 | 0.330749 | `azmcp_redis_cache_list` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660576 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.570238 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.540050 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.516256 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.509123 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.496874 | `azmcp_keyvault_key_get` | ❌ |
| 7 | 0.483404 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.458627 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.441425 | `azmcp_keyvault_admin_get` | ❌ |
| 10 | 0.420506 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.397031 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.362936 | `azmcp_subscription_list` | ❌ |
| 13 | 0.355052 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.344466 | `azmcp_search_service_list` | ❌ |
| 15 | 0.323177 | `azmcp_role_assignment_list` | ❌ |
| 16 | 0.309942 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.305651 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.295917 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.290644 | `azmcp_search_index_get` | ❌ |
| 20 | 0.286708 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676352 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.569250 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.555829 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.465742 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.458004 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.417395 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.413347 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.412581 | `azmcp_keyvault_certificate_import` | ❌ |
| 9 | 0.397141 | `azmcp_appconfig_kv_set` | ❌ |
| 10 | 0.389657 | `azmcp_keyvault_certificate_get` | ❌ |
| 11 | 0.372042 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.338097 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.283857 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.276139 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.261794 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.259835 | `azmcp_sql_db_rename` | ❌ |
| 17 | 0.230093 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.223719 | `azmcp_storage_blob_container_create` | ❌ |
| 19 | 0.215933 | `azmcp_subscription_list` | ❌ |
| 20 | 0.212003 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586316 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.554944 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.509271 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.500697 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.486714 | `azmcp_keyvault_certificate_list` | ❌ |
| 6 | 0.486385 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.484252 | `azmcp_keyvault_certificate_get` | ❌ |
| 8 | 0.439610 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.432313 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.423946 | `azmcp_keyvault_admin_get` | ❌ |
| 11 | 0.379569 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.328779 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.305874 | `azmcp_subscription_list` | ❌ |
| 14 | 0.281009 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.279316 | `azmcp_search_index_get` | ❌ |
| 16 | 0.276633 | `azmcp_search_service_list` | ❌ |
| 17 | 0.274427 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.268770 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.267669 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.256231 | `azmcp_quota_usage_check` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636333 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.545212 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.535014 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.504487 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.498875 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.478664 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.475152 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.470223 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.452395 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.450334 | `azmcp_keyvault_admin_get` | ❌ |
| 11 | 0.429437 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.428750 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.368743 | `azmcp_search_index_get` | ❌ |
| 14 | 0.347100 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.340626 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.337168 | `azmcp_servicebus_queue_details` | ❌ |
| 17 | 0.326326 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.316107 | `azmcp_subscription_list` | ❌ |
| 19 | 0.315915 | `azmcp_mysql_server_config_config` | ❌ |
| 20 | 0.311459 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737135 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.650024 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.631528 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.531109 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.498767 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.480129 | `azmcp_keyvault_admin_get` | ❌ |
| 7 | 0.468044 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.467326 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.463766 | `azmcp_keyvault_secret_get` | ❌ |
| 10 | 0.455641 | `azmcp_keyvault_certificate_get` | ❌ |
| 11 | 0.430322 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.426924 | `azmcp_subscription_list` | ❌ |
| 13 | 0.408341 | `azmcp_search_service_list` | ❌ |
| 14 | 0.387543 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.373903 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.368258 | `azmcp_mysql_database_list` | ❌ |
| 17 | 0.354984 | `azmcp_monitor_table_list` | ❌ |
| 18 | 0.353714 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.350154 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.348597 | `azmcp_role_assignment_list` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609392 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.562169 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.534913 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.520010 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.500593 | `azmcp_keyvault_secret_get` | ❌ |
| 6 | 0.479731 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.472465 | `azmcp_keyvault_admin_get` | ❌ |
| 8 | 0.462249 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.429515 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.421475 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.406776 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.356823 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.353506 | `azmcp_subscription_list` | ❌ |
| 14 | 0.327200 | `azmcp_search_service_list` | ❌ |
| 15 | 0.316124 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.308976 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.306567 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.296986 | `azmcp_search_index_get` | ❌ |
| 19 | 0.295954 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.293404 | `azmcp_quota_usage_check` | ❌ |

---

## Test 144

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.767701 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.613514 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.572297 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.531605 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.516678 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.461437 | `azmcp_appconfig_kv_set` | ❌ |
| 7 | 0.423999 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.417525 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.411481 | `azmcp_keyvault_certificate_import` | ❌ |
| 10 | 0.391024 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.387507 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 12 | 0.357221 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.288052 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.287941 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.287066 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.273512 | `azmcp_sql_db_rename` | ❌ |
| 17 | 0.246174 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.243337 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.218660 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 20 | 0.212873 | `azmcp_storage_blob_upload` | ❌ |

---

## Test 145

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618934 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.577817 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.549693 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.514888 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.482166 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.456696 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.442728 | `azmcp_keyvault_certificate_list` | ❌ |
| 8 | 0.423151 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.420937 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.414863 | `azmcp_keyvault_admin_get` | ❌ |
| 11 | 0.382573 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.348081 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.301642 | `azmcp_subscription_list` | ❌ |
| 14 | 0.294689 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.284184 | `azmcp_search_index_get` | ❌ |
| 16 | 0.281795 | `azmcp_search_service_list` | ❌ |
| 17 | 0.260730 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.257699 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.255279 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.254379 | `azmcp_sql_db_show` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607524 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.582782 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.564291 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.531971 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.503113 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.485180 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.483567 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.444683 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.444462 | `azmcp_keyvault_certificate_list` | ❌ |
| 10 | 0.437919 | `azmcp_keyvault_admin_get` | ❌ |
| 11 | 0.436786 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.413715 | `azmcp_keyvault_certificate_import` | ❌ |
| 13 | 0.378867 | `azmcp_search_index_get` | ❌ |
| 14 | 0.355162 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.346830 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.335079 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.333928 | `azmcp_servicebus_queue_details` | ❌ |
| 18 | 0.324284 | `azmcp_mysql_server_config_config` | ❌ |
| 19 | 0.321571 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.311614 | `azmcp_subscription_list` | ❌ |

---

## Test 147

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747274 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.617130 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.569911 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.536320 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.519133 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.473595 | `azmcp_keyvault_key_get` | ❌ |
| 7 | 0.455500 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.433185 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.427418 | `azmcp_keyvault_admin_get` | ❌ |
| 10 | 0.417973 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.391105 | `azmcp_subscription_list` | ❌ |
| 12 | 0.388773 | `azmcp_search_service_list` | ❌ |
| 13 | 0.387663 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.367012 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.340472 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 16 | 0.337595 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.334206 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.331203 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.326425 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.322010 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |

---

## Test 148

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.614855 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.577840 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.520896 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.520654 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.502403 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.467743 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.456291 | `azmcp_keyvault_certificate_get` | ❌ |
| 8 | 0.453130 | `azmcp_keyvault_admin_get` | ❌ |
| 9 | 0.411604 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.410929 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.401434 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.370860 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.345357 | `azmcp_subscription_list` | ❌ |
| 14 | 0.328354 | `azmcp_search_service_list` | ❌ |
| 15 | 0.305158 | `azmcp_search_index_get` | ❌ |
| 16 | 0.303769 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.299023 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.294614 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.293826 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.290273 | `azmcp_mysql_database_list` | ❌ |

---

## Test 149

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650418 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.558530 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.481416 | `azmcp_mysql_server_config_config` | ❌ |
| 4 | 0.463682 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.463065 | `azmcp_loadtesting_test_get` | ❌ |
| 6 | 0.430976 | `azmcp_postgres_server_config_config` | ❌ |
| 7 | 0.419629 | `azmcp_sql_server_show` | ❌ |
| 8 | 0.399345 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.391988 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.390959 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.390760 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.390141 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.371630 | `azmcp_mysql_server_param_param` | ❌ |
| 14 | 0.370169 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.369392 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.367841 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.360827 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.356990 | `azmcp_keyvault_key_get` | ❌ |
| 19 | 0.355388 | `azmcp_sql_server_list` | ❌ |
| 20 | 0.353948 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595077 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.508226 | `azmcp_kusto_cluster_get` | ❌ |
| 3 | 0.471692 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.461466 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.448796 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.428776 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.422993 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.413625 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.408421 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.396636 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.396256 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.390889 | `azmcp_sql_server_list` | ❌ |
| 13 | 0.385261 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.384654 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.382284 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.377705 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.371586 | `azmcp_group_list` | ❌ |
| 18 | 0.366113 | `azmcp_search_index_get` | ❌ |
| 19 | 0.362332 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.358237 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542755 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.464247 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.380301 | `azmcp_mysql_server_config_config` | ❌ |
| 4 | 0.368584 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.348885 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.342696 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.340294 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.334923 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.334860 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.329324 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.315228 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.314435 | `azmcp_appconfig_kv_list` | ❌ |
| 13 | 0.309845 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.300157 | `azmcp_foundry_agents_list` | ❌ |
| 15 | 0.299046 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.298637 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.296862 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.296593 | `azmcp_postgres_server_config_config` | ❌ |
| 19 | 0.289342 | `azmcp_mysql_server_param_param` | ❌ |
| 20 | 0.275751 | `azmcp_sql_db_show` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596102 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.503925 | `azmcp_kusto_cluster_get` | ❌ |
| 3 | 0.482616 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.434754 | `azmcp_functionapp_get` | ❌ |
| 5 | 0.433913 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.419339 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.418519 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.417836 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.405658 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.404921 | `azmcp_storage_blob_get` | ❌ |
| 11 | 0.402335 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.398686 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.391717 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.384782 | `azmcp_mysql_server_config_config` | ❌ |
| 15 | 0.383956 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.372812 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.367547 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.359877 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.357122 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.355599 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.723178 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.690255 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.599940 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.562043 | `azmcp_search_service_list` | ❌ |
| 5 | 0.544495 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.543684 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.515922 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.509202 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.502427 | `azmcp_subscription_list` | ❌ |
| 10 | 0.498286 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 11 | 0.498239 | `azmcp_group_list` | ❌ |
| 12 | 0.495977 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.486141 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.483592 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.482355 | `azmcp_acr_registry_list` | ❌ |
| 16 | 0.481469 | `azmcp_grafana_list` | ❌ |
| 17 | 0.464024 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.457949 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.452958 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.452681 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594886 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.492910 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.466970 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.455228 | `azmcp_search_service_list` | ❌ |
| 5 | 0.446270 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.428444 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.409711 | `azmcp_kusto_cluster_get` | ❌ |
| 8 | 0.408385 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.392997 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.376382 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 11 | 0.371809 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.371535 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.370889 | `azmcp_search_index_get` | ❌ |
| 14 | 0.370237 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.363878 | `azmcp_subscription_list` | ❌ |
| 16 | 0.362727 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.361928 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.358260 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.356926 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.356016 | `azmcp_storage_account_get` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593985 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.500397 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.449602 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.416564 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.392083 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.379421 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.378826 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.377567 | `azmcp_acr_registry_repository_list` | ❌ |
| 9 | 0.374585 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.364022 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.353365 | `azmcp_search_service_list` | ❌ |
| 12 | 0.345290 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.345241 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.341581 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.337354 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.336226 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.317977 | `azmcp_sql_elastic-pool_list` | ❌ |
| 18 | 0.317238 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.312405 | `azmcp_subscription_list` | ❌ |
| 20 | 0.311971 | `azmcp_quota_usage_check` | ❌ |

---

## Test 156

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681158 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.521913 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.482683 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.468392 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463192 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.434875 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.414751 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 8 | 0.401610 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.399425 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.384390 | `azmcp_keyvault_key_get` | ❌ |
| 11 | 0.383565 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 12 | 0.382352 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.380021 | `azmcp_storage_blob_get` | ❌ |
| 14 | 0.378281 | `azmcp_search_index_get` | ❌ |
| 15 | 0.378264 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.370172 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.362512 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.356766 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.343274 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.343229 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 157

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646941 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.458596 | `azmcp_sql_elastic-pool_list` | ❌ |
| 3 | 0.450190 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.440182 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.389989 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 6 | 0.384600 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.371847 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.367456 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.365231 | `azmcp_mysql_server_config_config` | ❌ |
| 10 | 0.357721 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.350998 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.350992 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 13 | 0.344818 | `azmcp_sql_db_show` | ❌ |
| 14 | 0.343726 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.342564 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.338364 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.332531 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.322714 | `azmcp_appconfig_kv_show` | ❌ |
| 19 | 0.321915 | `azmcp_eventgrid_subscription_list` | ❌ |
| 20 | 0.321672 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586166 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.411363 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.385173 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.383045 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.346023 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.338624 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 7 | 0.322784 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.320733 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.314439 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.313226 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.306678 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.306579 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.300123 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.298866 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.289422 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.287084 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.283171 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.281527 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.279652 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.276058 | `azmcp_sql_db_list` | ❌ |

---

## Test 159

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686975 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.521969 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.506624 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.487707 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.446699 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.440645 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.438636 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.435177 | `azmcp_acr_registry_repository_list` | ❌ |
| 9 | 0.431369 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.418681 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 11 | 0.413085 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 12 | 0.407783 | `azmcp_sql_server_list` | ❌ |
| 13 | 0.404890 | `azmcp_sql_db_list` | ❌ |
| 14 | 0.399249 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.393876 | `azmcp_group_list` | ❌ |
| 16 | 0.391869 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.389070 | `azmcp_redis_cluster_database_list` | ❌ |
| 18 | 0.385781 | `azmcp_workbooks_list` | ❌ |
| 19 | 0.382962 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.382851 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 160

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684416 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.544729 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.510269 | `azmcp_sql_elastic-pool_list` | ❌ |
| 4 | 0.509732 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.447545 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.441510 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 7 | 0.441482 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.433138 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.430830 | `azmcp_acr_registry_repository_list` | ❌ |
| 10 | 0.430739 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.421390 | `azmcp_sql_server_list` | ❌ |
| 12 | 0.408990 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 13 | 0.408569 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.407619 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.390197 | `azmcp_redis_cluster_database_list` | ❌ |
| 16 | 0.388937 | `azmcp_group_list` | ❌ |
| 17 | 0.387647 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.383234 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.378671 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.375102 | `azmcp_acr_registry_list` | ❌ |

---

## Test 161

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628929 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.457312 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.443902 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.425448 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.386949 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.378905 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.368944 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.363290 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.359493 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 10 | 0.356345 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.356139 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.354542 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.347994 | `azmcp_foundry_agents_list` | ❌ |
| 14 | 0.335285 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.329036 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.324551 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.323929 | `azmcp_deploy_plan_get` | ❌ |
| 18 | 0.323568 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.322487 | `azmcp_foundry_models_deployments_list` | ❌ |
| 20 | 0.321723 | `azmcp_acr_registry_list` | ❌ |

---

## Test 162

**Expected Tool:** `azmcp_loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586332 | `azmcp_loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.532463 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.509980 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.414555 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.395648 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.391589 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.347299 | `azmcp_loadtesting_testrun_update` | ❌ |
| 8 | 0.339287 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.338901 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.338500 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.324464 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.311500 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.311121 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.300391 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.297389 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.291418 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.289990 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.281208 | `azmcp_sql_server_create` | ❌ |
| 19 | 0.274621 | `azmcp_sql_server_list` | ❌ |
| 20 | 0.268558 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 163

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642420 | `azmcp_loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.608881 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.574394 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.534194 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.473323 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.469876 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.436995 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.404628 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.397451 | `azmcp_group_list` | ❌ |
| 10 | 0.379345 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.373229 | `azmcp_loadtesting_testrun_update` | ❌ |
| 12 | 0.370024 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.365569 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.360732 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.354531 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.347100 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 17 | 0.341360 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.329344 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.328339 | `azmcp_monitor_metrics_query` | ❌ |
| 20 | 0.322885 | `azmcp_quota_usage_check` | ❌ |

---

## Test 164

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
| 7 | 0.416919 | `azmcp_group_list` | ❌ |
| 8 | 0.407752 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.394967 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.382774 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.370093 | `azmcp_loadtesting_testrun_get` | ❌ |
| 12 | 0.369786 | `azmcp_sql_server_list` | ❌ |
| 13 | 0.369409 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.356910 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.350916 | `azmcp_loadtesting_testrun_update` | ❌ |
| 16 | 0.343649 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.342213 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.335696 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.326618 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.326596 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 165

**Expected Tool:** `azmcp_loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738027 | `azmcp_loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.591851 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.577464 | `azmcp_group_list` | ❌ |
| 4 | 0.565565 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.561516 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.526662 | `azmcp_workbooks_list` | ❌ |
| 7 | 0.515624 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.511607 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.506184 | `azmcp_loadtesting_test_get` | ❌ |
| 10 | 0.497916 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.487330 | `azmcp_grafana_list` | ❌ |
| 12 | 0.483681 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.478586 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.473444 | `azmcp_search_service_list` | ❌ |
| 15 | 0.473287 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.470899 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.463466 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.452190 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.447138 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.433793 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 166

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
| 11 | 0.362695 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.331019 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.325463 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.323782 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.306420 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.305429 | `azmcp_sql_db_rename` | ❌ |
| 17 | 0.273429 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.272151 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.267552 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.262297 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 167

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625332 | `azmcp_loadtesting_test_get` | ❌ |
| 2 | 0.603066 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.568405 | `azmcp_loadtesting_testrun_get` | ✅ **EXPECTED** |
| 4 | 0.561944 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.535183 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.496655 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.434255 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.415438 | `azmcp_loadtesting_testrun_update` | ❌ |
| 9 | 0.397878 | `azmcp_group_list` | ❌ |
| 10 | 0.394683 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.370196 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.366532 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.356307 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.353650 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.352984 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.346995 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.330484 | `azmcp_monitor_metrics_query` | ❌ |
| 18 | 0.329537 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.328853 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.315577 | `azmcp_quota_usage_check` | ❌ |

---

## Test 168

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
| 7 | 0.432165 | `azmcp_group_list` | ❌ |
| 8 | 0.416453 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.410933 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.406508 | `azmcp_loadtesting_test_create` | ❌ |
| 11 | 0.395915 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.392066 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.391147 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.375701 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.373875 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.356833 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.342588 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.340618 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.329464 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.328011 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 169

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
| 9 | 0.374342 | `azmcp_sql_db_rename` | ❌ |
| 10 | 0.332708 | `azmcp_sql_db_update` | ❌ |
| 11 | 0.320124 | `azmcp_workbooks_update` | ❌ |
| 12 | 0.300024 | `azmcp_workbooks_create` | ❌ |
| 13 | 0.268172 | `azmcp_workbooks_show` | ❌ |
| 14 | 0.267137 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.255408 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.253343 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.252149 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.250017 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.240916 | `azmcp_workbooks_delete` | ❌ |
| 20 | 0.233701 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 170

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
| 8 | 0.492316 | `azmcp_subscription_list` | ❌ |
| 9 | 0.489846 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.482789 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.479611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 12 | 0.459171 | `azmcp_eventgrid_topic_list` | ❌ |
| 13 | 0.457845 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 14 | 0.452244 | `azmcp_foundry_agents_list` | ❌ |
| 15 | 0.447752 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.447597 | `azmcp_sql_server_list` | ❌ |
| 17 | 0.441411 | `azmcp_group_list` | ❌ |
| 18 | 0.440393 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.436802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.428861 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 171

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.631810 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.516886 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.513156 | `azmcp_search_service_list` | ❌ |
| 5 | 0.507980 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.500639 | `azmcp_subscription_list` | ❌ |
| 7 | 0.499290 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.480850 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.472811 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.460936 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.460346 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.451887 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.450971 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.448426 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.447269 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.445430 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.442505 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.441460 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.438952 | `azmcp_grafana_list` | ❌ |
| 20 | 0.437939 | `azmcp_postgres_server_list` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743903 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.613249 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.519986 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.514120 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.492114 | `azmcp_acr_registry_repository_list` | ❌ |
| 6 | 0.477847 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.466545 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.452905 | `azmcp_acr_registry_list` | ❌ |
| 9 | 0.443767 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.441712 | `azmcp_group_list` | ❌ |
| 11 | 0.433933 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.412748 | `azmcp_search_service_list` | ❌ |
| 13 | 0.412709 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.409044 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.407705 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.406511 | `azmcp_mysql_database_list` | ❌ |
| 17 | 0.402926 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.398525 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.398168 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.397377 | `azmcp_functionapp_get` | ❌ |

---

## Test 173

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_required-subnet-size`  
**Prompt:** Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646978 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ✅ **EXPECTED** |
| 2 | 0.450342 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.327511 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.235376 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.204654 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.203596 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.198992 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.192371 | `azmcp_mysql_server_config_config` | ❌ |
| 9 | 0.188378 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 10 | 0.186367 | `azmcp_storage_blob_get` | ❌ |
| 11 | 0.176435 | `azmcp_marketplace_product_get` | ❌ |
| 12 | 0.175883 | `azmcp_postgres_server_param_param` | ❌ |
| 13 | 0.172920 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 14 | 0.170884 | `azmcp_mysql_table_schema_schema` | ❌ |
| 15 | 0.169503 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.166628 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.165373 | `azmcp_deploy_plan_get` | ❌ |
| 18 | 0.155464 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.154023 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.151555 | `azmcp_grafana_list` | ❌ |

---

## Test 174

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836033 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ✅ **EXPECTED** |
| 2 | 0.626238 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.453801 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.444792 | `azmcp_search_service_list` | ❌ |
| 5 | 0.438893 | `azmcp_quota_region_availability_list` | ❌ |
| 6 | 0.418743 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.411881 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 8 | 0.411221 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.405913 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.403218 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.402635 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.401697 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.401538 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.399919 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.398882 | `azmcp_subscription_list` | ❌ |
| 16 | 0.398576 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.395033 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.393969 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.393471 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.383764 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 175

**Expected Tool:** `azmcp_marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571292 | `azmcp_marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.475375 | `azmcp_marketplace_product_list` | ❌ |
| 3 | 0.356280 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.334347 | `azmcp_servicebus_queue_details` | ❌ |
| 5 | 0.327469 | `azmcp_search_index_get` | ❌ |
| 6 | 0.326345 | `azmcp_servicebus_topic_details` | ❌ |
| 7 | 0.317148 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.297918 | `azmcp_storage_blob_get` | ❌ |
| 9 | 0.291489 | `azmcp_workbooks_show` | ❌ |
| 10 | 0.287854 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.282849 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.280932 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.273746 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.265561 | `azmcp_foundry_models_list` | ❌ |
| 15 | 0.263033 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.256127 | `azmcp_keyvault_key_get` | ❌ |
| 17 | 0.255944 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 18 | 0.252182 | `azmcp_loadtesting_test_get` | ❌ |
| 19 | 0.247061 | `azmcp_grafana_list` | ❌ |
| 20 | 0.245933 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 176

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527078 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443174 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.343549 | `azmcp_search_service_list` | ❌ |
| 4 | 0.330500 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.328612 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 6 | 0.324866 | `azmcp_search_index_query` | ❌ |
| 7 | 0.302368 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.290877 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.290071 | `azmcp_search_index_get` | ❌ |
| 10 | 0.287924 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.264073 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.263529 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.258244 | `azmcp_foundry_models_deployments_list` | ❌ |
| 14 | 0.254438 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.251532 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.250343 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.248822 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 18 | 0.247119 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.245634 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.245271 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 177

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461616 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385183 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.308769 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.260385 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.259270 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.238760 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.238238 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.237988 | `azmcp_grafana_list` | ❌ |
| 9 | 0.226689 | `azmcp_search_service_list` | ❌ |
| 10 | 0.221140 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.218709 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.208553 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.206351 | `azmcp_eventgrid_events_publish` | ❌ |
| 14 | 0.204870 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.204011 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.203721 | `azmcp_eventgrid_topic_list` | ❌ |
| 17 | 0.202641 | `azmcp_workbooks_list` | ❌ |
| 18 | 0.201780 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 19 | 0.187594 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.185421 | `azmcp_subscription_list` | ❌ |

---

## Test 178

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.635406 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.531442 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.489890 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.447151 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.438801 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.379177 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 9 | 0.354191 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.353355 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.351664 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.322785 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.312391 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.312077 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.292040 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.290398 | `azmcp_search_service_list` | ❌ |
| 17 | 0.282195 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.276297 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.273557 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.273147 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 179

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.548542 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.541091 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.516786 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.516201 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.424443 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.424017 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.408963 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.392171 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369205 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.356238 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.342488 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.306627 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.305611 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.304620 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.304195 | `azmcp_search_service_list` | ❌ |
| 17 | 0.302423 | `azmcp_mysql_server_config_config` | ❌ |
| 18 | 0.302015 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.296142 | `azmcp_sql_db_create` | ❌ |
| 20 | 0.291071 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 180

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.594323 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.518643 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.465372 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.454158 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.430188 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.399071 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.392767 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.384118 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.380286 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.375863 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.362661 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.335296 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.330427 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.329342 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.322718 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.322483 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.317729 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.316805 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.314841 | `azmcp_search_service_list` | ❌ |

---

## Test 181

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.570488 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.522998 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.493714 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.444987 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.399706 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.381822 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.368157 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.367290 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.353417 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 11 | 0.317494 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.292977 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.284617 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.278941 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.275342 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.256382 | `azmcp_mysql_server_config_config` | ❌ |
| 17 | 0.252529 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.241745 | `azmcp_search_index_query` | ❌ |
| 19 | 0.239436 | `azmcp_search_service_list` | ❌ |
| 20 | 0.239371 | `azmcp_storage_blob_get` | ❌ |

---

## Test 182

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581850 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.497044 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495659 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.486886 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.474427 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.439182 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.412001 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.399154 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.376938 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.373497 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.323164 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.317931 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.303572 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290695 | `azmcp_mysql_server_config_config` | ❌ |
| 15 | 0.277947 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.276228 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.275749 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.270375 | `azmcp_search_service_list` | ❌ |
| 19 | 0.269415 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.269109 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 183

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.532790 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.487322 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.457794 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.412687 | `azmcp_functionapp_get` | ❌ |
| 6 | 0.395940 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.394761 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.393740 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.375723 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.363088 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.332626 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.332015 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.307885 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290894 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.289428 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.289326 | `azmcp_mysql_server_config_config` | ❌ |
| 17 | 0.284975 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.284215 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.275538 | `azmcp_search_index_query` | ❌ |
| 20 | 0.274580 | `azmcp_storage_blob_get` | ❌ |

---

## Test 184

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.513262 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.505123 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.483458 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.405143 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.400837 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.397488 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.389556 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.334624 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.315627 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.311847 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.292282 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.283198 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.275578 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.258767 | `azmcp_search_index_query` | ❌ |
| 16 | 0.256800 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.256751 | `azmcp_search_service_list` | ❌ |
| 18 | 0.254592 | `azmcp_storage_blob_get` | ❌ |
| 19 | 0.253368 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.251387 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 185

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582490 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.500283 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.471988 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.432884 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.385896 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.380793 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.374654 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.368385 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.358632 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.336180 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.293777 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.288804 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.259711 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.252935 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.251191 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.249974 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.246296 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.240277 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.231240 | `azmcp_search_index_query` | ❌ |
| 20 | 0.231047 | `azmcp_mysql_server_config_config` | ❌ |

---

## Test 186

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a simple HTTP-triggered function app in javascript that returns a random compliment from a predefined list in a JSON response. And deploy it to azure eventually. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428997 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.408134 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.380754 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.377184 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.352369 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.343872 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.319970 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.311848 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.300755 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.299149 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.235579 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.232320 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.218912 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.215940 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.210908 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.206254 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.203401 | `azmcp_search_index_query` | ❌ |
| 18 | 0.202251 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.197959 | `azmcp_mysql_database_query` | ❌ |
| 20 | 0.186531 | `azmcp_sql_server_create` | ❌ |

---

## Test 187

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497262 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.493127 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.404037 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.395623 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.385140 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.374153 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.354448 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.348171 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.300092 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.284049 | `azmcp_storage_blob_container_create` | ❌ |
| 11 | 0.266937 | `azmcp_foundry_models_deploy` | ❌ |
| 12 | 0.248999 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.243575 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.234797 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.221236 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.218621 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.210666 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.209213 | `azmcp_workbooks_create` | ❌ |
| 19 | 0.208812 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.195538 | `azmcp_sql_server_create` | ❌ |

---

## Test 188

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498345 | `azmcp_monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.472094 | `azmcp_monitor_workspace_list` | ❌ |
| 3 | 0.468895 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.468311 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.463168 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.436971 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.418755 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.413364 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.401596 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.385416 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.380121 | `azmcp_grafana_list` | ❌ |
| 12 | 0.358432 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.333342 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.314296 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.306266 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.302997 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.296719 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.286305 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.279273 | `azmcp_kusto_query` | ❌ |
| 20 | 0.276023 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 189

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592660 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424141 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.332359 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.315519 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.315321 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.311108 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 7 | 0.305464 | `azmcp_servicebus_queue_details` | ❌ |
| 8 | 0.304735 | `azmcp_grafana_list` | ❌ |
| 9 | 0.303453 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.298853 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.294124 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.287300 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.284519 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.277559 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.274784 | `azmcp_loadtesting_test_get` | ❌ |
| 16 | 0.262262 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.256957 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 18 | 0.248987 | `azmcp_bicepschema_get` | ❌ |
| 19 | 0.246033 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.234617 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 190

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589859 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.587682 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.550622 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.473421 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.472697 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.459829 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.439032 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.437922 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.431120 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.417098 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.414488 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.403921 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.401901 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.397499 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.391384 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.390422 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.384259 | `azmcp_keyvault_admin_get` | ❌ |
| 18 | 0.383510 | `azmcp_subscription_list` | ❌ |
| 19 | 0.378187 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.371164 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 191

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633139 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495513 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.382374 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.380460 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.370865 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.359089 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.353264 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.344326 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.341897 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.337875 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.329534 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.326682 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.324002 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.322121 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 15 | 0.317469 | `azmcp_monitor_workspace_log_query` | ❌ |
| 16 | 0.303621 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.301967 | `azmcp_workbooks_show` | ❌ |
| 18 | 0.291565 | `azmcp_cloudarchitect_design` | ❌ |
| 19 | 0.291260 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.287764 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 192

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555317 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.447659 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.447244 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.433796 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.422422 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.416106 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.413285 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.409166 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.388248 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.380058 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.356822 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.350105 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.342486 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.339770 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.335291 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.329496 | `azmcp_loadtesting_testresource_create` | ❌ |
| 17 | 0.326867 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.326804 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.326457 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.320848 | `azmcp_search_index_query` | ❌ |

---

## Test 193

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557875 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.508711 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.460631 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455925 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.438192 | `azmcp_monitor_metrics_definitions` | ❌ |
| 6 | 0.392074 | `azmcp_monitor_resource_log_query` | ❌ |
| 7 | 0.391689 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.373002 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.368505 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.354700 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 11 | 0.339444 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.336613 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.326925 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.326763 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.321482 | `azmcp_search_service_list` | ❌ |
| 16 | 0.321234 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.318257 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.317796 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.303918 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.303626 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 194

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461850 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390705 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.306703 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.304768 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.301795 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.289527 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.275753 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.267980 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.267739 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 10 | 0.266264 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.263840 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.263114 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.259520 | `azmcp_grafana_list` | ❌ |
| 14 | 0.254068 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.249275 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.247977 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.247804 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.242352 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.238667 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.235661 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 195

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
| 7 | 0.369668 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.366857 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.362030 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.359340 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.331730 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.316302 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.315502 | `azmcp_functionapp_get` | ❌ |
| 14 | 0.311842 | `azmcp_search_index_query` | ❌ |
| 15 | 0.308679 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.295918 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.293608 | `azmcp_search_service_list` | ❌ |
| 18 | 0.293312 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.288866 | `azmcp_foundry_agents_connect` | ❌ |
| 20 | 0.287707 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 196

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525713 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.384448 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.377051 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.367389 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.299861 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.293184 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 7 | 0.290184 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.277522 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 9 | 0.272379 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.267183 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.266236 | `azmcp_mysql_server_param_param` | ❌ |
| 12 | 0.265904 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.262917 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.262097 | `azmcp_grafana_list` | ❌ |
| 15 | 0.261628 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.248073 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 17 | 0.246494 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.244276 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.242675 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.239515 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 197

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480140 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.381961 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.363411 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.359285 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.350523 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.350402 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.346343 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.331215 | `azmcp_loadtesting_testresource_list` | ❌ |
| 9 | 0.330074 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.328776 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.324932 | `azmcp_search_index_query` | ❌ |
| 12 | 0.319343 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.317459 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.292195 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.290762 | `azmcp_search_service_list` | ❌ |
| 16 | 0.284284 | `azmcp_foundry_agents_connect` | ❌ |
| 17 | 0.282389 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.278490 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.277206 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.265303 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 198

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594175 | `azmcp_monitor_workspace_log_query` | ❌ |
| 2 | 0.580120 | `azmcp_monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.472064 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.469703 | `azmcp_monitor_metrics_query` | ❌ |
| 5 | 0.443501 | `azmcp_monitor_table_list` | ❌ |
| 6 | 0.443468 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.392429 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.390022 | `azmcp_grafana_list` | ❌ |
| 9 | 0.366124 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.359065 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.352821 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.345341 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.344781 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.337855 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.320690 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.313749 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 17 | 0.308949 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.307107 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.305149 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.302745 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 199

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851094 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725750 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.534829 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.511123 | `azmcp_kusto_table_list` | ❌ |
| 6 | 0.502075 | `azmcp_grafana_list` | ❌ |
| 7 | 0.488557 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.444296 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.420394 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.419859 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.413834 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.409199 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.400092 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.397440 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.396780 | `azmcp_search_service_list` | ❌ |
| 16 | 0.377057 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.375176 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.374930 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.366099 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.365781 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 200

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798710 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701130 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.599916 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.497065 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.487237 | `azmcp_grafana_list` | ❌ |
| 6 | 0.466630 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.449928 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.427408 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.413678 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.411646 | `azmcp_kusto_table_schema` | ❌ |
| 11 | 0.403863 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.398753 | `azmcp_mysql_table_schema_schema` | ❌ |
| 13 | 0.389881 | `azmcp_mysql_database_list` | ❌ |
| 14 | 0.376474 | `azmcp_kusto_sample` | ❌ |
| 15 | 0.376338 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.373298 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.370624 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.347853 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.346183 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.343837 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 201

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881531 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765290 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.569895 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.506201 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.476007 | `azmcp_grafana_list` | ❌ |
| 6 | 0.447201 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.446337 | `azmcp_mysql_table_schema_schema` | ❌ |
| 8 | 0.419204 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.416160 | `azmcp_kusto_table_schema` | ❌ |
| 10 | 0.413340 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.403853 | `azmcp_monitor_workspace_log_query` | ❌ |
| 12 | 0.403707 | `azmcp_monitor_metrics_definitions` | ❌ |
| 13 | 0.383663 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.380120 | `azmcp_kusto_sample` | ❌ |
| 15 | 0.373335 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.371885 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.369305 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.361360 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.353771 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.347560 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 202

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843108 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.737081 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.576731 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.481189 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.475734 | `azmcp_grafana_list` | ❌ |
| 6 | 0.451212 | `azmcp_mysql_table_schema_schema` | ❌ |
| 7 | 0.427956 | `azmcp_kusto_table_schema` | ❌ |
| 8 | 0.427581 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.421484 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.406242 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.391308 | `azmcp_kusto_sample` | ❌ |
| 12 | 0.384679 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.376196 | `azmcp_monitor_metrics_definitions` | ❌ |
| 14 | 0.372991 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.370860 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.367591 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.355178 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.348357 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.340101 | `azmcp_foundry_models_list` | ❌ |
| 20 | 0.339804 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 203

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `azmcp_grafana_list` | ❌ |
| 3 | 0.660265 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.600802 | `azmcp_search_service_list` | ❌ |
| 5 | 0.583259 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.530433 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.517493 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.500768 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.494683 | `azmcp_group_list` | ❌ |
| 10 | 0.493846 | `azmcp_subscription_list` | ❌ |
| 11 | 0.475628 | `azmcp_monitor_workspace_log_query` | ❌ |
| 12 | 0.471758 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.470266 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.467655 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.466748 | `azmcp_acr_registry_list` | ❌ |
| 16 | 0.464047 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.460452 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.448201 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.444214 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.443888 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 204

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.585880 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.531110 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `azmcp_grafana_list` | ❌ |
| 5 | 0.475196 | `azmcp_monitor_workspace_log_query` | ❌ |
| 6 | 0.459841 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.444207 | `azmcp_search_service_list` | ❌ |
| 8 | 0.414153 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.386422 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.380891 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.373786 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.371395 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.365111 | `azmcp_aks_cluster_get` | ❌ |
| 14 | 0.363287 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.358029 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.355175 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.354276 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.353783 | `azmcp_subscription_list` | ❌ |
| 19 | 0.352809 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.351397 | `azmcp_search_index_get` | ❌ |

---

## Test 205

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `azmcp_grafana_list` | ❌ |
| 3 | 0.580593 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.521342 | `azmcp_monitor_table_type_list` | ❌ |
| 5 | 0.521277 | `azmcp_search_service_list` | ❌ |
| 6 | 0.463821 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.453702 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.439297 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435475 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.428945 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.422854 | `azmcp_subscription_list` | ❌ |
| 12 | 0.422379 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.411648 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.411448 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.410082 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.409827 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.404262 | `azmcp_group_list` | ❌ |
| 18 | 0.402600 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.400615 | `azmcp_postgres_server_list` | ❌ |
| 20 | 0.395576 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 206

**Expected Tool:** `azmcp_monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591883 | `azmcp_monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.494715 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.486452 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.484159 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.483323 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.427266 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.374939 | `azmcp_monitor_metrics_query` | ❌ |
| 8 | 0.365704 | `azmcp_grafana_list` | ❌ |
| 9 | 0.330182 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.322875 | `azmcp_workbooks_delete` | ❌ |
| 11 | 0.322408 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 12 | 0.315638 | `azmcp_search_service_list` | ❌ |
| 13 | 0.309411 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.299830 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.292088 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.291669 | `azmcp_kusto_query` | ❌ |
| 17 | 0.289781 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.286966 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.283917 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.278789 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 207

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668828 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.434813 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.413173 | `azmcp_monitor_metrics_query` | ❌ |
| 4 | 0.408658 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.401731 | `azmcp_grafana_list` | ❌ |
| 6 | 0.393318 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.386669 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.369805 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.364360 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.356643 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.355415 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.345384 | `azmcp_postgres_database_list` | ❌ |
| 13 | 0.345382 | `azmcp_group_list` | ❌ |
| 14 | 0.330769 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.328960 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.327205 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.306977 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.304096 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.302405 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.296544 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 208

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624201 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443487 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.393308 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.374196 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.371128 | `azmcp_grafana_list` | ❌ |
| 6 | 0.370786 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.359369 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.350724 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.343330 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.342596 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.337245 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.320564 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.319933 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.303029 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.289952 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.287494 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.285414 | `azmcp_group_list` | ❌ |
| 18 | 0.274867 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.274670 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.272765 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 209

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `azmcp_quota_usage_check` | ❌ |
| 2 | 0.497434 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.481143 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.476826 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.462076 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 6 | 0.451690 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.440399 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.438386 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.434685 | `azmcp_search_service_list` | ❌ |
| 10 | 0.431096 | `azmcp_deploy_iac_rules_get` | ❌ |
| 11 | 0.423395 | `azmcp_subscription_list` | ❌ |
| 12 | 0.422293 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.417076 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.407946 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.405800 | `azmcp_deploy_plan_get` | ❌ |
| 16 | 0.400363 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.395234 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.391649 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.388980 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.381209 | `azmcp_storage_account_get` | ❌ |

---

## Test 210

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.488377 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.476164 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 5 | 0.473365 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.462743 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 7 | 0.452232 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.447632 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.442021 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.438683 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.426049 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 12 | 0.385771 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.382677 | `azmcp_search_service_list` | ❌ |
| 14 | 0.375913 | `azmcp_subscription_list` | ❌ |
| 15 | 0.375094 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.365859 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.365824 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.360612 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.349469 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.341827 | `azmcp_mysql_server_config_config` | ❌ |

---

## Test 211

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.514978 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 4 | 0.504673 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.494256 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.487387 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.481698 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.464304 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.463564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 10 | 0.463157 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.452811 | `azmcp_search_service_list` | ❌ |
| 12 | 0.433938 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.423636 | `azmcp_subscription_list` | ❌ |
| 14 | 0.417356 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.403357 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.398621 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.380267 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.377353 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.376550 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.376262 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 212

**Expected Tool:** `azmcp_quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590878 | `azmcp_quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413274 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.372940 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369867 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.361386 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.349740 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.348742 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.337839 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.331145 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.331087 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.328408 | `azmcp_grafana_list` | ❌ |
| 12 | 0.325796 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.313240 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.310624 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.307143 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.297286 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.292791 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.290187 | `azmcp_group_list` | ❌ |
| 19 | 0.287104 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.263276 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 213

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
| 8 | 0.351760 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.345161 | `azmcp_eventgrid_subscription_list` | ❌ |
| 10 | 0.345156 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.342266 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.342232 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.338636 | `azmcp_grafana_list` | ❌ |
| 14 | 0.331294 | `azmcp_monitor_metrics_definitions` | ❌ |
| 15 | 0.322571 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.321805 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.305083 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.304570 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.297650 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.284888 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 214

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645258 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.484094 | `azmcp_group_list` | ❌ |
| 3 | 0.483209 | `azmcp_subscription_list` | ❌ |
| 4 | 0.478700 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474795 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.471364 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.468597 | `azmcp_search_service_list` | ❌ |
| 8 | 0.460029 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.452819 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.446372 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 11 | 0.430667 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.427950 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.426624 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.425029 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.421599 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.409648 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.403310 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.398493 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.397565 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.374732 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 215

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609704 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.456956 | `azmcp_grafana_list` | ❌ |
| 3 | 0.436838 | `azmcp_subscription_list` | ❌ |
| 4 | 0.435642 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.435155 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.431865 | `azmcp_search_service_list` | ❌ |
| 7 | 0.428756 | `azmcp_group_list` | ❌ |
| 8 | 0.428370 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.421627 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.420804 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.415941 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.410380 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 13 | 0.406766 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.395445 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.390162 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.386800 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.383635 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.373204 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.368511 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.363720 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 216

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
| 7 | 0.306752 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.303549 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.300024 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.298380 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.296657 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.292315 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.286490 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.285905 | `azmcp_keyvault_admin_get` | ❌ |
| 15 | 0.285063 | `azmcp_search_service_list` | ❌ |
| 16 | 0.284898 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.283818 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.277730 | `azmcp_subscription_list` | ❌ |
| 19 | 0.274897 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.272895 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 217

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713864 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.523242 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.412442 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.338954 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.293621 | `azmcp_keyvault_admin_get` | ❌ |
| 6 | 0.286320 | `azmcp_appconfig_kv_list` | ❌ |
| 7 | 0.283740 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.280296 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.265256 | `azmcp_storage_blob_container_get` | ❌ |
| 10 | 0.264474 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.262057 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.258056 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.257887 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.257387 | `azmcp_mysql_server_config_config` | ❌ |
| 15 | 0.257166 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.249883 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.249625 | `azmcp_loadtesting_testrun_list` | ❌ |
| 18 | 0.247094 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.246918 | `azmcp_grafana_list` | ❌ |
| 20 | 0.246815 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 218

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
| 10 | 0.423186 | `azmcp_subscription_list` | ❌ |
| 11 | 0.414865 | `azmcp_search_service_list` | ❌ |
| 12 | 0.396295 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.387797 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.381343 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.373546 | `azmcp_group_list` | ❌ |
| 16 | 0.368719 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.367794 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.367552 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.364522 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.361464 | `azmcp_acr_registry_list` | ❌ |

---

## Test 219

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537906 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.450409 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 3 | 0.441096 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.401251 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.302289 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.283561 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.275942 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.265804 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.262055 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.257553 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.252020 | `azmcp_grafana_list` | ❌ |
| 12 | 0.246438 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.236069 | `azmcp_postgres_table_list` | ❌ |
| 14 | 0.233732 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.231227 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.225057 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.224066 | `azmcp_loadtesting_testrun_list` | ❌ |
| 18 | 0.217937 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.212340 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.210079 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 220

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692161 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.595630 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.461513 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.434906 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.427314 | `azmcp_grafana_list` | ❌ |
| 6 | 0.399271 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.383297 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.382230 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.361639 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.358870 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.353454 | `azmcp_subscription_list` | ❌ |
| 12 | 0.353362 | `azmcp_search_service_list` | ❌ |
| 13 | 0.340690 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.327240 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.310746 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.308738 | `azmcp_eventgrid_topic_list` | ❌ |
| 17 | 0.306320 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.305856 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.304160 | `azmcp_group_list` | ❌ |
| 20 | 0.300167 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 221

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752920 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.603780 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.594994 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.548277 | `azmcp_postgres_database_list` | ❌ |
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
| 16 | 0.367337 | `azmcp_aks_cluster_get` | ❌ |
| 17 | 0.366236 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.337547 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.328081 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.324867 | `azmcp_grafana_list` | ❌ |

---

## Test 222

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
| 6 | 0.480247 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.434940 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.414701 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.408379 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.397284 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.369086 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.353712 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.351025 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.349880 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.343274 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 16 | 0.338974 | `azmcp_aks_cluster_get` | ❌ |
| 17 | 0.318982 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.317716 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.302228 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.294458 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 223

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812960 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.679028 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.672104 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.588847 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.554299 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.527406 | `azmcp_kusto_database_list` | ❌ |
| 7 | 0.526892 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.503279 | `azmcp_grafana_list` | ❌ |
| 9 | 0.467957 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.462558 | `azmcp_search_service_list` | ❌ |
| 11 | 0.457600 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.455614 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.445628 | `azmcp_group_list` | ❌ |
| 14 | 0.445406 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.443534 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.442886 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 17 | 0.436559 | `azmcp_subscription_list` | ❌ |
| 18 | 0.435221 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.419136 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.411121 | `azmcp_mysql_server_list` | ❌ |

---

## Test 224

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591594 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.514374 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.467519 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.403282 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.385070 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 6 | 0.370495 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.337910 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.329389 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.322157 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.321180 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.308304 | `azmcp_aks_nodepool_get` | ❌ |
| 12 | 0.305874 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.295045 | `azmcp_grafana_list` | ❌ |
| 14 | 0.291650 | `azmcp_postgres_database_list` | ❌ |
| 15 | 0.272503 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.261138 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.260993 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.259662 | `azmcp_postgres_server_config_config` | ❌ |
| 19 | 0.257012 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.252053 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 225

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
| 6 | 0.466632 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.456252 | `azmcp_grafana_list` | ❌ |
| 8 | 0.446568 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.440660 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.412876 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.400256 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 12 | 0.398399 | `azmcp_search_service_list` | ❌ |
| 13 | 0.394530 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.394483 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.389814 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.372357 | `azmcp_group_list` | ❌ |
| 17 | 0.370370 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.369831 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.368926 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.367955 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 226

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
| 9 | 0.519242 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.518520 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.517060 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.509454 | `azmcp_search_service_list` | ❌ |
| 13 | 0.500858 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.491176 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.490734 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.486716 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.483567 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.479642 | `azmcp_subscription_list` | ❌ |
| 19 | 0.477800 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.432179 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 227

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529480 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.463685 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.462391 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.459304 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.453960 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.429014 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.426935 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.407817 | `azmcp_grafana_list` | ❌ |
| 9 | 0.398432 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.396822 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.391278 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.383058 | `azmcp_acr_registry_list` | ❌ |
| 13 | 0.379927 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.375998 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.373796 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.366273 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.351404 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.350999 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.340946 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.328487 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 228

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665784 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.531920 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.523088 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.522911 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.518543 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.515905 | `azmcp_grafana_list` | ❌ |
| 8 | 0.494579 | `azmcp_eventgrid_subscription_list` | ❌ |
| 9 | 0.492945 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.489079 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.487780 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.475708 | `azmcp_search_service_list` | ❌ |
| 13 | 0.470658 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.464637 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.460412 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.454711 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.454439 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.432994 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.429798 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.429564 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 229

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630647 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377586 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.349981 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.331510 | `azmcp_monitor_metrics_definitions` | ❌ |
| 6 | 0.330187 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.327691 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.325866 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.324331 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.322117 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.311644 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.308238 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.306616 | `azmcp_grafana_list` | ❌ |
| 14 | 0.290698 | `azmcp_workbooks_show` | ❌ |
| 15 | 0.286287 | `azmcp_loadtesting_test_get` | ❌ |
| 16 | 0.285444 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.284990 | `azmcp_applens_resource_diagnose` | ❌ |
| 18 | 0.272287 | `azmcp_group_list` | ❌ |
| 19 | 0.270773 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.269043 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 230

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549306 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.510464 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.490090 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 4 | 0.466885 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.455902 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.412584 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.411283 | `azmcp_quota_usage_check` | ❌ |
| 8 | 0.405847 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.403899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.375351 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.368263 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.349407 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.347885 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.347171 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.346145 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.336480 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.324616 | `azmcp_keyvault_admin_get` | ❌ |
| 18 | 0.321704 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.311399 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.306746 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 231

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577369 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 2 | 0.570898 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424806 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.393403 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.386562 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.373791 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.355638 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.352462 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.342110 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.337904 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.335677 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.327168 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.321245 | `azmcp_group_list` | ❌ |
| 14 | 0.318270 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.318211 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.316438 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.307191 | `azmcp_applens_resource_diagnose` | ❌ |
| 18 | 0.301135 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.295355 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.289090 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 232

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
| 9 | 0.531121 | `azmcp_group_list` | ❌ |
| 10 | 0.507740 | `azmcp_monitor_workspace_list` | ❌ |
| 11 | 0.496651 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.491454 | `azmcp_subscription_list` | ❌ |
| 13 | 0.491394 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.489514 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.484221 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.482623 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.476832 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.462544 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.459718 | `azmcp_workbooks_list` | ❌ |
| 20 | 0.457237 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 233

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
| 6 | 0.456449 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.441470 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.441430 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.430496 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.418944 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.409363 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.406849 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.406709 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.406408 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.405790 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.387835 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.381144 | `azmcp_get_bestpractices_get` | ❌ |
| 18 | 0.379969 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 19 | 0.371846 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.366055 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 234

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
| 13 | 0.354932 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.350520 | `azmcp_group_list` | ❌ |
| 15 | 0.348923 | `azmcp_monitor_metrics_query` | ❌ |
| 16 | 0.338595 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.330185 | `azmcp_extension_azqr` | ❌ |
| 18 | 0.328471 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.324217 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.309414 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 235

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.719917 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `azmcp_search_service_list` | ❌ |
| 3 | 0.531311 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.518372 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.503814 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.470139 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.456526 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.454448 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.446515 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.438741 | `azmcp_subscription_list` | ❌ |
| 11 | 0.426698 | `azmcp_grafana_list` | ❌ |
| 12 | 0.419828 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.419011 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.416883 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.412013 | `azmcp_group_list` | ❌ |
| 16 | 0.407099 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.385382 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.378841 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.377914 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.355746 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 236

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726947 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.513815 | `azmcp_search_service_list` | ❌ |
| 3 | 0.509195 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.491121 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484385 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.474847 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.459851 | `azmcp_subscription_list` | ❌ |
| 8 | 0.431511 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.425644 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.411892 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.410579 | `azmcp_marketplace_product_list` | ❌ |
| 12 | 0.404636 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.390652 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.390558 | `azmcp_group_list` | ❌ |
| 15 | 0.390381 | `azmcp_applens_resource_diagnose` | ❌ |
| 16 | 0.385710 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.384857 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 18 | 0.384613 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.381218 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.381190 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 237

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
| 15 | 0.164285 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.162936 | `azmcp_monitor_workspace_log_query` | ❌ |
| 17 | 0.155791 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.151666 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.147523 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.147023 | `azmcp_grafana_list` | ❌ |

---

## Test 238

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711107 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.545714 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.520197 | `azmcp_search_service_list` | ❌ |
| 4 | 0.502064 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.487386 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.453380 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 7 | 0.451351 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.439658 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.436070 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.411793 | `azmcp_grafana_list` | ❌ |
| 11 | 0.408792 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.407649 | `azmcp_subscription_list` | ❌ |
| 13 | 0.406949 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.391992 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.379017 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.371380 | `azmcp_group_list` | ❌ |
| 17 | 0.368866 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.358754 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.357139 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.349737 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527706 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.437868 | `azmcp_search_service_list` | ❌ |
| 3 | 0.402493 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.402232 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.400175 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.397735 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.382581 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.382254 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.375017 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.372844 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.371690 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.363470 | `azmcp_get_bestpractices_get` | ❌ |
| 13 | 0.362214 | `azmcp_applens_resource_diagnose` | ❌ |
| 14 | 0.360398 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.357531 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.341495 | `azmcp_foundry_models_deployments_list` | ❌ |
| 17 | 0.337965 | `azmcp_search_index_get` | ❌ |
| 18 | 0.335468 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.333382 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.333276 | `azmcp_subscription_list` | ❌ |

---

## Test 240

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `azmcp_servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.401012 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.359942 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.352801 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.352705 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.351024 | `azmcp_search_index_get` | ❌ |
| 8 | 0.344531 | `azmcp_eventgrid_subscription_list` | ❌ |
| 9 | 0.342349 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.337239 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.332541 | `azmcp_loadtesting_testrun_get` | ❌ |
| 12 | 0.323243 | `azmcp_marketplace_product_get` | ❌ |
| 13 | 0.323046 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.310612 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.309378 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.301971 | `azmcp_keyvault_key_get` | ❌ |
| 17 | 0.296523 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.290417 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.266737 | `azmcp_appconfig_kv_show` | ❌ |
| 20 | 0.264940 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 241

**Expected Tool:** `azmcp_servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591595 | `azmcp_servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571860 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.497732 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.494877 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.483976 | `azmcp_servicebus_queue_details` | ❌ |
| 6 | 0.365638 | `azmcp_search_index_get` | ❌ |
| 7 | 0.352488 | `azmcp_marketplace_product_get` | ❌ |
| 8 | 0.344942 | `azmcp_eventgrid_events_publish` | ❌ |
| 9 | 0.341289 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.340036 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.337713 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.335558 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.333396 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.330814 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.325041 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.307905 | `azmcp_aks_cluster_get` | ❌ |
| 17 | 0.306565 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.297323 | `azmcp_grafana_list` | ❌ |
| 19 | 0.290383 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.281789 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 242

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `azmcp_servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.523134 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.494515 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.457149 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.444629 | `azmcp_marketplace_product_get` | ❌ |
| 6 | 0.443942 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.429458 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.426573 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.421009 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.411293 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.405380 | `azmcp_search_service_list` | ❌ |
| 12 | 0.404740 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.395789 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.395176 | `azmcp_grafana_list` | ❌ |
| 15 | 0.382562 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.369986 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.368155 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.368121 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.367715 | `azmcp_group_list` | ❌ |
| 20 | 0.358070 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 243

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470996 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.420504 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.376833 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.371321 | `azmcp_appservice_database_add` | ❌ |
| 6 | 0.359945 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.357421 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.355629 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.347128 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.346831 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.329705 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 12 | 0.311744 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.301243 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.297803 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.279490 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.276192 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.272135 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.254831 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.238999 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.231289 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 244

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459742 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.424021 | `azmcp_appservice_database_add` | ❌ |
| 4 | 0.420843 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.396349 | `azmcp_sql_db_update` | ❌ |
| 6 | 0.395495 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.393427 | `azmcp_sql_db_rename` | ❌ |
| 8 | 0.384660 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.371559 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.361051 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.343099 | `azmcp_sql_db_delete` | ❌ |
| 12 | 0.324123 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.321837 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.317180 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.311150 | `azmcp_keyvault_key_create` | ❌ |
| 16 | 0.304604 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.301469 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.290173 | `azmcp_keyvault_certificate_create` | ❌ |
| 19 | 0.286796 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 20 | 0.276688 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 245

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545935 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.504013 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.494377 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.473975 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.456262 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.455293 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.440988 | `azmcp_appservice_database_add` | ❌ |
| 9 | 0.431069 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.422871 | `azmcp_workbooks_create` | ❌ |
| 11 | 0.413309 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.398726 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.392814 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.390278 | `azmcp_keyvault_secret_create` | ❌ |
| 15 | 0.379238 | `azmcp_keyvault_key_create` | ❌ |
| 16 | 0.378636 | `azmcp_keyvault_certificate_create` | ❌ |
| 17 | 0.365919 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.358566 | `azmcp_kusto_table_list` | ❌ |
| 19 | 0.323547 | `azmcp_group_list` | ❌ |
| 20 | 0.319382 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 246

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520786 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.484026 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.391509 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.386564 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 5 | 0.364776 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.351228 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.350121 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.345061 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.338052 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.337699 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.300711 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.286892 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.284794 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.278895 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.271010 | `azmcp_appconfig_kv_delete` | ❌ |
| 16 | 0.253808 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.243248 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.235173 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.211680 | `azmcp_kusto_query` | ❌ |
| 20 | 0.183587 | `azmcp_kusto_sample` | ❌ |

---

## Test 247

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579119 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.500756 | `azmcp_sql_db_show` | ❌ |
| 3 | 0.481083 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.478729 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.466216 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 6 | 0.437112 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.421365 | `azmcp_sql_db_create` | ❌ |
| 8 | 0.412704 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.392236 | `azmcp_workbooks_delete` | ❌ |
| 10 | 0.390334 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.388379 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.381377 | `azmcp_sql_server_create` | ❌ |
| 13 | 0.380074 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.370486 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.345627 | `azmcp_group_list` | ❌ |
| 16 | 0.334517 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.332517 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.327408 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.310465 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.304849 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 248

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.459421 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.427494 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.364519 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.355416 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.347837 | `azmcp_sql_db_rename` | ❌ |
| 6 | 0.319617 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.314902 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.311506 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.310758 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.305059 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 11 | 0.295355 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.295012 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.288554 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.283955 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.258654 | `azmcp_appconfig_kv_delete` | ❌ |
| 16 | 0.246948 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.229817 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.213266 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.187992 | `azmcp_kusto_query` | ❌ |
| 20 | 0.171883 | `azmcp_kusto_sample` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639694 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.609187 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.602889 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.532407 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.529058 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.527896 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.486638 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.485960 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.476350 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.475733 | `azmcp_sql_elastic-pool_list` | ❌ |
| 12 | 0.474927 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.457314 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.441355 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.440528 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.400489 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.395078 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.394429 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.380402 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.373774 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 250

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.609322 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.557353 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.553488 | `azmcp_mysql_server_config_config` | ❌ |
| 5 | 0.524274 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.471873 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.461650 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.458741 | `azmcp_postgres_server_config_config` | ❌ |
| 9 | 0.451452 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.446512 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.445291 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.387645 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.385113 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.380428 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.349880 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.347075 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.342733 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.342072 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.341681 | `azmcp_kusto_table_list` | ❌ |
| 20 | 0.340834 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 251

**Expected Tool:** `azmcp_sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593181 | `azmcp_sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.396906 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.381029 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.350729 | `azmcp_sql_db_update` | ❌ |
| 5 | 0.345991 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.340635 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.339551 | `azmcp_sql_db_delete` | ❌ |
| 8 | 0.337044 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.336806 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.317484 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.313245 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.281132 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.245931 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.230208 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.228053 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.227541 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.215246 | `azmcp_loadtesting_testrun_update` | ❌ |
| 18 | 0.183017 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.180728 | `azmcp_loadtesting_testrun_create` | ❌ |
| 20 | 0.171049 | `azmcp_kusto_query` | ❌ |

---

## Test 252

**Expected Tool:** `azmcp_sql_db_rename`  
**Prompt:** Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711063 | `azmcp_sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.501476 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.474176 | `azmcp_sql_db_update` | ❌ |
| 4 | 0.473371 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.433898 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.431320 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.424295 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.423143 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.421543 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.411748 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.381555 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.359479 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.311729 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.308041 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.306518 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.303220 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.269229 | `azmcp_loadtesting_testrun_update` | ❌ |
| 18 | 0.257841 | `azmcp_keyvault_key_create` | ❌ |
| 19 | 0.251195 | `azmcp_keyvault_secret_create` | ❌ |
| 20 | 0.248190 | `azmcp_kusto_query` | ❌ |

---

## Test 253

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610991 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.593150 | `azmcp_postgres_server_config_config` | ❌ |
| 3 | 0.530422 | `azmcp_mysql_server_config_config` | ❌ |
| 4 | 0.528136 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465693 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.446682 | `azmcp_postgres_server_param_param` | ❌ |
| 7 | 0.438925 | `azmcp_mysql_server_param_param` | ❌ |
| 8 | 0.398181 | `azmcp_mysql_table_schema_schema` | ❌ |
| 9 | 0.397510 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.396416 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.371413 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.326036 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.317619 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.297873 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.294946 | `azmcp_appconfig_kv_list` | ❌ |
| 16 | 0.281775 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.279952 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 18 | 0.273566 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.273315 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.269776 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 254

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530095 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503681 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.440073 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.438622 | `azmcp_mysql_table_schema_schema` | ❌ |
| 5 | 0.432919 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.421854 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.400963 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.398714 | `azmcp_mysql_server_config_config` | ❌ |
| 9 | 0.375668 | `azmcp_postgres_server_config_config` | ❌ |
| 10 | 0.361500 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.344797 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.337996 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.323587 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.300133 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.296827 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.291629 | `azmcp_loadtesting_testrun_get` | ❌ |
| 17 | 0.285843 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.274274 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.268824 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.261790 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 255

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565459 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.440493 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.427621 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.385817 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.384192 | `azmcp_appservice_database_add` | ❌ |
| 7 | 0.371461 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.369822 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.368957 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.344860 | `azmcp_sql_db_delete` | ❌ |
| 11 | 0.334213 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.306025 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.273809 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.270133 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.263397 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.250975 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.250753 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.240695 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.230967 | `azmcp_loadtesting_testrun_create` | ❌ |
| 20 | 0.223148 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 256

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.401943 | `azmcp_sql_db_list` | ❌ |
| 2 | 0.395461 | `azmcp_sql_db_rename` | ❌ |
| 3 | 0.394960 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.390086 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 5 | 0.386254 | `azmcp_sql_server_delete` | ❌ |
| 6 | 0.381843 | `azmcp_sql_db_create` | ❌ |
| 7 | 0.349961 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 8 | 0.342514 | `azmcp_sql_elastic-pool_list` | ❌ |
| 9 | 0.339099 | `azmcp_sql_db_delete` | ❌ |
| 10 | 0.333102 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.329381 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.304209 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.301177 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.297506 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 15 | 0.260964 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.257195 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.238640 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.232869 | `azmcp_eventgrid_events_publish` | ❌ |
| 19 | 0.232190 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.223096 | `azmcp_loadtesting_testrun_update` | ❌ |

---

## Test 257

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678124 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.498367 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.479044 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.464794 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.454426 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.450777 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.442892 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.441264 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.434570 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.431174 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.429007 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 13 | 0.394338 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.370652 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.363579 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.357347 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.351647 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.351044 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.349479 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.348658 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 258

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606425 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.457164 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.445343 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.432816 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.423047 | `azmcp_mysql_server_config_config` | ❌ |
| 7 | 0.419753 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.408202 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.400026 | `azmcp_mysql_server_param_param` | ❌ |
| 10 | 0.383940 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.378556 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.341694 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.335615 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.333099 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.330052 | `azmcp_aks_cluster_get` | ❌ |
| 16 | 0.317941 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.312361 | `azmcp_appservice_database_add` | ❌ |
| 18 | 0.304601 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.304317 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.298907 | `azmcp_kusto_database_list` | ❌ |

---

## Test 259

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592808 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420328 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.402644 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.397716 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.397660 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.387303 | `azmcp_aks_nodepool_get` | ❌ |
| 7 | 0.378596 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.357548 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.350845 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.344854 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.344437 | `azmcp_mysql_server_param_param` | ❌ |
| 12 | 0.342895 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.321833 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.315676 | `azmcp_foundry_agents_list` | ❌ |
| 15 | 0.298987 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.292679 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.284204 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.281757 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.259668 | `azmcp_appservice_database_add` | ❌ |
| 20 | 0.259362 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 260

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682647 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563707 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.537173 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.529198 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.482102 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.474207 | `azmcp_sql_db_rename` | ❌ |
| 7 | 0.473675 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.464987 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.451815 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.449920 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.449757 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.402511 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.400967 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.397113 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.352713 | `azmcp_appservice_database_add` | ❌ |
| 16 | 0.335935 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.332831 | `azmcp_extension_azqr` | ❌ |
| 18 | 0.326862 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.323405 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.319939 | `azmcp_acr_registry_list` | ❌ |

---

## Test 261

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618349 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510169 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.472463 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.434810 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.400939 | `azmcp_sql_db_rename` | ❌ |
| 6 | 0.397805 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.396073 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.369890 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.368115 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.367996 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.360875 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.358285 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.354438 | `azmcp_sql_elastic-pool_list` | ❌ |
| 14 | 0.352234 | `azmcp_keyvault_certificate_create` | ❌ |
| 15 | 0.324049 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.316772 | `azmcp_loadtesting_test_create` | ❌ |
| 17 | 0.315901 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.314174 | `azmcp_foundry_agents_connect` | ❌ |
| 19 | 0.300998 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.300788 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 262

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589859 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.501403 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.497890 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.469425 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.461181 | `azmcp_sql_db_rename` | ❌ |
| 6 | 0.442934 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.423887 | `azmcp_sql_server_show` | ❌ |
| 8 | 0.421502 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.417608 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.416146 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.415404 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.385267 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.332965 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.317210 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.312657 | `azmcp_loadtesting_test_create` | ❌ |
| 16 | 0.303177 | `azmcp_keyvault_certificate_create` | ❌ |
| 17 | 0.300973 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.298352 | `azmcp_group_list` | ❌ |
| 19 | 0.288584 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.284680 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 263

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702352 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.518037 | `azmcp_sql_server_list` | ❌ |
| 3 | 0.495549 | `azmcp_sql_server_create` | ❌ |
| 4 | 0.486195 | `azmcp_sql_db_delete` | ❌ |
| 5 | 0.483132 | `azmcp_workbooks_delete` | ❌ |
| 6 | 0.470205 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.469745 | `azmcp_sql_db_rename` | ❌ |
| 8 | 0.449008 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.448515 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.438950 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.346684 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.333270 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.323460 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.317588 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.317267 | `azmcp_group_list` | ❌ |
| 16 | 0.310672 | `azmcp_appservice_database_add` | ❌ |
| 17 | 0.307426 | `azmcp_appconfig_kv_delete` | ❌ |
| 18 | 0.290106 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.275321 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.273131 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 264

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429140 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393885 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.376660 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.350103 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.319865 | `azmcp_sql_db_rename` | ❌ |
| 6 | 0.309280 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 7 | 0.306368 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.301933 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.299788 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.295963 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.274726 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.258333 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.235107 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.234779 | `azmcp_appconfig_kv_delete` | ❌ |
| 15 | 0.234376 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.226608 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.226581 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.225579 | `azmcp_grafana_list` | ❌ |
| 19 | 0.219702 | `azmcp_kusto_table_list` | ❌ |
| 20 | 0.210483 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 265

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
| 6 | 0.293278 | `azmcp_sql_db_rename` | ❌ |
| 7 | 0.274895 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.262381 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 9 | 0.261688 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.254391 | `azmcp_appconfig_kv_delete` | ❌ |
| 11 | 0.247414 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.213567 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.174725 | `azmcp_keyvault_admin_get` | ❌ |
| 14 | 0.168042 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.164350 | `azmcp_loadtesting_testrun_update` | ❌ |
| 16 | 0.159907 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.156253 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.148272 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.146277 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.142127 | `azmcp_kusto_query` | ❌ |

---

## Test 266

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783479 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.456051 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.434868 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.401908 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376055 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.365636 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.352607 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.344454 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.343559 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.329464 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.291758 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.280450 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.258095 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.249297 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.249153 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.246587 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.245298 | `azmcp_group_list` | ❌ |
| 18 | 0.238150 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.234681 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.233337 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 267

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713306 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.413144 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.368082 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.315966 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.311085 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.304891 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 7 | 0.303560 | `azmcp_postgres_server_config_config` | ❌ |
| 8 | 0.289851 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.287372 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.283806 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.273063 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.214529 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.205963 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.197679 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.194313 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.193050 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.191515 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.188120 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.187623 | `azmcp_keyvault_admin_get` | ❌ |
| 20 | 0.182728 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 268

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646419 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.356025 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.322155 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.307923 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.253610 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.236850 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.236050 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.234937 | `azmcp_appservice_database_add` | ❌ |
| 9 | 0.230580 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.228196 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.222889 | `azmcp_sql_db_update` | ❌ |
| 12 | 0.212644 | `azmcp_cloudarchitect_design` | ❌ |
| 13 | 0.210385 | `azmcp_foundry_agents_list` | ❌ |
| 14 | 0.200387 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.189079 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.188299 | `azmcp_deploy_plan_get` | ❌ |
| 17 | 0.180995 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.180728 | `azmcp_foundry_agents_connect` | ❌ |
| 19 | 0.180594 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.174553 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 269

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635467 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532712 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448914 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.432802 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.423223 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.403858 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.397912 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.371540 | `azmcp_sql_db_rename` | ❌ |
| 10 | 0.361266 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.335670 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.318368 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.311149 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.295941 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.290296 | `azmcp_deploy_iac_rules_get` | ❌ |
| 16 | 0.288106 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.265059 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 18 | 0.260209 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.253369 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.251922 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 270

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670189 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533562 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316619 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.295018 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.287497 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.284187 | `azmcp_appservice_database_add` | ❌ |
| 8 | 0.271240 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.252970 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 10 | 0.251854 | `azmcp_sql_db_rename` | ❌ |
| 11 | 0.248493 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.222068 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 13 | 0.179175 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.174851 | `azmcp_deploy_iac_rules_get` | ❌ |
| 15 | 0.174584 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.166725 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.158176 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.156396 | `azmcp_keyvault_key_create` | ❌ |
| 19 | 0.149884 | `azmcp_kusto_query` | ❌ |
| 20 | 0.146012 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 271

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685107 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574336 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539577 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.429026 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.395165 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.356402 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.339841 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.337543 | `azmcp_sql_db_rename` | ❌ |
| 9 | 0.321920 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.316783 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.302214 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.296502 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.283923 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.280636 | `azmcp_postgres_server_param_set` | ❌ |
| 15 | 0.248939 | `azmcp_loadtesting_test_create` | ❌ |
| 16 | 0.221008 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.219182 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.209376 | `azmcp_loadtesting_testrun_create` | ❌ |
| 19 | 0.207284 | `azmcp_loadtesting_testresource_create` | ❌ |
| 20 | 0.197104 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 272

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.543857 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.540333 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.527546 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.436586 | `azmcp_sql_db_delete` | ❌ |
| 6 | 0.418504 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.410574 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.403360 | `azmcp_sql_db_rename` | ❌ |
| 9 | 0.386562 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.341960 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.312054 | `azmcp_appconfig_kv_delete` | ❌ |
| 12 | 0.306396 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.263959 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.246986 | `azmcp_keyvault_secret_get` | ❌ |
| 15 | 0.245270 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 16 | 0.241564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.235230 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.231416 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.225227 | `azmcp_kusto_query` | ❌ |
| 20 | 0.225054 | `azmcp_keyvault_certificate_get` | ❌ |

---

## Test 273

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
| 7 | 0.298249 | `azmcp_sql_db_rename` | ❌ |
| 8 | 0.293097 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.259110 | `azmcp_workbooks_delete` | ❌ |
| 10 | 0.254974 | `azmcp_appconfig_kv_delete` | ❌ |
| 11 | 0.251005 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 12 | 0.227837 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.182013 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.166475 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.158025 | `azmcp_kusto_query` | ❌ |
| 16 | 0.155844 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.153610 | `azmcp_keyvault_secret_get` | ❌ |
| 18 | 0.152458 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.152084 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 20 | 0.149449 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 274

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
| 6 | 0.352131 | `azmcp_sql_db_rename` | ❌ |
| 7 | 0.336482 | `azmcp_sql_db_delete` | ❌ |
| 8 | 0.332209 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.293331 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.291409 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.263966 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.252095 | `azmcp_appconfig_kv_delete` | ❌ |
| 13 | 0.222155 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.204194 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.185585 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.184867 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.183545 | `azmcp_deploy_iac_rules_get` | ❌ |
| 18 | 0.181757 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 19 | 0.180405 | `azmcp_kusto_query` | ❌ |
| 20 | 0.179710 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 275

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729372 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549667 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.418817 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.392512 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.385148 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.359228 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.356700 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.355202 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.304833 | `azmcp_keyvault_secret_list` | ❌ |
| 12 | 0.278098 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.277410 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.276828 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.274634 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.270667 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.263181 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.253852 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.248780 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 20 | 0.243146 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 276

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630731 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.524126 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410680 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.348100 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.316854 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.312035 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.298995 | `azmcp_mysql_server_param_param` | ❌ |
| 9 | 0.294466 | `azmcp_mysql_server_config_config` | ❌ |
| 10 | 0.293371 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.225372 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.217375 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.211187 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.210531 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.209562 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.206476 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.206379 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.206114 | `azmcp_kusto_table_list` | ❌ |
| 19 | 0.197711 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.189871 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 277

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630546 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532454 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473501 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412957 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.350513 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.308004 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.305701 | `azmcp_mysql_server_param_param` | ❌ |
| 8 | 0.304314 | `azmcp_mysql_server_config_config` | ❌ |
| 9 | 0.282623 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.277628 | `azmcp_postgres_server_config_config` | ❌ |
| 11 | 0.221706 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.216178 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.202425 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.200326 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.191165 | `azmcp_cloudarchitect_design` | ❌ |
| 16 | 0.185879 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.177454 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.176225 | `azmcp_get_bestpractices_get` | ❌ |
| 19 | 0.173184 | `azmcp_applens_resource_diagnose` | ❌ |
| 20 | 0.171465 | `azmcp_azureterraformbestpractices_get` | ❌ |

---

## Test 278

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694404 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596686 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.578238 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.515851 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509789 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.500373 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.496921 | `azmcp_group_list` | ❌ |
| 8 | 0.496434 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.495321 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.487648 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.487455 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.473452 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.449346 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.449287 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.419669 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.403710 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.395561 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.384532 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.384389 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.380949 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 279

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618218 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593837 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.542398 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.507404 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.496252 | `azmcp_group_list` | ❌ |
| 6 | 0.495949 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.492324 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.484599 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.477041 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.470456 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.464018 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.449733 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.444259 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.419472 | `azmcp_foundry_agents_list` | ❌ |
| 15 | 0.418009 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.410302 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.397201 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.395060 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.391940 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.384337 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 280

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629737 | `azmcp_sql_db_show` | ❌ |
| 2 | 0.595743 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587649 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.560005 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.540308 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.491746 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.488614 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.482105 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.480427 | `azmcp_mysql_server_config_config` | ❌ |
| 10 | 0.478879 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.445709 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.437084 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.424994 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.409869 | `azmcp_group_list` | ❌ |
| 15 | 0.393859 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.385115 | `azmcp_extension_azqr` | ❌ |
| 17 | 0.383096 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.376402 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.373217 | `azmcp_eventgrid_subscription_list` | ❌ |
| 20 | 0.368310 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 281

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `azmcp_postgres_server_config_config` | ❌ |
| 3 | 0.538034 | `azmcp_mysql_server_config_config` | ❌ |
| 4 | 0.471541 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.445430 | `azmcp_postgres_server_param_param` | ❌ |
| 6 | 0.443977 | `azmcp_mysql_server_param_param` | ❌ |
| 7 | 0.422646 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.414310 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.413964 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.406630 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.400916 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.316789 | `azmcp_appconfig_kv_list` | ❌ |
| 13 | 0.314932 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.308815 | `azmcp_functionapp_get` | ❌ |
| 15 | 0.303979 | `azmcp_aks_cluster_get` | ❌ |
| 16 | 0.300098 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.298409 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.295903 | `azmcp_loadtesting_testrun_list` | ❌ |
| 19 | 0.284481 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 20 | 0.283446 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 282

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `azmcp_postgres_server_config_config` | ❌ |
| 3 | 0.380021 | `azmcp_postgres_server_param_param` | ❌ |
| 4 | 0.372194 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370539 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.368788 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.367031 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.363268 | `azmcp_mysql_server_config_config` | ❌ |
| 9 | 0.361792 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.357960 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.288829 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.276327 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.272034 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.268920 | `azmcp_loadtesting_testrun_get` | ❌ |
| 15 | 0.257199 | `azmcp_appconfig_kv_list` | ❌ |
| 16 | 0.255863 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.253990 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.244972 | `azmcp_keyvault_key_get` | ❌ |
| 19 | 0.242982 | `azmcp_keyvault_secret_get` | ❌ |
| 20 | 0.240682 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 283

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.418473 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.394541 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.374006 | `azmcp_loadtesting_test_create` | ❌ |
| 5 | 0.355049 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.352176 | `azmcp_storage_blob_container_get` | ❌ |
| 7 | 0.325925 | `azmcp_keyvault_secret_create` | ❌ |
| 8 | 0.323501 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.319843 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.315241 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.311941 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.311275 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.305188 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.300283 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.297236 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.289652 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.286778 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.278047 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.277805 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.267474 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 284

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500646 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.400264 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.387021 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.382824 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.377261 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.376141 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.344378 | `azmcp_loadtesting_testresource_create` | ❌ |
| 8 | 0.340104 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.329150 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.324426 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.310887 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.310680 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.310342 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.296308 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.284354 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.284236 | `azmcp_deploy_plan_get` | ❌ |
| 17 | 0.282942 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 18 | 0.280471 | `azmcp_keyvault_certificate_create` | ❌ |
| 19 | 0.280198 | `azmcp_cloudarchitect_design` | ❌ |
| 20 | 0.279391 | `azmcp_eventgrid_events_publish` | ❌ |

---

## Test 285

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589002 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.464611 | `azmcp_storage_blob_container_create` | ❌ |
| 3 | 0.447156 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.437040 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.407412 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.383927 | `azmcp_loadtesting_testresource_create` | ❌ |
| 7 | 0.383855 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.382274 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.380638 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.380503 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.372801 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.372357 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.366543 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.363721 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.360940 | `azmcp_storage_blob_upload` | ❌ |
| 16 | 0.359330 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.325347 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.324944 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.324674 | `azmcp_appservice_database_add` | ❌ |
| 20 | 0.321408 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 286

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655152 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.603485 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.507565 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.483435 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.442816 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.439236 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.431020 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.403478 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.397051 | `azmcp_mysql_server_config_config` | ❌ |
| 10 | 0.395698 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.373964 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.373146 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.370115 | `azmcp_keyvault_admin_get` | ❌ |
| 14 | 0.370041 | `azmcp_keyvault_key_get` | ❌ |
| 15 | 0.368567 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.367354 | `azmcp_subscription_list` | ❌ |
| 17 | 0.367049 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.363322 | `azmcp_search_index_get` | ❌ |
| 19 | 0.356973 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.349157 | `azmcp_functionapp_get` | ❌ |

---

## Test 287

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676876 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.612665 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.518215 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.515103 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.415410 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.411765 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.401802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.380012 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.375790 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.375778 | `azmcp_keyvault_key_get` | ❌ |
| 11 | 0.369755 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.368337 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.368023 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.362602 | `azmcp_mysql_server_config_config` | ❌ |
| 15 | 0.362212 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.355093 | `azmcp_servicebus_queue_details` | ❌ |
| 17 | 0.354841 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.353261 | `azmcp_keyvault_admin_get` | ❌ |
| 19 | 0.351364 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.331854 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 288

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664087 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.556998 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.536909 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.535616 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.501198 | `azmcp_subscription_list` | ❌ |
| 6 | 0.496371 | `azmcp_quota_region_availability_list` | ❌ |
| 7 | 0.493246 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.484516 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.484163 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.473387 | `azmcp_search_service_list` | ❌ |
| 11 | 0.458793 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.454195 | `azmcp_acr_registry_list` | ❌ |
| 13 | 0.445545 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.441838 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.439919 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.438660 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.432645 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.416486 | `azmcp_group_list` | ❌ |
| 19 | 0.412679 | `azmcp_grafana_list` | ❌ |
| 20 | 0.404090 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 289

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499302 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.461284 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.455480 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.421642 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.392348 | `azmcp_keyvault_admin_get` | ❌ |
| 6 | 0.379853 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.378295 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 8 | 0.375553 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.367906 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.366021 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.362252 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.360585 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.347173 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.346039 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.344771 | `azmcp_search_service_list` | ❌ |
| 16 | 0.342241 | `azmcp_subscription_list` | ❌ |
| 17 | 0.335345 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.330862 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.322108 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.315590 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 290

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557142 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.473598 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.461777 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.454063 | `azmcp_subscription_list` | ❌ |
| 5 | 0.436170 | `azmcp_search_service_list` | ❌ |
| 6 | 0.432855 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.425048 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.418403 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.416483 | `azmcp_keyvault_admin_get` | ❌ |
| 10 | 0.415764 | `azmcp_storage_blob_get` | ❌ |
| 11 | 0.415080 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.389930 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.379856 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.377201 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.376646 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.374657 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.371828 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.359998 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.359053 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.356591 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 291

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563396 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.524779 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.507841 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.447784 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.403407 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.335039 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.331351 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.326352 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.324867 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.323215 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.322465 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.318855 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.305680 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.297890 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 15 | 0.297384 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.292093 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.291137 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.281907 | `azmcp_sql_server_create` | ❌ |
| 19 | 0.280865 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.274863 | `azmcp_workbooks_create` | ❌ |

---

## Test 292

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512578 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.500625 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.470838 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415378 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.414730 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.368859 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.334039 | `azmcp_storage_blob_upload` | ❌ |
| 8 | 0.320168 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 9 | 0.309739 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.296899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.296438 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.285153 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.278047 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.275240 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.275199 | `azmcp_keyvault_key_create` | ❌ |
| 16 | 0.270168 | `azmcp_appconfig_kv_set` | ❌ |
| 17 | 0.269625 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.268922 | `azmcp_workbooks_create` | ❌ |
| 19 | 0.256599 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.249658 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 293

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463198 | `azmcp_storage_account_create` | ❌ |
| 2 | 0.455028 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.451690 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 4 | 0.435099 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.388387 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.378021 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.366330 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.329038 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.322364 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.314128 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.309104 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.287885 | `azmcp_workbooks_create` | ❌ |
| 13 | 0.280806 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.277049 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.276533 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.275617 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.269719 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.266791 | `azmcp_appconfig_kv_set` | ❌ |
| 19 | 0.265284 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.265228 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 294

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664314 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.559177 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.523289 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.518678 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.496184 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.461577 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.421964 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.421228 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.384585 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.377009 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.367759 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.359339 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.354913 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.353561 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.350264 | `azmcp_mysql_server_config_config` | ❌ |
| 16 | 0.335714 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.334806 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.333155 | `azmcp_keyvault_admin_get` | ❌ |
| 19 | 0.332134 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.308777 | `azmcp_mysql_server_list` | ❌ |

---

## Test 295

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613933 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.605603 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 3 | 0.521919 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.479014 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.471385 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.453044 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.409820 | `azmcp_acr_registry_repository_list` | ❌ |
| 8 | 0.404640 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.393989 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.386144 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.367207 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.359501 | `azmcp_subscription_list` | ❌ |
| 13 | 0.359465 | `azmcp_search_service_list` | ❌ |
| 14 | 0.356400 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.353319 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.351601 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.351266 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.333677 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.333282 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.333091 | `azmcp_monitor_table_list` | ❌ |

---

## Test 296

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625281 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.511261 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.439698 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.437887 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.429622 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.418128 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.405678 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.390261 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.384096 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.355955 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.354374 | `azmcp_search_service_list` | ❌ |
| 13 | 0.352484 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.348138 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.347296 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.346936 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.345644 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.340849 | `azmcp_subscription_list` | ❌ |
| 19 | 0.336549 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.327093 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 297

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613064 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.585418 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.483564 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.477956 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.434608 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.420736 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.408488 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.386468 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.359444 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.349551 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.345545 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.338017 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.333829 | `azmcp_mysql_server_config_config` | ❌ |
| 14 | 0.330937 | `azmcp_storage_blob_upload` | ❌ |
| 15 | 0.326511 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.323062 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.321138 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.318358 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.303472 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.297503 | `azmcp_keyvault_admin_get` | ❌ |

---

## Test 298

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661761 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.661627 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.537528 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.460612 | `azmcp_storage_blob_container_create` | ❌ |
| 5 | 0.457071 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.453694 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.370171 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.360753 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.358395 | `azmcp_storage_blob_upload` | ❌ |
| 10 | 0.353456 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.353123 | `azmcp_workbooks_show` | ❌ |
| 12 | 0.352636 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.348574 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.348390 | `azmcp_keyvault_key_get` | ❌ |
| 15 | 0.337018 | `azmcp_mysql_server_config_config` | ❌ |
| 16 | 0.334204 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.329772 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.319533 | `azmcp_keyvault_certificate_get` | ❌ |
| 19 | 0.319401 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.314168 | `azmcp_functionapp_get` | ❌ |

---

## Test 299

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592363 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.579070 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.568333 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 4 | 0.465942 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.452160 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.415853 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.413279 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.400483 | `azmcp_acr_registry_repository_list` | ❌ |
| 9 | 0.394852 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.379851 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.379099 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.369271 | `azmcp_keyvault_secret_list` | ❌ |
| 13 | 0.361689 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.359099 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.348875 | `azmcp_subscription_list` | ❌ |
| 16 | 0.340190 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.331563 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.328193 | `azmcp_search_service_list` | ❌ |
| 19 | 0.313259 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.310914 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 300

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.569787 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.549387 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.533515 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.449128 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.433883 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.397367 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.395810 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.385243 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.362337 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.353799 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.345263 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.342756 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.339846 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.336142 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.314069 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.308890 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.307023 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.300295 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.298968 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.294888 | `azmcp_subscription_list` | ❌ |

---

## Test 301

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566287 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.403531 | `azmcp_storage_blob_get` | ❌ |
| 3 | 0.397524 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.382123 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.377255 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.351920 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.327416 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.324049 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.294744 | `azmcp_keyvault_certificate_import` | ❌ |
| 10 | 0.284896 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.284377 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.273730 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 13 | 0.273513 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.272315 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 15 | 0.257845 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.254581 | `azmcp_workbooks_delete` | ❌ |
| 17 | 0.253369 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.239522 | `azmcp_foundry_models_deploy` | ❌ |
| 19 | 0.211052 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.210171 | `azmcp_quota_usage_check` | ❌ |

---

## Test 302

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576101 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.473852 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471653 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.465428 | `azmcp_eventgrid_subscription_list` | ❌ |
| 6 | 0.452471 | `azmcp_search_service_list` | ❌ |
| 7 | 0.450973 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.445724 | `azmcp_grafana_list` | ❌ |
| 9 | 0.431337 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.430382 | `azmcp_group_list` | ❌ |
| 11 | 0.422803 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.406935 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.388737 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.380636 | `azmcp_marketplace_product_list` | ❌ |
| 15 | 0.367761 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.366860 | `azmcp_loadtesting_testresource_list` | ❌ |
| 17 | 0.355384 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.354246 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.348524 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.345154 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 303

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405770 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.381238 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.380789 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.351864 | `azmcp_grafana_list` | ❌ |
| 5 | 0.350951 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.341813 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.334829 | `azmcp_eventgrid_topic_list` | ❌ |
| 8 | 0.328109 | `azmcp_search_service_list` | ❌ |
| 9 | 0.315604 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.308874 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.303528 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.303367 | `azmcp_marketplace_product_list` | ❌ |
| 13 | 0.297296 | `azmcp_group_list` | ❌ |
| 14 | 0.296282 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.295243 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.285434 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.275417 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.269922 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.258047 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.237900 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 304

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.320021 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.315602 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.307697 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.286711 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.282645 | `azmcp_grafana_list` | ❌ |
| 6 | 0.279702 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.278798 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.273758 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.256358 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.254815 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.252965 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.252504 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.251683 | `azmcp_search_service_list` | ❌ |
| 14 | 0.251368 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.233143 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.230571 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.230324 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.226446 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.222799 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.216584 | `azmcp_group_list` | ❌ |

---

## Test 305

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.403307 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.370693 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.354504 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.342318 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.340339 | `azmcp_grafana_list` | ❌ |
| 6 | 0.336798 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.311939 | `azmcp_search_service_list` | ❌ |
| 8 | 0.311109 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.305185 | `azmcp_marketplace_product_get` | ❌ |
| 10 | 0.304965 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.302355 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.300478 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.294080 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.291826 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.282326 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.281294 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.274224 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.269972 | `azmcp_group_list` | ❌ |
| 19 | 0.233362 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.228634 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 306

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625270 | `azmcp_deploy_iac_rules_get` | ❌ |
| 3 | 0.605048 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.482697 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.465661 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.431102 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.388538 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.386480 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.372596 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369184 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.362343 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.354086 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.339022 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.333210 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.312592 | `azmcp_mysql_server_config_config` | ❌ |
| 16 | 0.310274 | `azmcp_mysql_table_schema_schema` | ❌ |
| 17 | 0.305259 | `azmcp_mysql_database_query` | ❌ |
| 18 | 0.303849 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.302307 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.300825 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 307

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.523828 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.512141 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.510005 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.474447 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.443943 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 7 | 0.439535 | `azmcp_keyvault_secret_create` | ❌ |
| 8 | 0.439375 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.428954 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.389450 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.304912 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.304137 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.300776 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.292743 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.285517 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.281261 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.279035 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.278638 | `azmcp_mysql_server_config_config` | ❌ |
| 19 | 0.276708 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.274691 | `azmcp_subscription_list` | ❌ |

---

## Test 308

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
| 7 | 0.535116 | `azmcp_aks_nodepool_get` | ❌ |
| 8 | 0.527948 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.525637 | `azmcp_sql_elastic-pool_list` | ❌ |
| 10 | 0.506608 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.505239 | `azmcp_subscription_list` | ❌ |
| 12 | 0.496298 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.495490 | `azmcp_grafana_list` | ❌ |
| 14 | 0.492515 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.476824 | `azmcp_group_list` | ❌ |
| 16 | 0.465390 | `azmcp_aks_cluster_get` | ❌ |
| 17 | 0.463069 | `azmcp_eventgrid_topic_list` | ❌ |
| 18 | 0.460388 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.459250 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.459072 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 309

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714468 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.423250 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.393721 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.364696 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.362307 | `azmcp_search_service_list` | ❌ |
| 8 | 0.359417 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.344853 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.337530 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.335295 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.333517 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.332894 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.328623 | `azmcp_keyvault_key_list` | ❌ |
| 15 | 0.324603 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.312156 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.311262 | `azmcp_grafana_list` | ❌ |
| 18 | 0.308168 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.302706 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.294394 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 310

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812659 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.659213 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501168 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.336848 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.336385 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.327422 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.324750 | `azmcp_subscription_list` | ❌ |
| 8 | 0.324289 | `azmcp_search_service_list` | ❌ |
| 9 | 0.316295 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.315778 | `azmcp_loadtesting_testrun_list` | ❌ |
| 11 | 0.305449 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.304414 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.300364 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.299696 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.297320 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.295899 | `azmcp_grafana_list` | ❌ |
| 17 | 0.284934 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.278813 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.278222 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.277176 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 311

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
| 7 | 0.217263 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.214818 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.188137 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.173308 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.169440 | `azmcp_grafana_list` | ❌ |
| 12 | 0.164006 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.153950 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.148897 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.147365 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.143979 | `azmcp_sql_db_rename` | ❌ |
| 17 | 0.130524 | `azmcp_loadtesting_testrun_create` | ❌ |
| 18 | 0.130432 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.116803 | `azmcp_loadtesting_testrun_update` | ❌ |
| 20 | 0.114110 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 312

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
| 13 | 0.148882 | `azmcp_extension_azqr` | ❌ |
| 14 | 0.145141 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.134979 | `azmcp_loadtesting_testrun_update` | ❌ |
| 16 | 0.132504 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.131856 | `azmcp_group_list` | ❌ |
| 18 | 0.122450 | `azmcp_loadtesting_test_get` | ❌ |
| 19 | 0.119436 | `azmcp_loadtesting_testresource_create` | ❌ |
| 20 | 0.114247 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 313

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772430 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.532565 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.516739 | `azmcp_grafana_list` | ❌ |
| 5 | 0.488597 | `azmcp_group_list` | ❌ |
| 6 | 0.487920 | `azmcp_workbooks_delete` | ❌ |
| 7 | 0.459976 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.454209 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.439944 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.428781 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.417281 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.413409 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.405963 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.405949 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.399758 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.365302 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.362740 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.357204 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.352940 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.349674 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 314

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708628 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570203 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.539937 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.485451 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.472443 | `azmcp_grafana_list` | ❌ |
| 6 | 0.428061 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.425437 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.422845 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421751 | `azmcp_group_list` | ❌ |
| 10 | 0.412437 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.392416 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.381006 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.380408 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.371197 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.363786 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.350841 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.338366 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.338225 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.334576 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.333154 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 315

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
| 6 | 0.353547 | `azmcp_grafana_list` | ❌ |
| 7 | 0.277807 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.264649 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.256678 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.250024 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.236741 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.225294 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.218999 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.207693 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.197245 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.195391 | `azmcp_group_list` | ❌ |
| 17 | 0.189900 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.189657 | `azmcp_extension_azqr` | ❌ |
| 19 | 0.187800 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.187564 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 316

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
| 7 | 0.267409 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.239907 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.227366 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.176481 | `azmcp_role_assignment_list` | ❌ |
| 11 | 0.175883 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.174513 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.173251 | `azmcp_postgres_table_list` | ❌ |
| 14 | 0.168191 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.165774 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.154760 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.152608 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.149678 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.146055 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.141970 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 317

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
| 7 | 0.174326 | `azmcp_sql_db_update` | ❌ |
| 8 | 0.170118 | `azmcp_grafana_list` | ❌ |
| 9 | 0.148730 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.145636 | `azmcp_sql_db_rename` | ❌ |
| 11 | 0.142518 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 12 | 0.142186 | `azmcp_loadtesting_testrun_create` | ❌ |
| 13 | 0.138354 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.136105 | `azmcp_loadtesting_testresource_create` | ❌ |
| 15 | 0.132979 | `azmcp_foundry_agents_evaluate` | ❌ |
| 16 | 0.131038 | `azmcp_postgres_database_query` | ❌ |
| 17 | 0.129989 | `azmcp_postgres_server_param_set` | ❌ |
| 18 | 0.129660 | `azmcp_deploy_iac_rules_get` | ❌ |
| 19 | 0.116768 | `azmcp_appservice_database_add` | ❌ |
| 20 | 0.113470 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 318

**Expected Tool:** `azmcp_bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485889 | `azmcp_deploy_iac_rules_get` | ❌ |
| 2 | 0.448373 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.439987 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.432411 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.432409 | `azmcp_bicepschema_get` | ✅ **EXPECTED** |
| 6 | 0.400985 | `azmcp_foundry_models_deploy` | ❌ |
| 7 | 0.398834 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.393541 | `azmcp_foundry_agents_connect` | ❌ |
| 9 | 0.391625 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 10 | 0.385433 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.372097 | `azmcp_search_service_list` | ❌ |
| 12 | 0.325716 | `azmcp_search_index_query` | ❌ |
| 13 | 0.324745 | `azmcp_search_index_get` | ❌ |
| 14 | 0.317232 | `azmcp_sql_db_create` | ❌ |
| 15 | 0.303183 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.291291 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.281487 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.279983 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.274770 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.270531 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 319

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.349336 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290902 | `azmcp_storage_blob_upload` | ❌ |
| 3 | 0.254656 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.221285 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.217623 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.216162 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 7 | 0.191391 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.191268 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.178575 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.175833 | `azmcp_deploy_iac_rules_get` | ❌ |
| 11 | 0.160403 | `azmcp_eventgrid_events_publish` | ❌ |
| 12 | 0.136825 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.135768 | `azmcp_get_bestpractices_get` | ❌ |
| 14 | 0.132826 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.118383 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.115898 | `azmcp_marketplace_product_get` | ❌ |
| 17 | 0.111404 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.106651 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.104162 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.100892 | `azmcp_mysql_table_schema_schema` | ❌ |

---

## Test 320

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.290270 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.267319 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258113 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.225803 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.215748 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.207352 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.195387 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.189220 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.179120 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.170158 | `azmcp_foundry_agents_connect` | ❌ |
| 11 | 0.168850 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.163694 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.163615 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.162199 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.160743 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.154249 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.152324 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.145124 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.139758 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.135801 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 321

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.299640 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271834 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265274 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242716 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.218064 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.213173 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.179199 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.169727 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.164328 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.156441 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.156119 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.151357 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.142854 | `azmcp_marketplace_product_list` | ❌ |
| 14 | 0.139970 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.138067 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.132355 | `azmcp_mysql_database_query` | ❌ |
| 17 | 0.130132 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.123936 | `azmcp_storage_blob_upload` | ❌ |
| 19 | 0.119586 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.114994 | `azmcp_mysql_table_schema_schema` | ❌ |

---

## Test 322

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.420259 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369866 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.352763 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.323920 | `azmcp_storage_blob_upload` | ❌ |
| 5 | 0.310372 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.306967 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.304209 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.300393 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.299479 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.298989 | `azmcp_get_bestpractices_get` | ❌ |
| 11 | 0.293806 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.292455 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.291878 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.281706 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.275997 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.275550 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.272671 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.261446 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.259814 | `azmcp_search_service_list` | ❌ |
| 20 | 0.258445 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Summary

**Total Prompts Tested:** 322  
**Execution Time:** 67.0582103s  

### Success Rate Metrics

**Top Choice Success:** 80.1% (258/322 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 4.0% (13/322 tests)  
**🎯 High Confidence (≥0.7):** 18.3% (59/322 tests)  
**✅ Good Confidence (≥0.6):** 54.7% (176/322 tests)  
**👍 Fair Confidence (≥0.5):** 81.4% (262/322 tests)  
**👌 Acceptable Confidence (≥0.4):** 90.1% (290/322 tests)  
**❌ Low Confidence (<0.4):** 9.9% (32/322 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 4.0% (13/322 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 18.3% (59/322 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 52.2% (168/322 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 73.0% (235/322 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 77.3% (249/322 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing adequately but has room for improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

