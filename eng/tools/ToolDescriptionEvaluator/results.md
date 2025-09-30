# Tool Selection Analysis Setup

**Setup completed:** 2025-09-26 17:28:14  
**Tool count:** 143  
**Database setup time:** 1.2628797s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-26 17:28:14  
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
- [Test 127: azmcp_keyvault_certificate_create](#test-127)
- [Test 128: azmcp_keyvault_certificate_create](#test-128)
- [Test 129: azmcp_keyvault_certificate_create](#test-129)
- [Test 130: azmcp_keyvault_certificate_create](#test-130)
- [Test 131: azmcp_keyvault_certificate_get](#test-131)
- [Test 132: azmcp_keyvault_certificate_get](#test-132)
- [Test 133: azmcp_keyvault_certificate_get](#test-133)
- [Test 134: azmcp_keyvault_certificate_get](#test-134)
- [Test 135: azmcp_keyvault_certificate_get](#test-135)
- [Test 136: azmcp_keyvault_certificate_import](#test-136)
- [Test 137: azmcp_keyvault_certificate_import](#test-137)
- [Test 138: azmcp_keyvault_certificate_import](#test-138)
- [Test 139: azmcp_keyvault_certificate_import](#test-139)
- [Test 140: azmcp_keyvault_certificate_import](#test-140)
- [Test 141: azmcp_keyvault_certificate_list](#test-141)
- [Test 142: azmcp_keyvault_certificate_list](#test-142)
- [Test 143: azmcp_keyvault_certificate_list](#test-143)
- [Test 144: azmcp_keyvault_certificate_list](#test-144)
- [Test 145: azmcp_keyvault_certificate_list](#test-145)
- [Test 146: azmcp_keyvault_certificate_list](#test-146)
- [Test 147: azmcp_keyvault_key_create](#test-147)
- [Test 148: azmcp_keyvault_key_create](#test-148)
- [Test 149: azmcp_keyvault_key_create](#test-149)
- [Test 150: azmcp_keyvault_key_create](#test-150)
- [Test 151: azmcp_keyvault_key_create](#test-151)
- [Test 152: azmcp_keyvault_key_get](#test-152)
- [Test 153: azmcp_keyvault_key_get](#test-153)
- [Test 154: azmcp_keyvault_key_get](#test-154)
- [Test 155: azmcp_keyvault_key_get](#test-155)
- [Test 156: azmcp_keyvault_key_get](#test-156)
- [Test 157: azmcp_keyvault_key_list](#test-157)
- [Test 158: azmcp_keyvault_key_list](#test-158)
- [Test 159: azmcp_keyvault_key_list](#test-159)
- [Test 160: azmcp_keyvault_key_list](#test-160)
- [Test 161: azmcp_keyvault_key_list](#test-161)
- [Test 162: azmcp_keyvault_key_list](#test-162)
- [Test 163: azmcp_keyvault_secret_create](#test-163)
- [Test 164: azmcp_keyvault_secret_create](#test-164)
- [Test 165: azmcp_keyvault_secret_create](#test-165)
- [Test 166: azmcp_keyvault_secret_create](#test-166)
- [Test 167: azmcp_keyvault_secret_create](#test-167)
- [Test 168: azmcp_keyvault_secret_get](#test-168)
- [Test 169: azmcp_keyvault_secret_get](#test-169)
- [Test 170: azmcp_keyvault_secret_get](#test-170)
- [Test 171: azmcp_keyvault_secret_get](#test-171)
- [Test 172: azmcp_keyvault_secret_get](#test-172)
- [Test 173: azmcp_keyvault_secret_list](#test-173)
- [Test 174: azmcp_keyvault_secret_list](#test-174)
- [Test 175: azmcp_keyvault_secret_list](#test-175)
- [Test 176: azmcp_keyvault_secret_list](#test-176)
- [Test 177: azmcp_keyvault_secret_list](#test-177)
- [Test 178: azmcp_keyvault_secret_list](#test-178)
- [Test 179: azmcp_aks_cluster_get](#test-179)
- [Test 180: azmcp_aks_cluster_get](#test-180)
- [Test 181: azmcp_aks_cluster_get](#test-181)
- [Test 182: azmcp_aks_cluster_get](#test-182)
- [Test 183: azmcp_aks_cluster_list](#test-183)
- [Test 184: azmcp_aks_cluster_list](#test-184)
- [Test 185: azmcp_aks_cluster_list](#test-185)
- [Test 186: azmcp_aks_nodepool_get](#test-186)
- [Test 187: azmcp_aks_nodepool_get](#test-187)
- [Test 188: azmcp_aks_nodepool_get](#test-188)
- [Test 189: azmcp_aks_nodepool_list](#test-189)
- [Test 190: azmcp_aks_nodepool_list](#test-190)
- [Test 191: azmcp_aks_nodepool_list](#test-191)
- [Test 192: azmcp_loadtesting_test_create](#test-192)
- [Test 193: azmcp_loadtesting_test_get](#test-193)
- [Test 194: azmcp_loadtesting_testresource_create](#test-194)
- [Test 195: azmcp_loadtesting_testresource_list](#test-195)
- [Test 196: azmcp_loadtesting_testrun_create](#test-196)
- [Test 197: azmcp_loadtesting_testrun_get](#test-197)
- [Test 198: azmcp_loadtesting_testrun_list](#test-198)
- [Test 199: azmcp_loadtesting_testrun_update](#test-199)
- [Test 200: azmcp_grafana_list](#test-200)
- [Test 201: azmcp_azuremanagedlustre_filesystem_list](#test-201)
- [Test 202: azmcp_azuremanagedlustre_filesystem_list](#test-202)
- [Test 203: azmcp_azuremanagedlustre_filesystem_required-subnet-size](#test-203)
- [Test 204: azmcp_azuremanagedlustre_filesystem_sku_get](#test-204)
- [Test 205: azmcp_marketplace_product_get](#test-205)
- [Test 206: azmcp_marketplace_product_list](#test-206)
- [Test 207: azmcp_marketplace_product_list](#test-207)
- [Test 208: azmcp_bestpractices_get](#test-208)
- [Test 209: azmcp_bestpractices_get](#test-209)
- [Test 210: azmcp_bestpractices_get](#test-210)
- [Test 211: azmcp_bestpractices_get](#test-211)
- [Test 212: azmcp_bestpractices_get](#test-212)
- [Test 213: azmcp_bestpractices_get](#test-213)
- [Test 214: azmcp_bestpractices_get](#test-214)
- [Test 215: azmcp_bestpractices_get](#test-215)
- [Test 216: azmcp_bestpractices_get](#test-216)
- [Test 217: azmcp_bestpractices_get](#test-217)
- [Test 218: azmcp_monitor_healthmodels_entity_gethealth](#test-218)
- [Test 219: azmcp_monitor_metrics_definitions](#test-219)
- [Test 220: azmcp_monitor_metrics_definitions](#test-220)
- [Test 221: azmcp_monitor_metrics_definitions](#test-221)
- [Test 222: azmcp_monitor_metrics_query](#test-222)
- [Test 223: azmcp_monitor_metrics_query](#test-223)
- [Test 224: azmcp_monitor_metrics_query](#test-224)
- [Test 225: azmcp_monitor_metrics_query](#test-225)
- [Test 226: azmcp_monitor_metrics_query](#test-226)
- [Test 227: azmcp_monitor_metrics_query](#test-227)
- [Test 228: azmcp_monitor_resource_log_query](#test-228)
- [Test 229: azmcp_monitor_table_list](#test-229)
- [Test 230: azmcp_monitor_table_list](#test-230)
- [Test 231: azmcp_monitor_table_type_list](#test-231)
- [Test 232: azmcp_monitor_table_type_list](#test-232)
- [Test 233: azmcp_monitor_workspace_list](#test-233)
- [Test 234: azmcp_monitor_workspace_list](#test-234)
- [Test 235: azmcp_monitor_workspace_list](#test-235)
- [Test 236: azmcp_monitor_workspace_log_query](#test-236)
- [Test 237: azmcp_datadog_monitoredresources_list](#test-237)
- [Test 238: azmcp_datadog_monitoredresources_list](#test-238)
- [Test 239: azmcp_extension_azqr](#test-239)
- [Test 240: azmcp_extension_azqr](#test-240)
- [Test 241: azmcp_extension_azqr](#test-241)
- [Test 242: azmcp_quota_region_availability_list](#test-242)
- [Test 243: azmcp_quota_usage_check](#test-243)
- [Test 244: azmcp_role_assignment_list](#test-244)
- [Test 245: azmcp_role_assignment_list](#test-245)
- [Test 246: azmcp_redis_cache_accesspolicy_list](#test-246)
- [Test 247: azmcp_redis_cache_accesspolicy_list](#test-247)
- [Test 248: azmcp_redis_cache_list](#test-248)
- [Test 249: azmcp_redis_cache_list](#test-249)
- [Test 250: azmcp_redis_cache_list](#test-250)
- [Test 251: azmcp_redis_cluster_database_list](#test-251)
- [Test 252: azmcp_redis_cluster_database_list](#test-252)
- [Test 253: azmcp_redis_cluster_list](#test-253)
- [Test 254: azmcp_redis_cluster_list](#test-254)
- [Test 255: azmcp_redis_cluster_list](#test-255)
- [Test 256: azmcp_group_list](#test-256)
- [Test 257: azmcp_group_list](#test-257)
- [Test 258: azmcp_group_list](#test-258)
- [Test 259: azmcp_resourcehealth_availability-status_get](#test-259)
- [Test 260: azmcp_resourcehealth_availability-status_get](#test-260)
- [Test 261: azmcp_resourcehealth_availability-status_get](#test-261)
- [Test 262: azmcp_resourcehealth_availability-status_list](#test-262)
- [Test 263: azmcp_resourcehealth_availability-status_list](#test-263)
- [Test 264: azmcp_resourcehealth_availability-status_list](#test-264)
- [Test 265: azmcp_resourcehealth_service-health-events_list](#test-265)
- [Test 266: azmcp_resourcehealth_service-health-events_list](#test-266)
- [Test 267: azmcp_resourcehealth_service-health-events_list](#test-267)
- [Test 268: azmcp_resourcehealth_service-health-events_list](#test-268)
- [Test 269: azmcp_resourcehealth_service-health-events_list](#test-269)
- [Test 270: azmcp_servicebus_queue_details](#test-270)
- [Test 271: azmcp_servicebus_topic_details](#test-271)
- [Test 272: azmcp_servicebus_topic_subscription_details](#test-272)
- [Test 273: azmcp_sql_db_create](#test-273)
- [Test 274: azmcp_sql_db_create](#test-274)
- [Test 275: azmcp_sql_db_create](#test-275)
- [Test 276: azmcp_sql_db_delete](#test-276)
- [Test 277: azmcp_sql_db_delete](#test-277)
- [Test 278: azmcp_sql_db_delete](#test-278)
- [Test 279: azmcp_sql_db_list](#test-279)
- [Test 280: azmcp_sql_db_list](#test-280)
- [Test 281: azmcp_sql_db_show](#test-281)
- [Test 282: azmcp_sql_db_show](#test-282)
- [Test 283: azmcp_sql_db_update](#test-283)
- [Test 284: azmcp_sql_db_update](#test-284)
- [Test 285: azmcp_sql_elastic-pool_list](#test-285)
- [Test 286: azmcp_sql_elastic-pool_list](#test-286)
- [Test 287: azmcp_sql_elastic-pool_list](#test-287)
- [Test 288: azmcp_sql_server_create](#test-288)
- [Test 289: azmcp_sql_server_create](#test-289)
- [Test 290: azmcp_sql_server_create](#test-290)
- [Test 291: azmcp_sql_server_delete](#test-291)
- [Test 292: azmcp_sql_server_delete](#test-292)
- [Test 293: azmcp_sql_server_delete](#test-293)
- [Test 294: azmcp_sql_server_entra-admin_list](#test-294)
- [Test 295: azmcp_sql_server_entra-admin_list](#test-295)
- [Test 296: azmcp_sql_server_entra-admin_list](#test-296)
- [Test 297: azmcp_sql_server_firewall-rule_create](#test-297)
- [Test 298: azmcp_sql_server_firewall-rule_create](#test-298)
- [Test 299: azmcp_sql_server_firewall-rule_create](#test-299)
- [Test 300: azmcp_sql_server_firewall-rule_delete](#test-300)
- [Test 301: azmcp_sql_server_firewall-rule_delete](#test-301)
- [Test 302: azmcp_sql_server_firewall-rule_delete](#test-302)
- [Test 303: azmcp_sql_server_firewall-rule_list](#test-303)
- [Test 304: azmcp_sql_server_firewall-rule_list](#test-304)
- [Test 305: azmcp_sql_server_firewall-rule_list](#test-305)
- [Test 306: azmcp_sql_server_list](#test-306)
- [Test 307: azmcp_sql_server_list](#test-307)
- [Test 308: azmcp_sql_server_show](#test-308)
- [Test 309: azmcp_sql_server_show](#test-309)
- [Test 310: azmcp_sql_server_show](#test-310)
- [Test 311: azmcp_storage_account_create](#test-311)
- [Test 312: azmcp_storage_account_create](#test-312)
- [Test 313: azmcp_storage_account_create](#test-313)
- [Test 314: azmcp_storage_account_get](#test-314)
- [Test 315: azmcp_storage_account_get](#test-315)
- [Test 316: azmcp_storage_account_get](#test-316)
- [Test 317: azmcp_storage_account_get](#test-317)
- [Test 318: azmcp_storage_account_get](#test-318)
- [Test 319: azmcp_storage_blob_container_create](#test-319)
- [Test 320: azmcp_storage_blob_container_create](#test-320)
- [Test 321: azmcp_storage_blob_container_create](#test-321)
- [Test 322: azmcp_storage_blob_container_get](#test-322)
- [Test 323: azmcp_storage_blob_container_get](#test-323)
- [Test 324: azmcp_storage_blob_container_get](#test-324)
- [Test 325: azmcp_storage_blob_get](#test-325)
- [Test 326: azmcp_storage_blob_get](#test-326)
- [Test 327: azmcp_storage_blob_get](#test-327)
- [Test 328: azmcp_storage_blob_get](#test-328)
- [Test 329: azmcp_storage_blob_upload](#test-329)
- [Test 330: azmcp_subscription_list](#test-330)
- [Test 331: azmcp_subscription_list](#test-331)
- [Test 332: azmcp_subscription_list](#test-332)
- [Test 333: azmcp_subscription_list](#test-333)
- [Test 334: azmcp_azureterraformbestpractices_get](#test-334)
- [Test 335: azmcp_azureterraformbestpractices_get](#test-335)
- [Test 336: azmcp_virtualdesktop_hostpool_list](#test-336)
- [Test 337: azmcp_virtualdesktop_hostpool_sessionhost_list](#test-337)
- [Test 338: azmcp_virtualdesktop_hostpool_sessionhost_usersession-list](#test-338)
- [Test 339: azmcp_workbooks_create](#test-339)
- [Test 340: azmcp_workbooks_delete](#test-340)
- [Test 341: azmcp_workbooks_list](#test-341)
- [Test 342: azmcp_workbooks_list](#test-342)
- [Test 343: azmcp_workbooks_show](#test-343)
- [Test 344: azmcp_workbooks_show](#test-344)
- [Test 345: azmcp_workbooks_update](#test-345)
- [Test 346: azmcp_bicepschema_get](#test-346)
- [Test 347: azmcp_cloudarchitect_design](#test-347)
- [Test 348: azmcp_cloudarchitect_design](#test-348)
- [Test 349: azmcp_cloudarchitect_design](#test-349)
- [Test 350: azmcp_cloudarchitect_design](#test-350)

---

## Test 1

**Expected Tool:** `azmcp_foundry_agents_connect`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603124 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.535829 | `azmcp_foundry_agents_connect` | ✅ **EXPECTED** |
| 3 | 0.494462 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.443558 | `azmcp_foundry_agents_evaluate` | ❌ |
| 5 | 0.379587 | `azmcp_search_index_query` | ❌ |
| 6 | 0.365856 | `azmcp_foundry_models_list` | ❌ |
| 7 | 0.355385 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 8 | 0.327613 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.319855 | `azmcp_foundry_models_deploy` | ❌ |
| 10 | 0.305579 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.297391 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 12 | 0.272398 | `azmcp_search_service_list` | ❌ |
| 13 | 0.243499 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.241241 | `azmcp_postgres_database_query` | ❌ |
| 15 | 0.232656 | `azmcp_search_index_get` | ❌ |
| 16 | 0.230797 | `azmcp_mysql_server_list` | ❌ |
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
| 1 | 0.544099 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469980 | `azmcp_foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.356465 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.280833 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.235412 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.233739 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.233415 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.232102 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.228525 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.224884 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.221027 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.218372 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.214507 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.210219 | `azmcp_search_index_query` | ❌ |
| 15 | 0.207677 | `azmcp_postgres_database_query` | ❌ |
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
| 1 | 0.580566 | `azmcp_foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 2 | 0.519050 | `azmcp_foundry_agents_evaluate` | ❌ |
| 3 | 0.471059 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.381887 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.315849 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.315347 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.308767 | `azmcp_foundry_models_deploy` | ❌ |
| 8 | 0.276459 | `azmcp_foundry_models_list` | ❌ |
| 9 | 0.253361 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 10 | 0.246328 | `azmcp_search_index_query` | ❌ |
| 11 | 0.231512 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.207748 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.188340 | `azmcp_monitor_workspace_log_query` | ❌ |
| 14 | 0.183834 | `azmcp_postgres_database_query` | ❌ |
| 15 | 0.179159 | `azmcp_search_service_list` | ❌ |
| 16 | 0.166181 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.163139 | `azmcp_sql_server_entra-admin_list` | ❌ |
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
| 1 | 0.695201 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.532985 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.526485 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 4 | 0.433117 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.422779 | `azmcp_search_index_get` | ❌ |
| 6 | 0.412895 | `azmcp_search_service_list` | ❌ |
| 7 | 0.349506 | `azmcp_search_index_query` | ❌ |
| 8 | 0.329682 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.310470 | `azmcp_foundry_models_deployments_list` | ❌ |
| 10 | 0.309513 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.296877 | `azmcp_grafana_list` | ❌ |
| 12 | 0.294101 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.291635 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.286074 | `azmcp_monitor_table_type_list` | ❌ |
| 15 | 0.270303 | `azmcp_redis_cache_list` | ❌ |
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
| 1 | 0.603362 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489302 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.473963 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.396806 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.374683 | `azmcp_search_index_get` | ❌ |
| 6 | 0.350742 | `azmcp_search_service_list` | ❌ |
| 7 | 0.341852 | `azmcp_search_index_query` | ❌ |
| 8 | 0.317973 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.310597 | `azmcp_foundry_models_deploy` | ❌ |
| 10 | 0.278143 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.276854 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.272218 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.256208 | `azmcp_grafana_list` | ❌ |
| 14 | 0.250431 | `azmcp_foundry_agents_connect` | ❌ |
| 15 | 0.232856 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.225269 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.224181 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.223832 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.223736 | `azmcp_monitor_metrics_definitions` | ❌ |
| 20 | 0.218039 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 6

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672563 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.564860 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.424581 | `azmcp_search_index_get` | ❌ |
| 4 | 0.397225 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.375275 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.363951 | `azmcp_kusto_table_schema` | ❌ |
| 7 | 0.358315 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.349967 | `azmcp_search_index_query` | ❌ |
| 9 | 0.347762 | `azmcp_foundry_models_list` | ❌ |
| 10 | 0.346329 | `azmcp_monitor_table_list` | ❌ |
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
| 1 | 0.650243 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432759 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.415963 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.408316 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.398186 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.379800 | `azmcp_search_index_get` | ❌ |
| 7 | 0.352243 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.318648 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.311623 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.309927 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.286991 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.271893 | `azmcp_aks_cluster_get` | ❌ |
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
| 3 | 0.274011 | `azmcp_deploy_plan_get` | ❌ |
| 4 | 0.269848 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.268967 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.234019 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.222504 | `azmcp_grafana_list` | ❌ |
| 8 | 0.222478 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.221635 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.217001 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.216588 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 12 | 0.215255 | `azmcp_loadtesting_testrun_create` | ❌ |
| 13 | 0.209865 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.208124 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.207601 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.204420 | `azmcp_postgres_server_param_set` | ❌ |
| 17 | 0.197001 | `azmcp_loadtesting_testrun_update` | ❌ |
| 18 | 0.195615 | `azmcp_workbooks_list` | ❌ |
| 19 | 0.192764 | `azmcp_monitor_metrics_query` | ❌ |
| 20 | 0.192373 | `azmcp_storage_account_create` | ❌ |

---

## Test 9

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559508 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.549636 | `azmcp_foundry_models_list` | ❌ |
| 3 | 0.539695 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.533239 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.448711 | `azmcp_search_service_list` | ❌ |
| 6 | 0.434472 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 7 | 0.368173 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.334867 | `azmcp_grafana_list` | ❌ |
| 9 | 0.332002 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.328215 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 11 | 0.326752 | `azmcp_search_index_get` | ❌ |
| 12 | 0.320998 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.318854 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.310280 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.302262 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.301302 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.300357 | `azmcp_search_index_query` | ❌ |
| 18 | 0.289448 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.288342 | `azmcp_redis_cache_list` | ❌ |
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
| 7 | 0.328814 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.311234 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 9 | 0.305997 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.301476 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.298821 | `azmcp_search_index_query` | ❌ |
| 12 | 0.291256 | `azmcp_search_index_get` | ❌ |
| 13 | 0.286814 | `azmcp_grafana_list` | ❌ |
| 14 | 0.269912 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.254926 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.250392 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.246893 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.243133 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.236572 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.234179 | `azmcp_redis_cache_list` | ❌ |

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
| 7 | 0.298648 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.290552 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 9 | 0.285437 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.277883 | `azmcp_grafana_list` | ❌ |
| 11 | 0.275316 | `azmcp_search_index_get` | ❌ |
| 12 | 0.273026 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.265730 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.255790 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.255760 | `azmcp_search_index_query` | ❌ |
| 16 | 0.252297 | `azmcp_postgres_database_list` | ❌ |
| 17 | 0.248741 | `azmcp_redis_cache_list` | ❌ |
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
| 2 | 0.475139 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.430513 | `azmcp_foundry_models_deploy` | ❌ |
| 4 | 0.388967 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.356899 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.339069 | `azmcp_search_service_list` | ❌ |
| 7 | 0.299212 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 8 | 0.283250 | `azmcp_search_index_query` | ❌ |
| 9 | 0.280061 | `azmcp_foundry_agents_connect` | ❌ |
| 10 | 0.274019 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.266937 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.261834 | `azmcp_search_index_get` | ❌ |
| 13 | 0.260144 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.245943 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.244697 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.240274 | `azmcp_monitor_metrics_definitions` | ❌ |
| 17 | 0.234050 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.217331 | `azmcp_marketplace_product_list` | ❌ |
| 19 | 0.211456 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.207870 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681052 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544460 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.490624 | `azmcp_search_service_list` | ❌ |
| 4 | 0.466005 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.459609 | `azmcp_search_index_query` | ❌ |
| 6 | 0.393556 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.388240 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.379706 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.372382 | `azmcp_marketplace_product_get` | ❌ |
| 10 | 0.370915 | `azmcp_mysql_table_schema_get` | ❌ |
| 11 | 0.358541 | `azmcp_keyvault_secret_get` | ❌ |
| 12 | 0.358315 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.356252 | `azmcp_sql_db_show` | ❌ |
| 14 | 0.355785 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.354845 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.353617 | `azmcp_keyvault_certificate_get` | ❌ |
| 17 | 0.351685 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.351083 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.348263 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.343186 | `azmcp_aks_nodepool_get` | ❌ |

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
| 5 | 0.453047 | `azmcp_foundry_agents_list` | ❌ |
| 6 | 0.445327 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 7 | 0.439452 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.417838 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.416404 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.412109 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.409307 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.406485 | `azmcp_monitor_table_type_list` | ❌ |
| 13 | 0.397423 | `azmcp_mysql_database_list` | ❌ |
| 14 | 0.387174 | `azmcp_keyvault_secret_list` | ❌ |
| 15 | 0.378750 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.378297 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.375372 | `azmcp_foundry_models_deployments_list` | ❌ |
| 18 | 0.371099 | `azmcp_mysql_table_list` | ❌ |
| 19 | 0.367804 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.367416 | `azmcp_redis_cache_list` | ❌ |

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
| 5 | 0.463814 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.408569 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.401807 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.382692 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.372639 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.370330 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.367868 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.355910 | `azmcp_keyvault_key_list` | ❌ |
| 13 | 0.351788 | `azmcp_foundry_models_deployments_list` | ❌ |
| 14 | 0.351161 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.350043 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.349605 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.347566 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.346994 | `azmcp_mysql_table_list` | ❌ |
| 19 | 0.341728 | `azmcp_foundry_models_list` | ❌ |
| 20 | 0.328039 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 16

**Expected Tool:** `azmcp_search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522807 | `azmcp_search_index_get` | ❌ |
| 2 | 0.515909 | `azmcp_search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497441 | `azmcp_search_service_list` | ❌ |
| 4 | 0.373932 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.372752 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 6 | 0.327104 | `azmcp_kusto_query` | ❌ |
| 7 | 0.322326 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.311080 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.307299 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.305893 | `azmcp_marketplace_product_list` | ❌ |
| 11 | 0.295388 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.291381 | `azmcp_foundry_agents_connect` | ❌ |
| 13 | 0.290223 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.288238 | `azmcp_foundry_models_deployments_list` | ❌ |
| 15 | 0.287504 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.283561 | `azmcp_foundry_models_list` | ❌ |
| 17 | 0.275029 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.269900 | `azmcp_monitor_table_list` | ❌ |
| 19 | 0.259752 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.244907 | `azmcp_kusto_sample` | ❌ |

---

## Test 17

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.520340 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.505971 | `azmcp_search_index_get` | ❌ |
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
| 14 | 0.451886 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.443495 | `azmcp_search_index_query` | ❌ |
| 16 | 0.431621 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.427923 | `azmcp_group_list` | ❌ |
| 18 | 0.425463 | `azmcp_resourcehealth_availability-status_list` | ❌ |
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
| 2 | 0.479898 | `azmcp_search_index_get` | ❌ |
| 3 | 0.467337 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.453489 | `azmcp_marketplace_product_list` | ❌ |
| 5 | 0.448446 | `azmcp_search_index_query` | ❌ |
| 6 | 0.425939 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.419493 | `azmcp_marketplace_product_get` | ❌ |
| 8 | 0.412158 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.408456 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.400284 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.399822 | `azmcp_grafana_list` | ❌ |
| 12 | 0.397883 | `azmcp_foundry_models_deployments_list` | ❌ |
| 13 | 0.393708 | `azmcp_subscription_list` | ❌ |
| 14 | 0.391071 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.390559 | `azmcp_foundry_models_list` | ❌ |
| 16 | 0.389433 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.379805 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 18 | 0.376089 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.373463 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.363429 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 19

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553025 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.436230 | `azmcp_search_index_get` | ❌ |
| 3 | 0.417096 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.404758 | `azmcp_search_index_query` | ❌ |
| 5 | 0.344699 | `azmcp_foundry_models_deployments_list` | ❌ |
| 6 | 0.336174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.322580 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 8 | 0.322540 | `azmcp_foundry_models_list` | ❌ |
| 9 | 0.300427 | `azmcp_marketplace_product_list` | ❌ |
| 10 | 0.292677 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.290360 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.290214 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.283366 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.282246 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.281672 | `azmcp_get_bestpractices_get` | ❌ |
| 16 | 0.281134 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.278601 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.278574 | `azmcp_cloudarchitect_design` | ❌ |
| 19 | 0.277693 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.275013 | `azmcp_sql_server_show` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.635561 | `azmcp_appconfig_kv_list` | ❌ |
| 3 | 0.492173 | `azmcp_redis_cache_list` | ❌ |
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
| 16 | 0.398507 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.389537 | `azmcp_foundry_agents_list` | ❌ |
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
| 6 | 0.368753 | `azmcp_redis_cache_list` | ❌ |
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
| 5 | 0.348661 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.327234 | `azmcp_appconfig_kv_set` | ❌ |
| 7 | 0.289682 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 8 | 0.282153 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.272373 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.255774 | `azmcp_mysql_server_param_get` | ❌ |
| 11 | 0.250457 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.239130 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.236387 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.234890 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.233357 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.231688 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.228042 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.225851 | `azmcp_sql_db_update` | ❌ |
| 19 | 0.221645 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.221405 | `azmcp_postgres_database_list` | ❌ |

---

## Test 23

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
| 8 | 0.259907 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.251994 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.230464 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.218487 | `azmcp_mysql_server_param_set` | ❌ |
| 12 | 0.218373 | `azmcp_sql_server_delete` | ❌ |
| 13 | 0.203182 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.196121 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 15 | 0.194933 | `azmcp_sql_db_delete` | ❌ |
| 16 | 0.194831 | `azmcp_postgres_server_config_get` | ❌ |
| 17 | 0.183461 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.175403 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.173143 | `azmcp_postgres_server_param_set` | ❌ |
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
| 5 | 0.464635 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.439089 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.377534 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.356156 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.333355 | `azmcp_mysql_server_param_get` | ❌ |
| 10 | 0.327550 | `azmcp_loadtesting_testrun_list` | ❌ |
| 11 | 0.323615 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.317744 | `azmcp_mysql_server_config_get` | ❌ |
| 13 | 0.308699 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.302700 | `azmcp_loadtesting_test_get` | ❌ |
| 15 | 0.296084 | `azmcp_postgres_table_list` | ❌ |
| 16 | 0.292127 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.275469 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.267026 | `azmcp_postgres_database_list` | ❌ |
| 19 | 0.265694 | `azmcp_sql_db_update` | ❌ |
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
| 5 | 0.468503 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.457866 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.370520 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.356793 | `azmcp_mysql_server_param_get` | ❌ |
| 9 | 0.317662 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.316093 | `azmcp_keyvault_key_get` | ❌ |
| 11 | 0.314774 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.304557 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.288088 | `azmcp_mysql_server_param_set` | ❌ |
| 14 | 0.278909 | `azmcp_loadtesting_testrun_list` | ❌ |
| 15 | 0.277848 | `azmcp_keyvault_secret_get` | ❌ |
| 16 | 0.269354 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.258688 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.249105 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.243655 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.238151 | `azmcp_sql_server_show` | ❌ |

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
| 4 | 0.431516 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.423650 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.373656 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.253705 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.250821 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.249335 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.247883 | `azmcp_keyvault_key_get` | ❌ |
| 11 | 0.238242 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.211331 | `azmcp_postgres_server_config_get` | ❌ |
| 13 | 0.210593 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.185346 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.163738 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.158946 | `azmcp_mysql_server_param_get` | ❌ |
| 17 | 0.154529 | `azmcp_postgres_server_param_get` | ❌ |
| 18 | 0.144377 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.139871 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.134175 | `azmcp_loadtesting_testrun_update` | ❌ |

---

## Test 27

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
| 7 | 0.293442 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.262796 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.240190 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.237098 | `azmcp_mysql_server_param_set` | ❌ |
| 11 | 0.235691 | `azmcp_keyvault_secret_get` | ❌ |
| 12 | 0.225350 | `azmcp_postgres_server_config_get` | ❌ |
| 13 | 0.190554 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.190136 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.185141 | `azmcp_postgres_server_param_set` | ❌ |
| 16 | 0.179797 | `azmcp_mysql_server_param_get` | ❌ |
| 17 | 0.171375 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.159767 | `azmcp_postgres_server_param_get` | ❌ |
| 19 | 0.149672 | `azmcp_storage_blob_container_get` | ❌ |
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
| 5 | 0.505571 | `azmcp_appconfig_kv_delete` | ❌ |
| 6 | 0.377919 | `azmcp_appconfig_account_list` | ❌ |
| 7 | 0.360015 | `azmcp_mysql_server_param_set` | ❌ |
| 8 | 0.346927 | `azmcp_postgres_server_param_set` | ❌ |
| 9 | 0.330526 | `azmcp_keyvault_secret_create` | ❌ |
| 10 | 0.287544 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.276094 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.262903 | `azmcp_sql_db_update` | ❌ |
| 13 | 0.258726 | `azmcp_keyvault_key_get` | ❌ |
| 14 | 0.213592 | `azmcp_mysql_server_param_get` | ❌ |
| 15 | 0.208947 | `azmcp_postgres_server_config_get` | ❌ |
| 16 | 0.201882 | `azmcp_loadtesting_testrun_update` | ❌ |
| 17 | 0.193989 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.167006 | `azmcp_postgres_server_param_get` | ❌ |
| 19 | 0.164376 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.137964 | `azmcp_storage_account_create` | ❌ |

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
| 4 | 0.441713 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.437432 | `azmcp_appconfig_account_list` | ❌ |
| 6 | 0.416264 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 7 | 0.360113 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.315208 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.291448 | `azmcp_postgres_server_config_get` | ❌ |
| 10 | 0.269387 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.259549 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.257940 | `azmcp_mysql_server_param_get` | ❌ |
| 13 | 0.251822 | `azmcp_loadtesting_testrun_list` | ❌ |
| 14 | 0.229242 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.225141 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.217856 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.206445 | `azmcp_redis_cache_list` | ❌ |
| 18 | 0.201872 | `azmcp_mysql_server_param_set` | ❌ |
| 19 | 0.186734 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.185986 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 30

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.355622 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.329354 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.257790 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.216077 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.206477 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.205235 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.193032 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 9 | 0.181894 | `azmcp_foundry_agents_evaluate` | ❌ |
| 10 | 0.177942 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.169553 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.163658 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.148018 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.141970 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.132884 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.128768 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.125631 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.120066 | `azmcp_mysql_table_schema_get` | ❌ |
| 19 | 0.116209 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.111783 | `azmcp_redis_cache_list` | ❌ |

---

## Test 31

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.318582 | `azmcp_deploy_app_logs_get` | ❌ |
| 2 | 0.302501 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.255570 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.225972 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.222234 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.200402 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.199366 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.186927 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.172691 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.163364 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.162857 | `azmcp_foundry_agents_evaluate` | ❌ |
| 12 | 0.150964 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.150313 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.144054 | `azmcp_mysql_server_param_get` | ❌ |
| 15 | 0.132993 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.125889 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.118881 | `azmcp_mysql_table_schema_get` | ❌ |
| 18 | 0.112992 | `azmcp_monitor_workspace_log_query` | ❌ |
| 19 | 0.107063 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.101787 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 32

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.256325 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 2 | 0.250432 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.215848 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.198999 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.188245 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.188050 | `azmcp_appservice_database_add` | ❌ |
| 7 | 0.179320 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.178879 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.159064 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.158352 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.156599 | `azmcp_search_service_list` | ❌ |
| 12 | 0.156560 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 13 | 0.153379 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.151702 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.146689 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.139604 | `azmcp_postgres_server_param_get` | ❌ |
| 17 | 0.130326 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.129424 | `azmcp_mysql_server_param_get` | ❌ |
| 19 | 0.126169 | `azmcp_search_index_get` | ❌ |
| 20 | 0.125614 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 33

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a database connection to my app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729119 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.398617 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.368252 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.364437 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.361951 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.353953 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.348774 | `azmcp_sql_server_create` | ❌ |
| 8 | 0.342556 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.342352 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.334383 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.301680 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.300846 | `azmcp_mysql_database_list` | ❌ |
| 13 | 0.298638 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.286125 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.281814 | `azmcp_loadtesting_testresource_create` | ❌ |
| 16 | 0.280123 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.266282 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.264904 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.256870 | `azmcp_keyvault_secret_create` | ❌ |
| 20 | 0.254975 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 34

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure a SQL Server database for app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612148 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.484312 | `azmcp_sql_db_update` | ❌ |
| 3 | 0.471103 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.408878 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.405300 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.389144 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.381822 | `azmcp_mysql_server_config_get` | ❌ |
| 8 | 0.367325 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.366390 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.355360 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.352382 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.350677 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.345345 | `azmcp_sql_db_delete` | ❌ |
| 14 | 0.340399 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.329197 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.322825 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.316005 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.304744 | `azmcp_loadtesting_test_create` | ❌ |
| 19 | 0.299644 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.295124 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 35

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a MySQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648502 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.418902 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.409593 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.382602 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.351839 | `azmcp_mysql_table_list` | ❌ |
| 6 | 0.345957 | `azmcp_sql_db_update` | ❌ |
| 7 | 0.344869 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.335323 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.323158 | `azmcp_mysql_database_query` | ❌ |
| 10 | 0.320639 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.314492 | `azmcp_mysql_server_param_set` | ❌ |
| 12 | 0.311349 | `azmcp_sql_db_show` | ❌ |
| 13 | 0.297738 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.295428 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.279702 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.272652 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.272634 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
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
| 1 | 0.579532 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449085 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.439679 | `azmcp_postgres_database_query` | ❌ |
| 4 | 0.409515 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.405431 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.399782 | `azmcp_postgres_server_param_set` | ❌ |
| 7 | 0.383413 | `azmcp_sql_db_create` | ❌ |
| 8 | 0.337005 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.328855 | `azmcp_postgres_server_param_get` | ❌ |
| 10 | 0.305507 | `azmcp_sql_db_update` | ❌ |
| 11 | 0.302980 | `azmcp_sql_db_list` | ❌ |
| 12 | 0.289343 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.279654 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.258603 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.257729 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.254307 | `azmcp_kusto_table_list` | ❌ |
| 17 | 0.241522 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
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
| 1 | 0.643070 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.477031 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.465637 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.421268 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.400458 | `azmcp_sql_db_update` | ❌ |
| 6 | 0.378402 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.374251 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.370137 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.362494 | `azmcp_sql_db_show` | ❌ |
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
| 20 | 0.292770 | `azmcp_sql_server_create` | ❌ |

---

## Test 38

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645562 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.489228 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.423910 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.422266 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.394910 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.394433 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.381822 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.368592 | `azmcp_postgres_database_list` | ❌ |
| 9 | 0.360144 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.357354 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.349820 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.348615 | `azmcp_sql_db_update` | ❌ |
| 13 | 0.348100 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.346009 | `azmcp_sql_db_delete` | ❌ |
| 15 | 0.304416 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.281301 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.277310 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.274848 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 19 | 0.274590 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.266423 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 39

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665268 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.371277 | `azmcp_sql_db_update` | ❌ |
| 3 | 0.369071 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.332119 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.314270 | `azmcp_cosmos_database_list` | ❌ |
| 6 | 0.312395 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.307420 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.304622 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.297194 | `azmcp_mysql_server_param_get` | ❌ |
| 10 | 0.294182 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.292606 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.286149 | `azmcp_postgres_server_param_set` | ❌ |
| 13 | 0.273579 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.269033 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.268113 | `azmcp_keyvault_secret_create` | ❌ |
| 16 | 0.267621 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.267098 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.266587 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.265629 | `azmcp_sql_db_delete` | ❌ |
| 20 | 0.260212 | `azmcp_functionapp_get` | ❌ |

---

## Test 40

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536758 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.394572 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.391857 | `azmcp_sql_db_update` | ❌ |
| 4 | 0.329110 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.318461 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.318263 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.305550 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.301240 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.298453 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.298122 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.297607 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.295901 | `azmcp_subscription_list` | ❌ |
| 13 | 0.294831 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.290182 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.280891 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.274754 | `azmcp_sql_db_delete` | ❌ |
| 17 | 0.274380 | `azmcp_sql_server_delete` | ❌ |
| 18 | 0.273430 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.272238 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.267064 | `azmcp_mysql_server_list` | ❌ |

---

## Test 41

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560288 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.426753 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.361028 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.349556 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.346672 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.345158 | `azmcp_sql_db_update` | ❌ |
| 7 | 0.342276 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.339789 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.339459 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.330944 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.317003 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.292346 | `azmcp_sql_server_delete` | ❌ |
| 13 | 0.281774 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.277068 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.270334 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.268258 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.263797 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.257394 | `azmcp_mysql_table_list` | ❌ |
| 19 | 0.257248 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.253565 | `azmcp_deploy_plan_get` | ❌ |

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
| 4 | 0.385409 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.375293 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.357934 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.352902 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.346020 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.344858 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.330014 | `azmcp_search_service_list` | ❌ |
| 11 | 0.326102 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.297036 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.296190 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.268844 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.265955 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.263811 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.260352 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.258617 | `azmcp_monitor_table_list` | ❌ |
| 19 | 0.248483 | `azmcp_search_index_get` | ❌ |
| 20 | 0.245697 | `azmcp_redis_cache_list` | ❌ |

---

## Test 43

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696531 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.468384 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.452173 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.435241 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.424622 | `azmcp_search_service_list` | ❌ |
| 6 | 0.405520 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.405253 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.401105 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.393786 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.387892 | `azmcp_deploy_plan_get` | ❌ |
| 11 | 0.380224 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.371654 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.367714 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.367243 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.362902 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.355398 | `azmcp_redis_cluster_list` | ❌ |
| 17 | 0.339417 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.336797 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.334552 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.332071 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

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
| 6 | 0.451846 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.449821 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.446454 | `azmcp_search_service_list` | ❌ |
| 9 | 0.419715 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.417639 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.416057 | `azmcp_get_bestpractices_get` | ❌ |
| 12 | 0.415557 | `azmcp_monitor_metrics_definitions` | ❌ |
| 13 | 0.407374 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.401177 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.401135 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.398757 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.389786 | `azmcp_monitor_table_type_list` | ❌ |
| 18 | 0.388734 | `azmcp_group_list` | ❌ |
| 19 | 0.386954 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.385121 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 45

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.398321 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.383767 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.369053 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.367278 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.341619 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.325789 | `azmcp_deploy_iac_rules_get` | ❌ |
| 8 | 0.324496 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.321854 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.313589 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.287677 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.285234 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.262685 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.259246 | `azmcp_search_service_list` | ❌ |
| 15 | 0.254871 | `azmcp_search_index_query` | ❌ |
| 16 | 0.247378 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
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
| 1 | 0.743533 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.541506 | `azmcp_search_service_list` | ❌ |
| 4 | 0.527511 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.515937 | `azmcp_subscription_list` | ❌ |
| 6 | 0.514293 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.509386 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.503032 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.490776 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.487256 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.483500 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.482236 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.481776 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.480883 | `azmcp_group_list` | ❌ |
| 15 | 0.469958 | `azmcp_datadog_monitoredresources_list` | ❌ |
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
| 1 | 0.585968 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.449642 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.382728 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.373107 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.372153 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.370858 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.364918 | `azmcp_search_service_list` | ❌ |
| 10 | 0.359160 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.356444 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.354277 | `azmcp_storage_blob_container_create` | ❌ |
| 13 | 0.353379 | `azmcp_subscription_list` | ❌ |
| 14 | 0.352818 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.349526 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.349291 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.347970 | `azmcp_storage_blob_get` | ❌ |
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
| 1 | 0.637158 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563476 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.474039 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471804 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.463742 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.459880 | `azmcp_search_service_list` | ❌ |
| 7 | 0.452938 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.451253 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.443939 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.440464 | `azmcp_subscription_list` | ❌ |
| 11 | 0.435835 | `azmcp_grafana_list` | ❌ |
| 12 | 0.435314 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.431745 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.430961 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.430308 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.419749 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.404718 | `azmcp_group_list` | ❌ |
| 18 | 0.398556 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.386495 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.364214 | `azmcp_mysql_server_list` | ❌ |

---

## Test 49

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `azmcp_acr_registry_repository_list` | ❌ |
| 2 | 0.633923 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
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
| 13 | 0.403816 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.388773 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.378482 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.371025 | `azmcp_sql_elastic-pool_list` | ❌ |
| 17 | 0.370359 | `azmcp_redis_cluster_database_list` | ❌ |
| 18 | 0.356119 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.354145 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.352288 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 50

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639357 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.468028 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.449649 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.445716 | `azmcp_group_list` | ❌ |
| 6 | 0.416353 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.413975 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.413191 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.406554 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.403242 | `azmcp_storage_blob_container_get` | ❌ |
| 11 | 0.400209 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.389603 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.378353 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.369912 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.369779 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.367738 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.355657 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.354772 | `azmcp_loadtesting_testresource_list` | ❌ |
| 19 | 0.351411 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.347199 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 51

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617537 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.510459 | `azmcp_redis_cache_list` | ❌ |
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
| 14 | 0.438302 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.437125 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.431019 | `azmcp_group_list` | ❌ |
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
| 2 | 0.469284 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.407973 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.399537 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.339307 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.326631 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.308650 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.306819 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.306442 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.302660 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.300174 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.295832 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.292155 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.290148 | `azmcp_redis_cluster_list` | ❌ |
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
| 2 | 0.541785 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.433927 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.387979 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.370375 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.359617 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.356901 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.355380 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.351007 | `azmcp_redis_cluster_database_list` | ❌ |
| 10 | 0.347437 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.347084 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.340014 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.332785 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.332704 | `azmcp_sql_db_list` | ❌ |
| 15 | 0.332572 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.330046 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.322287 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.311530 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.310929 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.309179 | `azmcp_group_list` | ❌ |

---

## Test 54

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501852 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.418623 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.374100 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.359922 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.341556 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.335467 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.333318 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.324104 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.318706 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.316614 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.315414 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.311692 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.309627 | `azmcp_search_service_list` | ❌ |
| 15 | 0.306052 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.303931 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.302428 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.300101 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.299629 | `azmcp_appconfig_account_list` | ❌ |
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
| 4 | 0.587691 | `azmcp_subscription_list` | ❌ |
| 5 | 0.560795 | `azmcp_search_service_list` | ❌ |
| 6 | 0.538321 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.528963 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.516914 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.502428 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.502199 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.499146 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.497679 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.487067 | `azmcp_group_list` | ❌ |
| 14 | 0.483046 | `azmcp_grafana_list` | ❌ |
| 15 | 0.474934 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.473658 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.460181 | `azmcp_foundry_agents_list` | ❌ |
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
| 2 | 0.605357 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.571613 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.486033 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.436283 | `azmcp_subscription_list` | ❌ |
| 6 | 0.431496 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.427967 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.427709 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.408659 | `azmcp_search_service_list` | ❌ |
| 10 | 0.405748 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.397574 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.390141 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.389842 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.386108 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.383985 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.381323 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.379496 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.373667 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.367942 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.348358 | `azmcp_datadog_monitoredresources_list` | ❌ |

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
| 4 | 0.546327 | `azmcp_subscription_list` | ❌ |
| 5 | 0.530175 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.527812 | `azmcp_search_service_list` | ❌ |
| 7 | 0.488006 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.466414 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.457584 | `azmcp_appconfig_account_list` | ❌ |
| 10 | 0.456205 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.455017 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.453626 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.441136 | `azmcp_grafana_list` | ❌ |
| 14 | 0.438277 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.437501 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.437026 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.434623 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.433094 | `azmcp_postgres_server_list` | ❌ |
| 19 | 0.430352 | `azmcp_aks_cluster_list` | ❌ |
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
| 5 | 0.444883 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.429363 | `azmcp_search_service_list` | ❌ |
| 7 | 0.399756 | `azmcp_search_index_query` | ❌ |
| 8 | 0.378151 | `azmcp_kusto_query` | ❌ |
| 9 | 0.374844 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.372689 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.366503 | `azmcp_search_index_get` | ❌ |
| 12 | 0.358903 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.351331 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.340982 | `azmcp_monitor_table_list` | ❌ |
| 15 | 0.337570 | `azmcp_storage_blob_get` | ❌ |
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
| 1 | 0.852710 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.680955 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.630682 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.581368 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.527400 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 6 | 0.486392 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.448929 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.447530 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.439792 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.427503 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.424152 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.422240 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.421485 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.420401 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.411588 | `azmcp_monitor_table_list` | ❌ |
| 16 | 0.392845 | `azmcp_postgres_database_list` | ❌ |
| 17 | 0.386419 | `azmcp_storage_blob_get` | ❌ |
| 18 | 0.383532 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.374596 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.372078 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 60

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789435 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.614210 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.561934 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.537005 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.521482 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 6 | 0.449229 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.411797 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.397996 | `azmcp_kusto_database_list` | ❌ |
| 9 | 0.397808 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.397755 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.395455 | `azmcp_kusto_table_list` | ❌ |
| 12 | 0.392770 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.386754 | `azmcp_redis_cluster_database_list` | ❌ |
| 14 | 0.356008 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.355518 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.345890 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.342405 | `azmcp_monitor_table_list` | ❌ |
| 18 | 0.326053 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.319544 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.318557 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 61

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
| 11 | 0.450854 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.442540 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.418871 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.407722 | `azmcp_search_service_list` | ❌ |
| 15 | 0.406805 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.401638 | `azmcp_subscription_list` | ❌ |
| 17 | 0.387534 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.384252 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.381044 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.379631 | `azmcp_keyvault_key_list` | ❌ |

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
| 5 | 0.524837 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.498206 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.497414 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.449759 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.447875 | `azmcp_postgres_database_list` | ❌ |
| 10 | 0.437993 | `azmcp_kusto_table_list` | ❌ |
| 11 | 0.408605 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.402767 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.396280 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.383780 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.379009 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.369344 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.348999 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.342424 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.339516 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.335852 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 63

**Expected Tool:** `azmcp_kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482148 | `azmcp_kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.464523 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.457669 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.416762 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.378455 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.362977 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.361786 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.353792 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.350998 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.344871 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.344590 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.333244 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.332639 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.326526 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.326306 | `azmcp_search_index_get` | ❌ |
| 16 | 0.326052 | `azmcp_aks_nodepool_list` | ❌ |
| 17 | 0.318754 | `azmcp_kusto_query` | ❌ |
| 18 | 0.318505 | `azmcp_storage_blob_container_get` | ❌ |
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
| 2 | 0.644037 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.549093 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.536167 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.509396 | `azmcp_grafana_list` | ❌ |
| 6 | 0.505975 | `azmcp_redis_cache_list` | ❌ |
| 7 | 0.492107 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.491278 | `azmcp_search_service_list` | ❌ |
| 9 | 0.487583 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.486159 | `azmcp_kusto_cluster_get` | ❌ |
| 11 | 0.460255 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.458754 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.451500 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.427759 | `azmcp_subscription_list` | ❌ |
| 15 | 0.420174 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.412630 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.411911 | `azmcp_group_list` | ❌ |
| 18 | 0.410016 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.399251 | `azmcp_monitor_table_list` | ❌ |
| 20 | 0.391238 | `azmcp_monitor_table_type_list` | ❌ |

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
| 6 | 0.338260 | `azmcp_aks_cluster_list` | ❌ |
| 7 | 0.314734 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.303083 | `azmcp_grafana_list` | ❌ |
| 9 | 0.292994 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.292934 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.287768 | `azmcp_kusto_sample` | ❌ |
| 12 | 0.285603 | `azmcp_kusto_query` | ❌ |
| 13 | 0.283331 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.277014 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.275559 | `azmcp_mysql_database_query` | ❌ |
| 16 | 0.270804 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.265906 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.264112 | `azmcp_monitor_table_type_list` | ❌ |
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
| 3 | 0.471216 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.469570 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.464294 | `azmcp_kusto_database_list` | ❌ |
| 6 | 0.462945 | `azmcp_grafana_list` | ❌ |
| 7 | 0.446192 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.440326 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.434016 | `azmcp_search_service_list` | ❌ |
| 10 | 0.432048 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.406863 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.396253 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.392541 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.386776 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.380006 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.377490 | `azmcp_kusto_query` | ❌ |
| 17 | 0.371088 | `azmcp_subscription_list` | ❌ |
| 18 | 0.368890 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.365323 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.355671 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

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
| 4 | 0.549673 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.517039 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.474354 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.461496 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.459180 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.434330 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.431669 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.419528 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.404095 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.396060 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.375535 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.363663 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.363266 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.350253 | `azmcp_aks_cluster_list` | ❌ |
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
| 5 | 0.486732 | `azmcp_postgres_database_list` | ❌ |
| 6 | 0.440064 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.427251 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.422588 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.391411 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.383664 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.368013 | `azmcp_postgres_table_list` | ❌ |
| 12 | 0.362905 | `azmcp_cosmos_database_container_list` | ❌ |
| 13 | 0.359378 | `azmcp_monitor_table_list` | ❌ |
| 14 | 0.344010 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.336400 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.336104 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.334803 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.310912 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.309809 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.305718 | `azmcp_kusto_query` | ❌ |

---

## Test 69

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.381343 | `azmcp_kusto_query` | ✅ **EXPECTED** |
| 2 | 0.363591 | `azmcp_mysql_table_list` | ❌ |
| 3 | 0.363250 | `azmcp_kusto_sample` | ❌ |
| 4 | 0.349152 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.345774 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.334759 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.328637 | `azmcp_search_service_list` | ❌ |
| 8 | 0.328158 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.324772 | `azmcp_mysql_table_schema_get` | ❌ |
| 10 | 0.319109 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.318884 | `azmcp_kusto_table_schema` | ❌ |
| 12 | 0.314955 | `azmcp_monitor_table_type_list` | ❌ |
| 13 | 0.314929 | `azmcp_search_index_query` | ❌ |
| 14 | 0.308107 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.304017 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.302883 | `azmcp_postgres_table_list` | ❌ |
| 17 | 0.292078 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.264023 | `azmcp_grafana_list` | ❌ |
| 19 | 0.263093 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.257473 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 70

**Expected Tool:** `azmcp_kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537157 | `azmcp_kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.419458 | `azmcp_kusto_table_schema` | ❌ |
| 3 | 0.391607 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.391417 | `azmcp_kusto_table_list` | ❌ |
| 5 | 0.380708 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.377035 | `azmcp_redis_cluster_database_list` | ❌ |
| 7 | 0.364609 | `azmcp_postgres_table_schema_get` | ❌ |
| 8 | 0.364361 | `azmcp_mysql_table_list` | ❌ |
| 9 | 0.361843 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.343666 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.341682 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.337279 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.319248 | `azmcp_kusto_query` | ❌ |
| 14 | 0.318197 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.310193 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.285945 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.282656 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.267693 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.249386 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.242121 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

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
| 18 | 0.337427 | `azmcp_kusto_query` | ❌ |
| 19 | 0.330111 | `azmcp_aks_cluster_list` | ❌ |
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
| 18 | 0.330383 | `azmcp_kusto_query` | ❌ |
| 19 | 0.314766 | `azmcp_kusto_cluster_get` | ❌ |
| 20 | 0.300302 | `azmcp_aks_cluster_list` | ❌ |

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
| 7 | 0.413924 | `azmcp_monitor_table_list` | ❌ |
| 8 | 0.398632 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.387660 | `azmcp_postgres_table_list` | ❌ |
| 10 | 0.366346 | `azmcp_monitor_table_type_list` | ❌ |
| 11 | 0.366081 | `azmcp_kusto_database_list` | ❌ |
| 12 | 0.358088 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.345263 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.343548 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.340038 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.314580 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.298243 | `azmcp_kusto_query` | ❌ |
| 18 | 0.294840 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.282712 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.275795 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 74

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
| 18 | 0.268856 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.253148 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.252501 | `azmcp_group_list` | ❌ |

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
| 16 | 0.251297 | `azmcp_appservice_database_add` | ❌ |
| 17 | 0.247558 | `azmcp_grafana_list` | ❌ |
| 18 | 0.239544 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.236450 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.236174 | `azmcp_acr_registry_list` | ❌ |

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

## Test 77

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
| 17 | 0.214504 | `azmcp_appservice_database_add` | ❌ |
| 18 | 0.198877 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.180063 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.169430 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 78

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
| 7 | 0.467854 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.458406 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.457318 | `azmcp_grafana_list` | ❌ |
| 10 | 0.451969 | `azmcp_postgres_database_list` | ❌ |
| 11 | 0.431642 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.431126 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.422584 | `azmcp_search_service_list` | ❌ |
| 14 | 0.410134 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.403984 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.379322 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.377511 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.374433 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.365605 | `azmcp_group_list` | ❌ |
| 20 | 0.354490 | `azmcp_datadog_monitoredresources_list` | ❌ |

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
| 15 | 0.244811 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.241497 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.235455 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.232383 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.224586 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.218049 | `azmcp_acr_registry_list` | ❌ |

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
| 5 | 0.458498 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.456616 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.441885 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.431914 | `azmcp_grafana_list` | ❌ |
| 9 | 0.419663 | `azmcp_search_service_list` | ❌ |
| 10 | 0.416021 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.412407 | `azmcp_mysql_database_query` | ❌ |
| 12 | 0.408235 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.399358 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.376596 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.375789 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.364016 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.356680 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.341439 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.341087 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.337333 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 81

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
| 11 | 0.241275 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.183735 | `azmcp_appconfig_kv_show` | ❌ |
| 13 | 0.160082 | `azmcp_appconfig_kv_list` | ❌ |
| 14 | 0.153259 | `azmcp_keyvault_secret_get` | ❌ |
| 15 | 0.146292 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.131542 | `azmcp_keyvault_key_get` | ❌ |
| 17 | 0.124274 | `azmcp_grafana_list` | ❌ |
| 18 | 0.121413 | `azmcp_foundry_agents_connect` | ❌ |
| 19 | 0.120498 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 20 | 0.118505 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.390761 | `azmcp_mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `azmcp_mysql_server_param_get` | ❌ |
| 3 | 0.307508 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.298911 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.277669 | `azmcp_appservice_database_add` | ❌ |
| 6 | 0.254180 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.253189 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.246424 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.246019 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.238742 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.236453 | `azmcp_postgres_server_param_get` | ❌ |
| 12 | 0.140364 | `azmcp_foundry_agents_connect` | ❌ |
| 13 | 0.112499 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.109739 | `azmcp_keyvault_secret_create` | ❌ |
| 15 | 0.090695 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.090334 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.089483 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.088027 | `azmcp_loadtesting_test_create` | ❌ |
| 19 | 0.086308 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 20 | 0.084424 | `azmcp_foundry_agents_evaluate` | ❌ |

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
| 6 | 0.447284 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.442053 | `azmcp_kusto_table_list` | ❌ |
| 8 | 0.429975 | `azmcp_mysql_database_query` | ❌ |
| 9 | 0.418619 | `azmcp_monitor_table_list` | ❌ |
| 10 | 0.410273 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.401217 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.393205 | `azmcp_redis_cluster_database_list` | ❌ |
| 13 | 0.361477 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.335069 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.308385 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.268415 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.260118 | `azmcp_kusto_sample` | ❌ |
| 18 | 0.253046 | `azmcp_grafana_list` | ❌ |
| 19 | 0.239226 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.235180 | `azmcp_datadog_monitoredresources_list` | ❌ |

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
| 7 | 0.419905 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.403265 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.385166 | `azmcp_postgres_table_schema_get` | ❌ |
| 10 | 0.382169 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.378011 | `azmcp_redis_cluster_database_list` | ❌ |
| 12 | 0.349434 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.342926 | `azmcp_kusto_table_schema` | ❌ |
| 14 | 0.319674 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.303999 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.281571 | `azmcp_kusto_sample` | ❌ |
| 17 | 0.236723 | `azmcp_grafana_list` | ❌ |
| 18 | 0.231173 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.225876 | `azmcp_appservice_database_add` | ❌ |
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
| 9 | 0.372911 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.348909 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.347368 | `azmcp_postgres_server_config_get` | ❌ |
| 12 | 0.324675 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.307950 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.271938 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.268321 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.243861 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.239328 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.208806 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.202788 | `azmcp_bicepschema_get` | ❌ |
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
| 5 | 0.490955 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.471672 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.453436 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.444410 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.435828 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.418343 | `azmcp_postgres_database_query` | ❌ |
| 11 | 0.414679 | `azmcp_postgres_server_param_set` | ❌ |
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
| 1 | 0.760033 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589783 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.585891 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.552660 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.495683 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.452128 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.433860 | `azmcp_redis_cluster_database_list` | ❌ |
| 8 | 0.430589 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.426820 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.416937 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.385475 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.365997 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.281529 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.261442 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.257971 | `azmcp_grafana_list` | ❌ |
| 16 | 0.247726 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.235347 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.227995 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.223442 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
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
| 4 | 0.415838 | `azmcp_postgres_database_query` | ✅ **EXPECTED** |
| 5 | 0.403969 | `azmcp_postgres_server_param_get` | ❌ |
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

## Test 89

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
| 8 | 0.394618 | `azmcp_postgres_database_query` | ❌ |
| 9 | 0.356774 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.337899 | `azmcp_mysql_server_config_get` | ❌ |
| 11 | 0.269224 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.233426 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.222849 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.220186 | `azmcp_loadtesting_test_get` | ❌ |
| 15 | 0.208314 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.189446 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.185547 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.178215 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.177778 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
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
| 5 | 0.507714 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.483663 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.472458 | `azmcp_grafana_list` | ❌ |
| 8 | 0.457583 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.453841 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.446591 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.435298 | `azmcp_search_service_list` | ❌ |
| 12 | 0.416315 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.406617 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.399158 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.397428 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.389191 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.373641 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.373639 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.366102 | `azmcp_group_list` | ❌ |
| 20 | 0.362900 | `azmcp_eventgrid_topic_list` | ❌ |

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
| 5 | 0.506171 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.409378 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.400088 | `azmcp_postgres_server_param_set` | ❌ |
| 8 | 0.372955 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.336934 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.336270 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.274763 | `azmcp_grafana_list` | ❌ |
| 12 | 0.260533 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.253264 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.245311 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.241835 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.239500 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.238588 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.229741 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.227547 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.225295 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

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
| 5 | 0.505970 | `azmcp_postgres_server_param_get` | ❌ |
| 6 | 0.452608 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.444127 | `azmcp_grafana_list` | ❌ |
| 8 | 0.430033 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.421577 | `azmcp_search_service_list` | ❌ |
| 10 | 0.414774 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.410707 | `azmcp_postgres_database_query` | ❌ |
| 12 | 0.403538 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.376954 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.367001 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.362650 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.362557 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.360586 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.358358 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.334679 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.334101 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 93

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
| 9 | 0.305333 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.295439 | `azmcp_mysql_server_param_set` | ❌ |
| 11 | 0.185273 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.183435 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.174107 | `azmcp_grafana_list` | ❌ |
| 14 | 0.169190 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.166286 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.158090 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.155785 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.145056 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.142408 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.141137 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |

---

## Test 94

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
| 7 | 0.341619 | `azmcp_postgres_database_query` | ❌ |
| 8 | 0.317484 | `azmcp_postgres_table_schema_get` | ❌ |
| 9 | 0.241642 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.227740 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.192610 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.133385 | `azmcp_kusto_sample` | ❌ |
| 13 | 0.127120 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.126706 | `azmcp_foundry_agents_evaluate` | ❌ |
| 15 | 0.123491 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.119027 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.118089 | `azmcp_cosmos_database_list` | ❌ |
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
| 7 | 0.449212 | `azmcp_postgres_database_query` | ❌ |
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

## Test 96

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.735747 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.689945 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.557599 | `azmcp_postgres_table_schema_get` | ❌ |
| 4 | 0.543183 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.521813 | `azmcp_postgres_server_config_get` | ❌ |
| 6 | 0.465141 | `azmcp_postgres_database_query` | ❌ |
| 7 | 0.457541 | `azmcp_mysql_table_list` | ❌ |
| 8 | 0.447567 | `azmcp_postgres_server_param_get` | ❌ |
| 9 | 0.390089 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.383317 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.371888 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.334224 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.315801 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.307117 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.272303 | `azmcp_kusto_sample` | ❌ |
| 16 | 0.266378 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.243530 | `azmcp_grafana_list` | ❌ |
| 18 | 0.207757 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.205790 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.202853 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 97

**Expected Tool:** `azmcp_postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714894 | `azmcp_postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597891 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.574233 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.508086 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.480729 | `azmcp_mysql_table_schema_get` | ❌ |
| 6 | 0.475678 | `azmcp_kusto_table_schema` | ❌ |
| 7 | 0.443802 | `azmcp_postgres_server_param_get` | ❌ |
| 8 | 0.442577 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.427540 | `azmcp_postgres_database_query` | ❌ |
| 10 | 0.406776 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.362670 | `azmcp_postgres_server_param_set` | ❌ |
| 12 | 0.322788 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.303755 | `azmcp_kusto_sample` | ❌ |
| 14 | 0.253856 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 15 | 0.253391 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.239243 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.212285 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.201665 | `azmcp_grafana_list` | ❌ |
| 19 | 0.185101 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.184052 | `azmcp_appservice_database_add` | ❌ |

---

## Test 98

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686657 | `azmcp_deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.404890 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.392466 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.389603 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.354472 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.342594 | `azmcp_monitor_resource_log_query` | ❌ |
| 8 | 0.334992 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.334522 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.333577 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.327028 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.325553 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.320572 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.314964 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.314890 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.312821 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.307291 | `azmcp_sql_db_show` | ❌ |
| 18 | 0.297568 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.294636 | `azmcp_sql_server_list` | ❌ |
| 20 | 0.288823 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 99

**Expected Tool:** `azmcp_deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680640 | `azmcp_deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562505 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.505052 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.497193 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.435890 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.430764 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.417333 | `azmcp_get_bestpractices_get` | ❌ |
| 8 | 0.371152 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.343117 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.322230 | `azmcp_extension_azqr` | ❌ |
| 11 | 0.317906 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.284401 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.270093 | `azmcp_sql_db_create` | ❌ |
| 14 | 0.264888 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.264060 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.263521 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.255084 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.250629 | `azmcp_search_service_list` | ❌ |
| 19 | 0.248128 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.247818 | `azmcp_sql_server_show` | ❌ |

---

## Test 100

**Expected Tool:** `azmcp_deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529285 | `azmcp_deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.404956 | `azmcp_bicepschema_get` | ❌ |
| 3 | 0.392073 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.383307 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.341541 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.304894 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.278758 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.266972 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.266660 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.252796 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.236481 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.224014 | `azmcp_extension_azqr` | ❌ |
| 13 | 0.219574 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.206992 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.202308 | `azmcp_mysql_table_schema_get` | ❌ |
| 16 | 0.201412 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.195479 | `azmcp_mysql_table_list` | ❌ |
| 18 | 0.194647 | `azmcp_sql_db_create` | ❌ |
| 19 | 0.188727 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.178639 | `azmcp_storage_blob_get` | ❌ |

---

## Test 101

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `azmcp_deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.448842 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.382240 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.375202 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.373411 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.350101 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.338440 | `azmcp_foundry_models_deploy` | ❌ |
| 9 | 0.322906 | `azmcp_cloudarchitect_design` | ❌ |
| 10 | 0.297761 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.262768 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.240757 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.234556 | `azmcp_sql_db_update` | ❌ |
| 14 | 0.230063 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.222451 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.212123 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.211103 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.203987 | `azmcp_sql_server_delete` | ❌ |
| 19 | 0.198696 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.195915 | `azmcp_workbooks_delete` | ❌ |

---

## Test 102

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688051 | `azmcp_deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499311 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.432825 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.425393 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 7 | 0.421744 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.413781 | `azmcp_loadtesting_test_create` | ❌ |
| 9 | 0.393574 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.365875 | `azmcp_foundry_models_deploy` | ❌ |
| 11 | 0.344407 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.312839 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.300643 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.299552 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.296697 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.292652 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.277064 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.258195 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.252696 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.249358 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 103

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
| 6 | 0.495664 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 7 | 0.492754 | `azmcp_group_list` | ❌ |
| 8 | 0.485584 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.484509 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.475667 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.475056 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.472764 | `azmcp_grafana_list` | ❌ |
| 13 | 0.470316 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.442229 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.440645 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.439820 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.438287 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.427536 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.422414 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.421784 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |

---

## Test 104

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691068 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.599956 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.477932 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.475119 | `azmcp_search_service_list` | ❌ |
| 5 | 0.450712 | `azmcp_redis_cluster_list` | ❌ |
| 6 | 0.441607 | `azmcp_kusto_cluster_list` | ❌ |
| 7 | 0.437153 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.431249 | `azmcp_subscription_list` | ❌ |
| 9 | 0.430494 | `azmcp_grafana_list` | ❌ |
| 10 | 0.428470 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.424907 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.420072 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.419194 | `azmcp_group_list` | ❌ |
| 14 | 0.408708 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.399253 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.396758 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.390290 | `azmcp_servicebus_topic_details` | ❌ |
| 18 | 0.384757 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.381705 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.381664 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 105

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759396 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.632794 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.526595 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `azmcp_search_service_list` | ❌ |
| 5 | 0.495195 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.494153 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.481439 | `azmcp_group_list` | ❌ |
| 8 | 0.481065 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.476812 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.476780 | `azmcp_subscription_list` | ❌ |
| 11 | 0.471888 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.468200 | `azmcp_grafana_list` | ❌ |
| 13 | 0.466774 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.445991 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.429646 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.428727 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.424984 | `azmcp_servicebus_topic_details` | ❌ |
| 18 | 0.421430 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.417918 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.392039 | `azmcp_kusto_database_list` | ❌ |

---

## Test 106

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659238 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.618947 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.609372 | `azmcp_group_list` | ❌ |
| 4 | 0.514693 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.506299 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.491568 | `azmcp_sql_server_list` | ❌ |
| 7 | 0.484927 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.475509 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.464487 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.460577 | `azmcp_search_service_list` | ❌ |
| 11 | 0.456600 | `azmcp_grafana_list` | ❌ |
| 12 | 0.455542 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.453017 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.448172 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.443050 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.441809 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.432442 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.422990 | `azmcp_postgres_server_list` | ❌ |
| 19 | 0.412017 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.407959 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 107

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682900 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.637188 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.486216 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.480537 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.477660 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.457868 | `azmcp_search_service_list` | ❌ |
| 7 | 0.445053 | `azmcp_subscription_list` | ❌ |
| 8 | 0.435412 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.434659 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.422093 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.420976 | `azmcp_group_list` | ❌ |
| 12 | 0.417538 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.415247 | `azmcp_redis_cache_list` | ❌ |
| 14 | 0.408588 | `azmcp_grafana_list` | ❌ |
| 15 | 0.397665 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.392784 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.382858 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.378136 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.376133 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.367397 | `azmcp_acr_registry_list` | ❌ |

---

## Test 108

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672482 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.656023 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.539977 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.497869 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.459817 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.444774 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.443291 | `azmcp_subscription_list` | ❌ |
| 8 | 0.438131 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435639 | `azmcp_search_service_list` | ❌ |
| 10 | 0.434401 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.433482 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.431618 | `azmcp_grafana_list` | ❌ |
| 13 | 0.427056 | `azmcp_group_list` | ❌ |
| 14 | 0.419194 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.402159 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.398589 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.392822 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.386881 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.376625 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.376197 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 109

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669199 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.663347 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.524880 | `azmcp_group_list` | ❌ |
| 4 | 0.488627 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484144 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 6 | 0.478901 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.474030 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.473088 | `azmcp_servicebus_topic_details` | ❌ |
| 9 | 0.465353 | `azmcp_acr_registry_list` | ❌ |
| 10 | 0.465017 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.462100 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.457043 | `azmcp_search_service_list` | ❌ |
| 13 | 0.452311 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.452174 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.443060 | `azmcp_subscription_list` | ❌ |
| 16 | 0.436586 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.435971 | `azmcp_grafana_list` | ❌ |
| 18 | 0.428668 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.412463 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.409126 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 110

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593171 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.592262 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.525017 | `azmcp_subscription_list` | ❌ |
| 4 | 0.518857 | `azmcp_search_service_list` | ❌ |
| 5 | 0.508407 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.490371 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.489687 | `azmcp_kusto_cluster_list` | ❌ |
| 8 | 0.480579 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.475795 | `azmcp_group_list` | ❌ |
| 10 | 0.475220 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.473274 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.467172 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.460683 | `azmcp_grafana_list` | ❌ |
| 14 | 0.451759 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.440759 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.439125 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.422468 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.415047 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.410156 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.399352 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 111

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604278 | `azmcp_eventgrid_topic_list` | ❌ |
| 2 | 0.600323 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.535955 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.518141 | `azmcp_subscription_list` | ❌ |
| 5 | 0.510216 | `azmcp_group_list` | ❌ |
| 6 | 0.508443 | `azmcp_search_service_list` | ❌ |
| 7 | 0.493621 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.492726 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.485794 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.483497 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.481609 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.481450 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.473868 | `azmcp_grafana_list` | ❌ |
| 14 | 0.467622 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 15 | 0.453352 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.446484 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.428290 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.426897 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.411734 | `azmcp_sql_server_list` | ❌ |
| 20 | 0.410778 | `azmcp_acr_registry_list` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622684 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.558722 | `azmcp_group_list` | ❌ |
| 3 | 0.532589 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.507088 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.503801 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 6 | 0.490489 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.473240 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.469708 | `azmcp_workbooks_list` | ❌ |
| 9 | 0.468719 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.466767 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.462138 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.460740 | `azmcp_grafana_list` | ❌ |
| 13 | 0.443239 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.439165 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.439005 | `azmcp_subscription_list` | ❌ |
| 16 | 0.435251 | `azmcp_search_service_list` | ❌ |
| 17 | 0.433549 | `azmcp_monitor_workspace_list` | ❌ |
| 18 | 0.425325 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.413844 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.413763 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 113

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653850 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.581728 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.479893 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.478385 | `azmcp_subscription_list` | ❌ |
| 5 | 0.476763 | `azmcp_search_service_list` | ❌ |
| 6 | 0.475482 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.471596 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.471384 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.466489 | `azmcp_group_list` | ❌ |
| 10 | 0.449826 | `azmcp_redis_cache_list` | ❌ |
| 11 | 0.449681 | `azmcp_grafana_list` | ❌ |
| 12 | 0.447080 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 13 | 0.446605 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.444645 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.437300 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.436793 | `azmcp_postgres_server_list` | ❌ |
| 17 | 0.436653 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.425231 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.422662 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.420013 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.448153 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.390048 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.380314 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.379655 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.376433 | `azmcp_applens_resource_diagnose` | ❌ |
| 7 | 0.373215 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352982 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.347628 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.347347 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.342763 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.341448 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.341443 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 14 | 0.338591 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.337465 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 16 | 0.333091 | `azmcp_extension_azqr` | ❌ |
| 17 | 0.328326 | `azmcp_storage_account_create` | ❌ |
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
| 1 | 0.607276 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447400 | `azmcp_mysql_server_config_get` | ❌ |
| 3 | 0.424693 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.422327 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.407133 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.397977 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.392852 | `azmcp_appconfig_kv_list` | ❌ |
| 8 | 0.384151 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.383957 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.369436 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.367183 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.363307 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.361753 | `azmcp_deploy_plan_get` | ❌ |
| 14 | 0.353601 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.351961 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.342398 | `azmcp_postgres_server_config_get` | ❌ |
| 17 | 0.321697 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.314950 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.314079 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.312611 | `azmcp_sql_db_list` | ❌ |

---

## Test 116

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.460066 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.420198 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.390708 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.334473 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.322197 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.320055 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.317583 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.317359 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.312732 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.311384 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.309941 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.305193 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.297747 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.297135 | `azmcp_aks_nodepool_get` | ❌ |
| 16 | 0.295538 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.295174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.290156 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.281639 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.277653 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 117

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690933 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433973 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.432317 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.424646 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.419302 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.416967 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.396163 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.390791 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.389322 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.387898 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.383857 | `azmcp_sql_server_list` | ❌ |
| 12 | 0.383191 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.378811 | `azmcp_get_bestpractices_get` | ❌ |
| 14 | 0.376019 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.375268 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.368506 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.360165 | `azmcp_aks_cluster_get` | ❌ |
| 18 | 0.352505 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.348610 | `azmcp_foundry_models_deployments_list` | ❌ |
| 20 | 0.346265 | `azmcp_group_list` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.443440 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.441319 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.391480 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.383893 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.355527 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.353543 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.351217 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.349540 | `azmcp_get_bestpractices_get` | ❌ |
| 10 | 0.347266 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.344702 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.342868 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.337247 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.333000 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.331119 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.325680 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.320825 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.319736 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.318160 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.305803 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 119

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.445153 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.368188 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.366279 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.365569 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.363324 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.358624 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352754 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.351460 | `azmcp_aks_cluster_get` | ❌ |
| 10 | 0.350151 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.349596 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.348949 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.336938 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.335848 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.327763 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.325899 | `azmcp_sql_server_list` | ❌ |
| 17 | 0.325827 | `azmcp_workbooks_show` | ❌ |
| 18 | 0.323655 | `azmcp_foundry_models_deployments_list` | ❌ |
| 19 | 0.323377 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.319790 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433962 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.388678 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.369779 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.368021 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.367902 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.367552 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.359781 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.355956 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.355282 | `azmcp_search_index_get` | ❌ |
| 11 | 0.352417 | `azmcp_keyvault_key_get` | ❌ |
| 12 | 0.349891 | `azmcp_mysql_server_config_get` | ❌ |
| 13 | 0.349476 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.346974 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.344067 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.343381 | `azmcp_get_bestpractices_get` | ❌ |
| 17 | 0.342238 | `azmcp_servicebus_queue_details` | ❌ |
| 18 | 0.338127 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.337614 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.326091 | `azmcp_quota_usage_check` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554980 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426703 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.418364 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.408011 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.381629 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.364785 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.350663 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.335606 | `azmcp_appconfig_account_list` | ❌ |
| 9 | 0.325214 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.321466 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.318517 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.313831 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.306310 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.304263 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.301769 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.281401 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.277916 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.267215 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.267170 | `azmcp_search_service_list` | ❌ |
| 20 | 0.266494 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 122

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565797 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.440299 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.422799 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.384159 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.342552 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.333621 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.319464 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.318008 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.310636 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.298434 | `azmcp_foundry_models_deployments_list` | ❌ |
| 11 | 0.297073 | `azmcp_deploy_plan_get` | ❌ |
| 12 | 0.295694 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.292793 | `azmcp_cloudarchitect_design` | ❌ |
| 14 | 0.283653 | `azmcp_sql_server_show` | ❌ |
| 15 | 0.272348 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.270846 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.267087 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.265948 | `azmcp_storage_blob_container_get` | ❌ |
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
| 7 | 0.465690 | `azmcp_group_list` | ❌ |
| 8 | 0.464534 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.462296 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.455851 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.455388 | `azmcp_postgres_server_list` | ❌ |
| 12 | 0.445087 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.442614 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.433245 | `azmcp_eventgrid_topic_list` | ❌ |
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
| 2 | 0.452156 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.436167 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.412646 | `azmcp_search_service_list` | ❌ |
| 5 | 0.411323 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.385832 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.374655 | `azmcp_appconfig_account_list` | ❌ |
| 8 | 0.372790 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.370393 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.369681 | `azmcp_subscription_list` | ❌ |
| 11 | 0.368004 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.358720 | `azmcp_deploy_plan_get` | ❌ |
| 13 | 0.357329 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.347887 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.347802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.339873 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.334019 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.333136 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.327870 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.327326 | `azmcp_sql_server_list` | ❌ |

---

## Test 125

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433674 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.348099 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.284362 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.281607 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.249658 | `azmcp_appconfig_account_list` | ❌ |
| 6 | 0.244782 | `azmcp_appconfig_kv_list` | ❌ |
| 7 | 0.240729 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.239514 | `azmcp_foundry_models_deployments_list` | ❌ |
| 9 | 0.217775 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.213974 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.207391 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.197655 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.195857 | `azmcp_role_assignment_list` | ❌ |
| 14 | 0.194372 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.186328 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.184120 | `azmcp_monitor_workspace_list` | ❌ |
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
| 1 | 0.627872 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570318 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.540199 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.519218 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.500111 | `azmcp_keyvault_certificate_list` | ❌ |
| 6 | 0.474019 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.372046 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.370863 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.352953 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.348935 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.345316 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.330026 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.296673 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.285184 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.267718 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.237081 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.222638 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.219479 | `azmcp_subscription_list` | ❌ |
| 19 | 0.217086 | `azmcp_search_service_list` | ❌ |
| 20 | 0.208862 | `azmcp_workbooks_delete` | ❌ |

---

## Test 127

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600125 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561459 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.522743 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.502052 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.497280 | `azmcp_keyvault_certificate_list` | ❌ |
| 6 | 0.439914 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.396480 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.365355 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.355331 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.348085 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.317241 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.306422 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.293034 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.271837 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.260731 | `azmcp_sql_server_create` | ❌ |
| 16 | 0.252010 | `azmcp_workbooks_delete` | ❌ |
| 17 | 0.245574 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.242550 | `azmcp_subscription_list` | ❌ |
| 19 | 0.240000 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.228831 | `azmcp_search_service_list` | ❌ |

---

## Test 128

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574214 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527759 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.481548 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.469703 | `azmcp_keyvault_certificate_list` | ❌ |
| 6 | 0.408030 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.367681 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.359121 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.334271 | `azmcp_keyvault_secret_get` | ❌ |
| 10 | 0.322220 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.306781 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.304171 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.294438 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.260984 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.260723 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.252172 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.229588 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.228728 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.225536 | `azmcp_search_index_get` | ❌ |
| 20 | 0.223368 | `azmcp_subscription_list` | ❌ |

---

## Test 129

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592062 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.562265 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.522147 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.502529 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.480082 | `azmcp_keyvault_certificate_list` | ❌ |
| 6 | 0.432097 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.386798 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.348511 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.346112 | `azmcp_keyvault_secret_get` | ❌ |
| 10 | 0.317684 | `azmcp_appconfig_kv_set` | ❌ |
| 11 | 0.302700 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.301808 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.272932 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.241208 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.237598 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.232434 | `azmcp_search_service_list` | ❌ |
| 17 | 0.232147 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.228768 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.218138 | `azmcp_search_index_get` | ❌ |
| 20 | 0.217559 | `azmcp_mysql_server_list` | ❌ |

---

## Test 130

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622998 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.558532 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.534503 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.521426 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.465056 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.410000 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.390046 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.356917 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.354429 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.311559 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 11 | 0.292534 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.291440 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.281865 | `azmcp_sql_db_create` | ❌ |
| 14 | 0.257221 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.251631 | `azmcp_subscription_list` | ❌ |
| 16 | 0.251493 | `azmcp_search_service_list` | ❌ |
| 17 | 0.250229 | `azmcp_search_index_get` | ❌ |
| 18 | 0.243511 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.234014 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.233404 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 131

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600625 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528484 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.519037 | `azmcp_keyvault_certificate_import` | ❌ |
| 4 | 0.499634 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.486609 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.450716 | `azmcp_keyvault_secret_get` | ❌ |
| 7 | 0.390699 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.383271 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.379434 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.359751 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.346167 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.318305 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.293421 | `azmcp_subscription_list` | ❌ |
| 14 | 0.289685 | `azmcp_search_service_list` | ❌ |
| 15 | 0.279695 | `azmcp_search_index_get` | ❌ |
| 16 | 0.276581 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.271911 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.269735 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.266478 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.263141 | `azmcp_storage_account_create` | ❌ |

---

## Test 132

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646098 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.562988 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.514170 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.509504 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.507737 | `azmcp_keyvault_certificate_import` | ❌ |
| 6 | 0.485573 | `azmcp_keyvault_certificate_create` | ❌ |
| 7 | 0.459167 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.417886 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.411136 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.392729 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.381410 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.368360 | `azmcp_search_index_get` | ❌ |
| 13 | 0.365386 | `azmcp_sql_db_show` | ❌ |
| 14 | 0.363228 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.350193 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.332770 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.331645 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.317884 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.305778 | `azmcp_subscription_list` | ❌ |
| 20 | 0.301710 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 133

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609523 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.515661 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.511443 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.507768 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.474394 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.445807 | `azmcp_keyvault_secret_get` | ❌ |
| 7 | 0.387472 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.356616 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.352578 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.351633 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.350108 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.315724 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.294397 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.269229 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 15 | 0.262046 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.261616 | `azmcp_subscription_list` | ❌ |
| 17 | 0.252793 | `azmcp_search_index_get` | ❌ |
| 18 | 0.248508 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.239849 | `azmcp_search_service_list` | ❌ |
| 20 | 0.237997 | `azmcp_storage_blob_get` | ❌ |

---

## Test 134

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647669 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527400 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.521623 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.509796 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.501988 | `azmcp_keyvault_secret_get` | ❌ |
| 6 | 0.485855 | `azmcp_keyvault_certificate_create` | ❌ |
| 7 | 0.440760 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.409563 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.405442 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.371542 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.359834 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.355790 | `azmcp_search_index_get` | ❌ |
| 13 | 0.347809 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.346992 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.344642 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.320533 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.312231 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.302716 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.296019 | `azmcp_mysql_table_schema_get` | ❌ |
| 20 | 0.281628 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 135

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595959 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527473 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.519059 | `azmcp_keyvault_certificate_import` | ❌ |
| 4 | 0.501564 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.465174 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.429174 | `azmcp_keyvault_secret_get` | ❌ |
| 7 | 0.369183 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.368025 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.365084 | `azmcp_storage_blob_container_get` | ❌ |
| 10 | 0.361116 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.343934 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.328187 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.321769 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.311060 | `azmcp_search_index_get` | ❌ |
| 15 | 0.305979 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.295477 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.288678 | `azmcp_marketplace_product_get` | ❌ |
| 18 | 0.288660 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.287497 | `azmcp_workbooks_show` | ❌ |
| 20 | 0.286224 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585481 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420747 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.402804 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.399427 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.352905 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.336830 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.325224 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.289124 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.283424 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.272263 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 11 | 0.248212 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.228508 | `azmcp_workbooks_delete` | ❌ |
| 13 | 0.222971 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.205023 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.181129 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.180219 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.174606 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.170326 | `azmcp_subscription_list` | ❌ |
| 19 | 0.158491 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.153106 | `azmcp_search_service_list` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622125 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.504314 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.498982 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.448218 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.419811 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.392959 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.349395 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.320194 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.304930 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.287107 | `azmcp_appconfig_kv_set` | ❌ |
| 11 | 0.259893 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.256832 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.250432 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.233767 | `azmcp_workbooks_delete` | ❌ |
| 15 | 0.210905 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.209234 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.203672 | `azmcp_sql_server_create` | ❌ |
| 18 | 0.197598 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.196937 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.189634 | `azmcp_sql_server_delete` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595707 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.454041 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.452551 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.418310 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.413377 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.399690 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.365954 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.351043 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.331651 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 10 | 0.330571 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.316126 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.280997 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.254835 | `azmcp_workbooks_delete` | ❌ |
| 14 | 0.250270 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.218097 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.211580 | `azmcp_subscription_list` | ❌ |
| 17 | 0.204904 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.189527 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.187649 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.181210 | `azmcp_workbooks_create` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619480 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.517804 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.480907 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.444483 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.381873 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.365641 | `azmcp_keyvault_key_get` | ❌ |
| 7 | 0.358637 | `azmcp_keyvault_secret_create` | ❌ |
| 8 | 0.341569 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.279774 | `azmcp_keyvault_key_list` | ❌ |
| 10 | 0.274910 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 11 | 0.243122 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.224245 | `azmcp_workbooks_delete` | ❌ |
| 13 | 0.209976 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.207206 | `azmcp_storage_blob_upload` | ❌ |
| 15 | 0.189880 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.187278 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.175139 | `azmcp_subscription_list` | ❌ |
| 18 | 0.174460 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.172686 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.167599 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595418 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452544 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.441653 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.408031 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.392277 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.390758 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.353230 | `azmcp_appconfig_kv_set` | ❌ |
| 8 | 0.325428 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.306406 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 10 | 0.305748 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.235766 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.230271 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.227373 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.221101 | `azmcp_sql_db_create` | ❌ |
| 15 | 0.196149 | `azmcp_workbooks_delete` | ❌ |
| 16 | 0.182997 | `azmcp_sql_db_update` | ❌ |
| 17 | 0.182012 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.161890 | `azmcp_workbooks_create` | ❌ |
| 19 | 0.161597 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.159675 | `azmcp_subscription_list` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726225 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.583110 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.531988 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.515236 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.486236 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.478100 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.453226 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.449821 | `azmcp_keyvault_certificate_import` | ❌ |
| 9 | 0.431201 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.418995 | `azmcp_keyvault_key_get` | ❌ |
| 11 | 0.408042 | `azmcp_subscription_list` | ❌ |
| 12 | 0.394434 | `azmcp_search_service_list` | ❌ |
| 13 | 0.393940 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.362953 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.362873 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.358938 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.350862 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.339860 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 19 | 0.336796 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 20 | 0.330749 | `azmcp_redis_cache_list` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615623 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.522453 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.475156 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.461455 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.448139 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.446003 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.433093 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.422019 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.420506 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.397031 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.382082 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.362782 | `azmcp_subscription_list` | ❌ |
| 13 | 0.355022 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.344466 | `azmcp_search_service_list` | ❌ |
| 15 | 0.323177 | `azmcp_role_assignment_list` | ❌ |
| 16 | 0.309942 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.305651 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.295917 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.290719 | `azmcp_search_index_get` | ❌ |
| 20 | 0.286707 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624781 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.519739 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.510463 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.505534 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.497356 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.442299 | `azmcp_keyvault_secret_list` | ❌ |
| 7 | 0.429075 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.418505 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.410237 | `azmcp_keyvault_secret_get` | ❌ |
| 10 | 0.373641 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.367613 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.344813 | `azmcp_search_service_list` | ❌ |
| 13 | 0.344599 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.332664 | `azmcp_subscription_list` | ❌ |
| 15 | 0.303707 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.297482 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.294830 | `azmcp_search_index_get` | ❌ |
| 18 | 0.292905 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.287082 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.279813 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 144

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672715 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.553990 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.511905 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.507062 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.492720 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.450104 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.447793 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.413220 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.406736 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.398143 | `azmcp_keyvault_key_get` | ❌ |
| 11 | 0.387661 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.386498 | `azmcp_subscription_list` | ❌ |
| 13 | 0.379358 | `azmcp_search_service_list` | ❌ |
| 14 | 0.361476 | `azmcp_monitor_table_type_list` | ❌ |
| 15 | 0.357200 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.355418 | `azmcp_role_assignment_list` | ❌ |
| 17 | 0.346059 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.343744 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.338369 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.332920 | `azmcp_monitor_table_list` | ❌ |

---

## Test 145

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747513 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.594216 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.558771 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.515568 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.491385 | `azmcp_keyvault_certificate_create` | ❌ |
| 6 | 0.460786 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.426252 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.399392 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.380821 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.378614 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.378269 | `azmcp_subscription_list` | ❌ |
| 12 | 0.374340 | `azmcp_search_service_list` | ❌ |
| 13 | 0.365517 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.348720 | `azmcp_mysql_table_list` | ❌ |
| 15 | 0.343591 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.328744 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.323968 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.317045 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.312041 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.309550 | `azmcp_mysql_database_list` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639788 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.512471 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.507559 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.483041 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.464713 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.455872 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.448957 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.405319 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.403553 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.403266 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.401215 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.395025 | `azmcp_subscription_list` | ❌ |
| 13 | 0.369554 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.360162 | `azmcp_search_service_list` | ❌ |
| 15 | 0.342555 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.341598 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.340827 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.340528 | `azmcp_monitor_table_type_list` | ❌ |
| 19 | 0.326085 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.322559 | `azmcp_mysql_server_list` | ❌ |

---

## Test 147

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661429 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456549 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.451805 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.429578 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.399288 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.397170 | `azmcp_appconfig_kv_set` | ❌ |
| 7 | 0.375846 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.372087 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.348165 | `azmcp_keyvault_certificate_list` | ❌ |
| 10 | 0.338066 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.335249 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 12 | 0.330489 | `azmcp_keyvault_certificate_get` | ❌ |
| 13 | 0.283856 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.276171 | `azmcp_storage_account_get` | ❌ |
| 15 | 0.261807 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.230099 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.223638 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.215833 | `azmcp_subscription_list` | ❌ |
| 19 | 0.212007 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.199618 | `azmcp_sql_server_firewall-rule_create` | ❌ |

---

## Test 148

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641070 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428502 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.422921 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.420045 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.405644 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.392529 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.385990 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 8 | 0.380906 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.347115 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.328934 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.294628 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.255583 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.234445 | `azmcp_sql_db_create` | ❌ |
| 14 | 0.224488 | `azmcp_monitor_table_type_list` | ❌ |
| 15 | 0.217599 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.216636 | `azmcp_subscription_list` | ❌ |
| 17 | 0.210036 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.198030 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.185361 | `azmcp_storage_blob_container_create` | ❌ |
| 20 | 0.183409 | `azmcp_search_index_get` | ❌ |

---

## Test 149

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547493 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.463557 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.447581 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.420366 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.404350 | `azmcp_keyvault_certificate_import` | ❌ |
| 6 | 0.398670 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.384348 | `azmcp_keyvault_secret_get` | ❌ |
| 8 | 0.369976 | `azmcp_keyvault_certificate_list` | ❌ |
| 9 | 0.349688 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.346151 | `azmcp_appconfig_kv_set` | ❌ |
| 11 | 0.306311 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.277139 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.251124 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.230824 | `azmcp_subscription_list` | ❌ |
| 15 | 0.227708 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.226595 | `azmcp_workbooks_delete` | ❌ |
| 17 | 0.224872 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.223473 | `azmcp_sql_server_create` | ❌ |
| 19 | 0.221834 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.219613 | `azmcp_workbooks_create` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641369 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.501636 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.491825 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.464557 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.451016 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.391097 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.380663 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.379135 | `azmcp_keyvault_secret_get` | ❌ |
| 9 | 0.375945 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.368655 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.358426 | `azmcp_appconfig_kv_set` | ❌ |
| 12 | 0.344933 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.293393 | `azmcp_sql_server_create` | ❌ |
| 14 | 0.287655 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.275938 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.235474 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.234865 | `azmcp_storage_blob_container_create` | ❌ |
| 18 | 0.226237 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 19 | 0.224445 | `azmcp_workbooks_delete` | ❌ |
| 20 | 0.221689 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571700 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443483 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.434425 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.422040 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.400377 | `azmcp_keyvault_certificate_import` | ❌ |
| 6 | 0.370799 | `azmcp_keyvault_key_list` | ❌ |
| 7 | 0.367662 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 8 | 0.357332 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.349930 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.349202 | `azmcp_keyvault_secret_get` | ❌ |
| 11 | 0.318472 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.276217 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.254549 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.231306 | `azmcp_workbooks_create` | ❌ |
| 15 | 0.227297 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.225987 | `azmcp_sql_server_create` | ❌ |
| 17 | 0.217703 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.189379 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.188840 | `azmcp_sql_elastic-pool_list` | ❌ |
| 20 | 0.184490 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549557 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.468211 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.452845 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.439963 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.432378 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.426572 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.396623 | `azmcp_keyvault_certificate_list` | ❌ |
| 8 | 0.379566 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.376229 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 10 | 0.371986 | `azmcp_keyvault_secret_create` | ❌ |
| 11 | 0.369350 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.328787 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.305777 | `azmcp_subscription_list` | ❌ |
| 14 | 0.280988 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.279409 | `azmcp_search_index_get` | ❌ |
| 16 | 0.276631 | `azmcp_search_service_list` | ❌ |
| 17 | 0.274348 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.268767 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.267576 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.256151 | `azmcp_quota_usage_check` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629390 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.532389 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.495673 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.475206 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.456897 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.452242 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.433747 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.428763 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.394124 | `azmcp_keyvault_certificate_list` | ❌ |
| 10 | 0.375858 | `azmcp_aks_nodepool_get` | ❌ |
| 11 | 0.374537 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.373932 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.368652 | `azmcp_search_index_get` | ❌ |
| 14 | 0.346240 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.340585 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.337223 | `azmcp_servicebus_queue_details` | ❌ |
| 17 | 0.326328 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.315909 | `azmcp_subscription_list` | ❌ |
| 19 | 0.315893 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.311508 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484645 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.443182 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.409388 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.383519 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.371486 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.371084 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.368861 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.342757 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.334854 | `azmcp_keyvault_certificate_create` | ❌ |
| 10 | 0.334377 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.333757 | `azmcp_keyvault_certificate_list` | ❌ |
| 12 | 0.286153 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.265631 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.233543 | `azmcp_subscription_list` | ❌ |
| 15 | 0.227292 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 16 | 0.218318 | `azmcp_servicebus_queue_details` | ❌ |
| 17 | 0.214910 | `azmcp_search_index_get` | ❌ |
| 18 | 0.213794 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.197995 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.197734 | `azmcp_redis_cache_accesspolicy_list` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590303 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.488213 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.460796 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.452075 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.440938 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.417546 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.398120 | `azmcp_keyvault_key_list` | ❌ |
| 8 | 0.389917 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.374129 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 10 | 0.353134 | `azmcp_appconfig_kv_list` | ❌ |
| 11 | 0.349098 | `azmcp_keyvault_certificate_list` | ❌ |
| 12 | 0.339313 | `azmcp_keyvault_certificate_import` | ❌ |
| 13 | 0.338023 | `azmcp_search_index_get` | ❌ |
| 14 | 0.318239 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.296062 | `azmcp_mysql_table_schema_get` | ❌ |
| 16 | 0.286741 | `azmcp_servicebus_queue_details` | ❌ |
| 17 | 0.273934 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.273132 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.271225 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 20 | 0.271125 | `azmcp_sql_db_show` | ❌ |

---

## Test 156

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518886 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.429131 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.422536 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.406215 | `azmcp_appconfig_kv_show` | ❌ |
| 5 | 0.395959 | `azmcp_keyvault_key_list` | ❌ |
| 6 | 0.387928 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.386015 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.379025 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.376384 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 10 | 0.355070 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.343490 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.336655 | `azmcp_keyvault_certificate_create` | ❌ |
| 13 | 0.329865 | `azmcp_storage_blob_get` | ❌ |
| 14 | 0.301328 | `azmcp_mysql_table_schema_get` | ❌ |
| 15 | 0.297365 | `azmcp_search_index_get` | ❌ |
| 16 | 0.288239 | `azmcp_monitor_metrics_definitions` | ❌ |
| 17 | 0.276450 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.275942 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.272172 | `azmcp_workbooks_show` | ❌ |
| 20 | 0.269592 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 157

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701448 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.601614 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.587427 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.498767 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.468044 | `azmcp_cosmos_database_list` | ❌ |
| 6 | 0.458200 | `azmcp_keyvault_key_get` | ❌ |
| 7 | 0.443785 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.439167 | `azmcp_appconfig_kv_list` | ❌ |
| 9 | 0.430322 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.426909 | `azmcp_subscription_list` | ❌ |
| 11 | 0.423833 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.417967 | `azmcp_keyvault_secret_get` | ❌ |
| 13 | 0.408341 | `azmcp_search_service_list` | ❌ |
| 14 | 0.387510 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.373903 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.368258 | `azmcp_mysql_database_list` | ❌ |
| 17 | 0.354970 | `azmcp_monitor_table_list` | ❌ |
| 18 | 0.353723 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.350154 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.348597 | `azmcp_role_assignment_list` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549453 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.506815 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.475587 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.455683 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.446522 | `azmcp_keyvault_secret_list` | ❌ |
| 6 | 0.424356 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.421475 | `azmcp_cosmos_account_list` | ❌ |
| 8 | 0.420839 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.406776 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.405205 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.377735 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.356816 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.353390 | `azmcp_subscription_list` | ❌ |
| 14 | 0.327200 | `azmcp_search_service_list` | ❌ |
| 15 | 0.316124 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.308976 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.306567 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.297022 | `azmcp_search_index_get` | ❌ |
| 19 | 0.295954 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.293404 | `azmcp_quota_usage_check` | ❌ |

---

## Test 159

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581970 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501556 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.476470 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.472414 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.467129 | `azmcp_keyvault_key_create` | ❌ |
| 6 | 0.444134 | `azmcp_keyvault_secret_get` | ❌ |
| 7 | 0.416919 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.415017 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.406630 | `azmcp_keyvault_certificate_get` | ❌ |
| 10 | 0.397060 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.396268 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.353983 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.344539 | `azmcp_search_service_list` | ❌ |
| 14 | 0.342161 | `azmcp_subscription_list` | ❌ |
| 15 | 0.321891 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.310049 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.301891 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.300518 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.296885 | `azmcp_search_index_get` | ❌ |
| 20 | 0.291704 | `azmcp_role_assignment_list` | ❌ |

---

## Test 160

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641314 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.559613 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.553553 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.475992 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.461397 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.447336 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.442855 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.439089 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.427248 | `azmcp_appconfig_kv_list` | ❌ |
| 10 | 0.422399 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.421692 | `azmcp_keyvault_secret_get` | ❌ |
| 12 | 0.405945 | `azmcp_subscription_list` | ❌ |
| 13 | 0.405838 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.399675 | `azmcp_monitor_table_type_list` | ❌ |
| 15 | 0.397041 | `azmcp_search_service_list` | ❌ |
| 16 | 0.390198 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.375222 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.362561 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.356176 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.354871 | `azmcp_redis_cache_list` | ❌ |

---

## Test 161

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.723266 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611472 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.611185 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.441881 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.440237 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.425927 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.406526 | `azmcp_keyvault_secret_get` | ❌ |
| 8 | 0.402910 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.398274 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.392351 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.391599 | `azmcp_appconfig_kv_list` | ❌ |
| 12 | 0.383112 | `azmcp_subscription_list` | ❌ |
| 13 | 0.376853 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.371788 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.371133 | `azmcp_search_service_list` | ❌ |
| 16 | 0.334297 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.328897 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.325507 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.323988 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.323510 | `azmcp_mysql_database_list` | ❌ |

---

## Test 162

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570444 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.500207 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.490367 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.461941 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.460716 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.457958 | `azmcp_keyvault_secret_get` | ❌ |
| 8 | 0.454423 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.427683 | `azmcp_appconfig_kv_list` | ❌ |
| 10 | 0.427246 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.416532 | `azmcp_keyvault_certificate_get` | ❌ |
| 12 | 0.411961 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.403078 | `azmcp_subscription_list` | ❌ |
| 14 | 0.361323 | `azmcp_search_service_list` | ❌ |
| 15 | 0.356353 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.346934 | `azmcp_monitor_table_type_list` | ❌ |
| 17 | 0.346482 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.345239 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.339468 | `azmcp_search_index_get` | ❌ |
| 20 | 0.338654 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 163

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678482 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.553018 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.512856 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.475176 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.461437 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.448966 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.406698 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.397784 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.391024 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.387507 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 11 | 0.357221 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.355685 | `azmcp_keyvault_certificate_get` | ❌ |
| 13 | 0.288052 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.287943 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.287066 | `azmcp_workbooks_create` | ❌ |
| 16 | 0.246174 | `azmcp_storage_blob_container_create` | ❌ |
| 17 | 0.243340 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.218702 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 19 | 0.212873 | `azmcp_storage_blob_upload` | ❌ |
| 20 | 0.209815 | `azmcp_subscription_list` | ❌ |

---

## Test 164

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663094 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.519601 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.512233 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.458502 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.429785 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 6 | 0.421412 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.409186 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.401420 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.388480 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.370748 | `azmcp_keyvault_certificate_create` | ❌ |
| 11 | 0.311345 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.308706 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.256685 | `azmcp_postgres_server_param_set` | ❌ |
| 14 | 0.256473 | `azmcp_sql_db_create` | ❌ |
| 15 | 0.255243 | `azmcp_sql_db_update` | ❌ |
| 16 | 0.251053 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.231789 | `azmcp_mysql_server_param_set` | ❌ |
| 18 | 0.225637 | `azmcp_storage_blob_upload` | ❌ |
| 19 | 0.223339 | `azmcp_workbooks_delete` | ❌ |
| 20 | 0.209177 | `azmcp_subscription_list` | ❌ |

---

## Test 165

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639897 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.509674 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.485203 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.484680 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.448995 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 6 | 0.447027 | `azmcp_keyvault_certificate_import` | ❌ |
| 7 | 0.411509 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.392814 | `azmcp_keyvault_certificate_create` | ❌ |
| 9 | 0.391220 | `azmcp_keyvault_secret_list` | ❌ |
| 10 | 0.384378 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.338477 | `azmcp_storage_account_create` | ❌ |
| 12 | 0.336008 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.276962 | `azmcp_storage_blob_container_get` | ❌ |
| 14 | 0.261107 | `azmcp_sql_db_create` | ❌ |
| 15 | 0.237926 | `azmcp_storage_blob_upload` | ❌ |
| 16 | 0.229204 | `azmcp_subscription_list` | ❌ |
| 17 | 0.218708 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.216186 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.215486 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.211898 | `azmcp_workbooks_create` | ❌ |

---

## Test 166

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675145 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.499612 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.498228 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.479174 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.458574 | `azmcp_appconfig_kv_set` | ❌ |
| 6 | 0.444240 | `azmcp_keyvault_certificate_create` | ❌ |
| 7 | 0.421114 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 8 | 0.403133 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.390897 | `azmcp_keyvault_key_get` | ❌ |
| 10 | 0.354069 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.301166 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.292024 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.267714 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.263706 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.248670 | `azmcp_storage_blob_upload` | ❌ |
| 16 | 0.230380 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.224220 | `azmcp_workbooks_create` | ❌ |
| 18 | 0.219376 | `azmcp_storage_blob_container_create` | ❌ |
| 19 | 0.214690 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.206496 | `azmcp_loadtesting_testrun_update` | ❌ |

---

## Test 167

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571612 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.513767 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.441223 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.417943 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.408242 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.400708 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.394478 | `azmcp_keyvault_certificate_import` | ❌ |
| 8 | 0.361664 | `azmcp_keyvault_secret_list` | ❌ |
| 9 | 0.358807 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.352518 | `azmcp_keyvault_certificate_get` | ❌ |
| 11 | 0.348194 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.309610 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.263012 | `azmcp_loadtesting_testrun_update` | ❌ |
| 14 | 0.260523 | `azmcp_mysql_server_param_set` | ❌ |
| 15 | 0.254274 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.252734 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.247254 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.241653 | `azmcp_workbooks_update` | ❌ |
| 19 | 0.230660 | `azmcp_postgres_server_param_set` | ❌ |
| 20 | 0.219535 | `azmcp_sql_db_create` | ❌ |

---

## Test 168

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602769 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504212 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.501397 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.478769 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.420938 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.394974 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.389659 | `azmcp_keyvault_key_list` | ❌ |
| 9 | 0.382573 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.375047 | `azmcp_keyvault_certificate_list` | ❌ |
| 11 | 0.369409 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.348100 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.301544 | `azmcp_subscription_list` | ❌ |
| 14 | 0.294689 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.284255 | `azmcp_search_index_get` | ❌ |
| 16 | 0.281795 | `azmcp_search_service_list` | ❌ |
| 17 | 0.260730 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.257699 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.255278 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.254379 | `azmcp_sql_db_show` | ❌ |

---

## Test 169

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653871 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.566786 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.496050 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.485249 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.483567 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.481364 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.444666 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.436761 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.387515 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.384526 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.378874 | `azmcp_search_index_get` | ❌ |
| 12 | 0.372243 | `azmcp_keyvault_certificate_list` | ❌ |
| 13 | 0.370109 | `azmcp_keyvault_certificate_import` | ❌ |
| 14 | 0.354501 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.346830 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.335079 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.333928 | `azmcp_servicebus_queue_details` | ❌ |
| 18 | 0.324284 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.321621 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.311552 | `azmcp_subscription_list` | ❌ |

---

## Test 170

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578479 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.492213 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.488705 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.443676 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.420533 | `azmcp_keyvault_certificate_get` | ❌ |
| 6 | 0.403982 | `azmcp_keyvault_key_create` | ❌ |
| 7 | 0.388485 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.377552 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.367132 | `azmcp_keyvault_certificate_create` | ❌ |
| 10 | 0.364454 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.357450 | `azmcp_keyvault_certificate_list` | ❌ |
| 12 | 0.348267 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.312081 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.278672 | `azmcp_subscription_list` | ❌ |
| 15 | 0.273094 | `azmcp_search_index_get` | ❌ |
| 16 | 0.254472 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.253592 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 18 | 0.253425 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.251718 | `azmcp_monitor_resource_log_query` | ❌ |
| 20 | 0.250263 | `azmcp_mysql_server_param_get` | ❌ |

---

## Test 171

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649267 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.546992 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.492583 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.491596 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.480354 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.465487 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.441265 | `azmcp_storage_blob_container_get` | ❌ |
| 8 | 0.440721 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.378375 | `azmcp_search_index_get` | ❌ |
| 10 | 0.373737 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.370606 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.370291 | `azmcp_keyvault_key_create` | ❌ |
| 13 | 0.370223 | `azmcp_keyvault_certificate_list` | ❌ |
| 14 | 0.351129 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.338686 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.321929 | `azmcp_sql_server_show` | ❌ |
| 17 | 0.316244 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.310314 | `azmcp_servicebus_queue_details` | ❌ |
| 19 | 0.306118 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.302840 | `azmcp_subscription_list` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573458 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.471281 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.467246 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.443770 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.406298 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.405955 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.397429 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.388047 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.367111 | `azmcp_keyvault_key_create` | ❌ |
| 10 | 0.351589 | `azmcp_storage_blob_get` | ❌ |
| 11 | 0.350789 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.346463 | `azmcp_keyvault_certificate_list` | ❌ |
| 13 | 0.343587 | `azmcp_keyvault_certificate_create` | ❌ |
| 14 | 0.326409 | `azmcp_monitor_metrics_definitions` | ❌ |
| 15 | 0.322010 | `azmcp_search_index_get` | ❌ |
| 16 | 0.302328 | `azmcp_marketplace_product_get` | ❌ |
| 17 | 0.301796 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.301208 | `azmcp_mysql_table_schema_get` | ❌ |
| 19 | 0.299082 | `azmcp_mysql_server_config_get` | ❌ |
| 20 | 0.296979 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 173

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701227 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.563736 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.538392 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.499642 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.455500 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.433290 | `azmcp_keyvault_secret_create` | ❌ |
| 7 | 0.433185 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.418971 | `azmcp_keyvault_key_get` | ❌ |
| 9 | 0.417973 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.391082 | `azmcp_subscription_list` | ❌ |
| 11 | 0.388773 | `azmcp_search_service_list` | ❌ |
| 12 | 0.387663 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.377838 | `azmcp_keyvault_certificate_get` | ❌ |
| 14 | 0.366988 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.340503 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 16 | 0.337595 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.334206 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.331203 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.326430 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.322010 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |

---

## Test 174

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555681 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.543861 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.464661 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.433489 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.429826 | `azmcp_keyvault_certificate_get` | ❌ |
| 7 | 0.428284 | `azmcp_keyvault_certificate_list` | ❌ |
| 8 | 0.410957 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.401434 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.397448 | `azmcp_keyvault_key_create` | ❌ |
| 11 | 0.385852 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.370855 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.345256 | `azmcp_subscription_list` | ❌ |
| 14 | 0.328354 | `azmcp_search_service_list` | ❌ |
| 15 | 0.305225 | `azmcp_search_index_get` | ❌ |
| 16 | 0.303769 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.299023 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.294614 | `azmcp_mysql_server_list` | ❌ |
| 19 | 0.293826 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.290273 | `azmcp_mysql_database_list` | ❌ |

---

## Test 175

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572540 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.529258 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.493761 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.475273 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.460262 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.452339 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.423709 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.412124 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.411803 | `azmcp_keyvault_certificate_import` | ❌ |
| 10 | 0.407537 | `azmcp_keyvault_certificate_get` | ❌ |
| 11 | 0.399680 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.388362 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.339263 | `azmcp_subscription_list` | ❌ |
| 14 | 0.325306 | `azmcp_search_service_list` | ❌ |
| 15 | 0.315223 | `azmcp_redis_cache_list` | ❌ |
| 16 | 0.312209 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.309381 | `azmcp_search_index_get` | ❌ |
| 18 | 0.308713 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.301464 | `azmcp_storage_account_create` | ❌ |
| 20 | 0.292964 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 176

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624290 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.559681 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.517565 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.479547 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.441075 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.431079 | `azmcp_keyvault_key_get` | ❌ |
| 7 | 0.421769 | `azmcp_cosmos_database_list` | ❌ |
| 8 | 0.421553 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.412649 | `azmcp_storage_account_get` | ❌ |
| 10 | 0.412279 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.404613 | `azmcp_keyvault_key_create` | ❌ |
| 12 | 0.389021 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.377304 | `azmcp_subscription_list` | ❌ |
| 14 | 0.365717 | `azmcp_search_service_list` | ❌ |
| 15 | 0.359226 | `azmcp_mysql_database_list` | ❌ |
| 16 | 0.355228 | `azmcp_monitor_table_type_list` | ❌ |
| 17 | 0.339337 | `azmcp_monitor_table_list` | ❌ |
| 18 | 0.332727 | `azmcp_role_assignment_list` | ❌ |
| 19 | 0.331508 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.326518 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 177

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742358 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.601183 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.567880 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.496127 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.435433 | `azmcp_keyvault_secret_create` | ❌ |
| 6 | 0.418413 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.402782 | `azmcp_keyvault_key_get` | ❌ |
| 8 | 0.388025 | `azmcp_search_service_list` | ❌ |
| 9 | 0.383981 | `azmcp_cosmos_database_container_list` | ❌ |
| 10 | 0.377814 | `azmcp_cosmos_database_list` | ❌ |
| 11 | 0.376911 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.373574 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.372316 | `azmcp_subscription_list` | ❌ |
| 14 | 0.365791 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.354830 | `azmcp_mysql_table_list` | ❌ |
| 16 | 0.334126 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.322255 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.318129 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.317808 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.315987 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 178

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567110 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.522398 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.476309 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.462676 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `azmcp_keyvault_key_get` | ❌ |
| 6 | 0.440008 | `azmcp_keyvault_certificate_list` | ❌ |
| 7 | 0.413606 | `azmcp_keyvault_key_create` | ❌ |
| 8 | 0.411700 | `azmcp_storage_account_get` | ❌ |
| 9 | 0.409511 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.409141 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.407517 | `azmcp_keyvault_certificate_get` | ❌ |
| 12 | 0.376522 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.361737 | `azmcp_subscription_list` | ❌ |
| 14 | 0.325238 | `azmcp_mysql_database_list` | ❌ |
| 15 | 0.321954 | `azmcp_search_service_list` | ❌ |
| 16 | 0.316216 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.303465 | `azmcp_role_assignment_list` | ❌ |
| 18 | 0.300470 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.299650 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.297014 | `azmcp_search_index_get` | ❌ |

---

## Test 179

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
| 9 | 0.419629 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.399345 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.391924 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.390959 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.390819 | `azmcp_appconfig_kv_list` | ❌ |
| 14 | 0.390141 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.371630 | `azmcp_mysql_server_param_get` | ❌ |
| 16 | 0.370108 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.369397 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.367841 | `azmcp_redis_cluster_list` | ❌ |
| 19 | 0.360695 | `azmcp_storage_blob_get` | ❌ |
| 20 | 0.355388 | `azmcp_sql_server_list` | ❌ |

---

## Test 180

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666849 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.589060 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.545820 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.530314 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.508226 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.461466 | `azmcp_sql_db_show` | ❌ |
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
| 17 | 0.382236 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.377088 | `azmcp_storage_blob_get` | ❌ |
| 19 | 0.366088 | `azmcp_search_index_get` | ❌ |
| 20 | 0.362332 | `azmcp_sql_db_list` | ❌ |

---

## Test 181

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567273 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563045 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.493940 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.486040 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.380301 | `azmcp_mysql_server_config_get` | ❌ |
| 6 | 0.368584 | `azmcp_kusto_cluster_get` | ❌ |
| 7 | 0.348885 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.342696 | `azmcp_loadtesting_test_get` | ❌ |
| 9 | 0.340293 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.334923 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.334860 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.329324 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.315228 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.314513 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.309738 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.299047 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.296889 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.296592 | `azmcp_postgres_server_config_get` | ❌ |
| 19 | 0.289342 | `azmcp_mysql_server_param_get` | ❌ |
| 20 | 0.275751 | `azmcp_sql_db_show` | ❌ |

---

## Test 182

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661426 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.578650 | `azmcp_aks_cluster_list` | ❌ |
| 3 | 0.563549 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.534089 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.503925 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.434587 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.433913 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.419338 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.418518 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.417836 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.405658 | `azmcp_storage_account_get` | ❌ |
| 12 | 0.404415 | `azmcp_storage_blob_get` | ❌ |
| 13 | 0.402335 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.398616 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.391699 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.384782 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.383956 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.372812 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.367514 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.359877 | `azmcp_acr_registry_repository_list` | ❌ |

---

## Test 183

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801113 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.690255 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.599940 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.594509 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.562043 | `azmcp_search_service_list` | ❌ |
| 6 | 0.560861 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.543684 | `azmcp_monitor_workspace_list` | ❌ |
| 8 | 0.515922 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.509202 | `azmcp_kusto_database_list` | ❌ |
| 10 | 0.502401 | `azmcp_subscription_list` | ❌ |
| 11 | 0.498286 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 12 | 0.498239 | `azmcp_group_list` | ❌ |
| 13 | 0.495977 | `azmcp_postgres_server_list` | ❌ |
| 14 | 0.486167 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.483592 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.482328 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.481469 | `azmcp_grafana_list` | ❌ |
| 18 | 0.457949 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.452959 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.452681 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 184

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608081 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.536412 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.500890 | `azmcp_aks_nodepool_list` | ❌ |
| 4 | 0.492910 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.455228 | `azmcp_search_service_list` | ❌ |
| 6 | 0.446270 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.428444 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.416475 | `azmcp_aks_nodepool_get` | ❌ |
| 9 | 0.409711 | `azmcp_kusto_cluster_get` | ❌ |
| 10 | 0.408385 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.392997 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.376362 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.371809 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.371535 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.370963 | `azmcp_search_index_get` | ❌ |
| 16 | 0.363804 | `azmcp_subscription_list` | ❌ |
| 17 | 0.361928 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.358213 | `azmcp_storage_blob_container_get` | ❌ |
| 19 | 0.356926 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.356016 | `azmcp_storage_account_get` | ❌ |

---

## Test 185

**Expected Tool:** `azmcp_aks_cluster_list`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623942 | `azmcp_aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.538749 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.530023 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.466749 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.449602 | `azmcp_kusto_cluster_list` | ❌ |
| 6 | 0.416564 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.392083 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.379421 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.378826 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.377567 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.374585 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.363981 | `azmcp_deploy_app_logs_get` | ❌ |
| 13 | 0.353365 | `azmcp_search_service_list` | ❌ |
| 14 | 0.345290 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.345241 | `azmcp_kusto_cluster_get` | ❌ |
| 16 | 0.337354 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.317977 | `azmcp_sql_elastic-pool_list` | ❌ |
| 18 | 0.317212 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.312354 | `azmcp_subscription_list` | ❌ |
| 20 | 0.311971 | `azmcp_quota_usage_check` | ❌ |

---

## Test 186

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.753920 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.699423 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.597308 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.498544 | `azmcp_aks_cluster_list` | ❌ |
| 5 | 0.482683 | `azmcp_kusto_cluster_get` | ❌ |
| 6 | 0.468392 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.463192 | `azmcp_sql_elastic-pool_list` | ❌ |
| 8 | 0.434875 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.414751 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 10 | 0.401610 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.399215 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.383606 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 13 | 0.382352 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.379665 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.378264 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.378238 | `azmcp_search_index_get` | ❌ |
| 17 | 0.370172 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.367944 | `azmcp_keyvault_key_get` | ❌ |
| 19 | 0.362512 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.357874 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 187

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678158 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.640096 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.481312 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.458596 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.445979 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.440182 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.391067 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 8 | 0.384600 | `azmcp_loadtesting_test_get` | ❌ |
| 9 | 0.371847 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.367455 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.365231 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.357721 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.350998 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.350992 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.344818 | `azmcp_sql_db_show` | ❌ |
| 16 | 0.343726 | `azmcp_kusto_cluster_get` | ❌ |
| 17 | 0.342564 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.338364 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.332531 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.322685 | `azmcp_appconfig_kv_show` | ❌ |

---

## Test 188

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599506 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.582325 | `azmcp_aks_nodepool_list` | ❌ |
| 3 | 0.412109 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.391576 | `azmcp_aks_cluster_list` | ❌ |
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
| 17 | 0.289422 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.287015 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.283171 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.276058 | `azmcp_sql_db_list` | ❌ |

---

## Test 189

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694117 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.615516 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.531980 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.506624 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.487707 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.461701 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.446699 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.440646 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.438637 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.435177 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.431369 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.418681 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 13 | 0.413085 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.407783 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.404890 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.399222 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.393876 | `azmcp_group_list` | ❌ |
| 18 | 0.391869 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.389071 | `azmcp_redis_cluster_database_list` | ❌ |
| 20 | 0.385781 | `azmcp_workbooks_list` | ❌ |

---

## Test 190

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712299 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.644451 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.547383 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.510269 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509732 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 6 | 0.497966 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.447545 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.442122 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.441482 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.433138 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.430830 | `azmcp_acr_registry_repository_list` | ❌ |
| 12 | 0.430739 | `azmcp_kusto_cluster_list` | ❌ |
| 13 | 0.421390 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.408986 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.408569 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.407619 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.390197 | `azmcp_redis_cluster_database_list` | ❌ |
| 18 | 0.388937 | `azmcp_group_list` | ❌ |
| 19 | 0.387647 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.383234 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 191

**Expected Tool:** `azmcp_aks_nodepool_list`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623138 | `azmcp_aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.580535 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.453773 | `azmcp_aks_cluster_list` | ❌ |
| 4 | 0.443902 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.425448 | `azmcp_sql_elastic-pool_list` | ❌ |
| 6 | 0.409286 | `azmcp_aks_cluster_get` | ❌ |
| 7 | 0.386949 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.378905 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.368944 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.363262 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.360005 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 12 | 0.356345 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.356139 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.354542 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.347994 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.335285 | `azmcp_sql_server_list` | ❌ |
| 17 | 0.329036 | `azmcp_sql_db_list` | ❌ |
| 18 | 0.324508 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.324257 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.323568 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 192

**Expected Tool:** `azmcp_loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585409 | `azmcp_loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.531850 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.508709 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.413787 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.394676 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.390081 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.346526 | `azmcp_loadtesting_testrun_update` | ❌ |
| 8 | 0.338668 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.338173 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.337311 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.325279 | `azmcp_keyvault_certificate_create` | ❌ |
| 12 | 0.323519 | `azmcp_storage_account_create` | ❌ |
| 13 | 0.310144 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.296991 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.292107 | `azmcp_keyvault_key_create` | ❌ |
| 16 | 0.290957 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.289844 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.288940 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.280439 | `azmcp_sql_server_create` | ❌ |
| 20 | 0.273769 | `azmcp_sql_server_list` | ❌ |

---

## Test 193

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642272 | `azmcp_loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.608723 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.574283 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.534033 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.473230 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.469690 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.437034 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.404563 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.397382 | `azmcp_group_list` | ❌ |
| 10 | 0.379214 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.373008 | `azmcp_loadtesting_testrun_update` | ❌ |
| 12 | 0.369857 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.365514 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.360656 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.354303 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.346950 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 17 | 0.341295 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.340345 | `azmcp_extension_azqr` | ❌ |
| 19 | 0.329316 | `azmcp_sql_db_show` | ❌ |
| 20 | 0.328497 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 194

**Expected Tool:** `azmcp_loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718157 | `azmcp_loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.596726 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.514675 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.476713 | `azmcp_loadtesting_testrun_create` | ❌ |
| 5 | 0.443117 | `azmcp_loadtesting_test_get` | ❌ |
| 6 | 0.442167 | `azmcp_workbooks_create` | ❌ |
| 7 | 0.416919 | `azmcp_group_list` | ❌ |
| 8 | 0.407752 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.394967 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.382774 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.370084 | `azmcp_loadtesting_testrun_get` | ❌ |
| 12 | 0.369786 | `azmcp_sql_server_list` | ❌ |
| 13 | 0.369409 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.356831 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.350916 | `azmcp_loadtesting_testrun_update` | ❌ |
| 16 | 0.343649 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.342213 | `azmcp_redis_cluster_list` | ❌ |
| 18 | 0.341251 | `azmcp_grafana_list` | ❌ |
| 19 | 0.335675 | `azmcp_redis_cache_list` | ❌ |
| 20 | 0.326617 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 195

**Expected Tool:** `azmcp_loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737917 | `azmcp_loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.592295 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.577464 | `azmcp_group_list` | ❌ |
| 4 | 0.565565 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.561537 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.526378 | `azmcp_workbooks_list` | ❌ |
| 7 | 0.515624 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.511602 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.506184 | `azmcp_loadtesting_test_get` | ❌ |
| 10 | 0.497916 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.487330 | `azmcp_grafana_list` | ❌ |
| 12 | 0.483681 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.478586 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.473444 | `azmcp_search_service_list` | ❌ |
| 15 | 0.473287 | `azmcp_mysql_server_list` | ❌ |
| 16 | 0.470869 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.463420 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.452190 | `azmcp_monitor_workspace_list` | ❌ |
| 19 | 0.447138 | `azmcp_quota_region_availability_list` | ❌ |
| 20 | 0.433793 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 196

**Expected Tool:** `azmcp_loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621901 | `azmcp_loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.592841 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.540949 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.530882 | `azmcp_loadtesting_testrun_update` | ❌ |
| 5 | 0.488143 | `azmcp_loadtesting_testrun_get` | ❌ |
| 6 | 0.469444 | `azmcp_loadtesting_test_get` | ❌ |
| 7 | 0.418431 | `azmcp_loadtesting_testrun_list` | ❌ |
| 8 | 0.411567 | `azmcp_loadtesting_testresource_list` | ❌ |
| 9 | 0.402120 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.383719 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.362695 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.323774 | `azmcp_sql_server_create` | ❌ |
| 13 | 0.308740 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.306420 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.302128 | `azmcp_foundry_agents_connect` | ❌ |
| 16 | 0.300269 | `azmcp_extension_azqr` | ❌ |
| 17 | 0.273429 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.272151 | `azmcp_sql_db_show` | ❌ |
| 19 | 0.267551 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.262297 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 197

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625325 | `azmcp_loadtesting_test_get` | ❌ |
| 2 | 0.602973 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.568403 | `azmcp_loadtesting_testrun_get` | ✅ **EXPECTED** |
| 4 | 0.561974 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.535261 | `azmcp_loadtesting_testrun_create` | ❌ |
| 6 | 0.496647 | `azmcp_loadtesting_testrun_list` | ❌ |
| 7 | 0.434486 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.415422 | `azmcp_loadtesting_testrun_update` | ❌ |
| 9 | 0.397888 | `azmcp_group_list` | ❌ |
| 10 | 0.394665 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.370202 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.366564 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.356330 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.353672 | `azmcp_sql_server_list` | ❌ |
| 15 | 0.352912 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.347000 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.339712 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.330715 | `azmcp_monitor_metrics_query` | ❌ |
| 19 | 0.329509 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.328877 | `azmcp_sql_db_show` | ❌ |

---

## Test 198

**Expected Tool:** `azmcp_loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615897 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.606058 | `azmcp_loadtesting_test_get` | ❌ |
| 3 | 0.569139 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.565093 | `azmcp_loadtesting_testrun_list` | ✅ **EXPECTED** |
| 5 | 0.535259 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.492783 | `azmcp_loadtesting_testrun_create` | ❌ |
| 7 | 0.432165 | `azmcp_group_list` | ❌ |
| 8 | 0.416453 | `azmcp_monitor_resource_log_query` | ❌ |
| 9 | 0.410933 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.406651 | `azmcp_loadtesting_test_create` | ❌ |
| 11 | 0.395915 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.392066 | `azmcp_loadtesting_testrun_update` | ❌ |
| 13 | 0.391147 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.375782 | `azmcp_monitor_metrics_query` | ❌ |
| 15 | 0.373875 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.367897 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.356833 | `azmcp_quota_region_availability_list` | ❌ |
| 18 | 0.342526 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.340466 | `azmcp_workbooks_show` | ❌ |
| 20 | 0.329464 | `azmcp_sql_db_list` | ❌ |

---

## Test 199

**Expected Tool:** `azmcp_loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659812 | `azmcp_loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.509265 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.454755 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.443828 | `azmcp_loadtesting_test_get` | ❌ |
| 5 | 0.422017 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.399670 | `azmcp_loadtesting_test_create` | ❌ |
| 7 | 0.384601 | `azmcp_loadtesting_testresource_list` | ❌ |
| 8 | 0.384237 | `azmcp_loadtesting_testrun_list` | ❌ |
| 9 | 0.332746 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.320124 | `azmcp_workbooks_update` | ❌ |
| 11 | 0.300023 | `azmcp_workbooks_create` | ❌ |
| 12 | 0.268124 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.267137 | `azmcp_appconfig_kv_set` | ❌ |
| 14 | 0.255408 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.253250 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.252149 | `azmcp_sql_server_list` | ❌ |
| 17 | 0.250017 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.245757 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.240916 | `azmcp_workbooks_delete` | ❌ |
| 20 | 0.234240 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 200

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
| 9 | 0.491777 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.489846 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.482825 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.479611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.459138 | `azmcp_eventgrid_topic_list` | ❌ |
| 14 | 0.457845 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.452244 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.447752 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.447597 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.441411 | `azmcp_group_list` | ❌ |
| 19 | 0.440392 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.436802 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 201

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
| 9 | 0.477193 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.472811 | `azmcp_redis_cluster_list` | ❌ |
| 11 | 0.460925 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.460322 | `azmcp_redis_cache_list` | ❌ |
| 13 | 0.451887 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.450971 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.448426 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.447269 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.445430 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.442506 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.438952 | `azmcp_grafana_list` | ❌ |
| 20 | 0.437939 | `azmcp_postgres_server_list` | ❌ |

---

## Test 202

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
| 7 | 0.466545 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.452844 | `azmcp_acr_registry_list` | ❌ |
| 9 | 0.443767 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.441712 | `azmcp_group_list` | ❌ |
| 11 | 0.433933 | `azmcp_workbooks_list` | ❌ |
| 12 | 0.412747 | `azmcp_search_service_list` | ❌ |
| 13 | 0.412709 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.409044 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.407704 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.406511 | `azmcp_mysql_database_list` | ❌ |
| 17 | 0.402926 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.398525 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.398168 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.397222 | `azmcp_functionapp_get` | ❌ |

---

## Test 203

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
| 10 | 0.188403 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.187133 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.176407 | `azmcp_marketplace_product_get` | ❌ |
| 13 | 0.175883 | `azmcp_postgres_server_param_get` | ❌ |
| 14 | 0.174849 | `azmcp_aks_nodepool_list` | ❌ |
| 15 | 0.172856 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 16 | 0.170883 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.169792 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.166729 | `azmcp_applens_resource_diagnose` | ❌ |
| 19 | 0.165332 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.165173 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 204

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
| 6 | 0.418743 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.411881 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 8 | 0.411221 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.405913 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.403188 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.402635 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.401697 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 13 | 0.401538 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.399919 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.398741 | `azmcp_subscription_list` | ❌ |
| 16 | 0.398576 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.395033 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.393969 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.393471 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.392605 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 205

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
| 6 | 0.323644 | `azmcp_servicebus_topic_details` | ❌ |
| 7 | 0.317352 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.302335 | `azmcp_aks_cluster_get` | ❌ |
| 9 | 0.294194 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.289350 | `azmcp_workbooks_show` | ❌ |
| 11 | 0.286194 | `azmcp_keyvault_key_get` | ❌ |
| 12 | 0.285577 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.283554 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.276826 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.275714 | `azmcp_keyvault_secret_get` | ❌ |
| 16 | 0.274388 | `azmcp_redis_cache_list` | ❌ |
| 17 | 0.266271 | `azmcp_foundry_models_list` | ❌ |
| 18 | 0.259116 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.257285 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.254258 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 206

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
| 7 | 0.302368 | `azmcp_foundry_agents_list` | ❌ |
| 8 | 0.290877 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.290185 | `azmcp_search_index_get` | ❌ |
| 10 | 0.287924 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.263954 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.263529 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.258243 | `azmcp_foundry_models_deployments_list` | ❌ |
| 14 | 0.254509 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.251537 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.250343 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.248885 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 18 | 0.247644 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.245634 | `azmcp_quota_usage_check` | ❌ |
| 20 | 0.245181 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 207

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461616 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385167 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.308769 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.260387 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.259318 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.238760 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.238238 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.237988 | `azmcp_grafana_list` | ❌ |
| 9 | 0.226689 | `azmcp_search_service_list` | ❌ |
| 10 | 0.221138 | `azmcp_appconfig_kv_show` | ❌ |
| 11 | 0.218709 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.208553 | `azmcp_eventgrid_subscription_list` | ❌ |
| 13 | 0.204870 | `azmcp_appconfig_account_list` | ❌ |
| 14 | 0.204011 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.203695 | `azmcp_eventgrid_topic_list` | ❌ |
| 16 | 0.202641 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.202430 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.201780 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 19 | 0.187594 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.185423 | `azmcp_subscription_list` | ❌ |

---

## Test 208

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.635406 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.586936 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.531727 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490235 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.447777 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.438801 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.379177 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 9 | 0.354178 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.353388 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.351664 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.322702 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.312391 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.312077 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.292039 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.290398 | `azmcp_search_service_list` | ❌ |
| 17 | 0.282195 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.276297 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.273557 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.273060 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 209

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.548542 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.541062 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.516852 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.516443 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.424443 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.424017 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.409787 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.392192 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369170 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.356143 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.342487 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.306627 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.305636 | `azmcp_sql_db_update` | ❌ |
| 15 | 0.304620 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.304195 | `azmcp_search_service_list` | ❌ |
| 17 | 0.302423 | `azmcp_mysql_server_config_get` | ❌ |
| 18 | 0.302015 | `azmcp_sql_server_show` | ❌ |
| 19 | 0.296142 | `azmcp_sql_db_create` | ❌ |
| 20 | 0.291123 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 210

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.594323 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.518671 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.465572 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.454158 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.430552 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.399433 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.392733 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.383995 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.380315 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.375863 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.362669 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.335296 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.331683 | `azmcp_storage_blob_get` | ❌ |
| 15 | 0.329342 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.322718 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.322410 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.317765 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.316805 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 20 | 0.314841 | `azmcp_search_service_list` | ❌ |

---

## Test 211

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.570488 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.523002 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.445321 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.400447 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.381822 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.368217 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.367714 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.353416 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 11 | 0.317494 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.292977 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.284617 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.278941 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.275278 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.256382 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.252529 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.241745 | `azmcp_search_index_query` | ❌ |
| 19 | 0.239985 | `azmcp_storage_blob_get` | ❌ |
| 20 | 0.239436 | `azmcp_search_service_list` | ❌ |

---

## Test 212

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581850 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.497350 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495618 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.486886 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.474511 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.439182 | `azmcp_foundry_models_deployments_list` | ❌ |
| 7 | 0.412037 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.399571 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.377790 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.373497 | `azmcp_cloudarchitect_design` | ❌ |
| 11 | 0.323091 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.317931 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.303572 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290695 | `azmcp_mysql_server_config_get` | ❌ |
| 15 | 0.277946 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.276161 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.275785 | `azmcp_sql_db_update` | ❌ |
| 18 | 0.270375 | `azmcp_search_service_list` | ❌ |
| 19 | 0.269415 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.269109 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 213

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.532790 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.487333 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.458060 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.413150 | `azmcp_functionapp_get` | ❌ |
| 6 | 0.395991 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.394762 | `azmcp_cloudarchitect_design` | ❌ |
| 8 | 0.394214 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.375665 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.363596 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.332532 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 12 | 0.332015 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.307885 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.290929 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.289428 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.289326 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.284975 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.284215 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.275538 | `azmcp_search_index_query` | ❌ |
| 20 | 0.275498 | `azmcp_storage_blob_get` | ❌ |

---

## Test 214

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.513262 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.505138 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.405163 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.401209 | `azmcp_deploy_plan_get` | ❌ |
| 7 | 0.398226 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.389556 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.334566 | `azmcp_applens_resource_diagnose` | ❌ |
| 10 | 0.315539 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.312250 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.292282 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.283198 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.275578 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.258767 | `azmcp_search_index_query` | ❌ |
| 16 | 0.256800 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.256751 | `azmcp_search_service_list` | ❌ |
| 18 | 0.255215 | `azmcp_storage_blob_get` | ❌ |
| 19 | 0.253386 | `azmcp_sql_db_update` | ❌ |
| 20 | 0.251300 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 215

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `azmcp_get_bestpractices_get` | ❌ |
| 2 | 0.500368 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.472115 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.433134 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.385965 | `azmcp_cloudarchitect_design` | ❌ |
| 6 | 0.381179 | `azmcp_functionapp_get` | ❌ |
| 7 | 0.374583 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.368831 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.358748 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.337024 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.293848 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.288873 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.259723 | `azmcp_mysql_database_query` | ❌ |
| 14 | 0.253005 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.251196 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 16 | 0.249981 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.246347 | `azmcp_workbooks_delete` | ❌ |
| 18 | 0.240146 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.231234 | `azmcp_search_index_query` | ❌ |
| 20 | 0.231120 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 216

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a simple HTTP-triggered function app in javascript that returns a random compliment from a predefined list in a JSON response. And deploy it to azure eventually. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429159 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.408233 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.380754 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.377184 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.352316 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.345059 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.319863 | `azmcp_loadtesting_test_create` | ❌ |
| 8 | 0.311848 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 9 | 0.301028 | `azmcp_functionapp_get` | ❌ |
| 10 | 0.299203 | `azmcp_deploy_app_logs_get` | ❌ |
| 11 | 0.235579 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.232320 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.218912 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.215940 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.210908 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.206254 | `azmcp_sql_db_create` | ❌ |
| 17 | 0.203401 | `azmcp_search_index_query` | ❌ |
| 18 | 0.202251 | `azmcp_storage_account_create` | ❌ |
| 19 | 0.197959 | `azmcp_mysql_database_query` | ❌ |
| 20 | 0.186514 | `azmcp_sql_server_create` | ❌ |

---

## Test 217

**Expected Tool:** `azmcp_bestpractices_get`  
**Prompt:** Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497276 | `azmcp_deploy_plan_get` | ❌ |
| 2 | 0.493182 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.405146 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.395579 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.385140 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.374154 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.354448 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 8 | 0.348236 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.300125 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.284049 | `azmcp_storage_blob_container_create` | ❌ |
| 11 | 0.266937 | `azmcp_foundry_models_deploy` | ❌ |
| 12 | 0.248999 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.243575 | `azmcp_quota_usage_check` | ❌ |
| 14 | 0.234797 | `azmcp_storage_account_create` | ❌ |
| 15 | 0.221235 | `azmcp_storage_blob_container_get` | ❌ |
| 16 | 0.218621 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.210666 | `azmcp_storage_blob_upload` | ❌ |
| 18 | 0.209213 | `azmcp_workbooks_create` | ❌ |
| 19 | 0.208812 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.195544 | `azmcp_sql_server_create` | ❌ |

---

## Test 218

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498365 | `azmcp_monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.472119 | `azmcp_monitor_workspace_list` | ❌ |
| 3 | 0.468185 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.467958 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.463000 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.436981 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.418724 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.413466 | `azmcp_monitor_table_type_list` | ❌ |
| 9 | 0.401591 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.385793 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.379945 | `azmcp_grafana_list` | ❌ |
| 12 | 0.358961 | `azmcp_monitor_metrics_query` | ❌ |
| 13 | 0.342882 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.339427 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.333436 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.314436 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.305757 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.303095 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.297852 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.296680 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 219

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592579 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424070 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.332356 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.315519 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.315190 | `azmcp_servicebus_topic_details` | ❌ |
| 6 | 0.311108 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 7 | 0.305464 | `azmcp_servicebus_queue_details` | ❌ |
| 8 | 0.304735 | `azmcp_grafana_list` | ❌ |
| 9 | 0.303453 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.298800 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.294124 | `azmcp_quota_region_availability_list` | ❌ |
| 12 | 0.287300 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.284519 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.277566 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.274784 | `azmcp_loadtesting_test_get` | ❌ |
| 16 | 0.262141 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.256836 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 18 | 0.254848 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.249144 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.248987 | `azmcp_bicepschema_get` | ❌ |

---

## Test 220

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589859 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.587659 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.550581 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.473421 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.472264 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.459829 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.439032 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.437739 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.431109 | `azmcp_appconfig_kv_show` | ❌ |
| 10 | 0.417098 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.414488 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.403921 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.401894 | `azmcp_monitor_metrics_query` | ❌ |
| 14 | 0.397526 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.391340 | `azmcp_monitor_table_type_list` | ❌ |
| 16 | 0.390422 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.383443 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 18 | 0.371164 | `azmcp_foundry_agents_list` | ❌ |
| 19 | 0.359476 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.357647 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 221

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633072 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495455 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.382534 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.380436 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.370848 | `azmcp_monitor_table_type_list` | ❌ |
| 6 | 0.359089 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.353235 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.344326 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.341713 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.337874 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.329494 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.326682 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.324002 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.322121 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 15 | 0.317475 | `azmcp_monitor_workspace_log_query` | ❌ |
| 16 | 0.302823 | `azmcp_monitor_table_list` | ❌ |
| 17 | 0.301575 | `azmcp_workbooks_show` | ❌ |
| 18 | 0.291565 | `azmcp_cloudarchitect_design` | ❌ |
| 19 | 0.291249 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.287764 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 222

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555527 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.447607 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.447294 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.433812 | `azmcp_loadtesting_testrun_get` | ❌ |
| 5 | 0.422333 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.416100 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.413282 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.409134 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.388205 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.380075 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.356549 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.350085 | `azmcp_loadtesting_testrun_list` | ❌ |
| 13 | 0.341791 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.339718 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.335318 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.329267 | `azmcp_loadtesting_testresource_create` | ❌ |
| 17 | 0.327339 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.326790 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.326398 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.320852 | `azmcp_search_index_query` | ❌ |

---

## Test 223

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557872 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.508669 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.460611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.438156 | `azmcp_monitor_metrics_definitions` | ❌ |
| 6 | 0.392094 | `azmcp_monitor_resource_log_query` | ❌ |
| 7 | 0.391809 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.372992 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.368589 | `azmcp_monitor_workspace_log_query` | ❌ |
| 10 | 0.354733 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 11 | 0.339388 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.336638 | `azmcp_loadtesting_testrun_get` | ❌ |
| 13 | 0.326836 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.326643 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.321538 | `azmcp_search_service_list` | ❌ |
| 16 | 0.321225 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.318196 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.317565 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.303931 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.303909 | `azmcp_quota_region_availability_list` | ❌ |

---

## Test 224

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461550 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.389442 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.306305 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.303501 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.302222 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.289565 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.275428 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.267387 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 9 | 0.267293 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 10 | 0.265722 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.263285 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.263235 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.258647 | `azmcp_grafana_list` | ❌ |
| 14 | 0.252867 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.249280 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.248358 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.247650 | `azmcp_applens_resource_diagnose` | ❌ |
| 18 | 0.243171 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.235115 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.229879 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 225

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.491784 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.416945 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.415966 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.406284 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.399007 | `azmcp_deploy_app_logs_get` | ❌ |
| 6 | 0.397335 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.369668 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.366959 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.362042 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.359340 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.332322 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.316281 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.315326 | `azmcp_functionapp_get` | ❌ |
| 14 | 0.311842 | `azmcp_search_index_query` | ❌ |
| 15 | 0.308672 | `azmcp_monitor_metrics_definitions` | ❌ |
| 16 | 0.295918 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.293608 | `azmcp_search_service_list` | ❌ |
| 18 | 0.293120 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.289035 | `azmcp_foundry_agents_connect` | ❌ |
| 20 | 0.287126 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 226

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525352 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.384442 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.376658 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.367167 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.299448 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.292929 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 7 | 0.290172 | `azmcp_loadtesting_testrun_get` | ❌ |
| 8 | 0.277697 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 9 | 0.272349 | `azmcp_monitor_table_type_list` | ❌ |
| 10 | 0.267076 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 11 | 0.266376 | `azmcp_mysql_server_param_get` | ❌ |
| 12 | 0.265620 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.262699 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.261986 | `azmcp_grafana_list` | ❌ |
| 15 | 0.261656 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.248226 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 17 | 0.246502 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.244147 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.242689 | `azmcp_loadtesting_test_get` | ❌ |
| 20 | 0.239400 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 227

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480388 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.381879 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.363412 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.359401 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.350523 | `azmcp_monitor_resource_log_query` | ❌ |
| 6 | 0.350491 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.346343 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 8 | 0.331139 | `azmcp_loadtesting_testresource_list` | ❌ |
| 9 | 0.330074 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.328731 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.324932 | `azmcp_search_index_query` | ❌ |
| 12 | 0.319106 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.317502 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.292188 | `azmcp_deploy_app_logs_get` | ❌ |
| 15 | 0.290762 | `azmcp_search_service_list` | ❌ |
| 16 | 0.284265 | `azmcp_foundry_agents_connect` | ❌ |
| 17 | 0.282267 | `azmcp_functionapp_get` | ❌ |
| 18 | 0.278327 | `azmcp_workbooks_show` | ❌ |
| 19 | 0.276999 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.265303 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 228

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593906 | `azmcp_monitor_workspace_log_query` | ❌ |
| 2 | 0.580238 | `azmcp_monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.471775 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.470265 | `azmcp_monitor_metrics_query` | ❌ |
| 5 | 0.443043 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.442582 | `azmcp_monitor_table_list` | ❌ |
| 7 | 0.392123 | `azmcp_monitor_table_type_list` | ❌ |
| 8 | 0.389906 | `azmcp_grafana_list` | ❌ |
| 9 | 0.366149 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.359102 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 11 | 0.352673 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 12 | 0.345359 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.345252 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.337915 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.320705 | `azmcp_loadtesting_testrun_get` | ❌ |
| 16 | 0.313577 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 17 | 0.308865 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.307827 | `azmcp_aks_cluster_get` | ❌ |
| 19 | 0.307196 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.305105 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 229

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851075 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.534829 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.511123 | `azmcp_kusto_table_list` | ❌ |
| 6 | 0.502075 | `azmcp_grafana_list` | ❌ |
| 7 | 0.488557 | `azmcp_postgres_table_list` | ❌ |
| 8 | 0.443812 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.420394 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.419859 | `azmcp_kusto_database_list` | ❌ |
| 11 | 0.413834 | `azmcp_mysql_database_list` | ❌ |
| 12 | 0.409199 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.400092 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.397408 | `azmcp_kusto_table_schema` | ❌ |
| 15 | 0.396780 | `azmcp_search_service_list` | ❌ |
| 16 | 0.377057 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.375149 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.374930 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.366099 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.365781 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 230

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798460 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.599917 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.497065 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.487237 | `azmcp_grafana_list` | ❌ |
| 6 | 0.466630 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.449407 | `azmcp_monitor_workspace_log_query` | ❌ |
| 8 | 0.427408 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.413678 | `azmcp_monitor_resource_log_query` | ❌ |
| 10 | 0.411590 | `azmcp_kusto_table_schema` | ❌ |
| 11 | 0.403836 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.398753 | `azmcp_mysql_table_schema_get` | ❌ |
| 13 | 0.389881 | `azmcp_mysql_database_list` | ❌ |
| 14 | 0.376474 | `azmcp_kusto_sample` | ❌ |
| 15 | 0.376338 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.373298 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.370624 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.347853 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.346183 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.343837 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 231

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765702 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.569921 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.504683 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.477280 | `azmcp_grafana_list` | ❌ |
| 6 | 0.447435 | `azmcp_kusto_table_list` | ❌ |
| 7 | 0.445347 | `azmcp_mysql_table_schema_get` | ❌ |
| 8 | 0.418517 | `azmcp_postgres_table_list` | ❌ |
| 9 | 0.416351 | `azmcp_kusto_table_schema` | ❌ |
| 10 | 0.412293 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.404852 | `azmcp_monitor_workspace_log_query` | ❌ |
| 12 | 0.404113 | `azmcp_monitor_metrics_definitions` | ❌ |
| 13 | 0.383606 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.380581 | `azmcp_kusto_sample` | ❌ |
| 15 | 0.374197 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.372490 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.369889 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.361820 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.354757 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.351333 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 232

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843138 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736837 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.576731 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.481189 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.475734 | `azmcp_grafana_list` | ❌ |
| 6 | 0.451212 | `azmcp_mysql_table_schema_get` | ❌ |
| 7 | 0.427934 | `azmcp_kusto_table_schema` | ❌ |
| 8 | 0.427153 | `azmcp_monitor_workspace_log_query` | ❌ |
| 9 | 0.421484 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.406242 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.391308 | `azmcp_kusto_sample` | ❌ |
| 12 | 0.384679 | `azmcp_monitor_resource_log_query` | ❌ |
| 13 | 0.376171 | `azmcp_monitor_metrics_definitions` | ❌ |
| 14 | 0.372991 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.370860 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.367568 | `azmcp_deploy_app_logs_get` | ❌ |
| 17 | 0.355178 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.348357 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.340101 | `azmcp_foundry_models_list` | ❌ |
| 20 | 0.339804 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 233

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
| 8 | 0.513679 | `azmcp_aks_cluster_list` | ❌ |
| 9 | 0.500768 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.494683 | `azmcp_group_list` | ❌ |
| 11 | 0.493730 | `azmcp_subscription_list` | ❌ |
| 12 | 0.475212 | `azmcp_monitor_workspace_log_query` | ❌ |
| 13 | 0.471758 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.470266 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.467655 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.466729 | `azmcp_acr_registry_list` | ❌ |
| 17 | 0.464047 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.460481 | `azmcp_redis_cache_list` | ❌ |
| 19 | 0.448201 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.444211 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 234

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
| 6 | 0.459793 | `azmcp_deploy_app_logs_get` | ❌ |
| 7 | 0.444207 | `azmcp_search_service_list` | ❌ |
| 8 | 0.414153 | `azmcp_foundry_agents_list` | ❌ |
| 9 | 0.386422 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.383601 | `azmcp_aks_cluster_list` | ❌ |
| 11 | 0.380891 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.373786 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.371395 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.363287 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.358029 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.354811 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.354276 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.353651 | `azmcp_subscription_list` | ❌ |
| 19 | 0.352756 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.351453 | `azmcp_search_index_get` | ❌ |

---

## Test 235

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
| 6 | 0.463378 | `azmcp_monitor_workspace_log_query` | ❌ |
| 7 | 0.453659 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.439297 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.435071 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.428945 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.427226 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.422707 | `azmcp_subscription_list` | ❌ |
| 13 | 0.422356 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.411630 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.411448 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 16 | 0.410082 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.409827 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.404262 | `azmcp_group_list` | ❌ |
| 19 | 0.402600 | `azmcp_redis_cluster_list` | ❌ |
| 20 | 0.400615 | `azmcp_postgres_server_list` | ❌ |

---

## Test 236

**Expected Tool:** `azmcp_monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591648 | `azmcp_monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.494715 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.485984 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.484088 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.483323 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.427241 | `azmcp_monitor_table_type_list` | ❌ |
| 7 | 0.375438 | `azmcp_monitor_metrics_query` | ❌ |
| 8 | 0.365704 | `azmcp_grafana_list` | ❌ |
| 9 | 0.330307 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.322875 | `azmcp_workbooks_delete` | ❌ |
| 11 | 0.322324 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 12 | 0.315638 | `azmcp_search_service_list` | ❌ |
| 13 | 0.309477 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.299851 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.292089 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.291669 | `azmcp_kusto_query` | ❌ |
| 17 | 0.289781 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.288625 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.287253 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.283294 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 237

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668827 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.434836 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.413232 | `azmcp_monitor_metrics_query` | ❌ |
| 4 | 0.408658 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.401731 | `azmcp_grafana_list` | ❌ |
| 6 | 0.393318 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.386680 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.369805 | `azmcp_redis_cluster_database_list` | ❌ |
| 9 | 0.364076 | `azmcp_workbooks_list` | ❌ |
| 10 | 0.356643 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.355391 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.345409 | `azmcp_postgres_database_list` | ❌ |
| 13 | 0.345382 | `azmcp_group_list` | ❌ |
| 14 | 0.330769 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.328960 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.327205 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.306977 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.304097 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.302405 | `azmcp_acr_registry_repository_list` | ❌ |
| 20 | 0.296544 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 238

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443840 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.393260 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.374071 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.371017 | `azmcp_grafana_list` | ❌ |
| 6 | 0.370681 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 7 | 0.359262 | `azmcp_monitor_metrics_definitions` | ❌ |
| 8 | 0.350656 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.343181 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.342468 | `azmcp_redis_cluster_database_list` | ❌ |
| 11 | 0.337109 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.320462 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.319895 | `azmcp_workbooks_list` | ❌ |
| 14 | 0.302947 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.289883 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.287390 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.285326 | `azmcp_group_list` | ❌ |
| 18 | 0.274836 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.274575 | `azmcp_deploy_app_logs_get` | ❌ |
| 20 | 0.272689 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `azmcp_quota_usage_check` | ❌ |
| 2 | 0.497413 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.481143 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.476826 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.462075 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 6 | 0.451690 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.440399 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.438387 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.434685 | `azmcp_search_service_list` | ❌ |
| 10 | 0.431119 | `azmcp_deploy_iac_rules_get` | ❌ |
| 11 | 0.423237 | `azmcp_subscription_list` | ❌ |
| 12 | 0.422218 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 13 | 0.416798 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.408023 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.406591 | `azmcp_deploy_plan_get` | ❌ |
| 16 | 0.400363 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.395234 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.391633 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.388980 | `azmcp_monitor_workspace_list` | ❌ |
| 20 | 0.381209 | `azmcp_storage_account_get` | ❌ |

---

## Test 240

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.488377 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.476164 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 5 | 0.473411 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.462743 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 7 | 0.452244 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.448085 | `azmcp_deploy_plan_get` | ❌ |
| 9 | 0.442021 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.439040 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.426161 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 12 | 0.385771 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.382677 | `azmcp_search_service_list` | ❌ |
| 14 | 0.375770 | `azmcp_subscription_list` | ❌ |
| 15 | 0.375071 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.365824 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 17 | 0.365699 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.360510 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.349469 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.341827 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 241

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.514978 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 4 | 0.504673 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.494872 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.487387 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.481713 | `azmcp_applens_resource_diagnose` | ❌ |
| 8 | 0.464304 | `azmcp_cloudarchitect_design` | ❌ |
| 9 | 0.463587 | `azmcp_deploy_iac_rules_get` | ❌ |
| 10 | 0.463172 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.452811 | `azmcp_search_service_list` | ❌ |
| 12 | 0.433938 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.423512 | `azmcp_subscription_list` | ❌ |
| 14 | 0.417356 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 15 | 0.403533 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.398621 | `azmcp_monitor_workspace_list` | ❌ |
| 17 | 0.380268 | `azmcp_storage_account_get` | ❌ |
| 18 | 0.377353 | `azmcp_sql_server_list` | ❌ |
| 19 | 0.376533 | `azmcp_marketplace_product_get` | ❌ |
| 20 | 0.376044 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 242

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
| 8 | 0.337835 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.331145 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.331097 | `azmcp_monitor_metrics_definitions` | ❌ |
| 11 | 0.328408 | `azmcp_grafana_list` | ❌ |
| 12 | 0.325796 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.313178 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.310655 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.307143 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.297286 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.292791 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.290187 | `azmcp_group_list` | ❌ |
| 19 | 0.287074 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.263276 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 243

**Expected Tool:** `azmcp_quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609244 | `azmcp_quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `azmcp_quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383920 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.378998 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.365684 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.358215 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.351637 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.345161 | `azmcp_eventgrid_subscription_list` | ❌ |
| 10 | 0.345156 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.342400 | `azmcp_applens_resource_diagnose` | ❌ |
| 12 | 0.342231 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.338636 | `azmcp_grafana_list` | ❌ |
| 14 | 0.331263 | `azmcp_monitor_metrics_definitions` | ❌ |
| 15 | 0.322571 | `azmcp_workbooks_list` | ❌ |
| 16 | 0.321805 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.305083 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.304595 | `azmcp_loadtesting_testrun_get` | ❌ |
| 19 | 0.300710 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.297650 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 244

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.484094 | `azmcp_group_list` | ❌ |
| 3 | 0.483125 | `azmcp_subscription_list` | ❌ |
| 4 | 0.478700 | `azmcp_grafana_list` | ❌ |
| 5 | 0.474775 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.471364 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.468596 | `azmcp_search_service_list` | ❌ |
| 8 | 0.460029 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.452819 | `azmcp_monitor_workspace_list` | ❌ |
| 10 | 0.446372 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 11 | 0.430667 | `azmcp_kusto_cluster_list` | ❌ |
| 12 | 0.427666 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.426629 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 14 | 0.425029 | `azmcp_postgres_server_list` | ❌ |
| 15 | 0.421599 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.409648 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.403310 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.398447 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.397565 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.397003 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 245

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609705 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.456956 | `azmcp_grafana_list` | ❌ |
| 3 | 0.436747 | `azmcp_subscription_list` | ❌ |
| 4 | 0.435629 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.435155 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.431865 | `azmcp_search_service_list` | ❌ |
| 7 | 0.428756 | `azmcp_group_list` | ❌ |
| 8 | 0.428370 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.421637 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.420804 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.415941 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.410380 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 13 | 0.406766 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.395445 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.390162 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.386800 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.383635 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.373204 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.368493 | `azmcp_loadtesting_testresource_list` | ❌ |
| 20 | 0.363678 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 246

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757057 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.565115 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.445073 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.377563 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.322930 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.312428 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.303531 | `azmcp_appconfig_kv_list` | ❌ |
| 8 | 0.300024 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.292315 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.286490 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.285062 | `azmcp_search_service_list` | ❌ |
| 12 | 0.284898 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.284304 | `azmcp_grafana_list` | ❌ |
| 14 | 0.283818 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.281989 | `azmcp_keyvault_secret_list` | ❌ |
| 16 | 0.280741 | `azmcp_loadtesting_testrun_list` | ❌ |
| 17 | 0.279800 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.277696 | `azmcp_subscription_list` | ❌ |
| 19 | 0.274897 | `azmcp_role_assignment_list` | ❌ |
| 20 | 0.272918 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 247

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713839 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.523218 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.412377 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.338859 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.286321 | `azmcp_appconfig_kv_list` | ❌ |
| 6 | 0.283725 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.280245 | `azmcp_appconfig_kv_show` | ❌ |
| 8 | 0.265313 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.264484 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.262084 | `azmcp_storage_account_get` | ❌ |
| 11 | 0.258045 | `azmcp_appconfig_account_list` | ❌ |
| 12 | 0.257957 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.257447 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.257151 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.249585 | `azmcp_loadtesting_testrun_list` | ❌ |
| 16 | 0.246871 | `azmcp_grafana_list` | ❌ |
| 17 | 0.246847 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.246678 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.243208 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.240600 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 248

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.764115 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
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
| 15 | 0.380531 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.373546 | `azmcp_group_list` | ❌ |
| 17 | 0.368719 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.367794 | `azmcp_mysql_database_list` | ❌ |
| 19 | 0.367496 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.364522 | `azmcp_virtualdesktop_hostpool_list` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537994 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.450409 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 3 | 0.441096 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.401251 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.302289 | `azmcp_mysql_database_list` | ❌ |
| 6 | 0.283596 | `azmcp_postgres_database_list` | ❌ |
| 7 | 0.275942 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.265872 | `azmcp_appconfig_kv_list` | ❌ |
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

## Test 250

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692266 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.595721 | `azmcp_redis_cluster_list` | ❌ |
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
| 13 | 0.340764 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.327160 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.315634 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.310802 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.308807 | `azmcp_eventgrid_topic_list` | ❌ |
| 18 | 0.306367 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.305932 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.300248 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 251

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752964 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.603862 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.594977 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.548166 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.538309 | `azmcp_cosmos_database_list` | ❌ |
| 6 | 0.520799 | `azmcp_mysql_database_list` | ❌ |
| 7 | 0.471425 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.458239 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.456099 | `azmcp_kusto_table_list` | ❌ |
| 10 | 0.449494 | `azmcp_sql_db_list` | ❌ |
| 11 | 0.419526 | `azmcp_postgres_table_list` | ❌ |
| 12 | 0.395373 | `azmcp_mysql_server_list` | ❌ |
| 13 | 0.390361 | `azmcp_mysql_table_list` | ❌ |
| 14 | 0.385479 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.379918 | `azmcp_postgres_server_list` | ❌ |
| 16 | 0.376260 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.366166 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.328430 | `azmcp_aks_nodepool_list` | ❌ |
| 19 | 0.328052 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.324876 | `azmcp_grafana_list` | ❌ |

---

## Test 252

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
| 7 | 0.434991 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.414701 | `azmcp_kusto_table_list` | ❌ |
| 9 | 0.408379 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.397285 | `azmcp_kusto_cluster_list` | ❌ |
| 11 | 0.369086 | `azmcp_mysql_server_list` | ❌ |
| 12 | 0.353712 | `azmcp_mysql_table_list` | ❌ |
| 13 | 0.351025 | `azmcp_cosmos_database_container_list` | ❌ |
| 14 | 0.349880 | `azmcp_postgres_table_list` | ❌ |
| 15 | 0.343275 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 16 | 0.325405 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.318982 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.302228 | `azmcp_kusto_sample` | ❌ |
| 19 | 0.294393 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.292180 | `azmcp_grafana_list` | ❌ |

---

## Test 253

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812960 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.679028 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.672139 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.588847 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.569296 | `azmcp_aks_cluster_list` | ❌ |
| 6 | 0.554298 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.527406 | `azmcp_kusto_database_list` | ❌ |
| 8 | 0.503279 | `azmcp_grafana_list` | ❌ |
| 9 | 0.467957 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.462558 | `azmcp_search_service_list` | ❌ |
| 11 | 0.457600 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.455613 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.445628 | `azmcp_group_list` | ❌ |
| 14 | 0.445406 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.443534 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.442886 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 17 | 0.436494 | `azmcp_subscription_list` | ❌ |
| 18 | 0.435221 | `azmcp_eventgrid_subscription_list` | ❌ |
| 19 | 0.419126 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.411121 | `azmcp_mysql_server_list` | ❌ |

---

## Test 254

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591593 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.514375 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.467592 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.403281 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.385069 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 6 | 0.368025 | `azmcp_aks_cluster_list` | ❌ |
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
| 20 | 0.252050 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 255

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me the Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.744239 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.607561 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.580866 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.518857 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.494170 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.491315 | `azmcp_aks_cluster_list` | ❌ |
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

## Test 256

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
| 7 | 0.530516 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.524796 | `azmcp_kusto_cluster_list` | ❌ |
| 9 | 0.519242 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.518475 | `azmcp_acr_registry_list` | ❌ |
| 11 | 0.517031 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.509454 | `azmcp_search_service_list` | ❌ |
| 13 | 0.500858 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.491176 | `azmcp_acr_registry_repository_list` | ❌ |
| 15 | 0.490734 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.486716 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.483567 | `azmcp_eventgrid_subscription_list` | ❌ |
| 18 | 0.479545 | `azmcp_subscription_list` | ❌ |
| 19 | 0.477800 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.477131 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 257

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
| 6 | 0.428894 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.426935 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.407817 | `azmcp_grafana_list` | ❌ |
| 9 | 0.398432 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.396822 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.391250 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.382985 | `azmcp_acr_registry_list` | ❌ |
| 13 | 0.379927 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.375998 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.373796 | `azmcp_quota_region_availability_list` | ❌ |
| 16 | 0.366273 | `azmcp_sql_db_list` | ❌ |
| 17 | 0.351405 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.350999 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.340946 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.328913 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 258

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665784 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.531964 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.523088 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.522911 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.518498 | `azmcp_loadtesting_testresource_list` | ❌ |
| 7 | 0.515905 | `azmcp_grafana_list` | ❌ |
| 8 | 0.494579 | `azmcp_eventgrid_subscription_list` | ❌ |
| 9 | 0.492895 | `azmcp_redis_cache_list` | ❌ |
| 10 | 0.489079 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.487740 | `azmcp_acr_registry_list` | ❌ |
| 12 | 0.475708 | `azmcp_search_service_list` | ❌ |
| 13 | 0.470658 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.464637 | `azmcp_quota_region_availability_list` | ❌ |
| 15 | 0.460412 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.454711 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.454439 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.437460 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.432994 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.429798 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 259

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630680 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377586 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.349980 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.331474 | `azmcp_monitor_metrics_definitions` | ❌ |
| 6 | 0.330187 | `azmcp_mysql_server_list` | ❌ |
| 7 | 0.327660 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.325794 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.324331 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.322117 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.311602 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.308238 | `azmcp_redis_cluster_list` | ❌ |
| 13 | 0.306616 | `azmcp_grafana_list` | ❌ |
| 14 | 0.292084 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.290772 | `azmcp_workbooks_show` | ❌ |
| 16 | 0.286287 | `azmcp_loadtesting_test_get` | ❌ |
| 17 | 0.285047 | `azmcp_applens_resource_diagnose` | ❌ |
| 18 | 0.284986 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.272387 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.272288 | `azmcp_group_list` | ❌ |

---

## Test 260

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549306 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.510467 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.490071 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 4 | 0.466885 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.455902 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.413034 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.411283 | `azmcp_quota_usage_check` | ❌ |
| 8 | 0.405847 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.403899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.375351 | `azmcp_cosmos_database_container_list` | ❌ |
| 11 | 0.368262 | `azmcp_appconfig_kv_show` | ❌ |
| 12 | 0.349407 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.348006 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.347171 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.346145 | `azmcp_storage_blob_container_create` | ❌ |
| 16 | 0.336357 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.321694 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.318472 | `azmcp_aks_nodepool_get` | ❌ |
| 19 | 0.311399 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.300500 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 261

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 2 | 0.570931 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424939 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.393479 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.386598 | `azmcp_quota_usage_check` | ❌ |
| 6 | 0.373883 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.355414 | `azmcp_functionapp_get` | ❌ |
| 8 | 0.352447 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.342229 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.338012 | `azmcp_sql_server_list` | ❌ |
| 11 | 0.337593 | `azmcp_aks_nodepool_get` | ❌ |
| 12 | 0.335759 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.327197 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.321350 | `azmcp_group_list` | ❌ |
| 15 | 0.318379 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.318319 | `azmcp_workbooks_list` | ❌ |
| 17 | 0.316508 | `azmcp_sql_server_show` | ❌ |
| 18 | 0.307309 | `azmcp_applens_resource_diagnose` | ❌ |
| 19 | 0.294197 | `azmcp_aks_cluster_get` | ❌ |
| 20 | 0.289102 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 262

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.587331 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.578609 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.563455 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.548549 | `azmcp_grafana_list` | ❌ |
| 6 | 0.540583 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 7 | 0.534299 | `azmcp_search_service_list` | ❌ |
| 8 | 0.531356 | `azmcp_quota_region_availability_list` | ❌ |
| 9 | 0.531121 | `azmcp_group_list` | ❌ |
| 10 | 0.507740 | `azmcp_monitor_workspace_list` | ❌ |
| 11 | 0.496651 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.491394 | `azmcp_quota_usage_check` | ❌ |
| 13 | 0.491359 | `azmcp_subscription_list` | ❌ |
| 14 | 0.489514 | `azmcp_eventgrid_subscription_list` | ❌ |
| 15 | 0.484188 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.482623 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.476832 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.465503 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.462565 | `azmcp_eventgrid_topic_list` | ❌ |
| 20 | 0.459718 | `azmcp_workbooks_list` | ❌ |

---

## Test 263

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.586936 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.508252 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.473905 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.462125 | `azmcp_search_service_list` | ❌ |
| 6 | 0.456449 | `azmcp_foundry_agents_list` | ❌ |
| 7 | 0.441470 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.441417 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.430687 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.418944 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.409377 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.406784 | `azmcp_storage_blob_container_get` | ❌ |
| 13 | 0.406709 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.406408 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.405790 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.403414 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.387835 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.381144 | `azmcp_get_bestpractices_get` | ❌ |
| 19 | 0.379969 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 20 | 0.371766 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 264

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.543368 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.427638 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.420682 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.420387 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.411111 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.410799 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.374184 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.370858 | `azmcp_loadtesting_testresource_list` | ❌ |
| 10 | 0.363808 | `azmcp_workbooks_list` | ❌ |
| 11 | 0.360039 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.358871 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.354932 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.350520 | `azmcp_group_list` | ❌ |
| 15 | 0.349212 | `azmcp_monitor_metrics_query` | ❌ |
| 16 | 0.338595 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.330185 | `azmcp_extension_azqr` | ❌ |
| 18 | 0.328471 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 19 | 0.324217 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.309389 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 265

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.719763 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `azmcp_search_service_list` | ❌ |
| 3 | 0.531311 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.518372 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.503744 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.470139 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.456562 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.454448 | `azmcp_redis_cluster_list` | ❌ |
| 9 | 0.446475 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 10 | 0.438780 | `azmcp_subscription_list` | ❌ |
| 11 | 0.427177 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.426698 | `azmcp_grafana_list` | ❌ |
| 13 | 0.419828 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.419011 | `azmcp_kusto_cluster_list` | ❌ |
| 15 | 0.416883 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.412013 | `azmcp_group_list` | ❌ |
| 17 | 0.407099 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.385382 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.378841 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.377914 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 266

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726700 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.513815 | `azmcp_search_service_list` | ❌ |
| 3 | 0.509196 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.491121 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484366 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.474835 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.459791 | `azmcp_subscription_list` | ❌ |
| 8 | 0.431455 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.425644 | `azmcp_quota_region_availability_list` | ❌ |
| 10 | 0.411892 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.410579 | `azmcp_marketplace_product_list` | ❌ |
| 12 | 0.409037 | `azmcp_aks_cluster_list` | ❌ |
| 13 | 0.404636 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.390652 | `azmcp_kusto_cluster_get` | ❌ |
| 15 | 0.390558 | `azmcp_group_list` | ❌ |
| 16 | 0.390381 | `azmcp_applens_resource_diagnose` | ❌ |
| 17 | 0.389256 | `azmcp_keyvault_key_get` | ❌ |
| 18 | 0.385710 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.384857 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.384613 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 267

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.302331 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.270225 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.251821 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.216847 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211842 | `azmcp_search_service_list` | ❌ |
| 6 | 0.191890 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.189628 | `azmcp_foundry_models_deployments_list` | ❌ |
| 8 | 0.188665 | `azmcp_get_bestpractices_get` | ❌ |
| 9 | 0.187819 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.185941 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.174877 | `azmcp_deploy_app_logs_get` | ❌ |
| 12 | 0.170157 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.169947 | `azmcp_servicebus_queue_details` | ❌ |
| 14 | 0.164622 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.164285 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.163022 | `azmcp_monitor_workspace_log_query` | ❌ |
| 17 | 0.155791 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 18 | 0.155483 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.151666 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.149112 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 268

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710990 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.545714 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.520197 | `azmcp_search_service_list` | ❌ |
| 4 | 0.502064 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.487327 | `azmcp_eventgrid_topic_list` | ❌ |
| 6 | 0.453339 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 7 | 0.451351 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.439701 | `azmcp_redis_cache_list` | ❌ |
| 9 | 0.436070 | `azmcp_redis_cluster_list` | ❌ |
| 10 | 0.411793 | `azmcp_grafana_list` | ❌ |
| 11 | 0.408792 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 12 | 0.407707 | `azmcp_subscription_list` | ❌ |
| 13 | 0.406949 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.405031 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.391992 | `azmcp_kusto_cluster_list` | ❌ |
| 16 | 0.379016 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.371380 | `azmcp_group_list` | ❌ |
| 18 | 0.368866 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 19 | 0.358754 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.357139 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 269

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528170 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.437868 | `azmcp_search_service_list` | ❌ |
| 3 | 0.402493 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.402232 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.400114 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 6 | 0.397735 | `azmcp_quota_usage_check` | ❌ |
| 7 | 0.382901 | `azmcp_deploy_plan_get` | ❌ |
| 8 | 0.382597 | `azmcp_deploy_app_logs_get` | ❌ |
| 9 | 0.375034 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.372844 | `azmcp_eventgrid_subscription_list` | ❌ |
| 11 | 0.372005 | `azmcp_monitor_metrics_query` | ❌ |
| 12 | 0.363470 | `azmcp_get_bestpractices_get` | ❌ |
| 13 | 0.362191 | `azmcp_applens_resource_diagnose` | ❌ |
| 14 | 0.360562 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.357531 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.341495 | `azmcp_foundry_models_deployments_list` | ❌ |
| 17 | 0.338062 | `azmcp_search_index_get` | ❌ |
| 18 | 0.335471 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.333382 | `azmcp_sql_server_show` | ❌ |
| 20 | 0.333201 | `azmcp_subscription_list` | ❌ |

---

## Test 270

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `azmcp_servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.400645 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.375386 | `azmcp_aks_cluster_get` | ❌ |
| 5 | 0.359891 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.352705 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.352342 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.351081 | `azmcp_search_index_get` | ❌ |
| 9 | 0.344531 | `azmcp_eventgrid_subscription_list` | ❌ |
| 10 | 0.342349 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.337239 | `azmcp_sql_db_show` | ❌ |
| 12 | 0.333043 | `azmcp_keyvault_key_get` | ❌ |
| 13 | 0.332560 | `azmcp_loadtesting_testrun_get` | ❌ |
| 14 | 0.331418 | `azmcp_keyvault_secret_get` | ❌ |
| 15 | 0.327611 | `azmcp_aks_nodepool_get` | ❌ |
| 16 | 0.323281 | `azmcp_marketplace_product_get` | ❌ |
| 17 | 0.323046 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.310612 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.309214 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.296299 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 271

**Expected Tool:** `azmcp_servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591324 | `azmcp_servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571861 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.497732 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.494885 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.483976 | `azmcp_servicebus_queue_details` | ❌ |
| 6 | 0.365658 | `azmcp_search_index_get` | ❌ |
| 7 | 0.361354 | `azmcp_aks_cluster_get` | ❌ |
| 8 | 0.352485 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.341304 | `azmcp_loadtesting_testrun_get` | ❌ |
| 10 | 0.340036 | `azmcp_sql_db_show` | ❌ |
| 11 | 0.336913 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.335558 | `azmcp_kusto_cluster_get` | ❌ |
| 13 | 0.333396 | `azmcp_storage_account_get` | ❌ |
| 14 | 0.331645 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.326590 | `azmcp_keyvault_secret_get` | ❌ |
| 16 | 0.325007 | `azmcp_storage_blob_container_get` | ❌ |
| 17 | 0.317423 | `azmcp_aks_cluster_list` | ❌ |
| 18 | 0.314540 | `azmcp_keyvault_key_get` | ❌ |
| 19 | 0.306388 | `azmcp_functionapp_get` | ❌ |
| 20 | 0.297323 | `azmcp_grafana_list` | ❌ |

---

## Test 272

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `azmcp_servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.523134 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.494515 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.456939 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.444604 | `azmcp_marketplace_product_get` | ❌ |
| 6 | 0.443973 | `azmcp_eventgrid_topic_list` | ❌ |
| 7 | 0.429444 | `azmcp_redis_cache_list` | ❌ |
| 8 | 0.426573 | `azmcp_kusto_cluster_get` | ❌ |
| 9 | 0.421009 | `azmcp_sql_db_show` | ❌ |
| 10 | 0.411495 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.409619 | `azmcp_aks_cluster_list` | ❌ |
| 12 | 0.405380 | `azmcp_search_service_list` | ❌ |
| 13 | 0.404739 | `azmcp_redis_cluster_list` | ❌ |
| 14 | 0.398648 | `azmcp_keyvault_secret_get` | ❌ |
| 15 | 0.395789 | `azmcp_storage_account_get` | ❌ |
| 16 | 0.395176 | `azmcp_grafana_list` | ❌ |
| 17 | 0.390372 | `azmcp_keyvault_key_get` | ❌ |
| 18 | 0.382225 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.369986 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.368411 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 273

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470950 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.376833 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.371359 | `azmcp_appservice_database_add` | ❌ |
| 5 | 0.359945 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.357421 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.355614 | `azmcp_postgres_database_list` | ❌ |
| 8 | 0.347128 | `azmcp_mysql_database_list` | ❌ |
| 9 | 0.346831 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.329384 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 11 | 0.327837 | `azmcp_sql_db_delete` | ❌ |
| 12 | 0.301243 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.279490 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.276192 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.254831 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 16 | 0.238999 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.236839 | `azmcp_keyvault_key_create` | ❌ |
| 18 | 0.234649 | `azmcp_keyvault_secret_create` | ❌ |
| 19 | 0.231273 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.210486 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 274

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459711 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.424075 | `azmcp_appservice_database_add` | ❌ |
| 4 | 0.420843 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.396455 | `azmcp_sql_db_update` | ❌ |
| 6 | 0.395495 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.384660 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.371559 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.361051 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.343099 | `azmcp_sql_db_delete` | ❌ |
| 11 | 0.326225 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 12 | 0.324123 | `azmcp_kusto_table_list` | ❌ |
| 13 | 0.321837 | `azmcp_cosmos_database_list` | ❌ |
| 14 | 0.317180 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.301487 | `azmcp_kusto_table_schema` | ❌ |
| 16 | 0.286796 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.280312 | `azmcp_keyvault_key_create` | ❌ |
| 18 | 0.277149 | `azmcp_keyvault_secret_create` | ❌ |
| 19 | 0.276652 | `azmcp_loadtesting_test_create` | ❌ |
| 20 | 0.257394 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 275

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545932 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.494377 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.473975 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.456262 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.455293 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.441044 | `azmcp_appservice_database_add` | ❌ |
| 8 | 0.431068 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.422871 | `azmcp_workbooks_create` | ❌ |
| 10 | 0.413309 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.399254 | `azmcp_loadtesting_testresource_create` | ❌ |
| 12 | 0.392814 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.374962 | `azmcp_sql_elastic-pool_list` | ❌ |
| 14 | 0.365919 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.358566 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.323547 | `azmcp_group_list` | ❌ |
| 17 | 0.322659 | `azmcp_keyvault_key_create` | ❌ |
| 18 | 0.319901 | `azmcp_keyvault_secret_create` | ❌ |
| 19 | 0.319381 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.315134 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 276

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
| 11 | 0.300723 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.286892 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.284794 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.278895 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.271009 | `azmcp_appconfig_kv_delete` | ❌ |
| 16 | 0.253808 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.243201 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.235173 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.211680 | `azmcp_kusto_query` | ❌ |
| 20 | 0.183587 | `azmcp_kusto_sample` | ❌ |

---

## Test 277

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579119 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.500756 | `azmcp_sql_db_show` | ❌ |
| 3 | 0.478729 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.466216 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 5 | 0.437112 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.421365 | `azmcp_sql_db_create` | ❌ |
| 7 | 0.412704 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.392236 | `azmcp_workbooks_delete` | ❌ |
| 9 | 0.390334 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.388400 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.381400 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.380074 | `azmcp_kusto_database_list` | ❌ |
| 13 | 0.370486 | `azmcp_kusto_table_list` | ❌ |
| 14 | 0.368841 | `azmcp_sql_elastic-pool_list` | ❌ |
| 15 | 0.345627 | `azmcp_group_list` | ❌ |
| 16 | 0.334517 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 17 | 0.332517 | `azmcp_acr_registry_repository_list` | ❌ |
| 18 | 0.327408 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.310437 | `azmcp_kusto_table_schema` | ❌ |
| 20 | 0.304849 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 278

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
| 11 | 0.295030 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.294781 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.288554 | `azmcp_kusto_database_list` | ❌ |
| 14 | 0.283955 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.258654 | `azmcp_appconfig_kv_delete` | ❌ |
| 16 | 0.246948 | `azmcp_cosmos_database_container_list` | ❌ |
| 17 | 0.229764 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.213266 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 19 | 0.187992 | `azmcp_kusto_query` | ❌ |
| 20 | 0.171883 | `azmcp_kusto_sample` | ❌ |

---

## Test 279

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
| 16 | 0.380402 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.373774 | `azmcp_foundry_agents_list` | ❌ |
| 18 | 0.371962 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.367404 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.365423 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 280

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
| 13 | 0.385130 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.380428 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.357282 | `azmcp_aks_cluster_list` | ❌ |
| 16 | 0.354581 | `azmcp_aks_nodepool_list` | ❌ |
| 17 | 0.349880 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.347075 | `azmcp_cosmos_database_container_list` | ❌ |
| 19 | 0.342792 | `azmcp_appconfig_kv_list` | ❌ |
| 20 | 0.342284 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 281

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
| 14 | 0.320054 | `azmcp_aks_cluster_get` | ❌ |
| 15 | 0.317622 | `azmcp_appservice_database_add` | ❌ |
| 16 | 0.297839 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.294987 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.281546 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.279942 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 20 | 0.273566 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 282

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
| 15 | 0.299814 | `azmcp_aks_cluster_get` | ❌ |
| 16 | 0.296827 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.291633 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.285843 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.276453 | `azmcp_keyvault_key_get` | ❌ |
| 20 | 0.274305 | `azmcp_appservice_database_add` | ❌ |

---

## Test 283

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565596 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.427621 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.385817 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.384245 | `azmcp_appservice_database_add` | ❌ |
| 6 | 0.371461 | `azmcp_sql_db_list` | ❌ |
| 7 | 0.369822 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.368957 | `azmcp_mysql_server_param_set` | ❌ |
| 9 | 0.344860 | `azmcp_sql_db_delete` | ❌ |
| 10 | 0.334237 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.316890 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.273809 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.270134 | `azmcp_kusto_table_schema` | ❌ |
| 14 | 0.263397 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.250975 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.250753 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 17 | 0.240663 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.230973 | `azmcp_loadtesting_testrun_create` | ❌ |
| 19 | 0.223239 | `azmcp_loadtesting_test_create` | ❌ |
| 20 | 0.223086 | `azmcp_kusto_query` | ❌ |

---

## Test 284

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.401817 | `azmcp_sql_db_list` | ❌ |
| 2 | 0.394770 | `azmcp_sql_db_show` | ❌ |
| 3 | 0.390219 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 4 | 0.386628 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.381889 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.349256 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 7 | 0.342291 | `azmcp_sql_elastic-pool_list` | ❌ |
| 8 | 0.339054 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.333336 | `azmcp_sql_server_show` | ❌ |
| 10 | 0.329770 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.325658 | `azmcp_mysql_server_config_get` | ❌ |
| 12 | 0.304373 | `azmcp_kusto_table_schema` | ❌ |
| 13 | 0.301576 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.296988 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 15 | 0.261164 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.257330 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.238540 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.232099 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.221295 | `azmcp_kusto_query` | ❌ |
| 20 | 0.219365 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 285

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
| 8 | 0.442892 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.441264 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.434570 | `azmcp_postgres_server_list` | ❌ |
| 11 | 0.431174 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.429039 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 13 | 0.394548 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.394337 | `azmcp_kusto_database_list` | ❌ |
| 15 | 0.370652 | `azmcp_cosmos_account_list` | ❌ |
| 16 | 0.363579 | `azmcp_kusto_cluster_list` | ❌ |
| 17 | 0.357347 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.352036 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.351647 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.351045 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 286

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
| 11 | 0.383946 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 12 | 0.378556 | `azmcp_postgres_server_list` | ❌ |
| 13 | 0.341694 | `azmcp_foundry_agents_list` | ❌ |
| 14 | 0.335615 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.333099 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.319788 | `azmcp_aks_cluster_list` | ❌ |
| 17 | 0.317886 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.312419 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.304600 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.304317 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 287

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
| 15 | 0.315606 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.298933 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.292566 | `azmcp_kusto_cluster_list` | ❌ |
| 18 | 0.284157 | `azmcp_kusto_database_list` | ❌ |
| 19 | 0.281680 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.259656 | `azmcp_appservice_database_add` | ❌ |

---

## Test 288

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682403 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563927 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.536905 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.529077 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.482050 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.473614 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.465097 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.452450 | `azmcp_loadtesting_testresource_create` | ❌ |
| 9 | 0.449739 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.449707 | `azmcp_sql_server_show` | ❌ |
| 11 | 0.418801 | `azmcp_sql_elastic-pool_list` | ❌ |
| 12 | 0.352888 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.338888 | `azmcp_keyvault_key_create` | ❌ |
| 14 | 0.335879 | `azmcp_functionapp_get` | ❌ |
| 15 | 0.332475 | `azmcp_extension_azqr` | ❌ |
| 16 | 0.329985 | `azmcp_keyvault_certificate_create` | ❌ |
| 17 | 0.326922 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.323380 | `azmcp_acr_registry_repository_list` | ❌ |
| 19 | 0.320328 | `azmcp_keyvault_secret_create` | ❌ |
| 20 | 0.319732 | `azmcp_acr_registry_list` | ❌ |

---

## Test 289

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618376 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510169 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.472463 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.434810 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.397805 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.396073 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.368115 | `azmcp_sql_db_show` | ❌ |
| 8 | 0.360875 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.358318 | `azmcp_appservice_database_add` | ❌ |
| 10 | 0.354438 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.349337 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 12 | 0.325467 | `azmcp_keyvault_secret_create` | ❌ |
| 13 | 0.324021 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.319450 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.316979 | `azmcp_loadtesting_test_create` | ❌ |
| 16 | 0.315987 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.314142 | `azmcp_foundry_agents_connect` | ❌ |
| 18 | 0.301134 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.301132 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.298414 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 290

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589802 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.501302 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.497923 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.469342 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.442982 | `azmcp_mysql_server_list` | ❌ |
| 6 | 0.423953 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.421581 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.417595 | `azmcp_sql_db_show` | ❌ |
| 9 | 0.416002 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.415419 | `azmcp_appservice_database_add` | ❌ |
| 11 | 0.389656 | `azmcp_sql_elastic-pool_list` | ❌ |
| 12 | 0.385725 | `azmcp_loadtesting_testresource_create` | ❌ |
| 13 | 0.312647 | `azmcp_loadtesting_test_create` | ❌ |
| 14 | 0.301026 | `azmcp_functionapp_get` | ❌ |
| 15 | 0.298397 | `azmcp_group_list` | ❌ |
| 16 | 0.291625 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.288589 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 18 | 0.284723 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.277823 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.271191 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 291

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702353 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.518036 | `azmcp_sql_server_list` | ❌ |
| 3 | 0.495580 | `azmcp_sql_server_create` | ❌ |
| 4 | 0.486195 | `azmcp_sql_db_delete` | ❌ |
| 5 | 0.483132 | `azmcp_workbooks_delete` | ❌ |
| 6 | 0.470205 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.449007 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.448514 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.438950 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.417035 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 11 | 0.346442 | `azmcp_functionapp_get` | ❌ |
| 12 | 0.333269 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 13 | 0.323460 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.317588 | `azmcp_extension_azqr` | ❌ |
| 15 | 0.317267 | `azmcp_group_list` | ❌ |
| 16 | 0.310685 | `azmcp_appservice_database_add` | ❌ |
| 17 | 0.307426 | `azmcp_appconfig_kv_delete` | ❌ |
| 18 | 0.289982 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.275321 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 20 | 0.273552 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 292

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
| 6 | 0.306368 | `azmcp_sql_db_show` | ❌ |
| 7 | 0.301933 | `azmcp_sql_db_delete` | ❌ |
| 8 | 0.299785 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.295963 | `azmcp_sql_db_list` | ❌ |
| 10 | 0.295089 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.274716 | `azmcp_appservice_database_add` | ❌ |
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

## Test 293

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
| 6 | 0.274865 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.262387 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 8 | 0.261656 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 9 | 0.254391 | `azmcp_appconfig_kv_delete` | ❌ |
| 10 | 0.247364 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.237815 | `azmcp_mysql_table_list` | ❌ |
| 12 | 0.213559 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.168042 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.159907 | `azmcp_kusto_table_list` | ❌ |
| 15 | 0.156253 | `azmcp_cosmos_database_list` | ❌ |
| 16 | 0.148272 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.146243 | `azmcp_kusto_table_schema` | ❌ |
| 18 | 0.142127 | `azmcp_kusto_query` | ❌ |
| 19 | 0.125251 | `azmcp_loadtesting_testrun_list` | ❌ |
| 20 | 0.123510 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 294

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783526 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.456051 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.434868 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.401853 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376055 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.365636 | `azmcp_postgres_server_list` | ❌ |
| 7 | 0.352607 | `azmcp_mysql_database_list` | ❌ |
| 8 | 0.344454 | `azmcp_mysql_server_list` | ❌ |
| 9 | 0.343559 | `azmcp_mysql_table_list` | ❌ |
| 10 | 0.329480 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.291758 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.280450 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.258095 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.249297 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.249153 | `azmcp_kusto_database_list` | ❌ |
| 16 | 0.245298 | `azmcp_group_list` | ❌ |
| 17 | 0.234681 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.234181 | `azmcp_keyvault_secret_list` | ❌ |
| 19 | 0.233337 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.228391 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 295

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713284 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.413144 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.368082 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.315966 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.311085 | `azmcp_postgres_server_list` | ❌ |
| 6 | 0.304832 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 7 | 0.303560 | `azmcp_postgres_server_config_get` | ❌ |
| 8 | 0.289857 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.287372 | `azmcp_mysql_database_list` | ❌ |
| 10 | 0.283806 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.273063 | `azmcp_foundry_agents_list` | ❌ |
| 12 | 0.214529 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.205965 | `azmcp_appservice_database_add` | ❌ |
| 14 | 0.197679 | `azmcp_cosmos_database_container_list` | ❌ |
| 15 | 0.194313 | `azmcp_appconfig_account_list` | ❌ |
| 16 | 0.193050 | `azmcp_kusto_database_list` | ❌ |
| 17 | 0.191538 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.188120 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.183184 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.182280 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 296

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646469 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.356004 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.322163 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.307885 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.253846 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.237068 | `azmcp_mysql_table_list` | ❌ |
| 7 | 0.236121 | `azmcp_mysql_server_list` | ❌ |
| 8 | 0.235091 | `azmcp_appservice_database_add` | ❌ |
| 9 | 0.230857 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.228284 | `azmcp_sql_server_delete` | ❌ |
| 11 | 0.222975 | `azmcp_sql_db_update` | ❌ |
| 12 | 0.212602 | `azmcp_cloudarchitect_design` | ❌ |
| 13 | 0.210601 | `azmcp_foundry_agents_list` | ❌ |
| 14 | 0.200430 | `azmcp_applens_resource_diagnose` | ❌ |
| 15 | 0.189829 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.188167 | `azmcp_deploy_plan_get` | ❌ |
| 17 | 0.180851 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.180663 | `azmcp_foundry_agents_connect` | ❌ |
| 19 | 0.180437 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.174342 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 297

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635134 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532729 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448932 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.432802 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.423223 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.403858 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.397912 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.361316 | `azmcp_appservice_database_add` | ❌ |
| 10 | 0.335670 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.326512 | `azmcp_sql_db_update` | ❌ |
| 12 | 0.290311 | `azmcp_deploy_iac_rules_get` | ❌ |
| 13 | 0.288030 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.271102 | `azmcp_keyvault_secret_create` | ❌ |
| 15 | 0.268480 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.265059 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 17 | 0.260209 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.253771 | `azmcp_deploy_plan_get` | ❌ |
| 19 | 0.251921 | `azmcp_foundry_agents_connect` | ❌ |
| 20 | 0.250771 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 298

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670345 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533523 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316619 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.295018 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.287526 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.284229 | `azmcp_appservice_database_add` | ❌ |
| 8 | 0.271240 | `azmcp_sql_server_delete` | ❌ |
| 9 | 0.252999 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 10 | 0.248826 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.237646 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.222068 | `azmcp_azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 13 | 0.174851 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.174584 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.166723 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.151674 | `azmcp_keyvault_secret_create` | ❌ |
| 17 | 0.149884 | `azmcp_kusto_query` | ❌ |
| 18 | 0.145883 | `azmcp_foundry_agents_connect` | ❌ |
| 19 | 0.143603 | `azmcp_appconfig_kv_set` | ❌ |
| 20 | 0.140206 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 299

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684716 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574392 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539577 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.429010 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.395165 | `azmcp_sql_db_create` | ❌ |
| 6 | 0.356402 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.339841 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.316783 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.296548 | `azmcp_appservice_database_add` | ❌ |
| 10 | 0.281043 | `azmcp_postgres_server_param_set` | ❌ |
| 11 | 0.270400 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 12 | 0.248777 | `azmcp_loadtesting_test_create` | ❌ |
| 13 | 0.240101 | `azmcp_keyvault_secret_create` | ❌ |
| 14 | 0.229431 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.221981 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.221003 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.219182 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 18 | 0.209374 | `azmcp_loadtesting_testrun_create` | ❌ |
| 19 | 0.207520 | `azmcp_loadtesting_testresource_create` | ❌ |
| 20 | 0.197104 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 300

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.543896 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.540052 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.527546 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.436585 | `azmcp_sql_db_delete` | ❌ |
| 6 | 0.418504 | `azmcp_sql_server_show` | ❌ |
| 7 | 0.410574 | `azmcp_workbooks_delete` | ❌ |
| 8 | 0.386562 | `azmcp_sql_server_list` | ❌ |
| 9 | 0.342045 | `azmcp_sql_db_update` | ❌ |
| 10 | 0.342004 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.312054 | `azmcp_appconfig_kv_delete` | ❌ |
| 12 | 0.306441 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.263959 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.245270 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 15 | 0.241580 | `azmcp_deploy_iac_rules_get` | ❌ |
| 16 | 0.231494 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.225227 | `azmcp_kusto_query` | ❌ |
| 18 | 0.223780 | `azmcp_keyvault_secret_get` | ❌ |
| 19 | 0.220989 | `azmcp_get_bestpractices_get` | ❌ |
| 20 | 0.216857 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 301

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670179 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574400 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.530158 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.398706 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.310449 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.298395 | `azmcp_sql_db_delete` | ❌ |
| 7 | 0.293097 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.259110 | `azmcp_workbooks_delete` | ❌ |
| 9 | 0.254974 | `azmcp_appconfig_kv_delete` | ❌ |
| 10 | 0.251024 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 11 | 0.231953 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.227875 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.182013 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.158025 | `azmcp_kusto_query` | ❌ |
| 15 | 0.156028 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.152458 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.152084 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 18 | 0.145688 | `azmcp_loadtesting_test_get` | ❌ |
| 19 | 0.142881 | `azmcp_deploy_iac_rules_get` | ❌ |
| 20 | 0.142512 | `azmcp_kusto_database_list` | ❌ |

---

## Test 302

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671212 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601294 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.576930 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.441605 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.367883 | `azmcp_sql_server_show` | ❌ |
| 6 | 0.336482 | `azmcp_sql_db_delete` | ❌ |
| 7 | 0.332209 | `azmcp_sql_server_list` | ❌ |
| 8 | 0.293354 | `azmcp_sql_server_create` | ❌ |
| 9 | 0.291427 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 10 | 0.286559 | `azmcp_sql_db_update` | ❌ |
| 11 | 0.264013 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.252095 | `azmcp_appconfig_kv_delete` | ❌ |
| 13 | 0.222155 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 14 | 0.185585 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.185007 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.183564 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.181757 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 18 | 0.180404 | `azmcp_kusto_query` | ❌ |
| 19 | 0.175839 | `azmcp_keyvault_secret_get` | ❌ |
| 20 | 0.174348 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 303

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729372 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549389 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.418817 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.392556 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.385148 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.359228 | `azmcp_sql_db_list` | ❌ |
| 9 | 0.356700 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.355203 | `azmcp_mysql_table_list` | ❌ |
| 11 | 0.278098 | `azmcp_cosmos_database_list` | ❌ |
| 12 | 0.274634 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.271222 | `azmcp_keyvault_secret_list` | ❌ |
| 14 | 0.270667 | `azmcp_cosmos_account_list` | ❌ |
| 15 | 0.263181 | `azmcp_kusto_table_list` | ❌ |
| 16 | 0.256310 | `azmcp_aks_nodepool_list` | ❌ |
| 17 | 0.253852 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.251395 | `azmcp_keyvault_certificate_list` | ❌ |
| 19 | 0.249012 | `azmcp_keyvault_key_list` | ❌ |
| 20 | 0.248780 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 304

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630774 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.523847 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410680 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.348100 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.316879 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.312035 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.298995 | `azmcp_mysql_server_param_get` | ❌ |
| 9 | 0.294466 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.293371 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.225372 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.217423 | `azmcp_appservice_database_add` | ❌ |
| 13 | 0.211187 | `azmcp_eventgrid_subscription_list` | ❌ |
| 14 | 0.210531 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.209562 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.206477 | `azmcp_deploy_iac_rules_get` | ❌ |
| 17 | 0.206114 | `azmcp_kusto_table_list` | ❌ |
| 18 | 0.200976 | `azmcp_keyvault_secret_get` | ❌ |
| 19 | 0.197711 | `azmcp_kusto_sample` | ❌ |
| 20 | 0.195864 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 305

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630536 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532236 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473501 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412957 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.350513 | `azmcp_sql_server_list` | ❌ |
| 6 | 0.308033 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.305701 | `azmcp_mysql_server_param_get` | ❌ |
| 8 | 0.304314 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.282631 | `azmcp_sql_server_create` | ❌ |
| 10 | 0.277628 | `azmcp_postgres_server_config_get` | ❌ |
| 11 | 0.221752 | `azmcp_appservice_database_add` | ❌ |
| 12 | 0.216178 | `azmcp_foundry_agents_list` | ❌ |
| 13 | 0.202416 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.200326 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 15 | 0.191165 | `azmcp_cloudarchitect_design` | ❌ |
| 16 | 0.185879 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.177454 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.176225 | `azmcp_get_bestpractices_get` | ❌ |
| 19 | 0.173318 | `azmcp_applens_resource_diagnose` | ❌ |
| 20 | 0.172371 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 306

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694404 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596686 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.578239 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.515851 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509789 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.500373 | `azmcp_sql_server_delete` | ❌ |
| 7 | 0.496921 | `azmcp_group_list` | ❌ |
| 8 | 0.496434 | `azmcp_postgres_server_list` | ❌ |
| 9 | 0.495321 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 10 | 0.487690 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.487455 | `azmcp_sql_server_show` | ❌ |
| 12 | 0.473451 | `azmcp_workbooks_list` | ❌ |
| 13 | 0.449346 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.449174 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.419283 | `azmcp_functionapp_get` | ❌ |
| 16 | 0.403710 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.395561 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.384532 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.384389 | `azmcp_kusto_database_list` | ❌ |
| 20 | 0.380949 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 307

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
| 13 | 0.444175 | `azmcp_acr_registry_list` | ❌ |
| 14 | 0.419472 | `azmcp_foundry_agents_list` | ❌ |
| 15 | 0.418009 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.410302 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.397122 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.395060 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.391940 | `azmcp_kusto_cluster_list` | ❌ |
| 20 | 0.384337 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 308

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `azmcp_sql_db_show` | ❌ |
| 2 | 0.595184 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587728 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.559893 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.540218 | `azmcp_sql_db_list` | ❌ |
| 6 | 0.491437 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.488317 | `azmcp_sql_server_delete` | ❌ |
| 8 | 0.481847 | `azmcp_functionapp_get` | ❌ |
| 9 | 0.480067 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.478713 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.450140 | `azmcp_aks_cluster_get` | ❌ |
| 12 | 0.445602 | `azmcp_storage_account_get` | ❌ |
| 13 | 0.437447 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 14 | 0.424890 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.410399 | `azmcp_group_list` | ❌ |
| 16 | 0.400396 | `azmcp_aks_nodepool_get` | ❌ |
| 17 | 0.394066 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.385318 | `azmcp_extension_azqr` | ❌ |
| 19 | 0.383493 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.373431 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 309

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
| 9 | 0.413928 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 10 | 0.406630 | `azmcp_loadtesting_test_get` | ❌ |
| 11 | 0.400928 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.359439 | `azmcp_aks_cluster_get` | ❌ |
| 13 | 0.349963 | `azmcp_aks_nodepool_get` | ❌ |
| 14 | 0.316818 | `azmcp_appconfig_kv_list` | ❌ |
| 15 | 0.314864 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.308718 | `azmcp_functionapp_get` | ❌ |
| 17 | 0.300098 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.298409 | `azmcp_appconfig_account_list` | ❌ |
| 19 | 0.296248 | `azmcp_keyvault_key_get` | ❌ |
| 20 | 0.295903 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 310

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.380021 | `azmcp_postgres_server_param_get` | ❌ |
| 4 | 0.372108 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370539 | `azmcp_sql_db_show` | ❌ |
| 6 | 0.368846 | `azmcp_sql_server_entra-admin_list` | ❌ |
| 7 | 0.367031 | `azmcp_sql_db_list` | ❌ |
| 8 | 0.363268 | `azmcp_mysql_server_config_get` | ❌ |
| 9 | 0.361792 | `azmcp_sql_server_list` | ❌ |
| 10 | 0.357960 | `azmcp_mysql_database_list` | ❌ |
| 11 | 0.288829 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.276327 | `azmcp_cosmos_database_list` | ❌ |
| 13 | 0.271945 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.268884 | `azmcp_loadtesting_testrun_get` | ❌ |
| 15 | 0.257258 | `azmcp_appconfig_kv_list` | ❌ |
| 16 | 0.255863 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.246261 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.240682 | `azmcp_cosmos_account_list` | ❌ |
| 19 | 0.234423 | `azmcp_cosmos_database_container_list` | ❌ |
| 20 | 0.234389 | `azmcp_aks_nodepool_list` | ❌ |

---

## Test 311

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.418472 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.394541 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.374072 | `azmcp_loadtesting_test_create` | ❌ |
| 5 | 0.355645 | `azmcp_loadtesting_testresource_create` | ❌ |
| 6 | 0.352179 | `azmcp_storage_blob_container_get` | ❌ |
| 7 | 0.323501 | `azmcp_appconfig_kv_set` | ❌ |
| 8 | 0.319843 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.311941 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.311275 | `azmcp_storage_blob_upload` | ❌ |
| 11 | 0.300268 | `azmcp_sql_server_create` | ❌ |
| 12 | 0.297236 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.289742 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.289299 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.286778 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.278047 | `azmcp_quota_region_availability_list` | ❌ |
| 17 | 0.277805 | `azmcp_cosmos_database_container_list` | ❌ |
| 18 | 0.269922 | `azmcp_keyvault_secret_create` | ❌ |
| 19 | 0.267474 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.262425 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 312

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
| 7 | 0.344628 | `azmcp_loadtesting_testresource_create` | ❌ |
| 8 | 0.340135 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.329141 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.324376 | `azmcp_sql_server_create` | ❌ |
| 11 | 0.310931 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.310707 | `azmcp_storage_blob_upload` | ❌ |
| 13 | 0.310332 | `azmcp_workbooks_create` | ❌ |
| 14 | 0.296380 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.284467 | `azmcp_deploy_plan_get` | ❌ |
| 16 | 0.284385 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.283067 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 18 | 0.280192 | `azmcp_cloudarchitect_design` | ❌ |
| 19 | 0.272299 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 20 | 0.270659 | `azmcp_appservice_database_add` | ❌ |

---

## Test 313

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589003 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.464611 | `azmcp_storage_blob_container_create` | ❌ |
| 3 | 0.447156 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.437040 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.407389 | `azmcp_storage_blob_container_get` | ❌ |
| 6 | 0.383865 | `azmcp_sql_server_create` | ❌ |
| 7 | 0.383853 | `azmcp_loadtesting_testresource_create` | ❌ |
| 8 | 0.382274 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.380711 | `azmcp_loadtesting_test_create` | ❌ |
| 10 | 0.372681 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 11 | 0.366696 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 12 | 0.363721 | `azmcp_workbooks_create` | ❌ |
| 13 | 0.360940 | `azmcp_storage_blob_upload` | ❌ |
| 14 | 0.345804 | `azmcp_keyvault_key_create` | ❌ |
| 15 | 0.326175 | `azmcp_keyvault_certificate_create` | ❌ |
| 16 | 0.324991 | `azmcp_storage_blob_get` | ❌ |
| 17 | 0.324944 | `azmcp_monitor_resource_log_query` | ❌ |
| 18 | 0.324692 | `azmcp_appservice_database_add` | ❌ |
| 19 | 0.321846 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.309672 | `azmcp_keyvault_secret_create` | ❌ |

---

## Test 314

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655152 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.603458 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.507232 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.483435 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.442858 | `azmcp_appconfig_kv_show` | ❌ |
| 6 | 0.439236 | `azmcp_cosmos_account_list` | ❌ |
| 7 | 0.431020 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.403478 | `azmcp_cosmos_database_container_list` | ❌ |
| 9 | 0.397051 | `azmcp_mysql_server_config_get` | ❌ |
| 10 | 0.395698 | `azmcp_quota_usage_check` | ❌ |
| 11 | 0.388425 | `azmcp_aks_cluster_get` | ❌ |
| 12 | 0.382163 | `azmcp_keyvault_key_get` | ❌ |
| 13 | 0.376586 | `azmcp_keyvault_secret_get` | ❌ |
| 14 | 0.373840 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.373146 | `azmcp_sql_server_show` | ❌ |
| 16 | 0.368567 | `azmcp_sql_db_show` | ❌ |
| 17 | 0.367173 | `azmcp_subscription_list` | ❌ |
| 18 | 0.367049 | `azmcp_kusto_cluster_get` | ❌ |
| 19 | 0.366645 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.363293 | `azmcp_search_index_get` | ❌ |

---

## Test 315

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676940 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.612732 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.518279 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.514932 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.415417 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.411847 | `azmcp_appconfig_kv_show` | ❌ |
| 7 | 0.401838 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.380103 | `azmcp_sql_server_show` | ❌ |
| 9 | 0.375843 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.373568 | `azmcp_aks_cluster_get` | ❌ |
| 11 | 0.369794 | `azmcp_cosmos_database_container_list` | ❌ |
| 12 | 0.368227 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.368110 | `azmcp_kusto_cluster_get` | ❌ |
| 14 | 0.362681 | `azmcp_aks_nodepool_get` | ❌ |
| 15 | 0.362676 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.362365 | `azmcp_marketplace_product_get` | ❌ |
| 17 | 0.355144 | `azmcp_servicebus_queue_details` | ❌ |
| 18 | 0.354889 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 19 | 0.353567 | `azmcp_keyvault_key_get` | ❌ |
| 20 | 0.351188 | `azmcp_functionapp_get` | ❌ |

---

## Test 316

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
| 8 | 0.484449 | `azmcp_storage_blob_container_get` | ❌ |
| 9 | 0.484163 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.473387 | `azmcp_search_service_list` | ❌ |
| 11 | 0.458793 | `azmcp_monitor_workspace_list` | ❌ |
| 12 | 0.454207 | `azmcp_acr_registry_list` | ❌ |
| 13 | 0.448075 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.445522 | `azmcp_redis_cache_list` | ❌ |
| 15 | 0.441838 | `azmcp_redis_cluster_list` | ❌ |
| 16 | 0.439919 | `azmcp_eventgrid_subscription_list` | ❌ |
| 17 | 0.438660 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 18 | 0.432645 | `azmcp_kusto_cluster_list` | ❌ |
| 19 | 0.416486 | `azmcp_group_list` | ❌ |
| 20 | 0.412679 | `azmcp_grafana_list` | ❌ |

---

## Test 317

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499302 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.461284 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.455440 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.421642 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.379853 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 6 | 0.378256 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 7 | 0.375553 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.367906 | `azmcp_cosmos_database_list` | ❌ |
| 9 | 0.366021 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.362252 | `azmcp_storage_account_create` | ❌ |
| 11 | 0.360671 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.347173 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.346039 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.344771 | `azmcp_search_service_list` | ❌ |
| 15 | 0.342056 | `azmcp_subscription_list` | ❌ |
| 16 | 0.335306 | `azmcp_appconfig_kv_show` | ❌ |
| 17 | 0.330862 | `azmcp_mysql_database_list` | ❌ |
| 18 | 0.330437 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.315590 | `azmcp_foundry_agents_list` | ❌ |
| 20 | 0.312384 | `azmcp_acr_registry_list` | ❌ |

---

## Test 318

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557142 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.473598 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.461770 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.453933 | `azmcp_subscription_list` | ❌ |
| 5 | 0.436170 | `azmcp_search_service_list` | ❌ |
| 6 | 0.432854 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.425048 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 8 | 0.418403 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.415799 | `azmcp_storage_blob_get` | ❌ |
| 10 | 0.415080 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.389930 | `azmcp_eventgrid_subscription_list` | ❌ |
| 12 | 0.382516 | `azmcp_aks_cluster_list` | ❌ |
| 13 | 0.379856 | `azmcp_monitor_workspace_list` | ❌ |
| 14 | 0.377201 | `azmcp_quota_usage_check` | ❌ |
| 15 | 0.376660 | `azmcp_appconfig_kv_show` | ❌ |
| 16 | 0.374635 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.371828 | `azmcp_sql_server_list` | ❌ |
| 18 | 0.359998 | `azmcp_cosmos_database_list` | ❌ |
| 19 | 0.359051 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.356611 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 319

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563396 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.524779 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.507900 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.447784 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.403407 | `azmcp_storage_account_get` | ❌ |
| 6 | 0.335039 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.330963 | `azmcp_storage_blob_get` | ❌ |
| 8 | 0.326352 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.324867 | `azmcp_sql_db_create` | ❌ |
| 10 | 0.322464 | `azmcp_storage_blob_upload` | ❌ |
| 11 | 0.297912 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 12 | 0.297384 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.292093 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.291137 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.287619 | `azmcp_keyvault_secret_create` | ❌ |
| 16 | 0.281850 | `azmcp_sql_server_create` | ❌ |
| 17 | 0.281170 | `azmcp_loadtesting_testresource_create` | ❌ |
| 18 | 0.280866 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.277932 | `azmcp_keyvault_key_create` | ❌ |
| 20 | 0.274863 | `azmcp_workbooks_create` | ❌ |

---

## Test 320

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512578 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.500624 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.470896 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415378 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.414284 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.368859 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.334040 | `azmcp_storage_blob_upload` | ❌ |
| 8 | 0.320173 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 9 | 0.309739 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.296899 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.296438 | `azmcp_sql_db_create` | ❌ |
| 12 | 0.285153 | `azmcp_cosmos_account_list` | ❌ |
| 13 | 0.275240 | `azmcp_acr_registry_repository_list` | ❌ |
| 14 | 0.270167 | `azmcp_appconfig_kv_set` | ❌ |
| 15 | 0.269622 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.268922 | `azmcp_workbooks_create` | ❌ |
| 17 | 0.263818 | `azmcp_loadtesting_testresource_create` | ❌ |
| 18 | 0.256562 | `azmcp_sql_server_create` | ❌ |
| 19 | 0.252639 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.249658 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 321

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463198 | `azmcp_storage_account_create` | ❌ |
| 2 | 0.455073 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.451690 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 4 | 0.435099 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.387977 | `azmcp_storage_blob_get` | ❌ |
| 6 | 0.378021 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 7 | 0.366330 | `azmcp_storage_account_get` | ❌ |
| 8 | 0.329038 | `azmcp_cosmos_account_list` | ❌ |
| 9 | 0.322364 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.314128 | `azmcp_sql_db_create` | ❌ |
| 11 | 0.309104 | `azmcp_storage_blob_upload` | ❌ |
| 12 | 0.287885 | `azmcp_workbooks_create` | ❌ |
| 13 | 0.277049 | `azmcp_monitor_resource_log_query` | ❌ |
| 14 | 0.276533 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.269719 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.266791 | `azmcp_appconfig_kv_set` | ❌ |
| 17 | 0.265237 | `azmcp_sql_server_create` | ❌ |
| 18 | 0.262681 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.244113 | `azmcp_keyvault_certificate_create` | ❌ |
| 20 | 0.243696 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 322

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664379 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.559177 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.523288 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.518035 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.496184 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.461577 | `azmcp_storage_account_create` | ❌ |
| 7 | 0.421964 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.421220 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.384585 | `azmcp_cosmos_account_list` | ❌ |
| 10 | 0.377009 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.367759 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.359218 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.354913 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.353561 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.350264 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.335739 | `azmcp_appconfig_kv_list` | ❌ |
| 17 | 0.334806 | `azmcp_cosmos_database_list` | ❌ |
| 18 | 0.332109 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.327271 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.308777 | `azmcp_mysql_server_list` | ❌ |

---

## Test 323

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613933 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.605617 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 3 | 0.521710 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.479014 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.471385 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.453044 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.409820 | `azmcp_acr_registry_repository_list` | ❌ |
| 8 | 0.404640 | `azmcp_storage_account_create` | ❌ |
| 9 | 0.393989 | `azmcp_storage_blob_container_create` | ❌ |
| 10 | 0.386144 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.359892 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.359465 | `azmcp_search_service_list` | ❌ |
| 13 | 0.359411 | `azmcp_subscription_list` | ❌ |
| 14 | 0.356416 | `azmcp_acr_registry_list` | ❌ |
| 15 | 0.353319 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.349327 | `azmcp_keyvault_certificate_list` | ❌ |
| 17 | 0.348288 | `azmcp_appconfig_account_list` | ❌ |
| 18 | 0.333677 | `azmcp_sql_db_list` | ❌ |
| 19 | 0.333282 | `azmcp_mysql_database_list` | ❌ |
| 20 | 0.332759 | `azmcp_monitor_table_list` | ❌ |

---

## Test 324

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625329 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.511261 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.439698 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.437887 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.429587 | `azmcp_storage_blob_get` | ❌ |
| 7 | 0.418128 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.405678 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.390261 | `azmcp_cosmos_database_list` | ❌ |
| 10 | 0.384096 | `azmcp_acr_registry_repository_list` | ❌ |
| 11 | 0.355955 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 12 | 0.354374 | `azmcp_search_service_list` | ❌ |
| 13 | 0.352491 | `azmcp_appconfig_kv_show` | ❌ |
| 14 | 0.348138 | `azmcp_appconfig_account_list` | ❌ |
| 15 | 0.347296 | `azmcp_foundry_agents_list` | ❌ |
| 16 | 0.346936 | `azmcp_quota_usage_check` | ❌ |
| 17 | 0.345665 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.340651 | `azmcp_subscription_list` | ❌ |
| 19 | 0.336549 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.327093 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 325

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612311 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.585549 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.483687 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.478015 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.434630 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.420653 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.408732 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.386614 | `azmcp_appconfig_kv_show` | ❌ |
| 9 | 0.359511 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.349650 | `azmcp_cosmos_account_list` | ❌ |
| 11 | 0.345484 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.337993 | `azmcp_sql_server_show` | ❌ |
| 13 | 0.333818 | `azmcp_mysql_server_config_get` | ❌ |
| 14 | 0.331007 | `azmcp_storage_blob_upload` | ❌ |
| 15 | 0.326436 | `azmcp_monitor_resource_log_query` | ❌ |
| 16 | 0.323111 | `azmcp_cosmos_database_list` | ❌ |
| 17 | 0.320972 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.318099 | `azmcp_deploy_app_logs_get` | ❌ |
| 19 | 0.303959 | `azmcp_aks_nodepool_get` | ❌ |
| 20 | 0.303449 | `azmcp_appconfig_kv_list` | ❌ |

---

## Test 326

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661707 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.661515 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.537535 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.460657 | `azmcp_storage_blob_container_create` | ❌ |
| 5 | 0.457038 | `azmcp_storage_account_create` | ❌ |
| 6 | 0.453696 | `azmcp_cosmos_database_container_list` | ❌ |
| 7 | 0.370177 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.360712 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 9 | 0.359655 | `azmcp_aks_cluster_get` | ❌ |
| 10 | 0.358376 | `azmcp_storage_blob_upload` | ❌ |
| 11 | 0.353461 | `azmcp_kusto_cluster_get` | ❌ |
| 12 | 0.352984 | `azmcp_workbooks_show` | ❌ |
| 13 | 0.352671 | `azmcp_sql_server_show` | ❌ |
| 14 | 0.348551 | `azmcp_appconfig_kv_show` | ❌ |
| 15 | 0.342979 | `azmcp_aks_nodepool_get` | ❌ |
| 16 | 0.337010 | `azmcp_mysql_server_config_get` | ❌ |
| 17 | 0.334138 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.329754 | `azmcp_monitor_resource_log_query` | ❌ |
| 19 | 0.326300 | `azmcp_keyvault_secret_get` | ❌ |
| 20 | 0.319366 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 327

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592389 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.579070 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.568264 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 4 | 0.465942 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.452160 | `azmcp_cosmos_account_list` | ❌ |
| 6 | 0.415853 | `azmcp_cosmos_database_list` | ❌ |
| 7 | 0.413280 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.400483 | `azmcp_acr_registry_repository_list` | ❌ |
| 9 | 0.394852 | `azmcp_storage_account_create` | ❌ |
| 10 | 0.379099 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 11 | 0.367250 | `azmcp_keyvault_key_list` | ❌ |
| 12 | 0.361689 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.348959 | `azmcp_keyvault_certificate_list` | ❌ |
| 14 | 0.348821 | `azmcp_subscription_list` | ❌ |
| 15 | 0.348144 | `azmcp_keyvault_secret_list` | ❌ |
| 16 | 0.340190 | `azmcp_monitor_resource_log_query` | ❌ |
| 17 | 0.331545 | `azmcp_appconfig_kv_list` | ❌ |
| 18 | 0.328193 | `azmcp_search_service_list` | ❌ |
| 19 | 0.313259 | `azmcp_sql_db_list` | ❌ |
| 20 | 0.310914 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 328

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.569839 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.549205 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
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
| 13 | 0.339828 | `azmcp_deploy_app_logs_get` | ❌ |
| 14 | 0.336142 | `azmcp_monitor_resource_log_query` | ❌ |
| 15 | 0.314069 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.308890 | `azmcp_storage_blob_upload` | ❌ |
| 17 | 0.306951 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.300276 | `azmcp_acr_registry_list` | ❌ |
| 19 | 0.298968 | `azmcp_mysql_server_list` | ❌ |
| 20 | 0.294761 | `azmcp_subscription_list` | ❌ |

---

## Test 329

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566287 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.403254 | `azmcp_storage_blob_get` | ❌ |
| 3 | 0.397534 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.382123 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.377255 | `azmcp_storage_blob_container_create` | ❌ |
| 6 | 0.351920 | `azmcp_storage_account_get` | ❌ |
| 7 | 0.327416 | `azmcp_cosmos_database_container_list` | ❌ |
| 8 | 0.324049 | `azmcp_appconfig_kv_set` | ❌ |
| 9 | 0.284896 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 10 | 0.284377 | `azmcp_monitor_resource_log_query` | ❌ |
| 11 | 0.278290 | `azmcp_keyvault_certificate_import` | ❌ |
| 12 | 0.273638 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 13 | 0.273513 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.272315 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 15 | 0.257821 | `azmcp_deploy_app_logs_get` | ❌ |
| 16 | 0.254581 | `azmcp_workbooks_delete` | ❌ |
| 17 | 0.253430 | `azmcp_appconfig_kv_show` | ❌ |
| 18 | 0.239522 | `azmcp_foundry_models_deploy` | ❌ |
| 19 | 0.211052 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.210171 | `azmcp_quota_usage_check` | ❌ |

---

## Test 330

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576055 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.473844 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.471653 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.465428 | `azmcp_eventgrid_subscription_list` | ❌ |
| 6 | 0.452471 | `azmcp_search_service_list` | ❌ |
| 7 | 0.450973 | `azmcp_redis_cluster_list` | ❌ |
| 8 | 0.445724 | `azmcp_grafana_list` | ❌ |
| 9 | 0.431337 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.430382 | `azmcp_group_list` | ❌ |
| 11 | 0.422723 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.406935 | `azmcp_appconfig_account_list` | ❌ |
| 13 | 0.395083 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.388737 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.380636 | `azmcp_marketplace_product_list` | ❌ |
| 16 | 0.367761 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.366842 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.355344 | `azmcp_marketplace_product_get` | ❌ |
| 19 | 0.354245 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 20 | 0.348524 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 331

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405723 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.381238 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.380789 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.351864 | `azmcp_grafana_list` | ❌ |
| 5 | 0.350964 | `azmcp_redis_cache_list` | ❌ |
| 6 | 0.341813 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.334744 | `azmcp_eventgrid_topic_list` | ❌ |
| 8 | 0.328109 | `azmcp_search_service_list` | ❌ |
| 9 | 0.315604 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.308874 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.303528 | `azmcp_cosmos_account_list` | ❌ |
| 12 | 0.303367 | `azmcp_marketplace_product_list` | ❌ |
| 13 | 0.297296 | `azmcp_group_list` | ❌ |
| 14 | 0.296282 | `azmcp_monitor_workspace_list` | ❌ |
| 15 | 0.295180 | `azmcp_marketplace_product_get` | ❌ |
| 16 | 0.285434 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 17 | 0.275389 | `azmcp_loadtesting_testresource_list` | ❌ |
| 18 | 0.274945 | `azmcp_aks_cluster_list` | ❌ |
| 19 | 0.268988 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.258047 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 332

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.319958 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.315547 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.307697 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.286726 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.282645 | `azmcp_grafana_list` | ❌ |
| 6 | 0.279702 | `azmcp_redis_cluster_list` | ❌ |
| 7 | 0.278798 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.273758 | `azmcp_marketplace_product_list` | ❌ |
| 9 | 0.256358 | `azmcp_kusto_cluster_list` | ❌ |
| 10 | 0.254815 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 11 | 0.252879 | `azmcp_eventgrid_topic_list` | ❌ |
| 12 | 0.252520 | `azmcp_loadtesting_testresource_list` | ❌ |
| 13 | 0.251683 | `azmcp_search_service_list` | ❌ |
| 14 | 0.250688 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.233143 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.230571 | `azmcp_cosmos_account_list` | ❌ |
| 17 | 0.230324 | `azmcp_kusto_cluster_get` | ❌ |
| 18 | 0.226446 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.222799 | `azmcp_appconfig_account_list` | ❌ |
| 20 | 0.218838 | `azmcp_aks_cluster_list` | ❌ |

---

## Test 333

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.403229 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.370692 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.354553 | `azmcp_redis_cache_list` | ❌ |
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
| 15 | 0.282310 | `azmcp_loadtesting_testresource_list` | ❌ |
| 16 | 0.281294 | `azmcp_appconfig_account_list` | ❌ |
| 17 | 0.273270 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.269972 | `azmcp_group_list` | ❌ |
| 19 | 0.258577 | `azmcp_aks_cluster_list` | ❌ |
| 20 | 0.233362 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 334

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625240 | `azmcp_deploy_iac_rules_get` | ❌ |
| 3 | 0.605047 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.482936 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466161 | `azmcp_deploy_plan_get` | ❌ |
| 6 | 0.431102 | `azmcp_cloudarchitect_design` | ❌ |
| 7 | 0.389080 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.386480 | `azmcp_quota_usage_check` | ❌ |
| 9 | 0.372605 | `azmcp_deploy_app_logs_get` | ❌ |
| 10 | 0.369169 | `azmcp_applens_resource_diagnose` | ❌ |
| 11 | 0.362323 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.354086 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.339022 | `azmcp_mysql_server_list` | ❌ |
| 14 | 0.333150 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 15 | 0.312592 | `azmcp_mysql_server_config_get` | ❌ |
| 16 | 0.310275 | `azmcp_mysql_table_schema_get` | ❌ |
| 17 | 0.305259 | `azmcp_mysql_database_query` | ❌ |
| 18 | 0.303853 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 19 | 0.302307 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.300716 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 335

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.509957 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.473596 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.444297 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.421559 | `azmcp_keyvault_key_get` | ❌ |
| 7 | 0.395752 | `azmcp_keyvault_secret_list` | ❌ |
| 8 | 0.388926 | `azmcp_keyvault_secret_create` | ❌ |
| 9 | 0.385668 | `azmcp_keyvault_certificate_create` | ❌ |
| 10 | 0.379390 | `azmcp_keyvault_key_list` | ❌ |
| 11 | 0.304912 | `azmcp_quota_usage_check` | ❌ |
| 12 | 0.304137 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.300776 | `azmcp_quota_region_availability_list` | ❌ |
| 14 | 0.292743 | `azmcp_mysql_server_list` | ❌ |
| 15 | 0.285517 | `azmcp_sql_db_create` | ❌ |
| 16 | 0.281261 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.279035 | `azmcp_storage_account_create` | ❌ |
| 18 | 0.278638 | `azmcp_mysql_server_config_get` | ❌ |
| 19 | 0.276655 | `azmcp_storage_blob_container_get` | ❌ |
| 20 | 0.274538 | `azmcp_subscription_list` | ❌ |

---

## Test 336

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
| 6 | 0.535777 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 7 | 0.527948 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.527095 | `azmcp_aks_nodepool_list` | ❌ |
| 9 | 0.525883 | `azmcp_aks_cluster_list` | ❌ |
| 10 | 0.525637 | `azmcp_sql_elastic-pool_list` | ❌ |
| 11 | 0.506601 | `azmcp_redis_cache_list` | ❌ |
| 12 | 0.505116 | `azmcp_subscription_list` | ❌ |
| 13 | 0.496297 | `azmcp_cosmos_account_list` | ❌ |
| 14 | 0.495490 | `azmcp_grafana_list` | ❌ |
| 15 | 0.492515 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.476824 | `azmcp_group_list` | ❌ |
| 17 | 0.465569 | `azmcp_aks_nodepool_get` | ❌ |
| 18 | 0.463074 | `azmcp_eventgrid_topic_list` | ❌ |
| 19 | 0.460429 | `azmcp_acr_registry_list` | ❌ |
| 20 | 0.459250 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 337

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714553 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.439611 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.402909 | `azmcp_aks_nodepool_get` | ❌ |
| 6 | 0.393721 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.364696 | `azmcp_postgres_server_list` | ❌ |
| 8 | 0.362307 | `azmcp_search_service_list` | ❌ |
| 9 | 0.359417 | `azmcp_foundry_agents_list` | ❌ |
| 10 | 0.344853 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.337530 | `azmcp_redis_cluster_list` | ❌ |
| 12 | 0.335295 | `azmcp_monitor_workspace_list` | ❌ |
| 13 | 0.333517 | `azmcp_kusto_cluster_list` | ❌ |
| 14 | 0.330904 | `azmcp_aks_cluster_list` | ❌ |
| 15 | 0.324603 | `azmcp_sql_server_list` | ❌ |
| 16 | 0.311262 | `azmcp_grafana_list` | ❌ |
| 17 | 0.310899 | `azmcp_keyvault_secret_list` | ❌ |
| 18 | 0.308168 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.306384 | `azmcp_keyvault_certificate_list` | ❌ |
| 20 | 0.302706 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 338

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812787 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.659212 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501167 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.356479 | `azmcp_aks_nodepool_list` | ❌ |
| 5 | 0.336385 | `azmcp_monitor_workspace_list` | ❌ |
| 6 | 0.327423 | `azmcp_sql_elastic-pool_list` | ❌ |
| 7 | 0.324603 | `azmcp_subscription_list` | ❌ |
| 8 | 0.324289 | `azmcp_search_service_list` | ❌ |
| 9 | 0.316295 | `azmcp_postgres_server_list` | ❌ |
| 10 | 0.315778 | `azmcp_loadtesting_testrun_list` | ❌ |
| 11 | 0.307927 | `azmcp_aks_nodepool_get` | ❌ |
| 12 | 0.305300 | `azmcp_monitor_table_list` | ❌ |
| 13 | 0.305186 | `azmcp_aks_cluster_list` | ❌ |
| 14 | 0.304414 | `azmcp_workbooks_list` | ❌ |
| 15 | 0.300364 | `azmcp_eventgrid_subscription_list` | ❌ |
| 16 | 0.297320 | `azmcp_foundry_agents_list` | ❌ |
| 17 | 0.295899 | `azmcp_grafana_list` | ❌ |
| 18 | 0.284934 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.278813 | `azmcp_cosmos_account_list` | ❌ |
| 20 | 0.278222 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 339

**Expected Tool:** `azmcp_workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552212 | `azmcp_workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.433162 | `azmcp_workbooks_update` | ❌ |
| 3 | 0.361372 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.361364 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.328113 | `azmcp_workbooks_list` | ❌ |
| 6 | 0.188088 | `azmcp_loadtesting_testresource_create` | ❌ |
| 7 | 0.178250 | `azmcp_keyvault_secret_create` | ❌ |
| 8 | 0.178035 | `azmcp_keyvault_key_create` | ❌ |
| 9 | 0.176903 | `azmcp_keyvault_certificate_create` | ❌ |
| 10 | 0.172751 | `azmcp_monitor_table_list` | ❌ |
| 11 | 0.169440 | `azmcp_grafana_list` | ❌ |
| 12 | 0.164006 | `azmcp_sql_db_create` | ❌ |
| 13 | 0.153950 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.148932 | `azmcp_loadtesting_test_create` | ❌ |
| 15 | 0.147365 | `azmcp_monitor_workspace_list` | ❌ |
| 16 | 0.143733 | `azmcp_sql_server_create` | ❌ |
| 17 | 0.130516 | `azmcp_loadtesting_testrun_create` | ❌ |
| 18 | 0.130339 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.113882 | `azmcp_deploy_plan_get` | ❌ |
| 20 | 0.111941 | `azmcp_extension_azqr` | ❌ |

---

## Test 340

**Expected Tool:** `azmcp_workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621310 | `azmcp_workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.518668 | `azmcp_workbooks_show` | ❌ |
| 3 | 0.432454 | `azmcp_workbooks_create` | ❌ |
| 4 | 0.425505 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.390355 | `azmcp_workbooks_update` | ❌ |
| 6 | 0.273939 | `azmcp_grafana_list` | ❌ |
| 7 | 0.256795 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 8 | 0.248002 | `azmcp_sql_db_delete` | ❌ |
| 9 | 0.242993 | `azmcp_sql_server_delete` | ❌ |
| 10 | 0.198585 | `azmcp_appconfig_kv_delete` | ❌ |
| 11 | 0.190455 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.186665 | `azmcp_quota_region_availability_list` | ❌ |
| 13 | 0.148882 | `azmcp_extension_azqr` | ❌ |
| 14 | 0.145156 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.132504 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 16 | 0.131856 | `azmcp_group_list` | ❌ |
| 17 | 0.122450 | `azmcp_loadtesting_test_get` | ❌ |
| 18 | 0.119301 | `azmcp_loadtesting_testresource_create` | ❌ |
| 19 | 0.114388 | `azmcp_foundry_agents_connect` | ❌ |
| 20 | 0.110003 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 341

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772431 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.532493 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.516739 | `azmcp_grafana_list` | ❌ |
| 5 | 0.488597 | `azmcp_group_list` | ❌ |
| 6 | 0.487920 | `azmcp_workbooks_delete` | ❌ |
| 7 | 0.459976 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.454210 | `azmcp_monitor_workspace_list` | ❌ |
| 9 | 0.439945 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 10 | 0.428781 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.416446 | `azmcp_monitor_table_list` | ❌ |
| 12 | 0.413409 | `azmcp_sql_db_list` | ❌ |
| 13 | 0.405949 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.405913 | `azmcp_loadtesting_testresource_list` | ❌ |
| 15 | 0.399758 | `azmcp_acr_registry_repository_list` | ❌ |
| 16 | 0.365302 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.362685 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.356739 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.352940 | `azmcp_cosmos_database_list` | ❌ |
| 20 | 0.349674 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 342

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570259 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.539919 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.485504 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.472378 | `azmcp_grafana_list` | ❌ |
| 6 | 0.428025 | `azmcp_monitor_workspace_list` | ❌ |
| 7 | 0.425426 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 8 | 0.422785 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421659 | `azmcp_group_list` | ❌ |
| 10 | 0.412390 | `azmcp_mysql_server_list` | ❌ |
| 11 | 0.392298 | `azmcp_loadtesting_testresource_list` | ❌ |
| 12 | 0.380991 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.380399 | `azmcp_sql_server_list` | ❌ |
| 14 | 0.371128 | `azmcp_redis_cluster_list` | ❌ |
| 15 | 0.363744 | `azmcp_sql_db_list` | ❌ |
| 16 | 0.350839 | `azmcp_acr_registry_repository_list` | ❌ |
| 17 | 0.338282 | `azmcp_acr_registry_list` | ❌ |
| 18 | 0.337786 | `azmcp_functionapp_get` | ❌ |
| 19 | 0.334580 | `azmcp_extension_azqr` | ❌ |
| 20 | 0.333154 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 343

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.697464 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498390 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.494708 | `azmcp_workbooks_list` | ❌ |
| 4 | 0.452348 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.419105 | `azmcp_workbooks_update` | ❌ |
| 6 | 0.353546 | `azmcp_grafana_list` | ❌ |
| 7 | 0.277807 | `azmcp_quota_region_availability_list` | ❌ |
| 8 | 0.264638 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.256678 | `azmcp_quota_usage_check` | ❌ |
| 10 | 0.249917 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 11 | 0.236741 | `azmcp_monitor_resource_log_query` | ❌ |
| 12 | 0.225294 | `azmcp_loadtesting_test_get` | ❌ |
| 13 | 0.219007 | `azmcp_loadtesting_testresource_list` | ❌ |
| 14 | 0.207693 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 15 | 0.197186 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 16 | 0.195391 | `azmcp_group_list` | ❌ |
| 17 | 0.189914 | `azmcp_loadtesting_testrun_get` | ❌ |
| 18 | 0.189657 | `azmcp_extension_azqr` | ❌ |
| 19 | 0.187682 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.187564 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 344

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Show me the workbook with display name <workbook_display_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469542 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
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
| 13 | 0.168191 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.165774 | `azmcp_cosmos_database_list` | ❌ |
| 15 | 0.154760 | `azmcp_cosmos_database_container_list` | ❌ |
| 16 | 0.152535 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.149678 | `azmcp_cosmos_account_list` | ❌ |
| 18 | 0.146054 | `azmcp_kusto_table_schema` | ❌ |
| 19 | 0.141985 | `azmcp_loadtesting_testrun_get` | ❌ |
| 20 | 0.141559 | `azmcp_foundry_models_list` | ❌ |

---

## Test 345

**Expected Tool:** `azmcp_workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469915 | `azmcp_workbooks_update` | ✅ **EXPECTED** |
| 2 | 0.382651 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.362450 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.349689 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.276727 | `azmcp_loadtesting_testrun_update` | ❌ |
| 6 | 0.262873 | `azmcp_workbooks_list` | ❌ |
| 7 | 0.174311 | `azmcp_sql_db_update` | ❌ |
| 8 | 0.170118 | `azmcp_grafana_list` | ❌ |
| 9 | 0.148730 | `azmcp_mysql_server_param_set` | ❌ |
| 10 | 0.142404 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.142186 | `azmcp_loadtesting_testrun_create` | ❌ |
| 12 | 0.138354 | `azmcp_appconfig_kv_set` | ❌ |
| 13 | 0.135987 | `azmcp_loadtesting_testresource_create` | ❌ |
| 14 | 0.133097 | `azmcp_foundry_agents_evaluate` | ❌ |
| 15 | 0.131042 | `azmcp_postgres_database_query` | ❌ |
| 16 | 0.129973 | `azmcp_postgres_server_param_set` | ❌ |
| 17 | 0.129630 | `azmcp_deploy_iac_rules_get` | ❌ |
| 18 | 0.127559 | `azmcp_keyvault_certificate_import` | ❌ |
| 19 | 0.124550 | `azmcp_keyvault_secret_create` | ❌ |
| 20 | 0.116780 | `azmcp_appservice_database_add` | ❌ |

---

## Test 346

**Expected Tool:** `azmcp_bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485925 | `azmcp_deploy_iac_rules_get` | ❌ |
| 2 | 0.448373 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.440302 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.432777 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.430675 | `azmcp_bicepschema_get` | ✅ **EXPECTED** |
| 6 | 0.400985 | `azmcp_foundry_models_deploy` | ❌ |
| 7 | 0.398046 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.393793 | `azmcp_foundry_agents_connect` | ❌ |
| 9 | 0.391625 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 10 | 0.385433 | `azmcp_foundry_agents_list` | ❌ |
| 11 | 0.372097 | `azmcp_search_service_list` | ❌ |
| 12 | 0.325716 | `azmcp_search_index_query` | ❌ |
| 13 | 0.324857 | `azmcp_search_index_get` | ❌ |
| 14 | 0.317232 | `azmcp_sql_db_create` | ❌ |
| 15 | 0.303183 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.291291 | `azmcp_storage_account_create` | ❌ |
| 17 | 0.281487 | `azmcp_mysql_server_list` | ❌ |
| 18 | 0.279983 | `azmcp_workbooks_delete` | ❌ |
| 19 | 0.274793 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 20 | 0.270531 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 347

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
| 7 | 0.191391 | `azmcp_storage_blob_container_create` | ❌ |
| 8 | 0.191096 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.178245 | `azmcp_deploy_plan_get` | ❌ |
| 10 | 0.175826 | `azmcp_deploy_iac_rules_get` | ❌ |
| 11 | 0.136643 | `azmcp_storage_blob_get` | ❌ |
| 12 | 0.135768 | `azmcp_get_bestpractices_get` | ❌ |
| 13 | 0.132826 | `azmcp_storage_account_create` | ❌ |
| 14 | 0.130037 | `azmcp_foundry_models_deploy` | ❌ |
| 15 | 0.118383 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.115876 | `azmcp_marketplace_product_get` | ❌ |
| 17 | 0.111376 | `azmcp_storage_blob_container_get` | ❌ |
| 18 | 0.106651 | `azmcp_mysql_database_query` | ❌ |
| 19 | 0.104162 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.100892 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 348

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.290270 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.267683 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258160 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.225622 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.215748 | `azmcp_get_bestpractices_get` | ❌ |
| 6 | 0.207322 | `azmcp_deploy_iac_rules_get` | ❌ |
| 7 | 0.195387 | `azmcp_storage_account_create` | ❌ |
| 8 | 0.189124 | `azmcp_applens_resource_diagnose` | ❌ |
| 9 | 0.179452 | `azmcp_loadtesting_testresource_create` | ❌ |
| 10 | 0.170231 | `azmcp_foundry_agents_connect` | ❌ |
| 11 | 0.168850 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.163694 | `azmcp_mysql_database_query` | ❌ |
| 13 | 0.163615 | `azmcp_storage_blob_container_create` | ❌ |
| 14 | 0.162154 | `azmcp_sql_server_create` | ❌ |
| 15 | 0.160743 | `azmcp_quota_usage_check` | ❌ |
| 16 | 0.154249 | `azmcp_mysql_server_list` | ❌ |
| 17 | 0.152324 | `azmcp_sql_db_create` | ❌ |
| 18 | 0.145124 | `azmcp_quota_region_availability_list` | ❌ |
| 19 | 0.139758 | `azmcp_storage_account_get` | ❌ |
| 20 | 0.135749 | `azmcp_marketplace_product_get` | ❌ |

---

## Test 349

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.299640 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242581 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.218054 | `azmcp_deploy_iac_rules_get` | ❌ |
| 6 | 0.213173 | `azmcp_get_bestpractices_get` | ❌ |
| 7 | 0.179248 | `azmcp_deploy_app_logs_get` | ❌ |
| 8 | 0.169691 | `azmcp_marketplace_product_get` | ❌ |
| 9 | 0.164328 | `azmcp_mysql_server_list` | ❌ |
| 10 | 0.156442 | `azmcp_appconfig_account_list` | ❌ |
| 11 | 0.156119 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 12 | 0.151368 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.142854 | `azmcp_marketplace_product_list` | ❌ |
| 14 | 0.139970 | `azmcp_storage_blob_container_create` | ❌ |
| 15 | 0.138067 | `azmcp_storage_account_create` | ❌ |
| 16 | 0.132355 | `azmcp_mysql_database_query` | ❌ |
| 17 | 0.130132 | `azmcp_quota_usage_check` | ❌ |
| 18 | 0.123936 | `azmcp_storage_blob_upload` | ❌ |
| 19 | 0.119586 | `azmcp_workbooks_create` | ❌ |
| 20 | 0.114994 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 350

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
| 7 | 0.304531 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.300392 | `azmcp_storage_blob_container_create` | ❌ |
| 9 | 0.299412 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.298989 | `azmcp_get_bestpractices_get` | ❌ |
| 11 | 0.293806 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.292390 | `azmcp_applens_resource_diagnose` | ❌ |
| 13 | 0.291868 | `azmcp_deploy_iac_rules_get` | ❌ |
| 14 | 0.281671 | `azmcp_storage_blob_container_get` | ❌ |
| 15 | 0.275653 | `azmcp_storage_blob_get` | ❌ |
| 16 | 0.275550 | `azmcp_storage_account_get` | ❌ |
| 17 | 0.272694 | `azmcp_deploy_app_logs_get` | ❌ |
| 18 | 0.261446 | `azmcp_quota_usage_check` | ❌ |
| 19 | 0.259814 | `azmcp_search_service_list` | ❌ |
| 20 | 0.258445 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Summary

**Total Prompts Tested:** 350  
**Execution Time:** 49.3523522s  

### Success Rate Metrics

**Top Choice Success:** 83.4% (292/350 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 4.0% (14/350 tests)  
**🎯 High Confidence (≥0.7):** 17.7% (62/350 tests)  
**✅ Good Confidence (≥0.6):** 55.4% (194/350 tests)  
**👍 Fair Confidence (≥0.5):** 84.3% (295/350 tests)  
**👌 Acceptable Confidence (≥0.4):** 93.1% (326/350 tests)  
**❌ Low Confidence (<0.4):** 6.9% (24/350 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 4.0% (14/350 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 17.7% (62/350 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 52.9% (185/350 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 76.3% (267/350 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 80.9% (283/350 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing adequately but has room for improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

