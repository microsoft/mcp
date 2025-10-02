# Tool Selection Analysis Setup

**Setup completed:** 2025-09-26 17:28:14  
**Tool count:** 143  
**Database setup time:** 1.2628797s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-09-26 17:28:14  
**Tool count:** 143  

## Table of Contents

- [Test 1: foundry_agents_connect](#test-1)
- [Test 2: foundry_agents_evaluate](#test-2)
- [Test 3: foundry_agents_query-and-evaluate](#test-3)
- [Test 4: foundry_knowledge_index_list](#test-4)
- [Test 5: foundry_knowledge_index_list](#test-5)
- [Test 6: foundry_knowledge_index_schema](#test-6)
- [Test 7: foundry_knowledge_index_schema](#test-7)
- [Test 8: foundry_models_deploy](#test-8)
- [Test 9: foundry_models_deployments_list](#test-9)
- [Test 10: foundry_models_deployments_list](#test-10)
- [Test 11: foundry_models_list](#test-11)
- [Test 12: foundry_models_list](#test-12)
- [Test 13: search_index_get](#test-13)
- [Test 14: search_index_get](#test-14)
- [Test 15: search_index_get](#test-15)
- [Test 16: search_index_query](#test-16)
- [Test 17: search_service_list](#test-17)
- [Test 18: search_service_list](#test-18)
- [Test 19: search_service_list](#test-19)
- [Test 20: appconfig_account_list](#test-20)
- [Test 21: appconfig_account_list](#test-21)
- [Test 22: appconfig_account_list](#test-22)
- [Test 23: appconfig_kv_delete](#test-23)
- [Test 24: appconfig_kv_list](#test-24)
- [Test 25: appconfig_kv_list](#test-25)
- [Test 26: appconfig_kv_lock_set](#test-26)
- [Test 27: appconfig_kv_lock_set](#test-27)
- [Test 28: appconfig_kv_set](#test-28)
- [Test 29: appconfig_kv_show](#test-29)
- [Test 30: applens_resource_diagnose](#test-30)
- [Test 31: applens_resource_diagnose](#test-31)
- [Test 32: applens_resource_diagnose](#test-32)
- [Test 33: appservice_database_add](#test-33)
- [Test 34: appservice_database_add](#test-34)
- [Test 35: appservice_database_add](#test-35)
- [Test 36: appservice_database_add](#test-36)
- [Test 37: appservice_database_add](#test-37)
- [Test 38: appservice_database_add](#test-38)
- [Test 39: appservice_database_add](#test-39)
- [Test 40: appservice_database_add](#test-40)
- [Test 41: appservice_database_add](#test-41)
- [Test 42: applicationinsights_recommendation_list](#test-42)
- [Test 43: applicationinsights_recommendation_list](#test-43)
- [Test 44: applicationinsights_recommendation_list](#test-44)
- [Test 45: applicationinsights_recommendation_list](#test-45)
- [Test 46: acr_registry_list](#test-46)
- [Test 47: acr_registry_list](#test-47)
- [Test 48: acr_registry_list](#test-48)
- [Test 49: acr_registry_list](#test-49)
- [Test 50: acr_registry_list](#test-50)
- [Test 51: acr_registry_repository_list](#test-51)
- [Test 52: acr_registry_repository_list](#test-52)
- [Test 53: acr_registry_repository_list](#test-53)
- [Test 54: acr_registry_repository_list](#test-54)
- [Test 55: cosmos_account_list](#test-55)
- [Test 56: cosmos_account_list](#test-56)
- [Test 57: cosmos_account_list](#test-57)
- [Test 58: cosmos_database_container_item_query](#test-58)
- [Test 59: cosmos_database_container_list](#test-59)
- [Test 60: cosmos_database_container_list](#test-60)
- [Test 61: cosmos_database_list](#test-61)
- [Test 62: cosmos_database_list](#test-62)
- [Test 63: kusto_cluster_get](#test-63)
- [Test 64: kusto_cluster_list](#test-64)
- [Test 65: kusto_cluster_list](#test-65)
- [Test 66: kusto_cluster_list](#test-66)
- [Test 67: kusto_database_list](#test-67)
- [Test 68: kusto_database_list](#test-68)
- [Test 69: kusto_query](#test-69)
- [Test 70: kusto_sample](#test-70)
- [Test 71: kusto_table_list](#test-71)
- [Test 72: kusto_table_list](#test-72)
- [Test 73: kusto_table_schema](#test-73)
- [Test 74: mysql_database_list](#test-74)
- [Test 75: mysql_database_list](#test-75)
- [Test 76: mysql_database_query](#test-76)
- [Test 77: mysql_server_config_get](#test-77)
- [Test 78: mysql_server_list](#test-78)
- [Test 79: mysql_server_list](#test-79)
- [Test 80: mysql_server_list](#test-80)
- [Test 81: mysql_server_param_get](#test-81)
- [Test 82: mysql_server_param_set](#test-82)
- [Test 83: mysql_table_list](#test-83)
- [Test 84: mysql_table_list](#test-84)
- [Test 85: mysql_table_schema_get](#test-85)
- [Test 86: postgres_database_list](#test-86)
- [Test 87: postgres_database_list](#test-87)
- [Test 88: postgres_database_query](#test-88)
- [Test 89: postgres_server_config_get](#test-89)
- [Test 90: postgres_server_list](#test-90)
- [Test 91: postgres_server_list](#test-91)
- [Test 92: postgres_server_list](#test-92)
- [Test 93: postgres_server_param](#test-93)
- [Test 94: postgres_server_param_set](#test-94)
- [Test 95: postgres_table_list](#test-95)
- [Test 96: postgres_table_list](#test-96)
- [Test 97: postgres_table_schema_get](#test-97)
- [Test 98: deploy_app_logs_get](#test-98)
- [Test 99: deploy_architecture_diagram_generate](#test-99)
- [Test 100: deploy_iac_rules_get](#test-100)
- [Test 101: deploy_pipeline_guidance_get](#test-101)
- [Test 102: deploy_plan_get](#test-102)
- [Test 103: eventgrid_topic_list](#test-103)
- [Test 104: eventgrid_topic_list](#test-104)
- [Test 105: eventgrid_topic_list](#test-105)
- [Test 106: eventgrid_topic_list](#test-106)
- [Test 107: eventgrid_subscription_list](#test-107)
- [Test 108: eventgrid_subscription_list](#test-108)
- [Test 109: eventgrid_subscription_list](#test-109)
- [Test 110: eventgrid_subscription_list](#test-110)
- [Test 111: eventgrid_subscription_list](#test-111)
- [Test 112: eventgrid_subscription_list](#test-112)
- [Test 113: eventgrid_subscription_list](#test-113)
- [Test 114: functionapp_get](#test-114)
- [Test 115: functionapp_get](#test-115)
- [Test 116: functionapp_get](#test-116)
- [Test 117: functionapp_get](#test-117)
- [Test 118: functionapp_get](#test-118)
- [Test 119: functionapp_get](#test-119)
- [Test 120: functionapp_get](#test-120)
- [Test 121: functionapp_get](#test-121)
- [Test 122: functionapp_get](#test-122)
- [Test 123: functionapp_get](#test-123)
- [Test 124: functionapp_get](#test-124)
- [Test 125: functionapp_get](#test-125)
- [Test 126: keyvault_certificate_create](#test-126)
- [Test 127: keyvault_certificate_create](#test-127)
- [Test 128: keyvault_certificate_create](#test-128)
- [Test 129: keyvault_certificate_create](#test-129)
- [Test 130: keyvault_certificate_create](#test-130)
- [Test 131: keyvault_certificate_get](#test-131)
- [Test 132: keyvault_certificate_get](#test-132)
- [Test 133: keyvault_certificate_get](#test-133)
- [Test 134: keyvault_certificate_get](#test-134)
- [Test 135: keyvault_certificate_get](#test-135)
- [Test 136: keyvault_certificate_import](#test-136)
- [Test 137: keyvault_certificate_import](#test-137)
- [Test 138: keyvault_certificate_import](#test-138)
- [Test 139: keyvault_certificate_import](#test-139)
- [Test 140: keyvault_certificate_import](#test-140)
- [Test 141: keyvault_certificate_list](#test-141)
- [Test 142: keyvault_certificate_list](#test-142)
- [Test 143: keyvault_certificate_list](#test-143)
- [Test 144: keyvault_certificate_list](#test-144)
- [Test 145: keyvault_certificate_list](#test-145)
- [Test 146: keyvault_certificate_list](#test-146)
- [Test 147: keyvault_key_create](#test-147)
- [Test 148: keyvault_key_create](#test-148)
- [Test 149: keyvault_key_create](#test-149)
- [Test 150: keyvault_key_create](#test-150)
- [Test 151: keyvault_key_create](#test-151)
- [Test 152: keyvault_key_get](#test-152)
- [Test 153: keyvault_key_get](#test-153)
- [Test 154: keyvault_key_get](#test-154)
- [Test 155: keyvault_key_get](#test-155)
- [Test 156: keyvault_key_get](#test-156)
- [Test 157: keyvault_key_list](#test-157)
- [Test 158: keyvault_key_list](#test-158)
- [Test 159: keyvault_key_list](#test-159)
- [Test 160: keyvault_key_list](#test-160)
- [Test 161: keyvault_key_list](#test-161)
- [Test 162: keyvault_key_list](#test-162)
- [Test 163: keyvault_secret_create](#test-163)
- [Test 164: keyvault_secret_create](#test-164)
- [Test 165: keyvault_secret_create](#test-165)
- [Test 166: keyvault_secret_create](#test-166)
- [Test 167: keyvault_secret_create](#test-167)
- [Test 168: keyvault_secret_get](#test-168)
- [Test 169: keyvault_secret_get](#test-169)
- [Test 170: keyvault_secret_get](#test-170)
- [Test 171: keyvault_secret_get](#test-171)
- [Test 172: keyvault_secret_get](#test-172)
- [Test 173: keyvault_secret_list](#test-173)
- [Test 174: keyvault_secret_list](#test-174)
- [Test 175: keyvault_secret_list](#test-175)
- [Test 176: keyvault_secret_list](#test-176)
- [Test 177: keyvault_secret_list](#test-177)
- [Test 178: keyvault_secret_list](#test-178)
- [Test 179: aks_cluster_get](#test-179)
- [Test 180: aks_cluster_get](#test-180)
- [Test 181: aks_cluster_get](#test-181)
- [Test 182: aks_cluster_get](#test-182)
- [Test 183: aks_cluster_list](#test-183)
- [Test 184: aks_cluster_list](#test-184)
- [Test 185: aks_cluster_list](#test-185)
- [Test 186: aks_nodepool_get](#test-186)
- [Test 187: aks_nodepool_get](#test-187)
- [Test 188: aks_nodepool_get](#test-188)
- [Test 189: aks_nodepool_list](#test-189)
- [Test 190: aks_nodepool_list](#test-190)
- [Test 191: aks_nodepool_list](#test-191)
- [Test 192: loadtesting_test_create](#test-192)
- [Test 193: loadtesting_test_get](#test-193)
- [Test 194: loadtesting_testresource_create](#test-194)
- [Test 195: loadtesting_testresource_list](#test-195)
- [Test 196: loadtesting_testrun_create](#test-196)
- [Test 197: loadtesting_testrun_get](#test-197)
- [Test 198: loadtesting_testrun_list](#test-198)
- [Test 199: loadtesting_testrun_update](#test-199)
- [Test 200: grafana_list](#test-200)
- [Test 201: azuremanagedlustre_filesystem_list](#test-201)
- [Test 202: azuremanagedlustre_filesystem_list](#test-202)
- [Test 203: azuremanagedlustre_filesystem_required-subnet-size](#test-203)
- [Test 204: azuremanagedlustre_filesystem_sku_get](#test-204)
- [Test 205: marketplace_product_get](#test-205)
- [Test 206: marketplace_product_list](#test-206)
- [Test 207: marketplace_product_list](#test-207)
- [Test 208: bestpractices_get](#test-208)
- [Test 209: bestpractices_get](#test-209)
- [Test 210: bestpractices_get](#test-210)
- [Test 211: bestpractices_get](#test-211)
- [Test 212: bestpractices_get](#test-212)
- [Test 213: bestpractices_get](#test-213)
- [Test 214: bestpractices_get](#test-214)
- [Test 215: bestpractices_get](#test-215)
- [Test 216: bestpractices_get](#test-216)
- [Test 217: bestpractices_get](#test-217)
- [Test 218: monitor_healthmodels_entity_gethealth](#test-218)
- [Test 219: monitor_metrics_definitions](#test-219)
- [Test 220: monitor_metrics_definitions](#test-220)
- [Test 221: monitor_metrics_definitions](#test-221)
- [Test 222: monitor_metrics_query](#test-222)
- [Test 223: monitor_metrics_query](#test-223)
- [Test 224: monitor_metrics_query](#test-224)
- [Test 225: monitor_metrics_query](#test-225)
- [Test 226: monitor_metrics_query](#test-226)
- [Test 227: monitor_metrics_query](#test-227)
- [Test 228: monitor_resource_log_query](#test-228)
- [Test 229: monitor_table_list](#test-229)
- [Test 230: monitor_table_list](#test-230)
- [Test 231: monitor_table_type_list](#test-231)
- [Test 232: monitor_table_type_list](#test-232)
- [Test 233: monitor_workspace_list](#test-233)
- [Test 234: monitor_workspace_list](#test-234)
- [Test 235: monitor_workspace_list](#test-235)
- [Test 236: monitor_workspace_log_query](#test-236)
- [Test 237: datadog_monitoredresources_list](#test-237)
- [Test 238: datadog_monitoredresources_list](#test-238)
- [Test 239: extension_azqr](#test-239)
- [Test 240: extension_azqr](#test-240)
- [Test 241: extension_azqr](#test-241)
- [Test 242: quota_region_availability_list](#test-242)
- [Test 243: quota_usage_check](#test-243)
- [Test 244: role_assignment_list](#test-244)
- [Test 245: role_assignment_list](#test-245)
- [Test 246: redis_cache_accesspolicy_list](#test-246)
- [Test 247: redis_cache_accesspolicy_list](#test-247)
- [Test 248: redis_cache_list](#test-248)
- [Test 249: redis_cache_list](#test-249)
- [Test 250: redis_cache_list](#test-250)
- [Test 251: redis_cluster_database_list](#test-251)
- [Test 252: redis_cluster_database_list](#test-252)
- [Test 253: redis_cluster_list](#test-253)
- [Test 254: redis_cluster_list](#test-254)
- [Test 255: redis_cluster_list](#test-255)
- [Test 256: group_list](#test-256)
- [Test 257: group_list](#test-257)
- [Test 258: group_list](#test-258)
- [Test 259: resourcehealth_availability-status_get](#test-259)
- [Test 260: resourcehealth_availability-status_get](#test-260)
- [Test 261: resourcehealth_availability-status_get](#test-261)
- [Test 262: resourcehealth_availability-status_list](#test-262)
- [Test 263: resourcehealth_availability-status_list](#test-263)
- [Test 264: resourcehealth_availability-status_list](#test-264)
- [Test 265: resourcehealth_service-health-events_list](#test-265)
- [Test 266: resourcehealth_service-health-events_list](#test-266)
- [Test 267: resourcehealth_service-health-events_list](#test-267)
- [Test 268: resourcehealth_service-health-events_list](#test-268)
- [Test 269: resourcehealth_service-health-events_list](#test-269)
- [Test 270: servicebus_queue_details](#test-270)
- [Test 271: servicebus_topic_details](#test-271)
- [Test 272: servicebus_topic_subscription_details](#test-272)
- [Test 273: sql_db_create](#test-273)
- [Test 274: sql_db_create](#test-274)
- [Test 275: sql_db_create](#test-275)
- [Test 276: sql_db_delete](#test-276)
- [Test 277: sql_db_delete](#test-277)
- [Test 278: sql_db_delete](#test-278)
- [Test 279: sql_db_list](#test-279)
- [Test 280: sql_db_list](#test-280)
- [Test 281: sql_db_show](#test-281)
- [Test 282: sql_db_show](#test-282)
- [Test 283: sql_db_update](#test-283)
- [Test 284: sql_db_update](#test-284)
- [Test 285: sql_elastic-pool_list](#test-285)
- [Test 286: sql_elastic-pool_list](#test-286)
- [Test 287: sql_elastic-pool_list](#test-287)
- [Test 288: sql_server_create](#test-288)
- [Test 289: sql_server_create](#test-289)
- [Test 290: sql_server_create](#test-290)
- [Test 291: sql_server_delete](#test-291)
- [Test 292: sql_server_delete](#test-292)
- [Test 293: sql_server_delete](#test-293)
- [Test 294: sql_server_entra-admin_list](#test-294)
- [Test 295: sql_server_entra-admin_list](#test-295)
- [Test 296: sql_server_entra-admin_list](#test-296)
- [Test 297: sql_server_firewall-rule_create](#test-297)
- [Test 298: sql_server_firewall-rule_create](#test-298)
- [Test 299: sql_server_firewall-rule_create](#test-299)
- [Test 300: sql_server_firewall-rule_delete](#test-300)
- [Test 301: sql_server_firewall-rule_delete](#test-301)
- [Test 302: sql_server_firewall-rule_delete](#test-302)
- [Test 303: sql_server_firewall-rule_list](#test-303)
- [Test 304: sql_server_firewall-rule_list](#test-304)
- [Test 305: sql_server_firewall-rule_list](#test-305)
- [Test 306: sql_server_list](#test-306)
- [Test 307: sql_server_list](#test-307)
- [Test 308: sql_server_show](#test-308)
- [Test 309: sql_server_show](#test-309)
- [Test 310: sql_server_show](#test-310)
- [Test 311: storage_account_create](#test-311)
- [Test 312: storage_account_create](#test-312)
- [Test 313: storage_account_create](#test-313)
- [Test 314: storage_account_get](#test-314)
- [Test 315: storage_account_get](#test-315)
- [Test 316: storage_account_get](#test-316)
- [Test 317: storage_account_get](#test-317)
- [Test 318: storage_account_get](#test-318)
- [Test 319: storage_blob_container_create](#test-319)
- [Test 320: storage_blob_container_create](#test-320)
- [Test 321: storage_blob_container_create](#test-321)
- [Test 322: storage_blob_container_get](#test-322)
- [Test 323: storage_blob_container_get](#test-323)
- [Test 324: storage_blob_container_get](#test-324)
- [Test 325: storage_blob_get](#test-325)
- [Test 326: storage_blob_get](#test-326)
- [Test 327: storage_blob_get](#test-327)
- [Test 328: storage_blob_get](#test-328)
- [Test 329: storage_blob_upload](#test-329)
- [Test 330: subscription_list](#test-330)
- [Test 331: subscription_list](#test-331)
- [Test 332: subscription_list](#test-332)
- [Test 333: subscription_list](#test-333)
- [Test 334: azureterraformbestpractices_get](#test-334)
- [Test 335: azureterraformbestpractices_get](#test-335)
- [Test 336: virtualdesktop_hostpool_list](#test-336)
- [Test 337: virtualdesktop_hostpool_sessionhost_list](#test-337)
- [Test 338: virtualdesktop_hostpool_sessionhost_usersession-list](#test-338)
- [Test 339: workbooks_create](#test-339)
- [Test 340: workbooks_delete](#test-340)
- [Test 341: workbooks_list](#test-341)
- [Test 342: workbooks_list](#test-342)
- [Test 343: workbooks_show](#test-343)
- [Test 344: workbooks_show](#test-344)
- [Test 345: workbooks_update](#test-345)
- [Test 346: bicepschema_get](#test-346)
- [Test 347: cloudarchitect_design](#test-347)
- [Test 348: cloudarchitect_design](#test-348)
- [Test 349: cloudarchitect_design](#test-349)
- [Test 350: cloudarchitect_design](#test-350)

---

## Test 1

**Expected Tool:** `foundry_agents_connect`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603124 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.535829 | `foundry_agents_connect` | ✅ **EXPECTED** |
| 3 | 0.494462 | `foundry_agents_list` | ❌ |
| 4 | 0.443558 | `foundry_agents_evaluate` | ❌ |
| 5 | 0.379587 | `search_index_query` | ❌ |
| 6 | 0.365856 | `foundry_models_list` | ❌ |
| 7 | 0.355385 | `foundry_knowledge_index_list` | ❌ |
| 8 | 0.327613 | `cloudarchitect_design` | ❌ |
| 9 | 0.319855 | `foundry_models_deploy` | ❌ |
| 10 | 0.305579 | `deploy_plan_get` | ❌ |
| 11 | 0.297391 | `foundry_knowledge_index_schema` | ❌ |
| 12 | 0.272398 | `search_service_list` | ❌ |
| 13 | 0.243499 | `quota_usage_check` | ❌ |
| 14 | 0.241241 | `postgres_database_query` | ❌ |
| 15 | 0.232656 | `search_index_get` | ❌ |
| 16 | 0.230797 | `mysql_server_list` | ❌ |
| 17 | 0.226514 | `monitor_workspace_log_query` | ❌ |
| 18 | 0.217753 | `monitor_resource_log_query` | ❌ |
| 19 | 0.211141 | `mysql_database_query` | ❌ |
| 20 | 0.191244 | `monitor_healthmodels_entity_gethealth` | ❌ |

---

## Test 2

**Expected Tool:** `foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544099 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469980 | `foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.356465 | `foundry_agents_connect` | ❌ |
| 4 | 0.280833 | `cloudarchitect_design` | ❌ |
| 5 | 0.235412 | `foundry_agents_list` | ❌ |
| 6 | 0.233739 | `deploy_plan_get` | ❌ |
| 7 | 0.233415 | `loadtesting_testrun_get` | ❌ |
| 8 | 0.232102 | `quota_usage_check` | ❌ |
| 9 | 0.228525 | `applens_resource_diagnose` | ❌ |
| 10 | 0.224884 | `sql_server_entra-admin_list` | ❌ |
| 11 | 0.221027 | `deploy_app_logs_get` | ❌ |
| 12 | 0.218372 | `monitor_resource_log_query` | ❌ |
| 13 | 0.214507 | `monitor_workspace_log_query` | ❌ |
| 14 | 0.210219 | `search_index_query` | ❌ |
| 15 | 0.207677 | `postgres_database_query` | ❌ |
| 16 | 0.207578 | `loadtesting_testrun_list` | ❌ |
| 17 | 0.203902 | `redis_cache_accesspolicy_list` | ❌ |
| 18 | 0.194160 | `mysql_database_query` | ❌ |
| 19 | 0.187851 | `mysql_table_schema_get` | ❌ |
| 20 | 0.183167 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 3

**Expected Tool:** `foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my AI Foundry project for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580566 | `foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 2 | 0.519050 | `foundry_agents_evaluate` | ❌ |
| 3 | 0.471059 | `foundry_agents_connect` | ❌ |
| 4 | 0.381887 | `foundry_agents_list` | ❌ |
| 5 | 0.315849 | `deploy_plan_get` | ❌ |
| 6 | 0.315347 | `cloudarchitect_design` | ❌ |
| 7 | 0.308767 | `foundry_models_deploy` | ❌ |
| 8 | 0.276459 | `foundry_models_list` | ❌ |
| 9 | 0.253361 | `foundry_knowledge_index_list` | ❌ |
| 10 | 0.246328 | `search_index_query` | ❌ |
| 11 | 0.231512 | `deploy_app_logs_get` | ❌ |
| 12 | 0.207748 | `quota_usage_check` | ❌ |
| 13 | 0.188340 | `monitor_workspace_log_query` | ❌ |
| 14 | 0.183834 | `postgres_database_query` | ❌ |
| 15 | 0.179159 | `search_service_list` | ❌ |
| 16 | 0.166181 | `monitor_resource_log_query` | ❌ |
| 17 | 0.163139 | `sql_server_entra-admin_list` | ❌ |
| 18 | 0.162163 | `mysql_database_query` | ❌ |
| 19 | 0.153536 | `mysql_server_list` | ❌ |
| 20 | 0.152762 | `redis_cache_accesspolicy_list` | ❌ |

---

## Test 4

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695201 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.532985 | `foundry_agents_list` | ❌ |
| 3 | 0.526485 | `foundry_knowledge_index_schema` | ❌ |
| 4 | 0.433117 | `foundry_models_list` | ❌ |
| 5 | 0.422779 | `search_index_get` | ❌ |
| 6 | 0.412895 | `search_service_list` | ❌ |
| 7 | 0.349506 | `search_index_query` | ❌ |
| 8 | 0.329682 | `foundry_models_deploy` | ❌ |
| 9 | 0.310470 | `foundry_models_deployments_list` | ❌ |
| 10 | 0.309513 | `monitor_table_list` | ❌ |
| 11 | 0.296877 | `grafana_list` | ❌ |
| 12 | 0.294101 | `keyvault_key_list` | ❌ |
| 13 | 0.291635 | `azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.286074 | `monitor_table_type_list` | ❌ |
| 15 | 0.270303 | `redis_cache_list` | ❌ |
| 16 | 0.270162 | `monitor_workspace_list` | ❌ |
| 17 | 0.267906 | `kusto_cluster_list` | ❌ |
| 18 | 0.265680 | `mysql_server_list` | ❌ |
| 19 | 0.264056 | `mysql_database_list` | ❌ |
| 20 | 0.262242 | `redis_cluster_list` | ❌ |

---

## Test 5

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603362 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489302 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.473963 | `foundry_agents_list` | ❌ |
| 4 | 0.396806 | `foundry_models_list` | ❌ |
| 5 | 0.374683 | `search_index_get` | ❌ |
| 6 | 0.350742 | `search_service_list` | ❌ |
| 7 | 0.341852 | `search_index_query` | ❌ |
| 8 | 0.317973 | `cloudarchitect_design` | ❌ |
| 9 | 0.310597 | `foundry_models_deploy` | ❌ |
| 10 | 0.278143 | `foundry_models_deployments_list` | ❌ |
| 11 | 0.276854 | `quota_usage_check` | ❌ |
| 12 | 0.272218 | `azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.256208 | `grafana_list` | ❌ |
| 14 | 0.250431 | `foundry_agents_connect` | ❌ |
| 15 | 0.232856 | `monitor_table_list` | ❌ |
| 16 | 0.225269 | `redis_cache_list` | ❌ |
| 17 | 0.224181 | `redis_cluster_list` | ❌ |
| 18 | 0.223832 | `mysql_server_list` | ❌ |
| 19 | 0.223736 | `monitor_metrics_definitions` | ❌ |
| 20 | 0.218039 | `quota_region_availability_list` | ❌ |

---

## Test 6

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672563 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.564860 | `foundry_knowledge_index_list` | ❌ |
| 3 | 0.424581 | `search_index_get` | ❌ |
| 4 | 0.397225 | `foundry_agents_list` | ❌ |
| 5 | 0.375275 | `mysql_table_schema_get` | ❌ |
| 6 | 0.363951 | `kusto_table_schema` | ❌ |
| 7 | 0.358315 | `postgres_table_schema_get` | ❌ |
| 8 | 0.349967 | `search_index_query` | ❌ |
| 9 | 0.347762 | `foundry_models_list` | ❌ |
| 10 | 0.346329 | `monitor_table_list` | ❌ |
| 11 | 0.326807 | `search_service_list` | ❌ |
| 12 | 0.297822 | `foundry_models_deploy` | ❌ |
| 13 | 0.295847 | `mysql_table_list` | ❌ |
| 14 | 0.285897 | `monitor_table_type_list` | ❌ |
| 15 | 0.277468 | `deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.271427 | `cloudarchitect_design` | ❌ |
| 17 | 0.266288 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.259298 | `mysql_database_list` | ❌ |
| 19 | 0.253702 | `grafana_list` | ❌ |
| 20 | 0.236805 | `mysql_server_list` | ❌ |

---

## Test 7

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650243 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432759 | `postgres_table_schema_get` | ❌ |
| 3 | 0.415963 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.408316 | `kusto_table_schema` | ❌ |
| 5 | 0.398186 | `mysql_table_schema_get` | ❌ |
| 6 | 0.379800 | `search_index_get` | ❌ |
| 7 | 0.352243 | `postgres_server_config_get` | ❌ |
| 8 | 0.318648 | `appconfig_kv_list` | ❌ |
| 9 | 0.311623 | `monitor_table_list` | ❌ |
| 10 | 0.309927 | `loadtesting_test_get` | ❌ |
| 11 | 0.286991 | `mysql_server_config_get` | ❌ |
| 12 | 0.271893 | `aks_cluster_get` | ❌ |
| 13 | 0.271701 | `loadtesting_testrun_list` | ❌ |
| 14 | 0.262783 | `aks_nodepool_get` | ❌ |
| 15 | 0.257402 | `mysql_table_list` | ❌ |
| 16 | 0.256303 | `appconfig_kv_show` | ❌ |
| 17 | 0.249010 | `search_index_query` | ❌ |
| 18 | 0.246815 | `monitor_table_type_list` | ❌ |
| 19 | 0.242191 | `mysql_server_param_get` | ❌ |
| 20 | 0.239938 | `azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 8

**Expected Tool:** `foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.313400 | `foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.282464 | `mysql_server_list` | ❌ |
| 3 | 0.274011 | `deploy_plan_get` | ❌ |
| 4 | 0.269848 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.268967 | `deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.234019 | `deploy_iac_rules_get` | ❌ |
| 7 | 0.222504 | `grafana_list` | ❌ |
| 8 | 0.222478 | `datadog_monitoredresources_list` | ❌ |
| 9 | 0.221635 | `workbooks_create` | ❌ |
| 10 | 0.217001 | `monitor_resource_log_query` | ❌ |
| 11 | 0.216588 | `applicationinsights_recommendation_list` | ❌ |
| 12 | 0.215255 | `loadtesting_testrun_create` | ❌ |
| 13 | 0.209865 | `azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.208124 | `quota_region_availability_list` | ❌ |
| 15 | 0.207601 | `quota_usage_check` | ❌ |
| 16 | 0.204420 | `postgres_server_param_set` | ❌ |
| 17 | 0.197001 | `loadtesting_testrun_update` | ❌ |
| 18 | 0.195615 | `workbooks_list` | ❌ |
| 19 | 0.192764 | `monitor_metrics_query` | ❌ |
| 20 | 0.192373 | `storage_account_create` | ❌ |

---

## Test 9

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.559508 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.549636 | `foundry_models_list` | ❌ |
| 3 | 0.539695 | `foundry_agents_list` | ❌ |
| 4 | 0.533239 | `foundry_models_deploy` | ❌ |
| 5 | 0.448711 | `search_service_list` | ❌ |
| 6 | 0.434472 | `foundry_knowledge_index_list` | ❌ |
| 7 | 0.368173 | `deploy_plan_get` | ❌ |
| 8 | 0.334867 | `grafana_list` | ❌ |
| 9 | 0.332002 | `mysql_server_list` | ❌ |
| 10 | 0.328215 | `foundry_knowledge_index_schema` | ❌ |
| 11 | 0.326752 | `search_index_get` | ❌ |
| 12 | 0.320998 | `azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.318854 | `postgres_server_list` | ❌ |
| 14 | 0.310280 | `deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.302262 | `monitor_table_type_list` | ❌ |
| 16 | 0.301302 | `redis_cluster_list` | ❌ |
| 17 | 0.300357 | `search_index_query` | ❌ |
| 18 | 0.289448 | `monitor_workspace_list` | ❌ |
| 19 | 0.288342 | `redis_cache_list` | ❌ |
| 20 | 0.285916 | `quota_region_availability_list` | ❌ |

---

## Test 10

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518221 | `foundry_models_list` | ❌ |
| 2 | 0.503424 | `foundry_models_deploy` | ❌ |
| 3 | 0.488885 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 4 | 0.486395 | `foundry_agents_list` | ❌ |
| 5 | 0.401016 | `search_service_list` | ❌ |
| 6 | 0.396422 | `foundry_knowledge_index_list` | ❌ |
| 7 | 0.328814 | `deploy_plan_get` | ❌ |
| 8 | 0.311234 | `foundry_knowledge_index_schema` | ❌ |
| 9 | 0.305997 | `deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.301476 | `deploy_app_logs_get` | ❌ |
| 11 | 0.298821 | `search_index_query` | ❌ |
| 12 | 0.291256 | `search_index_get` | ❌ |
| 13 | 0.286814 | `grafana_list` | ❌ |
| 14 | 0.269912 | `mysql_server_list` | ❌ |
| 15 | 0.254926 | `postgres_server_list` | ❌ |
| 16 | 0.250392 | `redis_cluster_list` | ❌ |
| 17 | 0.246893 | `quota_region_availability_list` | ❌ |
| 18 | 0.243133 | `monitor_table_type_list` | ❌ |
| 19 | 0.236572 | `mysql_database_list` | ❌ |
| 20 | 0.234179 | `redis_cache_list` | ❌ |

---

## Test 11

**Expected Tool:** `foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560022 | `foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.491952 | `foundry_agents_list` | ❌ |
| 3 | 0.401146 | `foundry_models_deploy` | ❌ |
| 4 | 0.387861 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.386180 | `search_service_list` | ❌ |
| 6 | 0.346909 | `foundry_models_deployments_list` | ❌ |
| 7 | 0.298648 | `monitor_table_type_list` | ❌ |
| 8 | 0.290552 | `foundry_knowledge_index_schema` | ❌ |
| 9 | 0.285437 | `postgres_table_list` | ❌ |
| 10 | 0.277883 | `grafana_list` | ❌ |
| 11 | 0.275316 | `search_index_get` | ❌ |
| 12 | 0.273026 | `monitor_table_list` | ❌ |
| 13 | 0.265730 | `azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.255790 | `mysql_server_list` | ❌ |
| 15 | 0.255760 | `search_index_query` | ❌ |
| 16 | 0.252297 | `postgres_database_list` | ❌ |
| 17 | 0.248741 | `redis_cache_list` | ❌ |
| 18 | 0.248405 | `mysql_table_list` | ❌ |
| 19 | 0.245193 | `datadog_monitoredresources_list` | ❌ |
| 20 | 0.235676 | `loadtesting_testrun_list` | ❌ |

---

## Test 12

**Expected Tool:** `foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574818 | `foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.475139 | `foundry_agents_list` | ❌ |
| 3 | 0.430513 | `foundry_models_deploy` | ❌ |
| 4 | 0.388967 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.356899 | `foundry_models_deployments_list` | ❌ |
| 6 | 0.339069 | `search_service_list` | ❌ |
| 7 | 0.299212 | `foundry_knowledge_index_schema` | ❌ |
| 8 | 0.283250 | `search_index_query` | ❌ |
| 9 | 0.280061 | `foundry_agents_connect` | ❌ |
| 10 | 0.274019 | `cloudarchitect_design` | ❌ |
| 11 | 0.266937 | `deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.261834 | `search_index_get` | ❌ |
| 13 | 0.260144 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.245943 | `quota_region_availability_list` | ❌ |
| 15 | 0.244697 | `monitor_table_type_list` | ❌ |
| 16 | 0.240274 | `monitor_metrics_definitions` | ❌ |
| 17 | 0.234050 | `mysql_server_list` | ❌ |
| 18 | 0.217331 | `marketplace_product_list` | ❌ |
| 19 | 0.211456 | `mysql_database_list` | ❌ |
| 20 | 0.207870 | `marketplace_product_get` | ❌ |

---

## Test 13

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681052 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544460 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.490624 | `search_service_list` | ❌ |
| 4 | 0.466005 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.459609 | `search_index_query` | ❌ |
| 6 | 0.393556 | `aks_cluster_get` | ❌ |
| 7 | 0.388240 | `loadtesting_testrun_get` | ❌ |
| 8 | 0.379706 | `keyvault_key_get` | ❌ |
| 9 | 0.372382 | `marketplace_product_get` | ❌ |
| 10 | 0.370915 | `mysql_table_schema_get` | ❌ |
| 11 | 0.358541 | `keyvault_secret_get` | ❌ |
| 12 | 0.358315 | `kusto_cluster_get` | ❌ |
| 13 | 0.356252 | `sql_db_show` | ❌ |
| 14 | 0.355785 | `storage_blob_get` | ❌ |
| 15 | 0.354845 | `storage_account_get` | ❌ |
| 16 | 0.353617 | `keyvault_certificate_get` | ❌ |
| 17 | 0.351685 | `storage_blob_container_get` | ❌ |
| 18 | 0.351083 | `sql_server_show` | ❌ |
| 19 | 0.348263 | `foundry_agents_list` | ❌ |
| 20 | 0.343186 | `aks_nodepool_get` | ❌ |

---

## Test 14

**Expected Tool:** `search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639545 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.620140 | `search_service_list` | ❌ |
| 3 | 0.561856 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.480817 | `search_index_query` | ❌ |
| 5 | 0.453047 | `foundry_agents_list` | ❌ |
| 6 | 0.445327 | `foundry_knowledge_index_schema` | ❌ |
| 7 | 0.439452 | `monitor_table_list` | ❌ |
| 8 | 0.417838 | `keyvault_key_list` | ❌ |
| 9 | 0.416404 | `cosmos_database_list` | ❌ |
| 10 | 0.412109 | `keyvault_certificate_list` | ❌ |
| 11 | 0.409307 | `cosmos_account_list` | ❌ |
| 12 | 0.406485 | `monitor_table_type_list` | ❌ |
| 13 | 0.397423 | `mysql_database_list` | ❌ |
| 14 | 0.387174 | `keyvault_secret_list` | ❌ |
| 15 | 0.378750 | `kusto_database_list` | ❌ |
| 16 | 0.378297 | `monitor_workspace_list` | ❌ |
| 17 | 0.375372 | `foundry_models_deployments_list` | ❌ |
| 18 | 0.371099 | `mysql_table_list` | ❌ |
| 19 | 0.367804 | `mysql_server_list` | ❌ |
| 20 | 0.367416 | `redis_cache_list` | ❌ |

---

## Test 15

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620268 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562775 | `search_service_list` | ❌ |
| 3 | 0.561154 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.471416 | `search_index_query` | ❌ |
| 5 | 0.463814 | `foundry_knowledge_index_schema` | ❌ |
| 6 | 0.408569 | `foundry_agents_list` | ❌ |
| 7 | 0.401807 | `monitor_table_list` | ❌ |
| 8 | 0.382692 | `monitor_table_type_list` | ❌ |
| 9 | 0.372639 | `cosmos_account_list` | ❌ |
| 10 | 0.370330 | `cosmos_database_list` | ❌ |
| 11 | 0.367868 | `mysql_database_list` | ❌ |
| 12 | 0.355910 | `keyvault_key_list` | ❌ |
| 13 | 0.351788 | `foundry_models_deployments_list` | ❌ |
| 14 | 0.351161 | `mysql_server_list` | ❌ |
| 15 | 0.350043 | `kusto_database_list` | ❌ |
| 16 | 0.349605 | `keyvault_certificate_list` | ❌ |
| 17 | 0.347566 | `monitor_workspace_list` | ❌ |
| 18 | 0.346994 | `mysql_table_list` | ❌ |
| 19 | 0.341728 | `foundry_models_list` | ❌ |
| 20 | 0.328039 | `redis_cluster_list` | ❌ |

---

## Test 16

**Expected Tool:** `search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522807 | `search_index_get` | ❌ |
| 2 | 0.515909 | `search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497441 | `search_service_list` | ❌ |
| 4 | 0.373932 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.372752 | `foundry_knowledge_index_schema` | ❌ |
| 6 | 0.327104 | `kusto_query` | ❌ |
| 7 | 0.322326 | `monitor_workspace_log_query` | ❌ |
| 8 | 0.311080 | `cosmos_database_container_item_query` | ❌ |
| 9 | 0.307299 | `resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.305893 | `marketplace_product_list` | ❌ |
| 11 | 0.295388 | `monitor_resource_log_query` | ❌ |
| 12 | 0.291381 | `foundry_agents_connect` | ❌ |
| 13 | 0.290223 | `monitor_metrics_query` | ❌ |
| 14 | 0.288238 | `foundry_models_deployments_list` | ❌ |
| 15 | 0.287504 | `mysql_server_list` | ❌ |
| 16 | 0.283561 | `foundry_models_list` | ❌ |
| 17 | 0.275029 | `foundry_agents_list` | ❌ |
| 18 | 0.269900 | `monitor_table_list` | ❌ |
| 19 | 0.259752 | `applicationinsights_recommendation_list` | ❌ |
| 20 | 0.244907 | `kusto_sample` | ❌ |

---

## Test 17

**Expected Tool:** `search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.520340 | `foundry_agents_list` | ❌ |
| 3 | 0.505971 | `search_index_get` | ❌ |
| 4 | 0.500497 | `redis_cache_list` | ❌ |
| 5 | 0.494272 | `monitor_workspace_list` | ❌ |
| 6 | 0.493011 | `redis_cluster_list` | ❌ |
| 7 | 0.492228 | `cosmos_account_list` | ❌ |
| 8 | 0.486066 | `postgres_server_list` | ❌ |
| 9 | 0.482464 | `grafana_list` | ❌ |
| 10 | 0.477471 | `subscription_list` | ❌ |
| 11 | 0.470384 | `kusto_cluster_list` | ❌ |
| 12 | 0.470055 | `marketplace_product_list` | ❌ |
| 13 | 0.454460 | `foundry_models_deployments_list` | ❌ |
| 14 | 0.451886 | `aks_cluster_list` | ❌ |
| 15 | 0.443495 | `search_index_query` | ❌ |
| 16 | 0.431621 | `eventgrid_subscription_list` | ❌ |
| 17 | 0.427923 | `group_list` | ❌ |
| 18 | 0.425463 | `resourcehealth_availability-status_list` | ❌ |
| 19 | 0.418007 | `eventgrid_topic_list` | ❌ |
| 20 | 0.417472 | `appconfig_account_list` | ❌ |

---

## Test 18

**Expected Tool:** `search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.479898 | `search_index_get` | ❌ |
| 3 | 0.467337 | `foundry_agents_list` | ❌ |
| 4 | 0.453489 | `marketplace_product_list` | ❌ |
| 5 | 0.448446 | `search_index_query` | ❌ |
| 6 | 0.425939 | `monitor_workspace_list` | ❌ |
| 7 | 0.419493 | `marketplace_product_get` | ❌ |
| 8 | 0.412158 | `cosmos_account_list` | ❌ |
| 9 | 0.408456 | `redis_cluster_list` | ❌ |
| 10 | 0.400284 | `redis_cache_list` | ❌ |
| 11 | 0.399822 | `grafana_list` | ❌ |
| 12 | 0.397883 | `foundry_models_deployments_list` | ❌ |
| 13 | 0.393708 | `subscription_list` | ❌ |
| 14 | 0.391071 | `resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.390559 | `foundry_models_list` | ❌ |
| 16 | 0.389433 | `eventgrid_subscription_list` | ❌ |
| 17 | 0.379805 | `foundry_knowledge_index_list` | ❌ |
| 18 | 0.376089 | `kusto_cluster_list` | ❌ |
| 19 | 0.373463 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.363429 | `aks_cluster_list` | ❌ |

---

## Test 19

**Expected Tool:** `search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553025 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.436230 | `search_index_get` | ❌ |
| 3 | 0.417096 | `foundry_agents_list` | ❌ |
| 4 | 0.404758 | `search_index_query` | ❌ |
| 5 | 0.344699 | `foundry_models_deployments_list` | ❌ |
| 6 | 0.336174 | `deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.322580 | `foundry_knowledge_index_list` | ❌ |
| 8 | 0.322540 | `foundry_models_list` | ❌ |
| 9 | 0.300427 | `marketplace_product_list` | ❌ |
| 10 | 0.292677 | `mysql_server_list` | ❌ |
| 11 | 0.290360 | `resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.290214 | `cosmos_account_list` | ❌ |
| 13 | 0.283366 | `redis_cluster_list` | ❌ |
| 14 | 0.282246 | `foundry_knowledge_index_schema` | ❌ |
| 15 | 0.281672 | `get_bestpractices_get` | ❌ |
| 16 | 0.281134 | `monitor_workspace_list` | ❌ |
| 17 | 0.278601 | `redis_cache_list` | ❌ |
| 18 | 0.278574 | `cloudarchitect_design` | ❌ |
| 19 | 0.277693 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.275013 | `sql_server_show` | ❌ |

---

## Test 20

**Expected Tool:** `appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.635561 | `appconfig_kv_list` | ❌ |
| 3 | 0.492173 | `redis_cache_list` | ❌ |
| 4 | 0.491380 | `postgres_server_list` | ❌ |
| 5 | 0.473554 | `redis_cluster_list` | ❌ |
| 6 | 0.442214 | `grafana_list` | ❌ |
| 7 | 0.441656 | `cosmos_account_list` | ❌ |
| 8 | 0.433594 | `eventgrid_topic_list` | ❌ |
| 9 | 0.432238 | `search_service_list` | ❌ |
| 10 | 0.427658 | `subscription_list` | ❌ |
| 11 | 0.427460 | `appconfig_kv_show` | ❌ |
| 12 | 0.423903 | `eventgrid_subscription_list` | ❌ |
| 13 | 0.420794 | `kusto_cluster_list` | ❌ |
| 14 | 0.412274 | `storage_account_get` | ❌ |
| 15 | 0.408599 | `monitor_workspace_list` | ❌ |
| 16 | 0.398507 | `aks_cluster_list` | ❌ |
| 17 | 0.389537 | `foundry_agents_list` | ❌ |
| 18 | 0.385938 | `sql_db_list` | ❌ |
| 19 | 0.380818 | `quota_region_availability_list` | ❌ |
| 20 | 0.370646 | `postgres_server_config_get` | ❌ |

---

## Test 21

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634978 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.533437 | `appconfig_kv_list` | ❌ |
| 3 | 0.425610 | `appconfig_kv_show` | ❌ |
| 4 | 0.372683 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.372456 | `postgres_server_list` | ❌ |
| 6 | 0.368753 | `redis_cache_list` | ❌ |
| 7 | 0.359567 | `postgres_server_config_get` | ❌ |
| 8 | 0.356514 | `redis_cluster_list` | ❌ |
| 9 | 0.355830 | `storage_account_get` | ❌ |
| 10 | 0.354747 | `appconfig_kv_delete` | ❌ |
| 11 | 0.348603 | `appconfig_kv_set` | ❌ |
| 12 | 0.344550 | `marketplace_product_get` | ❌ |
| 13 | 0.341263 | `grafana_list` | ❌ |
| 14 | 0.340731 | `eventgrid_topic_list` | ❌ |
| 15 | 0.332824 | `mysql_server_config_get` | ❌ |
| 16 | 0.325885 | `subscription_list` | ❌ |
| 17 | 0.325232 | `functionapp_get` | ❌ |
| 18 | 0.318639 | `azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.310432 | `search_service_list` | ❌ |
| 20 | 0.292788 | `monitor_workspace_list` | ❌ |

---

## Test 22

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565435 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.564705 | `appconfig_kv_list` | ❌ |
| 3 | 0.414689 | `appconfig_kv_show` | ❌ |
| 4 | 0.355916 | `postgres_server_config_get` | ❌ |
| 5 | 0.348661 | `appconfig_kv_delete` | ❌ |
| 6 | 0.327234 | `appconfig_kv_set` | ❌ |
| 7 | 0.289682 | `appconfig_kv_lock_set` | ❌ |
| 8 | 0.282153 | `mysql_server_config_get` | ❌ |
| 9 | 0.272373 | `storage_account_get` | ❌ |
| 10 | 0.255774 | `mysql_server_param_get` | ❌ |
| 11 | 0.250457 | `foundry_agents_list` | ❌ |
| 12 | 0.239130 | `loadtesting_testrun_list` | ❌ |
| 13 | 0.236387 | `deploy_app_logs_get` | ❌ |
| 14 | 0.234890 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.233357 | `postgres_server_list` | ❌ |
| 16 | 0.231688 | `redis_cache_list` | ❌ |
| 17 | 0.228042 | `mysql_server_param_set` | ❌ |
| 18 | 0.225851 | `sql_db_update` | ❌ |
| 19 | 0.221645 | `sql_server_show` | ❌ |
| 20 | 0.221405 | `postgres_database_list` | ❌ |

---

## Test 23

**Expected Tool:** `appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618277 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.486631 | `appconfig_kv_list` | ❌ |
| 3 | 0.424344 | `appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.399569 | `appconfig_kv_show` | ❌ |
| 6 | 0.392016 | `appconfig_account_list` | ❌ |
| 7 | 0.268822 | `workbooks_delete` | ❌ |
| 8 | 0.259907 | `keyvault_key_get` | ❌ |
| 9 | 0.251994 | `keyvault_key_create` | ❌ |
| 10 | 0.230464 | `keyvault_secret_create` | ❌ |
| 11 | 0.218487 | `mysql_server_param_set` | ❌ |
| 12 | 0.218373 | `sql_server_delete` | ❌ |
| 13 | 0.203182 | `appservice_database_add` | ❌ |
| 14 | 0.196121 | `sql_server_firewall-rule_delete` | ❌ |
| 15 | 0.194933 | `sql_db_delete` | ❌ |
| 16 | 0.194831 | `postgres_server_config_get` | ❌ |
| 17 | 0.183461 | `sql_db_update` | ❌ |
| 18 | 0.175403 | `mysql_server_config_get` | ❌ |
| 19 | 0.173143 | `postgres_server_param_set` | ❌ |
| 20 | 0.166763 | `storage_account_get` | ❌ |

---

## Test 24

**Expected Tool:** `appconfig_kv_list`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.730852 | `appconfig_kv_list` | ✅ **EXPECTED** |
| 2 | 0.595054 | `appconfig_kv_show` | ❌ |
| 3 | 0.557810 | `appconfig_account_list` | ❌ |
| 4 | 0.530884 | `appconfig_kv_set` | ❌ |
| 5 | 0.464635 | `appconfig_kv_delete` | ❌ |
| 6 | 0.439089 | `appconfig_kv_lock_set` | ❌ |
| 7 | 0.377534 | `postgres_server_config_get` | ❌ |
| 8 | 0.356156 | `keyvault_key_list` | ❌ |
| 9 | 0.333355 | `mysql_server_param_get` | ❌ |
| 10 | 0.327550 | `loadtesting_testrun_list` | ❌ |
| 11 | 0.323615 | `storage_account_get` | ❌ |
| 12 | 0.317744 | `mysql_server_config_get` | ❌ |
| 13 | 0.308699 | `keyvault_secret_list` | ❌ |
| 14 | 0.302700 | `loadtesting_test_get` | ❌ |
| 15 | 0.296084 | `postgres_table_list` | ❌ |
| 16 | 0.292127 | `redis_cache_list` | ❌ |
| 17 | 0.275469 | `mysql_server_param_set` | ❌ |
| 18 | 0.267026 | `postgres_database_list` | ❌ |
| 19 | 0.265694 | `sql_db_update` | ❌ |
| 20 | 0.264833 | `redis_cache_accesspolicy_list` | ❌ |

---

## Test 25

**Expected Tool:** `appconfig_kv_list`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682275 | `appconfig_kv_list` | ✅ **EXPECTED** |
| 2 | 0.606545 | `appconfig_kv_show` | ❌ |
| 3 | 0.522426 | `appconfig_account_list` | ❌ |
| 4 | 0.512945 | `appconfig_kv_set` | ❌ |
| 5 | 0.468503 | `appconfig_kv_delete` | ❌ |
| 6 | 0.457866 | `appconfig_kv_lock_set` | ❌ |
| 7 | 0.370520 | `postgres_server_config_get` | ❌ |
| 8 | 0.356793 | `mysql_server_param_get` | ❌ |
| 9 | 0.317662 | `mysql_server_config_get` | ❌ |
| 10 | 0.316093 | `keyvault_key_get` | ❌ |
| 11 | 0.314774 | `storage_account_get` | ❌ |
| 12 | 0.304557 | `loadtesting_test_get` | ❌ |
| 13 | 0.288088 | `mysql_server_param_set` | ❌ |
| 14 | 0.278909 | `loadtesting_testrun_list` | ❌ |
| 15 | 0.277848 | `keyvault_secret_get` | ❌ |
| 16 | 0.269354 | `sql_db_update` | ❌ |
| 17 | 0.258688 | `postgres_server_param_get` | ❌ |
| 18 | 0.249105 | `storage_blob_container_get` | ❌ |
| 19 | 0.243655 | `postgres_server_param_set` | ❌ |
| 20 | 0.238151 | `sql_server_show` | ❌ |

---

## Test 26

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591237 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.508804 | `appconfig_kv_list` | ❌ |
| 3 | 0.445551 | `appconfig_kv_set` | ❌ |
| 4 | 0.431516 | `appconfig_kv_delete` | ❌ |
| 5 | 0.423650 | `appconfig_kv_show` | ❌ |
| 6 | 0.373656 | `appconfig_account_list` | ❌ |
| 7 | 0.253705 | `mysql_server_param_set` | ❌ |
| 8 | 0.250821 | `keyvault_secret_create` | ❌ |
| 9 | 0.249335 | `keyvault_key_create` | ❌ |
| 10 | 0.247883 | `keyvault_key_get` | ❌ |
| 11 | 0.238242 | `postgres_server_param_set` | ❌ |
| 12 | 0.211331 | `postgres_server_config_get` | ❌ |
| 13 | 0.210593 | `appservice_database_add` | ❌ |
| 14 | 0.185346 | `sql_db_update` | ❌ |
| 15 | 0.163738 | `storage_account_get` | ❌ |
| 16 | 0.158946 | `mysql_server_param_get` | ❌ |
| 17 | 0.154529 | `postgres_server_param_get` | ❌ |
| 18 | 0.144377 | `servicebus_queue_details` | ❌ |
| 19 | 0.139871 | `mysql_server_config_get` | ❌ |
| 20 | 0.134175 | `loadtesting_testrun_update` | ❌ |

---

## Test 27

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555699 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.541557 | `appconfig_kv_list` | ❌ |
| 3 | 0.476496 | `appconfig_kv_delete` | ❌ |
| 4 | 0.435759 | `appconfig_kv_show` | ❌ |
| 5 | 0.425488 | `appconfig_kv_set` | ❌ |
| 6 | 0.409406 | `appconfig_account_list` | ❌ |
| 7 | 0.293442 | `keyvault_key_get` | ❌ |
| 8 | 0.262796 | `keyvault_key_create` | ❌ |
| 9 | 0.240190 | `keyvault_secret_create` | ❌ |
| 10 | 0.237098 | `mysql_server_param_set` | ❌ |
| 11 | 0.235691 | `keyvault_secret_get` | ❌ |
| 12 | 0.225350 | `postgres_server_config_get` | ❌ |
| 13 | 0.190554 | `sql_db_update` | ❌ |
| 14 | 0.190136 | `storage_account_get` | ❌ |
| 15 | 0.185141 | `postgres_server_param_set` | ❌ |
| 16 | 0.179797 | `mysql_server_param_get` | ❌ |
| 17 | 0.171375 | `mysql_server_config_get` | ❌ |
| 18 | 0.159767 | `postgres_server_param_get` | ❌ |
| 19 | 0.149672 | `storage_blob_container_get` | ❌ |
| 20 | 0.143564 | `servicebus_queue_details` | ❌ |

---

## Test 28

**Expected Tool:** `appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609635 | `appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536497 | `appconfig_kv_lock_set` | ❌ |
| 3 | 0.518499 | `appconfig_kv_list` | ❌ |
| 4 | 0.507170 | `appconfig_kv_show` | ❌ |
| 5 | 0.505571 | `appconfig_kv_delete` | ❌ |
| 6 | 0.377919 | `appconfig_account_list` | ❌ |
| 7 | 0.360015 | `mysql_server_param_set` | ❌ |
| 8 | 0.346927 | `postgres_server_param_set` | ❌ |
| 9 | 0.330526 | `keyvault_secret_create` | ❌ |
| 10 | 0.287544 | `keyvault_key_create` | ❌ |
| 11 | 0.276094 | `appservice_database_add` | ❌ |
| 12 | 0.262903 | `sql_db_update` | ❌ |
| 13 | 0.258726 | `keyvault_key_get` | ❌ |
| 14 | 0.213592 | `mysql_server_param_get` | ❌ |
| 15 | 0.208947 | `postgres_server_config_get` | ❌ |
| 16 | 0.201882 | `loadtesting_testrun_update` | ❌ |
| 17 | 0.193989 | `storage_account_get` | ❌ |
| 18 | 0.167006 | `postgres_server_param_get` | ❌ |
| 19 | 0.164376 | `mysql_server_config_get` | ❌ |
| 20 | 0.137964 | `storage_account_create` | ❌ |

---

## Test 29

**Expected Tool:** `appconfig_kv_show`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603216 | `appconfig_kv_list` | ❌ |
| 2 | 0.561508 | `appconfig_kv_show` | ✅ **EXPECTED** |
| 3 | 0.448912 | `appconfig_kv_set` | ❌ |
| 4 | 0.441713 | `appconfig_kv_delete` | ❌ |
| 5 | 0.437432 | `appconfig_account_list` | ❌ |
| 6 | 0.416264 | `appconfig_kv_lock_set` | ❌ |
| 7 | 0.360113 | `keyvault_key_get` | ❌ |
| 8 | 0.315208 | `keyvault_secret_get` | ❌ |
| 9 | 0.291448 | `postgres_server_config_get` | ❌ |
| 10 | 0.269387 | `loadtesting_test_get` | ❌ |
| 11 | 0.259549 | `storage_account_get` | ❌ |
| 12 | 0.257940 | `mysql_server_param_get` | ❌ |
| 13 | 0.251822 | `loadtesting_testrun_list` | ❌ |
| 14 | 0.229242 | `mysql_server_config_get` | ❌ |
| 15 | 0.225141 | `storage_blob_container_get` | ❌ |
| 16 | 0.217856 | `postgres_server_param_get` | ❌ |
| 17 | 0.206445 | `redis_cache_list` | ❌ |
| 18 | 0.201872 | `mysql_server_param_set` | ❌ |
| 19 | 0.186734 | `sql_server_show` | ❌ |
| 20 | 0.185986 | `redis_cache_accesspolicy_list` | ❌ |

---

## Test 30

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.355622 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.329354 | `deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.257790 | `cloudarchitect_design` | ❌ |
| 5 | 0.216077 | `get_bestpractices_get` | ❌ |
| 6 | 0.206477 | `deploy_plan_get` | ❌ |
| 7 | 0.205235 | `loadtesting_testrun_get` | ❌ |
| 8 | 0.193032 | `applicationinsights_recommendation_list` | ❌ |
| 9 | 0.181894 | `foundry_agents_evaluate` | ❌ |
| 10 | 0.177942 | `deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.169553 | `quota_usage_check` | ❌ |
| 12 | 0.163658 | `resourcehealth_availability-status_get` | ❌ |
| 13 | 0.148018 | `mysql_database_query` | ❌ |
| 14 | 0.141970 | `monitor_resource_log_query` | ❌ |
| 15 | 0.132884 | `resourcehealth_service-health-events_list` | ❌ |
| 16 | 0.128768 | `resourcehealth_availability-status_list` | ❌ |
| 17 | 0.125631 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.120066 | `mysql_table_schema_get` | ❌ |
| 19 | 0.116209 | `mysql_server_list` | ❌ |
| 20 | 0.111783 | `redis_cache_list` | ❌ |

---

## Test 31

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.318582 | `deploy_app_logs_get` | ❌ |
| 2 | 0.302501 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.255570 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.225972 | `quota_usage_check` | ❌ |
| 5 | 0.222234 | `loadtesting_testrun_get` | ❌ |
| 6 | 0.200402 | `cloudarchitect_design` | ❌ |
| 7 | 0.199366 | `applicationinsights_recommendation_list` | ❌ |
| 8 | 0.186927 | `functionapp_get` | ❌ |
| 9 | 0.172691 | `get_bestpractices_get` | ❌ |
| 10 | 0.163364 | `azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.162857 | `foundry_agents_evaluate` | ❌ |
| 12 | 0.150964 | `monitor_resource_log_query` | ❌ |
| 13 | 0.150313 | `mysql_database_query` | ❌ |
| 14 | 0.144054 | `mysql_server_param_get` | ❌ |
| 15 | 0.132993 | `resourcehealth_availability-status_get` | ❌ |
| 16 | 0.125889 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 17 | 0.118881 | `mysql_table_schema_get` | ❌ |
| 18 | 0.112992 | `monitor_workspace_log_query` | ❌ |
| 19 | 0.107063 | `resourcehealth_availability-status_list` | ❌ |
| 20 | 0.101787 | `monitor_metrics_query` | ❌ |

---

## Test 32

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.256325 | `deploy_architecture_diagram_generate` | ❌ |
| 2 | 0.250432 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 3 | 0.215848 | `deploy_app_logs_get` | ❌ |
| 4 | 0.198999 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.188245 | `cloudarchitect_design` | ❌ |
| 6 | 0.188050 | `appservice_database_add` | ❌ |
| 7 | 0.179320 | `functionapp_get` | ❌ |
| 8 | 0.178879 | `azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.159064 | `get_bestpractices_get` | ❌ |
| 10 | 0.158352 | `deploy_plan_get` | ❌ |
| 11 | 0.156599 | `search_service_list` | ❌ |
| 12 | 0.156560 | `resourcehealth_service-health-events_list` | ❌ |
| 13 | 0.153379 | `resourcehealth_availability-status_list` | ❌ |
| 14 | 0.151702 | `appconfig_kv_show` | ❌ |
| 15 | 0.146689 | `quota_usage_check` | ❌ |
| 16 | 0.139604 | `postgres_server_param_get` | ❌ |
| 17 | 0.130326 | `sql_server_show` | ❌ |
| 18 | 0.129424 | `mysql_server_param_get` | ❌ |
| 19 | 0.126169 | `search_index_get` | ❌ |
| 20 | 0.125614 | `marketplace_product_get` | ❌ |

---

## Test 33

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add a database connection to my app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729119 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.398617 | `sql_db_create` | ❌ |
| 3 | 0.368252 | `sql_db_list` | ❌ |
| 4 | 0.364437 | `mysql_server_list` | ❌ |
| 5 | 0.361951 | `sql_db_show` | ❌ |
| 6 | 0.353953 | `sql_server_list` | ❌ |
| 7 | 0.348774 | `sql_server_create` | ❌ |
| 8 | 0.342556 | `functionapp_get` | ❌ |
| 9 | 0.342352 | `sql_db_update` | ❌ |
| 10 | 0.334383 | `sql_server_delete` | ❌ |
| 11 | 0.301680 | `storage_account_create` | ❌ |
| 12 | 0.300846 | `mysql_database_list` | ❌ |
| 13 | 0.298638 | `appconfig_kv_set` | ❌ |
| 14 | 0.286125 | `cosmos_database_list` | ❌ |
| 15 | 0.281814 | `loadtesting_testresource_create` | ❌ |
| 16 | 0.280123 | `datadog_monitoredresources_list` | ❌ |
| 17 | 0.266282 | `deploy_app_logs_get` | ❌ |
| 18 | 0.264904 | `kusto_database_list` | ❌ |
| 19 | 0.256870 | `keyvault_secret_create` | ❌ |
| 20 | 0.254975 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 34

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure a SQL Server database for app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612148 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.484312 | `sql_db_update` | ❌ |
| 3 | 0.471103 | `sql_db_create` | ❌ |
| 4 | 0.408878 | `sql_server_show` | ❌ |
| 5 | 0.405300 | `sql_db_list` | ❌ |
| 6 | 0.389144 | `sql_db_show` | ❌ |
| 7 | 0.381822 | `mysql_server_config_get` | ❌ |
| 8 | 0.367325 | `sql_server_delete` | ❌ |
| 9 | 0.366390 | `sql_server_create` | ❌ |
| 10 | 0.355360 | `sql_server_list` | ❌ |
| 11 | 0.352382 | `deploy_plan_get` | ❌ |
| 12 | 0.350677 | `deploy_architecture_diagram_generate` | ❌ |
| 13 | 0.345345 | `sql_db_delete` | ❌ |
| 14 | 0.340399 | `appconfig_kv_set` | ❌ |
| 15 | 0.329197 | `deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.322825 | `functionapp_get` | ❌ |
| 17 | 0.316005 | `deploy_app_logs_get` | ❌ |
| 18 | 0.304744 | `loadtesting_test_create` | ❌ |
| 19 | 0.299644 | `cosmos_database_list` | ❌ |
| 20 | 0.295124 | `appconfig_kv_show` | ❌ |

---

## Test 35

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add a MySQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648502 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.418902 | `sql_db_create` | ❌ |
| 3 | 0.409593 | `mysql_database_list` | ❌ |
| 4 | 0.382602 | `mysql_server_list` | ❌ |
| 5 | 0.351839 | `mysql_table_list` | ❌ |
| 6 | 0.345957 | `sql_db_update` | ❌ |
| 7 | 0.344869 | `mysql_table_schema_get` | ❌ |
| 8 | 0.335323 | `sql_db_list` | ❌ |
| 9 | 0.323158 | `mysql_database_query` | ❌ |
| 10 | 0.320639 | `cosmos_database_list` | ❌ |
| 11 | 0.314492 | `mysql_server_param_set` | ❌ |
| 12 | 0.311349 | `sql_db_show` | ❌ |
| 13 | 0.297738 | `appconfig_kv_set` | ❌ |
| 14 | 0.295428 | `kusto_database_list` | ❌ |
| 15 | 0.279702 | `deploy_app_logs_get` | ❌ |
| 16 | 0.272652 | `kusto_table_list` | ❌ |
| 17 | 0.272634 | `deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.269892 | `deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.269785 | `cosmos_database_container_list` | ❌ |
| 20 | 0.260632 | `functionapp_get` | ❌ |

---

## Test 36

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add a PostgreSQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579532 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449085 | `postgres_database_list` | ❌ |
| 3 | 0.439679 | `postgres_database_query` | ❌ |
| 4 | 0.409515 | `postgres_table_list` | ❌ |
| 5 | 0.405431 | `postgres_server_list` | ❌ |
| 6 | 0.399782 | `postgres_server_param_set` | ❌ |
| 7 | 0.383413 | `sql_db_create` | ❌ |
| 8 | 0.337005 | `postgres_table_schema_get` | ❌ |
| 9 | 0.328855 | `postgres_server_param_get` | ❌ |
| 10 | 0.305507 | `sql_db_update` | ❌ |
| 11 | 0.302980 | `sql_db_list` | ❌ |
| 12 | 0.289343 | `cosmos_database_list` | ❌ |
| 13 | 0.279654 | `kusto_database_list` | ❌ |
| 14 | 0.258603 | `appconfig_kv_set` | ❌ |
| 15 | 0.257729 | `deploy_app_logs_get` | ❌ |
| 16 | 0.254307 | `kusto_table_list` | ❌ |
| 17 | 0.241522 | `deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.233707 | `deploy_plan_get` | ❌ |
| 19 | 0.231783 | `deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.223353 | `cosmos_database_container_list` | ❌ |

---

## Test 37

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add a CosmosDB database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643070 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.477031 | `cosmos_database_list` | ❌ |
| 3 | 0.465637 | `sql_db_create` | ❌ |
| 4 | 0.421268 | `cosmos_database_container_list` | ❌ |
| 5 | 0.400458 | `sql_db_update` | ❌ |
| 6 | 0.378402 | `sql_db_list` | ❌ |
| 7 | 0.374251 | `cosmos_account_list` | ❌ |
| 8 | 0.370137 | `kusto_database_list` | ❌ |
| 9 | 0.362494 | `sql_db_show` | ❌ |
| 10 | 0.353056 | `cosmos_database_container_item_query` | ❌ |
| 11 | 0.352381 | `kusto_table_list` | ❌ |
| 12 | 0.349533 | `mysql_database_list` | ❌ |
| 13 | 0.326631 | `sql_db_delete` | ❌ |
| 14 | 0.325004 | `appconfig_kv_set` | ❌ |
| 15 | 0.314834 | `functionapp_get` | ❌ |
| 16 | 0.314554 | `sql_server_delete` | ❌ |
| 17 | 0.314391 | `mysql_server_list` | ❌ |
| 18 | 0.309146 | `redis_cluster_database_list` | ❌ |
| 19 | 0.303278 | `deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.292770 | `sql_server_create` | ❌ |

---

## Test 38

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645562 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.489228 | `sql_db_create` | ❌ |
| 3 | 0.423910 | `mysql_database_list` | ❌ |
| 4 | 0.422266 | `sql_db_list` | ❌ |
| 5 | 0.394910 | `sql_db_show` | ❌ |
| 6 | 0.394433 | `cosmos_database_list` | ❌ |
| 7 | 0.381822 | `sql_server_delete` | ❌ |
| 8 | 0.368592 | `postgres_database_list` | ❌ |
| 9 | 0.360144 | `kusto_database_list` | ❌ |
| 10 | 0.357354 | `sql_server_create` | ❌ |
| 11 | 0.349820 | `mysql_server_list` | ❌ |
| 12 | 0.348615 | `sql_db_update` | ❌ |
| 13 | 0.348100 | `kusto_table_list` | ❌ |
| 14 | 0.346009 | `sql_db_delete` | ❌ |
| 15 | 0.304416 | `cosmos_database_container_list` | ❌ |
| 16 | 0.281301 | `functionapp_get` | ❌ |
| 17 | 0.277310 | `kusto_table_schema` | ❌ |
| 18 | 0.274848 | `deploy_architecture_diagram_generate` | ❌ |
| 19 | 0.274590 | `appconfig_kv_set` | ❌ |
| 20 | 0.266423 | `deploy_app_logs_get` | ❌ |

---

## Test 39

**Expected Tool:** `appservice_database_add`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665268 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.371277 | `sql_db_update` | ❌ |
| 3 | 0.369071 | `sql_db_create` | ❌ |
| 4 | 0.332119 | `appconfig_kv_set` | ❌ |
| 5 | 0.314270 | `cosmos_database_list` | ❌ |
| 6 | 0.312395 | `sql_db_show` | ❌ |
| 7 | 0.307420 | `sql_db_list` | ❌ |
| 8 | 0.304622 | `mysql_database_list` | ❌ |
| 9 | 0.297194 | `mysql_server_param_get` | ❌ |
| 10 | 0.294182 | `kusto_database_list` | ❌ |
| 11 | 0.292606 | `kusto_table_list` | ❌ |
| 12 | 0.286149 | `postgres_server_param_set` | ❌ |
| 13 | 0.273579 | `cosmos_database_container_list` | ❌ |
| 14 | 0.269033 | `appconfig_kv_show` | ❌ |
| 15 | 0.268113 | `keyvault_secret_create` | ❌ |
| 16 | 0.267621 | `sql_server_show` | ❌ |
| 17 | 0.267098 | `mysql_server_param_set` | ❌ |
| 18 | 0.266587 | `cosmos_database_container_item_query` | ❌ |
| 19 | 0.265629 | `sql_db_delete` | ❌ |
| 20 | 0.260212 | `functionapp_get` | ❌ |

---

## Test 40

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536758 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.394572 | `sql_db_create` | ❌ |
| 3 | 0.391857 | `sql_db_update` | ❌ |
| 4 | 0.329110 | `keyvault_secret_create` | ❌ |
| 5 | 0.318461 | `appconfig_kv_set` | ❌ |
| 6 | 0.318263 | `sql_db_show` | ❌ |
| 7 | 0.305550 | `deploy_plan_get` | ❌ |
| 8 | 0.301240 | `mysql_table_list` | ❌ |
| 9 | 0.298453 | `sql_db_list` | ❌ |
| 10 | 0.298122 | `cosmos_database_list` | ❌ |
| 11 | 0.297607 | `mysql_database_list` | ❌ |
| 12 | 0.295901 | `subscription_list` | ❌ |
| 13 | 0.294831 | `deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.290182 | `kusto_table_list` | ❌ |
| 15 | 0.280891 | `cosmos_database_container_list` | ❌ |
| 16 | 0.274754 | `sql_db_delete` | ❌ |
| 17 | 0.274380 | `sql_server_delete` | ❌ |
| 18 | 0.273430 | `functionapp_get` | ❌ |
| 19 | 0.272238 | `kusto_table_schema` | ❌ |
| 20 | 0.267064 | `mysql_server_list` | ❌ |

---

## Test 41

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560288 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.426753 | `sql_db_create` | ❌ |
| 3 | 0.361028 | `cosmos_database_list` | ❌ |
| 4 | 0.349556 | `mysql_database_list` | ❌ |
| 5 | 0.346672 | `sql_db_list` | ❌ |
| 6 | 0.345158 | `sql_db_update` | ❌ |
| 7 | 0.342276 | `kusto_database_list` | ❌ |
| 8 | 0.339789 | `sql_db_delete` | ❌ |
| 9 | 0.339459 | `sql_db_show` | ❌ |
| 10 | 0.330944 | `redis_cluster_database_list` | ❌ |
| 11 | 0.317003 | `kusto_table_list` | ❌ |
| 12 | 0.292346 | `sql_server_delete` | ❌ |
| 13 | 0.281774 | `mysql_server_list` | ❌ |
| 14 | 0.277068 | `deploy_app_logs_get` | ❌ |
| 15 | 0.270334 | `kusto_table_schema` | ❌ |
| 16 | 0.268258 | `cosmos_database_container_list` | ❌ |
| 17 | 0.263797 | `functionapp_get` | ❌ |
| 18 | 0.257394 | `mysql_table_list` | ❌ |
| 19 | 0.257248 | `deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.253565 | `deploy_plan_get` | ❌ |

---

## Test 42

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572473 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.445157 | `get_bestpractices_get` | ❌ |
| 3 | 0.390478 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.385409 | `applens_resource_diagnose` | ❌ |
| 5 | 0.375293 | `deploy_iac_rules_get` | ❌ |
| 6 | 0.357934 | `deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.352902 | `foundry_agents_list` | ❌ |
| 8 | 0.346020 | `deploy_plan_get` | ❌ |
| 9 | 0.344858 | `cloudarchitect_design` | ❌ |
| 10 | 0.330014 | `search_service_list` | ❌ |
| 11 | 0.326102 | `deploy_app_logs_get` | ❌ |
| 12 | 0.297036 | `resourcehealth_availability-status_list` | ❌ |
| 13 | 0.296190 | `quota_usage_check` | ❌ |
| 14 | 0.268844 | `quota_region_availability_list` | ❌ |
| 15 | 0.265955 | `monitor_metrics_definitions` | ❌ |
| 16 | 0.263811 | `monitor_workspace_list` | ❌ |
| 17 | 0.260352 | `mysql_server_list` | ❌ |
| 18 | 0.258617 | `monitor_table_list` | ❌ |
| 19 | 0.248483 | `search_index_get` | ❌ |
| 20 | 0.245697 | `redis_cache_list` | ❌ |

---

## Test 43

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696531 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.468384 | `get_bestpractices_get` | ❌ |
| 3 | 0.452173 | `applens_resource_diagnose` | ❌ |
| 4 | 0.435241 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.424622 | `search_service_list` | ❌ |
| 6 | 0.405520 | `deploy_iac_rules_get` | ❌ |
| 7 | 0.405253 | `deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.401105 | `quota_usage_check` | ❌ |
| 9 | 0.393786 | `resourcehealth_availability-status_list` | ❌ |
| 10 | 0.387892 | `deploy_plan_get` | ❌ |
| 11 | 0.380224 | `foundry_agents_list` | ❌ |
| 12 | 0.371654 | `redis_cache_list` | ❌ |
| 13 | 0.367714 | `deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.367243 | `quota_region_availability_list` | ❌ |
| 15 | 0.362902 | `deploy_app_logs_get` | ❌ |
| 16 | 0.355398 | `redis_cluster_list` | ❌ |
| 17 | 0.339417 | `monitor_workspace_list` | ❌ |
| 18 | 0.336797 | `monitor_metrics_query` | ❌ |
| 19 | 0.334552 | `monitor_resource_log_query` | ❌ |
| 20 | 0.332071 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 44

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.479392 | `mysql_server_list` | ❌ |
| 3 | 0.468847 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.467717 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.461695 | `foundry_agents_list` | ❌ |
| 6 | 0.451846 | `applens_resource_diagnose` | ❌ |
| 7 | 0.449821 | `sql_server_list` | ❌ |
| 8 | 0.446454 | `search_service_list` | ❌ |
| 9 | 0.419715 | `azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.417639 | `sql_db_list` | ❌ |
| 11 | 0.416057 | `get_bestpractices_get` | ❌ |
| 12 | 0.415557 | `monitor_metrics_definitions` | ❌ |
| 13 | 0.407374 | `loadtesting_testresource_list` | ❌ |
| 14 | 0.401177 | `monitor_metrics_query` | ❌ |
| 15 | 0.401135 | `workbooks_list` | ❌ |
| 16 | 0.398757 | `acr_registry_list` | ❌ |
| 17 | 0.389786 | `monitor_table_type_list` | ❌ |
| 18 | 0.388734 | `group_list` | ❌ |
| 19 | 0.386954 | `quota_usage_check` | ❌ |
| 20 | 0.385121 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 45

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.398321 | `applens_resource_diagnose` | ❌ |
| 3 | 0.383767 | `get_bestpractices_get` | ❌ |
| 4 | 0.369053 | `cloudarchitect_design` | ❌ |
| 5 | 0.367278 | `deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.341619 | `azureterraformbestpractices_get` | ❌ |
| 7 | 0.325789 | `deploy_iac_rules_get` | ❌ |
| 8 | 0.324496 | `deploy_app_logs_get` | ❌ |
| 9 | 0.321854 | `deploy_plan_get` | ❌ |
| 10 | 0.313589 | `deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.287677 | `monitor_metrics_query` | ❌ |
| 12 | 0.285234 | `quota_usage_check` | ❌ |
| 13 | 0.262685 | `resourcehealth_availability-status_get` | ❌ |
| 14 | 0.259246 | `search_service_list` | ❌ |
| 15 | 0.254871 | `search_index_query` | ❌ |
| 16 | 0.247378 | `resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.233938 | `resourcehealth_availability-status_list` | ❌ |
| 18 | 0.230227 | `monitor_workspace_log_query` | ❌ |
| 19 | 0.229476 | `mysql_server_config_get` | ❌ |
| 20 | 0.225298 | `monitor_resource_log_query` | ❌ |

---

## Test 46

**Expected Tool:** `acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743533 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `acr_registry_repository_list` | ❌ |
| 3 | 0.541506 | `search_service_list` | ❌ |
| 4 | 0.527511 | `aks_cluster_list` | ❌ |
| 5 | 0.515937 | `subscription_list` | ❌ |
| 6 | 0.514293 | `cosmos_account_list` | ❌ |
| 7 | 0.509386 | `monitor_workspace_list` | ❌ |
| 8 | 0.503032 | `kusto_cluster_list` | ❌ |
| 9 | 0.490776 | `appconfig_account_list` | ❌ |
| 10 | 0.487256 | `storage_blob_container_get` | ❌ |
| 11 | 0.483500 | `cosmos_database_container_list` | ❌ |
| 12 | 0.482236 | `redis_cluster_list` | ❌ |
| 13 | 0.481776 | `redis_cache_list` | ❌ |
| 14 | 0.480883 | `group_list` | ❌ |
| 15 | 0.469958 | `datadog_monitoredresources_list` | ❌ |
| 16 | 0.462353 | `quota_region_availability_list` | ❌ |
| 17 | 0.460523 | `sql_db_list` | ❌ |
| 18 | 0.460343 | `cosmos_database_list` | ❌ |
| 19 | 0.456503 | `mysql_server_list` | ❌ |
| 20 | 0.454170 | `virtualdesktop_hostpool_list` | ❌ |

---

## Test 47

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585968 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `acr_registry_repository_list` | ❌ |
| 3 | 0.449642 | `storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `cosmos_database_container_list` | ❌ |
| 5 | 0.382728 | `mysql_server_list` | ❌ |
| 6 | 0.373107 | `foundry_agents_list` | ❌ |
| 7 | 0.372153 | `mysql_database_list` | ❌ |
| 8 | 0.370858 | `azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.364918 | `search_service_list` | ❌ |
| 10 | 0.359160 | `deploy_app_logs_get` | ❌ |
| 11 | 0.356444 | `aks_cluster_list` | ❌ |
| 12 | 0.354277 | `storage_blob_container_create` | ❌ |
| 13 | 0.353379 | `subscription_list` | ❌ |
| 14 | 0.352818 | `storage_account_get` | ❌ |
| 15 | 0.349526 | `cosmos_database_list` | ❌ |
| 16 | 0.349291 | `sql_db_list` | ❌ |
| 17 | 0.347970 | `storage_blob_get` | ❌ |
| 18 | 0.344750 | `quota_usage_check` | ❌ |
| 19 | 0.344071 | `cosmos_account_list` | ❌ |
| 20 | 0.339252 | `appconfig_account_list` | ❌ |

---

## Test 48

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637158 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563476 | `acr_registry_repository_list` | ❌ |
| 3 | 0.474039 | `redis_cache_list` | ❌ |
| 4 | 0.471804 | `redis_cluster_list` | ❌ |
| 5 | 0.463742 | `postgres_server_list` | ❌ |
| 6 | 0.459880 | `search_service_list` | ❌ |
| 7 | 0.452938 | `kusto_cluster_list` | ❌ |
| 8 | 0.451253 | `monitor_workspace_list` | ❌ |
| 9 | 0.443939 | `appconfig_account_list` | ❌ |
| 10 | 0.440464 | `subscription_list` | ❌ |
| 11 | 0.435835 | `grafana_list` | ❌ |
| 12 | 0.435314 | `storage_blob_container_get` | ❌ |
| 13 | 0.431745 | `cosmos_database_container_list` | ❌ |
| 14 | 0.430961 | `aks_cluster_list` | ❌ |
| 15 | 0.430308 | `cosmos_account_list` | ❌ |
| 16 | 0.419749 | `eventgrid_subscription_list` | ❌ |
| 17 | 0.404718 | `group_list` | ❌ |
| 18 | 0.398556 | `quota_region_availability_list` | ❌ |
| 19 | 0.386495 | `virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.364214 | `mysql_server_list` | ❌ |

---

## Test 49

**Expected Tool:** `acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `acr_registry_repository_list` | ❌ |
| 2 | 0.633923 | `acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476015 | `mysql_server_list` | ❌ |
| 4 | 0.454931 | `group_list` | ❌ |
| 5 | 0.454003 | `datadog_monitoredresources_list` | ❌ |
| 6 | 0.446008 | `cosmos_database_container_list` | ❌ |
| 7 | 0.428000 | `workbooks_list` | ❌ |
| 8 | 0.423541 | `resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421030 | `azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.417327 | `sql_server_list` | ❌ |
| 11 | 0.411316 | `redis_cluster_list` | ❌ |
| 12 | 0.409133 | `sql_db_list` | ❌ |
| 13 | 0.403816 | `storage_blob_container_get` | ❌ |
| 14 | 0.388773 | `redis_cache_list` | ❌ |
| 15 | 0.378482 | `eventgrid_subscription_list` | ❌ |
| 16 | 0.371025 | `sql_elastic-pool_list` | ❌ |
| 17 | 0.370359 | `redis_cluster_database_list` | ❌ |
| 18 | 0.356119 | `kusto_cluster_list` | ❌ |
| 19 | 0.354145 | `cosmos_database_list` | ❌ |
| 20 | 0.352288 | `loadtesting_testresource_list` | ❌ |

---

## Test 50

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639357 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `acr_registry_repository_list` | ❌ |
| 3 | 0.468028 | `mysql_server_list` | ❌ |
| 4 | 0.449649 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.445716 | `group_list` | ❌ |
| 6 | 0.416353 | `cosmos_database_container_list` | ❌ |
| 7 | 0.413975 | `sql_db_list` | ❌ |
| 8 | 0.413191 | `sql_server_list` | ❌ |
| 9 | 0.406554 | `resourcehealth_availability-status_list` | ❌ |
| 10 | 0.403242 | `storage_blob_container_get` | ❌ |
| 11 | 0.400209 | `workbooks_list` | ❌ |
| 12 | 0.389603 | `azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.378353 | `redis_cluster_list` | ❌ |
| 14 | 0.369912 | `sql_elastic-pool_list` | ❌ |
| 15 | 0.369779 | `mysql_database_list` | ❌ |
| 16 | 0.367738 | `redis_cache_list` | ❌ |
| 17 | 0.355657 | `foundry_agents_list` | ❌ |
| 18 | 0.354772 | `loadtesting_testresource_list` | ❌ |
| 19 | 0.351411 | `cosmos_database_list` | ❌ |
| 20 | 0.347199 | `eventgrid_subscription_list` | ❌ |

---

## Test 51

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617537 | `acr_registry_list` | ❌ |
| 3 | 0.510459 | `redis_cache_list` | ❌ |
| 4 | 0.495567 | `postgres_server_list` | ❌ |
| 5 | 0.492550 | `redis_cluster_list` | ❌ |
| 6 | 0.475629 | `kusto_cluster_list` | ❌ |
| 7 | 0.466001 | `search_service_list` | ❌ |
| 8 | 0.461777 | `cosmos_database_container_list` | ❌ |
| 9 | 0.461369 | `grafana_list` | ❌ |
| 10 | 0.456838 | `appconfig_account_list` | ❌ |
| 11 | 0.449239 | `cosmos_account_list` | ❌ |
| 12 | 0.448228 | `monitor_workspace_list` | ❌ |
| 13 | 0.440083 | `subscription_list` | ❌ |
| 14 | 0.438302 | `aks_cluster_list` | ❌ |
| 15 | 0.437125 | `storage_blob_container_get` | ❌ |
| 16 | 0.431019 | `group_list` | ❌ |
| 17 | 0.414463 | `kusto_database_list` | ❌ |
| 18 | 0.405472 | `virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.390890 | `quota_region_availability_list` | ❌ |
| 20 | 0.377142 | `mysql_database_list` | ❌ |

---

## Test 52

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546333 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469284 | `acr_registry_list` | ❌ |
| 3 | 0.407973 | `cosmos_database_container_list` | ❌ |
| 4 | 0.399537 | `storage_blob_container_get` | ❌ |
| 5 | 0.339307 | `mysql_database_list` | ❌ |
| 6 | 0.326631 | `mysql_server_list` | ❌ |
| 7 | 0.308650 | `cosmos_database_list` | ❌ |
| 8 | 0.306819 | `foundry_agents_list` | ❌ |
| 9 | 0.306442 | `storage_blob_container_create` | ❌ |
| 10 | 0.302660 | `redis_cache_list` | ❌ |
| 11 | 0.300174 | `azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.295832 | `storage_blob_get` | ❌ |
| 13 | 0.292155 | `datadog_monitoredresources_list` | ❌ |
| 14 | 0.290148 | `redis_cluster_list` | ❌ |
| 15 | 0.289864 | `search_service_list` | ❌ |
| 16 | 0.283716 | `appconfig_account_list` | ❌ |
| 17 | 0.283390 | `kusto_database_list` | ❌ |
| 18 | 0.282581 | `sql_db_list` | ❌ |
| 19 | 0.276498 | `cosmos_account_list` | ❌ |
| 20 | 0.272964 | `redis_cluster_database_list` | ❌ |

---

## Test 53

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541785 | `acr_registry_list` | ❌ |
| 3 | 0.433927 | `cosmos_database_container_list` | ❌ |
| 4 | 0.387979 | `storage_blob_container_get` | ❌ |
| 5 | 0.370375 | `mysql_database_list` | ❌ |
| 6 | 0.359617 | `cosmos_database_list` | ❌ |
| 7 | 0.356901 | `mysql_server_list` | ❌ |
| 8 | 0.355380 | `redis_cache_list` | ❌ |
| 9 | 0.351007 | `redis_cluster_database_list` | ❌ |
| 10 | 0.347437 | `postgres_database_list` | ❌ |
| 11 | 0.347084 | `kusto_database_list` | ❌ |
| 12 | 0.340014 | `redis_cluster_list` | ❌ |
| 13 | 0.332785 | `datadog_monitoredresources_list` | ❌ |
| 14 | 0.332704 | `sql_db_list` | ❌ |
| 15 | 0.332572 | `monitor_workspace_list` | ❌ |
| 16 | 0.330046 | `kusto_cluster_list` | ❌ |
| 17 | 0.322287 | `mysql_table_list` | ❌ |
| 18 | 0.311530 | `azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.310929 | `cosmos_account_list` | ❌ |
| 20 | 0.309179 | `group_list` | ❌ |

---

## Test 54

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501852 | `acr_registry_list` | ❌ |
| 3 | 0.418623 | `cosmos_database_container_list` | ❌ |
| 4 | 0.374100 | `storage_blob_container_get` | ❌ |
| 5 | 0.359922 | `mysql_database_list` | ❌ |
| 6 | 0.341556 | `redis_cache_list` | ❌ |
| 7 | 0.335467 | `mysql_server_list` | ❌ |
| 8 | 0.333318 | `cosmos_database_list` | ❌ |
| 9 | 0.324104 | `redis_cluster_list` | ❌ |
| 10 | 0.318706 | `kusto_database_list` | ❌ |
| 11 | 0.316614 | `datadog_monitoredresources_list` | ❌ |
| 12 | 0.315414 | `redis_cluster_database_list` | ❌ |
| 13 | 0.311692 | `monitor_workspace_list` | ❌ |
| 14 | 0.309627 | `search_service_list` | ❌ |
| 15 | 0.306052 | `sql_db_list` | ❌ |
| 16 | 0.303931 | `kusto_cluster_list` | ❌ |
| 17 | 0.302428 | `foundry_agents_list` | ❌ |
| 18 | 0.300101 | `cosmos_account_list` | ❌ |
| 19 | 0.299629 | `appconfig_account_list` | ❌ |
| 20 | 0.299303 | `mysql_table_list` | ❌ |

---

## Test 55

**Expected Tool:** `cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668480 | `cosmos_database_list` | ❌ |
| 3 | 0.615268 | `cosmos_database_container_list` | ❌ |
| 4 | 0.587691 | `subscription_list` | ❌ |
| 5 | 0.560795 | `search_service_list` | ❌ |
| 6 | 0.538321 | `storage_account_get` | ❌ |
| 7 | 0.528963 | `monitor_workspace_list` | ❌ |
| 8 | 0.516914 | `kusto_cluster_list` | ❌ |
| 9 | 0.502428 | `kusto_database_list` | ❌ |
| 10 | 0.502199 | `redis_cluster_list` | ❌ |
| 11 | 0.499146 | `redis_cache_list` | ❌ |
| 12 | 0.497679 | `appconfig_account_list` | ❌ |
| 13 | 0.487067 | `group_list` | ❌ |
| 14 | 0.483046 | `grafana_list` | ❌ |
| 15 | 0.474934 | `postgres_server_list` | ❌ |
| 16 | 0.473658 | `aks_cluster_list` | ❌ |
| 17 | 0.460181 | `foundry_agents_list` | ❌ |
| 18 | 0.459502 | `sql_db_list` | ❌ |
| 19 | 0.459002 | `mysql_database_list` | ❌ |
| 20 | 0.453975 | `virtualdesktop_hostpool_list` | ❌ |

---

## Test 56

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665447 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605357 | `cosmos_database_list` | ❌ |
| 3 | 0.571613 | `cosmos_database_container_list` | ❌ |
| 4 | 0.486033 | `storage_account_get` | ❌ |
| 5 | 0.436283 | `subscription_list` | ❌ |
| 6 | 0.431496 | `cosmos_database_container_item_query` | ❌ |
| 7 | 0.427967 | `storage_blob_container_get` | ❌ |
| 8 | 0.427709 | `mysql_database_list` | ❌ |
| 9 | 0.408659 | `search_service_list` | ❌ |
| 10 | 0.405748 | `foundry_agents_list` | ❌ |
| 11 | 0.397574 | `azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.390141 | `kusto_database_list` | ❌ |
| 13 | 0.389842 | `mysql_server_list` | ❌ |
| 14 | 0.386108 | `monitor_workspace_list` | ❌ |
| 15 | 0.383985 | `appconfig_account_list` | ❌ |
| 16 | 0.381323 | `sql_db_list` | ❌ |
| 17 | 0.379496 | `appconfig_kv_show` | ❌ |
| 18 | 0.373667 | `redis_cluster_list` | ❌ |
| 19 | 0.367942 | `quota_usage_check` | ❌ |
| 20 | 0.348358 | `datadog_monitoredresources_list` | ❌ |

---

## Test 57

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605125 | `cosmos_database_list` | ❌ |
| 3 | 0.566249 | `cosmos_database_container_list` | ❌ |
| 4 | 0.546327 | `subscription_list` | ❌ |
| 5 | 0.530175 | `storage_account_get` | ❌ |
| 6 | 0.527812 | `search_service_list` | ❌ |
| 7 | 0.488006 | `monitor_workspace_list` | ❌ |
| 8 | 0.466414 | `redis_cluster_list` | ❌ |
| 9 | 0.457584 | `appconfig_account_list` | ❌ |
| 10 | 0.456205 | `redis_cache_list` | ❌ |
| 11 | 0.455017 | `kusto_cluster_list` | ❌ |
| 12 | 0.453626 | `kusto_database_list` | ❌ |
| 13 | 0.441136 | `grafana_list` | ❌ |
| 14 | 0.438277 | `cosmos_database_container_item_query` | ❌ |
| 15 | 0.437501 | `storage_blob_container_get` | ❌ |
| 16 | 0.437026 | `azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.434623 | `mysql_database_list` | ❌ |
| 18 | 0.433094 | `postgres_server_list` | ❌ |
| 19 | 0.430352 | `aks_cluster_list` | ❌ |
| 20 | 0.426516 | `sql_db_list` | ❌ |

---

## Test 58

**Expected Tool:** `cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.605253 | `cosmos_database_container_list` | ❌ |
| 2 | 0.566854 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 3 | 0.477874 | `cosmos_database_list` | ❌ |
| 4 | 0.447757 | `cosmos_account_list` | ❌ |
| 5 | 0.444883 | `storage_blob_container_get` | ❌ |
| 6 | 0.429363 | `search_service_list` | ❌ |
| 7 | 0.399756 | `search_index_query` | ❌ |
| 8 | 0.378151 | `kusto_query` | ❌ |
| 9 | 0.374844 | `mysql_table_list` | ❌ |
| 10 | 0.372689 | `mysql_database_list` | ❌ |
| 11 | 0.366503 | `search_index_get` | ❌ |
| 12 | 0.358903 | `mysql_server_list` | ❌ |
| 13 | 0.351331 | `kusto_table_list` | ❌ |
| 14 | 0.340982 | `monitor_table_list` | ❌ |
| 15 | 0.337570 | `storage_blob_get` | ❌ |
| 16 | 0.335256 | `sql_db_list` | ❌ |
| 17 | 0.334389 | `kusto_database_list` | ❌ |
| 18 | 0.331041 | `kusto_sample` | ❌ |
| 19 | 0.308694 | `acr_registry_repository_list` | ❌ |
| 20 | 0.302962 | `appconfig_kv_show` | ❌ |

---

## Test 59

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852710 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.680955 | `cosmos_database_list` | ❌ |
| 3 | 0.630682 | `cosmos_account_list` | ❌ |
| 4 | 0.581368 | `storage_blob_container_get` | ❌ |
| 5 | 0.527400 | `cosmos_database_container_item_query` | ❌ |
| 6 | 0.486392 | `mysql_database_list` | ❌ |
| 7 | 0.448929 | `kusto_database_list` | ❌ |
| 8 | 0.447530 | `mysql_table_list` | ❌ |
| 9 | 0.439792 | `sql_db_list` | ❌ |
| 10 | 0.427503 | `kusto_table_list` | ❌ |
| 11 | 0.424152 | `redis_cluster_database_list` | ❌ |
| 12 | 0.422240 | `mysql_server_list` | ❌ |
| 13 | 0.421485 | `acr_registry_repository_list` | ❌ |
| 14 | 0.420401 | `storage_account_get` | ❌ |
| 15 | 0.411588 | `monitor_table_list` | ❌ |
| 16 | 0.392845 | `postgres_database_list` | ❌ |
| 17 | 0.386419 | `storage_blob_get` | ❌ |
| 18 | 0.383532 | `foundry_agents_list` | ❌ |
| 19 | 0.374596 | `keyvault_certificate_list` | ❌ |
| 20 | 0.372078 | `kusto_cluster_list` | ❌ |

---

## Test 60

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789435 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.614210 | `cosmos_database_list` | ❌ |
| 3 | 0.561934 | `cosmos_account_list` | ❌ |
| 4 | 0.537005 | `storage_blob_container_get` | ❌ |
| 5 | 0.521482 | `cosmos_database_container_item_query` | ❌ |
| 6 | 0.449229 | `mysql_database_list` | ❌ |
| 7 | 0.411797 | `mysql_table_list` | ❌ |
| 8 | 0.397996 | `kusto_database_list` | ❌ |
| 9 | 0.397808 | `storage_account_get` | ❌ |
| 10 | 0.397755 | `sql_db_list` | ❌ |
| 11 | 0.395455 | `kusto_table_list` | ❌ |
| 12 | 0.392770 | `mysql_server_list` | ❌ |
| 13 | 0.386754 | `redis_cluster_database_list` | ❌ |
| 14 | 0.356008 | `storage_blob_get` | ❌ |
| 15 | 0.355518 | `acr_registry_repository_list` | ❌ |
| 16 | 0.345890 | `sql_db_show` | ❌ |
| 17 | 0.342405 | `monitor_table_list` | ❌ |
| 18 | 0.326053 | `azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.319544 | `datadog_monitoredresources_list` | ❌ |
| 20 | 0.318557 | `appconfig_kv_show` | ❌ |

---

## Test 61

**Expected Tool:** `cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668515 | `cosmos_account_list` | ❌ |
| 3 | 0.665298 | `cosmos_database_container_list` | ❌ |
| 4 | 0.573704 | `mysql_database_list` | ❌ |
| 5 | 0.571319 | `kusto_database_list` | ❌ |
| 6 | 0.548066 | `sql_db_list` | ❌ |
| 7 | 0.526046 | `redis_cluster_database_list` | ❌ |
| 8 | 0.501477 | `postgres_database_list` | ❌ |
| 9 | 0.471453 | `kusto_table_list` | ❌ |
| 10 | 0.459194 | `cosmos_database_container_item_query` | ❌ |
| 11 | 0.450854 | `monitor_table_list` | ❌ |
| 12 | 0.442540 | `mysql_table_list` | ❌ |
| 13 | 0.418871 | `storage_account_get` | ❌ |
| 14 | 0.407722 | `search_service_list` | ❌ |
| 15 | 0.406805 | `mysql_server_list` | ❌ |
| 16 | 0.401638 | `subscription_list` | ❌ |
| 17 | 0.387534 | `acr_registry_repository_list` | ❌ |
| 18 | 0.384252 | `kusto_cluster_list` | ❌ |
| 19 | 0.381044 | `keyvault_certificate_list` | ❌ |
| 20 | 0.379631 | `keyvault_key_list` | ❌ |

---

## Test 62

**Expected Tool:** `cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `cosmos_database_container_list` | ❌ |
| 3 | 0.614572 | `cosmos_account_list` | ❌ |
| 4 | 0.538479 | `mysql_database_list` | ❌ |
| 5 | 0.524837 | `kusto_database_list` | ❌ |
| 6 | 0.498206 | `sql_db_list` | ❌ |
| 7 | 0.497414 | `redis_cluster_database_list` | ❌ |
| 8 | 0.449759 | `cosmos_database_container_item_query` | ❌ |
| 9 | 0.447875 | `postgres_database_list` | ❌ |
| 10 | 0.437993 | `kusto_table_list` | ❌ |
| 11 | 0.408605 | `mysql_table_list` | ❌ |
| 12 | 0.402767 | `storage_account_get` | ❌ |
| 13 | 0.396280 | `monitor_table_list` | ❌ |
| 14 | 0.383780 | `storage_blob_container_get` | ❌ |
| 15 | 0.379009 | `mysql_server_list` | ❌ |
| 16 | 0.369344 | `sql_db_create` | ❌ |
| 17 | 0.348999 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.342424 | `acr_registry_repository_list` | ❌ |
| 19 | 0.339516 | `kusto_cluster_list` | ❌ |
| 20 | 0.335852 | `datadog_monitoredresources_list` | ❌ |

---

## Test 63

**Expected Tool:** `kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.482148 | `kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.464523 | `aks_cluster_get` | ❌ |
| 3 | 0.457669 | `redis_cluster_list` | ❌ |
| 4 | 0.416762 | `redis_cluster_database_list` | ❌ |
| 5 | 0.378455 | `aks_nodepool_get` | ❌ |
| 6 | 0.362977 | `aks_cluster_list` | ❌ |
| 7 | 0.361786 | `loadtesting_testrun_get` | ❌ |
| 8 | 0.353792 | `sql_server_show` | ❌ |
| 9 | 0.350998 | `storage_blob_get` | ❌ |
| 10 | 0.344871 | `sql_db_show` | ❌ |
| 11 | 0.344590 | `kusto_database_list` | ❌ |
| 12 | 0.333244 | `mysql_table_schema_get` | ❌ |
| 13 | 0.332639 | `kusto_cluster_list` | ❌ |
| 14 | 0.326526 | `redis_cache_list` | ❌ |
| 15 | 0.326306 | `search_index_get` | ❌ |
| 16 | 0.326052 | `aks_nodepool_list` | ❌ |
| 17 | 0.318754 | `kusto_query` | ❌ |
| 18 | 0.318505 | `storage_blob_container_get` | ❌ |
| 19 | 0.318082 | `mysql_server_config_get` | ❌ |
| 20 | 0.314617 | `kusto_table_schema` | ❌ |

---

## Test 64

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.651218 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.644037 | `redis_cluster_list` | ❌ |
| 3 | 0.549093 | `kusto_database_list` | ❌ |
| 4 | 0.536167 | `aks_cluster_list` | ❌ |
| 5 | 0.509396 | `grafana_list` | ❌ |
| 6 | 0.505975 | `redis_cache_list` | ❌ |
| 7 | 0.492107 | `postgres_server_list` | ❌ |
| 8 | 0.491278 | `search_service_list` | ❌ |
| 9 | 0.487583 | `monitor_workspace_list` | ❌ |
| 10 | 0.486159 | `kusto_cluster_get` | ❌ |
| 11 | 0.460255 | `cosmos_account_list` | ❌ |
| 12 | 0.458754 | `redis_cluster_database_list` | ❌ |
| 13 | 0.451500 | `kusto_table_list` | ❌ |
| 14 | 0.427759 | `subscription_list` | ❌ |
| 15 | 0.420174 | `foundry_agents_list` | ❌ |
| 16 | 0.412630 | `eventgrid_subscription_list` | ❌ |
| 17 | 0.411911 | `group_list` | ❌ |
| 18 | 0.410016 | `virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.399251 | `monitor_table_list` | ❌ |
| 20 | 0.391238 | `monitor_table_type_list` | ❌ |

---

## Test 65

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.437363 | `redis_cluster_list` | ❌ |
| 2 | 0.391087 | `redis_cluster_database_list` | ❌ |
| 3 | 0.386126 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 4 | 0.359551 | `kusto_database_list` | ❌ |
| 5 | 0.341784 | `kusto_cluster_get` | ❌ |
| 6 | 0.338260 | `aks_cluster_list` | ❌ |
| 7 | 0.314734 | `aks_cluster_get` | ❌ |
| 8 | 0.303083 | `grafana_list` | ❌ |
| 9 | 0.292994 | `foundry_agents_list` | ❌ |
| 10 | 0.292934 | `redis_cache_list` | ❌ |
| 11 | 0.287768 | `kusto_sample` | ❌ |
| 12 | 0.285603 | `kusto_query` | ❌ |
| 13 | 0.283331 | `kusto_table_list` | ❌ |
| 14 | 0.277014 | `mysql_database_list` | ❌ |
| 15 | 0.275559 | `mysql_database_query` | ❌ |
| 16 | 0.270804 | `monitor_table_list` | ❌ |
| 17 | 0.265906 | `mysql_server_list` | ❌ |
| 18 | 0.264112 | `monitor_table_type_list` | ❌ |
| 19 | 0.264035 | `monitor_workspace_list` | ❌ |
| 20 | 0.263226 | `quota_usage_check` | ❌ |

---

## Test 66

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.584053 | `redis_cluster_list` | ❌ |
| 2 | 0.549797 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 3 | 0.471216 | `aks_cluster_list` | ❌ |
| 4 | 0.469570 | `kusto_cluster_get` | ❌ |
| 5 | 0.464294 | `kusto_database_list` | ❌ |
| 6 | 0.462945 | `grafana_list` | ❌ |
| 7 | 0.446192 | `redis_cache_list` | ❌ |
| 8 | 0.440326 | `monitor_workspace_list` | ❌ |
| 9 | 0.434016 | `search_service_list` | ❌ |
| 10 | 0.432048 | `postgres_server_list` | ❌ |
| 11 | 0.406863 | `eventgrid_subscription_list` | ❌ |
| 12 | 0.396253 | `redis_cluster_database_list` | ❌ |
| 13 | 0.392541 | `kusto_table_list` | ❌ |
| 14 | 0.386776 | `cosmos_account_list` | ❌ |
| 15 | 0.380006 | `azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.377490 | `kusto_query` | ❌ |
| 17 | 0.371088 | `subscription_list` | ❌ |
| 18 | 0.368890 | `quota_usage_check` | ❌ |
| 19 | 0.365323 | `quota_region_availability_list` | ❌ |
| 20 | 0.355671 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 67

**Expected Tool:** `kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628129 | `redis_cluster_database_list` | ❌ |
| 2 | 0.610646 | `kusto_database_list` | ✅ **EXPECTED** |
| 3 | 0.553218 | `postgres_database_list` | ❌ |
| 4 | 0.549673 | `cosmos_database_list` | ❌ |
| 5 | 0.517039 | `mysql_database_list` | ❌ |
| 6 | 0.474354 | `kusto_table_list` | ❌ |
| 7 | 0.461496 | `sql_db_list` | ❌ |
| 8 | 0.459180 | `redis_cluster_list` | ❌ |
| 9 | 0.434330 | `postgres_table_list` | ❌ |
| 10 | 0.431669 | `kusto_cluster_list` | ❌ |
| 11 | 0.419528 | `mysql_table_list` | ❌ |
| 12 | 0.404095 | `monitor_table_list` | ❌ |
| 13 | 0.396060 | `cosmos_database_container_list` | ❌ |
| 14 | 0.375535 | `cosmos_account_list` | ❌ |
| 15 | 0.363663 | `postgres_server_list` | ❌ |
| 16 | 0.363266 | `mysql_server_list` | ❌ |
| 17 | 0.350253 | `aks_cluster_list` | ❌ |
| 18 | 0.334270 | `grafana_list` | ❌ |
| 19 | 0.320622 | `datadog_monitoredresources_list` | ❌ |
| 20 | 0.318850 | `kusto_query` | ❌ |

---

## Test 68

**Expected Tool:** `kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.597975 | `redis_cluster_database_list` | ❌ |
| 2 | 0.558503 | `kusto_database_list` | ✅ **EXPECTED** |
| 3 | 0.497144 | `cosmos_database_list` | ❌ |
| 4 | 0.491400 | `mysql_database_list` | ❌ |
| 5 | 0.486732 | `postgres_database_list` | ❌ |
| 6 | 0.440064 | `kusto_table_list` | ❌ |
| 7 | 0.427251 | `redis_cluster_list` | ❌ |
| 8 | 0.422588 | `sql_db_list` | ❌ |
| 9 | 0.391411 | `mysql_table_list` | ❌ |
| 10 | 0.383664 | `kusto_cluster_list` | ❌ |
| 11 | 0.368013 | `postgres_table_list` | ❌ |
| 12 | 0.362905 | `cosmos_database_container_list` | ❌ |
| 13 | 0.359378 | `monitor_table_list` | ❌ |
| 14 | 0.344010 | `mysql_server_list` | ❌ |
| 15 | 0.336400 | `monitor_table_type_list` | ❌ |
| 16 | 0.336104 | `cosmos_account_list` | ❌ |
| 17 | 0.334803 | `kusto_table_schema` | ❌ |
| 18 | 0.310912 | `aks_cluster_list` | ❌ |
| 19 | 0.309809 | `kusto_sample` | ❌ |
| 20 | 0.305718 | `kusto_query` | ❌ |

---

## Test 69

**Expected Tool:** `kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.381343 | `kusto_query` | ✅ **EXPECTED** |
| 2 | 0.363591 | `mysql_table_list` | ❌ |
| 3 | 0.363250 | `kusto_sample` | ❌ |
| 4 | 0.349152 | `monitor_table_list` | ❌ |
| 5 | 0.345774 | `redis_cluster_list` | ❌ |
| 6 | 0.334759 | `kusto_table_list` | ❌ |
| 7 | 0.328637 | `search_service_list` | ❌ |
| 8 | 0.328158 | `mysql_database_query` | ❌ |
| 9 | 0.324772 | `mysql_table_schema_get` | ❌ |
| 10 | 0.319109 | `redis_cluster_database_list` | ❌ |
| 11 | 0.318884 | `kusto_table_schema` | ❌ |
| 12 | 0.314955 | `monitor_table_type_list` | ❌ |
| 13 | 0.314929 | `search_index_query` | ❌ |
| 14 | 0.308107 | `kusto_database_list` | ❌ |
| 15 | 0.304017 | `cosmos_database_container_item_query` | ❌ |
| 16 | 0.302883 | `postgres_table_list` | ❌ |
| 17 | 0.292078 | `kusto_cluster_list` | ❌ |
| 18 | 0.264023 | `grafana_list` | ❌ |
| 19 | 0.263093 | `kusto_cluster_get` | ❌ |
| 20 | 0.257473 | `aks_cluster_list` | ❌ |

---

## Test 70

**Expected Tool:** `kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537157 | `kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.419458 | `kusto_table_schema` | ❌ |
| 3 | 0.391607 | `mysql_database_query` | ❌ |
| 4 | 0.391417 | `kusto_table_list` | ❌ |
| 5 | 0.380708 | `mysql_table_schema_get` | ❌ |
| 6 | 0.377035 | `redis_cluster_database_list` | ❌ |
| 7 | 0.364609 | `postgres_table_schema_get` | ❌ |
| 8 | 0.364361 | `mysql_table_list` | ❌ |
| 9 | 0.361843 | `redis_cluster_list` | ❌ |
| 10 | 0.343666 | `monitor_table_type_list` | ❌ |
| 11 | 0.341682 | `monitor_table_list` | ❌ |
| 12 | 0.337279 | `kusto_database_list` | ❌ |
| 13 | 0.319248 | `kusto_query` | ❌ |
| 14 | 0.318197 | `postgres_table_list` | ❌ |
| 15 | 0.310193 | `kusto_cluster_get` | ❌ |
| 16 | 0.285945 | `kusto_cluster_list` | ❌ |
| 17 | 0.282656 | `mysql_database_list` | ❌ |
| 18 | 0.267693 | `aks_cluster_get` | ❌ |
| 19 | 0.249386 | `aks_cluster_list` | ❌ |
| 20 | 0.242121 | `azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 71

**Expected Tool:** `kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591668 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585237 | `postgres_table_list` | ❌ |
| 3 | 0.556724 | `mysql_table_list` | ❌ |
| 4 | 0.549940 | `monitor_table_list` | ❌ |
| 5 | 0.521516 | `kusto_database_list` | ❌ |
| 6 | 0.520802 | `redis_cluster_database_list` | ❌ |
| 7 | 0.475496 | `postgres_database_list` | ❌ |
| 8 | 0.464341 | `monitor_table_type_list` | ❌ |
| 9 | 0.449656 | `kusto_table_schema` | ❌ |
| 10 | 0.436518 | `cosmos_database_list` | ❌ |
| 11 | 0.433775 | `mysql_database_list` | ❌ |
| 12 | 0.429278 | `redis_cluster_list` | ❌ |
| 13 | 0.412275 | `kusto_sample` | ❌ |
| 14 | 0.410425 | `kusto_cluster_list` | ❌ |
| 15 | 0.400099 | `mysql_table_schema_get` | ❌ |
| 16 | 0.384895 | `postgres_table_schema_get` | ❌ |
| 17 | 0.380671 | `cosmos_database_container_list` | ❌ |
| 18 | 0.337427 | `kusto_query` | ❌ |
| 19 | 0.330111 | `aks_cluster_list` | ❌ |
| 20 | 0.329669 | `grafana_list` | ❌ |

---

## Test 72

**Expected Tool:** `kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549885 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.524691 | `mysql_table_list` | ❌ |
| 3 | 0.523432 | `postgres_table_list` | ❌ |
| 4 | 0.494108 | `redis_cluster_database_list` | ❌ |
| 5 | 0.490759 | `monitor_table_list` | ❌ |
| 6 | 0.475412 | `kusto_database_list` | ❌ |
| 7 | 0.466212 | `kusto_table_schema` | ❌ |
| 8 | 0.431964 | `monitor_table_type_list` | ❌ |
| 9 | 0.425623 | `kusto_sample` | ❌ |
| 10 | 0.421413 | `postgres_database_list` | ❌ |
| 11 | 0.418153 | `mysql_table_schema_get` | ❌ |
| 12 | 0.415682 | `mysql_database_list` | ❌ |
| 13 | 0.403445 | `redis_cluster_list` | ❌ |
| 14 | 0.402646 | `postgres_table_schema_get` | ❌ |
| 15 | 0.391081 | `cosmos_database_list` | ❌ |
| 16 | 0.367187 | `kusto_cluster_list` | ❌ |
| 17 | 0.348891 | `cosmos_database_container_list` | ❌ |
| 18 | 0.330383 | `kusto_query` | ❌ |
| 19 | 0.314766 | `kusto_cluster_get` | ❌ |
| 20 | 0.300302 | `aks_cluster_list` | ❌ |

---

## Test 73

**Expected Tool:** `kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588151 | `kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564311 | `postgres_table_schema_get` | ❌ |
| 3 | 0.527917 | `mysql_table_schema_get` | ❌ |
| 4 | 0.445190 | `mysql_table_list` | ❌ |
| 5 | 0.437466 | `kusto_table_list` | ❌ |
| 6 | 0.432585 | `kusto_sample` | ❌ |
| 7 | 0.413924 | `monitor_table_list` | ❌ |
| 8 | 0.398632 | `redis_cluster_database_list` | ❌ |
| 9 | 0.387660 | `postgres_table_list` | ❌ |
| 10 | 0.366346 | `monitor_table_type_list` | ❌ |
| 11 | 0.366081 | `kusto_database_list` | ❌ |
| 12 | 0.358088 | `mysql_database_query` | ❌ |
| 13 | 0.345263 | `redis_cluster_list` | ❌ |
| 14 | 0.343548 | `foundry_knowledge_index_schema` | ❌ |
| 15 | 0.340038 | `mysql_database_list` | ❌ |
| 16 | 0.314580 | `kusto_cluster_get` | ❌ |
| 17 | 0.298243 | `kusto_query` | ❌ |
| 18 | 0.294840 | `cosmos_database_list` | ❌ |
| 19 | 0.282712 | `kusto_cluster_list` | ❌ |
| 20 | 0.275795 | `cosmos_database_container_list` | ❌ |

---

## Test 74

**Expected Tool:** `mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634056 | `postgres_database_list` | ❌ |
| 2 | 0.623421 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534457 | `mysql_table_list` | ❌ |
| 4 | 0.498918 | `mysql_server_list` | ❌ |
| 5 | 0.490148 | `sql_db_list` | ❌ |
| 6 | 0.472745 | `cosmos_database_list` | ❌ |
| 7 | 0.462034 | `redis_cluster_database_list` | ❌ |
| 8 | 0.453687 | `postgres_table_list` | ❌ |
| 9 | 0.430335 | `postgres_server_list` | ❌ |
| 10 | 0.428203 | `mysql_database_query` | ❌ |
| 11 | 0.421794 | `kusto_database_list` | ❌ |
| 12 | 0.406803 | `mysql_table_schema_get` | ❌ |
| 13 | 0.338476 | `cosmos_database_container_list` | ❌ |
| 14 | 0.327614 | `kusto_table_list` | ❌ |
| 15 | 0.317875 | `cosmos_account_list` | ❌ |
| 16 | 0.284786 | `grafana_list` | ❌ |
| 17 | 0.278428 | `acr_registry_repository_list` | ❌ |
| 18 | 0.268856 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.253148 | `acr_registry_list` | ❌ |
| 20 | 0.252501 | `group_list` | ❌ |

---

## Test 75

**Expected Tool:** `mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588122 | `mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.574089 | `postgres_database_list` | ❌ |
| 3 | 0.483855 | `mysql_table_list` | ❌ |
| 4 | 0.463244 | `mysql_server_list` | ❌ |
| 5 | 0.448169 | `redis_cluster_database_list` | ❌ |
| 6 | 0.444547 | `sql_db_list` | ❌ |
| 7 | 0.415119 | `cosmos_database_list` | ❌ |
| 8 | 0.405492 | `mysql_database_query` | ❌ |
| 9 | 0.404871 | `mysql_table_schema_get` | ❌ |
| 10 | 0.384974 | `postgres_table_list` | ❌ |
| 11 | 0.384778 | `postgres_server_list` | ❌ |
| 12 | 0.380422 | `kusto_database_list` | ❌ |
| 13 | 0.297709 | `cosmos_database_container_list` | ❌ |
| 14 | 0.290592 | `kusto_table_list` | ❌ |
| 15 | 0.259334 | `cosmos_account_list` | ❌ |
| 16 | 0.251297 | `appservice_database_add` | ❌ |
| 17 | 0.247558 | `grafana_list` | ❌ |
| 18 | 0.239544 | `azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.236450 | `acr_registry_repository_list` | ❌ |
| 20 | 0.236174 | `acr_registry_list` | ❌ |

---

## Test 76

**Expected Tool:** `mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476423 | `mysql_table_list` | ❌ |
| 2 | 0.455770 | `mysql_database_list` | ❌ |
| 3 | 0.433392 | `mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419859 | `mysql_server_list` | ❌ |
| 5 | 0.409445 | `mysql_table_schema_get` | ❌ |
| 6 | 0.393876 | `postgres_database_list` | ❌ |
| 7 | 0.345179 | `postgres_table_list` | ❌ |
| 8 | 0.328744 | `sql_db_list` | ❌ |
| 9 | 0.320053 | `postgres_server_list` | ❌ |
| 10 | 0.298681 | `mysql_server_param_get` | ❌ |
| 11 | 0.291451 | `cosmos_database_container_item_query` | ❌ |
| 12 | 0.285803 | `cosmos_database_list` | ❌ |
| 13 | 0.279038 | `kusto_query` | ❌ |
| 14 | 0.278067 | `cosmos_database_container_list` | ❌ |
| 15 | 0.264434 | `kusto_table_list` | ❌ |
| 16 | 0.257657 | `kusto_database_list` | ❌ |
| 17 | 0.230415 | `kusto_sample` | ❌ |
| 18 | 0.226519 | `kusto_table_schema` | ❌ |
| 19 | 0.225958 | `grafana_list` | ❌ |
| 20 | 0.198397 | `datadog_monitoredresources_list` | ❌ |

---

## Test 77

**Expected Tool:** `mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531887 | `postgres_server_config_get` | ❌ |
| 2 | 0.489816 | `mysql_server_config_get` | ✅ **EXPECTED** |
| 3 | 0.485952 | `mysql_server_param_set` | ❌ |
| 4 | 0.476863 | `mysql_server_param_get` | ❌ |
| 5 | 0.426507 | `mysql_table_schema_get` | ❌ |
| 6 | 0.413226 | `mysql_server_list` | ❌ |
| 7 | 0.398405 | `sql_server_show` | ❌ |
| 8 | 0.391644 | `mysql_database_list` | ❌ |
| 9 | 0.376750 | `mysql_database_query` | ❌ |
| 10 | 0.374852 | `postgres_server_param_get` | ❌ |
| 11 | 0.267903 | `appconfig_kv_list` | ❌ |
| 12 | 0.252810 | `loadtesting_test_get` | ❌ |
| 13 | 0.238583 | `appconfig_kv_show` | ❌ |
| 14 | 0.232680 | `loadtesting_testrun_list` | ❌ |
| 15 | 0.224212 | `appconfig_account_list` | ❌ |
| 16 | 0.215307 | `aks_nodepool_get` | ❌ |
| 17 | 0.214504 | `appservice_database_add` | ❌ |
| 18 | 0.198877 | `aks_cluster_get` | ❌ |
| 19 | 0.180063 | `azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.169430 | `aks_cluster_list` | ❌ |

---

## Test 78

**Expected Tool:** `mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678472 | `postgres_server_list` | ❌ |
| 2 | 0.558177 | `mysql_database_list` | ❌ |
| 3 | 0.554817 | `mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.501199 | `mysql_table_list` | ❌ |
| 5 | 0.482079 | `redis_cluster_list` | ❌ |
| 6 | 0.478541 | `sql_server_list` | ❌ |
| 7 | 0.467854 | `redis_cache_list` | ❌ |
| 8 | 0.458406 | `kusto_cluster_list` | ❌ |
| 9 | 0.457318 | `grafana_list` | ❌ |
| 10 | 0.451969 | `postgres_database_list` | ❌ |
| 11 | 0.431642 | `cosmos_account_list` | ❌ |
| 12 | 0.431126 | `sql_db_list` | ❌ |
| 13 | 0.422584 | `search_service_list` | ❌ |
| 14 | 0.410134 | `kusto_database_list` | ❌ |
| 15 | 0.403984 | `aks_cluster_list` | ❌ |
| 16 | 0.379322 | `cosmos_database_list` | ❌ |
| 17 | 0.377511 | `appconfig_account_list` | ❌ |
| 18 | 0.374433 | `acr_registry_list` | ❌ |
| 19 | 0.365605 | `group_list` | ❌ |
| 20 | 0.354490 | `datadog_monitoredresources_list` | ❌ |

---

## Test 79

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478518 | `mysql_database_list` | ❌ |
| 2 | 0.474586 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `postgres_server_list` | ❌ |
| 4 | 0.412380 | `mysql_table_list` | ❌ |
| 5 | 0.389993 | `postgres_database_list` | ❌ |
| 6 | 0.377048 | `mysql_database_query` | ❌ |
| 7 | 0.372766 | `mysql_table_schema_get` | ❌ |
| 8 | 0.363906 | `mysql_server_param_get` | ❌ |
| 9 | 0.355142 | `postgres_server_config_get` | ❌ |
| 10 | 0.337771 | `mysql_server_config_get` | ❌ |
| 11 | 0.281558 | `cosmos_database_list` | ❌ |
| 12 | 0.251411 | `cosmos_database_container_list` | ❌ |
| 13 | 0.248026 | `grafana_list` | ❌ |
| 14 | 0.248003 | `kusto_database_list` | ❌ |
| 15 | 0.244811 | `aks_cluster_list` | ❌ |
| 16 | 0.241497 | `foundry_agents_list` | ❌ |
| 17 | 0.235455 | `cosmos_account_list` | ❌ |
| 18 | 0.232383 | `kusto_cluster_list` | ❌ |
| 19 | 0.224586 | `appconfig_account_list` | ❌ |
| 20 | 0.218049 | `acr_registry_list` | ❌ |

---

## Test 80

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `postgres_server_list` | ❌ |
| 2 | 0.534266 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530210 | `mysql_database_list` | ❌ |
| 4 | 0.464360 | `mysql_table_list` | ❌ |
| 5 | 0.458498 | `sql_server_list` | ❌ |
| 6 | 0.456616 | `redis_cluster_list` | ❌ |
| 7 | 0.441885 | `redis_cache_list` | ❌ |
| 8 | 0.431914 | `grafana_list` | ❌ |
| 9 | 0.419663 | `search_service_list` | ❌ |
| 10 | 0.416021 | `kusto_cluster_list` | ❌ |
| 11 | 0.412407 | `mysql_database_query` | ❌ |
| 12 | 0.408235 | `mysql_table_schema_get` | ❌ |
| 13 | 0.399358 | `cosmos_account_list` | ❌ |
| 14 | 0.376596 | `kusto_database_list` | ❌ |
| 15 | 0.375789 | `aks_cluster_list` | ❌ |
| 16 | 0.364016 | `appconfig_account_list` | ❌ |
| 17 | 0.356680 | `acr_registry_list` | ❌ |
| 18 | 0.341439 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.341087 | `cosmos_database_list` | ❌ |
| 20 | 0.337333 | `eventgrid_subscription_list` | ❌ |

---

## Test 81

**Expected Tool:** `mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.407671 | `mysql_server_param_set` | ❌ |
| 3 | 0.333841 | `mysql_database_query` | ❌ |
| 4 | 0.313150 | `mysql_table_schema_get` | ❌ |
| 5 | 0.310782 | `postgres_server_param_get` | ❌ |
| 6 | 0.300031 | `mysql_database_list` | ❌ |
| 7 | 0.296654 | `mysql_server_config_get` | ❌ |
| 8 | 0.292546 | `mysql_server_list` | ❌ |
| 9 | 0.285657 | `postgres_server_param_set` | ❌ |
| 10 | 0.285645 | `postgres_server_config_get` | ❌ |
| 11 | 0.241275 | `appservice_database_add` | ❌ |
| 12 | 0.183735 | `appconfig_kv_show` | ❌ |
| 13 | 0.160082 | `appconfig_kv_list` | ❌ |
| 14 | 0.153259 | `keyvault_secret_get` | ❌ |
| 15 | 0.146292 | `loadtesting_testrun_get` | ❌ |
| 16 | 0.131542 | `keyvault_key_get` | ❌ |
| 17 | 0.124274 | `grafana_list` | ❌ |
| 18 | 0.121413 | `foundry_agents_connect` | ❌ |
| 19 | 0.120498 | `appconfig_kv_lock_set` | ❌ |
| 20 | 0.118505 | `loadtesting_testrun_list` | ❌ |

---

## Test 82

**Expected Tool:** `mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.390761 | `mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `mysql_server_param_get` | ❌ |
| 3 | 0.307508 | `postgres_server_param_set` | ❌ |
| 4 | 0.298911 | `mysql_database_query` | ❌ |
| 5 | 0.277669 | `appservice_database_add` | ❌ |
| 6 | 0.254180 | `mysql_server_list` | ❌ |
| 7 | 0.253189 | `mysql_table_schema_get` | ❌ |
| 8 | 0.246424 | `mysql_database_list` | ❌ |
| 9 | 0.246019 | `mysql_server_config_get` | ❌ |
| 10 | 0.238742 | `postgres_server_config_get` | ❌ |
| 11 | 0.236453 | `postgres_server_param_get` | ❌ |
| 12 | 0.140364 | `foundry_agents_connect` | ❌ |
| 13 | 0.112499 | `appconfig_kv_set` | ❌ |
| 14 | 0.109739 | `keyvault_secret_create` | ❌ |
| 15 | 0.090695 | `cosmos_database_container_item_query` | ❌ |
| 16 | 0.090334 | `cosmos_database_list` | ❌ |
| 17 | 0.089483 | `appconfig_kv_show` | ❌ |
| 18 | 0.088027 | `loadtesting_test_create` | ❌ |
| 19 | 0.086308 | `appconfig_kv_lock_set` | ❌ |
| 20 | 0.084424 | `foundry_agents_evaluate` | ❌ |

---

## Test 83

**Expected Tool:** `mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633448 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573844 | `postgres_table_list` | ❌ |
| 3 | 0.550898 | `postgres_database_list` | ❌ |
| 4 | 0.546963 | `mysql_database_list` | ❌ |
| 5 | 0.475178 | `mysql_table_schema_get` | ❌ |
| 6 | 0.447284 | `mysql_server_list` | ❌ |
| 7 | 0.442053 | `kusto_table_list` | ❌ |
| 8 | 0.429975 | `mysql_database_query` | ❌ |
| 9 | 0.418619 | `monitor_table_list` | ❌ |
| 10 | 0.410273 | `sql_db_list` | ❌ |
| 11 | 0.401217 | `cosmos_database_list` | ❌ |
| 12 | 0.393205 | `redis_cluster_database_list` | ❌ |
| 13 | 0.361477 | `kusto_database_list` | ❌ |
| 14 | 0.335069 | `cosmos_database_container_list` | ❌ |
| 15 | 0.308385 | `kusto_table_schema` | ❌ |
| 16 | 0.268415 | `cosmos_account_list` | ❌ |
| 17 | 0.260118 | `kusto_sample` | ❌ |
| 18 | 0.253046 | `grafana_list` | ❌ |
| 19 | 0.239226 | `appconfig_kv_list` | ❌ |
| 20 | 0.235180 | `datadog_monitoredresources_list` | ❌ |

---

## Test 84

**Expected Tool:** `mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609131 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.526236 | `postgres_table_list` | ❌ |
| 3 | 0.525709 | `mysql_database_list` | ❌ |
| 4 | 0.507258 | `mysql_table_schema_get` | ❌ |
| 5 | 0.498050 | `postgres_database_list` | ❌ |
| 6 | 0.439004 | `mysql_database_query` | ❌ |
| 7 | 0.419905 | `mysql_server_list` | ❌ |
| 8 | 0.403265 | `kusto_table_list` | ❌ |
| 9 | 0.385166 | `postgres_table_schema_get` | ❌ |
| 10 | 0.382169 | `monitor_table_list` | ❌ |
| 11 | 0.378011 | `redis_cluster_database_list` | ❌ |
| 12 | 0.349434 | `cosmos_database_list` | ❌ |
| 13 | 0.342926 | `kusto_table_schema` | ❌ |
| 14 | 0.319674 | `kusto_database_list` | ❌ |
| 15 | 0.303999 | `cosmos_database_container_list` | ❌ |
| 16 | 0.281571 | `kusto_sample` | ❌ |
| 17 | 0.236723 | `grafana_list` | ❌ |
| 18 | 0.231173 | `cosmos_account_list` | ❌ |
| 19 | 0.225876 | `appservice_database_add` | ❌ |
| 20 | 0.214496 | `datadog_monitoredresources_list` | ❌ |

---

## Test 85

**Expected Tool:** `mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630623 | `mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558306 | `postgres_table_schema_get` | ❌ |
| 3 | 0.545025 | `mysql_table_list` | ❌ |
| 4 | 0.482505 | `kusto_table_schema` | ❌ |
| 5 | 0.457739 | `mysql_database_list` | ❌ |
| 6 | 0.443955 | `mysql_database_query` | ❌ |
| 7 | 0.407451 | `postgres_table_list` | ❌ |
| 8 | 0.398102 | `postgres_database_list` | ❌ |
| 9 | 0.372911 | `mysql_server_list` | ❌ |
| 10 | 0.348909 | `mysql_server_config_get` | ❌ |
| 11 | 0.347368 | `postgres_server_config_get` | ❌ |
| 12 | 0.324675 | `kusto_table_list` | ❌ |
| 13 | 0.307950 | `kusto_sample` | ❌ |
| 14 | 0.271938 | `cosmos_database_list` | ❌ |
| 15 | 0.268321 | `foundry_knowledge_index_schema` | ❌ |
| 16 | 0.243861 | `kusto_database_list` | ❌ |
| 17 | 0.239328 | `cosmos_database_container_list` | ❌ |
| 18 | 0.208806 | `appservice_database_add` | ❌ |
| 19 | 0.202788 | `bicepschema_get` | ❌ |
| 20 | 0.194220 | `grafana_list` | ❌ |

---

## Test 86

**Expected Tool:** `postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815617 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.644014 | `postgres_table_list` | ❌ |
| 3 | 0.622790 | `postgres_server_list` | ❌ |
| 4 | 0.542685 | `postgres_server_config_get` | ❌ |
| 5 | 0.490955 | `postgres_server_param_get` | ❌ |
| 6 | 0.471672 | `mysql_database_list` | ❌ |
| 7 | 0.453436 | `sql_db_list` | ❌ |
| 8 | 0.444410 | `redis_cluster_database_list` | ❌ |
| 9 | 0.435828 | `cosmos_database_list` | ❌ |
| 10 | 0.418343 | `postgres_database_query` | ❌ |
| 11 | 0.414679 | `postgres_server_param_set` | ❌ |
| 12 | 0.407877 | `kusto_database_list` | ❌ |
| 13 | 0.319946 | `kusto_table_list` | ❌ |
| 14 | 0.293787 | `cosmos_database_container_list` | ❌ |
| 15 | 0.292441 | `cosmos_account_list` | ❌ |
| 16 | 0.289334 | `grafana_list` | ❌ |
| 17 | 0.252438 | `datadog_monitoredresources_list` | ❌ |
| 18 | 0.249563 | `kusto_cluster_list` | ❌ |
| 19 | 0.245546 | `acr_registry_repository_list` | ❌ |
| 20 | 0.245483 | `group_list` | ❌ |

---

## Test 87

**Expected Tool:** `postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760033 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589783 | `postgres_server_list` | ❌ |
| 3 | 0.585891 | `postgres_table_list` | ❌ |
| 4 | 0.552660 | `postgres_server_config_get` | ❌ |
| 5 | 0.495683 | `postgres_server_param_get` | ❌ |
| 6 | 0.452128 | `mysql_database_list` | ❌ |
| 7 | 0.433860 | `redis_cluster_database_list` | ❌ |
| 8 | 0.430589 | `postgres_table_schema_get` | ❌ |
| 9 | 0.426820 | `postgres_database_query` | ❌ |
| 10 | 0.416937 | `sql_db_list` | ❌ |
| 11 | 0.385475 | `cosmos_database_list` | ❌ |
| 12 | 0.365997 | `kusto_database_list` | ❌ |
| 13 | 0.281529 | `kusto_table_list` | ❌ |
| 14 | 0.261442 | `cosmos_database_container_list` | ❌ |
| 15 | 0.257971 | `grafana_list` | ❌ |
| 16 | 0.247726 | `cosmos_account_list` | ❌ |
| 17 | 0.235347 | `acr_registry_list` | ❌ |
| 18 | 0.227995 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.223442 | `azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.222503 | `kusto_table_schema` | ❌ |

---

## Test 88

**Expected Tool:** `postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546211 | `postgres_database_list` | ❌ |
| 2 | 0.503267 | `postgres_table_list` | ❌ |
| 3 | 0.466599 | `postgres_server_list` | ❌ |
| 4 | 0.415838 | `postgres_database_query` | ✅ **EXPECTED** |
| 5 | 0.403969 | `postgres_server_param_get` | ❌ |
| 6 | 0.403924 | `postgres_server_config_get` | ❌ |
| 7 | 0.380446 | `postgres_table_schema_get` | ❌ |
| 8 | 0.361081 | `mysql_table_list` | ❌ |
| 9 | 0.354336 | `postgres_server_param_set` | ❌ |
| 10 | 0.341271 | `mysql_database_list` | ❌ |
| 11 | 0.264914 | `cosmos_database_container_item_query` | ❌ |
| 12 | 0.262356 | `cosmos_database_list` | ❌ |
| 13 | 0.262184 | `kusto_query` | ❌ |
| 14 | 0.254174 | `kusto_table_list` | ❌ |
| 15 | 0.248628 | `cosmos_database_container_list` | ❌ |
| 16 | 0.244295 | `kusto_database_list` | ❌ |
| 17 | 0.236363 | `grafana_list` | ❌ |
| 18 | 0.218677 | `kusto_table_schema` | ❌ |
| 19 | 0.217855 | `kusto_sample` | ❌ |
| 20 | 0.189002 | `foundry_models_list` | ❌ |

---

## Test 89

**Expected Tool:** `postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.599471 | `postgres_server_param_get` | ❌ |
| 3 | 0.535229 | `postgres_server_param_set` | ❌ |
| 4 | 0.535049 | `postgres_database_list` | ❌ |
| 5 | 0.518574 | `postgres_server_list` | ❌ |
| 6 | 0.463172 | `postgres_table_list` | ❌ |
| 7 | 0.431476 | `postgres_table_schema_get` | ❌ |
| 8 | 0.394618 | `postgres_database_query` | ❌ |
| 9 | 0.356774 | `sql_server_show` | ❌ |
| 10 | 0.337899 | `mysql_server_config_get` | ❌ |
| 11 | 0.269224 | `appconfig_kv_list` | ❌ |
| 12 | 0.233426 | `loadtesting_testrun_list` | ❌ |
| 13 | 0.222849 | `appconfig_account_list` | ❌ |
| 14 | 0.220186 | `loadtesting_test_get` | ❌ |
| 15 | 0.208314 | `appconfig_kv_show` | ❌ |
| 16 | 0.189446 | `aks_nodepool_get` | ❌ |
| 17 | 0.185547 | `eventgrid_subscription_list` | ❌ |
| 18 | 0.178215 | `appservice_database_add` | ❌ |
| 19 | 0.177778 | `azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.174936 | `aks_cluster_get` | ❌ |

---

## Test 90

**Expected Tool:** `postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900023 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640733 | `postgres_database_list` | ❌ |
| 3 | 0.565914 | `postgres_table_list` | ❌ |
| 4 | 0.538997 | `postgres_server_config_get` | ❌ |
| 5 | 0.507714 | `postgres_server_param_get` | ❌ |
| 6 | 0.483663 | `redis_cluster_list` | ❌ |
| 7 | 0.472458 | `grafana_list` | ❌ |
| 8 | 0.457583 | `sql_server_list` | ❌ |
| 9 | 0.453841 | `kusto_cluster_list` | ❌ |
| 10 | 0.446591 | `redis_cache_list` | ❌ |
| 11 | 0.435298 | `search_service_list` | ❌ |
| 12 | 0.416315 | `mysql_server_list` | ❌ |
| 13 | 0.406617 | `cosmos_account_list` | ❌ |
| 14 | 0.399158 | `aks_cluster_list` | ❌ |
| 15 | 0.397428 | `kusto_database_list` | ❌ |
| 16 | 0.389191 | `appconfig_account_list` | ❌ |
| 17 | 0.373641 | `eventgrid_subscription_list` | ❌ |
| 18 | 0.373639 | `acr_registry_list` | ❌ |
| 19 | 0.366102 | `group_list` | ❌ |
| 20 | 0.362900 | `eventgrid_topic_list` | ❌ |

---

## Test 91

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.607062 | `postgres_database_list` | ❌ |
| 3 | 0.576349 | `postgres_server_config_get` | ❌ |
| 4 | 0.522996 | `postgres_table_list` | ❌ |
| 5 | 0.506171 | `postgres_server_param_get` | ❌ |
| 6 | 0.409378 | `postgres_database_query` | ❌ |
| 7 | 0.400088 | `postgres_server_param_set` | ❌ |
| 8 | 0.372955 | `postgres_table_schema_get` | ❌ |
| 9 | 0.336934 | `mysql_database_list` | ❌ |
| 10 | 0.336270 | `mysql_server_list` | ❌ |
| 11 | 0.274763 | `grafana_list` | ❌ |
| 12 | 0.260533 | `cosmos_database_list` | ❌ |
| 13 | 0.253264 | `kusto_database_list` | ❌ |
| 14 | 0.245311 | `aks_cluster_list` | ❌ |
| 15 | 0.241835 | `kusto_cluster_list` | ❌ |
| 16 | 0.239500 | `appconfig_account_list` | ❌ |
| 17 | 0.238588 | `foundry_agents_list` | ❌ |
| 18 | 0.229741 | `acr_registry_list` | ❌ |
| 19 | 0.227547 | `cosmos_account_list` | ❌ |
| 20 | 0.225295 | `azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 92

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579232 | `postgres_database_list` | ❌ |
| 3 | 0.531804 | `postgres_server_config_get` | ❌ |
| 4 | 0.514445 | `postgres_table_list` | ❌ |
| 5 | 0.505970 | `postgres_server_param_get` | ❌ |
| 6 | 0.452608 | `redis_cluster_list` | ❌ |
| 7 | 0.444127 | `grafana_list` | ❌ |
| 8 | 0.430033 | `sql_server_list` | ❌ |
| 9 | 0.421577 | `search_service_list` | ❌ |
| 10 | 0.414774 | `redis_cache_list` | ❌ |
| 11 | 0.410707 | `postgres_database_query` | ❌ |
| 12 | 0.403538 | `kusto_cluster_list` | ❌ |
| 13 | 0.376954 | `cosmos_account_list` | ❌ |
| 14 | 0.367001 | `eventgrid_subscription_list` | ❌ |
| 15 | 0.362650 | `kusto_database_list` | ❌ |
| 16 | 0.362557 | `appconfig_account_list` | ❌ |
| 17 | 0.360586 | `aks_cluster_list` | ❌ |
| 18 | 0.358358 | `acr_registry_list` | ❌ |
| 19 | 0.334679 | `azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.334101 | `datadog_monitoredresources_list` | ❌ |

---

## Test 93

**Expected Tool:** `postgres_server_param`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594733 | `postgres_server_param_get` | ❌ |
| 2 | 0.539671 | `postgres_server_config_get` | ❌ |
| 3 | 0.489693 | `postgres_server_list` | ❌ |
| 4 | 0.480872 | `postgres_server_param_set` | ❌ |
| 5 | 0.451871 | `postgres_database_list` | ❌ |
| 6 | 0.357606 | `postgres_table_list` | ❌ |
| 7 | 0.343799 | `mysql_server_param_get` | ❌ |
| 8 | 0.330875 | `postgres_table_schema_get` | ❌ |
| 9 | 0.305333 | `postgres_database_query` | ❌ |
| 10 | 0.295439 | `mysql_server_param_set` | ❌ |
| 11 | 0.185273 | `appconfig_kv_list` | ❌ |
| 12 | 0.183435 | `eventgrid_subscription_list` | ❌ |
| 13 | 0.174107 | `grafana_list` | ❌ |
| 14 | 0.169190 | `appconfig_account_list` | ❌ |
| 15 | 0.166286 | `azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.158090 | `cosmos_database_list` | ❌ |
| 17 | 0.155785 | `appconfig_kv_show` | ❌ |
| 18 | 0.145056 | `kusto_database_list` | ❌ |
| 19 | 0.142408 | `aks_cluster_list` | ❌ |
| 20 | 0.141137 | `foundry_agents_query-and-evaluate` | ❌ |

---

## Test 94

**Expected Tool:** `postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.488474 | `postgres_server_config_get` | ❌ |
| 2 | 0.469794 | `postgres_server_list` | ❌ |
| 3 | 0.464604 | `postgres_server_param_set` | ✅ **EXPECTED** |
| 4 | 0.447026 | `postgres_server_param_get` | ❌ |
| 5 | 0.440760 | `postgres_database_list` | ❌ |
| 6 | 0.354049 | `postgres_table_list` | ❌ |
| 7 | 0.341619 | `postgres_database_query` | ❌ |
| 8 | 0.317484 | `postgres_table_schema_get` | ❌ |
| 9 | 0.241642 | `mysql_server_param_set` | ❌ |
| 10 | 0.227740 | `mysql_server_list` | ❌ |
| 11 | 0.192610 | `appservice_database_add` | ❌ |
| 12 | 0.133385 | `kusto_sample` | ❌ |
| 13 | 0.127120 | `kusto_database_list` | ❌ |
| 14 | 0.126706 | `foundry_agents_evaluate` | ❌ |
| 15 | 0.123491 | `kusto_table_schema` | ❌ |
| 16 | 0.119027 | `eventgrid_subscription_list` | ❌ |
| 17 | 0.118089 | `cosmos_database_list` | ❌ |
| 18 | 0.114978 | `kusto_cluster_get` | ❌ |
| 19 | 0.113841 | `grafana_list` | ❌ |
| 20 | 0.112605 | `deploy_plan_get` | ❌ |

---

## Test 95

**Expected Tool:** `postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789883 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.750580 | `postgres_database_list` | ❌ |
| 3 | 0.574930 | `postgres_server_list` | ❌ |
| 4 | 0.519820 | `postgres_table_schema_get` | ❌ |
| 5 | 0.501400 | `postgres_server_config_get` | ❌ |
| 6 | 0.477688 | `mysql_table_list` | ❌ |
| 7 | 0.449212 | `postgres_database_query` | ❌ |
| 8 | 0.432813 | `kusto_table_list` | ❌ |
| 9 | 0.430171 | `postgres_server_param_get` | ❌ |
| 10 | 0.396688 | `mysql_database_list` | ❌ |
| 11 | 0.394396 | `monitor_table_list` | ❌ |
| 12 | 0.373673 | `cosmos_database_list` | ❌ |
| 13 | 0.352211 | `kusto_database_list` | ❌ |
| 14 | 0.308203 | `kusto_table_schema` | ❌ |
| 15 | 0.299785 | `cosmos_database_container_list` | ❌ |
| 16 | 0.257808 | `grafana_list` | ❌ |
| 17 | 0.256245 | `kusto_sample` | ❌ |
| 18 | 0.249162 | `cosmos_account_list` | ❌ |
| 19 | 0.236931 | `appconfig_kv_list` | ❌ |
| 20 | 0.229889 | `kusto_cluster_list` | ❌ |

---

## Test 96

**Expected Tool:** `postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.735747 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.689945 | `postgres_database_list` | ❌ |
| 3 | 0.557599 | `postgres_table_schema_get` | ❌ |
| 4 | 0.543183 | `postgres_server_list` | ❌ |
| 5 | 0.521813 | `postgres_server_config_get` | ❌ |
| 6 | 0.465141 | `postgres_database_query` | ❌ |
| 7 | 0.457541 | `mysql_table_list` | ❌ |
| 8 | 0.447567 | `postgres_server_param_get` | ❌ |
| 9 | 0.390089 | `kusto_table_list` | ❌ |
| 10 | 0.383317 | `mysql_database_list` | ❌ |
| 11 | 0.371888 | `postgres_server_param_set` | ❌ |
| 12 | 0.334224 | `kusto_table_schema` | ❌ |
| 13 | 0.315801 | `cosmos_database_list` | ❌ |
| 14 | 0.307117 | `kusto_database_list` | ❌ |
| 15 | 0.272303 | `kusto_sample` | ❌ |
| 16 | 0.266378 | `cosmos_database_container_list` | ❌ |
| 17 | 0.243530 | `grafana_list` | ❌ |
| 18 | 0.207757 | `cosmos_account_list` | ❌ |
| 19 | 0.205790 | `appconfig_kv_list` | ❌ |
| 20 | 0.202853 | `datadog_monitoredresources_list` | ❌ |

---

## Test 97

**Expected Tool:** `postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714894 | `postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597891 | `postgres_table_list` | ❌ |
| 3 | 0.574233 | `postgres_database_list` | ❌ |
| 4 | 0.508086 | `postgres_server_config_get` | ❌ |
| 5 | 0.480729 | `mysql_table_schema_get` | ❌ |
| 6 | 0.475678 | `kusto_table_schema` | ❌ |
| 7 | 0.443802 | `postgres_server_param_get` | ❌ |
| 8 | 0.442577 | `postgres_server_list` | ❌ |
| 9 | 0.427540 | `postgres_database_query` | ❌ |
| 10 | 0.406776 | `mysql_table_list` | ❌ |
| 11 | 0.362670 | `postgres_server_param_set` | ❌ |
| 12 | 0.322788 | `kusto_table_list` | ❌ |
| 13 | 0.303755 | `kusto_sample` | ❌ |
| 14 | 0.253856 | `foundry_knowledge_index_schema` | ❌ |
| 15 | 0.253391 | `cosmos_database_list` | ❌ |
| 16 | 0.239243 | `kusto_database_list` | ❌ |
| 17 | 0.212285 | `cosmos_database_container_list` | ❌ |
| 18 | 0.201665 | `grafana_list` | ❌ |
| 19 | 0.185101 | `appconfig_kv_list` | ❌ |
| 20 | 0.184052 | `appservice_database_add` | ❌ |

---

## Test 98

**Expected Tool:** `deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686657 | `deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `deploy_plan_get` | ❌ |
| 3 | 0.404890 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.392466 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.389603 | `deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.354472 | `applens_resource_diagnose` | ❌ |
| 7 | 0.342594 | `monitor_resource_log_query` | ❌ |
| 8 | 0.334992 | `quota_usage_check` | ❌ |
| 9 | 0.334522 | `mysql_server_list` | ❌ |
| 10 | 0.333577 | `foundry_agents_list` | ❌ |
| 11 | 0.327028 | `datadog_monitoredresources_list` | ❌ |
| 12 | 0.325553 | `extension_azqr` | ❌ |
| 13 | 0.320572 | `aks_nodepool_get` | ❌ |
| 14 | 0.314964 | `sql_server_show` | ❌ |
| 15 | 0.314890 | `sql_db_create` | ❌ |
| 16 | 0.312821 | `sql_db_update` | ❌ |
| 17 | 0.307291 | `sql_db_show` | ❌ |
| 18 | 0.297568 | `resourcehealth_availability-status_get` | ❌ |
| 19 | 0.294636 | `sql_server_list` | ❌ |
| 20 | 0.288823 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 99

**Expected Tool:** `deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680640 | `deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562505 | `deploy_plan_get` | ❌ |
| 3 | 0.505052 | `cloudarchitect_design` | ❌ |
| 4 | 0.497193 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.435890 | `deploy_iac_rules_get` | ❌ |
| 6 | 0.430764 | `azureterraformbestpractices_get` | ❌ |
| 7 | 0.417333 | `get_bestpractices_get` | ❌ |
| 8 | 0.371152 | `deploy_app_logs_get` | ❌ |
| 9 | 0.343117 | `quota_usage_check` | ❌ |
| 10 | 0.322230 | `extension_azqr` | ❌ |
| 11 | 0.317906 | `foundry_agents_list` | ❌ |
| 12 | 0.284401 | `mysql_table_schema_get` | ❌ |
| 13 | 0.270093 | `sql_db_create` | ❌ |
| 14 | 0.264888 | `resourcehealth_availability-status_get` | ❌ |
| 15 | 0.264060 | `mysql_server_list` | ❌ |
| 16 | 0.263521 | `quota_region_availability_list` | ❌ |
| 17 | 0.255084 | `mysql_table_list` | ❌ |
| 18 | 0.250629 | `search_service_list` | ❌ |
| 19 | 0.248128 | `sql_db_update` | ❌ |
| 20 | 0.247818 | `sql_server_show` | ❌ |

---

## Test 100

**Expected Tool:** `deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529285 | `deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.404956 | `bicepschema_get` | ❌ |
| 3 | 0.392073 | `get_bestpractices_get` | ❌ |
| 4 | 0.383307 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.341541 | `deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.304894 | `deploy_plan_get` | ❌ |
| 7 | 0.278758 | `cloudarchitect_design` | ❌ |
| 8 | 0.266972 | `deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.266660 | `sql_server_firewall-rule_list` | ❌ |
| 10 | 0.252796 | `sql_server_firewall-rule_create` | ❌ |
| 11 | 0.236481 | `applens_resource_diagnose` | ❌ |
| 12 | 0.224014 | `extension_azqr` | ❌ |
| 13 | 0.219574 | `quota_usage_check` | ❌ |
| 14 | 0.206992 | `mysql_server_list` | ❌ |
| 15 | 0.202308 | `mysql_table_schema_get` | ❌ |
| 16 | 0.201412 | `quota_region_availability_list` | ❌ |
| 17 | 0.195479 | `mysql_table_list` | ❌ |
| 18 | 0.194647 | `sql_db_create` | ❌ |
| 19 | 0.188727 | `role_assignment_list` | ❌ |
| 20 | 0.178639 | `storage_blob_get` | ❌ |

---

## Test 101

**Expected Tool:** `deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `deploy_plan_get` | ❌ |
| 3 | 0.448842 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.382240 | `get_bestpractices_get` | ❌ |
| 5 | 0.375202 | `deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.373411 | `deploy_app_logs_get` | ❌ |
| 7 | 0.350101 | `azureterraformbestpractices_get` | ❌ |
| 8 | 0.338440 | `foundry_models_deploy` | ❌ |
| 9 | 0.322906 | `cloudarchitect_design` | ❌ |
| 10 | 0.297761 | `appservice_database_add` | ❌ |
| 11 | 0.262768 | `sql_db_create` | ❌ |
| 12 | 0.240757 | `storage_blob_upload` | ❌ |
| 13 | 0.234556 | `sql_db_update` | ❌ |
| 14 | 0.230063 | `quota_usage_check` | ❌ |
| 15 | 0.222451 | `sql_server_create` | ❌ |
| 16 | 0.212123 | `storage_blob_container_create` | ❌ |
| 17 | 0.211103 | `storage_account_create` | ❌ |
| 18 | 0.203987 | `sql_server_delete` | ❌ |
| 19 | 0.198696 | `mysql_server_list` | ❌ |
| 20 | 0.195915 | `workbooks_delete` | ❌ |

---

## Test 102

**Expected Tool:** `deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688051 | `deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499311 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.432825 | `get_bestpractices_get` | ❌ |
| 6 | 0.425393 | `azureterraformbestpractices_get` | ❌ |
| 7 | 0.421744 | `cloudarchitect_design` | ❌ |
| 8 | 0.413781 | `loadtesting_test_create` | ❌ |
| 9 | 0.393574 | `deploy_app_logs_get` | ❌ |
| 10 | 0.365875 | `foundry_models_deploy` | ❌ |
| 11 | 0.344407 | `sql_db_create` | ❌ |
| 12 | 0.312839 | `quota_usage_check` | ❌ |
| 13 | 0.300643 | `mysql_server_list` | ❌ |
| 14 | 0.299552 | `storage_account_create` | ❌ |
| 15 | 0.296697 | `sql_server_create` | ❌ |
| 16 | 0.292652 | `sql_db_update` | ❌ |
| 17 | 0.277064 | `workbooks_delete` | ❌ |
| 18 | 0.258195 | `sql_server_show` | ❌ |
| 19 | 0.252696 | `workbooks_create` | ❌ |
| 20 | 0.249358 | `storage_blob_container_create` | ❌ |

---

## Test 103

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759178 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.610315 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.545540 | `search_service_list` | ❌ |
| 4 | 0.514189 | `kusto_cluster_list` | ❌ |
| 5 | 0.496537 | `subscription_list` | ❌ |
| 6 | 0.495664 | `resourcehealth_service-health-events_list` | ❌ |
| 7 | 0.492754 | `group_list` | ❌ |
| 8 | 0.485584 | `redis_cluster_list` | ❌ |
| 9 | 0.484509 | `postgres_server_list` | ❌ |
| 10 | 0.475667 | `cosmos_account_list` | ❌ |
| 11 | 0.475056 | `monitor_workspace_list` | ❌ |
| 12 | 0.472764 | `grafana_list` | ❌ |
| 13 | 0.470316 | `redis_cache_list` | ❌ |
| 14 | 0.442229 | `virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.440645 | `aks_cluster_list` | ❌ |
| 16 | 0.439820 | `servicebus_topic_subscription_details` | ❌ |
| 17 | 0.438287 | `appconfig_account_list` | ❌ |
| 18 | 0.427536 | `foundry_agents_list` | ❌ |
| 19 | 0.422414 | `datadog_monitoredresources_list` | ❌ |
| 20 | 0.421784 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |

---

## Test 104

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691068 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.599956 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.477932 | `resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.475119 | `search_service_list` | ❌ |
| 5 | 0.450712 | `redis_cluster_list` | ❌ |
| 6 | 0.441607 | `kusto_cluster_list` | ❌ |
| 7 | 0.437153 | `postgres_server_list` | ❌ |
| 8 | 0.431249 | `subscription_list` | ❌ |
| 9 | 0.430494 | `grafana_list` | ❌ |
| 10 | 0.428470 | `redis_cache_list` | ❌ |
| 11 | 0.424907 | `monitor_workspace_list` | ❌ |
| 12 | 0.420072 | `servicebus_topic_subscription_details` | ❌ |
| 13 | 0.419194 | `group_list` | ❌ |
| 14 | 0.408708 | `cosmos_account_list` | ❌ |
| 15 | 0.399253 | `appconfig_account_list` | ❌ |
| 16 | 0.396758 | `resourcehealth_availability-status_list` | ❌ |
| 17 | 0.390290 | `servicebus_topic_details` | ❌ |
| 18 | 0.384757 | `foundry_agents_list` | ❌ |
| 19 | 0.381705 | `aks_cluster_list` | ❌ |
| 20 | 0.381664 | `datadog_monitoredresources_list` | ❌ |

---

## Test 105

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759396 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.632794 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.526595 | `kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `search_service_list` | ❌ |
| 5 | 0.495195 | `resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.494153 | `postgres_server_list` | ❌ |
| 7 | 0.481439 | `group_list` | ❌ |
| 8 | 0.481065 | `redis_cluster_list` | ❌ |
| 9 | 0.476812 | `redis_cache_list` | ❌ |
| 10 | 0.476780 | `subscription_list` | ❌ |
| 11 | 0.471888 | `servicebus_topic_subscription_details` | ❌ |
| 12 | 0.468200 | `grafana_list` | ❌ |
| 13 | 0.466774 | `monitor_workspace_list` | ❌ |
| 14 | 0.445991 | `cosmos_account_list` | ❌ |
| 15 | 0.429646 | `virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.428727 | `appconfig_account_list` | ❌ |
| 17 | 0.424984 | `servicebus_topic_details` | ❌ |
| 18 | 0.421430 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.417918 | `aks_cluster_list` | ❌ |
| 20 | 0.392039 | `kusto_database_list` | ❌ |

---

## Test 106

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659238 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.618947 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.609372 | `group_list` | ❌ |
| 4 | 0.514693 | `workbooks_list` | ❌ |
| 5 | 0.506299 | `datadog_monitoredresources_list` | ❌ |
| 6 | 0.491568 | `sql_server_list` | ❌ |
| 7 | 0.484927 | `resourcehealth_availability-status_list` | ❌ |
| 8 | 0.475509 | `redis_cluster_list` | ❌ |
| 9 | 0.464487 | `kusto_cluster_list` | ❌ |
| 10 | 0.460577 | `search_service_list` | ❌ |
| 11 | 0.456600 | `grafana_list` | ❌ |
| 12 | 0.455542 | `virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.453017 | `acr_registry_list` | ❌ |
| 14 | 0.448172 | `redis_cache_list` | ❌ |
| 15 | 0.443050 | `monitor_workspace_list` | ❌ |
| 16 | 0.441809 | `resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.432442 | `loadtesting_testresource_list` | ❌ |
| 18 | 0.422990 | `postgres_server_list` | ❌ |
| 19 | 0.412017 | `acr_registry_repository_list` | ❌ |
| 20 | 0.407959 | `cosmos_account_list` | ❌ |

---

## Test 107

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682900 | `eventgrid_topic_list` | ❌ |
| 2 | 0.637188 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.486216 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.480537 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.477660 | `servicebus_topic_details` | ❌ |
| 6 | 0.457868 | `search_service_list` | ❌ |
| 7 | 0.445053 | `subscription_list` | ❌ |
| 8 | 0.435412 | `kusto_cluster_list` | ❌ |
| 9 | 0.434659 | `redis_cluster_list` | ❌ |
| 10 | 0.422093 | `postgres_server_list` | ❌ |
| 11 | 0.420976 | `group_list` | ❌ |
| 12 | 0.417538 | `monitor_workspace_list` | ❌ |
| 13 | 0.415247 | `redis_cache_list` | ❌ |
| 14 | 0.408588 | `grafana_list` | ❌ |
| 15 | 0.397665 | `cosmos_account_list` | ❌ |
| 16 | 0.392784 | `resourcehealth_availability-status_list` | ❌ |
| 17 | 0.382858 | `aks_cluster_list` | ❌ |
| 18 | 0.378136 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.376133 | `appconfig_account_list` | ❌ |
| 20 | 0.367397 | `acr_registry_list` | ❌ |

---

## Test 108

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672482 | `eventgrid_topic_list` | ❌ |
| 2 | 0.656023 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.539977 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.497869 | `servicebus_topic_details` | ❌ |
| 5 | 0.459817 | `resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.444774 | `redis_cluster_list` | ❌ |
| 7 | 0.443291 | `subscription_list` | ❌ |
| 8 | 0.438131 | `kusto_cluster_list` | ❌ |
| 9 | 0.435639 | `search_service_list` | ❌ |
| 10 | 0.434401 | `redis_cache_list` | ❌ |
| 11 | 0.433482 | `monitor_workspace_list` | ❌ |
| 12 | 0.431618 | `grafana_list` | ❌ |
| 13 | 0.427056 | `group_list` | ❌ |
| 14 | 0.419194 | `postgres_server_list` | ❌ |
| 15 | 0.402159 | `cosmos_account_list` | ❌ |
| 16 | 0.398589 | `datadog_monitoredresources_list` | ❌ |
| 17 | 0.392822 | `resourcehealth_availability-status_list` | ❌ |
| 18 | 0.386881 | `acr_registry_list` | ❌ |
| 19 | 0.376625 | `aks_cluster_list` | ❌ |
| 20 | 0.376197 | `appconfig_account_list` | ❌ |

---

## Test 109

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669199 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.663347 | `eventgrid_topic_list` | ❌ |
| 3 | 0.524880 | `group_list` | ❌ |
| 4 | 0.488627 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484144 | `servicebus_topic_subscription_details` | ❌ |
| 6 | 0.478901 | `datadog_monitoredresources_list` | ❌ |
| 7 | 0.474030 | `resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.473088 | `servicebus_topic_details` | ❌ |
| 9 | 0.465353 | `acr_registry_list` | ❌ |
| 10 | 0.465017 | `workbooks_list` | ❌ |
| 11 | 0.462100 | `redis_cluster_list` | ❌ |
| 12 | 0.457043 | `search_service_list` | ❌ |
| 13 | 0.452311 | `monitor_workspace_list` | ❌ |
| 14 | 0.452174 | `sql_server_list` | ❌ |
| 15 | 0.443060 | `subscription_list` | ❌ |
| 16 | 0.436586 | `kusto_cluster_list` | ❌ |
| 17 | 0.435971 | `grafana_list` | ❌ |
| 18 | 0.428668 | `functionapp_get` | ❌ |
| 19 | 0.412463 | `loadtesting_testresource_list` | ❌ |
| 20 | 0.409126 | `applicationinsights_recommendation_list` | ❌ |

---

## Test 110

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593171 | `eventgrid_topic_list` | ❌ |
| 2 | 0.592262 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.525017 | `subscription_list` | ❌ |
| 4 | 0.518857 | `search_service_list` | ❌ |
| 5 | 0.508407 | `resourcehealth_service-health-events_list` | ❌ |
| 6 | 0.490371 | `redis_cluster_list` | ❌ |
| 7 | 0.489687 | `kusto_cluster_list` | ❌ |
| 8 | 0.480579 | `cosmos_account_list` | ❌ |
| 9 | 0.475795 | `group_list` | ❌ |
| 10 | 0.475220 | `redis_cache_list` | ❌ |
| 11 | 0.473274 | `postgres_server_list` | ❌ |
| 12 | 0.467172 | `monitor_workspace_list` | ❌ |
| 13 | 0.460683 | `grafana_list` | ❌ |
| 14 | 0.451759 | `appconfig_account_list` | ❌ |
| 15 | 0.440759 | `aks_cluster_list` | ❌ |
| 16 | 0.439125 | `resourcehealth_availability-status_list` | ❌ |
| 17 | 0.422468 | `virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.415047 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.410156 | `acr_registry_list` | ❌ |
| 20 | 0.399352 | `quota_region_availability_list` | ❌ |

---

## Test 111

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604278 | `eventgrid_topic_list` | ❌ |
| 2 | 0.600323 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 3 | 0.535955 | `kusto_cluster_list` | ❌ |
| 4 | 0.518141 | `subscription_list` | ❌ |
| 5 | 0.510216 | `group_list` | ❌ |
| 6 | 0.508443 | `search_service_list` | ❌ |
| 7 | 0.493621 | `resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.492726 | `postgres_server_list` | ❌ |
| 9 | 0.485794 | `redis_cluster_list` | ❌ |
| 10 | 0.483497 | `redis_cache_list` | ❌ |
| 11 | 0.481609 | `cosmos_account_list` | ❌ |
| 12 | 0.481450 | `monitor_workspace_list` | ❌ |
| 13 | 0.473868 | `grafana_list` | ❌ |
| 14 | 0.467622 | `servicebus_topic_subscription_details` | ❌ |
| 15 | 0.453352 | `appconfig_account_list` | ❌ |
| 16 | 0.446484 | `aks_cluster_list` | ❌ |
| 17 | 0.428290 | `virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.426897 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.411734 | `sql_server_list` | ❌ |
| 20 | 0.410778 | `acr_registry_list` | ❌ |

---

## Test 112

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622684 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.558722 | `group_list` | ❌ |
| 3 | 0.532589 | `eventgrid_topic_list` | ❌ |
| 4 | 0.507088 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.503801 | `datadog_monitoredresources_list` | ❌ |
| 6 | 0.490489 | `redis_cluster_list` | ❌ |
| 7 | 0.473240 | `resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.469708 | `workbooks_list` | ❌ |
| 9 | 0.468719 | `sql_server_list` | ❌ |
| 10 | 0.466767 | `redis_cache_list` | ❌ |
| 11 | 0.462138 | `acr_registry_list` | ❌ |
| 12 | 0.460740 | `grafana_list` | ❌ |
| 13 | 0.443239 | `loadtesting_testresource_list` | ❌ |
| 14 | 0.439165 | `kusto_cluster_list` | ❌ |
| 15 | 0.439005 | `subscription_list` | ❌ |
| 16 | 0.435251 | `search_service_list` | ❌ |
| 17 | 0.433549 | `monitor_workspace_list` | ❌ |
| 18 | 0.425325 | `virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.413844 | `azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.413763 | `cosmos_account_list` | ❌ |

---

## Test 113

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653850 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.581728 | `eventgrid_topic_list` | ❌ |
| 3 | 0.479893 | `resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.478385 | `subscription_list` | ❌ |
| 5 | 0.476763 | `search_service_list` | ❌ |
| 6 | 0.475482 | `monitor_workspace_list` | ❌ |
| 7 | 0.471596 | `redis_cluster_list` | ❌ |
| 8 | 0.471384 | `kusto_cluster_list` | ❌ |
| 9 | 0.466489 | `group_list` | ❌ |
| 10 | 0.449826 | `redis_cache_list` | ❌ |
| 11 | 0.449681 | `grafana_list` | ❌ |
| 12 | 0.447080 | `servicebus_topic_subscription_details` | ❌ |
| 13 | 0.446605 | `acr_registry_list` | ❌ |
| 14 | 0.444645 | `resourcehealth_availability-status_list` | ❌ |
| 15 | 0.437300 | `quota_region_availability_list` | ❌ |
| 16 | 0.436793 | `postgres_server_list` | ❌ |
| 17 | 0.436653 | `cosmos_account_list` | ❌ |
| 18 | 0.425231 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.422662 | `aks_cluster_list` | ❌ |
| 20 | 0.420013 | `appconfig_account_list` | ❌ |

---

## Test 114

**Expected Tool:** `functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.448153 | `deploy_app_logs_get` | ❌ |
| 3 | 0.390048 | `mysql_server_list` | ❌ |
| 4 | 0.380314 | `get_bestpractices_get` | ❌ |
| 5 | 0.379655 | `resourcehealth_availability-status_list` | ❌ |
| 6 | 0.376433 | `applens_resource_diagnose` | ❌ |
| 7 | 0.373215 | `deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352982 | `sql_server_list` | ❌ |
| 9 | 0.347628 | `azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.347347 | `quota_usage_check` | ❌ |
| 11 | 0.342763 | `deploy_plan_get` | ❌ |
| 12 | 0.341448 | `datadog_monitoredresources_list` | ❌ |
| 13 | 0.341443 | `resourcehealth_availability-status_get` | ❌ |
| 14 | 0.338591 | `workbooks_list` | ❌ |
| 15 | 0.337465 | `applicationinsights_recommendation_list` | ❌ |
| 16 | 0.333091 | `extension_azqr` | ❌ |
| 17 | 0.328326 | `storage_account_create` | ❌ |
| 18 | 0.323953 | `sql_db_show` | ❌ |
| 19 | 0.322437 | `sql_db_list` | ❌ |
| 20 | 0.317412 | `monitor_resource_log_query` | ❌ |

---

## Test 115

**Expected Tool:** `functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607276 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447400 | `mysql_server_config_get` | ❌ |
| 3 | 0.424693 | `appconfig_account_list` | ❌ |
| 4 | 0.422327 | `deploy_app_logs_get` | ❌ |
| 5 | 0.407133 | `appconfig_kv_show` | ❌ |
| 6 | 0.397977 | `loadtesting_test_get` | ❌ |
| 7 | 0.392852 | `appconfig_kv_list` | ❌ |
| 8 | 0.384151 | `get_bestpractices_get` | ❌ |
| 9 | 0.383957 | `sql_server_show` | ❌ |
| 10 | 0.369436 | `storage_account_get` | ❌ |
| 11 | 0.367183 | `mysql_server_param_get` | ❌ |
| 12 | 0.363307 | `loadtesting_test_create` | ❌ |
| 13 | 0.361753 | `deploy_plan_get` | ❌ |
| 14 | 0.353601 | `appconfig_kv_set` | ❌ |
| 15 | 0.351961 | `sql_db_update` | ❌ |
| 16 | 0.342398 | `postgres_server_config_get` | ❌ |
| 17 | 0.321697 | `quota_usage_check` | ❌ |
| 18 | 0.314950 | `storage_blob_container_get` | ❌ |
| 19 | 0.314079 | `resourcehealth_availability-status_get` | ❌ |
| 20 | 0.312611 | `sql_db_list` | ❌ |

---

## Test 116

**Expected Tool:** `functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.460066 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.420198 | `deploy_app_logs_get` | ❌ |
| 4 | 0.390708 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.334473 | `get_bestpractices_get` | ❌ |
| 6 | 0.322197 | `foundry_models_deployments_list` | ❌ |
| 7 | 0.320055 | `aks_cluster_get` | ❌ |
| 8 | 0.317583 | `quota_usage_check` | ❌ |
| 9 | 0.317359 | `sql_server_show` | ❌ |
| 10 | 0.312732 | `storage_account_get` | ❌ |
| 11 | 0.311384 | `appconfig_account_list` | ❌ |
| 12 | 0.309941 | `loadtesting_testrun_get` | ❌ |
| 13 | 0.305193 | `storage_blob_container_get` | ❌ |
| 14 | 0.297747 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.297135 | `aks_nodepool_get` | ❌ |
| 16 | 0.295538 | `mysql_server_list` | ❌ |
| 17 | 0.295174 | `deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.290156 | `servicebus_queue_details` | ❌ |
| 19 | 0.281639 | `resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.277653 | `mysql_server_config_get` | ❌ |

---

## Test 117

**Expected Tool:** `functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690933 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433973 | `deploy_app_logs_get` | ❌ |
| 3 | 0.432317 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.424646 | `quota_usage_check` | ❌ |
| 5 | 0.419302 | `resourcehealth_availability-status_get` | ❌ |
| 6 | 0.416967 | `mysql_server_list` | ❌ |
| 7 | 0.396163 | `storage_account_get` | ❌ |
| 8 | 0.390791 | `applens_resource_diagnose` | ❌ |
| 9 | 0.389322 | `sql_db_show` | ❌ |
| 10 | 0.387898 | `storage_account_create` | ❌ |
| 11 | 0.383857 | `sql_server_list` | ❌ |
| 12 | 0.383191 | `sql_server_show` | ❌ |
| 13 | 0.378811 | `get_bestpractices_get` | ❌ |
| 14 | 0.376019 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.375268 | `workbooks_show` | ❌ |
| 16 | 0.368506 | `datadog_monitoredresources_list` | ❌ |
| 17 | 0.360165 | `aks_cluster_get` | ❌ |
| 18 | 0.352505 | `applicationinsights_recommendation_list` | ❌ |
| 19 | 0.348610 | `foundry_models_deployments_list` | ❌ |
| 20 | 0.346265 | `group_list` | ❌ |

---

## Test 118

**Expected Tool:** `functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.443440 | `deploy_app_logs_get` | ❌ |
| 3 | 0.441319 | `resourcehealth_availability-status_get` | ❌ |
| 4 | 0.391480 | `sql_server_show` | ❌ |
| 5 | 0.383893 | `resourcehealth_availability-status_list` | ❌ |
| 6 | 0.355527 | `mysql_server_list` | ❌ |
| 7 | 0.353543 | `applens_resource_diagnose` | ❌ |
| 8 | 0.351217 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.349540 | `get_bestpractices_get` | ❌ |
| 10 | 0.347266 | `appconfig_account_list` | ❌ |
| 11 | 0.344702 | `storage_account_get` | ❌ |
| 12 | 0.342868 | `virtualdesktop_hostpool_list` | ❌ |
| 13 | 0.337247 | `quota_usage_check` | ❌ |
| 14 | 0.333000 | `mysql_server_config_get` | ❌ |
| 15 | 0.331119 | `storage_blob_container_get` | ❌ |
| 16 | 0.325680 | `deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.320825 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.319736 | `aks_nodepool_get` | ❌ |
| 19 | 0.318160 | `deploy_plan_get` | ❌ |
| 20 | 0.305803 | `appconfig_kv_show` | ❌ |

---

## Test 119

**Expected Tool:** `functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.445153 | `deploy_app_logs_get` | ❌ |
| 3 | 0.368188 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.366279 | `sql_db_show` | ❌ |
| 5 | 0.365569 | `get_bestpractices_get` | ❌ |
| 6 | 0.363324 | `mysql_server_list` | ❌ |
| 7 | 0.358624 | `deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.352754 | `quota_usage_check` | ❌ |
| 9 | 0.351460 | `aks_cluster_get` | ❌ |
| 10 | 0.350151 | `applens_resource_diagnose` | ❌ |
| 11 | 0.349596 | `storage_account_get` | ❌ |
| 12 | 0.348949 | `resourcehealth_availability-status_get` | ❌ |
| 13 | 0.336938 | `azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.335848 | `datadog_monitoredresources_list` | ❌ |
| 15 | 0.327763 | `eventgrid_subscription_list` | ❌ |
| 16 | 0.325899 | `sql_server_list` | ❌ |
| 17 | 0.325827 | `workbooks_show` | ❌ |
| 18 | 0.323655 | `foundry_models_deployments_list` | ❌ |
| 19 | 0.323377 | `sql_db_list` | ❌ |
| 20 | 0.319790 | `storage_blob_container_get` | ❌ |

---

## Test 120

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.433962 | `deploy_app_logs_get` | ❌ |
| 3 | 0.388678 | `storage_account_get` | ❌ |
| 4 | 0.369779 | `storage_blob_container_get` | ❌ |
| 5 | 0.368021 | `loadtesting_testrun_get` | ❌ |
| 6 | 0.367902 | `storage_blob_get` | ❌ |
| 7 | 0.367552 | `aks_cluster_get` | ❌ |
| 8 | 0.359781 | `keyvault_secret_get` | ❌ |
| 9 | 0.355956 | `sql_db_show` | ❌ |
| 10 | 0.355282 | `search_index_get` | ❌ |
| 11 | 0.352417 | `keyvault_key_get` | ❌ |
| 12 | 0.349891 | `mysql_server_config_get` | ❌ |
| 13 | 0.349476 | `sql_server_show` | ❌ |
| 14 | 0.346974 | `appconfig_kv_show` | ❌ |
| 15 | 0.344067 | `deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.343381 | `get_bestpractices_get` | ❌ |
| 17 | 0.342238 | `servicebus_queue_details` | ❌ |
| 18 | 0.338127 | `aks_nodepool_get` | ❌ |
| 19 | 0.337614 | `marketplace_product_get` | ❌ |
| 20 | 0.326091 | `quota_usage_check` | ❌ |

---

## Test 121

**Expected Tool:** `functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554980 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426703 | `quota_usage_check` | ❌ |
| 3 | 0.418364 | `deploy_app_logs_get` | ❌ |
| 4 | 0.408011 | `deploy_plan_get` | ❌ |
| 5 | 0.381629 | `deploy_architecture_diagram_generate` | ❌ |
| 6 | 0.364785 | `get_bestpractices_get` | ❌ |
| 7 | 0.350663 | `quota_region_availability_list` | ❌ |
| 8 | 0.335606 | `appconfig_account_list` | ❌ |
| 9 | 0.325214 | `applens_resource_diagnose` | ❌ |
| 10 | 0.321466 | `storage_account_get` | ❌ |
| 11 | 0.318517 | `mysql_server_config_get` | ❌ |
| 12 | 0.313831 | `foundry_agents_list` | ❌ |
| 13 | 0.306310 | `eventgrid_subscription_list` | ❌ |
| 14 | 0.304263 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.301769 | `mysql_server_list` | ❌ |
| 16 | 0.281401 | `resourcehealth_availability-status_list` | ❌ |
| 17 | 0.277916 | `resourcehealth_availability-status_get` | ❌ |
| 18 | 0.267215 | `marketplace_product_get` | ❌ |
| 19 | 0.267170 | `search_service_list` | ❌ |
| 20 | 0.266494 | `monitor_resource_log_query` | ❌ |

---

## Test 122

**Expected Tool:** `functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565797 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.440299 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.422799 | `deploy_app_logs_get` | ❌ |
| 4 | 0.384159 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.342552 | `get_bestpractices_get` | ❌ |
| 6 | 0.333621 | `quota_usage_check` | ❌ |
| 7 | 0.319464 | `deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.318008 | `applens_resource_diagnose` | ❌ |
| 9 | 0.310636 | `azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.298434 | `foundry_models_deployments_list` | ❌ |
| 11 | 0.297073 | `deploy_plan_get` | ❌ |
| 12 | 0.295694 | `foundry_agents_list` | ❌ |
| 13 | 0.292793 | `cloudarchitect_design` | ❌ |
| 14 | 0.283653 | `sql_server_show` | ❌ |
| 15 | 0.272348 | `storage_account_get` | ❌ |
| 16 | 0.270846 | `mysql_server_list` | ❌ |
| 17 | 0.267087 | `resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.265948 | `storage_blob_container_get` | ❌ |
| 19 | 0.258431 | `search_service_list` | ❌ |
| 20 | 0.241875 | `sql_db_list` | ❌ |

---

## Test 123

**Expected Tool:** `functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.559382 | `search_service_list` | ❌ |
| 3 | 0.516618 | `cosmos_account_list` | ❌ |
| 4 | 0.516217 | `appconfig_account_list` | ❌ |
| 5 | 0.485259 | `subscription_list` | ❌ |
| 6 | 0.474425 | `kusto_cluster_list` | ❌ |
| 7 | 0.465690 | `group_list` | ❌ |
| 8 | 0.464534 | `monitor_workspace_list` | ❌ |
| 9 | 0.462296 | `foundry_agents_list` | ❌ |
| 10 | 0.455851 | `aks_cluster_list` | ❌ |
| 11 | 0.455388 | `postgres_server_list` | ❌ |
| 12 | 0.445087 | `redis_cache_list` | ❌ |
| 13 | 0.442614 | `redis_cluster_list` | ❌ |
| 14 | 0.433245 | `eventgrid_topic_list` | ❌ |
| 15 | 0.432144 | `grafana_list` | ❌ |
| 16 | 0.431611 | `resourcehealth_availability-status_list` | ❌ |
| 17 | 0.429253 | `eventgrid_subscription_list` | ❌ |
| 18 | 0.417070 | `sql_server_list` | ❌ |
| 19 | 0.413034 | `virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.411904 | `sql_db_list` | ❌ |

---

## Test 124

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.452156 | `deploy_app_logs_get` | ❌ |
| 3 | 0.436167 | `foundry_agents_list` | ❌ |
| 4 | 0.412646 | `search_service_list` | ❌ |
| 5 | 0.411323 | `get_bestpractices_get` | ❌ |
| 6 | 0.385832 | `foundry_models_deployments_list` | ❌ |
| 7 | 0.374655 | `appconfig_account_list` | ❌ |
| 8 | 0.372790 | `cosmos_account_list` | ❌ |
| 9 | 0.370393 | `mysql_server_list` | ❌ |
| 10 | 0.369681 | `subscription_list` | ❌ |
| 11 | 0.368004 | `deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.358720 | `deploy_plan_get` | ❌ |
| 13 | 0.357329 | `quota_usage_check` | ❌ |
| 14 | 0.347887 | `mysql_database_list` | ❌ |
| 15 | 0.347802 | `azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.339873 | `storage_account_get` | ❌ |
| 17 | 0.334019 | `role_assignment_list` | ❌ |
| 18 | 0.333136 | `sql_db_list` | ❌ |
| 19 | 0.327870 | `monitor_workspace_list` | ❌ |
| 20 | 0.327326 | `sql_server_list` | ❌ |

---

## Test 125

**Expected Tool:** `functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433674 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.348099 | `deploy_app_logs_get` | ❌ |
| 3 | 0.284362 | `get_bestpractices_get` | ❌ |
| 4 | 0.281607 | `applens_resource_diagnose` | ❌ |
| 5 | 0.249658 | `appconfig_account_list` | ❌ |
| 6 | 0.244782 | `appconfig_kv_list` | ❌ |
| 7 | 0.240729 | `deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.239514 | `foundry_models_deployments_list` | ❌ |
| 9 | 0.217775 | `azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.213974 | `foundry_agents_list` | ❌ |
| 11 | 0.207391 | `quota_usage_check` | ❌ |
| 12 | 0.197655 | `mysql_server_list` | ❌ |
| 13 | 0.195857 | `role_assignment_list` | ❌ |
| 14 | 0.194372 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.186328 | `monitor_resource_log_query` | ❌ |
| 16 | 0.184120 | `monitor_workspace_list` | ❌ |
| 17 | 0.184051 | `resourcehealth_availability-status_list` | ❌ |
| 18 | 0.179069 | `mysql_database_list` | ❌ |
| 19 | 0.178961 | `search_service_list` | ❌ |
| 20 | 0.175281 | `subscription_list` | ❌ |

---

## Test 126

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627872 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570318 | `keyvault_certificate_import` | ❌ |
| 3 | 0.540199 | `keyvault_key_create` | ❌ |
| 4 | 0.519218 | `keyvault_certificate_get` | ❌ |
| 5 | 0.500111 | `keyvault_certificate_list` | ❌ |
| 6 | 0.474019 | `keyvault_secret_create` | ❌ |
| 7 | 0.372046 | `storage_account_create` | ❌ |
| 8 | 0.370863 | `keyvault_key_get` | ❌ |
| 9 | 0.352953 | `sql_db_create` | ❌ |
| 10 | 0.348935 | `keyvault_secret_get` | ❌ |
| 11 | 0.345316 | `keyvault_key_list` | ❌ |
| 12 | 0.330026 | `appconfig_kv_set` | ❌ |
| 13 | 0.296673 | `sql_server_create` | ❌ |
| 14 | 0.285184 | `workbooks_create` | ❌ |
| 15 | 0.267718 | `storage_account_get` | ❌ |
| 16 | 0.237081 | `storage_blob_container_create` | ❌ |
| 17 | 0.222638 | `storage_blob_container_get` | ❌ |
| 18 | 0.219479 | `subscription_list` | ❌ |
| 19 | 0.217086 | `search_service_list` | ❌ |
| 20 | 0.208862 | `workbooks_delete` | ❌ |

---

## Test 127

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600125 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561459 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522743 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502052 | `keyvault_key_create` | ❌ |
| 5 | 0.497280 | `keyvault_certificate_list` | ❌ |
| 6 | 0.439914 | `keyvault_secret_create` | ❌ |
| 7 | 0.396480 | `keyvault_key_get` | ❌ |
| 8 | 0.365355 | `keyvault_secret_get` | ❌ |
| 9 | 0.355331 | `storage_account_create` | ❌ |
| 10 | 0.348085 | `keyvault_key_list` | ❌ |
| 11 | 0.317241 | `sql_db_create` | ❌ |
| 12 | 0.306422 | `appconfig_kv_set` | ❌ |
| 13 | 0.293034 | `storage_account_get` | ❌ |
| 14 | 0.271837 | `workbooks_create` | ❌ |
| 15 | 0.260731 | `sql_server_create` | ❌ |
| 16 | 0.252010 | `workbooks_delete` | ❌ |
| 17 | 0.245574 | `storage_blob_container_get` | ❌ |
| 18 | 0.242550 | `subscription_list` | ❌ |
| 19 | 0.240000 | `virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.228831 | `search_service_list` | ❌ |

---

## Test 128

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574214 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527759 | `keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `keyvault_certificate_get` | ❌ |
| 4 | 0.481548 | `keyvault_key_create` | ❌ |
| 5 | 0.469703 | `keyvault_certificate_list` | ❌ |
| 6 | 0.408030 | `keyvault_secret_create` | ❌ |
| 7 | 0.367681 | `keyvault_key_get` | ❌ |
| 8 | 0.359121 | `storage_account_create` | ❌ |
| 9 | 0.334271 | `keyvault_secret_get` | ❌ |
| 10 | 0.322220 | `keyvault_key_list` | ❌ |
| 11 | 0.306781 | `appconfig_kv_set` | ❌ |
| 12 | 0.304171 | `sql_db_create` | ❌ |
| 13 | 0.294438 | `storage_account_get` | ❌ |
| 14 | 0.260984 | `sql_server_create` | ❌ |
| 15 | 0.260723 | `workbooks_create` | ❌ |
| 16 | 0.252172 | `storage_blob_container_get` | ❌ |
| 17 | 0.229588 | `storage_blob_container_create` | ❌ |
| 18 | 0.228728 | `virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.225536 | `search_index_get` | ❌ |
| 20 | 0.223368 | `subscription_list` | ❌ |

---

## Test 129

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592062 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.562265 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522147 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502529 | `keyvault_key_create` | ❌ |
| 5 | 0.480082 | `keyvault_certificate_list` | ❌ |
| 6 | 0.432097 | `keyvault_secret_create` | ❌ |
| 7 | 0.386798 | `keyvault_key_get` | ❌ |
| 8 | 0.348511 | `keyvault_key_list` | ❌ |
| 9 | 0.346112 | `keyvault_secret_get` | ❌ |
| 10 | 0.317684 | `appconfig_kv_set` | ❌ |
| 11 | 0.302700 | `storage_account_create` | ❌ |
| 12 | 0.301808 | `sql_db_create` | ❌ |
| 13 | 0.272932 | `storage_account_get` | ❌ |
| 14 | 0.241208 | `sql_server_create` | ❌ |
| 15 | 0.237598 | `workbooks_create` | ❌ |
| 16 | 0.232434 | `search_service_list` | ❌ |
| 17 | 0.232147 | `storage_blob_container_get` | ❌ |
| 18 | 0.228768 | `workbooks_delete` | ❌ |
| 19 | 0.218138 | `search_index_get` | ❌ |
| 20 | 0.217559 | `mysql_server_list` | ❌ |

---

## Test 130

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622998 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.558532 | `keyvault_certificate_import` | ❌ |
| 3 | 0.534503 | `keyvault_certificate_get` | ❌ |
| 4 | 0.521426 | `keyvault_certificate_list` | ❌ |
| 5 | 0.465056 | `keyvault_key_create` | ❌ |
| 6 | 0.410000 | `keyvault_secret_create` | ❌ |
| 7 | 0.390046 | `keyvault_key_get` | ❌ |
| 8 | 0.356917 | `keyvault_secret_get` | ❌ |
| 9 | 0.354429 | `keyvault_key_list` | ❌ |
| 10 | 0.311559 | `appconfig_kv_lock_set` | ❌ |
| 11 | 0.292534 | `storage_account_create` | ❌ |
| 12 | 0.291440 | `storage_account_get` | ❌ |
| 13 | 0.281865 | `sql_db_create` | ❌ |
| 14 | 0.257221 | `storage_blob_container_get` | ❌ |
| 15 | 0.251631 | `subscription_list` | ❌ |
| 16 | 0.251493 | `search_service_list` | ❌ |
| 17 | 0.250229 | `search_index_get` | ❌ |
| 18 | 0.243511 | `workbooks_delete` | ❌ |
| 19 | 0.234014 | `role_assignment_list` | ❌ |
| 20 | 0.233404 | `virtualdesktop_hostpool_list` | ❌ |

---

## Test 131

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600625 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528484 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519037 | `keyvault_certificate_import` | ❌ |
| 4 | 0.499634 | `keyvault_certificate_create` | ❌ |
| 5 | 0.486609 | `keyvault_key_get` | ❌ |
| 6 | 0.450716 | `keyvault_secret_get` | ❌ |
| 7 | 0.390699 | `appconfig_kv_show` | ❌ |
| 8 | 0.383271 | `keyvault_key_create` | ❌ |
| 9 | 0.379434 | `keyvault_key_list` | ❌ |
| 10 | 0.359751 | `storage_account_get` | ❌ |
| 11 | 0.346167 | `cosmos_account_list` | ❌ |
| 12 | 0.318305 | `storage_blob_container_get` | ❌ |
| 13 | 0.293421 | `subscription_list` | ❌ |
| 14 | 0.289685 | `search_service_list` | ❌ |
| 15 | 0.279695 | `search_index_get` | ❌ |
| 16 | 0.276581 | `role_assignment_list` | ❌ |
| 17 | 0.271911 | `quota_usage_check` | ❌ |
| 18 | 0.269735 | `sql_db_show` | ❌ |
| 19 | 0.266478 | `virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.263141 | `storage_account_create` | ❌ |

---

## Test 132

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646098 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.562988 | `keyvault_key_get` | ❌ |
| 3 | 0.514170 | `keyvault_secret_get` | ❌ |
| 4 | 0.509504 | `keyvault_certificate_list` | ❌ |
| 5 | 0.507737 | `keyvault_certificate_import` | ❌ |
| 6 | 0.485573 | `keyvault_certificate_create` | ❌ |
| 7 | 0.459167 | `storage_account_get` | ❌ |
| 8 | 0.417886 | `storage_blob_container_get` | ❌ |
| 9 | 0.411136 | `appconfig_kv_show` | ❌ |
| 10 | 0.392729 | `keyvault_key_create` | ❌ |
| 11 | 0.381410 | `keyvault_key_list` | ❌ |
| 12 | 0.368360 | `search_index_get` | ❌ |
| 13 | 0.365386 | `sql_db_show` | ❌ |
| 14 | 0.363228 | `aks_cluster_get` | ❌ |
| 15 | 0.350193 | `storage_blob_get` | ❌ |
| 16 | 0.332770 | `mysql_server_config_get` | ❌ |
| 17 | 0.331645 | `sql_server_show` | ❌ |
| 18 | 0.317884 | `marketplace_product_get` | ❌ |
| 19 | 0.305778 | `subscription_list` | ❌ |
| 20 | 0.301710 | `servicebus_queue_details` | ❌ |

---

## Test 133

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609523 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.515661 | `keyvault_certificate_list` | ❌ |
| 3 | 0.511443 | `keyvault_certificate_create` | ❌ |
| 4 | 0.507768 | `keyvault_certificate_import` | ❌ |
| 5 | 0.474394 | `keyvault_key_get` | ❌ |
| 6 | 0.445807 | `keyvault_secret_get` | ❌ |
| 7 | 0.387472 | `keyvault_key_create` | ❌ |
| 8 | 0.356616 | `storage_account_get` | ❌ |
| 9 | 0.352578 | `appconfig_kv_show` | ❌ |
| 10 | 0.351633 | `keyvault_secret_create` | ❌ |
| 11 | 0.350108 | `keyvault_key_list` | ❌ |
| 12 | 0.315724 | `storage_blob_container_get` | ❌ |
| 13 | 0.294397 | `storage_account_create` | ❌ |
| 14 | 0.269229 | `monitor_healthmodels_entity_gethealth` | ❌ |
| 15 | 0.262046 | `sql_db_show` | ❌ |
| 16 | 0.261616 | `subscription_list` | ❌ |
| 17 | 0.252793 | `search_index_get` | ❌ |
| 18 | 0.248508 | `sql_server_show` | ❌ |
| 19 | 0.239849 | `search_service_list` | ❌ |
| 20 | 0.237997 | `storage_blob_get` | ❌ |

---

## Test 134

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647669 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527400 | `keyvault_key_get` | ❌ |
| 3 | 0.521623 | `keyvault_certificate_list` | ❌ |
| 4 | 0.509796 | `keyvault_certificate_import` | ❌ |
| 5 | 0.501988 | `keyvault_secret_get` | ❌ |
| 6 | 0.485855 | `keyvault_certificate_create` | ❌ |
| 7 | 0.440760 | `storage_account_get` | ❌ |
| 8 | 0.409563 | `storage_blob_container_get` | ❌ |
| 9 | 0.405442 | `appconfig_kv_show` | ❌ |
| 10 | 0.371542 | `keyvault_key_list` | ❌ |
| 11 | 0.359834 | `keyvault_key_create` | ❌ |
| 12 | 0.355790 | `search_index_get` | ❌ |
| 13 | 0.347809 | `cosmos_account_list` | ❌ |
| 14 | 0.346992 | `storage_blob_get` | ❌ |
| 15 | 0.344642 | `sql_db_show` | ❌ |
| 16 | 0.320533 | `role_assignment_list` | ❌ |
| 17 | 0.312231 | `sql_server_show` | ❌ |
| 18 | 0.302716 | `mysql_server_config_get` | ❌ |
| 19 | 0.296019 | `mysql_table_schema_get` | ❌ |
| 20 | 0.281628 | `servicebus_queue_details` | ❌ |

---

## Test 135

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595959 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527473 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519059 | `keyvault_certificate_import` | ❌ |
| 4 | 0.501564 | `keyvault_certificate_create` | ❌ |
| 5 | 0.465174 | `keyvault_key_get` | ❌ |
| 6 | 0.429174 | `keyvault_secret_get` | ❌ |
| 7 | 0.369183 | `keyvault_key_list` | ❌ |
| 8 | 0.368025 | `keyvault_key_create` | ❌ |
| 9 | 0.365084 | `storage_blob_container_get` | ❌ |
| 10 | 0.361116 | `storage_account_get` | ❌ |
| 11 | 0.343934 | `appconfig_kv_show` | ❌ |
| 12 | 0.328187 | `storage_blob_get` | ❌ |
| 13 | 0.321769 | `keyvault_secret_create` | ❌ |
| 14 | 0.311060 | `search_index_get` | ❌ |
| 15 | 0.305979 | `monitor_metrics_definitions` | ❌ |
| 16 | 0.295477 | `mysql_table_schema_get` | ❌ |
| 17 | 0.288678 | `marketplace_product_get` | ❌ |
| 18 | 0.288660 | `monitor_metrics_query` | ❌ |
| 19 | 0.287497 | `workbooks_show` | ❌ |
| 20 | 0.286224 | `mysql_server_config_get` | ❌ |

---

## Test 136

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585481 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420747 | `keyvault_certificate_get` | ❌ |
| 3 | 0.402804 | `keyvault_certificate_create` | ❌ |
| 4 | 0.399427 | `keyvault_certificate_list` | ❌ |
| 5 | 0.352905 | `keyvault_key_create` | ❌ |
| 6 | 0.336830 | `keyvault_secret_create` | ❌ |
| 7 | 0.325224 | `keyvault_key_get` | ❌ |
| 8 | 0.289124 | `keyvault_secret_get` | ❌ |
| 9 | 0.283424 | `keyvault_key_list` | ❌ |
| 10 | 0.272263 | `appconfig_kv_lock_set` | ❌ |
| 11 | 0.248212 | `storage_blob_upload` | ❌ |
| 12 | 0.228508 | `workbooks_delete` | ❌ |
| 13 | 0.222971 | `storage_account_get` | ❌ |
| 14 | 0.205023 | `storage_account_create` | ❌ |
| 15 | 0.181129 | `storage_blob_container_get` | ❌ |
| 16 | 0.180219 | `sql_db_create` | ❌ |
| 17 | 0.174606 | `monitor_resource_log_query` | ❌ |
| 18 | 0.170326 | `subscription_list` | ❌ |
| 19 | 0.158491 | `virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.153106 | `search_service_list` | ❌ |

---

## Test 137

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622125 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.504314 | `keyvault_certificate_get` | ❌ |
| 3 | 0.498982 | `keyvault_certificate_create` | ❌ |
| 4 | 0.448218 | `keyvault_certificate_list` | ❌ |
| 5 | 0.419811 | `keyvault_key_create` | ❌ |
| 6 | 0.392959 | `keyvault_secret_create` | ❌ |
| 7 | 0.349395 | `keyvault_key_get` | ❌ |
| 8 | 0.320194 | `keyvault_secret_get` | ❌ |
| 9 | 0.304930 | `keyvault_key_list` | ❌ |
| 10 | 0.287107 | `appconfig_kv_set` | ❌ |
| 11 | 0.259893 | `sql_db_create` | ❌ |
| 12 | 0.256832 | `storage_account_create` | ❌ |
| 13 | 0.250432 | `storage_account_get` | ❌ |
| 14 | 0.233767 | `workbooks_delete` | ❌ |
| 15 | 0.210905 | `storage_blob_container_get` | ❌ |
| 16 | 0.209234 | `storage_blob_upload` | ❌ |
| 17 | 0.203672 | `sql_server_create` | ❌ |
| 18 | 0.197598 | `sql_db_show` | ❌ |
| 19 | 0.196937 | `workbooks_create` | ❌ |
| 20 | 0.189634 | `sql_server_delete` | ❌ |

---

## Test 138

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595707 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.454041 | `keyvault_certificate_create` | ❌ |
| 3 | 0.452551 | `keyvault_certificate_get` | ❌ |
| 4 | 0.418310 | `keyvault_certificate_list` | ❌ |
| 5 | 0.413377 | `keyvault_key_create` | ❌ |
| 6 | 0.399690 | `keyvault_secret_create` | ❌ |
| 7 | 0.365954 | `keyvault_key_get` | ❌ |
| 8 | 0.351043 | `appconfig_kv_set` | ❌ |
| 9 | 0.331651 | `appconfig_kv_lock_set` | ❌ |
| 10 | 0.330571 | `keyvault_secret_get` | ❌ |
| 11 | 0.316126 | `storage_blob_upload` | ❌ |
| 12 | 0.280997 | `storage_account_get` | ❌ |
| 13 | 0.254835 | `workbooks_delete` | ❌ |
| 14 | 0.250270 | `storage_account_create` | ❌ |
| 15 | 0.218097 | `storage_blob_container_get` | ❌ |
| 16 | 0.211580 | `subscription_list` | ❌ |
| 17 | 0.204904 | `sql_db_create` | ❌ |
| 18 | 0.189527 | `monitor_resource_log_query` | ❌ |
| 19 | 0.187649 | `sql_db_update` | ❌ |
| 20 | 0.181210 | `workbooks_create` | ❌ |

---

## Test 139

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619480 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.517804 | `keyvault_certificate_get` | ❌ |
| 3 | 0.480907 | `keyvault_certificate_create` | ❌ |
| 4 | 0.444483 | `keyvault_certificate_list` | ❌ |
| 5 | 0.381873 | `keyvault_key_create` | ❌ |
| 6 | 0.365641 | `keyvault_key_get` | ❌ |
| 7 | 0.358637 | `keyvault_secret_create` | ❌ |
| 8 | 0.341569 | `keyvault_secret_get` | ❌ |
| 9 | 0.279774 | `keyvault_key_list` | ❌ |
| 10 | 0.274910 | `appconfig_kv_lock_set` | ❌ |
| 11 | 0.243122 | `storage_account_get` | ❌ |
| 12 | 0.224245 | `workbooks_delete` | ❌ |
| 13 | 0.209976 | `storage_account_create` | ❌ |
| 14 | 0.207206 | `storage_blob_upload` | ❌ |
| 15 | 0.189880 | `sql_db_create` | ❌ |
| 16 | 0.187278 | `storage_blob_container_get` | ❌ |
| 17 | 0.175139 | `subscription_list` | ❌ |
| 18 | 0.174460 | `sql_db_show` | ❌ |
| 19 | 0.172686 | `monitor_resource_log_query` | ❌ |
| 20 | 0.167599 | `monitor_healthmodels_entity_gethealth` | ❌ |

---

## Test 140

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595418 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452544 | `keyvault_certificate_create` | ❌ |
| 3 | 0.441653 | `keyvault_certificate_get` | ❌ |
| 4 | 0.408031 | `keyvault_key_create` | ❌ |
| 5 | 0.392277 | `keyvault_secret_create` | ❌ |
| 6 | 0.390758 | `keyvault_certificate_list` | ❌ |
| 7 | 0.353230 | `appconfig_kv_set` | ❌ |
| 8 | 0.325428 | `keyvault_key_get` | ❌ |
| 9 | 0.306406 | `appconfig_kv_lock_set` | ❌ |
| 10 | 0.305748 | `appservice_database_add` | ❌ |
| 11 | 0.235766 | `storage_blob_upload` | ❌ |
| 12 | 0.230271 | `storage_account_get` | ❌ |
| 13 | 0.227373 | `storage_account_create` | ❌ |
| 14 | 0.221101 | `sql_db_create` | ❌ |
| 15 | 0.196149 | `workbooks_delete` | ❌ |
| 16 | 0.182997 | `sql_db_update` | ❌ |
| 17 | 0.182012 | `storage_blob_container_get` | ❌ |
| 18 | 0.161890 | `workbooks_create` | ❌ |
| 19 | 0.161597 | `sql_server_create` | ❌ |
| 20 | 0.159675 | `subscription_list` | ❌ |

---

## Test 141

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726225 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.583110 | `keyvault_key_list` | ❌ |
| 3 | 0.531988 | `keyvault_secret_list` | ❌ |
| 4 | 0.515236 | `keyvault_certificate_get` | ❌ |
| 5 | 0.486236 | `keyvault_certificate_create` | ❌ |
| 6 | 0.478100 | `cosmos_account_list` | ❌ |
| 7 | 0.453226 | `cosmos_database_list` | ❌ |
| 8 | 0.449821 | `keyvault_certificate_import` | ❌ |
| 9 | 0.431201 | `cosmos_database_container_list` | ❌ |
| 10 | 0.418995 | `keyvault_key_get` | ❌ |
| 11 | 0.408042 | `subscription_list` | ❌ |
| 12 | 0.394434 | `search_service_list` | ❌ |
| 13 | 0.393940 | `storage_account_get` | ❌ |
| 14 | 0.362953 | `storage_blob_container_get` | ❌ |
| 15 | 0.362873 | `virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.358938 | `role_assignment_list` | ❌ |
| 17 | 0.350862 | `mysql_database_list` | ❌ |
| 18 | 0.339860 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 19 | 0.336796 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 20 | 0.330749 | `redis_cache_list` | ❌ |

---

## Test 142

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615623 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.522453 | `keyvault_certificate_get` | ❌ |
| 3 | 0.475156 | `keyvault_key_list` | ❌ |
| 4 | 0.461455 | `keyvault_certificate_create` | ❌ |
| 5 | 0.448139 | `keyvault_key_get` | ❌ |
| 6 | 0.446003 | `keyvault_certificate_import` | ❌ |
| 7 | 0.433093 | `keyvault_secret_list` | ❌ |
| 8 | 0.422019 | `keyvault_secret_get` | ❌ |
| 9 | 0.420506 | `cosmos_account_list` | ❌ |
| 10 | 0.397031 | `storage_account_get` | ❌ |
| 11 | 0.382082 | `cosmos_database_list` | ❌ |
| 12 | 0.362782 | `subscription_list` | ❌ |
| 13 | 0.355022 | `storage_blob_container_get` | ❌ |
| 14 | 0.344466 | `search_service_list` | ❌ |
| 15 | 0.323177 | `role_assignment_list` | ❌ |
| 16 | 0.309942 | `virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.305651 | `mysql_database_list` | ❌ |
| 18 | 0.295917 | `quota_usage_check` | ❌ |
| 19 | 0.290719 | `search_index_get` | ❌ |
| 20 | 0.286707 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 143

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624781 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.519739 | `keyvault_certificate_get` | ❌ |
| 3 | 0.510463 | `keyvault_certificate_create` | ❌ |
| 4 | 0.505534 | `keyvault_certificate_import` | ❌ |
| 5 | 0.497356 | `keyvault_key_list` | ❌ |
| 6 | 0.442299 | `keyvault_secret_list` | ❌ |
| 7 | 0.429075 | `keyvault_key_get` | ❌ |
| 8 | 0.418505 | `keyvault_key_create` | ❌ |
| 9 | 0.410237 | `keyvault_secret_get` | ❌ |
| 10 | 0.373641 | `cosmos_account_list` | ❌ |
| 11 | 0.367613 | `storage_account_get` | ❌ |
| 12 | 0.344813 | `search_service_list` | ❌ |
| 13 | 0.344599 | `storage_blob_container_get` | ❌ |
| 14 | 0.332664 | `subscription_list` | ❌ |
| 15 | 0.303707 | `virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.297482 | `redis_cache_list` | ❌ |
| 17 | 0.294830 | `search_index_get` | ❌ |
| 18 | 0.292905 | `monitor_table_type_list` | ❌ |
| 19 | 0.287082 | `role_assignment_list` | ❌ |
| 20 | 0.279813 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 144

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672715 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.553990 | `keyvault_key_list` | ❌ |
| 3 | 0.511905 | `keyvault_secret_list` | ❌ |
| 4 | 0.507062 | `keyvault_certificate_get` | ❌ |
| 5 | 0.492720 | `keyvault_certificate_create` | ❌ |
| 6 | 0.450104 | `keyvault_certificate_import` | ❌ |
| 7 | 0.447793 | `cosmos_account_list` | ❌ |
| 8 | 0.413220 | `cosmos_database_list` | ❌ |
| 9 | 0.406736 | `cosmos_database_container_list` | ❌ |
| 10 | 0.398143 | `keyvault_key_get` | ❌ |
| 11 | 0.387661 | `storage_account_get` | ❌ |
| 12 | 0.386498 | `subscription_list` | ❌ |
| 13 | 0.379358 | `search_service_list` | ❌ |
| 14 | 0.361476 | `monitor_table_type_list` | ❌ |
| 15 | 0.357200 | `storage_blob_container_get` | ❌ |
| 16 | 0.355418 | `role_assignment_list` | ❌ |
| 17 | 0.346059 | `mysql_database_list` | ❌ |
| 18 | 0.343744 | `monitor_workspace_list` | ❌ |
| 19 | 0.338369 | `virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.332920 | `monitor_table_list` | ❌ |

---

## Test 145

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747513 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.594216 | `keyvault_key_list` | ❌ |
| 3 | 0.558771 | `keyvault_secret_list` | ❌ |
| 4 | 0.515568 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491385 | `keyvault_certificate_create` | ❌ |
| 6 | 0.460786 | `keyvault_certificate_import` | ❌ |
| 7 | 0.426252 | `cosmos_account_list` | ❌ |
| 8 | 0.399392 | `keyvault_key_get` | ❌ |
| 9 | 0.380821 | `cosmos_database_container_list` | ❌ |
| 10 | 0.378614 | `cosmos_database_list` | ❌ |
| 11 | 0.378269 | `subscription_list` | ❌ |
| 12 | 0.374340 | `search_service_list` | ❌ |
| 13 | 0.365517 | `storage_account_get` | ❌ |
| 14 | 0.348720 | `mysql_table_list` | ❌ |
| 15 | 0.343591 | `storage_blob_container_get` | ❌ |
| 16 | 0.328744 | `virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.323968 | `role_assignment_list` | ❌ |
| 18 | 0.317045 | `mysql_server_list` | ❌ |
| 19 | 0.312041 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.309550 | `mysql_database_list` | ❌ |

---

## Test 146

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639788 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.512471 | `keyvault_certificate_get` | ❌ |
| 3 | 0.507559 | `keyvault_key_list` | ❌ |
| 4 | 0.483041 | `keyvault_certificate_create` | ❌ |
| 5 | 0.464713 | `keyvault_secret_list` | ❌ |
| 6 | 0.455872 | `keyvault_certificate_import` | ❌ |
| 7 | 0.448957 | `cosmos_account_list` | ❌ |
| 8 | 0.405319 | `keyvault_key_get` | ❌ |
| 9 | 0.403553 | `cosmos_database_list` | ❌ |
| 10 | 0.403266 | `cosmos_database_container_list` | ❌ |
| 11 | 0.401215 | `storage_account_get` | ❌ |
| 12 | 0.395025 | `subscription_list` | ❌ |
| 13 | 0.369554 | `storage_blob_container_get` | ❌ |
| 14 | 0.360162 | `search_service_list` | ❌ |
| 15 | 0.342555 | `mysql_database_list` | ❌ |
| 16 | 0.341598 | `virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.340827 | `role_assignment_list` | ❌ |
| 18 | 0.340528 | `monitor_table_type_list` | ❌ |
| 19 | 0.326085 | `monitor_workspace_list` | ❌ |
| 20 | 0.322559 | `mysql_server_list` | ❌ |

---

## Test 147

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661429 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456549 | `keyvault_secret_create` | ❌ |
| 3 | 0.451805 | `keyvault_certificate_create` | ❌ |
| 4 | 0.429578 | `keyvault_certificate_import` | ❌ |
| 5 | 0.399288 | `keyvault_key_get` | ❌ |
| 6 | 0.397170 | `appconfig_kv_set` | ❌ |
| 7 | 0.375846 | `keyvault_key_list` | ❌ |
| 8 | 0.372087 | `storage_account_create` | ❌ |
| 9 | 0.348165 | `keyvault_certificate_list` | ❌ |
| 10 | 0.338066 | `sql_db_create` | ❌ |
| 11 | 0.335249 | `appconfig_kv_lock_set` | ❌ |
| 12 | 0.330489 | `keyvault_certificate_get` | ❌ |
| 13 | 0.283856 | `sql_server_create` | ❌ |
| 14 | 0.276171 | `storage_account_get` | ❌ |
| 15 | 0.261807 | `workbooks_create` | ❌ |
| 16 | 0.230099 | `storage_blob_container_get` | ❌ |
| 17 | 0.223638 | `storage_blob_container_create` | ❌ |
| 18 | 0.215833 | `subscription_list` | ❌ |
| 19 | 0.212007 | `monitor_resource_log_query` | ❌ |
| 20 | 0.199618 | `sql_server_firewall-rule_create` | ❌ |

---

## Test 148

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641070 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428502 | `keyvault_key_get` | ❌ |
| 3 | 0.422921 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420045 | `keyvault_secret_create` | ❌ |
| 5 | 0.405644 | `appconfig_kv_set` | ❌ |
| 6 | 0.392529 | `keyvault_certificate_import` | ❌ |
| 7 | 0.385990 | `appconfig_kv_lock_set` | ❌ |
| 8 | 0.380906 | `keyvault_key_list` | ❌ |
| 9 | 0.347115 | `appconfig_kv_show` | ❌ |
| 10 | 0.328934 | `keyvault_secret_get` | ❌ |
| 11 | 0.294628 | `storage_account_create` | ❌ |
| 12 | 0.255583 | `storage_account_get` | ❌ |
| 13 | 0.234445 | `sql_db_create` | ❌ |
| 14 | 0.224488 | `monitor_table_type_list` | ❌ |
| 15 | 0.217599 | `storage_blob_container_get` | ❌ |
| 16 | 0.216636 | `subscription_list` | ❌ |
| 17 | 0.210036 | `quota_region_availability_list` | ❌ |
| 18 | 0.198030 | `monitor_resource_log_query` | ❌ |
| 19 | 0.185361 | `storage_blob_container_create` | ❌ |
| 20 | 0.183409 | `search_index_get` | ❌ |

---

## Test 149

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547493 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.463557 | `keyvault_secret_create` | ❌ |
| 3 | 0.447581 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420366 | `keyvault_key_get` | ❌ |
| 5 | 0.404350 | `keyvault_certificate_import` | ❌ |
| 6 | 0.398670 | `keyvault_key_list` | ❌ |
| 7 | 0.384348 | `keyvault_secret_get` | ❌ |
| 8 | 0.369976 | `keyvault_certificate_list` | ❌ |
| 9 | 0.349688 | `keyvault_secret_list` | ❌ |
| 10 | 0.346151 | `appconfig_kv_set` | ❌ |
| 11 | 0.306311 | `storage_account_create` | ❌ |
| 12 | 0.277139 | `sql_db_create` | ❌ |
| 13 | 0.251124 | `storage_account_get` | ❌ |
| 14 | 0.230824 | `subscription_list` | ❌ |
| 15 | 0.227708 | `monitor_resource_log_query` | ❌ |
| 16 | 0.226595 | `workbooks_delete` | ❌ |
| 17 | 0.224872 | `storage_blob_container_create` | ❌ |
| 18 | 0.223473 | `sql_server_create` | ❌ |
| 19 | 0.221834 | `virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.219613 | `workbooks_create` | ❌ |

---

## Test 150

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641369 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.501636 | `keyvault_secret_create` | ❌ |
| 3 | 0.491825 | `keyvault_certificate_create` | ❌ |
| 4 | 0.464557 | `keyvault_certificate_import` | ❌ |
| 5 | 0.451016 | `keyvault_key_get` | ❌ |
| 6 | 0.391097 | `keyvault_key_list` | ❌ |
| 7 | 0.380663 | `storage_account_create` | ❌ |
| 8 | 0.379135 | `keyvault_secret_get` | ❌ |
| 9 | 0.375945 | `keyvault_certificate_get` | ❌ |
| 10 | 0.368655 | `keyvault_certificate_list` | ❌ |
| 11 | 0.358426 | `appconfig_kv_set` | ❌ |
| 12 | 0.344933 | `sql_db_create` | ❌ |
| 13 | 0.293393 | `sql_server_create` | ❌ |
| 14 | 0.287655 | `workbooks_create` | ❌ |
| 15 | 0.275938 | `storage_account_get` | ❌ |
| 16 | 0.235474 | `storage_blob_container_get` | ❌ |
| 17 | 0.234865 | `storage_blob_container_create` | ❌ |
| 18 | 0.226237 | `sql_server_firewall-rule_create` | ❌ |
| 19 | 0.224445 | `workbooks_delete` | ❌ |
| 20 | 0.221689 | `monitor_resource_log_query` | ❌ |

---

## Test 151

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571700 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443483 | `keyvault_certificate_create` | ❌ |
| 3 | 0.434425 | `keyvault_secret_create` | ❌ |
| 4 | 0.422040 | `keyvault_key_get` | ❌ |
| 5 | 0.400377 | `keyvault_certificate_import` | ❌ |
| 6 | 0.370799 | `keyvault_key_list` | ❌ |
| 7 | 0.367662 | `appconfig_kv_lock_set` | ❌ |
| 8 | 0.357332 | `appconfig_kv_set` | ❌ |
| 9 | 0.349930 | `keyvault_certificate_get` | ❌ |
| 10 | 0.349202 | `keyvault_secret_get` | ❌ |
| 11 | 0.318472 | `storage_account_create` | ❌ |
| 12 | 0.276217 | `sql_db_create` | ❌ |
| 13 | 0.254549 | `storage_account_get` | ❌ |
| 14 | 0.231306 | `workbooks_create` | ❌ |
| 15 | 0.227297 | `storage_blob_container_create` | ❌ |
| 16 | 0.225987 | `sql_server_create` | ❌ |
| 17 | 0.217703 | `storage_blob_container_get` | ❌ |
| 18 | 0.189379 | `monitor_resource_log_query` | ❌ |
| 19 | 0.188840 | `sql_elastic-pool_list` | ❌ |
| 20 | 0.184490 | `monitor_healthmodels_entity_gethealth` | ❌ |

---

## Test 152

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549557 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.468211 | `keyvault_secret_get` | ❌ |
| 3 | 0.452845 | `keyvault_key_create` | ❌ |
| 4 | 0.439963 | `keyvault_key_list` | ❌ |
| 5 | 0.432378 | `appconfig_kv_show` | ❌ |
| 6 | 0.426572 | `keyvault_certificate_get` | ❌ |
| 7 | 0.396623 | `keyvault_certificate_list` | ❌ |
| 8 | 0.379566 | `storage_account_get` | ❌ |
| 9 | 0.376229 | `appconfig_kv_lock_set` | ❌ |
| 10 | 0.371986 | `keyvault_secret_create` | ❌ |
| 11 | 0.369350 | `keyvault_certificate_import` | ❌ |
| 12 | 0.328787 | `storage_blob_container_get` | ❌ |
| 13 | 0.305777 | `subscription_list` | ❌ |
| 14 | 0.280988 | `storage_account_create` | ❌ |
| 15 | 0.279409 | `search_index_get` | ❌ |
| 16 | 0.276631 | `search_service_list` | ❌ |
| 17 | 0.274348 | `monitor_resource_log_query` | ❌ |
| 18 | 0.268767 | `virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.267576 | `role_assignment_list` | ❌ |
| 20 | 0.256151 | `quota_usage_check` | ❌ |

---

## Test 153

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629390 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.532389 | `keyvault_secret_get` | ❌ |
| 3 | 0.495673 | `keyvault_certificate_get` | ❌ |
| 4 | 0.475206 | `storage_account_get` | ❌ |
| 5 | 0.456897 | `keyvault_key_create` | ❌ |
| 6 | 0.452242 | `appconfig_kv_show` | ❌ |
| 7 | 0.433747 | `keyvault_key_list` | ❌ |
| 8 | 0.428763 | `storage_blob_container_get` | ❌ |
| 9 | 0.394124 | `keyvault_certificate_list` | ❌ |
| 10 | 0.375858 | `aks_nodepool_get` | ❌ |
| 11 | 0.374537 | `keyvault_certificate_import` | ❌ |
| 12 | 0.373932 | `keyvault_certificate_create` | ❌ |
| 13 | 0.368652 | `search_index_get` | ❌ |
| 14 | 0.346240 | `storage_blob_get` | ❌ |
| 15 | 0.340585 | `sql_db_show` | ❌ |
| 16 | 0.337223 | `servicebus_queue_details` | ❌ |
| 17 | 0.326328 | `sql_server_show` | ❌ |
| 18 | 0.315909 | `subscription_list` | ❌ |
| 19 | 0.315893 | `mysql_server_config_get` | ❌ |
| 20 | 0.311508 | `marketplace_product_get` | ❌ |

---

## Test 154

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484645 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.443182 | `keyvault_key_create` | ❌ |
| 3 | 0.409388 | `keyvault_secret_get` | ❌ |
| 4 | 0.383519 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.371486 | `keyvault_key_list` | ❌ |
| 6 | 0.371084 | `keyvault_certificate_get` | ❌ |
| 7 | 0.368861 | `appconfig_kv_show` | ❌ |
| 8 | 0.342757 | `keyvault_secret_create` | ❌ |
| 9 | 0.334854 | `keyvault_certificate_create` | ❌ |
| 10 | 0.334377 | `storage_account_get` | ❌ |
| 11 | 0.333757 | `keyvault_certificate_list` | ❌ |
| 12 | 0.286153 | `storage_blob_container_get` | ❌ |
| 13 | 0.265631 | `storage_account_create` | ❌ |
| 14 | 0.233543 | `subscription_list` | ❌ |
| 15 | 0.227292 | `monitor_healthmodels_entity_gethealth` | ❌ |
| 16 | 0.218318 | `servicebus_queue_details` | ❌ |
| 17 | 0.214910 | `search_index_get` | ❌ |
| 18 | 0.213794 | `monitor_resource_log_query` | ❌ |
| 19 | 0.197995 | `mysql_server_param_get` | ❌ |
| 20 | 0.197734 | `redis_cache_accesspolicy_list` | ❌ |

---

## Test 155

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590303 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.488213 | `keyvault_secret_get` | ❌ |
| 3 | 0.460796 | `keyvault_certificate_get` | ❌ |
| 4 | 0.452075 | `appconfig_kv_show` | ❌ |
| 5 | 0.440938 | `storage_account_get` | ❌ |
| 6 | 0.417546 | `keyvault_key_create` | ❌ |
| 7 | 0.398120 | `keyvault_key_list` | ❌ |
| 8 | 0.389917 | `storage_blob_container_get` | ❌ |
| 9 | 0.374129 | `appconfig_kv_lock_set` | ❌ |
| 10 | 0.353134 | `appconfig_kv_list` | ❌ |
| 11 | 0.349098 | `keyvault_certificate_list` | ❌ |
| 12 | 0.339313 | `keyvault_certificate_import` | ❌ |
| 13 | 0.338023 | `search_index_get` | ❌ |
| 14 | 0.318239 | `storage_blob_get` | ❌ |
| 15 | 0.296062 | `mysql_table_schema_get` | ❌ |
| 16 | 0.286741 | `servicebus_queue_details` | ❌ |
| 17 | 0.273934 | `sql_server_show` | ❌ |
| 18 | 0.273132 | `role_assignment_list` | ❌ |
| 19 | 0.271225 | `redis_cache_accesspolicy_list` | ❌ |
| 20 | 0.271125 | `sql_db_show` | ❌ |

---

## Test 156

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518886 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.429131 | `keyvault_key_create` | ❌ |
| 3 | 0.422536 | `keyvault_secret_get` | ❌ |
| 4 | 0.406215 | `appconfig_kv_show` | ❌ |
| 5 | 0.395959 | `keyvault_key_list` | ❌ |
| 6 | 0.387928 | `keyvault_certificate_get` | ❌ |
| 7 | 0.386015 | `storage_account_get` | ❌ |
| 8 | 0.379025 | `storage_blob_container_get` | ❌ |
| 9 | 0.376384 | `appconfig_kv_lock_set` | ❌ |
| 10 | 0.355070 | `keyvault_certificate_list` | ❌ |
| 11 | 0.343490 | `keyvault_certificate_import` | ❌ |
| 12 | 0.336655 | `keyvault_certificate_create` | ❌ |
| 13 | 0.329865 | `storage_blob_get` | ❌ |
| 14 | 0.301328 | `mysql_table_schema_get` | ❌ |
| 15 | 0.297365 | `search_index_get` | ❌ |
| 16 | 0.288239 | `monitor_metrics_definitions` | ❌ |
| 17 | 0.276450 | `mysql_server_config_get` | ❌ |
| 18 | 0.275942 | `storage_account_create` | ❌ |
| 19 | 0.272172 | `workbooks_show` | ❌ |
| 20 | 0.269592 | `marketplace_product_get` | ❌ |

---

## Test 157

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701448 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.601614 | `keyvault_certificate_list` | ❌ |
| 3 | 0.587427 | `keyvault_secret_list` | ❌ |
| 4 | 0.498767 | `cosmos_account_list` | ❌ |
| 5 | 0.468044 | `cosmos_database_list` | ❌ |
| 6 | 0.458200 | `keyvault_key_get` | ❌ |
| 7 | 0.443785 | `cosmos_database_container_list` | ❌ |
| 8 | 0.439167 | `appconfig_kv_list` | ❌ |
| 9 | 0.430322 | `storage_account_get` | ❌ |
| 10 | 0.426909 | `subscription_list` | ❌ |
| 11 | 0.423833 | `keyvault_key_create` | ❌ |
| 12 | 0.417967 | `keyvault_secret_get` | ❌ |
| 13 | 0.408341 | `search_service_list` | ❌ |
| 14 | 0.387510 | `storage_blob_container_get` | ❌ |
| 15 | 0.373903 | `virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.368258 | `mysql_database_list` | ❌ |
| 17 | 0.354970 | `monitor_table_list` | ❌ |
| 18 | 0.353723 | `redis_cache_list` | ❌ |
| 19 | 0.350154 | `monitor_workspace_list` | ❌ |
| 20 | 0.348597 | `role_assignment_list` | ❌ |

---

## Test 158

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549453 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.506815 | `keyvault_key_get` | ❌ |
| 3 | 0.475587 | `keyvault_certificate_list` | ❌ |
| 4 | 0.455683 | `keyvault_secret_get` | ❌ |
| 5 | 0.446522 | `keyvault_secret_list` | ❌ |
| 6 | 0.424356 | `keyvault_certificate_get` | ❌ |
| 7 | 0.421475 | `cosmos_account_list` | ❌ |
| 8 | 0.420839 | `keyvault_key_create` | ❌ |
| 9 | 0.406776 | `storage_account_get` | ❌ |
| 10 | 0.405205 | `appconfig_kv_show` | ❌ |
| 11 | 0.377735 | `keyvault_certificate_create` | ❌ |
| 12 | 0.356816 | `storage_blob_container_get` | ❌ |
| 13 | 0.353390 | `subscription_list` | ❌ |
| 14 | 0.327200 | `search_service_list` | ❌ |
| 15 | 0.316124 | `virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.308976 | `storage_account_create` | ❌ |
| 17 | 0.306567 | `role_assignment_list` | ❌ |
| 18 | 0.297022 | `search_index_get` | ❌ |
| 19 | 0.295954 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.293404 | `quota_usage_check` | ❌ |

---

## Test 159

**Expected Tool:** `keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581970 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501556 | `keyvault_certificate_list` | ❌ |
| 3 | 0.476470 | `keyvault_key_get` | ❌ |
| 4 | 0.472414 | `keyvault_secret_list` | ❌ |
| 5 | 0.467129 | `keyvault_key_create` | ❌ |
| 6 | 0.444134 | `keyvault_secret_get` | ❌ |
| 7 | 0.416919 | `keyvault_certificate_import` | ❌ |
| 8 | 0.415017 | `keyvault_certificate_create` | ❌ |
| 9 | 0.406630 | `keyvault_certificate_get` | ❌ |
| 10 | 0.397060 | `cosmos_account_list` | ❌ |
| 11 | 0.396268 | `storage_account_get` | ❌ |
| 12 | 0.353983 | `storage_blob_container_get` | ❌ |
| 13 | 0.344539 | `search_service_list` | ❌ |
| 14 | 0.342161 | `subscription_list` | ❌ |
| 15 | 0.321891 | `storage_account_create` | ❌ |
| 16 | 0.310049 | `virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.301891 | `quota_usage_check` | ❌ |
| 18 | 0.300518 | `redis_cache_list` | ❌ |
| 19 | 0.296885 | `search_index_get` | ❌ |
| 20 | 0.291704 | `role_assignment_list` | ❌ |

---

## Test 160

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641314 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.559613 | `keyvault_certificate_list` | ❌ |
| 3 | 0.553553 | `keyvault_secret_list` | ❌ |
| 4 | 0.475992 | `cosmos_account_list` | ❌ |
| 5 | 0.461397 | `keyvault_key_get` | ❌ |
| 6 | 0.447336 | `cosmos_database_list` | ❌ |
| 7 | 0.442855 | `storage_account_get` | ❌ |
| 8 | 0.439089 | `cosmos_database_container_list` | ❌ |
| 9 | 0.427248 | `appconfig_kv_list` | ❌ |
| 10 | 0.422399 | `keyvault_key_create` | ❌ |
| 11 | 0.421692 | `keyvault_secret_get` | ❌ |
| 12 | 0.405945 | `subscription_list` | ❌ |
| 13 | 0.405838 | `storage_blob_container_get` | ❌ |
| 14 | 0.399675 | `monitor_table_type_list` | ❌ |
| 15 | 0.397041 | `search_service_list` | ❌ |
| 16 | 0.390198 | `monitor_table_list` | ❌ |
| 17 | 0.375222 | `mysql_database_list` | ❌ |
| 18 | 0.362561 | `monitor_workspace_list` | ❌ |
| 19 | 0.356176 | `role_assignment_list` | ❌ |
| 20 | 0.354871 | `redis_cache_list` | ❌ |

---

## Test 161

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.723266 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611472 | `keyvault_certificate_list` | ❌ |
| 3 | 0.611185 | `keyvault_secret_list` | ❌ |
| 4 | 0.441881 | `keyvault_key_get` | ❌ |
| 5 | 0.440237 | `cosmos_account_list` | ❌ |
| 6 | 0.425927 | `keyvault_key_create` | ❌ |
| 7 | 0.406526 | `keyvault_secret_get` | ❌ |
| 8 | 0.402910 | `storage_account_get` | ❌ |
| 9 | 0.398274 | `cosmos_database_container_list` | ❌ |
| 10 | 0.392351 | `cosmos_database_list` | ❌ |
| 11 | 0.391599 | `appconfig_kv_list` | ❌ |
| 12 | 0.383112 | `subscription_list` | ❌ |
| 13 | 0.376853 | `mysql_table_list` | ❌ |
| 14 | 0.371788 | `storage_blob_container_get` | ❌ |
| 15 | 0.371133 | `search_service_list` | ❌ |
| 16 | 0.334297 | `virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.328897 | `mysql_server_list` | ❌ |
| 18 | 0.325507 | `monitor_workspace_list` | ❌ |
| 19 | 0.323988 | `redis_cache_list` | ❌ |
| 20 | 0.323510 | `mysql_database_list` | ❌ |

---

## Test 162

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570444 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `keyvault_key_get` | ❌ |
| 3 | 0.500207 | `keyvault_certificate_list` | ❌ |
| 4 | 0.490367 | `keyvault_secret_list` | ❌ |
| 5 | 0.461941 | `storage_account_get` | ❌ |
| 6 | 0.460716 | `cosmos_account_list` | ❌ |
| 7 | 0.457958 | `keyvault_secret_get` | ❌ |
| 8 | 0.454423 | `appconfig_kv_show` | ❌ |
| 9 | 0.427683 | `appconfig_kv_list` | ❌ |
| 10 | 0.427246 | `keyvault_key_create` | ❌ |
| 11 | 0.416532 | `keyvault_certificate_get` | ❌ |
| 12 | 0.411961 | `storage_blob_container_get` | ❌ |
| 13 | 0.403078 | `subscription_list` | ❌ |
| 14 | 0.361323 | `search_service_list` | ❌ |
| 15 | 0.356353 | `mysql_database_list` | ❌ |
| 16 | 0.346934 | `monitor_table_type_list` | ❌ |
| 17 | 0.346482 | `role_assignment_list` | ❌ |
| 18 | 0.345239 | `mysql_server_list` | ❌ |
| 19 | 0.339468 | `search_index_get` | ❌ |
| 20 | 0.338654 | `virtualdesktop_hostpool_list` | ❌ |

---

## Test 163

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678482 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.553018 | `keyvault_key_create` | ❌ |
| 3 | 0.512856 | `keyvault_secret_get` | ❌ |
| 4 | 0.475176 | `keyvault_certificate_create` | ❌ |
| 5 | 0.461437 | `appconfig_kv_set` | ❌ |
| 6 | 0.448966 | `keyvault_certificate_import` | ❌ |
| 7 | 0.406698 | `keyvault_secret_list` | ❌ |
| 8 | 0.397784 | `keyvault_key_get` | ❌ |
| 9 | 0.391024 | `storage_account_create` | ❌ |
| 10 | 0.387507 | `appconfig_kv_lock_set` | ❌ |
| 11 | 0.357221 | `sql_db_create` | ❌ |
| 12 | 0.355685 | `keyvault_certificate_get` | ❌ |
| 13 | 0.288052 | `storage_account_get` | ❌ |
| 14 | 0.287943 | `sql_server_create` | ❌ |
| 15 | 0.287066 | `workbooks_create` | ❌ |
| 16 | 0.246174 | `storage_blob_container_create` | ❌ |
| 17 | 0.243340 | `storage_blob_container_get` | ❌ |
| 18 | 0.218702 | `sql_server_firewall-rule_create` | ❌ |
| 19 | 0.212873 | `storage_blob_upload` | ❌ |
| 20 | 0.209815 | `subscription_list` | ❌ |

---

## Test 164

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663094 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.519601 | `keyvault_secret_get` | ❌ |
| 3 | 0.512233 | `appconfig_kv_set` | ❌ |
| 4 | 0.458502 | `keyvault_key_create` | ❌ |
| 5 | 0.429785 | `appconfig_kv_lock_set` | ❌ |
| 6 | 0.421412 | `keyvault_certificate_import` | ❌ |
| 7 | 0.409186 | `keyvault_key_get` | ❌ |
| 8 | 0.401420 | `appconfig_kv_show` | ❌ |
| 9 | 0.388480 | `keyvault_secret_list` | ❌ |
| 10 | 0.370748 | `keyvault_certificate_create` | ❌ |
| 11 | 0.311345 | `storage_account_get` | ❌ |
| 12 | 0.308706 | `storage_account_create` | ❌ |
| 13 | 0.256685 | `postgres_server_param_set` | ❌ |
| 14 | 0.256473 | `sql_db_create` | ❌ |
| 15 | 0.255243 | `sql_db_update` | ❌ |
| 16 | 0.251053 | `storage_blob_container_get` | ❌ |
| 17 | 0.231789 | `mysql_server_param_set` | ❌ |
| 18 | 0.225637 | `storage_blob_upload` | ❌ |
| 19 | 0.223339 | `workbooks_delete` | ❌ |
| 20 | 0.209177 | `subscription_list` | ❌ |

---

## Test 165

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639897 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.509674 | `keyvault_secret_get` | ❌ |
| 3 | 0.485203 | `appconfig_kv_set` | ❌ |
| 4 | 0.484680 | `keyvault_key_create` | ❌ |
| 5 | 0.448995 | `appconfig_kv_lock_set` | ❌ |
| 6 | 0.447027 | `keyvault_certificate_import` | ❌ |
| 7 | 0.411509 | `keyvault_key_get` | ❌ |
| 8 | 0.392814 | `keyvault_certificate_create` | ❌ |
| 9 | 0.391220 | `keyvault_secret_list` | ❌ |
| 10 | 0.384378 | `appconfig_kv_show` | ❌ |
| 11 | 0.338477 | `storage_account_create` | ❌ |
| 12 | 0.336008 | `storage_account_get` | ❌ |
| 13 | 0.276962 | `storage_blob_container_get` | ❌ |
| 14 | 0.261107 | `sql_db_create` | ❌ |
| 15 | 0.237926 | `storage_blob_upload` | ❌ |
| 16 | 0.229204 | `subscription_list` | ❌ |
| 17 | 0.218708 | `sql_db_update` | ❌ |
| 18 | 0.216186 | `workbooks_delete` | ❌ |
| 19 | 0.215486 | `monitor_resource_log_query` | ❌ |
| 20 | 0.211898 | `workbooks_create` | ❌ |

---

## Test 166

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675145 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.499612 | `keyvault_secret_get` | ❌ |
| 3 | 0.498228 | `keyvault_key_create` | ❌ |
| 4 | 0.479174 | `keyvault_certificate_import` | ❌ |
| 5 | 0.458574 | `appconfig_kv_set` | ❌ |
| 6 | 0.444240 | `keyvault_certificate_create` | ❌ |
| 7 | 0.421114 | `appconfig_kv_lock_set` | ❌ |
| 8 | 0.403133 | `keyvault_secret_list` | ❌ |
| 9 | 0.390897 | `keyvault_key_get` | ❌ |
| 10 | 0.354069 | `appservice_database_add` | ❌ |
| 11 | 0.301166 | `sql_db_create` | ❌ |
| 12 | 0.292024 | `storage_account_create` | ❌ |
| 13 | 0.267714 | `storage_account_get` | ❌ |
| 14 | 0.263706 | `sql_db_update` | ❌ |
| 15 | 0.248670 | `storage_blob_upload` | ❌ |
| 16 | 0.230380 | `storage_blob_container_get` | ❌ |
| 17 | 0.224220 | `workbooks_create` | ❌ |
| 18 | 0.219376 | `storage_blob_container_create` | ❌ |
| 19 | 0.214690 | `sql_server_create` | ❌ |
| 20 | 0.206496 | `loadtesting_testrun_update` | ❌ |

---

## Test 167

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571612 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.513767 | `keyvault_secret_get` | ❌ |
| 3 | 0.441223 | `appconfig_kv_set` | ❌ |
| 4 | 0.417943 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.408242 | `keyvault_key_get` | ❌ |
| 6 | 0.400708 | `keyvault_key_create` | ❌ |
| 7 | 0.394478 | `keyvault_certificate_import` | ❌ |
| 8 | 0.361664 | `keyvault_secret_list` | ❌ |
| 9 | 0.358807 | `sql_db_update` | ❌ |
| 10 | 0.352518 | `keyvault_certificate_get` | ❌ |
| 11 | 0.348194 | `appconfig_kv_show` | ❌ |
| 12 | 0.309610 | `storage_account_get` | ❌ |
| 13 | 0.263012 | `loadtesting_testrun_update` | ❌ |
| 14 | 0.260523 | `mysql_server_param_set` | ❌ |
| 15 | 0.254274 | `storage_account_create` | ❌ |
| 16 | 0.252734 | `storage_blob_container_get` | ❌ |
| 17 | 0.247254 | `workbooks_delete` | ❌ |
| 18 | 0.241653 | `workbooks_update` | ❌ |
| 19 | 0.230660 | `postgres_server_param_set` | ❌ |
| 20 | 0.219535 | `sql_db_create` | ❌ |

---

## Test 168

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602769 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504212 | `keyvault_key_get` | ❌ |
| 3 | 0.501397 | `keyvault_secret_create` | ❌ |
| 4 | 0.478769 | `keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `keyvault_certificate_get` | ❌ |
| 6 | 0.420938 | `appconfig_kv_show` | ❌ |
| 7 | 0.394974 | `keyvault_key_create` | ❌ |
| 8 | 0.389659 | `keyvault_key_list` | ❌ |
| 9 | 0.382573 | `storage_account_get` | ❌ |
| 10 | 0.375047 | `keyvault_certificate_list` | ❌ |
| 11 | 0.369409 | `keyvault_certificate_import` | ❌ |
| 12 | 0.348100 | `storage_blob_container_get` | ❌ |
| 13 | 0.301544 | `subscription_list` | ❌ |
| 14 | 0.294689 | `storage_account_create` | ❌ |
| 15 | 0.284255 | `search_index_get` | ❌ |
| 16 | 0.281795 | `search_service_list` | ❌ |
| 17 | 0.260730 | `mysql_database_list` | ❌ |
| 18 | 0.257699 | `role_assignment_list` | ❌ |
| 19 | 0.255278 | `quota_usage_check` | ❌ |
| 20 | 0.254379 | `sql_db_show` | ❌ |

---

## Test 169

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653871 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.566786 | `keyvault_key_get` | ❌ |
| 3 | 0.496050 | `keyvault_certificate_get` | ❌ |
| 4 | 0.485249 | `keyvault_secret_list` | ❌ |
| 5 | 0.483567 | `storage_account_get` | ❌ |
| 6 | 0.481364 | `keyvault_secret_create` | ❌ |
| 7 | 0.444666 | `storage_blob_container_get` | ❌ |
| 8 | 0.436761 | `appconfig_kv_show` | ❌ |
| 9 | 0.387515 | `keyvault_key_create` | ❌ |
| 10 | 0.384526 | `keyvault_key_list` | ❌ |
| 11 | 0.378874 | `search_index_get` | ❌ |
| 12 | 0.372243 | `keyvault_certificate_list` | ❌ |
| 13 | 0.370109 | `keyvault_certificate_import` | ❌ |
| 14 | 0.354501 | `storage_blob_get` | ❌ |
| 15 | 0.346830 | `sql_db_show` | ❌ |
| 16 | 0.335079 | `sql_server_show` | ❌ |
| 17 | 0.333928 | `servicebus_queue_details` | ❌ |
| 18 | 0.324284 | `mysql_server_config_get` | ❌ |
| 19 | 0.321621 | `marketplace_product_get` | ❌ |
| 20 | 0.311552 | `subscription_list` | ❌ |

---

## Test 170

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578479 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.492213 | `keyvault_key_get` | ❌ |
| 3 | 0.488705 | `keyvault_secret_create` | ❌ |
| 4 | 0.443676 | `keyvault_secret_list` | ❌ |
| 5 | 0.420533 | `keyvault_certificate_get` | ❌ |
| 6 | 0.403982 | `keyvault_key_create` | ❌ |
| 7 | 0.388485 | `appconfig_kv_show` | ❌ |
| 8 | 0.377552 | `storage_account_get` | ❌ |
| 9 | 0.367132 | `keyvault_certificate_create` | ❌ |
| 10 | 0.364454 | `keyvault_key_list` | ❌ |
| 11 | 0.357450 | `keyvault_certificate_list` | ❌ |
| 12 | 0.348267 | `storage_blob_container_get` | ❌ |
| 13 | 0.312081 | `storage_account_create` | ❌ |
| 14 | 0.278672 | `subscription_list` | ❌ |
| 15 | 0.273094 | `search_index_get` | ❌ |
| 16 | 0.254472 | `storage_blob_get` | ❌ |
| 17 | 0.253592 | `monitor_healthmodels_entity_gethealth` | ❌ |
| 18 | 0.253425 | `mysql_database_list` | ❌ |
| 19 | 0.251718 | `monitor_resource_log_query` | ❌ |
| 20 | 0.250263 | `mysql_server_param_get` | ❌ |

---

## Test 171

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649267 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.546992 | `keyvault_key_get` | ❌ |
| 3 | 0.492583 | `keyvault_certificate_get` | ❌ |
| 4 | 0.491596 | `keyvault_secret_list` | ❌ |
| 5 | 0.480354 | `keyvault_secret_create` | ❌ |
| 6 | 0.465487 | `storage_account_get` | ❌ |
| 7 | 0.441265 | `storage_blob_container_get` | ❌ |
| 8 | 0.440721 | `appconfig_kv_show` | ❌ |
| 9 | 0.378375 | `search_index_get` | ❌ |
| 10 | 0.373737 | `keyvault_key_list` | ❌ |
| 11 | 0.370606 | `keyvault_certificate_import` | ❌ |
| 12 | 0.370291 | `keyvault_key_create` | ❌ |
| 13 | 0.370223 | `keyvault_certificate_list` | ❌ |
| 14 | 0.351129 | `storage_blob_get` | ❌ |
| 15 | 0.338686 | `sql_db_show` | ❌ |
| 16 | 0.321929 | `sql_server_show` | ❌ |
| 17 | 0.316244 | `role_assignment_list` | ❌ |
| 18 | 0.310314 | `servicebus_queue_details` | ❌ |
| 19 | 0.306118 | `mysql_server_config_get` | ❌ |
| 20 | 0.302840 | `subscription_list` | ❌ |

---

## Test 172

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573458 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.471281 | `keyvault_key_get` | ❌ |
| 3 | 0.467246 | `keyvault_secret_create` | ❌ |
| 4 | 0.443770 | `keyvault_secret_list` | ❌ |
| 5 | 0.406298 | `storage_blob_container_get` | ❌ |
| 6 | 0.405955 | `keyvault_certificate_get` | ❌ |
| 7 | 0.397429 | `storage_account_get` | ❌ |
| 8 | 0.388047 | `appconfig_kv_show` | ❌ |
| 9 | 0.367111 | `keyvault_key_create` | ❌ |
| 10 | 0.351589 | `storage_blob_get` | ❌ |
| 11 | 0.350789 | `keyvault_key_list` | ❌ |
| 12 | 0.346463 | `keyvault_certificate_list` | ❌ |
| 13 | 0.343587 | `keyvault_certificate_create` | ❌ |
| 14 | 0.326409 | `monitor_metrics_definitions` | ❌ |
| 15 | 0.322010 | `search_index_get` | ❌ |
| 16 | 0.302328 | `marketplace_product_get` | ❌ |
| 17 | 0.301796 | `storage_account_create` | ❌ |
| 18 | 0.301208 | `mysql_table_schema_get` | ❌ |
| 19 | 0.299082 | `mysql_server_config_get` | ❌ |
| 20 | 0.296979 | `monitor_metrics_query` | ❌ |

---

## Test 173

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701227 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.563736 | `keyvault_key_list` | ❌ |
| 3 | 0.538392 | `keyvault_certificate_list` | ❌ |
| 4 | 0.499642 | `keyvault_secret_get` | ❌ |
| 5 | 0.455500 | `cosmos_account_list` | ❌ |
| 6 | 0.433290 | `keyvault_secret_create` | ❌ |
| 7 | 0.433185 | `cosmos_database_list` | ❌ |
| 8 | 0.418971 | `keyvault_key_get` | ❌ |
| 9 | 0.417973 | `cosmos_database_container_list` | ❌ |
| 10 | 0.391082 | `subscription_list` | ❌ |
| 11 | 0.388773 | `search_service_list` | ❌ |
| 12 | 0.387663 | `storage_account_get` | ❌ |
| 13 | 0.377838 | `keyvault_certificate_get` | ❌ |
| 14 | 0.366988 | `storage_blob_container_get` | ❌ |
| 15 | 0.340503 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 16 | 0.337595 | `virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.334206 | `mysql_database_list` | ❌ |
| 18 | 0.331203 | `role_assignment_list` | ❌ |
| 19 | 0.326430 | `redis_cache_list` | ❌ |
| 20 | 0.322010 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |

---

## Test 174

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555681 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.543861 | `keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `keyvault_key_get` | ❌ |
| 4 | 0.464661 | `keyvault_key_list` | ❌ |
| 5 | 0.433489 | `keyvault_secret_create` | ❌ |
| 6 | 0.429826 | `keyvault_certificate_get` | ❌ |
| 7 | 0.428284 | `keyvault_certificate_list` | ❌ |
| 8 | 0.410957 | `appconfig_kv_show` | ❌ |
| 9 | 0.401434 | `storage_account_get` | ❌ |
| 10 | 0.397448 | `keyvault_key_create` | ❌ |
| 11 | 0.385852 | `cosmos_account_list` | ❌ |
| 12 | 0.370855 | `storage_blob_container_get` | ❌ |
| 13 | 0.345256 | `subscription_list` | ❌ |
| 14 | 0.328354 | `search_service_list` | ❌ |
| 15 | 0.305225 | `search_index_get` | ❌ |
| 16 | 0.303769 | `quota_usage_check` | ❌ |
| 17 | 0.299023 | `storage_account_create` | ❌ |
| 18 | 0.294614 | `mysql_server_list` | ❌ |
| 19 | 0.293826 | `role_assignment_list` | ❌ |
| 20 | 0.290273 | `mysql_database_list` | ❌ |

---

## Test 175

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572540 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.529258 | `keyvault_secret_get` | ❌ |
| 3 | 0.493761 | `keyvault_key_list` | ❌ |
| 4 | 0.475273 | `keyvault_key_get` | ❌ |
| 5 | 0.460262 | `keyvault_secret_create` | ❌ |
| 6 | 0.452339 | `keyvault_certificate_list` | ❌ |
| 7 | 0.423709 | `keyvault_key_create` | ❌ |
| 8 | 0.412124 | `storage_account_get` | ❌ |
| 9 | 0.411803 | `keyvault_certificate_import` | ❌ |
| 10 | 0.407537 | `keyvault_certificate_get` | ❌ |
| 11 | 0.399680 | `appconfig_kv_show` | ❌ |
| 12 | 0.388362 | `storage_blob_container_get` | ❌ |
| 13 | 0.339263 | `subscription_list` | ❌ |
| 14 | 0.325306 | `search_service_list` | ❌ |
| 15 | 0.315223 | `redis_cache_list` | ❌ |
| 16 | 0.312209 | `storage_blob_get` | ❌ |
| 17 | 0.309381 | `search_index_get` | ❌ |
| 18 | 0.308713 | `quota_usage_check` | ❌ |
| 19 | 0.301464 | `storage_account_create` | ❌ |
| 20 | 0.292964 | `monitor_resource_log_query` | ❌ |

---

## Test 176

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624290 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.559681 | `keyvault_key_list` | ❌ |
| 3 | 0.517565 | `keyvault_certificate_list` | ❌ |
| 4 | 0.479547 | `keyvault_secret_get` | ❌ |
| 5 | 0.441075 | `cosmos_account_list` | ❌ |
| 6 | 0.431079 | `keyvault_key_get` | ❌ |
| 7 | 0.421769 | `cosmos_database_list` | ❌ |
| 8 | 0.421553 | `keyvault_secret_create` | ❌ |
| 9 | 0.412649 | `storage_account_get` | ❌ |
| 10 | 0.412279 | `cosmos_database_container_list` | ❌ |
| 11 | 0.404613 | `keyvault_key_create` | ❌ |
| 12 | 0.389021 | `storage_blob_container_get` | ❌ |
| 13 | 0.377304 | `subscription_list` | ❌ |
| 14 | 0.365717 | `search_service_list` | ❌ |
| 15 | 0.359226 | `mysql_database_list` | ❌ |
| 16 | 0.355228 | `monitor_table_type_list` | ❌ |
| 17 | 0.339337 | `monitor_table_list` | ❌ |
| 18 | 0.332727 | `role_assignment_list` | ❌ |
| 19 | 0.331508 | `mysql_server_list` | ❌ |
| 20 | 0.326518 | `monitor_workspace_list` | ❌ |

---

## Test 177

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742358 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.601183 | `keyvault_key_list` | ❌ |
| 3 | 0.567880 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496127 | `keyvault_secret_get` | ❌ |
| 5 | 0.435433 | `keyvault_secret_create` | ❌ |
| 6 | 0.418413 | `cosmos_account_list` | ❌ |
| 7 | 0.402782 | `keyvault_key_get` | ❌ |
| 8 | 0.388025 | `search_service_list` | ❌ |
| 9 | 0.383981 | `cosmos_database_container_list` | ❌ |
| 10 | 0.377814 | `cosmos_database_list` | ❌ |
| 11 | 0.376911 | `keyvault_certificate_create` | ❌ |
| 12 | 0.373574 | `storage_account_get` | ❌ |
| 13 | 0.372316 | `subscription_list` | ❌ |
| 14 | 0.365791 | `storage_blob_container_get` | ❌ |
| 15 | 0.354830 | `mysql_table_list` | ❌ |
| 16 | 0.334126 | `mysql_server_list` | ❌ |
| 17 | 0.322255 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 18 | 0.318129 | `redis_cache_list` | ❌ |
| 19 | 0.317808 | `mysql_database_list` | ❌ |
| 20 | 0.315987 | `virtualdesktop_hostpool_list` | ❌ |

---

## Test 178

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567110 | `keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.522398 | `keyvault_secret_get` | ❌ |
| 3 | 0.476309 | `keyvault_key_list` | ❌ |
| 4 | 0.462676 | `keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `keyvault_key_get` | ❌ |
| 6 | 0.440008 | `keyvault_certificate_list` | ❌ |
| 7 | 0.413606 | `keyvault_key_create` | ❌ |
| 8 | 0.411700 | `storage_account_get` | ❌ |
| 9 | 0.409511 | `cosmos_account_list` | ❌ |
| 10 | 0.409141 | `appconfig_kv_show` | ❌ |
| 11 | 0.407517 | `keyvault_certificate_get` | ❌ |
| 12 | 0.376522 | `storage_blob_container_get` | ❌ |
| 13 | 0.361737 | `subscription_list` | ❌ |
| 14 | 0.325238 | `mysql_database_list` | ❌ |
| 15 | 0.321954 | `search_service_list` | ❌ |
| 16 | 0.316216 | `mysql_server_list` | ❌ |
| 17 | 0.303465 | `role_assignment_list` | ❌ |
| 18 | 0.300470 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.299650 | `virtualdesktop_hostpool_list` | ❌ |
| 20 | 0.297014 | `search_index_get` | ❌ |

---

## Test 179

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660869 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.611425 | `aks_cluster_list` | ❌ |
| 3 | 0.579676 | `aks_nodepool_get` | ❌ |
| 4 | 0.540767 | `aks_nodepool_list` | ❌ |
| 5 | 0.481416 | `mysql_server_config_get` | ❌ |
| 6 | 0.463682 | `kusto_cluster_get` | ❌ |
| 7 | 0.463065 | `loadtesting_test_get` | ❌ |
| 8 | 0.430975 | `postgres_server_config_get` | ❌ |
| 9 | 0.419629 | `sql_server_show` | ❌ |
| 10 | 0.399345 | `storage_account_get` | ❌ |
| 11 | 0.391924 | `appconfig_kv_show` | ❌ |
| 12 | 0.390959 | `appconfig_account_list` | ❌ |
| 13 | 0.390819 | `appconfig_kv_list` | ❌ |
| 14 | 0.390141 | `kusto_cluster_list` | ❌ |
| 15 | 0.371630 | `mysql_server_param_get` | ❌ |
| 16 | 0.370108 | `storage_blob_container_get` | ❌ |
| 17 | 0.369397 | `sql_db_update` | ❌ |
| 18 | 0.367841 | `redis_cluster_list` | ❌ |
| 19 | 0.360695 | `storage_blob_get` | ❌ |
| 20 | 0.355388 | `sql_server_list` | ❌ |

---

## Test 180

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666849 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.589060 | `aks_cluster_list` | ❌ |
| 3 | 0.545820 | `aks_nodepool_get` | ❌ |
| 4 | 0.530314 | `aks_nodepool_list` | ❌ |
| 5 | 0.508226 | `kusto_cluster_get` | ❌ |
| 6 | 0.461466 | `sql_db_show` | ❌ |
| 7 | 0.448796 | `redis_cluster_list` | ❌ |
| 8 | 0.428449 | `functionapp_get` | ❌ |
| 9 | 0.423023 | `resourcehealth_availability-status_list` | ❌ |
| 10 | 0.413625 | `mysql_server_list` | ❌ |
| 11 | 0.408420 | `azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.396636 | `datadog_monitoredresources_list` | ❌ |
| 13 | 0.396256 | `storage_account_get` | ❌ |
| 14 | 0.390889 | `sql_server_list` | ❌ |
| 15 | 0.385261 | `acr_registry_repository_list` | ❌ |
| 16 | 0.384654 | `kusto_cluster_list` | ❌ |
| 17 | 0.382236 | `storage_blob_container_get` | ❌ |
| 18 | 0.377088 | `storage_blob_get` | ❌ |
| 19 | 0.366088 | `search_index_get` | ❌ |
| 20 | 0.362332 | `sql_db_list` | ❌ |

---

## Test 181

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567273 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563045 | `aks_cluster_list` | ❌ |
| 3 | 0.493940 | `aks_nodepool_list` | ❌ |
| 4 | 0.486040 | `aks_nodepool_get` | ❌ |
| 5 | 0.380301 | `mysql_server_config_get` | ❌ |
| 6 | 0.368584 | `kusto_cluster_get` | ❌ |
| 7 | 0.348885 | `sql_server_list` | ❌ |
| 8 | 0.342696 | `loadtesting_test_get` | ❌ |
| 9 | 0.340293 | `kusto_cluster_list` | ❌ |
| 10 | 0.334923 | `appconfig_account_list` | ❌ |
| 11 | 0.334860 | `redis_cluster_list` | ❌ |
| 12 | 0.329324 | `sql_server_show` | ❌ |
| 13 | 0.315228 | `storage_account_get` | ❌ |
| 14 | 0.314513 | `appconfig_kv_list` | ❌ |
| 15 | 0.309738 | `appconfig_kv_show` | ❌ |
| 16 | 0.299047 | `mysql_server_list` | ❌ |
| 17 | 0.296889 | `sql_db_update` | ❌ |
| 18 | 0.296592 | `postgres_server_config_get` | ❌ |
| 19 | 0.289342 | `mysql_server_param_get` | ❌ |
| 20 | 0.275751 | `sql_db_show` | ❌ |

---

## Test 182

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661426 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.578650 | `aks_cluster_list` | ❌ |
| 3 | 0.563549 | `aks_nodepool_get` | ❌ |
| 4 | 0.534089 | `aks_nodepool_list` | ❌ |
| 5 | 0.503925 | `kusto_cluster_get` | ❌ |
| 6 | 0.434587 | `functionapp_get` | ❌ |
| 7 | 0.433913 | `azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.419338 | `resourcehealth_availability-status_list` | ❌ |
| 9 | 0.418518 | `redis_cluster_list` | ❌ |
| 10 | 0.417836 | `sql_db_show` | ❌ |
| 11 | 0.405658 | `storage_account_get` | ❌ |
| 12 | 0.404415 | `storage_blob_get` | ❌ |
| 13 | 0.402335 | `mysql_server_list` | ❌ |
| 14 | 0.398616 | `storage_blob_container_get` | ❌ |
| 15 | 0.391699 | `resourcehealth_availability-status_get` | ❌ |
| 16 | 0.384782 | `mysql_server_config_get` | ❌ |
| 17 | 0.383956 | `sql_server_list` | ❌ |
| 18 | 0.372812 | `kusto_cluster_list` | ❌ |
| 19 | 0.367514 | `deploy_app_logs_get` | ❌ |
| 20 | 0.359877 | `acr_registry_repository_list` | ❌ |

---

## Test 183

**Expected Tool:** `aks_cluster_list`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.801113 | `aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.690255 | `kusto_cluster_list` | ❌ |
| 3 | 0.599940 | `redis_cluster_list` | ❌ |
| 4 | 0.594509 | `aks_nodepool_list` | ❌ |
| 5 | 0.562043 | `search_service_list` | ❌ |
| 6 | 0.560861 | `aks_cluster_get` | ❌ |
| 7 | 0.543684 | `monitor_workspace_list` | ❌ |
| 8 | 0.515922 | `cosmos_account_list` | ❌ |
| 9 | 0.509202 | `kusto_database_list` | ❌ |
| 10 | 0.502401 | `subscription_list` | ❌ |
| 11 | 0.498286 | `virtualdesktop_hostpool_list` | ❌ |
| 12 | 0.498239 | `group_list` | ❌ |
| 13 | 0.495977 | `postgres_server_list` | ❌ |
| 14 | 0.486167 | `redis_cache_list` | ❌ |
| 15 | 0.483592 | `kusto_cluster_get` | ❌ |
| 16 | 0.482328 | `acr_registry_list` | ❌ |
| 17 | 0.481469 | `grafana_list` | ❌ |
| 18 | 0.457949 | `sql_server_list` | ❌ |
| 19 | 0.452959 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 20 | 0.452681 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 184

**Expected Tool:** `aks_cluster_list`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608081 | `aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.536412 | `aks_cluster_get` | ❌ |
| 3 | 0.500890 | `aks_nodepool_list` | ❌ |
| 4 | 0.492910 | `kusto_cluster_list` | ❌ |
| 5 | 0.455228 | `search_service_list` | ❌ |
| 6 | 0.446270 | `redis_cluster_list` | ❌ |
| 7 | 0.428444 | `foundry_agents_list` | ❌ |
| 8 | 0.416475 | `aks_nodepool_get` | ❌ |
| 9 | 0.409711 | `kusto_cluster_get` | ❌ |
| 10 | 0.408385 | `kusto_database_list` | ❌ |
| 11 | 0.392997 | `mysql_server_list` | ❌ |
| 12 | 0.376362 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.371809 | `azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.371535 | `monitor_workspace_list` | ❌ |
| 15 | 0.370963 | `search_index_get` | ❌ |
| 16 | 0.363804 | `subscription_list` | ❌ |
| 17 | 0.361928 | `mysql_database_list` | ❌ |
| 18 | 0.358213 | `storage_blob_container_get` | ❌ |
| 19 | 0.356926 | `resourcehealth_availability-status_list` | ❌ |
| 20 | 0.356016 | `storage_account_get` | ❌ |

---

## Test 185

**Expected Tool:** `aks_cluster_list`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623942 | `aks_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.538749 | `aks_nodepool_list` | ❌ |
| 3 | 0.530023 | `aks_cluster_get` | ❌ |
| 4 | 0.466749 | `aks_nodepool_get` | ❌ |
| 5 | 0.449602 | `kusto_cluster_list` | ❌ |
| 6 | 0.416564 | `redis_cluster_list` | ❌ |
| 7 | 0.392083 | `azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.379421 | `foundry_agents_list` | ❌ |
| 9 | 0.378826 | `monitor_workspace_list` | ❌ |
| 10 | 0.377567 | `acr_registry_repository_list` | ❌ |
| 11 | 0.374585 | `mysql_server_list` | ❌ |
| 12 | 0.363981 | `deploy_app_logs_get` | ❌ |
| 13 | 0.353365 | `search_service_list` | ❌ |
| 14 | 0.345290 | `resourcehealth_availability-status_list` | ❌ |
| 15 | 0.345241 | `kusto_cluster_get` | ❌ |
| 16 | 0.337354 | `virtualdesktop_hostpool_list` | ❌ |
| 17 | 0.317977 | `sql_elastic-pool_list` | ❌ |
| 18 | 0.317212 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.312354 | `subscription_list` | ❌ |
| 20 | 0.311971 | `quota_usage_check` | ❌ |

---

## Test 186

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.753920 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.699423 | `aks_nodepool_list` | ❌ |
| 3 | 0.597308 | `aks_cluster_get` | ❌ |
| 4 | 0.498544 | `aks_cluster_list` | ❌ |
| 5 | 0.482683 | `kusto_cluster_get` | ❌ |
| 6 | 0.468392 | `virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.463192 | `sql_elastic-pool_list` | ❌ |
| 8 | 0.434875 | `sql_db_show` | ❌ |
| 9 | 0.414751 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 10 | 0.401610 | `redis_cluster_list` | ❌ |
| 11 | 0.399215 | `functionapp_get` | ❌ |
| 12 | 0.383606 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 13 | 0.382352 | `mysql_server_list` | ❌ |
| 14 | 0.379665 | `storage_blob_get` | ❌ |
| 15 | 0.378264 | `resourcehealth_availability-status_list` | ❌ |
| 16 | 0.378238 | `search_index_get` | ❌ |
| 17 | 0.370172 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.367944 | `keyvault_key_get` | ❌ |
| 19 | 0.362512 | `loadtesting_test_get` | ❌ |
| 20 | 0.357874 | `keyvault_secret_get` | ❌ |

---

## Test 187

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678158 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.640096 | `aks_nodepool_list` | ❌ |
| 3 | 0.481312 | `aks_cluster_get` | ❌ |
| 4 | 0.458596 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.445979 | `aks_cluster_list` | ❌ |
| 6 | 0.440182 | `virtualdesktop_hostpool_list` | ❌ |
| 7 | 0.391067 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 8 | 0.384600 | `loadtesting_test_get` | ❌ |
| 9 | 0.371847 | `sql_server_list` | ❌ |
| 10 | 0.367455 | `mysql_server_list` | ❌ |
| 11 | 0.365231 | `mysql_server_config_get` | ❌ |
| 12 | 0.357721 | `sql_db_list` | ❌ |
| 13 | 0.350998 | `redis_cluster_list` | ❌ |
| 14 | 0.350992 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.344818 | `sql_db_show` | ❌ |
| 16 | 0.343726 | `kusto_cluster_get` | ❌ |
| 17 | 0.342564 | `datadog_monitoredresources_list` | ❌ |
| 18 | 0.338364 | `azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.332531 | `foundry_agents_list` | ❌ |
| 20 | 0.322685 | `appconfig_kv_show` | ❌ |

---

## Test 188

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599506 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.582325 | `aks_nodepool_list` | ❌ |
| 3 | 0.412109 | `aks_cluster_get` | ❌ |
| 4 | 0.391576 | `aks_cluster_list` | ❌ |
| 5 | 0.385173 | `virtualdesktop_hostpool_list` | ❌ |
| 6 | 0.383045 | `sql_elastic-pool_list` | ❌ |
| 7 | 0.346262 | `deploy_pipeline_guidance_get` | ❌ |
| 8 | 0.339934 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.323057 | `deploy_plan_get` | ❌ |
| 10 | 0.320733 | `mysql_server_list` | ❌ |
| 11 | 0.314439 | `redis_cluster_list` | ❌ |
| 12 | 0.313226 | `sql_server_list` | ❌ |
| 13 | 0.306678 | `kusto_cluster_get` | ❌ |
| 14 | 0.306579 | `storage_account_create` | ❌ |
| 15 | 0.300123 | `datadog_monitoredresources_list` | ❌ |
| 16 | 0.298866 | `acr_registry_repository_list` | ❌ |
| 17 | 0.289422 | `resourcehealth_availability-status_list` | ❌ |
| 18 | 0.287015 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.283171 | `azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.276058 | `sql_db_list` | ❌ |

---

## Test 189

**Expected Tool:** `aks_nodepool_list`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694117 | `aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.615516 | `aks_nodepool_get` | ❌ |
| 3 | 0.531980 | `aks_cluster_list` | ❌ |
| 4 | 0.506624 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.487707 | `sql_elastic-pool_list` | ❌ |
| 6 | 0.461701 | `aks_cluster_get` | ❌ |
| 7 | 0.446699 | `redis_cluster_list` | ❌ |
| 8 | 0.440646 | `mysql_server_list` | ❌ |
| 9 | 0.438637 | `kusto_cluster_list` | ❌ |
| 10 | 0.435177 | `acr_registry_repository_list` | ❌ |
| 11 | 0.431369 | `datadog_monitoredresources_list` | ❌ |
| 12 | 0.418681 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 13 | 0.413085 | `resourcehealth_availability-status_list` | ❌ |
| 14 | 0.407783 | `sql_server_list` | ❌ |
| 15 | 0.404890 | `sql_db_list` | ❌ |
| 16 | 0.399222 | `acr_registry_list` | ❌ |
| 17 | 0.393876 | `group_list` | ❌ |
| 18 | 0.391869 | `kusto_database_list` | ❌ |
| 19 | 0.389071 | `redis_cluster_database_list` | ❌ |
| 20 | 0.385781 | `workbooks_list` | ❌ |

---

## Test 190

**Expected Tool:** `aks_nodepool_list`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712299 | `aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.644451 | `aks_nodepool_get` | ❌ |
| 3 | 0.547383 | `aks_cluster_list` | ❌ |
| 4 | 0.510269 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.509732 | `virtualdesktop_hostpool_list` | ❌ |
| 6 | 0.497966 | `aks_cluster_get` | ❌ |
| 7 | 0.447545 | `mysql_server_list` | ❌ |
| 8 | 0.442122 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 9 | 0.441482 | `redis_cluster_list` | ❌ |
| 10 | 0.433138 | `datadog_monitoredresources_list` | ❌ |
| 11 | 0.430830 | `acr_registry_repository_list` | ❌ |
| 12 | 0.430739 | `kusto_cluster_list` | ❌ |
| 13 | 0.421390 | `sql_server_list` | ❌ |
| 14 | 0.408986 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 15 | 0.408569 | `resourcehealth_availability-status_list` | ❌ |
| 16 | 0.407619 | `sql_db_list` | ❌ |
| 17 | 0.390197 | `redis_cluster_database_list` | ❌ |
| 18 | 0.388937 | `group_list` | ❌ |
| 19 | 0.387647 | `foundry_agents_list` | ❌ |
| 20 | 0.383234 | `azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 191

**Expected Tool:** `aks_nodepool_list`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623138 | `aks_nodepool_list` | ✅ **EXPECTED** |
| 2 | 0.580535 | `aks_nodepool_get` | ❌ |
| 3 | 0.453773 | `aks_cluster_list` | ❌ |
| 4 | 0.443902 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.425448 | `sql_elastic-pool_list` | ❌ |
| 6 | 0.409286 | `aks_cluster_get` | ❌ |
| 7 | 0.386949 | `redis_cluster_list` | ❌ |
| 8 | 0.378905 | `mysql_server_list` | ❌ |
| 9 | 0.368944 | `kusto_cluster_list` | ❌ |
| 10 | 0.363262 | `resourcehealth_availability-status_list` | ❌ |
| 11 | 0.360005 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 12 | 0.356345 | `datadog_monitoredresources_list` | ❌ |
| 13 | 0.356139 | `acr_registry_repository_list` | ❌ |
| 14 | 0.354542 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.347994 | `foundry_agents_list` | ❌ |
| 16 | 0.335285 | `sql_server_list` | ❌ |
| 17 | 0.329036 | `sql_db_list` | ❌ |
| 18 | 0.324508 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 19 | 0.324257 | `deploy_plan_get` | ❌ |
| 20 | 0.323568 | `monitor_workspace_list` | ❌ |

---

## Test 192

**Expected Tool:** `loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585409 | `loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.531850 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.508709 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.413787 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.394676 | `loadtesting_testrun_get` | ❌ |
| 6 | 0.390081 | `loadtesting_test_get` | ❌ |
| 7 | 0.346526 | `loadtesting_testrun_update` | ❌ |
| 8 | 0.338668 | `loadtesting_testrun_list` | ❌ |
| 9 | 0.338173 | `monitor_workspace_log_query` | ❌ |
| 10 | 0.337311 | `monitor_resource_log_query` | ❌ |
| 11 | 0.325279 | `keyvault_certificate_create` | ❌ |
| 12 | 0.323519 | `storage_account_create` | ❌ |
| 13 | 0.310144 | `workbooks_create` | ❌ |
| 14 | 0.296991 | `resourcehealth_availability-status_list` | ❌ |
| 15 | 0.292107 | `keyvault_key_create` | ❌ |
| 16 | 0.290957 | `quota_usage_check` | ❌ |
| 17 | 0.289844 | `eventgrid_subscription_list` | ❌ |
| 18 | 0.288940 | `quota_region_availability_list` | ❌ |
| 19 | 0.280439 | `sql_server_create` | ❌ |
| 20 | 0.273769 | `sql_server_list` | ❌ |

---

## Test 193

**Expected Tool:** `loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642272 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 2 | 0.608723 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.574283 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.534033 | `loadtesting_testrun_get` | ❌ |
| 5 | 0.473230 | `loadtesting_testrun_create` | ❌ |
| 6 | 0.469690 | `loadtesting_testrun_list` | ❌ |
| 7 | 0.437034 | `loadtesting_test_create` | ❌ |
| 8 | 0.404563 | `monitor_resource_log_query` | ❌ |
| 9 | 0.397382 | `group_list` | ❌ |
| 10 | 0.379214 | `resourcehealth_availability-status_list` | ❌ |
| 11 | 0.373008 | `loadtesting_testrun_update` | ❌ |
| 12 | 0.369857 | `workbooks_show` | ❌ |
| 13 | 0.365514 | `workbooks_list` | ❌ |
| 14 | 0.360656 | `datadog_monitoredresources_list` | ❌ |
| 15 | 0.354303 | `sql_server_list` | ❌ |
| 16 | 0.346950 | `resourcehealth_availability-status_get` | ❌ |
| 17 | 0.341295 | `quota_region_availability_list` | ❌ |
| 18 | 0.340345 | `extension_azqr` | ❌ |
| 19 | 0.329316 | `sql_db_show` | ❌ |
| 20 | 0.328497 | `monitor_metrics_query` | ❌ |

---

## Test 194

**Expected Tool:** `loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718157 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.596726 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.514675 | `loadtesting_test_create` | ❌ |
| 4 | 0.476713 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.443117 | `loadtesting_test_get` | ❌ |
| 6 | 0.442167 | `workbooks_create` | ❌ |
| 7 | 0.416919 | `group_list` | ❌ |
| 8 | 0.407752 | `storage_account_create` | ❌ |
| 9 | 0.394967 | `datadog_monitoredresources_list` | ❌ |
| 10 | 0.382774 | `resourcehealth_availability-status_list` | ❌ |
| 11 | 0.370084 | `loadtesting_testrun_get` | ❌ |
| 12 | 0.369786 | `sql_server_list` | ❌ |
| 13 | 0.369409 | `workbooks_list` | ❌ |
| 14 | 0.356831 | `sql_server_create` | ❌ |
| 15 | 0.350916 | `loadtesting_testrun_update` | ❌ |
| 16 | 0.343649 | `eventgrid_subscription_list` | ❌ |
| 17 | 0.342213 | `redis_cluster_list` | ❌ |
| 18 | 0.341251 | `grafana_list` | ❌ |
| 19 | 0.335675 | `redis_cache_list` | ❌ |
| 20 | 0.326617 | `quota_region_availability_list` | ❌ |

---

## Test 195

**Expected Tool:** `loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737917 | `loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.592295 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.577464 | `group_list` | ❌ |
| 4 | 0.565565 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.561537 | `resourcehealth_availability-status_list` | ❌ |
| 6 | 0.526378 | `workbooks_list` | ❌ |
| 7 | 0.515624 | `redis_cluster_list` | ❌ |
| 8 | 0.511602 | `redis_cache_list` | ❌ |
| 9 | 0.506184 | `loadtesting_test_get` | ❌ |
| 10 | 0.497916 | `sql_server_list` | ❌ |
| 11 | 0.487330 | `grafana_list` | ❌ |
| 12 | 0.483681 | `loadtesting_testrun_list` | ❌ |
| 13 | 0.478586 | `eventgrid_subscription_list` | ❌ |
| 14 | 0.473444 | `search_service_list` | ❌ |
| 15 | 0.473287 | `mysql_server_list` | ❌ |
| 16 | 0.470869 | `acr_registry_list` | ❌ |
| 17 | 0.463420 | `loadtesting_testrun_get` | ❌ |
| 18 | 0.452190 | `monitor_workspace_list` | ❌ |
| 19 | 0.447138 | `quota_region_availability_list` | ❌ |
| 20 | 0.433793 | `virtualdesktop_hostpool_list` | ❌ |

---

## Test 196

**Expected Tool:** `loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621901 | `loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.592841 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.540949 | `loadtesting_test_create` | ❌ |
| 4 | 0.530882 | `loadtesting_testrun_update` | ❌ |
| 5 | 0.488143 | `loadtesting_testrun_get` | ❌ |
| 6 | 0.469444 | `loadtesting_test_get` | ❌ |
| 7 | 0.418431 | `loadtesting_testrun_list` | ❌ |
| 8 | 0.411567 | `loadtesting_testresource_list` | ❌ |
| 9 | 0.402120 | `workbooks_create` | ❌ |
| 10 | 0.383719 | `storage_account_create` | ❌ |
| 11 | 0.362695 | `sql_db_create` | ❌ |
| 12 | 0.323774 | `sql_server_create` | ❌ |
| 13 | 0.308740 | `keyvault_key_create` | ❌ |
| 14 | 0.306420 | `monitor_resource_log_query` | ❌ |
| 15 | 0.302128 | `foundry_agents_connect` | ❌ |
| 16 | 0.300269 | `extension_azqr` | ❌ |
| 17 | 0.273429 | `sql_server_list` | ❌ |
| 18 | 0.272151 | `sql_db_show` | ❌ |
| 19 | 0.267551 | `resourcehealth_availability-status_list` | ❌ |
| 20 | 0.262297 | `storage_blob_container_create` | ❌ |

---

## Test 197

**Expected Tool:** `loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625325 | `loadtesting_test_get` | ❌ |
| 2 | 0.602973 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.568403 | `loadtesting_testrun_get` | ✅ **EXPECTED** |
| 4 | 0.561974 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.535261 | `loadtesting_testrun_create` | ❌ |
| 6 | 0.496647 | `loadtesting_testrun_list` | ❌ |
| 7 | 0.434486 | `loadtesting_test_create` | ❌ |
| 8 | 0.415422 | `loadtesting_testrun_update` | ❌ |
| 9 | 0.397888 | `group_list` | ❌ |
| 10 | 0.394665 | `monitor_resource_log_query` | ❌ |
| 11 | 0.370202 | `datadog_monitoredresources_list` | ❌ |
| 12 | 0.366564 | `resourcehealth_availability-status_list` | ❌ |
| 13 | 0.356330 | `workbooks_list` | ❌ |
| 14 | 0.353672 | `sql_server_list` | ❌ |
| 15 | 0.352912 | `workbooks_show` | ❌ |
| 16 | 0.347000 | `quota_region_availability_list` | ❌ |
| 17 | 0.339712 | `functionapp_get` | ❌ |
| 18 | 0.330715 | `monitor_metrics_query` | ❌ |
| 19 | 0.329509 | `resourcehealth_availability-status_get` | ❌ |
| 20 | 0.328877 | `sql_db_show` | ❌ |

---

## Test 198

**Expected Tool:** `loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615897 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.606058 | `loadtesting_test_get` | ❌ |
| 3 | 0.569139 | `loadtesting_testrun_get` | ❌ |
| 4 | 0.565093 | `loadtesting_testrun_list` | ✅ **EXPECTED** |
| 5 | 0.535259 | `loadtesting_testresource_create` | ❌ |
| 6 | 0.492783 | `loadtesting_testrun_create` | ❌ |
| 7 | 0.432165 | `group_list` | ❌ |
| 8 | 0.416453 | `monitor_resource_log_query` | ❌ |
| 9 | 0.410933 | `resourcehealth_availability-status_list` | ❌ |
| 10 | 0.406651 | `loadtesting_test_create` | ❌ |
| 11 | 0.395915 | `datadog_monitoredresources_list` | ❌ |
| 12 | 0.392066 | `loadtesting_testrun_update` | ❌ |
| 13 | 0.391147 | `workbooks_list` | ❌ |
| 14 | 0.375782 | `monitor_metrics_query` | ❌ |
| 15 | 0.373875 | `sql_server_list` | ❌ |
| 16 | 0.367897 | `functionapp_get` | ❌ |
| 17 | 0.356833 | `quota_region_availability_list` | ❌ |
| 18 | 0.342526 | `resourcehealth_availability-status_get` | ❌ |
| 19 | 0.340466 | `workbooks_show` | ❌ |
| 20 | 0.329464 | `sql_db_list` | ❌ |

---

## Test 199

**Expected Tool:** `loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659812 | `loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.509265 | `loadtesting_testrun_create` | ❌ |
| 3 | 0.454755 | `loadtesting_testrun_get` | ❌ |
| 4 | 0.443828 | `loadtesting_test_get` | ❌ |
| 5 | 0.422017 | `loadtesting_testresource_create` | ❌ |
| 6 | 0.399670 | `loadtesting_test_create` | ❌ |
| 7 | 0.384601 | `loadtesting_testresource_list` | ❌ |
| 8 | 0.384237 | `loadtesting_testrun_list` | ❌ |
| 9 | 0.332746 | `sql_db_update` | ❌ |
| 10 | 0.320124 | `workbooks_update` | ❌ |
| 11 | 0.300023 | `workbooks_create` | ❌ |
| 12 | 0.268124 | `workbooks_show` | ❌ |
| 13 | 0.267137 | `appconfig_kv_set` | ❌ |
| 14 | 0.255408 | `resourcehealth_availability-status_list` | ❌ |
| 15 | 0.253250 | `functionapp_get` | ❌ |
| 16 | 0.252149 | `sql_server_list` | ❌ |
| 17 | 0.250017 | `monitor_resource_log_query` | ❌ |
| 18 | 0.245757 | `appservice_database_add` | ❌ |
| 19 | 0.240916 | `workbooks_delete` | ❌ |
| 20 | 0.234240 | `monitor_metrics_query` | ❌ |

---

## Test 200

**Expected Tool:** `grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578892 | `grafana_list` | ✅ **EXPECTED** |
| 2 | 0.551851 | `search_service_list` | ❌ |
| 3 | 0.513028 | `monitor_workspace_list` | ❌ |
| 4 | 0.505836 | `kusto_cluster_list` | ❌ |
| 5 | 0.498077 | `datadog_monitoredresources_list` | ❌ |
| 6 | 0.493645 | `redis_cluster_list` | ❌ |
| 7 | 0.492724 | `postgres_server_list` | ❌ |
| 8 | 0.492210 | `subscription_list` | ❌ |
| 9 | 0.491777 | `aks_cluster_list` | ❌ |
| 10 | 0.489846 | `cosmos_account_list` | ❌ |
| 11 | 0.482825 | `redis_cache_list` | ❌ |
| 12 | 0.479611 | `resourcehealth_availability-status_list` | ❌ |
| 13 | 0.459138 | `eventgrid_topic_list` | ❌ |
| 14 | 0.457845 | `virtualdesktop_hostpool_list` | ❌ |
| 15 | 0.452244 | `foundry_agents_list` | ❌ |
| 16 | 0.447752 | `mysql_server_list` | ❌ |
| 17 | 0.447597 | `sql_server_list` | ❌ |
| 18 | 0.441411 | `group_list` | ❌ |
| 19 | 0.440392 | `kusto_database_list` | ❌ |
| 20 | 0.436802 | `azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 201

**Expected Tool:** `azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.516886 | `kusto_cluster_list` | ❌ |
| 4 | 0.513156 | `search_service_list` | ❌ |
| 5 | 0.507981 | `monitor_workspace_list` | ❌ |
| 6 | 0.500471 | `subscription_list` | ❌ |
| 7 | 0.499290 | `cosmos_account_list` | ❌ |
| 8 | 0.480850 | `datadog_monitoredresources_list` | ❌ |
| 9 | 0.477193 | `aks_cluster_list` | ❌ |
| 10 | 0.472811 | `redis_cluster_list` | ❌ |
| 11 | 0.460925 | `acr_registry_list` | ❌ |
| 12 | 0.460322 | `redis_cache_list` | ❌ |
| 13 | 0.451887 | `storage_account_get` | ❌ |
| 14 | 0.450971 | `kusto_database_list` | ❌ |
| 15 | 0.448426 | `sql_server_list` | ❌ |
| 16 | 0.447269 | `quota_region_availability_list` | ❌ |
| 17 | 0.445430 | `acr_registry_repository_list` | ❌ |
| 18 | 0.442506 | `virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.438952 | `grafana_list` | ❌ |
| 20 | 0.437939 | `postgres_server_list` | ❌ |

---

## Test 202

**Expected Tool:** `azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743903 | `azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.613217 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.519986 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.514120 | `mysql_server_list` | ❌ |
| 5 | 0.492115 | `acr_registry_repository_list` | ❌ |
| 6 | 0.477847 | `sql_server_list` | ❌ |
| 7 | 0.466545 | `resourcehealth_availability-status_list` | ❌ |
| 8 | 0.452844 | `acr_registry_list` | ❌ |
| 9 | 0.443767 | `sql_db_list` | ❌ |
| 10 | 0.441712 | `group_list` | ❌ |
| 11 | 0.433933 | `workbooks_list` | ❌ |
| 12 | 0.412747 | `search_service_list` | ❌ |
| 13 | 0.412709 | `redis_cluster_list` | ❌ |
| 14 | 0.409044 | `sql_elastic-pool_list` | ❌ |
| 15 | 0.407704 | `virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.406511 | `mysql_database_list` | ❌ |
| 17 | 0.402926 | `cosmos_account_list` | ❌ |
| 18 | 0.398525 | `foundry_agents_list` | ❌ |
| 19 | 0.398168 | `kusto_cluster_list` | ❌ |
| 20 | 0.397222 | `functionapp_get` | ❌ |

---

## Test 203

**Expected Tool:** `azuremanagedlustre_filesystem_required-subnet-size`  
**Prompt:** Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647272 | `azuremanagedlustre_filesystem_required-subnet-size` | ✅ **EXPECTED** |
| 2 | 0.450342 | `azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.327359 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.235376 | `cloudarchitect_design` | ❌ |
| 5 | 0.204654 | `mysql_server_list` | ❌ |
| 6 | 0.204313 | `aks_nodepool_get` | ❌ |
| 7 | 0.203596 | `quota_usage_check` | ❌ |
| 8 | 0.198992 | `storage_account_get` | ❌ |
| 9 | 0.192371 | `mysql_server_config_get` | ❌ |
| 10 | 0.188403 | `sql_server_firewall-rule_create` | ❌ |
| 11 | 0.187133 | `storage_blob_get` | ❌ |
| 12 | 0.176407 | `marketplace_product_get` | ❌ |
| 13 | 0.175883 | `postgres_server_param_get` | ❌ |
| 14 | 0.174849 | `aks_nodepool_list` | ❌ |
| 15 | 0.172856 | `sql_server_firewall-rule_list` | ❌ |
| 16 | 0.170883 | `mysql_table_schema_get` | ❌ |
| 17 | 0.169792 | `deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.166729 | `applens_resource_diagnose` | ❌ |
| 19 | 0.165332 | `aks_cluster_get` | ❌ |
| 20 | 0.165173 | `deploy_plan_get` | ❌ |

---

## Test 204

**Expected Tool:** `azuremanagedlustre_filesystem_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836071 | `azuremanagedlustre_filesystem_sku_get` | ✅ **EXPECTED** |
| 2 | 0.626238 | `azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.453801 | `storage_account_get` | ❌ |
| 4 | 0.444792 | `search_service_list` | ❌ |
| 5 | 0.438893 | `quota_region_availability_list` | ❌ |
| 6 | 0.418743 | `foundry_agents_list` | ❌ |
| 7 | 0.411881 | `azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 8 | 0.411221 | `mysql_server_list` | ❌ |
| 9 | 0.405913 | `storage_account_create` | ❌ |
| 10 | 0.403188 | `acr_registry_list` | ❌ |
| 11 | 0.402635 | `quota_usage_check` | ❌ |
| 12 | 0.401697 | `resourcehealth_availability-status_list` | ❌ |
| 13 | 0.401538 | `kusto_cluster_list` | ❌ |
| 14 | 0.399919 | `datadog_monitoredresources_list` | ❌ |
| 15 | 0.398741 | `subscription_list` | ❌ |
| 16 | 0.398576 | `monitor_workspace_list` | ❌ |
| 17 | 0.395033 | `cosmos_account_list` | ❌ |
| 18 | 0.393969 | `eventgrid_subscription_list` | ❌ |
| 19 | 0.393471 | `redis_cluster_list` | ❌ |
| 20 | 0.392605 | `aks_cluster_list` | ❌ |

---

## Test 205

**Expected Tool:** `marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570145 | `marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.477522 | `marketplace_product_list` | ❌ |
| 3 | 0.353256 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.330935 | `servicebus_queue_details` | ❌ |
| 5 | 0.324083 | `search_index_get` | ❌ |
| 6 | 0.323644 | `servicebus_topic_details` | ❌ |
| 7 | 0.317352 | `loadtesting_testrun_get` | ❌ |
| 8 | 0.302335 | `aks_cluster_get` | ❌ |
| 9 | 0.294194 | `storage_blob_get` | ❌ |
| 10 | 0.289350 | `workbooks_show` | ❌ |
| 11 | 0.286194 | `keyvault_key_get` | ❌ |
| 12 | 0.285577 | `storage_account_get` | ❌ |
| 13 | 0.283554 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.276826 | `kusto_cluster_get` | ❌ |
| 15 | 0.275714 | `keyvault_secret_get` | ❌ |
| 16 | 0.274388 | `redis_cache_list` | ❌ |
| 17 | 0.266271 | `foundry_models_list` | ❌ |
| 18 | 0.259116 | `functionapp_get` | ❌ |
| 19 | 0.257285 | `aks_nodepool_get` | ❌ |
| 20 | 0.254258 | `foundry_knowledge_index_schema` | ❌ |

---

## Test 206

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527077 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443133 | `marketplace_product_get` | ❌ |
| 3 | 0.343549 | `search_service_list` | ❌ |
| 4 | 0.330500 | `foundry_models_list` | ❌ |
| 5 | 0.328676 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 6 | 0.324866 | `search_index_query` | ❌ |
| 7 | 0.302368 | `foundry_agents_list` | ❌ |
| 8 | 0.290877 | `get_bestpractices_get` | ❌ |
| 9 | 0.290185 | `search_index_get` | ❌ |
| 10 | 0.287924 | `cloudarchitect_design` | ❌ |
| 11 | 0.263954 | `deploy_architecture_diagram_generate` | ❌ |
| 12 | 0.263529 | `mysql_server_list` | ❌ |
| 13 | 0.258243 | `foundry_models_deployments_list` | ❌ |
| 14 | 0.254509 | `applens_resource_diagnose` | ❌ |
| 15 | 0.251537 | `deploy_app_logs_get` | ❌ |
| 16 | 0.250343 | `quota_region_availability_list` | ❌ |
| 17 | 0.248885 | `sql_server_entra-admin_list` | ❌ |
| 18 | 0.247644 | `deploy_plan_get` | ❌ |
| 19 | 0.245634 | `quota_usage_check` | ❌ |
| 20 | 0.245181 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 207

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461616 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385167 | `marketplace_product_get` | ❌ |
| 3 | 0.308769 | `foundry_models_list` | ❌ |
| 4 | 0.260387 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.259318 | `redis_cache_list` | ❌ |
| 6 | 0.238760 | `redis_cluster_list` | ❌ |
| 7 | 0.238238 | `postgres_server_list` | ❌ |
| 8 | 0.237988 | `grafana_list` | ❌ |
| 9 | 0.226689 | `search_service_list` | ❌ |
| 10 | 0.221138 | `appconfig_kv_show` | ❌ |
| 11 | 0.218709 | `foundry_agents_list` | ❌ |
| 12 | 0.208553 | `eventgrid_subscription_list` | ❌ |
| 13 | 0.204870 | `appconfig_account_list` | ❌ |
| 14 | 0.204011 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.203695 | `eventgrid_topic_list` | ❌ |
| 16 | 0.202641 | `workbooks_list` | ❌ |
| 17 | 0.202430 | `appconfig_kv_list` | ❌ |
| 18 | 0.201780 | `servicebus_topic_subscription_details` | ❌ |
| 19 | 0.187594 | `monitor_workspace_list` | ❌ |
| 20 | 0.185423 | `subscription_list` | ❌ |

---

## Test 208

**Expected Tool:** `bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `get_bestpractices_get` | ❌ |
| 2 | 0.635406 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.586936 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.531727 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490235 | `deploy_plan_get` | ❌ |
| 6 | 0.447777 | `deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.438801 | `cloudarchitect_design` | ❌ |
| 8 | 0.379177 | `applicationinsights_recommendation_list` | ❌ |
| 9 | 0.354178 | `applens_resource_diagnose` | ❌ |
| 10 | 0.353388 | `deploy_app_logs_get` | ❌ |
| 11 | 0.351664 | `quota_usage_check` | ❌ |
| 12 | 0.322702 | `resourcehealth_availability-status_get` | ❌ |
| 13 | 0.312391 | `quota_region_availability_list` | ❌ |
| 14 | 0.312077 | `storage_blob_container_create` | ❌ |
| 15 | 0.292039 | `sql_db_create` | ❌ |
| 16 | 0.290398 | `search_service_list` | ❌ |
| 17 | 0.282195 | `storage_blob_upload` | ❌ |
| 18 | 0.276297 | `storage_account_create` | ❌ |
| 19 | 0.273557 | `storage_account_get` | ❌ |
| 20 | 0.273060 | `storage_blob_container_get` | ❌ |

---

## Test 209

**Expected Tool:** `bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `get_bestpractices_get` | ❌ |
| 2 | 0.548542 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.541062 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.516852 | `deploy_plan_get` | ❌ |
| 5 | 0.516443 | `deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.424443 | `cloudarchitect_design` | ❌ |
| 7 | 0.424017 | `foundry_models_deployments_list` | ❌ |
| 8 | 0.409787 | `deploy_architecture_diagram_generate` | ❌ |
| 9 | 0.392192 | `deploy_app_logs_get` | ❌ |
| 10 | 0.369170 | `applens_resource_diagnose` | ❌ |
| 11 | 0.356143 | `resourcehealth_availability-status_get` | ❌ |
| 12 | 0.342487 | `quota_usage_check` | ❌ |
| 13 | 0.306627 | `quota_region_availability_list` | ❌ |
| 14 | 0.305636 | `sql_db_update` | ❌ |
| 15 | 0.304620 | `resourcehealth_availability-status_list` | ❌ |
| 16 | 0.304195 | `search_service_list` | ❌ |
| 17 | 0.302423 | `mysql_server_config_get` | ❌ |
| 18 | 0.302015 | `sql_server_show` | ❌ |
| 19 | 0.296142 | `sql_db_create` | ❌ |
| 20 | 0.291123 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 210

**Expected Tool:** `bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `get_bestpractices_get` | ❌ |
| 2 | 0.594323 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.518671 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.465572 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.454158 | `cloudarchitect_design` | ❌ |
| 6 | 0.430552 | `deploy_plan_get` | ❌ |
| 7 | 0.399433 | `deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.392733 | `applens_resource_diagnose` | ❌ |
| 9 | 0.383995 | `resourcehealth_availability-status_get` | ❌ |
| 10 | 0.380315 | `deploy_app_logs_get` | ❌ |
| 11 | 0.375863 | `quota_usage_check` | ❌ |
| 12 | 0.362669 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.335296 | `sql_server_show` | ❌ |
| 14 | 0.331683 | `storage_blob_get` | ❌ |
| 15 | 0.329342 | `quota_region_availability_list` | ❌ |
| 16 | 0.322718 | `storage_account_get` | ❌ |
| 17 | 0.322410 | `storage_blob_container_get` | ❌ |
| 18 | 0.317765 | `marketplace_product_get` | ❌ |
| 19 | 0.316805 | `resourcehealth_availability-status_list` | ❌ |
| 20 | 0.314841 | `search_service_list` | ❌ |

---

## Test 211

**Expected Tool:** `bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `get_bestpractices_get` | ❌ |
| 2 | 0.570488 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.523002 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.445321 | `deploy_plan_get` | ❌ |
| 6 | 0.400447 | `deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.381822 | `cloudarchitect_design` | ❌ |
| 8 | 0.368217 | `deploy_app_logs_get` | ❌ |
| 9 | 0.367714 | `functionapp_get` | ❌ |
| 10 | 0.353416 | `applicationinsights_recommendation_list` | ❌ |
| 11 | 0.317494 | `quota_usage_check` | ❌ |
| 12 | 0.292977 | `storage_blob_upload` | ❌ |
| 13 | 0.284617 | `storage_blob_container_create` | ❌ |
| 14 | 0.278941 | `quota_region_availability_list` | ❌ |
| 15 | 0.275278 | `resourcehealth_availability-status_get` | ❌ |
| 16 | 0.256382 | `mysql_server_config_get` | ❌ |
| 17 | 0.252529 | `sql_db_create` | ❌ |
| 18 | 0.241745 | `search_index_query` | ❌ |
| 19 | 0.239985 | `storage_blob_get` | ❌ |
| 20 | 0.239436 | `search_service_list` | ❌ |

---

## Test 212

**Expected Tool:** `bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581850 | `get_bestpractices_get` | ❌ |
| 2 | 0.497350 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495618 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.486886 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.474511 | `deploy_plan_get` | ❌ |
| 6 | 0.439182 | `foundry_models_deployments_list` | ❌ |
| 7 | 0.412037 | `deploy_app_logs_get` | ❌ |
| 8 | 0.399571 | `functionapp_get` | ❌ |
| 9 | 0.377790 | `deploy_architecture_diagram_generate` | ❌ |
| 10 | 0.373497 | `cloudarchitect_design` | ❌ |
| 11 | 0.323091 | `resourcehealth_availability-status_get` | ❌ |
| 12 | 0.317931 | `quota_usage_check` | ❌ |
| 13 | 0.303572 | `storage_blob_upload` | ❌ |
| 14 | 0.290695 | `mysql_server_config_get` | ❌ |
| 15 | 0.277946 | `quota_region_availability_list` | ❌ |
| 16 | 0.276161 | `resourcehealth_service-health-events_list` | ❌ |
| 17 | 0.275785 | `sql_db_update` | ❌ |
| 18 | 0.270375 | `search_service_list` | ❌ |
| 19 | 0.269415 | `sql_server_show` | ❌ |
| 20 | 0.269109 | `storage_blob_container_create` | ❌ |

---

## Test 213

**Expected Tool:** `bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `get_bestpractices_get` | ❌ |
| 2 | 0.532790 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.487333 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.458060 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.413150 | `functionapp_get` | ❌ |
| 6 | 0.395991 | `deploy_app_logs_get` | ❌ |
| 7 | 0.394762 | `cloudarchitect_design` | ❌ |
| 8 | 0.394214 | `deploy_plan_get` | ❌ |
| 9 | 0.375665 | `applens_resource_diagnose` | ❌ |
| 10 | 0.363596 | `deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.332532 | `resourcehealth_availability-status_get` | ❌ |
| 12 | 0.332015 | `quota_usage_check` | ❌ |
| 13 | 0.307885 | `storage_blob_upload` | ❌ |
| 14 | 0.290929 | `resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.289428 | `storage_blob_container_create` | ❌ |
| 16 | 0.289326 | `mysql_server_config_get` | ❌ |
| 17 | 0.284975 | `sql_server_show` | ❌ |
| 18 | 0.284215 | `quota_region_availability_list` | ❌ |
| 19 | 0.275538 | `search_index_query` | ❌ |
| 20 | 0.275498 | `storage_blob_get` | ❌ |

---

## Test 214

**Expected Tool:** `bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `get_bestpractices_get` | ❌ |
| 2 | 0.513262 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.505138 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.405163 | `deploy_app_logs_get` | ❌ |
| 6 | 0.401209 | `deploy_plan_get` | ❌ |
| 7 | 0.398226 | `deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.389556 | `cloudarchitect_design` | ❌ |
| 9 | 0.334566 | `applens_resource_diagnose` | ❌ |
| 10 | 0.315539 | `resourcehealth_availability-status_get` | ❌ |
| 11 | 0.312250 | `functionapp_get` | ❌ |
| 12 | 0.292282 | `storage_blob_upload` | ❌ |
| 13 | 0.283198 | `quota_usage_check` | ❌ |
| 14 | 0.275578 | `storage_blob_container_create` | ❌ |
| 15 | 0.258767 | `search_index_query` | ❌ |
| 16 | 0.256800 | `sql_db_create` | ❌ |
| 17 | 0.256751 | `search_service_list` | ❌ |
| 18 | 0.255215 | `storage_blob_get` | ❌ |
| 19 | 0.253386 | `sql_db_update` | ❌ |
| 20 | 0.251300 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 215

**Expected Tool:** `bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `get_bestpractices_get` | ❌ |
| 2 | 0.500368 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.472115 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.433134 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.385965 | `cloudarchitect_design` | ❌ |
| 6 | 0.381179 | `functionapp_get` | ❌ |
| 7 | 0.374583 | `applens_resource_diagnose` | ❌ |
| 8 | 0.368831 | `deploy_plan_get` | ❌ |
| 9 | 0.358748 | `deploy_app_logs_get` | ❌ |
| 10 | 0.337024 | `deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.293848 | `quota_usage_check` | ❌ |
| 12 | 0.288873 | `storage_blob_upload` | ❌ |
| 13 | 0.259723 | `mysql_database_query` | ❌ |
| 14 | 0.253005 | `storage_blob_container_create` | ❌ |
| 15 | 0.251196 | `resourcehealth_availability-status_get` | ❌ |
| 16 | 0.249981 | `monitor_resource_log_query` | ❌ |
| 17 | 0.246347 | `workbooks_delete` | ❌ |
| 18 | 0.240146 | `resourcehealth_service-health-events_list` | ❌ |
| 19 | 0.231234 | `search_index_query` | ❌ |
| 20 | 0.231120 | `mysql_server_config_get` | ❌ |

---

## Test 216

**Expected Tool:** `bestpractices_get`  
**Prompt:** Create the plan for creating a simple HTTP-triggered function app in javascript that returns a random compliment from a predefined list in a JSON response. And deploy it to azure eventually. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429159 | `deploy_plan_get` | ❌ |
| 2 | 0.408233 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.380754 | `cloudarchitect_design` | ❌ |
| 4 | 0.377184 | `get_bestpractices_get` | ❌ |
| 5 | 0.352316 | `deploy_iac_rules_get` | ❌ |
| 6 | 0.345059 | `deploy_architecture_diagram_generate` | ❌ |
| 7 | 0.319863 | `loadtesting_test_create` | ❌ |
| 8 | 0.311848 | `azureterraformbestpractices_get` | ❌ |
| 9 | 0.301028 | `functionapp_get` | ❌ |
| 10 | 0.299203 | `deploy_app_logs_get` | ❌ |
| 11 | 0.235579 | `storage_blob_upload` | ❌ |
| 12 | 0.232320 | `quota_usage_check` | ❌ |
| 13 | 0.218912 | `workbooks_create` | ❌ |
| 14 | 0.215940 | `storage_blob_container_create` | ❌ |
| 15 | 0.210908 | `quota_region_availability_list` | ❌ |
| 16 | 0.206254 | `sql_db_create` | ❌ |
| 17 | 0.203401 | `search_index_query` | ❌ |
| 18 | 0.202251 | `storage_account_create` | ❌ |
| 19 | 0.197959 | `mysql_database_query` | ❌ |
| 20 | 0.186514 | `sql_server_create` | ❌ |

---

## Test 217

**Expected Tool:** `bestpractices_get`  
**Prompt:** Create the plan for creating a to-do list app. And deploy it to azure as a container app. But don't create any code until I confirm.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.497276 | `deploy_plan_get` | ❌ |
| 2 | 0.493182 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.405146 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.395579 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.385140 | `get_bestpractices_get` | ❌ |
| 6 | 0.374154 | `cloudarchitect_design` | ❌ |
| 7 | 0.354448 | `azureterraformbestpractices_get` | ❌ |
| 8 | 0.348236 | `deploy_app_logs_get` | ❌ |
| 9 | 0.300125 | `loadtesting_test_create` | ❌ |
| 10 | 0.284049 | `storage_blob_container_create` | ❌ |
| 11 | 0.266937 | `foundry_models_deploy` | ❌ |
| 12 | 0.248999 | `sql_db_create` | ❌ |
| 13 | 0.243575 | `quota_usage_check` | ❌ |
| 14 | 0.234797 | `storage_account_create` | ❌ |
| 15 | 0.221235 | `storage_blob_container_get` | ❌ |
| 16 | 0.218621 | `quota_region_availability_list` | ❌ |
| 17 | 0.210666 | `storage_blob_upload` | ❌ |
| 18 | 0.209213 | `workbooks_create` | ❌ |
| 19 | 0.208812 | `mysql_server_list` | ❌ |
| 20 | 0.195544 | `sql_server_create` | ❌ |

---

## Test 218

**Expected Tool:** `monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498365 | `monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.472119 | `monitor_workspace_list` | ❌ |
| 3 | 0.468185 | `monitor_table_list` | ❌ |
| 4 | 0.467958 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.463000 | `resourcehealth_availability-status_get` | ❌ |
| 6 | 0.436981 | `deploy_app_logs_get` | ❌ |
| 7 | 0.418724 | `resourcehealth_availability-status_list` | ❌ |
| 8 | 0.413466 | `monitor_table_type_list` | ❌ |
| 9 | 0.401591 | `monitor_resource_log_query` | ❌ |
| 10 | 0.385793 | `resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.379945 | `grafana_list` | ❌ |
| 12 | 0.358961 | `monitor_metrics_query` | ❌ |
| 13 | 0.342882 | `aks_nodepool_get` | ❌ |
| 14 | 0.339427 | `aks_cluster_get` | ❌ |
| 15 | 0.333436 | `loadtesting_testrun_get` | ❌ |
| 16 | 0.314436 | `applens_resource_diagnose` | ❌ |
| 17 | 0.305757 | `deploy_architecture_diagram_generate` | ❌ |
| 18 | 0.303095 | `foundry_agents_list` | ❌ |
| 19 | 0.297852 | `aks_cluster_list` | ❌ |
| 20 | 0.296680 | `azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 219

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592579 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424070 | `monitor_metrics_query` | ❌ |
| 3 | 0.332356 | `monitor_table_type_list` | ❌ |
| 4 | 0.315519 | `azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.315190 | `servicebus_topic_details` | ❌ |
| 6 | 0.311108 | `servicebus_topic_subscription_details` | ❌ |
| 7 | 0.305464 | `servicebus_queue_details` | ❌ |
| 8 | 0.304735 | `grafana_list` | ❌ |
| 9 | 0.303453 | `datadog_monitoredresources_list` | ❌ |
| 10 | 0.298800 | `resourcehealth_availability-status_get` | ❌ |
| 11 | 0.294124 | `quota_region_availability_list` | ❌ |
| 12 | 0.287300 | `monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.284519 | `resourcehealth_availability-status_list` | ❌ |
| 14 | 0.277566 | `kusto_table_schema` | ❌ |
| 15 | 0.274784 | `loadtesting_test_get` | ❌ |
| 16 | 0.262141 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.256836 | `foundry_knowledge_index_schema` | ❌ |
| 18 | 0.254848 | `aks_nodepool_get` | ❌ |
| 19 | 0.249144 | `aks_cluster_get` | ❌ |
| 20 | 0.248987 | `bicepschema_get` | ❌ |

---

## Test 220

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589859 | `storage_account_get` | ❌ |
| 2 | 0.587659 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.550581 | `storage_blob_container_get` | ❌ |
| 4 | 0.473421 | `azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.472264 | `storage_blob_get` | ❌ |
| 6 | 0.459829 | `cosmos_account_list` | ❌ |
| 7 | 0.439032 | `storage_account_create` | ❌ |
| 8 | 0.437739 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.431109 | `appconfig_kv_show` | ❌ |
| 10 | 0.417098 | `resourcehealth_availability-status_list` | ❌ |
| 11 | 0.414488 | `cosmos_database_container_list` | ❌ |
| 12 | 0.403921 | `quota_usage_check` | ❌ |
| 13 | 0.401894 | `monitor_metrics_query` | ❌ |
| 14 | 0.397526 | `appconfig_kv_list` | ❌ |
| 15 | 0.391340 | `monitor_table_type_list` | ❌ |
| 16 | 0.390422 | `cosmos_database_list` | ❌ |
| 17 | 0.383443 | `resourcehealth_availability-status_get` | ❌ |
| 18 | 0.371164 | `foundry_agents_list` | ❌ |
| 19 | 0.359476 | `appconfig_account_list` | ❌ |
| 20 | 0.357647 | `datadog_monitoredresources_list` | ❌ |

---

## Test 221

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633072 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495455 | `monitor_metrics_query` | ❌ |
| 3 | 0.382534 | `applens_resource_diagnose` | ❌ |
| 4 | 0.380436 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.370848 | `monitor_table_type_list` | ❌ |
| 6 | 0.359089 | `azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.353235 | `resourcehealth_availability-status_list` | ❌ |
| 8 | 0.344326 | `quota_usage_check` | ❌ |
| 9 | 0.341713 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.337874 | `monitor_resource_log_query` | ❌ |
| 11 | 0.329494 | `loadtesting_testresource_list` | ❌ |
| 12 | 0.326682 | `foundry_agents_list` | ❌ |
| 13 | 0.324002 | `datadog_monitoredresources_list` | ❌ |
| 14 | 0.322121 | `applicationinsights_recommendation_list` | ❌ |
| 15 | 0.317475 | `monitor_workspace_log_query` | ❌ |
| 16 | 0.302823 | `monitor_table_list` | ❌ |
| 17 | 0.301575 | `workbooks_show` | ❌ |
| 18 | 0.291565 | `cloudarchitect_design` | ❌ |
| 19 | 0.291249 | `deploy_app_logs_get` | ❌ |
| 20 | 0.287764 | `loadtesting_testrun_get` | ❌ |

---

## Test 222

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555527 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.447607 | `monitor_resource_log_query` | ❌ |
| 3 | 0.447294 | `applens_resource_diagnose` | ❌ |
| 4 | 0.433812 | `loadtesting_testrun_get` | ❌ |
| 5 | 0.422333 | `resourcehealth_availability-status_get` | ❌ |
| 6 | 0.416100 | `monitor_workspace_log_query` | ❌ |
| 7 | 0.413282 | `applicationinsights_recommendation_list` | ❌ |
| 8 | 0.409134 | `deploy_app_logs_get` | ❌ |
| 9 | 0.388205 | `quota_usage_check` | ❌ |
| 10 | 0.380075 | `resourcehealth_availability-status_list` | ❌ |
| 11 | 0.356549 | `functionapp_get` | ❌ |
| 12 | 0.350085 | `loadtesting_testrun_list` | ❌ |
| 13 | 0.341791 | `deploy_architecture_diagram_generate` | ❌ |
| 14 | 0.339718 | `loadtesting_testresource_list` | ❌ |
| 15 | 0.335318 | `monitor_metrics_definitions` | ❌ |
| 16 | 0.329267 | `loadtesting_testresource_create` | ❌ |
| 17 | 0.327339 | `resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.326790 | `workbooks_show` | ❌ |
| 19 | 0.326398 | `datadog_monitoredresources_list` | ❌ |
| 20 | 0.320852 | `search_index_query` | ❌ |

---

## Test 223

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557872 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.508669 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.460611 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `quota_usage_check` | ❌ |
| 5 | 0.438156 | `monitor_metrics_definitions` | ❌ |
| 6 | 0.392094 | `monitor_resource_log_query` | ❌ |
| 7 | 0.391809 | `applens_resource_diagnose` | ❌ |
| 8 | 0.372992 | `deploy_app_logs_get` | ❌ |
| 9 | 0.368589 | `monitor_workspace_log_query` | ❌ |
| 10 | 0.354733 | `applicationinsights_recommendation_list` | ❌ |
| 11 | 0.339388 | `datadog_monitoredresources_list` | ❌ |
| 12 | 0.336638 | `loadtesting_testrun_get` | ❌ |
| 13 | 0.326836 | `loadtesting_testresource_list` | ❌ |
| 14 | 0.326643 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.321538 | `search_service_list` | ❌ |
| 16 | 0.321225 | `foundry_agents_list` | ❌ |
| 17 | 0.318196 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.317565 | `functionapp_get` | ❌ |
| 19 | 0.303931 | `resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.303909 | `quota_region_availability_list` | ❌ |

---

## Test 224

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461550 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.389442 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.306305 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.303501 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.302222 | `monitor_resource_log_query` | ❌ |
| 6 | 0.289565 | `monitor_workspace_log_query` | ❌ |
| 7 | 0.275428 | `monitor_table_type_list` | ❌ |
| 8 | 0.267387 | `datadog_monitoredresources_list` | ❌ |
| 9 | 0.267293 | `monitor_healthmodels_entity_gethealth` | ❌ |
| 10 | 0.265722 | `azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.263285 | `quota_usage_check` | ❌ |
| 12 | 0.263235 | `quota_region_availability_list` | ❌ |
| 13 | 0.258647 | `grafana_list` | ❌ |
| 14 | 0.252867 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.249280 | `loadtesting_testresource_list` | ❌ |
| 16 | 0.248358 | `loadtesting_test_get` | ❌ |
| 17 | 0.247650 | `applens_resource_diagnose` | ❌ |
| 18 | 0.243171 | `loadtesting_testrun_get` | ❌ |
| 19 | 0.235115 | `kusto_table_schema` | ❌ |
| 20 | 0.229879 | `loadtesting_testrun_list` | ❌ |

---

## Test 225

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.491784 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.416945 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.415966 | `monitor_resource_log_query` | ❌ |
| 4 | 0.406284 | `applens_resource_diagnose` | ❌ |
| 5 | 0.399007 | `deploy_app_logs_get` | ❌ |
| 6 | 0.397335 | `quota_usage_check` | ❌ |
| 7 | 0.369668 | `applicationinsights_recommendation_list` | ❌ |
| 8 | 0.366959 | `monitor_workspace_log_query` | ❌ |
| 9 | 0.362042 | `loadtesting_testrun_get` | ❌ |
| 10 | 0.359340 | `resourcehealth_availability-status_list` | ❌ |
| 11 | 0.332322 | `resourcehealth_service-health-events_list` | ❌ |
| 12 | 0.316281 | `loadtesting_testresource_list` | ❌ |
| 13 | 0.315326 | `functionapp_get` | ❌ |
| 14 | 0.311842 | `search_index_query` | ❌ |
| 15 | 0.308672 | `monitor_metrics_definitions` | ❌ |
| 16 | 0.295918 | `datadog_monitoredresources_list` | ❌ |
| 17 | 0.293608 | `search_service_list` | ❌ |
| 18 | 0.293120 | `loadtesting_testresource_create` | ❌ |
| 19 | 0.289035 | `foundry_agents_connect` | ❌ |
| 20 | 0.287126 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 226

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525352 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.384442 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.376658 | `monitor_resource_log_query` | ❌ |
| 4 | 0.367167 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.299448 | `quota_usage_check` | ❌ |
| 6 | 0.292929 | `resourcehealth_availability-status_get` | ❌ |
| 7 | 0.290172 | `loadtesting_testrun_get` | ❌ |
| 8 | 0.277697 | `monitor_healthmodels_entity_gethealth` | ❌ |
| 9 | 0.272349 | `monitor_table_type_list` | ❌ |
| 10 | 0.267076 | `datadog_monitoredresources_list` | ❌ |
| 11 | 0.266376 | `mysql_server_param_get` | ❌ |
| 12 | 0.265620 | `applens_resource_diagnose` | ❌ |
| 13 | 0.262699 | `resourcehealth_availability-status_list` | ❌ |
| 14 | 0.261986 | `grafana_list` | ❌ |
| 15 | 0.261656 | `loadtesting_testrun_list` | ❌ |
| 16 | 0.248226 | `foundry_agents_query-and-evaluate` | ❌ |
| 17 | 0.246502 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.244147 | `cosmos_database_container_item_query` | ❌ |
| 19 | 0.242689 | `loadtesting_test_get` | ❌ |
| 20 | 0.239400 | `azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 227

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480388 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.381879 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.363412 | `quota_usage_check` | ❌ |
| 4 | 0.359401 | `applens_resource_diagnose` | ❌ |
| 5 | 0.350523 | `monitor_resource_log_query` | ❌ |
| 6 | 0.350491 | `monitor_workspace_log_query` | ❌ |
| 7 | 0.346343 | `applicationinsights_recommendation_list` | ❌ |
| 8 | 0.331139 | `loadtesting_testresource_list` | ❌ |
| 9 | 0.330074 | `resourcehealth_availability-status_list` | ❌ |
| 10 | 0.328731 | `monitor_metrics_definitions` | ❌ |
| 11 | 0.324932 | `search_index_query` | ❌ |
| 12 | 0.319106 | `loadtesting_testresource_create` | ❌ |
| 13 | 0.317502 | `loadtesting_testrun_get` | ❌ |
| 14 | 0.292188 | `deploy_app_logs_get` | ❌ |
| 15 | 0.290762 | `search_service_list` | ❌ |
| 16 | 0.284265 | `foundry_agents_connect` | ❌ |
| 17 | 0.282267 | `functionapp_get` | ❌ |
| 18 | 0.278327 | `workbooks_show` | ❌ |
| 19 | 0.276999 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.265303 | `azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 228

**Expected Tool:** `monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593906 | `monitor_workspace_log_query` | ❌ |
| 2 | 0.580238 | `monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.471775 | `deploy_app_logs_get` | ❌ |
| 4 | 0.470265 | `monitor_metrics_query` | ❌ |
| 5 | 0.443043 | `monitor_workspace_list` | ❌ |
| 6 | 0.442582 | `monitor_table_list` | ❌ |
| 7 | 0.392123 | `monitor_table_type_list` | ❌ |
| 8 | 0.389906 | `grafana_list` | ❌ |
| 9 | 0.366149 | `resourcehealth_availability-status_get` | ❌ |
| 10 | 0.359102 | `resourcehealth_availability-status_list` | ❌ |
| 11 | 0.352673 | `datadog_monitoredresources_list` | ❌ |
| 12 | 0.345359 | `quota_usage_check` | ❌ |
| 13 | 0.345252 | `resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.337915 | `applens_resource_diagnose` | ❌ |
| 15 | 0.320705 | `loadtesting_testrun_get` | ❌ |
| 16 | 0.313577 | `applicationinsights_recommendation_list` | ❌ |
| 17 | 0.308865 | `eventgrid_subscription_list` | ❌ |
| 18 | 0.307827 | `aks_cluster_get` | ❌ |
| 19 | 0.307196 | `azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.305105 | `loadtesting_testrun_list` | ❌ |

---

## Test 229

**Expected Tool:** `monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851075 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `monitor_workspace_list` | ❌ |
| 4 | 0.534829 | `mysql_table_list` | ❌ |
| 5 | 0.511123 | `kusto_table_list` | ❌ |
| 6 | 0.502075 | `grafana_list` | ❌ |
| 7 | 0.488557 | `postgres_table_list` | ❌ |
| 8 | 0.443812 | `monitor_workspace_log_query` | ❌ |
| 9 | 0.420394 | `cosmos_database_list` | ❌ |
| 10 | 0.419859 | `kusto_database_list` | ❌ |
| 11 | 0.413834 | `mysql_database_list` | ❌ |
| 12 | 0.409199 | `monitor_resource_log_query` | ❌ |
| 13 | 0.400092 | `workbooks_list` | ❌ |
| 14 | 0.397408 | `kusto_table_schema` | ❌ |
| 15 | 0.396780 | `search_service_list` | ❌ |
| 16 | 0.377057 | `foundry_agents_list` | ❌ |
| 17 | 0.375149 | `deploy_app_logs_get` | ❌ |
| 18 | 0.374930 | `cosmos_database_container_list` | ❌ |
| 19 | 0.366099 | `kusto_sample` | ❌ |
| 20 | 0.365781 | `cosmos_account_list` | ❌ |

---

## Test 230

**Expected Tool:** `monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798460 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `monitor_table_type_list` | ❌ |
| 3 | 0.599917 | `monitor_workspace_list` | ❌ |
| 4 | 0.497065 | `mysql_table_list` | ❌ |
| 5 | 0.487237 | `grafana_list` | ❌ |
| 6 | 0.466630 | `kusto_table_list` | ❌ |
| 7 | 0.449407 | `monitor_workspace_log_query` | ❌ |
| 8 | 0.427408 | `postgres_table_list` | ❌ |
| 9 | 0.413678 | `monitor_resource_log_query` | ❌ |
| 10 | 0.411590 | `kusto_table_schema` | ❌ |
| 11 | 0.403836 | `deploy_app_logs_get` | ❌ |
| 12 | 0.398753 | `mysql_table_schema_get` | ❌ |
| 13 | 0.389881 | `mysql_database_list` | ❌ |
| 14 | 0.376474 | `kusto_sample` | ❌ |
| 15 | 0.376338 | `kusto_database_list` | ❌ |
| 16 | 0.373298 | `workbooks_list` | ❌ |
| 17 | 0.370624 | `cosmos_database_list` | ❌ |
| 18 | 0.347853 | `cosmos_database_container_list` | ❌ |
| 19 | 0.346183 | `foundry_agents_list` | ❌ |
| 20 | 0.343837 | `azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 231

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765702 | `monitor_table_list` | ❌ |
| 3 | 0.569921 | `monitor_workspace_list` | ❌ |
| 4 | 0.504683 | `mysql_table_list` | ❌ |
| 5 | 0.477280 | `grafana_list` | ❌ |
| 6 | 0.447435 | `kusto_table_list` | ❌ |
| 7 | 0.445347 | `mysql_table_schema_get` | ❌ |
| 8 | 0.418517 | `postgres_table_list` | ❌ |
| 9 | 0.416351 | `kusto_table_schema` | ❌ |
| 10 | 0.412293 | `mysql_database_list` | ❌ |
| 11 | 0.404852 | `monitor_workspace_log_query` | ❌ |
| 12 | 0.404113 | `monitor_metrics_definitions` | ❌ |
| 13 | 0.383606 | `azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.380581 | `kusto_sample` | ❌ |
| 15 | 0.374197 | `foundry_agents_list` | ❌ |
| 16 | 0.372490 | `monitor_resource_log_query` | ❌ |
| 17 | 0.369889 | `cosmos_database_list` | ❌ |
| 18 | 0.361820 | `kusto_database_list` | ❌ |
| 19 | 0.354757 | `kusto_cluster_list` | ❌ |
| 20 | 0.351333 | `aks_nodepool_list` | ❌ |

---

## Test 232

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843138 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736837 | `monitor_table_list` | ❌ |
| 3 | 0.576731 | `monitor_workspace_list` | ❌ |
| 4 | 0.481189 | `mysql_table_list` | ❌ |
| 5 | 0.475734 | `grafana_list` | ❌ |
| 6 | 0.451212 | `mysql_table_schema_get` | ❌ |
| 7 | 0.427934 | `kusto_table_schema` | ❌ |
| 8 | 0.427153 | `monitor_workspace_log_query` | ❌ |
| 9 | 0.421484 | `kusto_table_list` | ❌ |
| 10 | 0.406242 | `mysql_database_list` | ❌ |
| 11 | 0.391308 | `kusto_sample` | ❌ |
| 12 | 0.384679 | `monitor_resource_log_query` | ❌ |
| 13 | 0.376171 | `monitor_metrics_definitions` | ❌ |
| 14 | 0.372991 | `postgres_table_list` | ❌ |
| 15 | 0.370860 | `azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.367568 | `deploy_app_logs_get` | ❌ |
| 17 | 0.355178 | `foundry_agents_list` | ❌ |
| 18 | 0.348357 | `cosmos_database_list` | ❌ |
| 19 | 0.340101 | `foundry_models_list` | ❌ |
| 20 | 0.339804 | `kusto_cluster_list` | ❌ |

---

## Test 233

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `grafana_list` | ❌ |
| 3 | 0.659497 | `monitor_table_list` | ❌ |
| 4 | 0.600802 | `search_service_list` | ❌ |
| 5 | 0.583213 | `monitor_table_type_list` | ❌ |
| 6 | 0.530433 | `kusto_cluster_list` | ❌ |
| 7 | 0.517493 | `cosmos_account_list` | ❌ |
| 8 | 0.513679 | `aks_cluster_list` | ❌ |
| 9 | 0.500768 | `workbooks_list` | ❌ |
| 10 | 0.494683 | `group_list` | ❌ |
| 11 | 0.493730 | `subscription_list` | ❌ |
| 12 | 0.475212 | `monitor_workspace_log_query` | ❌ |
| 13 | 0.471758 | `redis_cluster_list` | ❌ |
| 14 | 0.470266 | `postgres_server_list` | ❌ |
| 15 | 0.467655 | `appconfig_account_list` | ❌ |
| 16 | 0.466729 | `acr_registry_list` | ❌ |
| 17 | 0.464047 | `foundry_agents_list` | ❌ |
| 18 | 0.460481 | `redis_cache_list` | ❌ |
| 19 | 0.448201 | `kusto_database_list` | ❌ |
| 20 | 0.444211 | `loadtesting_testresource_list` | ❌ |

---

## Test 234

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.584758 | `monitor_table_list` | ❌ |
| 3 | 0.531083 | `monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `grafana_list` | ❌ |
| 5 | 0.474745 | `monitor_workspace_log_query` | ❌ |
| 6 | 0.459793 | `deploy_app_logs_get` | ❌ |
| 7 | 0.444207 | `search_service_list` | ❌ |
| 8 | 0.414153 | `foundry_agents_list` | ❌ |
| 9 | 0.386422 | `workbooks_list` | ❌ |
| 10 | 0.383601 | `aks_cluster_list` | ❌ |
| 11 | 0.380891 | `monitor_resource_log_query` | ❌ |
| 12 | 0.373786 | `cosmos_account_list` | ❌ |
| 13 | 0.371395 | `azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.363287 | `resourcehealth_availability-status_list` | ❌ |
| 15 | 0.358029 | `kusto_cluster_list` | ❌ |
| 16 | 0.354811 | `deploy_architecture_diagram_generate` | ❌ |
| 17 | 0.354276 | `cosmos_database_list` | ❌ |
| 18 | 0.353651 | `subscription_list` | ❌ |
| 19 | 0.352756 | `acr_registry_list` | ❌ |
| 20 | 0.351453 | `search_index_get` | ❌ |

---

## Test 235

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `grafana_list` | ❌ |
| 3 | 0.580261 | `monitor_table_list` | ❌ |
| 4 | 0.521316 | `monitor_table_type_list` | ❌ |
| 5 | 0.521276 | `search_service_list` | ❌ |
| 6 | 0.463378 | `monitor_workspace_log_query` | ❌ |
| 7 | 0.453659 | `deploy_app_logs_get` | ❌ |
| 8 | 0.439297 | `kusto_cluster_list` | ❌ |
| 9 | 0.435071 | `workbooks_list` | ❌ |
| 10 | 0.428945 | `cosmos_account_list` | ❌ |
| 11 | 0.427226 | `aks_cluster_list` | ❌ |
| 12 | 0.422707 | `subscription_list` | ❌ |
| 13 | 0.422356 | `loadtesting_testresource_list` | ❌ |
| 14 | 0.411630 | `acr_registry_list` | ❌ |
| 15 | 0.411448 | `resourcehealth_availability-status_list` | ❌ |
| 16 | 0.410082 | `azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.409827 | `foundry_agents_list` | ❌ |
| 18 | 0.404262 | `group_list` | ❌ |
| 19 | 0.402600 | `redis_cluster_list` | ❌ |
| 20 | 0.400615 | `postgres_server_list` | ❌ |

---

## Test 236

**Expected Tool:** `monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591648 | `monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.494715 | `monitor_resource_log_query` | ❌ |
| 3 | 0.485984 | `monitor_table_list` | ❌ |
| 4 | 0.484088 | `deploy_app_logs_get` | ❌ |
| 5 | 0.483323 | `monitor_workspace_list` | ❌ |
| 6 | 0.427241 | `monitor_table_type_list` | ❌ |
| 7 | 0.375438 | `monitor_metrics_query` | ❌ |
| 8 | 0.365704 | `grafana_list` | ❌ |
| 9 | 0.330307 | `resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.322875 | `workbooks_delete` | ❌ |
| 11 | 0.322324 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 12 | 0.315638 | `search_service_list` | ❌ |
| 13 | 0.309477 | `loadtesting_testrun_get` | ❌ |
| 14 | 0.299851 | `applens_resource_diagnose` | ❌ |
| 15 | 0.292089 | `loadtesting_testrun_list` | ❌ |
| 16 | 0.291669 | `kusto_query` | ❌ |
| 17 | 0.289781 | `foundry_agents_list` | ❌ |
| 18 | 0.288625 | `aks_cluster_list` | ❌ |
| 19 | 0.287253 | `aks_cluster_get` | ❌ |
| 20 | 0.283294 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 237

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668827 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.434836 | `redis_cache_list` | ❌ |
| 3 | 0.413232 | `monitor_metrics_query` | ❌ |
| 4 | 0.408658 | `redis_cluster_list` | ❌ |
| 5 | 0.401731 | `grafana_list` | ❌ |
| 6 | 0.393318 | `resourcehealth_availability-status_list` | ❌ |
| 7 | 0.386680 | `monitor_metrics_definitions` | ❌ |
| 8 | 0.369805 | `redis_cluster_database_list` | ❌ |
| 9 | 0.364076 | `workbooks_list` | ❌ |
| 10 | 0.356643 | `mysql_server_list` | ❌ |
| 11 | 0.355391 | `loadtesting_testresource_list` | ❌ |
| 12 | 0.345409 | `postgres_database_list` | ❌ |
| 13 | 0.345382 | `group_list` | ❌ |
| 14 | 0.330769 | `postgres_table_list` | ❌ |
| 15 | 0.328960 | `foundry_agents_list` | ❌ |
| 16 | 0.327205 | `cosmos_database_list` | ❌ |
| 17 | 0.306977 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.304097 | `cosmos_account_list` | ❌ |
| 19 | 0.302405 | `acr_registry_repository_list` | ❌ |
| 20 | 0.296544 | `cosmos_database_container_list` | ❌ |

---

## Test 238

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443840 | `monitor_metrics_query` | ❌ |
| 3 | 0.393260 | `redis_cache_list` | ❌ |
| 4 | 0.374071 | `redis_cluster_list` | ❌ |
| 5 | 0.371017 | `grafana_list` | ❌ |
| 6 | 0.370681 | `resourcehealth_availability-status_list` | ❌ |
| 7 | 0.359262 | `monitor_metrics_definitions` | ❌ |
| 8 | 0.350656 | `quota_usage_check` | ❌ |
| 9 | 0.343181 | `loadtesting_testresource_list` | ❌ |
| 10 | 0.342468 | `redis_cluster_database_list` | ❌ |
| 11 | 0.337109 | `mysql_server_list` | ❌ |
| 12 | 0.320462 | `resourcehealth_availability-status_get` | ❌ |
| 13 | 0.319895 | `workbooks_list` | ❌ |
| 14 | 0.302947 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.289883 | `eventgrid_subscription_list` | ❌ |
| 16 | 0.287390 | `foundry_agents_list` | ❌ |
| 17 | 0.285326 | `group_list` | ❌ |
| 18 | 0.274836 | `applicationinsights_recommendation_list` | ❌ |
| 19 | 0.274575 | `deploy_app_logs_get` | ❌ |
| 20 | 0.272689 | `loadtesting_testrun_list` | ❌ |

---

## Test 239

**Expected Tool:** `extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `quota_usage_check` | ❌ |
| 2 | 0.497413 | `applens_resource_diagnose` | ❌ |
| 3 | 0.481143 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.476826 | `extension_azqr` | ✅ **EXPECTED** |
| 5 | 0.462075 | `applicationinsights_recommendation_list` | ❌ |
| 6 | 0.451690 | `get_bestpractices_get` | ❌ |
| 7 | 0.440399 | `resourcehealth_availability-status_list` | ❌ |
| 8 | 0.438387 | `cloudarchitect_design` | ❌ |
| 9 | 0.434685 | `search_service_list` | ❌ |
| 10 | 0.431119 | `deploy_iac_rules_get` | ❌ |
| 11 | 0.423237 | `subscription_list` | ❌ |
| 12 | 0.422218 | `resourcehealth_availability-status_get` | ❌ |
| 13 | 0.416798 | `resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.408023 | `deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.406591 | `deploy_plan_get` | ❌ |
| 16 | 0.400363 | `quota_region_availability_list` | ❌ |
| 17 | 0.395234 | `eventgrid_subscription_list` | ❌ |
| 18 | 0.391633 | `marketplace_product_get` | ❌ |
| 19 | 0.388980 | `monitor_workspace_list` | ❌ |
| 20 | 0.381209 | `storage_account_get` | ❌ |

---

## Test 240

**Expected Tool:** `extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `get_bestpractices_get` | ❌ |
| 3 | 0.488377 | `cloudarchitect_design` | ❌ |
| 4 | 0.476164 | `applicationinsights_recommendation_list` | ❌ |
| 5 | 0.473411 | `deploy_iac_rules_get` | ❌ |
| 6 | 0.462743 | `extension_azqr` | ✅ **EXPECTED** |
| 7 | 0.452244 | `applens_resource_diagnose` | ❌ |
| 8 | 0.448085 | `deploy_plan_get` | ❌ |
| 9 | 0.442021 | `quota_usage_check` | ❌ |
| 10 | 0.439040 | `deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.426161 | `deploy_pipeline_guidance_get` | ❌ |
| 12 | 0.385771 | `quota_region_availability_list` | ❌ |
| 13 | 0.382677 | `search_service_list` | ❌ |
| 14 | 0.375770 | `subscription_list` | ❌ |
| 15 | 0.375071 | `marketplace_product_get` | ❌ |
| 16 | 0.365824 | `resourcehealth_availability-status_list` | ❌ |
| 17 | 0.365699 | `resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.360510 | `resourcehealth_availability-status_get` | ❌ |
| 19 | 0.349469 | `storage_account_get` | ❌ |
| 20 | 0.341827 | `mysql_server_config_get` | ❌ |

---

## Test 241

**Expected Tool:** `extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.514978 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.504673 | `quota_usage_check` | ❌ |
| 5 | 0.494872 | `deploy_plan_get` | ❌ |
| 6 | 0.487387 | `get_bestpractices_get` | ❌ |
| 7 | 0.481713 | `applens_resource_diagnose` | ❌ |
| 8 | 0.464304 | `cloudarchitect_design` | ❌ |
| 9 | 0.463587 | `deploy_iac_rules_get` | ❌ |
| 10 | 0.463172 | `deploy_architecture_diagram_generate` | ❌ |
| 11 | 0.452811 | `search_service_list` | ❌ |
| 12 | 0.433938 | `quota_region_availability_list` | ❌ |
| 13 | 0.423512 | `subscription_list` | ❌ |
| 14 | 0.417356 | `resourcehealth_availability-status_list` | ❌ |
| 15 | 0.403533 | `deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.398621 | `monitor_workspace_list` | ❌ |
| 17 | 0.380268 | `storage_account_get` | ❌ |
| 18 | 0.377353 | `sql_server_list` | ❌ |
| 19 | 0.376533 | `marketplace_product_get` | ❌ |
| 20 | 0.376044 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 242

**Expected Tool:** `quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590878 | `quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413274 | `quota_usage_check` | ❌ |
| 3 | 0.372921 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369855 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.361386 | `datadog_monitoredresources_list` | ❌ |
| 6 | 0.349685 | `monitor_table_type_list` | ❌ |
| 7 | 0.348742 | `redis_cluster_list` | ❌ |
| 8 | 0.337835 | `redis_cache_list` | ❌ |
| 9 | 0.331145 | `mysql_server_list` | ❌ |
| 10 | 0.331097 | `monitor_metrics_definitions` | ❌ |
| 11 | 0.328408 | `grafana_list` | ❌ |
| 12 | 0.325796 | `azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.313178 | `loadtesting_testresource_list` | ❌ |
| 14 | 0.310655 | `resourcehealth_availability-status_get` | ❌ |
| 15 | 0.307143 | `workbooks_list` | ❌ |
| 16 | 0.297286 | `foundry_agents_list` | ❌ |
| 17 | 0.292791 | `eventgrid_subscription_list` | ❌ |
| 18 | 0.290187 | `group_list` | ❌ |
| 19 | 0.287074 | `acr_registry_list` | ❌ |
| 20 | 0.263276 | `loadtesting_test_get` | ❌ |

---

## Test 243

**Expected Tool:** `quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609244 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383920 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.378998 | `redis_cache_list` | ❌ |
| 6 | 0.365684 | `redis_cluster_list` | ❌ |
| 7 | 0.358215 | `azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.351637 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.345161 | `eventgrid_subscription_list` | ❌ |
| 10 | 0.345156 | `mysql_server_list` | ❌ |
| 11 | 0.342400 | `applens_resource_diagnose` | ❌ |
| 12 | 0.342231 | `datadog_monitoredresources_list` | ❌ |
| 13 | 0.338636 | `grafana_list` | ❌ |
| 14 | 0.331263 | `monitor_metrics_definitions` | ❌ |
| 15 | 0.322571 | `workbooks_list` | ❌ |
| 16 | 0.321805 | `monitor_resource_log_query` | ❌ |
| 17 | 0.305083 | `loadtesting_test_get` | ❌ |
| 18 | 0.304595 | `loadtesting_testrun_get` | ❌ |
| 19 | 0.300710 | `aks_cluster_get` | ❌ |
| 20 | 0.297650 | `applicationinsights_recommendation_list` | ❌ |

---

## Test 244

**Expected Tool:** `role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.484094 | `group_list` | ❌ |
| 3 | 0.483125 | `subscription_list` | ❌ |
| 4 | 0.478700 | `grafana_list` | ❌ |
| 5 | 0.474775 | `redis_cache_list` | ❌ |
| 6 | 0.471364 | `cosmos_account_list` | ❌ |
| 7 | 0.468596 | `search_service_list` | ❌ |
| 8 | 0.460029 | `redis_cluster_list` | ❌ |
| 9 | 0.452819 | `monitor_workspace_list` | ❌ |
| 10 | 0.446372 | `redis_cache_accesspolicy_list` | ❌ |
| 11 | 0.430667 | `kusto_cluster_list` | ❌ |
| 12 | 0.427666 | `workbooks_list` | ❌ |
| 13 | 0.426629 | `resourcehealth_availability-status_list` | ❌ |
| 14 | 0.425029 | `postgres_server_list` | ❌ |
| 15 | 0.421599 | `eventgrid_subscription_list` | ❌ |
| 16 | 0.409648 | `foundry_agents_list` | ❌ |
| 17 | 0.403310 | `datadog_monitoredresources_list` | ❌ |
| 18 | 0.398447 | `eventgrid_topic_list` | ❌ |
| 19 | 0.397565 | `appconfig_account_list` | ❌ |
| 20 | 0.397003 | `aks_cluster_list` | ❌ |

---

## Test 245

**Expected Tool:** `role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609705 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.456956 | `grafana_list` | ❌ |
| 3 | 0.436747 | `subscription_list` | ❌ |
| 4 | 0.435629 | `redis_cache_list` | ❌ |
| 5 | 0.435155 | `monitor_workspace_list` | ❌ |
| 6 | 0.431865 | `search_service_list` | ❌ |
| 7 | 0.428756 | `group_list` | ❌ |
| 8 | 0.428370 | `redis_cluster_list` | ❌ |
| 9 | 0.421637 | `resourcehealth_availability-status_list` | ❌ |
| 10 | 0.420804 | `cosmos_account_list` | ❌ |
| 11 | 0.415941 | `eventgrid_subscription_list` | ❌ |
| 12 | 0.410380 | `redis_cache_accesspolicy_list` | ❌ |
| 13 | 0.406766 | `quota_region_availability_list` | ❌ |
| 14 | 0.395445 | `workbooks_list` | ❌ |
| 15 | 0.390162 | `foundry_agents_list` | ❌ |
| 16 | 0.386800 | `kusto_cluster_list` | ❌ |
| 17 | 0.383635 | `datadog_monitoredresources_list` | ❌ |
| 18 | 0.373204 | `appconfig_account_list` | ❌ |
| 19 | 0.368493 | `loadtesting_testresource_list` | ❌ |
| 20 | 0.363678 | `eventgrid_topic_list` | ❌ |

---

## Test 246

**Expected Tool:** `redis_cache_accesspolicy_list`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757057 | `redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.565115 | `redis_cache_list` | ❌ |
| 3 | 0.445073 | `redis_cluster_list` | ❌ |
| 4 | 0.377563 | `redis_cluster_database_list` | ❌ |
| 5 | 0.322930 | `mysql_database_list` | ❌ |
| 6 | 0.312428 | `cosmos_account_list` | ❌ |
| 7 | 0.303531 | `appconfig_kv_list` | ❌ |
| 8 | 0.300024 | `cosmos_database_list` | ❌ |
| 9 | 0.292315 | `foundry_agents_list` | ❌ |
| 10 | 0.286490 | `acr_registry_repository_list` | ❌ |
| 11 | 0.285062 | `search_service_list` | ❌ |
| 12 | 0.284898 | `appconfig_account_list` | ❌ |
| 13 | 0.284304 | `grafana_list` | ❌ |
| 14 | 0.283818 | `mysql_server_list` | ❌ |
| 15 | 0.281989 | `keyvault_secret_list` | ❌ |
| 16 | 0.280741 | `loadtesting_testrun_list` | ❌ |
| 17 | 0.279800 | `datadog_monitoredresources_list` | ❌ |
| 18 | 0.277696 | `subscription_list` | ❌ |
| 19 | 0.274897 | `role_assignment_list` | ❌ |
| 20 | 0.272918 | `storage_blob_container_get` | ❌ |

---

## Test 247

**Expected Tool:** `redis_cache_accesspolicy_list`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713839 | `redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.523218 | `redis_cache_list` | ❌ |
| 3 | 0.412377 | `redis_cluster_list` | ❌ |
| 4 | 0.338859 | `redis_cluster_database_list` | ❌ |
| 5 | 0.286321 | `appconfig_kv_list` | ❌ |
| 6 | 0.283725 | `mysql_database_list` | ❌ |
| 7 | 0.280245 | `appconfig_kv_show` | ❌ |
| 8 | 0.265313 | `storage_blob_container_get` | ❌ |
| 9 | 0.264484 | `mysql_server_list` | ❌ |
| 10 | 0.262084 | `storage_account_get` | ❌ |
| 11 | 0.258045 | `appconfig_account_list` | ❌ |
| 12 | 0.257957 | `quota_usage_check` | ❌ |
| 13 | 0.257447 | `mysql_server_config_get` | ❌ |
| 14 | 0.257151 | `cosmos_account_list` | ❌ |
| 15 | 0.249585 | `loadtesting_testrun_list` | ❌ |
| 16 | 0.246871 | `grafana_list` | ❌ |
| 17 | 0.246847 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.246678 | `eventgrid_subscription_list` | ❌ |
| 19 | 0.243208 | `foundry_agents_list` | ❌ |
| 20 | 0.240600 | `datadog_monitoredresources_list` | ❌ |

---

## Test 248

**Expected Tool:** `redis_cache_list`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.764115 | `redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.653924 | `redis_cluster_list` | ❌ |
| 3 | 0.501880 | `redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.495048 | `postgres_server_list` | ❌ |
| 5 | 0.472307 | `grafana_list` | ❌ |
| 6 | 0.466141 | `kusto_cluster_list` | ❌ |
| 7 | 0.464785 | `redis_cluster_database_list` | ❌ |
| 8 | 0.431968 | `cosmos_account_list` | ❌ |
| 9 | 0.431715 | `appconfig_account_list` | ❌ |
| 10 | 0.423145 | `subscription_list` | ❌ |
| 11 | 0.414865 | `search_service_list` | ❌ |
| 12 | 0.396295 | `monitor_workspace_list` | ❌ |
| 13 | 0.387797 | `eventgrid_subscription_list` | ❌ |
| 14 | 0.381343 | `kusto_database_list` | ❌ |
| 15 | 0.380531 | `aks_cluster_list` | ❌ |
| 16 | 0.373546 | `group_list` | ❌ |
| 17 | 0.368719 | `datadog_monitoredresources_list` | ❌ |
| 18 | 0.367794 | `mysql_database_list` | ❌ |
| 19 | 0.367496 | `eventgrid_topic_list` | ❌ |
| 20 | 0.364522 | `virtualdesktop_hostpool_list` | ❌ |

---

## Test 249

**Expected Tool:** `redis_cache_list`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537994 | `redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.450409 | `redis_cache_accesspolicy_list` | ❌ |
| 3 | 0.441096 | `redis_cluster_list` | ❌ |
| 4 | 0.401251 | `redis_cluster_database_list` | ❌ |
| 5 | 0.302289 | `mysql_database_list` | ❌ |
| 6 | 0.283596 | `postgres_database_list` | ❌ |
| 7 | 0.275942 | `mysql_server_list` | ❌ |
| 8 | 0.265872 | `appconfig_kv_list` | ❌ |
| 9 | 0.262055 | `postgres_server_list` | ❌ |
| 10 | 0.257553 | `appconfig_account_list` | ❌ |
| 11 | 0.252020 | `grafana_list` | ❌ |
| 12 | 0.246438 | `cosmos_database_list` | ❌ |
| 13 | 0.236069 | `postgres_table_list` | ❌ |
| 14 | 0.233732 | `cosmos_account_list` | ❌ |
| 15 | 0.231227 | `quota_usage_check` | ❌ |
| 16 | 0.225057 | `cosmos_database_container_list` | ❌ |
| 17 | 0.224066 | `loadtesting_testrun_list` | ❌ |
| 18 | 0.217937 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.212340 | `azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.210079 | `kusto_cluster_list` | ❌ |

---

## Test 250

**Expected Tool:** `redis_cache_list`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692266 | `redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.595721 | `redis_cluster_list` | ❌ |
| 3 | 0.461603 | `redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.434924 | `postgres_server_list` | ❌ |
| 5 | 0.427325 | `grafana_list` | ❌ |
| 6 | 0.399303 | `redis_cluster_database_list` | ❌ |
| 7 | 0.383383 | `appconfig_account_list` | ❌ |
| 8 | 0.382294 | `kusto_cluster_list` | ❌ |
| 9 | 0.361735 | `cosmos_account_list` | ❌ |
| 10 | 0.358978 | `eventgrid_subscription_list` | ❌ |
| 11 | 0.353487 | `subscription_list` | ❌ |
| 12 | 0.353419 | `search_service_list` | ❌ |
| 13 | 0.340764 | `monitor_workspace_list` | ❌ |
| 14 | 0.327160 | `loadtesting_testresource_list` | ❌ |
| 15 | 0.315634 | `aks_cluster_list` | ❌ |
| 16 | 0.310802 | `datadog_monitoredresources_list` | ❌ |
| 17 | 0.308807 | `eventgrid_topic_list` | ❌ |
| 18 | 0.306367 | `acr_registry_list` | ❌ |
| 19 | 0.305932 | `mysql_database_list` | ❌ |
| 20 | 0.300248 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 251

**Expected Tool:** `redis_cluster_database_list`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752964 | `redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.603862 | `redis_cluster_list` | ❌ |
| 3 | 0.594977 | `kusto_database_list` | ❌ |
| 4 | 0.548166 | `postgres_database_list` | ❌ |
| 5 | 0.538309 | `cosmos_database_list` | ❌ |
| 6 | 0.520799 | `mysql_database_list` | ❌ |
| 7 | 0.471425 | `redis_cache_list` | ❌ |
| 8 | 0.458239 | `kusto_cluster_list` | ❌ |
| 9 | 0.456099 | `kusto_table_list` | ❌ |
| 10 | 0.449494 | `sql_db_list` | ❌ |
| 11 | 0.419526 | `postgres_table_list` | ❌ |
| 12 | 0.395373 | `mysql_server_list` | ❌ |
| 13 | 0.390361 | `mysql_table_list` | ❌ |
| 14 | 0.385479 | `cosmos_database_container_list` | ❌ |
| 15 | 0.379918 | `postgres_server_list` | ❌ |
| 16 | 0.376260 | `aks_cluster_list` | ❌ |
| 17 | 0.366166 | `cosmos_account_list` | ❌ |
| 18 | 0.328430 | `aks_nodepool_list` | ❌ |
| 19 | 0.328052 | `datadog_monitoredresources_list` | ❌ |
| 20 | 0.324876 | `grafana_list` | ❌ |

---

## Test 252

**Expected Tool:** `redis_cluster_database_list`  
**Prompt:** Show me the databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.721506 | `redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.562860 | `redis_cluster_list` | ❌ |
| 3 | 0.537788 | `kusto_database_list` | ❌ |
| 4 | 0.490987 | `mysql_database_list` | ❌ |
| 5 | 0.481618 | `cosmos_database_list` | ❌ |
| 6 | 0.480274 | `postgres_database_list` | ❌ |
| 7 | 0.434991 | `redis_cache_list` | ❌ |
| 8 | 0.414701 | `kusto_table_list` | ❌ |
| 9 | 0.408379 | `sql_db_list` | ❌ |
| 10 | 0.397285 | `kusto_cluster_list` | ❌ |
| 11 | 0.369086 | `mysql_server_list` | ❌ |
| 12 | 0.353712 | `mysql_table_list` | ❌ |
| 13 | 0.351025 | `cosmos_database_container_list` | ❌ |
| 14 | 0.349880 | `postgres_table_list` | ❌ |
| 15 | 0.343275 | `redis_cache_accesspolicy_list` | ❌ |
| 16 | 0.325405 | `aks_cluster_list` | ❌ |
| 17 | 0.318982 | `cosmos_account_list` | ❌ |
| 18 | 0.302228 | `kusto_sample` | ❌ |
| 19 | 0.294393 | `kusto_table_schema` | ❌ |
| 20 | 0.292180 | `grafana_list` | ❌ |

---

## Test 253

**Expected Tool:** `redis_cluster_list`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812960 | `redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.679028 | `kusto_cluster_list` | ❌ |
| 3 | 0.672139 | `redis_cache_list` | ❌ |
| 4 | 0.588847 | `redis_cluster_database_list` | ❌ |
| 5 | 0.569296 | `aks_cluster_list` | ❌ |
| 6 | 0.554298 | `postgres_server_list` | ❌ |
| 7 | 0.527406 | `kusto_database_list` | ❌ |
| 8 | 0.503279 | `grafana_list` | ❌ |
| 9 | 0.467957 | `cosmos_account_list` | ❌ |
| 10 | 0.462558 | `search_service_list` | ❌ |
| 11 | 0.457600 | `kusto_cluster_get` | ❌ |
| 12 | 0.455613 | `monitor_workspace_list` | ❌ |
| 13 | 0.445628 | `group_list` | ❌ |
| 14 | 0.445406 | `appconfig_account_list` | ❌ |
| 15 | 0.443534 | `virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.442886 | `redis_cache_accesspolicy_list` | ❌ |
| 17 | 0.436494 | `subscription_list` | ❌ |
| 18 | 0.435221 | `eventgrid_subscription_list` | ❌ |
| 19 | 0.419126 | `acr_registry_list` | ❌ |
| 20 | 0.411121 | `mysql_server_list` | ❌ |

---

## Test 254

**Expected Tool:** `redis_cluster_list`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591593 | `redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.514375 | `redis_cluster_database_list` | ❌ |
| 3 | 0.467592 | `redis_cache_list` | ❌ |
| 4 | 0.403281 | `kusto_cluster_list` | ❌ |
| 5 | 0.385069 | `redis_cache_accesspolicy_list` | ❌ |
| 6 | 0.368025 | `aks_cluster_list` | ❌ |
| 7 | 0.337910 | `mysql_server_list` | ❌ |
| 8 | 0.329389 | `postgres_server_list` | ❌ |
| 9 | 0.322157 | `kusto_database_list` | ❌ |
| 10 | 0.321180 | `mysql_database_list` | ❌ |
| 11 | 0.305874 | `kusto_cluster_get` | ❌ |
| 12 | 0.301294 | `aks_cluster_get` | ❌ |
| 13 | 0.295045 | `grafana_list` | ❌ |
| 14 | 0.291684 | `postgres_database_list` | ❌ |
| 15 | 0.288103 | `aks_nodepool_list` | ❌ |
| 16 | 0.272504 | `cosmos_database_list` | ❌ |
| 17 | 0.261138 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.260993 | `appconfig_account_list` | ❌ |
| 19 | 0.259662 | `postgres_server_config_get` | ❌ |
| 20 | 0.252050 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 255

**Expected Tool:** `redis_cluster_list`  
**Prompt:** Show me the Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.744239 | `redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.607561 | `redis_cache_list` | ❌ |
| 3 | 0.580866 | `kusto_cluster_list` | ❌ |
| 4 | 0.518857 | `redis_cluster_database_list` | ❌ |
| 5 | 0.494170 | `postgres_server_list` | ❌ |
| 6 | 0.491315 | `aks_cluster_list` | ❌ |
| 7 | 0.456252 | `grafana_list` | ❌ |
| 8 | 0.446568 | `kusto_cluster_get` | ❌ |
| 9 | 0.440660 | `kusto_database_list` | ❌ |
| 10 | 0.412876 | `eventgrid_subscription_list` | ❌ |
| 11 | 0.400256 | `redis_cache_accesspolicy_list` | ❌ |
| 12 | 0.398399 | `search_service_list` | ❌ |
| 13 | 0.394530 | `cosmos_account_list` | ❌ |
| 14 | 0.394483 | `monitor_workspace_list` | ❌ |
| 15 | 0.389814 | `appconfig_account_list` | ❌ |
| 16 | 0.372357 | `group_list` | ❌ |
| 17 | 0.370370 | `mysql_server_list` | ❌ |
| 18 | 0.369831 | `virtualdesktop_hostpool_list` | ❌ |
| 19 | 0.368926 | `datadog_monitoredresources_list` | ❌ |
| 20 | 0.367955 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 256

**Expected Tool:** `group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755412 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.566460 | `workbooks_list` | ❌ |
| 3 | 0.552633 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.546182 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.545480 | `redis_cluster_list` | ❌ |
| 6 | 0.542878 | `grafana_list` | ❌ |
| 7 | 0.530516 | `redis_cache_list` | ❌ |
| 8 | 0.524796 | `kusto_cluster_list` | ❌ |
| 9 | 0.519242 | `sql_server_list` | ❌ |
| 10 | 0.518475 | `acr_registry_list` | ❌ |
| 11 | 0.517031 | `loadtesting_testresource_list` | ❌ |
| 12 | 0.509454 | `search_service_list` | ❌ |
| 13 | 0.500858 | `monitor_workspace_list` | ❌ |
| 14 | 0.491176 | `acr_registry_repository_list` | ❌ |
| 15 | 0.490734 | `virtualdesktop_hostpool_list` | ❌ |
| 16 | 0.486716 | `cosmos_account_list` | ❌ |
| 17 | 0.483567 | `eventgrid_subscription_list` | ❌ |
| 18 | 0.479545 | `subscription_list` | ❌ |
| 19 | 0.477800 | `mysql_server_list` | ❌ |
| 20 | 0.477131 | `aks_cluster_list` | ❌ |

---

## Test 257

**Expected Tool:** `group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529480 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.463685 | `datadog_monitoredresources_list` | ❌ |
| 3 | 0.462391 | `mysql_server_list` | ❌ |
| 4 | 0.459304 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.453960 | `workbooks_list` | ❌ |
| 6 | 0.428894 | `loadtesting_testresource_list` | ❌ |
| 7 | 0.426935 | `redis_cluster_list` | ❌ |
| 8 | 0.407817 | `grafana_list` | ❌ |
| 9 | 0.398432 | `sql_server_list` | ❌ |
| 10 | 0.396822 | `azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.391250 | `redis_cache_list` | ❌ |
| 12 | 0.382985 | `acr_registry_list` | ❌ |
| 13 | 0.379927 | `acr_registry_repository_list` | ❌ |
| 14 | 0.375998 | `eventgrid_subscription_list` | ❌ |
| 15 | 0.373796 | `quota_region_availability_list` | ❌ |
| 16 | 0.366273 | `sql_db_list` | ❌ |
| 17 | 0.351405 | `virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.350999 | `quota_usage_check` | ❌ |
| 19 | 0.340946 | `foundry_agents_list` | ❌ |
| 20 | 0.328913 | `loadtesting_testresource_create` | ❌ |

---

## Test 258

**Expected Tool:** `group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665784 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `datadog_monitoredresources_list` | ❌ |
| 3 | 0.531964 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.523088 | `redis_cluster_list` | ❌ |
| 5 | 0.522911 | `workbooks_list` | ❌ |
| 6 | 0.518498 | `loadtesting_testresource_list` | ❌ |
| 7 | 0.515905 | `grafana_list` | ❌ |
| 8 | 0.494579 | `eventgrid_subscription_list` | ❌ |
| 9 | 0.492895 | `redis_cache_list` | ❌ |
| 10 | 0.489079 | `sql_server_list` | ❌ |
| 11 | 0.487740 | `acr_registry_list` | ❌ |
| 12 | 0.475708 | `search_service_list` | ❌ |
| 13 | 0.470658 | `kusto_cluster_list` | ❌ |
| 14 | 0.464637 | `quota_region_availability_list` | ❌ |
| 15 | 0.460412 | `monitor_workspace_list` | ❌ |
| 16 | 0.454711 | `mysql_server_list` | ❌ |
| 17 | 0.454439 | `virtualdesktop_hostpool_list` | ❌ |
| 18 | 0.437460 | `aks_cluster_list` | ❌ |
| 19 | 0.432994 | `cosmos_account_list` | ❌ |
| 20 | 0.429798 | `azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 259

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630680 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377586 | `quota_usage_check` | ❌ |
| 4 | 0.349980 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.331474 | `monitor_metrics_definitions` | ❌ |
| 6 | 0.330187 | `mysql_server_list` | ❌ |
| 7 | 0.327660 | `redis_cache_list` | ❌ |
| 8 | 0.325794 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.324331 | `quota_region_availability_list` | ❌ |
| 10 | 0.322117 | `azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.311602 | `monitor_metrics_query` | ❌ |
| 12 | 0.308238 | `redis_cluster_list` | ❌ |
| 13 | 0.306616 | `grafana_list` | ❌ |
| 14 | 0.292084 | `aks_nodepool_get` | ❌ |
| 15 | 0.290772 | `workbooks_show` | ❌ |
| 16 | 0.286287 | `loadtesting_test_get` | ❌ |
| 17 | 0.285047 | `applens_resource_diagnose` | ❌ |
| 18 | 0.284986 | `functionapp_get` | ❌ |
| 19 | 0.272387 | `aks_cluster_get` | ❌ |
| 20 | 0.272288 | `group_list` | ❌ |

---

## Test 260

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549306 | `storage_account_get` | ❌ |
| 2 | 0.510467 | `storage_blob_container_get` | ❌ |
| 3 | 0.490071 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 4 | 0.466885 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.455902 | `storage_account_create` | ❌ |
| 6 | 0.413034 | `storage_blob_get` | ❌ |
| 7 | 0.411283 | `quota_usage_check` | ❌ |
| 8 | 0.405847 | `cosmos_account_list` | ❌ |
| 9 | 0.403899 | `azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.375351 | `cosmos_database_container_list` | ❌ |
| 11 | 0.368262 | `appconfig_kv_show` | ❌ |
| 12 | 0.349407 | `cosmos_database_list` | ❌ |
| 13 | 0.348006 | `resourcehealth_service-health-events_list` | ❌ |
| 14 | 0.347171 | `monitor_resource_log_query` | ❌ |
| 15 | 0.346145 | `storage_blob_container_create` | ❌ |
| 16 | 0.336357 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.321694 | `deploy_app_logs_get` | ❌ |
| 18 | 0.318472 | `aks_nodepool_get` | ❌ |
| 19 | 0.311399 | `appconfig_account_list` | ❌ |
| 20 | 0.300500 | `aks_cluster_get` | ❌ |

---

## Test 261

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.570931 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424939 | `mysql_server_list` | ❌ |
| 4 | 0.393479 | `azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.386598 | `quota_usage_check` | ❌ |
| 6 | 0.373883 | `datadog_monitoredresources_list` | ❌ |
| 7 | 0.355414 | `functionapp_get` | ❌ |
| 8 | 0.352447 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.342229 | `virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.338012 | `sql_server_list` | ❌ |
| 11 | 0.337593 | `aks_nodepool_get` | ❌ |
| 12 | 0.335759 | `foundry_agents_list` | ❌ |
| 13 | 0.327197 | `storage_account_create` | ❌ |
| 14 | 0.321350 | `group_list` | ❌ |
| 15 | 0.318379 | `sql_db_list` | ❌ |
| 16 | 0.318319 | `workbooks_list` | ❌ |
| 17 | 0.316508 | `sql_server_show` | ❌ |
| 18 | 0.307309 | `applens_resource_diagnose` | ❌ |
| 19 | 0.294197 | `aks_cluster_get` | ❌ |
| 20 | 0.289102 | `loadtesting_testresource_list` | ❌ |

---

## Test 262

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.587331 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.578609 | `redis_cache_list` | ❌ |
| 4 | 0.563455 | `redis_cluster_list` | ❌ |
| 5 | 0.548549 | `grafana_list` | ❌ |
| 6 | 0.540583 | `datadog_monitoredresources_list` | ❌ |
| 7 | 0.534299 | `search_service_list` | ❌ |
| 8 | 0.531356 | `quota_region_availability_list` | ❌ |
| 9 | 0.531121 | `group_list` | ❌ |
| 10 | 0.507740 | `monitor_workspace_list` | ❌ |
| 11 | 0.496651 | `cosmos_account_list` | ❌ |
| 12 | 0.491394 | `quota_usage_check` | ❌ |
| 13 | 0.491359 | `subscription_list` | ❌ |
| 14 | 0.489514 | `eventgrid_subscription_list` | ❌ |
| 15 | 0.484188 | `loadtesting_testresource_list` | ❌ |
| 16 | 0.482623 | `kusto_cluster_list` | ❌ |
| 17 | 0.476832 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.465503 | `aks_cluster_list` | ❌ |
| 19 | 0.462565 | `eventgrid_topic_list` | ❌ |
| 20 | 0.459718 | `workbooks_list` | ❌ |

---

## Test 263

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.586936 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.508252 | `quota_usage_check` | ❌ |
| 4 | 0.473905 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.462125 | `search_service_list` | ❌ |
| 6 | 0.456449 | `foundry_agents_list` | ❌ |
| 7 | 0.441470 | `mysql_server_list` | ❌ |
| 8 | 0.441417 | `applens_resource_diagnose` | ❌ |
| 9 | 0.430687 | `resourcehealth_service-health-events_list` | ❌ |
| 10 | 0.418944 | `sql_server_show` | ❌ |
| 11 | 0.409377 | `deploy_app_logs_get` | ❌ |
| 12 | 0.406784 | `storage_blob_container_get` | ❌ |
| 13 | 0.406709 | `quota_region_availability_list` | ❌ |
| 14 | 0.406408 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.405790 | `sql_db_list` | ❌ |
| 16 | 0.403414 | `aks_cluster_list` | ❌ |
| 17 | 0.387835 | `cosmos_account_list` | ❌ |
| 18 | 0.381144 | `get_bestpractices_get` | ❌ |
| 19 | 0.379969 | `azureterraformbestpractices_get` | ❌ |
| 20 | 0.371766 | `loadtesting_testresource_list` | ❌ |

---

## Test 264

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.543368 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.427638 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.420682 | `applens_resource_diagnose` | ❌ |
| 5 | 0.420387 | `mysql_server_list` | ❌ |
| 6 | 0.411111 | `quota_usage_check` | ❌ |
| 7 | 0.410799 | `resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.374184 | `azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.370858 | `loadtesting_testresource_list` | ❌ |
| 10 | 0.363808 | `workbooks_list` | ❌ |
| 11 | 0.360039 | `redis_cluster_list` | ❌ |
| 12 | 0.358871 | `monitor_healthmodels_entity_gethealth` | ❌ |
| 13 | 0.354932 | `sql_server_list` | ❌ |
| 14 | 0.350520 | `group_list` | ❌ |
| 15 | 0.349212 | `monitor_metrics_query` | ❌ |
| 16 | 0.338595 | `eventgrid_subscription_list` | ❌ |
| 17 | 0.330185 | `extension_azqr` | ❌ |
| 18 | 0.328471 | `applicationinsights_recommendation_list` | ❌ |
| 19 | 0.324217 | `foundry_agents_list` | ❌ |
| 20 | 0.309389 | `deploy_app_logs_get` | ❌ |

---

## Test 265

**Expected Tool:** `resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.719763 | `resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `search_service_list` | ❌ |
| 3 | 0.531311 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.518372 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.503744 | `eventgrid_topic_list` | ❌ |
| 6 | 0.470139 | `postgres_server_list` | ❌ |
| 7 | 0.456562 | `redis_cache_list` | ❌ |
| 8 | 0.454448 | `redis_cluster_list` | ❌ |
| 9 | 0.446475 | `resourcehealth_availability-status_get` | ❌ |
| 10 | 0.438780 | `subscription_list` | ❌ |
| 11 | 0.427177 | `aks_cluster_list` | ❌ |
| 12 | 0.426698 | `grafana_list` | ❌ |
| 13 | 0.419828 | `monitor_workspace_list` | ❌ |
| 14 | 0.419011 | `kusto_cluster_list` | ❌ |
| 15 | 0.416883 | `cosmos_account_list` | ❌ |
| 16 | 0.412013 | `group_list` | ❌ |
| 17 | 0.407099 | `servicebus_topic_subscription_details` | ❌ |
| 18 | 0.385382 | `appconfig_account_list` | ❌ |
| 19 | 0.378841 | `datadog_monitoredresources_list` | ❌ |
| 20 | 0.377914 | `foundry_agents_list` | ❌ |

---

## Test 266

**Expected Tool:** `resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726700 | `resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.513815 | `search_service_list` | ❌ |
| 3 | 0.509196 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.491121 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.484366 | `resourcehealth_availability-status_get` | ❌ |
| 6 | 0.474835 | `eventgrid_topic_list` | ❌ |
| 7 | 0.459791 | `subscription_list` | ❌ |
| 8 | 0.431455 | `marketplace_product_get` | ❌ |
| 9 | 0.425644 | `quota_region_availability_list` | ❌ |
| 10 | 0.411892 | `servicebus_topic_subscription_details` | ❌ |
| 11 | 0.410579 | `marketplace_product_list` | ❌ |
| 12 | 0.409037 | `aks_cluster_list` | ❌ |
| 13 | 0.404636 | `monitor_workspace_list` | ❌ |
| 14 | 0.390652 | `kusto_cluster_get` | ❌ |
| 15 | 0.390558 | `group_list` | ❌ |
| 16 | 0.390381 | `applens_resource_diagnose` | ❌ |
| 17 | 0.389256 | `keyvault_key_get` | ❌ |
| 18 | 0.385710 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.384857 | `applicationinsights_recommendation_list` | ❌ |
| 20 | 0.384613 | `kusto_cluster_list` | ❌ |

---

## Test 267

**Expected Tool:** `resourcehealth_service-health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.302331 | `resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.270225 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.251821 | `applens_resource_diagnose` | ❌ |
| 4 | 0.216847 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211842 | `search_service_list` | ❌ |
| 6 | 0.191890 | `cloudarchitect_design` | ❌ |
| 7 | 0.189628 | `foundry_models_deployments_list` | ❌ |
| 8 | 0.188665 | `get_bestpractices_get` | ❌ |
| 9 | 0.187819 | `azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.185941 | `quota_usage_check` | ❌ |
| 11 | 0.174877 | `deploy_app_logs_get` | ❌ |
| 12 | 0.170157 | `postgres_server_list` | ❌ |
| 13 | 0.169947 | `servicebus_queue_details` | ❌ |
| 14 | 0.164622 | `monitor_resource_log_query` | ❌ |
| 15 | 0.164285 | `eventgrid_subscription_list` | ❌ |
| 16 | 0.163022 | `monitor_workspace_log_query` | ❌ |
| 17 | 0.155791 | `servicebus_topic_subscription_details` | ❌ |
| 18 | 0.155483 | `aks_cluster_list` | ❌ |
| 19 | 0.151666 | `foundry_agents_list` | ❌ |
| 20 | 0.149112 | `aks_cluster_get` | ❌ |

---

## Test 268

**Expected Tool:** `resourcehealth_service-health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710990 | `resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.545714 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.520197 | `search_service_list` | ❌ |
| 4 | 0.502064 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.487327 | `eventgrid_topic_list` | ❌ |
| 6 | 0.453339 | `resourcehealth_availability-status_get` | ❌ |
| 7 | 0.451351 | `postgres_server_list` | ❌ |
| 8 | 0.439701 | `redis_cache_list` | ❌ |
| 9 | 0.436070 | `redis_cluster_list` | ❌ |
| 10 | 0.411793 | `grafana_list` | ❌ |
| 11 | 0.408792 | `servicebus_topic_subscription_details` | ❌ |
| 12 | 0.407707 | `subscription_list` | ❌ |
| 13 | 0.406949 | `monitor_workspace_list` | ❌ |
| 14 | 0.405031 | `aks_cluster_list` | ❌ |
| 15 | 0.391992 | `kusto_cluster_list` | ❌ |
| 16 | 0.379016 | `cosmos_account_list` | ❌ |
| 17 | 0.371380 | `group_list` | ❌ |
| 18 | 0.368866 | `datadog_monitoredresources_list` | ❌ |
| 19 | 0.358754 | `foundry_agents_list` | ❌ |
| 20 | 0.357139 | `appconfig_account_list` | ❌ |

---

## Test 269

**Expected Tool:** `resourcehealth_service-health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528170 | `resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.437868 | `search_service_list` | ❌ |
| 3 | 0.402493 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.402232 | `foundry_agents_list` | ❌ |
| 5 | 0.400114 | `resourcehealth_availability-status_get` | ❌ |
| 6 | 0.397735 | `quota_usage_check` | ❌ |
| 7 | 0.382901 | `deploy_plan_get` | ❌ |
| 8 | 0.382597 | `deploy_app_logs_get` | ❌ |
| 9 | 0.375034 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.372844 | `eventgrid_subscription_list` | ❌ |
| 11 | 0.372005 | `monitor_metrics_query` | ❌ |
| 12 | 0.363470 | `get_bestpractices_get` | ❌ |
| 13 | 0.362191 | `applens_resource_diagnose` | ❌ |
| 14 | 0.360562 | `deploy_architecture_diagram_generate` | ❌ |
| 15 | 0.357531 | `azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.341495 | `foundry_models_deployments_list` | ❌ |
| 17 | 0.338062 | `search_index_get` | ❌ |
| 18 | 0.335471 | `marketplace_product_get` | ❌ |
| 19 | 0.333382 | `sql_server_show` | ❌ |
| 20 | 0.333201 | `subscription_list` | ❌ |

---

## Test 270

**Expected Tool:** `servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.400645 | `servicebus_topic_details` | ❌ |
| 4 | 0.375386 | `aks_cluster_get` | ❌ |
| 5 | 0.359891 | `storage_blob_container_get` | ❌ |
| 6 | 0.352705 | `storage_account_get` | ❌ |
| 7 | 0.352342 | `storage_blob_get` | ❌ |
| 8 | 0.351081 | `search_index_get` | ❌ |
| 9 | 0.344531 | `eventgrid_subscription_list` | ❌ |
| 10 | 0.342349 | `sql_server_show` | ❌ |
| 11 | 0.337239 | `sql_db_show` | ❌ |
| 12 | 0.333043 | `keyvault_key_get` | ❌ |
| 13 | 0.332560 | `loadtesting_testrun_get` | ❌ |
| 14 | 0.331418 | `keyvault_secret_get` | ❌ |
| 15 | 0.327611 | `aks_nodepool_get` | ❌ |
| 16 | 0.323281 | `marketplace_product_get` | ❌ |
| 17 | 0.323046 | `kusto_cluster_get` | ❌ |
| 18 | 0.310612 | `azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.309214 | `functionapp_get` | ❌ |
| 20 | 0.296299 | `aks_cluster_list` | ❌ |

---

## Test 271

**Expected Tool:** `servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591324 | `servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571861 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.497732 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.494885 | `eventgrid_topic_list` | ❌ |
| 5 | 0.483976 | `servicebus_queue_details` | ❌ |
| 6 | 0.365658 | `search_index_get` | ❌ |
| 7 | 0.361354 | `aks_cluster_get` | ❌ |
| 8 | 0.352485 | `marketplace_product_get` | ❌ |
| 9 | 0.341304 | `loadtesting_testrun_get` | ❌ |
| 10 | 0.340036 | `sql_db_show` | ❌ |
| 11 | 0.336913 | `storage_blob_get` | ❌ |
| 12 | 0.335558 | `kusto_cluster_get` | ❌ |
| 13 | 0.333396 | `storage_account_get` | ❌ |
| 14 | 0.331645 | `resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.326590 | `keyvault_secret_get` | ❌ |
| 16 | 0.325007 | `storage_blob_container_get` | ❌ |
| 17 | 0.317423 | `aks_cluster_list` | ❌ |
| 18 | 0.314540 | `keyvault_key_get` | ❌ |
| 19 | 0.306388 | `functionapp_get` | ❌ |
| 20 | 0.297323 | `grafana_list` | ❌ |

---

## Test 272

**Expected Tool:** `servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.523134 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.494515 | `servicebus_queue_details` | ❌ |
| 4 | 0.456939 | `servicebus_topic_details` | ❌ |
| 5 | 0.444604 | `marketplace_product_get` | ❌ |
| 6 | 0.443973 | `eventgrid_topic_list` | ❌ |
| 7 | 0.429444 | `redis_cache_list` | ❌ |
| 8 | 0.426573 | `kusto_cluster_get` | ❌ |
| 9 | 0.421009 | `sql_db_show` | ❌ |
| 10 | 0.411495 | `resourcehealth_service-health-events_list` | ❌ |
| 11 | 0.409619 | `aks_cluster_list` | ❌ |
| 12 | 0.405380 | `search_service_list` | ❌ |
| 13 | 0.404739 | `redis_cluster_list` | ❌ |
| 14 | 0.398648 | `keyvault_secret_get` | ❌ |
| 15 | 0.395789 | `storage_account_get` | ❌ |
| 16 | 0.395176 | `grafana_list` | ❌ |
| 17 | 0.390372 | `keyvault_key_get` | ❌ |
| 18 | 0.382225 | `functionapp_get` | ❌ |
| 19 | 0.369986 | `appconfig_account_list` | ❌ |
| 20 | 0.368411 | `aks_cluster_get` | ❌ |

---

## Test 273

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470950 | `sql_server_create` | ❌ |
| 3 | 0.376833 | `sql_server_delete` | ❌ |
| 4 | 0.371359 | `appservice_database_add` | ❌ |
| 5 | 0.359945 | `sql_db_show` | ❌ |
| 6 | 0.357421 | `sql_db_list` | ❌ |
| 7 | 0.355614 | `postgres_database_list` | ❌ |
| 8 | 0.347128 | `mysql_database_list` | ❌ |
| 9 | 0.346831 | `sql_server_show` | ❌ |
| 10 | 0.329384 | `sql_server_firewall-rule_create` | ❌ |
| 11 | 0.327837 | `sql_db_delete` | ❌ |
| 12 | 0.301243 | `cosmos_database_list` | ❌ |
| 13 | 0.279490 | `kusto_table_list` | ❌ |
| 14 | 0.276192 | `kusto_database_list` | ❌ |
| 15 | 0.254831 | `cosmos_database_container_item_query` | ❌ |
| 16 | 0.238999 | `cosmos_database_container_list` | ❌ |
| 17 | 0.236839 | `keyvault_key_create` | ❌ |
| 18 | 0.234649 | `keyvault_secret_create` | ❌ |
| 19 | 0.231273 | `kusto_table_schema` | ❌ |
| 20 | 0.210486 | `keyvault_certificate_create` | ❌ |

---

## Test 274

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459711 | `sql_server_create` | ❌ |
| 3 | 0.424075 | `appservice_database_add` | ❌ |
| 4 | 0.420843 | `sql_db_show` | ❌ |
| 5 | 0.396455 | `sql_db_update` | ❌ |
| 6 | 0.395495 | `sql_server_delete` | ❌ |
| 7 | 0.384660 | `sql_db_list` | ❌ |
| 8 | 0.371559 | `sql_server_show` | ❌ |
| 9 | 0.361051 | `mysql_database_list` | ❌ |
| 10 | 0.343099 | `sql_db_delete` | ❌ |
| 11 | 0.326225 | `sql_server_firewall-rule_create` | ❌ |
| 12 | 0.324123 | `kusto_table_list` | ❌ |
| 13 | 0.321837 | `cosmos_database_list` | ❌ |
| 14 | 0.317180 | `kusto_database_list` | ❌ |
| 15 | 0.301487 | `kusto_table_schema` | ❌ |
| 16 | 0.286796 | `cosmos_database_container_item_query` | ❌ |
| 17 | 0.280312 | `keyvault_key_create` | ❌ |
| 18 | 0.277149 | `keyvault_secret_create` | ❌ |
| 19 | 0.276652 | `loadtesting_test_create` | ❌ |
| 20 | 0.257394 | `cosmos_database_container_list` | ❌ |

---

## Test 275

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545932 | `sql_server_create` | ❌ |
| 3 | 0.494377 | `sql_db_show` | ❌ |
| 4 | 0.473975 | `sql_db_list` | ❌ |
| 5 | 0.456262 | `storage_account_create` | ❌ |
| 6 | 0.455293 | `sql_server_delete` | ❌ |
| 7 | 0.441044 | `appservice_database_add` | ❌ |
| 8 | 0.431068 | `sql_server_list` | ❌ |
| 9 | 0.422871 | `workbooks_create` | ❌ |
| 10 | 0.413309 | `mysql_server_list` | ❌ |
| 11 | 0.399254 | `loadtesting_testresource_create` | ❌ |
| 12 | 0.392814 | `cosmos_database_list` | ❌ |
| 13 | 0.374962 | `sql_elastic-pool_list` | ❌ |
| 14 | 0.365919 | `kusto_database_list` | ❌ |
| 15 | 0.358566 | `kusto_table_list` | ❌ |
| 16 | 0.323547 | `group_list` | ❌ |
| 17 | 0.322659 | `keyvault_key_create` | ❌ |
| 18 | 0.319901 | `keyvault_secret_create` | ❌ |
| 19 | 0.319381 | `cosmos_database_container_list` | ❌ |
| 20 | 0.315134 | `keyvault_certificate_create` | ❌ |

---

## Test 276

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520786 | `sql_server_delete` | ❌ |
| 2 | 0.484026 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.386564 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.364776 | `sql_db_show` | ❌ |
| 5 | 0.351204 | `postgres_database_list` | ❌ |
| 6 | 0.350121 | `mysql_database_list` | ❌ |
| 7 | 0.345061 | `sql_db_list` | ❌ |
| 8 | 0.338052 | `sql_server_show` | ❌ |
| 9 | 0.337699 | `sql_db_create` | ❌ |
| 10 | 0.317215 | `mysql_table_list` | ❌ |
| 11 | 0.300723 | `appservice_database_add` | ❌ |
| 12 | 0.286892 | `cosmos_database_list` | ❌ |
| 13 | 0.284794 | `kusto_table_list` | ❌ |
| 14 | 0.278895 | `kusto_database_list` | ❌ |
| 15 | 0.271009 | `appconfig_kv_delete` | ❌ |
| 16 | 0.253808 | `cosmos_database_container_item_query` | ❌ |
| 17 | 0.243201 | `kusto_table_schema` | ❌ |
| 18 | 0.235173 | `cosmos_database_container_list` | ❌ |
| 19 | 0.211680 | `kusto_query` | ❌ |
| 20 | 0.183587 | `kusto_sample` | ❌ |

---

## Test 277

**Expected Tool:** `sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579119 | `sql_server_delete` | ❌ |
| 2 | 0.500756 | `sql_db_show` | ❌ |
| 3 | 0.478729 | `sql_db_list` | ❌ |
| 4 | 0.466216 | `sql_db_delete` | ✅ **EXPECTED** |
| 5 | 0.437112 | `sql_server_list` | ❌ |
| 6 | 0.421365 | `sql_db_create` | ❌ |
| 7 | 0.412704 | `mysql_server_list` | ❌ |
| 8 | 0.392236 | `workbooks_delete` | ❌ |
| 9 | 0.390334 | `cosmos_database_list` | ❌ |
| 10 | 0.388400 | `appservice_database_add` | ❌ |
| 11 | 0.381400 | `sql_server_create` | ❌ |
| 12 | 0.380074 | `kusto_database_list` | ❌ |
| 13 | 0.370486 | `kusto_table_list` | ❌ |
| 14 | 0.368841 | `sql_elastic-pool_list` | ❌ |
| 15 | 0.345627 | `group_list` | ❌ |
| 16 | 0.334517 | `datadog_monitoredresources_list` | ❌ |
| 17 | 0.332517 | `acr_registry_repository_list` | ❌ |
| 18 | 0.327408 | `cosmos_database_container_list` | ❌ |
| 19 | 0.310437 | `kusto_table_schema` | ❌ |
| 20 | 0.304849 | `cosmos_database_container_item_query` | ❌ |

---

## Test 278

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.459422 | `sql_server_delete` | ❌ |
| 2 | 0.427494 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.364494 | `postgres_database_list` | ❌ |
| 4 | 0.355416 | `mysql_database_list` | ❌ |
| 5 | 0.319617 | `sql_db_show` | ❌ |
| 6 | 0.314902 | `cosmos_database_list` | ❌ |
| 7 | 0.311506 | `mysql_table_list` | ❌ |
| 8 | 0.310758 | `sql_db_list` | ❌ |
| 9 | 0.305059 | `sql_server_firewall-rule_delete` | ❌ |
| 10 | 0.295355 | `redis_cluster_database_list` | ❌ |
| 11 | 0.295030 | `appservice_database_add` | ❌ |
| 12 | 0.294781 | `sql_db_create` | ❌ |
| 13 | 0.288554 | `kusto_database_list` | ❌ |
| 14 | 0.283955 | `kusto_table_list` | ❌ |
| 15 | 0.258654 | `appconfig_kv_delete` | ❌ |
| 16 | 0.246948 | `cosmos_database_container_list` | ❌ |
| 17 | 0.229764 | `kusto_table_schema` | ❌ |
| 18 | 0.213266 | `cosmos_database_container_item_query` | ❌ |
| 19 | 0.187992 | `kusto_query` | ❌ |
| 20 | 0.171883 | `kusto_sample` | ❌ |

---

## Test 279

**Expected Tool:** `sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639694 | `mysql_database_list` | ❌ |
| 3 | 0.609178 | `postgres_database_list` | ❌ |
| 4 | 0.602890 | `cosmos_database_list` | ❌ |
| 5 | 0.532407 | `sql_server_show` | ❌ |
| 6 | 0.529058 | `mysql_table_list` | ❌ |
| 7 | 0.527896 | `kusto_database_list` | ❌ |
| 8 | 0.486638 | `mysql_server_list` | ❌ |
| 9 | 0.485960 | `sql_server_list` | ❌ |
| 10 | 0.476350 | `sql_server_delete` | ❌ |
| 11 | 0.475733 | `sql_elastic-pool_list` | ❌ |
| 12 | 0.474927 | `redis_cluster_database_list` | ❌ |
| 13 | 0.457314 | `kusto_table_list` | ❌ |
| 14 | 0.441355 | `cosmos_account_list` | ❌ |
| 15 | 0.440528 | `cosmos_database_container_list` | ❌ |
| 16 | 0.380402 | `acr_registry_repository_list` | ❌ |
| 17 | 0.373774 | `foundry_agents_list` | ❌ |
| 18 | 0.371962 | `appservice_database_add` | ❌ |
| 19 | 0.367404 | `datadog_monitoredresources_list` | ❌ |
| 20 | 0.365423 | `keyvault_certificate_list` | ❌ |

---

## Test 280

**Expected Tool:** `sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `sql_server_show` | ❌ |
| 2 | 0.609322 | `sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.557353 | `mysql_database_list` | ❌ |
| 4 | 0.553488 | `mysql_server_config_get` | ❌ |
| 5 | 0.524207 | `sql_db_show` | ❌ |
| 6 | 0.471862 | `postgres_database_list` | ❌ |
| 7 | 0.461650 | `cosmos_database_list` | ❌ |
| 8 | 0.458742 | `postgres_server_config_get` | ❌ |
| 9 | 0.451453 | `sql_db_create` | ❌ |
| 10 | 0.446512 | `sql_server_list` | ❌ |
| 11 | 0.445291 | `mysql_table_list` | ❌ |
| 12 | 0.387645 | `kusto_database_list` | ❌ |
| 13 | 0.385130 | `appservice_database_add` | ❌ |
| 14 | 0.380428 | `appconfig_account_list` | ❌ |
| 15 | 0.357282 | `aks_cluster_list` | ❌ |
| 16 | 0.354581 | `aks_nodepool_list` | ❌ |
| 17 | 0.349880 | `cosmos_account_list` | ❌ |
| 18 | 0.347075 | `cosmos_database_container_list` | ❌ |
| 19 | 0.342792 | `appconfig_kv_list` | ❌ |
| 20 | 0.342284 | `aks_cluster_get` | ❌ |

---

## Test 281

**Expected Tool:** `sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610991 | `sql_server_show` | ❌ |
| 2 | 0.593150 | `postgres_server_config_get` | ❌ |
| 3 | 0.530422 | `mysql_server_config_get` | ❌ |
| 4 | 0.528128 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465693 | `sql_db_list` | ❌ |
| 6 | 0.446728 | `postgres_server_param_get` | ❌ |
| 7 | 0.438925 | `mysql_server_param_get` | ❌ |
| 8 | 0.398181 | `mysql_table_schema_get` | ❌ |
| 9 | 0.397510 | `mysql_database_list` | ❌ |
| 10 | 0.396416 | `sql_db_create` | ❌ |
| 11 | 0.371413 | `loadtesting_test_get` | ❌ |
| 12 | 0.325945 | `kusto_table_schema` | ❌ |
| 13 | 0.325788 | `aks_nodepool_get` | ❌ |
| 14 | 0.320054 | `aks_cluster_get` | ❌ |
| 15 | 0.317622 | `appservice_database_add` | ❌ |
| 16 | 0.297839 | `appconfig_kv_show` | ❌ |
| 17 | 0.294987 | `appconfig_kv_list` | ❌ |
| 18 | 0.281546 | `functionapp_get` | ❌ |
| 19 | 0.279942 | `foundry_knowledge_index_schema` | ❌ |
| 20 | 0.273566 | `kusto_cluster_get` | ❌ |

---

## Test 282

**Expected Tool:** `sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530077 | `sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503681 | `sql_server_show` | ❌ |
| 3 | 0.440073 | `sql_db_list` | ❌ |
| 4 | 0.438622 | `mysql_table_schema_get` | ❌ |
| 5 | 0.432919 | `mysql_database_list` | ❌ |
| 6 | 0.421862 | `postgres_database_list` | ❌ |
| 7 | 0.400963 | `mysql_table_list` | ❌ |
| 8 | 0.398714 | `mysql_server_config_get` | ❌ |
| 9 | 0.375668 | `postgres_server_config_get` | ❌ |
| 10 | 0.361500 | `redis_cluster_database_list` | ❌ |
| 11 | 0.344694 | `kusto_table_schema` | ❌ |
| 12 | 0.337996 | `cosmos_database_list` | ❌ |
| 13 | 0.323587 | `kusto_table_list` | ❌ |
| 14 | 0.300133 | `cosmos_database_container_list` | ❌ |
| 15 | 0.299814 | `aks_cluster_get` | ❌ |
| 16 | 0.296827 | `kusto_database_list` | ❌ |
| 17 | 0.291633 | `loadtesting_testrun_get` | ❌ |
| 18 | 0.285843 | `kusto_cluster_get` | ❌ |
| 19 | 0.276453 | `keyvault_key_get` | ❌ |
| 20 | 0.274305 | `appservice_database_add` | ❌ |

---

## Test 283

**Expected Tool:** `sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565596 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `sql_db_create` | ❌ |
| 3 | 0.427621 | `sql_db_show` | ❌ |
| 4 | 0.385817 | `sql_server_show` | ❌ |
| 5 | 0.384245 | `appservice_database_add` | ❌ |
| 6 | 0.371461 | `sql_db_list` | ❌ |
| 7 | 0.369822 | `sql_server_delete` | ❌ |
| 8 | 0.368957 | `mysql_server_param_set` | ❌ |
| 9 | 0.344860 | `sql_db_delete` | ❌ |
| 10 | 0.334237 | `postgres_server_param_set` | ❌ |
| 11 | 0.316890 | `mysql_server_config_get` | ❌ |
| 12 | 0.273809 | `cosmos_database_list` | ❌ |
| 13 | 0.270134 | `kusto_table_schema` | ❌ |
| 14 | 0.263397 | `kusto_table_list` | ❌ |
| 15 | 0.250975 | `kusto_database_list` | ❌ |
| 16 | 0.250753 | `cosmos_database_container_item_query` | ❌ |
| 17 | 0.240663 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.230973 | `loadtesting_testrun_create` | ❌ |
| 19 | 0.223239 | `loadtesting_test_create` | ❌ |
| 20 | 0.223086 | `kusto_query` | ❌ |

---

## Test 284

**Expected Tool:** `sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.401817 | `sql_db_list` | ❌ |
| 2 | 0.394770 | `sql_db_show` | ❌ |
| 3 | 0.390219 | `sql_db_update` | ✅ **EXPECTED** |
| 4 | 0.386628 | `sql_server_delete` | ❌ |
| 5 | 0.381889 | `sql_db_create` | ❌ |
| 6 | 0.349256 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 7 | 0.342291 | `sql_elastic-pool_list` | ❌ |
| 8 | 0.339054 | `sql_db_delete` | ❌ |
| 9 | 0.333336 | `sql_server_show` | ❌ |
| 10 | 0.329770 | `mysql_table_list` | ❌ |
| 11 | 0.325658 | `mysql_server_config_get` | ❌ |
| 12 | 0.304373 | `kusto_table_schema` | ❌ |
| 13 | 0.301576 | `appservice_database_add` | ❌ |
| 14 | 0.296988 | `azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 15 | 0.261164 | `kusto_table_list` | ❌ |
| 16 | 0.257330 | `kusto_database_list` | ❌ |
| 17 | 0.238540 | `cosmos_database_container_item_query` | ❌ |
| 18 | 0.232099 | `cosmos_database_list` | ❌ |
| 19 | 0.221295 | `kusto_query` | ❌ |
| 20 | 0.219365 | `foundry_knowledge_index_schema` | ❌ |

---

## Test 285

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678124 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `sql_db_list` | ❌ |
| 3 | 0.498367 | `mysql_database_list` | ❌ |
| 4 | 0.479044 | `sql_server_show` | ❌ |
| 5 | 0.473539 | `aks_nodepool_list` | ❌ |
| 6 | 0.454426 | `mysql_table_list` | ❌ |
| 7 | 0.450777 | `mysql_server_list` | ❌ |
| 8 | 0.442892 | `sql_server_list` | ❌ |
| 9 | 0.441264 | `virtualdesktop_hostpool_list` | ❌ |
| 10 | 0.434570 | `postgres_server_list` | ❌ |
| 11 | 0.431174 | `cosmos_database_list` | ❌ |
| 12 | 0.429039 | `sql_server_entra-admin_list` | ❌ |
| 13 | 0.394548 | `aks_nodepool_get` | ❌ |
| 14 | 0.394337 | `kusto_database_list` | ❌ |
| 15 | 0.370652 | `cosmos_account_list` | ❌ |
| 16 | 0.363579 | `kusto_cluster_list` | ❌ |
| 17 | 0.357347 | `kusto_table_list` | ❌ |
| 18 | 0.352036 | `aks_cluster_list` | ❌ |
| 19 | 0.351647 | `cosmos_database_container_list` | ❌ |
| 20 | 0.351045 | `foundry_agents_list` | ❌ |

---

## Test 286

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606425 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `sql_server_show` | ❌ |
| 3 | 0.457163 | `sql_db_list` | ❌ |
| 4 | 0.438522 | `aks_nodepool_list` | ❌ |
| 5 | 0.432816 | `mysql_database_list` | ❌ |
| 6 | 0.429793 | `aks_nodepool_get` | ❌ |
| 7 | 0.423047 | `mysql_server_config_get` | ❌ |
| 8 | 0.419753 | `mysql_server_list` | ❌ |
| 9 | 0.408202 | `sql_server_list` | ❌ |
| 10 | 0.400026 | `mysql_server_param_get` | ❌ |
| 11 | 0.383946 | `sql_server_entra-admin_list` | ❌ |
| 12 | 0.378556 | `postgres_server_list` | ❌ |
| 13 | 0.341694 | `foundry_agents_list` | ❌ |
| 14 | 0.335615 | `cosmos_database_list` | ❌ |
| 15 | 0.333099 | `azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.319788 | `aks_cluster_list` | ❌ |
| 17 | 0.317886 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.312419 | `appservice_database_add` | ❌ |
| 19 | 0.304600 | `cosmos_account_list` | ❌ |
| 20 | 0.304317 | `appconfig_account_list` | ❌ |

---

## Test 287

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592709 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420325 | `mysql_database_list` | ❌ |
| 3 | 0.402616 | `mysql_server_list` | ❌ |
| 4 | 0.397670 | `sql_db_list` | ❌ |
| 5 | 0.397640 | `sql_server_show` | ❌ |
| 6 | 0.386833 | `aks_nodepool_list` | ❌ |
| 7 | 0.378527 | `monitor_table_type_list` | ❌ |
| 8 | 0.365129 | `aks_nodepool_get` | ❌ |
| 9 | 0.357516 | `mysql_table_list` | ❌ |
| 10 | 0.350723 | `virtualdesktop_hostpool_list` | ❌ |
| 11 | 0.344799 | `postgres_server_list` | ❌ |
| 12 | 0.344468 | `mysql_server_param_get` | ❌ |
| 13 | 0.342703 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 14 | 0.321778 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.315606 | `foundry_agents_list` | ❌ |
| 16 | 0.298933 | `cosmos_database_list` | ❌ |
| 17 | 0.292566 | `kusto_cluster_list` | ❌ |
| 18 | 0.284157 | `kusto_database_list` | ❌ |
| 19 | 0.281680 | `cosmos_account_list` | ❌ |
| 20 | 0.259656 | `appservice_database_add` | ❌ |

---

## Test 288

**Expected Tool:** `sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682403 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563927 | `sql_db_create` | ❌ |
| 3 | 0.536905 | `sql_server_delete` | ❌ |
| 4 | 0.529077 | `sql_server_list` | ❌ |
| 5 | 0.482050 | `storage_account_create` | ❌ |
| 6 | 0.473614 | `sql_db_show` | ❌ |
| 7 | 0.465097 | `mysql_server_list` | ❌ |
| 8 | 0.452450 | `loadtesting_testresource_create` | ❌ |
| 9 | 0.449739 | `sql_db_list` | ❌ |
| 10 | 0.449707 | `sql_server_show` | ❌ |
| 11 | 0.418801 | `sql_elastic-pool_list` | ❌ |
| 12 | 0.352888 | `appservice_database_add` | ❌ |
| 13 | 0.338888 | `keyvault_key_create` | ❌ |
| 14 | 0.335879 | `functionapp_get` | ❌ |
| 15 | 0.332475 | `extension_azqr` | ❌ |
| 16 | 0.329985 | `keyvault_certificate_create` | ❌ |
| 17 | 0.326922 | `datadog_monitoredresources_list` | ❌ |
| 18 | 0.323380 | `acr_registry_repository_list` | ❌ |
| 19 | 0.320328 | `keyvault_secret_create` | ❌ |
| 20 | 0.319732 | `acr_registry_list` | ❌ |

---

## Test 289

**Expected Tool:** `sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618376 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510169 | `sql_db_create` | ❌ |
| 3 | 0.472463 | `sql_server_show` | ❌ |
| 4 | 0.434810 | `sql_server_delete` | ❌ |
| 5 | 0.397805 | `sql_server_list` | ❌ |
| 6 | 0.396073 | `storage_account_create` | ❌ |
| 7 | 0.368115 | `sql_db_show` | ❌ |
| 8 | 0.360875 | `mysql_server_list` | ❌ |
| 9 | 0.358318 | `appservice_database_add` | ❌ |
| 10 | 0.354438 | `sql_elastic-pool_list` | ❌ |
| 11 | 0.349337 | `sql_server_firewall-rule_create` | ❌ |
| 12 | 0.325467 | `keyvault_secret_create` | ❌ |
| 13 | 0.324021 | `deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.319450 | `keyvault_key_create` | ❌ |
| 15 | 0.316979 | `loadtesting_test_create` | ❌ |
| 16 | 0.315987 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.314142 | `foundry_agents_connect` | ❌ |
| 18 | 0.301134 | `loadtesting_testresource_create` | ❌ |
| 19 | 0.301132 | `deploy_plan_get` | ❌ |
| 20 | 0.298414 | `keyvault_certificate_create` | ❌ |

---

## Test 290

**Expected Tool:** `sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589802 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.501302 | `sql_db_create` | ❌ |
| 3 | 0.497923 | `sql_server_list` | ❌ |
| 4 | 0.469342 | `sql_server_delete` | ❌ |
| 5 | 0.442982 | `mysql_server_list` | ❌ |
| 6 | 0.423953 | `sql_server_show` | ❌ |
| 7 | 0.421581 | `sql_db_list` | ❌ |
| 8 | 0.417595 | `sql_db_show` | ❌ |
| 9 | 0.416002 | `storage_account_create` | ❌ |
| 10 | 0.415419 | `appservice_database_add` | ❌ |
| 11 | 0.389656 | `sql_elastic-pool_list` | ❌ |
| 12 | 0.385725 | `loadtesting_testresource_create` | ❌ |
| 13 | 0.312647 | `loadtesting_test_create` | ❌ |
| 14 | 0.301026 | `functionapp_get` | ❌ |
| 15 | 0.298397 | `group_list` | ❌ |
| 16 | 0.291625 | `keyvault_secret_create` | ❌ |
| 17 | 0.288589 | `datadog_monitoredresources_list` | ❌ |
| 18 | 0.284723 | `deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.277823 | `acr_registry_list` | ❌ |
| 20 | 0.271191 | `deploy_plan_get` | ❌ |

---

## Test 291

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702353 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.518036 | `sql_server_list` | ❌ |
| 3 | 0.495580 | `sql_server_create` | ❌ |
| 4 | 0.486195 | `sql_db_delete` | ❌ |
| 5 | 0.483132 | `workbooks_delete` | ❌ |
| 6 | 0.470205 | `sql_db_show` | ❌ |
| 7 | 0.449007 | `mysql_server_list` | ❌ |
| 8 | 0.448514 | `sql_server_show` | ❌ |
| 9 | 0.438950 | `sql_db_list` | ❌ |
| 10 | 0.417035 | `sql_server_firewall-rule_delete` | ❌ |
| 11 | 0.346442 | `functionapp_get` | ❌ |
| 12 | 0.333269 | `datadog_monitoredresources_list` | ❌ |
| 13 | 0.323460 | `acr_registry_repository_list` | ❌ |
| 14 | 0.317588 | `extension_azqr` | ❌ |
| 15 | 0.317267 | `group_list` | ❌ |
| 16 | 0.310685 | `appservice_database_add` | ❌ |
| 17 | 0.307426 | `appconfig_kv_delete` | ❌ |
| 18 | 0.289982 | `acr_registry_list` | ❌ |
| 19 | 0.275321 | `applicationinsights_recommendation_list` | ❌ |
| 20 | 0.273552 | `loadtesting_testresource_create` | ❌ |

---

## Test 292

**Expected Tool:** `sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.429140 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393885 | `postgres_server_list` | ❌ |
| 3 | 0.376660 | `sql_server_show` | ❌ |
| 4 | 0.350103 | `sql_server_list` | ❌ |
| 5 | 0.309280 | `sql_server_firewall-rule_delete` | ❌ |
| 6 | 0.306368 | `sql_db_show` | ❌ |
| 7 | 0.301933 | `sql_db_delete` | ❌ |
| 8 | 0.299785 | `sql_server_create` | ❌ |
| 9 | 0.295963 | `sql_db_list` | ❌ |
| 10 | 0.295089 | `sql_server_entra-admin_list` | ❌ |
| 11 | 0.274716 | `appservice_database_add` | ❌ |
| 12 | 0.258333 | `kusto_database_list` | ❌ |
| 13 | 0.235107 | `cosmos_account_list` | ❌ |
| 14 | 0.234779 | `appconfig_kv_delete` | ❌ |
| 15 | 0.234376 | `kusto_cluster_list` | ❌ |
| 16 | 0.226608 | `kusto_cluster_get` | ❌ |
| 17 | 0.226581 | `eventgrid_subscription_list` | ❌ |
| 18 | 0.225579 | `grafana_list` | ❌ |
| 19 | 0.219702 | `kusto_table_list` | ❌ |
| 20 | 0.210483 | `appconfig_account_list` | ❌ |

---

## Test 293

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527930 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.396541 | `sql_db_delete` | ❌ |
| 3 | 0.362389 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.341503 | `sql_server_show` | ❌ |
| 5 | 0.315820 | `workbooks_delete` | ❌ |
| 6 | 0.274865 | `sql_server_create` | ❌ |
| 7 | 0.262387 | `sql_server_entra-admin_list` | ❌ |
| 8 | 0.261656 | `sql_server_firewall-rule_list` | ❌ |
| 9 | 0.254391 | `appconfig_kv_delete` | ❌ |
| 10 | 0.247364 | `postgres_server_param_set` | ❌ |
| 11 | 0.237815 | `mysql_table_list` | ❌ |
| 12 | 0.213559 | `appservice_database_add` | ❌ |
| 13 | 0.168042 | `cosmos_database_container_item_query` | ❌ |
| 14 | 0.159907 | `kusto_table_list` | ❌ |
| 15 | 0.156253 | `cosmos_database_list` | ❌ |
| 16 | 0.148272 | `kusto_database_list` | ❌ |
| 17 | 0.146243 | `kusto_table_schema` | ❌ |
| 18 | 0.142127 | `kusto_query` | ❌ |
| 19 | 0.125251 | `loadtesting_testrun_list` | ❌ |
| 20 | 0.123510 | `cosmos_database_container_list` | ❌ |

---

## Test 294

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783526 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.456051 | `sql_server_show` | ❌ |
| 3 | 0.434868 | `sql_server_list` | ❌ |
| 4 | 0.401853 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376055 | `sql_db_list` | ❌ |
| 6 | 0.365636 | `postgres_server_list` | ❌ |
| 7 | 0.352607 | `mysql_database_list` | ❌ |
| 8 | 0.344454 | `mysql_server_list` | ❌ |
| 9 | 0.343559 | `mysql_table_list` | ❌ |
| 10 | 0.329480 | `sql_server_create` | ❌ |
| 11 | 0.291758 | `foundry_agents_list` | ❌ |
| 12 | 0.280450 | `cosmos_database_list` | ❌ |
| 13 | 0.258095 | `cosmos_account_list` | ❌ |
| 14 | 0.249297 | `datadog_monitoredresources_list` | ❌ |
| 15 | 0.249153 | `kusto_database_list` | ❌ |
| 16 | 0.245298 | `group_list` | ❌ |
| 17 | 0.234681 | `azuremanagedlustre_filesystem_list` | ❌ |
| 18 | 0.234181 | `keyvault_secret_list` | ❌ |
| 19 | 0.233337 | `cosmos_database_container_list` | ❌ |
| 20 | 0.228391 | `keyvault_certificate_list` | ❌ |

---

## Test 295

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713284 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.413144 | `sql_server_show` | ❌ |
| 3 | 0.368082 | `sql_server_list` | ❌ |
| 4 | 0.315966 | `sql_db_list` | ❌ |
| 5 | 0.311085 | `postgres_server_list` | ❌ |
| 6 | 0.304832 | `sql_server_firewall-rule_list` | ❌ |
| 7 | 0.303560 | `postgres_server_config_get` | ❌ |
| 8 | 0.289857 | `sql_server_create` | ❌ |
| 9 | 0.287372 | `mysql_database_list` | ❌ |
| 10 | 0.283806 | `mysql_table_list` | ❌ |
| 11 | 0.273063 | `foundry_agents_list` | ❌ |
| 12 | 0.214529 | `cosmos_database_list` | ❌ |
| 13 | 0.205965 | `appservice_database_add` | ❌ |
| 14 | 0.197679 | `cosmos_database_container_list` | ❌ |
| 15 | 0.194313 | `appconfig_account_list` | ❌ |
| 16 | 0.193050 | `kusto_database_list` | ❌ |
| 17 | 0.191538 | `appconfig_kv_list` | ❌ |
| 18 | 0.188120 | `cosmos_account_list` | ❌ |
| 19 | 0.183184 | `deploy_architecture_diagram_generate` | ❌ |
| 20 | 0.182280 | `deploy_app_logs_get` | ❌ |

---

## Test 296

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646469 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.356004 | `sql_server_show` | ❌ |
| 3 | 0.322163 | `sql_server_list` | ❌ |
| 4 | 0.307885 | `sql_server_create` | ❌ |
| 5 | 0.253846 | `sql_db_list` | ❌ |
| 6 | 0.237068 | `mysql_table_list` | ❌ |
| 7 | 0.236121 | `mysql_server_list` | ❌ |
| 8 | 0.235091 | `appservice_database_add` | ❌ |
| 9 | 0.230857 | `sql_db_create` | ❌ |
| 10 | 0.228284 | `sql_server_delete` | ❌ |
| 11 | 0.222975 | `sql_db_update` | ❌ |
| 12 | 0.212602 | `cloudarchitect_design` | ❌ |
| 13 | 0.210601 | `foundry_agents_list` | ❌ |
| 14 | 0.200430 | `applens_resource_diagnose` | ❌ |
| 15 | 0.189829 | `deploy_architecture_diagram_generate` | ❌ |
| 16 | 0.188167 | `deploy_plan_get` | ❌ |
| 17 | 0.180851 | `deploy_app_logs_get` | ❌ |
| 18 | 0.180663 | `foundry_agents_connect` | ❌ |
| 19 | 0.180437 | `deploy_pipeline_guidance_get` | ❌ |
| 20 | 0.174342 | `deploy_iac_rules_get` | ❌ |

---

## Test 297

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635134 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532729 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448932 | `sql_server_create` | ❌ |
| 5 | 0.432802 | `sql_db_create` | ❌ |
| 6 | 0.423223 | `sql_server_show` | ❌ |
| 7 | 0.403858 | `sql_server_delete` | ❌ |
| 8 | 0.397912 | `sql_server_list` | ❌ |
| 9 | 0.361316 | `appservice_database_add` | ❌ |
| 10 | 0.335670 | `mysql_server_list` | ❌ |
| 11 | 0.326512 | `sql_db_update` | ❌ |
| 12 | 0.290311 | `deploy_iac_rules_get` | ❌ |
| 13 | 0.288030 | `deploy_pipeline_guidance_get` | ❌ |
| 14 | 0.271102 | `keyvault_secret_create` | ❌ |
| 15 | 0.268480 | `keyvault_certificate_create` | ❌ |
| 16 | 0.265059 | `azureterraformbestpractices_get` | ❌ |
| 17 | 0.260209 | `cosmos_database_container_item_query` | ❌ |
| 18 | 0.253771 | `deploy_plan_get` | ❌ |
| 19 | 0.251921 | `foundry_agents_connect` | ❌ |
| 20 | 0.250771 | `keyvault_key_create` | ❌ |

---

## Test 298

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670345 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533523 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316619 | `sql_server_list` | ❌ |
| 5 | 0.295018 | `sql_server_show` | ❌ |
| 6 | 0.287526 | `sql_server_create` | ❌ |
| 7 | 0.284229 | `appservice_database_add` | ❌ |
| 8 | 0.271240 | `sql_server_delete` | ❌ |
| 9 | 0.252999 | `sql_server_entra-admin_list` | ❌ |
| 10 | 0.248826 | `postgres_server_param_set` | ❌ |
| 11 | 0.237646 | `sql_db_create` | ❌ |
| 12 | 0.222068 | `azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 13 | 0.174851 | `deploy_iac_rules_get` | ❌ |
| 14 | 0.174584 | `cosmos_database_container_item_query` | ❌ |
| 15 | 0.166723 | `deploy_pipeline_guidance_get` | ❌ |
| 16 | 0.151674 | `keyvault_secret_create` | ❌ |
| 17 | 0.149884 | `kusto_query` | ❌ |
| 18 | 0.145883 | `foundry_agents_connect` | ❌ |
| 19 | 0.143603 | `appconfig_kv_set` | ❌ |
| 20 | 0.140206 | `loadtesting_test_create` | ❌ |

---

## Test 299

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684716 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574392 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539577 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.429010 | `sql_server_create` | ❌ |
| 5 | 0.395165 | `sql_db_create` | ❌ |
| 6 | 0.356402 | `sql_server_show` | ❌ |
| 7 | 0.339841 | `sql_server_delete` | ❌ |
| 8 | 0.316783 | `sql_server_list` | ❌ |
| 9 | 0.296548 | `appservice_database_add` | ❌ |
| 10 | 0.281043 | `postgres_server_param_set` | ❌ |
| 11 | 0.270400 | `sql_server_entra-admin_list` | ❌ |
| 12 | 0.248777 | `loadtesting_test_create` | ❌ |
| 13 | 0.240101 | `keyvault_secret_create` | ❌ |
| 14 | 0.229431 | `keyvault_key_create` | ❌ |
| 15 | 0.221981 | `keyvault_certificate_create` | ❌ |
| 16 | 0.221003 | `deploy_iac_rules_get` | ❌ |
| 17 | 0.219182 | `cosmos_database_container_item_query` | ❌ |
| 18 | 0.209374 | `loadtesting_testrun_create` | ❌ |
| 19 | 0.207520 | `loadtesting_testresource_create` | ❌ |
| 20 | 0.197104 | `appconfig_kv_set` | ❌ |

---

## Test 300

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.543896 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.540052 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.527546 | `sql_server_delete` | ❌ |
| 5 | 0.436585 | `sql_db_delete` | ❌ |
| 6 | 0.418504 | `sql_server_show` | ❌ |
| 7 | 0.410574 | `workbooks_delete` | ❌ |
| 8 | 0.386562 | `sql_server_list` | ❌ |
| 9 | 0.342045 | `sql_db_update` | ❌ |
| 10 | 0.342004 | `sql_server_create` | ❌ |
| 11 | 0.312054 | `appconfig_kv_delete` | ❌ |
| 12 | 0.306441 | `appservice_database_add` | ❌ |
| 13 | 0.263959 | `cosmos_database_container_item_query` | ❌ |
| 14 | 0.245270 | `azureterraformbestpractices_get` | ❌ |
| 15 | 0.241580 | `deploy_iac_rules_get` | ❌ |
| 16 | 0.231494 | `functionapp_get` | ❌ |
| 17 | 0.225227 | `kusto_query` | ❌ |
| 18 | 0.223780 | `keyvault_secret_get` | ❌ |
| 19 | 0.220989 | `get_bestpractices_get` | ❌ |
| 20 | 0.216857 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 301

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670179 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574400 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.530158 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.398706 | `sql_server_delete` | ❌ |
| 5 | 0.310449 | `sql_server_show` | ❌ |
| 6 | 0.298395 | `sql_db_delete` | ❌ |
| 7 | 0.293097 | `sql_server_list` | ❌ |
| 8 | 0.259110 | `workbooks_delete` | ❌ |
| 9 | 0.254974 | `appconfig_kv_delete` | ❌ |
| 10 | 0.251024 | `sql_server_entra-admin_list` | ❌ |
| 11 | 0.231953 | `sql_server_create` | ❌ |
| 12 | 0.227875 | `appservice_database_add` | ❌ |
| 13 | 0.182013 | `cosmos_database_container_item_query` | ❌ |
| 14 | 0.158025 | `kusto_query` | ❌ |
| 15 | 0.156028 | `functionapp_get` | ❌ |
| 16 | 0.152458 | `cosmos_database_list` | ❌ |
| 17 | 0.152084 | `azureterraformbestpractices_get` | ❌ |
| 18 | 0.145688 | `loadtesting_test_get` | ❌ |
| 19 | 0.142881 | `deploy_iac_rules_get` | ❌ |
| 20 | 0.142512 | `kusto_database_list` | ❌ |

---

## Test 302

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671212 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601294 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.576930 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.441605 | `sql_server_delete` | ❌ |
| 5 | 0.367883 | `sql_server_show` | ❌ |
| 6 | 0.336482 | `sql_db_delete` | ❌ |
| 7 | 0.332209 | `sql_server_list` | ❌ |
| 8 | 0.293354 | `sql_server_create` | ❌ |
| 9 | 0.291427 | `sql_server_entra-admin_list` | ❌ |
| 10 | 0.286559 | `sql_db_update` | ❌ |
| 11 | 0.264013 | `appservice_database_add` | ❌ |
| 12 | 0.252095 | `appconfig_kv_delete` | ❌ |
| 13 | 0.222155 | `cosmos_database_container_item_query` | ❌ |
| 14 | 0.185585 | `cosmos_database_list` | ❌ |
| 15 | 0.185007 | `functionapp_get` | ❌ |
| 16 | 0.183564 | `deploy_iac_rules_get` | ❌ |
| 17 | 0.181757 | `azureterraformbestpractices_get` | ❌ |
| 18 | 0.180404 | `kusto_query` | ❌ |
| 19 | 0.175839 | `keyvault_secret_get` | ❌ |
| 20 | 0.174348 | `loadtesting_test_get` | ❌ |

---

## Test 303

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729372 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549389 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `sql_server_show` | ❌ |
| 5 | 0.418817 | `sql_server_list` | ❌ |
| 6 | 0.392556 | `sql_server_entra-admin_list` | ❌ |
| 7 | 0.385148 | `postgres_server_list` | ❌ |
| 8 | 0.359228 | `sql_db_list` | ❌ |
| 9 | 0.356700 | `mysql_server_list` | ❌ |
| 10 | 0.355203 | `mysql_table_list` | ❌ |
| 11 | 0.278098 | `cosmos_database_list` | ❌ |
| 12 | 0.274634 | `foundry_agents_list` | ❌ |
| 13 | 0.271222 | `keyvault_secret_list` | ❌ |
| 14 | 0.270667 | `cosmos_account_list` | ❌ |
| 15 | 0.263181 | `kusto_table_list` | ❌ |
| 16 | 0.256310 | `aks_nodepool_list` | ❌ |
| 17 | 0.253852 | `cosmos_database_container_list` | ❌ |
| 18 | 0.251395 | `keyvault_certificate_list` | ❌ |
| 19 | 0.249012 | `keyvault_key_list` | ❌ |
| 20 | 0.248780 | `cosmos_database_container_item_query` | ❌ |

---

## Test 304

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630774 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.523847 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410680 | `sql_server_show` | ❌ |
| 5 | 0.348100 | `sql_server_list` | ❌ |
| 6 | 0.316879 | `sql_server_entra-admin_list` | ❌ |
| 7 | 0.312035 | `postgres_server_list` | ❌ |
| 8 | 0.298995 | `mysql_server_param_get` | ❌ |
| 9 | 0.294466 | `mysql_server_config_get` | ❌ |
| 10 | 0.293371 | `mysql_server_list` | ❌ |
| 11 | 0.225372 | `cosmos_database_container_item_query` | ❌ |
| 12 | 0.217423 | `appservice_database_add` | ❌ |
| 13 | 0.211187 | `eventgrid_subscription_list` | ❌ |
| 14 | 0.210531 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.209562 | `foundry_agents_list` | ❌ |
| 16 | 0.206477 | `deploy_iac_rules_get` | ❌ |
| 17 | 0.206114 | `kusto_table_list` | ❌ |
| 18 | 0.200976 | `keyvault_secret_get` | ❌ |
| 19 | 0.197711 | `kusto_sample` | ❌ |
| 20 | 0.195864 | `aks_nodepool_list` | ❌ |

---

## Test 305

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630536 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532236 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473501 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412957 | `sql_server_show` | ❌ |
| 5 | 0.350513 | `sql_server_list` | ❌ |
| 6 | 0.308033 | `sql_server_entra-admin_list` | ❌ |
| 7 | 0.305701 | `mysql_server_param_get` | ❌ |
| 8 | 0.304314 | `mysql_server_config_get` | ❌ |
| 9 | 0.282631 | `sql_server_create` | ❌ |
| 10 | 0.277628 | `postgres_server_config_get` | ❌ |
| 11 | 0.221752 | `appservice_database_add` | ❌ |
| 12 | 0.216178 | `foundry_agents_list` | ❌ |
| 13 | 0.202416 | `deploy_iac_rules_get` | ❌ |
| 14 | 0.200326 | `cosmos_database_container_item_query` | ❌ |
| 15 | 0.191165 | `cloudarchitect_design` | ❌ |
| 16 | 0.185879 | `eventgrid_subscription_list` | ❌ |
| 17 | 0.177454 | `loadtesting_test_get` | ❌ |
| 18 | 0.176225 | `get_bestpractices_get` | ❌ |
| 19 | 0.173318 | `applens_resource_diagnose` | ❌ |
| 20 | 0.172371 | `aks_nodepool_get` | ❌ |

---

## Test 306

**Expected Tool:** `sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694404 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596686 | `mysql_server_list` | ❌ |
| 3 | 0.578239 | `sql_db_list` | ❌ |
| 4 | 0.515851 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.509789 | `sql_db_show` | ❌ |
| 6 | 0.500373 | `sql_server_delete` | ❌ |
| 7 | 0.496921 | `group_list` | ❌ |
| 8 | 0.496434 | `postgres_server_list` | ❌ |
| 9 | 0.495321 | `datadog_monitoredresources_list` | ❌ |
| 10 | 0.487690 | `sql_server_create` | ❌ |
| 11 | 0.487455 | `sql_server_show` | ❌ |
| 12 | 0.473451 | `workbooks_list` | ❌ |
| 13 | 0.449346 | `acr_registry_repository_list` | ❌ |
| 14 | 0.449174 | `acr_registry_list` | ❌ |
| 15 | 0.419283 | `functionapp_get` | ❌ |
| 16 | 0.403710 | `foundry_agents_list` | ❌ |
| 17 | 0.395561 | `cosmos_database_list` | ❌ |
| 18 | 0.384532 | `cosmos_account_list` | ❌ |
| 19 | 0.384389 | `kusto_database_list` | ❌ |
| 20 | 0.380949 | `azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 307

**Expected Tool:** `sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618218 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593837 | `mysql_server_list` | ❌ |
| 3 | 0.542398 | `sql_db_list` | ❌ |
| 4 | 0.507404 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.496252 | `group_list` | ❌ |
| 6 | 0.495949 | `sql_elastic-pool_list` | ❌ |
| 7 | 0.492324 | `datadog_monitoredresources_list` | ❌ |
| 8 | 0.484599 | `workbooks_list` | ❌ |
| 9 | 0.477041 | `postgres_server_list` | ❌ |
| 10 | 0.470456 | `sql_db_show` | ❌ |
| 11 | 0.464018 | `mysql_database_list` | ❌ |
| 12 | 0.449733 | `redis_cluster_list` | ❌ |
| 13 | 0.444175 | `acr_registry_list` | ❌ |
| 14 | 0.419472 | `foundry_agents_list` | ❌ |
| 15 | 0.418009 | `azuremanagedlustre_filesystem_list` | ❌ |
| 16 | 0.410302 | `acr_registry_repository_list` | ❌ |
| 17 | 0.397122 | `loadtesting_testresource_list` | ❌ |
| 18 | 0.395060 | `cosmos_database_list` | ❌ |
| 19 | 0.391940 | `kusto_cluster_list` | ❌ |
| 20 | 0.384337 | `cosmos_account_list` | ❌ |

---

## Test 308

**Expected Tool:** `sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `sql_db_show` | ❌ |
| 2 | 0.595184 | `sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587728 | `sql_server_list` | ❌ |
| 4 | 0.559893 | `mysql_server_list` | ❌ |
| 5 | 0.540218 | `sql_db_list` | ❌ |
| 6 | 0.491437 | `sql_server_create` | ❌ |
| 7 | 0.488317 | `sql_server_delete` | ❌ |
| 8 | 0.481847 | `functionapp_get` | ❌ |
| 9 | 0.480067 | `mysql_server_config_get` | ❌ |
| 10 | 0.478713 | `sql_elastic-pool_list` | ❌ |
| 11 | 0.450140 | `aks_cluster_get` | ❌ |
| 12 | 0.445602 | `storage_account_get` | ❌ |
| 13 | 0.437447 | `datadog_monitoredresources_list` | ❌ |
| 14 | 0.424890 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.410399 | `group_list` | ❌ |
| 16 | 0.400396 | `aks_nodepool_get` | ❌ |
| 17 | 0.394066 | `kusto_cluster_get` | ❌ |
| 18 | 0.385318 | `extension_azqr` | ❌ |
| 19 | 0.383493 | `acr_registry_list` | ❌ |
| 20 | 0.373431 | `eventgrid_subscription_list` | ❌ |

---

## Test 309

**Expected Tool:** `sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `postgres_server_config_get` | ❌ |
| 3 | 0.538034 | `mysql_server_config_get` | ❌ |
| 4 | 0.471477 | `sql_db_show` | ❌ |
| 5 | 0.445500 | `postgres_server_param_get` | ❌ |
| 6 | 0.443977 | `mysql_server_param_get` | ❌ |
| 7 | 0.422646 | `sql_db_list` | ❌ |
| 8 | 0.414309 | `sql_server_list` | ❌ |
| 9 | 0.413928 | `sql_server_firewall-rule_list` | ❌ |
| 10 | 0.406630 | `loadtesting_test_get` | ❌ |
| 11 | 0.400928 | `sql_server_create` | ❌ |
| 12 | 0.359439 | `aks_cluster_get` | ❌ |
| 13 | 0.349963 | `aks_nodepool_get` | ❌ |
| 14 | 0.316818 | `appconfig_kv_list` | ❌ |
| 15 | 0.314864 | `appconfig_kv_show` | ❌ |
| 16 | 0.308718 | `functionapp_get` | ❌ |
| 17 | 0.300098 | `kusto_cluster_get` | ❌ |
| 18 | 0.298409 | `appconfig_account_list` | ❌ |
| 19 | 0.296248 | `keyvault_key_get` | ❌ |
| 20 | 0.295903 | `loadtesting_testrun_list` | ❌ |

---

## Test 310

**Expected Tool:** `sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `postgres_server_config_get` | ❌ |
| 3 | 0.380021 | `postgres_server_param_get` | ❌ |
| 4 | 0.372108 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370539 | `sql_db_show` | ❌ |
| 6 | 0.368846 | `sql_server_entra-admin_list` | ❌ |
| 7 | 0.367031 | `sql_db_list` | ❌ |
| 8 | 0.363268 | `mysql_server_config_get` | ❌ |
| 9 | 0.361792 | `sql_server_list` | ❌ |
| 10 | 0.357960 | `mysql_database_list` | ❌ |
| 11 | 0.288829 | `azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.276327 | `cosmos_database_list` | ❌ |
| 13 | 0.271945 | `appconfig_kv_show` | ❌ |
| 14 | 0.268884 | `loadtesting_testrun_get` | ❌ |
| 15 | 0.257258 | `appconfig_kv_list` | ❌ |
| 16 | 0.255863 | `foundry_agents_list` | ❌ |
| 17 | 0.246261 | `aks_nodepool_get` | ❌ |
| 18 | 0.240682 | `cosmos_account_list` | ❌ |
| 19 | 0.234423 | `cosmos_database_container_list` | ❌ |
| 20 | 0.234389 | `aks_nodepool_list` | ❌ |

---

## Test 311

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.418472 | `storage_account_get` | ❌ |
| 3 | 0.394541 | `storage_blob_container_create` | ❌ |
| 4 | 0.374072 | `loadtesting_test_create` | ❌ |
| 5 | 0.355645 | `loadtesting_testresource_create` | ❌ |
| 6 | 0.352179 | `storage_blob_container_get` | ❌ |
| 7 | 0.323501 | `appconfig_kv_set` | ❌ |
| 8 | 0.319843 | `quota_usage_check` | ❌ |
| 9 | 0.311941 | `sql_db_create` | ❌ |
| 10 | 0.311275 | `storage_blob_upload` | ❌ |
| 11 | 0.300268 | `sql_server_create` | ❌ |
| 12 | 0.297236 | `cosmos_account_list` | ❌ |
| 13 | 0.289742 | `appconfig_kv_show` | ❌ |
| 14 | 0.289299 | `keyvault_key_create` | ❌ |
| 15 | 0.286778 | `monitor_resource_log_query` | ❌ |
| 16 | 0.278047 | `quota_region_availability_list` | ❌ |
| 17 | 0.277805 | `cosmos_database_container_list` | ❌ |
| 18 | 0.269922 | `keyvault_secret_create` | ❌ |
| 19 | 0.267474 | `azuremanagedlustre_filesystem_list` | ❌ |
| 20 | 0.262425 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 312

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.400151 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.387071 | `storage_account_get` | ❌ |
| 4 | 0.382836 | `azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.377221 | `sql_db_create` | ❌ |
| 6 | 0.376155 | `storage_blob_container_create` | ❌ |
| 7 | 0.344628 | `loadtesting_testresource_create` | ❌ |
| 8 | 0.340135 | `storage_blob_container_get` | ❌ |
| 9 | 0.329141 | `loadtesting_test_create` | ❌ |
| 10 | 0.324376 | `sql_server_create` | ❌ |
| 11 | 0.310931 | `monitor_resource_log_query` | ❌ |
| 12 | 0.310707 | `storage_blob_upload` | ❌ |
| 13 | 0.310332 | `workbooks_create` | ❌ |
| 14 | 0.296380 | `resourcehealth_availability-status_get` | ❌ |
| 15 | 0.284467 | `deploy_plan_get` | ❌ |
| 16 | 0.284385 | `cosmos_account_list` | ❌ |
| 17 | 0.283067 | `deploy_pipeline_guidance_get` | ❌ |
| 18 | 0.280192 | `cloudarchitect_design` | ❌ |
| 19 | 0.272299 | `cosmos_database_container_item_query` | ❌ |
| 20 | 0.270659 | `appservice_database_add` | ❌ |

---

## Test 313

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589003 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.464611 | `storage_blob_container_create` | ❌ |
| 3 | 0.447156 | `sql_db_create` | ❌ |
| 4 | 0.437040 | `storage_account_get` | ❌ |
| 5 | 0.407389 | `storage_blob_container_get` | ❌ |
| 6 | 0.383865 | `sql_server_create` | ❌ |
| 7 | 0.383853 | `loadtesting_testresource_create` | ❌ |
| 8 | 0.382274 | `azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.380711 | `loadtesting_test_create` | ❌ |
| 10 | 0.372681 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 11 | 0.366696 | `deploy_pipeline_guidance_get` | ❌ |
| 12 | 0.363721 | `workbooks_create` | ❌ |
| 13 | 0.360940 | `storage_blob_upload` | ❌ |
| 14 | 0.345804 | `keyvault_key_create` | ❌ |
| 15 | 0.326175 | `keyvault_certificate_create` | ❌ |
| 16 | 0.324991 | `storage_blob_get` | ❌ |
| 17 | 0.324944 | `monitor_resource_log_query` | ❌ |
| 18 | 0.324692 | `appservice_database_add` | ❌ |
| 19 | 0.321846 | `deploy_plan_get` | ❌ |
| 20 | 0.309672 | `keyvault_secret_create` | ❌ |

---

## Test 314

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655152 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.603458 | `storage_blob_container_get` | ❌ |
| 3 | 0.507232 | `storage_blob_get` | ❌ |
| 4 | 0.483435 | `storage_account_create` | ❌ |
| 5 | 0.442858 | `appconfig_kv_show` | ❌ |
| 6 | 0.439236 | `cosmos_account_list` | ❌ |
| 7 | 0.431020 | `azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.403478 | `cosmos_database_container_list` | ❌ |
| 9 | 0.397051 | `mysql_server_config_get` | ❌ |
| 10 | 0.395698 | `quota_usage_check` | ❌ |
| 11 | 0.388425 | `aks_cluster_get` | ❌ |
| 12 | 0.382163 | `keyvault_key_get` | ❌ |
| 13 | 0.376586 | `keyvault_secret_get` | ❌ |
| 14 | 0.373840 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 15 | 0.373146 | `sql_server_show` | ❌ |
| 16 | 0.368567 | `sql_db_show` | ❌ |
| 17 | 0.367173 | `subscription_list` | ❌ |
| 18 | 0.367049 | `kusto_cluster_get` | ❌ |
| 19 | 0.366645 | `aks_nodepool_get` | ❌ |
| 20 | 0.363293 | `search_index_get` | ❌ |

---

## Test 315

**Expected Tool:** `storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676940 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.612732 | `storage_blob_container_get` | ❌ |
| 3 | 0.518279 | `storage_account_create` | ❌ |
| 4 | 0.514932 | `storage_blob_get` | ❌ |
| 5 | 0.415417 | `cosmos_account_list` | ❌ |
| 6 | 0.411847 | `appconfig_kv_show` | ❌ |
| 7 | 0.401838 | `azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.380103 | `sql_server_show` | ❌ |
| 9 | 0.375843 | `quota_usage_check` | ❌ |
| 10 | 0.373568 | `aks_cluster_get` | ❌ |
| 11 | 0.369794 | `cosmos_database_container_list` | ❌ |
| 12 | 0.368227 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.368110 | `kusto_cluster_get` | ❌ |
| 14 | 0.362681 | `aks_nodepool_get` | ❌ |
| 15 | 0.362676 | `mysql_server_config_get` | ❌ |
| 16 | 0.362365 | `marketplace_product_get` | ❌ |
| 17 | 0.355144 | `servicebus_queue_details` | ❌ |
| 18 | 0.354889 | `resourcehealth_availability-status_get` | ❌ |
| 19 | 0.353567 | `keyvault_key_get` | ❌ |
| 20 | 0.351188 | `functionapp_get` | ❌ |

---

## Test 316

**Expected Tool:** `storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664087 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557016 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.536909 | `cosmos_account_list` | ❌ |
| 4 | 0.535616 | `storage_account_create` | ❌ |
| 5 | 0.501088 | `subscription_list` | ❌ |
| 6 | 0.496371 | `quota_region_availability_list` | ❌ |
| 7 | 0.493246 | `appconfig_account_list` | ❌ |
| 8 | 0.484449 | `storage_blob_container_get` | ❌ |
| 9 | 0.484163 | `azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.473387 | `search_service_list` | ❌ |
| 11 | 0.458793 | `monitor_workspace_list` | ❌ |
| 12 | 0.454207 | `acr_registry_list` | ❌ |
| 13 | 0.448075 | `aks_cluster_list` | ❌ |
| 14 | 0.445522 | `redis_cache_list` | ❌ |
| 15 | 0.441838 | `redis_cluster_list` | ❌ |
| 16 | 0.439919 | `eventgrid_subscription_list` | ❌ |
| 17 | 0.438660 | `resourcehealth_availability-status_list` | ❌ |
| 18 | 0.432645 | `kusto_cluster_list` | ❌ |
| 19 | 0.416486 | `group_list` | ❌ |
| 20 | 0.412679 | `grafana_list` | ❌ |

---

## Test 317

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499302 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.461284 | `azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.455440 | `storage_blob_container_get` | ❌ |
| 4 | 0.421642 | `cosmos_account_list` | ❌ |
| 5 | 0.379853 | `resourcehealth_availability-status_list` | ❌ |
| 6 | 0.378256 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 7 | 0.375553 | `cosmos_database_container_list` | ❌ |
| 8 | 0.367906 | `cosmos_database_list` | ❌ |
| 9 | 0.366021 | `quota_usage_check` | ❌ |
| 10 | 0.362252 | `storage_account_create` | ❌ |
| 11 | 0.360671 | `storage_blob_get` | ❌ |
| 12 | 0.347173 | `appconfig_account_list` | ❌ |
| 13 | 0.346039 | `monitor_workspace_list` | ❌ |
| 14 | 0.344771 | `search_service_list` | ❌ |
| 15 | 0.342056 | `subscription_list` | ❌ |
| 16 | 0.335306 | `appconfig_kv_show` | ❌ |
| 17 | 0.330862 | `mysql_database_list` | ❌ |
| 18 | 0.330437 | `aks_cluster_list` | ❌ |
| 19 | 0.315590 | `foundry_agents_list` | ❌ |
| 20 | 0.312384 | `acr_registry_list` | ❌ |

---

## Test 318

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557142 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.473598 | `cosmos_account_list` | ❌ |
| 3 | 0.461770 | `storage_blob_container_get` | ❌ |
| 4 | 0.453933 | `subscription_list` | ❌ |
| 5 | 0.436170 | `search_service_list` | ❌ |
| 6 | 0.432854 | `azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.425048 | `resourcehealth_availability-status_list` | ❌ |
| 8 | 0.418403 | `storage_account_create` | ❌ |
| 9 | 0.415799 | `storage_blob_get` | ❌ |
| 10 | 0.415080 | `appconfig_account_list` | ❌ |
| 11 | 0.389930 | `eventgrid_subscription_list` | ❌ |
| 12 | 0.382516 | `aks_cluster_list` | ❌ |
| 13 | 0.379856 | `monitor_workspace_list` | ❌ |
| 14 | 0.377201 | `quota_usage_check` | ❌ |
| 15 | 0.376660 | `appconfig_kv_show` | ❌ |
| 16 | 0.374635 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.371828 | `sql_server_list` | ❌ |
| 18 | 0.359998 | `cosmos_database_list` | ❌ |
| 19 | 0.359051 | `acr_registry_list` | ❌ |
| 20 | 0.356611 | `eventgrid_topic_list` | ❌ |

---

## Test 319

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563396 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.524779 | `storage_account_create` | ❌ |
| 3 | 0.507900 | `storage_blob_container_get` | ❌ |
| 4 | 0.447784 | `cosmos_database_container_list` | ❌ |
| 5 | 0.403407 | `storage_account_get` | ❌ |
| 6 | 0.335039 | `cosmos_database_container_item_query` | ❌ |
| 7 | 0.330963 | `storage_blob_get` | ❌ |
| 8 | 0.326352 | `appconfig_kv_set` | ❌ |
| 9 | 0.324867 | `sql_db_create` | ❌ |
| 10 | 0.322464 | `storage_blob_upload` | ❌ |
| 11 | 0.297912 | `deploy_pipeline_guidance_get` | ❌ |
| 12 | 0.297384 | `cosmos_account_list` | ❌ |
| 13 | 0.292093 | `acr_registry_repository_list` | ❌ |
| 14 | 0.291137 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.287619 | `keyvault_secret_create` | ❌ |
| 16 | 0.281850 | `sql_server_create` | ❌ |
| 17 | 0.281170 | `loadtesting_testresource_create` | ❌ |
| 18 | 0.280866 | `monitor_resource_log_query` | ❌ |
| 19 | 0.277932 | `keyvault_key_create` | ❌ |
| 20 | 0.274863 | `workbooks_create` | ❌ |

---

## Test 320

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512578 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.500624 | `storage_account_create` | ❌ |
| 3 | 0.470896 | `storage_blob_container_get` | ❌ |
| 4 | 0.415378 | `cosmos_database_container_list` | ❌ |
| 5 | 0.414284 | `storage_blob_get` | ❌ |
| 6 | 0.368859 | `storage_account_get` | ❌ |
| 7 | 0.334040 | `storage_blob_upload` | ❌ |
| 8 | 0.320173 | `deploy_pipeline_guidance_get` | ❌ |
| 9 | 0.309739 | `cosmos_database_container_item_query` | ❌ |
| 10 | 0.296899 | `azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.296438 | `sql_db_create` | ❌ |
| 12 | 0.285153 | `cosmos_account_list` | ❌ |
| 13 | 0.275240 | `acr_registry_repository_list` | ❌ |
| 14 | 0.270167 | `appconfig_kv_set` | ❌ |
| 15 | 0.269622 | `deploy_app_logs_get` | ❌ |
| 16 | 0.268922 | `workbooks_create` | ❌ |
| 17 | 0.263818 | `loadtesting_testresource_create` | ❌ |
| 18 | 0.256562 | `sql_server_create` | ❌ |
| 19 | 0.252639 | `deploy_plan_get` | ❌ |
| 20 | 0.249658 | `monitor_resource_log_query` | ❌ |

---

## Test 321

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463198 | `storage_account_create` | ❌ |
| 2 | 0.455073 | `storage_blob_container_get` | ❌ |
| 3 | 0.451690 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 4 | 0.435099 | `cosmos_database_container_list` | ❌ |
| 5 | 0.387977 | `storage_blob_get` | ❌ |
| 6 | 0.378021 | `cosmos_database_container_item_query` | ❌ |
| 7 | 0.366330 | `storage_account_get` | ❌ |
| 8 | 0.329038 | `cosmos_account_list` | ❌ |
| 9 | 0.322364 | `cosmos_database_list` | ❌ |
| 10 | 0.314128 | `sql_db_create` | ❌ |
| 11 | 0.309104 | `storage_blob_upload` | ❌ |
| 12 | 0.287885 | `workbooks_create` | ❌ |
| 13 | 0.277049 | `monitor_resource_log_query` | ❌ |
| 14 | 0.276533 | `azuremanagedlustre_filesystem_list` | ❌ |
| 15 | 0.269719 | `acr_registry_repository_list` | ❌ |
| 16 | 0.266791 | `appconfig_kv_set` | ❌ |
| 17 | 0.265237 | `sql_server_create` | ❌ |
| 18 | 0.262681 | `loadtesting_testresource_create` | ❌ |
| 19 | 0.244113 | `keyvault_certificate_create` | ❌ |
| 20 | 0.243696 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 322

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664379 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.559177 | `storage_account_get` | ❌ |
| 3 | 0.523288 | `cosmos_database_container_list` | ❌ |
| 4 | 0.518035 | `storage_blob_get` | ❌ |
| 5 | 0.496184 | `storage_blob_container_create` | ❌ |
| 6 | 0.461577 | `storage_account_create` | ❌ |
| 7 | 0.421964 | `azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.421220 | `appconfig_kv_show` | ❌ |
| 9 | 0.384585 | `cosmos_account_list` | ❌ |
| 10 | 0.377009 | `cosmos_database_container_item_query` | ❌ |
| 11 | 0.367759 | `quota_usage_check` | ❌ |
| 12 | 0.359218 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.354913 | `sql_server_show` | ❌ |
| 14 | 0.353561 | `monitor_resource_log_query` | ❌ |
| 15 | 0.350264 | `mysql_server_config_get` | ❌ |
| 16 | 0.335739 | `appconfig_kv_list` | ❌ |
| 17 | 0.334806 | `cosmos_database_list` | ❌ |
| 18 | 0.332109 | `deploy_app_logs_get` | ❌ |
| 19 | 0.327271 | `aks_nodepool_get` | ❌ |
| 20 | 0.308777 | `mysql_server_list` | ❌ |

---

## Test 323

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613933 | `cosmos_database_container_list` | ❌ |
| 2 | 0.605617 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 3 | 0.521710 | `storage_blob_get` | ❌ |
| 4 | 0.479014 | `storage_account_get` | ❌ |
| 5 | 0.471385 | `cosmos_account_list` | ❌ |
| 6 | 0.453044 | `cosmos_database_list` | ❌ |
| 7 | 0.409820 | `acr_registry_repository_list` | ❌ |
| 8 | 0.404640 | `storage_account_create` | ❌ |
| 9 | 0.393989 | `storage_blob_container_create` | ❌ |
| 10 | 0.386144 | `azuremanagedlustre_filesystem_list` | ❌ |
| 11 | 0.359892 | `keyvault_key_list` | ❌ |
| 12 | 0.359465 | `search_service_list` | ❌ |
| 13 | 0.359411 | `subscription_list` | ❌ |
| 14 | 0.356416 | `acr_registry_list` | ❌ |
| 15 | 0.353319 | `foundry_agents_list` | ❌ |
| 16 | 0.349327 | `keyvault_certificate_list` | ❌ |
| 17 | 0.348288 | `appconfig_account_list` | ❌ |
| 18 | 0.333677 | `sql_db_list` | ❌ |
| 19 | 0.333282 | `mysql_database_list` | ❌ |
| 20 | 0.332759 | `monitor_table_list` | ❌ |

---

## Test 324

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625329 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `cosmos_database_container_list` | ❌ |
| 3 | 0.511261 | `storage_account_get` | ❌ |
| 4 | 0.439698 | `storage_account_create` | ❌ |
| 5 | 0.437887 | `cosmos_account_list` | ❌ |
| 6 | 0.429587 | `storage_blob_get` | ❌ |
| 7 | 0.418128 | `storage_blob_container_create` | ❌ |
| 8 | 0.405678 | `azuremanagedlustre_filesystem_list` | ❌ |
| 9 | 0.390261 | `cosmos_database_list` | ❌ |
| 10 | 0.384096 | `acr_registry_repository_list` | ❌ |
| 11 | 0.355955 | `cosmos_database_container_item_query` | ❌ |
| 12 | 0.354374 | `search_service_list` | ❌ |
| 13 | 0.352491 | `appconfig_kv_show` | ❌ |
| 14 | 0.348138 | `appconfig_account_list` | ❌ |
| 15 | 0.347296 | `foundry_agents_list` | ❌ |
| 16 | 0.346936 | `quota_usage_check` | ❌ |
| 17 | 0.345665 | `acr_registry_list` | ❌ |
| 18 | 0.340651 | `subscription_list` | ❌ |
| 19 | 0.336549 | `mysql_server_list` | ❌ |
| 20 | 0.327093 | `monitor_resource_log_query` | ❌ |

---

## Test 325

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612311 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.585549 | `storage_blob_container_get` | ❌ |
| 3 | 0.483687 | `storage_account_get` | ❌ |
| 4 | 0.478015 | `cosmos_database_container_list` | ❌ |
| 5 | 0.434630 | `storage_blob_container_create` | ❌ |
| 6 | 0.420653 | `azuremanagedlustre_filesystem_list` | ❌ |
| 7 | 0.408732 | `storage_account_create` | ❌ |
| 8 | 0.386614 | `appconfig_kv_show` | ❌ |
| 9 | 0.359511 | `cosmos_database_container_item_query` | ❌ |
| 10 | 0.349650 | `cosmos_account_list` | ❌ |
| 11 | 0.345484 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.337993 | `sql_server_show` | ❌ |
| 13 | 0.333818 | `mysql_server_config_get` | ❌ |
| 14 | 0.331007 | `storage_blob_upload` | ❌ |
| 15 | 0.326436 | `monitor_resource_log_query` | ❌ |
| 16 | 0.323111 | `cosmos_database_list` | ❌ |
| 17 | 0.320972 | `quota_usage_check` | ❌ |
| 18 | 0.318099 | `deploy_app_logs_get` | ❌ |
| 19 | 0.303959 | `aks_nodepool_get` | ❌ |
| 20 | 0.303449 | `appconfig_kv_list` | ❌ |

---

## Test 326

**Expected Tool:** `storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661707 | `storage_blob_container_get` | ❌ |
| 2 | 0.661515 | `storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.537535 | `storage_account_get` | ❌ |
| 4 | 0.460657 | `storage_blob_container_create` | ❌ |
| 5 | 0.457038 | `storage_account_create` | ❌ |
| 6 | 0.453696 | `cosmos_database_container_list` | ❌ |
| 7 | 0.370177 | `azuremanagedlustre_filesystem_list` | ❌ |
| 8 | 0.360712 | `cosmos_database_container_item_query` | ❌ |
| 9 | 0.359655 | `aks_cluster_get` | ❌ |
| 10 | 0.358376 | `storage_blob_upload` | ❌ |
| 11 | 0.353461 | `kusto_cluster_get` | ❌ |
| 12 | 0.352984 | `workbooks_show` | ❌ |
| 13 | 0.352671 | `sql_server_show` | ❌ |
| 14 | 0.348551 | `appconfig_kv_show` | ❌ |
| 15 | 0.342979 | `aks_nodepool_get` | ❌ |
| 16 | 0.337010 | `mysql_server_config_get` | ❌ |
| 17 | 0.334138 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.329754 | `monitor_resource_log_query` | ❌ |
| 19 | 0.326300 | `keyvault_secret_get` | ❌ |
| 20 | 0.319366 | `deploy_app_logs_get` | ❌ |

---

## Test 327

**Expected Tool:** `storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592389 | `storage_blob_container_get` | ❌ |
| 2 | 0.579070 | `cosmos_database_container_list` | ❌ |
| 3 | 0.568264 | `storage_blob_get` | ✅ **EXPECTED** |
| 4 | 0.465942 | `storage_account_get` | ❌ |
| 5 | 0.452160 | `cosmos_account_list` | ❌ |
| 6 | 0.415853 | `cosmos_database_list` | ❌ |
| 7 | 0.413280 | `storage_blob_container_create` | ❌ |
| 8 | 0.400483 | `acr_registry_repository_list` | ❌ |
| 9 | 0.394852 | `storage_account_create` | ❌ |
| 10 | 0.379099 | `cosmos_database_container_item_query` | ❌ |
| 11 | 0.367250 | `keyvault_key_list` | ❌ |
| 12 | 0.361689 | `azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.348959 | `keyvault_certificate_list` | ❌ |
| 14 | 0.348821 | `subscription_list` | ❌ |
| 15 | 0.348144 | `keyvault_secret_list` | ❌ |
| 16 | 0.340190 | `monitor_resource_log_query` | ❌ |
| 17 | 0.331545 | `appconfig_kv_list` | ❌ |
| 18 | 0.328193 | `search_service_list` | ❌ |
| 19 | 0.313259 | `sql_db_list` | ❌ |
| 20 | 0.310914 | `monitor_workspace_list` | ❌ |

---

## Test 328

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.569839 | `storage_blob_container_get` | ❌ |
| 2 | 0.549205 | `storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.533515 | `cosmos_database_container_list` | ❌ |
| 4 | 0.449128 | `storage_account_get` | ❌ |
| 5 | 0.433883 | `storage_blob_container_create` | ❌ |
| 6 | 0.397367 | `storage_account_create` | ❌ |
| 7 | 0.395809 | `cosmos_account_list` | ❌ |
| 8 | 0.385242 | `cosmos_database_container_item_query` | ❌ |
| 9 | 0.362337 | `azuremanagedlustre_filesystem_list` | ❌ |
| 10 | 0.353799 | `cosmos_database_list` | ❌ |
| 11 | 0.345263 | `acr_registry_repository_list` | ❌ |
| 12 | 0.342766 | `appconfig_kv_show` | ❌ |
| 13 | 0.339828 | `deploy_app_logs_get` | ❌ |
| 14 | 0.336142 | `monitor_resource_log_query` | ❌ |
| 15 | 0.314069 | `quota_usage_check` | ❌ |
| 16 | 0.308890 | `storage_blob_upload` | ❌ |
| 17 | 0.306951 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 18 | 0.300276 | `acr_registry_list` | ❌ |
| 19 | 0.298968 | `mysql_server_list` | ❌ |
| 20 | 0.294761 | `subscription_list` | ❌ |

---

## Test 329

**Expected Tool:** `storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566287 | `storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.403254 | `storage_blob_get` | ❌ |
| 3 | 0.397534 | `storage_blob_container_get` | ❌ |
| 4 | 0.382123 | `storage_account_create` | ❌ |
| 5 | 0.377255 | `storage_blob_container_create` | ❌ |
| 6 | 0.351920 | `storage_account_get` | ❌ |
| 7 | 0.327416 | `cosmos_database_container_list` | ❌ |
| 8 | 0.324049 | `appconfig_kv_set` | ❌ |
| 9 | 0.284896 | `cosmos_database_container_item_query` | ❌ |
| 10 | 0.284377 | `monitor_resource_log_query` | ❌ |
| 11 | 0.278290 | `keyvault_certificate_import` | ❌ |
| 12 | 0.273638 | `deploy_pipeline_guidance_get` | ❌ |
| 13 | 0.273513 | `azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.272315 | `appconfig_kv_lock_set` | ❌ |
| 15 | 0.257821 | `deploy_app_logs_get` | ❌ |
| 16 | 0.254581 | `workbooks_delete` | ❌ |
| 17 | 0.253430 | `appconfig_kv_show` | ❌ |
| 18 | 0.239522 | `foundry_models_deploy` | ❌ |
| 19 | 0.211052 | `workbooks_create` | ❌ |
| 20 | 0.210171 | `quota_usage_check` | ❌ |

---

## Test 330

**Expected Tool:** `subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576055 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `cosmos_account_list` | ❌ |
| 3 | 0.473844 | `redis_cache_list` | ❌ |
| 4 | 0.471653 | `postgres_server_list` | ❌ |
| 5 | 0.465428 | `eventgrid_subscription_list` | ❌ |
| 6 | 0.452471 | `search_service_list` | ❌ |
| 7 | 0.450973 | `redis_cluster_list` | ❌ |
| 8 | 0.445724 | `grafana_list` | ❌ |
| 9 | 0.431337 | `kusto_cluster_list` | ❌ |
| 10 | 0.430382 | `group_list` | ❌ |
| 11 | 0.422723 | `eventgrid_topic_list` | ❌ |
| 12 | 0.406935 | `appconfig_account_list` | ❌ |
| 13 | 0.395083 | `aks_cluster_list` | ❌ |
| 14 | 0.388737 | `monitor_workspace_list` | ❌ |
| 15 | 0.380636 | `marketplace_product_list` | ❌ |
| 16 | 0.367761 | `storage_account_get` | ❌ |
| 17 | 0.366842 | `loadtesting_testresource_list` | ❌ |
| 18 | 0.355344 | `marketplace_product_get` | ❌ |
| 19 | 0.354245 | `datadog_monitoredresources_list` | ❌ |
| 20 | 0.348524 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 331

**Expected Tool:** `subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405723 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.381238 | `postgres_server_list` | ❌ |
| 3 | 0.380789 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.351864 | `grafana_list` | ❌ |
| 5 | 0.350964 | `redis_cache_list` | ❌ |
| 6 | 0.341813 | `redis_cluster_list` | ❌ |
| 7 | 0.334744 | `eventgrid_topic_list` | ❌ |
| 8 | 0.328109 | `search_service_list` | ❌ |
| 9 | 0.315604 | `kusto_cluster_list` | ❌ |
| 10 | 0.308874 | `appconfig_account_list` | ❌ |
| 11 | 0.303528 | `cosmos_account_list` | ❌ |
| 12 | 0.303367 | `marketplace_product_list` | ❌ |
| 13 | 0.297296 | `group_list` | ❌ |
| 14 | 0.296282 | `monitor_workspace_list` | ❌ |
| 15 | 0.295180 | `marketplace_product_get` | ❌ |
| 16 | 0.285434 | `servicebus_topic_subscription_details` | ❌ |
| 17 | 0.275389 | `loadtesting_testresource_list` | ❌ |
| 18 | 0.274945 | `aks_cluster_list` | ❌ |
| 19 | 0.268988 | `resourcehealth_service-health-events_list` | ❌ |
| 20 | 0.258047 | `datadog_monitoredresources_list` | ❌ |

---

## Test 332

**Expected Tool:** `subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.319958 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.315547 | `marketplace_product_get` | ❌ |
| 3 | 0.307697 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.286726 | `redis_cache_list` | ❌ |
| 5 | 0.282645 | `grafana_list` | ❌ |
| 6 | 0.279702 | `redis_cluster_list` | ❌ |
| 7 | 0.278798 | `postgres_server_list` | ❌ |
| 8 | 0.273758 | `marketplace_product_list` | ❌ |
| 9 | 0.256358 | `kusto_cluster_list` | ❌ |
| 10 | 0.254815 | `servicebus_topic_subscription_details` | ❌ |
| 11 | 0.252879 | `eventgrid_topic_list` | ❌ |
| 12 | 0.252520 | `loadtesting_testresource_list` | ❌ |
| 13 | 0.251683 | `search_service_list` | ❌ |
| 14 | 0.250688 | `resourcehealth_service-health-events_list` | ❌ |
| 15 | 0.233143 | `monitor_workspace_list` | ❌ |
| 16 | 0.230571 | `cosmos_account_list` | ❌ |
| 17 | 0.230324 | `kusto_cluster_get` | ❌ |
| 18 | 0.226446 | `azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.222799 | `appconfig_account_list` | ❌ |
| 20 | 0.218838 | `aks_cluster_list` | ❌ |

---

## Test 333

**Expected Tool:** `subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.403229 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.370692 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.354553 | `redis_cache_list` | ❌ |
| 4 | 0.342318 | `redis_cluster_list` | ❌ |
| 5 | 0.340339 | `grafana_list` | ❌ |
| 6 | 0.336798 | `postgres_server_list` | ❌ |
| 7 | 0.311939 | `search_service_list` | ❌ |
| 8 | 0.311109 | `marketplace_product_list` | ❌ |
| 9 | 0.305150 | `marketplace_product_get` | ❌ |
| 10 | 0.304965 | `kusto_cluster_list` | ❌ |
| 11 | 0.302271 | `eventgrid_topic_list` | ❌ |
| 12 | 0.300478 | `servicebus_topic_subscription_details` | ❌ |
| 13 | 0.294080 | `monitor_workspace_list` | ❌ |
| 14 | 0.291826 | `cosmos_account_list` | ❌ |
| 15 | 0.282310 | `loadtesting_testresource_list` | ❌ |
| 16 | 0.281294 | `appconfig_account_list` | ❌ |
| 17 | 0.273270 | `resourcehealth_service-health-events_list` | ❌ |
| 18 | 0.269972 | `group_list` | ❌ |
| 19 | 0.258577 | `aks_cluster_list` | ❌ |
| 20 | 0.233362 | `azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 334

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625240 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.605047 | `get_bestpractices_get` | ❌ |
| 4 | 0.482936 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466161 | `deploy_plan_get` | ❌ |
| 6 | 0.431102 | `cloudarchitect_design` | ❌ |
| 7 | 0.389080 | `deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.386480 | `quota_usage_check` | ❌ |
| 9 | 0.372605 | `deploy_app_logs_get` | ❌ |
| 10 | 0.369169 | `applens_resource_diagnose` | ❌ |
| 11 | 0.362323 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 12 | 0.354086 | `quota_region_availability_list` | ❌ |
| 13 | 0.339022 | `mysql_server_list` | ❌ |
| 14 | 0.333150 | `resourcehealth_availability-status_get` | ❌ |
| 15 | 0.312592 | `mysql_server_config_get` | ❌ |
| 16 | 0.310275 | `mysql_table_schema_get` | ❌ |
| 17 | 0.305259 | `mysql_database_query` | ❌ |
| 18 | 0.303853 | `resourcehealth_availability-status_list` | ❌ |
| 19 | 0.302307 | `storage_account_get` | ❌ |
| 20 | 0.300716 | `storage_blob_container_get` | ❌ |

---

## Test 335

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `get_bestpractices_get` | ❌ |
| 3 | 0.509957 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.473596 | `keyvault_secret_get` | ❌ |
| 5 | 0.444297 | `deploy_pipeline_guidance_get` | ❌ |
| 6 | 0.421559 | `keyvault_key_get` | ❌ |
| 7 | 0.395752 | `keyvault_secret_list` | ❌ |
| 8 | 0.388926 | `keyvault_secret_create` | ❌ |
| 9 | 0.385668 | `keyvault_certificate_create` | ❌ |
| 10 | 0.379390 | `keyvault_key_list` | ❌ |
| 11 | 0.304912 | `quota_usage_check` | ❌ |
| 12 | 0.304137 | `mysql_database_query` | ❌ |
| 13 | 0.300776 | `quota_region_availability_list` | ❌ |
| 14 | 0.292743 | `mysql_server_list` | ❌ |
| 15 | 0.285517 | `sql_db_create` | ❌ |
| 16 | 0.281261 | `storage_account_get` | ❌ |
| 17 | 0.279035 | `storage_account_create` | ❌ |
| 18 | 0.278638 | `mysql_server_config_get` | ❌ |
| 19 | 0.276655 | `storage_blob_container_get` | ❌ |
| 20 | 0.274538 | `subscription_list` | ❌ |

---

## Test 336

**Expected Tool:** `virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711969 | `virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659763 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.566615 | `kusto_cluster_list` | ❌ |
| 4 | 0.548888 | `search_service_list` | ❌ |
| 5 | 0.536542 | `redis_cluster_list` | ❌ |
| 6 | 0.535777 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 7 | 0.527948 | `postgres_server_list` | ❌ |
| 8 | 0.527095 | `aks_nodepool_list` | ❌ |
| 9 | 0.525883 | `aks_cluster_list` | ❌ |
| 10 | 0.525637 | `sql_elastic-pool_list` | ❌ |
| 11 | 0.506601 | `redis_cache_list` | ❌ |
| 12 | 0.505116 | `subscription_list` | ❌ |
| 13 | 0.496297 | `cosmos_account_list` | ❌ |
| 14 | 0.495490 | `grafana_list` | ❌ |
| 15 | 0.492515 | `monitor_workspace_list` | ❌ |
| 16 | 0.476824 | `group_list` | ❌ |
| 17 | 0.465569 | `aks_nodepool_get` | ❌ |
| 18 | 0.463074 | `eventgrid_topic_list` | ❌ |
| 19 | 0.460429 | `acr_registry_list` | ❌ |
| 20 | 0.459250 | `appconfig_account_list` | ❌ |

---

## Test 337

**Expected Tool:** `virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714553 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.439611 | `aks_nodepool_list` | ❌ |
| 5 | 0.402909 | `aks_nodepool_get` | ❌ |
| 6 | 0.393721 | `sql_elastic-pool_list` | ❌ |
| 7 | 0.364696 | `postgres_server_list` | ❌ |
| 8 | 0.362307 | `search_service_list` | ❌ |
| 9 | 0.359417 | `foundry_agents_list` | ❌ |
| 10 | 0.344853 | `mysql_server_list` | ❌ |
| 11 | 0.337530 | `redis_cluster_list` | ❌ |
| 12 | 0.335295 | `monitor_workspace_list` | ❌ |
| 13 | 0.333517 | `kusto_cluster_list` | ❌ |
| 14 | 0.330904 | `aks_cluster_list` | ❌ |
| 15 | 0.324603 | `sql_server_list` | ❌ |
| 16 | 0.311262 | `grafana_list` | ❌ |
| 17 | 0.310899 | `keyvault_secret_list` | ❌ |
| 18 | 0.308168 | `azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.306384 | `keyvault_certificate_list` | ❌ |
| 20 | 0.302706 | `cosmos_account_list` | ❌ |

---

## Test 338

**Expected Tool:** `virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812787 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.659212 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501167 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.356479 | `aks_nodepool_list` | ❌ |
| 5 | 0.336385 | `monitor_workspace_list` | ❌ |
| 6 | 0.327423 | `sql_elastic-pool_list` | ❌ |
| 7 | 0.324603 | `subscription_list` | ❌ |
| 8 | 0.324289 | `search_service_list` | ❌ |
| 9 | 0.316295 | `postgres_server_list` | ❌ |
| 10 | 0.315778 | `loadtesting_testrun_list` | ❌ |
| 11 | 0.307927 | `aks_nodepool_get` | ❌ |
| 12 | 0.305300 | `monitor_table_list` | ❌ |
| 13 | 0.305186 | `aks_cluster_list` | ❌ |
| 14 | 0.304414 | `workbooks_list` | ❌ |
| 15 | 0.300364 | `eventgrid_subscription_list` | ❌ |
| 16 | 0.297320 | `foundry_agents_list` | ❌ |
| 17 | 0.295899 | `grafana_list` | ❌ |
| 18 | 0.284934 | `azuremanagedlustre_filesystem_list` | ❌ |
| 19 | 0.278813 | `cosmos_account_list` | ❌ |
| 20 | 0.278222 | `datadog_monitoredresources_list` | ❌ |

---

## Test 339

**Expected Tool:** `workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552212 | `workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.433162 | `workbooks_update` | ❌ |
| 3 | 0.361372 | `workbooks_show` | ❌ |
| 4 | 0.361364 | `workbooks_delete` | ❌ |
| 5 | 0.328113 | `workbooks_list` | ❌ |
| 6 | 0.188088 | `loadtesting_testresource_create` | ❌ |
| 7 | 0.178250 | `keyvault_secret_create` | ❌ |
| 8 | 0.178035 | `keyvault_key_create` | ❌ |
| 9 | 0.176903 | `keyvault_certificate_create` | ❌ |
| 10 | 0.172751 | `monitor_table_list` | ❌ |
| 11 | 0.169440 | `grafana_list` | ❌ |
| 12 | 0.164006 | `sql_db_create` | ❌ |
| 13 | 0.153950 | `storage_account_create` | ❌ |
| 14 | 0.148932 | `loadtesting_test_create` | ❌ |
| 15 | 0.147365 | `monitor_workspace_list` | ❌ |
| 16 | 0.143733 | `sql_server_create` | ❌ |
| 17 | 0.130516 | `loadtesting_testrun_create` | ❌ |
| 18 | 0.130339 | `deploy_pipeline_guidance_get` | ❌ |
| 19 | 0.113882 | `deploy_plan_get` | ❌ |
| 20 | 0.111941 | `extension_azqr` | ❌ |

---

## Test 340

**Expected Tool:** `workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621310 | `workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.518668 | `workbooks_show` | ❌ |
| 3 | 0.432454 | `workbooks_create` | ❌ |
| 4 | 0.425505 | `workbooks_list` | ❌ |
| 5 | 0.390355 | `workbooks_update` | ❌ |
| 6 | 0.273939 | `grafana_list` | ❌ |
| 7 | 0.256795 | `sql_server_firewall-rule_delete` | ❌ |
| 8 | 0.248002 | `sql_db_delete` | ❌ |
| 9 | 0.242993 | `sql_server_delete` | ❌ |
| 10 | 0.198585 | `appconfig_kv_delete` | ❌ |
| 11 | 0.190455 | `monitor_resource_log_query` | ❌ |
| 12 | 0.186665 | `quota_region_availability_list` | ❌ |
| 13 | 0.148882 | `extension_azqr` | ❌ |
| 14 | 0.145156 | `loadtesting_testresource_list` | ❌ |
| 15 | 0.132504 | `datadog_monitoredresources_list` | ❌ |
| 16 | 0.131856 | `group_list` | ❌ |
| 17 | 0.122450 | `loadtesting_test_get` | ❌ |
| 18 | 0.119301 | `loadtesting_testresource_create` | ❌ |
| 19 | 0.114388 | `foundry_agents_connect` | ❌ |
| 20 | 0.110003 | `applicationinsights_recommendation_list` | ❌ |

---

## Test 341

**Expected Tool:** `workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772431 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `workbooks_create` | ❌ |
| 3 | 0.532493 | `workbooks_show` | ❌ |
| 4 | 0.516739 | `grafana_list` | ❌ |
| 5 | 0.488597 | `group_list` | ❌ |
| 6 | 0.487920 | `workbooks_delete` | ❌ |
| 7 | 0.459976 | `datadog_monitoredresources_list` | ❌ |
| 8 | 0.454210 | `monitor_workspace_list` | ❌ |
| 9 | 0.439945 | `resourcehealth_availability-status_list` | ❌ |
| 10 | 0.428781 | `mysql_server_list` | ❌ |
| 11 | 0.416446 | `monitor_table_list` | ❌ |
| 12 | 0.413409 | `sql_db_list` | ❌ |
| 13 | 0.405949 | `sql_server_list` | ❌ |
| 14 | 0.405913 | `loadtesting_testresource_list` | ❌ |
| 15 | 0.399758 | `acr_registry_repository_list` | ❌ |
| 16 | 0.365302 | `azuremanagedlustre_filesystem_list` | ❌ |
| 17 | 0.362685 | `acr_registry_list` | ❌ |
| 18 | 0.356739 | `functionapp_get` | ❌ |
| 19 | 0.352940 | `cosmos_database_list` | ❌ |
| 20 | 0.349674 | `cosmos_account_list` | ❌ |

---

## Test 342

**Expected Tool:** `workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570259 | `workbooks_create` | ❌ |
| 3 | 0.539919 | `workbooks_show` | ❌ |
| 4 | 0.485504 | `workbooks_delete` | ❌ |
| 5 | 0.472378 | `grafana_list` | ❌ |
| 6 | 0.428025 | `monitor_workspace_list` | ❌ |
| 7 | 0.425426 | `datadog_monitoredresources_list` | ❌ |
| 8 | 0.422785 | `resourcehealth_availability-status_list` | ❌ |
| 9 | 0.421659 | `group_list` | ❌ |
| 10 | 0.412390 | `mysql_server_list` | ❌ |
| 11 | 0.392298 | `loadtesting_testresource_list` | ❌ |
| 12 | 0.380991 | `azuremanagedlustre_filesystem_list` | ❌ |
| 13 | 0.380399 | `sql_server_list` | ❌ |
| 14 | 0.371128 | `redis_cluster_list` | ❌ |
| 15 | 0.363744 | `sql_db_list` | ❌ |
| 16 | 0.350839 | `acr_registry_repository_list` | ❌ |
| 17 | 0.338282 | `acr_registry_list` | ❌ |
| 18 | 0.337786 | `functionapp_get` | ❌ |
| 19 | 0.334580 | `extension_azqr` | ❌ |
| 20 | 0.333154 | `eventgrid_subscription_list` | ❌ |

---

## Test 343

**Expected Tool:** `workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.697464 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498390 | `workbooks_create` | ❌ |
| 3 | 0.494708 | `workbooks_list` | ❌ |
| 4 | 0.452348 | `workbooks_delete` | ❌ |
| 5 | 0.419105 | `workbooks_update` | ❌ |
| 6 | 0.353546 | `grafana_list` | ❌ |
| 7 | 0.277807 | `quota_region_availability_list` | ❌ |
| 8 | 0.264638 | `marketplace_product_get` | ❌ |
| 9 | 0.256678 | `quota_usage_check` | ❌ |
| 10 | 0.249917 | `resourcehealth_availability-status_get` | ❌ |
| 11 | 0.236741 | `monitor_resource_log_query` | ❌ |
| 12 | 0.225294 | `loadtesting_test_get` | ❌ |
| 13 | 0.219007 | `loadtesting_testresource_list` | ❌ |
| 14 | 0.207693 | `datadog_monitoredresources_list` | ❌ |
| 15 | 0.197186 | `foundry_knowledge_index_schema` | ❌ |
| 16 | 0.195391 | `group_list` | ❌ |
| 17 | 0.189914 | `loadtesting_testrun_get` | ❌ |
| 18 | 0.189657 | `extension_azqr` | ❌ |
| 19 | 0.187682 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 20 | 0.187564 | `foundry_knowledge_index_list` | ❌ |

---

## Test 344

**Expected Tool:** `workbooks_show`  
**Prompt:** Show me the workbook with display name <workbook_display_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469542 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.455158 | `workbooks_create` | ❌ |
| 3 | 0.437638 | `workbooks_update` | ❌ |
| 4 | 0.424092 | `workbooks_list` | ❌ |
| 5 | 0.366057 | `workbooks_delete` | ❌ |
| 6 | 0.292898 | `grafana_list` | ❌ |
| 7 | 0.266680 | `monitor_table_list` | ❌ |
| 8 | 0.239907 | `monitor_workspace_list` | ❌ |
| 9 | 0.227383 | `monitor_table_type_list` | ❌ |
| 10 | 0.176481 | `role_assignment_list` | ❌ |
| 11 | 0.175814 | `appconfig_kv_show` | ❌ |
| 12 | 0.174513 | `loadtesting_testrun_update` | ❌ |
| 13 | 0.168191 | `azuremanagedlustre_filesystem_list` | ❌ |
| 14 | 0.165774 | `cosmos_database_list` | ❌ |
| 15 | 0.154760 | `cosmos_database_container_list` | ❌ |
| 16 | 0.152535 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 17 | 0.149678 | `cosmos_account_list` | ❌ |
| 18 | 0.146054 | `kusto_table_schema` | ❌ |
| 19 | 0.141985 | `loadtesting_testrun_get` | ❌ |
| 20 | 0.141559 | `foundry_models_list` | ❌ |

---

## Test 345

**Expected Tool:** `workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469915 | `workbooks_update` | ✅ **EXPECTED** |
| 2 | 0.382651 | `workbooks_create` | ❌ |
| 3 | 0.362450 | `workbooks_show` | ❌ |
| 4 | 0.349689 | `workbooks_delete` | ❌ |
| 5 | 0.276727 | `loadtesting_testrun_update` | ❌ |
| 6 | 0.262873 | `workbooks_list` | ❌ |
| 7 | 0.174311 | `sql_db_update` | ❌ |
| 8 | 0.170118 | `grafana_list` | ❌ |
| 9 | 0.148730 | `mysql_server_param_set` | ❌ |
| 10 | 0.142404 | `deploy_pipeline_guidance_get` | ❌ |
| 11 | 0.142186 | `loadtesting_testrun_create` | ❌ |
| 12 | 0.138354 | `appconfig_kv_set` | ❌ |
| 13 | 0.135987 | `loadtesting_testresource_create` | ❌ |
| 14 | 0.133097 | `foundry_agents_evaluate` | ❌ |
| 15 | 0.131042 | `postgres_database_query` | ❌ |
| 16 | 0.129973 | `postgres_server_param_set` | ❌ |
| 17 | 0.129630 | `deploy_iac_rules_get` | ❌ |
| 18 | 0.127559 | `keyvault_certificate_import` | ❌ |
| 19 | 0.124550 | `keyvault_secret_create` | ❌ |
| 20 | 0.116780 | `appservice_database_add` | ❌ |

---

## Test 346

**Expected Tool:** `bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485925 | `deploy_iac_rules_get` | ❌ |
| 2 | 0.448373 | `get_bestpractices_get` | ❌ |
| 3 | 0.440302 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.432777 | `deploy_plan_get` | ❌ |
| 5 | 0.430675 | `bicepschema_get` | ✅ **EXPECTED** |
| 6 | 0.400985 | `foundry_models_deploy` | ❌ |
| 7 | 0.398046 | `deploy_architecture_diagram_generate` | ❌ |
| 8 | 0.393793 | `foundry_agents_connect` | ❌ |
| 9 | 0.391625 | `azureterraformbestpractices_get` | ❌ |
| 10 | 0.385433 | `foundry_agents_list` | ❌ |
| 11 | 0.372097 | `search_service_list` | ❌ |
| 12 | 0.325716 | `search_index_query` | ❌ |
| 13 | 0.324857 | `search_index_get` | ❌ |
| 14 | 0.317232 | `sql_db_create` | ❌ |
| 15 | 0.303183 | `quota_usage_check` | ❌ |
| 16 | 0.291291 | `storage_account_create` | ❌ |
| 17 | 0.281487 | `mysql_server_list` | ❌ |
| 18 | 0.279983 | `workbooks_delete` | ❌ |
| 19 | 0.274793 | `resourcehealth_availability-status_get` | ❌ |
| 20 | 0.270531 | `storage_blob_container_create` | ❌ |

---

## Test 347

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.349336 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290902 | `storage_blob_upload` | ❌ |
| 3 | 0.254991 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.221349 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.217623 | `azuremanagedlustre_filesystem_list` | ❌ |
| 6 | 0.216162 | `azuremanagedlustre_filesystem_required-subnet-size` | ❌ |
| 7 | 0.191391 | `storage_blob_container_create` | ❌ |
| 8 | 0.191096 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 9 | 0.178245 | `deploy_plan_get` | ❌ |
| 10 | 0.175826 | `deploy_iac_rules_get` | ❌ |
| 11 | 0.136643 | `storage_blob_get` | ❌ |
| 12 | 0.135768 | `get_bestpractices_get` | ❌ |
| 13 | 0.132826 | `storage_account_create` | ❌ |
| 14 | 0.130037 | `foundry_models_deploy` | ❌ |
| 15 | 0.118383 | `quota_usage_check` | ❌ |
| 16 | 0.115876 | `marketplace_product_get` | ❌ |
| 17 | 0.111376 | `storage_blob_container_get` | ❌ |
| 18 | 0.106651 | `mysql_database_query` | ❌ |
| 19 | 0.104162 | `storage_account_get` | ❌ |
| 20 | 0.100892 | `mysql_table_schema_get` | ❌ |

---

## Test 348

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.290270 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.267683 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258160 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.225622 | `deploy_plan_get` | ❌ |
| 5 | 0.215748 | `get_bestpractices_get` | ❌ |
| 6 | 0.207322 | `deploy_iac_rules_get` | ❌ |
| 7 | 0.195387 | `storage_account_create` | ❌ |
| 8 | 0.189124 | `applens_resource_diagnose` | ❌ |
| 9 | 0.179452 | `loadtesting_testresource_create` | ❌ |
| 10 | 0.170231 | `foundry_agents_connect` | ❌ |
| 11 | 0.168850 | `azureterraformbestpractices_get` | ❌ |
| 12 | 0.163694 | `mysql_database_query` | ❌ |
| 13 | 0.163615 | `storage_blob_container_create` | ❌ |
| 14 | 0.162154 | `sql_server_create` | ❌ |
| 15 | 0.160743 | `quota_usage_check` | ❌ |
| 16 | 0.154249 | `mysql_server_list` | ❌ |
| 17 | 0.152324 | `sql_db_create` | ❌ |
| 18 | 0.145124 | `quota_region_availability_list` | ❌ |
| 19 | 0.139758 | `storage_account_get` | ❌ |
| 20 | 0.135749 | `marketplace_product_get` | ❌ |

---

## Test 349

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.299640 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242581 | `deploy_plan_get` | ❌ |
| 5 | 0.218054 | `deploy_iac_rules_get` | ❌ |
| 6 | 0.213173 | `get_bestpractices_get` | ❌ |
| 7 | 0.179248 | `deploy_app_logs_get` | ❌ |
| 8 | 0.169691 | `marketplace_product_get` | ❌ |
| 9 | 0.164328 | `mysql_server_list` | ❌ |
| 10 | 0.156442 | `appconfig_account_list` | ❌ |
| 11 | 0.156119 | `azureterraformbestpractices_get` | ❌ |
| 12 | 0.151368 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 13 | 0.142854 | `marketplace_product_list` | ❌ |
| 14 | 0.139970 | `storage_blob_container_create` | ❌ |
| 15 | 0.138067 | `storage_account_create` | ❌ |
| 16 | 0.132355 | `mysql_database_query` | ❌ |
| 17 | 0.130132 | `quota_usage_check` | ❌ |
| 18 | 0.123936 | `storage_blob_upload` | ❌ |
| 19 | 0.119586 | `workbooks_create` | ❌ |
| 20 | 0.114994 | `mysql_table_schema_get` | ❌ |

---

## Test 350

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.420259 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.352797 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.323920 | `storage_blob_upload` | ❌ |
| 5 | 0.310563 | `deploy_plan_get` | ❌ |
| 6 | 0.306967 | `storage_account_create` | ❌ |
| 7 | 0.304531 | `resourcehealth_service-health-events_list` | ❌ |
| 8 | 0.300392 | `storage_blob_container_create` | ❌ |
| 9 | 0.299412 | `azuremanagedlustre_filesystem_sku_get` | ❌ |
| 10 | 0.298989 | `get_bestpractices_get` | ❌ |
| 11 | 0.293806 | `azuremanagedlustre_filesystem_list` | ❌ |
| 12 | 0.292390 | `applens_resource_diagnose` | ❌ |
| 13 | 0.291868 | `deploy_iac_rules_get` | ❌ |
| 14 | 0.281671 | `storage_blob_container_get` | ❌ |
| 15 | 0.275653 | `storage_blob_get` | ❌ |
| 16 | 0.275550 | `storage_account_get` | ❌ |
| 17 | 0.272694 | `deploy_app_logs_get` | ❌ |
| 18 | 0.261446 | `quota_usage_check` | ❌ |
| 19 | 0.259814 | `search_service_list` | ❌ |
| 20 | 0.258445 | `monitor_resource_log_query` | ❌ |

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

