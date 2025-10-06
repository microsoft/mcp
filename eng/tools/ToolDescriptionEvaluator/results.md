# Tool Selection Analysis Setup

**Setup completed:** 2025-10-06 17:16:03  
**Tool count:** 149  
**Database setup time:** 2.2042693s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-10-06 17:16:03  
**Tool count:** 149  

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
- [Test 13: azmcp_foundry_openai_create-completion](#test-13)
- [Test 14: azmcp_search_index_get](#test-14)
- [Test 15: azmcp_search_index_get](#test-15)
- [Test 16: azmcp_search_index_get](#test-16)
- [Test 17: azmcp_search_index_query](#test-17)
- [Test 18: azmcp_search_service_list](#test-18)
- [Test 19: azmcp_search_service_list](#test-19)
- [Test 20: azmcp_search_service_list](#test-20)
- [Test 21: azmcp_speech_stt_recognize](#test-21)
- [Test 22: azmcp_speech_stt_recognize](#test-22)
- [Test 23: azmcp_speech_stt_recognize](#test-23)
- [Test 24: azmcp_speech_stt_recognize](#test-24)
- [Test 25: azmcp_speech_stt_recognize](#test-25)
- [Test 26: azmcp_speech_stt_recognize](#test-26)
- [Test 27: azmcp_speech_stt_recognize](#test-27)
- [Test 28: azmcp_speech_stt_recognize](#test-28)
- [Test 29: azmcp_speech_stt_recognize](#test-29)
- [Test 30: azmcp_speech_stt_recognize](#test-30)
- [Test 31: azmcp_appconfig_account_list](#test-31)
- [Test 32: azmcp_appconfig_account_list](#test-32)
- [Test 33: azmcp_appconfig_account_list](#test-33)
- [Test 34: azmcp_appconfig_kv_delete](#test-34)
- [Test 35: azmcp_appconfig_kv_get](#test-35)
- [Test 36: azmcp_appconfig_kv_get](#test-36)
- [Test 37: azmcp_appconfig_kv_get](#test-37)
- [Test 38: azmcp_appconfig_kv_get](#test-38)
- [Test 39: azmcp_appconfig_kv_lock_set](#test-39)
- [Test 40: azmcp_appconfig_kv_lock_set](#test-40)
- [Test 41: azmcp_appconfig_kv_set](#test-41)
- [Test 42: azmcp_applens_resource_diagnose](#test-42)
- [Test 43: azmcp_applens_resource_diagnose](#test-43)
- [Test 44: azmcp_applens_resource_diagnose](#test-44)
- [Test 45: azmcp_appservice_database_add](#test-45)
- [Test 46: azmcp_appservice_database_add](#test-46)
- [Test 47: azmcp_appservice_database_add](#test-47)
- [Test 48: azmcp_appservice_database_add](#test-48)
- [Test 49: azmcp_appservice_database_add](#test-49)
- [Test 50: azmcp_appservice_database_add](#test-50)
- [Test 51: azmcp_appservice_database_add](#test-51)
- [Test 52: azmcp_appservice_database_add](#test-52)
- [Test 53: azmcp_appservice_database_add](#test-53)
- [Test 54: azmcp_applicationinsights_recommendation_list](#test-54)
- [Test 55: azmcp_applicationinsights_recommendation_list](#test-55)
- [Test 56: azmcp_applicationinsights_recommendation_list](#test-56)
- [Test 57: azmcp_applicationinsights_recommendation_list](#test-57)
- [Test 58: azmcp_acr_registry_list](#test-58)
- [Test 59: azmcp_acr_registry_list](#test-59)
- [Test 60: azmcp_acr_registry_list](#test-60)
- [Test 61: azmcp_acr_registry_list](#test-61)
- [Test 62: azmcp_acr_registry_list](#test-62)
- [Test 63: azmcp_acr_registry_repository_list](#test-63)
- [Test 64: azmcp_acr_registry_repository_list](#test-64)
- [Test 65: azmcp_acr_registry_repository_list](#test-65)
- [Test 66: azmcp_acr_registry_repository_list](#test-66)
- [Test 67: azmcp_communication_sms_send](#test-67)
- [Test 68: azmcp_communication_sms_send](#test-68)
- [Test 69: azmcp_communication_sms_send](#test-69)
- [Test 70: azmcp_communication_sms_send](#test-70)
- [Test 71: azmcp_communication_sms_send](#test-71)
- [Test 72: azmcp_communication_sms_send](#test-72)
- [Test 73: azmcp_communication_sms_send](#test-73)
- [Test 74: azmcp_communication_sms_send](#test-74)
- [Test 75: azmcp_confidentialledger_entries_append](#test-75)
- [Test 76: azmcp_confidentialledger_entries_append](#test-76)
- [Test 77: azmcp_confidentialledger_entries_append](#test-77)
- [Test 78: azmcp_confidentialledger_entries_append](#test-78)
- [Test 79: azmcp_confidentialledger_entries_append](#test-79)
- [Test 80: azmcp_cosmos_account_list](#test-80)
- [Test 81: azmcp_cosmos_account_list](#test-81)
- [Test 82: azmcp_cosmos_account_list](#test-82)
- [Test 83: azmcp_cosmos_database_container_item_query](#test-83)
- [Test 84: azmcp_cosmos_database_container_list](#test-84)
- [Test 85: azmcp_cosmos_database_container_list](#test-85)
- [Test 86: azmcp_cosmos_database_list](#test-86)
- [Test 87: azmcp_cosmos_database_list](#test-87)
- [Test 88: azmcp_kusto_cluster_get](#test-88)
- [Test 89: azmcp_kusto_cluster_list](#test-89)
- [Test 90: azmcp_kusto_cluster_list](#test-90)
- [Test 91: azmcp_kusto_cluster_list](#test-91)
- [Test 92: azmcp_kusto_database_list](#test-92)
- [Test 93: azmcp_kusto_database_list](#test-93)
- [Test 94: azmcp_kusto_query](#test-94)
- [Test 95: azmcp_kusto_sample](#test-95)
- [Test 96: azmcp_kusto_table_list](#test-96)
- [Test 97: azmcp_kusto_table_list](#test-97)
- [Test 98: azmcp_kusto_table_schema](#test-98)
- [Test 99: azmcp_mysql_database_list](#test-99)
- [Test 100: azmcp_mysql_database_list](#test-100)
- [Test 101: azmcp_mysql_database_query](#test-101)
- [Test 102: azmcp_mysql_server_config_get](#test-102)
- [Test 103: azmcp_mysql_server_list](#test-103)
- [Test 104: azmcp_mysql_server_list](#test-104)
- [Test 105: azmcp_mysql_server_list](#test-105)
- [Test 106: azmcp_mysql_server_param_get](#test-106)
- [Test 107: azmcp_mysql_server_param_set](#test-107)
- [Test 108: azmcp_mysql_table_list](#test-108)
- [Test 109: azmcp_mysql_table_list](#test-109)
- [Test 110: azmcp_mysql_table_schema_get](#test-110)
- [Test 111: azmcp_postgres_database_list](#test-111)
- [Test 112: azmcp_postgres_database_list](#test-112)
- [Test 113: azmcp_postgres_database_query](#test-113)
- [Test 114: azmcp_postgres_server_config_get](#test-114)
- [Test 115: azmcp_postgres_server_list](#test-115)
- [Test 116: azmcp_postgres_server_list](#test-116)
- [Test 117: azmcp_postgres_server_list](#test-117)
- [Test 118: azmcp_postgres_server_param_get](#test-118)
- [Test 119: azmcp_postgres_server_param_set](#test-119)
- [Test 120: azmcp_postgres_table_list](#test-120)
- [Test 121: azmcp_postgres_table_list](#test-121)
- [Test 122: azmcp_postgres_table_schema_get](#test-122)
- [Test 123: azmcp_deploy_app_logs_get](#test-123)
- [Test 124: azmcp_deploy_architecture_diagram_generate](#test-124)
- [Test 125: azmcp_deploy_iac_rules_get](#test-125)
- [Test 126: azmcp_deploy_pipeline_guidance_get](#test-126)
- [Test 127: azmcp_deploy_plan_get](#test-127)
- [Test 128: azmcp_eventgrid_events_publish](#test-128)
- [Test 129: azmcp_eventgrid_events_publish](#test-129)
- [Test 130: azmcp_eventgrid_events_publish](#test-130)
- [Test 131: azmcp_eventgrid_topic_list](#test-131)
- [Test 132: azmcp_eventgrid_topic_list](#test-132)
- [Test 133: azmcp_eventgrid_topic_list](#test-133)
- [Test 134: azmcp_eventgrid_topic_list](#test-134)
- [Test 135: azmcp_eventgrid_subscription_list](#test-135)
- [Test 136: azmcp_eventgrid_subscription_list](#test-136)
- [Test 137: azmcp_eventgrid_subscription_list](#test-137)
- [Test 138: azmcp_eventgrid_subscription_list](#test-138)
- [Test 139: azmcp_eventgrid_subscription_list](#test-139)
- [Test 140: azmcp_eventgrid_subscription_list](#test-140)
- [Test 141: azmcp_eventgrid_subscription_list](#test-141)
- [Test 142: azmcp_eventhubs_namespace_get](#test-142)
- [Test 143: azmcp_eventhubs_namespace_get](#test-143)
- [Test 144: azmcp_functionapp_get](#test-144)
- [Test 145: azmcp_functionapp_get](#test-145)
- [Test 146: azmcp_functionapp_get](#test-146)
- [Test 147: azmcp_functionapp_get](#test-147)
- [Test 148: azmcp_functionapp_get](#test-148)
- [Test 149: azmcp_functionapp_get](#test-149)
- [Test 150: azmcp_functionapp_get](#test-150)
- [Test 151: azmcp_functionapp_get](#test-151)
- [Test 152: azmcp_functionapp_get](#test-152)
- [Test 153: azmcp_functionapp_get](#test-153)
- [Test 154: azmcp_functionapp_get](#test-154)
- [Test 155: azmcp_functionapp_get](#test-155)
- [Test 156: azmcp_keyvault_admin_settings_get](#test-156)
- [Test 157: azmcp_keyvault_admin_settings_get](#test-157)
- [Test 158: azmcp_keyvault_admin_settings_get](#test-158)
- [Test 159: azmcp_keyvault_certificate_create](#test-159)
- [Test 160: azmcp_keyvault_certificate_create](#test-160)
- [Test 161: azmcp_keyvault_certificate_create](#test-161)
- [Test 162: azmcp_keyvault_certificate_create](#test-162)
- [Test 163: azmcp_keyvault_certificate_create](#test-163)
- [Test 164: azmcp_keyvault_certificate_get](#test-164)
- [Test 165: azmcp_keyvault_certificate_get](#test-165)
- [Test 166: azmcp_keyvault_certificate_get](#test-166)
- [Test 167: azmcp_keyvault_certificate_get](#test-167)
- [Test 168: azmcp_keyvault_certificate_get](#test-168)
- [Test 169: azmcp_keyvault_certificate_import](#test-169)
- [Test 170: azmcp_keyvault_certificate_import](#test-170)
- [Test 171: azmcp_keyvault_certificate_import](#test-171)
- [Test 172: azmcp_keyvault_certificate_import](#test-172)
- [Test 173: azmcp_keyvault_certificate_import](#test-173)
- [Test 174: azmcp_keyvault_certificate_list](#test-174)
- [Test 175: azmcp_keyvault_certificate_list](#test-175)
- [Test 176: azmcp_keyvault_certificate_list](#test-176)
- [Test 177: azmcp_keyvault_certificate_list](#test-177)
- [Test 178: azmcp_keyvault_certificate_list](#test-178)
- [Test 179: azmcp_keyvault_certificate_list](#test-179)
- [Test 180: azmcp_keyvault_key_create](#test-180)
- [Test 181: azmcp_keyvault_key_create](#test-181)
- [Test 182: azmcp_keyvault_key_create](#test-182)
- [Test 183: azmcp_keyvault_key_create](#test-183)
- [Test 184: azmcp_keyvault_key_create](#test-184)
- [Test 185: azmcp_keyvault_key_get](#test-185)
- [Test 186: azmcp_keyvault_key_get](#test-186)
- [Test 187: azmcp_keyvault_key_get](#test-187)
- [Test 188: azmcp_keyvault_key_get](#test-188)
- [Test 189: azmcp_keyvault_key_get](#test-189)
- [Test 190: azmcp_keyvault_key_list](#test-190)
- [Test 191: azmcp_keyvault_key_list](#test-191)
- [Test 192: azmcp_keyvault_key_list](#test-192)
- [Test 193: azmcp_keyvault_key_list](#test-193)
- [Test 194: azmcp_keyvault_key_list](#test-194)
- [Test 195: azmcp_keyvault_key_list](#test-195)
- [Test 196: azmcp_keyvault_secret_create](#test-196)
- [Test 197: azmcp_keyvault_secret_create](#test-197)
- [Test 198: azmcp_keyvault_secret_create](#test-198)
- [Test 199: azmcp_keyvault_secret_create](#test-199)
- [Test 200: azmcp_keyvault_secret_create](#test-200)
- [Test 201: azmcp_keyvault_secret_get](#test-201)
- [Test 202: azmcp_keyvault_secret_get](#test-202)
- [Test 203: azmcp_keyvault_secret_get](#test-203)
- [Test 204: azmcp_keyvault_secret_get](#test-204)
- [Test 205: azmcp_keyvault_secret_get](#test-205)
- [Test 206: azmcp_keyvault_secret_list](#test-206)
- [Test 207: azmcp_keyvault_secret_list](#test-207)
- [Test 208: azmcp_keyvault_secret_list](#test-208)
- [Test 209: azmcp_keyvault_secret_list](#test-209)
- [Test 210: azmcp_keyvault_secret_list](#test-210)
- [Test 211: azmcp_keyvault_secret_list](#test-211)
- [Test 212: azmcp_aks_cluster_get](#test-212)
- [Test 213: azmcp_aks_cluster_get](#test-213)
- [Test 214: azmcp_aks_cluster_get](#test-214)
- [Test 215: azmcp_aks_cluster_get](#test-215)
- [Test 216: azmcp_aks_cluster_get](#test-216)
- [Test 217: azmcp_aks_cluster_get](#test-217)
- [Test 218: azmcp_aks_cluster_get](#test-218)
- [Test 219: azmcp_aks_nodepool_get](#test-219)
- [Test 220: azmcp_aks_nodepool_get](#test-220)
- [Test 221: azmcp_aks_nodepool_get](#test-221)
- [Test 222: azmcp_aks_nodepool_get](#test-222)
- [Test 223: azmcp_aks_nodepool_get](#test-223)
- [Test 224: azmcp_aks_nodepool_get](#test-224)
- [Test 225: azmcp_loadtesting_test_create](#test-225)
- [Test 226: azmcp_loadtesting_test_get](#test-226)
- [Test 227: azmcp_loadtesting_testresource_create](#test-227)
- [Test 228: azmcp_loadtesting_testresource_list](#test-228)
- [Test 229: azmcp_loadtesting_testrun_create](#test-229)
- [Test 230: azmcp_loadtesting_testrun_get](#test-230)
- [Test 231: azmcp_loadtesting_testrun_list](#test-231)
- [Test 232: azmcp_loadtesting_testrun_update](#test-232)
- [Test 233: azmcp_grafana_list](#test-233)
- [Test 234: azmcp_azuremanagedlustre_filesystem_list](#test-234)
- [Test 235: azmcp_azuremanagedlustre_filesystem_list](#test-235)
- [Test 236: azmcp_azuremanagedlustre_filesystem_sku_get](#test-236)
- [Test 237: azmcp_azuremanagedlustre_filesystem_subnetsize_ask](#test-237)
- [Test 238: azmcp_azuremanagedlustre_filesystem_subnetsize_validate](#test-238)
- [Test 239: azmcp_marketplace_product_get](#test-239)
- [Test 240: azmcp_marketplace_product_list](#test-240)
- [Test 241: azmcp_marketplace_product_list](#test-241)
- [Test 242: azmcp_get_bestpractices_get](#test-242)
- [Test 243: azmcp_get_bestpractices_get](#test-243)
- [Test 244: azmcp_get_bestpractices_get](#test-244)
- [Test 245: azmcp_get_bestpractices_get](#test-245)
- [Test 246: azmcp_get_bestpractices_get](#test-246)
- [Test 247: azmcp_get_bestpractices_get](#test-247)
- [Test 248: azmcp_get_bestpractices_get](#test-248)
- [Test 249: azmcp_get_bestpractices_get](#test-249)
- [Test 250: azmcp_monitor_healthmodels_entity_gethealth](#test-250)
- [Test 251: azmcp_monitor_metrics_definitions](#test-251)
- [Test 252: azmcp_monitor_metrics_definitions](#test-252)
- [Test 253: azmcp_monitor_metrics_definitions](#test-253)
- [Test 254: azmcp_monitor_metrics_query](#test-254)
- [Test 255: azmcp_monitor_metrics_query](#test-255)
- [Test 256: azmcp_monitor_metrics_query](#test-256)
- [Test 257: azmcp_monitor_metrics_query](#test-257)
- [Test 258: azmcp_monitor_metrics_query](#test-258)
- [Test 259: azmcp_monitor_metrics_query](#test-259)
- [Test 260: azmcp_monitor_resource_log_query](#test-260)
- [Test 261: azmcp_monitor_table_list](#test-261)
- [Test 262: azmcp_monitor_table_list](#test-262)
- [Test 263: azmcp_monitor_table_type_list](#test-263)
- [Test 264: azmcp_monitor_table_type_list](#test-264)
- [Test 265: azmcp_monitor_workspace_list](#test-265)
- [Test 266: azmcp_monitor_workspace_list](#test-266)
- [Test 267: azmcp_monitor_workspace_list](#test-267)
- [Test 268: azmcp_monitor_workspace_log_query](#test-268)
- [Test 269: azmcp_datadog_monitoredresources_list](#test-269)
- [Test 270: azmcp_datadog_monitoredresources_list](#test-270)
- [Test 271: azmcp_extension_azqr](#test-271)
- [Test 272: azmcp_extension_azqr](#test-272)
- [Test 273: azmcp_extension_azqr](#test-273)
- [Test 274: azmcp_quota_region_availability_list](#test-274)
- [Test 275: azmcp_quota_usage_check](#test-275)
- [Test 276: azmcp_role_assignment_list](#test-276)
- [Test 277: azmcp_role_assignment_list](#test-277)
- [Test 278: azmcp_redis_cache_accesspolicy_list](#test-278)
- [Test 279: azmcp_redis_cache_accesspolicy_list](#test-279)
- [Test 280: azmcp_redis_cache_list](#test-280)
- [Test 281: azmcp_redis_cache_list](#test-281)
- [Test 282: azmcp_redis_cache_list](#test-282)
- [Test 283: azmcp_redis_cluster_database_list](#test-283)
- [Test 284: azmcp_redis_cluster_database_list](#test-284)
- [Test 285: azmcp_redis_cluster_list](#test-285)
- [Test 286: azmcp_redis_cluster_list](#test-286)
- [Test 287: azmcp_redis_cluster_list](#test-287)
- [Test 288: azmcp_group_list](#test-288)
- [Test 289: azmcp_group_list](#test-289)
- [Test 290: azmcp_group_list](#test-290)
- [Test 291: azmcp_resourcehealth_availability-status_get](#test-291)
- [Test 292: azmcp_resourcehealth_availability-status_get](#test-292)
- [Test 293: azmcp_resourcehealth_availability-status_get](#test-293)
- [Test 294: azmcp_resourcehealth_availability-status_list](#test-294)
- [Test 295: azmcp_resourcehealth_availability-status_list](#test-295)
- [Test 296: azmcp_resourcehealth_availability-status_list](#test-296)
- [Test 297: azmcp_resourcehealth_service-health-events_list](#test-297)
- [Test 298: azmcp_resourcehealth_service-health-events_list](#test-298)
- [Test 299: azmcp_resourcehealth_service-health-events_list](#test-299)
- [Test 300: azmcp_resourcehealth_service-health-events_list](#test-300)
- [Test 301: azmcp_resourcehealth_service-health-events_list](#test-301)
- [Test 302: azmcp_servicebus_queue_details](#test-302)
- [Test 303: azmcp_servicebus_topic_details](#test-303)
- [Test 304: azmcp_servicebus_topic_subscription_details](#test-304)
- [Test 305: azmcp_sql_db_create](#test-305)
- [Test 306: azmcp_sql_db_create](#test-306)
- [Test 307: azmcp_sql_db_create](#test-307)
- [Test 308: azmcp_sql_db_delete](#test-308)
- [Test 309: azmcp_sql_db_delete](#test-309)
- [Test 310: azmcp_sql_db_delete](#test-310)
- [Test 311: azmcp_sql_db_list](#test-311)
- [Test 312: azmcp_sql_db_list](#test-312)
- [Test 313: azmcp_sql_db_rename](#test-313)
- [Test 314: azmcp_sql_db_rename](#test-314)
- [Test 315: azmcp_sql_db_show](#test-315)
- [Test 316: azmcp_sql_db_show](#test-316)
- [Test 317: azmcp_sql_db_update](#test-317)
- [Test 318: azmcp_sql_db_update](#test-318)
- [Test 319: azmcp_sql_elastic-pool_list](#test-319)
- [Test 320: azmcp_sql_elastic-pool_list](#test-320)
- [Test 321: azmcp_sql_elastic-pool_list](#test-321)
- [Test 322: azmcp_sql_server_create](#test-322)
- [Test 323: azmcp_sql_server_create](#test-323)
- [Test 324: azmcp_sql_server_create](#test-324)
- [Test 325: azmcp_sql_server_delete](#test-325)
- [Test 326: azmcp_sql_server_delete](#test-326)
- [Test 327: azmcp_sql_server_delete](#test-327)
- [Test 328: azmcp_sql_server_entra-admin_list](#test-328)
- [Test 329: azmcp_sql_server_entra-admin_list](#test-329)
- [Test 330: azmcp_sql_server_entra-admin_list](#test-330)
- [Test 331: azmcp_sql_server_firewall-rule_create](#test-331)
- [Test 332: azmcp_sql_server_firewall-rule_create](#test-332)
- [Test 333: azmcp_sql_server_firewall-rule_create](#test-333)
- [Test 334: azmcp_sql_server_firewall-rule_delete](#test-334)
- [Test 335: azmcp_sql_server_firewall-rule_delete](#test-335)
- [Test 336: azmcp_sql_server_firewall-rule_delete](#test-336)
- [Test 337: azmcp_sql_server_firewall-rule_list](#test-337)
- [Test 338: azmcp_sql_server_firewall-rule_list](#test-338)
- [Test 339: azmcp_sql_server_firewall-rule_list](#test-339)
- [Test 340: azmcp_sql_server_list](#test-340)
- [Test 341: azmcp_sql_server_list](#test-341)
- [Test 342: azmcp_sql_server_show](#test-342)
- [Test 343: azmcp_sql_server_show](#test-343)
- [Test 344: azmcp_sql_server_show](#test-344)
- [Test 345: azmcp_storage_account_create](#test-345)
- [Test 346: azmcp_storage_account_create](#test-346)
- [Test 347: azmcp_storage_account_create](#test-347)
- [Test 348: azmcp_storage_account_get](#test-348)
- [Test 349: azmcp_storage_account_get](#test-349)
- [Test 350: azmcp_storage_account_get](#test-350)
- [Test 351: azmcp_storage_account_get](#test-351)
- [Test 352: azmcp_storage_account_get](#test-352)
- [Test 353: azmcp_storage_blob_container_create](#test-353)
- [Test 354: azmcp_storage_blob_container_create](#test-354)
- [Test 355: azmcp_storage_blob_container_create](#test-355)
- [Test 356: azmcp_storage_blob_container_get](#test-356)
- [Test 357: azmcp_storage_blob_container_get](#test-357)
- [Test 358: azmcp_storage_blob_container_get](#test-358)
- [Test 359: azmcp_storage_blob_get](#test-359)
- [Test 360: azmcp_storage_blob_get](#test-360)
- [Test 361: azmcp_storage_blob_get](#test-361)
- [Test 362: azmcp_storage_blob_get](#test-362)
- [Test 363: azmcp_storage_blob_upload](#test-363)
- [Test 364: azmcp_subscription_list](#test-364)
- [Test 365: azmcp_subscription_list](#test-365)
- [Test 366: azmcp_subscription_list](#test-366)
- [Test 367: azmcp_subscription_list](#test-367)
- [Test 368: azmcp_azureterraformbestpractices_get](#test-368)
- [Test 369: azmcp_azureterraformbestpractices_get](#test-369)
- [Test 370: azmcp_virtualdesktop_hostpool_list](#test-370)
- [Test 371: azmcp_virtualdesktop_hostpool_sessionhost_list](#test-371)
- [Test 372: azmcp_virtualdesktop_hostpool_sessionhost_usersession-list](#test-372)
- [Test 373: azmcp_workbooks_create](#test-373)
- [Test 374: azmcp_workbooks_delete](#test-374)
- [Test 375: azmcp_workbooks_list](#test-375)
- [Test 376: azmcp_workbooks_list](#test-376)
- [Test 377: azmcp_workbooks_show](#test-377)
- [Test 378: azmcp_workbooks_show](#test-378)
- [Test 379: azmcp_workbooks_update](#test-379)
- [Test 380: azmcp_bicepschema_get](#test-380)
- [Test 381: azmcp_cloudarchitect_design](#test-381)
- [Test 382: azmcp_cloudarchitect_design](#test-382)
- [Test 383: azmcp_cloudarchitect_design](#test-383)
- [Test 384: azmcp_cloudarchitect_design](#test-384)

---

## Test 1

**Expected Tool:** `azmcp_foundry_agents_connect`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622689 | `azmcp_foundry_agents_connect` | ✅ **EXPECTED** |
| 2 | 0.603098 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.494591 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.443011 | `azmcp_foundry_agents_evaluate` | ❌ |
| 5 | 0.379587 | `azmcp_search_index_query` | ❌ |

---

## Test 2

**Expected Tool:** `azmcp_foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543072 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469272 | `azmcp_foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.444943 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.235742 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.233856 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 3

**Expected Tool:** `azmcp_foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my AI Foundry project for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580559 | `azmcp_foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 2 | 0.568420 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.518655 | `azmcp_foundry_agents_evaluate` | ❌ |
| 4 | 0.382031 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.326026 | `azmcp_foundry_models_deploy` | ❌ |

---

## Test 4

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695202 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.533032 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.526528 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 4 | 0.433117 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.422336 | `azmcp_search_index_get` | ❌ |

---

## Test 5

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603396 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489311 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.474009 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.396819 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.374208 | `azmcp_search_index_get` | ❌ |

---

## Test 6

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672577 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.564860 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.423942 | `azmcp_search_index_get` | ❌ |
| 4 | 0.401718 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.397232 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 7

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650269 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432751 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.417421 | `azmcp_kusto_table_schema` | ❌ |
| 4 | 0.415963 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.398186 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 8

**Expected Tool:** `azmcp_foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.562920 | `azmcp_foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.298490 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.293050 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.282258 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.280674 | `azmcp_foundry_openai_create-completion` | ❌ |

---

## Test 9

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663532 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.549636 | `azmcp_foundry_models_list` | ❌ |
| 3 | 0.539729 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.536115 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.446686 | `azmcp_search_service_list` | ❌ |

---

## Test 10

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606645 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.521475 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.518221 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.486462 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.421169 | `azmcp_foundry_openai_create-completion` | ❌ |

---

## Test 11

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560155 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.506889 | `azmcp_foundry_models_deployments_list` | ❌ |
| 3 | 0.491960 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.415230 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.387908 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 12

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574818 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.497284 | `azmcp_foundry_models_deployments_list` | ❌ |
| 3 | 0.475216 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.467671 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.417075 | `azmcp_foundry_openai_create-completion` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_foundry_openai_create-completion`  
**Prompt:** Create a completion with the prompt "What is Azure?"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553675 | `azmcp_foundry_openai_create-completion` | ✅ **EXPECTED** |
| 2 | 0.403431 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.394144 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.386531 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.371786 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 14

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680081 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544557 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.490553 | `azmcp_search_service_list` | ❌ |
| 4 | 0.466005 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.459609 | `azmcp_search_index_query` | ❌ |

---

## Test 15

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639545 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.619949 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561856 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.480817 | `azmcp_search_index_query` | ❌ |
| 5 | 0.453014 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 16

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620268 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562503 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561154 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.471415 | `azmcp_search_index_query` | ❌ |
| 5 | 0.463972 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 17

**Expected Tool:** `azmcp_search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521761 | `azmcp_search_index_get` | ❌ |
| 2 | 0.515886 | `azmcp_search_index_query` | ✅ **EXPECTED** |
| 3 | 0.498420 | `azmcp_search_service_list` | ❌ |
| 4 | 0.437716 | `azmcp_postgres_database_query` | ❌ |
| 5 | 0.374017 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 18

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.791803 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.553038 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.520284 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.509460 | `azmcp_subscription_list` | ❌ |
| 5 | 0.505311 | `azmcp_search_index_get` | ❌ |

---

## Test 19

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684837 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.479332 | `azmcp_search_index_get` | ❌ |
| 3 | 0.467338 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.461780 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.453497 | `azmcp_marketplace_product_list` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551241 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.435702 | `azmcp_search_index_get` | ❌ |
| 3 | 0.417130 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.404758 | `azmcp_search_index_query` | ❌ |
| 5 | 0.336174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 21

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666038 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.351127 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.342808 | `azmcp_foundry_openai_create-completion` | ❌ |
| 4 | 0.338448 | `azmcp_communication_sms_send` | ❌ |
| 5 | 0.337685 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 22

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511324 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.184542 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.158898 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.154918 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.145205 | `azmcp_applens_resource_diagnose` | ❌ |

---

## Test 23

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486489 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.180941 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.160057 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.156850 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.143871 | `azmcp_foundry_models_deploy` | ❌ |

---

## Test 24

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611992 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.263196 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.237558 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.212149 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.203867 | `azmcp_foundry_models_deployments_list` | ❌ |

---

## Test 25

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.410533 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.152137 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.151632 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.142020 | `azmcp_foundry_openai_create-completion` | ❌ |
| 5 | 0.140373 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 26

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546259 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.196743 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.183420 | `azmcp_extension_azqr` | ❌ |
| 4 | 0.180766 | `azmcp_search_index_get` | ❌ |
| 5 | 0.177835 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 27

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539963 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.246979 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.203103 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.179810 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.174984 | `azmcp_azureterraformbestpractices_get` | ❌ |

---

## Test 28

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549151 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.345661 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.337961 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.333076 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.324507 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 29

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532536 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.378382 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.342113 | `azmcp_communication_sms_send` | ❌ |
| 4 | 0.326712 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.304372 | `azmcp_search_service_list` | ❌ |

---

## Test 30

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453396 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.181994 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.173205 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.160306 | `azmcp_foundry_agents_connect` | ❌ |
| 5 | 0.160185 | `azmcp_extension_azqr` | ❌ |

---

## Test 31

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.530613 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.491380 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.481256 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.479988 | `azmcp_subscription_list` | ❌ |

---

## Test 32

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634978 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.464865 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.398495 | `azmcp_subscription_list` | ❌ |
| 4 | 0.391608 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.372456 | `azmcp_postgres_server_list` | ❌ |

---

## Test 33

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565435 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.465344 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.355916 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.348661 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.327234 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 34

**Expected Tool:** `azmcp_appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617935 | `azmcp_appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.464293 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.424040 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.422735 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.391864 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 35

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.632687 | `azmcp_appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.557810 | `azmcp_appconfig_account_list` | ❌ |
| 3 | 0.530884 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.464635 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.439358 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 36

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612555 | `azmcp_appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.522426 | `azmcp_appconfig_account_list` | ❌ |
| 3 | 0.512945 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.468503 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.458231 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 37

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512883 | `azmcp_appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.449905 | `azmcp_appconfig_account_list` | ❌ |
| 3 | 0.398684 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.380614 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.346239 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 38

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552418 | `azmcp_appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.448821 | `azmcp_appconfig_kv_set` | ❌ |
| 3 | 0.441711 | `azmcp_appconfig_kv_delete` | ❌ |
| 4 | 0.437513 | `azmcp_appconfig_account_list` | ❌ |
| 5 | 0.416464 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 39

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591286 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.487174 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.445551 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.431516 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.373656 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 40

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555680 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.505681 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.476497 | `azmcp_appconfig_kv_delete` | ❌ |
| 4 | 0.425488 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.409406 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 41

**Expected Tool:** `azmcp_appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608834 | `azmcp_appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.537187 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 3 | 0.513130 | `azmcp_appconfig_kv_get` | ❌ |
| 4 | 0.505808 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.378252 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 42

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595724 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.336090 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.272763 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.216077 | `azmcp_get_bestpractices_get` | ❌ |

---

## Test 43

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502536 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.316297 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.255570 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.226018 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.223620 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 44

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.493019 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.256325 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.242667 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.225477 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.216177 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 45

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a database connection to my app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729094 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.398617 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.380126 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.368252 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.364387 | `azmcp_mysql_server_list` | ❌ |

---

## Test 46

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure a SQL Server database for app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612088 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.473224 | `azmcp_sql_db_update` | ❌ |
| 3 | 0.471103 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.454417 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.412179 | `azmcp_sql_server_delete` | ❌ |

---

## Test 47

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a MySQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648445 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.418902 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.409593 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.397907 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.382565 | `azmcp_mysql_server_list` | ❌ |

---

## Test 48

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a PostgreSQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579461 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449029 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.416387 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.409517 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.405431 | `azmcp_postgres_server_list` | ❌ |

---

## Test 49

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a CosmosDB database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643008 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.477008 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.465637 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.431581 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.428217 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 50

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645487 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.489228 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.440007 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.431532 | `azmcp_sql_db_delete` | ❌ |
| 5 | 0.425989 | `azmcp_sql_server_delete` | ❌ |

---

## Test 51

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665201 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.401714 | `azmcp_sql_db_rename` | ❌ |
| 3 | 0.369071 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.332119 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.328637 | `azmcp_sql_db_update` | ❌ |

---

## Test 52

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536723 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.408796 | `azmcp_sql_db_rename` | ❌ |
| 3 | 0.394572 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.355309 | `azmcp_sql_db_update` | ❌ |
| 5 | 0.329110 | `azmcp_keyvault_secret_create` | ❌ |

---

## Test 53

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560240 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.426753 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.392376 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.371906 | `azmcp_sql_db_delete` | ❌ |
| 5 | 0.361013 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 54

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572473 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.445157 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.390478 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.383888 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.375286 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 55

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696531 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.468384 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.452150 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.435241 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.423329 | `azmcp_search_service_list` | ❌ |

---

## Test 56

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.487949 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.479345 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.477314 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.468847 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 57

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.419669 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.383767 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.367278 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.344740 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 58

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.585694 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.540241 | `azmcp_search_service_list` | ❌ |
| 5 | 0.520746 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 59

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.450287 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.421815 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.415552 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 60

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637130 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563476 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.516745 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.496441 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.483909 | `azmcp_redis_cache_list` | ❌ |

---

## Test 61

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `azmcp_acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.475929 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.454929 | `azmcp_group_list` | ❌ |
| 5 | 0.454003 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 62

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.467983 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.449649 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `azmcp_group_list` | ❌ |

---

## Test 63

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.544162 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.495567 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.487470 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 64

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546334 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.407973 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.400145 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.356758 | `azmcp_redis_cache_list` | ❌ |

---

## Test 65

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.433927 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.388490 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.383183 | `azmcp_kusto_database_list` | ❌ |

---

## Test 66

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501842 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.418623 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.377031 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.376522 | `azmcp_redis_cache_list` | ❌ |

---

## Test 67

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send an SMS message to +1234567890 saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.333833 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.130271 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.117579 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.107921 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.107379 | `azmcp_loadtesting_testrun_create` | ❌ |

---

## Test 68

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS to +1234567890 from +1234567891 with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.331241 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.160250 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.124785 | `azmcp_loadtesting_testrun_update` | ❌ |
| 4 | 0.113866 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.113715 | `azmcp_appservice_database_add` | ❌ |

---

## Test 69

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: +1234567890, +1234567891  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.369434 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.139353 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.100205 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.090472 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.090176 | `azmcp_foundry_agents_evaluate` | ❌ |

---

## Test 70

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.404192 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.191848 | `azmcp_extension_azqr` | ❌ |
| 3 | 0.170775 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.151672 | `azmcp_postgres_server_param_set` | ❌ |
| 5 | 0.148193 | `azmcp_mysql_server_param_set` | ❌ |

---

## Test 71

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.367337 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.188153 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.159177 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.158295 | `azmcp_loadtesting_test_create` | ❌ |
| 5 | 0.155703 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |

---

## Test 72

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send broadcast SMS to +1234567890 and +1234567891 saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.323284 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.152098 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.137743 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.130925 | `azmcp_foundry_agents_evaluate` | ❌ |
| 5 | 0.121610 | `azmcp_eventgrid_events_publish` | ❌ |

---

## Test 73

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS from my communication service to +1234567890  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.442389 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.165566 | `azmcp_appservice_database_add` | ❌ |
| 3 | 0.142296 | `azmcp_foundry_openai_create-completion` | ❌ |
| 4 | 0.135617 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.121337 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 74

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.430162 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.206938 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.187824 | `azmcp_confidentialledger_entries_append` | ❌ |
| 4 | 0.162637 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.143138 | `azmcp_eventgrid_events_publish` | ❌ |

---

## Test 75

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510492 | `azmcp_confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.292216 | `azmcp_appconfig_kv_set` | ❌ |
| 3 | 0.259322 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 4 | 0.249935 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.240362 | `azmcp_keyvault_secret_create` | ❌ |

---

## Test 76

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602112 | `azmcp_confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.212204 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 3 | 0.195325 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.183625 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.183540 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 77

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546945 | `azmcp_confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.225654 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 3 | 0.216138 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.211734 | `azmcp_appservice_database_add` | ❌ |
| 5 | 0.203256 | `azmcp_keyvault_certificate_import` | ❌ |

---

## Test 78

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496023 | `azmcp_confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.198615 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.195282 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.188110 | `azmcp_storage_blob_container_create` | ❌ |
| 5 | 0.186933 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 79

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622138 | `azmcp_confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.252818 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 3 | 0.240252 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.186890 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.184865 | `azmcp_keyvault_certificate_import` | ❌ |

---

## Test 80

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818398 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668448 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.636036 | `azmcp_subscription_list` | ❌ |
| 4 | 0.615268 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.601494 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 81

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665477 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605310 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.571613 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.549361 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.494741 | `azmcp_subscription_list` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752554 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.607201 | `azmcp_subscription_list` | ❌ |
| 3 | 0.605071 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.566249 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.563778 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 83

**Expected Tool:** `azmcp_cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658727 | `azmcp_cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 2 | 0.605253 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.477847 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.447774 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.445640 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 84

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852832 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681048 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.680758 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 4 | 0.630628 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.581593 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 85

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789395 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.648182 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 3 | 0.614218 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.562051 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.537286 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 86

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815706 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668512 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.665298 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.606235 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.583535 | `azmcp_kusto_database_list` | ❌ |

---

## Test 87

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749384 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.614579 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.579807 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.538479 | `azmcp_mysql_database_list` | ❌ |

---

## Test 88

**Expected Tool:** `azmcp_kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590264 | `azmcp_kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.485651 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.463833 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.428159 | `azmcp_kusto_query` | ❌ |
| 5 | 0.425669 | `azmcp_kusto_database_list` | ❌ |

---

## Test 89

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793760 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.653993 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.630507 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.573395 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.509397 | `azmcp_grafana_list` | ❌ |

---

## Test 90

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531352 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.510138 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.465277 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.432288 | `azmcp_kusto_database_list` | ❌ |
| 5 | 0.391087 | `azmcp_redis_cluster_database_list` | ❌ |

---

## Test 91

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701482 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.616752 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.571191 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.548685 | `azmcp_kusto_database_list` | ❌ |
| 5 | 0.462945 | `azmcp_grafana_list` | ❌ |

---

## Test 92

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677059 | `azmcp_kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.628129 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.560625 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.556795 | `azmcp_kusto_table_list` | ❌ |
| 5 | 0.553191 | `azmcp_postgres_database_list` | ❌ |

---

## Test 93

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623336 | `azmcp_kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.597738 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.509856 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.506912 | `azmcp_kusto_table_list` | ❌ |
| 5 | 0.496864 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 94

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423671 | `azmcp_kusto_query` | ✅ **EXPECTED** |
| 2 | 0.409571 | `azmcp_postgres_database_query` | ❌ |
| 3 | 0.408183 | `azmcp_kusto_table_schema` | ❌ |
| 4 | 0.407762 | `azmcp_kusto_sample` | ❌ |
| 5 | 0.404003 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 95

**Expected Tool:** `azmcp_kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595550 | `azmcp_kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.510231 | `azmcp_kusto_table_schema` | ❌ |
| 3 | 0.424206 | `azmcp_kusto_table_list` | ❌ |
| 4 | 0.400938 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.399520 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 96

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.679642 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585213 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.581207 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.556724 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.549885 | `azmcp_monitor_table_list` | ❌ |

---

## Test 97

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619252 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.554332 | `azmcp_kusto_table_schema` | ❌ |
| 3 | 0.527625 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.524691 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.523392 | `azmcp_postgres_table_list` | ❌ |

---

## Test 98

**Expected Tool:** `azmcp_kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.667904 | `azmcp_kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.565245 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.529139 | `azmcp_mysql_table_schema_get` | ❌ |
| 4 | 0.490979 | `azmcp_kusto_sample` | ❌ |
| 5 | 0.490240 | `azmcp_kusto_table_list` | ❌ |

---

## Test 99

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633968 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.623421 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534457 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.498872 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.490148 | `azmcp_sql_db_list` | ❌ |

---

## Test 100

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588121 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.573961 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.483855 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.463188 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.448169 | `azmcp_redis_cluster_database_list` | ❌ |

---

## Test 101

**Expected Tool:** `azmcp_mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476423 | `azmcp_mysql_table_list` | ❌ |
| 2 | 0.455770 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.433202 | `azmcp_mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419827 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.409445 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 102

**Expected Tool:** `azmcp_mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531887 | `azmcp_postgres_server_config_get` | ❌ |
| 2 | 0.516893 | `azmcp_mysql_server_param_set` | ❌ |
| 3 | 0.489816 | `azmcp_mysql_server_config_get` | ✅ **EXPECTED** |
| 4 | 0.476863 | `azmcp_mysql_server_param_get` | ❌ |
| 5 | 0.426507 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 103

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678473 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.558177 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.554782 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.513699 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.501199 | `azmcp_mysql_table_list` | ❌ |

---

## Test 104

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478518 | `azmcp_mysql_database_list` | ❌ |
| 2 | 0.474565 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.412380 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.389882 | `azmcp_postgres_database_list` | ❌ |

---

## Test 105

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.534240 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530210 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.487914 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.475026 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 106

**Expected Tool:** `azmcp_mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `azmcp_mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.438075 | `azmcp_mysql_server_param_set` | ❌ |
| 3 | 0.333906 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.313150 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.310795 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 107

**Expected Tool:** `azmcp_mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449419 | `azmcp_mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `azmcp_mysql_server_param_get` | ❌ |
| 3 | 0.303421 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.299246 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.277626 | `azmcp_appservice_database_add` | ❌ |

---

## Test 108

**Expected Tool:** `azmcp_mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633448 | `azmcp_mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573905 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.550833 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.546963 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.511847 | `azmcp_kusto_table_list` | ❌ |

---

## Test 109

**Expected Tool:** `azmcp_mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609131 | `azmcp_mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.526285 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.525709 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.507258 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.497934 | `azmcp_postgres_database_list` | ❌ |

---

## Test 110

**Expected Tool:** `azmcp_mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.631047 | `azmcp_mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558619 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.545432 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.517928 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.458050 | `azmcp_mysql_database_list` | ❌ |

---

## Test 111

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815496 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.644026 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.622790 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.542685 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.490895 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759870 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589784 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.585907 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.552660 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.495634 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 113

**Expected Tool:** `azmcp_postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546104 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.523142 | `azmcp_postgres_database_query` | ✅ **EXPECTED** |
| 3 | 0.503246 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.466599 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.403937 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `azmcp_postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.615297 | `azmcp_postgres_server_param_set` | ❌ |
| 3 | 0.599442 | `azmcp_postgres_server_param_get` | ❌ |
| 4 | 0.534868 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.518574 | `azmcp_postgres_server_list` | ❌ |

---

## Test 115

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900023 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640606 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.565931 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.538997 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.534239 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 116

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.606903 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.576348 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.522966 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.506194 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 117

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579078 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.531804 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.514447 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.505913 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_postgres_server_param_get`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594687 | `azmcp_postgres_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.552581 | `azmcp_postgres_server_param_set` | ❌ |
| 3 | 0.539671 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.489693 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.451742 | `azmcp_postgres_database_list` | ❌ |

---

## Test 119

**Expected Tool:** `azmcp_postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579783 | `azmcp_postgres_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.488474 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.469794 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.446976 | `azmcp_postgres_server_param_get` | ❌ |
| 5 | 0.440620 | `azmcp_postgres_database_list` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789936 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.750489 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.574931 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.519854 | `azmcp_postgres_table_schema_get` | ❌ |
| 5 | 0.501400 | `azmcp_postgres_server_config_get` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736118 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.689974 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.558396 | `azmcp_postgres_table_schema_get` | ❌ |
| 4 | 0.543331 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.521570 | `azmcp_postgres_server_config_get` | ❌ |

---

## Test 122

**Expected Tool:** `azmcp_postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714662 | `azmcp_postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597853 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.574086 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.508213 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.502502 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 123

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711770 | `azmcp_deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.404891 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.398472 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.392565 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 124

**Expected Tool:** `azmcp_deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680507 | `azmcp_deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562413 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.497045 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.490079 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.435766 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 125

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

---

## Test 126

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `azmcp_deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.448917 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.385920 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.382240 | `azmcp_get_bestpractices_get` | ❌ |

---

## Test 127

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688055 | `azmcp_deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.448692 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 128

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756151 | `azmcp_eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.483384 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.466031 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.354344 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.328415 | `azmcp_eventhubs_namespace_get` | ❌ |

---

## Test 129

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655402 | `azmcp_eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.525396 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.510176 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.372653 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.332938 | `azmcp_eventhubs_namespace_get` | ❌ |

---

## Test 130

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600904 | `azmcp_eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.521240 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.505154 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.386072 | `azmcp_eventhubs_namespace_get` | ❌ |
| 5 | 0.352461 | `azmcp_servicebus_topic_details` | ❌ |

---

## Test 131

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770140 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.745553 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.561877 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.543887 | `azmcp_search_service_list` | ❌ |
| 5 | 0.526138 | `azmcp_subscription_list` | ❌ |

---

## Test 132

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738258 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.737616 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.492597 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.480287 | `azmcp_subscription_list` | ❌ |
| 5 | 0.473459 | `azmcp_search_service_list` | ❌ |

---

## Test 133

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770033 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.721432 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.535322 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.513964 | `azmcp_search_service_list` | ❌ |
| 5 | 0.495969 | `azmcp_subscription_list` | ❌ |

---

## Test 134

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.758816 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.704606 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.609175 | `azmcp_group_list` | ❌ |
| 4 | 0.529845 | `azmcp_eventhubs_namespace_get` | ❌ |
| 5 | 0.514613 | `azmcp_workbooks_list` | ❌ |

---

## Test 135

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769324 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.720606 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.497694 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.486984 | `azmcp_eventgrid_events_publish` | ❌ |
| 5 | 0.486216 | `azmcp_servicebus_topic_subscription_details` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718366 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.709806 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.539977 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.528247 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.478689 | `azmcp_eventgrid_events_publish` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747162 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.746224 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.525023 | `azmcp_group_list` | ❌ |
| 4 | 0.502471 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.492469 | `azmcp_eventhubs_namespace_get` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736465 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.659728 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.569254 | `azmcp_subscription_list` | ❌ |
| 4 | 0.537915 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.517276 | `azmcp_search_service_list` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684579 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.656277 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.542388 | `azmcp_subscription_list` | ❌ |
| 4 | 0.520995 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.510115 | `azmcp_group_list` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696294 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.691739 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.557573 | `azmcp_group_list` | ❌ |
| 4 | 0.504984 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.502308 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709822 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.642095 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.506697 | `azmcp_subscription_list` | ❌ |
| 4 | 0.476396 | `azmcp_search_service_list` | ❌ |
| 5 | 0.475756 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_eventhubs_namespace_get`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653609 | `azmcp_eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.607369 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.557200 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.556175 | `azmcp_eventgrid_subscription_list` | ❌ |
| 5 | 0.532926 | `azmcp_search_service_list` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_eventhubs_namespace_get`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.515275 | `azmcp_eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.497399 | `azmcp_servicebus_queue_details` | ❌ |
| 3 | 0.470456 | `azmcp_functionapp_get` | ❌ |
| 4 | 0.466515 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 5 | 0.458982 | `azmcp_sql_db_show` | ❌ |

---

## Test 144

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659989 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.451613 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.450467 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.406310 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.390028 | `azmcp_mysql_server_list` | ❌ |

---

## Test 145

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607233 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447400 | `azmcp_mysql_server_config_get` | ❌ |
| 3 | 0.424693 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.411267 | `azmcp_appconfig_kv_get` | ❌ |
| 5 | 0.400402 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622374 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.478470 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.390708 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383533 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.347421 | `azmcp_applens_resource_diagnose` | ❌ |

---

## Test 147

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690872 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.463003 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.432317 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.431739 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.425161 | `azmcp_quota_usage_check` | ❌ |

---

## Test 148

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592766 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.476738 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.409712 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.392228 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.391479 | `azmcp_sql_server_show` | ❌ |

---

## Test 149

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687297 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.449589 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.428647 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.392106 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.368188 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644905 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.430189 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.388631 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.370793 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.368212 | `azmcp_storage_blob_get` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555063 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426965 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.424610 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.408011 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.381629 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565752 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.473865 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.403665 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.384159 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.369897 | `azmcp_applens_resource_diagnose` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646680 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.557549 | `azmcp_search_service_list` | ❌ |
| 3 | 0.534930 | `azmcp_subscription_list` | ❌ |
| 4 | 0.529057 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.516672 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560228 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.464985 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.436098 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.411323 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.410461 | `azmcp_search_service_list` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433602 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.346619 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.338034 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.284362 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.250665 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 156

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604780 | `azmcp_keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.520479 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.496629 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.452367 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.448039 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 157

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671363 | `azmcp_keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.453635 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.441170 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.404694 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.395231 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505681 | `azmcp_keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.496498 | `azmcp_appconfig_kv_set` | ❌ |
| 3 | 0.420196 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 4 | 0.419141 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.410226 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 159

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627727 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570328 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.540230 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.519218 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.500103 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 160

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599990 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561476 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.522706 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.502156 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.497256 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 161

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573998 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527844 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.481557 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.469710 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 162

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591697 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.562308 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.522147 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.502559 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.480110 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 163

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622788 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.558524 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.534503 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.521408 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.465062 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 164

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600625 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528487 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.519046 | `azmcp_keyvault_certificate_import` | ❌ |
| 4 | 0.499293 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.486608 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 165

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646098 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.562988 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.514170 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.509527 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.507765 | `azmcp_keyvault_certificate_import` | ❌ |

---

## Test 166

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609523 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.515652 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.511197 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.507845 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.474394 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 167

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647669 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527400 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.521626 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.509818 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.501988 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 168

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595959 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527449 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.519075 | `azmcp_keyvault_certificate_import` | ❌ |
| 4 | 0.501138 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.465174 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 169

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585786 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420747 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.402595 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.399429 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.352945 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 170

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622540 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.504314 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.498847 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.448236 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.419858 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 171

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.597019 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.454790 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.452648 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.418548 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.413499 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619666 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.517789 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.480784 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.444456 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.381842 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 173

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595001 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.451954 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.441129 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.407379 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.391737 | `azmcp_keyvault_secret_create` | ❌ |

---

## Test 174

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726236 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.583472 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.531936 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.515236 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.485792 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 175

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615638 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.522453 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.475628 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.460973 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.448139 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 176

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624792 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.519739 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.510048 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.505477 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.497627 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 177

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672713 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.554364 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.511885 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.507062 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.492357 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 178

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747518 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.594430 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.558677 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.515568 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.490876 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 179

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639828 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.512475 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.507868 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.482583 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.464710 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 180

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661479 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456541 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.451730 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.429507 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.399322 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 181

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641014 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428502 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.422763 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.420045 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.405644 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 182

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547504 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.463557 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.447410 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.420366 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.404166 | `azmcp_keyvault_certificate_import` | ❌ |

---

## Test 183

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641388 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.501636 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.491735 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.464510 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.451016 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 184

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571667 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443369 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.434675 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.421721 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.400536 | `azmcp_keyvault_certificate_import` | ❌ |

---

## Test 185

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549488 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.468165 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.452830 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.441000 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.426545 | `azmcp_keyvault_certificate_get` | ❌ |

---

## Test 186

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629497 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.532562 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.495936 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.475258 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.456962 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 187

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484645 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.443188 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.409388 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.395491 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 5 | 0.383772 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 188

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590303 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.488213 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.460796 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.440996 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.436511 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 189

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518886 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.432742 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 3 | 0.429142 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.422536 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.397458 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 190

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702034 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.601667 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.587370 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.498748 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.480129 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 191

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550223 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.506815 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.475615 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.472465 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 5 | 0.455683 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 192

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582422 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.502245 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 3 | 0.501595 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.476470 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.472341 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 193

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642063 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.559655 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.553529 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.486377 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 5 | 0.475961 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 194

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.723732 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611515 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.611081 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.473886 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 5 | 0.441881 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 195

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571275 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.500239 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.490326 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.489635 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 196

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678482 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.553063 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.512856 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.475097 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.461437 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 197

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663070 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.519442 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.512222 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.458424 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.430478 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 198

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638637 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.508694 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.486442 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.483995 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.451504 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 199

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675145 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.499612 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.498322 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.479212 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.458574 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 200

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571470 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.513610 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.441230 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.418765 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.408089 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 201

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602769 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504212 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.501397 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.478680 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `azmcp_keyvault_certificate_get` | ❌ |

---

## Test 202

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654331 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.567300 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.496767 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.484936 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.484679 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 203

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578479 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.492213 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.488705 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.443591 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.424167 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 204

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649184 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.546928 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.492534 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.491409 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.480271 | `azmcp_keyvault_secret_create` | ❌ |

---

## Test 205

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577477 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.475443 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.466890 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.447526 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.421359 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 206

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701097 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.564300 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.538420 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.499642 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.455467 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 207

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555562 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.543861 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.465462 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.453130 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 208

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572456 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.529258 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.494076 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.487620 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 5 | 0.475273 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 209

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624226 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.560434 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.517576 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.479547 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.442945 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 210

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742188 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.601692 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.567897 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.496127 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.437560 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 211

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567022 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.522399 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.477063 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.462677 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 212

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650418 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.558530 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.517279 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.481416 | `azmcp_mysql_server_config_get` | ❌ |
| 5 | 0.430976 | `azmcp_postgres_server_config_get` | ❌ |

---

## Test 213

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595077 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.567870 | `azmcp_kusto_cluster_get` | ❌ |
| 3 | 0.475334 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.471692 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.461785 | `azmcp_sql_db_show` | ❌ |

---

## Test 214

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.542755 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.464247 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.434684 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.398748 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.380301 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 215

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596102 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.527511 | `azmcp_kusto_cluster_get` | ❌ |
| 3 | 0.482616 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.435356 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.434610 | `azmcp_functionapp_get` | ❌ |

---

## Test 216

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749415 | `azmcp_kusto_cluster_list` | ❌ |
| 2 | 0.723178 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 3 | 0.634765 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.568403 | `azmcp_kusto_database_list` | ❌ |
| 5 | 0.560522 | `azmcp_search_service_list` | ❌ |

---

## Test 217

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594886 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.586703 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.545101 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.489724 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.466970 | `azmcp_aks_nodepool_get` | ❌ |

---

## Test 218

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593985 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.526805 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.500397 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.477496 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.426157 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 219

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681158 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.521913 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.517021 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.468519 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463192 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 220

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646941 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.458596 | `azmcp_sql_elastic-pool_list` | ❌ |
| 3 | 0.450190 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.440304 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.413758 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 221

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586165 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.411365 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.385274 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.383043 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.355090 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 222

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686975 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.521969 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.506726 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500784 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.487707 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 223

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684416 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.544729 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.510269 | `azmcp_sql_elastic-pool_list` | ❌ |
| 4 | 0.509869 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.486718 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 224

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628929 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.457312 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.443957 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.433043 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.425448 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 225

**Expected Tool:** `azmcp_loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577811 | `azmcp_loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.519418 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.512099 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.460978 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.432550 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 226

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626272 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.619944 | `azmcp_loadtesting_test_get` | ✅ **EXPECTED** |
| 3 | 0.594666 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.520902 | `azmcp_loadtesting_testrun_list` | ❌ |
| 5 | 0.476666 | `azmcp_loadtesting_testrun_create` | ❌ |

---

## Test 227

**Expected Tool:** `azmcp_loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645562 | `azmcp_loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.618748 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.541732 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.539762 | `azmcp_loadtesting_testrun_create` | ❌ |
| 5 | 0.442079 | `azmcp_workbooks_create` | ❌ |

---

## Test 228

**Expected Tool:** `azmcp_loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794285 | `azmcp_loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.577408 | `azmcp_group_list` | ❌ |
| 3 | 0.575172 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.565565 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.561516 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 229

**Expected Tool:** `azmcp_loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688976 | `azmcp_loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.594879 | `azmcp_loadtesting_testrun_update` | ❌ |
| 3 | 0.558636 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.547102 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.496607 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 230

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619255 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.601927 | `azmcp_loadtesting_test_get` | ❌ |
| 3 | 0.597430 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.565925 | `azmcp_loadtesting_testrun_list` | ❌ |
| 5 | 0.549898 | `azmcp_loadtesting_testrun_create` | ❌ |

---

## Test 231

**Expected Tool:** `azmcp_loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669343 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.640151 | `azmcp_loadtesting_testrun_list` | ✅ **EXPECTED** |
| 3 | 0.601067 | `azmcp_loadtesting_test_get` | ❌ |
| 4 | 0.577444 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.516528 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 232

**Expected Tool:** `azmcp_loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.706747 | `azmcp_loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.514428 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.470738 | `azmcp_loadtesting_testresource_list` | ❌ |
| 4 | 0.446897 | `azmcp_loadtesting_test_get` | ❌ |
| 5 | 0.429045 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 233

**Expected Tool:** `azmcp_grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599431 | `azmcp_kusto_cluster_list` | ❌ |
| 2 | 0.578892 | `azmcp_grafana_list` | ✅ **EXPECTED** |
| 3 | 0.550372 | `azmcp_subscription_list` | ❌ |
| 4 | 0.549957 | `azmcp_search_service_list` | ❌ |
| 5 | 0.523091 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 234

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750574 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.562360 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.512086 | `azmcp_search_service_list` | ❌ |
| 5 | 0.509943 | `azmcp_kusto_database_list` | ❌ |

---

## Test 235

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743769 | `azmcp_azuremanagedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.613217 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.519986 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 4 | 0.515412 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.514052 | `azmcp_mysql_server_list` | ❌ |

---

## Test 236

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.836071 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ✅ **EXPECTED** |
| 2 | 0.626173 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.506309 | `azmcp_azuremanagedlustre_filesystem_subnetsize_validate` | ❌ |
| 4 | 0.473936 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.453609 | `azmcp_storage_account_get` | ❌ |

---

## Test 237

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646978 | `azmcp_azuremanagedlustre_filesystem_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.450386 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.449406 | `azmcp_azuremanagedlustre_filesystem_subnetsize_validate` | ❌ |
| 4 | 0.327359 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.265636 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 238

**Expected Tool:** `azmcp_azuremanagedlustre_filesystem_subnetsize_validate`  
**Prompt:** Validate if <subnet_id> can host <filesystem_size> of <amlfs_sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737312 | `azmcp_azuremanagedlustre_filesystem_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.564689 | `azmcp_azuremanagedlustre_filesystem_subnetsize_ask` | ❌ |
| 3 | 0.433458 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 4 | 0.314164 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.255153 | `azmcp_quota_usage_check` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570145 | `azmcp_marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.477496 | `azmcp_marketplace_product_list` | ❌ |
| 3 | 0.353256 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.332945 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.330935 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 240

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527042 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443133 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.341341 | `azmcp_search_service_list` | ❌ |
| 4 | 0.330500 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.328676 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |

---

## Test 241

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461723 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385167 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.308769 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.260387 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.247908 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 242

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.635406 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.531728 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490235 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 243

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.548542 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.541091 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.516852 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.516443 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 244

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.594323 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.518643 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.465573 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.452009 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 245

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.570488 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.522998 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.445382 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 246

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581850 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.497350 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495659 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.486886 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.474511 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 247

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.532790 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.487322 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.458060 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.413117 | `azmcp_functionapp_get` | ❌ |

---

## Test 248

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.513262 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.505123 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.422546 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.500368 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.472112 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.433134 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.432526 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 250

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498365 | `azmcp_monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.492241 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.472094 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.467848 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.467443 | `azmcp_monitor_table_list` | ❌ |

---

## Test 251

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592279 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424141 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.332661 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.332356 | `azmcp_monitor_table_type_list` | ❌ |
| 5 | 0.315549 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 252

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589839 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.587796 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.551156 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.485326 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.473534 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 253

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633517 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495513 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.398890 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.393056 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.383202 | `azmcp_applens_resource_diagnose` | ❌ |

---

## Test 254

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555377 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.464712 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.447607 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.428884 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.420433 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 255

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557830 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.509377 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.460611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.456308 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.437852 | `azmcp_monitor_metrics_definitions` | ❌ |

---

## Test 256

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461097 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.389347 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.340145 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.306329 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.301688 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 257

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492124 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.448129 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.419989 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.415950 | `azmcp_monitor_resource_log_query` | ❌ |
| 5 | 0.412169 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 258

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525585 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.383670 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.376657 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.367167 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.330628 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 259

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480142 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.388456 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.368507 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.363660 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.350538 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 260

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594068 | `azmcp_monitor_workspace_log_query` | ❌ |
| 2 | 0.580120 | `azmcp_monitor_resource_log_query` | ✅ **EXPECTED** |
| 3 | 0.485633 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.469703 | `azmcp_monitor_metrics_query` | ❌ |
| 5 | 0.443468 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 261

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850644 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.541928 | `azmcp_kusto_table_list` | ❌ |
| 5 | 0.534829 | `azmcp_mysql_table_list` | ❌ |

---

## Test 262

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798090 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.599916 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.497065 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.487237 | `azmcp_grafana_list` | ❌ |

---

## Test 263

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765563 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.569921 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.504683 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.477280 | `azmcp_grafana_list` | ❌ |

---

## Test 264

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843139 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736711 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.576731 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.481189 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.475734 | `azmcp_grafana_list` | ❌ |

---

## Test 265

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `azmcp_grafana_list` | ❌ |
| 3 | 0.659264 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.610624 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.599636 | `azmcp_search_service_list` | ❌ |

---

## Test 266

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.584494 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.531083 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `azmcp_grafana_list` | ❌ |
| 5 | 0.485219 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 267

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `azmcp_grafana_list` | ❌ |
| 3 | 0.579478 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.522728 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.521317 | `azmcp_monitor_table_type_list` | ❌ |

---

## Test 268

**Expected Tool:** `azmcp_monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591648 | `azmcp_monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.498269 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.494715 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.485439 | `azmcp_monitor_table_list` | ❌ |
| 5 | 0.483323 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 269

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668828 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.413630 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.413173 | `azmcp_monitor_metrics_query` | ❌ |
| 4 | 0.401731 | `azmcp_grafana_list` | ❌ |
| 5 | 0.393318 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 270

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443481 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.385026 | `azmcp_loadtesting_testresource_list` | ❌ |
| 4 | 0.371017 | `azmcp_grafana_list` | ❌ |
| 5 | 0.370681 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 271

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533387 | `azmcp_quota_usage_check` | ❌ |
| 2 | 0.481143 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.476826 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 4 | 0.471499 | `azmcp_subscription_list` | ❌ |
| 5 | 0.468268 | `azmcp_applens_resource_diagnose` | ❌ |

---

## Test 272

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.476164 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 4 | 0.473365 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.465616 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 273

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536934 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.514978 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 4 | 0.504902 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.494872 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 274

**Expected Tool:** `azmcp_quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590356 | `azmcp_quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.414344 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.373478 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369953 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.363130 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 275

**Expected Tool:** `azmcp_quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609680 | `azmcp_quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `azmcp_quota_region_availability_list` | ❌ |
| 3 | 0.399128 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 4 | 0.384350 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.358217 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 276

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645205 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.539760 | `azmcp_subscription_list` | ❌ |
| 3 | 0.483988 | `azmcp_group_list` | ❌ |
| 4 | 0.478700 | `azmcp_grafana_list` | ❌ |
| 5 | 0.471431 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 277

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609603 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.514725 | `azmcp_subscription_list` | ❌ |
| 3 | 0.456951 | `azmcp_grafana_list` | ❌ |
| 4 | 0.449110 | `azmcp_eventgrid_subscription_list` | ❌ |
| 5 | 0.435170 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 278

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757071 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.568367 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.448139 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.377563 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.322930 | `azmcp_mysql_database_list` | ❌ |

---

## Test 279

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713891 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.564079 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.450190 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.338859 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.293658 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 280

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793588 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.660160 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.509846 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.501925 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 5 | 0.494969 | `azmcp_postgres_server_list` | ❌ |

---

## Test 281

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643388 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.523980 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.450471 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.401235 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.302323 | `azmcp_mysql_database_list` | ❌ |

---

## Test 282

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.751222 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.631079 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.461648 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.434924 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.427325 | `azmcp_grafana_list` | ❌ |

---

## Test 283

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752920 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.643558 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.618538 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.548251 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.538411 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 284

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** Show me the databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.721489 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.624899 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.560301 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.494191 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.490923 | `azmcp_mysql_database_list` | ❌ |

---

## Test 285

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.844751 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.733511 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.665339 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.588946 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.571642 | `azmcp_kusto_database_list` | ❌ |

---

## Test 286

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687977 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.533449 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.514374 | `azmcp_redis_cluster_database_list` | ❌ |
| 4 | 0.448590 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.395942 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 287

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me the Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.797214 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.637018 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.632988 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.518857 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.515638 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 288

**Expected Tool:** `azmcp_group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755935 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.566552 | `azmcp_workbooks_list` | ❌ |
| 3 | 0.564489 | `azmcp_loadtesting_testresource_list` | ❌ |
| 4 | 0.552633 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.546156 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 289

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.463685 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.462303 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.460110 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.459304 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 290

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665772 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.532054 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.531920 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.529546 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 291

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643459 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377957 | `azmcp_quota_usage_check` | ❌ |
| 4 | 0.349981 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.349613 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 292

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609642 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.549305 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.510357 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.466885 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.455902 | `azmcp_storage_account_create` | ❌ |

---

## Test 293

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638408 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.577398 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.424930 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.411959 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.393454 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |

---

## Test 294

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.592648 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.549827 | `azmcp_loadtesting_testresource_list` | ❌ |
| 4 | 0.548549 | `azmcp_grafana_list` | ❌ |
| 5 | 0.544505 | `azmcp_subscription_list` | ❌ |

---

## Test 295

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.609494 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.509643 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.508683 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.473905 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 296

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612392 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 2 | 0.596890 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 3 | 0.496564 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.441797 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.433459 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 297

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690763 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.553485 | `azmcp_search_service_list` | ❌ |
| 3 | 0.534251 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.529903 | `azmcp_eventgrid_subscription_list` | ❌ |
| 5 | 0.518372 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 298

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686517 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.534806 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.513302 | `azmcp_search_service_list` | ❌ |
| 4 | 0.513259 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.501135 | `azmcp_subscription_list` | ❌ |

---

## Test 299

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.450703 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.275820 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.267671 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.246236 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.216847 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 300

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685335 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.528131 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.524060 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.518716 | `azmcp_search_service_list` | ❌ |
| 5 | 0.501966 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 301

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565771 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.436322 | `azmcp_search_service_list` | ❌ |
| 3 | 0.403798 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.402493 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.402195 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 302

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `azmcp_servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.438253 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.360755 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.352725 | `azmcp_storage_account_get` | ❌ |

---

## Test 303

**Expected Tool:** `azmcp_servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643193 | `azmcp_servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571860 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.483976 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.482958 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.458882 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 304

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `azmcp_servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.518284 | `azmcp_servicebus_topic_details` | ❌ |
| 3 | 0.494515 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.493853 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.472270 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 305

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516724 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470875 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.420426 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.408694 | `azmcp_sql_db_delete` | ❌ |
| 5 | 0.404777 | `azmcp_sql_server_delete` | ❌ |

---

## Test 306

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571750 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459546 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.437467 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.423988 | `azmcp_appservice_database_add` | ❌ |
| 5 | 0.421727 | `azmcp_sql_db_show` | ❌ |

---

## Test 307

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545906 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.504013 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.494988 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.473975 | `azmcp_sql_db_list` | ❌ |

---

## Test 308

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.568262 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.567348 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.391509 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.386564 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 5 | 0.365144 | `azmcp_sql_db_show` | ❌ |

---

## Test 309

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567507 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.543440 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.501201 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.481083 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.478729 | `azmcp_sql_db_list` | ❌ |

---

## Test 310

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509932 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.490817 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.364399 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.355416 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.347837 | `azmcp_sql_db_rename` | ❌ |

---

## Test 311

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639694 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.609101 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.602893 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.570140 | `azmcp_kusto_database_list` | ❌ |

---

## Test 312

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617696 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.609394 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 3 | 0.557385 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.553495 | `azmcp_mysql_server_config_get` | ❌ |
| 5 | 0.525097 | `azmcp_sql_db_show` | ❌ |

---

## Test 313

**Expected Tool:** `azmcp_sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593352 | `azmcp_sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.425200 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.416270 | `azmcp_sql_db_delete` | ❌ |
| 4 | 0.397010 | `azmcp_sql_db_create` | ❌ |
| 5 | 0.346473 | `azmcp_sql_db_show` | ❌ |

---

## Test 314

**Expected Tool:** `azmcp_sql_db_rename`  
**Prompt:** Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711133 | `azmcp_sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.516382 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.506614 | `azmcp_sql_db_delete` | ❌ |
| 4 | 0.501529 | `azmcp_sql_db_create` | ❌ |
| 5 | 0.433739 | `azmcp_sql_server_show` | ❌ |

---

## Test 315

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610909 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.593150 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.530422 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.528823 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465693 | `azmcp_sql_db_list` | ❌ |

---

## Test 316

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530700 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503592 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.440073 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.438622 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.432919 | `azmcp_mysql_database_list` | ❌ |

---

## Test 317

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603366 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.440493 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.428029 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.413892 | `azmcp_sql_server_delete` | ❌ |

---

## Test 318

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550556 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.418292 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.401817 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.395518 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.395280 | `azmcp_sql_db_show` | ❌ |

---

## Test 319

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678124 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.498367 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.479012 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.475439 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 320

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606433 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502787 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.457125 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.445306 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.432778 | `azmcp_mysql_database_list` | ❌ |

---

## Test 321

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592743 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420332 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.402545 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.397672 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.397582 | `azmcp_sql_server_show` | ❌ |

---

## Test 322

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682605 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563707 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.529507 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.482102 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.474207 | `azmcp_sql_db_rename` | ❌ |

---

## Test 323

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618309 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510169 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.472338 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.441122 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.400939 | `azmcp_sql_db_rename` | ❌ |

---

## Test 324

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589811 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.501412 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.498434 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.461200 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.442979 | `azmcp_mysql_server_list` | ❌ |

---

## Test 325

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656555 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.548123 | `azmcp_sql_db_delete` | ❌ |
| 3 | 0.518342 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.495550 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.483132 | `azmcp_workbooks_delete` | ❌ |

---

## Test 326

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615030 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393885 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.379875 | `azmcp_sql_db_delete` | ❌ |
| 4 | 0.376524 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.350236 | `azmcp_sql_server_list` | ❌ |

---

## Test 327

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624269 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.454984 | `azmcp_sql_db_delete` | ❌ |
| 3 | 0.362389 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.341381 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.315820 | `azmcp_workbooks_delete` | ❌ |

---

## Test 328

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783479 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.456011 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.435044 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.401883 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376055 | `azmcp_sql_db_list` | ❌ |

---

## Test 329

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713358 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.413077 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.368142 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.315967 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.311136 | `azmcp_postgres_server_list` | ❌ |

---

## Test 330

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646419 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.355981 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.322476 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.307823 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.269744 | `azmcp_sql_server_delete` | ❌ |

---

## Test 331

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634907 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532663 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448822 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.440785 | `azmcp_sql_server_delete` | ❌ |

---

## Test 332

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670467 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533360 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503587 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316521 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.302156 | `azmcp_sql_server_delete` | ❌ |

---

## Test 333

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684500 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574331 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539577 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428920 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.395165 | `azmcp_sql_db_create` | ❌ |

---

## Test 334

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.584322 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.543836 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 4 | 0.539768 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 5 | 0.498540 | `azmcp_sql_db_delete` | ❌ |

---

## Test 335

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670179 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574344 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.529996 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.488362 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.360444 | `azmcp_sql_db_delete` | ❌ |

---

## Test 336

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671211 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601238 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.576781 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.499201 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.378659 | `azmcp_sql_db_delete` | ❌ |

---

## Test 337

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729367 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549064 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468745 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.418853 | `azmcp_sql_server_list` | ❌ |

---

## Test 338

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630712 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.523635 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410611 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.348108 | `azmcp_sql_server_list` | ❌ |

---

## Test 339

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630527 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532034 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473501 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412900 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.350469 | `azmcp_sql_server_list` | ❌ |

---

## Test 340

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694608 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596672 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.578238 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.515851 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509906 | `azmcp_sql_db_show` | ❌ |

---

## Test 341

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618522 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593796 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.542398 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.507404 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.496200 | `azmcp_group_list` | ❌ |

---

## Test 342

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629852 | `azmcp_sql_db_show` | ❌ |
| 2 | 0.595128 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587841 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.559922 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.540218 | `azmcp_sql_db_list` | ❌ |

---

## Test 343

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658738 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.538034 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.471959 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.445382 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 344

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563152 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.379957 | `azmcp_postgres_server_param_get` | ❌ |
| 4 | 0.372146 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370895 | `azmcp_sql_db_show` | ❌ |

---

## Test 345

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.418430 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.394541 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.368591 | `azmcp_loadtesting_test_create` | ❌ |
| 5 | 0.357941 | `azmcp_loadtesting_testresource_create` | ❌ |

---

## Test 346

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.400151 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.386985 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.382875 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 5 | 0.377221 | `azmcp_sql_db_create` | ❌ |

---

## Test 347

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589087 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.464637 | `azmcp_storage_blob_container_create` | ❌ |
| 3 | 0.447186 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.437043 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.407443 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 348

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655156 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.603853 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.507541 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.483435 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.443365 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 349

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676866 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.612889 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.518215 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.514979 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.464285 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 350

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663972 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557015 | `azmcp_azuremanagedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.547647 | `azmcp_subscription_list` | ❌ |
| 4 | 0.536920 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.535616 | `azmcp_storage_account_create` | ❌ |

---

## Test 351

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499314 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.461263 | `azmcp_azuremanagedlustre_filesystem_list` | ❌ |
| 3 | 0.455477 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.421579 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.415346 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 352

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557171 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.473641 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.465571 | `azmcp_subscription_list` | ❌ |
| 4 | 0.461641 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.434007 | `azmcp_search_service_list` | ❌ |

---

## Test 353

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563396 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.524779 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.508053 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.447784 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.403404 | `azmcp_storage_account_get` | ❌ |

---

## Test 354

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512578 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.500625 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.470927 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415378 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.414846 | `azmcp_storage_blob_get` | ❌ |

---

## Test 355

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463198 | `azmcp_storage_account_create` | ❌ |
| 2 | 0.455376 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.451690 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 4 | 0.435099 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.407774 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 356

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665176 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.559166 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.523289 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.518713 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.496184 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 357

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613933 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.605437 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 3 | 0.522323 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.481109 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.479028 | `azmcp_storage_account_get` | ❌ |

---

## Test 358

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625166 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.511262 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.479612 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.439698 | `azmcp_storage_account_create` | ❌ |

---

## Test 359

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613044 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.586289 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.483547 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.477946 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.442817 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 360

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662093 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.661710 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.537423 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.460686 | `azmcp_storage_blob_container_create` | ❌ |
| 5 | 0.456961 | `azmcp_storage_account_create` | ❌ |

---

## Test 361

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592723 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.579070 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.568872 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 4 | 0.506685 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.465947 | `azmcp_storage_account_get` | ❌ |

---

## Test 362

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570353 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.549720 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.533515 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.484079 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.449091 | `azmcp_storage_account_get` | ❌ |

---

## Test 363

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566287 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.403607 | `azmcp_storage_blob_get` | ❌ |
| 3 | 0.397722 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.382123 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.377255 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 364

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654071 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512950 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.471653 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.469020 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.457850 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 365

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.458821 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.407478 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.393695 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.381238 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.366185 | `azmcp_redis_cache_list` | ❌ |

---

## Test 366

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433196 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.315547 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.293904 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.289334 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.288406 | `azmcp_redis_cache_list` | ❌ |

---

## Test 367

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.477591 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.357738 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.340836 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.340339 | `azmcp_grafana_list` | ❌ |
| 5 | 0.336798 | `azmcp_postgres_server_list` | ❌ |

---

## Test 368

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

---

## Test 369

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.510005 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.473597 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.444297 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 370

**Expected Tool:** `azmcp_virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711981 | `azmcp_virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659744 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.620663 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.546744 | `azmcp_search_service_list` | ❌ |
| 5 | 0.535739 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 371

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726982 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714468 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573406 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.423250 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.393721 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 372

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812659 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.659093 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501192 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.336848 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.336385 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 373

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

## Test 374

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

## Test 375

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

---

## Test 376

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

---

## Test 377

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

## Test 378

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

## Test 379

**Expected Tool:** `azmcp_workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469915 | `azmcp_workbooks_update` | ✅ **EXPECTED** |
| 2 | 0.382651 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.362354 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.349689 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.292904 | `azmcp_loadtesting_testrun_update` | ❌ |

---

## Test 380

**Expected Tool:** `azmcp_bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485970 | `azmcp_foundry_models_deploy` | ❌ |
| 2 | 0.485889 | `azmcp_deploy_iac_rules_get` | ❌ |
| 3 | 0.448373 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.440302 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.432773 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 381

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500887 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290902 | `azmcp_storage_blob_upload` | ❌ |
| 3 | 0.254991 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.245034 | `azmcp_azuremanagedlustre_filesystem_subnetsize_validate` | ❌ |
| 5 | 0.221349 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 382

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.403411 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.267683 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258160 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.225870 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.225622 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 383

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.421506 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242581 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.218064 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 384

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533774 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.352797 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.323920 | `azmcp_storage_blob_upload` | ❌ |
| 5 | 0.323561 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Summary

**Total Prompts Tested:** 384  
**Analysis Execution Time:** 85.3865012s  

### Success Rate Metrics

**Top Choice Success:** 91.9% (353/384 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 3.4% (13/384 tests)  
**🎯 High Confidence (≥0.7):** 18.0% (69/384 tests)  
**✅ Good Confidence (≥0.6):** 59.1% (227/384 tests)  
**👍 Fair Confidence (≥0.5):** 89.6% (344/384 tests)  
**👌 Acceptable Confidence (≥0.4):** 98.7% (379/384 tests)  
**❌ Low Confidence (<0.4):** 1.3% (5/384 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 3.4% (13/384 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 17.7% (68/384 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 57.0% (219/384 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 83.6% (321/384 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 90.6% (348/384 tests)  

### Success Rate Analysis

🟢 **Excellent** - The tool selection system is performing very well.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

