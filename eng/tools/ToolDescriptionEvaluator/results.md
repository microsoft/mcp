# Tool Selection Analysis Setup

**Setup completed:** 2025-10-10 17:30:59  
**Tool count:** 157  
**Database setup time:** 1.7363357s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-10-10 17:30:59  
**Tool count:** 157  

## Table of Contents

- [Test 1: azmcp_foundry_agents_connect](#test-1)
- [Test 2: azmcp_foundry_agents_evaluate](#test-2)
- [Test 3: azmcp_foundry_agents_list](#test-3)
- [Test 4: azmcp_foundry_agents_list](#test-4)
- [Test 5: azmcp_foundry_agents_query-and-evaluate](#test-5)
- [Test 6: azmcp_foundry_knowledge_index_list](#test-6)
- [Test 7: azmcp_foundry_knowledge_index_list](#test-7)
- [Test 8: azmcp_foundry_knowledge_index_schema](#test-8)
- [Test 9: azmcp_foundry_knowledge_index_schema](#test-9)
- [Test 10: azmcp_foundry_models_deploy](#test-10)
- [Test 11: azmcp_foundry_models_deployments_list](#test-11)
- [Test 12: azmcp_foundry_models_deployments_list](#test-12)
- [Test 13: azmcp_foundry_models_list](#test-13)
- [Test 14: azmcp_foundry_models_list](#test-14)
- [Test 15: azmcp_foundry_openai_chat-completions-create](#test-15)
- [Test 16: azmcp_foundry_openai_create-completion](#test-16)
- [Test 17: azmcp_foundry_openai_embeddings-create](#test-17)
- [Test 18: azmcp_foundry_openai_embeddings-create](#test-18)
- [Test 19: azmcp_foundry_openai_models-list](#test-19)
- [Test 20: azmcp_foundry_openai_models-list](#test-20)
- [Test 21: azmcp_foundry_resource_get](#test-21)
- [Test 22: azmcp_foundry_resource_get](#test-22)
- [Test 23: azmcp_foundry_resource_get](#test-23)
- [Test 24: azmcp_search_knowledge_base_get](#test-24)
- [Test 25: azmcp_search_knowledge_base_get](#test-25)
- [Test 26: azmcp_search_knowledge_base_get](#test-26)
- [Test 27: azmcp_search_knowledge_base_get](#test-27)
- [Test 28: azmcp_search_knowledge_base_get](#test-28)
- [Test 29: azmcp_search_knowledge_base_get](#test-29)
- [Test 30: azmcp_search_knowledge_base_retrieve](#test-30)
- [Test 31: azmcp_search_knowledge_base_retrieve](#test-31)
- [Test 32: azmcp_search_knowledge_base_retrieve](#test-32)
- [Test 33: azmcp_search_knowledge_base_retrieve](#test-33)
- [Test 34: azmcp_search_knowledge_base_retrieve](#test-34)
- [Test 35: azmcp_search_knowledge_base_retrieve](#test-35)
- [Test 36: azmcp_search_knowledge_base_retrieve](#test-36)
- [Test 37: azmcp_search_knowledge_base_retrieve](#test-37)
- [Test 38: azmcp_search_knowledge_source_get](#test-38)
- [Test 39: azmcp_search_knowledge_source_get](#test-39)
- [Test 40: azmcp_search_knowledge_source_get](#test-40)
- [Test 41: azmcp_search_knowledge_source_get](#test-41)
- [Test 42: azmcp_search_knowledge_source_get](#test-42)
- [Test 43: azmcp_search_knowledge_source_get](#test-43)
- [Test 44: azmcp_search_index_get](#test-44)
- [Test 45: azmcp_search_index_get](#test-45)
- [Test 46: azmcp_search_index_get](#test-46)
- [Test 47: azmcp_search_index_query](#test-47)
- [Test 48: azmcp_search_service_list](#test-48)
- [Test 49: azmcp_search_service_list](#test-49)
- [Test 50: azmcp_search_service_list](#test-50)
- [Test 51: azmcp_speech_stt_recognize](#test-51)
- [Test 52: azmcp_speech_stt_recognize](#test-52)
- [Test 53: azmcp_speech_stt_recognize](#test-53)
- [Test 54: azmcp_speech_stt_recognize](#test-54)
- [Test 55: azmcp_speech_stt_recognize](#test-55)
- [Test 56: azmcp_speech_stt_recognize](#test-56)
- [Test 57: azmcp_speech_stt_recognize](#test-57)
- [Test 58: azmcp_speech_stt_recognize](#test-58)
- [Test 59: azmcp_speech_stt_recognize](#test-59)
- [Test 60: azmcp_speech_stt_recognize](#test-60)
- [Test 61: azmcp_appconfig_account_list](#test-61)
- [Test 62: azmcp_appconfig_account_list](#test-62)
- [Test 63: azmcp_appconfig_account_list](#test-63)
- [Test 64: azmcp_appconfig_kv_delete](#test-64)
- [Test 65: azmcp_appconfig_kv_get](#test-65)
- [Test 66: azmcp_appconfig_kv_get](#test-66)
- [Test 67: azmcp_appconfig_kv_get](#test-67)
- [Test 68: azmcp_appconfig_kv_get](#test-68)
- [Test 69: azmcp_appconfig_kv_lock_set](#test-69)
- [Test 70: azmcp_appconfig_kv_lock_set](#test-70)
- [Test 71: azmcp_appconfig_kv_set](#test-71)
- [Test 72: azmcp_applens_resource_diagnose](#test-72)
- [Test 73: azmcp_applens_resource_diagnose](#test-73)
- [Test 74: azmcp_applens_resource_diagnose](#test-74)
- [Test 75: azmcp_appservice_database_add](#test-75)
- [Test 76: azmcp_appservice_database_add](#test-76)
- [Test 77: azmcp_appservice_database_add](#test-77)
- [Test 78: azmcp_appservice_database_add](#test-78)
- [Test 79: azmcp_appservice_database_add](#test-79)
- [Test 80: azmcp_appservice_database_add](#test-80)
- [Test 81: azmcp_appservice_database_add](#test-81)
- [Test 82: azmcp_appservice_database_add](#test-82)
- [Test 83: azmcp_appservice_database_add](#test-83)
- [Test 84: azmcp_applicationinsights_recommendation_list](#test-84)
- [Test 85: azmcp_applicationinsights_recommendation_list](#test-85)
- [Test 86: azmcp_applicationinsights_recommendation_list](#test-86)
- [Test 87: azmcp_applicationinsights_recommendation_list](#test-87)
- [Test 88: azmcp_extension_cli_generate](#test-88)
- [Test 89: azmcp_extension_cli_generate](#test-89)
- [Test 90: azmcp_extension_cli_generate](#test-90)
- [Test 91: azmcp_acr_registry_list](#test-91)
- [Test 92: azmcp_acr_registry_list](#test-92)
- [Test 93: azmcp_acr_registry_list](#test-93)
- [Test 94: azmcp_acr_registry_list](#test-94)
- [Test 95: azmcp_acr_registry_list](#test-95)
- [Test 96: azmcp_acr_registry_repository_list](#test-96)
- [Test 97: azmcp_acr_registry_repository_list](#test-97)
- [Test 98: azmcp_acr_registry_repository_list](#test-98)
- [Test 99: azmcp_acr_registry_repository_list](#test-99)
- [Test 100: azmcp_communication_email_send](#test-100)
- [Test 101: azmcp_communication_email_send](#test-101)
- [Test 102: azmcp_communication_email_send](#test-102)
- [Test 103: azmcp_communication_email_send](#test-103)
- [Test 104: azmcp_communication_email_send](#test-104)
- [Test 105: azmcp_communication_email_send](#test-105)
- [Test 106: azmcp_communication_email_send](#test-106)
- [Test 107: azmcp_communication_email_send](#test-107)
- [Test 108: azmcp_communication_sms_send](#test-108)
- [Test 109: azmcp_communication_sms_send](#test-109)
- [Test 110: azmcp_communication_sms_send](#test-110)
- [Test 111: azmcp_communication_sms_send](#test-111)
- [Test 112: azmcp_communication_sms_send](#test-112)
- [Test 113: azmcp_communication_sms_send](#test-113)
- [Test 114: azmcp_communication_sms_send](#test-114)
- [Test 115: azmcp_communication_sms_send](#test-115)
- [Test 116: azmcp_confidentialledger_entries_append](#test-116)
- [Test 117: azmcp_confidentialledger_entries_append](#test-117)
- [Test 118: azmcp_confidentialledger_entries_append](#test-118)
- [Test 119: azmcp_confidentialledger_entries_append](#test-119)
- [Test 120: azmcp_confidentialledger_entries_append](#test-120)
- [Test 121: azmcp_confidentialledger_entries_get](#test-121)
- [Test 122: azmcp_confidentialledger_entries_get](#test-122)
- [Test 123: azmcp_cosmos_account_list](#test-123)
- [Test 124: azmcp_cosmos_account_list](#test-124)
- [Test 125: azmcp_cosmos_account_list](#test-125)
- [Test 126: azmcp_cosmos_database_container_item_query](#test-126)
- [Test 127: azmcp_cosmos_database_container_list](#test-127)
- [Test 128: azmcp_cosmos_database_container_list](#test-128)
- [Test 129: azmcp_cosmos_database_list](#test-129)
- [Test 130: azmcp_cosmos_database_list](#test-130)
- [Test 131: azmcp_kusto_cluster_get](#test-131)
- [Test 132: azmcp_kusto_cluster_list](#test-132)
- [Test 133: azmcp_kusto_cluster_list](#test-133)
- [Test 134: azmcp_kusto_cluster_list](#test-134)
- [Test 135: azmcp_kusto_database_list](#test-135)
- [Test 136: azmcp_kusto_database_list](#test-136)
- [Test 137: azmcp_kusto_query](#test-137)
- [Test 138: azmcp_kusto_sample](#test-138)
- [Test 139: azmcp_kusto_table_list](#test-139)
- [Test 140: azmcp_kusto_table_list](#test-140)
- [Test 141: azmcp_kusto_table_schema](#test-141)
- [Test 142: azmcp_mysql_database_list](#test-142)
- [Test 143: azmcp_mysql_database_list](#test-143)
- [Test 144: azmcp_mysql_database_query](#test-144)
- [Test 145: azmcp_mysql_server_config_get](#test-145)
- [Test 146: azmcp_mysql_server_list](#test-146)
- [Test 147: azmcp_mysql_server_list](#test-147)
- [Test 148: azmcp_mysql_server_list](#test-148)
- [Test 149: azmcp_mysql_server_param_get](#test-149)
- [Test 150: azmcp_mysql_server_param_set](#test-150)
- [Test 151: azmcp_mysql_table_list](#test-151)
- [Test 152: azmcp_mysql_table_list](#test-152)
- [Test 153: azmcp_mysql_table_schema_get](#test-153)
- [Test 154: azmcp_postgres_database_list](#test-154)
- [Test 155: azmcp_postgres_database_list](#test-155)
- [Test 156: azmcp_postgres_database_query](#test-156)
- [Test 157: azmcp_postgres_server_config_get](#test-157)
- [Test 158: azmcp_postgres_server_list](#test-158)
- [Test 159: azmcp_postgres_server_list](#test-159)
- [Test 160: azmcp_postgres_server_list](#test-160)
- [Test 161: azmcp_postgres_server_param_get](#test-161)
- [Test 162: azmcp_postgres_server_param_set](#test-162)
- [Test 163: azmcp_postgres_table_list](#test-163)
- [Test 164: azmcp_postgres_table_list](#test-164)
- [Test 165: azmcp_postgres_table_schema_get](#test-165)
- [Test 166: azmcp_deploy_app_logs_get](#test-166)
- [Test 167: azmcp_deploy_architecture_diagram_generate](#test-167)
- [Test 168: azmcp_deploy_iac_rules_get](#test-168)
- [Test 169: azmcp_deploy_pipeline_guidance_get](#test-169)
- [Test 170: azmcp_deploy_plan_get](#test-170)
- [Test 171: azmcp_eventgrid_events_publish](#test-171)
- [Test 172: azmcp_eventgrid_events_publish](#test-172)
- [Test 173: azmcp_eventgrid_events_publish](#test-173)
- [Test 174: azmcp_eventgrid_topic_list](#test-174)
- [Test 175: azmcp_eventgrid_topic_list](#test-175)
- [Test 176: azmcp_eventgrid_topic_list](#test-176)
- [Test 177: azmcp_eventgrid_topic_list](#test-177)
- [Test 178: azmcp_eventgrid_subscription_list](#test-178)
- [Test 179: azmcp_eventgrid_subscription_list](#test-179)
- [Test 180: azmcp_eventgrid_subscription_list](#test-180)
- [Test 181: azmcp_eventgrid_subscription_list](#test-181)
- [Test 182: azmcp_eventgrid_subscription_list](#test-182)
- [Test 183: azmcp_eventgrid_subscription_list](#test-183)
- [Test 184: azmcp_eventgrid_subscription_list](#test-184)
- [Test 185: azmcp_eventhubs_namespace_get](#test-185)
- [Test 186: azmcp_eventhubs_namespace_get](#test-186)
- [Test 187: azmcp_eventhubs_namespace_update](#test-187)
- [Test 188: azmcp_eventhubs_namespace_update](#test-188)
- [Test 189: azmcp_eventhubs_namespace_delete](#test-189)
- [Test 190: azmcp_eventhubs_eventhub_get](#test-190)
- [Test 191: azmcp_eventhubs_eventhub_get](#test-191)
- [Test 192: azmcp_eventhubs_eventhub_update](#test-192)
- [Test 193: azmcp_eventhubs_eventhub_update](#test-193)
- [Test 194: azmcp_eventhubs_eventhub_delete](#test-194)
- [Test 195: azmcp_eventhubs_eventhub_consumergroup_get](#test-195)
- [Test 196: azmcp_eventhubs_eventhub_consumergroup_get](#test-196)
- [Test 197: azmcp_eventhubs_eventhub_consumergroup_update](#test-197)
- [Test 198: azmcp_eventhubs_eventhub_consumergroup_update](#test-198)
- [Test 199: azmcp_eventhubs_eventhub_consumergroup_delete](#test-199)
- [Test 200: azmcp_functionapp_get](#test-200)
- [Test 201: azmcp_functionapp_get](#test-201)
- [Test 202: azmcp_functionapp_get](#test-202)
- [Test 203: azmcp_functionapp_get](#test-203)
- [Test 204: azmcp_functionapp_get](#test-204)
- [Test 205: azmcp_functionapp_get](#test-205)
- [Test 206: azmcp_functionapp_get](#test-206)
- [Test 207: azmcp_functionapp_get](#test-207)
- [Test 208: azmcp_functionapp_get](#test-208)
- [Test 209: azmcp_functionapp_get](#test-209)
- [Test 210: azmcp_functionapp_get](#test-210)
- [Test 211: azmcp_functionapp_get](#test-211)
- [Test 212: azmcp_keyvault_admin_settings_get](#test-212)
- [Test 213: azmcp_keyvault_admin_settings_get](#test-213)
- [Test 214: azmcp_keyvault_admin_settings_get](#test-214)
- [Test 215: azmcp_keyvault_certificate_create](#test-215)
- [Test 216: azmcp_keyvault_certificate_create](#test-216)
- [Test 217: azmcp_keyvault_certificate_create](#test-217)
- [Test 218: azmcp_keyvault_certificate_create](#test-218)
- [Test 219: azmcp_keyvault_certificate_create](#test-219)
- [Test 220: azmcp_keyvault_certificate_get](#test-220)
- [Test 221: azmcp_keyvault_certificate_get](#test-221)
- [Test 222: azmcp_keyvault_certificate_get](#test-222)
- [Test 223: azmcp_keyvault_certificate_get](#test-223)
- [Test 224: azmcp_keyvault_certificate_get](#test-224)
- [Test 225: azmcp_keyvault_certificate_import](#test-225)
- [Test 226: azmcp_keyvault_certificate_import](#test-226)
- [Test 227: azmcp_keyvault_certificate_import](#test-227)
- [Test 228: azmcp_keyvault_certificate_import](#test-228)
- [Test 229: azmcp_keyvault_certificate_import](#test-229)
- [Test 230: azmcp_keyvault_certificate_list](#test-230)
- [Test 231: azmcp_keyvault_certificate_list](#test-231)
- [Test 232: azmcp_keyvault_certificate_list](#test-232)
- [Test 233: azmcp_keyvault_certificate_list](#test-233)
- [Test 234: azmcp_keyvault_certificate_list](#test-234)
- [Test 235: azmcp_keyvault_certificate_list](#test-235)
- [Test 236: azmcp_keyvault_key_create](#test-236)
- [Test 237: azmcp_keyvault_key_create](#test-237)
- [Test 238: azmcp_keyvault_key_create](#test-238)
- [Test 239: azmcp_keyvault_key_create](#test-239)
- [Test 240: azmcp_keyvault_key_create](#test-240)
- [Test 241: azmcp_keyvault_key_get](#test-241)
- [Test 242: azmcp_keyvault_key_get](#test-242)
- [Test 243: azmcp_keyvault_key_get](#test-243)
- [Test 244: azmcp_keyvault_key_get](#test-244)
- [Test 245: azmcp_keyvault_key_get](#test-245)
- [Test 246: azmcp_keyvault_key_list](#test-246)
- [Test 247: azmcp_keyvault_key_list](#test-247)
- [Test 248: azmcp_keyvault_key_list](#test-248)
- [Test 249: azmcp_keyvault_key_list](#test-249)
- [Test 250: azmcp_keyvault_key_list](#test-250)
- [Test 251: azmcp_keyvault_key_list](#test-251)
- [Test 252: azmcp_keyvault_secret_create](#test-252)
- [Test 253: azmcp_keyvault_secret_create](#test-253)
- [Test 254: azmcp_keyvault_secret_create](#test-254)
- [Test 255: azmcp_keyvault_secret_create](#test-255)
- [Test 256: azmcp_keyvault_secret_create](#test-256)
- [Test 257: azmcp_keyvault_secret_get](#test-257)
- [Test 258: azmcp_keyvault_secret_get](#test-258)
- [Test 259: azmcp_keyvault_secret_get](#test-259)
- [Test 260: azmcp_keyvault_secret_get](#test-260)
- [Test 261: azmcp_keyvault_secret_get](#test-261)
- [Test 262: azmcp_keyvault_secret_list](#test-262)
- [Test 263: azmcp_keyvault_secret_list](#test-263)
- [Test 264: azmcp_keyvault_secret_list](#test-264)
- [Test 265: azmcp_keyvault_secret_list](#test-265)
- [Test 266: azmcp_keyvault_secret_list](#test-266)
- [Test 267: azmcp_keyvault_secret_list](#test-267)
- [Test 268: azmcp_aks_cluster_get](#test-268)
- [Test 269: azmcp_aks_cluster_get](#test-269)
- [Test 270: azmcp_aks_cluster_get](#test-270)
- [Test 271: azmcp_aks_cluster_get](#test-271)
- [Test 272: azmcp_aks_cluster_get](#test-272)
- [Test 273: azmcp_aks_cluster_get](#test-273)
- [Test 274: azmcp_aks_cluster_get](#test-274)
- [Test 275: azmcp_aks_nodepool_get](#test-275)
- [Test 276: azmcp_aks_nodepool_get](#test-276)
- [Test 277: azmcp_aks_nodepool_get](#test-277)
- [Test 278: azmcp_aks_nodepool_get](#test-278)
- [Test 279: azmcp_aks_nodepool_get](#test-279)
- [Test 280: azmcp_aks_nodepool_get](#test-280)
- [Test 281: azmcp_loadtesting_test_create](#test-281)
- [Test 282: azmcp_loadtesting_test_get](#test-282)
- [Test 283: azmcp_loadtesting_testresource_create](#test-283)
- [Test 284: azmcp_loadtesting_testresource_list](#test-284)
- [Test 285: azmcp_loadtesting_testrun_create](#test-285)
- [Test 286: azmcp_loadtesting_testrun_get](#test-286)
- [Test 287: azmcp_loadtesting_testrun_list](#test-287)
- [Test 288: azmcp_loadtesting_testrun_update](#test-288)
- [Test 289: azmcp_grafana_list](#test-289)
- [Test 290: azmcp_managedlustre_filesystem_create](#test-290)
- [Test 291: azmcp_managedlustre_filesystem_list](#test-291)
- [Test 292: azmcp_managedlustre_filesystem_list](#test-292)
- [Test 293: azmcp_managedlustre_filesystem_sku_get](#test-293)
- [Test 294: azmcp_managedlustre_filesystem_subnetsize_ask](#test-294)
- [Test 295: azmcp_managedlustre_filesystem_subnetsize_validate](#test-295)
- [Test 296: azmcp_managedlustre_filesystem_update](#test-296)
- [Test 297: azmcp_marketplace_product_get](#test-297)
- [Test 298: azmcp_marketplace_product_list](#test-298)
- [Test 299: azmcp_marketplace_product_list](#test-299)
- [Test 300: azmcp_get_bestpractices_get](#test-300)
- [Test 301: azmcp_get_bestpractices_get](#test-301)
- [Test 302: azmcp_get_bestpractices_get](#test-302)
- [Test 303: azmcp_get_bestpractices_get](#test-303)
- [Test 304: azmcp_get_bestpractices_get](#test-304)
- [Test 305: azmcp_get_bestpractices_get](#test-305)
- [Test 306: azmcp_get_bestpractices_get](#test-306)
- [Test 307: azmcp_get_bestpractices_get](#test-307)
- [Test 308: azmcp_monitor_activitylog_list](#test-308)
- [Test 309: azmcp_monitor_healthmodels_entity_gethealth](#test-309)
- [Test 310: azmcp_monitor_metrics_definitions](#test-310)
- [Test 311: azmcp_monitor_metrics_definitions](#test-311)
- [Test 312: azmcp_monitor_metrics_definitions](#test-312)
- [Test 313: azmcp_monitor_metrics_query](#test-313)
- [Test 314: azmcp_monitor_metrics_query](#test-314)
- [Test 315: azmcp_monitor_metrics_query](#test-315)
- [Test 316: azmcp_monitor_metrics_query](#test-316)
- [Test 317: azmcp_monitor_metrics_query](#test-317)
- [Test 318: azmcp_monitor_metrics_query](#test-318)
- [Test 319: azmcp_monitor_resource_log_query](#test-319)
- [Test 320: azmcp_monitor_table_list](#test-320)
- [Test 321: azmcp_monitor_table_list](#test-321)
- [Test 322: azmcp_monitor_table_type_list](#test-322)
- [Test 323: azmcp_monitor_table_type_list](#test-323)
- [Test 324: azmcp_monitor_workspace_list](#test-324)
- [Test 325: azmcp_monitor_workspace_list](#test-325)
- [Test 326: azmcp_monitor_workspace_list](#test-326)
- [Test 327: azmcp_monitor_workspace_log_query](#test-327)
- [Test 328: azmcp_datadog_monitoredresources_list](#test-328)
- [Test 329: azmcp_datadog_monitoredresources_list](#test-329)
- [Test 330: azmcp_extension_azqr](#test-330)
- [Test 331: azmcp_extension_azqr](#test-331)
- [Test 332: azmcp_extension_azqr](#test-332)
- [Test 333: azmcp_quota_region_availability_list](#test-333)
- [Test 334: azmcp_quota_usage_check](#test-334)
- [Test 335: azmcp_role_assignment_list](#test-335)
- [Test 336: azmcp_role_assignment_list](#test-336)
- [Test 337: azmcp_redis_cache_accesspolicy_list](#test-337)
- [Test 338: azmcp_redis_cache_accesspolicy_list](#test-338)
- [Test 339: azmcp_redis_cache_list](#test-339)
- [Test 340: azmcp_redis_cache_list](#test-340)
- [Test 341: azmcp_redis_cache_list](#test-341)
- [Test 342: azmcp_redis_cluster_database_list](#test-342)
- [Test 343: azmcp_redis_cluster_database_list](#test-343)
- [Test 344: azmcp_redis_cluster_list](#test-344)
- [Test 345: azmcp_redis_cluster_list](#test-345)
- [Test 346: azmcp_redis_cluster_list](#test-346)
- [Test 347: azmcp_group_list](#test-347)
- [Test 348: azmcp_group_list](#test-348)
- [Test 349: azmcp_group_list](#test-349)
- [Test 350: azmcp_resourcehealth_availability-status_get](#test-350)
- [Test 351: azmcp_resourcehealth_availability-status_get](#test-351)
- [Test 352: azmcp_resourcehealth_availability-status_get](#test-352)
- [Test 353: azmcp_resourcehealth_availability-status_list](#test-353)
- [Test 354: azmcp_resourcehealth_availability-status_list](#test-354)
- [Test 355: azmcp_resourcehealth_availability-status_list](#test-355)
- [Test 356: azmcp_resourcehealth_service-health-events_list](#test-356)
- [Test 357: azmcp_resourcehealth_service-health-events_list](#test-357)
- [Test 358: azmcp_resourcehealth_service-health-events_list](#test-358)
- [Test 359: azmcp_resourcehealth_service-health-events_list](#test-359)
- [Test 360: azmcp_resourcehealth_service-health-events_list](#test-360)
- [Test 361: azmcp_servicebus_queue_details](#test-361)
- [Test 362: azmcp_servicebus_topic_details](#test-362)
- [Test 363: azmcp_servicebus_topic_subscription_details](#test-363)
- [Test 364: azmcp_signalr_runtime_get](#test-364)
- [Test 365: azmcp_signalr_runtime_get](#test-365)
- [Test 366: azmcp_signalr_runtime_get](#test-366)
- [Test 367: azmcp_signalr_runtime_get](#test-367)
- [Test 368: azmcp_signalr_runtime_get](#test-368)
- [Test 369: azmcp_signalr_runtime_get](#test-369)
- [Test 370: azmcp_sql_db_create](#test-370)
- [Test 371: azmcp_sql_db_create](#test-371)
- [Test 372: azmcp_sql_db_create](#test-372)
- [Test 373: azmcp_sql_db_delete](#test-373)
- [Test 374: azmcp_sql_db_delete](#test-374)
- [Test 375: azmcp_sql_db_delete](#test-375)
- [Test 376: azmcp_sql_db_list](#test-376)
- [Test 377: azmcp_sql_db_list](#test-377)
- [Test 378: azmcp_sql_db_rename](#test-378)
- [Test 379: azmcp_sql_db_rename](#test-379)
- [Test 380: azmcp_sql_db_show](#test-380)
- [Test 381: azmcp_sql_db_show](#test-381)
- [Test 382: azmcp_sql_db_update](#test-382)
- [Test 383: azmcp_sql_db_update](#test-383)
- [Test 384: azmcp_sql_elastic-pool_list](#test-384)
- [Test 385: azmcp_sql_elastic-pool_list](#test-385)
- [Test 386: azmcp_sql_elastic-pool_list](#test-386)
- [Test 387: azmcp_sql_server_create](#test-387)
- [Test 388: azmcp_sql_server_create](#test-388)
- [Test 389: azmcp_sql_server_create](#test-389)
- [Test 390: azmcp_sql_server_delete](#test-390)
- [Test 391: azmcp_sql_server_delete](#test-391)
- [Test 392: azmcp_sql_server_delete](#test-392)
- [Test 393: azmcp_sql_server_entra-admin_list](#test-393)
- [Test 394: azmcp_sql_server_entra-admin_list](#test-394)
- [Test 395: azmcp_sql_server_entra-admin_list](#test-395)
- [Test 396: azmcp_sql_server_firewall-rule_create](#test-396)
- [Test 397: azmcp_sql_server_firewall-rule_create](#test-397)
- [Test 398: azmcp_sql_server_firewall-rule_create](#test-398)
- [Test 399: azmcp_sql_server_firewall-rule_delete](#test-399)
- [Test 400: azmcp_sql_server_firewall-rule_delete](#test-400)
- [Test 401: azmcp_sql_server_firewall-rule_delete](#test-401)
- [Test 402: azmcp_sql_server_firewall-rule_list](#test-402)
- [Test 403: azmcp_sql_server_firewall-rule_list](#test-403)
- [Test 404: azmcp_sql_server_firewall-rule_list](#test-404)
- [Test 405: azmcp_sql_server_list](#test-405)
- [Test 406: azmcp_sql_server_list](#test-406)
- [Test 407: azmcp_sql_server_show](#test-407)
- [Test 408: azmcp_sql_server_show](#test-408)
- [Test 409: azmcp_sql_server_show](#test-409)
- [Test 410: azmcp_storage_account_create](#test-410)
- [Test 411: azmcp_storage_account_create](#test-411)
- [Test 412: azmcp_storage_account_create](#test-412)
- [Test 413: azmcp_storage_account_get](#test-413)
- [Test 414: azmcp_storage_account_get](#test-414)
- [Test 415: azmcp_storage_account_get](#test-415)
- [Test 416: azmcp_storage_account_get](#test-416)
- [Test 417: azmcp_storage_account_get](#test-417)
- [Test 418: azmcp_storage_blob_container_create](#test-418)
- [Test 419: azmcp_storage_blob_container_create](#test-419)
- [Test 420: azmcp_storage_blob_container_create](#test-420)
- [Test 421: azmcp_storage_blob_container_get](#test-421)
- [Test 422: azmcp_storage_blob_container_get](#test-422)
- [Test 423: azmcp_storage_blob_container_get](#test-423)
- [Test 424: azmcp_storage_blob_get](#test-424)
- [Test 425: azmcp_storage_blob_get](#test-425)
- [Test 426: azmcp_storage_blob_get](#test-426)
- [Test 427: azmcp_storage_blob_get](#test-427)
- [Test 428: azmcp_storage_blob_upload](#test-428)
- [Test 429: azmcp_subscription_list](#test-429)
- [Test 430: azmcp_subscription_list](#test-430)
- [Test 431: azmcp_subscription_list](#test-431)
- [Test 432: azmcp_subscription_list](#test-432)
- [Test 433: azmcp_azureterraformbestpractices_get](#test-433)
- [Test 434: azmcp_azureterraformbestpractices_get](#test-434)
- [Test 435: azmcp_virtualdesktop_hostpool_list](#test-435)
- [Test 436: azmcp_virtualdesktop_hostpool_sessionhost_list](#test-436)
- [Test 437: azmcp_virtualdesktop_hostpool_sessionhost_usersession-list](#test-437)
- [Test 438: azmcp_workbooks_create](#test-438)
- [Test 439: azmcp_workbooks_delete](#test-439)
- [Test 440: azmcp_workbooks_list](#test-440)
- [Test 441: azmcp_workbooks_list](#test-441)
- [Test 442: azmcp_workbooks_show](#test-442)
- [Test 443: azmcp_workbooks_show](#test-443)
- [Test 444: azmcp_workbooks_update](#test-444)
- [Test 445: azmcp_bicepschema_get](#test-445)
- [Test 446: azmcp_cloudarchitect_design](#test-446)
- [Test 447: azmcp_cloudarchitect_design](#test-447)
- [Test 448: azmcp_cloudarchitect_design](#test-448)
- [Test 449: azmcp_cloudarchitect_design](#test-449)

---

## Test 1

**Expected Tool:** `azmcp_foundry_agents_connect`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622854 | `azmcp_foundry_agents_connect` | ✅ **EXPECTED** |
| 2 | 0.603124 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.494462 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.443011 | `azmcp_foundry_agents_evaluate` | ❌ |
| 5 | 0.379587 | `azmcp_search_index_query` | ❌ |

---

## Test 2

**Expected Tool:** `azmcp_foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544099 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469428 | `azmcp_foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.445964 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.250023 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.235412 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 3

**Expected Tool:** `azmcp_foundry_agents_list`  
**Prompt:** List all agents in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.725613 | `azmcp_foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.468941 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.454081 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.432723 | `azmcp_foundry_models_deployments_list` | ❌ |
| 5 | 0.429039 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 4

**Expected Tool:** `azmcp_foundry_agents_list`  
**Prompt:** Show me the available agents in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714029 | `azmcp_foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.479155 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.466215 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.429842 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.427251 | `azmcp_foundry_models_deployments_list` | ❌ |

---

## Test 5

**Expected Tool:** `azmcp_foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my AI Foundry project for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580566 | `azmcp_foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 2 | 0.568662 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.518655 | `azmcp_foundry_agents_evaluate` | ❌ |
| 4 | 0.381887 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.326026 | `azmcp_foundry_models_deploy` | ❌ |

---

## Test 6

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695201 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.532985 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.526733 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 4 | 0.433117 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.422246 | `azmcp_search_index_get` | ❌ |

---

## Test 7

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603396 | `azmcp_foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489494 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.473949 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.396819 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.374157 | `azmcp_search_index_get` | ❌ |

---

## Test 8

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672734 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.564935 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.423982 | `azmcp_search_index_get` | ❌ |
| 4 | 0.401683 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.397171 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 9

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650189 | `azmcp_foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432751 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.417421 | `azmcp_kusto_table_schema` | ❌ |
| 4 | 0.415963 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.398186 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 10

**Expected Tool:** `azmcp_foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.562920 | `azmcp_foundry_models_deploy` | ✅ **EXPECTED** |
| 2 | 0.335116 | `azmcp_foundry_openai_models-list` | ❌ |
| 3 | 0.298490 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.293352 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.282464 | `azmcp_mysql_server_list` | ❌ |

---

## Test 11

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663663 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.583429 | `azmcp_foundry_openai_models-list` | ❌ |
| 3 | 0.549636 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.539695 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.536115 | `azmcp_foundry_models_deploy` | ❌ |

---

## Test 12

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606592 | `azmcp_foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.521475 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.518221 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.507301 | `azmcp_foundry_openai_models-list` | ❌ |
| 5 | 0.486395 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560022 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.506478 | `azmcp_foundry_models_deployments_list` | ❌ |
| 3 | 0.491952 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.475204 | `azmcp_foundry_openai_models-list` | ❌ |
| 5 | 0.415089 | `azmcp_foundry_models_deploy` | ❌ |

---

## Test 14

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574818 | `azmcp_foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.497107 | `azmcp_foundry_models_deployments_list` | ❌ |
| 3 | 0.475139 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.467671 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.463399 | `azmcp_foundry_openai_models-list` | ❌ |

---

## Test 15

**Expected Tool:** `azmcp_foundry_openai_chat-completions-create`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.417723 | `azmcp_foundry_openai_chat-completions-create` | ✅ **EXPECTED** |
| 2 | 0.332543 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.211879 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.203502 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.188268 | `azmcp_communication_sms_send` | ❌ |

---

## Test 16

**Expected Tool:** `azmcp_foundry_openai_create-completion`  
**Prompt:** Create a completion with the prompt "What is Azure?"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553675 | `azmcp_foundry_openai_create-completion` | ✅ **EXPECTED** |
| 2 | 0.447828 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.403431 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.394144 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.386531 | `azmcp_get_bestpractices_get` | ❌ |

---

## Test 17

**Expected Tool:** `azmcp_foundry_openai_embeddings-create`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656198 | `azmcp_foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.443461 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.408887 | `azmcp_foundry_openai_models-list` | ❌ |
| 4 | 0.403947 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.399865 | `azmcp_foundry_openai_chat-completions-create` | ❌ |

---

## Test 18

**Expected Tool:** `azmcp_foundry_openai_embeddings-create`  
**Prompt:** Create vector embeddings for my text using Azure OpenAI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702787 | `azmcp_foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.460359 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.426022 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.409975 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.408297 | `azmcp_foundry_openai_models-list` | ❌ |

---

## Test 19

**Expected Tool:** `azmcp_foundry_openai_models-list`  
**Prompt:** List all available OpenAI models in my Azure resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.788120 | `azmcp_foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.655391 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.586908 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.565893 | `azmcp_search_service_list` | ❌ |
| 5 | 0.540853 | `azmcp_foundry_models_deployments_list` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_foundry_openai_models-list`  
**Prompt:** Show me the OpenAI model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.575513 | `azmcp_foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.512409 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.503924 | `azmcp_foundry_models_deployments_list` | ❌ |
| 4 | 0.412858 | `azmcp_foundry_openai_create-completion` | ❌ |
| 5 | 0.405167 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 21

**Expected Tool:** `azmcp_foundry_resource_get`  
**Prompt:** List all AI Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.558290 | `azmcp_search_service_list` | ❌ |
| 2 | 0.524657 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.524645 | `azmcp_grafana_list` | ❌ |
| 4 | 0.480102 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.466673 | `azmcp_foundry_models_deployments_list` | ❌ |

---

## Test 22

**Expected Tool:** `azmcp_foundry_resource_get`  
**Prompt:** Show me the AI Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492911 | `azmcp_foundry_models_deploy` | ❌ |
| 2 | 0.474905 | `azmcp_foundry_agents_list` | ❌ |
| 3 | 0.467211 | `azmcp_loadtesting_testresource_list` | ❌ |
| 4 | 0.453632 | `azmcp_foundry_openai_models-list` | ❌ |
| 5 | 0.452145 | `azmcp_foundry_models_deployments_list` | ❌ |

---

## Test 23

**Expected Tool:** `azmcp_foundry_resource_get`  
**Prompt:** Get details for AI Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.455154 | `azmcp_foundry_openai_models-list` | ❌ |
| 2 | 0.452340 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.444390 | `azmcp_loadtesting_testresource_list` | ❌ |
| 4 | 0.430230 | `azmcp_search_index_get` | ❌ |
| 5 | 0.426524 | `azmcp_functionapp_get` | ❌ |

---

## Test 24

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.693471 | `azmcp_search_service_list` | ❌ |
| 2 | 0.603324 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.586336 | `azmcp_search_index_get` | ❌ |
| 4 | 0.530496 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.526038 | `azmcp_search_index_query` | ❌ |

---

## Test 25

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624479 | `azmcp_search_service_list` | ❌ |
| 2 | 0.566436 | `azmcp_search_index_get` | ❌ |
| 3 | 0.552421 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.520755 | `azmcp_search_index_query` | ❌ |
| 5 | 0.482804 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 26

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.513638 | `azmcp_search_service_list` | ❌ |
| 2 | 0.494288 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.431646 | `azmcp_search_index_get` | ❌ |
| 4 | 0.405838 | `azmcp_kusto_database_list` | ❌ |
| 5 | 0.395851 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 27

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.457427 | `azmcp_search_service_list` | ❌ |
| 2 | 0.451799 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.426387 | `azmcp_search_index_get` | ❌ |
| 4 | 0.387533 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 5 | 0.346871 | `azmcp_mysql_database_list` | ❌ |

---

## Test 28

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586059 | `azmcp_search_index_get` | ❌ |
| 2 | 0.533700 | `azmcp_search_service_list` | ❌ |
| 3 | 0.516512 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 4 | 0.515816 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.505841 | `azmcp_search_index_query` | ❌ |

---

## Test 29

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** Show me the knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.376599 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 2 | 0.366893 | `azmcp_search_service_list` | ❌ |
| 3 | 0.365258 | `azmcp_search_index_get` | ❌ |
| 4 | 0.344938 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 5 | 0.342059 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |

---

## Test 30

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.575105 | `azmcp_search_index_query` | ❌ |
| 2 | 0.520409 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.482399 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.480105 | `azmcp_search_service_list` | ❌ |
| 5 | 0.476959 | `azmcp_search_index_get` | ❌ |

---

## Test 31

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.422671 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.399595 | `azmcp_search_index_query` | ❌ |
| 3 | 0.393241 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.388756 | `azmcp_search_index_get` | ❌ |
| 5 | 0.381919 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 32

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.436739 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.422173 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.382221 | `azmcp_search_index_query` | ❌ |
| 4 | 0.370018 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.348594 | `azmcp_postgres_database_query` | ❌ |

---

## Test 33

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.422671 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.399595 | `azmcp_search_index_query` | ❌ |
| 3 | 0.393241 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.388756 | `azmcp_search_index_get` | ❌ |
| 5 | 0.381919 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 34

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.467907 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.412481 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.400552 | `azmcp_search_index_query` | ❌ |
| 4 | 0.377664 | `azmcp_search_service_list` | ❌ |
| 5 | 0.376672 | `azmcp_search_index_get` | ❌ |

---

## Test 35

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580704 | `azmcp_search_index_query` | ❌ |
| 2 | 0.543473 | `azmcp_search_service_list` | ❌ |
| 3 | 0.521721 | `azmcp_search_index_get` | ❌ |
| 4 | 0.481130 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.454248 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 36

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.402582 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.371055 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.360043 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.353116 | `azmcp_search_index_query` | ❌ |
| 5 | 0.335838 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 37

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.447780 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.397187 | `azmcp_foundry_agents_connect` | ❌ |
| 3 | 0.381859 | `azmcp_search_index_query` | ❌ |
| 4 | 0.356349 | `azmcp_search_index_get` | ❌ |
| 5 | 0.346870 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 38

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691931 | `azmcp_search_service_list` | ❌ |
| 2 | 0.579006 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.572490 | `azmcp_search_index_get` | ❌ |
| 4 | 0.564089 | `azmcp_foundry_agents_list` | ❌ |
| 5 | 0.548438 | `azmcp_search_index_query` | ❌ |

---

## Test 39

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660170 | `azmcp_search_service_list` | ❌ |
| 2 | 0.578438 | `azmcp_search_index_get` | ❌ |
| 3 | 0.560564 | `azmcp_search_index_query` | ❌ |
| 4 | 0.545154 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.512126 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 40

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511469 | `azmcp_search_service_list` | ❌ |
| 2 | 0.448614 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 3 | 0.413424 | `azmcp_search_index_get` | ❌ |
| 4 | 0.373782 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.366812 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 41

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.487022 | `azmcp_search_service_list` | ❌ |
| 2 | 0.430064 | `azmcp_search_index_get` | ❌ |
| 3 | 0.421757 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.376999 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 5 | 0.364590 | `azmcp_search_index_query` | ❌ |

---

## Test 42

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** Get the details of knowledge source <source-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595601 | `azmcp_search_index_get` | ❌ |
| 2 | 0.531247 | `azmcp_search_service_list` | ❌ |
| 3 | 0.518109 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 4 | 0.497759 | `azmcp_search_index_query` | ❌ |
| 5 | 0.489381 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 43

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** Show me the knowledge source <source-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.370881 | `azmcp_search_index_get` | ❌ |
| 2 | 0.370838 | `azmcp_search_service_list` | ❌ |
| 3 | 0.344288 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 4 | 0.342546 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.287392 | `azmcp_search_index_query` | ❌ |

---

## Test 44

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680195 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544604 | `azmcp_foundry_knowledge_index_schema` | ❌ |
| 3 | 0.490624 | `azmcp_search_service_list` | ❌ |
| 4 | 0.466005 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 5 | 0.459609 | `azmcp_search_index_query` | ❌ |

---

## Test 45

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639511 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.620140 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561856 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.480817 | `azmcp_search_index_query` | ❌ |
| 5 | 0.453047 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 46

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620259 | `azmcp_search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562775 | `azmcp_search_service_list` | ❌ |
| 3 | 0.561154 | `azmcp_foundry_knowledge_index_list` | ❌ |
| 4 | 0.471416 | `azmcp_search_index_query` | ❌ |
| 5 | 0.464054 | `azmcp_foundry_knowledge_index_schema` | ❌ |

---

## Test 47

**Expected Tool:** `azmcp_search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.521863 | `azmcp_search_index_get` | ❌ |
| 2 | 0.515870 | `azmcp_search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497467 | `azmcp_search_service_list` | ❌ |
| 4 | 0.437715 | `azmcp_postgres_database_query` | ❌ |
| 5 | 0.373917 | `azmcp_foundry_knowledge_index_list` | ❌ |

---

## Test 48

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.553011 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.520340 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.509461 | `azmcp_subscription_list` | ❌ |
| 5 | 0.505252 | `azmcp_search_index_get` | ❌ |

---

## Test 49

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.479291 | `azmcp_search_index_get` | ❌ |
| 3 | 0.467337 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.461786 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.453489 | `azmcp_marketplace_product_list` | ❌ |

---

## Test 50

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553025 | `azmcp_search_service_list` | ✅ **EXPECTED** |
| 2 | 0.435677 | `azmcp_search_index_get` | ❌ |
| 3 | 0.417096 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.404758 | `azmcp_search_index_query` | ❌ |
| 5 | 0.336174 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 51

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666038 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.415159 | `azmcp_foundry_openai_embeddings-create` | ❌ |
| 3 | 0.365228 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.351127 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.342808 | `azmcp_foundry_openai_create-completion` | ❌ |

---

## Test 52

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511324 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.202056 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.190267 | `azmcp_foundry_openai_embeddings-create` | ❌ |
| 4 | 0.184542 | `azmcp_foundry_openai_create-completion` | ❌ |
| 5 | 0.159108 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 53

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486489 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.180941 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.178944 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.160209 | `azmcp_foundry_agents_connect` | ❌ |
| 5 | 0.156850 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 54

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611992 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.322345 | `azmcp_foundry_openai_embeddings-create` | ❌ |
| 3 | 0.263196 | `azmcp_foundry_openai_create-completion` | ❌ |
| 4 | 0.251200 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.237761 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 55

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.410533 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.159787 | `azmcp_foundry_openai_embeddings-create` | ❌ |
| 3 | 0.158032 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.152137 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.151632 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 56

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546259 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.225386 | `azmcp_foundry_openai_embeddings-create` | ❌ |
| 3 | 0.200865 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.196743 | `azmcp_foundry_openai_create-completion` | ❌ |
| 5 | 0.183420 | `azmcp_extension_azqr` | ❌ |

---

## Test 57

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539963 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.246979 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.238192 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.203413 | `azmcp_foundry_agents_connect` | ❌ |
| 5 | 0.186570 | `azmcp_foundry_openai_embeddings-create` | ❌ |

---

## Test 58

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549151 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.357816 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.345661 | `azmcp_foundry_openai_create-completion` | ❌ |
| 4 | 0.337387 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.334153 | `azmcp_foundry_openai_embeddings-create` | ❌ |

---

## Test 59

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532536 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.384991 | `azmcp_foundry_openai_embeddings-create` | ❌ |
| 3 | 0.381487 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.378382 | `azmcp_foundry_openai_create-completion` | ❌ |
| 5 | 0.342143 | `azmcp_communication_sms_send` | ❌ |

---

## Test 60

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453396 | `azmcp_speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.181994 | `azmcp_foundry_openai_create-completion` | ❌ |
| 3 | 0.174375 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.173205 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.160483 | `azmcp_foundry_agents_connect` | ❌ |

---

## Test 61

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786274 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.530579 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.491330 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.481106 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.479886 | `azmcp_subscription_list` | ❌ |

---

## Test 62

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634995 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.464865 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.398495 | `azmcp_subscription_list` | ❌ |
| 4 | 0.391717 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.372456 | `azmcp_postgres_server_list` | ❌ |

---

## Test 63

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565371 | `azmcp_appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.465344 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.355916 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.348974 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.327234 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 64

**Expected Tool:** `azmcp_appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618353 | `azmcp_appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.464358 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.424344 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.392004 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 65

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.632687 | `azmcp_appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.557826 | `azmcp_appconfig_account_list` | ❌ |
| 3 | 0.530884 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.464911 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.439089 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 66

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612555 | `azmcp_appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.522453 | `azmcp_appconfig_account_list` | ❌ |
| 3 | 0.512945 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.468812 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.457866 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 67

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512883 | `azmcp_appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.449965 | `azmcp_appconfig_account_list` | ❌ |
| 3 | 0.398684 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.380812 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.346166 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 68

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552300 | `azmcp_appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.448912 | `azmcp_appconfig_kv_set` | ❌ |
| 3 | 0.441818 | `azmcp_appconfig_kv_delete` | ❌ |
| 4 | 0.437442 | `azmcp_appconfig_account_list` | ❌ |
| 5 | 0.416264 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 69

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591237 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.487174 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.445551 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.431706 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.373540 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 70

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555519 | `azmcp_appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.506033 | `azmcp_appconfig_kv_get` | ❌ |
| 3 | 0.476609 | `azmcp_appconfig_kv_delete` | ❌ |
| 4 | 0.425804 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.409857 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 71

**Expected Tool:** `azmcp_appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609635 | `azmcp_appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536497 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 3 | 0.512707 | `azmcp_appconfig_kv_get` | ❌ |
| 4 | 0.505571 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.377851 | `azmcp_appconfig_account_list` | ❌ |

---

## Test 72

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595632 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.336090 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.273082 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.254473 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 73

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502322 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.316255 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.255507 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.249563 | `azmcp_monitor_resource_log_query` | ❌ |
| 5 | 0.225908 | `azmcp_quota_usage_check` | ❌ |

---

## Test 74

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492820 | `azmcp_applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.256325 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.242574 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.225426 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.211564 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 75

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

---

## Test 76

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure a SQL Server database for app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612164 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.473224 | `azmcp_sql_db_update` | ❌ |
| 3 | 0.471103 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.454417 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.412229 | `azmcp_sql_server_delete` | ❌ |

---

## Test 77

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

---

## Test 78

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a PostgreSQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579502 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449038 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.416337 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.409515 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.405431 | `azmcp_postgres_server_list` | ❌ |

---

## Test 79

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a CosmosDB database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643046 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.477031 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.465637 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.431581 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.428178 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 80

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645533 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.489228 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.440007 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.431531 | `azmcp_sql_db_delete` | ❌ |
| 5 | 0.426090 | `azmcp_sql_server_delete` | ❌ |

---

## Test 81

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665216 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.401714 | `azmcp_sql_db_rename` | ❌ |
| 3 | 0.369071 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.332119 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.328637 | `azmcp_sql_db_update` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536761 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.408796 | `azmcp_sql_db_rename` | ❌ |
| 3 | 0.394572 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.355309 | `azmcp_sql_db_update` | ❌ |
| 5 | 0.329110 | `azmcp_keyvault_secret_create` | ❌ |

---

## Test 83

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560222 | `azmcp_appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.426727 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.392351 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.371947 | `azmcp_sql_db_delete` | ❌ |
| 5 | 0.360996 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 84

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572473 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.445157 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.390478 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 4 | 0.383948 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.375033 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 85

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696531 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.468384 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.452231 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.435241 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.424622 | `azmcp_search_service_list` | ❌ |

---

## Test 86

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.488002 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.479392 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.477396 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.468847 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 87

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `azmcp_applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.419670 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.383767 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.367278 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.343931 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 88

**Expected Tool:** `azmcp_extension_cli_generate`  
**Prompt:** Get Azure CLI command to create a Storage account with name <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627896 | `azmcp_storage_account_create` | ❌ |
| 2 | 0.543366 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.483493 | `azmcp_sql_db_create` | ❌ |
| 4 | 0.479160 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.450072 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 89

**Expected Tool:** `azmcp_extension_cli_generate`  
**Prompt:** Show me how to use Azure CLI to list all virtual machines in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530014 | `azmcp_search_service_list` | ❌ |
| 2 | 0.517673 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.505341 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.493200 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.487633 | `azmcp_subscription_list` | ❌ |

---

## Test 90

**Expected Tool:** `azmcp_extension_cli_generate`  
**Prompt:** Show me the details of the storage account <account_name> with Azure CLI commands  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690287 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.632814 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.540922 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.519788 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.493145 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 91

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711662 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.585675 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.541506 | `azmcp_search_service_list` | ❌ |
| 5 | 0.520548 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 92

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563613 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.450287 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.421808 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.415552 | `azmcp_cosmos_database_container_list` | ❌ |

---

## Test 93

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637130 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563592 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.516769 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.496363 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.483966 | `azmcp_redis_cache_list` | ❌ |

---

## Test 94

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654248 | `azmcp_acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 3 | 0.476015 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.454929 | `azmcp_group_list` | ❌ |
| 5 | 0.453926 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 95

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `azmcp_acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637904 | `azmcp_acr_registry_repository_list` | ❌ |
| 3 | 0.468028 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.449545 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `azmcp_group_list` | ❌ |

---

## Test 96

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626574 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.544172 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.495567 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.487490 | `azmcp_redis_cache_list` | ❌ |

---

## Test 97

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546295 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.407973 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.400145 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.356768 | `azmcp_redis_cache_list` | ❌ |

---

## Test 98

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674246 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.433927 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.388490 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.383183 | `azmcp_kusto_database_list` | ❌ |

---

## Test 99

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600750 | `azmcp_acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501842 | `azmcp_acr_registry_list` | ❌ |
| 3 | 0.418623 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.377069 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.376576 | `azmcp_redis_cache_list` | ❌ |

---

## Test 100

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.200486 | `azmcp_communication_sms_send` | ❌ |
| 2 | 0.188975 | `azmcp_eventgrid_events_publish` | ❌ |
| 3 | 0.145951 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.136621 | `azmcp_speech_stt_recognize` | ❌ |
| 5 | 0.133336 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |

---

## Test 101

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send an email from my communication service to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.393690 | `azmcp_communication_sms_send` | ❌ |
| 2 | 0.218524 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.196532 | `azmcp_appservice_database_add` | ❌ |
| 4 | 0.194094 | `azmcp_speech_stt_recognize` | ❌ |
| 5 | 0.182189 | `azmcp_foundry_openai_create-completion` | ❌ |

---

## Test 102

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.152418 | `azmcp_eventgrid_events_publish` | ❌ |
| 2 | 0.152013 | `azmcp_servicebus_topic_details` | ❌ |
| 3 | 0.149655 | `azmcp_communication_sms_send` | ❌ |
| 4 | 0.143660 | `azmcp_foundry_agents_evaluate` | ❌ |
| 5 | 0.132729 | `azmcp_foundry_openai_chat-completions-create` | ❌ |

---

## Test 103

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.221831 | `azmcp_communication_sms_send` | ❌ |
| 2 | 0.106026 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.087784 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.084905 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.083565 | `azmcp_appconfig_kv_delete` | ❌ |

---

## Test 104

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.211476 | `azmcp_communication_sms_send` | ❌ |
| 2 | 0.114324 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.098798 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.087063 | `azmcp_postgres_server_param_set` | ❌ |
| 5 | 0.078159 | `azmcp_kusto_query` | ❌ |

---

## Test 105

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.164115 | `azmcp_mysql_server_param_set` | ❌ |
| 2 | 0.160799 | `azmcp_communication_sms_send` | ❌ |
| 3 | 0.158759 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.143574 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.128990 | `azmcp_keyvault_secret_create` | ❌ |

---

## Test 106

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send email with custom sender name <sender-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.189792 | `azmcp_communication_sms_send` | ❌ |
| 2 | 0.156875 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 3 | 0.143550 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.139407 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.136040 | `azmcp_foundry_openai_create-completion` | ❌ |

---

## Test 107

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.258024 | `azmcp_communication_sms_send` | ❌ |
| 2 | 0.137538 | `azmcp_confidentialledger_entries_append` | ❌ |
| 3 | 0.108807 | `azmcp_confidentialledger_entries_get` | ❌ |
| 4 | 0.105033 | `azmcp_storage_blob_upload` | ❌ |
| 5 | 0.101102 | `azmcp_workbooks_create` | ❌ |

---

## Test 108

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.321362 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.178085 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.148584 | `azmcp_foundry_agents_connect` | ❌ |
| 4 | 0.141194 | `azmcp_foundry_openai_create-completion` | ❌ |
| 5 | 0.134178 | `azmcp_loadtesting_testrun_create` | ❌ |

---

## Test 109

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.351013 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.204572 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.155940 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.136790 | `azmcp_loadtesting_testrun_update` | ❌ |
| 5 | 0.135728 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 110

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.364337 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.142602 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.142030 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.097835 | `azmcp_postgres_server_param_set` | ❌ |
| 5 | 0.097461 | `azmcp_foundry_openai_create-completion` | ❌ |

---

## Test 111

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.404260 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.191848 | `azmcp_extension_azqr` | ❌ |
| 3 | 0.170726 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.166385 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.151614 | `azmcp_postgres_server_param_set` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.367439 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.188153 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.159177 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.158295 | `azmcp_loadtesting_test_create` | ❌ |
| 5 | 0.158281 | `azmcp_foundry_openai_chat-completions-create` | ❌ |

---

## Test 113

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.302501 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.164289 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.129965 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.128704 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.125475 | `azmcp_sql_server_delete` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.440764 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.213669 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.177315 | `azmcp_appservice_database_add` | ❌ |
| 4 | 0.173906 | `azmcp_foundry_openai_create-completion` | ❌ |
| 5 | 0.152664 | `azmcp_sql_server_delete` | ❌ |

---

## Test 115

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.430235 | `azmcp_communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.206916 | `azmcp_foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.187824 | `azmcp_confidentialledger_entries_append` | ❌ |
| 4 | 0.181824 | `azmcp_foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.163201 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 116

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510651 | `azmcp_confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.294964 | `azmcp_confidentialledger_entries_get` | ❌ |
| 3 | 0.292014 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.258967 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 5 | 0.249908 | `azmcp_keyvault_certificate_import` | ❌ |

---

## Test 117

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602247 | `azmcp_confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.357695 | `azmcp_confidentialledger_entries_get` | ❌ |
| 3 | 0.211990 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 4 | 0.195471 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.184077 | `azmcp_keyvault_certificate_import` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546660 | `azmcp_confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.452068 | `azmcp_confidentialledger_entries_get` | ❌ |
| 3 | 0.225141 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 4 | 0.215932 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.211738 | `azmcp_appservice_database_add` | ❌ |

---

## Test 119

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496023 | `azmcp_confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.340277 | `azmcp_confidentialledger_entries_get` | ❌ |
| 3 | 0.218473 | `azmcp_monitor_activitylog_list` | ❌ |
| 4 | 0.204925 | `azmcp_monitor_resource_log_query` | ❌ |
| 5 | 0.198615 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622138 | `azmcp_confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.524838 | `azmcp_confidentialledger_entries_get` | ❌ |
| 3 | 0.252508 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 4 | 0.240252 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.186890 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_confidentialledger_entries_get`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.707292 | `azmcp_confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.551953 | `azmcp_confidentialledger_entries_append` | ❌ |
| 3 | 0.245541 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.229943 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.211839 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 122

**Expected Tool:** `azmcp_confidentialledger_entries_get`  
**Prompt:** Get transaction <transaction_id> from ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509745 | `azmcp_confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.416580 | `azmcp_confidentialledger_entries_append` | ❌ |
| 3 | 0.223959 | `azmcp_loadtesting_testrun_get` | ❌ |
| 4 | 0.218412 | `azmcp_monitor_resource_log_query` | ❌ |
| 5 | 0.217671 | `azmcp_loadtesting_testrun_list` | ❌ |

---

## Test 123

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668480 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.636036 | `azmcp_subscription_list` | ❌ |
| 4 | 0.615268 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.601467 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 124

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665447 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605357 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.571613 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.549447 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.494741 | `azmcp_subscription_list` | ❌ |

---

## Test 125

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `azmcp_cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.607201 | `azmcp_subscription_list` | ❌ |
| 3 | 0.605125 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.566249 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.563921 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 126

**Expected Tool:** `azmcp_cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658701 | `azmcp_cosmos_database_container_item_query` | ✅ **EXPECTED** |
| 2 | 0.605253 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.477874 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.447757 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.445640 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 127

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852832 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681044 | `azmcp_cosmos_database_list` | ❌ |
| 3 | 0.680762 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 4 | 0.630659 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.581593 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 128

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789395 | `azmcp_cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.648126 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 3 | 0.614220 | `azmcp_cosmos_database_list` | ❌ |
| 4 | 0.562062 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.537286 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 129

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668515 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.665298 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.606433 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.583535 | `azmcp_kusto_database_list` | ❌ |

---

## Test 130

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `azmcp_cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.614572 | `azmcp_cosmos_account_list` | ❌ |
| 4 | 0.579919 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.538479 | `azmcp_mysql_database_list` | ❌ |

---

## Test 131

**Expected Tool:** `azmcp_kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590264 | `azmcp_kusto_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.485309 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.463832 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.428159 | `azmcp_kusto_query` | ❌ |
| 5 | 0.425669 | `azmcp_kusto_database_list` | ❌ |

---

## Test 132

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793744 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.653371 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.630507 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.573395 | `azmcp_kusto_cluster_get` | ❌ |
| 5 | 0.525025 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 133

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531307 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.510029 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.465277 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.432288 | `azmcp_kusto_database_list` | ❌ |
| 5 | 0.391087 | `azmcp_redis_cluster_database_list` | ❌ |

---

## Test 134

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701484 | `azmcp_kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.616448 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.571191 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.548685 | `azmcp_kusto_database_list` | ❌ |
| 5 | 0.498909 | `azmcp_aks_cluster_get` | ❌ |

---

## Test 135

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677059 | `azmcp_kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.628129 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.560592 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.556691 | `azmcp_kusto_table_list` | ❌ |
| 5 | 0.553237 | `azmcp_postgres_database_list` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623523 | `azmcp_kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.597975 | `azmcp_redis_cluster_database_list` | ❌ |
| 3 | 0.509953 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.506983 | `azmcp_kusto_table_list` | ❌ |
| 5 | 0.497144 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423712 | `azmcp_kusto_query` | ✅ **EXPECTED** |
| 2 | 0.409548 | `azmcp_postgres_database_query` | ❌ |
| 3 | 0.408723 | `azmcp_kusto_table_schema` | ❌ |
| 4 | 0.408309 | `azmcp_kusto_sample` | ❌ |
| 5 | 0.404430 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595554 | `azmcp_kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.510233 | `azmcp_kusto_table_schema` | ❌ |
| 3 | 0.424239 | `azmcp_kusto_table_list` | ❌ |
| 4 | 0.400924 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.399525 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.679624 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585237 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.581207 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.556724 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.549917 | `azmcp_monitor_table_list` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619069 | `azmcp_kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.554144 | `azmcp_kusto_table_schema` | ❌ |
| 3 | 0.527568 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.524629 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.523569 | `azmcp_postgres_table_list` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666947 | `azmcp_kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564229 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.527869 | `azmcp_mysql_table_schema_get` | ❌ |
| 4 | 0.490698 | `azmcp_kusto_sample` | ❌ |
| 5 | 0.489478 | `azmcp_kusto_table_list` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634017 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.623421 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534457 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.498918 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.490148 | `azmcp_sql_db_list` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587986 | `azmcp_mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.573942 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.483712 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.463167 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.448045 | `azmcp_redis_cluster_database_list` | ❌ |

---

## Test 144

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

---

## Test 145

**Expected Tool:** `azmcp_mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531887 | `azmcp_postgres_server_config_get` | ❌ |
| 2 | 0.516894 | `azmcp_mysql_server_param_set` | ❌ |
| 3 | 0.489816 | `azmcp_mysql_server_config_get` | ✅ **EXPECTED** |
| 4 | 0.476863 | `azmcp_mysql_server_param_get` | ❌ |
| 5 | 0.426507 | `azmcp_mysql_table_schema_get` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678472 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.558177 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.554817 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.513706 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.501199 | `azmcp_mysql_table_list` | ❌ |

---

## Test 147

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478518 | `azmcp_mysql_database_list` | ❌ |
| 2 | 0.474586 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.412380 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.389986 | `azmcp_postgres_database_list` | ❌ |

---

## Test 148

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `azmcp_postgres_server_list` | ❌ |
| 2 | 0.534266 | `azmcp_mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530210 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.487827 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.475052 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 149

**Expected Tool:** `azmcp_mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `azmcp_mysql_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.438075 | `azmcp_mysql_server_param_set` | ❌ |
| 3 | 0.333841 | `azmcp_mysql_database_query` | ❌ |
| 4 | 0.313150 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.310782 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449476 | `azmcp_mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381179 | `azmcp_mysql_server_param_get` | ❌ |
| 3 | 0.303526 | `azmcp_postgres_server_param_set` | ❌ |
| 4 | 0.298907 | `azmcp_mysql_database_query` | ❌ |
| 5 | 0.277641 | `azmcp_appservice_database_add` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633448 | `azmcp_mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573844 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.550895 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.546963 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.511869 | `azmcp_kusto_table_list` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608842 | `azmcp_mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.526012 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.525726 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.506687 | `azmcp_mysql_table_schema_get` | ❌ |
| 5 | 0.497882 | `azmcp_postgres_database_list` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630623 | `azmcp_mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558259 | `azmcp_postgres_table_schema_get` | ❌ |
| 3 | 0.545025 | `azmcp_mysql_table_list` | ❌ |
| 4 | 0.517419 | `azmcp_kusto_table_schema` | ❌ |
| 5 | 0.457739 | `azmcp_mysql_database_list` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815560 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.644014 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.622790 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.542685 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.490955 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.759952 | `azmcp_postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589783 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.585891 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.552660 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.495683 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 156

**Expected Tool:** `azmcp_postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546197 | `azmcp_postgres_database_list` | ❌ |
| 2 | 0.523142 | `azmcp_postgres_database_query` | ✅ **EXPECTED** |
| 3 | 0.503267 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.466599 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.403954 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 157

**Expected Tool:** `azmcp_postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `azmcp_postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.615429 | `azmcp_postgres_server_param_set` | ❌ |
| 3 | 0.599518 | `azmcp_postgres_server_param_get` | ❌ |
| 4 | 0.534942 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.518574 | `azmcp_postgres_server_list` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900063 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640666 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.565843 | `azmcp_postgres_table_list` | ❌ |
| 4 | 0.538939 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.534272 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 159

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.607041 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.576349 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.522996 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.506262 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 160

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `azmcp_postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579199 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.531804 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.514445 | `azmcp_postgres_table_list` | ❌ |
| 5 | 0.505970 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 161

**Expected Tool:** `azmcp_postgres_server_param_get`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594733 | `azmcp_postgres_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.552678 | `azmcp_postgres_server_param_set` | ❌ |
| 3 | 0.539671 | `azmcp_postgres_server_config_get` | ❌ |
| 4 | 0.489693 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.451793 | `azmcp_postgres_database_list` | ❌ |

---

## Test 162

**Expected Tool:** `azmcp_postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579873 | `azmcp_postgres_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.488474 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.469794 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.447026 | `azmcp_postgres_server_param_get` | ❌ |
| 5 | 0.440642 | `azmcp_postgres_database_list` | ❌ |

---

## Test 163

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789883 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.750573 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.574930 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.519854 | `azmcp_postgres_table_schema_get` | ❌ |
| 5 | 0.501400 | `azmcp_postgres_server_config_get` | ❌ |

---

## Test 164

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736083 | `azmcp_postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.690086 | `azmcp_postgres_database_list` | ❌ |
| 3 | 0.558396 | `azmcp_postgres_table_schema_get` | ❌ |
| 4 | 0.543331 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.521570 | `azmcp_postgres_server_config_get` | ❌ |

---

## Test 165

**Expected Tool:** `azmcp_postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.715034 | `azmcp_postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597980 | `azmcp_postgres_table_list` | ❌ |
| 3 | 0.574257 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.508147 | `azmcp_postgres_server_config_get` | ❌ |
| 5 | 0.502603 | `azmcp_kusto_table_schema` | ❌ |

---

## Test 166

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711770 | `azmcp_deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.451638 | `azmcp_monitor_activitylog_list` | ❌ |
| 4 | 0.404890 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.401388 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 167

**Expected Tool:** `azmcp_deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680640 | `azmcp_deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562521 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.497193 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.489344 | `azmcp_cloudarchitect_design` | ❌ |
| 5 | 0.435729 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 168

**Expected Tool:** `azmcp_deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528889 | `azmcp_deploy_iac_rules_get` | ✅ **EXPECTED** |
| 2 | 0.479903 | `azmcp_bicepschema_get` | ❌ |
| 3 | 0.391965 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.383210 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.341436 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 169

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `azmcp_deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `azmcp_deploy_plan_get` | ❌ |
| 3 | 0.448635 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.385920 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.382240 | `azmcp_get_bestpractices_get` | ❌ |

---

## Test 170

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688055 | `azmcp_deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587903 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499212 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.448692 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 171

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755365 | `azmcp_eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.483021 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.466031 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.355599 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.327844 | `azmcp_eventhubs_namespace_get` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654647 | `azmcp_eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.524503 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.510039 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.373718 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.332060 | `azmcp_eventhubs_namespace_get` | ❌ |

---

## Test 173

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600274 | `azmcp_eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.521240 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.504808 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.385359 | `azmcp_eventhubs_namespace_get` | ❌ |
| 5 | 0.353745 | `azmcp_servicebus_topic_details` | ❌ |

---

## Test 174

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770140 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.745470 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.561862 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.545540 | `azmcp_search_service_list` | ❌ |
| 5 | 0.526138 | `azmcp_subscription_list` | ❌ |

---

## Test 175

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738258 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.737486 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.492592 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.480287 | `azmcp_subscription_list` | ❌ |
| 5 | 0.475119 | `azmcp_search_service_list` | ❌ |

---

## Test 176

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770140 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.721362 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.535326 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `azmcp_search_service_list` | ❌ |
| 5 | 0.495987 | `azmcp_subscription_list` | ❌ |

---

## Test 177

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.758816 | `azmcp_eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.704462 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.609175 | `azmcp_group_list` | ❌ |
| 4 | 0.528270 | `azmcp_eventhubs_namespace_get` | ❌ |
| 5 | 0.514613 | `azmcp_workbooks_list` | ❌ |

---

## Test 178

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769097 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.720606 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.498615 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.486216 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 5 | 0.486162 | `azmcp_eventgrid_events_publish` | ❌ |

---

## Test 179

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718109 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.709805 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.539977 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.529286 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.477876 | `azmcp_eventgrid_events_publish` | ❌ |

---

## Test 180

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746815 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.746174 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.524919 | `azmcp_group_list` | ❌ |
| 4 | 0.503158 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.490915 | `azmcp_eventhubs_namespace_get` | ❌ |

---

## Test 181

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736436 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.659727 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.569254 | `azmcp_subscription_list` | ❌ |
| 4 | 0.537922 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.518857 | `azmcp_search_service_list` | ❌ |

---

## Test 182

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684543 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.656277 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.542388 | `azmcp_subscription_list` | ❌ |
| 4 | 0.521053 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.510115 | `azmcp_group_list` | ❌ |

---

## Test 183

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696101 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.691739 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.557573 | `azmcp_group_list` | ❌ |
| 4 | 0.504984 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.502053 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 184

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709801 | `azmcp_eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.642095 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.506697 | `azmcp_subscription_list` | ❌ |
| 4 | 0.476763 | `azmcp_search_service_list` | ❌ |
| 5 | 0.475782 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 185

**Expected Tool:** `azmcp_eventhubs_namespace_get`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653507 | `azmcp_eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.607372 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.557200 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.556126 | `azmcp_eventgrid_subscription_list` | ❌ |
| 5 | 0.535097 | `azmcp_search_service_list` | ❌ |

---

## Test 186

**Expected Tool:** `azmcp_eventhubs_namespace_get`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.513154 | `azmcp_eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.497399 | `azmcp_servicebus_queue_details` | ❌ |
| 3 | 0.470455 | `azmcp_functionapp_get` | ❌ |
| 4 | 0.466515 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 5 | 0.459491 | `azmcp_sql_db_show` | ❌ |

---

## Test 187

**Expected Tool:** `azmcp_eventhubs_namespace_update`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.466286 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.449724 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.413828 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.371145 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.362477 | `azmcp_group_list` | ❌ |

---

## Test 188

**Expected Tool:** `azmcp_eventhubs_namespace_update`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.448526 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.372632 | `azmcp_sql_db_rename` | ❌ |
| 3 | 0.365516 | `azmcp_loadtesting_testrun_update` | ❌ |
| 4 | 0.325139 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.323076 | `azmcp_managedlustre_filesystem_update` | ❌ |

---

## Test 189

**Expected Tool:** `azmcp_eventhubs_namespace_delete`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.454698 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.438150 | `azmcp_workbooks_delete` | ❌ |
| 3 | 0.401799 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.364066 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.353733 | `azmcp_group_list` | ❌ |

---

## Test 190

**Expected Tool:** `azmcp_eventhubs_eventhub_get`  
**Prompt:** List all Event Hubs in my namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681328 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.521847 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.484119 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.463816 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.448631 | `azmcp_kusto_database_list` | ❌ |

---

## Test 191

**Expected Tool:** `azmcp_eventhubs_eventhub_get`  
**Prompt:** Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637782 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.486265 | `azmcp_servicebus_queue_details` | ❌ |
| 3 | 0.452887 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.452568 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.450260 | `azmcp_servicebus_topic_details` | ❌ |

---

## Test 192

**Expected Tool:** `azmcp_eventhubs_eventhub_update`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600067 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.395608 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.393886 | `azmcp_workbooks_create` | ❌ |
| 4 | 0.385385 | `azmcp_sql_db_create` | ❌ |
| 5 | 0.369005 | `azmcp_eventgrid_events_publish` | ❌ |

---

## Test 193

**Expected Tool:** `azmcp_eventhubs_eventhub_update`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.562413 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.385393 | `azmcp_eventgrid_events_publish` | ❌ |
| 3 | 0.362566 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.358909 | `azmcp_loadtesting_testrun_update` | ❌ |
| 5 | 0.353183 | `azmcp_sql_db_rename` | ❌ |

---

## Test 194

**Expected Tool:** `azmcp_eventhubs_eventhub_delete`  
**Prompt:** Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580014 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.440495 | `azmcp_workbooks_delete` | ❌ |
| 3 | 0.371913 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.341404 | `azmcp_appconfig_kv_delete` | ❌ |
| 5 | 0.340035 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 195

**Expected Tool:** `azmcp_eventhubs_eventhub_consumergroup_get`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612748 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.493729 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.483909 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.437726 | `azmcp_group_list` | ❌ |
| 5 | 0.427606 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 196

**Expected Tool:** `azmcp_eventhubs_eventhub_consumergroup_get`  
**Prompt:** Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577208 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.458223 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.436912 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.436003 | `azmcp_functionapp_get` | ❌ |
| 5 | 0.431617 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 197

**Expected Tool:** `azmcp_eventhubs_eventhub_consumergroup_update`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.541192 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.388446 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.379943 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.376321 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.354561 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 198

**Expected Tool:** `azmcp_eventhubs_eventhub_consumergroup_update`  
**Prompt:** Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517950 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.382031 | `azmcp_eventgrid_topic_list` | ❌ |
| 3 | 0.351189 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.336236 | `azmcp_eventgrid_events_publish` | ❌ |
| 5 | 0.332183 | `azmcp_loadtesting_testrun_update` | ❌ |

---

## Test 199

**Expected Tool:** `azmcp_eventhubs_eventhub_consumergroup_delete`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.519798 | `azmcp_eventhubs_namespace_get` | ❌ |
| 2 | 0.377818 | `azmcp_workbooks_delete` | ❌ |
| 3 | 0.356629 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.343741 | `azmcp_eventgrid_subscription_list` | ❌ |
| 5 | 0.338610 | `azmcp_appconfig_kv_delete` | ❌ |

---

## Test 200

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.451613 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.450457 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.390048 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.380314 | `azmcp_get_bestpractices_get` | ❌ |

---

## Test 201

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607276 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447400 | `azmcp_mysql_server_config_get` | ❌ |
| 3 | 0.424457 | `azmcp_appconfig_account_list` | ❌ |
| 4 | 0.411267 | `azmcp_appconfig_kv_get` | ❌ |
| 5 | 0.400402 | `azmcp_deploy_app_logs_get` | ❌ |

---

## Test 202

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.411650 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.390708 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383533 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.347396 | `azmcp_applens_resource_diagnose` | ❌ |

---

## Test 203

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690933 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.432317 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.431821 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.424646 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.417281 | `azmcp_signalr_runtime_get` | ❌ |

---

## Test 204

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.417641 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.409712 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.392237 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.391480 | `azmcp_sql_server_show` | ❌ |

---

## Test 205

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.449588 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.428689 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.372466 | `azmcp_signalr_runtime_get` | ❌ |
| 5 | 0.368188 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 206

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.430189 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.403311 | `azmcp_signalr_runtime_get` | ❌ |
| 4 | 0.388678 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.370793 | `azmcp_storage_blob_container_get` | ❌ |

---

## Test 207

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554959 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.426709 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.424611 | `azmcp_deploy_app_logs_get` | ❌ |
| 4 | 0.408060 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.381725 | `azmcp_deploy_architecture_diagram_generate` | ❌ |

---

## Test 208

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565797 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.403665 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.384159 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369868 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.353063 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 209

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.559382 | `azmcp_search_service_list` | ❌ |
| 3 | 0.534930 | `azmcp_subscription_list` | ❌ |
| 4 | 0.529031 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.516618 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 210

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.464985 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.436167 | `azmcp_foundry_agents_list` | ❌ |
| 4 | 0.412646 | `azmcp_search_service_list` | ❌ |
| 5 | 0.411323 | `azmcp_get_bestpractices_get` | ❌ |

---

## Test 211

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433674 | `azmcp_functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.346619 | `azmcp_deploy_app_logs_get` | ❌ |
| 3 | 0.337966 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.284362 | `azmcp_get_bestpractices_get` | ❌ |
| 5 | 0.250920 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 212

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604780 | `azmcp_keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.520401 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.496629 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.452366 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.448039 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 213

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671370 | `azmcp_keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.453590 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.441225 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.404666 | `azmcp_appconfig_kv_set` | ❌ |
| 5 | 0.395274 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 214

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505591 | `azmcp_keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.496439 | `azmcp_appconfig_kv_set` | ❌ |
| 3 | 0.419847 | `azmcp_appconfig_kv_lock_set` | ❌ |
| 4 | 0.418824 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.410048 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 215

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627727 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570318 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.540199 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.519218 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.500027 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 216

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599990 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561445 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.522706 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.502128 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.497145 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 217

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573998 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527759 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.481548 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.469601 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 218

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591697 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.562265 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.522147 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.502529 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.479992 | `azmcp_keyvault_certificate_list` | ❌ |

---

## Test 219

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622788 | `azmcp_keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.558532 | `azmcp_keyvault_certificate_import` | ❌ |
| 3 | 0.534503 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.521316 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.465056 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 220

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600625 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528405 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.519037 | `azmcp_keyvault_certificate_import` | ❌ |
| 4 | 0.499293 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.486609 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 221

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646118 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.563007 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.514149 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.509447 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.507725 | `azmcp_keyvault_certificate_import` | ❌ |

---

## Test 222

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609523 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.515570 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.511197 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.507768 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.474394 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 223

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.647582 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527330 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.521393 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.510213 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.501971 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 224

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595959 | `azmcp_keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527404 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.519059 | `azmcp_keyvault_certificate_import` | ❌ |
| 4 | 0.501138 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.465174 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 225

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586149 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.421510 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.403546 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.399291 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.353661 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 226

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622023 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.504695 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.498940 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.448153 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.419505 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 227

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595707 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.453929 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.452551 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.418203 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.413377 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 228

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619480 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.517804 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.480815 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.444386 | `azmcp_keyvault_certificate_list` | ❌ |
| 5 | 0.381873 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 229

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595417 | `azmcp_keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452489 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.441616 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.408018 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.392244 | `azmcp_keyvault_secret_create` | ❌ |

---

## Test 230

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726124 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.583110 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.531871 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.515236 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.485792 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 231

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615541 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.522453 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.475156 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.460973 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.448139 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 232

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624711 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.519739 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.510048 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.505534 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.497356 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 233

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672622 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.553990 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.511853 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.507062 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.492357 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 234

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747407 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.594216 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.558627 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.515568 | `azmcp_keyvault_certificate_get` | ❌ |
| 5 | 0.490876 | `azmcp_keyvault_certificate_create` | ❌ |

---

## Test 235

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639647 | `azmcp_keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.512440 | `azmcp_keyvault_certificate_get` | ❌ |
| 3 | 0.507566 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.482559 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.464673 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 236

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661442 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456566 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.451777 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.429574 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.399304 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 237

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641070 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428502 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.422763 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.420045 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.405644 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 238

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547493 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.463557 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.447410 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.420366 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.404350 | `azmcp_keyvault_certificate_import` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641369 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.501636 | `azmcp_keyvault_secret_create` | ❌ |
| 3 | 0.491735 | `azmcp_keyvault_certificate_create` | ❌ |
| 4 | 0.464557 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.451016 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 240

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571718 | `azmcp_keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443369 | `azmcp_keyvault_certificate_create` | ❌ |
| 3 | 0.434675 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.421721 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.400533 | `azmcp_keyvault_certificate_import` | ❌ |

---

## Test 241

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549488 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.468165 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.452816 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.439969 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.426545 | `azmcp_keyvault_certificate_get` | ❌ |

---

## Test 242

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629552 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.532651 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.495957 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.475152 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.456992 | `azmcp_keyvault_key_create` | ❌ |

---

## Test 243

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484645 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.443182 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.409388 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.395491 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 5 | 0.383519 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 244

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590157 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.487656 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.460500 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.440675 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.436424 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 245

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518886 | `azmcp_keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.432742 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 3 | 0.429131 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.422536 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.395959 | `azmcp_keyvault_key_list` | ❌ |

---

## Test 246

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701448 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.601513 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.587323 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.498767 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.480129 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 247

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549453 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.506815 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.475507 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.472465 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 5 | 0.455683 | `azmcp_keyvault_secret_get` | ❌ |

---

## Test 248

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581970 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.502245 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 3 | 0.501481 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.476470 | `azmcp_keyvault_key_get` | ❌ |
| 5 | 0.472357 | `azmcp_keyvault_secret_list` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641314 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.559550 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.553502 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.486377 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 5 | 0.475992 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 250

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.723266 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611366 | `azmcp_keyvault_certificate_list` | ❌ |
| 3 | 0.611048 | `azmcp_keyvault_secret_list` | ❌ |
| 4 | 0.473886 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 5 | 0.441881 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 251

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570444 | `azmcp_keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.500103 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.490316 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.489635 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 252

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678482 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.553018 | `azmcp_keyvault_key_create` | ❌ |
| 3 | 0.512856 | `azmcp_keyvault_secret_get` | ❌ |
| 4 | 0.475097 | `azmcp_keyvault_certificate_create` | ❌ |
| 5 | 0.461437 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 253

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663119 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.519597 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.512253 | `azmcp_appconfig_kv_set` | ❌ |
| 4 | 0.458577 | `azmcp_keyvault_key_create` | ❌ |
| 5 | 0.429813 | `azmcp_appconfig_kv_lock_set` | ❌ |

---

## Test 254

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

---

## Test 255

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674939 | `azmcp_keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.499434 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.497930 | `azmcp_keyvault_key_create` | ❌ |
| 4 | 0.478799 | `azmcp_keyvault_certificate_import` | ❌ |
| 5 | 0.458370 | `azmcp_appconfig_kv_set` | ❌ |

---

## Test 256

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

---

## Test 257

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602769 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504212 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.501397 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.478678 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `azmcp_keyvault_certificate_get` | ❌ |

---

## Test 258

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653871 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.566786 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.496050 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.485160 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.483567 | `azmcp_storage_account_get` | ❌ |

---

## Test 259

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578479 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.492213 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.488705 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.443606 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.424167 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 260

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649267 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.546992 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.492583 | `azmcp_keyvault_certificate_get` | ❌ |
| 4 | 0.491496 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.480354 | `azmcp_keyvault_secret_create` | ❌ |

---

## Test 261

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577477 | `azmcp_keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.475443 | `azmcp_keyvault_key_get` | ❌ |
| 3 | 0.466890 | `azmcp_keyvault_secret_create` | ❌ |
| 4 | 0.447538 | `azmcp_keyvault_secret_list` | ❌ |
| 5 | 0.421359 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 262

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701101 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.563736 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.538337 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.499642 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.455500 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 263

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555592 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.543861 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `azmcp_keyvault_key_get` | ❌ |
| 4 | 0.464661 | `azmcp_keyvault_key_list` | ❌ |
| 5 | 0.453130 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 264

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572452 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.529235 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.493738 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.487630 | `azmcp_keyvault_admin_settings_get` | ❌ |
| 5 | 0.475257 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 265

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624232 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.559681 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.517516 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.479547 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.442945 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 266

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742204 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.601183 | `azmcp_keyvault_key_list` | ❌ |
| 3 | 0.567827 | `azmcp_keyvault_certificate_list` | ❌ |
| 4 | 0.496127 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.437560 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 267

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567055 | `azmcp_keyvault_secret_list` | ✅ **EXPECTED** |
| 2 | 0.522398 | `azmcp_keyvault_secret_get` | ❌ |
| 3 | 0.476309 | `azmcp_keyvault_key_list` | ❌ |
| 4 | 0.462676 | `azmcp_keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `azmcp_keyvault_key_get` | ❌ |

---

## Test 268

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588300 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.544302 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.517279 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.481416 | `azmcp_mysql_server_config_get` | ❌ |
| 5 | 0.430975 | `azmcp_postgres_server_config_get` | ❌ |

---

## Test 269

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621759 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.575625 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.567870 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.475341 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.461466 | `azmcp_sql_db_show` | ❌ |

---

## Test 270

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522525 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.483220 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.434684 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.399164 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.380301 | `azmcp_mysql_server_config_get` | ❌ |

---

## Test 271

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588634 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.550555 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.527511 | `azmcp_kusto_cluster_get` | ❌ |
| 4 | 0.434587 | `azmcp_functionapp_get` | ❌ |
| 5 | 0.433913 | `azmcp_managedlustre_filesystem_list` | ❌ |

---

## Test 272

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756471 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.749416 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.634645 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.590166 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.568403 | `azmcp_kusto_database_list` | ❌ |

---

## Test 273

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612123 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.586661 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.545007 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.507757 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.489724 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 274

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628429 | `azmcp_aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563189 | `azmcp_aks_nodepool_get` | ❌ |
| 3 | 0.526756 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.478155 | `azmcp_redis_cluster_list` | ❌ |
| 5 | 0.426157 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 275

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729080 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.516885 | `azmcp_kusto_cluster_get` | ❌ |
| 3 | 0.509900 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.468389 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463119 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 276

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654106 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.458596 | `azmcp_sql_elastic-pool_list` | ❌ |
| 3 | 0.446035 | `azmcp_aks_cluster_get` | ❌ |
| 4 | 0.440182 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.413758 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 277

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592806 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.402556 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.385173 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.383045 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.355090 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 278

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692231 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.519037 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.506624 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500749 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.487707 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 279

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732239 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.561931 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.510395 | `azmcp_sql_elastic-pool_list` | ❌ |
| 4 | 0.509869 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.486790 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 280

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629359 | `azmcp_aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.456911 | `azmcp_aks_cluster_get` | ❌ |
| 3 | 0.443902 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.433006 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.425448 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 281

**Expected Tool:** `azmcp_loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577811 | `azmcp_loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.519643 | `azmcp_loadtesting_testresource_create` | ❌ |
| 3 | 0.512099 | `azmcp_loadtesting_testrun_create` | ❌ |
| 4 | 0.460717 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.432550 | `azmcp_loadtesting_test_get` | ❌ |

---

## Test 282

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626184 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.619952 | `azmcp_loadtesting_test_get` | ✅ **EXPECTED** |
| 3 | 0.594848 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.520722 | `azmcp_loadtesting_testrun_list` | ❌ |
| 5 | 0.476704 | `azmcp_loadtesting_testrun_create` | ❌ |

---

## Test 283

**Expected Tool:** `azmcp_loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645931 | `azmcp_loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.618706 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.541939 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.539947 | `azmcp_loadtesting_testrun_create` | ❌ |
| 5 | 0.442141 | `azmcp_workbooks_create` | ❌ |

---

## Test 284

**Expected Tool:** `azmcp_loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794326 | `azmcp_loadtesting_testresource_list` | ✅ **EXPECTED** |
| 2 | 0.577408 | `azmcp_group_list` | ❌ |
| 3 | 0.575434 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.565435 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.561516 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 285

**Expected Tool:** `azmcp_loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688976 | `azmcp_loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.594879 | `azmcp_loadtesting_testrun_update` | ❌ |
| 3 | 0.558636 | `azmcp_loadtesting_test_create` | ❌ |
| 4 | 0.547404 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.496224 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 286

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619146 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.601927 | `azmcp_loadtesting_test_get` | ❌ |
| 3 | 0.597617 | `azmcp_loadtesting_testresource_create` | ❌ |
| 4 | 0.565996 | `azmcp_loadtesting_testrun_list` | ❌ |
| 5 | 0.549898 | `azmcp_loadtesting_testrun_create` | ❌ |

---

## Test 287

**Expected Tool:** `azmcp_loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669180 | `azmcp_loadtesting_testresource_list` | ❌ |
| 2 | 0.640360 | `azmcp_loadtesting_testrun_list` | ✅ **EXPECTED** |
| 3 | 0.601075 | `azmcp_loadtesting_test_get` | ❌ |
| 4 | 0.577624 | `azmcp_loadtesting_testresource_create` | ❌ |
| 5 | 0.516544 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 288

**Expected Tool:** `azmcp_loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.706747 | `azmcp_loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.514428 | `azmcp_loadtesting_testrun_create` | ❌ |
| 3 | 0.470337 | `azmcp_loadtesting_testresource_list` | ❌ |
| 4 | 0.446897 | `azmcp_loadtesting_test_get` | ❌ |
| 5 | 0.429044 | `azmcp_loadtesting_testrun_get` | ❌ |

---

## Test 289

**Expected Tool:** `azmcp_grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599427 | `azmcp_kusto_cluster_list` | ❌ |
| 2 | 0.578892 | `azmcp_grafana_list` | ✅ **EXPECTED** |
| 3 | 0.551851 | `azmcp_search_service_list` | ❌ |
| 4 | 0.550372 | `azmcp_subscription_list` | ❌ |
| 5 | 0.522930 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 290

**Expected Tool:** `azmcp_managedlustre_filesystem_create`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728113 | `azmcp_managedlustre_filesystem_create` | ✅ **EXPECTED** |
| 2 | 0.616164 | `azmcp_managedlustre_filesystem_list` | ❌ |
| 3 | 0.605775 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.598255 | `azmcp_managedlustre_filesystem_update` | ❌ |
| 5 | 0.557720 | `azmcp_managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 291

**Expected Tool:** `azmcp_managedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `azmcp_managedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.582660 | `azmcp_managedlustre_filesystem_create` | ❌ |
| 4 | 0.562377 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.513156 | `azmcp_search_service_list` | ❌ |

---

## Test 292

**Expected Tool:** `azmcp_managedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743903 | `azmcp_managedlustre_filesystem_list` | ✅ **EXPECTED** |
| 2 | 0.613217 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.565856 | `azmcp_managedlustre_filesystem_create` | ❌ |
| 4 | 0.519908 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.515433 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 293

**Expected Tool:** `azmcp_managedlustre_filesystem_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827381 | `azmcp_managedlustre_filesystem_sku_get` | ✅ **EXPECTED** |
| 2 | 0.613674 | `azmcp_managedlustre_filesystem_list` | ❌ |
| 3 | 0.513242 | `azmcp_managedlustre_filesystem_create` | ❌ |
| 4 | 0.496242 | `azmcp_managedlustre_filesystem_subnetsize_validate` | ❌ |
| 5 | 0.470241 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 294

**Expected Tool:** `azmcp_managedlustre_filesystem_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739690 | `azmcp_managedlustre_filesystem_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.651598 | `azmcp_managedlustre_filesystem_subnetsize_validate` | ❌ |
| 3 | 0.594585 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.559498 | `azmcp_managedlustre_filesystem_list` | ❌ |
| 5 | 0.533684 | `azmcp_managedlustre_filesystem_create` | ❌ |

---

## Test 295

**Expected Tool:** `azmcp_managedlustre_filesystem_subnetsize_validate`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879389 | `azmcp_managedlustre_filesystem_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.622421 | `azmcp_managedlustre_filesystem_subnetsize_ask` | ❌ |
| 3 | 0.542808 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.515936 | `azmcp_managedlustre_filesystem_create` | ❌ |
| 5 | 0.480855 | `azmcp_managedlustre_filesystem_list` | ❌ |

---

## Test 296

**Expected Tool:** `azmcp_managedlustre_filesystem_update`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738879 | `azmcp_managedlustre_filesystem_update` | ✅ **EXPECTED** |
| 2 | 0.527596 | `azmcp_managedlustre_filesystem_create` | ❌ |
| 3 | 0.487186 | `azmcp_managedlustre_filesystem_list` | ❌ |
| 4 | 0.384990 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.344947 | `azmcp_managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 297

**Expected Tool:** `azmcp_marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570145 | `azmcp_marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.477522 | `azmcp_marketplace_product_list` | ❌ |
| 3 | 0.353256 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 4 | 0.333160 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.330935 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 298

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.527077 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443133 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.343549 | `azmcp_search_service_list` | ❌ |
| 4 | 0.330500 | `azmcp_foundry_models_list` | ❌ |
| 5 | 0.328676 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |

---

## Test 299

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461616 | `azmcp_marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.385167 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.308769 | `azmcp_foundry_models_list` | ❌ |
| 4 | 0.260387 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.247908 | `azmcp_eventgrid_topic_list` | ❌ |

---

## Test 300

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.635406 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.586810 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.531727 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490235 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 301

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.548542 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.540992 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.516852 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.516443 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 302

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.594323 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.518536 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.465572 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.450629 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 303

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.570488 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.522918 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.445382 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 304

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581850 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.497350 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495545 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.486886 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 5 | 0.474511 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 305

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610981 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.532776 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.487146 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.458067 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.413183 | `azmcp_functionapp_get` | ❌ |

---

## Test 306

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.513262 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.504889 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.421581 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 307

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `azmcp_get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.500368 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.471986 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.433134 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.432087 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 308

**Expected Tool:** `azmcp_monitor_activitylog_list`  
**Prompt:** List the activity logs of the last month for <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537893 | `azmcp_monitor_activitylog_list` | ✅ **EXPECTED** |
| 2 | 0.506212 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.371727 | `azmcp_monitor_workspace_log_query` | ❌ |
| 4 | 0.363299 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.344578 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 309

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660947 | `azmcp_monitor_healthmodels_entity_gethealth` | ✅ **EXPECTED** |
| 2 | 0.603767 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.355116 | `azmcp_foundry_openai_models-list` | ❌ |
| 4 | 0.351697 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.327953 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 310

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592640 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424141 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.368319 | `azmcp_bicepschema_get` | ❌ |
| 4 | 0.332356 | `azmcp_monitor_table_type_list` | ❌ |
| 5 | 0.322486 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 311

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589859 | `azmcp_storage_account_get` | ❌ |
| 2 | 0.587736 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.551156 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.473421 | `azmcp_managedlustre_filesystem_list` | ❌ |
| 5 | 0.472677 | `azmcp_storage_blob_get` | ❌ |

---

## Test 312

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633173 | `azmcp_monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495513 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.433945 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.392960 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.388750 | `azmcp_bicepschema_get` | ❌ |

---

## Test 313

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555377 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.527530 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.464743 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.419781 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.413282 | `azmcp_applicationinsights_recommendation_list` | ❌ |

---

## Test 314

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557830 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.476671 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.460611 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.438233 | `azmcp_monitor_metrics_definitions` | ❌ |

---

## Test 315

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461341 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390043 | `azmcp_monitor_metrics_definitions` | ❌ |
| 3 | 0.338607 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.329996 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.306282 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 316

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496878 | `azmcp_monitor_resource_log_query` | ❌ |
| 2 | 0.492138 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 3 | 0.448148 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.411728 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.397335 | `azmcp_quota_usage_check` | ❌ |

---

## Test 317

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525585 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.406185 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.384482 | `azmcp_monitor_metrics_definitions` | ❌ |
| 4 | 0.347723 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.325685 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 318

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480140 | `azmcp_monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.444779 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.388382 | `azmcp_applens_resource_diagnose` | ❌ |
| 4 | 0.363412 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.350045 | `azmcp_resourcehealth_service-health-events_list` | ❌ |

---

## Test 319

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688290 | `azmcp_monitor_resource_log_query` | ✅ **EXPECTED** |
| 2 | 0.622288 | `azmcp_monitor_workspace_log_query` | ❌ |
| 3 | 0.598554 | `azmcp_monitor_activitylog_list` | ❌ |
| 4 | 0.485613 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.469720 | `azmcp_monitor_metrics_query` | ❌ |

---

## Test 320

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.850709 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.541900 | `azmcp_kusto_table_list` | ❌ |
| 5 | 0.539481 | `azmcp_monitor_workspace_log_query` | ❌ |

---

## Test 321

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798154 | `azmcp_monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701122 | `azmcp_monitor_table_type_list` | ❌ |
| 3 | 0.599917 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.542821 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.502882 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 322

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881524 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765553 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.569921 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.504683 | `azmcp_mysql_table_list` | ❌ |
| 5 | 0.497622 | `azmcp_monitor_workspace_log_query` | ❌ |

---

## Test 323

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843138 | `azmcp_monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736735 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.576731 | `azmcp_monitor_workspace_list` | ❌ |
| 4 | 0.509598 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.481189 | `azmcp_mysql_table_list` | ❌ |

---

## Test 324

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `azmcp_grafana_list` | ❌ |
| 3 | 0.659475 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.610623 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.600802 | `azmcp_search_service_list` | ❌ |

---

## Test 325

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.584750 | `azmcp_monitor_table_list` | ❌ |
| 3 | 0.531083 | `azmcp_monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `azmcp_grafana_list` | ❌ |
| 5 | 0.506772 | `azmcp_monitor_workspace_log_query` | ❌ |

---

## Test 326

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `azmcp_monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `azmcp_grafana_list` | ❌ |
| 3 | 0.579659 | `azmcp_monitor_table_list` | ❌ |
| 4 | 0.523782 | `azmcp_monitor_workspace_log_query` | ❌ |
| 5 | 0.522749 | `azmcp_kusto_cluster_list` | ❌ |

---

## Test 327

**Expected Tool:** `azmcp_monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610115 | `azmcp_monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.587614 | `azmcp_monitor_resource_log_query` | ❌ |
| 3 | 0.527733 | `azmcp_monitor_activitylog_list` | ❌ |
| 4 | 0.498269 | `azmcp_deploy_app_logs_get` | ❌ |
| 5 | 0.485495 | `azmcp_monitor_table_list` | ❌ |

---

## Test 328

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668937 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.413677 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.413234 | `azmcp_monitor_metrics_query` | ❌ |
| 4 | 0.401693 | `azmcp_grafana_list` | ❌ |
| 5 | 0.393348 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 329

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624125 | `azmcp_datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443481 | `azmcp_monitor_metrics_query` | ❌ |
| 3 | 0.424391 | `azmcp_monitor_resource_log_query` | ❌ |
| 4 | 0.385122 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.371017 | `azmcp_grafana_list` | ❌ |

---

## Test 330

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `azmcp_quota_usage_check` | ❌ |
| 2 | 0.481143 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 3 | 0.476826 | `azmcp_extension_azqr` | ✅ **EXPECTED** |
| 4 | 0.471499 | `azmcp_subscription_list` | ❌ |
| 5 | 0.468404 | `azmcp_applens_resource_diagnose` | ❌ |

---

## Test 331

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532792 | `azmcp_azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.476164 | `azmcp_applicationinsights_recommendation_list` | ❌ |
| 4 | 0.473232 | `azmcp_deploy_iac_rules_get` | ❌ |
| 5 | 0.464604 | `azmcp_cloudarchitect_design` | ❌ |

---

## Test 332

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

---

## Test 333

**Expected Tool:** `azmcp_quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590878 | `azmcp_quota_region_availability_list` | ✅ **EXPECTED** |
| 2 | 0.413274 | `azmcp_quota_usage_check` | ❌ |
| 3 | 0.372940 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369855 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.362711 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 334

**Expected Tool:** `azmcp_quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609244 | `azmcp_quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `azmcp_quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.373815 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 5 | 0.362713 | `azmcp_monitor_resource_log_query` | ❌ |

---

## Test 335

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.539761 | `azmcp_subscription_list` | ❌ |
| 3 | 0.483988 | `azmcp_group_list` | ❌ |
| 4 | 0.478700 | `azmcp_grafana_list` | ❌ |
| 5 | 0.471364 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 336

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609705 | `azmcp_role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.514696 | `azmcp_subscription_list` | ❌ |
| 3 | 0.456956 | `azmcp_grafana_list` | ❌ |
| 4 | 0.449210 | `azmcp_eventgrid_subscription_list` | ❌ |
| 5 | 0.435155 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 337

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** List all access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757057 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.568469 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.448142 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.377563 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.322930 | `azmcp_mysql_database_list` | ❌ |

---

## Test 338

**Expected Tool:** `azmcp_redis_cache_accesspolicy_list`  
**Prompt:** Show me the access policies in the Redis Cache <cache_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713839 | `azmcp_redis_cache_accesspolicy_list` | ✅ **EXPECTED** |
| 2 | 0.564135 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.450265 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.338859 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.293658 | `azmcp_keyvault_admin_settings_get` | ❌ |

---

## Test 339

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** List all Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793711 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.660260 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.509917 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.501880 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 5 | 0.495048 | `azmcp_postgres_server_list` | ❌ |

---

## Test 340

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me my Redis Caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643453 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.524278 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.450387 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.401235 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.302323 | `azmcp_mysql_database_list` | ❌ |

---

## Test 341

**Expected Tool:** `azmcp_redis_cache_list`  
**Prompt:** Show me the Redis Caches in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.751312 | `azmcp_redis_cache_list` | ✅ **EXPECTED** |
| 2 | 0.631256 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.461603 | `azmcp_redis_cache_accesspolicy_list` | ❌ |
| 4 | 0.434924 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.427325 | `azmcp_grafana_list` | ❌ |

---

## Test 342

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** List all databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752919 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.643579 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.618537 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.548293 | `azmcp_postgres_database_list` | ❌ |
| 5 | 0.538403 | `azmcp_cosmos_database_list` | ❌ |

---

## Test 343

**Expected Tool:** `azmcp_redis_cluster_database_list`  
**Prompt:** Show me the databases in the Redis Cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.721506 | `azmcp_redis_cluster_database_list` | ✅ **EXPECTED** |
| 2 | 0.624961 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.560350 | `azmcp_kusto_database_list` | ❌ |
| 4 | 0.494301 | `azmcp_redis_cache_list` | ❌ |
| 5 | 0.490987 | `azmcp_mysql_database_list` | ❌ |

---

## Test 344

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** List all Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.844541 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.733512 | `azmcp_kusto_cluster_list` | ❌ |
| 3 | 0.665414 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.588847 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.571644 | `azmcp_kusto_database_list` | ❌ |

---

## Test 345

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me my Redis Clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688237 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.533499 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.514375 | `azmcp_redis_cluster_database_list` | ❌ |
| 4 | 0.448557 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.395942 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 346

**Expected Tool:** `azmcp_redis_cluster_list`  
**Prompt:** Show me the Redis Clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.797199 | `azmcp_redis_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.637109 | `azmcp_redis_cache_list` | ❌ |
| 3 | 0.633001 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.518857 | `azmcp_redis_cluster_database_list` | ❌ |
| 5 | 0.515638 | `azmcp_kusto_cluster_get` | ❌ |

---

## Test 347

**Expected Tool:** `azmcp_group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755935 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.566552 | `azmcp_workbooks_list` | ❌ |
| 3 | 0.564566 | `azmcp_loadtesting_testresource_list` | ❌ |
| 4 | 0.552456 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 5 | 0.546156 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 348

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.463589 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.462391 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.460280 | `azmcp_loadtesting_testresource_list` | ❌ |
| 5 | 0.459304 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 349

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665771 | `azmcp_group_list` | ✅ **EXPECTED** |
| 2 | 0.532469 | `azmcp_datadog_monitoredresources_list` | ❌ |
| 3 | 0.532054 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.531920 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.529702 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 350

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555166 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.404305 | `azmcp_foundry_openai_models-list` | ❌ |
| 4 | 0.377586 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.373112 | `azmcp_monitor_healthmodels_entity_gethealth` | ❌ |

---

## Test 351

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565992 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.549306 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.510357 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.466885 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.455902 | `azmcp_storage_account_create` | ❌ |

---

## Test 352

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 2 | 0.501221 | `azmcp_resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424939 | `azmcp_mysql_server_list` | ❌ |
| 4 | 0.413484 | `azmcp_foundry_openai_models-list` | ❌ |
| 5 | 0.412025 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 353

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.549914 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.548549 | `azmcp_grafana_list` | ❌ |
| 4 | 0.544505 | `azmcp_subscription_list` | ❌ |
| 5 | 0.540549 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 354

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.546808 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.508947 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.508252 | `azmcp_quota_usage_check` | ❌ |
| 5 | 0.473945 | `azmcp_datadog_monitoredresources_list` | ❌ |

---

## Test 355

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `azmcp_resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.550357 | `azmcp_resourcehealth_availability-status_get` | ❌ |
| 3 | 0.496373 | `azmcp_resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.441921 | `azmcp_applens_resource_diagnose` | ❌ |
| 5 | 0.433614 | `azmcp_loadtesting_testresource_list` | ❌ |

---

## Test 356

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690499 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.554895 | `azmcp_search_service_list` | ❌ |
| 3 | 0.534250 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.529761 | `azmcp_eventgrid_subscription_list` | ❌ |
| 5 | 0.518372 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 357

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686236 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.534556 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.513815 | `azmcp_search_service_list` | ❌ |
| 4 | 0.513259 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.501135 | `azmcp_subscription_list` | ❌ |

---

## Test 358

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.450423 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.267641 | `azmcp_applens_resource_diagnose` | ❌ |
| 3 | 0.245715 | `azmcp_cloudarchitect_design` | ❌ |
| 4 | 0.216797 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211849 | `azmcp_search_service_list` | ❌ |

---

## Test 359

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685312 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.527905 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.524063 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.520197 | `azmcp_search_service_list` | ❌ |
| 5 | 0.502064 | `azmcp_resourcehealth_availability-status_list` | ❌ |

---

## Test 360

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564764 | `azmcp_resourcehealth_service-health-events_list` | ✅ **EXPECTED** |
| 2 | 0.437868 | `azmcp_search_service_list` | ❌ |
| 3 | 0.403665 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.402493 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.402232 | `azmcp_foundry_agents_list` | ❌ |

---

## Test 361

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `azmcp_servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.436980 | `azmcp_servicebus_topic_details` | ❌ |
| 4 | 0.360754 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.352789 | `azmcp_storage_blob_get` | ❌ |

---

## Test 362

**Expected Tool:** `azmcp_servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642952 | `azmcp_servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571861 | `azmcp_servicebus_topic_subscription_details` | ❌ |
| 3 | 0.483976 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.482958 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.458711 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 363

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633196 | `azmcp_servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.517626 | `azmcp_servicebus_topic_details` | ❌ |
| 3 | 0.494442 | `azmcp_servicebus_queue_details` | ❌ |
| 4 | 0.493898 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.472167 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 364

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** Show me the details of SignalR <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532544 | `azmcp_signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.348345 | `azmcp_redis_cluster_list` | ❌ |
| 3 | 0.321769 | `azmcp_redis_cache_list` | ❌ |
| 4 | 0.319981 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.304420 | `azmcp_servicebus_queue_details` | ❌ |

---

## Test 365

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** Show me the network information of SignalR runtime <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573446 | `azmcp_signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.337342 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.305486 | `azmcp_redis_cluster_list` | ❌ |
| 4 | 0.300956 | `azmcp_servicebus_topic_details` | ❌ |
| 5 | 0.288269 | `azmcp_servicebus_topic_subscription_details` | ❌ |

---

## Test 366

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710353 | `azmcp_signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.411396 | `azmcp_loadtesting_testresource_list` | ❌ |
| 3 | 0.399412 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 4 | 0.382209 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.365585 | `azmcp_functionapp_get` | ❌ |

---

## Test 367

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.715974 | `azmcp_signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.430829 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.430765 | `azmcp_loadtesting_testresource_list` | ❌ |
| 4 | 0.417032 | `azmcp_functionapp_get` | ❌ |
| 5 | 0.402913 | `azmcp_sql_server_show` | ❌ |

---

## Test 368

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564072 | `azmcp_signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.494478 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 3 | 0.481428 | `azmcp_loadtesting_testresource_list` | ❌ |
| 4 | 0.462090 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.459689 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 369

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** List all SignalRs in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530646 | `azmcp_signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.507653 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.494498 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.487856 | `azmcp_subscription_list` | ❌ |
| 5 | 0.478456 | `azmcp_redis_cluster_list` | ❌ |

---

## Test 370

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470892 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.420504 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.408649 | `azmcp_sql_db_delete` | ❌ |
| 5 | 0.404860 | `azmcp_sql_server_delete` | ❌ |

---

## Test 371

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459672 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.437526 | `azmcp_sql_server_delete` | ❌ |
| 4 | 0.424021 | `azmcp_appservice_database_add` | ❌ |
| 5 | 0.420843 | `azmcp_sql_db_show` | ❌ |

---

## Test 372

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `azmcp_sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545906 | `azmcp_sql_server_create` | ❌ |
| 3 | 0.504013 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.494377 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.473975 | `azmcp_sql_db_list` | ❌ |

---

## Test 373

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.568282 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.567412 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.391509 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.386564 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 5 | 0.364776 | `azmcp_sql_db_show` | ❌ |

---

## Test 374

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567513 | `azmcp_sql_server_delete` | ❌ |
| 2 | 0.543453 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.500756 | `azmcp_sql_db_show` | ❌ |
| 4 | 0.481083 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.478729 | `azmcp_sql_db_list` | ❌ |

---

## Test 375

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509995 | `azmcp_sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.490892 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.364463 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.355416 | `azmcp_mysql_database_list` | ❌ |
| 5 | 0.347837 | `azmcp_sql_db_rename` | ❌ |

---

## Test 376

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643186 | `azmcp_sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639694 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.609147 | `azmcp_postgres_database_list` | ❌ |
| 4 | 0.602890 | `azmcp_cosmos_database_list` | ❌ |
| 5 | 0.570140 | `azmcp_kusto_database_list` | ❌ |

---

## Test 377

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

---

## Test 378

**Expected Tool:** `azmcp_sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593348 | `azmcp_sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.425282 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.416268 | `azmcp_sql_db_delete` | ❌ |
| 4 | 0.396947 | `azmcp_sql_db_create` | ❌ |
| 5 | 0.346018 | `azmcp_sql_db_show` | ❌ |

---

## Test 379

**Expected Tool:** `azmcp_sql_db_rename`  
**Prompt:** Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711063 | `azmcp_sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.516485 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.506588 | `azmcp_sql_db_delete` | ❌ |
| 4 | 0.501476 | `azmcp_sql_db_create` | ❌ |
| 5 | 0.433897 | `azmcp_sql_server_show` | ❌ |

---

## Test 380

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610918 | `azmcp_sql_server_show` | ❌ |
| 2 | 0.593315 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.530428 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.528014 | `azmcp_sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465639 | `azmcp_sql_db_list` | ❌ |

---

## Test 381

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

## Test 382

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603366 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.440493 | `azmcp_sql_db_rename` | ❌ |
| 4 | 0.427621 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.413941 | `azmcp_sql_server_delete` | ❌ |

---

## Test 383

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550556 | `azmcp_sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.418358 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.401817 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.395518 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.394770 | `azmcp_sql_db_show` | ❌ |

---

## Test 384

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678124 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502376 | `azmcp_sql_db_list` | ❌ |
| 3 | 0.498367 | `azmcp_mysql_database_list` | ❌ |
| 4 | 0.485249 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.479044 | `azmcp_sql_server_show` | ❌ |

---

## Test 385

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606425 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.457163 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.450743 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.432816 | `azmcp_mysql_database_list` | ❌ |

---

## Test 386

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592709 | `azmcp_sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420325 | `azmcp_mysql_database_list` | ❌ |
| 3 | 0.407169 | `azmcp_aks_nodepool_get` | ❌ |
| 4 | 0.402616 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.397670 | `azmcp_sql_db_list` | ❌ |

---

## Test 387

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682606 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563708 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.529507 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.482102 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.474207 | `azmcp_sql_db_rename` | ❌ |

---

## Test 388

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618267 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510192 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.472444 | `azmcp_sql_server_show` | ❌ |
| 4 | 0.441147 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.400971 | `azmcp_sql_db_rename` | ❌ |

---

## Test 389

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589818 | `azmcp_sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.501403 | `azmcp_sql_db_create` | ❌ |
| 3 | 0.498443 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.461181 | `azmcp_sql_db_rename` | ❌ |
| 5 | 0.442934 | `azmcp_mysql_server_list` | ❌ |

---

## Test 390

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656593 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.548111 | `azmcp_sql_db_delete` | ❌ |
| 3 | 0.518342 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.495550 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.484810 | `azmcp_workbooks_delete` | ❌ |

---

## Test 391

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615073 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393885 | `azmcp_postgres_server_list` | ❌ |
| 3 | 0.379846 | `azmcp_sql_db_delete` | ❌ |
| 4 | 0.376660 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.350236 | `azmcp_sql_server_list` | ❌ |

---

## Test 392

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624310 | `azmcp_sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.454997 | `azmcp_sql_db_delete` | ❌ |
| 3 | 0.362389 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.341503 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.316739 | `azmcp_workbooks_delete` | ❌ |

---

## Test 393

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783520 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.456051 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.435043 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.401908 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376055 | `azmcp_sql_db_list` | ❌ |

---

## Test 394

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713322 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.413144 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.368138 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.315966 | `azmcp_sql_db_list` | ❌ |
| 5 | 0.311085 | `azmcp_postgres_server_list` | ❌ |

---

## Test 395

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646423 | `azmcp_sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.356025 | `azmcp_sql_server_show` | ❌ |
| 3 | 0.322476 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.307823 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.269788 | `azmcp_sql_server_delete` | ❌ |

---

## Test 396

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635466 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532712 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448822 | `azmcp_sql_server_create` | ❌ |
| 5 | 0.440845 | `azmcp_sql_server_delete` | ❌ |

---

## Test 397

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670220 | `azmcp_sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533570 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503682 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316683 | `azmcp_sql_server_list` | ❌ |
| 5 | 0.302424 | `azmcp_sql_server_delete` | ❌ |

---

## Test 398

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

## Test 399

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.584379 | `azmcp_sql_server_delete` | ❌ |
| 3 | 0.543857 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 4 | 0.540333 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 5 | 0.498511 | `azmcp_sql_db_delete` | ❌ |

---

## Test 400

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670179 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574340 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.530419 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.488418 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.360414 | `azmcp_sql_db_delete` | ❌ |

---

## Test 401

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671212 | `azmcp_sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601230 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 3 | 0.577330 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 4 | 0.499272 | `azmcp_sql_server_delete` | ❌ |
| 5 | 0.378626 | `azmcp_sql_db_delete` | ❌ |

---

## Test 402

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729372 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.549667 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.418853 | `azmcp_sql_server_list` | ❌ |

---

## Test 403

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630731 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.524126 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410680 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.348108 | `azmcp_sql_server_list` | ❌ |

---

## Test 404

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630546 | `azmcp_sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.532454 | `azmcp_sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473501 | `azmcp_sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412957 | `azmcp_sql_server_show` | ❌ |
| 5 | 0.350469 | `azmcp_sql_server_list` | ❌ |

---

## Test 405

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694608 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596686 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.578239 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.515851 | `azmcp_sql_elastic-pool_list` | ❌ |
| 5 | 0.509789 | `azmcp_sql_db_show` | ❌ |

---

## Test 406

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618522 | `azmcp_sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593837 | `azmcp_mysql_server_list` | ❌ |
| 3 | 0.542398 | `azmcp_sql_db_list` | ❌ |
| 4 | 0.507404 | `azmcp_resourcehealth_availability-status_list` | ❌ |
| 5 | 0.496200 | `azmcp_group_list` | ❌ |

---

## Test 407

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `azmcp_sql_db_show` | ❌ |
| 2 | 0.595184 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 3 | 0.587841 | `azmcp_sql_server_list` | ❌ |
| 4 | 0.559893 | `azmcp_mysql_server_list` | ❌ |
| 5 | 0.540218 | `azmcp_sql_db_list` | ❌ |

---

## Test 408

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.538034 | `azmcp_mysql_server_config_get` | ❌ |
| 4 | 0.471541 | `azmcp_sql_db_show` | ❌ |
| 5 | 0.445500 | `azmcp_postgres_server_param_get` | ❌ |

---

## Test 409

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `azmcp_sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `azmcp_postgres_server_config_get` | ❌ |
| 3 | 0.380047 | `azmcp_postgres_server_param_get` | ❌ |
| 4 | 0.372194 | `azmcp_sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370539 | `azmcp_sql_db_show` | ❌ |

---

## Test 410

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.418472 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.394550 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.370957 | `azmcp_managedlustre_filesystem_create` | ❌ |
| 5 | 0.368591 | `azmcp_loadtesting_test_create` | ❌ |

---

## Test 411

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.483202 | `azmcp_managedlustre_filesystem_create` | ❌ |
| 3 | 0.400151 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.387071 | `azmcp_storage_account_get` | ❌ |
| 5 | 0.382836 | `azmcp_managedlustre_filesystem_list` | ❌ |

---

## Test 412

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589003 | `azmcp_storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.535501 | `azmcp_managedlustre_filesystem_create` | ❌ |
| 3 | 0.464646 | `azmcp_storage_blob_container_create` | ❌ |
| 4 | 0.447156 | `azmcp_sql_db_create` | ❌ |
| 5 | 0.437040 | `azmcp_storage_account_get` | ❌ |

---

## Test 413

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655152 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.603853 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.507638 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.483435 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.439236 | `azmcp_cosmos_account_list` | ❌ |

---

## Test 414

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.676876 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.612889 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.518215 | `azmcp_storage_account_create` | ❌ |
| 4 | 0.515153 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.427255 | `azmcp_resourcehealth_availability-status_get` | ❌ |

---

## Test 415

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.664087 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557016 | `azmcp_managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.547647 | `azmcp_subscription_list` | ❌ |
| 4 | 0.536909 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.535616 | `azmcp_storage_account_create` | ❌ |

---

## Test 416

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.499302 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.461284 | `azmcp_managedlustre_filesystem_list` | ❌ |
| 3 | 0.455449 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.421642 | `azmcp_cosmos_account_list` | ❌ |
| 5 | 0.395323 | `azmcp_subscription_list` | ❌ |

---

## Test 417

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557142 | `azmcp_storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.473598 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.465571 | `azmcp_subscription_list` | ❌ |
| 4 | 0.461641 | `azmcp_storage_blob_container_get` | ❌ |
| 5 | 0.436170 | `azmcp_search_service_list` | ❌ |

---

## Test 418

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563420 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.524779 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.508053 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.447784 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.403407 | `azmcp_storage_account_get` | ❌ |

---

## Test 419

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512591 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.500624 | `azmcp_storage_account_create` | ❌ |
| 3 | 0.470927 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.415378 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.414820 | `azmcp_storage_blob_get` | ❌ |

---

## Test 420

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.463198 | `azmcp_storage_account_create` | ❌ |
| 2 | 0.455375 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.451706 | `azmcp_storage_blob_container_create` | ✅ **EXPECTED** |
| 4 | 0.435099 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.407657 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 421

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665176 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.559177 | `azmcp_storage_account_get` | ❌ |
| 3 | 0.523288 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.518763 | `azmcp_storage_blob_get` | ❌ |
| 5 | 0.496195 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 422

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613933 | `azmcp_cosmos_database_container_list` | ❌ |
| 2 | 0.605437 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 3 | 0.521995 | `azmcp_storage_blob_get` | ❌ |
| 4 | 0.481153 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.479014 | `azmcp_storage_account_get` | ❌ |

---

## Test 423

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625166 | `azmcp_storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.511261 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.479580 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.439698 | `azmcp_storage_account_create` | ❌ |

---

## Test 424

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.613091 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.586289 | `azmcp_storage_blob_container_get` | ❌ |
| 3 | 0.483614 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.477946 | `azmcp_cosmos_database_container_list` | ❌ |
| 5 | 0.442685 | `azmcp_cosmos_database_container_item_query` | ❌ |

---

## Test 425

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.662106 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.661919 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.537535 | `azmcp_storage_account_get` | ❌ |
| 4 | 0.460674 | `azmcp_storage_blob_container_create` | ❌ |
| 5 | 0.457038 | `azmcp_storage_account_create` | ❌ |

---

## Test 426

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592723 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.579070 | `azmcp_cosmos_database_container_list` | ❌ |
| 3 | 0.568421 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 4 | 0.506639 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.465942 | `azmcp_storage_account_get` | ❌ |

---

## Test 427

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570353 | `azmcp_storage_blob_container_get` | ❌ |
| 2 | 0.549442 | `azmcp_storage_blob_get` | ✅ **EXPECTED** |
| 3 | 0.533515 | `azmcp_cosmos_database_container_list` | ❌ |
| 4 | 0.483885 | `azmcp_cosmos_database_container_item_query` | ❌ |
| 5 | 0.449128 | `azmcp_storage_account_get` | ❌ |

---

## Test 428

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566287 | `azmcp_storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.403451 | `azmcp_storage_blob_get` | ❌ |
| 3 | 0.397722 | `azmcp_storage_blob_container_get` | ❌ |
| 4 | 0.382123 | `azmcp_storage_account_create` | ❌ |
| 5 | 0.377281 | `azmcp_storage_blob_container_create` | ❌ |

---

## Test 429

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654071 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `azmcp_cosmos_account_list` | ❌ |
| 3 | 0.471653 | `azmcp_postgres_server_list` | ❌ |
| 4 | 0.469023 | `azmcp_kusto_cluster_list` | ❌ |
| 5 | 0.458014 | `azmcp_eventgrid_subscription_list` | ❌ |

---

## Test 430

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.458821 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.407471 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.393695 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.381238 | `azmcp_postgres_server_list` | ❌ |
| 5 | 0.366285 | `azmcp_redis_cache_list` | ❌ |

---

## Test 431

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433196 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.315547 | `azmcp_marketplace_product_get` | ❌ |
| 3 | 0.293772 | `azmcp_eventgrid_subscription_list` | ❌ |
| 4 | 0.289334 | `azmcp_eventgrid_topic_list` | ❌ |
| 5 | 0.288464 | `azmcp_redis_cache_list` | ❌ |

---

## Test 432

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.477592 | `azmcp_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.357625 | `azmcp_eventgrid_subscription_list` | ❌ |
| 3 | 0.340837 | `azmcp_eventgrid_topic_list` | ❌ |
| 4 | 0.340339 | `azmcp_grafana_list` | ❌ |
| 5 | 0.336798 | `azmcp_postgres_server_list` | ❌ |

---

## Test 433

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625083 | `azmcp_deploy_iac_rules_get` | ❌ |
| 3 | 0.605047 | `azmcp_get_bestpractices_get` | ❌ |
| 4 | 0.482936 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466199 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 434

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581316 | `azmcp_azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `azmcp_get_bestpractices_get` | ❌ |
| 3 | 0.509871 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.473596 | `azmcp_keyvault_secret_get` | ❌ |
| 5 | 0.444297 | `azmcp_deploy_pipeline_guidance_get` | ❌ |

---

## Test 435

**Expected Tool:** `azmcp_virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711969 | `azmcp_virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659763 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.620666 | `azmcp_kusto_cluster_list` | ❌ |
| 4 | 0.548888 | `azmcp_search_service_list` | ❌ |
| 5 | 0.535864 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 436

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ✅ **EXPECTED** |
| 2 | 0.714592 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.438659 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.393721 | `azmcp_sql_elastic-pool_list` | ❌ |

---

## Test 437

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812749 | `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list` | ✅ **EXPECTED** |
| 2 | 0.659212 | `azmcp_virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501167 | `azmcp_virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.357561 | `azmcp_aks_nodepool_get` | ❌ |
| 5 | 0.336385 | `azmcp_monitor_workspace_list` | ❌ |

---

## Test 438

**Expected Tool:** `azmcp_workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552212 | `azmcp_workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.433162 | `azmcp_workbooks_update` | ❌ |
| 3 | 0.361395 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.359495 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.328113 | `azmcp_workbooks_list` | ❌ |

---

## Test 439

**Expected Tool:** `azmcp_workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621115 | `azmcp_workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.518789 | `azmcp_workbooks_show` | ❌ |
| 3 | 0.432454 | `azmcp_workbooks_create` | ❌ |
| 4 | 0.425569 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.390355 | `azmcp_workbooks_update` | ❌ |

---

## Test 440

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772431 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562485 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.532887 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.516739 | `azmcp_grafana_list` | ❌ |
| 5 | 0.488600 | `azmcp_group_list` | ❌ |

---

## Test 441

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `azmcp_workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570259 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.540266 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.485525 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.472378 | `azmcp_grafana_list` | ❌ |

---

## Test 442

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.697627 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498390 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.494708 | `azmcp_workbooks_list` | ❌ |
| 4 | 0.451615 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.419105 | `azmcp_workbooks_update` | ❌ |

---

## Test 443

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Show me the workbook with display name <workbook_display_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469523 | `azmcp_workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.455158 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.437638 | `azmcp_workbooks_update` | ❌ |
| 4 | 0.424338 | `azmcp_workbooks_list` | ❌ |
| 5 | 0.366290 | `azmcp_workbooks_delete` | ❌ |

---

## Test 444

**Expected Tool:** `azmcp_workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.469915 | `azmcp_workbooks_update` | ✅ **EXPECTED** |
| 2 | 0.382651 | `azmcp_workbooks_create` | ❌ |
| 3 | 0.362225 | `azmcp_workbooks_show` | ❌ |
| 4 | 0.348613 | `azmcp_workbooks_delete` | ❌ |
| 5 | 0.292904 | `azmcp_loadtesting_testrun_update` | ❌ |

---

## Test 445

**Expected Tool:** `azmcp_bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543154 | `azmcp_bicepschema_get` | ✅ **EXPECTED** |
| 2 | 0.485970 | `azmcp_foundry_models_deploy` | ❌ |
| 3 | 0.485789 | `azmcp_deploy_iac_rules_get` | ❌ |
| 4 | 0.462071 | `azmcp_foundry_openai_embeddings-create` | ❌ |
| 5 | 0.448373 | `azmcp_get_bestpractices_get` | ❌ |

---

## Test 446

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502125 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290902 | `azmcp_storage_blob_upload` | ❌ |
| 3 | 0.259162 | `azmcp_managedlustre_filesystem_create` | ❌ |
| 4 | 0.254991 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.245034 | `azmcp_managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 447

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405148 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.267683 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258160 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.225870 | `azmcp_foundry_models_deploy` | ❌ |
| 5 | 0.225622 | `azmcp_deploy_plan_get` | ❌ |

---

## Test 448

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423577 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271943 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242581 | `azmcp_deploy_plan_get` | ❌ |
| 5 | 0.217985 | `azmcp_deploy_iac_rules_get` | ❌ |

---

## Test 449

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534690 | `azmcp_cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.369969 | `azmcp_deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.356331 | `azmcp_managedlustre_filesystem_create` | ❌ |
| 4 | 0.352797 | `azmcp_deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.323920 | `azmcp_storage_blob_upload` | ❌ |

---

## Summary

**Total Prompts Tested:** 449  
**Analysis Execution Time:** 56.4034200s  

### Success Rate Metrics

**Top Choice Success:** 83.1% (373/449 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 3.1% (14/449 tests)  
**🎯 High Confidence (≥0.7):** 18.0% (81/449 tests)  
**✅ Good Confidence (≥0.6):** 53.2% (239/449 tests)  
**👍 Fair Confidence (≥0.5):** 80.8% (363/449 tests)  
**👌 Acceptable Confidence (≥0.4):** 88.4% (397/449 tests)  
**❌ Low Confidence (<0.4):** 11.6% (52/449 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 3.1% (14/449 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 18.0% (81/449 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 51.7% (232/449 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 76.2% (342/449 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 82.0% (368/449 tests)  

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing adequately but has room for improvement.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

