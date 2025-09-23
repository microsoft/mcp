# Tool Selection Analysis Setup

**Setup completed:** 2025-09-23 11:39:07  
**Tool count:** 143  
**Database setup time:** 2.5202320s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-23 11:39:07  
**Tool count:** 143  

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
- [Test 114: azmcp_functionapp_get](#test-114)
- [Test 115: azmcp_functionapp_get](#test-115)
- [Test 116: azmcp_functionapp_get](#test-116)
- [Test 117: azmcp_functionapp_get](#test-117)
- [Test 118: azmcp_functionapp_get](#test-118)
- [Test 119: azmcp_functionapp_get](#test-119)
- [Test 120: azmcp_functionapp_get](#test-120)
- [Test 121: azmcp_functionapp_get](#test-121)
- [Test 122: azmcp_functionapp_get](#test-122)
- [Test 123: azmcp_functionapp_get](#test-123)
- [Test 124: azmcp_functionapp_get](#test-124)
- [Test 125: azmcp_functionapp_get](#test-125)
- [Test 126: azmcp_keyvault_certificate_create](#test-126)
- [Test 127: azmcp_keyvault_certificate_get](#test-127)
- [Test 128: azmcp_keyvault_certificate_get](#test-128)
- [Test 129: azmcp_keyvault_certificate_import](#test-129)
- [Test 130: azmcp_keyvault_certificate_import](#test-130)
- [Test 131: azmcp_keyvault_certificate_list](#test-131)
- [Test 132: azmcp_keyvault_certificate_list](#test-132)
- [Test 133: azmcp_keyvault_key_create](#test-133)
- [Test 134: azmcp_keyvault_key_get](#test-134)
- [Test 135: azmcp_keyvault_key_get](#test-135)
- [Test 136: azmcp_keyvault_key_list](#test-136)
- [Test 137: azmcp_keyvault_key_list](#test-137)
- [Test 138: azmcp_keyvault_secret_create](#test-138)
- [Test 139: azmcp_keyvault_secret_get](#test-139)
- [Test 140: azmcp_keyvault_secret_get](#test-140)
- [Test 141: azmcp_keyvault_secret_list](#test-141)
- [Test 142: azmcp_keyvault_secret_list](#test-142)
- [Test 143: azmcp_aks_cluster_get](#test-143)
- [Test 144: azmcp_aks_cluster_get](#test-144)
- [Test 145: azmcp_aks_cluster_get](#test-145)
- [Test 146: azmcp_aks_cluster_get](#test-146)
- [Test 147: azmcp_aks_cluster_list](#test-147)
- [Test 148: azmcp_aks_cluster_list](#test-148)
- [Test 149: azmcp_aks_cluster_list](#test-149)
- [Test 150: azmcp_aks_nodepool_get](#test-150)
- [Test 151: azmcp_aks_nodepool_get](#test-151)
- [Test 152: azmcp_aks_nodepool_get](#test-152)
- [Test 153: azmcp_aks_nodepool_list](#test-153)
- [Test 154: azmcp_aks_nodepool_list](#test-154)
- [Test 155: azmcp_aks_nodepool_list](#test-155)
- [Test 156: azmcp_loadtesting_test_create](#test-156)
- [Test 157: azmcp_loadtesting_test_get](#test-157)
- [Test 158: azmcp_loadtesting_testresource_create](#test-158)
- [Test 159: azmcp_loadtesting_testresource_list](#test-159)
- [Test 160: azmcp_loadtesting_testrun_create](#test-160)
- [Test 161: azmcp_loadtesting_testrun_get](#test-161)
- [Test 162: azmcp_loadtesting_testrun_list](#test-162)
- [Test 163: azmcp_loadtesting_testrun_update](#test-163)
- [Test 164: azmcp_grafana_list](#test-164)
- [Test 165: azmcp_azuremanagedlustre_filesystem_list](#test-165)
- [Test 166: azmcp_azuremanagedlustre_filesystem_list](#test-166)
- [Test 167: azmcp_azuremanagedlustre_filesystem_required-subnet-size](#test-167)
- [Test 168: azmcp_azuremanagedlustre_filesystem_sku_get](#test-168)
- [Test 169: azmcp_marketplace_product_get](#test-169)
- [Test 170: azmcp_marketplace_product_list](#test-170)
- [Test 171: azmcp_marketplace_product_list](#test-171)
- [Test 172: azmcp_bestpractices_get](#test-172)
- [Test 173: azmcp_bestpractices_get](#test-173)
- [Test 174: azmcp_bestpractices_get](#test-174)
- [Test 175: azmcp_bestpractices_get](#test-175)
- [Test 176: azmcp_bestpractices_get](#test-176)
- [Test 177: azmcp_bestpractices_get](#test-177)
- [Test 178: azmcp_bestpractices_get](#test-178)
- [Test 179: azmcp_bestpractices_get](#test-179)
- [Test 180: azmcp_bestpractices_get](#test-180)
- [Test 181: azmcp_bestpractices_get](#test-181)
- [Test 182: azmcp_monitor_healthmodels_entity_gethealth](#test-182)
- [Test 183: azmcp_monitor_metrics_definitions](#test-183)
- [Test 184: azmcp_monitor_metrics_definitions](#test-184)
- [Test 185: azmcp_monitor_metrics_definitions](#test-185)
- [Test 186: azmcp_monitor_metrics_query](#test-186)
- [Test 187: azmcp_monitor_metrics_query](#test-187)
- [Test 188: azmcp_monitor_metrics_query](#test-188)
- [Test 189: azmcp_monitor_metrics_query](#test-189)
- [Test 190: azmcp_monitor_metrics_query](#test-190)
- [Test 191: azmcp_monitor_metrics_query](#test-191)
- [Test 192: azmcp_monitor_resource_log_query](#test-192)
- [Test 193: azmcp_monitor_table_list](#test-193)
- [Test 194: azmcp_monitor_table_list](#test-194)
- [Test 195: azmcp_monitor_table_type_list](#test-195)
- [Test 196: azmcp_monitor_table_type_list](#test-196)
- [Test 197: azmcp_monitor_workspace_list](#test-197)
- [Test 198: azmcp_monitor_workspace_list](#test-198)
- [Test 199: azmcp_monitor_workspace_list](#test-199)
- [Test 200: azmcp_monitor_workspace_log_query](#test-200)
- [Test 201: azmcp_datadog_monitoredresources_list](#test-201)
- [Test 202: azmcp_datadog_monitoredresources_list](#test-202)
- [Test 203: azmcp_extension_azqr](#test-203)
- [Test 204: azmcp_extension_azqr](#test-204)
- [Test 205: azmcp_extension_azqr](#test-205)
- [Test 206: azmcp_quota_region_availability_list](#test-206)
- [Test 207: azmcp_quota_usage_check](#test-207)
- [Test 208: azmcp_role_assignment_list](#test-208)
- [Test 209: azmcp_role_assignment_list](#test-209)
- [Test 210: azmcp_redis_cache_accesspolicy_list](#test-210)
- [Test 211: azmcp_redis_cache_accesspolicy_list](#test-211)
- [Test 212: azmcp_redis_cache_list](#test-212)
- [Test 213: azmcp_redis_cache_list](#test-213)
- [Test 214: azmcp_redis_cache_list](#test-214)
- [Test 215: azmcp_redis_cluster_database_list](#test-215)
- [Test 216: azmcp_redis_cluster_database_list](#test-216)
- [Test 217: azmcp_redis_cluster_list](#test-217)
- [Test 218: azmcp_redis_cluster_list](#test-218)
- [Test 219: azmcp_redis_cluster_list](#test-219)
- [Test 220: azmcp_group_list](#test-220)
- [Test 221: azmcp_group_list](#test-221)
- [Test 222: azmcp_group_list](#test-222)
- [Test 223: azmcp_resourcehealth_availability-status_get](#test-223)
- [Test 224: azmcp_resourcehealth_availability-status_get](#test-224)
- [Test 225: azmcp_resourcehealth_availability-status_get](#test-225)
- [Test 226: azmcp_resourcehealth_availability-status_list](#test-226)
- [Test 227: azmcp_resourcehealth_availability-status_list](#test-227)
- [Test 228: azmcp_resourcehealth_availability-status_list](#test-228)
- [Test 229: azmcp_resourcehealth_service-health-events_list](#test-229)
- [Test 230: azmcp_resourcehealth_service-health-events_list](#test-230)
- [Test 231: azmcp_resourcehealth_service-health-events_list](#test-231)
- [Test 232: azmcp_resourcehealth_service-health-events_list](#test-232)
- [Test 233: azmcp_resourcehealth_service-health-events_list](#test-233)
- [Test 234: azmcp_servicebus_queue_details](#test-234)
- [Test 235: azmcp_servicebus_topic_details](#test-235)
- [Test 236: azmcp_servicebus_topic_subscription_details](#test-236)
- [Test 237: azmcp_sql_db_create](#test-237)
- [Test 238: azmcp_sql_db_create](#test-238)
- [Test 239: azmcp_sql_db_create](#test-239)
- [Test 240: azmcp_sql_db_delete](#test-240)
- [Test 241: azmcp_sql_db_delete](#test-241)
- [Test 242: azmcp_sql_db_delete](#test-242)
- [Test 243: azmcp_sql_db_list](#test-243)
- [Test 244: azmcp_sql_db_list](#test-244)
- [Test 245: azmcp_sql_db_show](#test-245)
- [Test 246: azmcp_sql_db_show](#test-246)
- [Test 247: azmcp_sql_db_update](#test-247)
- [Test 248: azmcp_sql_db_update](#test-248)
- [Test 249: azmcp_sql_elastic-pool_list](#test-249)
- [Test 250: azmcp_sql_elastic-pool_list](#test-250)
- [Test 251: azmcp_sql_elastic-pool_list](#test-251)
- [Test 252: azmcp_sql_server_create](#test-252)
- [Test 253: azmcp_sql_server_create](#test-253)
- [Test 254: azmcp_sql_server_create](#test-254)
- [Test 255: azmcp_sql_server_delete](#test-255)
- [Test 256: azmcp_sql_server_delete](#test-256)
- [Test 257: azmcp_sql_server_delete](#test-257)
- [Test 258: azmcp_sql_server_entra-admin_list](#test-258)
- [Test 259: azmcp_sql_server_entra-admin_list](#test-259)
- [Test 260: azmcp_sql_server_entra-admin_list](#test-260)
- [Test 261: azmcp_sql_server_firewall-rule_create](#test-261)
- [Test 262: azmcp_sql_server_firewall-rule_create](#test-262)
- [Test 263: azmcp_sql_server_firewall-rule_create](#test-263)
- [Test 264: azmcp_sql_server_firewall-rule_delete](#test-264)
- [Test 265: azmcp_sql_server_firewall-rule_delete](#test-265)
- [Test 266: azmcp_sql_server_firewall-rule_delete](#test-266)
- [Test 267: azmcp_sql_server_firewall-rule_list](#test-267)
- [Test 268: azmcp_sql_server_firewall-rule_list](#test-268)
- [Test 269: azmcp_sql_server_firewall-rule_list](#test-269)
- [Test 270: azmcp_sql_server_list](#test-270)
- [Test 271: azmcp_sql_server_list](#test-271)
- [Test 272: azmcp_sql_server_show](#test-272)
- [Test 273: azmcp_sql_server_show](#test-273)
- [Test 274: azmcp_sql_server_show](#test-274)
- [Test 275: azmcp_storage_account_create](#test-275)
- [Test 276: azmcp_storage_account_create](#test-276)
- [Test 277: azmcp_storage_account_create](#test-277)
- [Test 278: azmcp_storage_account_get](#test-278)
- [Test 279: azmcp_storage_account_get](#test-279)
- [Test 280: azmcp_storage_account_get](#test-280)
- [Test 281: azmcp_storage_account_get](#test-281)
- [Test 282: azmcp_storage_account_get](#test-282)
- [Test 283: azmcp_storage_blob_container_create](#test-283)
- [Test 284: azmcp_storage_blob_container_create](#test-284)
- [Test 285: azmcp_storage_blob_container_create](#test-285)
- [Test 286: azmcp_storage_blob_container_get](#test-286)
- [Test 287: azmcp_storage_blob_container_get](#test-287)
- [Test 288: azmcp_storage_blob_container_get](#test-288)
- [Test 289: azmcp_storage_blob_get](#test-289)
- [Test 290: azmcp_storage_blob_get](#test-290)
- [Test 291: azmcp_storage_blob_get](#test-291)
- [Test 292: azmcp_storage_blob_get](#test-292)
- [Test 293: azmcp_storage_blob_upload](#test-293)
- [Test 294: azmcp_subscription_list](#test-294)
- [Test 295: azmcp_subscription_list](#test-295)
- [Test 296: azmcp_subscription_list](#test-296)
- [Test 297: azmcp_subscription_list](#test-297)
- [Test 298: azmcp_azureterraformbestpractices_get](#test-298)
- [Test 299: azmcp_azureterraformbestpractices_get](#test-299)
- [Test 300: azmcp_virtualdesktop_hostpool_list](#test-300)
- [Test 301: azmcp_virtualdesktop_hostpool_sessionhost_list](#test-301)
- [Test 302: azmcp_virtualdesktop_hostpool_sessionhost_usersession-list](#test-302)
- [Test 303: azmcp_workbooks_create](#test-303)
- [Test 304: azmcp_workbooks_delete](#test-304)
- [Test 305: azmcp_workbooks_list](#test-305)
- [Test 306: azmcp_workbooks_list](#test-306)
- [Test 307: azmcp_workbooks_show](#test-307)
- [Test 308: azmcp_workbooks_show](#test-308)
- [Test 309: azmcp_workbooks_update](#test-309)
- [Test 310: azmcp_bicepschema_get](#test-310)
- [Test 311: azmcp_cloudarchitect_design](#test-311)
- [Test 312: azmcp_cloudarchitect_design](#test-312)
- [Test 313: azmcp_cloudarchitect_design](#test-313)
- [Test 314: azmcp_cloudarchitect_design](#test-314)

---

## Test 1

**Expected Tool:** `azmcp_foundry_agents_connect`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603237 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.535783 | `azmcp_foundry_agents_connect` | ✅ **EXPECTED** |
| 3 | 0.494611 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.443011 | `azmcp_foundry_agents_evaluate` | ❌ |
| 5 | 0.379587 | `azmcp_search_index_query` | ❌ |
| 6 | 0.365856 | `azmcp_foundry_models_list` | ❌ |
| 7 | 0.355388 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 8 | 0.327613 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.319855 | `azmcp_foundry_models_deploy` | ❌ |
| 10 | 0.305579 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.297622 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 12 | 0.272398 | `azmcp_search_service_list` | ❌ |
| 13 | 0.243499 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.241205 | `azmcp_postgres_database_query` | ❌ |
| 15 | 0.232234 | `azmcp_search_index_get` | ❌ |
| 16 | 0.230945 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.226514 | `azmcp_monitor_workspace_log_query` | ❌ |
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
| 1 | 0.543845 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469428 | `azmcp_foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.356452 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.280833 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.235483 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.233739 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.233359 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.232102 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.228550 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.224773 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.220966 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.218372 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.214507 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.210219 | `azmcp_search_index_query` | ❌ |
| 15 | 0.207615 | `azmcp_postgres_database_query` | ❌ |
| 16 | 0.207578 | `azmcp_loadtesting_testrun_list` | ❌ |
| 17 | 0.203902 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 18 | 0.194160 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.187851 | `azmcp_mysql_table_schema_get` | ❌ |
| 20 | 0.183167 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 3

**Expected Tool:** `azmcp_foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my AI Foundry project for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580633 | `azmcp_foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 2 | 0.518655 | `azmcp_foundry_agents_evaluate` | ❌ |
| 3 | 0.471064 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.382070 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.315849 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.315347 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.308767 | `azmcp_foundry_models_deploy` | ❌ |
| 8 | 0.276459 | `azmcp_foundry_models_list` | ❌ |
| 9 | 0.253335 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 10 | 0.246328 | `azmcp_search_index_query` | ❌ |
| 11 | 0.231465 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.207748 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.188340 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.183764 | `azmcp_postgres_database_query` | ❌ |
| 15 | 0.179159 | `azmcp_search_service_list` | ❌ |
| 16 | 0.166181 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.163051 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 18 | 0.162163 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.153687 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.152762 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 4

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695139 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.532990 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.526733 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 4 | 0.433117 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.422230 | `azmcp_search_index_get` | ❌ |
| 6 | 0.412895 | `azmcp_search_service_list` | ❌ |
| 7 | 0.349506 | `azmcp_search_index_query` | ❌ |
| 8 | 0.329682 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.310470 | `azmcp_foundry_models_deployments_list` | ❌ |
| 10 | 0.309140 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.296877 | `azmcp_grafana_list` | ❌ |
| 12 | 0.291601 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.286074 | `azmcp_monitor_table_type_list` | ❌ |
| 14 | 0.279802 | `azmcp_keyvault_key_list` | ❌ |
| 15 | 0.270212 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.270204 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.267906 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.265827 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.264056 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.262262 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 5

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603334 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489494 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.474001 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.396819 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.374109 | `azmcp_search_index_get` | ❌ |
| 6 | 0.350751 | `azmcp_search_service_list` | ❌ |
| 7 | 0.341865 | `azmcp_search_index_query` | ❌ |
| 8 | 0.317997 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.310576 | `azmcp_foundry_models_deploy` | ❌ |
| 10 | 0.278147 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.276839 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.272211 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.256208 | `azmcp_grafana_list` | ❌ |
| 14 | 0.250318 | `azmcp_foundry_agents_connect` | ❌ |
| 15 | 0.232519 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.225181 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.224235 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.223815 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.223695 | `azmcp_monitor_metrics_definitions` | ❌ |
| 20 | 0.218010 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 6

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672692 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.564805 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.423836 | `azmcp_search_index_get` | ❌ |
| 4 | 0.397180 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.375275 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.363951 | `azmcp_kusto_table_schema` | ❌ |
| 7 | 0.358315 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.349967 | `azmcp_search_index_query` | ❌ |
| 9 | 0.347762 | `azmcp_foundry_models_list` | ❌ |
| 10 | 0.346490 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.326807 | `azmcp_search_service_list` | ❌ |
| 12 | 0.297822 | `azmcp_foundry_models_deploy` | ❌ |
| 13 | 0.295847 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.285897 | `azmcp_monitor_table_type_list` | ❌ |
| 15 | 0.277289 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.271427 | `azmcp_cloudarchitect_design` | ❌ |
| 17 | 0.266244 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.259298 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.253702 | `azmcp_grafana_list` | ❌ |
| 20 | 0.236958 | `azmcp_mysql_server_list` | ❌ |

---

## Test 7

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650189 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432759 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.415919 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.408316 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.398186 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.379734 | `azmcp_search_index_get` | ❌ |
| 7 | 0.352243 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.318648 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.312072 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.309927 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.286991 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.271893 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.271701 | `azmcp_loadtesting_testrun_list` | ❌ |
| 14 | 0.262783 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.257402 | `azmcp_mysql_table_list` | ❌ |
| 16 | 0.256303 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.249010 | `azmcp_search_index_query` | ❌ |
| 18 | 0.246815 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.242170 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.239938 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 8

**Expected Tool:** `azmcp_foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.313400 | `azmcp_foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.282794 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.274011 | `azmcp_deploy_plan_get` | ❌ |
| 4 | 0.269513 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.268967 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.234071 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.222504 | `azmcp_grafana_list` | ❌ |
| 8 | 0.222478 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.221517 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.217001 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.216470 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 12 | 0.215293 | `azmcp_loadtesting_testrun_create` | ❌ |
| 13 | 0.209877 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.208124 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.207601 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.204528 | `azmcp_postgres_server_param_set` | ❌ |
| 17 | 0.197004 | `azmcp_loadtesting_testrun_update` | ❌ |
| 18 | 0.195615 | `azmcp_workbooks_list` | ❌ |
| 19 | 0.192420 | `azmcp_monitor_metrics_query` | ❌ |
| 20 | 0.192386 | `azmcp_storage_account_create` | ❌ |

---

## Test 9

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559508 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.549636 | `azmcp_foundry_models_list` | ❌ |
| 3 | 0.539677 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.533239 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.448711 | `azmcp_search_service_list` | ❌ |
| 6 | 0.434455 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 7 | 0.368173 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.334867 | `azmcp_grafana_list` | ❌ |
| 9 | 0.332203 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.328418 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 11 | 0.326193 | `azmcp_search_index_get` | ❌ |
| 12 | 0.321010 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.318854 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.309921 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.302262 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.301376 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.300357 | `azmcp_search_index_query` | ❌ |
| 18 | 0.289519 | `azmcp_monitor_workspace_list` | ❌ |
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
| 4 | 0.486430 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.401016 | `azmcp_search_service_list` | ❌ |
| 6 | 0.396412 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 7 | 0.328814 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.311415 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 9 | 0.305634 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.301514 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.298821 | `azmcp_search_index_query` | ❌ |
| 12 | 0.290771 | `azmcp_search_index_get` | ❌ |
| 13 | 0.286814 | `azmcp_grafana_list` | ❌ |
| 14 | 0.270065 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.254926 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.250481 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.246893 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.243133 | `azmcp_monitor_table_type_list` | ❌ |
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
| 2 | 0.491936 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.401146 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.387877 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.386180 | `azmcp_search_service_list` | ❌ |
| 6 | 0.346909 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.298648 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.290624 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 9 | 0.285437 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.277883 | `azmcp_grafana_list` | ❌ |
| 11 | 0.274847 | `azmcp_search_index_get` | ❌ |
| 12 | 0.272586 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.265696 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.256000 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.255760 | `azmcp_search_index_query` | ❌ |
| 16 | 0.252297 | `azmcp_postgres_database_list` | ❌ |
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
| 2 | 0.475214 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.430513 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.388984 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.356899 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.339069 | `azmcp_search_service_list` | ❌ |
| 7 | 0.299371 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 8 | 0.283250 | `azmcp_search_index_query` | ❌ |
| 9 | 0.279924 | `azmcp_foundry_agents_connect` | ❌ |
| 10 | 0.274019 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.266987 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.261368 | `azmcp_search_index_get` | ❌ |
| 13 | 0.260144 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.245943 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.244697 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.240256 | `azmcp_monitor_metrics_definitions` | ❌ |
| 17 | 0.234167 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.217331 | `azmcp_marketplace_product_list` | ❌ |
| 19 | 0.211456 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.207906 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680061 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544604 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.490624 | `azmcp_search_service_list` | ❌ |
| 4 | 0.465948 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.459609 | `azmcp_search_index_query` | ❌ |
| 6 | 0.393556 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.388183 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.372449 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.370915 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.358315 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.356755 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.356252 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.354845 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.353093 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.351079 | `azmcp_sql_server_show` | ❌ |
| 16 | 0.348138 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.343186 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.337040 | `azmcp_keyvault_key_get` | ❌ |
| 19 | 0.333636 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.330038 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 14

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639517 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.620140 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561785 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.480817 | `azmcp_search_index_query` | ❌ |
| 5 | 0.452938 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.445534 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 7 | 0.439195 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.416395 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.409307 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.406485 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.397423 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.382791 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.378750 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.378586 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.375372 | `azmcp_foundry_models_deployments_list` | ❌ |
| 16 | 0.371099 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.369526 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.368931 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367818 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.367388 | `azmcp_redis_cache_list` | ❌ |

---

## Test 15

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620222 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562775 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561077 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.471416 | `azmcp_search_index_query` | ❌ |
| 5 | 0.464054 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.408478 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.401637 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.382692 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.372639 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.370309 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.367868 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.351788 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.351227 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.350043 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.347853 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.346994 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.341728 | `azmcp_foundry_models_list` | ❌ |
| 18 | 0.335766 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.332289 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.328159 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 16

**Expected Tool:** `azmcp_search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521803 | `azmcp_search_index_get` | ❌ |
| 2 | 0.516011 | `azmcp_search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497501 | `azmcp_search_service_list` | ❌ |
| 4 | 0.373908 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.373026 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.327208 | `azmcp_kusto_query` | ❌ |
| 7 | 0.322593 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.311195 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.306385 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.305978 | `azmcp_marketplace_product_list` | ❌ |
| 11 | 0.295608 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.291548 | `azmcp_foundry_agents_connect` | ❌ |
| 13 | 0.290934 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.288372 | `azmcp_foundry_models_deployments_list` | ❌ |
| 15 | 0.287748 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.283686 | `azmcp_foundry_models_list` | ❌ |
| 17 | 0.275008 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.269920 | `azmcp_monitor_table_list` | ❌ |
| 19 | 0.260298 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.244912 | `azmcp_kusto_sample` | ❌ |

---

## Test 17

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.520285 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.505322 | `azmcp_search_index_get` | ❌ |
| 4 | 0.500455 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.494662 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.493125 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.492228 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.486066 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.482464 | `azmcp_grafana_list` | ❌ |
| 10 | 0.477471 | `azmcp_subscription_list` | ❌ |
| 11 | 0.470384 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.470055 | `azmcp_marketplace_product_list` | ❌ |
| 13 | 0.454460 | `azmcp_foundry_models_deployments_list` | ❌ |
| 14 | 0.451895 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.443495 | `azmcp_search_index_query` | ❌ |
| 16 | 0.431621 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.427817 | `azmcp_group_list` | ❌ |
| 18 | 0.425463 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.418042 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.417472 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 18

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.479306 | `azmcp_search_index_get` | ❌ |
| 3 | 0.467373 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.453489 | `azmcp_marketplace_product_list` | ❌ |
| 5 | 0.448446 | `azmcp_search_index_query` | ❌ |
| 6 | 0.426315 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.419555 | `azmcp_marketplace_product_get` | ❌ |
| 8 | 0.412158 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.408613 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.400229 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.399822 | `azmcp_grafana_list` | ❌ |
| 12 | 0.397883 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.393708 | `azmcp_subscription_list` | ❌ |
| 14 | 0.390660 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.390559 | `azmcp_foundry_models_list` | ❌ |
| 16 | 0.389433 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.379741 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 18 | 0.376089 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.373463 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.363442 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 19

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553025 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.435707 | `azmcp_search_index_get` | ❌ |
| 3 | 0.417145 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.404758 | `azmcp_search_index_query` | ❌ |
| 5 | 0.344699 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.336657 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.322540 | `azmcp_foundry_models_list` | ❌ |
| 8 | 0.322526 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 9 | 0.300427 | `azmcp_marketplace_product_list` | ❌ |
| 10 | 0.292794 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.290214 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.289466 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 13 | 0.283514 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.282286 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.281672 | `azmcp_get_bestpractices_get` | ❌ |
| 16 | 0.281452 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.278574 | `azmcp_cloudarchitect_design` | ❌ |
| 18 | 0.278531 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.277693 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.275030 | `azmcp_sql_server_show` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.635561 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.492146 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.491380 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.473605 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.442214 | `azmcp_grafana_list` | ❌ |
| 7 | 0.441656 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.433637 | `azmcp_eventgrid_topic_list` | ❌ |
| 9 | 0.432238 | `azmcp_search_service_list` | ❌ |
| 10 | 0.427658 | `azmcp_subscription_list` | ❌ |
| 11 | 0.427460 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.423903 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.420794 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.412274 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.408833 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.398424 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.389462 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.385938 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.380818 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.370646 | `azmcp_postgres_server_config_get` | ❌ |

---

## Test 21

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634978 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.533437 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.425610 | `azmcp_appconfig_kv_show` | ❌ |
| 4 | 0.372683 | `azmcp_eventgrid_subscription_list` | ❌ |
| 5 | 0.372456 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.368731 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.359567 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.356588 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.355830 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.354707 | `azmcp_appconfig_kv_delete` | ❌ |
| 11 | 0.348603 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.344587 | `azmcp_marketplace_product_get` | ❌ |
| 13 | 0.341263 | `azmcp_grafana_list` | ❌ |
| 14 | 0.340769 | `azmcp_eventgrid_topic_list` | ❌ |
| 15 | 0.332824 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.325885 | `azmcp_subscription_list` | ❌ |
| 17 | 0.325232 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.318662 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.310432 | `azmcp_search_service_list` | ❌ |
| 20 | 0.293037 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 22

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565435 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.564705 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.414689 | `azmcp_appconfig_kv_show` | ❌ |
| 4 | 0.355916 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.348651 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.327234 | `azmcp_appconfig_kv_set` | ❌ |
| 7 | 0.289682 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 8 | 0.282153 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.272373 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.255778 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.250447 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.239130 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.236404 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.234900 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.233357 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.231649 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.228042 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.225913 | `azmcp_sql_db_update` | ❌ |
| 19 | 0.221603 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.221405 | `azmcp_postgres_database_list` | ❌ |

---

## Test 23

**Expected Tool:** `azmcp_appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618244 | `azmcp_appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.486631 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.424344 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.399569 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.392016 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.269874 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.262117 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.248752 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.240483 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.236177 | `azmcp_keyvault_key_get` | ❌ |
| 12 | 0.218487 | `azmcp_mysql_server_param_set` | ❌ |
| 13 | 0.216878 | `azmcp_sql_server_delete` | ❌ |
| 14 | 0.196121 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 15 | 0.195036 | `azmcp_sql_db_delete` | ❌ |
| 16 | 0.194831 | `azmcp_postgres_server_config_get` | ❌ |
| 17 | 0.183452 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.175403 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.173007 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.166763 | `azmcp_storage_account_get` | ❌ |

---

## Test 24

**Expected Tool:** `azmcp_appconfig_kv_list`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.730852 | `azmcp_appconfig_kv_list` | ✅ **EXPECTED** |
| 2 | 0.595054 | `azmcp_appconfig_kv_show` | ❌ |
| 3 | 0.557810 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.530884 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.464592 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.439089 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.377534 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.374460 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.338195 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.333355 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.327550 | `azmcp_loadtesting_testrun_list` | ❌ |
| 12 | 0.323615 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.321277 | `azmcp_keyvault_certificate_list` | ❌ |
| 14 | 0.317744 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.296084 | `azmcp_postgres_table_list` | ❌ |
| 16 | 0.292091 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.275469 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.267026 | `azmcp_postgres_database_list` | ❌ |
| 19 | 0.265734 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.264833 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 25

**Expected Tool:** `azmcp_appconfig_kv_list`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682275 | `azmcp_appconfig_kv_list` | ✅ **EXPECTED** |
| 2 | 0.606545 | `azmcp_appconfig_kv_show` | ❌ |
| 3 | 0.522426 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.512945 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.468461 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.457866 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.370520 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.356787 | `azmcp_mysql_server_param_get` | ❌ |
| 9 | 0.317662 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.314774 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.304557 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.294807 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.288088 | `azmcp_mysql_server_param_set` | ❌ |
| 14 | 0.278909 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.269402 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.264526 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.258704 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.249742 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.243878 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.238122 | `azmcp_sql_server_show` | ❌ |

---

## Test 26

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591237 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.508804 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.445551 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.431458 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.423650 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.373656 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.253705 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.251298 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.238544 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.238369 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.214150 | `azmcp_keyvault_key_get` | ❌ |
| 12 | 0.211331 | `azmcp_postgres_server_config_get` | ❌ |
| 13 | 0.210627 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.185349 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.163738 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.158942 | `azmcp_mysql_server_param_get` | ❌ |
| 17 | 0.154510 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.144377 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.139871 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.134195 | `azmcp_loadtesting_testrun_update` | ❌ |

---

## Test 27

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555699 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.541557 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.476464 | `azmcp_appconfig_kv_delete` | ❌ |
| 4 | 0.435759 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.425488 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.409406 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.272339 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.268062 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.259561 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.252818 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.237098 | `azmcp_mysql_server_param_set` | ❌ |
| 12 | 0.225350 | `azmcp_postgres_server_config_get` | ❌ |
| 13 | 0.190578 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.190136 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.185046 | `azmcp_postgres_server_param_set` | ❌ |
| 16 | 0.179793 | `azmcp_mysql_server_param_get` | ❌ |
| 17 | 0.171375 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.159752 | `azmcp_postgres_server_param_get` | ❌ |
| 19 | 0.150099 | `azmcp_storage_blob_container_get` | ❌ |
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
| 3 | 0.518499 | `azmcp_appconfig_kv_list` | ❌ |
| 4 | 0.507170 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.505470 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.377919 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.360015 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.346981 | `azmcp_postgres_server_param_set` | ❌ |
| 9 | 0.311433 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.305955 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.276129 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.262891 | `azmcp_sql_db_update` | ❌ |
| 13 | 0.236813 | `azmcp_keyvault_secret_get` | ❌ |
| 14 | 0.213593 | `azmcp_mysql_server_param_get` | ❌ |
| 15 | 0.208947 | `azmcp_postgres_server_config_get` | ❌ |
| 16 | 0.201885 | `azmcp_loadtesting_testrun_update` | ❌ |
| 17 | 0.193989 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.166969 | `azmcp_postgres_server_param_get` | ❌ |
| 19 | 0.164376 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.137994 | `azmcp_storage_account_create` | ❌ |

---

## Test 29

**Expected Tool:** `azmcp_appconfig_kv_show`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603216 | `azmcp_appconfig_kv_list` | ❌ |
| 2 | 0.561508 | `azmcp_appconfig_kv_show` | ✅ **EXPECTED** |
| 3 | 0.448912 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.441662 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.437432 | `azmcp_appconfig_account_list` | ❌ |
| 6 | 0.416264 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.323608 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.301859 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.301266 | `azmcp_keyvault_secret_get` | ❌ |
| 10 | 0.291448 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.269387 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.259549 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.257957 | `azmcp_mysql_server_param_get` | ❌ |
| 14 | 0.229242 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.226070 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.217867 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.206401 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.201872 | `azmcp_mysql_server_param_set` | ❌ |
| 19 | 0.186706 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.185986 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 30

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.355635 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.329345 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.300617 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.257790 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.216077 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.206477 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.205255 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.193551 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 9 | 0.181209 | `azmcp_foundry_agents_evaluate` | ❌ |
| 10 | 0.177942 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.169553 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.163768 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.148018 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.141970 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.133096 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.128768 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.125735 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.120066 | `azmcp_mysql_table_schema_get` | ❌ |
| 19 | 0.115970 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.111731 | `azmcp_redis_cache_list` | ❌ |

---

## Test 31

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.318608 | `azmcp_deploy_app_logs_get` | ❌ |
| 2 | 0.302557 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.255757 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.225972 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.222226 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.200402 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.199925 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.186927 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.172691 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.163345 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.162349 | `azmcp_foundry_agents_evaluate` | ❌ |
| 12 | 0.150964 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.150313 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.144004 | `azmcp_mysql_server_param_get` | ❌ |
| 15 | 0.133109 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.125941 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.118881 | `azmcp_mysql_table_schema_get` | ❌ |
| 18 | 0.112992 | `azmcp_monitor_workspace_log_query` | ❌ |
| 19 | 0.107063 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.101816 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 32

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.256753 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 2 | 0.250546 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.215890 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.199067 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.188245 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.188040 | `azmcp_appservice_database_add` | ❌ |
| 7 | 0.179320 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.178907 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.159064 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.158352 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.156599 | `azmcp_search_service_list` | ❌ |
| 12 | 0.156168 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 13 | 0.153379 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.151702 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.146689 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.139619 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.130395 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.129410 | `azmcp_mysql_server_param_get` | ❌ |
| 19 | 0.126234 | `azmcp_search_index_get` | ❌ |
| 20 | 0.125566 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 33

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a database connection to my app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729071 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.398617 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.368252 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.364595 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.361951 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.353953 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.348738 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.342556 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.342319 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.333234 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.301697 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.300846 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.298638 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.286104 | `azmcp_cosmos_database_list` | ❌ |
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
| 2 | 0.484258 | `azmcp_sql_db_update` | ❌ |
| 3 | 0.471103 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.408794 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.405300 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.389144 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.381822 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.366442 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.366336 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.355360 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.352382 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.350299 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.345379 | `azmcp_sql_db_delete` | ❌ |
| 14 | 0.340399 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.329197 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.322825 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.315986 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.304849 | `azmcp_loadtesting_test_create` | ❌ |
| 19 | 0.299620 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.295124 | `azmcp_appconfig_kv_show` | ❌ |

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
| 4 | 0.382742 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.351839 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.345939 | `azmcp_sql_db_update` | ❌ |
| 7 | 0.344869 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.335323 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.323158 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.320648 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.314492 | `azmcp_mysql_server_param_set` | ❌ |
| 12 | 0.311349 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.297738 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.295428 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.279652 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.272652 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.272178 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.269892 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.269785 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.260632 | `azmcp_functionapp_get` | ❌ |

---

## Test 36

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a PostgreSQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579502 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449085 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.439636 | `azmcp_postgres_database_query` | ❌ |
| 4 | 0.409515 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.405431 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.399406 | `azmcp_postgres_server_param_set` | ❌ |
| 7 | 0.383413 | `azmcp_sql_db_create` | ❌ |
| 8 | 0.337005 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.328889 | `azmcp_postgres_server_param_get` | ❌ |
| 10 | 0.305533 | `azmcp_sql_db_update` | ❌ |
| 11 | 0.302980 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.289352 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.279654 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.258603 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.257684 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.254307 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.241103 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.233707 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.231783 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.223353 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 37

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a CosmosDB database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643046 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.477008 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.465637 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.421268 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.400430 | `azmcp_sql_db_update` | ❌ |
| 6 | 0.378402 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.374251 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.370137 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.362494 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.353056 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.352381 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.349533 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.326665 | `azmcp_sql_db_delete` | ❌ |
| 14 | 0.325004 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.314834 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.314627 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.313397 | `azmcp_sql_server_delete` | ❌ |
| 18 | 0.309146 | `azmcp_redis_cluster_database_list` | ❌ |
| 19 | 0.302983 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.292774 | `azmcp_sql_server_create` | ❌ |

---

## Test 38

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645533 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.489228 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.423910 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.422266 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.394910 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.394447 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.380385 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.368592 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.360144 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.357307 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.349953 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.348596 | `azmcp_sql_db_update` | ❌ |
| 13 | 0.348100 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.346023 | `azmcp_sql_db_delete` | ❌ |
| 15 | 0.304416 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.281301 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.277310 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.274590 | `azmcp_appconfig_kv_set` | ❌ |
| 19 | 0.274299 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.266392 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 39

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665216 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.371272 | `azmcp_sql_db_update` | ❌ |
| 3 | 0.369071 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.332119 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.314230 | `azmcp_cosmos_database_list` | ❌ |
| 6 | 0.312395 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.307420 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.304622 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.297160 | `azmcp_mysql_server_param_get` | ❌ |
| 10 | 0.294182 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.292606 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.285830 | `azmcp_postgres_server_param_set` | ❌ |
| 13 | 0.273579 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.269033 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.267615 | `azmcp_sql_server_show` | ❌ |
| 16 | 0.267098 | `azmcp_mysql_server_param_set` | ❌ |
| 17 | 0.266587 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.265627 | `azmcp_sql_db_delete` | ❌ |
| 19 | 0.260212 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.256207 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 40

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536761 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.394572 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.391843 | `azmcp_sql_db_update` | ❌ |
| 4 | 0.318461 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.318263 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.305550 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.301240 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.298453 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.298094 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.297607 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.295901 | `azmcp_subscription_list` | ❌ |
| 12 | 0.294048 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.290182 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.280891 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.274767 | `azmcp_sql_db_delete` | ❌ |
| 16 | 0.273430 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.272731 | `azmcp_sql_server_delete` | ❌ |
| 18 | 0.272238 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.267101 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.265600 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 41

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560268 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.426753 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.361013 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.349556 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.346672 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.345185 | `azmcp_sql_db_update` | ❌ |
| 7 | 0.342276 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.339822 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.339459 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.330944 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.317003 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.291090 | `azmcp_sql_server_delete` | ❌ |
| 13 | 0.281985 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.277048 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.270334 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.268258 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.263797 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.258869 | `azmcp_keyvault_certificate_create` | ❌ |
| 19 | 0.257394 | `azmcp_mysql_table_list` | ❌ |
| 20 | 0.257248 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 42

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572497 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.445157 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.390478 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.385368 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.375286 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.358115 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.352874 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.346020 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.344858 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.330014 | `azmcp_search_service_list` | ❌ |
| 11 | 0.326046 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.297036 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.296190 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.268844 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.265962 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.263771 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.260357 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.258262 | `azmcp_monitor_table_list` | ❌ |
| 19 | 0.247848 | `azmcp_search_index_get` | ❌ |
| 20 | 0.245629 | `azmcp_redis_cache_list` | ❌ |

---

## Test 43

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696210 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.468384 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.452121 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.435241 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.424622 | `azmcp_search_service_list` | ❌ |
| 6 | 0.405506 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.405315 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.401105 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.393786 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.387892 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.380205 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.371626 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.367714 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.367243 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.362866 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.355516 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.339529 | `azmcp_monitor_workspace_list` | ❌ |
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
| 1 | 0.626959 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.479252 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.468847 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.467717 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.461660 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.451694 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.449821 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.446454 | `azmcp_search_service_list` | ❌ |
| 9 | 0.419753 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.417639 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.416057 | `azmcp_get_bestpractices_get` | ❌ |
| 12 | 0.415664 | `azmcp_monitor_metrics_definitions` | ❌ |
| 13 | 0.407441 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.401304 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.401135 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.398817 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.389786 | `azmcp_monitor_table_type_list` | ❌ |
| 18 | 0.388671 | `azmcp_group_list` | ❌ |
| 19 | 0.386954 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.385596 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 45

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509198 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.398251 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.383767 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.369053 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.367260 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.341619 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.325776 | `azmcp_deploy_iac_rules_get` | ❌ |
| 8 | 0.324433 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.321854 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.313589 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.287390 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.285234 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.262799 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.259246 | `azmcp_search_service_list` | ❌ |
| 15 | 0.254871 | `azmcp_search_index_query` | ❌ |
| 16 | 0.247065 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.233938 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.230227 | `azmcp_monitor_workspace_log_query` | ❌ |
| 19 | 0.229476 | `azmcp_mysql_server_config_get` | ❌ |
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
| 4 | 0.527461 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.515937 | `azmcp_subscription_list` | ❌ |
| 6 | 0.514293 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.509529 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.503032 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.490776 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.487764 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.483500 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.482333 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.481761 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.480869 | `azmcp_group_list` | ❌ |
| 15 | 0.469958 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.462353 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.460523 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.460358 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.456984 | `azmcp_mysql_server_list` | ❌ |
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
| 3 | 0.450387 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.383160 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.373066 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.372153 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.370924 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.364918 | `azmcp_search_service_list` | ❌ |
| 10 | 0.359177 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.356421 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.354277 | `azmcp_storage_blob_container_create` | ❌ |
| 13 | 0.353379 | `azmcp_subscription_list` | ❌ |
| 14 | 0.352818 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.349535 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.349291 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.348080 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.344750 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.344071 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.339252 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 48

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637140 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563489 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.473928 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471802 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.463676 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.459813 | `azmcp_search_service_list` | ❌ |
| 7 | 0.452889 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.451370 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.443925 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.440431 | `azmcp_subscription_list` | ❌ |
| 11 | 0.435812 | `azmcp_storage_blob_container_get` | ❌ |
| 12 | 0.435781 | `azmcp_grafana_list` | ❌ |
| 13 | 0.431780 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.430808 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.430260 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.419750 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.404595 | `azmcp_group_list` | ❌ |
| 18 | 0.398550 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.386474 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.364688 | `azmcp_mysql_server_list` | ❌ |

---

## Test 49

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `azmcp_acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476308 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.454929 | `azmcp_group_list` | ❌ |
| 5 | 0.454003 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.446008 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.428000 | `azmcp_workbooks_list` | ❌ |
| 8 | 0.423541 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421045 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.417327 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.411382 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.409133 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.404513 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.388773 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.378482 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.371025 | `azmcp_sql_elastic-pool_list` | ❌ |
| 17 | 0.370359 | `azmcp_redis_cluster_database_list` | ❌ |
| 18 | 0.356119 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.354136 | `azmcp_cosmos_database_list` | ❌ |
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
| 3 | 0.468394 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.449649 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `azmcp_group_list` | ❌ |
| 6 | 0.416353 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.413975 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.413191 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.406554 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.403800 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.400209 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.389635 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.378440 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.369912 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.369779 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.367734 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.355591 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.354807 | `azmcp_loadtesting_testresource_list` | ❌ |
| 19 | 0.351424 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.347199 | `azmcp_eventgrid_subscription_list` | ❌ |

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
| 5 | 0.492588 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.475629 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.466001 | `azmcp_search_service_list` | ❌ |
| 8 | 0.461777 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.461369 | `azmcp_grafana_list` | ❌ |
| 10 | 0.456838 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.449239 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.448298 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.440083 | `azmcp_subscription_list` | ❌ |
| 14 | 0.438227 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.437677 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.430939 | `azmcp_group_list` | ❌ |
| 17 | 0.414463 | `azmcp_kusto_database_list` | ❌ |
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
| 1 | 0.546333 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.407973 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.400266 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.339307 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.327010 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.308648 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.306804 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.306442 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.302635 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.300236 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.296073 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.292155 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.290245 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.289864 | `azmcp_search_service_list` | ❌ |
| 16 | 0.283716 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.283390 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.282581 | `azmcp_sql_db_list` | ❌ |
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
| 4 | 0.388727 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.370375 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.359618 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.357217 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.355328 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.351007 | `azmcp_redis_cluster_database_list` | ❌ |
| 10 | 0.347437 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.347084 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.340058 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.338404 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.337543 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.332856 | `azmcp_keyvault_key_list` | ❌ |
| 16 | 0.332785 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.332704 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.332629 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.330046 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.322287 | `azmcp_mysql_table_list` | ❌ |

---

## Test 54

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600824 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501882 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.418556 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.374830 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.359932 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.341575 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.335835 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.333339 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.324234 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.318700 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.316696 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.315430 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.311873 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.309646 | `azmcp_search_service_list` | ❌ |
| 15 | 0.306071 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.304756 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.303988 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.302405 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.300138 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.299338 | `azmcp_mysql_table_list` | ❌ |

---

## Test 55

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668448 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.615268 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.587691 | `azmcp_subscription_list` | ❌ |
| 5 | 0.560795 | `azmcp_search_service_list` | ❌ |
| 6 | 0.538321 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.529220 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.516914 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.502428 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.502302 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.499161 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.497679 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.486978 | `azmcp_group_list` | ❌ |
| 14 | 0.483046 | `azmcp_grafana_list` | ❌ |
| 15 | 0.474934 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.473638 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.460064 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.459502 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.459002 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.453975 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 56

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665447 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605310 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.571613 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.486033 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.436283 | `azmcp_subscription_list` | ❌ |
| 6 | 0.431496 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.428588 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.427709 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.408659 | `azmcp_search_service_list` | ❌ |
| 10 | 0.405680 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.397571 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.390141 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.390141 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.386297 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.383985 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.381323 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.379496 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.373827 | `azmcp_redis_cluster_list` | ❌ |
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
| 2 | 0.605071 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.566249 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.546327 | `azmcp_subscription_list` | ❌ |
| 5 | 0.530175 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.527812 | `azmcp_search_service_list` | ❌ |
| 7 | 0.488275 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.466559 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.457584 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.456219 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.455017 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.453626 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.441136 | `azmcp_grafana_list` | ❌ |
| 14 | 0.438277 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.437893 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.437008 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.434623 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.433094 | `azmcp_postgres_server_list` | ❌ |
| 19 | 0.430351 | `azmcp_aks_cluster_list` | ❌ |
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
| 3 | 0.477847 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.447757 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.445627 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.429363 | `azmcp_search_service_list` | ❌ |
| 7 | 0.399756 | `azmcp_search_index_query` | ❌ |
| 8 | 0.378151 | `azmcp_kusto_query` | ❌ |
| 9 | 0.374844 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.372689 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.365677 | `azmcp_search_index_get` | ❌ |
| 12 | 0.359083 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.351331 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.340794 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.338041 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.335256 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.334389 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.331041 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.308694 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.302962 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 59

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852750 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.680905 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.630581 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.581507 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.527758 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 6 | 0.486072 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.448947 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.447456 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.439760 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.427708 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.424052 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.422278 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.421329 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.420139 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.411373 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.392670 | `azmcp_postgres_database_list` | ❌ |
| 17 | 0.386634 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.383446 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.378019 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.372095 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 60

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789417 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.614275 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.562125 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.537334 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.521544 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 6 | 0.449188 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.411767 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.398075 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.397984 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.397775 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.395530 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.393003 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.386841 | `azmcp_redis_cluster_database_list` | ❌ |
| 14 | 0.356344 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.355701 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.345669 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.342289 | `azmcp_monitor_table_list` | ❌ |
| 18 | 0.326026 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.319672 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.318567 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 61

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815706 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668515 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.665298 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.573704 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.571319 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.548066 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.526046 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.501477 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.471453 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.459194 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.450662 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.442540 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.418871 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.407722 | `azmcp_search_service_list` | ❌ |
| 15 | 0.406947 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.405825 | `azmcp_keyvault_key_list` | ❌ |
| 17 | 0.401638 | `azmcp_subscription_list` | ❌ |
| 18 | 0.397642 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.389032 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.387534 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 62

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749384 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.614572 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.538479 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.524837 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.498206 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.497414 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.449759 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.447875 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.437993 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.408605 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.402767 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.396122 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.384062 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.379265 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.369344 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.348972 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
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
| 2 | 0.464523 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.457689 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.416762 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.378455 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.362956 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.361772 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.353766 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.351393 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.344871 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.344590 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.333244 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.332639 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.326472 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.326052 | `azmcp_aks_nodepool_list` | ❌ |
| 16 | 0.325670 | `azmcp_search_index_get` | ❌ |
| 17 | 0.319939 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.318754 | `azmcp_kusto_query` | ❌ |
| 19 | 0.318082 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.314617 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 64

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.651218 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.644050 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.549093 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.536042 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.509396 | `azmcp_grafana_list` | ❌ |
| 6 | 0.505912 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.492107 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.491278 | `azmcp_search_service_list` | ❌ |
| 9 | 0.487682 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.486159 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.460255 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.458754 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.451500 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.427759 | `azmcp_subscription_list` | ❌ |
| 15 | 0.420131 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.412630 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.411791 | `azmcp_group_list` | ❌ |
| 18 | 0.410016 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.399104 | `azmcp_monitor_table_list` | ❌ |
| 20 | 0.391238 | `azmcp_monitor_table_type_list` | ❌ |

---

## Test 65

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.437388 | `azmcp_redis_cluster_list` | ❌ |
| 2 | 0.391087 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.386126 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 4 | 0.359551 | `azmcp_kusto_database_list` | ❌ |
| 5 | 0.341784 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.338209 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.314734 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.303083 | `azmcp_grafana_list` | ❌ |
| 9 | 0.293022 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.292838 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.287768 | `azmcp_kusto_sample` | ❌ |
| 12 | 0.285603 | `azmcp_kusto_query` | ❌ |
| 13 | 0.283331 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.277014 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.275559 | `azmcp_mysql_database_query` | ❌ |
| 16 | 0.270759 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.265764 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.264112 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.264085 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.263226 | `azmcp_quota_usage_check` | ❌ |

---

## Test 66

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584096 | `azmcp_redis_cluster_list` | ❌ |
| 2 | 0.549797 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 3 | 0.471116 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.469570 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.464294 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.462945 | `azmcp_grafana_list` | ❌ |
| 7 | 0.446124 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.440412 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.434016 | `azmcp_search_service_list` | ❌ |
| 10 | 0.432048 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.406863 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.396253 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.392541 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.386776 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.379997 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.377490 | `azmcp_kusto_query` | ❌ |
| 17 | 0.371088 | `azmcp_subscription_list` | ❌ |
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
| 3 | 0.553218 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.549688 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.517039 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.474354 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.461496 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.459182 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.434330 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.431669 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.419528 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.404010 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.396060 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.375535 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.363663 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.363027 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.350216 | `azmcp_aks_cluster_list` | ❌ |
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
| 1 | 0.597939 | `azmcp_redis_cluster_database_list` | ❌ |
| 2 | 0.558435 | `azmcp_kusto_database_list` | ✅ **EXPECTED** |
| 3 | 0.497102 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.491378 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.486660 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.440017 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.427287 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.422554 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.391424 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.383664 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.367977 | `azmcp_postgres_table_list` | ❌ |
| 12 | 0.362897 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.359248 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.343891 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.336346 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.336104 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.334778 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.310946 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.309805 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.305766 | `azmcp_kusto_query` | ❌ |

---

## Test 69

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.381454 | `azmcp_kusto_query` | ✅ **EXPECTED** |
| 2 | 0.363411 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.363132 | `azmcp_kusto_sample` | ❌ |
| 4 | 0.348684 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.345933 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.334706 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.328607 | `azmcp_search_service_list` | ❌ |
| 8 | 0.328158 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.324595 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.319221 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.318736 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.314922 | `azmcp_search_index_query` | ❌ |
| 13 | 0.314687 | `azmcp_monitor_table_type_list` | ❌ |
| 14 | 0.308211 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.304060 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.302596 | `azmcp_postgres_table_list` | ❌ |
| 17 | 0.292133 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.263994 | `azmcp_grafana_list` | ❌ |
| 19 | 0.263209 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.257580 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 70

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
| 9 | 0.361865 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.343671 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.341749 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.337281 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.319239 | `azmcp_kusto_query` | ❌ |
| 14 | 0.318189 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.310196 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.285941 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.282651 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.267689 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.249426 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.242088 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

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
| 4 | 0.549900 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.521516 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.520802 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.475496 | `azmcp_postgres_database_list` | ❌ |
| 8 | 0.464341 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.449656 | `azmcp_kusto_table_schema` | ❌ |
| 10 | 0.436519 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.433775 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.429280 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.412275 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.410425 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.400099 | `azmcp_mysql_table_schema_get` | ❌ |
| 16 | 0.384895 | `azmcp_postgres_table_schema_get` | ❌ |
| 17 | 0.380671 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.337427 | `azmcp_kusto_query` | ❌ |
| 19 | 0.330072 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.329669 | `azmcp_grafana_list` | ❌ |

---

## Test 72

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549885 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.524691 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.523432 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.494108 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.490716 | `azmcp_monitor_table_list` | ❌ |
| 6 | 0.475412 | `azmcp_kusto_database_list` | ❌ |
| 7 | 0.466212 | `azmcp_kusto_table_schema` | ❌ |
| 8 | 0.431964 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.425623 | `azmcp_kusto_sample` | ❌ |
| 10 | 0.421413 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.418153 | `azmcp_mysql_table_schema_get` | ❌ |
| 12 | 0.415682 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.403471 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.402646 | `azmcp_postgres_table_schema_get` | ❌ |
| 15 | 0.391070 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.367187 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.348891 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.330383 | `azmcp_kusto_query` | ❌ |
| 19 | 0.314766 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.300292 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 73

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
| 7 | 0.413929 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.398632 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.387660 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.366346 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.366081 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.358088 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.345290 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.343530 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.340038 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.314580 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.298243 | `azmcp_kusto_query` | ❌ |
| 18 | 0.294854 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.282712 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.275795 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 74

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633991 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.623359 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534434 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.498863 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.490102 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.472699 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.461951 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.453619 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.430328 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.428181 | `azmcp_mysql_database_query` | ❌ |
| 11 | 0.421769 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.406768 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.338461 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.327613 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.317879 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.284754 | `azmcp_grafana_list` | ❌ |
| 17 | 0.278407 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.270886 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.268849 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.266204 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 75

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588122 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.574089 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.483855 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.463213 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.448169 | `azmcp_redis_cluster_database_list` | ❌ |
| 6 | 0.444547 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.415134 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.405492 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.404871 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.384974 | `azmcp_postgres_table_list` | ❌ |
| 11 | 0.384778 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.380422 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.297709 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.290592 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.259334 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.251227 | `azmcp_appservice_database_add` | ❌ |
| 17 | 0.247558 | `azmcp_grafana_list` | ❌ |
| 18 | 0.239532 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.236450 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.236206 | `azmcp_acr_registry_list` | ❌ |

---

## Test 76

**Expected Tool:** `azmcp_mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.477525 | `azmcp_mysql_table_list` | ❌ |
| 2 | 0.456762 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.434299 | `azmcp_mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.420801 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.411358 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.394520 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.345692 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.329160 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.320743 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.299737 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.292492 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.286923 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.280026 | `azmcp_kusto_query` | ❌ |
| 14 | 0.278926 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.265576 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.258403 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.231460 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.227589 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.225721 | `azmcp_grafana_list` | ❌ |
| 20 | 0.199005 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 77

**Expected Tool:** `azmcp_mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531887 | `azmcp_postgres_server_config_get` | ❌ |
| 2 | 0.489816 | `azmcp_mysql_server_config_get` | ✅ **EXPECTED** |
| 3 | 0.485952 | `azmcp_mysql_server_param_set` | ❌ |
| 4 | 0.476875 | `azmcp_mysql_server_param_get` | ❌ |
| 5 | 0.426507 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.413258 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.398345 | `azmcp_sql_server_show` | ❌ |
| 8 | 0.391644 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.376750 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.374867 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.267903 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.252810 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.238583 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.232680 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.224212 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.215307 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.214473 | `azmcp_appservice_database_add` | ❌ |
| 18 | 0.198877 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.180054 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.169473 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 78

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678472 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.558177 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.554809 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.501199 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.482086 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.478541 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.467807 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.458406 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.457318 | `azmcp_grafana_list` | ❌ |
| 10 | 0.451969 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.431642 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.431126 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.422584 | `azmcp_search_service_list` | ❌ |
| 14 | 0.410134 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.403856 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.379296 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.377511 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.374451 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.365458 | `azmcp_group_list` | ❌ |
| 20 | 0.354490 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 79

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478518 | `azmcp_mysql_database_list` | ❌ |
| 2 | 0.474721 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.412380 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.389993 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.377048 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.372766 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.363891 | `azmcp_mysql_server_param_get` | ❌ |
| 9 | 0.355142 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.337771 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.281557 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.251411 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.248026 | `azmcp_grafana_list` | ❌ |
| 14 | 0.248003 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.244760 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.241489 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.235455 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.232383 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.224586 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.218115 | `azmcp_acr_registry_list` | ❌ |

---

## Test 80

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.534328 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530210 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.464360 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.458498 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.456663 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.441837 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.431914 | `azmcp_grafana_list` | ❌ |
| 9 | 0.419663 | `azmcp_search_service_list` | ❌ |
| 10 | 0.416021 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.412407 | `azmcp_mysql_database_query` | ❌ |
| 12 | 0.408235 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.399358 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.376596 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.375697 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.364016 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.356691 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.341439 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.341058 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.337333 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 81

**Expected Tool:** `azmcp_mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495051 | `azmcp_mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.407671 | `azmcp_mysql_server_param_set` | ❌ |
| 3 | 0.333841 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.313150 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.310846 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.300031 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.296654 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.292680 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.286103 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.285645 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.241196 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.183735 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.160082 | `azmcp_appconfig_kv_list` | ❌ |
| 14 | 0.150801 | `azmcp_keyvault_secret_get` | ❌ |
| 15 | 0.146290 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.124274 | `azmcp_grafana_list` | ❌ |
| 17 | 0.121544 | `azmcp_foundry_agents_connect` | ❌ |
| 18 | 0.120498 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 19 | 0.118505 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.117704 | `azmcp_applens_resource_diagnose` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.390761 | `azmcp_mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381121 | `azmcp_mysql_server_param_get` | ❌ |
| 3 | 0.307808 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.298911 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.277569 | `azmcp_appservice_database_add` | ❌ |
| 6 | 0.254296 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.253189 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.246424 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.246019 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.238742 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.236450 | `azmcp_postgres_server_param_get` | ❌ |
| 12 | 0.140511 | `azmcp_foundry_agents_connect` | ❌ |
| 13 | 0.112499 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.090695 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.090328 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.089483 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.088097 | `azmcp_loadtesting_test_create` | ❌ |
| 18 | 0.086308 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 19 | 0.084357 | `azmcp_foundry_agents_evaluate` | ❌ |
| 20 | 0.082240 | `azmcp_loadtesting_testrun_create` | ❌ |

---

## Test 83

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
| 6 | 0.447304 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.442053 | `azmcp_kusto_table_list` | ❌ |
| 8 | 0.429975 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.418431 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.410273 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.401230 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.393205 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.361477 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.335069 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.308385 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.268415 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.260118 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.253046 | `azmcp_grafana_list` | ❌ |
| 19 | 0.241294 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.239226 | `azmcp_appconfig_kv_list` | ❌ |

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
| 4 | 0.507258 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.498050 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.439004 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.419912 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.403265 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.385166 | `azmcp_postgres_table_schema_get` | ❌ |
| 10 | 0.382022 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.378011 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.349444 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.342926 | `azmcp_kusto_table_schema` | ❌ |
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
| 1 | 0.630623 | `azmcp_mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558306 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.545025 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.482505 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.457739 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.443955 | `azmcp_mysql_database_query` | ❌ |
| 7 | 0.407451 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.398102 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.372919 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.348909 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.347368 | `azmcp_postgres_server_config_get` | ❌ |
| 12 | 0.324675 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.307950 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.271984 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.268262 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.243861 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.239328 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.208768 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.203013 | `azmcp_bicepschema_get` | ❌ |
| 20 | 0.194220 | `azmcp_grafana_list` | ❌ |

---

## Test 86

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815617 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.644014 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.622790 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.542685 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.490895 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.471672 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.453436 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.444410 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.435853 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.418257 | `azmcp_postgres_database_query` | ❌ |
| 11 | 0.414729 | `azmcp_postgres_server_param_set` | ❌ |
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

## Test 87

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760033 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589783 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.585891 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.552660 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.495647 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.452128 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.433860 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.430589 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.426741 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.416937 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.385491 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.365997 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.281529 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.261442 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.257971 | `azmcp_grafana_list` | ❌ |
| 16 | 0.247726 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.235403 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.227995 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.223439 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.222503 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 88

**Expected Tool:** `azmcp_postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546211 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.503267 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.466599 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.415721 | `azmcp_postgres_database_query` | ✅ **EXPECTED** |
| 5 | 0.403948 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.403924 | `azmcp_postgres_server_config_get` | ❌ |
| 7 | 0.380446 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.361081 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.354786 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.341271 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.264914 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.262332 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.262160 | `azmcp_kusto_query` | ❌ |
| 14 | 0.254174 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.248628 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.244295 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.236363 | `azmcp_grafana_list` | ❌ |
| 18 | 0.218677 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.217855 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.189002 | `azmcp_foundry_models_list` | ❌ |

---

## Test 89

**Expected Tool:** `azmcp_postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `azmcp_postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.599506 | `azmcp_postgres_server_param_get` | ❌ |
| 3 | 0.535655 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.535049 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.518574 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.463172 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.431476 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.394587 | `azmcp_postgres_database_query` | ❌ |
| 9 | 0.356721 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.337899 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.269224 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.233426 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.222849 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.220186 | `azmcp_loadtesting_test_get` | ❌ |
| 15 | 0.208314 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.189446 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.185547 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.178187 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.177787 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.174936 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 90

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900023 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640733 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.565914 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.538997 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.507647 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.483692 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.472458 | `azmcp_grafana_list` | ❌ |
| 8 | 0.457583 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.453841 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.446509 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.435298 | `azmcp_search_service_list` | ❌ |
| 12 | 0.416424 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.406617 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.399073 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.397428 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.389191 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.373699 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.373641 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.365995 | `azmcp_group_list` | ❌ |
| 20 | 0.362956 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 91

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.607062 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.576349 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.522996 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.506235 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.409351 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.400281 | `azmcp_postgres_server_param_set` | ❌ |
| 8 | 0.372955 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.336934 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.336486 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.274763 | `azmcp_grafana_list` | ❌ |
| 12 | 0.260538 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.253264 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.245294 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.241835 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.239500 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.238585 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.229842 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.227547 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.225286 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 92

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579232 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.531804 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.514445 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.505925 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.452671 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.444127 | `azmcp_grafana_list` | ❌ |
| 8 | 0.430033 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.421577 | `azmcp_search_service_list` | ❌ |
| 10 | 0.414695 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.410697 | `azmcp_postgres_database_query` | ❌ |
| 12 | 0.403538 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.376954 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.367001 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.362650 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.362557 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.360539 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.358409 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.334699 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.334101 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 93

**Expected Tool:** `azmcp_postgres_server_param`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594793 | `azmcp_postgres_server_param_get` | ❌ |
| 2 | 0.539671 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.489693 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.481243 | `azmcp_postgres_server_param_set` | ❌ |
| 5 | 0.451871 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.357606 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.343808 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.330875 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.305232 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.295439 | `azmcp_mysql_server_param_set` | ❌ |
| 11 | 0.185273 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.183435 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.174107 | `azmcp_grafana_list` | ❌ |
| 14 | 0.169190 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.166289 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.158068 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.155785 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.145056 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.142412 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.141177 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |

---

## Test 94

**Expected Tool:** `azmcp_postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488474 | `azmcp_postgres_server_config_get` | ❌ |
| 2 | 0.469794 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.464863 | `azmcp_postgres_server_param_set` | ✅ **EXPECTED** |
| 4 | 0.447016 | `azmcp_postgres_server_param_get` | ❌ |
| 5 | 0.440760 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.354049 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.341517 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.317484 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.241642 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.227942 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.192554 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.133385 | `azmcp_kusto_sample` | ❌ |
| 13 | 0.127120 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.126411 | `azmcp_foundry_agents_evaluate` | ❌ |
| 15 | 0.123491 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.119027 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.118088 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.114978 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.113841 | `azmcp_grafana_list` | ❌ |
| 20 | 0.112605 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 95

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
| 7 | 0.449122 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.432813 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.430146 | `azmcp_postgres_server_param_get` | ❌ |
| 10 | 0.396688 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.394169 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.373694 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.352211 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.308203 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.299785 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.257808 | `azmcp_grafana_list` | ❌ |
| 17 | 0.256245 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.249162 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.236931 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.229889 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 96

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
| 6 | 0.464864 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.457757 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.447168 | `azmcp_postgres_server_param_get` | ❌ |
| 9 | 0.390392 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.383179 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.371381 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.334843 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.315790 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.307262 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.272906 | `azmcp_kusto_sample` | ❌ |
| 16 | 0.266178 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.243542 | `azmcp_grafana_list` | ❌ |
| 18 | 0.207521 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.205697 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.202950 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 97

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
| 7 | 0.443784 | `azmcp_postgres_server_param_get` | ❌ |
| 8 | 0.442553 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.427485 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.406761 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.363325 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.322766 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.303748 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.253730 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.253395 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.239225 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.212206 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.201673 | `azmcp_grafana_list` | ❌ |
| 19 | 0.185124 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.184021 | `azmcp_appservice_database_add` | ❌ |

---

## Test 98

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686701 | `azmcp_deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.404890 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.392565 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.389206 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.354432 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.342594 | `azmcp_monitor_resource_log_query` | ❌ |
| 8 | 0.334992 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.334616 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.333572 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.327028 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.325553 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.320572 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.315038 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.314890 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.312900 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.307291 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.297642 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.294636 | `azmcp_sql_server_list` | ❌ |
| 20 | 0.288973 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 99

**Expected Tool:** `azmcp_deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680456 | `azmcp_deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562466 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.504967 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.497137 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.435844 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.430658 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.417235 | `azmcp_get_bestpractices_get` | ❌ |
| 8 | 0.371073 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.343036 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.322146 | `azmcp_extension_azqr` | ❌ |
| 11 | 0.317840 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.284385 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.270055 | `azmcp_sql_db_create` | ❌ |
| 14 | 0.264860 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.264328 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.263427 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.255064 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.250527 | `azmcp_search_service_list` | ❌ |
| 19 | 0.248130 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.247843 | `azmcp_sql_server_show` | ❌ |

---

## Test 100

**Expected Tool:** `azmcp_deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529092 | `azmcp_deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.405071 | `azmcp_bicepschema_get` | ❌ |
| 3 | 0.391965 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.383210 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.341436 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.304788 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.278653 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.266672 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.266629 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.252977 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.236341 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.223979 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.219521 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.207412 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.202239 | `azmcp_mysql_table_schema_get` | ❌ |
| 16 | 0.201288 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.195422 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.194571 | `azmcp_sql_db_create` | ❌ |
| 19 | 0.188615 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.177819 | `azmcp_storage_blob_get` | ❌ |

---

## Test 101

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `azmcp_deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.448918 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.382240 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.374896 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.373363 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.350101 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.338440 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.322906 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.297769 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.262768 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.240757 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.234539 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.230063 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.222416 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.212123 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.211085 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.203821 | `azmcp_sql_server_delete` | ❌ |
| 19 | 0.198896 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.196173 | `azmcp_workbooks_delete` | ❌ |

---

## Test 102

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688055 | `azmcp_deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.497869 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.432825 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.425393 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.421744 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.413718 | `azmcp_loadtesting_test_create` | ❌ |
| 9 | 0.393518 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.365875 | `azmcp_foundry_models_deploy` | ❌ |
| 11 | 0.344407 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.312839 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.301242 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.299546 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.296623 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.292650 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.277722 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.258265 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.252719 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.249358 | `azmcp_storage_blob_container_create` | ❌ |

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
| 5 | 0.496537 | `azmcp_subscription_list` | ❌ |
| 6 | 0.496002 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 7 | 0.492690 | `azmcp_group_list` | ❌ |
| 8 | 0.485615 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.484509 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.475667 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.475336 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.472764 | `azmcp_grafana_list` | ❌ |
| 13 | 0.470300 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.442229 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.440634 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.439820 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.438287 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.427404 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.422414 | `azmcp_datadog_monitoredresources_list` | ❌ |
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
| 3 | 0.478334 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.475119 | `azmcp_search_service_list` | ❌ |
| 5 | 0.450767 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.441607 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.437153 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.431249 | `azmcp_subscription_list` | ❌ |
| 9 | 0.430494 | `azmcp_grafana_list` | ❌ |
| 10 | 0.428437 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.425204 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.420072 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.419125 | `azmcp_group_list` | ❌ |
| 14 | 0.408708 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.399253 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.396758 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.390738 | `azmcp_servicebus_topic_details` | ❌ |
| 18 | 0.384689 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.381712 | `azmcp_aks_cluster_list` | ❌ |
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
| 7 | 0.481357 | `azmcp_group_list` | ❌ |
| 8 | 0.481082 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.476808 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.476780 | `azmcp_subscription_list` | ❌ |
| 11 | 0.471888 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.468200 | `azmcp_grafana_list` | ❌ |
| 13 | 0.467062 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.445991 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.429646 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.428727 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.425534 | `azmcp_servicebus_topic_details` | ❌ |
| 18 | 0.421430 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.417893 | `azmcp_aks_cluster_list` | ❌ |
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
| 8 | 0.475493 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.464233 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.460456 | `azmcp_search_service_list` | ❌ |
| 11 | 0.456540 | `azmcp_grafana_list` | ❌ |
| 12 | 0.455379 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.452988 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.448196 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.443120 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.442259 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.432333 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.423027 | `azmcp_postgres_server_list` | ❌ |
| 19 | 0.411811 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.407927 | `azmcp_cosmos_account_list` | ❌ |

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
| 4 | 0.480944 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.478217 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.457868 | `azmcp_search_service_list` | ❌ |
| 7 | 0.445053 | `azmcp_subscription_list` | ❌ |
| 8 | 0.435412 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.434697 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.422093 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.420907 | `azmcp_group_list` | ❌ |
| 12 | 0.417857 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.415223 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.408588 | `azmcp_grafana_list` | ❌ |
| 15 | 0.397665 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.392784 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.382853 | `azmcp_aks_cluster_list` | ❌ |
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
| 4 | 0.498485 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.460145 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.444779 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.443291 | `azmcp_subscription_list` | ❌ |
| 8 | 0.438131 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435639 | `azmcp_search_service_list` | ❌ |
| 10 | 0.434420 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.433763 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.431618 | `azmcp_grafana_list` | ❌ |
| 13 | 0.426989 | `azmcp_group_list` | ❌ |
| 14 | 0.419194 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.402159 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.398589 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.392822 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.386880 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.376613 | `azmcp_aks_cluster_list` | ❌ |
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
| 3 | 0.524919 | `azmcp_group_list` | ❌ |
| 4 | 0.488696 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484167 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 6 | 0.478967 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.474205 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.473708 | `azmcp_servicebus_topic_details` | ❌ |
| 9 | 0.465477 | `azmcp_acr_registry_list` | ❌ |
| 10 | 0.465098 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.462234 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.457114 | `azmcp_search_service_list` | ❌ |
| 13 | 0.452697 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.452191 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.443155 | `azmcp_subscription_list` | ❌ |
| 16 | 0.436651 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.436075 | `azmcp_grafana_list` | ❌ |
| 18 | 0.428724 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.412528 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.409436 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 110

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593274 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.592262 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.525017 | `azmcp_subscription_list` | ❌ |
| 4 | 0.518857 | `azmcp_search_service_list` | ❌ |
| 5 | 0.509007 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.490431 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.489687 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.480579 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.475701 | `azmcp_group_list` | ❌ |
| 10 | 0.475207 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.473274 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.467478 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.460683 | `azmcp_grafana_list` | ❌ |
| 14 | 0.451759 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.440698 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.439125 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.422468 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.415047 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.410171 | `azmcp_acr_registry_list` | ❌ |
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
| 4 | 0.518141 | `azmcp_subscription_list` | ❌ |
| 5 | 0.510115 | `azmcp_group_list` | ❌ |
| 6 | 0.508443 | `azmcp_search_service_list` | ❌ |
| 7 | 0.494659 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.492726 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.485806 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.483521 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.481753 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.481609 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.473868 | `azmcp_grafana_list` | ❌ |
| 14 | 0.467622 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 15 | 0.453352 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.446444 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.428290 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.426897 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.411734 | `azmcp_sql_server_list` | ❌ |
| 20 | 0.410745 | `azmcp_acr_registry_list` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621512 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.557573 | `azmcp_group_list` | ❌ |
| 3 | 0.531331 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.504984 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.502308 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.487301 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.472283 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.467550 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.466908 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.464545 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.459530 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.457119 | `azmcp_grafana_list` | ❌ |
| 13 | 0.440510 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.438267 | `azmcp_subscription_list` | ❌ |
| 15 | 0.438218 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.435258 | `azmcp_search_service_list` | ❌ |
| 17 | 0.431467 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.423121 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.411672 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.410992 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

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
| 4 | 0.478385 | `azmcp_subscription_list` | ❌ |
| 5 | 0.476763 | `azmcp_search_service_list` | ❌ |
| 6 | 0.475698 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.471628 | `azmcp_redis_cluster_list` | ❌ |
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
| 19 | 0.422616 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.420013 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.448179 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.390284 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.380314 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.379655 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.376542 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.372885 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352982 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.347672 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.347347 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.342763 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.341455 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.341448 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.338591 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.337356 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 16 | 0.333091 | `azmcp_extension_azqr` | ❌ |
| 17 | 0.328312 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.323953 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.322437 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.317412 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 115

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607286 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447410 | `azmcp_mysql_server_config_get` | ❌ |
| 3 | 0.424748 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.422312 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.407173 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.397958 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.392881 | `azmcp_appconfig_kv_list` | ❌ |
| 8 | 0.384163 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.383961 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.369511 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.367151 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.363454 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.361775 | `azmcp_deploy_plan_get` | ❌ |
| 14 | 0.353703 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.352033 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.342342 | `azmcp_postgres_server_config_get` | ❌ |
| 17 | 0.321681 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.315678 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.314047 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.312581 | `azmcp_sql_db_list` | ❌ |

---

## Test 116

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
| 7 | 0.320055 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.317583 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.317405 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.312732 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.311384 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.309942 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.305506 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.297797 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.297135 | `azmcp_aks_nodepool_get` | ❌ |
| 16 | 0.295816 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.295150 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 18 | 0.290156 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.281564 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.277653 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 117

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
| 6 | 0.417049 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.396163 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.390827 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.389322 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.387873 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.383857 | `azmcp_sql_server_list` | ❌ |
| 12 | 0.383206 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.378811 | `azmcp_get_bestpractices_get` | ❌ |
| 14 | 0.376071 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.375267 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.368506 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.360165 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.352458 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.348610 | `azmcp_foundry_models_deployments_list` | ❌ |
| 20 | 0.346255 | `azmcp_group_list` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.443459 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.441394 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.391532 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.383917 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.355792 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.353617 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.351217 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.349540 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.347266 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.344702 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.342868 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.337247 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.333000 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.332027 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.325272 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.320889 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.319736 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.318174 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.305803 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 119

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
| 6 | 0.363465 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.358256 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352754 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.351460 | `azmcp_aks_cluster_get` | ❌ |
| 10 | 0.350178 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.349596 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.349013 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.336971 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.335848 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.327763 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.325909 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.325899 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.323655 | `azmcp_foundry_models_deployments_list` | ❌ |
| 19 | 0.323377 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.320512 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644953 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433966 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.388756 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.371000 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.368468 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.368008 | `azmcp_loadtesting_testrun_get` | ❌ |
| 7 | 0.367605 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.355974 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.354945 | `azmcp_search_index_get` | ❌ |
| 10 | 0.349947 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.349545 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.347041 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.343836 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.343452 | `azmcp_get_bestpractices_get` | ❌ |
| 15 | 0.342253 | `azmcp_servicebus_queue_details` | ❌ |
| 16 | 0.338128 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.337571 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.334303 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.330455 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.326101 | `azmcp_quota_usage_check` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554980 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426703 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.418362 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.408011 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.381236 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.364785 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.350663 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.335606 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.325271 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.321466 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.318517 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.313790 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.306310 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.304324 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.302330 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.281401 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.277967 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.267170 | `azmcp_search_service_list` | ❌ |
| 19 | 0.267148 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.266494 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 122

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
| 7 | 0.319024 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.318076 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.310692 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.298434 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.297073 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.295718 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.292793 | `azmcp_cloudarchitect_design` | ❌ |
| 14 | 0.283713 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.272348 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.271200 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.267009 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.266513 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.258431 | `azmcp_search_service_list` | ❌ |
| 20 | 0.241875 | `azmcp_sql_db_list` | ❌ |

---

## Test 123

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.559382 | `azmcp_search_service_list` | ❌ |
| 3 | 0.516618 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.516217 | `azmcp_appconfig_account_list` | ❌ |
| 5 | 0.485259 | `azmcp_subscription_list` | ❌ |
| 6 | 0.474425 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.465575 | `azmcp_group_list` | ❌ |
| 8 | 0.464761 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.462188 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.455829 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.455388 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.445099 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.442691 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.433274 | `azmcp_eventgrid_topic_list` | ❌ |
| 15 | 0.432144 | `azmcp_grafana_list` | ❌ |
| 16 | 0.431611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.429253 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.417070 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.413034 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.411904 | `azmcp_sql_db_list` | ❌ |

---

## Test 124

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.452132 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.436122 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.412646 | `azmcp_search_service_list` | ❌ |
| 5 | 0.411323 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.385832 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.374655 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.372790 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.370781 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.369681 | `azmcp_subscription_list` | ❌ |
| 11 | 0.368251 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.358720 | `azmcp_deploy_plan_get` | ❌ |
| 13 | 0.357329 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.347887 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.347822 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.339873 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.334019 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.333136 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.328017 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.327326 | `azmcp_sql_server_list` | ❌ |

---

## Test 125

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433674 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.348106 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.284362 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.281676 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.249658 | `azmcp_appconfig_account_list` | ❌ |
| 6 | 0.244782 | `azmcp_appconfig_kv_list` | ❌ |
| 7 | 0.240541 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.239514 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.217817 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.214041 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.207391 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.197933 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.195857 | `azmcp_role_assignment_list` | ❌ |
| 14 | 0.194503 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.186328 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.184216 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.184051 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.179069 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.178961 | `azmcp_search_service_list` | ❌ |
| 20 | 0.175281 | `azmcp_subscription_list` | ❌ |

---

## Test 126

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.740327 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.595854 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.590531 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.575960 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.543140 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.526698 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.434682 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.416091 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.414022 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.380493 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.372032 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.352953 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.296644 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.285004 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.267718 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.237081 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.223228 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.219479 | `azmcp_subscription_list` | ❌ |
| 19 | 0.217086 | `azmcp_search_service_list` | ❌ |
| 20 | 0.208317 | `azmcp_workbooks_delete` | ❌ |

---

## Test 127

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628033 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.624457 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.565005 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.539554 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.515732 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.493432 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.483031 | `azmcp_keyvault_secret_get` | ❌ |
| 8 | 0.475385 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.423728 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.418861 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.359751 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.319410 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.293421 | `azmcp_subscription_list` | ❌ |
| 14 | 0.289685 | `azmcp_search_service_list` | ❌ |
| 15 | 0.279322 | `azmcp_search_index_get` | ❌ |
| 16 | 0.276581 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.271911 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.269735 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.266478 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.263115 | `azmcp_storage_account_create` | ❌ |

---

## Test 128

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662322 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.606534 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.574891 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.540155 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.535157 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.499272 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.482380 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.481367 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.459167 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.419391 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.415722 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.412434 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.367879 | `azmcp_search_index_get` | ❌ |
| 14 | 0.365386 | `azmcp_sql_db_show` | ❌ |
| 15 | 0.350930 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.332770 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.331665 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.317904 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.305778 | `azmcp_subscription_list` | ❌ |
| 20 | 0.301710 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 129

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649993 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.521183 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.469749 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.467097 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.426600 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.398035 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.386025 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.364868 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.354888 | `azmcp_keyvault_secret_get` | ❌ |
| 10 | 0.337967 | `azmcp_keyvault_secret_list` | ❌ |
| 11 | 0.248212 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.228943 | `azmcp_workbooks_delete` | ❌ |
| 13 | 0.222971 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.205016 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.182055 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.180219 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.174606 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.170326 | `azmcp_subscription_list` | ❌ |
| 19 | 0.158491 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.153106 | `azmcp_search_service_list` | ❌ |

---

## Test 130

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649676 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.629902 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.527468 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.525827 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.491898 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.472232 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.405961 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.399857 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.377865 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.371481 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.259893 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.256829 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.250432 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.233999 | `azmcp_workbooks_delete` | ❌ |
| 15 | 0.211540 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.209234 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.203658 | `azmcp_sql_server_create` | ❌ |
| 18 | 0.197598 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.196777 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.189710 | `azmcp_sql_server_delete` | ❌ |

---

## Test 131

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.762015 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.637437 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.608799 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.566512 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.539624 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.484660 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.484299 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.478100 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.453252 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.437349 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.408042 | `azmcp_subscription_list` | ❌ |
| 12 | 0.394434 | `azmcp_search_service_list` | ❌ |
| 13 | 0.393940 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.363630 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.362873 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.358938 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.350862 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.339860 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 19 | 0.336779 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 20 | 0.330749 | `azmcp_redis_cache_list` | ❌ |

---

## Test 132

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660575 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.570341 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.540046 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.516740 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.509147 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.496889 | `azmcp_keyvault_key_get` | ❌ |
| 7 | 0.483411 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.458639 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.420474 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.397038 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.396103 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.362736 | `azmcp_subscription_list` | ❌ |
| 13 | 0.355850 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.344382 | `azmcp_search_service_list` | ❌ |
| 15 | 0.323157 | `azmcp_role_assignment_list` | ❌ |
| 16 | 0.309874 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.305641 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.295856 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.290311 | `azmcp_search_index_get` | ❌ |
| 20 | 0.286685 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 133

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
| 7 | 0.413161 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.412581 | `azmcp_keyvault_certificate_import` | ❌ |
| 9 | 0.397141 | `azmcp_appconfig_kv_set` | ❌ |
| 10 | 0.389776 | `azmcp_keyvault_certificate_get` | ❌ |
| 11 | 0.372078 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.338097 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.283851 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.276139 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.261607 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.230534 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.223719 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.215837 | `azmcp_subscription_list` | ❌ |
| 19 | 0.212003 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.199592 | `azmcp_sql_server_firewall-rule_create` | ❌ |

---

## Test 134

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586316 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.554944 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.509248 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.501027 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.486714 | `azmcp_keyvault_certificate_list` | ❌ |
| 6 | 0.486385 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.484359 | `azmcp_keyvault_certificate_get` | ❌ |
| 8 | 0.439610 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.432344 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.417807 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.379569 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.329665 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.305753 | `azmcp_subscription_list` | ❌ |
| 14 | 0.281000 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.279008 | `azmcp_search_index_get` | ❌ |
| 16 | 0.276633 | `azmcp_search_service_list` | ❌ |
| 17 | 0.274427 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.268770 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.267669 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.256231 | `azmcp_quota_usage_check` | ❌ |

---

## Test 135

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636333 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.545212 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.535119 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.504441 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.499133 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.478664 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.475152 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.470223 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.452402 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.430080 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.429437 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.427286 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.368241 | `azmcp_search_index_get` | ❌ |
| 14 | 0.347124 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.340626 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.337168 | `azmcp_servicebus_queue_details` | ❌ |
| 17 | 0.326335 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.316002 | `azmcp_subscription_list` | ❌ |
| 19 | 0.315915 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.311524 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737135 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.650155 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.631528 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.531109 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.498767 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.468069 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.467326 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.463739 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.455826 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.443785 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.430322 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.426909 | `azmcp_subscription_list` | ❌ |
| 13 | 0.408341 | `azmcp_search_service_list` | ❌ |
| 14 | 0.388134 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.373903 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.368258 | `azmcp_mysql_database_list` | ❌ |
| 17 | 0.354804 | `azmcp_monitor_table_list` | ❌ |
| 18 | 0.353714 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.350260 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.348597 | `azmcp_role_assignment_list` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609392 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.562170 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.535381 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.520010 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.500591 | `azmcp_keyvault_secret_get` | ❌ |
| 6 | 0.479818 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.462249 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.429515 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.421475 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.412607 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.406776 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.357490 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.353390 | `azmcp_subscription_list` | ❌ |
| 14 | 0.327200 | `azmcp_search_service_list` | ❌ |
| 15 | 0.316124 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.308968 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.306567 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.296808 | `azmcp_search_index_get` | ❌ |
| 19 | 0.295954 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.293404 | `azmcp_quota_usage_check` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.767701 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.613514 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.572297 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.531680 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.516457 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.461437 | `azmcp_appconfig_kv_set` | ❌ |
| 7 | 0.423999 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.417525 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.411481 | `azmcp_keyvault_certificate_import` | ❌ |
| 10 | 0.391038 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.387507 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 12 | 0.357221 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.288052 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.287955 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.286849 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.246174 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.243905 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.218660 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 19 | 0.212873 | `azmcp_storage_blob_upload` | ❌ |
| 20 | 0.209815 | `azmcp_subscription_list` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618944 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.578206 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.549693 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.514888 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.482166 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.456712 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.442728 | `azmcp_keyvault_certificate_list` | ❌ |
| 8 | 0.423151 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.420938 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.398036 | `azmcp_keyvault_certificate_import` | ❌ |
| 11 | 0.382573 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.349033 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.301544 | `azmcp_subscription_list` | ❌ |
| 14 | 0.294686 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.283833 | `azmcp_search_index_get` | ❌ |
| 16 | 0.281795 | `azmcp_search_service_list` | ❌ |
| 17 | 0.260730 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.257699 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.255278 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.254379 | `azmcp_sql_db_show` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607510 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.583025 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.564291 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.531971 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.503145 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.485180 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.483567 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.446029 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.444462 | `azmcp_keyvault_certificate_list` | ❌ |
| 10 | 0.436761 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.413715 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.408665 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.378264 | `azmcp_search_index_get` | ❌ |
| 14 | 0.355180 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.346830 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.335069 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.333928 | `azmcp_servicebus_queue_details` | ❌ |
| 18 | 0.324284 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.321636 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.311552 | `azmcp_subscription_list` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747343 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.617131 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.569911 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.536331 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.519133 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.473595 | `azmcp_keyvault_key_get` | ❌ |
| 7 | 0.455500 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.433211 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.417973 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.414315 | `azmcp_keyvault_certificate_get` | ❌ |
| 11 | 0.391082 | `azmcp_subscription_list` | ❌ |
| 12 | 0.388773 | `azmcp_search_service_list` | ❌ |
| 13 | 0.387663 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.367556 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.340472 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 16 | 0.337595 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.334206 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.331203 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.326425 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.322010 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615400 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.577858 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.520896 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.520654 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.502403 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.467743 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.456363 | `azmcp_keyvault_certificate_get` | ❌ |
| 8 | 0.411604 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.410957 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.409126 | `azmcp_keyvault_certificate_import` | ❌ |
| 11 | 0.401434 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.371811 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.345256 | `azmcp_subscription_list` | ❌ |
| 14 | 0.328354 | `azmcp_search_service_list` | ❌ |
| 15 | 0.304751 | `azmcp_search_index_get` | ❌ |
| 16 | 0.303769 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.299021 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.294784 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.293826 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.290273 | `azmcp_mysql_database_list` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660869 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.611425 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.579676 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.540767 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.481416 | `azmcp_mysql_server_config_get` | ❌ |
| 6 | 0.463682 | `azmcp_kusto_cluster_get` | ❌ |
| 7 | 0.463065 | `azmcp_loadtesting_test_get` | ❌ |
| 8 | 0.430975 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.419586 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.399345 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.391924 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.390959 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.390819 | `azmcp_appconfig_kv_list` | ❌ |
| 14 | 0.390141 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.371669 | `azmcp_mysql_server_param_get` | ❌ |
| 16 | 0.370599 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.369379 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.367848 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.360930 | `azmcp_storage_blob_get` | ❌ |
| 20 | 0.355388 | `azmcp_sql_server_list` | ❌ |

---

## Test 144

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666849 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.589102 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.545820 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.530314 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.508226 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.461466 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.448840 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.428449 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.422993 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.413511 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.408385 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.396636 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.396256 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.390889 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.385261 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.384654 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.383279 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.377793 | `azmcp_storage_blob_get` | ❌ |
| 19 | 0.365736 | `azmcp_search_index_get` | ❌ |
| 20 | 0.362332 | `azmcp_sql_db_list` | ❌ |

---

## Test 145

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567273 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563028 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.493940 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.486040 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.380301 | `azmcp_mysql_server_config_get` | ❌ |
| 6 | 0.368584 | `azmcp_kusto_cluster_get` | ❌ |
| 7 | 0.348885 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.342696 | `azmcp_loadtesting_test_get` | ❌ |
| 9 | 0.340293 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.334923 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.334905 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.329337 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.315228 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.314513 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.309738 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.299224 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.296898 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.296592 | `azmcp_postgres_server_config_get` | ❌ |
| 19 | 0.289369 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.275751 | `azmcp_sql_db_show` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661426 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.578657 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.563549 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.534089 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.503925 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.434587 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.433929 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.419338 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.418560 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.417836 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.405658 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.405015 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.402155 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.399759 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.391717 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.384782 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.383956 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.372812 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367547 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.359877 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 147

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801061 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.690255 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.599956 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.594509 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.562043 | `azmcp_search_service_list` | ❌ |
| 6 | 0.560861 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.543848 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.515922 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.509202 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.502401 | `azmcp_subscription_list` | ❌ |
| 11 | 0.498286 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 12 | 0.498121 | `azmcp_group_list` | ❌ |
| 13 | 0.495977 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.486142 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.483592 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.482355 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.481469 | `azmcp_grafana_list` | ❌ |
| 18 | 0.457949 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.452959 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.452681 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 148

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608053 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.536412 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.500890 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.492910 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.455228 | `azmcp_search_service_list` | ❌ |
| 6 | 0.446353 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.428428 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.416475 | `azmcp_aks_nodepool_get` | ❌ |
| 9 | 0.409711 | `azmcp_kusto_cluster_get` | ❌ |
| 10 | 0.408385 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.393148 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.376362 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.371836 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.371655 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.371115 | `azmcp_search_index_get` | ❌ |
| 16 | 0.363804 | `azmcp_subscription_list` | ❌ |
| 17 | 0.361928 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.358652 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.356926 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.356016 | `azmcp_storage_account_get` | ❌ |

---

## Test 149

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623888 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.538749 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.530023 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.466749 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.449602 | `azmcp_kusto_cluster_list` | ❌ |
| 6 | 0.416621 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.392063 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.379479 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.378796 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.377567 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.374464 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.364022 | `azmcp_deploy_app_logs_get` | ❌ |
| 13 | 0.353365 | `azmcp_search_service_list` | ❌ |
| 14 | 0.345290 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.345241 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.337354 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.317977 | `azmcp_sql_elastic-pool_list` | ❌ |
| 18 | 0.317238 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.312354 | `azmcp_subscription_list` | ❌ |
| 20 | 0.311971 | `azmcp_quota_usage_check` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.753920 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.699423 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.597308 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.498591 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.482683 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.468392 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.463192 | `azmcp_sql_elastic-pool_list` | ❌ |
| 8 | 0.434875 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.414751 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 10 | 0.401637 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.399215 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.384390 | `azmcp_keyvault_key_get` | ❌ |
| 13 | 0.383565 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 14 | 0.382307 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.380152 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.378294 | `azmcp_search_index_get` | ❌ |
| 17 | 0.378264 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.370170 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.362512 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.356766 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678158 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.640096 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.481312 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.458596 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.446026 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.440182 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.389989 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 8 | 0.384600 | `azmcp_loadtesting_test_get` | ❌ |
| 9 | 0.371847 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.367502 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.365231 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.357721 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.351037 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.350992 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.344818 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.343726 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.342564 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.338343 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.332511 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.322685 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599506 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.582325 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.412109 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.391580 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.385173 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 6 | 0.383045 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.346262 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 8 | 0.338624 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.323027 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.320794 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.314466 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.313226 | `azmcp_sql_server_list` | ❌ |
| 13 | 0.306678 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.306585 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.300123 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.298866 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.289422 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.287084 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.283160 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.276058 | `azmcp_sql_db_list` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694117 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.615516 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.531976 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.506624 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.487707 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.461701 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.446759 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.440641 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.438637 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.435177 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.431369 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.418681 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 13 | 0.413085 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.407783 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.404890 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.399249 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.393850 | `azmcp_group_list` | ❌ |
| 18 | 0.391869 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.389071 | `azmcp_redis_cluster_database_list` | ❌ |
| 20 | 0.385781 | `azmcp_workbooks_list` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712299 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.644451 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.547452 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.510269 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509732 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 6 | 0.497966 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.447626 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.441522 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.441510 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 10 | 0.433138 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.430830 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.430739 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.421390 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.408990 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.408569 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.407619 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.390197 | `azmcp_redis_cluster_database_list` | ❌ |
| 18 | 0.388906 | `azmcp_group_list` | ❌ |
| 19 | 0.387579 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.383202 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623138 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.580535 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.453743 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.443902 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.425448 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.409286 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.387005 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.378796 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.368944 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.363290 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.359493 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 12 | 0.356345 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.356139 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.354555 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.348078 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.335285 | `azmcp_sql_server_list` | ❌ |
| 17 | 0.329036 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.324552 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.324257 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.323715 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 156

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
| 11 | 0.323557 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.310466 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.310116 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.299453 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.296991 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.291326 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.290957 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.288940 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.280434 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.273769 | `azmcp_sql_server_list` | ❌ |

---

## Test 157

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642508 | `azmcp_loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.608980 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.574045 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.534361 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.473021 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.469973 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.436763 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.404840 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.397565 | `azmcp_group_list` | ❌ |
| 10 | 0.379652 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.373362 | `azmcp_loadtesting_testrun_update` | ❌ |
| 12 | 0.370168 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.365570 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.360757 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.354685 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.347387 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 17 | 0.341655 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.340445 | `azmcp_extension_azqr` | ❌ |
| 19 | 0.329469 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.328383 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.717577 | `azmcp_loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.596828 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.514437 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.476662 | `azmcp_loadtesting_testrun_create` | ❌ |
| 5 | 0.443117 | `azmcp_loadtesting_test_get` | ❌ |
| 6 | 0.442258 | `azmcp_workbooks_create` | ❌ |
| 7 | 0.416885 | `azmcp_group_list` | ❌ |
| 8 | 0.407822 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.394967 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.382774 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.370093 | `azmcp_loadtesting_testrun_get` | ❌ |
| 12 | 0.369786 | `azmcp_sql_server_list` | ❌ |
| 13 | 0.369409 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.356801 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.350904 | `azmcp_loadtesting_testrun_update` | ❌ |
| 16 | 0.343649 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.342252 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.341251 | `azmcp_grafana_list` | ❌ |
| 19 | 0.335696 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.326617 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 159

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
| 7 | 0.515675 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.511607 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.506184 | `azmcp_loadtesting_test_get` | ❌ |
| 10 | 0.497916 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.487330 | `azmcp_grafana_list` | ❌ |
| 12 | 0.483681 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.478586 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.473567 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.473444 | `azmcp_search_service_list` | ❌ |
| 16 | 0.470899 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.463466 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.452403 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.447138 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.433793 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 160

**Expected Tool:** `azmcp_loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621803 | `azmcp_loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.592805 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.540789 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.530871 | `azmcp_loadtesting_testrun_update` | ❌ |
| 5 | 0.488142 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.469444 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.418431 | `azmcp_loadtesting_testrun_list` | ❌ |
| 8 | 0.411627 | `azmcp_loadtesting_testresource_list` | ❌ |
| 9 | 0.401948 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.383753 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.362695 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.331019 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.325463 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.323772 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.315375 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.306420 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.273429 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.272151 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.267551 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.262297 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 161

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624970 | `azmcp_loadtesting_test_get` | ❌ |
| 2 | 0.602729 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.568349 | `azmcp_loadtesting_testrun_get` | ✅ **EXPECTED** |
| 4 | 0.562147 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.536150 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.496538 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.434765 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.415684 | `azmcp_loadtesting_testrun_update` | ❌ |
| 9 | 0.397255 | `azmcp_group_list` | ❌ |
| 10 | 0.394591 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.369869 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.366478 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.356013 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.353260 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.352686 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.346757 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.339473 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.330218 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.329673 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.328702 | `azmcp_sql_db_show` | ❌ |

---

## Test 162

**Expected Tool:** `azmcp_loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615977 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.606014 | `azmcp_loadtesting_test_get` | ❌ |
| 3 | 0.569125 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.565064 | `azmcp_loadtesting_testrun_list` | ✅ **EXPECTED** |
| 5 | 0.535176 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.492638 | `azmcp_loadtesting_testrun_create` | ❌ |
| 7 | 0.432182 | `azmcp_group_list` | ❌ |
| 8 | 0.416490 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.411003 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.406462 | `azmcp_loadtesting_test_create` | ❌ |
| 11 | 0.395970 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.391910 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.391189 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.375784 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.373935 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.368000 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.356860 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.342614 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.340627 | `azmcp_workbooks_show` | ❌ |
| 20 | 0.329530 | `azmcp_sql_db_list` | ❌ |

---

## Test 163

**Expected Tool:** `azmcp_loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659828 | `azmcp_loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.509133 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.454711 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.443782 | `azmcp_loadtesting_test_get` | ❌ |
| 5 | 0.422012 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.399484 | `azmcp_loadtesting_test_create` | ❌ |
| 7 | 0.384679 | `azmcp_loadtesting_testresource_list` | ❌ |
| 8 | 0.384213 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.332813 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.320213 | `azmcp_workbooks_update` | ❌ |
| 11 | 0.299972 | `azmcp_workbooks_create` | ❌ |
| 12 | 0.268285 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.267279 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.255584 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.253373 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.252294 | `azmcp_sql_server_list` | ❌ |
| 17 | 0.250190 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.245766 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.242738 | `azmcp_workbooks_delete` | ❌ |
| 20 | 0.233804 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 164

**Expected Tool:** `azmcp_grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578892 | `azmcp_grafana_list` | ✅ **EXPECTED** |
| 2 | 0.551851 | `azmcp_search_service_list` | ❌ |
| 3 | 0.513135 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.505836 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.498077 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.493712 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.492724 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.492210 | `azmcp_subscription_list` | ❌ |
| 9 | 0.491755 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.489846 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.482789 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.479611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.459171 | `azmcp_eventgrid_topic_list` | ❌ |
| 14 | 0.457845 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.452186 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.447865 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.447597 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.441315 | `azmcp_group_list` | ❌ |
| 19 | 0.440392 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.436794 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 165

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750564 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.516886 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.513156 | `azmcp_search_service_list` | ❌ |
| 5 | 0.508367 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.500471 | `azmcp_subscription_list` | ❌ |
| 7 | 0.499290 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.480850 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.477184 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.472868 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.460936 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.460346 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.451887 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.450971 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.448426 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.447269 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.445430 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.442506 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.438952 | `azmcp_grafana_list` | ❌ |
| 20 | 0.437939 | `azmcp_postgres_server_list` | ❌ |

---

## Test 166

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743789 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.613217 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.519986 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.514290 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.492115 | `azmcp_acr_registry_repository_list` | ❌ |
| 6 | 0.477847 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.466545 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.452905 | `azmcp_acr_registry_list` | ❌ |
| 9 | 0.443767 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.441644 | `azmcp_group_list` | ❌ |
| 11 | 0.433933 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.412775 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.412747 | `azmcp_search_service_list` | ❌ |
| 14 | 0.409044 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.407704 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.406511 | `azmcp_mysql_database_list` | ❌ |
| 17 | 0.402926 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.398402 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.398168 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.397222 | `azmcp_functionapp_get` | ❌ |

---

## Test 167

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_required-subnet-size`  
**Prompt:** Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646978 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ✅ **EXPECTED** |
| 2 | 0.450353 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.327359 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.235376 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.204491 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.204313 | `azmcp_aks_nodepool_get` | ❌ |
| 7 | 0.203596 | `azmcp_quota_usage_check` | ❌ |
| 8 | 0.198992 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.192371 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.188378 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.186379 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.176561 | `azmcp_marketplace_product_get` | ❌ |
| 13 | 0.175974 | `azmcp_postgres_server_param_get` | ❌ |
| 14 | 0.174849 | `azmcp_aks_nodepool_list` | ❌ |
| 15 | 0.172920 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 16 | 0.170883 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.169581 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.166628 | `azmcp_applens_resource_diagnose` | ❌ |
| 19 | 0.165332 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.165173 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 168

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836071 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ✅ **EXPECTED** |
| 2 | 0.626122 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.453801 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.444792 | `azmcp_search_service_list` | ❌ |
| 5 | 0.438893 | `azmcp_quota_region_availability_list` | ❌ |
| 6 | 0.418652 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.411881 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 8 | 0.411331 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.405905 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.403218 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.402635 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.401697 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.401538 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.399919 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.398908 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.398741 | `azmcp_subscription_list` | ❌ |
| 17 | 0.395033 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.393969 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.393542 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.392603 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 169

**Expected Tool:** `azmcp_marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570164 | `azmcp_marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.477522 | `azmcp_marketplace_product_list` | ❌ |
| 3 | 0.353256 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.330935 | `azmcp_servicebus_queue_details` | ❌ |
| 5 | 0.323721 | `azmcp_search_index_get` | ❌ |
| 6 | 0.323704 | `azmcp_servicebus_topic_details` | ❌ |
| 7 | 0.317373 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.302335 | `azmcp_aks_cluster_get` | ❌ |
| 9 | 0.294798 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.289354 | `azmcp_workbooks_show` | ❌ |
| 11 | 0.285577 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.283554 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.276826 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.274403 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.266271 | `azmcp_foundry_models_list` | ❌ |
| 16 | 0.259116 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.257285 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.254378 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 19 | 0.253913 | `azmcp_keyvault_key_get` | ❌ |
| 20 | 0.251014 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 170

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527077 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443128 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.343549 | `azmcp_search_service_list` | ❌ |
| 4 | 0.330500 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.328676 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 6 | 0.324866 | `azmcp_search_index_query` | ❌ |
| 7 | 0.302418 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.290877 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.289538 | `azmcp_search_index_get` | ❌ |
| 10 | 0.287924 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.264103 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.263783 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.258243 | `azmcp_foundry_models_deployments_list` | ❌ |
| 14 | 0.254438 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.251532 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.250343 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.248822 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 18 | 0.247644 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.245634 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.245271 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 171

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461616 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385197 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.308769 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.260387 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.259270 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.238787 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.238238 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.237988 | `azmcp_grafana_list` | ❌ |
| 9 | 0.226689 | `azmcp_search_service_list` | ❌ |
| 10 | 0.221138 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.218705 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.208553 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.204870 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.203983 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.203721 | `azmcp_eventgrid_topic_list` | ❌ |
| 16 | 0.202641 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.202430 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.201780 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 19 | 0.187847 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.185423 | `azmcp_subscription_list` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.635406 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.531727 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490235 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.447644 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.438801 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.378946 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 9 | 0.354191 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.353355 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.351664 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.322785 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.312391 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.312077 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.292039 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.290398 | `azmcp_search_service_list` | ❌ |
| 17 | 0.282195 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.276345 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.273825 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.273557 | `azmcp_storage_account_get` | ❌ |

---

## Test 173

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
| 8 | 0.409147 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.392171 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369205 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.356238 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.342487 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.306627 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.305637 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.304620 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.304195 | `azmcp_search_service_list` | ❌ |
| 17 | 0.302423 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.302085 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.296142 | `azmcp_sql_db_create` | ❌ |
| 20 | 0.291071 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 174

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.594323 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.518643 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.465572 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.454158 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.430630 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.399411 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.392767 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.384118 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.380286 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.375863 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.362669 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.335373 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.330487 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.329342 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.322797 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.322718 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.317782 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.316805 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.314841 | `azmcp_search_service_list` | ❌ |

---

## Test 175

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
| 6 | 0.400183 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.381822 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.368157 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.367714 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.353238 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 11 | 0.317494 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.292977 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.284617 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.278941 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.275342 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.256382 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.252529 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.241745 | `azmcp_search_index_query` | ❌ |
| 19 | 0.239443 | `azmcp_storage_blob_get` | ❌ |
| 20 | 0.239436 | `azmcp_search_service_list` | ❌ |

---

## Test 176

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
| 9 | 0.377142 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.373497 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.323164 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.317931 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.303572 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290695 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.277946 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.276228 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.275789 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.270375 | `azmcp_search_service_list` | ❌ |
| 19 | 0.269453 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.269109 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 177

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
| 7 | 0.394762 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.394214 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.375723 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.363421 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.332626 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.332015 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.307885 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290894 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.289428 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.289326 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.285018 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.284215 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.275538 | `azmcp_search_index_query` | ❌ |
| 20 | 0.274643 | `azmcp_storage_blob_get` | ❌ |

---

## Test 178

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
| 7 | 0.397916 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
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
| 19 | 0.253420 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.251387 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 179

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
| 10 | 0.336648 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.293848 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.288873 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.259723 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.253005 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.251235 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.249981 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.247679 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.240292 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.231234 | `azmcp_search_index_query` | ❌ |
| 20 | 0.231120 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 180

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
| 6 | 0.344145 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.319970 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.311848 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.301028 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.299148 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.235579 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.232320 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.219200 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.215940 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.210908 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.206254 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.203401 | `azmcp_search_index_query` | ❌ |
| 18 | 0.202242 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.197959 | `azmcp_mysql_database_query` | ❌ |
| 20 | 0.186444 | `azmcp_sql_server_create` | ❌ |

---

## Test 181

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497276 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.493182 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.404372 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.395623 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.385140 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.374154 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.354448 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.348171 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.300092 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.284049 | `azmcp_storage_blob_container_create` | ❌ |
| 11 | 0.266937 | `azmcp_foundry_models_deploy` | ❌ |
| 12 | 0.248999 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.243575 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.234792 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.222005 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.218621 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.210666 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.209463 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.209291 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.195439 | `azmcp_sql_server_create` | ❌ |

---

## Test 182

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498345 | `azmcp_monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.472170 | `azmcp_monitor_workspace_list` | ❌ |
| 3 | 0.467848 | `azmcp_monitor_workspace_log_query` | ❌ |
| 4 | 0.467539 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.463168 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.436971 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.418755 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.413357 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.401596 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.385416 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.380121 | `azmcp_grafana_list` | ❌ |
| 12 | 0.358432 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.342873 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.339320 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.333342 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.314296 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.306134 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.302952 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.297778 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.296741 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 183

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592640 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424141 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.332356 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.315548 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.315310 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.311108 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 7 | 0.305464 | `azmcp_servicebus_queue_details` | ❌ |
| 8 | 0.304735 | `azmcp_grafana_list` | ❌ |
| 9 | 0.303453 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.298853 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.294124 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.287300 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.284519 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.277566 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.274784 | `azmcp_loadtesting_test_get` | ❌ |
| 16 | 0.262141 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.256937 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 18 | 0.254848 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.249144 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.249067 | `azmcp_bicepschema_get` | ❌ |

---

## Test 184

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589859 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.587736 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.551241 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.473461 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.472677 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.459829 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.438998 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.437739 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.431109 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.417098 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.414488 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.403921 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.401901 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.397526 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.391340 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.390444 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.383412 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.378187 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.371065 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.359476 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 185

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
| 6 | 0.359184 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.353264 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.344326 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.341713 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.337874 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.329534 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.326688 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.324002 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.321523 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 15 | 0.317475 | `azmcp_monitor_workspace_log_query` | ❌ |
| 16 | 0.302525 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.301966 | `azmcp_workbooks_show` | ❌ |
| 18 | 0.291565 | `azmcp_cloudarchitect_design` | ❌ |
| 19 | 0.291260 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.287764 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 186

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
| 7 | 0.413137 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.409107 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.388205 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.380075 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.356549 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.350085 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.342526 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.339771 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.335430 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.329460 | `azmcp_loadtesting_testresource_create` | ❌ |
| 17 | 0.326924 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.326802 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.326398 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.320852 | `azmcp_search_index_query` | ❌ |

---

## Test 187

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
| 10 | 0.354799 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 11 | 0.339388 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.336627 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.326899 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.326643 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.321538 | `azmcp_search_service_list` | ❌ |
| 16 | 0.321182 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.318235 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.317565 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.303909 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.303638 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 188

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461249 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390029 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.306338 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.304372 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.301811 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.289462 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.275443 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.267682 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.267390 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 10 | 0.265740 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.263777 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.263325 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.259193 | `azmcp_grafana_list` | ❌ |
| 14 | 0.253542 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.248754 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.247872 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.247663 | `azmcp_applens_resource_diagnose` | ❌ |
| 18 | 0.242263 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.235610 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.229145 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 189

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492129 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.416909 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.415989 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.406242 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.398992 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.397292 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.369898 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.366949 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.361970 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.359279 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.331654 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.316299 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.315275 | `azmcp_functionapp_get` | ❌ |
| 14 | 0.311804 | `azmcp_search_index_query` | ❌ |
| 15 | 0.308735 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.295922 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.293564 | `azmcp_search_service_list` | ❌ |
| 18 | 0.293316 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.288844 | `azmcp_foundry_agents_connect` | ❌ |
| 20 | 0.287884 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 190

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525622 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.384557 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.376661 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.367163 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.299463 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.293104 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 7 | 0.290246 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.277741 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 9 | 0.272345 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.267100 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.266436 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.265477 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.262734 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.261961 | `azmcp_grafana_list` | ❌ |
| 15 | 0.261747 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.248150 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 17 | 0.246574 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.244104 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.242782 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.239463 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 191

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
| 7 | 0.346333 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.331215 | `azmcp_loadtesting_testresource_list` | ❌ |
| 9 | 0.330074 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.328838 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.324932 | `azmcp_search_index_query` | ❌ |
| 12 | 0.319343 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.317459 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.292195 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.290762 | `azmcp_search_service_list` | ❌ |
| 16 | 0.284270 | `azmcp_foundry_agents_connect` | ❌ |
| 17 | 0.282267 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.278491 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.276999 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.265361 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 192

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593952 | `azmcp_monitor_workspace_log_query` | ❌ |
| 2 | 0.579992 | `azmcp_monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.472014 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.469547 | `azmcp_monitor_metrics_query` | ❌ |
| 5 | 0.443317 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.442117 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.392117 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.389887 | `azmcp_grafana_list` | ❌ |
| 9 | 0.365896 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.358817 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.352733 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.345398 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.344505 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.337796 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.320788 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.314232 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 17 | 0.308710 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.307727 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.307002 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.305232 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 193

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850698 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.620508 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.534829 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.511123 | `azmcp_kusto_table_list` | ❌ |
| 6 | 0.502075 | `azmcp_grafana_list` | ❌ |
| 7 | 0.488557 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.443812 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.420389 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.419859 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.413834 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.409199 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.400092 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.397408 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.396780 | `azmcp_search_service_list` | ❌ |
| 16 | 0.376903 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.375176 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.374930 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.366099 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.365781 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 194

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798120 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.600069 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.497065 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.487237 | `azmcp_grafana_list` | ❌ |
| 6 | 0.466630 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.449407 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.427408 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.413678 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.411590 | `azmcp_kusto_table_schema` | ❌ |
| 11 | 0.403863 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.398753 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.389881 | `azmcp_mysql_database_list` | ❌ |
| 14 | 0.376474 | `azmcp_kusto_sample` | ❌ |
| 15 | 0.376338 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.373298 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.370605 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.347853 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.346062 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.343808 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 195

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765523 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.570121 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.504683 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.477280 | `azmcp_grafana_list` | ❌ |
| 6 | 0.447435 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.445347 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.418517 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.416351 | `azmcp_kusto_table_schema` | ❌ |
| 10 | 0.412293 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.404852 | `azmcp_monitor_workspace_log_query` | ❌ |
| 12 | 0.404192 | `azmcp_monitor_metrics_definitions` | ❌ |
| 13 | 0.383613 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.380581 | `azmcp_kusto_sample` | ❌ |
| 15 | 0.374111 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.372490 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.369898 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.361820 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.354757 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.351333 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 196

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843138 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736700 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.576976 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.481189 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.475734 | `azmcp_grafana_list` | ❌ |
| 6 | 0.451212 | `azmcp_mysql_table_schema_get` | ❌ |
| 7 | 0.427934 | `azmcp_kusto_table_schema` | ❌ |
| 8 | 0.427153 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.421484 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.406242 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.391308 | `azmcp_kusto_sample` | ❌ |
| 12 | 0.384679 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.376246 | `azmcp_monitor_metrics_definitions` | ❌ |
| 14 | 0.372991 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.370861 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.367591 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.355116 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.348346 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.340101 | `azmcp_foundry_models_list` | ❌ |
| 20 | 0.339804 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 197

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813910 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `azmcp_grafana_list` | ❌ |
| 3 | 0.659468 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.600802 | `azmcp_search_service_list` | ❌ |
| 5 | 0.583213 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.530433 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.517493 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.513663 | `azmcp_aks_cluster_list` | ❌ |
| 9 | 0.500768 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.494595 | `azmcp_group_list` | ❌ |
| 11 | 0.493730 | `azmcp_subscription_list` | ❌ |
| 12 | 0.475212 | `azmcp_monitor_workspace_log_query` | ❌ |
| 13 | 0.471800 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.470266 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.467655 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.466748 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.463915 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.460452 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.448201 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.444214 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 198

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656138 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.584652 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.531025 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.518275 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474652 | `azmcp_monitor_workspace_log_query` | ❌ |
| 6 | 0.459749 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.444210 | `azmcp_search_service_list` | ❌ |
| 8 | 0.414009 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.386383 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.383585 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.380800 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.373820 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.371420 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.363313 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.358004 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.355365 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.354317 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.353697 | `azmcp_subscription_list` | ❌ |
| 19 | 0.352845 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.351328 | `azmcp_search_index_get` | ❌ |

---

## Test 199

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732977 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `azmcp_grafana_list` | ❌ |
| 3 | 0.579635 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.521316 | `azmcp_monitor_table_type_list` | ❌ |
| 5 | 0.521276 | `azmcp_search_service_list` | ❌ |
| 6 | 0.463378 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.453702 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.439297 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435475 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.428945 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.427186 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.422707 | `azmcp_subscription_list` | ❌ |
| 13 | 0.422379 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.411648 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.411448 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.410078 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.409764 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.404177 | `azmcp_group_list` | ❌ |
| 19 | 0.402680 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.400615 | `azmcp_postgres_server_list` | ❌ |

---

## Test 200

**Expected Tool:** `azmcp_monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591648 | `azmcp_monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.494715 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.485457 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.484159 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.483384 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.427241 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.374939 | `azmcp_monitor_metrics_query` | ❌ |
| 8 | 0.365704 | `azmcp_grafana_list` | ❌ |
| 9 | 0.330182 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.324888 | `azmcp_workbooks_delete` | ❌ |
| 11 | 0.322408 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 12 | 0.315638 | `azmcp_search_service_list` | ❌ |
| 13 | 0.309411 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.299830 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.292089 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.291669 | `azmcp_kusto_query` | ❌ |
| 17 | 0.289714 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.288712 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.287253 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.283993 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 201

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668827 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.434813 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.413173 | `azmcp_monitor_metrics_query` | ❌ |
| 4 | 0.408669 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.401731 | `azmcp_grafana_list` | ❌ |
| 6 | 0.393318 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.386685 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.369805 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.364360 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.356507 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.355415 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.345409 | `azmcp_postgres_database_list` | ❌ |
| 13 | 0.345298 | `azmcp_group_list` | ❌ |
| 14 | 0.330769 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.328867 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.327216 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.306946 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.304097 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.302405 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.296544 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 202

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443481 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.393227 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.374103 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.371017 | `azmcp_grafana_list` | ❌ |
| 6 | 0.370681 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.359274 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.350656 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.343214 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.342468 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.337096 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.320510 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.319895 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.302909 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.289883 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.287318 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.285253 | `azmcp_group_list` | ❌ |
| 18 | 0.274728 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.274589 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.272689 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 203

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `azmcp_quota_usage_check` | ❌ |
| 2 | 0.497434 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.481143 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.476826 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.461928 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 6 | 0.451690 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.440399 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.438387 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.434685 | `azmcp_search_service_list` | ❌ |
| 10 | 0.431096 | `azmcp_deploy_iac_rules_get` | ❌ |
| 11 | 0.423237 | `azmcp_subscription_list` | ❌ |
| 12 | 0.422293 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.417076 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.408114 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.406591 | `azmcp_deploy_plan_get` | ❌ |
| 16 | 0.400363 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.395234 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.391715 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.389180 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.381209 | `azmcp_storage_account_get` | ❌ |

---

## Test 204

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.488377 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.475990 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 5 | 0.473365 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.462743 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 7 | 0.452232 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.448085 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.442021 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.439145 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.426161 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 12 | 0.385771 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.382677 | `azmcp_search_service_list` | ❌ |
| 14 | 0.375770 | `azmcp_subscription_list` | ❌ |
| 15 | 0.375138 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.365859 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.365824 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.360612 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.349469 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.341827 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 205

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.514827 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 4 | 0.504673 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.494872 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.487387 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.481698 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.464304 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.463564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 10 | 0.463468 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.452811 | `azmcp_search_service_list` | ❌ |
| 12 | 0.433938 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.423512 | `azmcp_subscription_list` | ❌ |
| 14 | 0.417356 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.403533 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.398720 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.380268 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.377353 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.376594 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.376262 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 206

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
| 7 | 0.348787 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.337839 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.331375 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.331074 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.328408 | `azmcp_grafana_list` | ❌ |
| 12 | 0.325819 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.313240 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.310624 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.307143 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.297277 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.292791 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.290125 | `azmcp_group_list` | ❌ |
| 19 | 0.287104 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.263276 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 207

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
| 6 | 0.365726 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.358194 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.351637 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.345166 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.345161 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.342266 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.342231 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.338636 | `azmcp_grafana_list` | ❌ |
| 14 | 0.331262 | `azmcp_monitor_metrics_definitions` | ❌ |
| 15 | 0.322571 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.321805 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.305083 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.304570 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.300710 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.297856 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 208

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.483988 | `azmcp_group_list` | ❌ |
| 3 | 0.483125 | `azmcp_subscription_list` | ❌ |
| 4 | 0.478700 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474796 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.471364 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.468596 | `azmcp_search_service_list` | ❌ |
| 8 | 0.460080 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.453130 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.446372 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 11 | 0.430667 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.427950 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.426624 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.425029 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.421599 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.409577 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.403310 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.398493 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.397565 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.396966 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 209

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609705 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.456956 | `azmcp_grafana_list` | ❌ |
| 3 | 0.436747 | `azmcp_subscription_list` | ❌ |
| 4 | 0.435642 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.435539 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.431865 | `azmcp_search_service_list` | ❌ |
| 7 | 0.428663 | `azmcp_group_list` | ❌ |
| 8 | 0.428451 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.421627 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.420804 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.415941 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.410380 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 13 | 0.406766 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.395445 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.390157 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.386800 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.383635 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.373204 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.368511 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.363720 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 210

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757057 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.565047 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.445085 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.377563 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.322930 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.312428 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.307404 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.303531 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.300036 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.298380 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.296657 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.292204 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.286490 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.285062 | `azmcp_search_service_list` | ❌ |
| 15 | 0.284898 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.284304 | `azmcp_grafana_list` | ❌ |
| 17 | 0.283873 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.277696 | `azmcp_subscription_list` | ❌ |
| 19 | 0.274897 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.273744 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 211

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713839 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.523153 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.412421 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.338859 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.286321 | `azmcp_appconfig_kv_list` | ❌ |
| 6 | 0.283725 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.280245 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.266479 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.264599 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.262084 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.258045 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.257957 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.257447 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.257151 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.249585 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.247853 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.246875 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.246871 | `azmcp_grafana_list` | ❌ |
| 19 | 0.246678 | `azmcp_eventgrid_subscription_list` | ❌ |
| 20 | 0.243140 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 212

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.764063 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.653917 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.501880 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.495048 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.472307 | `azmcp_grafana_list` | ❌ |
| 6 | 0.466141 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.464785 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.431968 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.431715 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.423145 | `azmcp_subscription_list` | ❌ |
| 11 | 0.414865 | `azmcp_search_service_list` | ❌ |
| 12 | 0.396477 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.387797 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.381343 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.380457 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.373395 | `azmcp_group_list` | ❌ |
| 17 | 0.368719 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.367794 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.367552 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.364522 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 213

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537885 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.450387 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 3 | 0.441152 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.401235 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.302323 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.283598 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.276141 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.265858 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.262106 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.257556 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.252070 | `azmcp_grafana_list` | ❌ |
| 12 | 0.246413 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.236096 | `azmcp_postgres_table_list` | ❌ |
| 14 | 0.233781 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.231294 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.225079 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.224084 | `azmcp_loadtesting_testrun_list` | ❌ |
| 18 | 0.217990 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.212422 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.210134 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 214

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692210 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.595747 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.461603 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.434924 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.427325 | `azmcp_grafana_list` | ❌ |
| 6 | 0.399303 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.383383 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.382294 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.361735 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.358978 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.353487 | `azmcp_subscription_list` | ❌ |
| 12 | 0.353419 | `azmcp_search_service_list` | ❌ |
| 13 | 0.340964 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.327206 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.315583 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.310802 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.308840 | `azmcp_eventgrid_topic_list` | ❌ |
| 18 | 0.306356 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.305932 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.300248 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 215

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752919 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.603774 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.594994 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.548268 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.538411 | `azmcp_cosmos_database_list` | ❌ |
| 6 | 0.520914 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.471359 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.458244 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.456133 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.449548 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.419621 | `azmcp_postgres_table_list` | ❌ |
| 12 | 0.395297 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.390449 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.385544 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.379937 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.376273 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.366236 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.328453 | `azmcp_aks_nodepool_list` | ❌ |
| 19 | 0.328081 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.324867 | `azmcp_grafana_list` | ❌ |

---

## Test 216

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** Show me the databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.721506 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.562891 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.537788 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.490987 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.481619 | `azmcp_cosmos_database_list` | ❌ |
| 6 | 0.480274 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.434940 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.414701 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.408379 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.397285 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.369018 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.353712 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.351025 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.349880 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.343275 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 16 | 0.325430 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.318982 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.302228 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.294393 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.292180 | `azmcp_grafana_list` | ❌ |

---

## Test 217

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812965 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.679028 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.672104 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.588847 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.569232 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.554298 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.527406 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.503279 | `azmcp_grafana_list` | ❌ |
| 9 | 0.467957 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.462558 | `azmcp_search_service_list` | ❌ |
| 11 | 0.457600 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.455780 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.445496 | `azmcp_group_list` | ❌ |
| 14 | 0.445406 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.443534 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.442886 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 17 | 0.436494 | `azmcp_subscription_list` | ❌ |
| 18 | 0.435221 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.419137 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.411194 | `azmcp_mysql_server_list` | ❌ |

---

## Test 218

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591635 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.514375 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.467519 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.403281 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.385069 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 6 | 0.368020 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.338003 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.329389 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.322157 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.321180 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.305874 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.301294 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.295045 | `azmcp_grafana_list` | ❌ |
| 14 | 0.291684 | `azmcp_postgres_database_list` | ❌ |
| 15 | 0.288103 | `azmcp_aks_nodepool_list` | ❌ |
| 16 | 0.272493 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.261135 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.260993 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.259662 | `azmcp_postgres_server_config_get` | ❌ |
| 20 | 0.252053 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 219

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me the Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.744280 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.607511 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.580866 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.518857 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.494170 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.491276 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.456252 | `azmcp_grafana_list` | ❌ |
| 8 | 0.446568 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.440660 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.412876 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.400256 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 12 | 0.398399 | `azmcp_search_service_list` | ❌ |
| 13 | 0.394654 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.394530 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.389814 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.372221 | `azmcp_group_list` | ❌ |
| 17 | 0.370391 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.369831 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.368926 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.367955 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 220

**Expected Tool:** `azmcp_group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755935 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.566552 | `azmcp_workbooks_list` | ❌ |
| 3 | 0.552633 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.546156 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.545523 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.542878 | `azmcp_grafana_list` | ❌ |
| 7 | 0.530600 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.524796 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.519242 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.518520 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.517060 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.509454 | `azmcp_search_service_list` | ❌ |
| 13 | 0.501065 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.491176 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.490734 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.486716 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.483567 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.479545 | `azmcp_subscription_list` | ❌ |
| 19 | 0.478027 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.477036 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 221

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.463685 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.462589 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.459304 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.453960 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.429014 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.427034 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.407817 | `azmcp_grafana_list` | ❌ |
| 9 | 0.398432 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.396814 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.391278 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.383058 | `azmcp_acr_registry_list` | ❌ |
| 13 | 0.379927 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.375998 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.373796 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.366273 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.351405 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.350999 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.340893 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.328487 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 222

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665771 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.531920 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.523173 | `azmcp_redis_cluster_list` | ❌ |
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
| 15 | 0.460690 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.454965 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.454439 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.437404 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.432994 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.429789 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 223

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
| 6 | 0.330199 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.327691 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.325794 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.324331 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.322121 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.311644 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.308204 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.306616 | `azmcp_grafana_list` | ❌ |
| 14 | 0.292084 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.290698 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.286287 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.284990 | `azmcp_applens_resource_diagnose` | ❌ |
| 18 | 0.284986 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.272387 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.272207 | `azmcp_group_list` | ❌ |

---

## Test 224

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549306 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.510397 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.490090 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 4 | 0.466885 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.455892 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.412608 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.411283 | `azmcp_quota_usage_check` | ❌ |
| 8 | 0.405847 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.403922 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.375351 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.368262 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.349430 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.347885 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.347171 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.346145 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.336357 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.321704 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.318472 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.311399 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.306746 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 225

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 2 | 0.570884 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424920 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.393564 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.386598 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.373883 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.355414 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.352447 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.342229 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.338012 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.337593 | `azmcp_aks_nodepool_get` | ❌ |
| 12 | 0.335752 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.327205 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.321304 | `azmcp_group_list` | ❌ |
| 15 | 0.318379 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.318319 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.316556 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.307248 | `azmcp_applens_resource_diagnose` | ❌ |
| 19 | 0.294197 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.289170 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 226

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.587330 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.578620 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.563493 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.548549 | `azmcp_grafana_list` | ❌ |
| 6 | 0.540583 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.534299 | `azmcp_search_service_list` | ❌ |
| 8 | 0.531356 | `azmcp_quota_region_availability_list` | ❌ |
| 9 | 0.530985 | `azmcp_group_list` | ❌ |
| 10 | 0.508030 | `azmcp_monitor_workspace_list` | ❌ |
| 11 | 0.496651 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.491394 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.491359 | `azmcp_subscription_list` | ❌ |
| 14 | 0.489514 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.484221 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.482623 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.476826 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.465430 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.462544 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.459718 | `azmcp_workbooks_list` | ❌ |

---

## Test 227

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
| 6 | 0.456400 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.441703 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.441430 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.430496 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.419027 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.409363 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.407238 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.406709 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.406423 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.405790 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.403358 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.387835 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.381144 | `azmcp_get_bestpractices_get` | ❌ |
| 19 | 0.379969 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 20 | 0.371846 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 228

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.543421 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.427638 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.420567 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.420351 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.411111 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.411059 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.374215 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.370961 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.363808 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.360118 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.358871 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.354932 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.350454 | `azmcp_group_list` | ❌ |
| 15 | 0.348923 | `azmcp_monitor_metrics_query` | ❌ |
| 16 | 0.338595 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.330185 | `azmcp_extension_azqr` | ❌ |
| 18 | 0.328648 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.324257 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.309414 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 229

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.719917 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `azmcp_search_service_list` | ❌ |
| 3 | 0.531311 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.518372 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.503813 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.470139 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.456526 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.454488 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.446515 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.438780 | `azmcp_subscription_list` | ❌ |
| 11 | 0.427166 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.426698 | `azmcp_grafana_list` | ❌ |
| 13 | 0.420042 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.419011 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.416883 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.411902 | `azmcp_group_list` | ❌ |
| 17 | 0.407099 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.385382 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.378841 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.377848 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 230

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726947 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.513815 | `azmcp_search_service_list` | ❌ |
| 3 | 0.509196 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.491121 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484386 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.474847 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.459791 | `azmcp_subscription_list` | ❌ |
| 8 | 0.431459 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.425644 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.411892 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.410579 | `azmcp_marketplace_product_list` | ❌ |
| 12 | 0.409046 | `azmcp_aks_cluster_list` | ❌ |
| 13 | 0.404886 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.390652 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.390483 | `azmcp_group_list` | ❌ |
| 16 | 0.390381 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.385710 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.384914 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.384613 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.381218 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 231

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
| 9 | 0.187892 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.185941 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.174872 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.170157 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.169947 | `azmcp_servicebus_queue_details` | ❌ |
| 14 | 0.164622 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.164285 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.163022 | `azmcp_monitor_workspace_log_query` | ❌ |
| 17 | 0.155791 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.155447 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.151722 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.149112 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 232

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
| 9 | 0.436114 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.411793 | `azmcp_grafana_list` | ❌ |
| 11 | 0.408792 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.407707 | `azmcp_subscription_list` | ❌ |
| 13 | 0.407212 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.404995 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.391992 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.379016 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.371279 | `azmcp_group_list` | ❌ |
| 18 | 0.368866 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.358728 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.357139 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 233

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527706 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.437868 | `azmcp_search_service_list` | ❌ |
| 3 | 0.402493 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.402223 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.400175 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.397735 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.382901 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.382581 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.375034 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.372844 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.371691 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.363470 | `azmcp_get_bestpractices_get` | ❌ |
| 13 | 0.362214 | `azmcp_applens_resource_diagnose` | ❌ |
| 14 | 0.360356 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.357577 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.341495 | `azmcp_foundry_models_deployments_list` | ❌ |
| 17 | 0.337581 | `azmcp_search_index_get` | ❌ |
| 18 | 0.335441 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.333429 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.333201 | `azmcp_subscription_list` | ❌ |

---

## Test 234

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `azmcp_servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.400870 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.375386 | `azmcp_aks_cluster_get` | ❌ |
| 5 | 0.360779 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.352789 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.352705 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.350864 | `azmcp_search_index_get` | ❌ |
| 9 | 0.344531 | `azmcp_eventgrid_subscription_list` | ❌ |
| 10 | 0.342331 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.337239 | `azmcp_sql_db_show` | ❌ |
| 12 | 0.332541 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.327611 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.323287 | `azmcp_marketplace_product_get` | ❌ |
| 15 | 0.323046 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.310614 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.309214 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.301971 | `azmcp_keyvault_key_get` | ❌ |
| 19 | 0.296398 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.290417 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 235

**Expected Tool:** `azmcp_servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591649 | `azmcp_servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571861 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.497732 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.494877 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.483976 | `azmcp_servicebus_queue_details` | ❌ |
| 6 | 0.365149 | `azmcp_search_index_get` | ❌ |
| 7 | 0.361354 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.352554 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.341289 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.340036 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.337675 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.335558 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.333396 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.330814 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.326247 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.317518 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.306388 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.297323 | `azmcp_grafana_list` | ❌ |
| 19 | 0.290399 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.287440 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 236

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `azmcp_servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.523134 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.494515 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.457036 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.444633 | `azmcp_marketplace_product_get` | ❌ |
| 6 | 0.443942 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.429458 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.426573 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.421009 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.411293 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.409632 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.405380 | `azmcp_search_service_list` | ❌ |
| 13 | 0.404807 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.395789 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.395176 | `azmcp_grafana_list` | ❌ |
| 16 | 0.382225 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.369986 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.368411 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.368155 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.367649 | `azmcp_group_list` | ❌ |

---

## Test 237

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470892 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.376431 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.371321 | `azmcp_appservice_database_add` | ❌ |
| 5 | 0.359945 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.357421 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.355614 | `azmcp_postgres_database_list` | ❌ |
| 8 | 0.347128 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.346776 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.329705 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.327846 | `azmcp_sql_db_delete` | ❌ |
| 12 | 0.311744 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.301259 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.297803 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.279490 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.276192 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.272135 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.254831 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.238999 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.231273 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 238

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459672 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.424021 | `azmcp_appservice_database_add` | ❌ |
| 4 | 0.420843 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.396359 | `azmcp_sql_db_update` | ❌ |
| 6 | 0.394711 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.384660 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.371534 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.361051 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.343133 | `azmcp_sql_db_delete` | ❌ |
| 11 | 0.326611 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 12 | 0.324123 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.321862 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.317180 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.311150 | `azmcp_keyvault_key_create` | ❌ |
| 16 | 0.304604 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.301487 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.290173 | `azmcp_keyvault_certificate_create` | ❌ |
| 19 | 0.286796 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 20 | 0.276688 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604311 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545785 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.494347 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.473973 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.456040 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.454303 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.441003 | `azmcp_appservice_database_add` | ❌ |
| 8 | 0.431102 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.422829 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.413566 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.398645 | `azmcp_loadtesting_testresource_create` | ❌ |
| 12 | 0.392671 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.390157 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.379104 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.378513 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.374874 | `azmcp_sql_elastic-pool_list` | ❌ |
| 17 | 0.365856 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.358545 | `azmcp_kusto_table_list` | ❌ |
| 19 | 0.323526 | `azmcp_group_list` | ❌ |
| 20 | 0.319323 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 240

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.519906 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.484097 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.386564 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.364776 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.351204 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.350121 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.345061 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.338006 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.337699 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.317215 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.300711 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.286886 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.284794 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.278895 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.271038 | `azmcp_appconfig_kv_delete` | ❌ |
| 16 | 0.253808 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.243201 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.235173 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.211680 | `azmcp_kusto_query` | ❌ |
| 20 | 0.183587 | `azmcp_kusto_sample` | ❌ |

---

## Test 241

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577180 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.500756 | `azmcp_sql_db_show` | ❌ |
| 3 | 0.478729 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.466303 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 5 | 0.437112 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.421365 | `azmcp_sql_db_create` | ❌ |
| 7 | 0.412762 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.392815 | `azmcp_workbooks_delete` | ❌ |
| 9 | 0.390309 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.388379 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.381382 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.380074 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.370486 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.368841 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.345612 | `azmcp_group_list` | ❌ |
| 16 | 0.334517 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.332517 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.327408 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.310437 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.304849 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 242

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.457992 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.427567 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.364494 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.355416 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.319617 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.314916 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.311506 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.310758 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.305059 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 10 | 0.295355 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.295012 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.294781 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.288554 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.283955 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.258711 | `azmcp_appconfig_kv_delete` | ❌ |
| 16 | 0.246948 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.229764 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.213266 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.187992 | `azmcp_kusto_query` | ❌ |
| 20 | 0.171883 | `azmcp_kusto_sample` | ❌ |

---

## Test 243

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639694 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.609178 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.602893 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.532391 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.529058 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.527896 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.486554 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.485960 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.475733 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.475520 | `azmcp_sql_server_delete` | ❌ |
| 12 | 0.474927 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.457314 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.441355 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.440528 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.400489 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.395078 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.394543 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.380402 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.373624 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 244

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617695 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.609322 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.557353 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.553488 | `azmcp_mysql_server_config_get` | ❌ |
| 5 | 0.524274 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.471862 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.461657 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.458742 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.451453 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.446512 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.445291 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.387645 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.385113 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.380428 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.357337 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.354581 | `azmcp_aks_nodepool_list` | ❌ |
| 17 | 0.349880 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.347075 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.342792 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.342284 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 245

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610866 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.593150 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.530422 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.528136 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465693 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.446673 | `azmcp_postgres_server_param_get` | ❌ |
| 7 | 0.438959 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.398181 | `azmcp_mysql_table_schema_get` | ❌ |
| 9 | 0.397510 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.396416 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.371413 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.325945 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.325788 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.320054 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.317619 | `azmcp_appservice_database_add` | ❌ |
| 16 | 0.297839 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.294987 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.281546 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.279980 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 20 | 0.273566 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 246

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530095 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503626 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.440073 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.438622 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.432919 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.421862 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.400963 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.398714 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.375668 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.361500 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.344694 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.337975 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.323587 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.300133 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.299814 | `azmcp_aks_cluster_get` | ❌ |
| 16 | 0.296827 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.291629 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.285843 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.274274 | `azmcp_appservice_database_add` | ❌ |
| 20 | 0.268405 | `azmcp_functionapp_get` | ❌ |

---

## Test 247

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565512 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.427621 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.385777 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.384192 | `azmcp_appservice_database_add` | ❌ |
| 6 | 0.371461 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.368957 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.368957 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.344847 | `azmcp_sql_db_delete` | ❌ |
| 10 | 0.334336 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.316890 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.273787 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.270134 | `azmcp_kusto_table_schema` | ❌ |
| 14 | 0.263397 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.250975 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.250753 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.240663 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.230967 | `azmcp_loadtesting_testrun_create` | ❌ |
| 19 | 0.223147 | `azmcp_loadtesting_test_create` | ❌ |
| 20 | 0.223086 | `azmcp_kusto_query` | ❌ |

---

## Test 248

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.401804 | `azmcp_sql_db_list` | ❌ |
| 2 | 0.394721 | `azmcp_sql_db_show` | ❌ |
| 3 | 0.390061 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 4 | 0.385480 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.381817 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.349223 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 7 | 0.342283 | `azmcp_sql_elastic-pool_list` | ❌ |
| 8 | 0.339042 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.333202 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.329755 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.325654 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.304300 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.301512 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.297005 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 15 | 0.261121 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.257273 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.238503 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.232021 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.221262 | `azmcp_kusto_query` | ❌ |
| 20 | 0.219591 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678124 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.498367 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.478992 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.473539 | `azmcp_aks_nodepool_list` | ❌ |
| 6 | 0.454426 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.450591 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.442892 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.441264 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.434570 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.431184 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.429007 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 13 | 0.394548 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.394337 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.370652 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.363579 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.357347 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.352074 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.351647 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.350988 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 250

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606425 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502804 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.457163 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.438522 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.432816 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.429793 | `azmcp_aks_nodepool_get` | ❌ |
| 7 | 0.423047 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.419543 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.408202 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.400068 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.383940 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 12 | 0.378556 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.341705 | `azmcp_foundry_agents_list` | ❌ |
| 14 | 0.335596 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.333103 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.319864 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.317886 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.312361 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.304600 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.304317 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 251

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592709 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420325 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.402339 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.397670 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.397603 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.386833 | `azmcp_aks_nodepool_list` | ❌ |
| 7 | 0.378527 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.365129 | `azmcp_aks_nodepool_get` | ❌ |
| 9 | 0.357516 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.350723 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 11 | 0.344799 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.344495 | `azmcp_mysql_server_param_get` | ❌ |
| 13 | 0.342703 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.321802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.315659 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.298924 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.292566 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.284157 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.281680 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.259585 | `azmcp_appservice_database_add` | ❌ |

---

## Test 252

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682439 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.565607 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.537250 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.531550 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.483556 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.476225 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.466398 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.451169 | `azmcp_loadtesting_testresource_create` | ❌ |
| 9 | 0.450658 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.450320 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.418696 | `azmcp_sql_elastic-pool_list` | ❌ |
| 12 | 0.404854 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.401865 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.399175 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.356502 | `azmcp_appservice_database_add` | ❌ |
| 16 | 0.339127 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.333959 | `azmcp_extension_azqr` | ❌ |
| 18 | 0.327888 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.324092 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.321877 | `azmcp_acr_registry_list` | ❌ |

---

## Test 253

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618309 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510169 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.472403 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.435278 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.397805 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.396030 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.369890 | `azmcp_keyvault_secret_create` | ❌ |
| 8 | 0.368115 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.367996 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.360892 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.358285 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.354438 | `azmcp_sql_elastic-pool_list` | ❌ |
| 13 | 0.352234 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.349584 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 15 | 0.324021 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.316772 | `azmcp_loadtesting_test_create` | ❌ |
| 17 | 0.315987 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.314162 | `azmcp_foundry_agents_connect` | ❌ |
| 19 | 0.301132 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.300788 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 254

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590049 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.501477 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.497898 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.469124 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.443005 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.423936 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.421600 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.417664 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.416196 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.415395 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.389702 | `azmcp_sql_elastic-pool_list` | ❌ |
| 12 | 0.385209 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.333007 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.317484 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.312880 | `azmcp_loadtesting_test_create` | ❌ |
| 16 | 0.303411 | `azmcp_keyvault_certificate_create` | ❌ |
| 17 | 0.300811 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.298330 | `azmcp_group_list` | ❌ |
| 19 | 0.288337 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.284686 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 255

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701441 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.518036 | `azmcp_sql_server_list` | ❌ |
| 3 | 0.495550 | `azmcp_sql_server_create` | ❌ |
| 4 | 0.486260 | `azmcp_sql_db_delete` | ❌ |
| 5 | 0.484932 | `azmcp_workbooks_delete` | ❌ |
| 6 | 0.470205 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.448977 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.448506 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.438950 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.417035 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 11 | 0.346442 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.333269 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.323460 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.317588 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.317257 | `azmcp_group_list` | ❌ |
| 16 | 0.310672 | `azmcp_appservice_database_add` | ❌ |
| 17 | 0.307463 | `azmcp_appconfig_kv_delete` | ❌ |
| 18 | 0.290106 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.275707 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.273131 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 256

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.428294 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393885 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.376583 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.350103 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.309280 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 6 | 0.306368 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.301882 | `azmcp_sql_db_delete` | ❌ |
| 8 | 0.299760 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.295963 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.295073 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.274726 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.258333 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.235107 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.234792 | `azmcp_appconfig_kv_delete` | ❌ |
| 15 | 0.234376 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.226608 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.226581 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.225579 | `azmcp_grafana_list` | ❌ |
| 19 | 0.219702 | `azmcp_kusto_table_list` | ❌ |
| 20 | 0.210483 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 257

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528096 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.396518 | `azmcp_sql_db_delete` | ❌ |
| 3 | 0.362389 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.341477 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.316054 | `azmcp_workbooks_delete` | ❌ |
| 6 | 0.274818 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.262381 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 8 | 0.261689 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 9 | 0.254411 | `azmcp_appconfig_kv_delete` | ❌ |
| 10 | 0.247495 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.237815 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.213567 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.168042 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.159907 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.156253 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.148272 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.146243 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.142127 | `azmcp_kusto_query` | ❌ |
| 19 | 0.140290 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.125251 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 258

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783479 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.456057 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.434868 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.401908 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376055 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.365636 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.352607 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.344472 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.343559 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.329397 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.291733 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.280476 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.258095 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.249297 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.249153 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.246563 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.245267 | `azmcp_group_list` | ❌ |
| 18 | 0.238150 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.234703 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.233337 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 259

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713306 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.413113 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.368082 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.315966 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.311085 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.304891 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 7 | 0.303560 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.289764 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.287372 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.283806 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.273107 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.214536 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.205963 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.197679 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.194313 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.193050 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.191538 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.188120 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.182873 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.182237 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 260

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646419 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.356016 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.322155 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.307823 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.253610 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.236850 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.235961 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.234937 | `azmcp_appservice_database_add` | ❌ |
| 9 | 0.230580 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.227557 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.222855 | `azmcp_sql_db_update` | ❌ |
| 12 | 0.212644 | `azmcp_cloudarchitect_design` | ❌ |
| 13 | 0.210517 | `azmcp_foundry_agents_list` | ❌ |
| 14 | 0.200387 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.189303 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.188287 | `azmcp_deploy_plan_get` | ❌ |
| 17 | 0.180995 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.180656 | `azmcp_foundry_agents_connect` | ❌ |
| 19 | 0.180555 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.174553 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 261

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
| 6 | 0.423176 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.403351 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.397912 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.361266 | `azmcp_appservice_database_add` | ❌ |
| 10 | 0.335912 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.326420 | `azmcp_sql_db_update` | ❌ |
| 12 | 0.318368 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.311149 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.295941 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.290296 | `azmcp_deploy_iac_rules_get` | ❌ |
| 16 | 0.288030 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.265059 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 18 | 0.260209 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.253771 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.251941 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 262

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670189 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533562 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316619 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.294990 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.287457 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.284186 | `azmcp_appservice_database_add` | ❌ |
| 8 | 0.270600 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.252970 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 10 | 0.248588 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.237646 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.222068 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 13 | 0.179175 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.174851 | `azmcp_deploy_iac_rules_get` | ❌ |
| 15 | 0.174584 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.166723 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.158176 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.156396 | `azmcp_keyvault_key_create` | ❌ |
| 19 | 0.149884 | `azmcp_kusto_query` | ❌ |
| 20 | 0.146016 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 263

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685112 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574424 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539632 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428934 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.395197 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.356380 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.339339 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.321927 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.316841 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.302205 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.296480 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.283927 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.280749 | `azmcp_postgres_server_param_set` | ❌ |
| 14 | 0.270468 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 15 | 0.248924 | `azmcp_loadtesting_test_create` | ❌ |
| 16 | 0.220995 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.219192 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.209363 | `azmcp_loadtesting_testrun_create` | ❌ |
| 19 | 0.207284 | `azmcp_loadtesting_testresource_create` | ❌ |
| 20 | 0.197097 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 264

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.543857 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.540333 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.526736 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.436612 | `azmcp_sql_db_delete` | ❌ |
| 6 | 0.418454 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.412899 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.386562 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.341943 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.341915 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.312063 | `azmcp_appconfig_kv_delete` | ❌ |
| 12 | 0.306396 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.263959 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.247014 | `azmcp_keyvault_secret_get` | ❌ |
| 15 | 0.245270 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 16 | 0.241564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.235230 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.231494 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.225239 | `azmcp_keyvault_certificate_get` | ❌ |
| 20 | 0.225227 | `azmcp_kusto_query` | ❌ |

---

## Test 265

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670179 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574340 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.530419 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.397637 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.310415 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.298372 | `azmcp_sql_db_delete` | ❌ |
| 7 | 0.293097 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.260261 | `azmcp_workbooks_delete` | ❌ |
| 9 | 0.254942 | `azmcp_appconfig_kv_delete` | ❌ |
| 10 | 0.251005 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.231881 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.227837 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.182013 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.158025 | `azmcp_kusto_query` | ❌ |
| 15 | 0.156028 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.153635 | `azmcp_keyvault_secret_get` | ❌ |
| 17 | 0.152444 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.152084 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 19 | 0.149578 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.145688 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 266

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671212 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601230 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.577330 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.440487 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.367839 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.336472 | `azmcp_sql_db_delete` | ❌ |
| 7 | 0.332209 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.293303 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.291409 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 10 | 0.286469 | `azmcp_sql_db_update` | ❌ |
| 11 | 0.263966 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.252081 | `azmcp_appconfig_kv_delete` | ❌ |
| 13 | 0.222155 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.185578 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.185007 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.183545 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.181757 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 18 | 0.180404 | `azmcp_kusto_query` | ❌ |
| 19 | 0.179886 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.178831 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 267

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729372 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549667 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468753 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.418817 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.392512 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.385148 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.359228 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.356793 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.355203 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.304958 | `azmcp_keyvault_secret_list` | ❌ |
| 12 | 0.278095 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.277410 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.276828 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.274547 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.270667 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.263181 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.256310 | `azmcp_aks_nodepool_list` | ❌ |
| 19 | 0.253852 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.248780 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 268

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630731 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.524126 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410628 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.348100 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.316854 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.312035 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.299024 | `azmcp_mysql_server_param_get` | ❌ |
| 9 | 0.294466 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.293510 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.225372 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.217375 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.211187 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.210591 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.209533 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.206761 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.206476 | `azmcp_deploy_iac_rules_get` | ❌ |
| 18 | 0.206114 | `azmcp_kusto_table_list` | ❌ |
| 19 | 0.197711 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.195864 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 269

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630546 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532454 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473501 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412911 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.350513 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.308004 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.305738 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.304314 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.282538 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.277628 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.221706 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.216192 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.202425 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.200326 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.191165 | `azmcp_cloudarchitect_design` | ❌ |
| 16 | 0.185879 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.177454 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.176225 | `azmcp_get_bestpractices_get` | ❌ |
| 19 | 0.173184 | `azmcp_applens_resource_diagnose` | ❌ |
| 20 | 0.172371 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 270

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694404 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596577 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.578239 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.515851 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509789 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.499463 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.496897 | `azmcp_group_list` | ❌ |
| 8 | 0.496434 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.495321 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.487699 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.487431 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.473451 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.449346 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.449287 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.419283 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.403643 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.395549 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.384532 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.384389 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.380939 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 271

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618218 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593859 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.542398 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.507404 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.496200 | `azmcp_group_list` | ❌ |
| 6 | 0.495949 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.492324 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.484599 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.477041 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.470456 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.464018 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.449859 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.444259 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.419448 | `azmcp_foundry_agents_list` | ❌ |
| 15 | 0.417997 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.410302 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.397201 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.395064 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.391940 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.384337 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 272

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `azmcp_sql_db_show` | ❌ |
| 2 | 0.595146 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587728 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.559701 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.540218 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.491401 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.487582 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.481847 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.480067 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.478713 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.450140 | `azmcp_aks_cluster_get` | ❌ |
| 12 | 0.445602 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.437447 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.424878 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.410380 | `azmcp_group_list` | ❌ |
| 16 | 0.400396 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.394066 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.385318 | `azmcp_extension_azqr` | ❌ |
| 19 | 0.383563 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.373431 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 273

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658704 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.538034 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.471541 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.445453 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.444011 | `azmcp_mysql_server_param_get` | ❌ |
| 7 | 0.422646 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.414309 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.413964 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.406630 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.400827 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.359439 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.349963 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.316818 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.314864 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.308718 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.300098 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.298409 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.295903 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.284520 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 274

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563075 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.380056 | `azmcp_postgres_server_param_get` | ❌ |
| 4 | 0.372194 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370539 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.368788 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.367031 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.363268 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.361792 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.357960 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.288813 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.276308 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.271945 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.268920 | `azmcp_loadtesting_testrun_get` | ❌ |
| 15 | 0.257258 | `azmcp_appconfig_kv_list` | ❌ |
| 16 | 0.255800 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.253925 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.246261 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.244972 | `azmcp_keyvault_key_get` | ❌ |
| 20 | 0.242929 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 275

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533569 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.418472 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.394541 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.374006 | `azmcp_loadtesting_test_create` | ❌ |
| 5 | 0.355049 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.351846 | `azmcp_storage_blob_container_get` | ❌ |
| 7 | 0.325925 | `azmcp_keyvault_secret_create` | ❌ |
| 8 | 0.323501 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.319843 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.315241 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.311941 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.311275 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.305188 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.300188 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.297236 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.289742 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.286778 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.278047 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.277805 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.267557 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 276

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500655 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.400151 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.387071 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.382874 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.377221 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.376155 | `azmcp_storage_blob_container_create` | ❌ |
| 7 | 0.344343 | `azmcp_loadtesting_testresource_create` | ❌ |
| 8 | 0.340364 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.329099 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.324346 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.310931 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.310707 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.310279 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.296391 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.284467 | `azmcp_deploy_plan_get` | ❌ |
| 16 | 0.284385 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.283067 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 18 | 0.280404 | `azmcp_keyvault_certificate_create` | ❌ |
| 19 | 0.280192 | `azmcp_cloudarchitect_design` | ❌ |
| 20 | 0.278858 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 277

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589025 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.464611 | `azmcp_storage_blob_container_create` | ❌ |
| 3 | 0.447156 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.437040 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.407570 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.383927 | `azmcp_loadtesting_testresource_create` | ❌ |
| 7 | 0.383895 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.382334 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.380638 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.380503 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.372681 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.372357 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.366696 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.363391 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.360940 | `azmcp_storage_blob_upload` | ❌ |
| 16 | 0.359330 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.325337 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.324944 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.324674 | `azmcp_appservice_database_add` | ❌ |
| 20 | 0.321846 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 278

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655152 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.603870 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.507638 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.483400 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.442858 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.439236 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.431049 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.403478 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.397051 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.395698 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.388425 | `azmcp_aks_cluster_get` | ❌ |
| 12 | 0.373840 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.373119 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.370041 | `azmcp_keyvault_key_get` | ❌ |
| 15 | 0.368567 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.367173 | `azmcp_subscription_list` | ❌ |
| 17 | 0.367049 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.366645 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.362878 | `azmcp_search_index_get` | ❌ |
| 20 | 0.356970 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 279

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676876 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.612976 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.518191 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.515153 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.415410 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.411808 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.401846 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.379992 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.375790 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.375778 | `azmcp_keyvault_key_get` | ❌ |
| 11 | 0.373470 | `azmcp_aks_cluster_get` | ❌ |
| 12 | 0.369755 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.368207 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.368023 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.362607 | `azmcp_aks_nodepool_get` | ❌ |
| 16 | 0.362602 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.362371 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.355094 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.354842 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.351052 | `azmcp_functionapp_get` | ❌ |

---

## Test 280

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664087 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557016 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.536909 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.535622 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.501088 | `azmcp_subscription_list` | ❌ |
| 6 | 0.496371 | `azmcp_quota_region_availability_list` | ❌ |
| 7 | 0.493246 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.484383 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.484192 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.473387 | `azmcp_search_service_list` | ❌ |
| 11 | 0.458909 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.454195 | `azmcp_acr_registry_list` | ❌ |
| 13 | 0.447991 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.445545 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.441923 | `azmcp_redis_cluster_list` | ❌ |
| 16 | 0.439919 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.438660 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.432645 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.416387 | `azmcp_group_list` | ❌ |
| 20 | 0.412679 | `azmcp_grafana_list` | ❌ |

---

## Test 281

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499302 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.461307 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.455526 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.421642 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.379853 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.378256 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 7 | 0.375553 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.367899 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.366021 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.362227 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.360571 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.347173 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.346099 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.344771 | `azmcp_search_service_list` | ❌ |
| 15 | 0.342056 | `azmcp_subscription_list` | ❌ |
| 16 | 0.335306 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.330862 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.330377 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.322108 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.315492 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 282

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557142 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.473598 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.461741 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.453933 | `azmcp_subscription_list` | ❌ |
| 5 | 0.436170 | `azmcp_search_service_list` | ❌ |
| 6 | 0.432917 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.425048 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.418422 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.415843 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.415080 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.389930 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.382521 | `azmcp_aks_cluster_list` | ❌ |
| 13 | 0.379956 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.377201 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.376660 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.374635 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.371828 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.359996 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.359053 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.356591 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 283

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563396 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.524782 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.508014 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.447784 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.403407 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.335039 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.331449 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.326352 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.324867 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.323215 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.322464 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.318855 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.305680 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.297912 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 15 | 0.297384 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.292093 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.291191 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.281807 | `azmcp_sql_server_create` | ❌ |
| 19 | 0.280866 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.274669 | `azmcp_workbooks_create` | ❌ |

---

## Test 284

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512578 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.500636 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.470835 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415378 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.414820 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.368859 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.334040 | `azmcp_storage_blob_upload` | ❌ |
| 8 | 0.320173 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 9 | 0.309739 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.296967 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.296438 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.285153 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.278047 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.275240 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.275199 | `azmcp_keyvault_key_create` | ❌ |
| 16 | 0.270167 | `azmcp_appconfig_kv_set` | ❌ |
| 17 | 0.269625 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.268713 | `azmcp_workbooks_create` | ❌ |
| 19 | 0.256525 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.249658 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 285

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463236 | `azmcp_storage_account_create` | ❌ |
| 2 | 0.455331 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.451690 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 4 | 0.435099 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.388450 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.378021 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.366330 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.329038 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.322399 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.314128 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.309104 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.287818 | `azmcp_workbooks_create` | ❌ |
| 13 | 0.280806 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.277049 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.276586 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.275617 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.269719 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.266791 | `azmcp_appconfig_kv_set` | ❌ |
| 19 | 0.265228 | `azmcp_keyvault_key_create` | ❌ |
| 20 | 0.265216 | `azmcp_sql_server_create` | ❌ |

---

## Test 286

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665043 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.559177 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.523288 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.518763 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.496184 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.461556 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.421995 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.421220 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.384585 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.377009 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.367759 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.359218 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.354846 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.353561 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.350264 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.335739 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.334820 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.332134 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.327271 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.309006 | `azmcp_mysql_server_list` | ❌ |

---

## Test 287

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613933 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.605452 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 3 | 0.521995 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.479014 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.471385 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.453078 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.409820 | `azmcp_acr_registry_repository_list` | ❌ |
| 8 | 0.404594 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.393989 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.386170 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.367207 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.359465 | `azmcp_search_service_list` | ❌ |
| 13 | 0.359411 | `azmcp_subscription_list` | ❌ |
| 14 | 0.356400 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.353192 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.351601 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.351458 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.333677 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.333282 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.332129 | `azmcp_monitor_table_list` | ❌ |

---

## Test 288

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625048 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.511261 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.439651 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.437887 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.429767 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.418128 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.405700 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.390276 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.384096 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.355955 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.354374 | `azmcp_search_service_list` | ❌ |
| 13 | 0.352491 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.348138 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.347221 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.346936 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.345644 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.340651 | `azmcp_subscription_list` | ❌ |
| 19 | 0.336848 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.327093 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 289

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613091 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.586225 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.483614 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.477946 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.434667 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.420791 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.408497 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.386482 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.359392 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.349565 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.345511 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.337998 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.333887 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.330904 | `azmcp_storage_blob_upload` | ❌ |
| 15 | 0.326504 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.323067 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.321138 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.318346 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.303942 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.303596 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 290

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662151 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.661919 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.537535 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.460657 | `azmcp_storage_blob_container_create` | ❌ |
| 5 | 0.457018 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.453696 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.370236 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.360712 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.359655 | `azmcp_aks_cluster_get` | ❌ |
| 10 | 0.358376 | `azmcp_storage_blob_upload` | ❌ |
| 11 | 0.353461 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.353131 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.352646 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.348551 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.348387 | `azmcp_keyvault_key_get` | ❌ |
| 16 | 0.342979 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.337010 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.334138 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.329754 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.319610 | `azmcp_keyvault_certificate_get` | ❌ |

---

## Test 291

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592709 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.579070 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.568421 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 4 | 0.465942 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.452160 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.415890 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.413280 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.400483 | `azmcp_acr_registry_repository_list` | ❌ |
| 9 | 0.394810 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.379851 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.379099 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.369535 | `azmcp_keyvault_secret_list` | ❌ |
| 13 | 0.361706 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.359099 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.348821 | `azmcp_subscription_list` | ❌ |
| 16 | 0.340190 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.331545 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.328193 | `azmcp_search_service_list` | ❌ |
| 19 | 0.313259 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.310876 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 292

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570343 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.549442 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.533515 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.449128 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.433883 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.397323 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.395809 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.385242 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.362358 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.353815 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.345263 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.342766 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.339846 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.336142 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.314069 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.308890 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.306951 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.300295 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.299112 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.294761 | `azmcp_subscription_list` | ❌ |

---

## Test 293

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566287 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.403451 | `azmcp_storage_blob_get` | ❌ |
| 3 | 0.397780 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.382129 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.377255 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.351920 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.327416 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.324049 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.294744 | `azmcp_keyvault_certificate_import` | ❌ |
| 10 | 0.284896 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.284377 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.273638 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 13 | 0.273601 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.272315 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 15 | 0.257845 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.253430 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.253047 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.239522 | `azmcp_foundry_models_deploy` | ❌ |
| 19 | 0.211067 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.210171 | `azmcp_quota_usage_check` | ❌ |

---

## Test 294

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576055 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.473852 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471653 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.465428 | `azmcp_eventgrid_subscription_list` | ❌ |
| 6 | 0.452471 | `azmcp_search_service_list` | ❌ |
| 7 | 0.450979 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.445724 | `azmcp_grafana_list` | ❌ |
| 9 | 0.431337 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.430280 | `azmcp_group_list` | ❌ |
| 11 | 0.422803 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.406935 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.394965 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.388987 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.380636 | `azmcp_marketplace_product_list` | ❌ |
| 16 | 0.367761 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.366860 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.355414 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.354245 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.348524 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 295

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405624 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.381203 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.380724 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.351806 | `azmcp_grafana_list` | ❌ |
| 5 | 0.350890 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.341801 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.334757 | `azmcp_eventgrid_topic_list` | ❌ |
| 8 | 0.328033 | `azmcp_search_service_list` | ❌ |
| 9 | 0.315528 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.308802 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.303459 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.303290 | `azmcp_marketplace_product_list` | ❌ |
| 13 | 0.297150 | `azmcp_group_list` | ❌ |
| 14 | 0.296414 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.295154 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.285343 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.275346 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.274804 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.269862 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.257983 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 296

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.319958 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.315603 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.307697 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.286711 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.282645 | `azmcp_grafana_list` | ❌ |
| 6 | 0.279707 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.278798 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.273758 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.256358 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.254815 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.252965 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.252504 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.251683 | `azmcp_search_service_list` | ❌ |
| 14 | 0.251368 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.233292 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.230571 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.230324 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.226463 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.222799 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.218677 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 297

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.403171 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.370697 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.354480 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.342345 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.340306 | `azmcp_grafana_list` | ❌ |
| 6 | 0.336783 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.311956 | `azmcp_search_service_list` | ❌ |
| 8 | 0.311136 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.305260 | `azmcp_marketplace_product_get` | ❌ |
| 10 | 0.304930 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.302336 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.300488 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.294220 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.291807 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.282349 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.281298 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.274279 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.269841 | `azmcp_group_list` | ❌ |
| 19 | 0.258429 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.233332 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 298

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625270 | `azmcp_deploy_iac_rules_get` | ❌ |
| 3 | 0.605047 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.482936 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466199 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.431102 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.388685 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.386480 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.372596 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369184 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.362323 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.354086 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.339172 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.333210 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.312592 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.310275 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.305259 | `azmcp_mysql_database_query` | ❌ |
| 18 | 0.303849 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.302307 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.301777 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 299

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.523892 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.512141 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.510004 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.474447 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.444297 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 7 | 0.439871 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.439536 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.428886 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.389450 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.304912 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.304137 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.300776 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.292971 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.285517 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.281261 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.279067 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.278638 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.277852 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.274538 | `azmcp_subscription_list` | ❌ |

---

## Test 300

**Expected Tool:** `azmcp_virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711969 | `azmcp_virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659763 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.566615 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.548888 | `azmcp_search_service_list` | ❌ |
| 5 | 0.536547 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.535739 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 7 | 0.527948 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.527095 | `azmcp_aks_nodepool_list` | ❌ |
| 9 | 0.525813 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.525637 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.506608 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.505116 | `azmcp_subscription_list` | ❌ |
| 13 | 0.496297 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.495490 | `azmcp_grafana_list` | ❌ |
| 15 | 0.492717 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.476718 | `azmcp_group_list` | ❌ |
| 17 | 0.465569 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.463069 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.460388 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.459250 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 301

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714469 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.439611 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.402909 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.393721 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.364696 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.362307 | `azmcp_search_service_list` | ❌ |
| 9 | 0.359383 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.344920 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.337549 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.335493 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.333517 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.332928 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.330910 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.328623 | `azmcp_keyvault_key_list` | ❌ |
| 17 | 0.324603 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.312156 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.311262 | `azmcp_grafana_list` | ❌ |
| 20 | 0.308204 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 302

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812659 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.659212 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501167 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.356479 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.336586 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.327423 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.324603 | `azmcp_subscription_list` | ❌ |
| 8 | 0.324289 | `azmcp_search_service_list` | ❌ |
| 9 | 0.316295 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.315778 | `azmcp_loadtesting_testrun_list` | ❌ |
| 11 | 0.307927 | `azmcp_aks_nodepool_get` | ❌ |
| 12 | 0.305172 | `azmcp_aks_cluster_list` | ❌ |
| 13 | 0.304835 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.304414 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.300364 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.299973 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.297260 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.295899 | `azmcp_grafana_list` | ❌ |
| 19 | 0.284943 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.278813 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 303

**Expected Tool:** `azmcp_workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552307 | `azmcp_workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.433162 | `azmcp_workbooks_update` | ❌ |
| 3 | 0.361215 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.358977 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.328113 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.239813 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.217264 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.214818 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.188137 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.173163 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.169440 | `azmcp_grafana_list` | ❌ |
| 12 | 0.164006 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.153966 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.148897 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.147322 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.143713 | `azmcp_sql_server_create` | ❌ |
| 17 | 0.130524 | `azmcp_loadtesting_testrun_create` | ❌ |
| 18 | 0.130339 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.113882 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.111941 | `azmcp_extension_azqr` | ❌ |

---

## Test 304

**Expected Tool:** `azmcp_workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620502 | `azmcp_workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.518630 | `azmcp_workbooks_show` | ❌ |
| 3 | 0.432643 | `azmcp_workbooks_create` | ❌ |
| 4 | 0.425569 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.390355 | `azmcp_workbooks_update` | ❌ |
| 6 | 0.273939 | `azmcp_grafana_list` | ❌ |
| 7 | 0.256795 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 8 | 0.248122 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.241492 | `azmcp_sql_server_delete` | ❌ |
| 10 | 0.198568 | `azmcp_appconfig_kv_delete` | ❌ |
| 11 | 0.190455 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.186665 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.148882 | `azmcp_extension_azqr` | ❌ |
| 14 | 0.145141 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.132504 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.131813 | `azmcp_group_list` | ❌ |
| 17 | 0.122450 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.119436 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.114235 | `azmcp_foundry_agents_connect` | ❌ |
| 20 | 0.109986 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 305

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772431 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562794 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.532565 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.516739 | `azmcp_grafana_list` | ❌ |
| 5 | 0.488600 | `azmcp_group_list` | ❌ |
| 6 | 0.487451 | `azmcp_workbooks_delete` | ❌ |
| 7 | 0.459976 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.454247 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.439945 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.428786 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.416430 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.413409 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.405963 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.405949 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.399758 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.365289 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.362740 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.356739 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.352905 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.349674 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 306

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570521 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.539957 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.486336 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.472378 | `azmcp_grafana_list` | ❌ |
| 6 | 0.428066 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.425426 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.422785 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421646 | `azmcp_group_list` | ❌ |
| 10 | 0.412376 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.392371 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.380989 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.380399 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.371194 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.363744 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.350839 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.338334 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.337786 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.334580 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.333154 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 307

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.697539 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498518 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.494708 | `azmcp_workbooks_list` | ❌ |
| 4 | 0.451314 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.419105 | `azmcp_workbooks_update` | ❌ |
| 6 | 0.353546 | `azmcp_grafana_list` | ❌ |
| 7 | 0.277807 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.264692 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.256678 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.250024 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.236741 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.225294 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.218999 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.207693 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.197239 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.195373 | `azmcp_group_list` | ❌ |
| 17 | 0.189900 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.189657 | `azmcp_extension_azqr` | ❌ |
| 19 | 0.187682 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.187527 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 308

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Show me the workbook with display name <workbook_display_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469486 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.455544 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.437743 | `azmcp_workbooks_update` | ❌ |
| 4 | 0.424343 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.366136 | `azmcp_workbooks_delete` | ❌ |
| 6 | 0.292847 | `azmcp_grafana_list` | ❌ |
| 7 | 0.266656 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.240026 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.227346 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.176377 | `azmcp_role_assignment_list` | ❌ |
| 11 | 0.175766 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.174480 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.168123 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.165633 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.154663 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.152528 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.149545 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.145983 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.141936 | `azmcp_loadtesting_testrun_get` | ❌ |
| 20 | 0.141546 | `azmcp_foundry_models_list` | ❌ |

---

## Test 309

**Expected Tool:** `azmcp_workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469915 | `azmcp_workbooks_update` | ✅ **EXPECTED** |
| 2 | 0.382724 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.362354 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.348537 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.276750 | `azmcp_loadtesting_testrun_update` | ❌ |
| 6 | 0.262873 | `azmcp_workbooks_list` | ❌ |
| 7 | 0.174324 | `azmcp_sql_db_update` | ❌ |
| 8 | 0.170118 | `azmcp_grafana_list` | ❌ |
| 9 | 0.148730 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.142404 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.142186 | `azmcp_loadtesting_testrun_create` | ❌ |
| 12 | 0.138354 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.136105 | `azmcp_loadtesting_testresource_create` | ❌ |
| 14 | 0.132978 | `azmcp_foundry_agents_evaluate` | ❌ |
| 15 | 0.130993 | `azmcp_postgres_database_query` | ❌ |
| 16 | 0.130054 | `azmcp_postgres_server_param_set` | ❌ |
| 17 | 0.129660 | `azmcp_deploy_iac_rules_get` | ❌ |
| 18 | 0.116768 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.113470 | `azmcp_foundry_agents_connect` | ❌ |
| 20 | 0.111099 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 310

**Expected Tool:** `azmcp_bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485889 | `azmcp_deploy_iac_rules_get` | ❌ |
| 2 | 0.448373 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.440302 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.432773 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.432518 | `azmcp_bicepschema_get` | ✅ **EXPECTED** |
| 6 | 0.400985 | `azmcp_foundry_models_deploy` | ❌ |
| 7 | 0.398521 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.393522 | `azmcp_foundry_agents_connect` | ❌ |
| 9 | 0.391625 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 10 | 0.385512 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.372097 | `azmcp_search_service_list` | ❌ |
| 12 | 0.325716 | `azmcp_search_index_query` | ❌ |
| 13 | 0.324619 | `azmcp_search_index_get` | ❌ |
| 14 | 0.317232 | `azmcp_sql_db_create` | ❌ |
| 15 | 0.303183 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.291360 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.281780 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.281252 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.274770 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.270531 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 311

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.349500 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290897 | `azmcp_storage_blob_upload` | ❌ |
| 3 | 0.255040 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.221495 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.217809 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.216246 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 7 | 0.191410 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.191171 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.178356 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.175963 | `azmcp_deploy_iac_rules_get` | ❌ |
| 11 | 0.136766 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.135915 | `azmcp_get_bestpractices_get` | ❌ |
| 13 | 0.132708 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.130104 | `azmcp_foundry_models_deploy` | ❌ |
| 15 | 0.118536 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.116060 | `azmcp_marketplace_product_get` | ❌ |
| 17 | 0.111702 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.106829 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.104184 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.101040 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 312

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.290270 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.267260 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258160 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.225622 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.215748 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.207352 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.195344 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.189220 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.179120 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.170143 | `azmcp_foundry_agents_connect` | ❌ |
| 11 | 0.168850 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.163694 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.163615 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.162137 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.160743 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.154492 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.152324 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.145124 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.139758 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.135818 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 313

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.299640 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265632 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242581 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.218064 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.213173 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.179199 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.169729 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.164337 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.156442 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.156119 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.151368 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.142854 | `azmcp_marketplace_product_list` | ❌ |
| 14 | 0.139970 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.138083 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.132355 | `azmcp_mysql_database_query` | ❌ |
| 17 | 0.130132 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.123936 | `azmcp_storage_blob_upload` | ❌ |
| 19 | 0.119666 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.114994 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 314

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.420259 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.352914 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.323920 | `azmcp_storage_blob_upload` | ❌ |
| 5 | 0.310615 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.306935 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.304209 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.300392 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.299412 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.298989 | `azmcp_get_bestpractices_get` | ❌ |
| 11 | 0.293871 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.292455 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.291879 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.282407 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.275832 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.275550 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.272671 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.261446 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.259814 | `azmcp_search_service_list` | ❌ |
| 20 | 0.258445 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Summary

**Total Prompts Tested:** 314  
**Execution Time:** 56.9314869s  

### Success Rate Metrics

**Top Choice Success:** 81.5% (256/314 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 4.5% (14/314 tests)  
**🎯 High Confidence (≥0.7):** 19.4% (61/314 tests)  
**✅ Good Confidence (≥0.6):** 57.0% (179/314 tests)  
**👍 Fair Confidence (≥0.5):** 82.8% (260/314 tests)  
**👌 Acceptable Confidence (≥0.4):** 92.4% (290/314 tests)  
**❌ Low Confidence (<0.4):** 7.6% (24/314 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 4.5% (14/314 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 19.4% (61/314 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 54.1% (170/314 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 73.9% (232/314 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 78.7% (247/314 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing adequately but has room for improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

