# Tool Selection Analysis Setup

**Setup completed:** 2025-10-21 16:34:35  
**Tool count:** 174  
**Database setup time:** 1.4446213s  

---

# Tool Selection Analysis Results

**Analysis Date:** 2025-10-21 16:34:35  
**Tool count:** 174  

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
- [Test 61: azmcp_speech_tts_synthesize](#test-61)
- [Test 62: azmcp_speech_tts_synthesize](#test-62)
- [Test 63: azmcp_speech_tts_synthesize](#test-63)
- [Test 64: azmcp_speech_tts_synthesize](#test-64)
- [Test 65: azmcp_speech_tts_synthesize](#test-65)
- [Test 66: azmcp_speech_tts_synthesize](#test-66)
- [Test 67: azmcp_speech_tts_synthesize](#test-67)
- [Test 68: azmcp_speech_tts_synthesize](#test-68)
- [Test 69: azmcp_speech_tts_synthesize](#test-69)
- [Test 70: azmcp_speech_tts_synthesize](#test-70)
- [Test 71: azmcp_appconfig_account_list](#test-71)
- [Test 72: azmcp_appconfig_account_list](#test-72)
- [Test 73: azmcp_appconfig_account_list](#test-73)
- [Test 74: azmcp_appconfig_kv_delete](#test-74)
- [Test 75: azmcp_appconfig_kv_get](#test-75)
- [Test 76: azmcp_appconfig_kv_get](#test-76)
- [Test 77: azmcp_appconfig_kv_get](#test-77)
- [Test 78: azmcp_appconfig_kv_get](#test-78)
- [Test 79: azmcp_appconfig_kv_lock_set](#test-79)
- [Test 80: azmcp_appconfig_kv_lock_set](#test-80)
- [Test 81: azmcp_appconfig_kv_set](#test-81)
- [Test 82: azmcp_applens_resource_diagnose](#test-82)
- [Test 83: azmcp_applens_resource_diagnose](#test-83)
- [Test 84: azmcp_applens_resource_diagnose](#test-84)
- [Test 85: azmcp_appservice_database_add](#test-85)
- [Test 86: azmcp_appservice_database_add](#test-86)
- [Test 87: azmcp_appservice_database_add](#test-87)
- [Test 88: azmcp_appservice_database_add](#test-88)
- [Test 89: azmcp_appservice_database_add](#test-89)
- [Test 90: azmcp_appservice_database_add](#test-90)
- [Test 91: azmcp_appservice_database_add](#test-91)
- [Test 92: azmcp_appservice_database_add](#test-92)
- [Test 93: azmcp_appservice_database_add](#test-93)
- [Test 94: azmcp_applicationinsights_recommendation_list](#test-94)
- [Test 95: azmcp_applicationinsights_recommendation_list](#test-95)
- [Test 96: azmcp_applicationinsights_recommendation_list](#test-96)
- [Test 97: azmcp_applicationinsights_recommendation_list](#test-97)
- [Test 98: azmcp_extension_cli_generate](#test-98)
- [Test 99: azmcp_extension_cli_generate](#test-99)
- [Test 100: azmcp_extension_cli_generate](#test-100)
- [Test 101: azmcp_extension_cli_install](#test-101)
- [Test 102: azmcp_extension_cli_install](#test-102)
- [Test 103: azmcp_extension_cli_install](#test-103)
- [Test 104: azmcp_acr_registry_list](#test-104)
- [Test 105: azmcp_acr_registry_list](#test-105)
- [Test 106: azmcp_acr_registry_list](#test-106)
- [Test 107: azmcp_acr_registry_list](#test-107)
- [Test 108: azmcp_acr_registry_list](#test-108)
- [Test 109: azmcp_acr_registry_repository_list](#test-109)
- [Test 110: azmcp_acr_registry_repository_list](#test-110)
- [Test 111: azmcp_acr_registry_repository_list](#test-111)
- [Test 112: azmcp_acr_registry_repository_list](#test-112)
- [Test 113: azmcp_communication_email_send](#test-113)
- [Test 114: azmcp_communication_email_send](#test-114)
- [Test 115: azmcp_communication_email_send](#test-115)
- [Test 116: azmcp_communication_email_send](#test-116)
- [Test 117: azmcp_communication_email_send](#test-117)
- [Test 118: azmcp_communication_email_send](#test-118)
- [Test 119: azmcp_communication_email_send](#test-119)
- [Test 120: azmcp_communication_email_send](#test-120)
- [Test 121: azmcp_communication_sms_send](#test-121)
- [Test 122: azmcp_communication_sms_send](#test-122)
- [Test 123: azmcp_communication_sms_send](#test-123)
- [Test 124: azmcp_communication_sms_send](#test-124)
- [Test 125: azmcp_communication_sms_send](#test-125)
- [Test 126: azmcp_communication_sms_send](#test-126)
- [Test 127: azmcp_communication_sms_send](#test-127)
- [Test 128: azmcp_communication_sms_send](#test-128)
- [Test 129: azmcp_confidentialledger_entries_append](#test-129)
- [Test 130: azmcp_confidentialledger_entries_append](#test-130)
- [Test 131: azmcp_confidentialledger_entries_append](#test-131)
- [Test 132: azmcp_confidentialledger_entries_append](#test-132)
- [Test 133: azmcp_confidentialledger_entries_append](#test-133)
- [Test 134: azmcp_confidentialledger_entries_get](#test-134)
- [Test 135: azmcp_confidentialledger_entries_get](#test-135)
- [Test 136: azmcp_cosmos_account_list](#test-136)
- [Test 137: azmcp_cosmos_account_list](#test-137)
- [Test 138: azmcp_cosmos_account_list](#test-138)
- [Test 139: azmcp_cosmos_database_container_item_query](#test-139)
- [Test 140: azmcp_cosmos_database_container_list](#test-140)
- [Test 141: azmcp_cosmos_database_container_list](#test-141)
- [Test 142: azmcp_cosmos_database_list](#test-142)
- [Test 143: azmcp_cosmos_database_list](#test-143)
- [Test 144: azmcp_kusto_cluster_get](#test-144)
- [Test 145: azmcp_kusto_cluster_list](#test-145)
- [Test 146: azmcp_kusto_cluster_list](#test-146)
- [Test 147: azmcp_kusto_cluster_list](#test-147)
- [Test 148: azmcp_kusto_database_list](#test-148)
- [Test 149: azmcp_kusto_database_list](#test-149)
- [Test 150: azmcp_kusto_query](#test-150)
- [Test 151: azmcp_kusto_sample](#test-151)
- [Test 152: azmcp_kusto_table_list](#test-152)
- [Test 153: azmcp_kusto_table_list](#test-153)
- [Test 154: azmcp_kusto_table_schema](#test-154)
- [Test 155: azmcp_mysql_database_list](#test-155)
- [Test 156: azmcp_mysql_database_list](#test-156)
- [Test 157: azmcp_mysql_database_query](#test-157)
- [Test 158: azmcp_mysql_server_config_get](#test-158)
- [Test 159: azmcp_mysql_server_list](#test-159)
- [Test 160: azmcp_mysql_server_list](#test-160)
- [Test 161: azmcp_mysql_server_list](#test-161)
- [Test 162: azmcp_mysql_server_param_get](#test-162)
- [Test 163: azmcp_mysql_server_param_set](#test-163)
- [Test 164: azmcp_mysql_table_list](#test-164)
- [Test 165: azmcp_mysql_table_list](#test-165)
- [Test 166: azmcp_mysql_table_schema_get](#test-166)
- [Test 167: azmcp_postgres_database_list](#test-167)
- [Test 168: azmcp_postgres_database_list](#test-168)
- [Test 169: azmcp_postgres_database_query](#test-169)
- [Test 170: azmcp_postgres_server_config_get](#test-170)
- [Test 171: azmcp_postgres_server_list](#test-171)
- [Test 172: azmcp_postgres_server_list](#test-172)
- [Test 173: azmcp_postgres_server_list](#test-173)
- [Test 174: azmcp_postgres_server_param_get](#test-174)
- [Test 175: azmcp_postgres_server_param_set](#test-175)
- [Test 176: azmcp_postgres_table_list](#test-176)
- [Test 177: azmcp_postgres_table_list](#test-177)
- [Test 178: azmcp_postgres_table_schema_get](#test-178)
- [Test 179: azmcp_deploy_app_logs_get](#test-179)
- [Test 180: azmcp_deploy_architecture_diagram_generate](#test-180)
- [Test 181: azmcp_deploy_iac_rules_get](#test-181)
- [Test 182: azmcp_deploy_pipeline_guidance_get](#test-182)
- [Test 183: azmcp_deploy_plan_get](#test-183)
- [Test 184: azmcp_eventgrid_events_publish](#test-184)
- [Test 185: azmcp_eventgrid_events_publish](#test-185)
- [Test 186: azmcp_eventgrid_events_publish](#test-186)
- [Test 187: azmcp_eventgrid_topic_list](#test-187)
- [Test 188: azmcp_eventgrid_topic_list](#test-188)
- [Test 189: azmcp_eventgrid_topic_list](#test-189)
- [Test 190: azmcp_eventgrid_topic_list](#test-190)
- [Test 191: azmcp_eventgrid_subscription_list](#test-191)
- [Test 192: azmcp_eventgrid_subscription_list](#test-192)
- [Test 193: azmcp_eventgrid_subscription_list](#test-193)
- [Test 194: azmcp_eventgrid_subscription_list](#test-194)
- [Test 195: azmcp_eventgrid_subscription_list](#test-195)
- [Test 196: azmcp_eventgrid_subscription_list](#test-196)
- [Test 197: azmcp_eventgrid_subscription_list](#test-197)
- [Test 198: azmcp_eventhubs_consumergroup_delete](#test-198)
- [Test 199: azmcp_eventhubs_consumergroup_get](#test-199)
- [Test 200: azmcp_eventhubs_consumergroup_get](#test-200)
- [Test 201: azmcp_eventhubs_consumergroup_update](#test-201)
- [Test 202: azmcp_eventhubs_consumergroup_update](#test-202)
- [Test 203: azmcp_eventhubs_eventhub_delete](#test-203)
- [Test 204: azmcp_eventhubs_eventhub_get](#test-204)
- [Test 205: azmcp_eventhubs_eventhub_get](#test-205)
- [Test 206: azmcp_eventhubs_eventhub_update](#test-206)
- [Test 207: azmcp_eventhubs_eventhub_update](#test-207)
- [Test 208: azmcp_eventhubs_namespace_delete](#test-208)
- [Test 209: azmcp_eventhubs_namespace_get](#test-209)
- [Test 210: azmcp_eventhubs_namespace_get](#test-210)
- [Test 211: azmcp_eventhubs_namespace_update](#test-211)
- [Test 212: azmcp_eventhubs_namespace_update](#test-212)
- [Test 213: azmcp_functionapp_get](#test-213)
- [Test 214: azmcp_functionapp_get](#test-214)
- [Test 215: azmcp_functionapp_get](#test-215)
- [Test 216: azmcp_functionapp_get](#test-216)
- [Test 217: azmcp_functionapp_get](#test-217)
- [Test 218: azmcp_functionapp_get](#test-218)
- [Test 219: azmcp_functionapp_get](#test-219)
- [Test 220: azmcp_functionapp_get](#test-220)
- [Test 221: azmcp_functionapp_get](#test-221)
- [Test 222: azmcp_functionapp_get](#test-222)
- [Test 223: azmcp_functionapp_get](#test-223)
- [Test 224: azmcp_functionapp_get](#test-224)
- [Test 225: azmcp_keyvault_admin_settings_get](#test-225)
- [Test 226: azmcp_keyvault_admin_settings_get](#test-226)
- [Test 227: azmcp_keyvault_admin_settings_get](#test-227)
- [Test 228: azmcp_keyvault_certificate_create](#test-228)
- [Test 229: azmcp_keyvault_certificate_create](#test-229)
- [Test 230: azmcp_keyvault_certificate_create](#test-230)
- [Test 231: azmcp_keyvault_certificate_create](#test-231)
- [Test 232: azmcp_keyvault_certificate_create](#test-232)
- [Test 233: azmcp_keyvault_certificate_get](#test-233)
- [Test 234: azmcp_keyvault_certificate_get](#test-234)
- [Test 235: azmcp_keyvault_certificate_get](#test-235)
- [Test 236: azmcp_keyvault_certificate_get](#test-236)
- [Test 237: azmcp_keyvault_certificate_get](#test-237)
- [Test 238: azmcp_keyvault_certificate_import](#test-238)
- [Test 239: azmcp_keyvault_certificate_import](#test-239)
- [Test 240: azmcp_keyvault_certificate_import](#test-240)
- [Test 241: azmcp_keyvault_certificate_import](#test-241)
- [Test 242: azmcp_keyvault_certificate_import](#test-242)
- [Test 243: azmcp_keyvault_certificate_list](#test-243)
- [Test 244: azmcp_keyvault_certificate_list](#test-244)
- [Test 245: azmcp_keyvault_certificate_list](#test-245)
- [Test 246: azmcp_keyvault_certificate_list](#test-246)
- [Test 247: azmcp_keyvault_certificate_list](#test-247)
- [Test 248: azmcp_keyvault_certificate_list](#test-248)
- [Test 249: azmcp_keyvault_key_create](#test-249)
- [Test 250: azmcp_keyvault_key_create](#test-250)
- [Test 251: azmcp_keyvault_key_create](#test-251)
- [Test 252: azmcp_keyvault_key_create](#test-252)
- [Test 253: azmcp_keyvault_key_create](#test-253)
- [Test 254: azmcp_keyvault_key_get](#test-254)
- [Test 255: azmcp_keyvault_key_get](#test-255)
- [Test 256: azmcp_keyvault_key_get](#test-256)
- [Test 257: azmcp_keyvault_key_get](#test-257)
- [Test 258: azmcp_keyvault_key_get](#test-258)
- [Test 259: azmcp_keyvault_key_list](#test-259)
- [Test 260: azmcp_keyvault_key_list](#test-260)
- [Test 261: azmcp_keyvault_key_list](#test-261)
- [Test 262: azmcp_keyvault_key_list](#test-262)
- [Test 263: azmcp_keyvault_key_list](#test-263)
- [Test 264: azmcp_keyvault_key_list](#test-264)
- [Test 265: azmcp_keyvault_secret_create](#test-265)
- [Test 266: azmcp_keyvault_secret_create](#test-266)
- [Test 267: azmcp_keyvault_secret_create](#test-267)
- [Test 268: azmcp_keyvault_secret_create](#test-268)
- [Test 269: azmcp_keyvault_secret_create](#test-269)
- [Test 270: azmcp_keyvault_secret_get](#test-270)
- [Test 271: azmcp_keyvault_secret_get](#test-271)
- [Test 272: azmcp_keyvault_secret_get](#test-272)
- [Test 273: azmcp_keyvault_secret_get](#test-273)
- [Test 274: azmcp_keyvault_secret_get](#test-274)
- [Test 275: azmcp_keyvault_secret_list](#test-275)
- [Test 276: azmcp_keyvault_secret_list](#test-276)
- [Test 277: azmcp_keyvault_secret_list](#test-277)
- [Test 278: azmcp_keyvault_secret_list](#test-278)
- [Test 279: azmcp_keyvault_secret_list](#test-279)
- [Test 280: azmcp_keyvault_secret_list](#test-280)
- [Test 281: azmcp_aks_cluster_get](#test-281)
- [Test 282: azmcp_aks_cluster_get](#test-282)
- [Test 283: azmcp_aks_cluster_get](#test-283)
- [Test 284: azmcp_aks_cluster_get](#test-284)
- [Test 285: azmcp_aks_cluster_get](#test-285)
- [Test 286: azmcp_aks_cluster_get](#test-286)
- [Test 287: azmcp_aks_cluster_get](#test-287)
- [Test 288: azmcp_aks_nodepool_get](#test-288)
- [Test 289: azmcp_aks_nodepool_get](#test-289)
- [Test 290: azmcp_aks_nodepool_get](#test-290)
- [Test 291: azmcp_aks_nodepool_get](#test-291)
- [Test 292: azmcp_aks_nodepool_get](#test-292)
- [Test 293: azmcp_aks_nodepool_get](#test-293)
- [Test 294: azmcp_loadtesting_test_create](#test-294)
- [Test 295: azmcp_loadtesting_test_get](#test-295)
- [Test 296: azmcp_loadtesting_testresource_create](#test-296)
- [Test 297: azmcp_loadtesting_testresource_list](#test-297)
- [Test 298: azmcp_loadtesting_testrun_create](#test-298)
- [Test 299: azmcp_loadtesting_testrun_get](#test-299)
- [Test 300: azmcp_loadtesting_testrun_list](#test-300)
- [Test 301: azmcp_loadtesting_testrun_update](#test-301)
- [Test 302: azmcp_grafana_list](#test-302)
- [Test 303: azmcp_managedlustre_filesystem_create](#test-303)
- [Test 304: azmcp_managedlustre_filesystem_list](#test-304)
- [Test 305: azmcp_managedlustre_filesystem_list](#test-305)
- [Test 306: azmcp_managedlustre_filesystem_sku_get](#test-306)
- [Test 307: azmcp_managedlustre_filesystem_subnetsize_ask](#test-307)
- [Test 308: azmcp_managedlustre_filesystem_subnetsize_validate](#test-308)
- [Test 309: azmcp_managedlustre_filesystem_update](#test-309)
- [Test 310: azmcp_marketplace_product_get](#test-310)
- [Test 311: azmcp_marketplace_product_list](#test-311)
- [Test 312: azmcp_marketplace_product_list](#test-312)
- [Test 313: azmcp_get_bestpractices_get](#test-313)
- [Test 314: azmcp_get_bestpractices_get](#test-314)
- [Test 315: azmcp_get_bestpractices_get](#test-315)
- [Test 316: azmcp_get_bestpractices_get](#test-316)
- [Test 317: azmcp_get_bestpractices_get](#test-317)
- [Test 318: azmcp_get_bestpractices_get](#test-318)
- [Test 319: azmcp_get_bestpractices_get](#test-319)
- [Test 320: azmcp_get_bestpractices_get](#test-320)
- [Test 321: azmcp_monitor_activitylog_list](#test-321)
- [Test 322: azmcp_monitor_healthmodels_entity_gethealth](#test-322)
- [Test 323: azmcp_monitor_metrics_definitions](#test-323)
- [Test 324: azmcp_monitor_metrics_definitions](#test-324)
- [Test 325: azmcp_monitor_metrics_definitions](#test-325)
- [Test 326: azmcp_monitor_metrics_query](#test-326)
- [Test 327: azmcp_monitor_metrics_query](#test-327)
- [Test 328: azmcp_monitor_metrics_query](#test-328)
- [Test 329: azmcp_monitor_metrics_query](#test-329)
- [Test 330: azmcp_monitor_metrics_query](#test-330)
- [Test 331: azmcp_monitor_metrics_query](#test-331)
- [Test 332: azmcp_monitor_resource_log_query](#test-332)
- [Test 333: azmcp_monitor_table_list](#test-333)
- [Test 334: azmcp_monitor_table_list](#test-334)
- [Test 335: azmcp_monitor_table_type_list](#test-335)
- [Test 336: azmcp_monitor_table_type_list](#test-336)
- [Test 337: azmcp_monitor_webtests_create](#test-337)
- [Test 338: azmcp_monitor_webtests_get](#test-338)
- [Test 339: azmcp_monitor_webtests_list](#test-339)
- [Test 340: azmcp_monitor_webtests_list](#test-340)
- [Test 341: azmcp_monitor_webtests_update](#test-341)
- [Test 342: azmcp_monitor_workspace_list](#test-342)
- [Test 343: azmcp_monitor_workspace_list](#test-343)
- [Test 344: azmcp_monitor_workspace_list](#test-344)
- [Test 345: azmcp_monitor_workspace_log_query](#test-345)
- [Test 346: azmcp_datadog_monitoredresources_list](#test-346)
- [Test 347: azmcp_datadog_monitoredresources_list](#test-347)
- [Test 348: azmcp_extension_azqr](#test-348)
- [Test 349: azmcp_extension_azqr](#test-349)
- [Test 350: azmcp_extension_azqr](#test-350)
- [Test 351: azmcp_quota_region_availability_list](#test-351)
- [Test 352: azmcp_quota_usage_check](#test-352)
- [Test 353: azmcp_role_assignment_list](#test-353)
- [Test 354: azmcp_role_assignment_list](#test-354)
- [Test 355: azmcp_redis_list](#test-355)
- [Test 356: azmcp_redis_list](#test-356)
- [Test 357: azmcp_redis_list](#test-357)
- [Test 358: azmcp_redis_list](#test-358)
- [Test 359: azmcp_redis_list](#test-359)
- [Test 360: azmcp_group_list](#test-360)
- [Test 361: azmcp_group_list](#test-361)
- [Test 362: azmcp_group_list](#test-362)
- [Test 363: azmcp_resourcehealth_availability-status_get](#test-363)
- [Test 364: azmcp_resourcehealth_availability-status_get](#test-364)
- [Test 365: azmcp_resourcehealth_availability-status_get](#test-365)
- [Test 366: azmcp_resourcehealth_availability-status_list](#test-366)
- [Test 367: azmcp_resourcehealth_availability-status_list](#test-367)
- [Test 368: azmcp_resourcehealth_availability-status_list](#test-368)
- [Test 369: azmcp_resourcehealth_service-health-events_list](#test-369)
- [Test 370: azmcp_resourcehealth_service-health-events_list](#test-370)
- [Test 371: azmcp_resourcehealth_service-health-events_list](#test-371)
- [Test 372: azmcp_resourcehealth_service-health-events_list](#test-372)
- [Test 373: azmcp_resourcehealth_service-health-events_list](#test-373)
- [Test 374: azmcp_servicebus_queue_details](#test-374)
- [Test 375: azmcp_servicebus_topic_details](#test-375)
- [Test 376: azmcp_servicebus_topic_subscription_details](#test-376)
- [Test 377: azmcp_signalr_runtime_get](#test-377)
- [Test 378: azmcp_signalr_runtime_get](#test-378)
- [Test 379: azmcp_signalr_runtime_get](#test-379)
- [Test 380: azmcp_signalr_runtime_get](#test-380)
- [Test 381: azmcp_signalr_runtime_get](#test-381)
- [Test 382: azmcp_signalr_runtime_get](#test-382)
- [Test 383: azmcp_sql_db_create](#test-383)
- [Test 384: azmcp_sql_db_create](#test-384)
- [Test 385: azmcp_sql_db_create](#test-385)
- [Test 386: azmcp_sql_db_delete](#test-386)
- [Test 387: azmcp_sql_db_delete](#test-387)
- [Test 388: azmcp_sql_db_delete](#test-388)
- [Test 389: azmcp_sql_db_list](#test-389)
- [Test 390: azmcp_sql_db_list](#test-390)
- [Test 391: azmcp_sql_db_rename](#test-391)
- [Test 392: azmcp_sql_db_rename](#test-392)
- [Test 393: azmcp_sql_db_show](#test-393)
- [Test 394: azmcp_sql_db_show](#test-394)
- [Test 395: azmcp_sql_db_update](#test-395)
- [Test 396: azmcp_sql_db_update](#test-396)
- [Test 397: azmcp_sql_elastic-pool_list](#test-397)
- [Test 398: azmcp_sql_elastic-pool_list](#test-398)
- [Test 399: azmcp_sql_elastic-pool_list](#test-399)
- [Test 400: azmcp_sql_server_create](#test-400)
- [Test 401: azmcp_sql_server_create](#test-401)
- [Test 402: azmcp_sql_server_create](#test-402)
- [Test 403: azmcp_sql_server_delete](#test-403)
- [Test 404: azmcp_sql_server_delete](#test-404)
- [Test 405: azmcp_sql_server_delete](#test-405)
- [Test 406: azmcp_sql_server_entra-admin_list](#test-406)
- [Test 407: azmcp_sql_server_entra-admin_list](#test-407)
- [Test 408: azmcp_sql_server_entra-admin_list](#test-408)
- [Test 409: azmcp_sql_server_firewall-rule_create](#test-409)
- [Test 410: azmcp_sql_server_firewall-rule_create](#test-410)
- [Test 411: azmcp_sql_server_firewall-rule_create](#test-411)
- [Test 412: azmcp_sql_server_firewall-rule_delete](#test-412)
- [Test 413: azmcp_sql_server_firewall-rule_delete](#test-413)
- [Test 414: azmcp_sql_server_firewall-rule_delete](#test-414)
- [Test 415: azmcp_sql_server_firewall-rule_list](#test-415)
- [Test 416: azmcp_sql_server_firewall-rule_list](#test-416)
- [Test 417: azmcp_sql_server_firewall-rule_list](#test-417)
- [Test 418: azmcp_sql_server_list](#test-418)
- [Test 419: azmcp_sql_server_list](#test-419)
- [Test 420: azmcp_sql_server_show](#test-420)
- [Test 421: azmcp_sql_server_show](#test-421)
- [Test 422: azmcp_sql_server_show](#test-422)
- [Test 423: azmcp_storage_account_create](#test-423)
- [Test 424: azmcp_storage_account_create](#test-424)
- [Test 425: azmcp_storage_account_create](#test-425)
- [Test 426: azmcp_storage_account_get](#test-426)
- [Test 427: azmcp_storage_account_get](#test-427)
- [Test 428: azmcp_storage_account_get](#test-428)
- [Test 429: azmcp_storage_account_get](#test-429)
- [Test 430: azmcp_storage_account_get](#test-430)
- [Test 431: azmcp_storage_blob_container_create](#test-431)
- [Test 432: azmcp_storage_blob_container_create](#test-432)
- [Test 433: azmcp_storage_blob_container_create](#test-433)
- [Test 434: azmcp_storage_blob_container_get](#test-434)
- [Test 435: azmcp_storage_blob_container_get](#test-435)
- [Test 436: azmcp_storage_blob_container_get](#test-436)
- [Test 437: azmcp_storage_blob_get](#test-437)
- [Test 438: azmcp_storage_blob_get](#test-438)
- [Test 439: azmcp_storage_blob_get](#test-439)
- [Test 440: azmcp_storage_blob_get](#test-440)
- [Test 441: azmcp_storage_blob_upload](#test-441)
- [Test 442: azmcp_subscription_list](#test-442)
- [Test 443: azmcp_subscription_list](#test-443)
- [Test 444: azmcp_subscription_list](#test-444)
- [Test 445: azmcp_subscription_list](#test-445)
- [Test 446: azmcp_azureterraformbestpractices_get](#test-446)
- [Test 447: azmcp_azureterraformbestpractices_get](#test-447)
- [Test 448: azmcp_virtualdesktop_hostpool_list](#test-448)
- [Test 449: azmcp_virtualdesktop_hostpool_sessionhost_list](#test-449)
- [Test 450: azmcp_virtualdesktop_hostpool_sessionhost_usersession-list](#test-450)
- [Test 451: azmcp_workbooks_create](#test-451)
- [Test 452: azmcp_workbooks_delete](#test-452)
- [Test 453: azmcp_workbooks_list](#test-453)
- [Test 454: azmcp_workbooks_list](#test-454)
- [Test 455: azmcp_workbooks_show](#test-455)
- [Test 456: azmcp_workbooks_show](#test-456)
- [Test 457: azmcp_workbooks_update](#test-457)
- [Test 458: azmcp_bicepschema_get](#test-458)
- [Test 459: azmcp_cloudarchitect_design](#test-459)
- [Test 460: azmcp_cloudarchitect_design](#test-460)
- [Test 461: azmcp_cloudarchitect_design](#test-461)
- [Test 462: azmcp_cloudarchitect_design](#test-462)

---

## Test 1

**Expected Tool:** `azmcp_foundry_agents_connect`  
**Prompt:** Query an agent in my AI foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622854 | `foundry_agents_connect` | ❌ |
| 2 | 0.603124 | `foundry_agents_query-and-evaluate` | ❌ |
| 3 | 0.494462 | `foundry_agents_list` | ❌ |
| 4 | 0.443011 | `foundry_agents_evaluate` | ❌ |
| 5 | 0.392604 | `foundry_resource_get` | ❌ |

---

## Test 2

**Expected Tool:** `azmcp_foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.544099 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469428 | `foundry_agents_evaluate` | ❌ |
| 3 | 0.445964 | `foundry_agents_connect` | ❌ |
| 4 | 0.250023 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.235412 | `foundry_agents_list` | ❌ |

---

## Test 3

**Expected Tool:** `azmcp_foundry_agents_list`  
**Prompt:** List all agents in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.725613 | `foundry_agents_list` | ❌ |
| 2 | 0.468941 | `foundry_agents_connect` | ❌ |
| 3 | 0.457994 | `foundry_resource_get` | ❌ |
| 4 | 0.454081 | `foundry_models_list` | ❌ |
| 5 | 0.432863 | `foundry_models_deployments_list` | ❌ |

---

## Test 4

**Expected Tool:** `azmcp_foundry_agents_list`  
**Prompt:** Show me the available agents in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714029 | `foundry_agents_list` | ❌ |
| 2 | 0.479155 | `foundry_agents_connect` | ❌ |
| 3 | 0.469283 | `foundry_resource_get` | ❌ |
| 4 | 0.466215 | `foundry_models_list` | ❌ |
| 5 | 0.429842 | `foundry_knowledge_index_list` | ❌ |

---

## Test 5

**Expected Tool:** `azmcp_foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my AI Foundry project for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.580566 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.568662 | `foundry_agents_connect` | ❌ |
| 3 | 0.518655 | `foundry_agents_evaluate` | ❌ |
| 4 | 0.381887 | `foundry_agents_list` | ❌ |
| 5 | 0.326026 | `foundry_models_deploy` | ❌ |

---

## Test 6

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.695201 | `foundry_knowledge_index_list` | ❌ |
| 2 | 0.532985 | `foundry_agents_list` | ❌ |
| 3 | 0.526733 | `foundry_knowledge_index_schema` | ❌ |
| 4 | 0.441964 | `foundry_resource_get` | ❌ |
| 5 | 0.435696 | `search_knowledge_base_get` | ❌ |

---

## Test 7

**Expected Tool:** `azmcp_foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603396 | `foundry_knowledge_index_list` | ❌ |
| 2 | 0.489494 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.473949 | `foundry_agents_list` | ❌ |
| 4 | 0.441521 | `foundry_resource_get` | ❌ |
| 5 | 0.401099 | `search_knowledge_base_get` | ❌ |

---

## Test 8

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672692 | `foundry_knowledge_index_schema` | ❌ |
| 2 | 0.564860 | `foundry_knowledge_index_list` | ❌ |
| 3 | 0.431309 | `search_knowledge_base_get` | ❌ |
| 4 | 0.424581 | `search_index_get` | ❌ |
| 5 | 0.417942 | `search_knowledge_source_get` | ❌ |

---

## Test 9

**Expected Tool:** `azmcp_foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650189 | `foundry_knowledge_index_schema` | ❌ |
| 2 | 0.432777 | `postgres_table_schema_get` | ❌ |
| 3 | 0.417523 | `kusto_table_schema` | ❌ |
| 4 | 0.415963 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.398186 | `mysql_table_schema_get` | ❌ |

---

## Test 10

**Expected Tool:** `azmcp_foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.562920 | `foundry_models_deploy` | ❌ |
| 2 | 0.335116 | `foundry_openai_models-list` | ❌ |
| 3 | 0.298490 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.293050 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.282464 | `mysql_server_list` | ❌ |

---

## Test 11

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663735 | `foundry_models_deployments_list` | ❌ |
| 2 | 0.583429 | `foundry_openai_models-list` | ❌ |
| 3 | 0.566272 | `foundry_resource_get` | ❌ |
| 4 | 0.549636 | `foundry_models_list` | ❌ |
| 5 | 0.539695 | `foundry_agents_list` | ❌ |

---

## Test 12

**Expected Tool:** `azmcp_foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606689 | `foundry_models_deployments_list` | ❌ |
| 2 | 0.543352 | `foundry_resource_get` | ❌ |
| 3 | 0.521475 | `foundry_models_deploy` | ❌ |
| 4 | 0.518221 | `foundry_models_list` | ❌ |
| 5 | 0.507301 | `foundry_openai_models-list` | ❌ |

---

## Test 13

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560022 | `foundry_models_list` | ❌ |
| 2 | 0.514253 | `foundry_resource_get` | ❌ |
| 3 | 0.506535 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.491952 | `foundry_agents_list` | ❌ |
| 5 | 0.475204 | `foundry_openai_models-list` | ❌ |

---

## Test 14

**Expected Tool:** `azmcp_foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.574818 | `foundry_models_list` | ❌ |
| 2 | 0.525312 | `foundry_resource_get` | ❌ |
| 3 | 0.497191 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.475139 | `foundry_agents_list` | ❌ |
| 5 | 0.467671 | `foundry_models_deploy` | ❌ |

---

## Test 15

**Expected Tool:** `azmcp_foundry_openai_chat-completions-create`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.417723 | `foundry_openai_chat-completions-create` | ❌ |
| 2 | 0.332543 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.211879 | `foundry_agents_connect` | ❌ |
| 4 | 0.203502 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.168300 | `communication_sms_send` | ❌ |

---

## Test 16

**Expected Tool:** `azmcp_foundry_openai_create-completion`  
**Prompt:** Create a completion with the prompt "What is Azure?"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553675 | `foundry_openai_create-completion` | ❌ |
| 2 | 0.447828 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.434490 | `extension_cli_generate` | ❌ |
| 4 | 0.403536 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.394144 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 17

**Expected Tool:** `azmcp_foundry_openai_embeddings-create`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656252 | `foundry_openai_embeddings-create` | ❌ |
| 2 | 0.443461 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.408887 | `foundry_openai_models-list` | ❌ |
| 4 | 0.403947 | `foundry_models_deploy` | ❌ |
| 5 | 0.399865 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 18

**Expected Tool:** `azmcp_foundry_openai_embeddings-create`  
**Prompt:** Create vector embeddings for my text using Azure OpenAI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702777 | `foundry_openai_embeddings-create` | ❌ |
| 2 | 0.460359 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.426022 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.409975 | `foundry_models_deploy` | ❌ |
| 5 | 0.408297 | `foundry_openai_models-list` | ❌ |

---

## Test 19

**Expected Tool:** `azmcp_foundry_openai_models-list`  
**Prompt:** List all available OpenAI models in my Azure resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.788120 | `foundry_openai_models-list` | ❌ |
| 2 | 0.655391 | `foundry_agents_list` | ❌ |
| 3 | 0.586908 | `foundry_models_list` | ❌ |
| 4 | 0.565893 | `search_service_list` | ❌ |
| 5 | 0.562009 | `foundry_resource_get` | ❌ |

---

## Test 20

**Expected Tool:** `azmcp_foundry_openai_models-list`  
**Prompt:** Show me the OpenAI model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.575513 | `foundry_openai_models-list` | ❌ |
| 2 | 0.512409 | `foundry_models_deploy` | ❌ |
| 3 | 0.504046 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.412858 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.405167 | `foundry_agents_list` | ❌ |

---

## Test 21

**Expected Tool:** `azmcp_foundry_resource_get`  
**Prompt:** List all AI Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594096 | `foundry_resource_get` | ❌ |
| 2 | 0.558290 | `search_service_list` | ❌ |
| 3 | 0.524657 | `foundry_agents_list` | ❌ |
| 4 | 0.524645 | `grafana_list` | ❌ |
| 5 | 0.521417 | `redis_list` | ❌ |

---

## Test 22

**Expected Tool:** `azmcp_foundry_resource_get`  
**Prompt:** Show me the AI Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665311 | `foundry_resource_get` | ❌ |
| 2 | 0.492911 | `foundry_models_deploy` | ❌ |
| 3 | 0.474905 | `foundry_agents_list` | ❌ |
| 4 | 0.467211 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.453632 | `foundry_openai_models-list` | ❌ |

---

## Test 23

**Expected Tool:** `azmcp_foundry_resource_get`  
**Prompt:** Get details for AI Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.735316 | `foundry_resource_get` | ❌ |
| 2 | 0.509059 | `monitor_webtests_get` | ❌ |
| 3 | 0.455154 | `foundry_openai_models-list` | ❌ |
| 4 | 0.452340 | `foundry_models_deploy` | ❌ |
| 5 | 0.444390 | `loadtesting_testresource_list` | ❌ |

---

## Test 24

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.785967 | `search_knowledge_base_get` | ❌ |
| 2 | 0.700824 | `search_knowledge_source_get` | ❌ |
| 3 | 0.693471 | `search_service_list` | ❌ |
| 4 | 0.635863 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.603324 | `foundry_knowledge_index_list` | ❌ |

---

## Test 25

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.748213 | `search_knowledge_base_get` | ❌ |
| 2 | 0.668487 | `search_knowledge_source_get` | ❌ |
| 3 | 0.628582 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.624479 | `search_service_list` | ❌ |
| 5 | 0.566618 | `search_index_get` | ❌ |

---

## Test 26

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702942 | `search_knowledge_base_get` | ❌ |
| 2 | 0.605964 | `search_knowledge_source_get` | ❌ |
| 3 | 0.583234 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.513638 | `search_service_list` | ❌ |
| 5 | 0.494288 | `foundry_knowledge_index_list` | ❌ |

---

## Test 27

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688051 | `search_knowledge_base_get` | ❌ |
| 2 | 0.599247 | `search_knowledge_source_get` | ❌ |
| 3 | 0.578499 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.457619 | `search_service_list` | ❌ |
| 5 | 0.451801 | `foundry_knowledge_index_list` | ❌ |

---

## Test 28

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769402 | `search_knowledge_base_get` | ❌ |
| 2 | 0.685670 | `search_knowledge_source_get` | ❌ |
| 3 | 0.637009 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.585987 | `search_index_get` | ❌ |
| 5 | 0.533754 | `search_service_list` | ❌ |

---

## Test 29

**Expected Tool:** `azmcp_search_knowledge_base_get`  
**Prompt:** Show me the knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595585 | `search_knowledge_base_get` | ❌ |
| 2 | 0.551922 | `search_knowledge_base_retrieve` | ❌ |
| 3 | 0.515480 | `search_knowledge_source_get` | ❌ |
| 4 | 0.376599 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.366893 | `search_service_list` | ❌ |

---

## Test 30

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.724596 | `search_knowledge_base_retrieve` | ❌ |
| 2 | 0.650383 | `search_knowledge_base_get` | ❌ |
| 3 | 0.575323 | `search_index_query` | ❌ |
| 4 | 0.567236 | `search_knowledge_source_get` | ❌ |
| 5 | 0.520483 | `foundry_agents_connect` | ❌ |

---

## Test 31

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ❌ |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422671 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399595 | `search_index_query` | ❌ |

---

## Test 32

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657741 | `search_knowledge_base_retrieve` | ❌ |
| 2 | 0.557151 | `search_knowledge_base_get` | ❌ |
| 3 | 0.463517 | `search_knowledge_source_get` | ❌ |
| 4 | 0.436959 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.422341 | `foundry_agents_connect` | ❌ |

---

## Test 33

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ❌ |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422671 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399595 | `search_index_query` | ❌ |

---

## Test 34

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598868 | `search_knowledge_base_retrieve` | ❌ |
| 2 | 0.547862 | `search_knowledge_base_get` | ❌ |
| 3 | 0.467907 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.464904 | `search_knowledge_source_get` | ❌ |
| 5 | 0.412481 | `foundry_agents_connect` | ❌ |

---

## Test 35

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649767 | `search_knowledge_base_retrieve` | ❌ |
| 2 | 0.631435 | `search_knowledge_base_get` | ❌ |
| 3 | 0.581387 | `search_index_query` | ❌ |
| 4 | 0.571156 | `search_knowledge_source_get` | ❌ |
| 5 | 0.544501 | `search_service_list` | ❌ |

---

## Test 36

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579719 | `search_knowledge_base_retrieve` | ❌ |
| 2 | 0.560707 | `search_knowledge_base_get` | ❌ |
| 3 | 0.477984 | `search_knowledge_source_get` | ❌ |
| 4 | 0.402664 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.371117 | `foundry_knowledge_index_list` | ❌ |

---

## Test 37

**Expected Tool:** `azmcp_search_knowledge_base_retrieve`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582679 | `search_knowledge_base_retrieve` | ❌ |
| 2 | 0.528676 | `search_knowledge_base_get` | ❌ |
| 3 | 0.449380 | `search_knowledge_source_get` | ❌ |
| 4 | 0.447797 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.397180 | `foundry_agents_connect` | ❌ |

---

## Test 38

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760416 | `search_knowledge_source_get` | ❌ |
| 2 | 0.691931 | `search_service_list` | ❌ |
| 3 | 0.665923 | `search_knowledge_base_get` | ❌ |
| 4 | 0.579006 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.573012 | `search_index_get` | ❌ |

---

## Test 39

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737860 | `search_knowledge_source_get` | ❌ |
| 2 | 0.660170 | `search_service_list` | ❌ |
| 3 | 0.652969 | `search_knowledge_base_get` | ❌ |
| 4 | 0.578835 | `search_index_get` | ❌ |
| 5 | 0.560564 | `search_index_query` | ❌ |

---

## Test 40

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.657935 | `search_knowledge_source_get` | ❌ |
| 2 | 0.558516 | `search_knowledge_base_get` | ❌ |
| 3 | 0.511469 | `search_service_list` | ❌ |
| 4 | 0.470560 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.448614 | `foundry_knowledge_index_list` | ❌ |

---

## Test 41

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652945 | `search_knowledge_source_get` | ❌ |
| 2 | 0.563270 | `search_knowledge_base_get` | ❌ |
| 3 | 0.487022 | `search_service_list` | ❌ |
| 4 | 0.477636 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.430518 | `search_index_get` | ❌ |

---

## Test 42

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** Get the details of knowledge source <source-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.825604 | `search_knowledge_source_get` | ❌ |
| 2 | 0.693437 | `search_knowledge_base_get` | ❌ |
| 3 | 0.595643 | `search_index_get` | ❌ |
| 4 | 0.540550 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.531247 | `search_service_list` | ❌ |

---

## Test 43

**Expected Tool:** `azmcp_search_knowledge_source_get`  
**Prompt:** Show me the knowledge source <source-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630840 | `search_knowledge_source_get` | ❌ |
| 2 | 0.523643 | `search_knowledge_base_get` | ❌ |
| 3 | 0.459923 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.371465 | `search_index_get` | ❌ |
| 5 | 0.370838 | `search_service_list` | ❌ |

---

## Test 44

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681052 | `search_index_get` | ❌ |
| 2 | 0.544604 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.528153 | `search_knowledge_base_get` | ❌ |
| 4 | 0.521765 | `search_knowledge_source_get` | ❌ |
| 5 | 0.490624 | `search_service_list` | ❌ |

---

## Test 45

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640256 | `search_index_get` | ❌ |
| 2 | 0.620140 | `search_service_list` | ❌ |
| 3 | 0.561856 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.511485 | `search_knowledge_base_get` | ❌ |
| 5 | 0.496094 | `search_knowledge_source_get` | ❌ |

---

## Test 46

**Expected Tool:** `azmcp_search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620759 | `search_index_get` | ❌ |
| 2 | 0.562775 | `search_service_list` | ❌ |
| 3 | 0.561154 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.500365 | `search_knowledge_base_get` | ❌ |
| 5 | 0.490025 | `search_knowledge_source_get` | ❌ |

---

## Test 47

**Expected Tool:** `azmcp_search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522725 | `search_index_get` | ❌ |
| 2 | 0.515748 | `search_index_query` | ❌ |
| 3 | 0.497464 | `search_service_list` | ❌ |
| 4 | 0.447976 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.437626 | `postgres_database_query` | ❌ |

---

## Test 48

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793651 | `search_service_list` | ❌ |
| 2 | 0.553011 | `kusto_cluster_list` | ❌ |
| 3 | 0.520340 | `foundry_agents_list` | ❌ |
| 4 | 0.509461 | `subscription_list` | ❌ |
| 5 | 0.505971 | `search_index_get` | ❌ |

---

## Test 49

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686140 | `search_service_list` | ❌ |
| 2 | 0.483912 | `marketplace_product_list` | ❌ |
| 3 | 0.479898 | `search_index_get` | ❌ |
| 4 | 0.467337 | `foundry_agents_list` | ❌ |
| 5 | 0.462336 | `search_knowledge_base_get` | ❌ |

---

## Test 50

**Expected Tool:** `azmcp_search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.553025 | `search_service_list` | ❌ |
| 2 | 0.436230 | `search_index_get` | ❌ |
| 3 | 0.417096 | `foundry_agents_list` | ❌ |
| 4 | 0.415277 | `search_knowledge_base_get` | ❌ |
| 5 | 0.410461 | `search_knowledge_source_get` | ❌ |

---

## Test 51

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677871 | `speech_tts_synthesize` | ❌ |
| 2 | 0.666038 | `speech_stt_recognize` | ❌ |
| 3 | 0.415224 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.365228 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.351066 | `deploy_plan_get` | ❌ |

---

## Test 52

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511324 | `speech_stt_recognize` | ❌ |
| 2 | 0.353620 | `speech_tts_synthesize` | ❌ |
| 3 | 0.202056 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.190197 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.184542 | `foundry_openai_create-completion` | ❌ |

---

## Test 53

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486489 | `speech_stt_recognize` | ❌ |
| 2 | 0.354154 | `speech_tts_synthesize` | ❌ |
| 3 | 0.180941 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.178944 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.160209 | `foundry_agents_connect` | ❌ |

---

## Test 54

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.611992 | `speech_stt_recognize` | ❌ |
| 2 | 0.584104 | `speech_tts_synthesize` | ❌ |
| 3 | 0.322301 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.263196 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.251200 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 55

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.410533 | `speech_stt_recognize` | ❌ |
| 2 | 0.373433 | `speech_tts_synthesize` | ❌ |
| 3 | 0.159775 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.158032 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.152137 | `foundry_models_deploy` | ❌ |

---

## Test 56

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546259 | `speech_stt_recognize` | ❌ |
| 2 | 0.499808 | `speech_tts_synthesize` | ❌ |
| 3 | 0.225372 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.218092 | `foundry_resource_get` | ❌ |
| 5 | 0.200865 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 57

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.539963 | `speech_stt_recognize` | ❌ |
| 2 | 0.382022 | `speech_tts_synthesize` | ❌ |
| 3 | 0.246979 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.238192 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.203413 | `foundry_agents_connect` | ❌ |

---

## Test 58

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549151 | `speech_stt_recognize` | ❌ |
| 2 | 0.460662 | `speech_tts_synthesize` | ❌ |
| 3 | 0.357816 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.345661 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.342576 | `extension_cli_generate` | ❌ |

---

## Test 59

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532536 | `speech_stt_recognize` | ❌ |
| 2 | 0.506045 | `speech_tts_synthesize` | ❌ |
| 3 | 0.385033 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.381487 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.378382 | `foundry_openai_create-completion` | ❌ |

---

## Test 60

**Expected Tool:** `azmcp_speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453396 | `speech_stt_recognize` | ❌ |
| 2 | 0.342007 | `speech_tts_synthesize` | ❌ |
| 3 | 0.181994 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.174375 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.173205 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 61

**Expected Tool:** `azmcp_speech_tts_synthesize`  
**Prompt:** Convert text to speech and save to output.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547977 | `speech_tts_synthesize` | ❌ |
| 2 | 0.422457 | `speech_stt_recognize` | ❌ |
| 3 | 0.231058 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.200920 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.192179 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 62

**Expected Tool:** `azmcp_speech_tts_synthesize`  
**Prompt:** Synthesize speech from "Hello, welcome to Azure" and save to welcome.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531396 | `speech_tts_synthesize` | ❌ |
| 2 | 0.486019 | `speech_stt_recognize` | ❌ |
| 3 | 0.329765 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.323656 | `extension_cli_generate` | ❌ |
| 5 | 0.320006 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 63

**Expected Tool:** `azmcp_speech_tts_synthesize`  
**Prompt:** Generate speech audio from text "Hello world" using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590514 | `speech_tts_synthesize` | ❌ |
| 2 | 0.534002 | `speech_stt_recognize` | ❌ |
| 3 | 0.362626 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.341003 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.333557 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 64

**Expected Tool:** `azmcp_speech_tts_synthesize`  
**Prompt:** Convert text to speech with Spanish language and save to spanish-audio.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520866 | `speech_tts_synthesize` | ❌ |
| 2 | 0.452648 | `speech_stt_recognize` | ❌ |
| 3 | 0.231393 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.204970 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.202502 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 65

**Expected Tool:** `azmcp_speech_tts_synthesize`  
**Prompt:** Synthesize speech with voice en-US-JennyNeural from text "Azure AI Services"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604553 | `speech_tts_synthesize` | ❌ |
| 2 | 0.496715 | `speech_stt_recognize` | ❌ |
| 3 | 0.423461 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.390312 | `foundry_agents_list` | ❌ |
| 5 | 0.381735 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 66

**Expected Tool:** `azmcp_speech_tts_synthesize`  
**Prompt:** Create MP3 audio file from text "Welcome to Azure" with high quality format  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564876 | `speech_tts_synthesize` | ❌ |
| 2 | 0.510908 | `speech_stt_recognize` | ❌ |
| 3 | 0.360542 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.347597 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.345073 | `deploy_iac_rules_get` | ❌ |

---

## Test 67

**Expected Tool:** `azmcp_speech_tts_synthesize`  
**Prompt:** Generate speech with custom voice model using endpoint ID <endpoint-id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547864 | `speech_tts_synthesize` | ❌ |
| 2 | 0.455734 | `speech_stt_recognize` | ❌ |
| 3 | 0.367601 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.358913 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.356148 | `foundry_models_deployments_list` | ❌ |

---

## Test 68

**Expected Tool:** `azmcp_speech_tts_synthesize`  
**Prompt:** Convert text to OGG/Opus format audio file  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.446150 | `speech_tts_synthesize` | ❌ |
| 2 | 0.410086 | `speech_stt_recognize` | ❌ |
| 3 | 0.263503 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.199147 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.196061 | `extension_cli_generate` | ❌ |

---

## Test 69

**Expected Tool:** `azmcp_speech_tts_synthesize`  
**Prompt:** Synthesize long text content to audio file with streaming  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449165 | `speech_tts_synthesize` | ❌ |
| 2 | 0.369045 | `speech_stt_recognize` | ❌ |
| 3 | 0.225665 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.225088 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.218260 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 70

**Expected Tool:** `azmcp_speech_tts_synthesize`  
**Prompt:** Create audio file from text in French language with appropriate voice  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.467698 | `speech_tts_synthesize` | ❌ |
| 2 | 0.385267 | `speech_stt_recognize` | ❌ |
| 3 | 0.235591 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.215304 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.208978 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 71

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786360 | `appconfig_account_list` | ❌ |
| 2 | 0.530613 | `appconfig_kv_get` | ❌ |
| 3 | 0.491380 | `postgres_server_list` | ❌ |
| 4 | 0.481223 | `kusto_cluster_list` | ❌ |
| 5 | 0.479988 | `subscription_list` | ❌ |

---

## Test 72

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634978 | `appconfig_account_list` | ❌ |
| 2 | 0.464865 | `appconfig_kv_get` | ❌ |
| 3 | 0.398495 | `subscription_list` | ❌ |
| 4 | 0.391291 | `redis_list` | ❌ |
| 5 | 0.372456 | `postgres_server_list` | ❌ |

---

## Test 73

**Expected Tool:** `azmcp_appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565435 | `appconfig_account_list` | ❌ |
| 2 | 0.465344 | `appconfig_kv_get` | ❌ |
| 3 | 0.355916 | `postgres_server_config_get` | ❌ |
| 4 | 0.348661 | `appconfig_kv_delete` | ❌ |
| 5 | 0.327234 | `appconfig_kv_set` | ❌ |

---

## Test 74

**Expected Tool:** `azmcp_appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618277 | `appconfig_kv_delete` | ❌ |
| 2 | 0.464358 | `appconfig_kv_get` | ❌ |
| 3 | 0.424344 | `appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.392016 | `appconfig_account_list` | ❌ |

---

## Test 75

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.632687 | `appconfig_kv_get` | ❌ |
| 2 | 0.557810 | `appconfig_account_list` | ❌ |
| 3 | 0.530884 | `appconfig_kv_set` | ❌ |
| 4 | 0.464635 | `appconfig_kv_delete` | ❌ |
| 5 | 0.439089 | `appconfig_kv_lock_set` | ❌ |

---

## Test 76

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612555 | `appconfig_kv_get` | ❌ |
| 2 | 0.522426 | `appconfig_account_list` | ❌ |
| 3 | 0.512945 | `appconfig_kv_set` | ❌ |
| 4 | 0.468503 | `appconfig_kv_delete` | ❌ |
| 5 | 0.457866 | `appconfig_kv_lock_set` | ❌ |

---

## Test 77

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512883 | `appconfig_kv_get` | ❌ |
| 2 | 0.449905 | `appconfig_account_list` | ❌ |
| 3 | 0.398684 | `appconfig_kv_set` | ❌ |
| 4 | 0.380614 | `appconfig_kv_delete` | ❌ |
| 5 | 0.346166 | `appconfig_kv_lock_set` | ❌ |

---

## Test 78

**Expected Tool:** `azmcp_appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552300 | `appconfig_kv_get` | ❌ |
| 2 | 0.448912 | `appconfig_kv_set` | ❌ |
| 3 | 0.441713 | `appconfig_kv_delete` | ❌ |
| 4 | 0.437432 | `appconfig_account_list` | ❌ |
| 5 | 0.416264 | `appconfig_kv_lock_set` | ❌ |

---

## Test 79

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591237 | `appconfig_kv_lock_set` | ❌ |
| 2 | 0.487174 | `appconfig_kv_get` | ❌ |
| 3 | 0.445551 | `appconfig_kv_set` | ❌ |
| 4 | 0.431516 | `appconfig_kv_delete` | ❌ |
| 5 | 0.373656 | `appconfig_account_list` | ❌ |

---

## Test 80

**Expected Tool:** `azmcp_appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555699 | `appconfig_kv_lock_set` | ❌ |
| 2 | 0.505681 | `appconfig_kv_get` | ❌ |
| 3 | 0.476496 | `appconfig_kv_delete` | ❌ |
| 4 | 0.425488 | `appconfig_kv_set` | ❌ |
| 5 | 0.409406 | `appconfig_account_list` | ❌ |

---

## Test 81

**Expected Tool:** `azmcp_appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609635 | `appconfig_kv_set` | ❌ |
| 2 | 0.536497 | `appconfig_kv_lock_set` | ❌ |
| 3 | 0.512707 | `appconfig_kv_get` | ❌ |
| 4 | 0.505571 | `appconfig_kv_delete` | ❌ |
| 5 | 0.377919 | `appconfig_account_list` | ❌ |

---

## Test 82

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595632 | `applens_resource_diagnose` | ❌ |
| 2 | 0.336090 | `deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.273082 | `cloudarchitect_design` | ❌ |
| 5 | 0.254473 | `monitor_resource_log_query` | ❌ |

---

## Test 83

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502361 | `applens_resource_diagnose` | ❌ |
| 2 | 0.316297 | `deploy_app_logs_get` | ❌ |
| 3 | 0.255570 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.249583 | `monitor_resource_log_query` | ❌ |
| 5 | 0.225972 | `quota_usage_check` | ❌ |

---

## Test 84

**Expected Tool:** `azmcp_applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492820 | `applens_resource_diagnose` | ❌ |
| 2 | 0.256325 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.242574 | `cloudarchitect_design` | ❌ |
| 4 | 0.225608 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.211564 | `deploy_app_logs_get` | ❌ |

---

## Test 85

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a database connection to my app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729071 | `appservice_database_add` | ❌ |
| 2 | 0.398617 | `sql_db_create` | ❌ |
| 3 | 0.380126 | `sql_db_rename` | ❌ |
| 4 | 0.368222 | `sql_db_list` | ❌ |
| 5 | 0.364437 | `mysql_server_list` | ❌ |

---

## Test 86

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure a SQL Server database for app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612164 | `appservice_database_add` | ❌ |
| 2 | 0.472268 | `sql_db_update` | ❌ |
| 3 | 0.471103 | `sql_db_create` | ❌ |
| 4 | 0.454417 | `sql_db_rename` | ❌ |
| 5 | 0.412229 | `sql_server_delete` | ❌ |

---

## Test 87

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a MySQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.648464 | `appservice_database_add` | ❌ |
| 2 | 0.418902 | `sql_db_create` | ❌ |
| 3 | 0.409593 | `mysql_database_list` | ❌ |
| 4 | 0.397907 | `sql_db_rename` | ❌ |
| 5 | 0.382602 | `mysql_server_list` | ❌ |

---

## Test 88

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a PostgreSQL database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579502 | `appservice_database_add` | ❌ |
| 2 | 0.449085 | `postgres_database_list` | ❌ |
| 3 | 0.416365 | `postgres_server_param_set` | ❌ |
| 4 | 0.409515 | `postgres_table_list` | ❌ |
| 5 | 0.405431 | `postgres_server_list` | ❌ |

---

## Test 89

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add a CosmosDB database to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643046 | `appservice_database_add` | ❌ |
| 2 | 0.477031 | `cosmos_database_list` | ❌ |
| 3 | 0.465637 | `sql_db_create` | ❌ |
| 4 | 0.431581 | `sql_db_rename` | ❌ |
| 5 | 0.428761 | `cosmos_database_container_item_query` | ❌ |

---

## Test 90

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> on server <database_server> to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645533 | `appservice_database_add` | ❌ |
| 2 | 0.489228 | `sql_db_create` | ❌ |
| 3 | 0.440007 | `sql_db_rename` | ❌ |
| 4 | 0.431532 | `sql_db_delete` | ❌ |
| 5 | 0.426090 | `sql_server_delete` | ❌ |

---

## Test 91

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Set connection string for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665216 | `appservice_database_add` | ❌ |
| 2 | 0.401714 | `sql_db_rename` | ❌ |
| 3 | 0.369071 | `sql_db_create` | ❌ |
| 4 | 0.332119 | `appconfig_kv_set` | ❌ |
| 5 | 0.328120 | `sql_db_update` | ❌ |

---

## Test 92

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Configure tenant <tenant> for database <database_name> in app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536761 | `appservice_database_add` | ❌ |
| 2 | 0.408796 | `sql_db_rename` | ❌ |
| 3 | 0.394572 | `sql_db_create` | ❌ |
| 4 | 0.355035 | `sql_db_update` | ❌ |
| 5 | 0.329110 | `keyvault_secret_create` | ❌ |

---

## Test 93

**Expected Tool:** `azmcp_appservice_database_add`  
**Prompt:** Add database <database_name> with retry policy to app service <app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560268 | `appservice_database_add` | ❌ |
| 2 | 0.426753 | `sql_db_create` | ❌ |
| 3 | 0.392376 | `sql_db_rename` | ❌ |
| 4 | 0.371906 | `sql_db_delete` | ❌ |
| 5 | 0.361028 | `cosmos_database_list` | ❌ |

---

## Test 94

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572473 | `applicationinsights_recommendation_list` | ❌ |
| 2 | 0.445157 | `get_bestpractices_get` | ❌ |
| 3 | 0.390546 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.383948 | `applens_resource_diagnose` | ❌ |
| 5 | 0.375286 | `deploy_iac_rules_get` | ❌ |

---

## Test 95

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696531 | `applicationinsights_recommendation_list` | ❌ |
| 2 | 0.468384 | `get_bestpractices_get` | ❌ |
| 3 | 0.452231 | `applens_resource_diagnose` | ❌ |
| 4 | 0.435276 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.424622 | `search_service_list` | ❌ |

---

## Test 96

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `applicationinsights_recommendation_list` | ❌ |
| 2 | 0.488002 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.479392 | `mysql_server_list` | ❌ |
| 4 | 0.477396 | `applens_resource_diagnose` | ❌ |
| 5 | 0.468847 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 97

**Expected Tool:** `azmcp_applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509502 | `applicationinsights_recommendation_list` | ❌ |
| 2 | 0.419670 | `applens_resource_diagnose` | ❌ |
| 3 | 0.383767 | `get_bestpractices_get` | ❌ |
| 4 | 0.367278 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.343931 | `cloudarchitect_design` | ❌ |

---

## Test 98

**Expected Tool:** `azmcp_extension_cli_generate`  
**Prompt:** Get Azure CLI command to create a Storage account with name <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627896 | `storage_account_create` | ❌ |
| 2 | 0.569998 | `storage_blob_container_create` | ❌ |
| 3 | 0.548038 | `storage_account_get` | ❌ |
| 4 | 0.483493 | `sql_db_create` | ❌ |
| 5 | 0.480114 | `storage_blob_container_get` | ❌ |

---

## Test 99

**Expected Tool:** `azmcp_extension_cli_generate`  
**Prompt:** Show me how to use Azure CLI to list all virtual machines in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530014 | `search_service_list` | ❌ |
| 2 | 0.517673 | `kusto_cluster_list` | ❌ |
| 3 | 0.506746 | `redis_list` | ❌ |
| 4 | 0.493200 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.487633 | `subscription_list` | ❌ |

---

## Test 100

**Expected Tool:** `azmcp_extension_cli_generate`  
**Prompt:** Show me the details of the storage account <account_name> with Azure CLI commands  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710307 | `storage_account_get` | ❌ |
| 2 | 0.602173 | `storage_blob_container_get` | ❌ |
| 3 | 0.543268 | `storage_blob_get` | ❌ |
| 4 | 0.519788 | `storage_account_create` | ❌ |
| 5 | 0.493145 | `cosmos_account_list` | ❌ |

---

## Test 101

**Expected Tool:** `azmcp_extension_cli_install`  
**Prompt:** Get Azure CLI installation instructions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652199 | `extension_cli_install` | ❌ |
| 2 | 0.518876 | `extension_cli_generate` | ❌ |
| 3 | 0.430616 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.424984 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.420388 | `deploy_plan_get` | ❌ |

---

## Test 102

**Expected Tool:** `azmcp_extension_cli_install`  
**Prompt:** How to install azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.460416 | `extension_cli_install` | ❌ |
| 2 | 0.429599 | `deploy_app_logs_get` | ❌ |
| 3 | 0.365212 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.335288 | `deploy_plan_get` | ❌ |
| 5 | 0.326135 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 103

**Expected Tool:** `azmcp_extension_cli_install`  
**Prompt:** What is Azure Functions Core tools and how to install it  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622705 | `extension_cli_install` | ❌ |
| 2 | 0.439474 | `get_bestpractices_get` | ❌ |
| 3 | 0.432913 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.430554 | `extension_cli_generate` | ❌ |
| 5 | 0.418093 | `deploy_plan_get` | ❌ |

---

## Test 104

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `acr_registry_list` | ❌ |
| 2 | 0.711580 | `acr_registry_repository_list` | ❌ |
| 3 | 0.585675 | `kusto_cluster_list` | ❌ |
| 4 | 0.541506 | `search_service_list` | ❌ |
| 5 | 0.514293 | `cosmos_account_list` | ❌ |

---

## Test 105

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `acr_registry_list` | ❌ |
| 2 | 0.563636 | `acr_registry_repository_list` | ❌ |
| 3 | 0.460544 | `storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `cosmos_database_container_list` | ❌ |
| 5 | 0.402247 | `redis_list` | ❌ |

---

## Test 106

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637130 | `acr_registry_list` | ❌ |
| 2 | 0.563476 | `acr_registry_repository_list` | ❌ |
| 3 | 0.516769 | `kusto_cluster_list` | ❌ |
| 4 | 0.515378 | `storage_blob_container_get` | ❌ |
| 5 | 0.480352 | `redis_list` | ❌ |

---

## Test 107

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `acr_registry_list` | ❌ |
| 3 | 0.476015 | `mysql_server_list` | ❌ |
| 4 | 0.454929 | `group_list` | ❌ |
| 5 | 0.454003 | `datadog_monitoredresources_list` | ❌ |

---

## Test 108

**Expected Tool:** `azmcp_acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `acr_registry_list` | ❌ |
| 2 | 0.637972 | `acr_registry_repository_list` | ❌ |
| 3 | 0.468028 | `mysql_server_list` | ❌ |
| 4 | 0.449649 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `group_list` | ❌ |

---

## Test 109

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `acr_registry_repository_list` | ❌ |
| 2 | 0.617504 | `acr_registry_list` | ❌ |
| 3 | 0.544172 | `kusto_cluster_list` | ❌ |
| 4 | 0.508483 | `storage_blob_container_get` | ❌ |
| 5 | 0.495567 | `postgres_server_list` | ❌ |

---

## Test 110

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546333 | `acr_registry_repository_list` | ❌ |
| 2 | 0.469295 | `acr_registry_list` | ❌ |
| 3 | 0.451083 | `storage_blob_container_get` | ❌ |
| 4 | 0.407973 | `cosmos_database_container_list` | ❌ |
| 5 | 0.373464 | `storage_blob_get` | ❌ |

---

## Test 111

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `acr_registry_repository_list` | ❌ |
| 2 | 0.541779 | `acr_registry_list` | ❌ |
| 3 | 0.437509 | `storage_blob_container_get` | ❌ |
| 4 | 0.433927 | `cosmos_database_container_list` | ❌ |
| 5 | 0.383183 | `kusto_database_list` | ❌ |

---

## Test 112

**Expected Tool:** `azmcp_acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `acr_registry_repository_list` | ❌ |
| 2 | 0.501842 | `acr_registry_list` | ❌ |
| 3 | 0.430880 | `storage_blob_container_get` | ❌ |
| 4 | 0.418623 | `cosmos_database_container_list` | ❌ |
| 5 | 0.378151 | `redis_list` | ❌ |

---

## Test 113

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498292 | `communication_email_send` | ❌ |
| 2 | 0.229081 | `communication_sms_send` | ❌ |
| 3 | 0.188975 | `eventgrid_events_publish` | ❌ |
| 4 | 0.155364 | `speech_tts_synthesize` | ❌ |
| 5 | 0.145951 | `servicebus_topic_details` | ❌ |

---

## Test 114

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send an email from my communication service to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.498406 | `communication_email_send` | ❌ |
| 2 | 0.314462 | `communication_sms_send` | ❌ |
| 3 | 0.228890 | `speech_tts_synthesize` | ❌ |
| 4 | 0.218524 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.211154 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 115

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520967 | `communication_email_send` | ❌ |
| 2 | 0.207658 | `communication_sms_send` | ❌ |
| 3 | 0.152418 | `eventgrid_events_publish` | ❌ |
| 4 | 0.152013 | `servicebus_topic_details` | ❌ |
| 5 | 0.143660 | `foundry_agents_evaluate` | ❌ |

---

## Test 116

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533447 | `communication_email_send` | ❌ |
| 2 | 0.219584 | `communication_sms_send` | ❌ |
| 3 | 0.106026 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.087784 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.084905 | `cosmos_account_list` | ❌ |

---

## Test 117

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.540792 | `communication_email_send` | ❌ |
| 2 | 0.244521 | `communication_sms_send` | ❌ |
| 3 | 0.114324 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.098798 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.087041 | `postgres_server_param_set` | ❌ |

---

## Test 118

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.512623 | `communication_email_send` | ❌ |
| 2 | 0.200177 | `communication_sms_send` | ❌ |
| 3 | 0.164115 | `mysql_server_param_set` | ❌ |
| 4 | 0.158706 | `postgres_server_param_set` | ❌ |
| 5 | 0.143574 | `appconfig_kv_set` | ❌ |

---

## Test 119

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send email with custom sender name <sender-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.473175 | `communication_email_send` | ❌ |
| 2 | 0.255169 | `communication_sms_send` | ❌ |
| 3 | 0.156635 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.143626 | `sql_db_rename` | ❌ |
| 5 | 0.139388 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 120

**Expected Tool:** `azmcp_communication_email_send`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.528789 | `communication_email_send` | ❌ |
| 2 | 0.241114 | `communication_sms_send` | ❌ |
| 3 | 0.137538 | `confidentialledger_entries_append` | ❌ |
| 4 | 0.108748 | `confidentialledger_entries_get` | ❌ |
| 5 | 0.105033 | `storage_blob_upload` | ❌ |

---

## Test 121

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533868 | `communication_sms_send` | ❌ |
| 2 | 0.251429 | `communication_email_send` | ❌ |
| 3 | 0.178085 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.170676 | `speech_tts_synthesize` | ❌ |
| 5 | 0.148584 | `foundry_agents_connect` | ❌ |

---

## Test 122

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546019 | `communication_sms_send` | ❌ |
| 2 | 0.294859 | `communication_email_send` | ❌ |
| 3 | 0.204588 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.155927 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.139313 | `speech_tts_synthesize` | ❌ |

---

## Test 123

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.545755 | `communication_sms_send` | ❌ |
| 2 | 0.421988 | `communication_email_send` | ❌ |
| 3 | 0.142602 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.142030 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.104124 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 124

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554908 | `communication_sms_send` | ❌ |
| 2 | 0.269080 | `communication_email_send` | ❌ |
| 3 | 0.191848 | `extension_azqr` | ❌ |
| 4 | 0.170726 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.166385 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 125

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.538827 | `communication_sms_send` | ❌ |
| 2 | 0.269794 | `communication_email_send` | ❌ |
| 3 | 0.188153 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.159177 | `appconfig_kv_set` | ❌ |
| 5 | 0.158295 | `loadtesting_test_create` | ❌ |

---

## Test 126

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.474795 | `communication_sms_send` | ❌ |
| 2 | 0.286345 | `communication_email_send` | ❌ |
| 3 | 0.164287 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.129961 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.128721 | `cosmos_account_list` | ❌ |

---

## Test 127

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564114 | `communication_sms_send` | ❌ |
| 2 | 0.302363 | `communication_email_send` | ❌ |
| 3 | 0.213669 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.183651 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.177315 | `appservice_database_add` | ❌ |

---

## Test 128

**Expected Tool:** `azmcp_communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598211 | `communication_sms_send` | ❌ |
| 2 | 0.314134 | `communication_email_send` | ❌ |
| 3 | 0.206916 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.187824 | `confidentialledger_entries_append` | ❌ |
| 5 | 0.181824 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 129

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.510651 | `confidentialledger_entries_append` | ❌ |
| 2 | 0.294885 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.292014 | `appconfig_kv_set` | ❌ |
| 4 | 0.258967 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.249908 | `keyvault_certificate_import` | ❌ |

---

## Test 130

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602247 | `confidentialledger_entries_append` | ❌ |
| 2 | 0.357646 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.211990 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.195471 | `keyvault_secret_create` | ❌ |
| 5 | 0.184077 | `keyvault_certificate_import` | ❌ |

---

## Test 131

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546660 | `confidentialledger_entries_append` | ❌ |
| 2 | 0.451994 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.225141 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.215932 | `appconfig_kv_set` | ❌ |
| 5 | 0.211738 | `appservice_database_add` | ❌ |

---

## Test 132

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496023 | `confidentialledger_entries_append` | ❌ |
| 2 | 0.340187 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.218473 | `monitor_activitylog_list` | ❌ |
| 4 | 0.215229 | `storage_blob_container_create` | ❌ |
| 5 | 0.204925 | `monitor_resource_log_query` | ❌ |

---

## Test 133

**Expected Tool:** `azmcp_confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622138 | `confidentialledger_entries_append` | ❌ |
| 2 | 0.524777 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.252508 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.240252 | `keyvault_secret_create` | ❌ |
| 5 | 0.186890 | `appconfig_kv_set` | ❌ |

---

## Test 134

**Expected Tool:** `azmcp_confidentialledger_entries_get`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.707252 | `confidentialledger_entries_get` | ❌ |
| 2 | 0.551953 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.245541 | `keyvault_secret_get` | ❌ |
| 4 | 0.229943 | `keyvault_key_get` | ❌ |
| 5 | 0.212658 | `loadtesting_testrun_get` | ❌ |

---

## Test 135

**Expected Tool:** `azmcp_confidentialledger_entries_get`  
**Prompt:** Get transaction <transaction_id> from ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509714 | `confidentialledger_entries_get` | ❌ |
| 2 | 0.416580 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.224523 | `loadtesting_testrun_get` | ❌ |
| 4 | 0.218412 | `monitor_resource_log_query` | ❌ |
| 5 | 0.217671 | `loadtesting_testrun_list` | ❌ |

---

## Test 136

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818357 | `cosmos_account_list` | ❌ |
| 2 | 0.668480 | `cosmos_database_list` | ❌ |
| 3 | 0.636036 | `subscription_list` | ❌ |
| 4 | 0.615268 | `cosmos_database_container_list` | ❌ |
| 5 | 0.601467 | `kusto_cluster_list` | ❌ |

---

## Test 137

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665447 | `cosmos_account_list` | ❌ |
| 2 | 0.605357 | `cosmos_database_list` | ❌ |
| 3 | 0.571613 | `cosmos_database_container_list` | ❌ |
| 4 | 0.549433 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.503830 | `storage_account_get` | ❌ |

---

## Test 138

**Expected Tool:** `azmcp_cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.752494 | `cosmos_account_list` | ❌ |
| 2 | 0.607201 | `subscription_list` | ❌ |
| 3 | 0.605125 | `cosmos_database_list` | ❌ |
| 4 | 0.566249 | `cosmos_database_container_list` | ❌ |
| 5 | 0.563870 | `cosmos_database_container_item_query` | ❌ |

---

## Test 139

**Expected Tool:** `azmcp_cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658445 | `cosmos_database_container_item_query` | ❌ |
| 2 | 0.605253 | `cosmos_database_container_list` | ❌ |
| 3 | 0.487612 | `storage_blob_container_get` | ❌ |
| 4 | 0.477874 | `cosmos_database_list` | ❌ |
| 5 | 0.447757 | `cosmos_account_list` | ❌ |

---

## Test 140

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.852775 | `cosmos_database_container_list` | ❌ |
| 2 | 0.680969 | `cosmos_database_list` | ❌ |
| 3 | 0.680478 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.632356 | `storage_blob_container_get` | ❌ |
| 5 | 0.630622 | `cosmos_account_list` | ❌ |

---

## Test 141

**Expected Tool:** `azmcp_cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789395 | `cosmos_database_container_list` | ❌ |
| 2 | 0.647811 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.614220 | `cosmos_database_list` | ❌ |
| 4 | 0.591361 | `storage_blob_container_get` | ❌ |
| 5 | 0.562062 | `cosmos_account_list` | ❌ |

---

## Test 142

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `cosmos_database_list` | ❌ |
| 2 | 0.668515 | `cosmos_account_list` | ❌ |
| 3 | 0.665298 | `cosmos_database_container_list` | ❌ |
| 4 | 0.606307 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.583535 | `kusto_database_list` | ❌ |

---

## Test 143

**Expected Tool:** `azmcp_cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `cosmos_database_list` | ❌ |
| 2 | 0.624759 | `cosmos_database_container_list` | ❌ |
| 3 | 0.614572 | `cosmos_account_list` | ❌ |
| 4 | 0.579718 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.538479 | `mysql_database_list` | ❌ |

---

## Test 144

**Expected Tool:** `azmcp_kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590264 | `kusto_cluster_get` | ❌ |
| 2 | 0.463832 | `kusto_cluster_list` | ❌ |
| 3 | 0.428077 | `kusto_query` | ❌ |
| 4 | 0.425669 | `kusto_database_list` | ❌ |
| 5 | 0.422370 | `kusto_table_schema` | ❌ |

---

## Test 145

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793744 | `kusto_cluster_list` | ❌ |
| 2 | 0.630507 | `kusto_database_list` | ❌ |
| 3 | 0.573395 | `kusto_cluster_get` | ❌ |
| 4 | 0.525057 | `aks_cluster_get` | ❌ |
| 5 | 0.509396 | `grafana_list` | ❌ |

---

## Test 146

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531307 | `kusto_cluster_list` | ❌ |
| 2 | 0.465277 | `kusto_cluster_get` | ❌ |
| 3 | 0.432288 | `kusto_database_list` | ❌ |
| 4 | 0.369561 | `aks_cluster_get` | ❌ |
| 5 | 0.363063 | `kusto_table_schema` | ❌ |

---

## Test 147

**Expected Tool:** `azmcp_kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701484 | `kusto_cluster_list` | ❌ |
| 2 | 0.571191 | `kusto_cluster_get` | ❌ |
| 3 | 0.548685 | `kusto_database_list` | ❌ |
| 4 | 0.498904 | `aks_cluster_get` | ❌ |
| 5 | 0.474201 | `redis_list` | ❌ |

---

## Test 148

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677059 | `kusto_database_list` | ❌ |
| 2 | 0.560592 | `kusto_cluster_list` | ❌ |
| 3 | 0.556795 | `kusto_table_list` | ❌ |
| 4 | 0.553218 | `postgres_database_list` | ❌ |
| 5 | 0.549673 | `cosmos_database_list` | ❌ |

---

## Test 149

**Expected Tool:** `azmcp_kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.623523 | `kusto_database_list` | ❌ |
| 2 | 0.509953 | `kusto_cluster_list` | ❌ |
| 3 | 0.507073 | `kusto_table_list` | ❌ |
| 4 | 0.497144 | `cosmos_database_list` | ❌ |
| 5 | 0.491400 | `mysql_database_list` | ❌ |

---

## Test 150

**Expected Tool:** `azmcp_kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423852 | `kusto_query` | ❌ |
| 2 | 0.409647 | `postgres_database_query` | ❌ |
| 3 | 0.408335 | `kusto_table_schema` | ❌ |
| 4 | 0.407872 | `kusto_sample` | ❌ |
| 5 | 0.404108 | `kusto_cluster_list` | ❌ |

---

## Test 151

**Expected Tool:** `azmcp_kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595574 | `kusto_sample` | ❌ |
| 2 | 0.510303 | `kusto_table_schema` | ❌ |
| 3 | 0.424212 | `kusto_table_list` | ❌ |
| 4 | 0.400924 | `kusto_cluster_list` | ❌ |
| 5 | 0.399525 | `kusto_cluster_get` | ❌ |

---

## Test 152

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.679642 | `kusto_table_list` | ❌ |
| 2 | 0.585237 | `postgres_table_list` | ❌ |
| 3 | 0.581207 | `kusto_database_list` | ❌ |
| 4 | 0.556672 | `mysql_table_list` | ❌ |
| 5 | 0.550007 | `monitor_table_list` | ❌ |

---

## Test 153

**Expected Tool:** `azmcp_kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619252 | `kusto_table_list` | ❌ |
| 2 | 0.554219 | `kusto_table_schema` | ❌ |
| 3 | 0.527626 | `kusto_database_list` | ❌ |
| 4 | 0.524586 | `mysql_table_list` | ❌ |
| 5 | 0.523432 | `postgres_table_list` | ❌ |

---

## Test 154

**Expected Tool:** `azmcp_kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.666972 | `kusto_table_schema` | ❌ |
| 2 | 0.564332 | `postgres_table_schema_get` | ❌ |
| 3 | 0.527917 | `mysql_table_schema_get` | ❌ |
| 4 | 0.490928 | `kusto_sample` | ❌ |
| 5 | 0.489680 | `kusto_table_list` | ❌ |

---

## Test 155

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.634056 | `postgres_database_list` | ❌ |
| 2 | 0.623421 | `mysql_database_list` | ❌ |
| 3 | 0.534661 | `mysql_table_list` | ❌ |
| 4 | 0.498918 | `mysql_server_list` | ❌ |
| 5 | 0.490273 | `sql_db_list` | ❌ |

---

## Test 156

**Expected Tool:** `azmcp_mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588122 | `mysql_database_list` | ❌ |
| 2 | 0.574089 | `postgres_database_list` | ❌ |
| 3 | 0.483986 | `mysql_table_list` | ❌ |
| 4 | 0.463244 | `mysql_server_list` | ❌ |
| 5 | 0.444677 | `sql_db_list` | ❌ |

---

## Test 157

**Expected Tool:** `azmcp_mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.476539 | `mysql_table_list` | ❌ |
| 2 | 0.455770 | `mysql_database_list` | ❌ |
| 3 | 0.433392 | `mysql_database_query` | ❌ |
| 4 | 0.419859 | `mysql_server_list` | ❌ |
| 5 | 0.409445 | `mysql_table_schema_get` | ❌ |

---

## Test 158

**Expected Tool:** `azmcp_mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531887 | `postgres_server_config_get` | ❌ |
| 2 | 0.516894 | `mysql_server_param_set` | ❌ |
| 3 | 0.489816 | `mysql_server_config_get` | ❌ |
| 4 | 0.476863 | `mysql_server_param_get` | ❌ |
| 5 | 0.426507 | `mysql_table_schema_get` | ❌ |

---

## Test 159

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678472 | `postgres_server_list` | ❌ |
| 2 | 0.558177 | `mysql_database_list` | ❌ |
| 3 | 0.554817 | `mysql_server_list` | ❌ |
| 4 | 0.513706 | `kusto_cluster_list` | ❌ |
| 5 | 0.501329 | `mysql_table_list` | ❌ |

---

## Test 160

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478518 | `mysql_database_list` | ❌ |
| 2 | 0.474586 | `mysql_server_list` | ❌ |
| 3 | 0.435642 | `postgres_server_list` | ❌ |
| 4 | 0.412398 | `mysql_table_list` | ❌ |
| 5 | 0.389993 | `postgres_database_list` | ❌ |

---

## Test 161

**Expected Tool:** `azmcp_mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.636435 | `postgres_server_list` | ❌ |
| 2 | 0.534266 | `mysql_server_list` | ❌ |
| 3 | 0.530210 | `mysql_database_list` | ❌ |
| 4 | 0.475052 | `kusto_cluster_list` | ❌ |
| 5 | 0.470468 | `redis_list` | ❌ |

---

## Test 162

**Expected Tool:** `azmcp_mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `mysql_server_param_get` | ❌ |
| 2 | 0.438075 | `mysql_server_param_set` | ❌ |
| 3 | 0.333841 | `mysql_database_query` | ❌ |
| 4 | 0.313150 | `mysql_table_schema_get` | ❌ |
| 5 | 0.310834 | `postgres_server_param_get` | ❌ |

---

## Test 163

**Expected Tool:** `azmcp_mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449419 | `mysql_server_param_set` | ❌ |
| 2 | 0.381144 | `mysql_server_param_get` | ❌ |
| 3 | 0.303491 | `postgres_server_param_set` | ❌ |
| 4 | 0.298911 | `mysql_database_query` | ❌ |
| 5 | 0.277569 | `appservice_database_add` | ❌ |

---

## Test 164

**Expected Tool:** `azmcp_mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633592 | `mysql_table_list` | ❌ |
| 2 | 0.573844 | `postgres_table_list` | ❌ |
| 3 | 0.550898 | `postgres_database_list` | ❌ |
| 4 | 0.546963 | `mysql_database_list` | ❌ |
| 5 | 0.511847 | `kusto_table_list` | ❌ |

---

## Test 165

**Expected Tool:** `azmcp_mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609136 | `mysql_table_list` | ❌ |
| 2 | 0.526236 | `postgres_table_list` | ❌ |
| 3 | 0.525709 | `mysql_database_list` | ❌ |
| 4 | 0.507258 | `mysql_table_schema_get` | ❌ |
| 5 | 0.498050 | `postgres_database_list` | ❌ |

---

## Test 166

**Expected Tool:** `azmcp_mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630499 | `mysql_table_schema_get` | ❌ |
| 2 | 0.557941 | `postgres_table_schema_get` | ❌ |
| 3 | 0.545106 | `mysql_table_list` | ❌ |
| 4 | 0.516870 | `kusto_table_schema` | ❌ |
| 5 | 0.457910 | `mysql_database_list` | ❌ |

---

## Test 167

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815617 | `postgres_database_list` | ❌ |
| 2 | 0.644014 | `postgres_table_list` | ❌ |
| 3 | 0.622790 | `postgres_server_list` | ❌ |
| 4 | 0.542685 | `postgres_server_config_get` | ❌ |
| 5 | 0.490904 | `postgres_server_param_get` | ❌ |

---

## Test 168

**Expected Tool:** `azmcp_postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760033 | `postgres_database_list` | ❌ |
| 2 | 0.589783 | `postgres_server_list` | ❌ |
| 3 | 0.585891 | `postgres_table_list` | ❌ |
| 4 | 0.552660 | `postgres_server_config_get` | ❌ |
| 5 | 0.495629 | `postgres_server_param_get` | ❌ |

---

## Test 169

**Expected Tool:** `azmcp_postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546211 | `postgres_database_list` | ❌ |
| 2 | 0.523142 | `postgres_database_query` | ❌ |
| 3 | 0.503267 | `postgres_table_list` | ❌ |
| 4 | 0.466599 | `postgres_server_list` | ❌ |
| 5 | 0.403969 | `postgres_server_param_get` | ❌ |

---

## Test 170

**Expected Tool:** `azmcp_postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `postgres_server_config_get` | ❌ |
| 2 | 0.615366 | `postgres_server_param_set` | ❌ |
| 3 | 0.599471 | `postgres_server_param_get` | ❌ |
| 4 | 0.535049 | `postgres_database_list` | ❌ |
| 5 | 0.518574 | `postgres_server_list` | ❌ |

---

## Test 171

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.900023 | `postgres_server_list` | ❌ |
| 2 | 0.640733 | `postgres_database_list` | ❌ |
| 3 | 0.565914 | `postgres_table_list` | ❌ |
| 4 | 0.538997 | `postgres_server_config_get` | ❌ |
| 5 | 0.534239 | `kusto_cluster_list` | ❌ |

---

## Test 172

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `postgres_server_list` | ❌ |
| 2 | 0.607062 | `postgres_database_list` | ❌ |
| 3 | 0.576349 | `postgres_server_config_get` | ❌ |
| 4 | 0.522996 | `postgres_table_list` | ❌ |
| 5 | 0.506171 | `postgres_server_param_get` | ❌ |

---

## Test 173

**Expected Tool:** `azmcp_postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `postgres_server_list` | ❌ |
| 2 | 0.579232 | `postgres_database_list` | ❌ |
| 3 | 0.531804 | `postgres_server_config_get` | ❌ |
| 4 | 0.514445 | `postgres_table_list` | ❌ |
| 5 | 0.505869 | `postgres_server_param_get` | ❌ |

---

## Test 174

**Expected Tool:** `azmcp_postgres_server_param_get`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594753 | `postgres_server_param_get` | ❌ |
| 2 | 0.552478 | `postgres_server_param_set` | ❌ |
| 3 | 0.539671 | `postgres_server_config_get` | ❌ |
| 4 | 0.489693 | `postgres_server_list` | ❌ |
| 5 | 0.451871 | `postgres_database_list` | ❌ |

---

## Test 175

**Expected Tool:** `azmcp_postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579663 | `postgres_server_param_set` | ❌ |
| 2 | 0.488474 | `postgres_server_config_get` | ❌ |
| 3 | 0.469794 | `postgres_server_list` | ❌ |
| 4 | 0.447011 | `postgres_server_param_get` | ❌ |
| 5 | 0.440760 | `postgres_database_list` | ❌ |

---

## Test 176

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789883 | `postgres_table_list` | ❌ |
| 2 | 0.750580 | `postgres_database_list` | ❌ |
| 3 | 0.574930 | `postgres_server_list` | ❌ |
| 4 | 0.519908 | `postgres_table_schema_get` | ❌ |
| 5 | 0.501400 | `postgres_server_config_get` | ❌ |

---

## Test 177

**Expected Tool:** `azmcp_postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736083 | `postgres_table_list` | ❌ |
| 2 | 0.690112 | `postgres_database_list` | ❌ |
| 3 | 0.558435 | `postgres_table_schema_get` | ❌ |
| 4 | 0.543331 | `postgres_server_list` | ❌ |
| 5 | 0.521570 | `postgres_server_config_get` | ❌ |

---

## Test 178

**Expected Tool:** `azmcp_postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714937 | `postgres_table_schema_get` | ❌ |
| 2 | 0.597846 | `postgres_table_list` | ❌ |
| 3 | 0.574230 | `postgres_database_list` | ❌ |
| 4 | 0.508082 | `postgres_server_config_get` | ❌ |
| 5 | 0.502593 | `kusto_table_schema` | ❌ |

---

## Test 179

**Expected Tool:** `azmcp_deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711770 | `deploy_app_logs_get` | ❌ |
| 2 | 0.471731 | `deploy_plan_get` | ❌ |
| 3 | 0.451638 | `monitor_activitylog_list` | ❌ |
| 4 | 0.404890 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.401388 | `monitor_resource_log_query` | ❌ |

---

## Test 180

**Expected Tool:** `azmcp_deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680640 | `deploy_architecture_diagram_generate` | ❌ |
| 2 | 0.562529 | `deploy_plan_get` | ❌ |
| 3 | 0.497193 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.489344 | `cloudarchitect_design` | ❌ |
| 5 | 0.435921 | `deploy_iac_rules_get` | ❌ |

---

## Test 181

**Expected Tool:** `azmcp_deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529092 | `deploy_iac_rules_get` | ❌ |
| 2 | 0.479903 | `bicepschema_get` | ❌ |
| 3 | 0.391965 | `get_bestpractices_get` | ❌ |
| 4 | 0.383192 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.375634 | `extension_cli_generate` | ❌ |

---

## Test 182

**Expected Tool:** `azmcp_deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638841 | `deploy_pipeline_guidance_get` | ❌ |
| 2 | 0.499255 | `deploy_plan_get` | ❌ |
| 3 | 0.448918 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.385920 | `deploy_app_logs_get` | ❌ |
| 5 | 0.382240 | `get_bestpractices_get` | ❌ |

---

## Test 183

**Expected Tool:** `azmcp_deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688042 | `deploy_plan_get` | ❌ |
| 2 | 0.587903 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.448692 | `loadtesting_test_create` | ❌ |

---

## Test 184

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755365 | `eventgrid_events_publish` | ❌ |
| 2 | 0.483021 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.466031 | `eventgrid_topic_list` | ❌ |
| 4 | 0.360676 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.355599 | `servicebus_topic_details` | ❌ |

---

## Test 185

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654735 | `eventgrid_events_publish` | ❌ |
| 2 | 0.524454 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.509674 | `eventgrid_topic_list` | ❌ |
| 4 | 0.373649 | `servicebus_topic_details` | ❌ |
| 5 | 0.359988 | `eventhubs_eventhub_update` | ❌ |

---

## Test 186

**Expected Tool:** `azmcp_eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600274 | `eventgrid_events_publish` | ❌ |
| 2 | 0.521240 | `eventgrid_topic_list` | ❌ |
| 3 | 0.504808 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.411130 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.389439 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 187

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770140 | `eventgrid_topic_list` | ❌ |
| 2 | 0.745470 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.561862 | `kusto_cluster_list` | ❌ |
| 4 | 0.545540 | `search_service_list` | ❌ |
| 5 | 0.526138 | `subscription_list` | ❌ |

---

## Test 188

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738258 | `eventgrid_topic_list` | ❌ |
| 2 | 0.737486 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.492592 | `kusto_cluster_list` | ❌ |
| 4 | 0.480287 | `subscription_list` | ❌ |
| 5 | 0.475119 | `search_service_list` | ❌ |

---

## Test 189

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.770140 | `eventgrid_topic_list` | ❌ |
| 2 | 0.721362 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.535326 | `kusto_cluster_list` | ❌ |
| 4 | 0.514248 | `search_service_list` | ❌ |
| 5 | 0.495987 | `subscription_list` | ❌ |

---

## Test 190

**Expected Tool:** `azmcp_eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757982 | `eventgrid_topic_list` | ❌ |
| 2 | 0.703350 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.608523 | `group_list` | ❌ |
| 4 | 0.545158 | `monitor_webtests_list` | ❌ |
| 5 | 0.524305 | `eventhubs_namespace_get` | ❌ |

---

## Test 191

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.769097 | `eventgrid_subscription_list` | ❌ |
| 2 | 0.720606 | `eventgrid_topic_list` | ❌ |
| 3 | 0.498615 | `servicebus_topic_details` | ❌ |
| 4 | 0.486216 | `servicebus_topic_subscription_details` | ❌ |
| 5 | 0.486162 | `eventgrid_events_publish` | ❌ |

---

## Test 192

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.718109 | `eventgrid_subscription_list` | ❌ |
| 2 | 0.709805 | `eventgrid_topic_list` | ❌ |
| 3 | 0.539977 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.529286 | `servicebus_topic_details` | ❌ |
| 5 | 0.477876 | `eventgrid_events_publish` | ❌ |

---

## Test 193

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.746829 | `eventgrid_subscription_list` | ❌ |
| 2 | 0.746343 | `eventgrid_topic_list` | ❌ |
| 3 | 0.535859 | `monitor_webtests_list` | ❌ |
| 4 | 0.525029 | `group_list` | ❌ |
| 5 | 0.503528 | `servicebus_topic_details` | ❌ |

---

## Test 194

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736436 | `eventgrid_subscription_list` | ❌ |
| 2 | 0.659727 | `eventgrid_topic_list` | ❌ |
| 3 | 0.569254 | `subscription_list` | ❌ |
| 4 | 0.537922 | `kusto_cluster_list` | ❌ |
| 5 | 0.518857 | `search_service_list` | ❌ |

---

## Test 195

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684543 | `eventgrid_subscription_list` | ❌ |
| 2 | 0.656277 | `eventgrid_topic_list` | ❌ |
| 3 | 0.542388 | `subscription_list` | ❌ |
| 4 | 0.521053 | `kusto_cluster_list` | ❌ |
| 5 | 0.510115 | `group_list` | ❌ |

---

## Test 196

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696101 | `eventgrid_subscription_list` | ❌ |
| 2 | 0.691739 | `eventgrid_topic_list` | ❌ |
| 3 | 0.557573 | `group_list` | ❌ |
| 4 | 0.510684 | `monitor_webtests_list` | ❌ |
| 5 | 0.504984 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 197

**Expected Tool:** `azmcp_eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.709801 | `eventgrid_subscription_list` | ❌ |
| 2 | 0.642095 | `eventgrid_topic_list` | ❌ |
| 3 | 0.506697 | `subscription_list` | ❌ |
| 4 | 0.476763 | `search_service_list` | ❌ |
| 5 | 0.475782 | `kusto_cluster_list` | ❌ |

---

## Test 198

**Expected Tool:** `azmcp_eventhubs_consumergroup_delete`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.766977 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 2 | 0.675895 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.641209 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.631791 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.605486 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 199

**Expected Tool:** `azmcp_eventhubs_consumergroup_get`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738475 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 2 | 0.634517 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.626485 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.606619 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.593098 | `eventhubs_eventhub_get` | ❌ |

---

## Test 200

**Expected Tool:** `azmcp_eventhubs_consumergroup_get`  
**Prompt:** Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712861 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 2 | 0.637170 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.625913 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.576800 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.529940 | `eventhubs_eventhub_get` | ❌ |

---

## Test 201

**Expected Tool:** `azmcp_eventhubs_consumergroup_update`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.757614 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 2 | 0.688923 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.670026 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.554314 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.545003 | `eventhubs_namespace_get` | ❌ |

---

## Test 202

**Expected Tool:** `azmcp_eventhubs_consumergroup_update`  
**Prompt:** Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738818 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 2 | 0.655614 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 3 | 0.642219 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.552234 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.523158 | `eventhubs_namespace_get` | ❌ |

---

## Test 203

**Expected Tool:** `azmcp_eventhubs_eventhub_delete`  
**Prompt:** Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.697932 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.688526 | `eventhubs_eventhub_delete` | ❌ |
| 3 | 0.627643 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.578630 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.552939 | `eventhubs_eventhub_get` | ❌ |

---

## Test 204

**Expected Tool:** `azmcp_eventhubs_eventhub_get`  
**Prompt:** List all Event Hubs in my namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.773242 | `eventhubs_eventhub_get` | ❌ |
| 2 | 0.687582 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.578689 | `eventhubs_eventhub_update` | ❌ |
| 4 | 0.560272 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.545475 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 205

**Expected Tool:** `azmcp_eventhubs_eventhub_get`  
**Prompt:** Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638083 | `eventhubs_namespace_get` | ❌ |
| 2 | 0.627638 | `eventhubs_eventhub_get` | ❌ |
| 3 | 0.570904 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.527646 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.521113 | `eventhubs_namespace_delete` | ❌ |

---

## Test 206

**Expected Tool:** `azmcp_eventhubs_eventhub_update`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646042 | `eventhubs_eventhub_update` | ❌ |
| 2 | 0.606240 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.574245 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.572359 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.557753 | `eventhubs_namespace_delete` | ❌ |

---

## Test 207

**Expected Tool:** `azmcp_eventhubs_eventhub_update`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.655283 | `eventhubs_eventhub_update` | ❌ |
| 2 | 0.571661 | `eventhubs_eventhub_delete` | ❌ |
| 3 | 0.568606 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 4 | 0.568396 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.564776 | `eventhubs_namespace_delete` | ❌ |

---

## Test 208

**Expected Tool:** `azmcp_eventhubs_namespace_delete`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626040 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.526122 | `eventhubs_namespace_update` | ❌ |
| 3 | 0.505082 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.449841 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.435037 | `workbooks_delete` | ❌ |

---

## Test 209

**Expected Tool:** `azmcp_eventhubs_namespace_get`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659838 | `eventhubs_eventhub_get` | ❌ |
| 2 | 0.658827 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.607372 | `kusto_cluster_list` | ❌ |
| 4 | 0.557200 | `eventgrid_topic_list` | ❌ |
| 5 | 0.556126 | `eventgrid_subscription_list` | ❌ |

---

## Test 210

**Expected Tool:** `azmcp_eventhubs_namespace_get`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509749 | `eventhubs_namespace_get` | ❌ |
| 2 | 0.509081 | `monitor_webtests_get` | ❌ |
| 3 | 0.497399 | `servicebus_queue_details` | ❌ |
| 4 | 0.490472 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.470455 | `functionapp_get` | ❌ |

---

## Test 211

**Expected Tool:** `azmcp_eventhubs_namespace_update`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610733 | `eventhubs_namespace_update` | ❌ |
| 2 | 0.466721 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.461133 | `eventhubs_namespace_delete` | ❌ |
| 4 | 0.449589 | `workbooks_create` | ❌ |
| 5 | 0.438492 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

## Test 212

**Expected Tool:** `azmcp_eventhubs_namespace_update`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622458 | `eventhubs_namespace_update` | ❌ |
| 2 | 0.476260 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.448723 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.436549 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.372632 | `sql_db_rename` | ❌ |

---

## Test 213

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `functionapp_get` | ❌ |
| 2 | 0.451613 | `deploy_app_logs_get` | ❌ |
| 3 | 0.450457 | `applens_resource_diagnose` | ❌ |
| 4 | 0.390048 | `mysql_server_list` | ❌ |
| 5 | 0.380314 | `get_bestpractices_get` | ❌ |

---

## Test 214

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607276 | `functionapp_get` | ❌ |
| 2 | 0.447400 | `mysql_server_config_get` | ❌ |
| 3 | 0.424693 | `appconfig_account_list` | ❌ |
| 4 | 0.411267 | `appconfig_kv_get` | ❌ |
| 5 | 0.400402 | `deploy_app_logs_get` | ❌ |

---

## Test 215

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `functionapp_get` | ❌ |
| 2 | 0.411549 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.390708 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383533 | `deploy_app_logs_get` | ❌ |
| 5 | 0.360665 | `storage_account_get` | ❌ |

---

## Test 216

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690933 | `functionapp_get` | ❌ |
| 2 | 0.441937 | `foundry_resource_get` | ❌ |
| 3 | 0.432317 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.431821 | `applens_resource_diagnose` | ❌ |
| 5 | 0.429077 | `storage_account_get` | ❌ |

---

## Test 217

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `functionapp_get` | ❌ |
| 2 | 0.417494 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.409712 | `deploy_app_logs_get` | ❌ |
| 4 | 0.399953 | `storage_account_get` | ❌ |
| 5 | 0.392237 | `applens_resource_diagnose` | ❌ |

---

## Test 218

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `functionapp_get` | ❌ |
| 2 | 0.449588 | `deploy_app_logs_get` | ❌ |
| 3 | 0.428689 | `applens_resource_diagnose` | ❌ |
| 4 | 0.424686 | `foundry_resource_get` | ❌ |
| 5 | 0.391502 | `monitor_webtests_get` | ❌ |

---

## Test 219

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `functionapp_get` | ❌ |
| 2 | 0.430189 | `deploy_app_logs_get` | ❌ |
| 3 | 0.421082 | `storage_account_get` | ❌ |
| 4 | 0.403311 | `signalr_runtime_get` | ❌ |
| 5 | 0.391615 | `foundry_resource_get` | ❌ |

---

## Test 220

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554980 | `functionapp_get` | ❌ |
| 2 | 0.426703 | `quota_usage_check` | ❌ |
| 3 | 0.424610 | `deploy_app_logs_get` | ❌ |
| 4 | 0.407941 | `deploy_plan_get` | ❌ |
| 5 | 0.381629 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 221

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565797 | `functionapp_get` | ❌ |
| 2 | 0.403665 | `deploy_app_logs_get` | ❌ |
| 3 | 0.384159 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369868 | `applens_resource_diagnose` | ❌ |
| 5 | 0.352807 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 222

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `functionapp_get` | ❌ |
| 2 | 0.559382 | `search_service_list` | ❌ |
| 3 | 0.534930 | `subscription_list` | ❌ |
| 4 | 0.529031 | `kusto_cluster_list` | ❌ |
| 5 | 0.516618 | `cosmos_account_list` | ❌ |

---

## Test 223

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `functionapp_get` | ❌ |
| 2 | 0.464985 | `deploy_app_logs_get` | ❌ |
| 3 | 0.436167 | `foundry_agents_list` | ❌ |
| 4 | 0.412646 | `search_service_list` | ❌ |
| 5 | 0.411323 | `get_bestpractices_get` | ❌ |

---

## Test 224

**Expected Tool:** `azmcp_functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433674 | `functionapp_get` | ❌ |
| 2 | 0.346619 | `deploy_app_logs_get` | ❌ |
| 3 | 0.337966 | `applens_resource_diagnose` | ❌ |
| 4 | 0.316594 | `extension_cli_install` | ❌ |
| 5 | 0.284362 | `get_bestpractices_get` | ❌ |

---

## Test 225

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604780 | `keyvault_admin_settings_get` | ❌ |
| 2 | 0.532196 | `storage_account_get` | ❌ |
| 3 | 0.496629 | `keyvault_key_get` | ❌ |
| 4 | 0.452366 | `appconfig_kv_set` | ❌ |
| 5 | 0.448039 | `keyvault_secret_get` | ❌ |

---

## Test 226

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671370 | `keyvault_admin_settings_get` | ❌ |
| 2 | 0.455561 | `storage_account_get` | ❌ |
| 3 | 0.441225 | `keyvault_key_get` | ❌ |
| 4 | 0.404666 | `appconfig_kv_set` | ❌ |
| 5 | 0.395274 | `keyvault_secret_get` | ❌ |

---

## Test 227

**Expected Tool:** `azmcp_keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.505747 | `keyvault_admin_settings_get` | ❌ |
| 2 | 0.496496 | `appconfig_kv_set` | ❌ |
| 3 | 0.420107 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.419054 | `keyvault_key_get` | ❌ |
| 5 | 0.410221 | `keyvault_secret_get` | ❌ |

---

## Test 228

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.627727 | `keyvault_certificate_create` | ❌ |
| 2 | 0.570318 | `keyvault_certificate_import` | ❌ |
| 3 | 0.540199 | `keyvault_key_create` | ❌ |
| 4 | 0.519218 | `keyvault_certificate_get` | ❌ |
| 5 | 0.500027 | `keyvault_certificate_list` | ❌ |

---

## Test 229

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600066 | `keyvault_certificate_create` | ❌ |
| 2 | 0.561496 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522791 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502066 | `keyvault_key_create` | ❌ |
| 5 | 0.497200 | `keyvault_certificate_list` | ❌ |

---

## Test 230

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573998 | `keyvault_certificate_create` | ❌ |
| 2 | 0.527759 | `keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `keyvault_certificate_get` | ❌ |
| 4 | 0.481548 | `keyvault_key_create` | ❌ |
| 5 | 0.469601 | `keyvault_certificate_list` | ❌ |

---

## Test 231

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591697 | `keyvault_certificate_create` | ❌ |
| 2 | 0.562265 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522147 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502529 | `keyvault_key_create` | ❌ |
| 5 | 0.479992 | `keyvault_certificate_list` | ❌ |

---

## Test 232

**Expected Tool:** `azmcp_keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622788 | `keyvault_certificate_create` | ❌ |
| 2 | 0.558532 | `keyvault_certificate_import` | ❌ |
| 3 | 0.534503 | `keyvault_certificate_get` | ❌ |
| 4 | 0.521316 | `keyvault_certificate_list` | ❌ |
| 5 | 0.465056 | `keyvault_key_create` | ❌ |

---

## Test 233

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600625 | `keyvault_certificate_get` | ❌ |
| 2 | 0.528405 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519037 | `keyvault_certificate_import` | ❌ |
| 4 | 0.499293 | `keyvault_certificate_create` | ❌ |
| 5 | 0.486609 | `keyvault_key_get` | ❌ |

---

## Test 234

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646098 | `keyvault_certificate_get` | ❌ |
| 2 | 0.562988 | `keyvault_key_get` | ❌ |
| 3 | 0.514170 | `keyvault_secret_get` | ❌ |
| 4 | 0.509446 | `keyvault_certificate_list` | ❌ |
| 5 | 0.507737 | `keyvault_certificate_import` | ❌ |

---

## Test 235

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609523 | `keyvault_certificate_get` | ❌ |
| 2 | 0.515570 | `keyvault_certificate_list` | ❌ |
| 3 | 0.511197 | `keyvault_certificate_create` | ❌ |
| 4 | 0.507768 | `keyvault_certificate_import` | ❌ |
| 5 | 0.474394 | `keyvault_key_get` | ❌ |

---

## Test 236

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646842 | `keyvault_certificate_get` | ❌ |
| 2 | 0.526787 | `keyvault_key_get` | ❌ |
| 3 | 0.520947 | `keyvault_certificate_list` | ❌ |
| 4 | 0.509049 | `keyvault_certificate_import` | ❌ |
| 5 | 0.501258 | `keyvault_secret_get` | ❌ |

---

## Test 237

**Expected Tool:** `azmcp_keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595959 | `keyvault_certificate_get` | ❌ |
| 2 | 0.527404 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519059 | `keyvault_certificate_import` | ❌ |
| 4 | 0.501138 | `keyvault_certificate_create` | ❌ |
| 5 | 0.465174 | `keyvault_key_get` | ❌ |

---

## Test 238

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.585675 | `keyvault_certificate_import` | ❌ |
| 2 | 0.420872 | `keyvault_certificate_get` | ❌ |
| 3 | 0.402640 | `keyvault_certificate_create` | ❌ |
| 4 | 0.399681 | `keyvault_certificate_list` | ❌ |
| 5 | 0.353008 | `keyvault_key_create` | ❌ |

---

## Test 239

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622145 | `keyvault_certificate_import` | ❌ |
| 2 | 0.504353 | `keyvault_certificate_get` | ❌ |
| 3 | 0.498841 | `keyvault_certificate_create` | ❌ |
| 4 | 0.448103 | `keyvault_certificate_list` | ❌ |
| 5 | 0.419777 | `keyvault_key_create` | ❌ |

---

## Test 240

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595707 | `keyvault_certificate_import` | ❌ |
| 2 | 0.453929 | `keyvault_certificate_create` | ❌ |
| 3 | 0.452551 | `keyvault_certificate_get` | ❌ |
| 4 | 0.418203 | `keyvault_certificate_list` | ❌ |
| 5 | 0.413377 | `keyvault_key_create` | ❌ |

---

## Test 241

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619480 | `keyvault_certificate_import` | ❌ |
| 2 | 0.517804 | `keyvault_certificate_get` | ❌ |
| 3 | 0.480815 | `keyvault_certificate_create` | ❌ |
| 4 | 0.444386 | `keyvault_certificate_list` | ❌ |
| 5 | 0.381873 | `keyvault_key_create` | ❌ |

---

## Test 242

**Expected Tool:** `azmcp_keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595417 | `keyvault_certificate_import` | ❌ |
| 2 | 0.452489 | `keyvault_certificate_create` | ❌ |
| 3 | 0.441616 | `keyvault_certificate_get` | ❌ |
| 4 | 0.408018 | `keyvault_key_create` | ❌ |
| 5 | 0.392244 | `keyvault_secret_create` | ❌ |

---

## Test 243

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726124 | `keyvault_certificate_list` | ❌ |
| 2 | 0.583110 | `keyvault_key_list` | ❌ |
| 3 | 0.531988 | `keyvault_secret_list` | ❌ |
| 4 | 0.515236 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485792 | `keyvault_certificate_create` | ❌ |

---

## Test 244

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615541 | `keyvault_certificate_list` | ❌ |
| 2 | 0.522453 | `keyvault_certificate_get` | ❌ |
| 3 | 0.475156 | `keyvault_key_list` | ❌ |
| 4 | 0.460973 | `keyvault_certificate_create` | ❌ |
| 5 | 0.448139 | `keyvault_key_get` | ❌ |

---

## Test 245

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624711 | `keyvault_certificate_list` | ❌ |
| 2 | 0.519739 | `keyvault_certificate_get` | ❌ |
| 3 | 0.510048 | `keyvault_certificate_create` | ❌ |
| 4 | 0.505534 | `keyvault_certificate_import` | ❌ |
| 5 | 0.497356 | `keyvault_key_list` | ❌ |

---

## Test 246

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672622 | `keyvault_certificate_list` | ❌ |
| 2 | 0.553990 | `keyvault_key_list` | ❌ |
| 3 | 0.511905 | `keyvault_secret_list` | ❌ |
| 4 | 0.507062 | `keyvault_certificate_get` | ❌ |
| 5 | 0.492357 | `keyvault_certificate_create` | ❌ |

---

## Test 247

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.747407 | `keyvault_certificate_list` | ❌ |
| 2 | 0.594216 | `keyvault_key_list` | ❌ |
| 3 | 0.558771 | `keyvault_secret_list` | ❌ |
| 4 | 0.515568 | `keyvault_certificate_get` | ❌ |
| 5 | 0.490876 | `keyvault_certificate_create` | ❌ |

---

## Test 248

**Expected Tool:** `azmcp_keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639711 | `keyvault_certificate_list` | ❌ |
| 2 | 0.512475 | `keyvault_certificate_get` | ❌ |
| 3 | 0.507572 | `keyvault_key_list` | ❌ |
| 4 | 0.482583 | `keyvault_certificate_create` | ❌ |
| 5 | 0.464725 | `keyvault_secret_list` | ❌ |

---

## Test 249

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.661466 | `keyvault_key_create` | ❌ |
| 2 | 0.456580 | `keyvault_secret_create` | ❌ |
| 3 | 0.451790 | `keyvault_certificate_create` | ❌ |
| 4 | 0.429614 | `keyvault_certificate_import` | ❌ |
| 5 | 0.399326 | `keyvault_key_get` | ❌ |

---

## Test 250

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641070 | `keyvault_key_create` | ❌ |
| 2 | 0.428502 | `keyvault_key_get` | ❌ |
| 3 | 0.422763 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420045 | `keyvault_secret_create` | ❌ |
| 5 | 0.405644 | `appconfig_kv_set` | ❌ |

---

## Test 251

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547493 | `keyvault_key_create` | ❌ |
| 2 | 0.463557 | `keyvault_secret_create` | ❌ |
| 3 | 0.447410 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420366 | `keyvault_key_get` | ❌ |
| 5 | 0.404350 | `keyvault_certificate_import` | ❌ |

---

## Test 252

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641369 | `keyvault_key_create` | ❌ |
| 2 | 0.501636 | `keyvault_secret_create` | ❌ |
| 3 | 0.491735 | `keyvault_certificate_create` | ❌ |
| 4 | 0.464557 | `keyvault_certificate_import` | ❌ |
| 5 | 0.451016 | `keyvault_key_get` | ❌ |

---

## Test 253

**Expected Tool:** `azmcp_keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571718 | `keyvault_key_create` | ❌ |
| 2 | 0.443369 | `keyvault_certificate_create` | ❌ |
| 3 | 0.434675 | `keyvault_secret_create` | ❌ |
| 4 | 0.421721 | `keyvault_key_get` | ❌ |
| 5 | 0.400533 | `keyvault_certificate_import` | ❌ |

---

## Test 254

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549488 | `keyvault_key_get` | ❌ |
| 2 | 0.468165 | `keyvault_secret_get` | ❌ |
| 3 | 0.452816 | `keyvault_key_create` | ❌ |
| 4 | 0.439969 | `keyvault_key_list` | ❌ |
| 5 | 0.426545 | `keyvault_certificate_get` | ❌ |

---

## Test 255

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629552 | `keyvault_key_get` | ❌ |
| 2 | 0.532651 | `keyvault_secret_get` | ❌ |
| 3 | 0.512278 | `storage_account_get` | ❌ |
| 4 | 0.495957 | `keyvault_certificate_get` | ❌ |
| 5 | 0.456992 | `keyvault_key_create` | ❌ |

---

## Test 256

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.484645 | `keyvault_key_get` | ❌ |
| 2 | 0.443182 | `keyvault_key_create` | ❌ |
| 3 | 0.409388 | `keyvault_secret_get` | ❌ |
| 4 | 0.395491 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.383519 | `appconfig_kv_lock_set` | ❌ |

---

## Test 257

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590303 | `keyvault_key_get` | ❌ |
| 2 | 0.488213 | `keyvault_secret_get` | ❌ |
| 3 | 0.476498 | `storage_account_get` | ❌ |
| 4 | 0.460796 | `keyvault_certificate_get` | ❌ |
| 5 | 0.436511 | `keyvault_admin_settings_get` | ❌ |

---

## Test 258

**Expected Tool:** `azmcp_keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.518886 | `keyvault_key_get` | ❌ |
| 2 | 0.432950 | `storage_account_get` | ❌ |
| 3 | 0.432742 | `keyvault_admin_settings_get` | ❌ |
| 4 | 0.429131 | `keyvault_key_create` | ❌ |
| 5 | 0.422536 | `keyvault_secret_get` | ❌ |

---

## Test 259

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701448 | `keyvault_key_list` | ❌ |
| 2 | 0.601513 | `keyvault_certificate_list` | ❌ |
| 3 | 0.587427 | `keyvault_secret_list` | ❌ |
| 4 | 0.498767 | `cosmos_account_list` | ❌ |
| 5 | 0.480129 | `keyvault_admin_settings_get` | ❌ |

---

## Test 260

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549453 | `keyvault_key_list` | ❌ |
| 2 | 0.506815 | `keyvault_key_get` | ❌ |
| 3 | 0.475507 | `keyvault_certificate_list` | ❌ |
| 4 | 0.472465 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.455683 | `keyvault_secret_get` | ❌ |

---

## Test 261

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581970 | `keyvault_key_list` | ❌ |
| 2 | 0.502245 | `keyvault_admin_settings_get` | ❌ |
| 3 | 0.501481 | `keyvault_certificate_list` | ❌ |
| 4 | 0.476470 | `keyvault_key_get` | ❌ |
| 5 | 0.472414 | `keyvault_secret_list` | ❌ |

---

## Test 262

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.641314 | `keyvault_key_list` | ❌ |
| 2 | 0.559550 | `keyvault_certificate_list` | ❌ |
| 3 | 0.553553 | `keyvault_secret_list` | ❌ |
| 4 | 0.486377 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475992 | `cosmos_account_list` | ❌ |

---

## Test 263

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.723266 | `keyvault_key_list` | ❌ |
| 2 | 0.611366 | `keyvault_certificate_list` | ❌ |
| 3 | 0.611185 | `keyvault_secret_list` | ❌ |
| 4 | 0.473886 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.441881 | `keyvault_key_get` | ❌ |

---

## Test 264

**Expected Tool:** `azmcp_keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570444 | `keyvault_key_list` | ❌ |
| 2 | 0.501073 | `keyvault_key_get` | ❌ |
| 3 | 0.500103 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496817 | `storage_account_get` | ❌ |
| 5 | 0.490367 | `keyvault_secret_list` | ❌ |

---

## Test 265

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678482 | `keyvault_secret_create` | ❌ |
| 2 | 0.553018 | `keyvault_key_create` | ❌ |
| 3 | 0.512856 | `keyvault_secret_get` | ❌ |
| 4 | 0.475097 | `keyvault_certificate_create` | ❌ |
| 5 | 0.461437 | `appconfig_kv_set` | ❌ |

---

## Test 266

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663097 | `keyvault_secret_create` | ❌ |
| 2 | 0.519703 | `keyvault_secret_get` | ❌ |
| 3 | 0.512193 | `appconfig_kv_set` | ❌ |
| 4 | 0.458479 | `keyvault_key_create` | ❌ |
| 5 | 0.429715 | `appconfig_kv_lock_set` | ❌ |

---

## Test 267

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639897 | `keyvault_secret_create` | ❌ |
| 2 | 0.509674 | `keyvault_secret_get` | ❌ |
| 3 | 0.485203 | `appconfig_kv_set` | ❌ |
| 4 | 0.484680 | `keyvault_key_create` | ❌ |
| 5 | 0.448995 | `appconfig_kv_lock_set` | ❌ |

---

## Test 268

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675145 | `keyvault_secret_create` | ❌ |
| 2 | 0.499612 | `keyvault_secret_get` | ❌ |
| 3 | 0.498228 | `keyvault_key_create` | ❌ |
| 4 | 0.479174 | `keyvault_certificate_import` | ❌ |
| 5 | 0.458574 | `appconfig_kv_set` | ❌ |

---

## Test 269

**Expected Tool:** `azmcp_keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571380 | `keyvault_secret_create` | ❌ |
| 2 | 0.513529 | `keyvault_secret_get` | ❌ |
| 3 | 0.440984 | `appconfig_kv_set` | ❌ |
| 4 | 0.417785 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.408022 | `keyvault_key_get` | ❌ |

---

## Test 270

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.602769 | `keyvault_secret_get` | ❌ |
| 2 | 0.504212 | `keyvault_key_get` | ❌ |
| 3 | 0.501397 | `keyvault_secret_create` | ❌ |
| 4 | 0.478769 | `keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `keyvault_certificate_get` | ❌ |

---

## Test 271

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653871 | `keyvault_secret_get` | ❌ |
| 2 | 0.566786 | `keyvault_key_get` | ❌ |
| 3 | 0.517547 | `storage_account_get` | ❌ |
| 4 | 0.496050 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485249 | `keyvault_secret_list` | ❌ |

---

## Test 272

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578479 | `keyvault_secret_get` | ❌ |
| 2 | 0.492213 | `keyvault_key_get` | ❌ |
| 3 | 0.488705 | `keyvault_secret_create` | ❌ |
| 4 | 0.443676 | `keyvault_secret_list` | ❌ |
| 5 | 0.424167 | `keyvault_admin_settings_get` | ❌ |

---

## Test 273

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649267 | `keyvault_secret_get` | ❌ |
| 2 | 0.546992 | `keyvault_key_get` | ❌ |
| 3 | 0.497402 | `storage_account_get` | ❌ |
| 4 | 0.492583 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491596 | `keyvault_secret_list` | ❌ |

---

## Test 274

**Expected Tool:** `azmcp_keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573386 | `keyvault_secret_get` | ❌ |
| 2 | 0.471233 | `keyvault_key_get` | ❌ |
| 3 | 0.467105 | `keyvault_secret_create` | ❌ |
| 4 | 0.443812 | `keyvault_secret_list` | ❌ |
| 5 | 0.435976 | `storage_account_get` | ❌ |

---

## Test 275

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701227 | `keyvault_secret_list` | ❌ |
| 2 | 0.563736 | `keyvault_key_list` | ❌ |
| 3 | 0.538337 | `keyvault_certificate_list` | ❌ |
| 4 | 0.499642 | `keyvault_secret_get` | ❌ |
| 5 | 0.455500 | `cosmos_account_list` | ❌ |

---

## Test 276

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555681 | `keyvault_secret_list` | ❌ |
| 2 | 0.543861 | `keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `keyvault_key_get` | ❌ |
| 4 | 0.464661 | `keyvault_key_list` | ❌ |
| 5 | 0.453130 | `keyvault_admin_settings_get` | ❌ |

---

## Test 277

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572540 | `keyvault_secret_list` | ❌ |
| 2 | 0.529258 | `keyvault_secret_get` | ❌ |
| 3 | 0.493761 | `keyvault_key_list` | ❌ |
| 4 | 0.487620 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475273 | `keyvault_key_get` | ❌ |

---

## Test 278

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624290 | `keyvault_secret_list` | ❌ |
| 2 | 0.559681 | `keyvault_key_list` | ❌ |
| 3 | 0.517516 | `keyvault_certificate_list` | ❌ |
| 4 | 0.479547 | `keyvault_secret_get` | ❌ |
| 5 | 0.454288 | `storage_blob_container_get` | ❌ |

---

## Test 279

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742358 | `keyvault_secret_list` | ❌ |
| 2 | 0.601183 | `keyvault_key_list` | ❌ |
| 3 | 0.567827 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496127 | `keyvault_secret_get` | ❌ |
| 5 | 0.437560 | `keyvault_admin_settings_get` | ❌ |

---

## Test 280

**Expected Tool:** `azmcp_keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567110 | `keyvault_secret_list` | ❌ |
| 2 | 0.522398 | `keyvault_secret_get` | ❌ |
| 3 | 0.476309 | `keyvault_key_list` | ❌ |
| 4 | 0.462676 | `keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `keyvault_key_get` | ❌ |

---

## Test 281

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588349 | `aks_cluster_get` | ❌ |
| 2 | 0.544465 | `aks_nodepool_get` | ❌ |
| 3 | 0.517279 | `kusto_cluster_get` | ❌ |
| 4 | 0.481416 | `mysql_server_config_get` | ❌ |
| 5 | 0.430975 | `postgres_server_config_get` | ❌ |

---

## Test 282

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621748 | `aks_cluster_get` | ❌ |
| 2 | 0.575682 | `aks_nodepool_get` | ❌ |
| 3 | 0.567870 | `kusto_cluster_get` | ❌ |
| 4 | 0.461466 | `sql_db_show` | ❌ |
| 5 | 0.443820 | `monitor_webtests_get` | ❌ |

---

## Test 283

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522555 | `aks_cluster_get` | ❌ |
| 2 | 0.483337 | `aks_nodepool_get` | ❌ |
| 3 | 0.434684 | `kusto_cluster_get` | ❌ |
| 4 | 0.380301 | `mysql_server_config_get` | ❌ |
| 5 | 0.366689 | `kusto_cluster_list` | ❌ |

---

## Test 284

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588635 | `aks_cluster_get` | ❌ |
| 2 | 0.550650 | `aks_nodepool_get` | ❌ |
| 3 | 0.527511 | `kusto_cluster_get` | ❌ |
| 4 | 0.445722 | `storage_account_get` | ❌ |
| 5 | 0.435597 | `foundry_resource_get` | ❌ |

---

## Test 285

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756523 | `aks_cluster_get` | ❌ |
| 2 | 0.749416 | `kusto_cluster_list` | ❌ |
| 3 | 0.590256 | `aks_nodepool_get` | ❌ |
| 4 | 0.568403 | `kusto_database_list` | ❌ |
| 5 | 0.562043 | `search_service_list` | ❌ |

---

## Test 286

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612132 | `aks_cluster_get` | ❌ |
| 2 | 0.586661 | `kusto_cluster_list` | ❌ |
| 3 | 0.507801 | `aks_nodepool_get` | ❌ |
| 4 | 0.489724 | `kusto_cluster_get` | ❌ |
| 5 | 0.462874 | `kusto_database_list` | ❌ |

---

## Test 287

**Expected Tool:** `azmcp_aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.628472 | `aks_cluster_get` | ❌ |
| 2 | 0.563358 | `aks_nodepool_get` | ❌ |
| 3 | 0.526756 | `kusto_cluster_list` | ❌ |
| 4 | 0.426157 | `kusto_cluster_get` | ❌ |
| 5 | 0.409103 | `kusto_database_list` | ❌ |

---

## Test 288

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728889 | `aks_nodepool_get` | ❌ |
| 2 | 0.517021 | `kusto_cluster_get` | ❌ |
| 3 | 0.509825 | `aks_cluster_get` | ❌ |
| 4 | 0.468392 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463192 | `sql_elastic-pool_list` | ❌ |

---

## Test 289

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654168 | `aks_nodepool_get` | ❌ |
| 2 | 0.458596 | `sql_elastic-pool_list` | ❌ |
| 3 | 0.446023 | `aks_cluster_get` | ❌ |
| 4 | 0.440182 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.413758 | `kusto_cluster_get` | ❌ |

---

## Test 290

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592984 | `aks_nodepool_get` | ❌ |
| 2 | 0.402578 | `aks_cluster_get` | ❌ |
| 3 | 0.385148 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.383060 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.355192 | `kusto_cluster_get` | ❌ |

---

## Test 291

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692332 | `aks_nodepool_get` | ❌ |
| 2 | 0.519045 | `aks_cluster_get` | ❌ |
| 3 | 0.506624 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500749 | `kusto_cluster_list` | ❌ |
| 5 | 0.487707 | `sql_elastic-pool_list` | ❌ |

---

## Test 292

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732178 | `aks_nodepool_get` | ❌ |
| 2 | 0.561833 | `aks_cluster_get` | ❌ |
| 3 | 0.510269 | `sql_elastic-pool_list` | ❌ |
| 4 | 0.509732 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.486700 | `kusto_cluster_list` | ❌ |

---

## Test 293

**Expected Tool:** `azmcp_aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629476 | `aks_nodepool_get` | ❌ |
| 2 | 0.456916 | `aks_cluster_get` | ❌ |
| 3 | 0.443902 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.433006 | `kusto_cluster_list` | ❌ |
| 5 | 0.425448 | `sql_elastic-pool_list` | ❌ |

---

## Test 294

**Expected Tool:** `azmcp_loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577811 | `loadtesting_test_create` | ❌ |
| 2 | 0.519418 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.512099 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.472753 | `monitor_webtests_create` | ❌ |
| 5 | 0.460717 | `loadtesting_testresource_list` | ❌ |

---

## Test 295

**Expected Tool:** `azmcp_loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626149 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.619964 | `loadtesting_test_get` | ❌ |
| 3 | 0.594624 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.590556 | `monitor_webtests_get` | ❌ |
| 5 | 0.535974 | `monitor_webtests_list` | ❌ |

---

## Test 296

**Expected Tool:** `azmcp_loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645708 | `loadtesting_testresource_create` | ❌ |
| 2 | 0.618880 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.542027 | `loadtesting_test_create` | ❌ |
| 4 | 0.539891 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.526788 | `monitor_webtests_list` | ❌ |

---

## Test 297

**Expected Tool:** `azmcp_loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794326 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.653165 | `monitor_webtests_list` | ❌ |
| 3 | 0.577408 | `group_list` | ❌ |
| 4 | 0.575172 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.565565 | `datadog_monitoredresources_list` | ❌ |

---

## Test 298

**Expected Tool:** `azmcp_loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688976 | `loadtesting_testrun_create` | ❌ |
| 2 | 0.594879 | `loadtesting_testrun_update` | ❌ |
| 3 | 0.558636 | `loadtesting_test_create` | ❌ |
| 4 | 0.547102 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.496224 | `loadtesting_testresource_list` | ❌ |

---

## Test 299

**Expected Tool:** `azmcp_loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619146 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.601927 | `loadtesting_test_get` | ❌ |
| 3 | 0.597430 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.577350 | `monitor_webtests_get` | ❌ |
| 5 | 0.565996 | `loadtesting_testrun_list` | ❌ |

---

## Test 300

**Expected Tool:** `azmcp_loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669180 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.640360 | `loadtesting_testrun_list` | ❌ |
| 3 | 0.601075 | `loadtesting_test_get` | ❌ |
| 4 | 0.577460 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.569176 | `monitor_webtests_get` | ❌ |

---

## Test 301

**Expected Tool:** `azmcp_loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.706747 | `loadtesting_testrun_update` | ❌ |
| 2 | 0.514428 | `loadtesting_testrun_create` | ❌ |
| 3 | 0.486980 | `monitor_webtests_update` | ❌ |
| 4 | 0.470337 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.468356 | `monitor_webtests_get` | ❌ |

---

## Test 302

**Expected Tool:** `azmcp_grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.599427 | `kusto_cluster_list` | ❌ |
| 2 | 0.578892 | `grafana_list` | ❌ |
| 3 | 0.551851 | `search_service_list` | ❌ |
| 4 | 0.550372 | `subscription_list` | ❌ |
| 5 | 0.531259 | `redis_list` | ❌ |

---

## Test 303

**Expected Tool:** `azmcp_managedlustre_filesystem_create`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.728113 | `managedlustre_filesystem_create` | ❌ |
| 2 | 0.616164 | `managedlustre_filesystem_list` | ❌ |
| 3 | 0.605731 | `managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.598255 | `managedlustre_filesystem_update` | ❌ |
| 5 | 0.557720 | `managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 304

**Expected Tool:** `azmcp_managedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.750675 | `managedlustre_filesystem_list` | ❌ |
| 2 | 0.631746 | `managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.582660 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.562377 | `kusto_cluster_list` | ❌ |
| 5 | 0.513156 | `search_service_list` | ❌ |

---

## Test 305

**Expected Tool:** `azmcp_managedlustre_filesystem_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743903 | `managedlustre_filesystem_list` | ❌ |
| 2 | 0.613185 | `managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.565856 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.519986 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.515433 | `loadtesting_testresource_list` | ❌ |

---

## Test 306

**Expected Tool:** `azmcp_managedlustre_filesystem_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.827346 | `managedlustre_filesystem_sku_get` | ❌ |
| 2 | 0.613674 | `managedlustre_filesystem_list` | ❌ |
| 3 | 0.513242 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.496242 | `managedlustre_filesystem_subnetsize_validate` | ❌ |
| 5 | 0.470241 | `kusto_cluster_list` | ❌ |

---

## Test 307

**Expected Tool:** `azmcp_managedlustre_filesystem_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739766 | `managedlustre_filesystem_subnetsize_ask` | ❌ |
| 2 | 0.651598 | `managedlustre_filesystem_subnetsize_validate` | ❌ |
| 3 | 0.594558 | `managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.559498 | `managedlustre_filesystem_list` | ❌ |
| 5 | 0.533684 | `managedlustre_filesystem_create` | ❌ |

---

## Test 308

**Expected Tool:** `azmcp_managedlustre_filesystem_subnetsize_validate`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.879389 | `managedlustre_filesystem_subnetsize_validate` | ❌ |
| 2 | 0.622463 | `managedlustre_filesystem_subnetsize_ask` | ❌ |
| 3 | 0.542783 | `managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.515936 | `managedlustre_filesystem_create` | ❌ |
| 5 | 0.480855 | `managedlustre_filesystem_list` | ❌ |

---

## Test 309

**Expected Tool:** `azmcp_managedlustre_filesystem_update`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739000 | `managedlustre_filesystem_update` | ❌ |
| 2 | 0.527525 | `managedlustre_filesystem_create` | ❌ |
| 3 | 0.487193 | `managedlustre_filesystem_list` | ❌ |
| 4 | 0.385317 | `managedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.344891 | `managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 310

**Expected Tool:** `azmcp_marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.570145 | `marketplace_product_get` | ❌ |
| 2 | 0.499306 | `marketplace_product_list` | ❌ |
| 3 | 0.353256 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.333160 | `servicebus_topic_details` | ❌ |
| 5 | 0.330935 | `servicebus_queue_details` | ❌ |

---

## Test 311

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.608016 | `marketplace_product_list` | ❌ |
| 2 | 0.443133 | `marketplace_product_get` | ❌ |
| 3 | 0.343549 | `search_service_list` | ❌ |
| 4 | 0.330500 | `foundry_models_list` | ❌ |
| 5 | 0.328637 | `managedlustre_filesystem_sku_get` | ❌ |

---

## Test 312

**Expected Tool:** `azmcp_marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537769 | `marketplace_product_list` | ❌ |
| 2 | 0.385167 | `marketplace_product_get` | ❌ |
| 3 | 0.308769 | `foundry_models_list` | ❌ |
| 4 | 0.288006 | `redis_list` | ❌ |
| 5 | 0.260340 | `managedlustre_filesystem_sku_get` | ❌ |

---

## Test 313

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646844 | `get_bestpractices_get` | ❌ |
| 2 | 0.635416 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.531727 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490160 | `deploy_plan_get` | ❌ |

---

## Test 314

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600903 | `get_bestpractices_get` | ❌ |
| 2 | 0.548615 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.541091 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.516810 | `deploy_plan_get` | ❌ |
| 5 | 0.516443 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 315

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625259 | `get_bestpractices_get` | ❌ |
| 2 | 0.594399 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.518643 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.465572 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.450629 | `cloudarchitect_design` | ❌ |

---

## Test 316

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624273 | `get_bestpractices_get` | ❌ |
| 2 | 0.570508 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.522998 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.467377 | `extension_cli_install` | ❌ |

---

## Test 317

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581850 | `get_bestpractices_get` | ❌ |
| 2 | 0.497350 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.495659 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.486960 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.474460 | `deploy_plan_get` | ❌ |

---

## Test 318

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610986 | `get_bestpractices_get` | ❌ |
| 2 | 0.532867 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.487322 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.458060 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.448034 | `extension_cli_install` | ❌ |

---

## Test 319

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557862 | `get_bestpractices_get` | ❌ |
| 2 | 0.513354 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.505123 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.421581 | `cloudarchitect_design` | ❌ |

---

## Test 320

**Expected Tool:** `azmcp_get_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.582541 | `get_bestpractices_get` | ❌ |
| 2 | 0.500420 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.472112 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.433134 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.432087 | `cloudarchitect_design` | ❌ |

---

## Test 321

**Expected Tool:** `azmcp_monitor_activitylog_list`  
**Prompt:** List the activity logs of the last month for <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537893 | `monitor_activitylog_list` | ❌ |
| 2 | 0.506212 | `monitor_resource_log_query` | ❌ |
| 3 | 0.371727 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.363798 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.344629 | `datadog_monitoredresources_list` | ❌ |

---

## Test 322

**Expected Tool:** `azmcp_monitor_healthmodels_entity_gethealth`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660947 | `monitor_healthmodels_entity_gethealth` | ❌ |
| 2 | 0.603504 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.355116 | `foundry_openai_models-list` | ❌ |
| 4 | 0.351697 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.328321 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 323

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592640 | `monitor_metrics_definitions` | ❌ |
| 2 | 0.424141 | `monitor_metrics_query` | ❌ |
| 3 | 0.368319 | `bicepschema_get` | ❌ |
| 4 | 0.332445 | `monitor_table_type_list` | ❌ |
| 5 | 0.322726 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 324

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607600 | `storage_account_get` | ❌ |
| 2 | 0.587736 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.544781 | `storage_blob_container_get` | ❌ |
| 4 | 0.495829 | `storage_blob_get` | ❌ |
| 5 | 0.473421 | `managedlustre_filesystem_list` | ❌ |

---

## Test 325

**Expected Tool:** `azmcp_monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633173 | `monitor_metrics_definitions` | ❌ |
| 2 | 0.495513 | `monitor_metrics_query` | ❌ |
| 3 | 0.433945 | `monitor_resource_log_query` | ❌ |
| 4 | 0.392960 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.388750 | `bicepschema_get` | ❌ |

---

## Test 326

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555377 | `monitor_metrics_query` | ❌ |
| 2 | 0.527530 | `monitor_resource_log_query` | ❌ |
| 3 | 0.464743 | `applens_resource_diagnose` | ❌ |
| 4 | 0.420462 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.413282 | `applicationinsights_recommendation_list` | ❌ |

---

## Test 327

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557830 | `monitor_metrics_query` | ❌ |
| 2 | 0.476671 | `monitor_resource_log_query` | ❌ |
| 3 | 0.460611 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `quota_usage_check` | ❌ |
| 5 | 0.438233 | `monitor_metrics_definitions` | ❌ |

---

## Test 328

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.461312 | `monitor_metrics_query` | ❌ |
| 2 | 0.390082 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.338581 | `monitor_resource_log_query` | ❌ |
| 4 | 0.330272 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.306395 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 329

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496878 | `monitor_resource_log_query` | ❌ |
| 2 | 0.492138 | `monitor_metrics_query` | ❌ |
| 3 | 0.448148 | `applens_resource_diagnose` | ❌ |
| 4 | 0.412184 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.397335 | `quota_usage_check` | ❌ |

---

## Test 330

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.525585 | `monitor_metrics_query` | ❌ |
| 2 | 0.406185 | `monitor_resource_log_query` | ❌ |
| 3 | 0.384482 | `monitor_metrics_definitions` | ❌ |
| 4 | 0.347723 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.325851 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 331

**Expected Tool:** `azmcp_monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480143 | `monitor_metrics_query` | ❌ |
| 2 | 0.444673 | `monitor_resource_log_query` | ❌ |
| 3 | 0.388310 | `applens_resource_diagnose` | ❌ |
| 4 | 0.363517 | `quota_usage_check` | ❌ |
| 5 | 0.350254 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 332

**Expected Tool:** `azmcp_monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687852 | `monitor_resource_log_query` | ❌ |
| 2 | 0.621919 | `monitor_workspace_log_query` | ❌ |
| 3 | 0.598393 | `monitor_activitylog_list` | ❌ |
| 4 | 0.485633 | `deploy_app_logs_get` | ❌ |
| 5 | 0.469703 | `monitor_metrics_query` | ❌ |

---

## Test 333

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.851075 | `monitor_table_list` | ❌ |
| 2 | 0.725859 | `monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `monitor_workspace_list` | ❌ |
| 4 | 0.541928 | `kusto_table_list` | ❌ |
| 5 | 0.539481 | `monitor_workspace_log_query` | ❌ |

---

## Test 334

**Expected Tool:** `azmcp_monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.798473 | `monitor_table_list` | ❌ |
| 2 | 0.701289 | `monitor_table_type_list` | ❌ |
| 3 | 0.599970 | `monitor_workspace_list` | ❌ |
| 4 | 0.542860 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.502941 | `monitor_resource_log_query` | ❌ |

---

## Test 335

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.881555 | `monitor_table_type_list` | ❌ |
| 2 | 0.765702 | `monitor_table_list` | ❌ |
| 3 | 0.569921 | `monitor_workspace_list` | ❌ |
| 4 | 0.504780 | `mysql_table_list` | ❌ |
| 5 | 0.497622 | `monitor_workspace_log_query` | ❌ |

---

## Test 336

**Expected Tool:** `azmcp_monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.843199 | `monitor_table_type_list` | ❌ |
| 2 | 0.736837 | `monitor_table_list` | ❌ |
| 3 | 0.576731 | `monitor_workspace_list` | ❌ |
| 4 | 0.509598 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.481227 | `mysql_table_list` | ❌ |

---

## Test 337

**Expected Tool:** `azmcp_monitor_webtests_create`  
**Prompt:** Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650744 | `monitor_webtests_create` | ❌ |
| 2 | 0.570036 | `monitor_webtests_list` | ❌ |
| 3 | 0.549955 | `monitor_webtests_update` | ❌ |
| 4 | 0.532895 | `monitor_webtests_get` | ❌ |
| 5 | 0.482207 | `loadtesting_testresource_create` | ❌ |

---

## Test 338

**Expected Tool:** `azmcp_monitor_webtests_get`  
**Prompt:** Get Web Test details for <webtest_resource_name> in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.758692 | `monitor_webtests_get` | ❌ |
| 2 | 0.725442 | `monitor_webtests_list` | ❌ |
| 3 | 0.583816 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.562797 | `monitor_webtests_update` | ❌ |
| 5 | 0.530557 | `monitor_webtests_create` | ❌ |

---

## Test 339

**Expected Tool:** `azmcp_monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.730616 | `monitor_webtests_list` | ❌ |
| 2 | 0.610160 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.547708 | `grafana_list` | ❌ |
| 4 | 0.520829 | `redis_list` | ❌ |
| 5 | 0.495796 | `monitor_webtests_get` | ❌ |

---

## Test 340

**Expected Tool:** `azmcp_monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.793807 | `monitor_webtests_list` | ❌ |
| 2 | 0.675965 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.584093 | `monitor_webtests_get` | ❌ |
| 4 | 0.573602 | `group_list` | ❌ |
| 5 | 0.546088 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 341

**Expected Tool:** `azmcp_monitor_webtests_update`  
**Prompt:** Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686430 | `monitor_webtests_update` | ❌ |
| 2 | 0.558997 | `monitor_webtests_get` | ❌ |
| 3 | 0.558245 | `monitor_webtests_create` | ❌ |
| 4 | 0.553750 | `monitor_webtests_list` | ❌ |
| 5 | 0.508776 | `loadtesting_testrun_update` | ❌ |

---

## Test 342

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813902 | `monitor_workspace_list` | ❌ |
| 2 | 0.680201 | `grafana_list` | ❌ |
| 3 | 0.660135 | `monitor_table_list` | ❌ |
| 4 | 0.610623 | `kusto_cluster_list` | ❌ |
| 5 | 0.600802 | `search_service_list` | ❌ |

---

## Test 343

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656194 | `monitor_workspace_list` | ❌ |
| 2 | 0.585436 | `monitor_table_list` | ❌ |
| 3 | 0.531231 | `monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `grafana_list` | ❌ |
| 5 | 0.506772 | `monitor_workspace_log_query` | ❌ |

---

## Test 344

**Expected Tool:** `azmcp_monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732962 | `monitor_workspace_list` | ❌ |
| 2 | 0.601481 | `grafana_list` | ❌ |
| 3 | 0.580261 | `monitor_table_list` | ❌ |
| 4 | 0.523782 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.522749 | `kusto_cluster_list` | ❌ |

---

## Test 345

**Expected Tool:** `azmcp_monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610197 | `monitor_workspace_log_query` | ❌ |
| 2 | 0.587632 | `monitor_resource_log_query` | ❌ |
| 3 | 0.527786 | `monitor_activitylog_list` | ❌ |
| 4 | 0.498340 | `deploy_app_logs_get` | ❌ |
| 5 | 0.486058 | `monitor_table_list` | ❌ |

---

## Test 346

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668827 | `datadog_monitoredresources_list` | ❌ |
| 2 | 0.454270 | `redis_list` | ❌ |
| 3 | 0.413661 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.413173 | `monitor_metrics_query` | ❌ |
| 5 | 0.401731 | `grafana_list` | ❌ |

---

## Test 347

**Expected Tool:** `azmcp_datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `datadog_monitoredresources_list` | ❌ |
| 2 | 0.443481 | `monitor_metrics_query` | ❌ |
| 3 | 0.440052 | `redis_list` | ❌ |
| 4 | 0.424391 | `monitor_resource_log_query` | ❌ |
| 5 | 0.385122 | `loadtesting_testresource_list` | ❌ |

---

## Test 348

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533164 | `quota_usage_check` | ❌ |
| 2 | 0.481220 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.476826 | `extension_azqr` | ❌ |
| 4 | 0.471499 | `subscription_list` | ❌ |
| 5 | 0.468404 | `applens_resource_diagnose` | ❌ |

---

## Test 349

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532861 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.492863 | `get_bestpractices_get` | ❌ |
| 3 | 0.476164 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.473365 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.464604 | `cloudarchitect_design` | ❌ |

---

## Test 350

**Expected Tool:** `azmcp_extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.536997 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.516925 | `extension_azqr` | ❌ |
| 3 | 0.514978 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.504673 | `quota_usage_check` | ❌ |
| 5 | 0.494858 | `deploy_plan_get` | ❌ |

---

## Test 351

**Expected Tool:** `azmcp_quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590878 | `quota_region_availability_list` | ❌ |
| 2 | 0.413274 | `quota_usage_check` | ❌ |
| 3 | 0.391332 | `redis_list` | ❌ |
| 4 | 0.372940 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.369828 | `managedlustre_filesystem_sku_get` | ❌ |

---

## Test 352

**Expected Tool:** `azmcp_quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609244 | `quota_usage_check` | ❌ |
| 2 | 0.491058 | `quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.373776 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.371407 | `redis_list` | ❌ |

---

## Test 353

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645259 | `role_assignment_list` | ❌ |
| 2 | 0.539761 | `subscription_list` | ❌ |
| 3 | 0.483988 | `group_list` | ❌ |
| 4 | 0.478700 | `grafana_list` | ❌ |
| 5 | 0.471364 | `cosmos_account_list` | ❌ |

---

## Test 354

**Expected Tool:** `azmcp_role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609705 | `role_assignment_list` | ❌ |
| 2 | 0.514696 | `subscription_list` | ❌ |
| 3 | 0.456956 | `grafana_list` | ❌ |
| 4 | 0.449210 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.445149 | `redis_list` | ❌ |

---

## Test 355

**Expected Tool:** `azmcp_redis_list`  
**Prompt:** List all Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.810504 | `redis_list` | ❌ |
| 2 | 0.587836 | `grafana_list` | ❌ |
| 3 | 0.512954 | `kusto_cluster_list` | ❌ |
| 4 | 0.508531 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.501218 | `postgres_server_list` | ❌ |

---

## Test 356

**Expected Tool:** `azmcp_redis_list`  
**Prompt:** Show me my Redis resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685128 | `redis_list` | ❌ |
| 2 | 0.374328 | `grafana_list` | ❌ |
| 3 | 0.364197 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.359659 | `mysql_server_list` | ❌ |
| 5 | 0.331502 | `mysql_database_list` | ❌ |

---

## Test 357

**Expected Tool:** `azmcp_redis_list`  
**Prompt:** Show me the Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.781228 | `redis_list` | ❌ |
| 2 | 0.539177 | `grafana_list` | ❌ |
| 3 | 0.449276 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.449014 | `postgres_server_list` | ❌ |
| 5 | 0.442854 | `kusto_cluster_list` | ❌ |

---

## Test 358

**Expected Tool:** `azmcp_redis_list`  
**Prompt:** Show me my Redis caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572767 | `redis_list` | ❌ |
| 2 | 0.316630 | `mysql_database_list` | ❌ |
| 3 | 0.301786 | `postgres_database_list` | ❌ |
| 4 | 0.286513 | `mysql_server_list` | ❌ |
| 5 | 0.273014 | `kusto_cluster_list` | ❌ |

---

## Test 359

**Expected Tool:** `azmcp_redis_list`  
**Prompt:** Get Redis clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478070 | `redis_list` | ❌ |
| 2 | 0.456309 | `kusto_cluster_list` | ❌ |
| 3 | 0.384630 | `kusto_cluster_get` | ❌ |
| 4 | 0.359434 | `kusto_database_list` | ❌ |
| 5 | 0.343294 | `aks_cluster_get` | ❌ |

---

## Test 360

**Expected Tool:** `azmcp_group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755981 | `group_list` | ❌ |
| 2 | 0.566543 | `workbooks_list` | ❌ |
| 3 | 0.564520 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.552660 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.549464 | `monitor_webtests_list` | ❌ |

---

## Test 361

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `group_list` | ❌ |
| 2 | 0.464690 | `redis_list` | ❌ |
| 3 | 0.463685 | `datadog_monitoredresources_list` | ❌ |
| 4 | 0.462391 | `mysql_server_list` | ❌ |
| 5 | 0.460280 | `loadtesting_testresource_list` | ❌ |

---

## Test 362

**Expected Tool:** `azmcp_group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665771 | `group_list` | ❌ |
| 2 | 0.532656 | `datadog_monitoredresources_list` | ❌ |
| 3 | 0.532505 | `redis_list` | ❌ |
| 4 | 0.532054 | `eventgrid_topic_list` | ❌ |
| 5 | 0.531920 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 363

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555127 | `resourcehealth_availability-status_get` | ❌ |
| 2 | 0.538273 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.404305 | `foundry_openai_models-list` | ❌ |
| 4 | 0.377586 | `quota_usage_check` | ❌ |
| 5 | 0.373112 | `monitor_healthmodels_entity_gethealth` | ❌ |

---

## Test 364

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.576591 | `storage_account_get` | ❌ |
| 2 | 0.565897 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.556167 | `storage_blob_container_get` | ❌ |
| 4 | 0.487207 | `storage_blob_get` | ❌ |
| 5 | 0.466885 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 365

**Expected Tool:** `azmcp_resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577398 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.500907 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.424939 | `mysql_server_list` | ❌ |
| 4 | 0.413484 | `foundry_openai_models-list` | ❌ |
| 5 | 0.412025 | `loadtesting_testresource_list` | ❌ |

---

## Test 366

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.585501 | `redis_list` | ❌ |
| 3 | 0.549914 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.548549 | `grafana_list` | ❌ |
| 5 | 0.544505 | `subscription_list` | ❌ |

---

## Test 367

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644982 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.546533 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.509740 | `resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.508252 | `quota_usage_check` | ❌ |
| 5 | 0.505776 | `redis_list` | ❌ |

---

## Test 368

**Expected Tool:** `azmcp_resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.596890 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.550186 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.496640 | `resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.441921 | `applens_resource_diagnose` | ❌ |
| 5 | 0.433614 | `loadtesting_testresource_list` | ❌ |

---

## Test 369

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690719 | `resourcehealth_service-health-events_list` | ❌ |
| 2 | 0.554895 | `search_service_list` | ❌ |
| 3 | 0.534250 | `eventgrid_topic_list` | ❌ |
| 4 | 0.529761 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.518372 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 370

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686448 | `resourcehealth_service-health-events_list` | ❌ |
| 2 | 0.534556 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.513815 | `search_service_list` | ❌ |
| 4 | 0.513259 | `eventgrid_topic_list` | ❌ |
| 5 | 0.501135 | `subscription_list` | ❌ |

---

## Test 371

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.450841 | `resourcehealth_service-health-events_list` | ❌ |
| 2 | 0.267663 | `applens_resource_diagnose` | ❌ |
| 3 | 0.245720 | `cloudarchitect_design` | ❌ |
| 4 | 0.216847 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211842 | `search_service_list` | ❌ |

---

## Test 372

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685391 | `resourcehealth_service-health-events_list` | ❌ |
| 2 | 0.527905 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.524063 | `eventgrid_topic_list` | ❌ |
| 4 | 0.520197 | `search_service_list` | ❌ |
| 5 | 0.502064 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 373

**Expected Tool:** `azmcp_resourcehealth_service-health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565851 | `resourcehealth_service-health-events_list` | ❌ |
| 2 | 0.437868 | `search_service_list` | ❌ |
| 3 | 0.403665 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.402493 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.402232 | `foundry_agents_list` | ❌ |

---

## Test 374

**Expected Tool:** `azmcp_servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `servicebus_queue_details` | ❌ |
| 2 | 0.460932 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.436980 | `servicebus_topic_details` | ❌ |
| 4 | 0.385812 | `search_knowledge_base_get` | ❌ |
| 5 | 0.384139 | `storage_account_get` | ❌ |

---

## Test 375

**Expected Tool:** `azmcp_servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642952 | `servicebus_topic_details` | ❌ |
| 2 | 0.571861 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.483976 | `servicebus_queue_details` | ❌ |
| 4 | 0.482958 | `eventgrid_topic_list` | ❌ |
| 5 | 0.458711 | `eventgrid_subscription_list` | ❌ |

---

## Test 376

**Expected Tool:** `azmcp_servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `servicebus_topic_subscription_details` | ❌ |
| 2 | 0.517623 | `servicebus_topic_details` | ❌ |
| 3 | 0.494515 | `servicebus_queue_details` | ❌ |
| 4 | 0.493853 | `eventgrid_topic_list` | ❌ |
| 5 | 0.472128 | `eventgrid_subscription_list` | ❌ |

---

## Test 377

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** Show me the details of SignalR <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532544 | `signalr_runtime_get` | ❌ |
| 2 | 0.355028 | `redis_list` | ❌ |
| 3 | 0.329804 | `foundry_resource_get` | ❌ |
| 4 | 0.319981 | `sql_server_show` | ❌ |
| 5 | 0.304420 | `servicebus_queue_details` | ❌ |

---

## Test 378

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** Show me the network information of SignalR runtime <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573446 | `signalr_runtime_get` | ❌ |
| 2 | 0.337342 | `sql_server_show` | ❌ |
| 3 | 0.306559 | `foundry_resource_get` | ❌ |
| 4 | 0.305021 | `redis_list` | ❌ |
| 5 | 0.300956 | `servicebus_topic_details` | ❌ |

---

## Test 379

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710353 | `signalr_runtime_get` | ❌ |
| 2 | 0.411396 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.410606 | `foundry_resource_get` | ❌ |
| 4 | 0.399412 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.382028 | `sql_server_list` | ❌ |

---

## Test 380

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.715974 | `signalr_runtime_get` | ❌ |
| 2 | 0.459045 | `foundry_resource_get` | ❌ |
| 3 | 0.430829 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.430765 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.417032 | `functionapp_get` | ❌ |

---

## Test 381

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564072 | `signalr_runtime_get` | ❌ |
| 2 | 0.501077 | `redis_list` | ❌ |
| 3 | 0.494478 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.481428 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.462090 | `mysql_server_list` | ❌ |

---

## Test 382

**Expected Tool:** `azmcp_signalr_runtime_get`  
**Prompt:** List all SignalRs in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530646 | `signalr_runtime_get` | ❌ |
| 2 | 0.507653 | `postgres_server_list` | ❌ |
| 3 | 0.495157 | `redis_list` | ❌ |
| 4 | 0.494498 | `kusto_cluster_list` | ❌ |
| 5 | 0.487856 | `subscription_list` | ❌ |

---

## Test 383

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `sql_db_create` | ❌ |
| 2 | 0.470892 | `sql_server_create` | ❌ |
| 3 | 0.420504 | `sql_db_rename` | ❌ |
| 4 | 0.408700 | `sql_db_delete` | ❌ |
| 5 | 0.404860 | `sql_server_delete` | ❌ |

---

## Test 384

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `sql_db_create` | ❌ |
| 2 | 0.459672 | `sql_server_create` | ❌ |
| 3 | 0.437526 | `sql_server_delete` | ❌ |
| 4 | 0.424021 | `appservice_database_add` | ❌ |
| 5 | 0.420843 | `sql_db_show` | ❌ |

---

## Test 385

**Expected Tool:** `azmcp_sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604427 | `sql_db_create` | ❌ |
| 2 | 0.545786 | `sql_server_create` | ❌ |
| 3 | 0.503982 | `sql_db_rename` | ❌ |
| 4 | 0.494337 | `sql_db_show` | ❌ |
| 5 | 0.473777 | `sql_db_list` | ❌ |

---

## Test 386

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.568262 | `sql_db_delete` | ❌ |
| 2 | 0.567412 | `sql_server_delete` | ❌ |
| 3 | 0.391509 | `sql_db_rename` | ❌ |
| 4 | 0.386564 | `sql_server_firewall-rule_delete` | ❌ |
| 5 | 0.364776 | `sql_db_show` | ❌ |

---

## Test 387

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567513 | `sql_server_delete` | ❌ |
| 2 | 0.543440 | `sql_db_delete` | ❌ |
| 3 | 0.500756 | `sql_db_show` | ❌ |
| 4 | 0.481083 | `sql_db_rename` | ❌ |
| 5 | 0.478637 | `sql_db_list` | ❌ |

---

## Test 388

**Expected Tool:** `azmcp_sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509932 | `sql_db_delete` | ❌ |
| 2 | 0.490892 | `sql_server_delete` | ❌ |
| 3 | 0.364494 | `postgres_database_list` | ❌ |
| 4 | 0.355416 | `mysql_database_list` | ❌ |
| 5 | 0.347837 | `sql_db_rename` | ❌ |

---

## Test 389

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.643240 | `sql_db_list` | ❌ |
| 2 | 0.639694 | `mysql_database_list` | ❌ |
| 3 | 0.609178 | `postgres_database_list` | ❌ |
| 4 | 0.602890 | `cosmos_database_list` | ❌ |
| 5 | 0.570140 | `kusto_database_list` | ❌ |

---

## Test 390

**Expected Tool:** `azmcp_sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `sql_server_show` | ❌ |
| 2 | 0.609248 | `sql_db_list` | ❌ |
| 3 | 0.557353 | `mysql_database_list` | ❌ |
| 4 | 0.553488 | `mysql_server_config_get` | ❌ |
| 5 | 0.524274 | `sql_db_show` | ❌ |

---

## Test 391

**Expected Tool:** `azmcp_sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593348 | `sql_db_rename` | ❌ |
| 2 | 0.425282 | `sql_server_delete` | ❌ |
| 3 | 0.416296 | `sql_db_delete` | ❌ |
| 4 | 0.396947 | `sql_db_create` | ❌ |
| 5 | 0.346018 | `sql_db_show` | ❌ |

---

## Test 392

**Expected Tool:** `azmcp_sql_db_rename`  
**Prompt:** Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711051 | `sql_db_rename` | ❌ |
| 2 | 0.516409 | `sql_server_delete` | ❌ |
| 3 | 0.506541 | `sql_db_delete` | ❌ |
| 4 | 0.501203 | `sql_db_create` | ❌ |
| 5 | 0.433939 | `sql_server_show` | ❌ |

---

## Test 393

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610991 | `sql_server_show` | ❌ |
| 2 | 0.593150 | `postgres_server_config_get` | ❌ |
| 3 | 0.530422 | `mysql_server_config_get` | ❌ |
| 4 | 0.528136 | `sql_db_show` | ❌ |
| 5 | 0.465404 | `sql_db_list` | ❌ |

---

## Test 394

**Expected Tool:** `azmcp_sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530095 | `sql_db_show` | ❌ |
| 2 | 0.503681 | `sql_server_show` | ❌ |
| 3 | 0.439936 | `sql_db_list` | ❌ |
| 4 | 0.438622 | `mysql_table_schema_get` | ❌ |
| 5 | 0.432919 | `mysql_database_list` | ❌ |

---

## Test 395

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.603247 | `sql_db_update` | ❌ |
| 2 | 0.467571 | `sql_db_create` | ❌ |
| 3 | 0.440493 | `sql_db_rename` | ❌ |
| 4 | 0.427621 | `sql_db_show` | ❌ |
| 5 | 0.413941 | `sql_server_delete` | ❌ |

---

## Test 396

**Expected Tool:** `azmcp_sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.549608 | `sql_db_update` | ❌ |
| 2 | 0.418357 | `sql_server_delete` | ❌ |
| 3 | 0.401692 | `sql_db_list` | ❌ |
| 4 | 0.395480 | `sql_db_rename` | ❌ |
| 5 | 0.394806 | `sql_db_show` | ❌ |

---

## Test 397

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678124 | `sql_elastic-pool_list` | ❌ |
| 2 | 0.502397 | `sql_db_list` | ❌ |
| 3 | 0.498367 | `mysql_database_list` | ❌ |
| 4 | 0.485233 | `aks_nodepool_get` | ❌ |
| 5 | 0.479044 | `sql_server_show` | ❌ |

---

## Test 398

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606425 | `sql_elastic-pool_list` | ❌ |
| 2 | 0.502877 | `sql_server_show` | ❌ |
| 3 | 0.457146 | `sql_db_list` | ❌ |
| 4 | 0.450761 | `aks_nodepool_get` | ❌ |
| 5 | 0.432816 | `mysql_database_list` | ❌ |

---

## Test 399

**Expected Tool:** `azmcp_sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592709 | `sql_elastic-pool_list` | ❌ |
| 2 | 0.420325 | `mysql_database_list` | ❌ |
| 3 | 0.407160 | `aks_nodepool_get` | ❌ |
| 4 | 0.402616 | `mysql_server_list` | ❌ |
| 5 | 0.397694 | `sql_db_list` | ❌ |

---

## Test 400

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682606 | `sql_server_create` | ❌ |
| 2 | 0.563708 | `sql_db_create` | ❌ |
| 3 | 0.529198 | `sql_server_list` | ❌ |
| 4 | 0.482102 | `storage_account_create` | ❌ |
| 5 | 0.474207 | `sql_db_rename` | ❌ |

---

## Test 401

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618355 | `sql_server_create` | ❌ |
| 2 | 0.510185 | `sql_db_create` | ❌ |
| 3 | 0.472609 | `sql_server_show` | ❌ |
| 4 | 0.441243 | `sql_server_delete` | ❌ |
| 5 | 0.400986 | `sql_db_rename` | ❌ |

---

## Test 402

**Expected Tool:** `azmcp_sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589818 | `sql_server_create` | ❌ |
| 2 | 0.501403 | `sql_db_create` | ❌ |
| 3 | 0.497890 | `sql_server_list` | ❌ |
| 4 | 0.461181 | `sql_db_rename` | ❌ |
| 5 | 0.442934 | `mysql_server_list` | ❌ |

---

## Test 403

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656593 | `sql_server_delete` | ❌ |
| 2 | 0.548123 | `sql_db_delete` | ❌ |
| 3 | 0.518036 | `sql_server_list` | ❌ |
| 4 | 0.495550 | `sql_server_create` | ❌ |
| 5 | 0.483132 | `workbooks_delete` | ❌ |

---

## Test 404

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615073 | `sql_server_delete` | ❌ |
| 2 | 0.393885 | `postgres_server_list` | ❌ |
| 3 | 0.379875 | `sql_db_delete` | ❌ |
| 4 | 0.376660 | `sql_server_show` | ❌ |
| 5 | 0.350103 | `sql_server_list` | ❌ |

---

## Test 405

**Expected Tool:** `azmcp_sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624310 | `sql_server_delete` | ❌ |
| 2 | 0.454984 | `sql_db_delete` | ❌ |
| 3 | 0.362389 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.341503 | `sql_server_show` | ❌ |
| 5 | 0.318758 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 406

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783479 | `sql_server_entra-admin_list` | ❌ |
| 2 | 0.456051 | `sql_server_show` | ❌ |
| 3 | 0.434868 | `sql_server_list` | ❌ |
| 4 | 0.401908 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376049 | `sql_db_list` | ❌ |

---

## Test 407

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713306 | `sql_server_entra-admin_list` | ❌ |
| 2 | 0.413144 | `sql_server_show` | ❌ |
| 3 | 0.368082 | `sql_server_list` | ❌ |
| 4 | 0.315942 | `sql_db_list` | ❌ |
| 5 | 0.311085 | `postgres_server_list` | ❌ |

---

## Test 408

**Expected Tool:** `azmcp_sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646419 | `sql_server_entra-admin_list` | ❌ |
| 2 | 0.356025 | `sql_server_show` | ❌ |
| 3 | 0.322155 | `sql_server_list` | ❌ |
| 4 | 0.307823 | `sql_server_create` | ❌ |
| 5 | 0.269788 | `sql_server_delete` | ❌ |

---

## Test 409

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635466 | `sql_server_firewall-rule_create` | ❌ |
| 2 | 0.532712 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522184 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.448822 | `sql_server_create` | ❌ |
| 5 | 0.440845 | `sql_server_delete` | ❌ |

---

## Test 410

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670189 | `sql_server_firewall-rule_create` | ❌ |
| 2 | 0.533562 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316619 | `sql_server_list` | ❌ |
| 5 | 0.302362 | `sql_server_delete` | ❌ |

---

## Test 411

**Expected Tool:** `azmcp_sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685194 | `sql_server_firewall-rule_create` | ❌ |
| 2 | 0.574530 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539693 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428795 | `sql_server_create` | ❌ |
| 5 | 0.395147 | `sql_db_create` | ❌ |

---

## Test 412

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691421 | `sql_server_firewall-rule_delete` | ❌ |
| 2 | 0.584379 | `sql_server_delete` | ❌ |
| 3 | 0.543857 | `sql_server_firewall-rule_list` | ❌ |
| 4 | 0.540333 | `sql_server_firewall-rule_create` | ❌ |
| 5 | 0.498540 | `sql_db_delete` | ❌ |

---

## Test 413

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.670179 | `sql_server_firewall-rule_delete` | ❌ |
| 2 | 0.574340 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.530419 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.488418 | `sql_server_delete` | ❌ |
| 5 | 0.360444 | `sql_db_delete` | ❌ |

---

## Test 414

**Expected Tool:** `azmcp_sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.671212 | `sql_server_firewall-rule_delete` | ❌ |
| 2 | 0.601230 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.577330 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.499272 | `sql_server_delete` | ❌ |
| 5 | 0.378659 | `sql_db_delete` | ❌ |

---

## Test 415

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.729372 | `sql_server_firewall-rule_list` | ❌ |
| 2 | 0.549667 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513114 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `sql_server_show` | ❌ |
| 5 | 0.418817 | `sql_server_list` | ❌ |

---

## Test 416

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630731 | `sql_server_firewall-rule_list` | ❌ |
| 2 | 0.524126 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476757 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410680 | `sql_server_show` | ❌ |
| 5 | 0.348100 | `sql_server_list` | ❌ |

---

## Test 417

**Expected Tool:** `azmcp_sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630546 | `sql_server_firewall-rule_list` | ❌ |
| 2 | 0.532454 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473501 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412957 | `sql_server_show` | ❌ |
| 5 | 0.350513 | `sql_server_list` | ❌ |

---

## Test 418

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694404 | `sql_server_list` | ❌ |
| 2 | 0.596686 | `mysql_server_list` | ❌ |
| 3 | 0.578169 | `sql_db_list` | ❌ |
| 4 | 0.515851 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.509789 | `sql_db_show` | ❌ |

---

## Test 419

**Expected Tool:** `azmcp_sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.618218 | `sql_server_list` | ❌ |
| 2 | 0.593837 | `mysql_server_list` | ❌ |
| 3 | 0.542397 | `sql_db_list` | ❌ |
| 4 | 0.507404 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.496200 | `group_list` | ❌ |

---

## Test 420

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `sql_db_show` | ❌ |
| 2 | 0.595184 | `sql_server_show` | ❌ |
| 3 | 0.587728 | `sql_server_list` | ❌ |
| 4 | 0.559893 | `mysql_server_list` | ❌ |
| 5 | 0.540088 | `sql_db_list` | ❌ |

---

## Test 421

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `sql_server_show` | ❌ |
| 2 | 0.610507 | `postgres_server_config_get` | ❌ |
| 3 | 0.538034 | `mysql_server_config_get` | ❌ |
| 4 | 0.471541 | `sql_db_show` | ❌ |
| 5 | 0.445430 | `postgres_server_param_get` | ❌ |

---

## Test 422

**Expected Tool:** `azmcp_sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `sql_server_show` | ❌ |
| 2 | 0.392532 | `postgres_server_config_get` | ❌ |
| 3 | 0.380021 | `postgres_server_param_get` | ❌ |
| 4 | 0.372194 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.370539 | `sql_db_show` | ❌ |

---

## Test 423

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `storage_account_create` | ❌ |
| 2 | 0.438046 | `storage_blob_container_create` | ❌ |
| 3 | 0.418191 | `storage_account_get` | ❌ |
| 4 | 0.414518 | `storage_blob_container_get` | ❌ |
| 5 | 0.370957 | `managedlustre_filesystem_create` | ❌ |

---

## Test 424

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `storage_account_create` | ❌ |
| 2 | 0.483202 | `managedlustre_filesystem_create` | ❌ |
| 3 | 0.407222 | `storage_account_get` | ❌ |
| 4 | 0.406804 | `storage_blob_container_create` | ❌ |
| 5 | 0.400179 | `managedlustre_filesystem_sku_get` | ❌ |

---

## Test 425

**Expected Tool:** `azmcp_storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589003 | `storage_account_create` | ❌ |
| 2 | 0.535501 | `managedlustre_filesystem_create` | ❌ |
| 3 | 0.509731 | `storage_blob_container_create` | ❌ |
| 4 | 0.462519 | `storage_account_get` | ❌ |
| 5 | 0.447156 | `sql_db_create` | ❌ |

---

## Test 426

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.673749 | `storage_account_get` | ❌ |
| 2 | 0.608256 | `storage_blob_container_get` | ❌ |
| 3 | 0.556457 | `storage_blob_get` | ❌ |
| 4 | 0.483435 | `storage_account_create` | ❌ |
| 5 | 0.439236 | `cosmos_account_list` | ❌ |

---

## Test 427

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.692687 | `storage_account_get` | ❌ |
| 2 | 0.577547 | `storage_blob_container_get` | ❌ |
| 3 | 0.529205 | `storage_blob_get` | ❌ |
| 4 | 0.518215 | `storage_account_create` | ❌ |
| 5 | 0.448507 | `storage_blob_container_create` | ❌ |

---

## Test 428

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649215 | `storage_account_get` | ❌ |
| 2 | 0.557022 | `managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.550148 | `storage_blob_container_get` | ❌ |
| 4 | 0.547647 | `subscription_list` | ❌ |
| 5 | 0.536909 | `cosmos_account_list` | ❌ |

---

## Test 429

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.556860 | `storage_account_get` | ❌ |
| 2 | 0.482418 | `storage_blob_container_get` | ❌ |
| 3 | 0.461284 | `managedlustre_filesystem_list` | ❌ |
| 4 | 0.421642 | `cosmos_account_list` | ❌ |
| 5 | 0.410587 | `storage_blob_get` | ❌ |

---

## Test 430

**Expected Tool:** `azmcp_storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619462 | `storage_account_get` | ❌ |
| 2 | 0.556436 | `storage_blob_container_get` | ❌ |
| 3 | 0.518229 | `storage_blob_get` | ❌ |
| 4 | 0.473598 | `cosmos_account_list` | ❌ |
| 5 | 0.465571 | `subscription_list` | ❌ |

---

## Test 431

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649793 | `storage_blob_container_create` | ❌ |
| 2 | 0.583896 | `storage_blob_container_get` | ❌ |
| 3 | 0.524779 | `storage_account_create` | ❌ |
| 4 | 0.496679 | `storage_blob_get` | ❌ |
| 5 | 0.447784 | `cosmos_database_container_list` | ❌ |

---

## Test 432

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682161 | `storage_blob_container_create` | ❌ |
| 2 | 0.590160 | `storage_blob_container_get` | ❌ |
| 3 | 0.559263 | `storage_blob_get` | ❌ |
| 4 | 0.500624 | `storage_account_create` | ❌ |
| 5 | 0.420514 | `storage_account_get` | ❌ |

---

## Test 433

**Expected Tool:** `azmcp_storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625397 | `storage_blob_container_create` | ❌ |
| 2 | 0.543503 | `storage_blob_container_get` | ❌ |
| 3 | 0.497804 | `storage_blob_get` | ❌ |
| 4 | 0.463198 | `storage_account_create` | ❌ |
| 5 | 0.435099 | `cosmos_database_container_list` | ❌ |

---

## Test 434

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701642 | `storage_blob_container_get` | ❌ |
| 2 | 0.623681 | `storage_blob_get` | ❌ |
| 3 | 0.577921 | `storage_account_get` | ❌ |
| 4 | 0.549803 | `storage_blob_container_create` | ❌ |
| 5 | 0.523288 | `cosmos_database_container_list` | ❌ |

---

## Test 435

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712037 | `storage_blob_container_get` | ❌ |
| 2 | 0.680802 | `storage_blob_get` | ❌ |
| 3 | 0.613933 | `cosmos_database_container_list` | ❌ |
| 4 | 0.556319 | `storage_blob_container_create` | ❌ |
| 5 | 0.518266 | `storage_account_get` | ❌ |

---

## Test 436

**Expected Tool:** `azmcp_storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713527 | `storage_blob_container_get` | ❌ |
| 2 | 0.592373 | `cosmos_database_container_list` | ❌ |
| 3 | 0.586169 | `storage_blob_get` | ❌ |
| 4 | 0.523322 | `storage_account_get` | ❌ |
| 5 | 0.487521 | `storage_blob_container_create` | ❌ |

---

## Test 437

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.700973 | `storage_blob_get` | ❌ |
| 2 | 0.646973 | `storage_blob_container_get` | ❌ |
| 3 | 0.541019 | `storage_blob_container_create` | ❌ |
| 4 | 0.527427 | `storage_account_get` | ❌ |
| 5 | 0.477946 | `cosmos_database_container_list` | ❌ |

---

## Test 438

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694997 | `storage_blob_get` | ❌ |
| 2 | 0.630994 | `storage_blob_container_get` | ❌ |
| 3 | 0.589004 | `storage_blob_container_create` | ❌ |
| 4 | 0.579960 | `storage_account_get` | ❌ |
| 5 | 0.457064 | `storage_account_create` | ❌ |

---

## Test 439

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733586 | `storage_blob_get` | ❌ |
| 2 | 0.700891 | `storage_blob_container_get` | ❌ |
| 3 | 0.605993 | `storage_blob_container_create` | ❌ |
| 4 | 0.579070 | `cosmos_database_container_list` | ❌ |
| 5 | 0.506393 | `cosmos_database_container_item_query` | ❌ |

---

## Test 440

**Expected Tool:** `azmcp_storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.704426 | `storage_blob_get` | ❌ |
| 2 | 0.664940 | `storage_blob_container_get` | ❌ |
| 3 | 0.561557 | `storage_blob_container_create` | ❌ |
| 4 | 0.533515 | `cosmos_database_container_list` | ❌ |
| 5 | 0.484018 | `storage_account_get` | ❌ |

---

## Test 441

**Expected Tool:** `azmcp_storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.566287 | `storage_blob_upload` | ❌ |
| 2 | 0.525674 | `storage_blob_container_create` | ❌ |
| 3 | 0.517616 | `storage_blob_get` | ❌ |
| 4 | 0.473645 | `storage_blob_container_get` | ❌ |
| 5 | 0.382123 | `storage_account_create` | ❌ |

---

## Test 442

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654071 | `subscription_list` | ❌ |
| 2 | 0.512964 | `cosmos_account_list` | ❌ |
| 3 | 0.471653 | `postgres_server_list` | ❌ |
| 4 | 0.469023 | `kusto_cluster_list` | ❌ |
| 5 | 0.461078 | `redis_list` | ❌ |

---

## Test 443

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.458821 | `subscription_list` | ❌ |
| 2 | 0.407471 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.393695 | `eventgrid_topic_list` | ❌ |
| 4 | 0.391555 | `redis_list` | ❌ |
| 5 | 0.381238 | `postgres_server_list` | ❌ |

---

## Test 444

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433196 | `subscription_list` | ❌ |
| 2 | 0.319379 | `marketplace_product_list` | ❌ |
| 3 | 0.315547 | `marketplace_product_get` | ❌ |
| 4 | 0.293772 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.289334 | `eventgrid_topic_list` | ❌ |

---

## Test 445

**Expected Tool:** `azmcp_subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.477592 | `subscription_list` | ❌ |
| 2 | 0.357625 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.354013 | `marketplace_product_list` | ❌ |
| 4 | 0.344549 | `redis_list` | ❌ |
| 5 | 0.340837 | `eventgrid_topic_list` | ❌ |

---

## Test 446

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686911 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.625270 | `deploy_iac_rules_get` | ❌ |
| 3 | 0.605047 | `get_bestpractices_get` | ❌ |
| 4 | 0.482936 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466144 | `deploy_plan_get` | ❌ |

---

## Test 447

**Expected Tool:** `azmcp_azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581277 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.512141 | `get_bestpractices_get` | ❌ |
| 3 | 0.510004 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.473596 | `keyvault_secret_get` | ❌ |
| 5 | 0.444297 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 448

**Expected Tool:** `azmcp_virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711969 | `virtualdesktop_hostpool_list` | ❌ |
| 2 | 0.659763 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.620666 | `kusto_cluster_list` | ❌ |
| 4 | 0.548888 | `search_service_list` | ❌ |
| 5 | 0.535739 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |

---

## Test 449

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.727054 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 2 | 0.714469 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 3 | 0.573352 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.438687 | `aks_nodepool_get` | ❌ |
| 5 | 0.393721 | `sql_elastic-pool_list` | ❌ |

---

## Test 450

**Expected Tool:** `azmcp_virtualdesktop_hostpool_sessionhost_usersession-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.812659 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 2 | 0.659212 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501167 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.357614 | `aks_nodepool_get` | ❌ |
| 5 | 0.336385 | `monitor_workspace_list` | ❌ |

---

## Test 451

**Expected Tool:** `azmcp_workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552487 | `workbooks_create` | ❌ |
| 2 | 0.417950 | `workbooks_update` | ❌ |
| 3 | 0.361364 | `workbooks_delete` | ❌ |
| 4 | 0.329118 | `workbooks_show` | ❌ |
| 5 | 0.328113 | `workbooks_list` | ❌ |

---

## Test 452

**Expected Tool:** `azmcp_workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621310 | `workbooks_delete` | ❌ |
| 2 | 0.498518 | `workbooks_show` | ❌ |
| 3 | 0.432632 | `workbooks_create` | ❌ |
| 4 | 0.425569 | `workbooks_list` | ❌ |
| 5 | 0.421897 | `workbooks_update` | ❌ |

---

## Test 453

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772431 | `workbooks_list` | ❌ |
| 2 | 0.562643 | `workbooks_create` | ❌ |
| 3 | 0.516739 | `grafana_list` | ❌ |
| 4 | 0.494073 | `workbooks_show` | ❌ |
| 5 | 0.488600 | `group_list` | ❌ |

---

## Test 454

**Expected Tool:** `azmcp_workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `workbooks_list` | ❌ |
| 2 | 0.570444 | `workbooks_create` | ❌ |
| 3 | 0.499716 | `workbooks_show` | ❌ |
| 4 | 0.485504 | `workbooks_delete` | ❌ |
| 5 | 0.472378 | `grafana_list` | ❌ |

---

## Test 455

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686087 | `workbooks_show` | ❌ |
| 2 | 0.498464 | `workbooks_create` | ❌ |
| 3 | 0.494708 | `workbooks_list` | ❌ |
| 4 | 0.463156 | `workbooks_update` | ❌ |
| 5 | 0.452348 | `workbooks_delete` | ❌ |

---

## Test 456

**Expected Tool:** `azmcp_workbooks_show`  
**Prompt:** Show me the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581501 | `workbooks_show` | ❌ |
| 2 | 0.500475 | `workbooks_list` | ❌ |
| 3 | 0.469206 | `workbooks_create` | ❌ |
| 4 | 0.466266 | `workbooks_update` | ❌ |
| 5 | 0.455311 | `workbooks_delete` | ❌ |

---

## Test 457

**Expected Tool:** `azmcp_workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586347 | `workbooks_update` | ❌ |
| 2 | 0.382880 | `workbooks_create` | ❌ |
| 3 | 0.349689 | `workbooks_delete` | ❌ |
| 4 | 0.347944 | `workbooks_show` | ❌ |
| 5 | 0.292904 | `loadtesting_testrun_update` | ❌ |

---

## Test 458

**Expected Tool:** `azmcp_bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543154 | `bicepschema_get` | ❌ |
| 2 | 0.485970 | `foundry_models_deploy` | ❌ |
| 3 | 0.485889 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.462146 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.448373 | `get_bestpractices_get` | ❌ |

---

## Test 459

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502125 | `cloudarchitect_design` | ❌ |
| 2 | 0.290902 | `storage_blob_upload` | ❌ |
| 3 | 0.259162 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.254991 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.245034 | `managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 460

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** Help me create a cloud service that will serve as ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.405148 | `cloudarchitect_design` | ❌ |
| 2 | 0.267683 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.258160 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.242597 | `extension_cli_generate` | ❌ |
| 5 | 0.225870 | `foundry_models_deploy` | ❌ |

---

## Test 461

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423577 | `cloudarchitect_design` | ❌ |
| 2 | 0.271943 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242633 | `deploy_plan_get` | ❌ |
| 5 | 0.229000 | `extension_cli_generate` | ❌ |

---

## Test 462

**Expected Tool:** `azmcp_cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534690 | `cloudarchitect_design` | ❌ |
| 2 | 0.369969 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.356331 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.352797 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.323920 | `storage_blob_upload` | ❌ |

---

## Summary

**Total Prompts Tested:** 462  
**Analysis Execution Time:** 91.8625323s  

### Success Rate Metrics

**Top Choice Success:** 0.0% (0/462 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 0.0% (0/462 tests)  
**🎯 High Confidence (≥0.7):** 0.0% (0/462 tests)  
**✅ Good Confidence (≥0.6):** 0.0% (0/462 tests)  
**👍 Fair Confidence (≥0.5):** 0.0% (0/462 tests)  
**👌 Acceptable Confidence (≥0.4):** 0.0% (0/462 tests)  
**❌ Low Confidence (<0.4):** 100.0% (462/462 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 0.0% (0/462 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 0.0% (0/462 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 0.0% (0/462 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 0.0% (0/462 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 0.0% (0/462 tests)  

### Success Rate Analysis

🔴 **Poor** - The tool selection system requires major improvements.

🔧 **Recommendation:** Significant improvements needed in tool descriptions for better semantic matching.

