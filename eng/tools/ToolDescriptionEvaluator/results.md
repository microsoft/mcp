# Tool Selection Analysis Setup

**Setup completed:** 2025-09-23 15:49:56  
**Tool count:** 144  
**Database setup time:** 11.1280869s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-23 15:49:57  
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
- [Test 20: azmcp_speech_stt_recognize](#test-20)
- [Test 21: azmcp_speech_stt_recognize](#test-21)
- [Test 22: azmcp_speech_stt_recognize](#test-22)
- [Test 23: azmcp_speech_stt_recognize](#test-23)
- [Test 24: azmcp_speech_stt_recognize](#test-24)
- [Test 25: azmcp_speech_stt_recognize](#test-25)
- [Test 26: azmcp_speech_stt_recognize](#test-26)
- [Test 27: azmcp_speech_stt_recognize](#test-27)
- [Test 28: azmcp_speech_stt_recognize](#test-28)
- [Test 29: azmcp_speech_stt_recognize](#test-29)
- [Test 30: azmcp_appconfig_account_list](#test-30)
- [Test 31: azmcp_appconfig_account_list](#test-31)
- [Test 32: azmcp_appconfig_account_list](#test-32)
- [Test 33: azmcp_appconfig_kv_delete](#test-33)
- [Test 34: azmcp_appconfig_kv_list](#test-34)
- [Test 35: azmcp_appconfig_kv_list](#test-35)
- [Test 36: azmcp_appconfig_kv_lock_set](#test-36)
- [Test 37: azmcp_appconfig_kv_lock_set](#test-37)
- [Test 38: azmcp_appconfig_kv_set](#test-38)
- [Test 39: azmcp_appconfig_kv_show](#test-39)
- [Test 40: azmcp_applens_resource_diagnose](#test-40)
- [Test 41: azmcp_applens_resource_diagnose](#test-41)
- [Test 42: azmcp_applens_resource_diagnose](#test-42)
- [Test 43: azmcp_appservice_database_add](#test-43)
- [Test 44: azmcp_appservice_database_add](#test-44)
- [Test 45: azmcp_appservice_database_add](#test-45)
- [Test 46: azmcp_appservice_database_add](#test-46)
- [Test 47: azmcp_appservice_database_add](#test-47)
- [Test 48: azmcp_appservice_database_add](#test-48)
- [Test 49: azmcp_appservice_database_add](#test-49)
- [Test 50: azmcp_appservice_database_add](#test-50)
- [Test 51: azmcp_appservice_database_add](#test-51)
- [Test 52: azmcp_applicationinsights_recommendation_list](#test-52)
- [Test 53: azmcp_applicationinsights_recommendation_list](#test-53)
- [Test 54: azmcp_applicationinsights_recommendation_list](#test-54)
- [Test 55: azmcp_applicationinsights_recommendation_list](#test-55)
- [Test 56: azmcp_acr_registry_list](#test-56)
- [Test 57: azmcp_acr_registry_list](#test-57)
- [Test 58: azmcp_acr_registry_list](#test-58)
- [Test 59: azmcp_acr_registry_list](#test-59)
- [Test 60: azmcp_acr_registry_list](#test-60)
- [Test 61: azmcp_acr_registry_repository_list](#test-61)
- [Test 62: azmcp_acr_registry_repository_list](#test-62)
- [Test 63: azmcp_acr_registry_repository_list](#test-63)
- [Test 64: azmcp_acr_registry_repository_list](#test-64)
- [Test 65: azmcp_cosmos_account_list](#test-65)
- [Test 66: azmcp_cosmos_account_list](#test-66)
- [Test 67: azmcp_cosmos_account_list](#test-67)
- [Test 68: azmcp_cosmos_database_container_item_query](#test-68)
- [Test 69: azmcp_cosmos_database_container_list](#test-69)
- [Test 70: azmcp_cosmos_database_container_list](#test-70)
- [Test 71: azmcp_cosmos_database_list](#test-71)
- [Test 72: azmcp_cosmos_database_list](#test-72)
- [Test 73: azmcp_kusto_cluster_get](#test-73)
- [Test 74: azmcp_kusto_cluster_list](#test-74)
- [Test 75: azmcp_kusto_cluster_list](#test-75)
- [Test 76: azmcp_kusto_cluster_list](#test-76)
- [Test 77: azmcp_kusto_database_list](#test-77)
- [Test 78: azmcp_kusto_database_list](#test-78)
- [Test 79: azmcp_kusto_query](#test-79)
- [Test 80: azmcp_kusto_sample](#test-80)
- [Test 81: azmcp_kusto_table_list](#test-81)
- [Test 82: azmcp_kusto_table_list](#test-82)
- [Test 83: azmcp_kusto_table_schema](#test-83)
- [Test 84: azmcp_mysql_database_list](#test-84)
- [Test 85: azmcp_mysql_database_list](#test-85)
- [Test 86: azmcp_mysql_database_query](#test-86)
- [Test 87: azmcp_mysql_server_config_get](#test-87)
- [Test 88: azmcp_mysql_server_list](#test-88)
- [Test 89: azmcp_mysql_server_list](#test-89)
- [Test 90: azmcp_mysql_server_list](#test-90)
- [Test 91: azmcp_mysql_server_param_get](#test-91)
- [Test 92: azmcp_mysql_server_param_set](#test-92)
- [Test 93: azmcp_mysql_table_list](#test-93)
- [Test 94: azmcp_mysql_table_list](#test-94)
- [Test 95: azmcp_mysql_table_schema_get](#test-95)
- [Test 96: azmcp_postgres_database_list](#test-96)
- [Test 97: azmcp_postgres_database_list](#test-97)
- [Test 98: azmcp_postgres_database_query](#test-98)
- [Test 99: azmcp_postgres_server_config_get](#test-99)
- [Test 100: azmcp_postgres_server_list](#test-100)
- [Test 101: azmcp_postgres_server_list](#test-101)
- [Test 102: azmcp_postgres_server_list](#test-102)
- [Test 103: azmcp_postgres_server_param](#test-103)
- [Test 104: azmcp_postgres_server_param_set](#test-104)
- [Test 105: azmcp_postgres_table_list](#test-105)
- [Test 106: azmcp_postgres_table_list](#test-106)
- [Test 107: azmcp_postgres_table_schema_get](#test-107)
- [Test 108: azmcp_deploy_app_logs_get](#test-108)
- [Test 109: azmcp_deploy_architecture_diagram_generate](#test-109)
- [Test 110: azmcp_deploy_iac_rules_get](#test-110)
- [Test 111: azmcp_deploy_pipeline_guidance_get](#test-111)
- [Test 112: azmcp_deploy_plan_get](#test-112)
- [Test 113: azmcp_eventgrid_topic_list](#test-113)
- [Test 114: azmcp_eventgrid_topic_list](#test-114)
- [Test 115: azmcp_eventgrid_topic_list](#test-115)
- [Test 116: azmcp_eventgrid_topic_list](#test-116)
- [Test 117: azmcp_eventgrid_subscription_list](#test-117)
- [Test 118: azmcp_eventgrid_subscription_list](#test-118)
- [Test 119: azmcp_eventgrid_subscription_list](#test-119)
- [Test 120: azmcp_eventgrid_subscription_list](#test-120)
- [Test 121: azmcp_eventgrid_subscription_list](#test-121)
- [Test 122: azmcp_eventgrid_subscription_list](#test-122)
- [Test 123: azmcp_eventgrid_subscription_list](#test-123)
- [Test 124: azmcp_functionapp_get](#test-124)
- [Test 125: azmcp_functionapp_get](#test-125)
- [Test 126: azmcp_functionapp_get](#test-126)
- [Test 127: azmcp_functionapp_get](#test-127)
- [Test 128: azmcp_functionapp_get](#test-128)
- [Test 129: azmcp_functionapp_get](#test-129)
- [Test 130: azmcp_functionapp_get](#test-130)
- [Test 131: azmcp_functionapp_get](#test-131)
- [Test 132: azmcp_functionapp_get](#test-132)
- [Test 133: azmcp_functionapp_get](#test-133)
- [Test 134: azmcp_functionapp_get](#test-134)
- [Test 135: azmcp_functionapp_get](#test-135)
- [Test 136: azmcp_keyvault_certificate_create](#test-136)
- [Test 137: azmcp_keyvault_certificate_get](#test-137)
- [Test 138: azmcp_keyvault_certificate_get](#test-138)
- [Test 139: azmcp_keyvault_certificate_import](#test-139)
- [Test 140: azmcp_keyvault_certificate_import](#test-140)
- [Test 141: azmcp_keyvault_certificate_list](#test-141)
- [Test 142: azmcp_keyvault_certificate_list](#test-142)
- [Test 143: azmcp_keyvault_key_create](#test-143)
- [Test 144: azmcp_keyvault_key_get](#test-144)
- [Test 145: azmcp_keyvault_key_get](#test-145)
- [Test 146: azmcp_keyvault_key_list](#test-146)
- [Test 147: azmcp_keyvault_key_list](#test-147)
- [Test 148: azmcp_keyvault_secret_create](#test-148)
- [Test 149: azmcp_keyvault_secret_get](#test-149)
- [Test 150: azmcp_keyvault_secret_get](#test-150)
- [Test 151: azmcp_keyvault_secret_list](#test-151)
- [Test 152: azmcp_keyvault_secret_list](#test-152)
- [Test 153: azmcp_aks_cluster_get](#test-153)
- [Test 154: azmcp_aks_cluster_get](#test-154)
- [Test 155: azmcp_aks_cluster_get](#test-155)
- [Test 156: azmcp_aks_cluster_get](#test-156)
- [Test 157: azmcp_aks_cluster_list](#test-157)
- [Test 158: azmcp_aks_cluster_list](#test-158)
- [Test 159: azmcp_aks_cluster_list](#test-159)
- [Test 160: azmcp_aks_nodepool_get](#test-160)
- [Test 161: azmcp_aks_nodepool_get](#test-161)
- [Test 162: azmcp_aks_nodepool_get](#test-162)
- [Test 163: azmcp_aks_nodepool_list](#test-163)
- [Test 164: azmcp_aks_nodepool_list](#test-164)
- [Test 165: azmcp_aks_nodepool_list](#test-165)
- [Test 166: azmcp_loadtesting_test_create](#test-166)
- [Test 167: azmcp_loadtesting_test_get](#test-167)
- [Test 168: azmcp_loadtesting_testresource_create](#test-168)
- [Test 169: azmcp_loadtesting_testresource_list](#test-169)
- [Test 170: azmcp_loadtesting_testrun_create](#test-170)
- [Test 171: azmcp_loadtesting_testrun_get](#test-171)
- [Test 172: azmcp_loadtesting_testrun_list](#test-172)
- [Test 173: azmcp_loadtesting_testrun_update](#test-173)
- [Test 174: azmcp_grafana_list](#test-174)
- [Test 175: azmcp_azuremanagedlustre_filesystem_list](#test-175)
- [Test 176: azmcp_azuremanagedlustre_filesystem_list](#test-176)
- [Test 177: azmcp_azuremanagedlustre_filesystem_required-subnet-size](#test-177)
- [Test 178: azmcp_azuremanagedlustre_filesystem_sku_get](#test-178)
- [Test 179: azmcp_marketplace_product_get](#test-179)
- [Test 180: azmcp_marketplace_product_list](#test-180)
- [Test 181: azmcp_marketplace_product_list](#test-181)
- [Test 182: azmcp_bestpractices_get](#test-182)
- [Test 183: azmcp_bestpractices_get](#test-183)
- [Test 184: azmcp_bestpractices_get](#test-184)
- [Test 185: azmcp_bestpractices_get](#test-185)
- [Test 186: azmcp_bestpractices_get](#test-186)
- [Test 187: azmcp_bestpractices_get](#test-187)
- [Test 188: azmcp_bestpractices_get](#test-188)
- [Test 189: azmcp_bestpractices_get](#test-189)
- [Test 190: azmcp_bestpractices_get](#test-190)
- [Test 191: azmcp_bestpractices_get](#test-191)
- [Test 192: azmcp_monitor_healthmodels_entity_gethealth](#test-192)
- [Test 193: azmcp_monitor_metrics_definitions](#test-193)
- [Test 194: azmcp_monitor_metrics_definitions](#test-194)
- [Test 195: azmcp_monitor_metrics_definitions](#test-195)
- [Test 196: azmcp_monitor_metrics_query](#test-196)
- [Test 197: azmcp_monitor_metrics_query](#test-197)
- [Test 198: azmcp_monitor_metrics_query](#test-198)
- [Test 199: azmcp_monitor_metrics_query](#test-199)
- [Test 200: azmcp_monitor_metrics_query](#test-200)
- [Test 201: azmcp_monitor_metrics_query](#test-201)
- [Test 202: azmcp_monitor_resource_log_query](#test-202)
- [Test 203: azmcp_monitor_table_list](#test-203)
- [Test 204: azmcp_monitor_table_list](#test-204)
- [Test 205: azmcp_monitor_table_type_list](#test-205)
- [Test 206: azmcp_monitor_table_type_list](#test-206)
- [Test 207: azmcp_monitor_workspace_list](#test-207)
- [Test 208: azmcp_monitor_workspace_list](#test-208)
- [Test 209: azmcp_monitor_workspace_list](#test-209)
- [Test 210: azmcp_monitor_workspace_log_query](#test-210)
- [Test 211: azmcp_datadog_monitoredresources_list](#test-211)
- [Test 212: azmcp_datadog_monitoredresources_list](#test-212)
- [Test 213: azmcp_extension_azqr](#test-213)
- [Test 214: azmcp_extension_azqr](#test-214)
- [Test 215: azmcp_extension_azqr](#test-215)
- [Test 216: azmcp_quota_region_availability_list](#test-216)
- [Test 217: azmcp_quota_usage_check](#test-217)
- [Test 218: azmcp_role_assignment_list](#test-218)
- [Test 219: azmcp_role_assignment_list](#test-219)
- [Test 220: azmcp_redis_cache_accesspolicy_list](#test-220)
- [Test 221: azmcp_redis_cache_accesspolicy_list](#test-221)
- [Test 222: azmcp_redis_cache_list](#test-222)
- [Test 223: azmcp_redis_cache_list](#test-223)
- [Test 224: azmcp_redis_cache_list](#test-224)
- [Test 225: azmcp_redis_cluster_database_list](#test-225)
- [Test 226: azmcp_redis_cluster_database_list](#test-226)
- [Test 227: azmcp_redis_cluster_list](#test-227)
- [Test 228: azmcp_redis_cluster_list](#test-228)
- [Test 229: azmcp_redis_cluster_list](#test-229)
- [Test 230: azmcp_group_list](#test-230)
- [Test 231: azmcp_group_list](#test-231)
- [Test 232: azmcp_group_list](#test-232)
- [Test 233: azmcp_resourcehealth_availability-status_get](#test-233)
- [Test 234: azmcp_resourcehealth_availability-status_get](#test-234)
- [Test 235: azmcp_resourcehealth_availability-status_get](#test-235)
- [Test 236: azmcp_resourcehealth_availability-status_list](#test-236)
- [Test 237: azmcp_resourcehealth_availability-status_list](#test-237)
- [Test 238: azmcp_resourcehealth_availability-status_list](#test-238)
- [Test 239: azmcp_resourcehealth_service-health-events_list](#test-239)
- [Test 240: azmcp_resourcehealth_service-health-events_list](#test-240)
- [Test 241: azmcp_resourcehealth_service-health-events_list](#test-241)
- [Test 242: azmcp_resourcehealth_service-health-events_list](#test-242)
- [Test 243: azmcp_resourcehealth_service-health-events_list](#test-243)
- [Test 244: azmcp_servicebus_queue_details](#test-244)
- [Test 245: azmcp_servicebus_topic_details](#test-245)
- [Test 246: azmcp_servicebus_topic_subscription_details](#test-246)
- [Test 247: azmcp_sql_db_create](#test-247)
- [Test 248: azmcp_sql_db_create](#test-248)
- [Test 249: azmcp_sql_db_create](#test-249)
- [Test 250: azmcp_sql_db_delete](#test-250)
- [Test 251: azmcp_sql_db_delete](#test-251)
- [Test 252: azmcp_sql_db_delete](#test-252)
- [Test 253: azmcp_sql_db_list](#test-253)
- [Test 254: azmcp_sql_db_list](#test-254)
- [Test 255: azmcp_sql_db_show](#test-255)
- [Test 256: azmcp_sql_db_show](#test-256)
- [Test 257: azmcp_sql_db_update](#test-257)
- [Test 258: azmcp_sql_db_update](#test-258)
- [Test 259: azmcp_sql_elastic-pool_list](#test-259)
- [Test 260: azmcp_sql_elastic-pool_list](#test-260)
- [Test 261: azmcp_sql_elastic-pool_list](#test-261)
- [Test 262: azmcp_sql_server_create](#test-262)
- [Test 263: azmcp_sql_server_create](#test-263)
- [Test 264: azmcp_sql_server_create](#test-264)
- [Test 265: azmcp_sql_server_delete](#test-265)
- [Test 266: azmcp_sql_server_delete](#test-266)
- [Test 267: azmcp_sql_server_delete](#test-267)
- [Test 268: azmcp_sql_server_entra-admin_list](#test-268)
- [Test 269: azmcp_sql_server_entra-admin_list](#test-269)
- [Test 270: azmcp_sql_server_entra-admin_list](#test-270)
- [Test 271: azmcp_sql_server_firewall-rule_create](#test-271)
- [Test 272: azmcp_sql_server_firewall-rule_create](#test-272)
- [Test 273: azmcp_sql_server_firewall-rule_create](#test-273)
- [Test 274: azmcp_sql_server_firewall-rule_delete](#test-274)
- [Test 275: azmcp_sql_server_firewall-rule_delete](#test-275)
- [Test 276: azmcp_sql_server_firewall-rule_delete](#test-276)
- [Test 277: azmcp_sql_server_firewall-rule_list](#test-277)
- [Test 278: azmcp_sql_server_firewall-rule_list](#test-278)
- [Test 279: azmcp_sql_server_firewall-rule_list](#test-279)
- [Test 280: azmcp_sql_server_list](#test-280)
- [Test 281: azmcp_sql_server_list](#test-281)
- [Test 282: azmcp_sql_server_show](#test-282)
- [Test 283: azmcp_sql_server_show](#test-283)
- [Test 284: azmcp_sql_server_show](#test-284)
- [Test 285: azmcp_storage_account_create](#test-285)
- [Test 286: azmcp_storage_account_create](#test-286)
- [Test 287: azmcp_storage_account_create](#test-287)
- [Test 288: azmcp_storage_account_get](#test-288)
- [Test 289: azmcp_storage_account_get](#test-289)
- [Test 290: azmcp_storage_account_get](#test-290)
- [Test 291: azmcp_storage_account_get](#test-291)
- [Test 292: azmcp_storage_account_get](#test-292)
- [Test 293: azmcp_storage_blob_container_create](#test-293)
- [Test 294: azmcp_storage_blob_container_create](#test-294)
- [Test 295: azmcp_storage_blob_container_create](#test-295)
- [Test 296: azmcp_storage_blob_container_get](#test-296)
- [Test 297: azmcp_storage_blob_container_get](#test-297)
- [Test 298: azmcp_storage_blob_container_get](#test-298)
- [Test 299: azmcp_storage_blob_get](#test-299)
- [Test 300: azmcp_storage_blob_get](#test-300)
- [Test 301: azmcp_storage_blob_get](#test-301)
- [Test 302: azmcp_storage_blob_get](#test-302)
- [Test 303: azmcp_storage_blob_upload](#test-303)
- [Test 304: azmcp_subscription_list](#test-304)
- [Test 305: azmcp_subscription_list](#test-305)
- [Test 306: azmcp_subscription_list](#test-306)
- [Test 307: azmcp_subscription_list](#test-307)
- [Test 308: azmcp_azureterraformbestpractices_get](#test-308)
- [Test 309: azmcp_azureterraformbestpractices_get](#test-309)
- [Test 310: azmcp_virtualdesktop_hostpool_list](#test-310)
- [Test 311: azmcp_virtualdesktop_hostpool_sessionhost_list](#test-311)
- [Test 312: azmcp_virtualdesktop_hostpool_sessionhost_usersession-list](#test-312)
- [Test 313: azmcp_workbooks_create](#test-313)
- [Test 314: azmcp_workbooks_delete](#test-314)
- [Test 315: azmcp_workbooks_list](#test-315)
- [Test 316: azmcp_workbooks_list](#test-316)
- [Test 317: azmcp_workbooks_show](#test-317)
- [Test 318: azmcp_workbooks_show](#test-318)
- [Test 319: azmcp_workbooks_update](#test-319)
- [Test 320: azmcp_bicepschema_get](#test-320)
- [Test 321: azmcp_cloudarchitect_design](#test-321)
- [Test 322: azmcp_cloudarchitect_design](#test-322)
- [Test 323: azmcp_cloudarchitect_design](#test-323)
- [Test 324: azmcp_cloudarchitect_design](#test-324)

---

## Test 1

**Expected Tool:** `azmcp_foundry_agents_connect`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603124 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.535739 | `azmcp_foundry_agents_connect` | ✅ **EXPECTED** |
| 3 | 0.494703 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.443011 | `azmcp_foundry_agents_evaluate` | ❌ |
| 5 | 0.379587 | `azmcp_search_index_query` | ❌ |
| 6 | 0.365856 | `azmcp_foundry_models_list` | ❌ |
| 7 | 0.355385 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 8 | 0.327613 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.319855 | `azmcp_foundry_models_deploy` | ❌ |
| 10 | 0.305681 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.297812 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 12 | 0.272398 | `azmcp_search_service_list` | ❌ |
| 13 | 0.243499 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.241213 | `azmcp_postgres_database_query` | ❌ |
| 15 | 0.232346 | `azmcp_search_index_get` | ❌ |
| 16 | 0.230797 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.226514 | `azmcp_monitor_workspace_log_query` | ❌ |
| 18 | 0.217737 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.211141 | `azmcp_mysql_database_query` | ❌ |
| 20 | 0.192152 | `azmcp_speech_stt_recognize` | ❌ |

---

## Test 2

**Expected Tool:** `azmcp_foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544099 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469428 | `azmcp_foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.356359 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.280833 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.235573 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.233793 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.233359 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.232102 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.228550 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.224773 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.220994 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.218519 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.214507 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.210219 | `azmcp_search_index_query` | ❌ |
| 15 | 0.207689 | `azmcp_postgres_database_query` | ❌ |
| 16 | 0.207578 | `azmcp_loadtesting_testrun_list` | ❌ |
| 17 | 0.203873 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 18 | 0.194160 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.187851 | `azmcp_mysql_table_schema_get` | ❌ |
| 20 | 0.183165 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 3

**Expected Tool:** `azmcp_foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my AI Foundry project for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580566 | `azmcp_foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 2 | 0.518655 | `azmcp_foundry_agents_evaluate` | ❌ |
| 3 | 0.470998 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.382127 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.315921 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.315347 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.308767 | `azmcp_foundry_models_deploy` | ❌ |
| 8 | 0.276459 | `azmcp_foundry_models_list` | ❌ |
| 9 | 0.253361 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 10 | 0.246328 | `azmcp_search_index_query` | ❌ |
| 11 | 0.231482 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.207748 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.190273 | `azmcp_speech_stt_recognize` | ❌ |
| 14 | 0.188340 | `azmcp_monitor_workspace_log_query` | ❌ |
| 15 | 0.183816 | `azmcp_postgres_database_query` | ❌ |
| 16 | 0.179159 | `azmcp_search_service_list` | ❌ |
| 17 | 0.166262 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.163051 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 19 | 0.162163 | `azmcp_mysql_database_query` | ❌ |
| 20 | 0.153536 | `azmcp_mysql_server_list` | ❌ |

---

## Test 4

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695201 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.533085 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.526805 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 4 | 0.433117 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.422336 | `azmcp_search_index_get` | ❌ |
| 6 | 0.412895 | `azmcp_search_service_list` | ❌ |
| 7 | 0.349506 | `azmcp_search_index_query` | ❌ |
| 8 | 0.329682 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.310470 | `azmcp_foundry_models_deployments_list` | ❌ |
| 10 | 0.309220 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.296877 | `azmcp_grafana_list` | ❌ |
| 12 | 0.291635 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.286074 | `azmcp_monitor_table_type_list` | ❌ |
| 14 | 0.279802 | `azmcp_keyvault_key_list` | ❌ |
| 15 | 0.270306 | `azmcp_redis_cache_list` | ❌ |
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
| 2 | 0.489513 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.474093 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.396819 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.374208 | `azmcp_search_index_get` | ❌ |
| 6 | 0.350751 | `azmcp_search_service_list` | ❌ |
| 7 | 0.341865 | `azmcp_search_index_query` | ❌ |
| 8 | 0.317997 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.310576 | `azmcp_foundry_models_deploy` | ❌ |
| 10 | 0.278147 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.276839 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.272237 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.256208 | `azmcp_grafana_list` | ❌ |
| 14 | 0.250355 | `azmcp_foundry_agents_connect` | ❌ |
| 15 | 0.232587 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.225290 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.224194 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.223814 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.223695 | `azmcp_monitor_metrics_definitions` | ❌ |
| 20 | 0.218010 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 6

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672662 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.564860 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.423942 | `azmcp_search_index_get` | ❌ |
| 4 | 0.397302 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.375275 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.363951 | `azmcp_kusto_table_schema` | ❌ |
| 7 | 0.358315 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.349967 | `azmcp_search_index_query` | ❌ |
| 9 | 0.347762 | `azmcp_foundry_models_list` | ❌ |
| 10 | 0.346560 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.326807 | `azmcp_search_service_list` | ❌ |
| 12 | 0.297822 | `azmcp_foundry_models_deploy` | ❌ |
| 13 | 0.295847 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.285897 | `azmcp_monitor_table_type_list` | ❌ |
| 15 | 0.277468 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
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
| 1 | 0.649901 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432759 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.415963 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.408316 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.398186 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.379800 | `azmcp_search_index_get` | ❌ |
| 7 | 0.352243 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.318648 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.312132 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.309927 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.286991 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.271910 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.271701 | `azmcp_loadtesting_testrun_list` | ❌ |
| 14 | 0.262783 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.257402 | `azmcp_mysql_table_list` | ❌ |
| 16 | 0.256303 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.249010 | `azmcp_search_index_query` | ❌ |
| 18 | 0.246815 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.242191 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.239938 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 8

**Expected Tool:** `azmcp_foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.313400 | `azmcp_foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.282464 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.274073 | `azmcp_deploy_plan_get` | ❌ |
| 4 | 0.269681 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.268967 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.234071 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.222504 | `azmcp_grafana_list` | ❌ |
| 8 | 0.222478 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.221635 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.217213 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.216775 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 12 | 0.215293 | `azmcp_loadtesting_testrun_create` | ❌ |
| 13 | 0.209865 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.208124 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.207601 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.204420 | `azmcp_postgres_server_param_set` | ❌ |
| 17 | 0.195519 | `azmcp_workbooks_list` | ❌ |
| 18 | 0.192373 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.192370 | `azmcp_monitor_metrics_query` | ❌ |
| 20 | 0.190106 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 9

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559508 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.549636 | `azmcp_foundry_models_list` | ❌ |
| 3 | 0.539764 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.533239 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.448711 | `azmcp_search_service_list` | ❌ |
| 6 | 0.434472 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 7 | 0.368250 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.334867 | `azmcp_grafana_list` | ❌ |
| 9 | 0.332002 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.328526 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 11 | 0.326266 | `azmcp_search_index_get` | ❌ |
| 12 | 0.320998 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.318854 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.310280 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.302262 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.301302 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.300357 | `azmcp_search_index_query` | ❌ |
| 18 | 0.289448 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.288326 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.285916 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 10

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518278 | `azmcp_foundry_models_list` | ❌ |
| 2 | 0.503499 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.488965 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 4 | 0.486574 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.401100 | `azmcp_search_service_list` | ❌ |
| 6 | 0.396437 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 7 | 0.328854 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.311565 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 9 | 0.305957 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.301397 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.298826 | `azmcp_search_index_query` | ❌ |
| 12 | 0.290911 | `azmcp_search_index_get` | ❌ |
| 13 | 0.286764 | `azmcp_grafana_list` | ❌ |
| 14 | 0.269873 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.254924 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.250327 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.246875 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.243130 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.236546 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.234124 | `azmcp_redis_cache_list` | ❌ |

---

## Test 11

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560022 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.492050 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.401146 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.387861 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.386180 | `azmcp_search_service_list` | ❌ |
| 6 | 0.346909 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.298648 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.290690 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 9 | 0.285437 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.277883 | `azmcp_grafana_list` | ❌ |
| 11 | 0.274917 | `azmcp_search_index_get` | ❌ |
| 12 | 0.272652 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.265730 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.255790 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.255760 | `azmcp_search_index_query` | ❌ |
| 16 | 0.252297 | `azmcp_postgres_database_list` | ❌ |
| 17 | 0.248715 | `azmcp_redis_cache_list` | ❌ |
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
| 2 | 0.475292 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.430513 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.388967 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.356899 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.339069 | `azmcp_search_service_list` | ❌ |
| 7 | 0.299471 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 8 | 0.283250 | `azmcp_search_index_query` | ❌ |
| 9 | 0.279886 | `azmcp_foundry_agents_connect` | ❌ |
| 10 | 0.274019 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.266937 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.261445 | `azmcp_search_index_get` | ❌ |
| 13 | 0.260144 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.245943 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.244697 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.240256 | `azmcp_monitor_metrics_definitions` | ❌ |
| 17 | 0.234050 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.225102 | `azmcp_speech_stt_recognize` | ❌ |
| 19 | 0.217331 | `azmcp_marketplace_product_list` | ❌ |
| 20 | 0.211456 | `azmcp_mysql_database_list` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680085 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544286 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.490572 | `azmcp_search_service_list` | ❌ |
| 4 | 0.466025 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.459612 | `azmcp_search_index_query` | ❌ |
| 6 | 0.393515 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.388185 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.372359 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.370869 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.358283 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.356292 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.356182 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.354870 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.352197 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.351122 | `azmcp_sql_server_show` | ❌ |
| 16 | 0.348266 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.343152 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.337045 | `azmcp_keyvault_key_get` | ❌ |
| 19 | 0.333635 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.329961 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 14

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639545 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.620140 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561856 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.480817 | `azmcp_search_index_query` | ❌ |
| 5 | 0.453001 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.445389 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 7 | 0.439225 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.416404 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.409307 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.406485 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.397423 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.382791 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.378750 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.378297 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.375372 | `azmcp_foundry_models_deployments_list` | ❌ |
| 16 | 0.371099 | `azmcp_mysql_table_list` | ❌ |
| 17 | 0.369245 | `azmcp_keyvault_certificate_list` | ❌ |
| 18 | 0.368931 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367804 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.367450 | `azmcp_redis_cache_list` | ❌ |

---

## Test 15

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620268 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562775 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561154 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.471416 | `azmcp_search_index_query` | ❌ |
| 5 | 0.463913 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.408555 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.401662 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.382692 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.372639 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.370330 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.367868 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.351788 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.351161 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.350043 | `azmcp_kusto_database_list` | ❌ |
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
| 1 | 0.521790 | `azmcp_search_index_get` | ❌ |
| 2 | 0.515870 | `azmcp_search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497467 | `azmcp_search_service_list` | ❌ |
| 4 | 0.373917 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.372943 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.327158 | `azmcp_kusto_query` | ❌ |
| 7 | 0.322358 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.311044 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.306415 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.305939 | `azmcp_marketplace_product_list` | ❌ |
| 11 | 0.295581 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.291313 | `azmcp_foundry_agents_connect` | ❌ |
| 13 | 0.290845 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.288242 | `azmcp_foundry_models_deployments_list` | ❌ |
| 15 | 0.287501 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.283532 | `azmcp_foundry_models_list` | ❌ |
| 17 | 0.274979 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.274064 | `azmcp_speech_stt_recognize` | ❌ |
| 19 | 0.260171 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.244844 | `azmcp_kusto_sample` | ❌ |

---

## Test 17

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.520301 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.505311 | `azmcp_search_index_get` | ❌ |
| 4 | 0.500497 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.494272 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.493011 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.492228 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.486066 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.482464 | `azmcp_grafana_list` | ❌ |
| 10 | 0.477471 | `azmcp_subscription_list` | ❌ |
| 11 | 0.470384 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.470055 | `azmcp_marketplace_product_list` | ❌ |
| 13 | 0.454460 | `azmcp_foundry_models_deployments_list` | ❌ |
| 14 | 0.451893 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.443495 | `azmcp_search_index_query` | ❌ |
| 16 | 0.431621 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.428085 | `azmcp_group_list` | ❌ |
| 18 | 0.425454 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.418007 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.417472 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 18

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.479332 | `azmcp_search_index_get` | ❌ |
| 3 | 0.467379 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.453489 | `azmcp_marketplace_product_list` | ❌ |
| 5 | 0.448446 | `azmcp_search_index_query` | ❌ |
| 6 | 0.425939 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.419493 | `azmcp_marketplace_product_get` | ❌ |
| 8 | 0.412158 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.408456 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.400319 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.399822 | `azmcp_grafana_list` | ❌ |
| 12 | 0.397883 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.393708 | `azmcp_subscription_list` | ❌ |
| 14 | 0.390660 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.390559 | `azmcp_foundry_models_list` | ❌ |
| 16 | 0.389433 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.379805 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 18 | 0.376089 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.373463 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.363444 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 19

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553025 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.435702 | `azmcp_search_index_get` | ❌ |
| 3 | 0.417179 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.404758 | `azmcp_search_index_query` | ❌ |
| 5 | 0.344699 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.336174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.322580 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 8 | 0.322540 | `azmcp_foundry_models_list` | ❌ |
| 9 | 0.300427 | `azmcp_marketplace_product_list` | ❌ |
| 10 | 0.292677 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.290214 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.289466 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 13 | 0.283366 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.282232 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.281672 | `azmcp_get_bestpractices_get` | ❌ |
| 16 | 0.281134 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.278628 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.278574 | `azmcp_cloudarchitect_design` | ❌ |
| 19 | 0.277693 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.275013 | `azmcp_sql_server_show` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665993 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.351145 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.340007 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.337734 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.335331 | `azmcp_search_index_query` | ❌ |
| 6 | 0.334471 | `azmcp_storage_blob_upload` | ❌ |
| 7 | 0.323215 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.319704 | `azmcp_search_service_list` | ❌ |
| 9 | 0.316149 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.310353 | `azmcp_deploy_iac_rules_get` | ❌ |
| 11 | 0.307550 | `azmcp_extension_azqr` | ❌ |
| 12 | 0.303470 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.300497 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.297745 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.296134 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 16 | 0.286649 | `azmcp_search_index_get` | ❌ |
| 17 | 0.281174 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.262514 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.257922 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.253391 | `azmcp_storage_account_create` | ❌ |

---

## Test 21

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511324 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.194577 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.135394 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.131547 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.128270 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 6 | 0.125957 | `azmcp_foundry_agents_evaluate` | ❌ |
| 7 | 0.116789 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.114307 | `azmcp_quota_region_availability_list` | ❌ |
| 9 | 0.112138 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.110152 | `azmcp_foundry_models_list` | ❌ |
| 11 | 0.108836 | `azmcp_cloudarchitect_design` | ❌ |
| 12 | 0.104791 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 13 | 0.100839 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.097123 | `azmcp_search_index_query` | ❌ |
| 15 | 0.088423 | `azmcp_storage_blob_upload` | ❌ |
| 16 | 0.068723 | `azmcp_search_index_get` | ❌ |
| 17 | 0.067602 | `azmcp_search_service_list` | ❌ |
| 18 | 0.061775 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.055965 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.048983 | `azmcp_monitor_table_type_list` | ❌ |

---

## Test 22

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486489 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.156850 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.138836 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.137051 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.128450 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 6 | 0.127931 | `azmcp_foundry_agents_evaluate` | ❌ |
| 7 | 0.126361 | `azmcp_deploy_iac_rules_get` | ❌ |
| 8 | 0.125816 | `azmcp_extension_azqr` | ❌ |
| 9 | 0.123154 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 10 | 0.117189 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.108205 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.108025 | `azmcp_keyvault_certificate_import` | ❌ |
| 13 | 0.101679 | `azmcp_workbooks_delete` | ❌ |
| 14 | 0.088065 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 15 | 0.087278 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.085402 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.081850 | `azmcp_mysql_database_query` | ❌ |
| 18 | 0.077891 | `azmcp_search_index_query` | ❌ |
| 19 | 0.066898 | `azmcp_sql_server_delete` | ❌ |
| 20 | 0.064181 | `azmcp_monitor_workspace_log_query` | ❌ |

---

## Test 23

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611992 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.269482 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.206586 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.189418 | `azmcp_storage_blob_upload` | ❌ |
| 5 | 0.178516 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 6 | 0.165024 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.164101 | `azmcp_search_index_query` | ❌ |
| 8 | 0.154104 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 9 | 0.151861 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 10 | 0.151789 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.147424 | `azmcp_foundry_models_list` | ❌ |
| 12 | 0.146830 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.143047 | `azmcp_keyvault_certificate_import` | ❌ |
| 14 | 0.142914 | `azmcp_foundry_agents_evaluate` | ❌ |
| 15 | 0.135645 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.131741 | `azmcp_search_index_get` | ❌ |
| 17 | 0.123820 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.118104 | `azmcp_monitor_workspace_log_query` | ❌ |
| 19 | 0.117731 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.117696 | `azmcp_sql_db_create` | ❌ |

---

## Test 24

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.410533 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.151632 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.140362 | `azmcp_deploy_plan_get` | ❌ |
| 4 | 0.139584 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.136931 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.135763 | `azmcp_extension_azqr` | ❌ |
| 7 | 0.131559 | `azmcp_storage_blob_upload` | ❌ |
| 8 | 0.128429 | `azmcp_loadtesting_testrun_create` | ❌ |
| 9 | 0.126411 | `azmcp_foundry_agents_connect` | ❌ |
| 10 | 0.118981 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 11 | 0.117228 | `azmcp_foundry_agents_evaluate` | ❌ |
| 12 | 0.111400 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.099391 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.096968 | `azmcp_mysql_table_schema_get` | ❌ |
| 15 | 0.096526 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.094094 | `azmcp_mysql_server_param_set` | ❌ |
| 17 | 0.093750 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.086904 | `azmcp_postgres_table_schema_get` | ❌ |
| 19 | 0.086631 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.085639 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 25

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546120 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.210172 | `azmcp_loadtesting_testrun_get` | ❌ |
| 3 | 0.183385 | `azmcp_extension_azqr` | ❌ |
| 4 | 0.180691 | `azmcp_search_index_get` | ❌ |
| 5 | 0.177603 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.168108 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.167093 | `azmcp_foundry_agents_connect` | ❌ |
| 8 | 0.164619 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.153764 | `azmcp_foundry_models_deploy` | ❌ |
| 10 | 0.150961 | `azmcp_mysql_table_schema_get` | ❌ |
| 11 | 0.140191 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.138676 | `azmcp_deploy_plan_get` | ❌ |
| 13 | 0.137452 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.132225 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 15 | 0.131714 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.124875 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.123275 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.118596 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.117808 | `azmcp_servicebus_queue_details` | ❌ |
| 20 | 0.117254 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 26

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539963 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.184343 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.176561 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.174984 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.150263 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.149563 | `azmcp_foundry_models_deploy` | ❌ |
| 7 | 0.148242 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 8 | 0.140035 | `azmcp_extension_azqr` | ❌ |
| 9 | 0.139171 | `azmcp_deploy_iac_rules_get` | ❌ |
| 10 | 0.137972 | `azmcp_foundry_models_list` | ❌ |
| 11 | 0.135576 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 12 | 0.111748 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.108785 | `azmcp_search_index_query` | ❌ |
| 14 | 0.098972 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.089200 | `azmcp_mysql_database_query` | ❌ |
| 16 | 0.088668 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.083308 | `azmcp_monitor_workspace_log_query` | ❌ |
| 18 | 0.081702 | `azmcp_search_index_get` | ❌ |
| 19 | 0.081269 | `azmcp_storage_blob_upload` | ❌ |
| 20 | 0.079037 | `azmcp_mysql_server_param_set` | ❌ |

---

## Test 27

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549151 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.378634 | `azmcp_cloudarchitect_design` | ❌ |
| 3 | 0.333769 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.333076 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.331978 | `azmcp_foundry_agents_connect` | ❌ |
| 6 | 0.324507 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 7 | 0.308868 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.296731 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.295863 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.292815 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.282397 | `azmcp_deploy_iac_rules_get` | ❌ |
| 12 | 0.272556 | `azmcp_search_index_query` | ❌ |
| 13 | 0.268344 | `azmcp_search_service_list` | ❌ |
| 14 | 0.252812 | `azmcp_search_index_get` | ❌ |
| 15 | 0.240523 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.212820 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.207133 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.195969 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.190642 | `azmcp_monitor_workspace_log_query` | ❌ |
| 20 | 0.190381 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 28

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532536 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.326712 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.318836 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.304769 | `azmcp_search_service_list` | ❌ |
| 5 | 0.301427 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.289963 | `azmcp_search_index_query` | ❌ |
| 7 | 0.283932 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.283412 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.282810 | `azmcp_search_index_get` | ❌ |
| 10 | 0.281686 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.277339 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.274439 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 13 | 0.261375 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.240693 | `azmcp_deploy_iac_rules_get` | ❌ |
| 15 | 0.230608 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.210607 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.210399 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.202005 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.196828 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.195483 | `azmcp_subscription_list` | ❌ |

---

## Test 29

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453396 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.173205 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.160185 | `azmcp_extension_azqr` | ❌ |
| 4 | 0.151782 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.145911 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.144791 | `azmcp_foundry_models_deploy` | ❌ |
| 7 | 0.140232 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.137288 | `azmcp_keyvault_certificate_import` | ❌ |
| 9 | 0.129095 | `azmcp_foundry_agents_evaluate` | ❌ |
| 10 | 0.128780 | `azmcp_loadtesting_testrun_create` | ❌ |
| 11 | 0.127386 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 12 | 0.105193 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.102727 | `azmcp_workbooks_delete` | ❌ |
| 14 | 0.091674 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.091488 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.085952 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 17 | 0.082401 | `azmcp_mysql_database_query` | ❌ |
| 18 | 0.075326 | `azmcp_postgres_database_query` | ❌ |
| 19 | 0.072602 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.071789 | `azmcp_sql_server_delete` | ❌ |

---

## Test 30

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.635561 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.492182 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.491380 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.473554 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.442214 | `azmcp_grafana_list` | ❌ |
| 7 | 0.441656 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.433594 | `azmcp_eventgrid_topic_list` | ❌ |
| 9 | 0.432238 | `azmcp_search_service_list` | ❌ |
| 10 | 0.427658 | `azmcp_subscription_list` | ❌ |
| 11 | 0.427460 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.423903 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.420794 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.412274 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.408599 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.398419 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.389458 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.385938 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.380818 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.370646 | `azmcp_postgres_server_config_get` | ❌ |

---

## Test 31

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
| 6 | 0.368807 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.359567 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.356514 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.355830 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.354747 | `azmcp_appconfig_kv_delete` | ❌ |
| 11 | 0.348603 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.344550 | `azmcp_marketplace_product_get` | ❌ |
| 13 | 0.341263 | `azmcp_grafana_list` | ❌ |
| 14 | 0.340731 | `azmcp_eventgrid_topic_list` | ❌ |
| 15 | 0.332824 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.325885 | `azmcp_subscription_list` | ❌ |
| 17 | 0.325232 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.318639 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.310432 | `azmcp_search_service_list` | ❌ |
| 20 | 0.292788 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 32

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
| 11 | 0.250470 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.239130 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.236380 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.234890 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.233357 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.231713 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.228042 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.226029 | `azmcp_sql_db_update` | ❌ |
| 19 | 0.221645 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.221405 | `azmcp_postgres_database_list` | ❌ |

---

## Test 33

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
| 6 | 0.392016 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.268822 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.262117 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.248752 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.240483 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.236168 | `azmcp_keyvault_key_get` | ❌ |
| 12 | 0.218487 | `azmcp_mysql_server_param_set` | ❌ |
| 13 | 0.218373 | `azmcp_sql_server_delete` | ❌ |
| 14 | 0.196121 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 15 | 0.194933 | `azmcp_sql_db_delete` | ❌ |
| 16 | 0.194831 | `azmcp_postgres_server_config_get` | ❌ |
| 17 | 0.183662 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.175403 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.173133 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.166763 | `azmcp_storage_account_get` | ❌ |

---

## Test 34

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
| 8 | 0.374460 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.338195 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.333355 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.327550 | `azmcp_loadtesting_testrun_list` | ❌ |
| 12 | 0.323615 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.321550 | `azmcp_keyvault_certificate_list` | ❌ |
| 14 | 0.317744 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.296084 | `azmcp_postgres_table_list` | ❌ |
| 16 | 0.292126 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.275469 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.267026 | `azmcp_postgres_database_list` | ❌ |
| 19 | 0.266351 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.264850 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 35

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
| 12 | 0.294807 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.288088 | `azmcp_mysql_server_param_set` | ❌ |
| 14 | 0.278909 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.269939 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.264526 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.258640 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.250505 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.243650 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.238151 | `azmcp_sql_server_show` | ❌ |

---

## Test 36

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
| 9 | 0.238544 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.238247 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.214124 | `azmcp_keyvault_key_get` | ❌ |
| 12 | 0.211331 | `azmcp_postgres_server_config_get` | ❌ |
| 13 | 0.210627 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.185698 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.163738 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.158946 | `azmcp_mysql_server_param_get` | ❌ |
| 17 | 0.154473 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.144377 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.139871 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.123282 | `azmcp_storage_account_create` | ❌ |

---

## Test 37

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
| 6 | 0.409406 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.272305 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.268062 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.259561 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.252818 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.237098 | `azmcp_mysql_server_param_set` | ❌ |
| 12 | 0.225350 | `azmcp_postgres_server_config_get` | ❌ |
| 13 | 0.190875 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.190136 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.185161 | `azmcp_postgres_server_param_set` | ❌ |
| 16 | 0.179797 | `azmcp_mysql_server_param_get` | ❌ |
| 17 | 0.171375 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.159741 | `azmcp_postgres_server_param_get` | ❌ |
| 19 | 0.150490 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.143564 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 38

**Expected Tool:** `azmcp_appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609626 | `azmcp_appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536532 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 3 | 0.518428 | `azmcp_appconfig_kv_list` | ❌ |
| 4 | 0.507148 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.505525 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.377847 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.360097 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.346940 | `azmcp_postgres_server_param_set` | ❌ |
| 9 | 0.311522 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.306040 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.276041 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.263347 | `azmcp_sql_db_update` | ❌ |
| 13 | 0.236830 | `azmcp_keyvault_secret_get` | ❌ |
| 14 | 0.213602 | `azmcp_mysql_server_param_get` | ❌ |
| 15 | 0.208883 | `azmcp_postgres_server_config_get` | ❌ |
| 16 | 0.194012 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.166937 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.164369 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.137894 | `azmcp_storage_account_create` | ❌ |
| 20 | 0.128669 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 39

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
| 7 | 0.323583 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.301859 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.301266 | `azmcp_keyvault_secret_get` | ❌ |
| 10 | 0.291448 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.269387 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.259549 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.257940 | `azmcp_mysql_server_param_get` | ❌ |
| 14 | 0.229242 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.226651 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.217843 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.206456 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.201872 | `azmcp_mysql_server_param_set` | ❌ |
| 19 | 0.186734 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.186063 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 40

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.355635 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.329318 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.257790 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.216077 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.206503 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.205255 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.193708 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 9 | 0.181209 | `azmcp_foundry_agents_evaluate` | ❌ |
| 10 | 0.177942 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.169553 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.163768 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.148018 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.141714 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.133096 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.128778 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.125735 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.120066 | `azmcp_mysql_table_schema_get` | ❌ |
| 19 | 0.116209 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.111751 | `azmcp_redis_cache_list` | ❌ |

---

## Test 41

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.318537 | `azmcp_deploy_app_logs_get` | ❌ |
| 2 | 0.302557 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.255570 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.225972 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.222226 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.200402 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.200284 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.186927 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.172691 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.163364 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.162349 | `azmcp_foundry_agents_evaluate` | ❌ |
| 12 | 0.150567 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.150313 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.144054 | `azmcp_mysql_server_param_get` | ❌ |
| 15 | 0.133109 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.125941 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.118881 | `azmcp_mysql_table_schema_get` | ❌ |
| 18 | 0.112992 | `azmcp_monitor_workspace_log_query` | ❌ |
| 19 | 0.107084 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.101768 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 42

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.256325 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 2 | 0.250546 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.215860 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.199067 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.188245 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.188040 | `azmcp_appservice_database_add` | ❌ |
| 7 | 0.179320 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.178879 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.159064 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.158398 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.156599 | `azmcp_search_service_list` | ❌ |
| 12 | 0.156168 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 13 | 0.153402 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.151702 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.149489 | `azmcp_speech_stt_recognize` | ❌ |
| 16 | 0.146689 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.139567 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.130326 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.129424 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.126179 | `azmcp_search_index_get` | ❌ |

---

## Test 43

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a database connection to my app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729071 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.398617 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.368252 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.364437 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.361936 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.353953 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.348738 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.342556 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.342159 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.334383 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.301680 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.300846 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.298638 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.286125 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.281420 | `azmcp_loadtesting_testresource_create` | ❌ |
| 16 | 0.280123 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.272080 | `azmcp_keyvault_secret_create` | ❌ |
| 18 | 0.266198 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.264904 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.260527 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 44

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure a SQL Server database for app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612164 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.484307 | `azmcp_sql_db_update` | ❌ |
| 3 | 0.471103 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.408878 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.405300 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.389139 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.381822 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.367325 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.366336 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.355360 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.352359 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.350677 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.345345 | `azmcp_sql_db_delete` | ❌ |
| 14 | 0.340399 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.329197 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.322825 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.315983 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.304849 | `azmcp_loadtesting_test_create` | ❌ |
| 19 | 0.299644 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.295124 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 45

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a MySQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648464 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.418902 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.409593 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.382602 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.351839 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.345795 | `azmcp_sql_db_update` | ❌ |
| 7 | 0.344869 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.335323 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.323158 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.320639 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.314492 | `azmcp_mysql_server_param_set` | ❌ |
| 12 | 0.311382 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.297738 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.295428 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.279588 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.272652 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.272634 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.269892 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.269785 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.260632 | `azmcp_functionapp_get` | ❌ |

---

## Test 46

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a PostgreSQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579502 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449085 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.439660 | `azmcp_postgres_database_query` | ❌ |
| 4 | 0.409515 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.405431 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.399788 | `azmcp_postgres_server_param_set` | ❌ |
| 7 | 0.383413 | `azmcp_sql_db_create` | ❌ |
| 8 | 0.337005 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.328945 | `azmcp_postgres_server_param_get` | ❌ |
| 10 | 0.305301 | `azmcp_sql_db_update` | ❌ |
| 11 | 0.302980 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.289343 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.279654 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.258603 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.257593 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.254307 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.241522 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.233724 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.231783 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.223353 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 47

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a CosmosDB database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643046 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.477031 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.465637 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.421268 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.400020 | `azmcp_sql_db_update` | ❌ |
| 6 | 0.378402 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.374251 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.370137 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.362523 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.353056 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.352381 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.349533 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.326631 | `azmcp_sql_db_delete` | ❌ |
| 14 | 0.325004 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.314834 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.314554 | `azmcp_sql_server_delete` | ❌ |
| 17 | 0.314391 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.309146 | `azmcp_redis_cluster_database_list` | ❌ |
| 19 | 0.303278 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.292774 | `azmcp_sql_server_create` | ❌ |

---

## Test 48

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645031 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.488601 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.423831 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.422429 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.394862 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.394755 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.381636 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.369230 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.360718 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.357296 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.349724 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.348499 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.347719 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.345944 | `azmcp_sql_db_delete` | ❌ |
| 15 | 0.304588 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.281663 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.277387 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.274842 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 19 | 0.274345 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.266106 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 49

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665216 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.371335 | `azmcp_sql_db_update` | ❌ |
| 3 | 0.369071 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.332119 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.314270 | `azmcp_cosmos_database_list` | ❌ |
| 6 | 0.312458 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.307420 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.304622 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.297194 | `azmcp_mysql_server_param_get` | ❌ |
| 10 | 0.294182 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.292606 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.286137 | `azmcp_postgres_server_param_set` | ❌ |
| 13 | 0.273579 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.269033 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.267621 | `azmcp_sql_server_show` | ❌ |
| 16 | 0.267098 | `azmcp_mysql_server_param_set` | ❌ |
| 17 | 0.266587 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.265629 | `azmcp_sql_db_delete` | ❌ |
| 19 | 0.260212 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.256506 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 50

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536686 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.394884 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.391835 | `azmcp_sql_db_update` | ❌ |
| 4 | 0.318426 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.318151 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.305417 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.301292 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.298553 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.298236 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.297707 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.295503 | `azmcp_subscription_list` | ❌ |
| 12 | 0.294547 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.290423 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.281060 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.274946 | `azmcp_sql_db_delete` | ❌ |
| 16 | 0.274348 | `azmcp_sql_server_delete` | ❌ |
| 17 | 0.273329 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.272321 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.266969 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.265663 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 51

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560268 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.426753 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.361028 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.349556 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.346672 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.344869 | `azmcp_sql_db_update` | ❌ |
| 7 | 0.342276 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.339789 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.339482 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.330944 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.317003 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.292346 | `azmcp_sql_server_delete` | ❌ |
| 13 | 0.281774 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.276951 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.270334 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.268258 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.263797 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.258869 | `azmcp_keyvault_certificate_create` | ❌ |
| 19 | 0.257394 | `azmcp_mysql_table_list` | ❌ |
| 20 | 0.257248 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 52

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572911 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.445157 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.390478 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.385368 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.375286 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.357934 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.352871 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.346028 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.344858 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.330014 | `azmcp_search_service_list` | ❌ |
| 11 | 0.326095 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.297029 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.296190 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.268844 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.265962 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.263811 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.260352 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.258344 | `azmcp_monitor_table_list` | ❌ |
| 19 | 0.247882 | `azmcp_search_index_get` | ❌ |
| 20 | 0.245708 | `azmcp_redis_cache_list` | ❌ |

---

## Test 53

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696382 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.468384 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.452121 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.435241 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.424622 | `azmcp_search_service_list` | ❌ |
| 6 | 0.405506 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.405253 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.401105 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.393809 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.387901 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.380218 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.371668 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.367714 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.367243 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.362939 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.355398 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.339417 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.336684 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.334823 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.332236 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 54

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627030 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.479392 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.468861 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.467717 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.461677 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.451694 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.449821 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.446454 | `azmcp_search_service_list` | ❌ |
| 9 | 0.419715 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.417639 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.416057 | `azmcp_get_bestpractices_get` | ❌ |
| 12 | 0.415664 | `azmcp_monitor_metrics_definitions` | ❌ |
| 13 | 0.407441 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.401204 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.400853 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.398817 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.389786 | `azmcp_monitor_table_type_list` | ❌ |
| 18 | 0.389008 | `azmcp_group_list` | ❌ |
| 19 | 0.386954 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.385121 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 55

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509418 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.398251 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.383767 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.369053 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.367278 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.341619 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.325776 | `azmcp_deploy_iac_rules_get` | ❌ |
| 8 | 0.324538 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.321848 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.313589 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.287252 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.285234 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.262799 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.259246 | `azmcp_search_service_list` | ❌ |
| 15 | 0.254871 | `azmcp_search_index_query` | ❌ |
| 16 | 0.247065 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.233954 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.230227 | `azmcp_monitor_workspace_log_query` | ❌ |
| 19 | 0.229476 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.225566 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 56

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.541506 | `azmcp_search_service_list` | ❌ |
| 4 | 0.527457 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.515937 | `azmcp_subscription_list` | ❌ |
| 6 | 0.514293 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.509386 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.503032 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.490776 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.487235 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.483500 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.482236 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.481819 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.480998 | `azmcp_group_list` | ❌ |
| 15 | 0.469958 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.462353 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.460523 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.460343 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.456503 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.454170 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 57

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.449979 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.382728 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.373120 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.372153 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.370858 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.364918 | `azmcp_search_service_list` | ❌ |
| 10 | 0.359344 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.356414 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.354277 | `azmcp_storage_blob_container_create` | ❌ |
| 13 | 0.353379 | `azmcp_subscription_list` | ❌ |
| 14 | 0.352818 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.349526 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.349291 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.348109 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.344750 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.344071 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.339252 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 58

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637130 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563476 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.474087 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471804 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.463742 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.459880 | `azmcp_search_service_list` | ❌ |
| 7 | 0.452938 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.451253 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.443939 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.440464 | `azmcp_subscription_list` | ❌ |
| 11 | 0.435835 | `azmcp_grafana_list` | ❌ |
| 12 | 0.435510 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.431745 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.430867 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.430308 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.419749 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.405244 | `azmcp_group_list` | ❌ |
| 18 | 0.398556 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.386495 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.364214 | `azmcp_mysql_server_list` | ❌ |

---

## Test 59

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654394 | `azmcp_acr_registry_repository_list` | ❌ |
| 2 | 0.633746 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476131 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.454822 | `azmcp_group_list` | ❌ |
| 5 | 0.454261 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.446351 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.427825 | `azmcp_workbooks_list` | ❌ |
| 8 | 0.423663 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421371 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.417466 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.411578 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.409226 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.403938 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.389099 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.378611 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.371033 | `azmcp_sql_elastic-pool_list` | ❌ |
| 17 | 0.370596 | `azmcp_redis_cluster_database_list` | ❌ |
| 18 | 0.356434 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.354424 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.352420 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 60

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.468028 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.449649 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.445781 | `azmcp_group_list` | ❌ |
| 6 | 0.416353 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.413975 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.413191 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.406543 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.403041 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.399910 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.389603 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.378353 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.369912 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.369779 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.367756 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.355626 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.354807 | `azmcp_loadtesting_testresource_list` | ❌ |
| 19 | 0.351411 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.347199 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 61

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.510487 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.495567 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.492550 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.475629 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.466001 | `azmcp_search_service_list` | ❌ |
| 8 | 0.461777 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.461369 | `azmcp_grafana_list` | ❌ |
| 10 | 0.456838 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.449239 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.448228 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.440083 | `azmcp_subscription_list` | ❌ |
| 14 | 0.438219 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.437309 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.431028 | `azmcp_group_list` | ❌ |
| 17 | 0.414463 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.405472 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.390890 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.377142 | `azmcp_mysql_database_list` | ❌ |

---

## Test 62

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546333 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.407973 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.399932 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.339307 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.326631 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.308650 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.306869 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.306442 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.302705 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.300174 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.296059 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.292155 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.290148 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.289864 | `azmcp_search_service_list` | ❌ |
| 16 | 0.283716 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.283390 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.282581 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.276498 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.272964 | `azmcp_redis_cluster_database_list` | ❌ |

---

## Test 63

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.433927 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.388271 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.370375 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.359617 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.356901 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.355398 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.351007 | `azmcp_redis_cluster_database_list` | ❌ |
| 10 | 0.347437 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.347084 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.340014 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.338404 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.337749 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.332856 | `azmcp_keyvault_key_list` | ❌ |
| 16 | 0.332785 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.332704 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.332572 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.330046 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.322287 | `azmcp_mysql_table_list` | ❌ |

---

## Test 64

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501842 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.418623 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.374433 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.359922 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.341582 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.335467 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.333318 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.324104 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.318706 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.316614 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.315414 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.311692 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.309627 | `azmcp_search_service_list` | ❌ |
| 15 | 0.306052 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.305022 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.303931 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.302466 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.300101 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.299303 | `azmcp_mysql_table_list` | ❌ |

---

## Test 65

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668480 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.615268 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.587691 | `azmcp_subscription_list` | ❌ |
| 5 | 0.560795 | `azmcp_search_service_list` | ❌ |
| 6 | 0.538321 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.528963 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.516914 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.502428 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.502199 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.499217 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.497679 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.487304 | `azmcp_group_list` | ❌ |
| 14 | 0.483046 | `azmcp_grafana_list` | ❌ |
| 15 | 0.474934 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.473625 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.460067 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.459502 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.459002 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.453975 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 66

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665447 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605357 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.571613 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.486033 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.436283 | `azmcp_subscription_list` | ❌ |
| 6 | 0.431496 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.428309 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.427709 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.408659 | `azmcp_search_service_list` | ❌ |
| 10 | 0.405726 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.397574 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.390141 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.389842 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.386108 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.383985 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.381323 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.379496 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.373667 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.367942 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.358376 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 67

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605125 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.566249 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.546327 | `azmcp_subscription_list` | ❌ |
| 5 | 0.530175 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.527812 | `azmcp_search_service_list` | ❌ |
| 7 | 0.488006 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.466414 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.457584 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.456302 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.455017 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.453626 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.441136 | `azmcp_grafana_list` | ❌ |
| 14 | 0.438277 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.437562 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.437026 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.434623 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.433094 | `azmcp_postgres_server_list` | ❌ |
| 19 | 0.430336 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.426516 | `azmcp_sql_db_list` | ❌ |

---

## Test 68

**Expected Tool:** `azmcp_cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.605253 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.566854 | `azmcp_cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 3 | 0.477874 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.447757 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.445371 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.429363 | `azmcp_search_service_list` | ❌ |
| 7 | 0.399756 | `azmcp_search_index_query` | ❌ |
| 8 | 0.378297 | `azmcp_kusto_query` | ❌ |
| 9 | 0.374844 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.372689 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.365717 | `azmcp_search_index_get` | ❌ |
| 12 | 0.358903 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.351331 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.340791 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.337890 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.335256 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.334389 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.331041 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.308694 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.302962 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 69

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852820 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681071 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.630681 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.581406 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.527472 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 6 | 0.486402 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.449005 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.447607 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.439840 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.427687 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.424307 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.422226 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.421595 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.420284 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.411277 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.392918 | `azmcp_postgres_database_list` | ❌ |
| 17 | 0.386887 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.383435 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.378426 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.372158 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 70

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789395 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.614220 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.562062 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.537172 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.521532 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 6 | 0.449120 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.411703 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.398064 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.397969 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.397755 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.395513 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.392763 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.386806 | `azmcp_redis_cluster_database_list` | ❌ |
| 14 | 0.356299 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.355640 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.345652 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.342266 | `azmcp_monitor_table_list` | ❌ |
| 18 | 0.325994 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.319603 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.318540 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 71

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
| 6 | 0.548066 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.526046 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.501477 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.471453 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.459194 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.450663 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.442540 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.418871 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.407722 | `azmcp_search_service_list` | ❌ |
| 15 | 0.406805 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.405825 | `azmcp_keyvault_key_list` | ❌ |
| 17 | 0.401638 | `azmcp_subscription_list` | ❌ |
| 18 | 0.397880 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.389032 | `azmcp_keyvault_secret_list` | ❌ |
| 20 | 0.387534 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 72

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
| 6 | 0.498206 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.497414 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.449759 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.447875 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.437993 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.408605 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.402767 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.396123 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.383722 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.379009 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.369344 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.348999 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.344442 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.342424 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.339516 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 73

**Expected Tool:** `azmcp_kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482142 | `azmcp_kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.464533 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.457708 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.416832 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.378482 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.362958 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.361809 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.353819 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.351310 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.344925 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.344612 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.333349 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.332611 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.326636 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.326064 | `azmcp_aks_nodepool_list` | ❌ |
| 16 | 0.325691 | `azmcp_search_index_get` | ❌ |
| 17 | 0.319776 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.318800 | `azmcp_kusto_query` | ❌ |
| 19 | 0.318134 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.314718 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 74

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.651122 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.643971 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.548989 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.535878 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.509358 | `azmcp_grafana_list` | ❌ |
| 6 | 0.505979 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.492083 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.491140 | `azmcp_search_service_list` | ❌ |
| 9 | 0.487481 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.485971 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.460076 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.458723 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.451388 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.427560 | `azmcp_subscription_list` | ❌ |
| 15 | 0.420102 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.412496 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.412290 | `azmcp_group_list` | ❌ |
| 18 | 0.409917 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.399070 | `azmcp_monitor_table_list` | ❌ |
| 20 | 0.391179 | `azmcp_monitor_table_type_list` | ❌ |

---

## Test 75

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
| 6 | 0.338217 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.314733 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.303083 | `azmcp_grafana_list` | ❌ |
| 9 | 0.293096 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.292985 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.287768 | `azmcp_kusto_sample` | ❌ |
| 12 | 0.285680 | `azmcp_kusto_query` | ❌ |
| 13 | 0.283331 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.277014 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.275559 | `azmcp_mysql_database_query` | ❌ |
| 16 | 0.270804 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.265906 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.264112 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.264035 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.263226 | `azmcp_quota_usage_check` | ❌ |

---

## Test 76

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584053 | `azmcp_redis_cluster_list` | ❌ |
| 2 | 0.549797 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 3 | 0.471120 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.469570 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.464294 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.462945 | `azmcp_grafana_list` | ❌ |
| 7 | 0.446272 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.440326 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.434016 | `azmcp_search_service_list` | ❌ |
| 10 | 0.432048 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.406863 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.396253 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.392541 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.386776 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.380006 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.377663 | `azmcp_kusto_query` | ❌ |
| 17 | 0.371088 | `azmcp_subscription_list` | ❌ |
| 18 | 0.368890 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.365323 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.356138 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 77

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
| 12 | 0.403731 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.396014 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.375628 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.363598 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.363279 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.350316 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.334426 | `azmcp_grafana_list` | ❌ |
| 19 | 0.320635 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.319260 | `azmcp_kusto_query` | ❌ |

---

## Test 78

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
| 13 | 0.359344 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.344010 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.336400 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.336104 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.334803 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.310919 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.309809 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.305718 | `azmcp_kusto_query` | ❌ |

---

## Test 79

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.381403 | `azmcp_kusto_query` | ✅ **EXPECTED** |
| 2 | 0.363521 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.363215 | `azmcp_kusto_sample` | ❌ |
| 4 | 0.348944 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.345734 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.334702 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.328591 | `azmcp_search_service_list` | ❌ |
| 8 | 0.328069 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.324706 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.319029 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.318842 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.314949 | `azmcp_monitor_table_type_list` | ❌ |
| 13 | 0.314848 | `azmcp_search_index_query` | ❌ |
| 14 | 0.308024 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.303973 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.302865 | `azmcp_postgres_table_list` | ❌ |
| 17 | 0.292045 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.263954 | `azmcp_grafana_list` | ❌ |
| 19 | 0.263052 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.257428 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 80

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
| 11 | 0.341725 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.337281 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.319255 | `azmcp_kusto_query` | ❌ |
| 14 | 0.318189 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.310196 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.285941 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.282651 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.267695 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.249424 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.242112 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 81

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591668 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585237 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.556724 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.549940 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.521516 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.520802 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.475496 | `azmcp_postgres_database_list` | ❌ |
| 8 | 0.464341 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.449656 | `azmcp_kusto_table_schema` | ❌ |
| 10 | 0.436518 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.433775 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.429278 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.412275 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.410425 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.400099 | `azmcp_mysql_table_schema_get` | ❌ |
| 16 | 0.384895 | `azmcp_postgres_table_schema_get` | ❌ |
| 17 | 0.380671 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.337412 | `azmcp_kusto_query` | ❌ |
| 19 | 0.330068 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.329669 | `azmcp_grafana_list` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549885 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.524691 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.523432 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.494108 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.490759 | `azmcp_monitor_table_list` | ❌ |
| 6 | 0.475412 | `azmcp_kusto_database_list` | ❌ |
| 7 | 0.466212 | `azmcp_kusto_table_schema` | ❌ |
| 8 | 0.431964 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.425623 | `azmcp_kusto_sample` | ❌ |
| 10 | 0.421413 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.418153 | `azmcp_mysql_table_schema_get` | ❌ |
| 12 | 0.415682 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.403445 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.402646 | `azmcp_postgres_table_schema_get` | ❌ |
| 15 | 0.391081 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.367187 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.348891 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.330341 | `azmcp_kusto_query` | ❌ |
| 19 | 0.314766 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.300285 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 83

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
| 7 | 0.413924 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.398632 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.387660 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.366346 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.366081 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.358088 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.345263 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.343345 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.340038 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.314580 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.298186 | `azmcp_kusto_query` | ❌ |
| 18 | 0.294840 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.282712 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.275795 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 84

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
| 20 | 0.266185 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 85

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
| 16 | 0.251227 | `azmcp_appservice_database_add` | ❌ |
| 17 | 0.247558 | `azmcp_grafana_list` | ❌ |
| 18 | 0.239544 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.236450 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.236206 | `azmcp_acr_registry_list` | ❌ |

---

## Test 86

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
| 13 | 0.279038 | `azmcp_kusto_query` | ❌ |
| 14 | 0.278067 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.264434 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.257657 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.230415 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.226519 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.225958 | `azmcp_grafana_list` | ❌ |
| 20 | 0.198397 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 87

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
| 10 | 0.374852 | `azmcp_postgres_server_param_get` | ❌ |
| 11 | 0.267903 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.252810 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.238583 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.232680 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.224212 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.215307 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.214473 | `azmcp_appservice_database_add` | ❌ |
| 18 | 0.198883 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.180063 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.169461 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 88

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
| 6 | 0.478541 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.467873 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.458406 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.457318 | `azmcp_grafana_list` | ❌ |
| 10 | 0.451969 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.431642 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.431126 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.422584 | `azmcp_search_service_list` | ❌ |
| 14 | 0.410134 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.403845 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.379322 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.377511 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.374451 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.365389 | `azmcp_group_list` | ❌ |
| 20 | 0.354490 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 89

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
| 16 | 0.241553 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.235455 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.232383 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.224586 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.218115 | `azmcp_acr_registry_list` | ❌ |

---

## Test 90

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.534266 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530210 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.464360 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.458498 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.456616 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.441917 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.431914 | `azmcp_grafana_list` | ❌ |
| 9 | 0.419663 | `azmcp_search_service_list` | ❌ |
| 10 | 0.416021 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.412407 | `azmcp_mysql_database_query` | ❌ |
| 12 | 0.408235 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.399358 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.376596 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.375684 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.364016 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.356691 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.341439 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.341087 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.337333 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 91

**Expected Tool:** `azmcp_mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `azmcp_mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.407671 | `azmcp_mysql_server_param_set` | ❌ |
| 3 | 0.333841 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.313150 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.310782 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.300031 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.296654 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.292546 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.285657 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.285645 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.241196 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.183735 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.160082 | `azmcp_appconfig_kv_list` | ❌ |
| 14 | 0.151037 | `azmcp_keyvault_secret_get` | ❌ |
| 15 | 0.146290 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.124274 | `azmcp_grafana_list` | ❌ |
| 17 | 0.121723 | `azmcp_foundry_agents_connect` | ❌ |
| 18 | 0.120498 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 19 | 0.118505 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.117704 | `azmcp_applens_resource_diagnose` | ❌ |

---

## Test 92

**Expected Tool:** `azmcp_mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.390761 | `azmcp_mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `azmcp_mysql_server_param_get` | ❌ |
| 3 | 0.307508 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.298911 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.277569 | `azmcp_appservice_database_add` | ❌ |
| 6 | 0.254180 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.253189 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.246424 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.246019 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.238742 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.236435 | `azmcp_postgres_server_param_get` | ❌ |
| 12 | 0.140690 | `azmcp_foundry_agents_connect` | ❌ |
| 13 | 0.112499 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.094606 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.090695 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.090334 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.089483 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.088097 | `azmcp_loadtesting_test_create` | ❌ |
| 19 | 0.086308 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 20 | 0.084357 | `azmcp_foundry_agents_evaluate` | ❌ |

---

## Test 93

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
| 8 | 0.429975 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.418510 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.410273 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.401217 | `azmcp_cosmos_database_list` | ❌ |
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

## Test 94

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
| 9 | 0.385166 | `azmcp_postgres_table_schema_get` | ❌ |
| 10 | 0.382111 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.378011 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.349434 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.342926 | `azmcp_kusto_table_schema` | ❌ |
| 14 | 0.319674 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.303999 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.281571 | `azmcp_kusto_sample` | ❌ |
| 17 | 0.236723 | `azmcp_grafana_list` | ❌ |
| 18 | 0.231173 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.225827 | `azmcp_appservice_database_add` | ❌ |
| 20 | 0.214496 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 95

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
| 15 | 0.268084 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.243861 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.239328 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.208768 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.205155 | `azmcp_bicepschema_get` | ❌ |
| 20 | 0.194220 | `azmcp_grafana_list` | ❌ |

---

## Test 96

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815617 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.644014 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.622790 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.542685 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.490955 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.471672 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.453436 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.444410 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.435828 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.418348 | `azmcp_postgres_database_query` | ❌ |
| 11 | 0.414696 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.407877 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.319946 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.293787 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.292441 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.289334 | `azmcp_grafana_list` | ❌ |
| 17 | 0.252438 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.249563 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.245546 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.245216 | `azmcp_group_list` | ❌ |

---

## Test 97

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760033 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589783 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.585891 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.552660 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.495683 | `azmcp_postgres_server_param_get` | ❌ |
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

## Test 98

**Expected Tool:** `azmcp_postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546211 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.503267 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.466599 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.415817 | `azmcp_postgres_database_query` | ✅ **EXPECTED** |
| 5 | 0.403954 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.403924 | `azmcp_postgres_server_config_get` | ❌ |
| 7 | 0.380446 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.361081 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.354336 | `azmcp_postgres_server_param_set` | ❌ |
| 10 | 0.341271 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.264914 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.262356 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.262184 | `azmcp_kusto_query` | ❌ |
| 14 | 0.254174 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.248628 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.244295 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.236363 | `azmcp_grafana_list` | ❌ |
| 18 | 0.218677 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.217855 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.189002 | `azmcp_foundry_models_list` | ❌ |

---

## Test 99

**Expected Tool:** `azmcp_postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `azmcp_postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.599518 | `azmcp_postgres_server_param_get` | ❌ |
| 3 | 0.535284 | `azmcp_postgres_server_param_set` | ❌ |
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
| 16 | 0.189446 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.185547 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.178187 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.177778 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.174917 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 100

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900023 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640733 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.565914 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.538997 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.507714 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.483663 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.472458 | `azmcp_grafana_list` | ❌ |
| 8 | 0.457583 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.453841 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.446608 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.435298 | `azmcp_search_service_list` | ❌ |
| 12 | 0.416315 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.406617 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.399056 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.397428 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.389191 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.373699 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.373641 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.366514 | `azmcp_group_list` | ❌ |
| 20 | 0.362900 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 101

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.607062 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.576349 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.522996 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.506262 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.409406 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.400138 | `azmcp_postgres_server_param_set` | ❌ |
| 8 | 0.372955 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.336934 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.336270 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.274763 | `azmcp_grafana_list` | ❌ |
| 12 | 0.260533 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.253264 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.245276 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.241835 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.239500 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.238645 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.229842 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.227547 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.225295 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 102

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579232 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.531804 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.514445 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.505970 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.452608 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.444127 | `azmcp_grafana_list` | ❌ |
| 8 | 0.430033 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.421577 | `azmcp_search_service_list` | ❌ |
| 10 | 0.414799 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.410719 | `azmcp_postgres_database_query` | ❌ |
| 12 | 0.403538 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.376954 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.367001 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.362650 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.362557 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.360521 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.358409 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.334679 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.334101 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 103

**Expected Tool:** `azmcp_postgres_server_param`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594733 | `azmcp_postgres_server_param_get` | ❌ |
| 2 | 0.539671 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.489693 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.480872 | `azmcp_postgres_server_param_set` | ❌ |
| 5 | 0.451871 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.357606 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.343799 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.330875 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.305351 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.295439 | `azmcp_mysql_server_param_set` | ❌ |
| 11 | 0.185273 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.183435 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.174107 | `azmcp_grafana_list` | ❌ |
| 14 | 0.169190 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.166286 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.158090 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.155785 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.145056 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.142387 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.141137 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |

---

## Test 104

**Expected Tool:** `azmcp_postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488474 | `azmcp_postgres_server_config_get` | ❌ |
| 2 | 0.469794 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.464604 | `azmcp_postgres_server_param_set` | ✅ **EXPECTED** |
| 4 | 0.447026 | `azmcp_postgres_server_param_get` | ❌ |
| 5 | 0.440760 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.354049 | `azmcp_postgres_table_list` | ❌ |
| 7 | 0.341624 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.317484 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.241642 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.227740 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.192554 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.133385 | `azmcp_kusto_sample` | ❌ |
| 13 | 0.127120 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.126411 | `azmcp_foundry_agents_evaluate` | ❌ |
| 15 | 0.123491 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.119027 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.118089 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.114978 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.113841 | `azmcp_grafana_list` | ❌ |
| 20 | 0.112671 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 105

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789463 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.751620 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.575616 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.519015 | `azmcp_postgres_table_schema_get` | ❌ |
| 5 | 0.502362 | `azmcp_postgres_server_config_get` | ❌ |
| 6 | 0.477412 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.447896 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.431911 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.431730 | `azmcp_postgres_server_param_get` | ❌ |
| 10 | 0.397479 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.393714 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.373397 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.351960 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.307529 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.299027 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.257867 | `azmcp_grafana_list` | ❌ |
| 17 | 0.256004 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.248593 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.237511 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.229478 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 106

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
| 8 | 0.447209 | `azmcp_postgres_server_param_get` | ❌ |
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

## Test 107

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
| 7 | 0.443826 | `azmcp_postgres_server_param_get` | ❌ |
| 8 | 0.442553 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.427530 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.406761 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.362711 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.322766 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.303748 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.253563 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.253353 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.239225 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.212206 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.201673 | `azmcp_grafana_list` | ❌ |
| 19 | 0.186195 | `azmcp_bicepschema_get` | ❌ |
| 20 | 0.185124 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 108

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686617 | `azmcp_deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471687 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.404890 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.392565 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.389603 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.354432 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.342314 | `azmcp_monitor_resource_log_query` | ❌ |
| 8 | 0.334992 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.334522 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.333590 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.327028 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.325553 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.320572 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.314964 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.314890 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.312836 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.307231 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.297642 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.294636 | `azmcp_sql_server_list` | ❌ |
| 20 | 0.288973 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 109

**Expected Tool:** `azmcp_deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680640 | `azmcp_deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562505 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.505052 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.497193 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.435921 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.430764 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.417333 | `azmcp_get_bestpractices_get` | ❌ |
| 8 | 0.371155 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.343117 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.322230 | `azmcp_extension_azqr` | ❌ |
| 11 | 0.317900 | `azmcp_foundry_agents_list` | ❌ |
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

## Test 110

**Expected Tool:** `azmcp_deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529092 | `azmcp_deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.402924 | `azmcp_bicepschema_get` | ❌ |
| 3 | 0.391965 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.383210 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.341436 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.304791 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.278653 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.266851 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.266605 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.252977 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.236341 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.223979 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.219521 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.206928 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.202239 | `azmcp_mysql_table_schema_get` | ❌ |
| 16 | 0.201288 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.195422 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.194571 | `azmcp_sql_db_create` | ❌ |
| 19 | 0.188615 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.178628 | `azmcp_storage_blob_get` | ❌ |

---

## Test 111

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `azmcp_deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499243 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.448918 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.382240 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.375202 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.373380 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.350101 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.338440 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.322906 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.297769 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.262768 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.240757 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.234690 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.230063 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.222416 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.219650 | `azmcp_speech_stt_recognize` | ❌ |
| 17 | 0.212123 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.211103 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.203987 | `azmcp_sql_server_delete` | ❌ |
| 20 | 0.198696 | `azmcp_mysql_server_list` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688051 | `azmcp_deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.432825 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.425393 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.421744 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.413718 | `azmcp_loadtesting_test_create` | ❌ |
| 9 | 0.393486 | `azmcp_deploy_app_logs_get` | ❌ |
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

## Test 113

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759178 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.610315 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.545540 | `azmcp_search_service_list` | ❌ |
| 4 | 0.514189 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.496537 | `azmcp_subscription_list` | ❌ |
| 6 | 0.496002 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 7 | 0.493331 | `azmcp_group_list` | ❌ |
| 8 | 0.485584 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.484509 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.475667 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.475056 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.472764 | `azmcp_grafana_list` | ❌ |
| 13 | 0.470347 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.442229 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.440619 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.439820 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.438287 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.427412 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.422452 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.422414 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691068 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.599956 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.478334 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.475119 | `azmcp_search_service_list` | ❌ |
| 5 | 0.450712 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.441607 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.437153 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.431249 | `azmcp_subscription_list` | ❌ |
| 9 | 0.430494 | `azmcp_grafana_list` | ❌ |
| 10 | 0.428529 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.424907 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.420072 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.419979 | `azmcp_group_list` | ❌ |
| 14 | 0.408708 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.399253 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.396758 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.390619 | `azmcp_servicebus_topic_details` | ❌ |
| 18 | 0.384708 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.381698 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.381664 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 115

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759396 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.632794 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.526595 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `azmcp_search_service_list` | ❌ |
| 5 | 0.495814 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.494153 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.482428 | `azmcp_group_list` | ❌ |
| 8 | 0.481065 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.476857 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.476780 | `azmcp_subscription_list` | ❌ |
| 11 | 0.471888 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.468200 | `azmcp_grafana_list` | ❌ |
| 13 | 0.466774 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.445991 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.429646 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.428727 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.425386 | `azmcp_servicebus_topic_details` | ❌ |
| 18 | 0.421430 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.417876 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.392039 | `azmcp_kusto_database_list` | ❌ |

---

## Test 116

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659232 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.618817 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.609977 | `azmcp_group_list` | ❌ |
| 4 | 0.514277 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.505966 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.491433 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.484777 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.475467 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.464233 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.460456 | `azmcp_search_service_list` | ❌ |
| 11 | 0.456540 | `azmcp_grafana_list` | ❌ |
| 12 | 0.455379 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.452988 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.448178 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.442914 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.442259 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.432333 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.423027 | `azmcp_postgres_server_list` | ❌ |
| 19 | 0.411811 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.407927 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 117

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682900 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.637188 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.486216 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.480944 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.478026 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.457868 | `azmcp_search_service_list` | ❌ |
| 7 | 0.445053 | `azmcp_subscription_list` | ❌ |
| 8 | 0.435412 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.434659 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.422093 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.421994 | `azmcp_group_list` | ❌ |
| 12 | 0.417538 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.415300 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.408588 | `azmcp_grafana_list` | ❌ |
| 15 | 0.397665 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.392813 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.382829 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.378136 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.376133 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.367406 | `azmcp_acr_registry_list` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672635 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.656238 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.539901 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.498226 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.460289 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.445036 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.443181 | `azmcp_subscription_list` | ❌ |
| 8 | 0.438134 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435527 | `azmcp_search_service_list` | ❌ |
| 10 | 0.434691 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.433451 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.431792 | `azmcp_grafana_list` | ❌ |
| 13 | 0.428035 | `azmcp_group_list` | ❌ |
| 14 | 0.419085 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.402048 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.398792 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.393003 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.386906 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.376546 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.376232 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 119

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669236 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.663335 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.526186 | `azmcp_group_list` | ❌ |
| 4 | 0.488740 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484167 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 6 | 0.478967 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.474205 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.473544 | `azmcp_servicebus_topic_details` | ❌ |
| 9 | 0.465477 | `azmcp_acr_registry_list` | ❌ |
| 10 | 0.464598 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.462201 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.457114 | `azmcp_search_service_list` | ❌ |
| 13 | 0.452429 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.452191 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.443155 | `azmcp_subscription_list` | ❌ |
| 16 | 0.436651 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.436075 | `azmcp_grafana_list` | ❌ |
| 18 | 0.428724 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.412528 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.409139 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593171 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.592262 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.525017 | `azmcp_subscription_list` | ❌ |
| 4 | 0.518857 | `azmcp_search_service_list` | ❌ |
| 5 | 0.509007 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.490371 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.489687 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.480579 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.476022 | `azmcp_group_list` | ❌ |
| 10 | 0.475289 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.473274 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.467172 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.460683 | `azmcp_grafana_list` | ❌ |
| 14 | 0.451759 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.440684 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.439145 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.422468 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.415047 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.410171 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.399352 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604278 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.600323 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.535955 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.518141 | `azmcp_subscription_list` | ❌ |
| 5 | 0.510875 | `azmcp_group_list` | ❌ |
| 6 | 0.508443 | `azmcp_search_service_list` | ❌ |
| 7 | 0.494659 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.492726 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.485794 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.483568 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.481609 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.481450 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.473868 | `azmcp_grafana_list` | ❌ |
| 14 | 0.467622 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 15 | 0.453352 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.446430 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.428290 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.426897 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.411734 | `azmcp_sql_server_list` | ❌ |
| 20 | 0.410745 | `azmcp_acr_registry_list` | ❌ |

---

## Test 122

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621512 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.558539 | `azmcp_group_list` | ❌ |
| 3 | 0.531313 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.505026 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.502308 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.487257 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.472283 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.467183 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.466908 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.464550 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.459530 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.457119 | `azmcp_grafana_list` | ❌ |
| 13 | 0.440510 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.438267 | `azmcp_subscription_list` | ❌ |
| 15 | 0.438218 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.435258 | `azmcp_search_service_list` | ❌ |
| 17 | 0.431166 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.423121 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.411672 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.411012 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 123

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653955 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.581785 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.480843 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.478294 | `azmcp_subscription_list` | ❌ |
| 5 | 0.476666 | `azmcp_search_service_list` | ❌ |
| 6 | 0.475490 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.471828 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.471284 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.467148 | `azmcp_group_list` | ❌ |
| 10 | 0.450146 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.449871 | `azmcp_grafana_list` | ❌ |
| 12 | 0.447095 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.446627 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.444786 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.437364 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.436647 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.436607 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.425435 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.422507 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.420002 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 124

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.448153 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.390048 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.380314 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.379616 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.376542 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.373215 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352982 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.347628 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.347347 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.342747 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.341455 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.341448 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.338149 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.337621 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 16 | 0.333091 | `azmcp_extension_azqr` | ❌ |
| 17 | 0.328326 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.323897 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.322437 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.317399 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 125

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607276 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447400 | `azmcp_mysql_server_config_get` | ❌ |
| 3 | 0.424693 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.422356 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.407133 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.397977 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.392852 | `azmcp_appconfig_kv_list` | ❌ |
| 8 | 0.384151 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.383957 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.369436 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.367183 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.363406 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.361718 | `azmcp_deploy_plan_get` | ❌ |
| 14 | 0.353601 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.352018 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.342398 | `azmcp_postgres_server_config_get` | ❌ |
| 17 | 0.321697 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.315210 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.314100 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.312611 | `azmcp_sql_db_list` | ❌ |

---

## Test 126

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.460102 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.420161 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.390689 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.334473 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.322197 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.320070 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.317583 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.317359 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.312732 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.311384 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.309942 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.305130 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.297747 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.297135 | `azmcp_aks_nodepool_get` | ❌ |
| 16 | 0.295538 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.295174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.290156 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.281564 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.277653 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 127

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690837 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433952 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.432265 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.424630 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.419317 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.417006 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.396147 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.390776 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.389255 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.387866 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.383836 | `azmcp_sql_server_list` | ❌ |
| 12 | 0.383165 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.378798 | `azmcp_get_bestpractices_get` | ❌ |
| 14 | 0.376021 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.375282 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.368530 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.360127 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.352645 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.348659 | `azmcp_foundry_models_deployments_list` | ❌ |
| 20 | 0.346433 | `azmcp_group_list` | ❌ |

---

## Test 128

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.443406 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.441394 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.391480 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.383893 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.355527 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.353617 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.351282 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.349540 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.347266 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.344702 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.342868 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.337247 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.333000 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.331548 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.325680 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.320825 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.319736 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.318160 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.305803 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 129

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.445114 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.368193 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.366254 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.365569 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.363324 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.358624 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352754 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.351465 | `azmcp_aks_cluster_get` | ❌ |
| 10 | 0.350178 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.349596 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.349013 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.336938 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.335848 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.327763 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.325909 | `azmcp_workbooks_show` | ❌ |
| 17 | 0.325899 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.323655 | `azmcp_foundry_models_deployments_list` | ❌ |
| 19 | 0.323377 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.319946 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 130

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433961 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.388678 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.370242 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.368212 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.368018 | `azmcp_loadtesting_testrun_get` | ❌ |
| 7 | 0.367564 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.355983 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.354877 | `azmcp_search_index_get` | ❌ |
| 10 | 0.349891 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.349476 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.346974 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.344067 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.343381 | `azmcp_get_bestpractices_get` | ❌ |
| 15 | 0.342238 | `azmcp_servicebus_queue_details` | ❌ |
| 16 | 0.338127 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.337614 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.334256 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.330450 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.326091 | `azmcp_quota_usage_check` | ❌ |

---

## Test 131

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554980 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426703 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.418330 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.407981 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.381629 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.364785 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.350663 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.335606 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.325271 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.321466 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.318517 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.313774 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.306310 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.304263 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.301769 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.281353 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.277967 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.267215 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.267170 | `azmcp_search_service_list` | ❌ |
| 20 | 0.266297 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 132

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565797 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.440329 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.422751 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.384131 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.342552 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.333621 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.319464 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.318076 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.310636 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.298434 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.297102 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.295667 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.292793 | `azmcp_cloudarchitect_design` | ❌ |
| 14 | 0.283653 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.272348 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.270846 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.267009 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.266071 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.258431 | `azmcp_search_service_list` | ❌ |
| 20 | 0.241875 | `azmcp_sql_db_list` | ❌ |

---

## Test 133

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
| 7 | 0.465668 | `azmcp_group_list` | ❌ |
| 8 | 0.464534 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.462149 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.455819 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.455388 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.445115 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.442614 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.433245 | `azmcp_eventgrid_topic_list` | ❌ |
| 15 | 0.432144 | `azmcp_grafana_list` | ❌ |
| 16 | 0.431560 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.429253 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.417070 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.413034 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.411904 | `azmcp_sql_db_list` | ❌ |

---

## Test 134

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.452147 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.436115 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.412646 | `azmcp_search_service_list` | ❌ |
| 5 | 0.411323 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.385832 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.374655 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.372790 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.370393 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.369681 | `azmcp_subscription_list` | ❌ |
| 11 | 0.368004 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.358699 | `azmcp_deploy_plan_get` | ❌ |
| 13 | 0.357329 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.347887 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.347802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.339873 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.334019 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.333136 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.327870 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.327326 | `azmcp_sql_server_list` | ❌ |

---

## Test 135

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433674 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.348087 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.284362 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.281676 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.249658 | `azmcp_appconfig_account_list` | ❌ |
| 6 | 0.244782 | `azmcp_appconfig_kv_list` | ❌ |
| 7 | 0.240729 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.239514 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.217775 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.214031 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.207391 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.197655 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.195857 | `azmcp_role_assignment_list` | ❌ |
| 14 | 0.194503 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.186046 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.184120 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.184017 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.179069 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.178961 | `azmcp_search_service_list` | ❌ |
| 20 | 0.175281 | `azmcp_subscription_list` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.740327 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.595854 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.590531 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.576087 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.543043 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.526739 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.434682 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.416078 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.414022 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.380350 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.372046 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.352953 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.296644 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.285184 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.267718 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.237081 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.223027 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.219479 | `azmcp_subscription_list` | ❌ |
| 19 | 0.217086 | `azmcp_search_service_list` | ❌ |
| 20 | 0.208862 | `azmcp_workbooks_delete` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627979 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.624690 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.565005 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.539603 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.515698 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.493432 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.482934 | `azmcp_keyvault_secret_get` | ❌ |
| 8 | 0.475385 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.423728 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.418861 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.359751 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.319204 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.293421 | `azmcp_subscription_list` | ❌ |
| 14 | 0.289685 | `azmcp_search_service_list` | ❌ |
| 15 | 0.279383 | `azmcp_search_index_get` | ❌ |
| 16 | 0.276581 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.271911 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.269713 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.266478 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.263141 | `azmcp_storage_account_create` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662324 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.606657 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.574902 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.540203 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.535157 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.499272 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.482380 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.481239 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.459167 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.419011 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.415722 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.412434 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.367899 | `azmcp_search_index_get` | ❌ |
| 14 | 0.365366 | `azmcp_sql_db_show` | ❌ |
| 15 | 0.350544 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.332770 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.331645 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.317884 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.305778 | `azmcp_subscription_list` | ❌ |
| 20 | 0.301710 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650078 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.521183 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.469706 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.467364 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.426600 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.398035 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.386013 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.364868 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.354768 | `azmcp_keyvault_secret_get` | ❌ |
| 10 | 0.337967 | `azmcp_keyvault_secret_list` | ❌ |
| 11 | 0.248212 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.231569 | `azmcp_speech_stt_recognize` | ❌ |
| 13 | 0.228508 | `azmcp_workbooks_delete` | ❌ |
| 14 | 0.222971 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.205023 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.182121 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.180219 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.174570 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.170326 | `azmcp_subscription_list` | ❌ |
| 20 | 0.158491 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649720 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.629902 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.527564 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.525743 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.491898 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.472232 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.405942 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.399857 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.377865 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.371386 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.259893 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.256832 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.250432 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.233767 | `azmcp_workbooks_delete` | ❌ |
| 15 | 0.211707 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.209234 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.203658 | `azmcp_sql_server_create` | ❌ |
| 18 | 0.197520 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.196937 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.189634 | `azmcp_sql_server_delete` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.762171 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.637437 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.608799 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.566460 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.539624 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.484713 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.484259 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.478100 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.453226 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.437257 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.408042 | `azmcp_subscription_list` | ❌ |
| 12 | 0.394434 | `azmcp_search_service_list` | ❌ |
| 13 | 0.393940 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.363524 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.362873 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.358938 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.350862 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.339829 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 19 | 0.336779 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 20 | 0.330779 | `azmcp_redis_cache_list` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660711 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.570282 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.540050 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.516722 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.509123 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.496839 | `azmcp_keyvault_key_get` | ❌ |
| 7 | 0.483447 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.458551 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.420506 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.397031 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.396055 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.362782 | `azmcp_subscription_list` | ❌ |
| 13 | 0.355602 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.344466 | `azmcp_search_service_list` | ❌ |
| 15 | 0.323177 | `azmcp_role_assignment_list` | ❌ |
| 16 | 0.309942 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.305651 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.295917 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.290437 | `azmcp_search_index_get` | ❌ |
| 20 | 0.286742 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676352 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.569250 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.555829 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.465742 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.457982 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.417779 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.413161 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.412664 | `azmcp_keyvault_certificate_import` | ❌ |
| 9 | 0.397141 | `azmcp_appconfig_kv_set` | ❌ |
| 10 | 0.389769 | `azmcp_keyvault_certificate_get` | ❌ |
| 11 | 0.372042 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.338097 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.283851 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.276139 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.261794 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.230400 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.223719 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.215837 | `azmcp_subscription_list` | ❌ |
| 19 | 0.211818 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.199592 | `azmcp_sql_server_firewall-rule_create` | ❌ |

---

## Test 144

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586288 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.554944 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.509196 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.501027 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.487292 | `azmcp_keyvault_certificate_list` | ❌ |
| 6 | 0.486385 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.484379 | `azmcp_keyvault_certificate_get` | ❌ |
| 8 | 0.439610 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.432344 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.417807 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.379569 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.329333 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.305753 | `azmcp_subscription_list` | ❌ |
| 14 | 0.281009 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.279084 | `azmcp_search_index_get` | ❌ |
| 16 | 0.276633 | `azmcp_search_service_list` | ❌ |
| 17 | 0.274078 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.268770 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.267669 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.256231 | `azmcp_quota_usage_check` | ❌ |

---

## Test 145

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636412 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.545320 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.535190 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.504336 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.499184 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.479103 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.474975 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.470315 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.452303 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.429501 | `azmcp_keyvault_certificate_import` | ❌ |
| 11 | 0.429501 | `azmcp_storage_blob_container_get` | ❌ |
| 12 | 0.427301 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.368246 | `azmcp_search_index_get` | ❌ |
| 14 | 0.346630 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.340632 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.337184 | `azmcp_servicebus_queue_details` | ❌ |
| 17 | 0.326292 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.315942 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.315888 | `azmcp_subscription_list` | ❌ |
| 20 | 0.311473 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737135 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.650155 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.631933 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.531076 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.498767 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.468044 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.467326 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.463654 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.455805 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.443785 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.430322 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.426909 | `azmcp_subscription_list` | ❌ |
| 13 | 0.408341 | `azmcp_search_service_list` | ❌ |
| 14 | 0.388040 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.373903 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.368258 | `azmcp_mysql_database_list` | ❌ |
| 17 | 0.354818 | `azmcp_monitor_table_list` | ❌ |
| 18 | 0.353748 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.350154 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.348597 | `azmcp_role_assignment_list` | ❌ |

---

## Test 147

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611900 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.564132 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.537271 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.522262 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.501697 | `azmcp_keyvault_secret_get` | ❌ |
| 6 | 0.481066 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.463471 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.430403 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.423504 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.413074 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.409499 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.359624 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.355246 | `azmcp_subscription_list` | ❌ |
| 14 | 0.327816 | `azmcp_search_service_list` | ❌ |
| 15 | 0.318368 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.310390 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.308595 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.299052 | `azmcp_search_index_get` | ❌ |
| 19 | 0.298268 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.293627 | `azmcp_mysql_database_list` | ❌ |

---

## Test 148

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.767701 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.613514 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.572297 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.531632 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.516457 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.461437 | `azmcp_appconfig_kv_set` | ❌ |
| 7 | 0.423937 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.417525 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.411502 | `azmcp_keyvault_certificate_import` | ❌ |
| 10 | 0.391024 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.387507 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 12 | 0.357221 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.288052 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.287955 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.287066 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.246174 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.243438 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.218660 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 19 | 0.212873 | `azmcp_storage_blob_upload` | ❌ |
| 20 | 0.209815 | `azmcp_subscription_list` | ❌ |

---

## Test 149

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618959 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.578206 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.549693 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.514831 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.482166 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.456701 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.443266 | `azmcp_keyvault_certificate_list` | ❌ |
| 8 | 0.423151 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.420938 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.398060 | `azmcp_keyvault_certificate_import` | ❌ |
| 11 | 0.382573 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.348599 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.301544 | `azmcp_subscription_list` | ❌ |
| 14 | 0.294689 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.283875 | `azmcp_search_index_get` | ❌ |
| 16 | 0.281795 | `azmcp_search_service_list` | ❌ |
| 17 | 0.260730 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.257699 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.255278 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.254353 | `azmcp_sql_db_show` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607481 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.583025 | `azmcp_keyvault_secret_list` | ❌ |
| 3 | 0.564277 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.531971 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.503182 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.485180 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.483567 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.445445 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.444855 | `azmcp_keyvault_certificate_list` | ❌ |
| 10 | 0.436761 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.413731 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.408665 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.378273 | `azmcp_search_index_get` | ❌ |
| 14 | 0.354770 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.346818 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.335079 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.333928 | `azmcp_servicebus_queue_details` | ❌ |
| 18 | 0.324284 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.321621 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.311552 | `azmcp_subscription_list` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747343 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.617131 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.570429 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.536333 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.519133 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.473530 | `azmcp_keyvault_key_get` | ❌ |
| 7 | 0.455500 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.433185 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.417973 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.414310 | `azmcp_keyvault_certificate_get` | ❌ |
| 11 | 0.391082 | `azmcp_subscription_list` | ❌ |
| 12 | 0.388773 | `azmcp_search_service_list` | ❌ |
| 13 | 0.387663 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.367395 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.340472 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 16 | 0.337595 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.334206 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.331203 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.326456 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.322084 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615400 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.577880 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.520825 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.520654 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.502403 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.468205 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.456355 | `azmcp_keyvault_certificate_get` | ❌ |
| 8 | 0.411604 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.410957 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.409151 | `azmcp_keyvault_certificate_import` | ❌ |
| 11 | 0.401434 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.371438 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.345256 | `azmcp_subscription_list` | ❌ |
| 14 | 0.328354 | `azmcp_search_service_list` | ❌ |
| 15 | 0.304798 | `azmcp_search_index_get` | ❌ |
| 16 | 0.303769 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.299023 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.294614 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.293826 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.290273 | `azmcp_mysql_database_list` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660878 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.611431 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.579676 | `azmcp_aks_nodepool_get` | ❌ |
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
| 16 | 0.370169 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.369525 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.367841 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.360380 | `azmcp_storage_blob_get` | ❌ |
| 20 | 0.355388 | `azmcp_sql_server_list` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666853 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.589101 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.545820 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.530314 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.508226 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.461482 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.448796 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.428449 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.423023 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.413625 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.408420 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.396636 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.396256 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.390889 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.385261 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.384654 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.382311 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.377272 | `azmcp_storage_blob_get` | ❌ |
| 19 | 0.365778 | `azmcp_search_index_get` | ❌ |
| 20 | 0.362332 | `azmcp_sql_db_list` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566455 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.562528 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.493500 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.485342 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.380009 | `azmcp_mysql_server_config_get` | ❌ |
| 6 | 0.367190 | `azmcp_kusto_cluster_get` | ❌ |
| 7 | 0.348541 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.342347 | `azmcp_loadtesting_test_get` | ❌ |
| 9 | 0.339270 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.334844 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.333832 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.328950 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.314767 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.314220 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.309300 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.298422 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.296656 | `azmcp_postgres_server_config_get` | ❌ |
| 18 | 0.296219 | `azmcp_sql_db_update` | ❌ |
| 19 | 0.288502 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.275426 | `azmcp_sql_db_show` | ❌ |

---

## Test 156

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661431 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.578662 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.563549 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.534089 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.503925 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.434587 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.433913 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.419340 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.418518 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.417854 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.405658 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.404641 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.402335 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.398818 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.391717 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.384782 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.383956 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.372812 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367545 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.359877 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 157

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
| 6 | 0.560871 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.543684 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.515922 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.509202 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.502401 | `azmcp_subscription_list` | ❌ |
| 11 | 0.498507 | `azmcp_group_list` | ❌ |
| 12 | 0.498286 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.495977 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.486232 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.483592 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.482355 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.481469 | `azmcp_grafana_list` | ❌ |
| 18 | 0.457949 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.452770 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.452682 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607919 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.536337 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.500781 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.492751 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.455152 | `azmcp_search_service_list` | ❌ |
| 6 | 0.446137 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.428375 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.416325 | `azmcp_aks_nodepool_get` | ❌ |
| 9 | 0.409567 | `azmcp_kusto_cluster_get` | ❌ |
| 10 | 0.408301 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.392953 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.376307 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.371759 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.371426 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.371015 | `azmcp_search_index_get` | ❌ |
| 16 | 0.363713 | `azmcp_subscription_list` | ❌ |
| 17 | 0.361897 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.357832 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.356812 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.355892 | `azmcp_storage_account_get` | ❌ |

---

## Test 159

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623896 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.538749 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.530027 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.466749 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.449602 | `azmcp_kusto_cluster_list` | ❌ |
| 6 | 0.416564 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.392083 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.379483 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.378826 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.377567 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.374585 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.363979 | `azmcp_deploy_app_logs_get` | ❌ |
| 13 | 0.353365 | `azmcp_search_service_list` | ❌ |
| 14 | 0.345286 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.345241 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.337354 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.317977 | `azmcp_sql_elastic-pool_list` | ❌ |
| 18 | 0.317238 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.312354 | `azmcp_subscription_list` | ❌ |
| 20 | 0.311971 | `azmcp_quota_usage_check` | ❌ |

---

## Test 160

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.753920 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.699423 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.597314 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.498592 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.482683 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.468392 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.463192 | `azmcp_sql_elastic-pool_list` | ❌ |
| 8 | 0.434881 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.416083 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 10 | 0.401610 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.399215 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.384453 | `azmcp_keyvault_key_get` | ❌ |
| 13 | 0.383565 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 14 | 0.382352 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.379523 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.378347 | `azmcp_search_index_get` | ❌ |
| 17 | 0.378266 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.370172 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.362512 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.356766 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 161

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678158 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.640096 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.481316 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.458596 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.446020 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.440182 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.391067 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 8 | 0.384600 | `azmcp_loadtesting_test_get` | ❌ |
| 9 | 0.371847 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.367455 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.365231 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.357721 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.350998 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.350992 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.344836 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.343726 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.342564 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.338364 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.332492 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.322685 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 162

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599506 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.582325 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.412112 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.391590 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.385173 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 6 | 0.383045 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.346262 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 8 | 0.339934 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.323057 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.320733 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.314439 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.313226 | `azmcp_sql_server_list` | ❌ |
| 13 | 0.306678 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.306579 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.300123 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.298866 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.289393 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.287084 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.283171 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.276058 | `azmcp_sql_db_list` | ❌ |

---

## Test 163

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
| 6 | 0.461702 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.446699 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.440646 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.438637 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.435177 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.431369 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.418852 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 13 | 0.413062 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.407783 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.404890 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.399249 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.393723 | `azmcp_group_list` | ❌ |
| 18 | 0.391869 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.389071 | `azmcp_redis_cluster_database_list` | ❌ |
| 20 | 0.385498 | `azmcp_workbooks_list` | ❌ |

---

## Test 164

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
| 6 | 0.497970 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.447545 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.442122 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.441482 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.433138 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.430830 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.430739 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.421390 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.408990 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.408567 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.407619 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.390197 | `azmcp_redis_cluster_database_list` | ❌ |
| 18 | 0.388911 | `azmcp_group_list` | ❌ |
| 19 | 0.387600 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.383234 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 165

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623138 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.580535 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.453744 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.443902 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.425448 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.409291 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.386949 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.378905 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.368944 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.363262 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.360005 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 12 | 0.356345 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.356139 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.354542 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.348047 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.335285 | `azmcp_sql_server_list` | ❌ |
| 17 | 0.329036 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.324552 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.324283 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.323568 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 166

**Expected Tool:** `azmcp_loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585388 | `azmcp_loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.531336 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.508690 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.413857 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.394664 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.390081 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.346526 | `azmcp_loadtesting_testrun_update` | ❌ |
| 8 | 0.338668 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.338173 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.337450 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.323519 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.310466 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.310144 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.299453 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.296982 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.290957 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.288940 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.280434 | `azmcp_sql_server_create` | ❌ |
| 19 | 0.273769 | `azmcp_sql_server_list` | ❌ |
| 20 | 0.267790 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 167

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642420 | `azmcp_loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.608881 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.574084 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.534194 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.473323 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.469876 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.436995 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.405182 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.397033 | `azmcp_group_list` | ❌ |
| 10 | 0.379377 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.373229 | `azmcp_loadtesting_testrun_update` | ❌ |
| 12 | 0.370024 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.365143 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.360732 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.354531 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.347100 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 17 | 0.341360 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.329323 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.328293 | `azmcp_monitor_metrics_query` | ❌ |
| 20 | 0.322886 | `azmcp_quota_usage_check` | ❌ |

---

## Test 168

**Expected Tool:** `azmcp_loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.717429 | `azmcp_loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.596828 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.514437 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.476662 | `azmcp_loadtesting_testrun_create` | ❌ |
| 5 | 0.443117 | `azmcp_loadtesting_test_get` | ❌ |
| 6 | 0.442167 | `azmcp_workbooks_create` | ❌ |
| 7 | 0.416497 | `azmcp_group_list` | ❌ |
| 8 | 0.407752 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.394967 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.382762 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.370093 | `azmcp_loadtesting_testrun_get` | ❌ |
| 12 | 0.369786 | `azmcp_sql_server_list` | ❌ |
| 13 | 0.369344 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.356801 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.350916 | `azmcp_loadtesting_testrun_update` | ❌ |
| 16 | 0.343649 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.342213 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.335696 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.326824 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.326617 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 169

**Expected Tool:** `azmcp_loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738027 | `azmcp_loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.591551 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.576966 | `azmcp_group_list` | ❌ |
| 4 | 0.565565 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.561537 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.526378 | `azmcp_workbooks_list` | ❌ |
| 7 | 0.515624 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.511621 | `azmcp_redis_cache_list` | ❌ |
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

## Test 170

**Expected Tool:** `azmcp_loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621803 | `azmcp_loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.592916 | `azmcp_loadtesting_testresource_create` | ❌ |
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
| 14 | 0.323772 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.306765 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.273429 | `azmcp_sql_server_list` | ❌ |
| 17 | 0.272107 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.267527 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.262297 | `azmcp_storage_blob_container_create` | ❌ |
| 20 | 0.256004 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 171

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625332 | `azmcp_loadtesting_test_get` | ❌ |
| 2 | 0.603066 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.568405 | `azmcp_loadtesting_testrun_get` | ✅ **EXPECTED** |
| 4 | 0.561729 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.535183 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.496655 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.434255 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.415438 | `azmcp_loadtesting_testrun_update` | ❌ |
| 9 | 0.397496 | `azmcp_group_list` | ❌ |
| 10 | 0.395146 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.370196 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.366579 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.355819 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.353650 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.352984 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.346995 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.330457 | `azmcp_monitor_metrics_query` | ❌ |
| 18 | 0.329537 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.328823 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.315577 | `azmcp_quota_usage_check` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.616105 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.605492 | `azmcp_loadtesting_test_get` | ❌ |
| 3 | 0.568915 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.565114 | `azmcp_loadtesting_testrun_list` | ✅ **EXPECTED** |
| 5 | 0.534931 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.492611 | `azmcp_loadtesting_testrun_create` | ❌ |
| 7 | 0.432298 | `azmcp_group_list` | ❌ |
| 8 | 0.416206 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.411030 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.406289 | `azmcp_loadtesting_test_create` | ❌ |
| 11 | 0.396071 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.392100 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.391120 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.375222 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.374051 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.357081 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.342229 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.340745 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.329900 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.328226 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 173

**Expected Tool:** `azmcp_loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659812 | `azmcp_loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.509199 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.454745 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.443828 | `azmcp_loadtesting_test_get` | ❌ |
| 5 | 0.422155 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.399536 | `azmcp_loadtesting_test_create` | ❌ |
| 7 | 0.384654 | `azmcp_loadtesting_testresource_list` | ❌ |
| 8 | 0.384237 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.332544 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.320124 | `azmcp_workbooks_update` | ❌ |
| 11 | 0.300023 | `azmcp_workbooks_create` | ❌ |
| 12 | 0.268172 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.267137 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.255413 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.253250 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.252149 | `azmcp_sql_server_list` | ❌ |
| 17 | 0.250292 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.240916 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.233666 | `azmcp_monitor_metrics_query` | ❌ |
| 20 | 0.232548 | `azmcp_sql_db_show` | ❌ |

---

## Test 174

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
| 8 | 0.492210 | `azmcp_subscription_list` | ❌ |
| 9 | 0.491740 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.489846 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.482846 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.479584 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.459138 | `azmcp_eventgrid_topic_list` | ❌ |
| 14 | 0.457845 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.452178 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.447752 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.447597 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.441426 | `azmcp_group_list` | ❌ |
| 19 | 0.440392 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.436802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 175

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.516886 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.513156 | `azmcp_search_service_list` | ❌ |
| 5 | 0.507981 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.500471 | `azmcp_subscription_list` | ❌ |
| 7 | 0.499290 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.480850 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.477164 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.472811 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.460936 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.460379 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.451887 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.450971 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.448426 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.447269 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.445430 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.442506 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.438952 | `azmcp_grafana_list` | ❌ |
| 20 | 0.437939 | `azmcp_postgres_server_list` | ❌ |

---

## Test 176

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
| 6 | 0.477847 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.466563 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.452905 | `azmcp_acr_registry_list` | ❌ |
| 9 | 0.443767 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.441223 | `azmcp_group_list` | ❌ |
| 11 | 0.433559 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.412747 | `azmcp_search_service_list` | ❌ |
| 13 | 0.412709 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.409044 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.407704 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.406511 | `azmcp_mysql_database_list` | ❌ |
| 17 | 0.402926 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.398398 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.398168 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.397222 | `azmcp_functionapp_get` | ❌ |

---

## Test 177

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_required-subnet-size`  
**Prompt:** Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647272 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ✅ **EXPECTED** |
| 2 | 0.450342 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.327359 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.235376 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.204654 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.204313 | `azmcp_aks_nodepool_get` | ❌ |
| 7 | 0.203596 | `azmcp_quota_usage_check` | ❌ |
| 8 | 0.198992 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.192371 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.188378 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.186811 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.176407 | `azmcp_marketplace_product_get` | ❌ |
| 13 | 0.175917 | `azmcp_postgres_server_param_get` | ❌ |
| 14 | 0.174849 | `azmcp_aks_nodepool_list` | ❌ |
| 15 | 0.172924 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 16 | 0.170883 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.169792 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.166628 | `azmcp_applens_resource_diagnose` | ❌ |
| 19 | 0.165340 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.165216 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 178

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
| 6 | 0.418701 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.412360 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 8 | 0.411221 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.405913 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.403218 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.402635 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.401663 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.401538 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.399919 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.398741 | `azmcp_subscription_list` | ❌ |
| 16 | 0.398576 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.395033 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.393969 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.393471 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.392601 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 179

**Expected Tool:** `azmcp_marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570145 | `azmcp_marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.477522 | `azmcp_marketplace_product_list` | ❌ |
| 3 | 0.353256 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.330935 | `azmcp_servicebus_queue_details` | ❌ |
| 5 | 0.323812 | `azmcp_search_index_get` | ❌ |
| 6 | 0.323800 | `azmcp_servicebus_topic_details` | ❌ |
| 7 | 0.317373 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.302358 | `azmcp_aks_cluster_get` | ❌ |
| 9 | 0.294283 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.289354 | `azmcp_workbooks_show` | ❌ |
| 11 | 0.285577 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.283554 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.276826 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.274420 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.266271 | `azmcp_foundry_models_list` | ❌ |
| 16 | 0.259116 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.257285 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.254139 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 19 | 0.253946 | `azmcp_keyvault_key_get` | ❌ |
| 20 | 0.251014 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 180

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527077 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443133 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.343549 | `azmcp_search_service_list` | ❌ |
| 4 | 0.330500 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.328676 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 6 | 0.324866 | `azmcp_search_index_query` | ❌ |
| 7 | 0.302488 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.290877 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.289603 | `azmcp_search_index_get` | ❌ |
| 10 | 0.287924 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.263954 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.263529 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.258243 | `azmcp_foundry_models_deployments_list` | ❌ |
| 14 | 0.254438 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.251540 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.250343 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.248822 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 18 | 0.247656 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.245634 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.245271 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 181

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461616 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385167 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.308769 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.260387 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.259364 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.238760 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.238238 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.237988 | `azmcp_grafana_list` | ❌ |
| 9 | 0.226689 | `azmcp_search_service_list` | ❌ |
| 10 | 0.221138 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.218863 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.208553 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.204870 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.204011 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.203695 | `azmcp_eventgrid_topic_list` | ❌ |
| 16 | 0.202604 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.202430 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.201780 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 19 | 0.187594 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.185423 | `azmcp_subscription_list` | ❌ |

---

## Test 182

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.635406 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.531727 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490172 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.447777 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.438801 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.378611 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 9 | 0.354191 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.353408 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.351664 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.322785 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.312391 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.312077 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.292039 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.290398 | `azmcp_search_service_list` | ❌ |
| 17 | 0.282195 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.276297 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.273623 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.273557 | `azmcp_storage_account_get` | ❌ |

---

## Test 183

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.548542 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.541091 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.516813 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.516443 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.424443 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.424017 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.409787 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.392162 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369205 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.356238 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.342487 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.306627 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.305711 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.304596 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.304195 | `azmcp_search_service_list` | ❌ |
| 17 | 0.302423 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.302015 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.296142 | `azmcp_sql_db_create` | ❌ |
| 20 | 0.291071 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 184

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
| 6 | 0.430552 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.399433 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.392767 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.384118 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.380323 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.375863 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.362669 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.335296 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.331230 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.329342 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.322718 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.322581 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.317765 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.316790 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.314841 | `azmcp_search_service_list` | ❌ |

---

## Test 185

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.570488 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.522998 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.445321 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.400447 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.381822 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.368218 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.367714 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.352933 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 11 | 0.317494 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.292977 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.284617 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.278941 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.275342 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.256382 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.252529 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.241745 | `azmcp_search_index_query` | ❌ |
| 19 | 0.239964 | `azmcp_storage_blob_get` | ❌ |
| 20 | 0.239436 | `azmcp_search_service_list` | ❌ |

---

## Test 186

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581732 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.497346 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495596 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.486817 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.474486 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.439199 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.412034 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.399532 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.377802 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.373509 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.323207 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.317948 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.303599 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290696 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.277959 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.276240 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.275810 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.270398 | `azmcp_search_service_list` | ❌ |
| 19 | 0.269439 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.269115 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 187

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610923 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.532720 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.487254 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.458020 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.413192 | `azmcp_functionapp_get` | ❌ |
| 6 | 0.396018 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.394713 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.394096 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.375709 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.363569 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.332561 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.331993 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.307816 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290892 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.289367 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.289326 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.284961 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.284192 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.275553 | `azmcp_search_index_query` | ❌ |
| 20 | 0.275210 | `azmcp_storage_blob_get` | ❌ |

---

## Test 188

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.513262 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.505123 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.405182 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.401147 | `azmcp_deploy_plan_get` | ❌ |
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
| 18 | 0.255065 | `azmcp_storage_blob_get` | ❌ |
| 19 | 0.253397 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.251387 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 189

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582531 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.500323 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.472034 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.433124 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.385974 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.381160 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.374669 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.368767 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.358712 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.336987 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.293797 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.288810 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.259711 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.252974 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.251208 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.249918 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.246303 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.240288 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.231224 | `azmcp_search_index_query` | ❌ |
| 20 | 0.231115 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 190

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a simple HTTP-triggered function app in javascript that returns a random compliment from a predefined list in a JSON response. And deploy it to azure eventually. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429159 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.408233 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.380754 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.377184 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.352369 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.345059 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.319970 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.311848 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.301028 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.299104 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.235579 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.232320 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.218912 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.215940 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.215348 | `azmcp_speech_stt_recognize` | ❌ |
| 16 | 0.210908 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.206254 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.203401 | `azmcp_search_index_query` | ❌ |
| 19 | 0.202251 | `azmcp_storage_account_create` | ❌ |
| 20 | 0.197959 | `azmcp_mysql_database_query` | ❌ |

---

## Test 191

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497298 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.493164 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.405180 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.395651 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.385151 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.374207 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.354493 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.348150 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.300079 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.284039 | `azmcp_storage_blob_container_create` | ❌ |
| 11 | 0.266978 | `azmcp_foundry_models_deploy` | ❌ |
| 12 | 0.248967 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.243588 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.234780 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.221811 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.218616 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.210672 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.209187 | `azmcp_workbooks_create` | ❌ |
| 19 | 0.208800 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.195436 | `azmcp_sql_server_create` | ❌ |

---

## Test 192

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498345 | `azmcp_monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.472094 | `azmcp_monitor_workspace_list` | ❌ |
| 3 | 0.467848 | `azmcp_monitor_workspace_log_query` | ❌ |
| 4 | 0.467559 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.463168 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.436945 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.418816 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.413357 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.401910 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.385416 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.380121 | `azmcp_grafana_list` | ❌ |
| 12 | 0.358420 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.342873 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.339326 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.333342 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.314296 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.305738 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.302983 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.297767 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.296719 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 193

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592640 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424062 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.332356 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.315519 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.315321 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.311108 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 7 | 0.305464 | `azmcp_servicebus_queue_details` | ❌ |
| 8 | 0.304735 | `azmcp_grafana_list` | ❌ |
| 9 | 0.303453 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.298853 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.294124 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.287300 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.284526 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.277566 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.274784 | `azmcp_loadtesting_test_get` | ❌ |
| 16 | 0.262141 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.256867 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 18 | 0.254848 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.249161 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.247288 | `azmcp_bicepschema_get` | ❌ |

---

## Test 194

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589859 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.587736 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.551559 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.473421 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.473026 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.459829 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.439032 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.437739 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.431109 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.417065 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.414488 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.403921 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.401850 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.397526 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.391340 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.390422 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.383412 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.378187 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.371137 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.359476 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 195

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633173 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495323 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.382374 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.380460 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.370848 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.359089 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.353235 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.344326 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.341713 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.338702 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.329534 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.326711 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.324002 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.321732 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 15 | 0.317475 | `azmcp_monitor_workspace_log_query` | ❌ |
| 16 | 0.302558 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.301966 | `azmcp_workbooks_show` | ❌ |
| 18 | 0.291565 | `azmcp_cloudarchitect_design` | ❌ |
| 19 | 0.291362 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.287764 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 196

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555272 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.448085 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.447192 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.433777 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.422404 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.416100 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.413340 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.409187 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.388205 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.380032 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.356549 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.350085 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.341791 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.339771 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.335430 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.329493 | `azmcp_loadtesting_testresource_create` | ❌ |
| 17 | 0.326924 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.326802 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.326398 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.320852 | `azmcp_search_index_query` | ❌ |

---

## Test 197

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557692 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.508674 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.460552 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.438233 | `azmcp_monitor_metrics_definitions` | ❌ |
| 6 | 0.392538 | `azmcp_monitor_resource_log_query` | ❌ |
| 7 | 0.391670 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.373042 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.368589 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.355141 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 11 | 0.339388 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.336627 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.326899 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.326643 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.321538 | `azmcp_search_service_list` | ❌ |
| 16 | 0.321227 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.318196 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.317565 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.303909 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.303638 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 198

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461120 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390020 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.306276 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.304336 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.302394 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.289427 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.275441 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.267635 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.267346 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 10 | 0.265738 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.263744 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.263357 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.259165 | `azmcp_grafana_list` | ❌ |
| 14 | 0.253566 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.248808 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.247870 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.247605 | `azmcp_applens_resource_diagnose` | ❌ |
| 18 | 0.242224 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.235643 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.229100 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 199

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492021 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.417008 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.416443 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.406200 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.399055 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.397335 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.369920 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.366959 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.362030 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.359305 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.331730 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.316302 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.315326 | `azmcp_functionapp_get` | ❌ |
| 14 | 0.311842 | `azmcp_search_index_query` | ❌ |
| 15 | 0.308767 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.295918 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.293608 | `azmcp_search_service_list` | ❌ |
| 18 | 0.293452 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.288842 | `azmcp_foundry_agents_connect` | ❌ |
| 20 | 0.287126 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 200

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525426 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.384439 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.377055 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.367124 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.299528 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.292938 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 7 | 0.290182 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.277536 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 9 | 0.272554 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.267122 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.266368 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.265513 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.262661 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.262020 | `azmcp_grafana_list` | ❌ |
| 15 | 0.261653 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.248145 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 17 | 0.246495 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.244114 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.242704 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.239379 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 201

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480040 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.381961 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.363412 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.359285 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.350999 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.350491 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.346462 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.331215 | `azmcp_loadtesting_testresource_list` | ❌ |
| 9 | 0.330048 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.328838 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.324932 | `azmcp_search_index_query` | ❌ |
| 12 | 0.319487 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.317459 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.292274 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.290762 | `azmcp_search_service_list` | ❌ |
| 16 | 0.284306 | `azmcp_foundry_agents_connect` | ❌ |
| 17 | 0.282267 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.278491 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.276999 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.265303 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 202

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594068 | `azmcp_monitor_workspace_log_query` | ❌ |
| 2 | 0.580210 | `azmcp_monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.472080 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.469703 | `azmcp_monitor_metrics_query` | ❌ |
| 5 | 0.443468 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.442380 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.392377 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.390022 | `azmcp_grafana_list` | ❌ |
| 9 | 0.366124 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.359099 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.352821 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.345341 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.344781 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.337855 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.320690 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.313775 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 17 | 0.308949 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.307860 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.307107 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.305149 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 203

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850771 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725777 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.620507 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.534886 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.511101 | `azmcp_kusto_table_list` | ❌ |
| 6 | 0.502152 | `azmcp_grafana_list` | ❌ |
| 7 | 0.488548 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.443831 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.420419 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.419873 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.413918 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.409390 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.399827 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.397410 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.396802 | `azmcp_search_service_list` | ❌ |
| 16 | 0.376982 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.375182 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.374952 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.366056 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.365787 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 204

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798147 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.599917 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.497065 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.487237 | `azmcp_grafana_list` | ❌ |
| 6 | 0.466630 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.449407 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.427408 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.413765 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.411590 | `azmcp_kusto_table_schema` | ❌ |
| 11 | 0.403835 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.398753 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.389881 | `azmcp_mysql_database_list` | ❌ |
| 14 | 0.376474 | `azmcp_kusto_sample` | ❌ |
| 15 | 0.376338 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.372955 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.370624 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.347853 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.346151 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.343837 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 205

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881757 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765647 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.569963 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.505326 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.477361 | `azmcp_grafana_list` | ❌ |
| 6 | 0.447807 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.445849 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.418954 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.416667 | `azmcp_kusto_table_schema` | ❌ |
| 10 | 0.412916 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.404692 | `azmcp_monitor_workspace_log_query` | ❌ |
| 12 | 0.404632 | `azmcp_monitor_metrics_definitions` | ❌ |
| 13 | 0.383929 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.380807 | `azmcp_kusto_sample` | ❌ |
| 15 | 0.374417 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.372605 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.370265 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.362202 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.354889 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.351261 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 206

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843138 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736728 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.576731 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.481189 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.475734 | `azmcp_grafana_list` | ❌ |
| 6 | 0.451212 | `azmcp_mysql_table_schema_get` | ❌ |
| 7 | 0.427934 | `azmcp_kusto_table_schema` | ❌ |
| 8 | 0.427153 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.421484 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.406242 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.391308 | `azmcp_kusto_sample` | ❌ |
| 12 | 0.384855 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.376246 | `azmcp_monitor_metrics_definitions` | ❌ |
| 14 | 0.372991 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.370860 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.367567 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.355198 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.348357 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.340101 | `azmcp_foundry_models_list` | ❌ |
| 20 | 0.339804 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 207

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `azmcp_grafana_list` | ❌ |
| 3 | 0.659497 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.600802 | `azmcp_search_service_list` | ❌ |
| 5 | 0.583213 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.530433 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.517493 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.513663 | `azmcp_aks_cluster_list` | ❌ |
| 9 | 0.500434 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.494857 | `azmcp_group_list` | ❌ |
| 11 | 0.493730 | `azmcp_subscription_list` | ❌ |
| 12 | 0.475212 | `azmcp_monitor_workspace_log_query` | ❌ |
| 13 | 0.471758 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.470266 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.467655 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.466748 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.463930 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.460502 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.448201 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.444214 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 208

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.584758 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.531083 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474745 | `azmcp_monitor_workspace_log_query` | ❌ |
| 6 | 0.459825 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.444207 | `azmcp_search_service_list` | ❌ |
| 8 | 0.414136 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.386092 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.383596 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.380834 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.373786 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.371395 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.363276 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.358029 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.354811 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.354276 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.353651 | `azmcp_subscription_list` | ❌ |
| 19 | 0.352809 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.351317 | `azmcp_search_index_get` | ❌ |

---

## Test 209

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `azmcp_grafana_list` | ❌ |
| 3 | 0.579669 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.521316 | `azmcp_monitor_table_type_list` | ❌ |
| 5 | 0.521276 | `azmcp_search_service_list` | ❌ |
| 6 | 0.463378 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.453668 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.439297 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435071 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.428945 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.427183 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.422707 | `azmcp_subscription_list` | ❌ |
| 13 | 0.422379 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.411648 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.411443 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.410082 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.409806 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.404364 | `azmcp_group_list` | ❌ |
| 19 | 0.402600 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.400615 | `azmcp_postgres_server_list` | ❌ |

---

## Test 210

**Expected Tool:** `azmcp_monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591648 | `azmcp_monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.494604 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.485470 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.484144 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.483323 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.427241 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.374937 | `azmcp_monitor_metrics_query` | ❌ |
| 8 | 0.365704 | `azmcp_grafana_list` | ❌ |
| 9 | 0.330182 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.322875 | `azmcp_workbooks_delete` | ❌ |
| 11 | 0.322408 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 12 | 0.315638 | `azmcp_search_service_list` | ❌ |
| 13 | 0.309411 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.299830 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.292089 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.291733 | `azmcp_kusto_query` | ❌ |
| 17 | 0.289755 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.288698 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.287261 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.283294 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 211

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668827 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.434818 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.413136 | `azmcp_monitor_metrics_query` | ❌ |
| 4 | 0.408658 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.401731 | `azmcp_grafana_list` | ❌ |
| 6 | 0.393310 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.386685 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.369805 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.364076 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.356643 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.355415 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.345409 | `azmcp_postgres_database_list` | ❌ |
| 13 | 0.345356 | `azmcp_group_list` | ❌ |
| 14 | 0.330769 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.328923 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.327205 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.306977 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.304097 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.302405 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.296544 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 212

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443428 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.393244 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.374071 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.371017 | `azmcp_grafana_list` | ❌ |
| 6 | 0.370685 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.359274 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.350656 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.343214 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.342468 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.337109 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.320510 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.319496 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.302947 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.289883 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.287390 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.285906 | `azmcp_group_list` | ❌ |
| 18 | 0.274650 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.274601 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.272689 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 213

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `azmcp_quota_usage_check` | ❌ |
| 2 | 0.497434 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.481143 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.476826 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.461996 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 6 | 0.451690 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.440437 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.438387 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.434685 | `azmcp_search_service_list` | ❌ |
| 10 | 0.431096 | `azmcp_deploy_iac_rules_get` | ❌ |
| 11 | 0.423237 | `azmcp_subscription_list` | ❌ |
| 12 | 0.422293 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.417076 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.408023 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.406543 | `azmcp_deploy_plan_get` | ❌ |
| 16 | 0.400363 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.395234 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.391633 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.388980 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.381209 | `azmcp_storage_account_get` | ❌ |

---

## Test 214

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.488377 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.475854 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 5 | 0.473365 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.462743 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 7 | 0.452232 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.448036 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.442021 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.439040 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.426161 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 12 | 0.385771 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.382677 | `azmcp_search_service_list` | ❌ |
| 14 | 0.375770 | `azmcp_subscription_list` | ❌ |
| 15 | 0.375071 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.365859 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.365841 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.360612 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.349469 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.341827 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 215

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.514907 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 4 | 0.504673 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.494846 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.487387 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.481698 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.464304 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.463564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 10 | 0.463172 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.452811 | `azmcp_search_service_list` | ❌ |
| 12 | 0.433938 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.423512 | `azmcp_subscription_list` | ❌ |
| 14 | 0.417396 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.403533 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.398621 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.380268 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.377353 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.376533 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.376262 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 216

**Expected Tool:** `azmcp_quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590878 | `azmcp_quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413274 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.372921 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369855 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.361386 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.349685 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.348742 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.337821 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.331145 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.331074 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.328408 | `azmcp_grafana_list` | ❌ |
| 12 | 0.325796 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.313240 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.310624 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.307181 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.297387 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.292791 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.290334 | `azmcp_group_list` | ❌ |
| 19 | 0.287104 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.263276 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 217

**Expected Tool:** `azmcp_quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609244 | `azmcp_quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `azmcp_quota_region_availability_list` | ❌ |
| 3 | 0.384339 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383928 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.379006 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.365684 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.358215 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.351637 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.345161 | `azmcp_eventgrid_subscription_list` | ❌ |
| 10 | 0.345156 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.342266 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.342231 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.338636 | `azmcp_grafana_list` | ❌ |
| 14 | 0.331262 | `azmcp_monitor_metrics_definitions` | ❌ |
| 15 | 0.322524 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.321961 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.305083 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.304570 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.300726 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.297684 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 218

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.484839 | `azmcp_group_list` | ❌ |
| 3 | 0.483125 | `azmcp_subscription_list` | ❌ |
| 4 | 0.478700 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474790 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.471364 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.468596 | `azmcp_search_service_list` | ❌ |
| 8 | 0.460029 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.452819 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.446337 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 11 | 0.430667 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.427666 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.426629 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.425029 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.421599 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.409623 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.403310 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.398447 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.397565 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.396961 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 219

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609705 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.456956 | `azmcp_grafana_list` | ❌ |
| 3 | 0.436747 | `azmcp_subscription_list` | ❌ |
| 4 | 0.435669 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.435155 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.431865 | `azmcp_search_service_list` | ❌ |
| 7 | 0.429811 | `azmcp_group_list` | ❌ |
| 8 | 0.428370 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.421637 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.420804 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.415941 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.410368 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 13 | 0.406766 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.395077 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.390202 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.386800 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.383635 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.373204 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.368511 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.363678 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 220

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757000 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.565099 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.444974 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.377493 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.322833 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.312379 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.307358 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.303512 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.299977 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.298715 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.296629 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.292192 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.286412 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.284990 | `azmcp_search_service_list` | ❌ |
| 15 | 0.284880 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.284267 | `azmcp_grafana_list` | ❌ |
| 17 | 0.283749 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.277609 | `azmcp_subscription_list` | ❌ |
| 19 | 0.274857 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.273445 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 221

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713855 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.523280 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.412377 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.338859 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.286321 | `azmcp_appconfig_kv_list` | ❌ |
| 6 | 0.283725 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.280245 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.266314 | `azmcp_storage_blob_container_get` | ❌ |
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
| 19 | 0.246678 | `azmcp_eventgrid_subscription_list` | ❌ |
| 20 | 0.243219 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 222

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.764157 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.653924 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.501880 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.495048 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.472307 | `azmcp_grafana_list` | ❌ |
| 6 | 0.466141 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.464785 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.431968 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.431715 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.423145 | `azmcp_subscription_list` | ❌ |
| 11 | 0.414865 | `azmcp_search_service_list` | ❌ |
| 12 | 0.396295 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.387797 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.381343 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.380443 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.373942 | `azmcp_group_list` | ❌ |
| 17 | 0.368719 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.367794 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.367496 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.364522 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 223

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538003 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.450446 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
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

## Test 224

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692324 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.595721 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.461616 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.434924 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.427325 | `azmcp_grafana_list` | ❌ |
| 6 | 0.399303 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.383383 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.382294 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.361735 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.358978 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.353487 | `azmcp_subscription_list` | ❌ |
| 12 | 0.353419 | `azmcp_search_service_list` | ❌ |
| 13 | 0.340764 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.327206 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.315565 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.310802 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.308807 | `azmcp_eventgrid_topic_list` | ❌ |
| 18 | 0.306356 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.305932 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.300220 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 225

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
| 7 | 0.471469 | `azmcp_redis_cache_list` | ❌ |
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

## Test 226

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** Show me the databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.721439 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.562765 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.537732 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.490898 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.481566 | `azmcp_cosmos_database_list` | ❌ |
| 6 | 0.480214 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.434924 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.414651 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.408354 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.397240 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.368983 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.353593 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.351013 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.349785 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.343171 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 16 | 0.325370 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.318892 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.302127 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.294312 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.292053 | `azmcp_grafana_list` | ❌ |

---

## Test 227

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812969 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.679071 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.672212 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.588847 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.569219 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.554316 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.527448 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.503324 | `azmcp_grafana_list` | ❌ |
| 9 | 0.467972 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.462618 | `azmcp_search_service_list` | ❌ |
| 11 | 0.457619 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.455682 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.445523 | `azmcp_group_list` | ❌ |
| 14 | 0.445447 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.443586 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.442865 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 17 | 0.436476 | `azmcp_subscription_list` | ❌ |
| 18 | 0.435231 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.419155 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.411181 | `azmcp_mysql_server_list` | ❌ |

---

## Test 228

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591593 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.514375 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.467648 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.403281 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.385112 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 6 | 0.368011 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.337910 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.329389 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.322157 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.321180 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.305874 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.301285 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.295045 | `azmcp_grafana_list` | ❌ |
| 14 | 0.291684 | `azmcp_postgres_database_list` | ❌ |
| 15 | 0.288103 | `azmcp_aks_nodepool_list` | ❌ |
| 16 | 0.272504 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.261138 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.260993 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.259662 | `azmcp_postgres_server_config_get` | ❌ |
| 20 | 0.252050 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 229

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me the Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.744239 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.607643 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.580866 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.518857 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.494170 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.491262 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.456252 | `azmcp_grafana_list` | ❌ |
| 8 | 0.446568 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.440660 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.412876 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.400258 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 12 | 0.398399 | `azmcp_search_service_list` | ❌ |
| 13 | 0.394530 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.394483 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.389814 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.372587 | `azmcp_group_list` | ❌ |
| 17 | 0.370370 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.369831 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.368926 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.367969 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 230

**Expected Tool:** `azmcp_group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755412 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.566460 | `azmcp_workbooks_list` | ❌ |
| 3 | 0.552633 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.546182 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.545480 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.542878 | `azmcp_grafana_list` | ❌ |
| 7 | 0.530568 | `azmcp_redis_cache_list` | ❌ |
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
| 18 | 0.479545 | `azmcp_subscription_list` | ❌ |
| 19 | 0.477800 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.477024 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 231

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528769 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.463685 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.462391 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.459340 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.453815 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.429014 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.426935 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.407817 | `azmcp_grafana_list` | ❌ |
| 9 | 0.398432 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.396822 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.391295 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.383058 | `azmcp_acr_registry_list` | ❌ |
| 13 | 0.379927 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.375998 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.373796 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.366273 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.351405 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.350999 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.340946 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.328006 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 232

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665780 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.531964 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.523088 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.522701 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.518543 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.515905 | `azmcp_grafana_list` | ❌ |
| 8 | 0.494579 | `azmcp_eventgrid_subscription_list` | ❌ |
| 9 | 0.492961 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.489079 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.487780 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.475708 | `azmcp_search_service_list` | ❌ |
| 13 | 0.470658 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.464637 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.460412 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.454711 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.454439 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.437393 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.432994 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.429798 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 233

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630647 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538239 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377586 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.349980 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.331563 | `azmcp_monitor_metrics_definitions` | ❌ |
| 6 | 0.330187 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.327596 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.325794 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.324331 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.322117 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.311668 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.308238 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.306616 | `azmcp_grafana_list` | ❌ |
| 14 | 0.292084 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.290698 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.286287 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.284990 | `azmcp_applens_resource_diagnose` | ❌ |
| 18 | 0.284986 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.272400 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.271843 | `azmcp_group_list` | ❌ |

---

## Test 234

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549306 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.510474 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.490090 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 4 | 0.466881 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.455902 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.413275 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.411283 | `azmcp_quota_usage_check` | ❌ |
| 8 | 0.405847 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.403899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.375351 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.368262 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.349407 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.347885 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.346773 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.346145 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.336357 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.321762 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.318472 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.311399 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.306746 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 235

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577415 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 2 | 0.570884 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424939 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.393479 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.386598 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.373883 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.355414 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.352447 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.342229 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.338012 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.337593 | `azmcp_aks_nodepool_get` | ❌ |
| 12 | 0.335744 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.327197 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.321127 | `azmcp_group_list` | ❌ |
| 15 | 0.318379 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.317912 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.316508 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.307248 | `azmcp_applens_resource_diagnose` | ❌ |
| 19 | 0.294201 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.289170 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 236

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737234 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.587330 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.578582 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.563455 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.548549 | `azmcp_grafana_list` | ❌ |
| 6 | 0.540583 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.534299 | `azmcp_search_service_list` | ❌ |
| 8 | 0.531356 | `azmcp_quota_region_availability_list` | ❌ |
| 9 | 0.530803 | `azmcp_group_list` | ❌ |
| 10 | 0.507740 | `azmcp_monitor_workspace_list` | ❌ |
| 11 | 0.496651 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.491394 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.491359 | `azmcp_subscription_list` | ❌ |
| 14 | 0.489514 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.484221 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.482623 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.476832 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.465422 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.462565 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.459578 | `azmcp_workbooks_list` | ❌ |

---

## Test 237

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645009 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.587088 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.508252 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.473905 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.462125 | `azmcp_search_service_list` | ❌ |
| 6 | 0.456424 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.441470 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.441430 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.430496 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.418944 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.409394 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.406835 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.406709 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.406408 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.405790 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.403347 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.387835 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.381144 | `azmcp_get_bestpractices_get` | ❌ |
| 19 | 0.379969 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 20 | 0.371846 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 238

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596968 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.543421 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.427638 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.420567 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.420387 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.411111 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.411059 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.374184 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.370961 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.363560 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.360039 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.358871 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.354932 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.350570 | `azmcp_group_list` | ❌ |
| 15 | 0.348876 | `azmcp_monitor_metrics_query` | ❌ |
| 16 | 0.338595 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.330185 | `azmcp_extension_azqr` | ❌ |
| 18 | 0.328639 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.324246 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.309463 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.719917 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `azmcp_search_service_list` | ❌ |
| 3 | 0.531311 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.518399 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.503744 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.470139 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.456567 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.454448 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.446515 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.438780 | `azmcp_subscription_list` | ❌ |
| 11 | 0.427154 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.426698 | `azmcp_grafana_list` | ❌ |
| 13 | 0.419828 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.419011 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.416883 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.412451 | `azmcp_group_list` | ❌ |
| 17 | 0.407099 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.385382 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.378841 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.377860 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 240

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726947 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.513815 | `azmcp_search_service_list` | ❌ |
| 3 | 0.509196 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.491140 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484386 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.474835 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.459791 | `azmcp_subscription_list` | ❌ |
| 8 | 0.431455 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.425644 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.411892 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.410579 | `azmcp_marketplace_product_list` | ❌ |
| 12 | 0.409027 | `azmcp_aks_cluster_list` | ❌ |
| 13 | 0.404636 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.391178 | `azmcp_group_list` | ❌ |
| 15 | 0.390652 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.390381 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.385710 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.384613 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.384551 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.381228 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 241

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.301604 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.270290 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.251870 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.216830 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211842 | `azmcp_search_service_list` | ❌ |
| 6 | 0.191890 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.189628 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.188665 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.187819 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.185941 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.174941 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.170157 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.169947 | `azmcp_servicebus_queue_details` | ❌ |
| 14 | 0.164595 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.164285 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.163022 | `azmcp_monitor_workspace_log_query` | ❌ |
| 17 | 0.155791 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.155444 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.151778 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.149118 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 242

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711107 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.545714 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.520197 | `azmcp_search_service_list` | ❌ |
| 4 | 0.502086 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.487327 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.453380 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 7 | 0.451351 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.439704 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.436070 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.411793 | `azmcp_grafana_list` | ❌ |
| 11 | 0.408792 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.407707 | `azmcp_subscription_list` | ❌ |
| 13 | 0.406949 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.404981 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.391992 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.379016 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.372194 | `azmcp_group_list` | ❌ |
| 18 | 0.368866 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.358747 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.357139 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 243

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527706 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.437868 | `azmcp_search_service_list` | ❌ |
| 3 | 0.402503 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.402234 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.400175 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.397735 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.382848 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.382634 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.375034 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.372844 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.371692 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.363470 | `azmcp_get_bestpractices_get` | ❌ |
| 13 | 0.362214 | `azmcp_applens_resource_diagnose` | ❌ |
| 14 | 0.360562 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.357531 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.341495 | `azmcp_foundry_models_deployments_list` | ❌ |
| 17 | 0.337589 | `azmcp_search_index_get` | ❌ |
| 18 | 0.335471 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.333382 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.333201 | `azmcp_subscription_list` | ❌ |

---

## Test 244

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `azmcp_servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.401012 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.375406 | `azmcp_aks_cluster_get` | ❌ |
| 5 | 0.360251 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.352705 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.352525 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.350804 | `azmcp_search_index_get` | ❌ |
| 9 | 0.344531 | `azmcp_eventgrid_subscription_list` | ❌ |
| 10 | 0.342349 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.337216 | `azmcp_sql_db_show` | ❌ |
| 12 | 0.332541 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.327611 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.323281 | `azmcp_marketplace_product_get` | ❌ |
| 15 | 0.323046 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.310612 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.309214 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.301992 | `azmcp_keyvault_key_get` | ❌ |
| 19 | 0.296380 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.290453 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 245

**Expected Tool:** `azmcp_servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591595 | `azmcp_servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571861 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.497732 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.494885 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.483976 | `azmcp_servicebus_queue_details` | ❌ |
| 6 | 0.365118 | `azmcp_search_index_get` | ❌ |
| 7 | 0.361362 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.352485 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.341289 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.339985 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.337312 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.335558 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.333396 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.330814 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.325589 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.317497 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.306388 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.297323 | `azmcp_grafana_list` | ❌ |
| 19 | 0.290383 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.287440 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 246

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `azmcp_servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.523134 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.494515 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.457149 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.444604 | `azmcp_marketplace_product_get` | ❌ |
| 6 | 0.443973 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.429521 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.426573 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.420914 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.411293 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.409614 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.405380 | `azmcp_search_service_list` | ❌ |
| 13 | 0.404739 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.395789 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.395176 | `azmcp_grafana_list` | ❌ |
| 16 | 0.382225 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.369986 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.368424 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.368357 | `azmcp_group_list` | ❌ |
| 20 | 0.368155 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 247

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470892 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.376833 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.371321 | `azmcp_appservice_database_add` | ❌ |
| 5 | 0.359892 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.357421 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.355614 | `azmcp_postgres_database_list` | ❌ |
| 8 | 0.347128 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.346831 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.329705 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.327837 | `azmcp_sql_db_delete` | ❌ |
| 12 | 0.311744 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.301243 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.297803 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.279490 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.276192 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.272135 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.254831 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.238999 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.231273 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 248

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459672 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.424021 | `azmcp_appservice_database_add` | ❌ |
| 4 | 0.420856 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.396106 | `azmcp_sql_db_update` | ❌ |
| 6 | 0.395495 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.384660 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.371559 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.361051 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.343099 | `azmcp_sql_db_delete` | ❌ |
| 11 | 0.326611 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 12 | 0.324123 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.321837 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.317180 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.311150 | `azmcp_keyvault_key_create` | ❌ |
| 16 | 0.304604 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.301487 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.290173 | `azmcp_keyvault_certificate_create` | ❌ |
| 19 | 0.286796 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 20 | 0.276688 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545906 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.494316 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.473975 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.456262 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.455293 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.440988 | `azmcp_appservice_database_add` | ❌ |
| 8 | 0.431068 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.422871 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.413309 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.398677 | `azmcp_loadtesting_testresource_create` | ❌ |
| 12 | 0.392814 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.390278 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.379238 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.378636 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.374962 | `azmcp_sql_elastic-pool_list` | ❌ |
| 17 | 0.365919 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.358566 | `azmcp_kusto_table_list` | ❌ |
| 19 | 0.323083 | `azmcp_group_list` | ❌ |
| 20 | 0.319381 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 250

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520786 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.484026 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.386564 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.364725 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.351204 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.350121 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.345061 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.338052 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.337699 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.317215 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.300711 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.286892 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.284794 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.278895 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.271009 | `azmcp_appconfig_kv_delete` | ❌ |
| 16 | 0.253808 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.243201 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.235173 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.211556 | `azmcp_kusto_query` | ❌ |
| 20 | 0.183587 | `azmcp_kusto_sample` | ❌ |

---

## Test 251

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579119 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.500699 | `azmcp_sql_db_show` | ❌ |
| 3 | 0.478729 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.466216 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 5 | 0.437112 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.421365 | `azmcp_sql_db_create` | ❌ |
| 7 | 0.412704 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.392236 | `azmcp_workbooks_delete` | ❌ |
| 9 | 0.390334 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.388379 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.381382 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.380074 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.370486 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.368841 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.344846 | `azmcp_group_list` | ❌ |
| 16 | 0.334517 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.332517 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.327408 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.310437 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.304849 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 252

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.459422 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.427494 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.364494 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.355416 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.319583 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.314902 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.311506 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.310758 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.305059 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 10 | 0.295355 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.295012 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.294781 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.288554 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.283955 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.258654 | `azmcp_appconfig_kv_delete` | ❌ |
| 16 | 0.246948 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.229764 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.213266 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.187839 | `azmcp_kusto_query` | ❌ |
| 20 | 0.171883 | `azmcp_kusto_sample` | ❌ |

---

## Test 253

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
| 9 | 0.485960 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.476350 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.475733 | `azmcp_sql_elastic-pool_list` | ❌ |
| 12 | 0.474927 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.457314 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.441355 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.440528 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.400865 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.395078 | `azmcp_keyvault_key_list` | ❌ |
| 18 | 0.394543 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.380402 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.373653 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 254

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.609322 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.557353 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.553488 | `azmcp_mysql_server_config_get` | ❌ |
| 5 | 0.524207 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.471862 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.461650 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.458742 | `azmcp_postgres_server_config_get` | ❌ |
| 9 | 0.451453 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.446512 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.445291 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.387645 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.385113 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.380428 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.357318 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.354581 | `azmcp_aks_nodepool_list` | ❌ |
| 17 | 0.349880 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.347075 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.342792 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.342305 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 255

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610991 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.593150 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.530422 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.528128 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465693 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.446728 | `azmcp_postgres_server_param_get` | ❌ |
| 7 | 0.438925 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.398181 | `azmcp_mysql_table_schema_get` | ❌ |
| 9 | 0.397510 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.396416 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.371413 | `azmcp_loadtesting_test_get` | ❌ |
| 12 | 0.325945 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.325788 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.320067 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.317619 | `azmcp_appservice_database_add` | ❌ |
| 16 | 0.297839 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.294987 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.281546 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.279692 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 20 | 0.273566 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 256

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530077 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
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
| 15 | 0.299832 | `azmcp_aks_cluster_get` | ❌ |
| 16 | 0.296827 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.291629 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.285843 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.274274 | `azmcp_appservice_database_add` | ❌ |
| 20 | 0.268405 | `azmcp_functionapp_get` | ❌ |

---

## Test 257

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565229 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.427644 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.385817 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.384192 | `azmcp_appservice_database_add` | ❌ |
| 6 | 0.371461 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.369822 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.368957 | `azmcp_mysql_server_param_set` | ❌ |
| 9 | 0.344860 | `azmcp_sql_db_delete` | ❌ |
| 10 | 0.334302 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.316890 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.306025 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.273809 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.270134 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.263397 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.250975 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.250753 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.240663 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.230967 | `azmcp_loadtesting_testrun_create` | ❌ |
| 20 | 0.223147 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 258

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.401817 | `azmcp_sql_db_list` | ❌ |
| 2 | 0.394789 | `azmcp_sql_db_show` | ❌ |
| 3 | 0.389947 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 4 | 0.386628 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.381889 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.349256 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 7 | 0.342291 | `azmcp_sql_elastic-pool_list` | ❌ |
| 8 | 0.339054 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.333336 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.329770 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.325658 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.304373 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.301534 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.297094 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 15 | 0.261164 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.257330 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.238540 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.232099 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.223081 | `azmcp_loadtesting_testrun_update` | ❌ |
| 20 | 0.221187 | `azmcp_kusto_query` | ❌ |

---

## Test 259

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678132 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502372 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.498386 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.479110 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.473545 | `azmcp_aks_nodepool_list` | ❌ |
| 6 | 0.454472 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.450843 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.442901 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.441222 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.434618 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.431145 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.429067 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 13 | 0.394554 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.394299 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.370623 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.363559 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.357334 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.352048 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.351631 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.350979 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 260

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
| 6 | 0.429793 | `azmcp_aks_nodepool_get` | ❌ |
| 7 | 0.423047 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.419753 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.408202 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.400026 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.383940 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 12 | 0.378556 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.341678 | `azmcp_foundry_agents_list` | ❌ |
| 14 | 0.335615 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.333099 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.319836 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.317886 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.312361 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.304600 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.304317 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 261

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
| 15 | 0.315665 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.298933 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.292566 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.284157 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.281680 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.259585 | `azmcp_appservice_database_add` | ❌ |

---

## Test 262

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.683298 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.564483 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.537913 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.530005 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.482704 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.474101 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.465375 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.451666 | `azmcp_loadtesting_testresource_create` | ❌ |
| 9 | 0.450520 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.450194 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.419510 | `azmcp_sql_elastic-pool_list` | ❌ |
| 12 | 0.402999 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.401550 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.397655 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.353077 | `azmcp_appservice_database_add` | ❌ |
| 16 | 0.336030 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.333673 | `azmcp_extension_azqr` | ❌ |
| 18 | 0.327054 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.323682 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.320235 | `azmcp_acr_registry_list` | ❌ |

---

## Test 263

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618309 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510169 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.472463 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.434810 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.397805 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.396073 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.369890 | `azmcp_keyvault_secret_create` | ❌ |
| 8 | 0.367996 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.367981 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.360875 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.358285 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.354438 | `azmcp_sql_elastic-pool_list` | ❌ |
| 13 | 0.352234 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.349584 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 15 | 0.324021 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.316772 | `azmcp_loadtesting_test_create` | ❌ |
| 17 | 0.315987 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.314253 | `azmcp_foundry_agents_connect` | ❌ |
| 19 | 0.301087 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.300916 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 264

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589775 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.501306 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.497849 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.469338 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.442886 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.423813 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.421456 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.417439 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.416113 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.415367 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.389562 | `azmcp_sql_elastic-pool_list` | ❌ |
| 12 | 0.385022 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.332985 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.317217 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.312669 | `azmcp_loadtesting_test_create` | ❌ |
| 16 | 0.303170 | `azmcp_keyvault_certificate_create` | ❌ |
| 17 | 0.300994 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.297655 | `azmcp_group_list` | ❌ |
| 19 | 0.288571 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.284874 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 265

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702353 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.518036 | `azmcp_sql_server_list` | ❌ |
| 3 | 0.495550 | `azmcp_sql_server_create` | ❌ |
| 4 | 0.486195 | `azmcp_sql_db_delete` | ❌ |
| 5 | 0.483132 | `azmcp_workbooks_delete` | ❌ |
| 6 | 0.470037 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.449007 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.448514 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.438950 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.417035 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 11 | 0.346442 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.333269 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.323460 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.317588 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.316796 | `azmcp_group_list` | ❌ |
| 16 | 0.310672 | `azmcp_appservice_database_add` | ❌ |
| 17 | 0.307426 | `azmcp_appconfig_kv_delete` | ❌ |
| 18 | 0.290106 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.275215 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.272963 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 266

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429140 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393885 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.376660 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.350103 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.309280 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 6 | 0.306203 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.301933 | `azmcp_sql_db_delete` | ❌ |
| 8 | 0.299760 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.295963 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.295073 | `azmcp_sql_server_entra-admin_list` | ❌ |
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

## Test 267

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
| 8 | 0.261681 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 9 | 0.254391 | `azmcp_appconfig_kv_delete` | ❌ |
| 10 | 0.247446 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.237815 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.213567 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.168042 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.164350 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.159907 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.156253 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.148272 | `azmcp_kusto_database_list` | ❌ |
| 18 | 0.146243 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.142112 | `azmcp_kusto_query` | ❌ |
| 20 | 0.140290 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 268

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783479 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.456051 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.434868 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.401900 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376055 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.365636 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.352607 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.344454 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.343559 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.329397 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.291767 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.280450 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.258095 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.249297 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.249153 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.246563 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.245119 | `azmcp_group_list` | ❌ |
| 18 | 0.238150 | `azmcp_keyvault_key_list` | ❌ |
| 19 | 0.234681 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.233337 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 269

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
| 6 | 0.304881 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 7 | 0.303560 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.289764 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.287372 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.283806 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.273121 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.214529 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.205963 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.197679 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.194313 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.193050 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.191538 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.188120 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.183184 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.182252 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 270

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646455 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.356083 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.322219 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.307832 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.253698 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.236910 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.236183 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.234882 | `azmcp_appservice_database_add` | ❌ |
| 9 | 0.230612 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.228267 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.223179 | `azmcp_sql_db_update` | ❌ |
| 12 | 0.212731 | `azmcp_cloudarchitect_design` | ❌ |
| 13 | 0.210599 | `azmcp_foundry_agents_list` | ❌ |
| 14 | 0.200465 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.190030 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.188366 | `azmcp_deploy_plan_get` | ❌ |
| 17 | 0.181090 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.181029 | `azmcp_foundry_agents_connect` | ❌ |
| 19 | 0.180596 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.174618 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 271

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635466 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532714 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448822 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.432802 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.423223 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.403858 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.397912 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.361266 | `azmcp_appservice_database_add` | ❌ |
| 10 | 0.335670 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.326425 | `azmcp_sql_db_update` | ❌ |
| 12 | 0.318368 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.311149 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.295941 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.290296 | `azmcp_deploy_iac_rules_get` | ❌ |
| 16 | 0.288030 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.265059 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 18 | 0.260209 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.253769 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.252071 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 272

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670189 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533553 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316619 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.295018 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.287457 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.284186 | `azmcp_appservice_database_add` | ❌ |
| 8 | 0.271240 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.252970 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 10 | 0.248884 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.237646 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.221694 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 13 | 0.179175 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.174851 | `azmcp_deploy_iac_rules_get` | ❌ |
| 15 | 0.174584 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.166723 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 17 | 0.158176 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.156396 | `azmcp_keyvault_key_create` | ❌ |
| 19 | 0.149912 | `azmcp_kusto_query` | ❌ |
| 20 | 0.146141 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 273

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685107 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574335 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539577 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428919 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.395165 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.356402 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.339841 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.321920 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.316783 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.302214 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.296502 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.283923 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.281128 | `azmcp_postgres_server_param_set` | ❌ |
| 14 | 0.270399 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 15 | 0.248939 | `azmcp_loadtesting_test_create` | ❌ |
| 16 | 0.221008 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.219182 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.209376 | `azmcp_loadtesting_testrun_create` | ❌ |
| 19 | 0.207193 | `azmcp_loadtesting_testresource_create` | ❌ |
| 20 | 0.197104 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 274

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.543862 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.540333 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.527546 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.436585 | `azmcp_sql_db_delete` | ❌ |
| 6 | 0.418504 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.410574 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.386562 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.341915 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.341838 | `azmcp_sql_db_update` | ❌ |
| 11 | 0.312054 | `azmcp_appconfig_kv_delete` | ❌ |
| 12 | 0.306396 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.263959 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.246991 | `azmcp_keyvault_secret_get` | ❌ |
| 15 | 0.245270 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 16 | 0.241564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.235230 | `azmcp_keyvault_certificate_create` | ❌ |
| 18 | 0.231494 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.225273 | `azmcp_kusto_query` | ❌ |
| 20 | 0.225236 | `azmcp_keyvault_certificate_get` | ❌ |

---

## Test 275

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670179 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574346 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.530419 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.398706 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.310449 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.298395 | `azmcp_sql_db_delete` | ❌ |
| 7 | 0.293097 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.259110 | `azmcp_workbooks_delete` | ❌ |
| 9 | 0.254974 | `azmcp_appconfig_kv_delete` | ❌ |
| 10 | 0.251005 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.231881 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.227837 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.182013 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.166475 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.158042 | `azmcp_kusto_query` | ❌ |
| 16 | 0.156028 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.153608 | `azmcp_keyvault_secret_get` | ❌ |
| 18 | 0.152458 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.152084 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 20 | 0.149578 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 276

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671212 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601235 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.577330 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.441605 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.367883 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.336482 | `azmcp_sql_db_delete` | ❌ |
| 7 | 0.332209 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.293303 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.291409 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 10 | 0.286333 | `azmcp_sql_db_update` | ❌ |
| 11 | 0.263966 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.252095 | `azmcp_appconfig_kv_delete` | ❌ |
| 13 | 0.222155 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.204194 | `azmcp_loadtesting_testrun_update` | ❌ |
| 15 | 0.185585 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.185007 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.183545 | `azmcp_deploy_iac_rules_get` | ❌ |
| 18 | 0.181757 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 19 | 0.180480 | `azmcp_kusto_query` | ❌ |
| 20 | 0.179886 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 277

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729366 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549667 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.418817 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.392512 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.385148 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.359228 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.356700 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.355203 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.304958 | `azmcp_keyvault_secret_list` | ❌ |
| 12 | 0.278098 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.277410 | `azmcp_keyvault_key_list` | ❌ |
| 14 | 0.277054 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.274554 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.270667 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.263181 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.256310 | `azmcp_aks_nodepool_list` | ❌ |
| 19 | 0.253852 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.248780 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 278

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630841 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.524198 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476865 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410678 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.348137 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.316834 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.312010 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.298975 | `azmcp_mysql_server_param_get` | ❌ |
| 9 | 0.294481 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.293374 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.225370 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.217351 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.211182 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.210496 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.209530 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.206791 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.206398 | `azmcp_deploy_iac_rules_get` | ❌ |
| 18 | 0.206107 | `azmcp_kusto_table_list` | ❌ |
| 19 | 0.197674 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.195896 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 279

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630543 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532454 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473501 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412957 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.350513 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.308004 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.305701 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.304314 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.282538 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.277628 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.221706 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.216156 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.202425 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.200326 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.191165 | `azmcp_cloudarchitect_design` | ❌ |
| 16 | 0.185879 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.177454 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.176225 | `azmcp_get_bestpractices_get` | ❌ |
| 19 | 0.173184 | `azmcp_applens_resource_diagnose` | ❌ |
| 20 | 0.172371 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 280

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694404 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596686 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.578239 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.515851 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509639 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.500373 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.496455 | `azmcp_group_list` | ❌ |
| 8 | 0.496434 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.495321 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.487699 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.487455 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.473235 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.449346 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.449287 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.419283 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.403609 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.395561 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.384532 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.384389 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.380949 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 281

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618218 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593837 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.542398 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.507425 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.495949 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.495792 | `azmcp_group_list` | ❌ |
| 7 | 0.492324 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.484259 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.477041 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.470335 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.464018 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.449733 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.444259 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.419448 | `azmcp_foundry_agents_list` | ❌ |
| 15 | 0.418009 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.410302 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.397201 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.395060 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.391940 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.384337 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 282

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629438 | `azmcp_sql_db_show` | ❌ |
| 2 | 0.595268 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587691 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.559847 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.540232 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.491398 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.488271 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.481801 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.480106 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.478714 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.450051 | `azmcp_aks_cluster_get` | ❌ |
| 12 | 0.445737 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.437334 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.425061 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.410220 | `azmcp_group_list` | ❌ |
| 16 | 0.400265 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.394094 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.385373 | `azmcp_extension_azqr` | ❌ |
| 19 | 0.383575 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.373416 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 283

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.538034 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.471477 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.445500 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.443977 | `azmcp_mysql_server_param_get` | ❌ |
| 7 | 0.422646 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.414309 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.413961 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.406630 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.400827 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.359458 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.349963 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.316818 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.314864 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.308718 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.300098 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.298409 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.295903 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.284200 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 284

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.380047 | `azmcp_postgres_server_param_get` | ❌ |
| 4 | 0.372190 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370447 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.368788 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.367031 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.363268 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.361792 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.357960 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.288829 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.276327 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.271945 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.268920 | `azmcp_loadtesting_testrun_get` | ❌ |
| 15 | 0.257258 | `azmcp_appconfig_kv_list` | ❌ |
| 16 | 0.255881 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.253925 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.246261 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.245005 | `azmcp_keyvault_key_get` | ❌ |
| 20 | 0.243029 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 285

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.418472 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.394541 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.374006 | `azmcp_loadtesting_test_create` | ❌ |
| 5 | 0.355025 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.352380 | `azmcp_storage_blob_container_get` | ❌ |
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
| 17 | 0.286311 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.278047 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.277805 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.267474 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 286

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
| 7 | 0.344488 | `azmcp_loadtesting_testresource_create` | ❌ |
| 8 | 0.339839 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.329099 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.324346 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.310743 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.310707 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.310332 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.296391 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.284420 | `azmcp_deploy_plan_get` | ❌ |
| 16 | 0.284385 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.283067 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 18 | 0.280404 | `azmcp_keyvault_certificate_create` | ❌ |
| 19 | 0.280192 | `azmcp_cloudarchitect_design` | ❌ |
| 20 | 0.278858 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 287

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589003 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.464611 | `azmcp_storage_blob_container_create` | ❌ |
| 3 | 0.447156 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.437040 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.407118 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.384168 | `azmcp_loadtesting_testresource_create` | ❌ |
| 7 | 0.383895 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.382274 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.380638 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.380503 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.372681 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.372357 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.366696 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.363721 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.360940 | `azmcp_storage_blob_upload` | ❌ |
| 16 | 0.359330 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.325418 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.324756 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.324674 | `azmcp_appservice_database_add` | ❌ |
| 20 | 0.321775 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 288

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655152 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.604076 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.507541 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.483435 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.442858 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.439236 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.431020 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.403478 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.397051 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.395698 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.388437 | `azmcp_aks_cluster_get` | ❌ |
| 12 | 0.373840 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.373146 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.370022 | `azmcp_keyvault_key_get` | ❌ |
| 15 | 0.368598 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.367173 | `azmcp_subscription_list` | ❌ |
| 17 | 0.367049 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.366645 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.362836 | `azmcp_search_index_get` | ❌ |
| 20 | 0.356973 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 289

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676900 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.613348 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.518279 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.515071 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.415406 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.411833 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.401829 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.380011 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.375889 | `azmcp_keyvault_key_get` | ❌ |
| 10 | 0.375812 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.373524 | `azmcp_aks_cluster_get` | ❌ |
| 12 | 0.369799 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.368205 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.368016 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.362620 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.362608 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.362242 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.355115 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.354850 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.351121 | `azmcp_functionapp_get` | ❌ |

---

## Test 290

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664087 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557016 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.536909 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.535616 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.501088 | `azmcp_subscription_list` | ❌ |
| 6 | 0.496371 | `azmcp_quota_region_availability_list` | ❌ |
| 7 | 0.493246 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.484363 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.484163 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.473387 | `azmcp_search_service_list` | ❌ |
| 11 | 0.458793 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.454195 | `azmcp_acr_registry_list` | ❌ |
| 13 | 0.447992 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.445563 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.441838 | `azmcp_redis_cluster_list` | ❌ |
| 16 | 0.439919 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.438615 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.432645 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.416456 | `azmcp_group_list` | ❌ |
| 20 | 0.412679 | `azmcp_grafana_list` | ❌ |

---

## Test 291

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499302 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.461284 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.455566 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.421642 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.379865 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.378256 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 7 | 0.375553 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.367906 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.366021 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.362252 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.361073 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.347173 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.346039 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.344771 | `azmcp_search_service_list` | ❌ |
| 15 | 0.342056 | `azmcp_subscription_list` | ❌ |
| 16 | 0.335306 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.330862 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.330363 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.322108 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.315526 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 292

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557142 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.473598 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.461727 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.453933 | `azmcp_subscription_list` | ❌ |
| 5 | 0.436170 | `azmcp_search_service_list` | ❌ |
| 6 | 0.432854 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.425023 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.418403 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.416137 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.415080 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.389930 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.382504 | `azmcp_aks_cluster_list` | ❌ |
| 13 | 0.379856 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.377201 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.376660 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.374635 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.371828 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.359998 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.359053 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.356611 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 293

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563396 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.524779 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.508280 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.447784 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.403407 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.335039 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.331415 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.326352 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.324867 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.323215 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.322464 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.318855 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.305680 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.297912 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 15 | 0.297384 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.292093 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.291137 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.281807 | `azmcp_sql_server_create` | ❌ |
| 19 | 0.280439 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.274863 | `azmcp_workbooks_create` | ❌ |

---

## Test 294

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512466 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.500604 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.471142 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415450 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.414933 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.368878 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.334027 | `azmcp_storage_blob_upload` | ❌ |
| 8 | 0.320162 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 9 | 0.309775 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.296920 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.296353 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.285179 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.277977 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.275245 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.275125 | `azmcp_keyvault_key_create` | ❌ |
| 16 | 0.270113 | `azmcp_appconfig_kv_set` | ❌ |
| 17 | 0.269783 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.268808 | `azmcp_workbooks_create` | ❌ |
| 19 | 0.256453 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.249272 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 295

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463224 | `azmcp_storage_account_create` | ❌ |
| 2 | 0.455406 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.451765 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 4 | 0.435122 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.388350 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.378057 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.366287 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.328973 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.322331 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.314118 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.309010 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.287863 | `azmcp_workbooks_create` | ❌ |
| 13 | 0.280813 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.276755 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.276426 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.275625 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.269707 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.266760 | `azmcp_appconfig_kv_set` | ❌ |
| 19 | 0.265251 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.265245 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 296

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665503 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.559177 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.523288 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.518713 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.496184 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.461577 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.421964 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.421220 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.384585 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.377009 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.367759 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.359218 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.354913 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.353278 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.350264 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.335739 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.334806 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.332359 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.327271 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.308777 | `azmcp_mysql_server_list` | ❌ |

---

## Test 297

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613933 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.605599 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 3 | 0.522323 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.479014 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.471385 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.453044 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.409820 | `azmcp_acr_registry_repository_list` | ❌ |
| 8 | 0.404640 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.393989 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.386144 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.367207 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.359465 | `azmcp_search_service_list` | ❌ |
| 13 | 0.359411 | `azmcp_subscription_list` | ❌ |
| 14 | 0.356400 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.353231 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.351862 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.351458 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.333677 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.333282 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.332194 | `azmcp_monitor_table_list` | ❌ |

---

## Test 298

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625206 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592364 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.511167 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.439646 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.437840 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.429945 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.418035 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.405639 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.390272 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.384107 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.355911 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.354367 | `azmcp_search_service_list` | ❌ |
| 13 | 0.352390 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.348065 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.347261 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.346884 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.345676 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.340579 | `azmcp_subscription_list` | ❌ |
| 19 | 0.336535 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.326596 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 299

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613044 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.586507 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.483614 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.477946 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.434667 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.420748 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.408521 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.386482 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.359392 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.349565 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.345511 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.338048 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.333887 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.330904 | `azmcp_storage_blob_upload` | ❌ |
| 15 | 0.326177 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.323065 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.321138 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.318563 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.303942 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.303596 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 300

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662277 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.661738 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.537535 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.460657 | `azmcp_storage_blob_container_create` | ❌ |
| 5 | 0.457038 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.453696 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.370177 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.360712 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.359655 | `azmcp_aks_cluster_get` | ❌ |
| 10 | 0.358376 | `azmcp_storage_blob_upload` | ❌ |
| 11 | 0.353461 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.353131 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.352671 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.348551 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.348396 | `azmcp_keyvault_key_get` | ❌ |
| 16 | 0.342979 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.337010 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.334138 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 19 | 0.329399 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.319604 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 301

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593029 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.579070 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.568872 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 4 | 0.465942 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.452160 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.415853 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.413280 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.400483 | `azmcp_acr_registry_repository_list` | ❌ |
| 9 | 0.394852 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.379851 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.379099 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.369535 | `azmcp_keyvault_secret_list` | ❌ |
| 13 | 0.361689 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.359358 | `azmcp_keyvault_certificate_list` | ❌ |
| 15 | 0.348821 | `azmcp_subscription_list` | ❌ |
| 16 | 0.339776 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.331545 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.328193 | `azmcp_search_service_list` | ❌ |
| 19 | 0.313259 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.310914 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 302

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570479 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.549720 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.533515 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.449128 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.433883 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.397367 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.395809 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.385242 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.362337 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.353799 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.345263 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.342766 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.340043 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.335768 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.314069 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.308890 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.306951 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.300295 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.298968 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.294761 | `azmcp_subscription_list` | ❌ |

---

## Test 303

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566287 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.403607 | `azmcp_storage_blob_get` | ❌ |
| 3 | 0.398069 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.382123 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.377255 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.351920 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.327416 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.324049 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.294727 | `azmcp_keyvault_certificate_import` | ❌ |
| 10 | 0.284896 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.284080 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.280431 | `azmcp_speech_stt_recognize` | ❌ |
| 13 | 0.273638 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.273513 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.272315 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 16 | 0.257984 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.254581 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.253430 | `azmcp_appconfig_kv_show` | ❌ |
| 19 | 0.239522 | `azmcp_foundry_models_deploy` | ❌ |
| 20 | 0.211052 | `azmcp_workbooks_create` | ❌ |

---

## Test 304

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576055 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.473898 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471653 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.465428 | `azmcp_eventgrid_subscription_list` | ❌ |
| 6 | 0.452471 | `azmcp_search_service_list` | ❌ |
| 7 | 0.450973 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.445724 | `azmcp_grafana_list` | ❌ |
| 9 | 0.431337 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.430915 | `azmcp_group_list` | ❌ |
| 11 | 0.422723 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.406935 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.394953 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.388737 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.380636 | `azmcp_marketplace_product_list` | ❌ |
| 16 | 0.367761 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.366860 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.355344 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.354245 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.348505 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 305

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405723 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.381238 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.380789 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.351864 | `azmcp_grafana_list` | ❌ |
| 5 | 0.351027 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.341813 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.334744 | `azmcp_eventgrid_topic_list` | ❌ |
| 8 | 0.328109 | `azmcp_search_service_list` | ❌ |
| 9 | 0.315604 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.308874 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.303528 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.303367 | `azmcp_marketplace_product_list` | ❌ |
| 13 | 0.298324 | `azmcp_group_list` | ❌ |
| 14 | 0.296282 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.295180 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.285434 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.275417 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.274876 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.269922 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.258047 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 306

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.319958 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.315547 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.307697 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.286770 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.282645 | `azmcp_grafana_list` | ❌ |
| 6 | 0.279702 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.278798 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.273758 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.256358 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.254815 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.252879 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.252504 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.251683 | `azmcp_search_service_list` | ❌ |
| 14 | 0.251368 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.233143 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.230571 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.230324 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.226446 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.222799 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.218684 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 307

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.403229 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.370692 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.354575 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.342318 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.340339 | `azmcp_grafana_list` | ❌ |
| 6 | 0.336798 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.311939 | `azmcp_search_service_list` | ❌ |
| 8 | 0.311109 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.305150 | `azmcp_marketplace_product_get` | ❌ |
| 10 | 0.304965 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.302271 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.300478 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.294080 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.291826 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.282326 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.281294 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.274224 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.271223 | `azmcp_group_list` | ❌ |
| 19 | 0.258468 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.233362 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 308

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625270 | `azmcp_deploy_iac_rules_get` | ❌ |
| 3 | 0.605047 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.482936 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466161 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.431102 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.389080 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.386480 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.372620 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369184 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.362323 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.354086 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.339022 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.333210 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.312592 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.310275 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.305259 | `azmcp_mysql_database_query` | ❌ |
| 18 | 0.303853 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.302307 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.301552 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 309

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.523972 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.512141 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.510004 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.474384 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.444297 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 7 | 0.439871 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.439536 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.428888 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.389450 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.304912 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.304137 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.300776 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.292743 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.285517 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.281261 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.279035 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.278638 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.277384 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.274538 | `azmcp_subscription_list` | ❌ |

---

## Test 310

**Expected Tool:** `azmcp_virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711981 | `azmcp_virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659200 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.566692 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.548984 | `azmcp_search_service_list` | ❌ |
| 5 | 0.536587 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.535704 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 7 | 0.527992 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.527106 | `azmcp_aks_nodepool_list` | ❌ |
| 9 | 0.525856 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.525604 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.506707 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.505152 | `azmcp_subscription_list` | ❌ |
| 13 | 0.496373 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.495526 | `azmcp_grafana_list` | ❌ |
| 15 | 0.492624 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.476756 | `azmcp_group_list` | ❌ |
| 17 | 0.465605 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.463107 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.460457 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.459295 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 311

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726014 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714469 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.439611 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.402909 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.393721 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.364696 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.362307 | `azmcp_search_service_list` | ❌ |
| 9 | 0.359409 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.344853 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.337530 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.335295 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.333517 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.332928 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.330896 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.328623 | `azmcp_keyvault_key_list` | ❌ |
| 17 | 0.324603 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.312251 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.311262 | `azmcp_grafana_list` | ❌ |
| 20 | 0.308168 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 312

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.811926 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.656627 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.500250 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.355211 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.335643 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.327049 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.321845 | `azmcp_subscription_list` | ❌ |
| 8 | 0.320841 | `azmcp_search_service_list` | ❌ |
| 9 | 0.314998 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.314493 | `azmcp_loadtesting_testrun_list` | ❌ |
| 11 | 0.306614 | `azmcp_aks_nodepool_get` | ❌ |
| 12 | 0.303225 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.302806 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.302255 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.299027 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.298739 | `azmcp_keyvault_secret_list` | ❌ |
| 17 | 0.295115 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.294902 | `azmcp_grafana_list` | ❌ |
| 19 | 0.284682 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.277159 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 313

**Expected Tool:** `azmcp_workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552212 | `azmcp_workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.433162 | `azmcp_workbooks_update` | ❌ |
| 3 | 0.361364 | `azmcp_workbooks_delete` | ❌ |
| 4 | 0.361215 | `azmcp_workbooks_show` | ❌ |
| 5 | 0.328058 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.239813 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.217264 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.214818 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.188168 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.173147 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.169440 | `azmcp_grafana_list` | ❌ |
| 12 | 0.164006 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.153950 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.148897 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.147365 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.143713 | `azmcp_sql_server_create` | ❌ |
| 17 | 0.130524 | `azmcp_loadtesting_testrun_create` | ❌ |
| 18 | 0.130339 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.116803 | `azmcp_loadtesting_testrun_update` | ❌ |
| 20 | 0.113914 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 314

**Expected Tool:** `azmcp_workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621310 | `azmcp_workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.518630 | `azmcp_workbooks_show` | ❌ |
| 3 | 0.432454 | `azmcp_workbooks_create` | ❌ |
| 4 | 0.425505 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.390355 | `azmcp_workbooks_update` | ❌ |
| 6 | 0.273939 | `azmcp_grafana_list` | ❌ |
| 7 | 0.256795 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 8 | 0.248002 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.242993 | `azmcp_sql_server_delete` | ❌ |
| 10 | 0.198585 | `azmcp_appconfig_kv_delete` | ❌ |
| 11 | 0.191002 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.186665 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.148882 | `azmcp_extension_azqr` | ❌ |
| 14 | 0.145141 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.134979 | `azmcp_loadtesting_testrun_update` | ❌ |
| 16 | 0.132504 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.131693 | `azmcp_group_list` | ❌ |
| 18 | 0.122450 | `azmcp_loadtesting_test_get` | ❌ |
| 19 | 0.119548 | `azmcp_loadtesting_testresource_create` | ❌ |
| 20 | 0.114355 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 315

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772165 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.532565 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.516739 | `azmcp_grafana_list` | ❌ |
| 5 | 0.488489 | `azmcp_group_list` | ❌ |
| 6 | 0.487920 | `azmcp_workbooks_delete` | ❌ |
| 7 | 0.459976 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.454210 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.439959 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.428781 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.416446 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.413409 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.405963 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.405949 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.399758 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.365302 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.362740 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.356739 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.352940 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.349674 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 316

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708214 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570259 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.539957 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.485504 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.472378 | `azmcp_grafana_list` | ❌ |
| 6 | 0.428025 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.425426 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.422782 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.422069 | `azmcp_group_list` | ❌ |
| 10 | 0.412390 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.392371 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.380991 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.380399 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.371128 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.363744 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.350839 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.338334 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.337786 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.334580 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.333154 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 317

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.697539 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498390 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.494504 | `azmcp_workbooks_list` | ❌ |
| 4 | 0.452348 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.419105 | `azmcp_workbooks_update` | ❌ |
| 6 | 0.353546 | `azmcp_grafana_list` | ❌ |
| 7 | 0.277807 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.264638 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.256678 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.250024 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.237373 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.225294 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.218999 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.207693 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.197060 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.196003 | `azmcp_group_list` | ❌ |
| 17 | 0.189900 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.189657 | `azmcp_extension_azqr` | ❌ |
| 19 | 0.187682 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.187564 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 318

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Show me the workbook with display name <workbook_display_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469476 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.455158 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.437638 | `azmcp_workbooks_update` | ❌ |
| 4 | 0.424092 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.366057 | `azmcp_workbooks_delete` | ❌ |
| 6 | 0.292898 | `azmcp_grafana_list` | ❌ |
| 7 | 0.266680 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.239907 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.227383 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.176481 | `azmcp_role_assignment_list` | ❌ |
| 11 | 0.175814 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.174513 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.173251 | `azmcp_postgres_table_list` | ❌ |
| 14 | 0.168191 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.165774 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.154760 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.152535 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.149678 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.146054 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.141970 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 319

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
| 6 | 0.262781 | `azmcp_workbooks_list` | ❌ |
| 7 | 0.174535 | `azmcp_sql_db_update` | ❌ |
| 8 | 0.170118 | `azmcp_grafana_list` | ❌ |
| 9 | 0.148730 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.142404 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.142186 | `azmcp_loadtesting_testrun_create` | ❌ |
| 12 | 0.138354 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.136204 | `azmcp_loadtesting_testresource_create` | ❌ |
| 14 | 0.132978 | `azmcp_foundry_agents_evaluate` | ❌ |
| 15 | 0.131007 | `azmcp_postgres_database_query` | ❌ |
| 16 | 0.130022 | `azmcp_postgres_server_param_set` | ❌ |
| 17 | 0.129660 | `azmcp_deploy_iac_rules_get` | ❌ |
| 18 | 0.126312 | `azmcp_storage_blob_upload` | ❌ |
| 19 | 0.116768 | `azmcp_appservice_database_add` | ❌ |
| 20 | 0.113589 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 320

**Expected Tool:** `azmcp_bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485889 | `azmcp_deploy_iac_rules_get` | ❌ |
| 2 | 0.448373 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.440302 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.432777 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.430675 | `azmcp_bicepschema_get` | ✅ **EXPECTED** |
| 6 | 0.400985 | `azmcp_foundry_models_deploy` | ❌ |
| 7 | 0.398046 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.393648 | `azmcp_foundry_agents_connect` | ❌ |
| 9 | 0.391625 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 10 | 0.385497 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.372097 | `azmcp_search_service_list` | ❌ |
| 12 | 0.361891 | `azmcp_speech_stt_recognize` | ❌ |
| 13 | 0.325716 | `azmcp_search_index_query` | ❌ |
| 14 | 0.324659 | `azmcp_search_index_get` | ❌ |
| 15 | 0.317232 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.303183 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.291291 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.281487 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.279983 | `azmcp_workbooks_delete` | ❌ |
| 20 | 0.274770 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 321

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
| 6 | 0.216744 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 7 | 0.191391 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.191096 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.178292 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.175833 | `azmcp_deploy_iac_rules_get` | ❌ |
| 11 | 0.136607 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.135768 | `azmcp_get_bestpractices_get` | ❌ |
| 13 | 0.135157 | `azmcp_speech_stt_recognize` | ❌ |
| 14 | 0.132826 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.130037 | `azmcp_foundry_models_deploy` | ❌ |
| 16 | 0.118383 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.115876 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.111446 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.106651 | `azmcp_mysql_database_query` | ❌ |
| 20 | 0.104162 | `azmcp_storage_account_get` | ❌ |

---

## Test 322

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.290270 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.267683 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258160 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.225683 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.215748 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.207352 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.195387 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.189220 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.179038 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.170220 | `azmcp_foundry_agents_connect` | ❌ |
| 11 | 0.168850 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.163694 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.163615 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.162137 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.160743 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.157427 | `azmcp_speech_stt_recognize` | ❌ |
| 17 | 0.154249 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.152324 | `azmcp_sql_db_create` | ❌ |
| 19 | 0.145124 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.139758 | `azmcp_storage_account_get` | ❌ |

---

## Test 323

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.299704 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271994 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.266039 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242673 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.218175 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.213237 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.179265 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.169701 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.164343 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.156446 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.156206 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.151386 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.142825 | `azmcp_marketplace_product_list` | ❌ |
| 14 | 0.139994 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.138040 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.132386 | `azmcp_mysql_database_query` | ❌ |
| 17 | 0.130149 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.123957 | `azmcp_storage_blob_upload` | ❌ |
| 19 | 0.119594 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.115064 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 324

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.420259 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.352797 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.323920 | `azmcp_storage_blob_upload` | ❌ |
| 5 | 0.310563 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.306967 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.304209 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.300392 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.299412 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.298989 | `azmcp_get_bestpractices_get` | ❌ |
| 11 | 0.293806 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.292455 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.291879 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.281982 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.276047 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.275550 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.272730 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.271357 | `azmcp_speech_stt_recognize` | ❌ |
| 19 | 0.261446 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.259814 | `azmcp_search_service_list` | ❌ |

---

## Summary

**Total Prompts Tested:** 324  
**Execution Time:** 88.3668136s  

### Success Rate Metrics

**Top Choice Success:** 82.1% (266/324 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 4.3% (14/324 tests)  
**🎯 High Confidence (≥0.7):** 18.8% (61/324 tests)  
**✅ Good Confidence (≥0.6):** 55.9% (181/324 tests)  
**👍 Fair Confidence (≥0.5):** 82.4% (267/324 tests)  
**👌 Acceptable Confidence (≥0.4):** 92.6% (300/324 tests)  
**❌ Low Confidence (<0.4):** 7.4% (24/324 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 4.3% (14/324 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 18.8% (61/324 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 53.1% (172/324 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 73.8% (239/324 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 79.3% (257/324 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing adequately but has room for improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

