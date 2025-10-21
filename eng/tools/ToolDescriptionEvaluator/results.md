# Tool Selection Analysis Setup

<<<<<<< HEAD
**Setup completed:** 2025-11-06 17:16:26  
**Tool count:** 179  
**Database setup time:** 32.4934401s  
=======
<<<<<<< HEAD
**Setup completed:** 2025-11-03 14:57:47  
**Tool count:** 173  
**Database setup time:** 1.2016078s  
=======
**Setup completed:** 2025-11-04 15:41:36  
**Tool count:** 174  
**Database setup time:** 1.4888934s  
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

# Tool Selection Analysis Results

<<<<<<< HEAD
**Analysis Date:** 2025-11-06 17:16:26  
**Tool count:** 179  
=======
<<<<<<< HEAD
**Analysis Date:** 2025-11-03 14:57:47  
**Tool count:** 173  
=======
**Analysis Date:** 2025-11-04 15:41:36  
**Tool count:** 174  
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

## Table of Contents

- [Test 1: foundry_agents_connect](#test-1)
- [Test 2: foundry_agents_evaluate](#test-2)
- [Test 3: foundry_agents_list](#test-3)
- [Test 4: foundry_agents_list](#test-4)
- [Test 5: foundry_agents_create](#test-5)
- [Test 6: foundry_agents_query-and-evaluate](#test-6)
- [Test 7: foundry_agents_get-sdk-sample](#test-7)
- [Test 8: foundry_threads_create](#test-8)
- [Test 9: foundry_threads_list](#test-9)
- [Test 10: foundry_threads_get-messages](#test-10)
- [Test 11: foundry_knowledge_index_list](#test-11)
- [Test 12: foundry_knowledge_index_list](#test-12)
- [Test 13: foundry_knowledge_index_schema](#test-13)
- [Test 14: foundry_knowledge_index_schema](#test-14)
- [Test 15: foundry_models_deploy](#test-15)
- [Test 16: foundry_models_deployments_list](#test-16)
- [Test 17: foundry_models_deployments_list](#test-17)
- [Test 18: foundry_models_list](#test-18)
- [Test 19: foundry_models_list](#test-19)
- [Test 20: foundry_openai_chat-completions-create](#test-20)
- [Test 21: foundry_openai_create-completion](#test-21)
- [Test 22: foundry_openai_embeddings-create](#test-22)
- [Test 23: foundry_openai_embeddings-create](#test-23)
- [Test 24: foundry_openai_models-list](#test-24)
- [Test 25: foundry_openai_models-list](#test-25)
- [Test 26: foundry_resource_get](#test-26)
- [Test 27: foundry_resource_get](#test-27)
- [Test 28: foundry_resource_get](#test-28)
- [Test 29: search_knowledge_base_get](#test-29)
- [Test 30: search_knowledge_base_get](#test-30)
- [Test 31: search_knowledge_base_get](#test-31)
- [Test 32: search_knowledge_base_get](#test-32)
- [Test 33: search_knowledge_base_get](#test-33)
- [Test 34: search_knowledge_base_get](#test-34)
- [Test 35: search_knowledge_base_retrieve](#test-35)
- [Test 36: search_knowledge_base_retrieve](#test-36)
- [Test 37: search_knowledge_base_retrieve](#test-37)
- [Test 38: search_knowledge_base_retrieve](#test-38)
- [Test 39: search_knowledge_base_retrieve](#test-39)
- [Test 40: search_knowledge_base_retrieve](#test-40)
- [Test 41: search_knowledge_base_retrieve](#test-41)
- [Test 42: search_knowledge_base_retrieve](#test-42)
- [Test 43: search_knowledge_source_get](#test-43)
- [Test 44: search_knowledge_source_get](#test-44)
- [Test 45: search_knowledge_source_get](#test-45)
- [Test 46: search_knowledge_source_get](#test-46)
- [Test 47: search_knowledge_source_get](#test-47)
- [Test 48: search_knowledge_source_get](#test-48)
- [Test 49: search_index_get](#test-49)
- [Test 50: search_index_get](#test-50)
- [Test 51: search_index_get](#test-51)
- [Test 52: search_index_query](#test-52)
- [Test 53: search_service_list](#test-53)
- [Test 54: search_service_list](#test-54)
- [Test 55: search_service_list](#test-55)
- [Test 56: speech_stt_recognize](#test-56)
- [Test 57: speech_stt_recognize](#test-57)
- [Test 58: speech_stt_recognize](#test-58)
- [Test 59: speech_stt_recognize](#test-59)
- [Test 60: speech_stt_recognize](#test-60)
<<<<<<< HEAD
- [Test 61: speech_stt_recognize](#test-61)
- [Test 62: speech_stt_recognize](#test-62)
- [Test 63: speech_stt_recognize](#test-63)
- [Test 64: speech_stt_recognize](#test-64)
- [Test 65: speech_stt_recognize](#test-65)
- [Test 66: appconfig_account_list](#test-66)
- [Test 67: appconfig_account_list](#test-67)
- [Test 68: appconfig_account_list](#test-68)
- [Test 69: appconfig_kv_delete](#test-69)
- [Test 70: appconfig_kv_get](#test-70)
- [Test 71: appconfig_kv_get](#test-71)
- [Test 72: appconfig_kv_get](#test-72)
- [Test 73: appconfig_kv_get](#test-73)
- [Test 74: appconfig_kv_lock_set](#test-74)
- [Test 75: appconfig_kv_lock_set](#test-75)
- [Test 76: appconfig_kv_set](#test-76)
- [Test 77: applens_resource_diagnose](#test-77)
- [Test 78: applens_resource_diagnose](#test-78)
- [Test 79: applens_resource_diagnose](#test-79)
=======
<<<<<<< HEAD
- [Test 61: appconfig_account_list](#test-61)
- [Test 62: appconfig_account_list](#test-62)
- [Test 63: appconfig_account_list](#test-63)
- [Test 64: appconfig_kv_delete](#test-64)
- [Test 65: appconfig_kv_get](#test-65)
- [Test 66: appconfig_kv_get](#test-66)
- [Test 67: appconfig_kv_get](#test-67)
- [Test 68: appconfig_kv_get](#test-68)
- [Test 69: appconfig_kv_lock_set](#test-69)
- [Test 70: appconfig_kv_lock_set](#test-70)
- [Test 71: appconfig_kv_set](#test-71)
- [Test 72: applens_resource_diagnose](#test-72)
- [Test 73: applens_resource_diagnose](#test-73)
- [Test 74: applens_resource_diagnose](#test-74)
- [Test 75: appservice_database_add](#test-75)
- [Test 76: appservice_database_add](#test-76)
- [Test 77: appservice_database_add](#test-77)
- [Test 78: appservice_database_add](#test-78)
- [Test 79: appservice_database_add](#test-79)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
- [Test 80: appservice_database_add](#test-80)
- [Test 81: appservice_database_add](#test-81)
- [Test 82: appservice_database_add](#test-82)
- [Test 83: appservice_database_add](#test-83)
- [Test 84: appservice_database_add](#test-84)
- [Test 85: appservice_database_add](#test-85)
- [Test 86: appservice_database_add](#test-86)
- [Test 87: appservice_database_add](#test-87)
- [Test 88: appservice_database_add](#test-88)
- [Test 89: appservice_database_add](#test-89)
- [Test 90: applicationinsights_recommendation_list](#test-90)
- [Test 91: applicationinsights_recommendation_list](#test-91)
- [Test 92: applicationinsights_recommendation_list](#test-92)
- [Test 93: applicationinsights_recommendation_list](#test-93)
- [Test 94: extension_cli_generate](#test-94)
- [Test 95: extension_cli_generate](#test-95)
- [Test 96: extension_cli_generate](#test-96)
- [Test 97: extension_cli_install](#test-97)
- [Test 98: extension_cli_install](#test-98)
- [Test 99: extension_cli_install](#test-99)
- [Test 100: acr_registry_list](#test-100)
- [Test 101: acr_registry_list](#test-101)
- [Test 102: acr_registry_list](#test-102)
- [Test 103: acr_registry_list](#test-103)
- [Test 104: acr_registry_list](#test-104)
- [Test 105: acr_registry_repository_list](#test-105)
- [Test 106: acr_registry_repository_list](#test-106)
- [Test 107: acr_registry_repository_list](#test-107)
- [Test 108: acr_registry_repository_list](#test-108)
- [Test 109: communication_email_send](#test-109)
- [Test 110: communication_email_send](#test-110)
- [Test 111: communication_email_send](#test-111)
- [Test 112: communication_email_send](#test-112)
- [Test 113: communication_email_send](#test-113)
- [Test 114: communication_email_send](#test-114)
- [Test 115: communication_email_send](#test-115)
- [Test 116: communication_email_send](#test-116)
- [Test 117: communication_sms_send](#test-117)
- [Test 118: communication_sms_send](#test-118)
- [Test 119: communication_sms_send](#test-119)
- [Test 120: communication_sms_send](#test-120)
- [Test 121: communication_sms_send](#test-121)
- [Test 122: communication_sms_send](#test-122)
- [Test 123: communication_sms_send](#test-123)
- [Test 124: communication_sms_send](#test-124)
- [Test 125: confidentialledger_entries_append](#test-125)
- [Test 126: confidentialledger_entries_append](#test-126)
- [Test 127: confidentialledger_entries_append](#test-127)
- [Test 128: confidentialledger_entries_append](#test-128)
- [Test 129: confidentialledger_entries_append](#test-129)
- [Test 130: confidentialledger_entries_get](#test-130)
- [Test 131: confidentialledger_entries_get](#test-131)
- [Test 132: cosmos_account_list](#test-132)
- [Test 133: cosmos_account_list](#test-133)
- [Test 134: cosmos_account_list](#test-134)
- [Test 135: cosmos_database_container_item_query](#test-135)
- [Test 136: cosmos_database_container_list](#test-136)
- [Test 137: cosmos_database_container_list](#test-137)
- [Test 138: cosmos_database_list](#test-138)
- [Test 139: cosmos_database_list](#test-139)
- [Test 140: kusto_cluster_get](#test-140)
- [Test 141: kusto_cluster_list](#test-141)
- [Test 142: kusto_cluster_list](#test-142)
- [Test 143: kusto_cluster_list](#test-143)
- [Test 144: kusto_database_list](#test-144)
- [Test 145: kusto_database_list](#test-145)
- [Test 146: kusto_query](#test-146)
- [Test 147: kusto_sample](#test-147)
- [Test 148: kusto_table_list](#test-148)
- [Test 149: kusto_table_list](#test-149)
- [Test 150: kusto_table_schema](#test-150)
- [Test 151: mysql_database_list](#test-151)
- [Test 152: mysql_database_list](#test-152)
- [Test 153: mysql_database_query](#test-153)
- [Test 154: mysql_server_config_get](#test-154)
- [Test 155: mysql_server_list](#test-155)
- [Test 156: mysql_server_list](#test-156)
- [Test 157: mysql_server_list](#test-157)
- [Test 158: mysql_server_param_get](#test-158)
- [Test 159: mysql_server_param_set](#test-159)
- [Test 160: mysql_table_list](#test-160)
- [Test 161: mysql_table_list](#test-161)
- [Test 162: mysql_table_schema_get](#test-162)
- [Test 163: postgres_database_list](#test-163)
- [Test 164: postgres_database_list](#test-164)
- [Test 165: postgres_database_query](#test-165)
- [Test 166: postgres_server_config_get](#test-166)
- [Test 167: postgres_server_list](#test-167)
- [Test 168: postgres_server_list](#test-168)
- [Test 169: postgres_server_list](#test-169)
- [Test 170: postgres_server_param_get](#test-170)
- [Test 171: postgres_server_param_set](#test-171)
- [Test 172: postgres_table_list](#test-172)
- [Test 173: postgres_table_list](#test-173)
- [Test 174: postgres_table_schema_get](#test-174)
- [Test 175: deploy_app_logs_get](#test-175)
- [Test 176: deploy_architecture_diagram_generate](#test-176)
- [Test 177: deploy_iac_rules_get](#test-177)
- [Test 178: deploy_pipeline_guidance_get](#test-178)
- [Test 179: deploy_plan_get](#test-179)
- [Test 180: eventgrid_events_publish](#test-180)
- [Test 181: eventgrid_events_publish](#test-181)
- [Test 182: eventgrid_events_publish](#test-182)
- [Test 183: eventgrid_topic_list](#test-183)
- [Test 184: eventgrid_topic_list](#test-184)
- [Test 185: eventgrid_topic_list](#test-185)
- [Test 186: eventgrid_topic_list](#test-186)
- [Test 187: eventgrid_subscription_list](#test-187)
- [Test 188: eventgrid_subscription_list](#test-188)
- [Test 189: eventgrid_subscription_list](#test-189)
- [Test 190: eventgrid_subscription_list](#test-190)
- [Test 191: eventgrid_subscription_list](#test-191)
- [Test 192: eventgrid_subscription_list](#test-192)
- [Test 193: eventgrid_subscription_list](#test-193)
- [Test 194: eventhubs_eventhub_consumergroup_delete](#test-194)
- [Test 195: eventhubs_eventhub_consumergroup_get](#test-195)
- [Test 196: eventhubs_eventhub_consumergroup_get](#test-196)
- [Test 197: eventhubs_eventhub_consumergroup_update](#test-197)
- [Test 198: eventhubs_eventhub_consumergroup_update](#test-198)
- [Test 199: eventhubs_eventhub_delete](#test-199)
- [Test 200: eventhubs_eventhub_get](#test-200)
- [Test 201: eventhubs_eventhub_get](#test-201)
- [Test 202: eventhubs_eventhub_update](#test-202)
- [Test 203: eventhubs_eventhub_update](#test-203)
- [Test 204: eventhubs_namespace_delete](#test-204)
- [Test 205: eventhubs_namespace_get](#test-205)
- [Test 206: eventhubs_namespace_get](#test-206)
- [Test 207: eventhubs_namespace_update](#test-207)
- [Test 208: eventhubs_namespace_update](#test-208)
- [Test 209: functionapp_get](#test-209)
- [Test 210: functionapp_get](#test-210)
- [Test 211: functionapp_get](#test-211)
- [Test 212: functionapp_get](#test-212)
- [Test 213: functionapp_get](#test-213)
- [Test 214: functionapp_get](#test-214)
- [Test 215: functionapp_get](#test-215)
- [Test 216: functionapp_get](#test-216)
- [Test 217: functionapp_get](#test-217)
- [Test 218: functionapp_get](#test-218)
- [Test 219: functionapp_get](#test-219)
- [Test 220: functionapp_get](#test-220)
- [Test 221: keyvault_admin_settings_get](#test-221)
- [Test 222: keyvault_admin_settings_get](#test-222)
- [Test 223: keyvault_admin_settings_get](#test-223)
- [Test 224: keyvault_certificate_create](#test-224)
- [Test 225: keyvault_certificate_create](#test-225)
- [Test 226: keyvault_certificate_create](#test-226)
- [Test 227: keyvault_certificate_create](#test-227)
- [Test 228: keyvault_certificate_create](#test-228)
- [Test 229: keyvault_certificate_get](#test-229)
- [Test 230: keyvault_certificate_get](#test-230)
- [Test 231: keyvault_certificate_get](#test-231)
- [Test 232: keyvault_certificate_get](#test-232)
- [Test 233: keyvault_certificate_get](#test-233)
- [Test 234: keyvault_certificate_import](#test-234)
- [Test 235: keyvault_certificate_import](#test-235)
- [Test 236: keyvault_certificate_import](#test-236)
- [Test 237: keyvault_certificate_import](#test-237)
- [Test 238: keyvault_certificate_import](#test-238)
- [Test 239: keyvault_certificate_list](#test-239)
- [Test 240: keyvault_certificate_list](#test-240)
- [Test 241: keyvault_certificate_list](#test-241)
- [Test 242: keyvault_certificate_list](#test-242)
- [Test 243: keyvault_certificate_list](#test-243)
- [Test 244: keyvault_certificate_list](#test-244)
- [Test 245: keyvault_key_create](#test-245)
- [Test 246: keyvault_key_create](#test-246)
- [Test 247: keyvault_key_create](#test-247)
- [Test 248: keyvault_key_create](#test-248)
- [Test 249: keyvault_key_create](#test-249)
- [Test 250: keyvault_key_get](#test-250)
- [Test 251: keyvault_key_get](#test-251)
- [Test 252: keyvault_key_get](#test-252)
- [Test 253: keyvault_key_get](#test-253)
- [Test 254: keyvault_key_get](#test-254)
- [Test 255: keyvault_key_list](#test-255)
- [Test 256: keyvault_key_list](#test-256)
- [Test 257: keyvault_key_list](#test-257)
- [Test 258: keyvault_key_list](#test-258)
- [Test 259: keyvault_key_list](#test-259)
- [Test 260: keyvault_key_list](#test-260)
- [Test 261: keyvault_secret_create](#test-261)
- [Test 262: keyvault_secret_create](#test-262)
- [Test 263: keyvault_secret_create](#test-263)
- [Test 264: keyvault_secret_create](#test-264)
- [Test 265: keyvault_secret_create](#test-265)
- [Test 266: keyvault_secret_get](#test-266)
- [Test 267: keyvault_secret_get](#test-267)
- [Test 268: keyvault_secret_get](#test-268)
- [Test 269: keyvault_secret_get](#test-269)
- [Test 270: keyvault_secret_get](#test-270)
- [Test 271: keyvault_secret_list](#test-271)
- [Test 272: keyvault_secret_list](#test-272)
- [Test 273: keyvault_secret_list](#test-273)
- [Test 274: keyvault_secret_list](#test-274)
- [Test 275: keyvault_secret_list](#test-275)
- [Test 276: keyvault_secret_list](#test-276)
- [Test 277: aks_cluster_get](#test-277)
- [Test 278: aks_cluster_get](#test-278)
- [Test 279: aks_cluster_get](#test-279)
- [Test 280: aks_cluster_get](#test-280)
- [Test 281: aks_cluster_get](#test-281)
- [Test 282: aks_cluster_get](#test-282)
- [Test 283: aks_cluster_get](#test-283)
- [Test 284: aks_nodepool_get](#test-284)
<<<<<<< HEAD
- [Test 285: aks_nodepool_get](#test-285)
- [Test 286: aks_nodepool_get](#test-286)
- [Test 287: aks_nodepool_get](#test-287)
- [Test 288: aks_nodepool_get](#test-288)
- [Test 289: aks_nodepool_get](#test-289)
- [Test 290: loadtesting_test_create](#test-290)
- [Test 291: loadtesting_test_get](#test-291)
- [Test 292: loadtesting_testresource_create](#test-292)
- [Test 293: loadtesting_testresource_list](#test-293)
- [Test 294: loadtesting_testrun_create](#test-294)
- [Test 295: loadtesting_testrun_get](#test-295)
- [Test 296: loadtesting_testrun_list](#test-296)
- [Test 297: loadtesting_testrun_update](#test-297)
- [Test 298: grafana_list](#test-298)
- [Test 299: managedlustre_fs_create](#test-299)
- [Test 300: managedlustre_fs_list](#test-300)
- [Test 301: managedlustre_fs_list](#test-301)
- [Test 302: managedlustre_fs_sku_get](#test-302)
- [Test 303: managedlustre_fs_subnetsize_ask](#test-303)
- [Test 304: managedlustre_fs_subnetsize_validate](#test-304)
- [Test 305: managedlustre_fs_update](#test-305)
- [Test 306: marketplace_product_get](#test-306)
- [Test 307: marketplace_product_list](#test-307)
- [Test 308: marketplace_product_list](#test-308)
- [Test 309: azureaibestpractices_get](#test-309)
- [Test 310: azureaibestpractices_get](#test-310)
- [Test 311: azureaibestpractices_get](#test-311)
- [Test 312: azureaibestpractices_get](#test-312)
- [Test 313: azureaibestpractices_get](#test-313)
=======
- [Test 285: loadtesting_test_create](#test-285)
- [Test 286: loadtesting_test_get](#test-286)
- [Test 287: loadtesting_testresource_create](#test-287)
- [Test 288: loadtesting_testresource_list](#test-288)
- [Test 289: loadtesting_testrun_create](#test-289)
- [Test 290: loadtesting_testrun_get](#test-290)
- [Test 291: loadtesting_testrun_list](#test-291)
- [Test 292: loadtesting_testrun_update](#test-292)
- [Test 293: grafana_list](#test-293)
- [Test 294: managedlustre_fs_create](#test-294)
- [Test 295: managedlustre_fs_list](#test-295)
- [Test 296: managedlustre_fs_list](#test-296)
- [Test 297: managedlustre_fs_sku_get](#test-297)
- [Test 298: managedlustre_fs_subnetsize_ask](#test-298)
- [Test 299: managedlustre_fs_subnetsize_validate](#test-299)
- [Test 300: managedlustre_fs_update](#test-300)
- [Test 301: marketplace_product_get](#test-301)
- [Test 302: marketplace_product_list](#test-302)
- [Test 303: marketplace_product_list](#test-303)
- [Test 304: get_bestpractices_get](#test-304)
- [Test 305: get_bestpractices_get](#test-305)
- [Test 306: get_bestpractices_get](#test-306)
- [Test 307: get_bestpractices_get](#test-307)
- [Test 308: get_bestpractices_get](#test-308)
- [Test 309: get_bestpractices_get](#test-309)
- [Test 310: get_bestpractices_get](#test-310)
- [Test 311: get_bestpractices_get](#test-311)
- [Test 312: get_bestpractices_get](#test-312)
- [Test 313: monitor_activitylog_list](#test-313)
- [Test 314: monitor_healthmodels_entity_get](#test-314)
- [Test 315: monitor_metrics_definitions](#test-315)
- [Test 316: monitor_metrics_definitions](#test-316)
- [Test 317: monitor_metrics_definitions](#test-317)
- [Test 318: monitor_metrics_query](#test-318)
- [Test 319: monitor_metrics_query](#test-319)
- [Test 320: monitor_metrics_query](#test-320)
- [Test 321: monitor_metrics_query](#test-321)
- [Test 322: monitor_metrics_query](#test-322)
- [Test 323: monitor_metrics_query](#test-323)
- [Test 324: monitor_resource_log_query](#test-324)
- [Test 325: monitor_table_list](#test-325)
- [Test 326: monitor_table_list](#test-326)
- [Test 327: monitor_table_type_list](#test-327)
- [Test 328: monitor_table_type_list](#test-328)
- [Test 329: monitor_webtests_create](#test-329)
- [Test 330: monitor_webtests_get](#test-330)
- [Test 331: monitor_webtests_list](#test-331)
- [Test 332: monitor_webtests_list](#test-332)
- [Test 333: monitor_webtests_update](#test-333)
- [Test 334: monitor_workspace_list](#test-334)
- [Test 335: monitor_workspace_list](#test-335)
- [Test 336: monitor_workspace_list](#test-336)
- [Test 337: monitor_workspace_log_query](#test-337)
- [Test 338: datadog_monitoredresources_list](#test-338)
- [Test 339: datadog_monitoredresources_list](#test-339)
- [Test 340: extension_azqr](#test-340)
- [Test 341: extension_azqr](#test-341)
- [Test 342: extension_azqr](#test-342)
- [Test 343: quota_region_availability_list](#test-343)
- [Test 344: quota_usage_check](#test-344)
- [Test 345: role_assignment_list](#test-345)
- [Test 346: role_assignment_list](#test-346)
- [Test 347: redis_list](#test-347)
- [Test 348: redis_list](#test-348)
- [Test 349: redis_list](#test-349)
- [Test 350: redis_list](#test-350)
- [Test 351: redis_list](#test-351)
- [Test 352: group_list](#test-352)
- [Test 353: group_list](#test-353)
- [Test 354: group_list](#test-354)
- [Test 355: resourcehealth_availability-status_get](#test-355)
- [Test 356: resourcehealth_availability-status_get](#test-356)
- [Test 357: resourcehealth_availability-status_get](#test-357)
- [Test 358: resourcehealth_availability-status_list](#test-358)
- [Test 359: resourcehealth_availability-status_list](#test-359)
- [Test 360: resourcehealth_availability-status_list](#test-360)
- [Test 361: resourcehealth_health-events_list](#test-361)
- [Test 362: resourcehealth_health-events_list](#test-362)
- [Test 363: resourcehealth_health-events_list](#test-363)
- [Test 364: resourcehealth_health-events_list](#test-364)
- [Test 365: resourcehealth_health-events_list](#test-365)
- [Test 366: servicebus_queue_details](#test-366)
- [Test 367: servicebus_topic_details](#test-367)
- [Test 368: servicebus_topic_subscription_details](#test-368)
- [Test 369: signalr_runtime_get](#test-369)
- [Test 370: signalr_runtime_get](#test-370)
- [Test 371: signalr_runtime_get](#test-371)
- [Test 372: signalr_runtime_get](#test-372)
- [Test 373: signalr_runtime_get](#test-373)
- [Test 374: signalr_runtime_get](#test-374)
- [Test 375: sql_db_create](#test-375)
- [Test 376: sql_db_create](#test-376)
- [Test 377: sql_db_create](#test-377)
- [Test 378: sql_db_delete](#test-378)
- [Test 379: sql_db_delete](#test-379)
- [Test 380: sql_db_delete](#test-380)
- [Test 381: sql_db_list](#test-381)
- [Test 382: sql_db_list](#test-382)
- [Test 383: sql_db_rename](#test-383)
- [Test 384: sql_db_rename](#test-384)
- [Test 385: sql_db_show](#test-385)
- [Test 386: sql_db_show](#test-386)
- [Test 387: sql_db_update](#test-387)
- [Test 388: sql_db_update](#test-388)
- [Test 389: sql_elastic-pool_list](#test-389)
- [Test 390: sql_elastic-pool_list](#test-390)
- [Test 391: sql_elastic-pool_list](#test-391)
- [Test 392: sql_server_create](#test-392)
- [Test 393: sql_server_create](#test-393)
- [Test 394: sql_server_create](#test-394)
- [Test 395: sql_server_delete](#test-395)
- [Test 396: sql_server_delete](#test-396)
- [Test 397: sql_server_delete](#test-397)
- [Test 398: sql_server_entra-admin_list](#test-398)
- [Test 399: sql_server_entra-admin_list](#test-399)
- [Test 400: sql_server_entra-admin_list](#test-400)
- [Test 401: sql_server_firewall-rule_create](#test-401)
- [Test 402: sql_server_firewall-rule_create](#test-402)
- [Test 403: sql_server_firewall-rule_create](#test-403)
- [Test 404: sql_server_firewall-rule_delete](#test-404)
- [Test 405: sql_server_firewall-rule_delete](#test-405)
- [Test 406: sql_server_firewall-rule_delete](#test-406)
- [Test 407: sql_server_firewall-rule_list](#test-407)
- [Test 408: sql_server_firewall-rule_list](#test-408)
- [Test 409: sql_server_firewall-rule_list](#test-409)
- [Test 410: sql_server_list](#test-410)
- [Test 411: sql_server_list](#test-411)
- [Test 412: sql_server_show](#test-412)
- [Test 413: sql_server_show](#test-413)
- [Test 414: sql_server_show](#test-414)
- [Test 415: storage_account_create](#test-415)
- [Test 416: storage_account_create](#test-416)
- [Test 417: storage_account_create](#test-417)
- [Test 418: storage_account_get](#test-418)
- [Test 419: storage_account_get](#test-419)
- [Test 420: storage_account_get](#test-420)
- [Test 421: storage_account_get](#test-421)
- [Test 422: storage_account_get](#test-422)
- [Test 423: storage_blob_container_create](#test-423)
- [Test 424: storage_blob_container_create](#test-424)
- [Test 425: storage_blob_container_create](#test-425)
- [Test 426: storage_blob_container_get](#test-426)
- [Test 427: storage_blob_container_get](#test-427)
- [Test 428: storage_blob_container_get](#test-428)
- [Test 429: storage_blob_get](#test-429)
- [Test 430: storage_blob_get](#test-430)
- [Test 431: storage_blob_get](#test-431)
- [Test 432: storage_blob_get](#test-432)
- [Test 433: storage_blob_upload](#test-433)
- [Test 434: subscription_list](#test-434)
- [Test 435: subscription_list](#test-435)
- [Test 436: subscription_list](#test-436)
- [Test 437: subscription_list](#test-437)
- [Test 438: azureterraformbestpractices_get](#test-438)
- [Test 439: azureterraformbestpractices_get](#test-439)
- [Test 440: virtualdesktop_hostpool_list](#test-440)
- [Test 441: virtualdesktop_hostpool_host_list](#test-441)
- [Test 442: virtualdesktop_hostpool_host_user-list](#test-442)
- [Test 443: workbooks_create](#test-443)
- [Test 444: workbooks_delete](#test-444)
- [Test 445: workbooks_list](#test-445)
- [Test 446: workbooks_list](#test-446)
- [Test 447: workbooks_show](#test-447)
- [Test 448: workbooks_show](#test-448)
- [Test 449: workbooks_update](#test-449)
- [Test 450: bicepschema_get](#test-450)
- [Test 451: cloudarchitect_design](#test-451)
- [Test 452: cloudarchitect_design](#test-452)
- [Test 453: cloudarchitect_design](#test-453)
- [Test 454: cloudarchitect_design](#test-454)
=======
- [Test 61: speech_tts_synthesize](#test-61)
- [Test 62: speech_tts_synthesize](#test-62)
- [Test 63: speech_tts_synthesize](#test-63)
- [Test 64: speech_tts_synthesize](#test-64)
- [Test 65: speech_tts_synthesize](#test-65)
- [Test 66: speech_tts_synthesize](#test-66)
- [Test 67: speech_tts_synthesize](#test-67)
- [Test 68: speech_tts_synthesize](#test-68)
- [Test 69: speech_tts_synthesize](#test-69)
- [Test 70: speech_tts_synthesize](#test-70)
- [Test 71: appconfig_account_list](#test-71)
- [Test 72: appconfig_account_list](#test-72)
- [Test 73: appconfig_account_list](#test-73)
- [Test 74: appconfig_kv_delete](#test-74)
- [Test 75: appconfig_kv_get](#test-75)
- [Test 76: appconfig_kv_get](#test-76)
- [Test 77: appconfig_kv_get](#test-77)
- [Test 78: appconfig_kv_get](#test-78)
- [Test 79: appconfig_kv_lock_set](#test-79)
- [Test 80: appconfig_kv_lock_set](#test-80)
- [Test 81: appconfig_kv_set](#test-81)
- [Test 82: applens_resource_diagnose](#test-82)
- [Test 83: applens_resource_diagnose](#test-83)
- [Test 84: applens_resource_diagnose](#test-84)
- [Test 85: appservice_database_add](#test-85)
- [Test 86: appservice_database_add](#test-86)
- [Test 87: appservice_database_add](#test-87)
- [Test 88: appservice_database_add](#test-88)
- [Test 89: appservice_database_add](#test-89)
- [Test 90: appservice_database_add](#test-90)
- [Test 91: appservice_database_add](#test-91)
- [Test 92: appservice_database_add](#test-92)
- [Test 93: appservice_database_add](#test-93)
- [Test 94: appservice_database_add](#test-94)
- [Test 95: applicationinsights_recommendation_list](#test-95)
- [Test 96: applicationinsights_recommendation_list](#test-96)
- [Test 97: applicationinsights_recommendation_list](#test-97)
- [Test 98: applicationinsights_recommendation_list](#test-98)
- [Test 99: extension_cli_generate](#test-99)
- [Test 100: extension_cli_generate](#test-100)
- [Test 101: extension_cli_generate](#test-101)
- [Test 102: extension_cli_install](#test-102)
- [Test 103: extension_cli_install](#test-103)
- [Test 104: extension_cli_install](#test-104)
- [Test 105: acr_registry_list](#test-105)
- [Test 106: acr_registry_list](#test-106)
- [Test 107: acr_registry_list](#test-107)
- [Test 108: acr_registry_list](#test-108)
- [Test 109: acr_registry_list](#test-109)
- [Test 110: acr_registry_repository_list](#test-110)
- [Test 111: acr_registry_repository_list](#test-111)
- [Test 112: acr_registry_repository_list](#test-112)
- [Test 113: acr_registry_repository_list](#test-113)
- [Test 114: communication_email_send](#test-114)
- [Test 115: communication_email_send](#test-115)
- [Test 116: communication_email_send](#test-116)
- [Test 117: communication_email_send](#test-117)
- [Test 118: communication_email_send](#test-118)
- [Test 119: communication_email_send](#test-119)
- [Test 120: communication_email_send](#test-120)
- [Test 121: communication_email_send](#test-121)
- [Test 122: communication_sms_send](#test-122)
- [Test 123: communication_sms_send](#test-123)
- [Test 124: communication_sms_send](#test-124)
- [Test 125: communication_sms_send](#test-125)
- [Test 126: communication_sms_send](#test-126)
- [Test 127: communication_sms_send](#test-127)
- [Test 128: communication_sms_send](#test-128)
- [Test 129: communication_sms_send](#test-129)
- [Test 130: confidentialledger_entries_append](#test-130)
- [Test 131: confidentialledger_entries_append](#test-131)
- [Test 132: confidentialledger_entries_append](#test-132)
- [Test 133: confidentialledger_entries_append](#test-133)
- [Test 134: confidentialledger_entries_append](#test-134)
- [Test 135: confidentialledger_entries_get](#test-135)
- [Test 136: confidentialledger_entries_get](#test-136)
- [Test 137: cosmos_account_list](#test-137)
- [Test 138: cosmos_account_list](#test-138)
- [Test 139: cosmos_account_list](#test-139)
- [Test 140: cosmos_database_container_item_query](#test-140)
- [Test 141: cosmos_database_container_list](#test-141)
- [Test 142: cosmos_database_container_list](#test-142)
- [Test 143: cosmos_database_list](#test-143)
- [Test 144: cosmos_database_list](#test-144)
- [Test 145: kusto_cluster_get](#test-145)
- [Test 146: kusto_cluster_list](#test-146)
- [Test 147: kusto_cluster_list](#test-147)
- [Test 148: kusto_cluster_list](#test-148)
- [Test 149: kusto_database_list](#test-149)
- [Test 150: kusto_database_list](#test-150)
- [Test 151: kusto_query](#test-151)
- [Test 152: kusto_sample](#test-152)
- [Test 153: kusto_table_list](#test-153)
- [Test 154: kusto_table_list](#test-154)
- [Test 155: kusto_table_schema](#test-155)
- [Test 156: mysql_database_list](#test-156)
- [Test 157: mysql_database_list](#test-157)
- [Test 158: mysql_database_query](#test-158)
- [Test 159: mysql_server_config_get](#test-159)
- [Test 160: mysql_server_list](#test-160)
- [Test 161: mysql_server_list](#test-161)
- [Test 162: mysql_server_list](#test-162)
- [Test 163: mysql_server_param_get](#test-163)
- [Test 164: mysql_server_param_set](#test-164)
- [Test 165: mysql_table_list](#test-165)
- [Test 166: mysql_table_list](#test-166)
- [Test 167: mysql_table_schema_get](#test-167)
- [Test 168: postgres_database_list](#test-168)
- [Test 169: postgres_database_list](#test-169)
- [Test 170: postgres_database_query](#test-170)
- [Test 171: postgres_server_config_get](#test-171)
- [Test 172: postgres_server_list](#test-172)
- [Test 173: postgres_server_list](#test-173)
- [Test 174: postgres_server_list](#test-174)
- [Test 175: postgres_server_param_get](#test-175)
- [Test 176: postgres_server_param_set](#test-176)
- [Test 177: postgres_table_list](#test-177)
- [Test 178: postgres_table_list](#test-178)
- [Test 179: postgres_table_schema_get](#test-179)
- [Test 180: deploy_app_logs_get](#test-180)
- [Test 181: deploy_architecture_diagram_generate](#test-181)
- [Test 182: deploy_iac_rules_get](#test-182)
- [Test 183: deploy_pipeline_guidance_get](#test-183)
- [Test 184: deploy_plan_get](#test-184)
- [Test 185: eventgrid_events_publish](#test-185)
- [Test 186: eventgrid_events_publish](#test-186)
- [Test 187: eventgrid_events_publish](#test-187)
- [Test 188: eventgrid_topic_list](#test-188)
- [Test 189: eventgrid_topic_list](#test-189)
- [Test 190: eventgrid_topic_list](#test-190)
- [Test 191: eventgrid_topic_list](#test-191)
- [Test 192: eventgrid_subscription_list](#test-192)
- [Test 193: eventgrid_subscription_list](#test-193)
- [Test 194: eventgrid_subscription_list](#test-194)
- [Test 195: eventgrid_subscription_list](#test-195)
- [Test 196: eventgrid_subscription_list](#test-196)
- [Test 197: eventgrid_subscription_list](#test-197)
- [Test 198: eventgrid_subscription_list](#test-198)
- [Test 199: eventhubs_eventhub_consumergroup_delete](#test-199)
- [Test 200: eventhubs_eventhub_consumergroup_get](#test-200)
- [Test 201: eventhubs_eventhub_consumergroup_get](#test-201)
- [Test 202: eventhubs_eventhub_consumergroup_update](#test-202)
- [Test 203: eventhubs_eventhub_consumergroup_update](#test-203)
- [Test 204: eventhubs_eventhub_delete](#test-204)
- [Test 205: eventhubs_eventhub_get](#test-205)
- [Test 206: eventhubs_eventhub_get](#test-206)
- [Test 207: eventhubs_eventhub_update](#test-207)
- [Test 208: eventhubs_eventhub_update](#test-208)
- [Test 209: eventhubs_namespace_delete](#test-209)
- [Test 210: eventhubs_namespace_get](#test-210)
- [Test 211: eventhubs_namespace_get](#test-211)
- [Test 212: eventhubs_namespace_update](#test-212)
- [Test 213: eventhubs_namespace_update](#test-213)
- [Test 214: functionapp_get](#test-214)
- [Test 215: functionapp_get](#test-215)
- [Test 216: functionapp_get](#test-216)
- [Test 217: functionapp_get](#test-217)
- [Test 218: functionapp_get](#test-218)
- [Test 219: functionapp_get](#test-219)
- [Test 220: functionapp_get](#test-220)
- [Test 221: functionapp_get](#test-221)
- [Test 222: functionapp_get](#test-222)
- [Test 223: functionapp_get](#test-223)
- [Test 224: functionapp_get](#test-224)
- [Test 225: functionapp_get](#test-225)
- [Test 226: keyvault_admin_settings_get](#test-226)
- [Test 227: keyvault_admin_settings_get](#test-227)
- [Test 228: keyvault_admin_settings_get](#test-228)
- [Test 229: keyvault_certificate_create](#test-229)
- [Test 230: keyvault_certificate_create](#test-230)
- [Test 231: keyvault_certificate_create](#test-231)
- [Test 232: keyvault_certificate_create](#test-232)
- [Test 233: keyvault_certificate_create](#test-233)
- [Test 234: keyvault_certificate_get](#test-234)
- [Test 235: keyvault_certificate_get](#test-235)
- [Test 236: keyvault_certificate_get](#test-236)
- [Test 237: keyvault_certificate_get](#test-237)
- [Test 238: keyvault_certificate_get](#test-238)
- [Test 239: keyvault_certificate_import](#test-239)
- [Test 240: keyvault_certificate_import](#test-240)
- [Test 241: keyvault_certificate_import](#test-241)
- [Test 242: keyvault_certificate_import](#test-242)
- [Test 243: keyvault_certificate_import](#test-243)
- [Test 244: keyvault_certificate_list](#test-244)
- [Test 245: keyvault_certificate_list](#test-245)
- [Test 246: keyvault_certificate_list](#test-246)
- [Test 247: keyvault_certificate_list](#test-247)
- [Test 248: keyvault_certificate_list](#test-248)
- [Test 249: keyvault_certificate_list](#test-249)
- [Test 250: keyvault_key_create](#test-250)
- [Test 251: keyvault_key_create](#test-251)
- [Test 252: keyvault_key_create](#test-252)
- [Test 253: keyvault_key_create](#test-253)
- [Test 254: keyvault_key_create](#test-254)
- [Test 255: keyvault_key_get](#test-255)
- [Test 256: keyvault_key_get](#test-256)
- [Test 257: keyvault_key_get](#test-257)
- [Test 258: keyvault_key_get](#test-258)
- [Test 259: keyvault_key_get](#test-259)
- [Test 260: keyvault_key_list](#test-260)
- [Test 261: keyvault_key_list](#test-261)
- [Test 262: keyvault_key_list](#test-262)
- [Test 263: keyvault_key_list](#test-263)
- [Test 264: keyvault_key_list](#test-264)
- [Test 265: keyvault_key_list](#test-265)
- [Test 266: keyvault_secret_create](#test-266)
- [Test 267: keyvault_secret_create](#test-267)
- [Test 268: keyvault_secret_create](#test-268)
- [Test 269: keyvault_secret_create](#test-269)
- [Test 270: keyvault_secret_create](#test-270)
- [Test 271: keyvault_secret_get](#test-271)
- [Test 272: keyvault_secret_get](#test-272)
- [Test 273: keyvault_secret_get](#test-273)
- [Test 274: keyvault_secret_get](#test-274)
- [Test 275: keyvault_secret_get](#test-275)
- [Test 276: keyvault_secret_list](#test-276)
- [Test 277: keyvault_secret_list](#test-277)
- [Test 278: keyvault_secret_list](#test-278)
- [Test 279: keyvault_secret_list](#test-279)
- [Test 280: keyvault_secret_list](#test-280)
- [Test 281: keyvault_secret_list](#test-281)
- [Test 282: aks_cluster_get](#test-282)
- [Test 283: aks_cluster_get](#test-283)
- [Test 284: aks_cluster_get](#test-284)
- [Test 285: aks_cluster_get](#test-285)
- [Test 286: aks_cluster_get](#test-286)
- [Test 287: aks_cluster_get](#test-287)
- [Test 288: aks_cluster_get](#test-288)
- [Test 289: aks_nodepool_get](#test-289)
- [Test 290: aks_nodepool_get](#test-290)
- [Test 291: aks_nodepool_get](#test-291)
- [Test 292: aks_nodepool_get](#test-292)
- [Test 293: aks_nodepool_get](#test-293)
- [Test 294: aks_nodepool_get](#test-294)
- [Test 295: loadtesting_test_create](#test-295)
- [Test 296: loadtesting_test_get](#test-296)
- [Test 297: loadtesting_testresource_create](#test-297)
- [Test 298: loadtesting_testresource_list](#test-298)
- [Test 299: loadtesting_testrun_create](#test-299)
- [Test 300: loadtesting_testrun_get](#test-300)
- [Test 301: loadtesting_testrun_list](#test-301)
- [Test 302: loadtesting_testrun_update](#test-302)
- [Test 303: grafana_list](#test-303)
- [Test 304: managedlustre_fs_create](#test-304)
- [Test 305: managedlustre_fs_list](#test-305)
- [Test 306: managedlustre_fs_list](#test-306)
- [Test 307: managedlustre_fs_sku_get](#test-307)
- [Test 308: managedlustre_fs_subnetsize_ask](#test-308)
- [Test 309: managedlustre_fs_subnetsize_validate](#test-309)
- [Test 310: managedlustre_fs_update](#test-310)
- [Test 311: marketplace_product_get](#test-311)
- [Test 312: marketplace_product_list](#test-312)
- [Test 313: marketplace_product_list](#test-313)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
- [Test 314: get_bestpractices_get](#test-314)
- [Test 315: get_bestpractices_get](#test-315)
- [Test 316: get_bestpractices_get](#test-316)
- [Test 317: get_bestpractices_get](#test-317)
- [Test 318: get_bestpractices_get](#test-318)
- [Test 319: get_bestpractices_get](#test-319)
- [Test 320: get_bestpractices_get](#test-320)
- [Test 321: get_bestpractices_get](#test-321)
- [Test 322: get_bestpractices_get](#test-322)
- [Test 323: monitor_activitylog_list](#test-323)
- [Test 324: monitor_healthmodels_entity_get](#test-324)
- [Test 325: monitor_metrics_definitions](#test-325)
- [Test 326: monitor_metrics_definitions](#test-326)
- [Test 327: monitor_metrics_definitions](#test-327)
- [Test 328: monitor_metrics_query](#test-328)
- [Test 329: monitor_metrics_query](#test-329)
- [Test 330: monitor_metrics_query](#test-330)
- [Test 331: monitor_metrics_query](#test-331)
- [Test 332: monitor_metrics_query](#test-332)
- [Test 333: monitor_metrics_query](#test-333)
- [Test 334: monitor_resource_log_query](#test-334)
- [Test 335: monitor_table_list](#test-335)
- [Test 336: monitor_table_list](#test-336)
- [Test 337: monitor_table_type_list](#test-337)
- [Test 338: monitor_table_type_list](#test-338)
- [Test 339: monitor_webtests_create](#test-339)
- [Test 340: monitor_webtests_get](#test-340)
- [Test 341: monitor_webtests_list](#test-341)
- [Test 342: monitor_webtests_list](#test-342)
- [Test 343: monitor_webtests_update](#test-343)
- [Test 344: monitor_workspace_list](#test-344)
- [Test 345: monitor_workspace_list](#test-345)
- [Test 346: monitor_workspace_list](#test-346)
- [Test 347: monitor_workspace_log_query](#test-347)
- [Test 348: datadog_monitoredresources_list](#test-348)
- [Test 349: datadog_monitoredresources_list](#test-349)
- [Test 350: extension_azqr](#test-350)
- [Test 351: extension_azqr](#test-351)
- [Test 352: extension_azqr](#test-352)
- [Test 353: quota_region_availability_list](#test-353)
- [Test 354: quota_usage_check](#test-354)
- [Test 355: role_assignment_list](#test-355)
- [Test 356: role_assignment_list](#test-356)
- [Test 357: redis_list](#test-357)
- [Test 358: redis_list](#test-358)
- [Test 359: redis_list](#test-359)
- [Test 360: redis_list](#test-360)
- [Test 361: redis_list](#test-361)
- [Test 362: group_list](#test-362)
- [Test 363: group_list](#test-363)
- [Test 364: group_list](#test-364)
- [Test 365: resourcehealth_availability-status_get](#test-365)
- [Test 366: resourcehealth_availability-status_get](#test-366)
- [Test 367: resourcehealth_availability-status_get](#test-367)
- [Test 368: resourcehealth_availability-status_list](#test-368)
- [Test 369: resourcehealth_availability-status_list](#test-369)
- [Test 370: resourcehealth_availability-status_list](#test-370)
- [Test 371: resourcehealth_health-events_list](#test-371)
- [Test 372: resourcehealth_health-events_list](#test-372)
- [Test 373: resourcehealth_health-events_list](#test-373)
- [Test 374: resourcehealth_health-events_list](#test-374)
- [Test 375: resourcehealth_health-events_list](#test-375)
- [Test 376: servicebus_queue_details](#test-376)
- [Test 377: servicebus_topic_details](#test-377)
- [Test 378: servicebus_topic_subscription_details](#test-378)
- [Test 379: signalr_runtime_get](#test-379)
- [Test 380: signalr_runtime_get](#test-380)
- [Test 381: signalr_runtime_get](#test-381)
- [Test 382: signalr_runtime_get](#test-382)
- [Test 383: signalr_runtime_get](#test-383)
- [Test 384: signalr_runtime_get](#test-384)
- [Test 385: sql_db_create](#test-385)
- [Test 386: sql_db_create](#test-386)
- [Test 387: sql_db_create](#test-387)
- [Test 388: sql_db_delete](#test-388)
- [Test 389: sql_db_delete](#test-389)
- [Test 390: sql_db_delete](#test-390)
- [Test 391: sql_db_list](#test-391)
- [Test 392: sql_db_list](#test-392)
- [Test 393: sql_db_rename](#test-393)
- [Test 394: sql_db_rename](#test-394)
- [Test 395: sql_db_show](#test-395)
- [Test 396: sql_db_show](#test-396)
- [Test 397: sql_db_update](#test-397)
- [Test 398: sql_db_update](#test-398)
- [Test 399: sql_elastic-pool_list](#test-399)
- [Test 400: sql_elastic-pool_list](#test-400)
- [Test 401: sql_elastic-pool_list](#test-401)
- [Test 402: sql_server_create](#test-402)
- [Test 403: sql_server_create](#test-403)
- [Test 404: sql_server_create](#test-404)
- [Test 405: sql_server_delete](#test-405)
- [Test 406: sql_server_delete](#test-406)
- [Test 407: sql_server_delete](#test-407)
- [Test 408: sql_server_entra-admin_list](#test-408)
- [Test 409: sql_server_entra-admin_list](#test-409)
- [Test 410: sql_server_entra-admin_list](#test-410)
- [Test 411: sql_server_firewall-rule_create](#test-411)
- [Test 412: sql_server_firewall-rule_create](#test-412)
- [Test 413: sql_server_firewall-rule_create](#test-413)
- [Test 414: sql_server_firewall-rule_delete](#test-414)
- [Test 415: sql_server_firewall-rule_delete](#test-415)
- [Test 416: sql_server_firewall-rule_delete](#test-416)
- [Test 417: sql_server_firewall-rule_list](#test-417)
- [Test 418: sql_server_firewall-rule_list](#test-418)
- [Test 419: sql_server_firewall-rule_list](#test-419)
- [Test 420: sql_server_list](#test-420)
- [Test 421: sql_server_list](#test-421)
- [Test 422: sql_server_show](#test-422)
- [Test 423: sql_server_show](#test-423)
- [Test 424: sql_server_show](#test-424)
- [Test 425: storage_account_create](#test-425)
- [Test 426: storage_account_create](#test-426)
- [Test 427: storage_account_create](#test-427)
- [Test 428: storage_account_get](#test-428)
- [Test 429: storage_account_get](#test-429)
- [Test 430: storage_account_get](#test-430)
- [Test 431: storage_account_get](#test-431)
- [Test 432: storage_account_get](#test-432)
- [Test 433: storage_blob_container_create](#test-433)
- [Test 434: storage_blob_container_create](#test-434)
- [Test 435: storage_blob_container_create](#test-435)
- [Test 436: storage_blob_container_get](#test-436)
- [Test 437: storage_blob_container_get](#test-437)
- [Test 438: storage_blob_container_get](#test-438)
- [Test 439: storage_blob_get](#test-439)
- [Test 440: storage_blob_get](#test-440)
- [Test 441: storage_blob_get](#test-441)
- [Test 442: storage_blob_get](#test-442)
- [Test 443: storage_blob_upload](#test-443)
- [Test 444: subscription_list](#test-444)
- [Test 445: subscription_list](#test-445)
- [Test 446: subscription_list](#test-446)
- [Test 447: subscription_list](#test-447)
- [Test 448: azureterraformbestpractices_get](#test-448)
- [Test 449: azureterraformbestpractices_get](#test-449)
- [Test 450: virtualdesktop_hostpool_list](#test-450)
- [Test 451: virtualdesktop_hostpool_host_list](#test-451)
- [Test 452: virtualdesktop_hostpool_host_user-list](#test-452)
- [Test 453: workbooks_create](#test-453)
- [Test 454: workbooks_delete](#test-454)
- [Test 455: workbooks_list](#test-455)
- [Test 456: workbooks_list](#test-456)
- [Test 457: workbooks_show](#test-457)
- [Test 458: workbooks_show](#test-458)
- [Test 459: workbooks_update](#test-459)
- [Test 460: bicepschema_get](#test-460)
- [Test 461: cloudarchitect_design](#test-461)
- [Test 462: cloudarchitect_design](#test-462)
- [Test 463: cloudarchitect_design](#test-463)
- [Test 464: cloudarchitect_design](#test-464)
<<<<<<< HEAD
=======
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 1

**Expected Tool:** `foundry_agents_connect`  
**Prompt:** Query an agent in my Azure AI foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.705410 | `foundry_agents_connect` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.663468 | `foundry_agents_list` | ❌ |
| 3 | 0.617213 | `foundry_resource_get` | ❌ |
| 4 | 0.548044 | `foundry_openai_models-list` | ❌ |
| 5 | 0.547459 | `foundry_agents_get-sdk-sample` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.663568 | `foundry_agents_list` | ❌ |
| 3 | 0.617213 | `foundry_resource_get` | ❌ |
| 4 | 0.548044 | `foundry_openai_models-list` | ❌ |
| 5 | 0.537580 | `foundry_agents_query-and-evaluate` | ❌ |
=======
| 2 | 0.617213 | `foundry_resource_get` | ❌ |
| 3 | 0.592487 | `foundry_agents_list` | ❌ |
| 4 | 0.537591 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.536533 | `search_index_query` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 2

**Expected Tool:** `foundry_agents_evaluate`  
**Prompt:** Evaluate the full query and response I got from my agent for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.543045 | `foundry_agents_query-and-evaluate` | ❌ |
| 2 | 0.469272 | `foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.445585 | `foundry_agents_connect` | ❌ |
| 4 | 0.298494 | `foundry_threads_list` | ❌ |
| 5 | 0.279058 | `foundry_agents_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.544099 | `foundry_agents_query-and-evaluate` | ❌ |
=======
| 1 | 0.544237 | `foundry_agents_query-and-evaluate` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.469428 | `foundry_agents_evaluate` | ✅ **EXPECTED** |
| 3 | 0.445964 | `foundry_agents_connect` | ❌ |
| 4 | 0.278921 | `foundry_agents_list` | ❌ |
| 5 | 0.250023 | `monitor_workspace_log_query` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 3

**Expected Tool:** `foundry_agents_list`  
**Prompt:** List all agents in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.797701 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.666021 | `foundry_resource_get` | ❌ |
| 3 | 0.654206 | `foundry_openai_models-list` | ❌ |
| 4 | 0.647246 | `foundry_threads_list` | ❌ |
| 5 | 0.575761 | `foundry_models_deployments_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.797877 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.666021 | `foundry_resource_get` | ❌ |
| 3 | 0.654206 | `foundry_openai_models-list` | ❌ |
| 4 | 0.575553 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.561946 | `search_service_list` | ❌ |
=======
| 1 | 0.748474 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.666021 | `foundry_resource_get` | ❌ |
| 3 | 0.561946 | `search_service_list` | ❌ |
| 4 | 0.556912 | `foundry_agents_connect` | ❌ |
| 5 | 0.542125 | `foundry_knowledge_index_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 4

**Expected Tool:** `foundry_agents_list`  
**Prompt:** Show me the available agents in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.749704 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.630323 | `foundry_resource_get` | ❌ |
| 3 | 0.611801 | `foundry_openai_models-list` | ❌ |
| 4 | 0.603708 | `foundry_threads_list` | ❌ |
| 5 | 0.556580 | `foundry_agents_get-sdk-sample` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.749829 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.630288 | `foundry_resource_get` | ❌ |
| 3 | 0.611722 | `foundry_openai_models-list` | ❌ |
| 4 | 0.548511 | `foundry_agents_connect` | ❌ |
| 5 | 0.535020 | `foundry_models_list` | ❌ |
=======
| 1 | 0.730759 | `foundry_agents_list` | ✅ **EXPECTED** |
| 2 | 0.630288 | `foundry_resource_get` | ❌ |
| 3 | 0.548511 | `foundry_agents_connect` | ❌ |
| 4 | 0.535020 | `foundry_models_list` | ❌ |
| 5 | 0.519892 | `foundry_knowledge_index_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 5

**Expected Tool:** `foundry_agents_create`  
**Prompt:** Create a new Azure AI Foundry agent using instructions in the active editor  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.587064 | `foundry_agents_create` | ✅ **EXPECTED** |
| 2 | 0.561567 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.554070 | `foundry_threads_create` | ❌ |
| 4 | 0.525727 | `foundry_models_deploy` | ❌ |
| 5 | 0.525461 | `foundry_agents_list` | ❌ |

---

## Test 6

**Expected Tool:** `foundry_agents_query-and-evaluate`  
**Prompt:** Query and evaluate an agent in my Azure AI Foundry resource for task_adherence  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.652200 | `foundry_agents_connect` | ❌ |
<<<<<<< HEAD
| 2 | 0.570725 | `foundry_agents_list` | ❌ |
| 3 | 0.553233 | `foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 4 | 0.493778 | `foundry_agents_evaluate` | ❌ |
| 5 | 0.469431 | `foundry_threads_list` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.570788 | `foundry_agents_list` | ❌ |
| 3 | 0.553190 | `foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 4 | 0.493779 | `foundry_agents_evaluate` | ❌ |
=======
| 2 | 0.553370 | `foundry_agents_query-and-evaluate` | ✅ **EXPECTED** |
| 3 | 0.493779 | `foundry_agents_evaluate` | ❌ |
| 4 | 0.469096 | `foundry_agents_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.460662 | `foundry_resource_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 7

**Expected Tool:** `foundry_agents_get-sdk-sample`  
**Prompt:** Create a CLI app that can talk to an Azure AI Foundry Agent using Python SDK  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595581 | `foundry_agents_get-sdk-sample` | ✅ **EXPECTED** |
| 2 | 0.552197 | `foundry_threads_create` | ❌ |
| 3 | 0.521920 | `foundry_agents_connect` | ❌ |
| 4 | 0.518552 | `foundry_agents_create` | ❌ |
| 5 | 0.509581 | `foundry_agents_list` | ❌ |

---

## Test 8

**Expected Tool:** `foundry_threads_create`  
**Prompt:** Create an Azure AI Foundry thread to hold the conversation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.606811 | `foundry_threads_create` | ✅ **EXPECTED** |
| 2 | 0.528310 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.519709 | `foundry_threads_get-messages` | ❌ |
| 4 | 0.506089 | `foundry_threads_list` | ❌ |
| 5 | 0.490796 | `foundry_models_deploy` | ❌ |

---

## Test 9

**Expected Tool:** `foundry_threads_list`  
**Prompt:** List my AI Foundry threads  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.677249 | `foundry_threads_list` | ✅ **EXPECTED** |
| 2 | 0.574068 | `foundry_threads_get-messages` | ❌ |
| 3 | 0.566999 | `foundry_threads_create` | ❌ |
| 4 | 0.471737 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.448682 | `foundry_agents_list` | ❌ |

---

## Test 10

**Expected Tool:** `foundry_threads_get-messages`  
**Prompt:** Show me the messages in the AI Foundry thread with id <thread_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.669937 | `foundry_threads_get-messages` | ✅ **EXPECTED** |
| 2 | 0.584431 | `foundry_threads_create` | ❌ |
| 3 | 0.529381 | `foundry_threads_list` | ❌ |
| 4 | 0.437894 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.427894 | `foundry_agents_create` | ❌ |

---

## Test 11

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** List all knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.703772 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.537540 | `foundry_agents_list` | ❌ |
| 3 | 0.526528 | `foundry_knowledge_index_schema` | ❌ |
| 4 | 0.500786 | `foundry_threads_list` | ❌ |
| 5 | 0.475746 | `foundry_models_deployments_list` | ❌ |

---

## Test 12

**Expected Tool:** `foundry_knowledge_index_list`  
**Prompt:** Show me the knowledge indexes in my AI Foundry project  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615458 | `foundry_knowledge_index_list` | ✅ **EXPECTED** |
| 2 | 0.489311 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.484329 | `foundry_agents_list` | ❌ |
| 4 | 0.454174 | `foundry_threads_list` | ❌ |
| 5 | 0.441521 | `foundry_resource_get` | ❌ |

---

## Test 13

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Show me the schema for knowledge index <index-name> in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.739885 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.589536 | `foundry_knowledge_index_list` | ❌ |
=======
| 2 | 0.614851 | `foundry_knowledge_index_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.494004 | `foundry_resource_get` | ❌ |
| 4 | 0.491510 | `search_index_get` | ❌ |
| 5 | 0.490410 | `search_knowledge_base_get` | ❌ |

---

## Test 14

**Expected Tool:** `foundry_knowledge_index_schema`  
**Prompt:** Get the schema configuration for knowledge index <index-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650203 | `foundry_knowledge_index_schema` | ✅ **EXPECTED** |
| 2 | 0.432792 | `postgres_table_schema_get` | ❌ |
| 3 | 0.417496 | `kusto_table_schema` | ❌ |
| 4 | 0.398322 | `mysql_table_schema_get` | ❌ |
| 5 | 0.396119 | `foundry_knowledge_index_list` | ❌ |

---

## Test 15

**Expected Tool:** `foundry_models_deploy`  
**Prompt:** Deploy a GPT4o instance on my resource <resource-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.562920 | `foundry_models_deploy` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.299986 | `foundry_openai_models-list` | ❌ |
| 3 | 0.298490 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.293050 | `loadtesting_testresource_create` | ❌ |
<<<<<<< HEAD
| 5 | 0.290387 | `foundry_openai_embeddings-create` | ❌ |
=======
| 5 | 0.290381 | `foundry_openai_embeddings-create` | ❌ |
=======
| 2 | 0.335116 | `foundry_openai_models-list` | ❌ |
| 3 | 0.298490 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.293050 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.282464 | `mysql_server_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 16

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** List all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.681081 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.674510 | `foundry_openai_models-list` | ❌ |
| 3 | 0.572625 | `foundry_threads_list` | ❌ |
| 4 | 0.568871 | `foundry_agents_list` | ❌ |
| 5 | 0.566272 | `foundry_resource_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.681385 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.674510 | `foundry_openai_models-list` | ❌ |
| 3 | 0.569059 | `foundry_agents_list` | ❌ |
| 4 | 0.566272 | `foundry_resource_get` | ❌ |
| 5 | 0.549636 | `foundry_models_list` | ❌ |
=======
| 1 | 0.663599 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.583429 | `foundry_openai_models-list` | ❌ |
| 3 | 0.566272 | `foundry_resource_get` | ❌ |
| 4 | 0.549636 | `foundry_models_list` | ❌ |
| 5 | 0.539695 | `foundry_agents_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 17

**Expected Tool:** `foundry_models_deployments_list`  
**Prompt:** Show me all AI Foundry model deployments  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.619840 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.619299 | `foundry_openai_models-list` | ❌ |
| 3 | 0.543385 | `foundry_resource_get` | ❌ |
| 4 | 0.540528 | `foundry_agents_list` | ❌ |
| 5 | 0.527141 | `foundry_threads_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.620173 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.619231 | `foundry_openai_models-list` | ❌ |
| 3 | 0.543352 | `foundry_resource_get` | ❌ |
| 4 | 0.540551 | `foundry_agents_list` | ❌ |
| 5 | 0.521475 | `foundry_models_deploy` | ❌ |
=======
| 1 | 0.606516 | `foundry_models_deployments_list` | ✅ **EXPECTED** |
| 2 | 0.543352 | `foundry_resource_get` | ❌ |
| 3 | 0.521475 | `foundry_models_deploy` | ❌ |
| 4 | 0.518221 | `foundry_models_list` | ❌ |
| 5 | 0.507301 | `foundry_openai_models-list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 18

**Expected Tool:** `foundry_models_list`  
**Prompt:** List all AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.603415 | `foundry_openai_models-list` | ❌ |
| 2 | 0.560022 | `foundry_models_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 3 | 0.553634 | `foundry_threads_list` | ❌ |
| 4 | 0.537958 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.519191 | `foundry_agents_list` | ❌ |
=======
| 3 | 0.537981 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.519472 | `foundry_agents_list` | ❌ |
| 5 | 0.514253 | `foundry_resource_get` | ❌ |
=======
| 1 | 0.560022 | `foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.514253 | `foundry_resource_get` | ❌ |
| 3 | 0.506418 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.491952 | `foundry_agents_list` | ❌ |
| 5 | 0.475204 | `foundry_openai_models-list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 19

**Expected Tool:** `foundry_models_list`  
**Prompt:** Show me the available AI Foundry models  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.576904 | `foundry_openai_models-list` | ❌ |
| 2 | 0.574818 | `foundry_models_list` | ✅ **EXPECTED** |
| 3 | 0.525312 | `foundry_resource_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.522153 | `foundry_agents_get-sdk-sample` | ❌ |
| 5 | 0.517825 | `foundry_models_deployments_list` | ❌ |
=======
| 4 | 0.517980 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.504087 | `foundry_agents_list` | ❌ |
=======
| 1 | 0.574818 | `foundry_models_list` | ✅ **EXPECTED** |
| 2 | 0.525312 | `foundry_resource_get` | ❌ |
| 3 | 0.497061 | `foundry_models_deployments_list` | ❌ |
| 4 | 0.475139 | `foundry_agents_list` | ❌ |
| 5 | 0.467671 | `foundry_models_deploy` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 20

**Expected Tool:** `foundry_openai_chat-completions-create`  
**Prompt:** Create a chat completion with the message "Hello, how are you today?" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.641293 | `foundry_openai_chat-completions-create` | ✅ **EXPECTED** |
| 2 | 0.546736 | `foundry_openai_create-completion` | ❌ |
<<<<<<< HEAD
| 3 | 0.420018 | `foundry_threads_create` | ❌ |
| 4 | 0.415482 | `foundry_agents_connect` | ❌ |
| 5 | 0.399382 | `foundry_openai_embeddings-create` | ❌ |
=======
| 3 | 0.415483 | `foundry_agents_connect` | ❌ |
| 4 | 0.399383 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.364105 | `foundry_models_deploy` | ❌ |
=======
| 1 | 0.558888 | `foundry_openai_chat-completions-create` | ✅ **EXPECTED** |
| 2 | 0.533147 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.415483 | `foundry_agents_connect` | ❌ |
| 4 | 0.364105 | `foundry_models_deploy` | ❌ |
| 5 | 0.361151 | `foundry_resource_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 21

**Expected Tool:** `foundry_openai_create-completion`  
**Prompt:** Create a completion with the prompt "What is Azure?" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.696936 | `foundry_openai_create-completion` | ✅ **EXPECTED** |
| 2 | 0.579108 | `foundry_openai_chat-completions-create` | ❌ |
<<<<<<< HEAD
| 3 | 0.465558 | `azureaibestpractices_get` | ❌ |
| 4 | 0.463703 | `foundry_models_deploy` | ❌ |
| 5 | 0.459126 | `foundry_resource_get` | ❌ |
=======
| 3 | 0.463703 | `foundry_models_deploy` | ❌ |
| 4 | 0.459126 | `foundry_resource_get` | ❌ |
| 5 | 0.458622 | `foundry_openai_embeddings-create` | ❌ |
=======
| 1 | 0.682250 | `foundry_openai_create-completion` | ✅ **EXPECTED** |
| 2 | 0.539297 | `foundry_openai_chat-completions-create` | ❌ |
| 3 | 0.463703 | `foundry_models_deploy` | ❌ |
| 4 | 0.459126 | `foundry_resource_get` | ❌ |
| 5 | 0.450993 | `deploy_pipeline_guidance_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 22

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Generate embeddings for the text "Azure OpenAI Service" using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.766496 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.543339 | `foundry_models_deploy` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.766338 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.543338 | `foundry_models_deploy` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.542214 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.520746 | `foundry_openai_models-list` | ❌ |
| 5 | 0.519335 | `foundry_resource_get` | ❌ |
=======
| 1 | 0.681346 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.556419 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.543338 | `foundry_models_deploy` | ❌ |
| 4 | 0.519335 | `foundry_resource_get` | ❌ |
| 5 | 0.463954 | `foundry_openai_models-list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 23

**Expected Tool:** `foundry_openai_embeddings-create`  
**Prompt:** Create vector embeddings for my text using my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.724369 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.494544 | `foundry_resource_get` | ❌ |
| 3 | 0.480389 | `foundry_models_deploy` | ❌ |
| 4 | 0.480294 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.463885 | `foundry_openai_chat-completions-create` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.724120 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.494485 | `foundry_resource_get` | ❌ |
| 3 | 0.480296 | `foundry_models_deploy` | ❌ |
| 4 | 0.480218 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.463797 | `foundry_openai_chat-completions-create` | ❌ |
=======
| 1 | 0.638843 | `foundry_openai_embeddings-create` | ✅ **EXPECTED** |
| 2 | 0.494506 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.494485 | `foundry_resource_get` | ❌ |
| 4 | 0.480296 | `foundry_models_deploy` | ❌ |
| 5 | 0.399908 | `foundry_openai_chat-completions-create` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 24

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** List all available OpenAI models in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.799059 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.668887 | `foundry_resource_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.667041 | `foundry_models_list` | ❌ |
| 4 | 0.666560 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.657393 | `foundry_agents_list` | ❌ |
=======
| 3 | 0.667040 | `foundry_models_list` | ❌ |
| 4 | 0.666207 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.657546 | `foundry_agents_list` | ❌ |
=======
| 1 | 0.729075 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.668887 | `foundry_resource_get` | ❌ |
| 3 | 0.667040 | `foundry_models_list` | ❌ |
| 4 | 0.660489 | `foundry_agents_list` | ❌ |
| 5 | 0.604808 | `foundry_models_deployments_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 25

**Expected Tool:** `foundry_openai_models-list`  
**Prompt:** Show me the OpenAI model deployments in my Azure AI Foundry resource  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.741659 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.660115 | `foundry_models_deployments_list` | ❌ |
| 3 | 0.648218 | `foundry_resource_get` | ❌ |
| 4 | 0.640650 | `foundry_models_deploy` | ❌ |
<<<<<<< HEAD
| 5 | 0.619790 | `foundry_agents_list` | ❌ |
=======
| 5 | 0.619878 | `foundry_agents_list` | ❌ |
=======
| 1 | 0.654318 | `foundry_openai_models-list` | ✅ **EXPECTED** |
| 2 | 0.648219 | `foundry_resource_get` | ❌ |
| 3 | 0.640650 | `foundry_models_deploy` | ❌ |
| 4 | 0.637676 | `foundry_models_deployments_list` | ❌ |
| 5 | 0.576563 | `foundry_agents_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 26

**Expected Tool:** `foundry_resource_get`  
**Prompt:** List all AI Foundry resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594096 | `foundry_resource_get` | ✅ **EXPECTED** |
| 2 | 0.571916 | `foundry_openai_models-list` | ❌ |
| 3 | 0.566762 | `foundry_agents_list` | ❌ |
| 4 | 0.558075 | `foundry_threads_list` | ❌ |
| 5 | 0.556154 | `search_service_list` | ❌ |

---

## Test 27

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Show me the AI Foundry resources in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665311 | `foundry_resource_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.585305 | `foundry_openai_models-list` | ❌ |
| 3 | 0.553808 | `foundry_agents_list` | ❌ |
| 4 | 0.518747 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.492911 | `foundry_models_deploy` | ❌ |
=======
| 2 | 0.492911 | `foundry_models_deploy` | ❌ |
| 3 | 0.474905 | `foundry_agents_list` | ❌ |
| 4 | 0.467211 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.453632 | `foundry_openai_models-list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 28

**Expected Tool:** `foundry_resource_get`  
**Prompt:** Get details for AI Foundry resource <resource_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.735316 | `foundry_resource_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.571906 | `foundry_openai_models-list` | ❌ |
<<<<<<< HEAD
| 3 | 0.509484 | `monitor_webtests_get` | ❌ |
| 4 | 0.496980 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.475498 | `foundry_agents_list` | ❌ |
=======
| 3 | 0.510197 | `monitor_webtests_get` | ❌ |
| 4 | 0.497090 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.475722 | `foundry_agents_list` | ❌ |
=======
| 2 | 0.509484 | `monitor_webtests_get` | ❌ |
| 3 | 0.455154 | `foundry_openai_models-list` | ❌ |
| 4 | 0.452340 | `foundry_models_deploy` | ❌ |
| 5 | 0.444390 | `loadtesting_testresource_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 29

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.785967 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.700824 | `search_knowledge_source_get` | ❌ |
| 3 | 0.692681 | `search_service_list` | ❌ |
| 4 | 0.635863 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.586575 | `search_index_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.785556 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.700785 | `search_knowledge_source_get` | ❌ |
| 3 | 0.693600 | `search_service_list` | ❌ |
| 4 | 0.635477 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.586578 | `search_index_get` | ❌ |
=======
| 1 | 0.785967 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.700968 | `search_knowledge_source_get` | ❌ |
| 3 | 0.693471 | `search_service_list` | ❌ |
| 4 | 0.635863 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.603324 | `foundry_knowledge_index_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 30

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.748213 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.668479 | `search_knowledge_source_get` | ❌ |
| 3 | 0.628582 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.623715 | `search_service_list` | ❌ |
| 5 | 0.566618 | `search_index_get` | ❌ |

---

## Test 31

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** List all knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.702942 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.606164 | `search_knowledge_source_get` | ❌ |
| 3 | 0.583234 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.512825 | `search_service_list` | ❌ |
| 5 | 0.476815 | `foundry_knowledge_index_list` | ❌ |

---

## Test 32

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge bases in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.688155 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.599348 | `search_knowledge_source_get` | ❌ |
| 3 | 0.578437 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.456512 | `search_service_list` | ❌ |
| 5 | 0.439493 | `foundry_knowledge_index_list` | ❌ |
=======
| 1 | 0.688051 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.599305 | `search_knowledge_source_get` | ❌ |
| 3 | 0.578499 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.457619 | `search_service_list` | ❌ |
| 5 | 0.439529 | `foundry_knowledge_index_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 33

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Get the details of knowledge base <agent-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.769383 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.685640 | `search_knowledge_source_get` | ❌ |
| 3 | 0.636958 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.585949 | `search_index_get` | ❌ |
| 5 | 0.533298 | `search_service_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.769443 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.685642 | `search_knowledge_source_get` | ❌ |
| 3 | 0.636767 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.586085 | `search_index_get` | ❌ |
| 5 | 0.533859 | `search_service_list` | ❌ |
=======
| 1 | 0.769384 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.685412 | `search_knowledge_source_get` | ❌ |
| 3 | 0.636958 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.585949 | `search_index_get` | ❌ |
| 5 | 0.533700 | `search_service_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 34

**Expected Tool:** `search_knowledge_base_get`  
**Prompt:** Show me the knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595585 | `search_knowledge_base_get` | ✅ **EXPECTED** |
| 2 | 0.551922 | `search_knowledge_base_retrieve` | ❌ |
<<<<<<< HEAD
| 3 | 0.515480 | `search_knowledge_source_get` | ❌ |
| 4 | 0.366170 | `search_service_list` | ❌ |
| 5 | 0.365633 | `search_index_get` | ❌ |
=======
| 3 | 0.515607 | `search_knowledge_source_get` | ❌ |
| 4 | 0.376599 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.366893 | `search_service_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 35

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in Azure AI Search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.724869 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.650606 | `search_knowledge_base_get` | ❌ |
| 3 | 0.575356 | `search_index_query` | ❌ |
| 4 | 0.567386 | `search_knowledge_source_get` | ❌ |
| 5 | 0.520336 | `foundry_agents_connect` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.724846 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.650590 | `search_knowledge_base_get` | ❌ |
| 3 | 0.575307 | `search_index_query` | ❌ |
| 4 | 0.567361 | `search_knowledge_source_get` | ❌ |
| 5 | 0.520360 | `foundry_agents_connect` | ❌ |
=======
| 1 | 0.724733 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.650523 | `search_knowledge_base_get` | ❌ |
| 3 | 0.575078 | `search_index_query` | ❌ |
| 4 | 0.566839 | `search_knowledge_source_get` | ❌ |
| 5 | 0.520277 | `foundry_agents_connect` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 36

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.633877 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589927 | `search_knowledge_base_get` | ❌ |
| 3 | 0.502173 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422676 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399110 | `search_index_query` | ❌ |
=======
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422671 | `foundry_agents_query-and-evaluate` | ❌ |
=======
| 3 | 0.501973 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422489 | `foundry_agents_query-and-evaluate` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.399595 | `search_index_query` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 37

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Run a retrieval with knowledge base <agent-name> in search service <service-name> for the query <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.657866 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.557206 | `search_knowledge_base_get` | ❌ |
| 3 | 0.463605 | `search_knowledge_source_get` | ❌ |
| 4 | 0.436719 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.422173 | `foundry_agents_connect` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.657844 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.557115 | `search_knowledge_base_get` | ❌ |
| 3 | 0.463461 | `search_knowledge_source_get` | ❌ |
| 4 | 0.436952 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.422469 | `foundry_agents_connect` | ❌ |
=======
| 1 | 0.657865 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.557206 | `search_knowledge_base_get` | ❌ |
| 3 | 0.463023 | `search_knowledge_source_get` | ❌ |
| 4 | 0.436580 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.422173 | `foundry_agents_connect` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 38

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Ask knowledge base <agent-name> in search service <service-name> to retrieve information about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633766 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.589869 | `search_knowledge_base_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.502085 | `search_knowledge_source_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.422610 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.399521 | `search_index_query` | ❌ |
=======
| 4 | 0.422671 | `foundry_agents_query-and-evaluate` | ❌ |
=======
| 3 | 0.501973 | `search_knowledge_source_get` | ❌ |
| 4 | 0.422489 | `foundry_agents_query-and-evaluate` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.399595 | `search_index_query` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 39

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Query knowledge base <agent-name> in search service <service-name> about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.598868 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.547862 | `search_knowledge_base_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.467868 | `foundry_agents_query-and-evaluate` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.467907 | `foundry_agents_query-and-evaluate` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.464904 | `search_knowledge_source_get` | ❌ |
=======
| 3 | 0.467711 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.464987 | `search_knowledge_source_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.412481 | `foundry_agents_connect` | ❌ |

---

## Test 40

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Search knowledge base <agent-name> in Azure AI Search service <service-name> for <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.649767 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.631435 | `search_knowledge_base_get` | ❌ |
| 3 | 0.581359 | `search_index_query` | ❌ |
| 4 | 0.571156 | `search_knowledge_source_get` | ❌ |
| 5 | 0.544545 | `search_service_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.649751 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.631420 | `search_knowledge_base_get` | ❌ |
| 3 | 0.581412 | `search_index_query` | ❌ |
| 4 | 0.571126 | `search_knowledge_source_get` | ❌ |
| 5 | 0.544488 | `search_service_list` | ❌ |
=======
| 1 | 0.649767 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.631435 | `search_knowledge_base_get` | ❌ |
| 3 | 0.581387 | `search_index_query` | ❌ |
| 4 | 0.571101 | `search_knowledge_source_get` | ❌ |
| 5 | 0.544501 | `search_service_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 41

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** What does knowledge base <agent-name> in search service <service-name> know about <query>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579716 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.560688 | `search_knowledge_base_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.477941 | `search_knowledge_source_get` | ❌ |
| 4 | 0.402530 | `foundry_agents_query-and-evaluate` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.477942 | `search_knowledge_source_get` | ❌ |
| 4 | 0.402582 | `foundry_agents_query-and-evaluate` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.361231 | `foundry_knowledge_index_list` | ❌ |
=======
| 3 | 0.478132 | `search_knowledge_source_get` | ❌ |
| 4 | 0.402474 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.371055 | `foundry_knowledge_index_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 42

**Expected Tool:** `search_knowledge_base_retrieve`  
**Prompt:** Find information about <query> using knowledge base <agent-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.582662 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.528610 | `search_knowledge_base_get` | ❌ |
| 3 | 0.449336 | `search_knowledge_source_get` | ❌ |
| 4 | 0.447690 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.397187 | `foundry_agents_connect` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.582660 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.528583 | `search_knowledge_base_get` | ❌ |
| 3 | 0.449290 | `search_knowledge_source_get` | ❌ |
| 4 | 0.447915 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.397238 | `foundry_agents_connect` | ❌ |
=======
| 1 | 0.582662 | `search_knowledge_base_retrieve` | ✅ **EXPECTED** |
| 2 | 0.528610 | `search_knowledge_base_get` | ❌ |
| 3 | 0.449340 | `search_knowledge_source_get` | ❌ |
| 4 | 0.447632 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.397187 | `foundry_agents_connect` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 43

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.760406 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.690845 | `search_service_list` | ❌ |
| 3 | 0.665905 | `search_knowledge_base_get` | ❌ |
| 4 | 0.573014 | `search_index_get` | ❌ |
| 5 | 0.560755 | `search_knowledge_base_retrieve` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.760416 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.691931 | `search_service_list` | ❌ |
| 3 | 0.665923 | `search_knowledge_base_get` | ❌ |
| 4 | 0.573012 | `search_index_get` | ❌ |
| 5 | 0.560779 | `search_knowledge_base_retrieve` | ❌ |
=======
| 1 | 0.760757 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.692251 | `search_service_list` | ❌ |
| 3 | 0.666204 | `search_knowledge_base_get` | ❌ |
| 4 | 0.579582 | `foundry_knowledge_index_list` | ❌ |
| 5 | 0.573177 | `search_index_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 44

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.737860 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.659236 | `search_service_list` | ❌ |
=======
| 1 | 0.737971 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.660170 | `search_service_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.652969 | `search_knowledge_base_get` | ❌ |
| 4 | 0.578836 | `search_index_get` | ❌ |
| 5 | 0.560519 | `search_index_query` | ❌ |

---

## Test 45

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** List all knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.657936 | `search_knowledge_source_get` | ✅ **EXPECTED** |
=======
| 1 | 0.658365 | `search_knowledge_source_get` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.558516 | `search_knowledge_base_get` | ❌ |
| 3 | 0.510338 | `search_service_list` | ❌ |
| 4 | 0.470560 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.433657 | `foundry_knowledge_index_list` | ❌ |

---

## Test 46

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge sources in the search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.653143 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.563270 | `search_knowledge_base_get` | ❌ |
| 3 | 0.485934 | `search_service_list` | ❌ |
| 4 | 0.477636 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.430518 | `search_index_get` | ❌ |

---

## Test 47

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Get the details of knowledge source <source-name> in the Azure AI Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.825604 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.693438 | `search_knowledge_base_get` | ❌ |
=======
| 1 | 0.825664 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.693437 | `search_knowledge_base_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.595643 | `search_index_get` | ❌ |
| 4 | 0.540550 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.531085 | `search_service_list` | ❌ |

---

## Test 48

**Expected Tool:** `search_knowledge_source_get`  
**Prompt:** Show me the knowledge source <source-name> in search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.631283 | `search_knowledge_source_get` | ✅ **EXPECTED** |
| 2 | 0.523643 | `search_knowledge_base_get` | ❌ |
| 3 | 0.459923 | `search_knowledge_base_retrieve` | ❌ |
| 4 | 0.371465 | `search_index_get` | ❌ |
| 5 | 0.370585 | `search_service_list` | ❌ |

---

## Test 49

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the details of the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.681052 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.544557 | `foundry_knowledge_index_schema` | ❌ |
| 3 | 0.528153 | `search_knowledge_base_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.521765 | `search_knowledge_source_get` | ❌ |
| 5 | 0.490553 | `search_service_list` | ❌ |
=======
| 4 | 0.522514 | `search_knowledge_source_get` | ❌ |
| 5 | 0.490624 | `search_service_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 50

**Expected Tool:** `search_index_get`  
**Prompt:** List all indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.640256 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.619949 | `search_service_list` | ❌ |
| 3 | 0.538885 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.511485 | `search_knowledge_base_get` | ❌ |
| 5 | 0.496554 | `search_knowledge_source_get` | ❌ |

---

## Test 51

**Expected Tool:** `search_index_get`  
**Prompt:** Show me the indexes in the Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.620759 | `search_index_get` | ✅ **EXPECTED** |
| 2 | 0.562503 | `search_service_list` | ❌ |
| 3 | 0.538471 | `foundry_knowledge_index_list` | ❌ |
| 4 | 0.500365 | `search_knowledge_base_get` | ❌ |
| 5 | 0.490330 | `search_knowledge_source_get` | ❌ |

---

## Test 52

**Expected Tool:** `search_index_query`  
**Prompt:** Search for instances of <search_term> in the index <index-name> in Cognitive Search service <service-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.522598 | `search_index_get` | ❌ |
| 2 | 0.515911 | `search_index_query` | ✅ **EXPECTED** |
| 3 | 0.498264 | `search_service_list` | ❌ |
| 4 | 0.447868 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.437608 | `postgres_database_query` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.522953 | `search_index_get` | ❌ |
| 2 | 0.515871 | `search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497392 | `search_service_list` | ❌ |
| 4 | 0.447993 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.437640 | `postgres_database_query` | ❌ |
=======
| 1 | 0.522754 | `search_index_get` | ❌ |
| 2 | 0.515812 | `search_index_query` | ✅ **EXPECTED** |
| 3 | 0.497494 | `search_service_list` | ❌ |
| 4 | 0.447954 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.437709 | `postgres_database_query` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 53

**Expected Tool:** `search_service_list`  
**Prompt:** List all Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.791803 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.553012 | `kusto_cluster_list` | ❌ |
=======
| 1 | 0.793651 | `search_service_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.553011 | `kusto_cluster_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.509479 | `subscription_list` | ❌ |
| 4 | 0.505971 | `search_index_get` | ❌ |
| 5 | 0.504693 | `marketplace_product_list` | ❌ |
=======
| 2 | 0.553043 | `kusto_cluster_list` | ❌ |
| 3 | 0.520340 | `foundry_agents_list` | ❌ |
| 4 | 0.509461 | `subscription_list` | ❌ |
| 5 | 0.505971 | `search_index_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 54

**Expected Tool:** `search_service_list`  
**Prompt:** Show me the Cognitive Search services in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.684837 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.484092 | `marketplace_product_list` | ❌ |
| 3 | 0.479898 | `search_index_get` | ❌ |
| 4 | 0.462337 | `search_knowledge_base_get` | ❌ |
| 5 | 0.461786 | `kusto_cluster_list` | ❌ |

---

## Test 55

**Expected Tool:** `search_service_list`  
**Prompt:** Show me my Cognitive Search services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.551241 | `search_service_list` | ✅ **EXPECTED** |
| 2 | 0.436230 | `search_index_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.415277 | `search_knowledge_base_get` | ❌ |
| 4 | 0.410461 | `search_knowledge_source_get` | ❌ |
<<<<<<< HEAD
| 5 | 0.404707 | `search_index_query` | ❌ |
=======
| 5 | 0.404758 | `search_index_query` | ❌ |
=======
| 3 | 0.417096 | `foundry_agents_list` | ❌ |
| 4 | 0.415277 | `search_knowledge_base_get` | ❌ |
| 5 | 0.410568 | `search_knowledge_source_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 56

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert this audio file to text using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.666038 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.377210 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.351127 | `deploy_plan_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.338137 | `extension_cli_generate` | ❌ |
| 5 | 0.337763 | `deploy_pipeline_guidance_get` | ❌ |
=======
| 4 | 0.338047 | `extension_cli_generate` | ❌ |
| 5 | 0.337685 | `deploy_pipeline_guidance_get` | ❌ |
=======
| 1 | 0.677871 | `speech_tts_synthesize` | ❌ |
| 2 | 0.666038 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 3 | 0.415224 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.365228 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.351127 | `deploy_plan_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 57

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from my audio file with language detection  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.511324 | `speech_stt_recognize` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.198123 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.192462 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.170157 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.167159 | `foundry_openai_chat-completions-create` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.192450 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.170157 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.167159 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.159108 | `foundry_agents_connect` | ❌ |
=======
| 2 | 0.353620 | `speech_tts_synthesize` | ❌ |
| 3 | 0.202056 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.190197 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.184542 | `foundry_openai_create-completion` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 58

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe speech from audio file <file_path> with profanity filtering  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.486489 | `speech_stt_recognize` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.162863 | `foundry_threads_create` | ❌ |
| 3 | 0.160209 | `foundry_agents_connect` | ❌ |
| 4 | 0.156936 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.154737 | `foundry_openai_create-completion` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.160209 | `foundry_agents_connect` | ❌ |
| 3 | 0.156850 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.154737 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.154098 | `foundry_openai_embeddings-create` | ❌ |
=======
| 2 | 0.354154 | `speech_tts_synthesize` | ❌ |
| 3 | 0.180941 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.178944 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.160209 | `foundry_agents_connect` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 59

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text from audio file <file_path> using endpoint <endpoint>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.612032 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.309860 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.244223 | `foundry_resource_get` | ❌ |
| 4 | 0.243658 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.242816 | `foundry_openai_chat-completions-create` | ❌ |
=======
| 1 | 0.611992 | `speech_stt_recognize` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.309895 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.244218 | `foundry_resource_get` | ❌ |
| 4 | 0.243626 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.242771 | `foundry_openai_chat-completions-create` | ❌ |
=======
| 2 | 0.584104 | `speech_tts_synthesize` | ❌ |
| 3 | 0.322301 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.263196 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.251200 | `foundry_openai_chat-completions-create` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 60

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe the audio file <file_path> in Spanish language  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.410533 | `speech_stt_recognize` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.152414 | `foundry_openai_embeddings-create` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.152391 | `foundry_openai_embeddings-create` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.152137 | `foundry_models_deploy` | ❌ |
| 4 | 0.151799 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.140373 | `deploy_plan_get` | ❌ |
=======
| 2 | 0.373433 | `speech_tts_synthesize` | ❌ |
| 3 | 0.159775 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.158032 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.152137 | `foundry_models_deploy` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 61

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with detailed output format from audio file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546259 | `speech_stt_recognize` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.218092 | `foundry_resource_get` | ❌ |
| 3 | 0.202860 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.183420 | `extension_azqr` | ❌ |
| 5 | 0.181020 | `search_index_get` | ❌ |
=======
| 2 | 0.499808 | `speech_tts_synthesize` | ❌ |
| 3 | 0.225372 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.218092 | `foundry_resource_get` | ❌ |
| 5 | 0.200865 | `foundry_openai_chat-completions-create` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 62

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Recognize speech from <file_path> with phrase hints for better accuracy  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.539963 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.228587 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.203413 | `foundry_agents_connect` | ❌ |
| 4 | 0.199517 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.197301 | `foundry_openai_chat-completions-create` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.540249 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.227953 | `foundry_openai_create-completion` | ❌ |
| 3 | 0.203215 | `foundry_agents_connect` | ❌ |
| 4 | 0.199441 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.197199 | `foundry_openai_chat-completions-create` | ❌ |
=======
| 1 | 0.539963 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.382022 | `speech_tts_synthesize` | ❌ |
| 3 | 0.246979 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.238192 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.203413 | `foundry_agents_connect` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 63

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio using multiple phrase hints: "Azure", "cognitive services", "machine learning"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.549151 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.393626 | `azureaibestpractices_get` | ❌ |
| 3 | 0.342537 | `extension_cli_generate` | ❌ |
| 4 | 0.337387 | `cloudarchitect_design` | ❌ |
| 5 | 0.335741 | `foundry_openai_create-completion` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.548967 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.342494 | `extension_cli_generate` | ❌ |
| 3 | 0.337434 | `cloudarchitect_design` | ❌ |
| 4 | 0.335792 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.333130 | `get_bestpractices_get` | ❌ |
=======
| 1 | 0.549151 | `speech_stt_recognize` | ✅ **EXPECTED** |
| 2 | 0.460662 | `speech_tts_synthesize` | ❌ |
| 3 | 0.357816 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.345661 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.342537 | `extension_cli_generate` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 64

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Convert speech to text with comma-separated phrase hints: "Azure, cognitive services, API"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532536 | `speech_stt_recognize` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.349892 | `foundry_openai_create-completion` | ❌ |
<<<<<<< HEAD
| 3 | 0.348381 | `azureaibestpractices_get` | ❌ |
| 4 | 0.340893 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.332862 | `foundry_openai_embeddings-create` | ❌ |
=======
| 3 | 0.340893 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.332669 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.326712 | `get_bestpractices_get` | ❌ |
=======
| 2 | 0.506045 | `speech_tts_synthesize` | ❌ |
| 3 | 0.385033 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.381487 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.378382 | `foundry_openai_create-completion` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 65

**Expected Tool:** `speech_stt_recognize`  
**Prompt:** Transcribe audio with raw profanity output from file <file_path>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.453396 | `speech_stt_recognize` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.173280 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.164929 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.160483 | `foundry_agents_connect` | ❌ |
| 5 | 0.160185 | `extension_azqr` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.173205 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.164990 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.160523 | `extension_azqr` | ❌ |
| 5 | 0.160483 | `foundry_agents_connect` | ❌ |
=======
| 2 | 0.342007 | `speech_tts_synthesize` | ❌ |
| 3 | 0.181994 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.174375 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.173205 | `deploy_pipeline_guidance_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Test 66

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to speech and save to output.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547977 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.422457 | `speech_stt_recognize` | ❌ |
| 3 | 0.231058 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.200920 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.192179 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 62

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize speech from "Hello, welcome to Azure" and save to welcome.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531396 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.486019 | `speech_stt_recognize` | ❌ |
| 3 | 0.329765 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.323728 | `extension_cli_generate` | ❌ |
| 5 | 0.320006 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 63

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech audio from text "Hello world" using Azure Speech Services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590514 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.534002 | `speech_stt_recognize` | ❌ |
| 3 | 0.362626 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.341003 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.333557 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 64

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to speech with Spanish language and save to spanish-audio.wav  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.520866 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.452648 | `speech_stt_recognize` | ❌ |
| 3 | 0.231393 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.204970 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.202502 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 65

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize speech with voice en-US-JennyNeural from text "Azure AI Services"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604553 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.496715 | `speech_stt_recognize` | ❌ |
| 3 | 0.423461 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.390312 | `foundry_agents_list` | ❌ |
| 5 | 0.381735 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 66

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create MP3 audio file from text "Welcome to Azure" with high quality format  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.564876 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.510908 | `speech_stt_recognize` | ❌ |
| 3 | 0.360542 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.347597 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.345073 | `deploy_iac_rules_get` | ❌ |

---

## Test 67

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Generate speech with custom voice model using endpoint ID <endpoint-id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.547864 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.455734 | `speech_stt_recognize` | ❌ |
| 3 | 0.367601 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.358913 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.356105 | `foundry_models_deployments_list` | ❌ |

---

## Test 68

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Convert text to OGG/Opus format audio file  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.446150 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.410086 | `speech_stt_recognize` | ❌ |
| 3 | 0.263503 | `foundry_openai_embeddings-create` | ❌ |
| 4 | 0.199147 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.196153 | `extension_cli_generate` | ❌ |

---

## Test 69

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Synthesize long text content to audio file with streaming  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449165 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.369045 | `speech_stt_recognize` | ❌ |
| 3 | 0.225665 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.225088 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.218260 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 70

**Expected Tool:** `speech_tts_synthesize`  
**Prompt:** Create audio file from text in French language with appropriate voice  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.467698 | `speech_tts_synthesize` | ✅ **EXPECTED** |
| 2 | 0.385267 | `speech_stt_recognize` | ❌ |
| 3 | 0.235591 | `foundry_openai_create-completion` | ❌ |
| 4 | 0.215304 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.208978 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 71

**Expected Tool:** `appconfig_account_list`  
**Prompt:** List all App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.786298 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.530613 | `appconfig_kv_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.491380 | `postgres_server_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.491358 | `postgres_server_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.481223 | `kusto_cluster_list` | ❌ |
| 5 | 0.479997 | `subscription_list` | ❌ |
=======
| 3 | 0.491380 | `postgres_server_list` | ❌ |
| 4 | 0.481174 | `kusto_cluster_list` | ❌ |
| 5 | 0.479988 | `subscription_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

<<<<<<< HEAD
## Test 67
=======
## Test 72
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me the App Configuration stores in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635056 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.464826 | `appconfig_kv_get` | ❌ |
| 3 | 0.398562 | `subscription_list` | ❌ |
| 4 | 0.391398 | `redis_list` | ❌ |
| 5 | 0.372579 | `postgres_server_list` | ❌ |

---

<<<<<<< HEAD
## Test 68
=======
## Test 73
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appconfig_account_list`  
**Prompt:** Show me my App Configuration stores  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565365 | `appconfig_account_list` | ✅ **EXPECTED** |
| 2 | 0.465344 | `appconfig_kv_get` | ❌ |
| 3 | 0.355916 | `postgres_server_config_get` | ❌ |
| 4 | 0.348661 | `appconfig_kv_delete` | ❌ |
| 5 | 0.327234 | `appconfig_kv_set` | ❌ |

---

<<<<<<< HEAD
## Test 69
=======
## Test 74
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appconfig_kv_delete`  
**Prompt:** Delete the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.618276 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.464358 | `appconfig_kv_get` | ❌ |
| 3 | 0.424344 | `appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.392260 | `appconfig_account_list` | ❌ |

---

## Test 70
=======
<<<<<<< HEAD
| 1 | 0.618277 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.464358 | `appconfig_kv_get` | ❌ |
| 3 | 0.424344 | `appconfig_kv_set` | ❌ |
| 4 | 0.422700 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.392016 | `appconfig_account_list` | ❌ |
=======
| 1 | 0.618267 | `appconfig_kv_delete` | ✅ **EXPECTED** |
| 2 | 0.464368 | `appconfig_kv_get` | ❌ |
| 3 | 0.424296 | `appconfig_kv_set` | ❌ |
| 4 | 0.422722 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.392081 | `appconfig_account_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 75
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.632652 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.558116 | `appconfig_account_list` | ❌ |
| 3 | 0.531033 | `appconfig_kv_set` | ❌ |
| 4 | 0.464568 | `appconfig_kv_delete` | ❌ |
| 5 | 0.438999 | `appconfig_kv_lock_set` | ❌ |

---

<<<<<<< HEAD
## Test 71
=======
## Test 76
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show me the key-value settings in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612555 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.522671 | `appconfig_account_list` | ❌ |
| 3 | 0.512945 | `appconfig_kv_set` | ❌ |
| 4 | 0.468503 | `appconfig_kv_delete` | ❌ |
| 5 | 0.457866 | `appconfig_kv_lock_set` | ❌ |

---

<<<<<<< HEAD
## Test 72
=======
## Test 77
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** List all key-value settings with key name starting with 'prod-' in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.512883 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.450109 | `appconfig_account_list` | ❌ |
| 3 | 0.398684 | `appconfig_kv_set` | ❌ |
| 4 | 0.380614 | `appconfig_kv_delete` | ❌ |
| 5 | 0.346166 | `appconfig_kv_lock_set` | ❌ |

---

## Test 73
=======
<<<<<<< HEAD
| 1 | 0.512880 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.449934 | `appconfig_account_list` | ❌ |
| 3 | 0.398698 | `appconfig_kv_set` | ❌ |
| 4 | 0.380636 | `appconfig_kv_delete` | ❌ |
| 5 | 0.346156 | `appconfig_kv_lock_set` | ❌ |
=======
| 1 | 0.512804 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.449871 | `appconfig_account_list` | ❌ |
| 3 | 0.398608 | `appconfig_kv_set` | ❌ |
| 4 | 0.380599 | `appconfig_kv_delete` | ❌ |
| 5 | 0.346117 | `appconfig_kv_lock_set` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 78
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appconfig_kv_get`  
**Prompt:** Show the content for the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552300 | `appconfig_kv_get` | ✅ **EXPECTED** |
| 2 | 0.448912 | `appconfig_kv_set` | ❌ |
| 3 | 0.441713 | `appconfig_kv_delete` | ❌ |
| 4 | 0.437745 | `appconfig_account_list` | ❌ |
| 5 | 0.416264 | `appconfig_kv_lock_set` | ❌ |

---

<<<<<<< HEAD
## Test 74
=======
## Test 79
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Lock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591253 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.487221 | `appconfig_kv_get` | ❌ |
| 3 | 0.445541 | `appconfig_kv_set` | ❌ |
| 4 | 0.431462 | `appconfig_kv_delete` | ❌ |
| 5 | 0.373617 | `appconfig_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 75
=======
## Test 80
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appconfig_kv_lock_set`  
**Prompt:** Unlock the key <key_name> in App Configuration store <app_config_store_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.555699 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.505681 | `appconfig_kv_get` | ❌ |
| 3 | 0.476497 | `appconfig_kv_delete` | ❌ |
| 4 | 0.425488 | `appconfig_kv_set` | ❌ |
<<<<<<< HEAD
| 5 | 0.409649 | `appconfig_account_list` | ❌ |

---

## Test 76
=======
| 5 | 0.409406 | `appconfig_account_list` | ❌ |
=======
| 1 | 0.555732 | `appconfig_kv_lock_set` | ✅ **EXPECTED** |
| 2 | 0.505675 | `appconfig_kv_get` | ❌ |
| 3 | 0.476507 | `appconfig_kv_delete` | ❌ |
| 4 | 0.425479 | `appconfig_kv_set` | ❌ |
| 5 | 0.409370 | `appconfig_account_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 81
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appconfig_kv_set`  
**Prompt:** Set the key <key_name> in App Configuration store <app_config_store_name> to <value>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609635 | `appconfig_kv_set` | ✅ **EXPECTED** |
| 2 | 0.536497 | `appconfig_kv_lock_set` | ❌ |
| 3 | 0.512707 | `appconfig_kv_get` | ❌ |
| 4 | 0.505571 | `appconfig_kv_delete` | ❌ |
| 5 | 0.378223 | `appconfig_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 77
=======
## Test 82
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Please help me diagnose issues with my app using app lens  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595632 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.335768 | `deploy_app_logs_get` | ❌ |
| 3 | 0.300786 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.273083 | `cloudarchitect_design` | ❌ |
| 5 | 0.254473 | `monitor_resource_log_query` | ❌ |

---

<<<<<<< HEAD
## Test 78
=======
## Test 83
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** Use app lens to check why my app is slow?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502361 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.316002 | `deploy_app_logs_get` | ❌ |
| 3 | 0.255570 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.249583 | `monitor_resource_log_query` | ❌ |
<<<<<<< HEAD
| 5 | 0.226030 | `quota_usage_check` | ❌ |

---

## Test 79
=======
<<<<<<< HEAD
| 5 | 0.226092 | `quota_usage_check` | ❌ |
=======
| 5 | 0.225972 | `quota_usage_check` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 84
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `applens_resource_diagnose`  
**Prompt:** What does app lens say is wrong with my service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.492820 | `applens_resource_diagnose` | ✅ **EXPECTED** |
| 2 | 0.256325 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.242574 | `cloudarchitect_design` | ❌ |
| 4 | 0.225608 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.211260 | `deploy_app_logs_get` | ❌ |

---

<<<<<<< HEAD
## Test 80
=======
## Test 85
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> to my app service <app_name> for database <database_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.717878 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.401376 | `sql_db_rename` | ❌ |
| 3 | 0.399941 | `sql_db_create` | ❌ |
| 4 | 0.362997 | `sql_db_show` | ❌ |
| 5 | 0.357919 | `sql_db_list` | ❌ |

---

## Test 81
=======
<<<<<<< HEAD
| 1 | 0.717887 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.401337 | `sql_db_rename` | ❌ |
| 3 | 0.399997 | `sql_db_create` | ❌ |
| 4 | 0.362889 | `sql_db_show` | ❌ |
| 5 | 0.357708 | `sql_db_list` | ❌ |
=======
| 1 | 0.682502 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.401311 | `sql_db_rename` | ❌ |
| 3 | 0.400175 | `sql_db_create` | ❌ |
| 4 | 0.363123 | `sql_db_show` | ❌ |
| 5 | 0.357874 | `sql_db_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 86
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure SQL Server database <database_name> for app service <app_name> with connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.688410 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.498122 | `sql_db_rename` | ❌ |
| 3 | 0.497502 | `sql_db_create` | ❌ |
| 4 | 0.469326 | `sql_db_show` | ❌ |
| 5 | 0.452937 | `sql_db_list` | ❌ |

---

## Test 82
=======
<<<<<<< HEAD
| 1 | 0.688364 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.498175 | `sql_db_rename` | ❌ |
| 3 | 0.497711 | `sql_db_create` | ❌ |
| 4 | 0.469526 | `sql_db_show` | ❌ |
| 5 | 0.453040 | `sql_db_list` | ❌ |
=======
| 1 | 0.654513 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.498175 | `sql_db_rename` | ❌ |
| 3 | 0.497522 | `sql_db_create` | ❌ |
| 4 | 0.469526 | `sql_db_show` | ❌ |
| 5 | 0.453088 | `sql_db_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 87
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add MySQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.675970 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.464756 | `sql_db_create` | ❌ |
| 3 | 0.452407 | `sql_db_rename` | ❌ |
| 4 | 0.432948 | `mysql_server_list` | ❌ |
| 5 | 0.410292 | `sql_db_show` | ❌ |

---

## Test 83
=======
<<<<<<< HEAD
| 1 | 0.675548 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.465376 | `sql_db_create` | ❌ |
| 3 | 0.452528 | `sql_db_rename` | ❌ |
| 4 | 0.433256 | `mysql_server_list` | ❌ |
| 5 | 0.410221 | `sql_db_show` | ❌ |
=======
| 1 | 0.655045 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.465281 | `sql_db_create` | ❌ |
| 3 | 0.452630 | `sql_db_rename` | ❌ |
| 4 | 0.433191 | `mysql_server_list` | ❌ |
| 5 | 0.410316 | `sql_db_show` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 88
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add PostgreSQL database <database_name> to app service <app_name> using connection <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.628119 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.444212 | `sql_db_create` | ❌ |
| 3 | 0.405314 | `postgres_database_query` | ❌ |
| 4 | 0.401117 | `postgres_database_list` | ❌ |
| 5 | 0.400767 | `sql_db_rename` | ❌ |

---

## Test 84
=======
<<<<<<< HEAD
| 1 | 0.627847 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.444822 | `sql_db_create` | ❌ |
| 3 | 0.404711 | `postgres_database_query` | ❌ |
| 4 | 0.401105 | `postgres_database_list` | ❌ |
| 5 | 0.400866 | `sql_db_rename` | ❌ |
=======
| 1 | 0.599525 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.444152 | `sql_db_create` | ❌ |
| 3 | 0.404912 | `postgres_database_query` | ❌ |
| 4 | 0.401137 | `postgres_database_list` | ❌ |
| 5 | 0.400754 | `sql_db_rename` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 89
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect CosmosDB database <database_name> using connection string <connection_string> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.663086 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.446465 | `cosmos_database_list` | ❌ |
| 3 | 0.441966 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.427284 | `cosmos_database_container_list` | ❌ |
| 5 | 0.420488 | `sql_db_rename` | ❌ |

---

## Test 85
=======
<<<<<<< HEAD
| 1 | 0.663498 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.446339 | `cosmos_database_list` | ❌ |
| 3 | 0.441990 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.427167 | `cosmos_database_container_list` | ❌ |
| 5 | 0.420405 | `sql_db_rename` | ❌ |
=======
| 1 | 0.608259 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.445781 | `cosmos_database_list` | ❌ |
| 3 | 0.441836 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.426789 | `cosmos_database_container_list` | ❌ |
| 5 | 0.420630 | `sql_db_rename` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 90
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection <connection_string> for database <database_name> on server <database_server> to app service <app_name> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.733852 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.454554 | `sql_db_create` | ❌ |
| 3 | 0.415271 | `sql_db_rename` | ❌ |
| 4 | 0.414045 | `sql_server_create` | ❌ |
| 5 | 0.410260 | `sql_db_list` | ❌ |

---

## Test 86
=======
<<<<<<< HEAD
| 1 | 0.733775 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.454433 | `sql_db_create` | ❌ |
| 3 | 0.415274 | `sql_db_rename` | ❌ |
| 4 | 0.414045 | `sql_server_create` | ❌ |
| 5 | 0.410100 | `sql_db_list` | ❌ |
=======
| 1 | 0.702259 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.454592 | `sql_db_create` | ❌ |
| 3 | 0.415290 | `sql_db_rename` | ❌ |
| 4 | 0.414069 | `sql_server_create` | ❌ |
| 5 | 0.410258 | `sql_db_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 91
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appservice_database_add`  
**Prompt:** Add database connection string for <database_name> to app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.746766 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.441682 | `sql_db_rename` | ❌ |
| 3 | 0.434020 | `sql_db_create` | ❌ |
| 4 | 0.391311 | `sql_db_list` | ❌ |
| 5 | 0.390014 | `sql_db_show` | ❌ |

---

## Test 87
=======
<<<<<<< HEAD
| 1 | 0.746379 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.441584 | `sql_db_rename` | ❌ |
| 3 | 0.434079 | `sql_db_create` | ❌ |
| 4 | 0.391000 | `sql_db_list` | ❌ |
| 5 | 0.389995 | `sql_db_show` | ❌ |
=======
| 1 | 0.686506 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.441542 | `sql_db_rename` | ❌ |
| 3 | 0.433865 | `sql_db_create` | ❌ |
| 4 | 0.391188 | `sql_db_list` | ❌ |
| 5 | 0.390129 | `sql_db_show` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 92
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appservice_database_add`  
**Prompt:** Connect database <database_name> to my app service <app_name> using connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.680503 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.429273 | `sql_db_rename` | ❌ |
| 3 | 0.406267 | `sql_db_create` | ❌ |
| 4 | 0.396537 | `sql_db_show` | ❌ |
| 5 | 0.391409 | `sql_db_list` | ❌ |

---

## Test 88
=======
<<<<<<< HEAD
| 1 | 0.680525 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.429291 | `sql_db_rename` | ❌ |
| 3 | 0.406599 | `sql_db_create` | ❌ |
| 4 | 0.396524 | `sql_db_show` | ❌ |
| 5 | 0.391416 | `sql_db_list` | ❌ |
=======
| 1 | 0.643888 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.429317 | `sql_db_rename` | ❌ |
| 3 | 0.406322 | `sql_db_create` | ❌ |
| 4 | 0.396523 | `sql_db_show` | ❌ |
| 5 | 0.391430 | `sql_db_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 93
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appservice_database_add`  
**Prompt:** Set up database <database_name> for app service <app_name> with connection string <connection_string> under resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.640738 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.456785 | `sql_db_create` | ❌ |
| 3 | 0.402668 | `sql_db_rename` | ❌ |
| 4 | 0.401985 | `sql_db_show` | ❌ |
| 5 | 0.394072 | `sql_db_list` | ❌ |

---

## Test 89
=======
<<<<<<< HEAD
| 1 | 0.640622 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.456508 | `sql_db_create` | ❌ |
| 3 | 0.402651 | `sql_db_rename` | ❌ |
| 4 | 0.402081 | `sql_db_show` | ❌ |
| 5 | 0.394177 | `sql_db_list` | ❌ |
=======
| 1 | 0.598494 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.456884 | `sql_db_create` | ❌ |
| 3 | 0.402743 | `sql_db_rename` | ❌ |
| 4 | 0.402138 | `sql_db_show` | ❌ |
| 5 | 0.394211 | `sql_db_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)

---

## Test 94

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure database <database_name> for app service <app_name> with the connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.650888 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449175 | `sql_db_rename` | ❌ |
| 3 | 0.448382 | `sql_db_create` | ❌ |
| 4 | 0.414323 | `sql_db_show` | ❌ |
| 5 | 0.411790 | `sql_db_list` | ❌ |

---

## Test 95
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `appservice_database_add`  
**Prompt:** Configure database <database_name> for app service <app_name> with the connection string <connection_string> in resource group <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688527 | `appservice_database_add` | ✅ **EXPECTED** |
| 2 | 0.449176 | `sql_db_rename` | ❌ |
| 3 | 0.448382 | `sql_db_create` | ❌ |
| 4 | 0.414329 | `sql_db_show` | ❌ |
| 5 | 0.411782 | `sql_db_list` | ❌ |

---

## Test 90

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List code optimization recommendations across my Application Insights components  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572473 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.454559 | `azureaibestpractices_get` | ❌ |
| 3 | 0.445157 | `get_bestpractices_get` | ❌ |
| 4 | 0.390478 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.383948 | `applens_resource_diagnose` | ❌ |

---

## Test 91
=======
<<<<<<< HEAD
| 2 | 0.445157 | `get_bestpractices_get` | ❌ |
| 3 | 0.390549 | `azureterraformbestpractices_get` | ❌ |
=======
| 2 | 0.449459 | `get_bestpractices_get` | ❌ |
| 3 | 0.390478 | `azureterraformbestpractices_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.383948 | `applens_resource_diagnose` | ❌ |
| 5 | 0.375286 | `deploy_iac_rules_get` | ❌ |

---

<<<<<<< HEAD
## Test 86
=======
## Test 96
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me code optimization recommendations for all Application Insights resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.696531 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.506351 | `azureaibestpractices_get` | ❌ |
| 3 | 0.468384 | `get_bestpractices_get` | ❌ |
| 4 | 0.452231 | `applens_resource_diagnose` | ❌ |
| 5 | 0.435241 | `azureterraformbestpractices_get` | ❌ |

---

<<<<<<< HEAD
## Test 92
=======
## Test 87
=======
| 1 | 0.696565 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.470670 | `get_bestpractices_get` | ❌ |
| 3 | 0.452233 | `applens_resource_diagnose` | ❌ |
| 4 | 0.435290 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.424629 | `search_service_list` | ❌ |

---

## Test 97
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** List profiler recommendations for Application Insights in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626722 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.488002 | `loadtesting_testresource_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.479392 | `mysql_server_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.479416 | `mysql_server_list` | ❌ |
=======
| 3 | 0.479392 | `mysql_server_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.477396 | `applens_resource_diagnose` | ❌ |
| 5 | 0.468847 | `resourcehealth_availability-status_list` | ❌ |

---

<<<<<<< HEAD
## Test 93
=======
<<<<<<< HEAD
## Test 88
=======
## Test 98
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `applicationinsights_recommendation_list`  
**Prompt:** Show me performance improvement recommendations from Application Insights  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.509615 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.433835 | `azureaibestpractices_get` | ❌ |
| 3 | 0.419699 | `applens_resource_diagnose` | ❌ |
| 4 | 0.383861 | `get_bestpractices_get` | ❌ |
| 5 | 0.367317 | `deploy_architecture_diagram_generate` | ❌ |

---

## Test 94
=======
| 1 | 0.509502 | `applicationinsights_recommendation_list` | ✅ **EXPECTED** |
| 2 | 0.419670 | `applens_resource_diagnose` | ❌ |
<<<<<<< HEAD
| 3 | 0.383767 | `get_bestpractices_get` | ❌ |
| 4 | 0.367260 | `deploy_architecture_diagram_generate` | ❌ |
=======
| 3 | 0.385936 | `get_bestpractices_get` | ❌ |
| 4 | 0.367278 | `deploy_architecture_diagram_generate` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.343931 | `cloudarchitect_design` | ❌ |

---

<<<<<<< HEAD
## Test 89
=======
## Test 99
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Create a Storage account with name <storage_account_name> using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.593241 | `storage_account_create` | ❌ |
| 2 | 0.564940 | `storage_blob_container_create` | ❌ |
<<<<<<< HEAD
| 3 | 0.493684 | `storage_account_get` | ❌ |
| 4 | 0.473547 | `storage_blob_container_get` | ❌ |
| 5 | 0.456428 | `managedlustre_fs_create` | ❌ |

---

## Test 95
=======
<<<<<<< HEAD
| 3 | 0.493609 | `storage_account_get` | ❌ |
=======
| 3 | 0.493641 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.474399 | `storage_blob_container_get` | ❌ |
| 5 | 0.454194 | `managedlustre_fs_create` | ❌ |

---

<<<<<<< HEAD
## Test 90
=======
## Test 100
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `extension_cli_generate`  
**Prompt:** List all virtual machines in my subscription using Azure CLI  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.592102 | `search_service_list` | ❌ |
=======
| 1 | 0.593467 | `search_service_list` | ❌ |
<<<<<<< HEAD
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.575274 | `kusto_cluster_list` | ❌ |
| 3 | 0.549918 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.544688 | `monitor_workspace_list` | ❌ |
| 5 | 0.536238 | `subscription_list` | ❌ |

---

<<<<<<< HEAD
## Test 96
=======
## Test 91
=======
| 2 | 0.575351 | `kusto_cluster_list` | ❌ |
| 3 | 0.549966 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.544412 | `monitor_workspace_list` | ❌ |
| 5 | 0.536252 | `subscription_list` | ❌ |

---

## Test 101
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `extension_cli_generate`  
**Prompt:** Show me the details of the storage account <account_name> with Azure CLI commands  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.710307 | `storage_account_get` | ❌ |
| 2 | 0.601571 | `storage_blob_container_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.710155 | `storage_account_get` | ❌ |
=======
| 1 | 0.710305 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.602173 | `storage_blob_container_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.543268 | `storage_blob_get` | ❌ |
| 4 | 0.519788 | `storage_account_create` | ❌ |
| 5 | 0.493100 | `cosmos_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 97
=======
<<<<<<< HEAD
## Test 92
=======
## Test 102
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `extension_cli_install`  
**Prompt:** <Ask the MCP host to uninstall az cli on your machine and run test prompts for extension_cli_generate>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.479652 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.473369 | `extension_cli_generate` | ❌ |
| 3 | 0.389405 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.382473 | `deploy_plan_get` | ❌ |
| 5 | 0.366067 | `get_bestpractices_get` | ❌ |

---

## Test 98
=======
<<<<<<< HEAD
| 1 | 0.479590 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.473266 | `extension_cli_generate` | ❌ |
| 3 | 0.389369 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.382389 | `deploy_plan_get` | ❌ |
| 5 | 0.366012 | `get_bestpractices_get` | ❌ |

---

## Test 93
=======
| 1 | 0.497777 | `extension_cli_generate` | ❌ |
| 2 | 0.497497 | `extension_cli_install` | ✅ **EXPECTED** |
| 3 | 0.401453 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.383619 | `deploy_plan_get` | ❌ |
| 5 | 0.382552 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 103
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `extension_cli_install`  
**Prompt:** How to install azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.460416 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.429269 | `deploy_app_logs_get` | ❌ |
| 3 | 0.365212 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.335279 | `deploy_plan_get` | ❌ |
| 5 | 0.326165 | `deploy_pipeline_guidance_get` | ❌ |

---

<<<<<<< HEAD
## Test 99
=======
<<<<<<< HEAD
## Test 94
=======
## Test 104
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `extension_cli_install`  
**Prompt:** What is Azure Functions Core tools and how to install it  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.622670 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.439414 | `get_bestpractices_get` | ❌ |
| 3 | 0.432859 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.430682 | `extension_cli_generate` | ❌ |
| 5 | 0.418085 | `deploy_plan_get` | ❌ |

---

## Test 100
=======
| 1 | 0.622705 | `extension_cli_install` | ✅ **EXPECTED** |
| 2 | 0.443050 | `get_bestpractices_get` | ❌ |
| 3 | 0.432913 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.430483 | `extension_cli_generate` | ❌ |
| 5 | 0.418161 | `deploy_plan_get` | ❌ |

---

<<<<<<< HEAD
## Test 95
=======
## Test 105
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `acr_registry_list`  
**Prompt:** List all Azure Container Registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.743568 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.711580 | `acr_registry_repository_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.585675 | `kusto_cluster_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.540241 | `search_service_list` | ❌ |
| 5 | 0.514293 | `cosmos_account_list` | ❌ |

---

## Test 101
=======
=======
| 3 | 0.585618 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.541506 | `search_service_list` | ❌ |
| 5 | 0.514326 | `cosmos_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 96
=======
## Test 106
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me my Azure Container Registries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586014 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563636 | `acr_registry_repository_list` | ❌ |
| 3 | 0.460834 | `storage_blob_container_get` | ❌ |
| 4 | 0.415552 | `cosmos_database_container_list` | ❌ |
| 5 | 0.402247 | `redis_list` | ❌ |

---

<<<<<<< HEAD
## Test 102
=======
<<<<<<< HEAD
## Test 97
=======
## Test 107
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.637130 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.563476 | `acr_registry_repository_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.516769 | `kusto_cluster_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.515365 | `storage_blob_container_get` | ❌ |
=======
=======
| 3 | 0.516826 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.515378 | `storage_blob_container_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.480352 | `redis_list` | ❌ |

---

<<<<<<< HEAD
## Test 103
=======
<<<<<<< HEAD
## Test 98
=======
## Test 108
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `acr_registry_list`  
**Prompt:** List container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.654318 | `acr_registry_repository_list` | ❌ |
| 2 | 0.633938 | `acr_registry_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 3 | 0.476015 | `mysql_server_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.476043 | `mysql_server_list` | ❌ |
=======
| 3 | 0.476015 | `mysql_server_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.454929 | `group_list` | ❌ |
| 5 | 0.454003 | `datadog_monitoredresources_list` | ❌ |

---

<<<<<<< HEAD
## Test 104
=======
<<<<<<< HEAD
## Test 99
=======
## Test 109
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `acr_registry_list`  
**Prompt:** Show me the container registries in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639391 | `acr_registry_list` | ✅ **EXPECTED** |
| 2 | 0.637972 | `acr_registry_repository_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.468028 | `mysql_server_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.468078 | `mysql_server_list` | ❌ |
=======
| 3 | 0.468028 | `mysql_server_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.449649 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.445741 | `group_list` | ❌ |

---

<<<<<<< HEAD
## Test 105
=======
<<<<<<< HEAD
## Test 100
=======
## Test 110
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List all container registry repositories in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.626482 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.617504 | `acr_registry_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.544172 | `kusto_cluster_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.508863 | `storage_blob_container_get` | ❌ |
| 5 | 0.495567 | `postgres_server_list` | ❌ |

---

## Test 106
=======
=======
| 3 | 0.544238 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.508483 | `storage_blob_container_get` | ❌ |
| 5 | 0.495526 | `postgres_server_list` | ❌ |

---

<<<<<<< HEAD
## Test 101
=======
## Test 111
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me my container registry repositories  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546334 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.469295 | `acr_registry_list` | ❌ |
| 3 | 0.451973 | `storage_blob_container_get` | ❌ |
| 4 | 0.407973 | `cosmos_database_container_list` | ❌ |
| 5 | 0.373464 | `storage_blob_get` | ❌ |

---

<<<<<<< HEAD
## Test 107
=======
<<<<<<< HEAD
## Test 102
=======
## Test 112
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** List repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674296 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.541779 | `acr_registry_list` | ❌ |
| 3 | 0.437756 | `storage_blob_container_get` | ❌ |
| 4 | 0.433927 | `cosmos_database_container_list` | ❌ |
<<<<<<< HEAD
| 5 | 0.383001 | `kusto_database_list` | ❌ |

---

## Test 108
=======
<<<<<<< HEAD
| 5 | 0.383201 | `kusto_database_list` | ❌ |

---

## Test 103
=======
| 5 | 0.383621 | `kusto_database_list` | ❌ |

---

## Test 113
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `acr_registry_repository_list`  
**Prompt:** Show me the repositories in the container registry <registry_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600780 | `acr_registry_repository_list` | ✅ **EXPECTED** |
| 2 | 0.501842 | `acr_registry_list` | ❌ |
| 3 | 0.431148 | `storage_blob_container_get` | ❌ |
| 4 | 0.418623 | `cosmos_database_container_list` | ❌ |
| 5 | 0.378151 | `redis_list` | ❌ |

---

<<<<<<< HEAD
## Test 109
=======
<<<<<<< HEAD
## Test 104
=======
## Test 114
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.498396 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.229071 | `communication_sms_send` | ❌ |
=======
| 1 | 0.498292 | `communication_email_send` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.226847 | `communication_sms_send` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.188975 | `eventgrid_events_publish` | ❌ |
| 4 | 0.161257 | `foundry_agents_create` | ❌ |
| 5 | 0.146045 | `servicebus_topic_details` | ❌ |

---

<<<<<<< HEAD
## Test 110
=======
## Test 105
=======
| 2 | 0.229081 | `communication_sms_send` | ❌ |
| 3 | 0.189000 | `eventgrid_events_publish` | ❌ |
| 4 | 0.155364 | `speech_tts_synthesize` | ❌ |
| 5 | 0.145951 | `servicebus_topic_details` | ❌ |

---

## Test 115
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email from my communication service to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.498459 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.314408 | `communication_sms_send` | ❌ |
| 3 | 0.235110 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.211067 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.210014 | `foundry_agents_create` | ❌ |

---

## Test 111
=======
| 1 | 0.498406 | `communication_email_send` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.313058 | `communication_sms_send` | ❌ |
| 3 | 0.235127 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.211154 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.194094 | `speech_stt_recognize` | ❌ |

---

## Test 106
=======
| 2 | 0.314462 | `communication_sms_send` | ❌ |
| 3 | 0.228890 | `speech_tts_synthesize` | ❌ |
| 4 | 0.218524 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.211154 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 116
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_email_send`  
**Prompt:** Send HTML-formatted email to <email-address> with subject <subject>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.521087 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.207644 | `communication_sms_send` | ❌ |
| 3 | 0.152418 | `eventgrid_events_publish` | ❌ |
| 4 | 0.152056 | `servicebus_topic_details` | ❌ |
=======
| 1 | 0.520967 | `communication_email_send` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.205130 | `communication_sms_send` | ❌ |
| 3 | 0.152418 | `eventgrid_events_publish` | ❌ |
=======
| 2 | 0.207658 | `communication_sms_send` | ❌ |
| 3 | 0.152427 | `eventgrid_events_publish` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.152013 | `servicebus_topic_details` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.143660 | `foundry_agents_evaluate` | ❌ |

---

<<<<<<< HEAD
## Test 112
=======
<<<<<<< HEAD
## Test 107
=======
## Test 117
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with CC to <email-address-1> and <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.533532 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.219566 | `communication_sms_send` | ❌ |
| 3 | 0.106042 | `foundry_agents_query-and-evaluate` | ❌ |
=======
| 1 | 0.533447 | `communication_email_send` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.217412 | `communication_sms_send` | ❌ |
| 3 | 0.106026 | `foundry_agents_query-and-evaluate` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.103723 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.084905 | `cosmos_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 113
=======
## Test 108
=======
| 2 | 0.219584 | `communication_sms_send` | ❌ |
| 3 | 0.106044 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.087784 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.084933 | `cosmos_account_list` | ❌ |

---

## Test 118
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email to multiple recipients: <email-address-1>, <email-address-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.540910 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.244525 | `communication_sms_send` | ❌ |
| 3 | 0.134996 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.114359 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.087005 | `postgres_server_param_set` | ❌ |

---

## Test 114
=======
| 1 | 0.540792 | `communication_email_send` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.241620 | `communication_sms_send` | ❌ |
| 3 | 0.134975 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.114324 | `foundry_agents_query-and-evaluate` | ❌ |
=======
| 2 | 0.244521 | `communication_sms_send` | ❌ |
| 3 | 0.114380 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.098798 | `foundry_openai_chat-completions-create` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.087063 | `postgres_server_param_set` | ❌ |

---

<<<<<<< HEAD
## Test 109
=======
## Test 119
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with reply-to address set to <email-address>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.512721 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.200189 | `communication_sms_send` | ❌ |
| 3 | 0.164422 | `mysql_server_param_set` | ❌ |
=======
| 1 | 0.512623 | `communication_email_send` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.198552 | `communication_sms_send` | ❌ |
=======
| 2 | 0.200177 | `communication_sms_send` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.164115 | `mysql_server_param_set` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.158759 | `postgres_server_param_set` | ❌ |
| 5 | 0.143574 | `appconfig_kv_set` | ❌ |

---

<<<<<<< HEAD
## Test 115
=======
<<<<<<< HEAD
## Test 110
=======
## Test 120
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_email_send`  
**Prompt:** Send email with custom sender name <sender-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.473192 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.255124 | `communication_sms_send` | ❌ |
=======
| 1 | 0.473175 | `communication_email_send` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.253449 | `communication_sms_send` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.164811 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.160285 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.156869 | `cosmos_database_container_item_query` | ❌ |

---

<<<<<<< HEAD
## Test 116
=======
## Test 111
=======
| 2 | 0.255169 | `communication_sms_send` | ❌ |
| 3 | 0.156869 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.143626 | `sql_db_rename` | ❌ |
| 5 | 0.139388 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 121
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_email_send`  
**Prompt:** Send an email with BCC recipients  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.528899 | `communication_email_send` | ✅ **EXPECTED** |
| 2 | 0.241091 | `communication_sms_send` | ❌ |
| 3 | 0.137538 | `confidentialledger_entries_append` | ❌ |
| 4 | 0.108748 | `confidentialledger_entries_get` | ❌ |
=======
| 1 | 0.528789 | `communication_email_send` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.239846 | `communication_sms_send` | ❌ |
| 3 | 0.137565 | `confidentialledger_entries_append` | ❌ |
| 4 | 0.108725 | `confidentialledger_entries_get` | ❌ |
=======
| 2 | 0.241114 | `communication_sms_send` | ❌ |
| 3 | 0.137538 | `confidentialledger_entries_append` | ❌ |
| 4 | 0.108748 | `confidentialledger_entries_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.105033 | `storage_blob_upload` | ❌ |

---

<<<<<<< HEAD
## Test 117
=======
<<<<<<< HEAD
## Test 112
=======
## Test 122
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS message to <phone-number> saying "Hello"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.533822 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.251480 | `communication_email_send` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.533763 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.251429 | `communication_email_send` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.218656 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.175534 | `foundry_agents_create` | ❌ |
| 5 | 0.156040 | `foundry_threads_create` | ❌ |

---

<<<<<<< HEAD
## Test 118
=======
## Test 113
=======
| 1 | 0.533868 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.251429 | `communication_email_send` | ❌ |
| 3 | 0.178085 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.170676 | `speech_tts_synthesize` | ❌ |
| 5 | 0.148584 | `foundry_agents_connect` | ❌ |

---

## Test 123
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to <phone-number-2> from <phone-number-1> with message "Test message"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.546006 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.294912 | `communication_email_send` | ❌ |
| 3 | 0.204585 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.200656 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.141105 | `foundry_agents_create` | ❌ |

---

## Test 119
=======
<<<<<<< HEAD
| 1 | 0.543875 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.294603 | `communication_email_send` | ❌ |
| 3 | 0.204487 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.200633 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.136763 | `loadtesting_testrun_update` | ❌ |

---

## Test 114
=======
| 1 | 0.546019 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.294859 | `communication_email_send` | ❌ |
| 3 | 0.204588 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.155927 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.139313 | `speech_tts_synthesize` | ❌ |

---

## Test 124
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS to multiple recipients: <phone-number-1>, <phone-number-2>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.545744 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.422028 | `communication_email_send` | ❌ |
| 3 | 0.186088 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.142054 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.113722 | `foundry_threads_get-messages` | ❌ |

---

## Test 120
=======
<<<<<<< HEAD
| 1 | 0.543753 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.421988 | `communication_email_send` | ❌ |
| 3 | 0.186088 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.142030 | `foundry_agents_query-and-evaluate` | ❌ |
=======
| 1 | 0.545755 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.421988 | `communication_email_send` | ❌ |
| 3 | 0.142602 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.141987 | `foundry_agents_query-and-evaluate` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.104124 | `search_knowledge_base_retrieve` | ❌ |

---

<<<<<<< HEAD
## Test 115
=======
## Test 125
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS with delivery reporting enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.554917 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269203 | `communication_email_send` | ❌ |
| 3 | 0.191848 | `extension_azqr` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.548617 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269080 | `communication_email_send` | ❌ |
| 3 | 0.192340 | `extension_azqr` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.185916 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.170749 | `foundry_agents_query-and-evaluate` | ❌ |

---

<<<<<<< HEAD
## Test 121
=======
## Test 116
=======
| 1 | 0.554908 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269080 | `communication_email_send` | ❌ |
| 3 | 0.191848 | `extension_azqr` | ❌ |
| 4 | 0.170743 | `foundry_agents_query-and-evaluate` | ❌ |
| 5 | 0.166385 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 126
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS message with custom tracking tag "campaign1"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.538893 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269915 | `communication_email_send` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.534739 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269794 | `communication_email_send` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.188153 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.185403 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.175135 | `foundry_agents_create` | ❌ |

---

<<<<<<< HEAD
## Test 122
=======
## Test 117
=======
| 1 | 0.538827 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.269794 | `communication_email_send` | ❌ |
| 3 | 0.188153 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.159177 | `appconfig_kv_set` | ❌ |
| 5 | 0.158295 | `loadtesting_test_create` | ❌ |

---

## Test 127
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send broadcast SMS to <phone-number-1> and <phone-number-2> saying "Urgent notification"  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.474775 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.286381 | `communication_email_send` | ❌ |
| 3 | 0.164341 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.147338 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.128704 | `cosmos_account_list` | ❌ |

---

## Test 123
=======
<<<<<<< HEAD
| 1 | 0.471991 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.286936 | `communication_email_send` | ❌ |
| 3 | 0.164059 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.146501 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.128592 | `cosmos_account_list` | ❌ |

---

## Test 118
=======
| 1 | 0.474786 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.286338 | `communication_email_send` | ❌ |
| 3 | 0.164288 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.129965 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.128744 | `cosmos_account_list` | ❌ |

---

## Test 128
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send SMS from my communication service to <phone-number-1>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.564058 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.302377 | `communication_email_send` | ❌ |
| 3 | 0.238340 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.184240 | `foundry_agents_create` | ❌ |
| 5 | 0.183684 | `search_knowledge_base_retrieve` | ❌ |

---

## Test 124
=======
<<<<<<< HEAD
| 1 | 0.563359 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.302360 | `communication_email_send` | ❌ |
| 3 | 0.238341 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.183684 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.174092 | `foundry_openai_create-completion` | ❌ |

---

## Test 119
=======
| 1 | 0.564114 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.302363 | `communication_email_send` | ❌ |
| 3 | 0.213669 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.183651 | `search_knowledge_base_retrieve` | ❌ |
| 5 | 0.177315 | `appservice_database_add` | ❌ |

---

## Test 129
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `communication_sms_send`  
**Prompt:** Send an SMS with delivery receipt tracking  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.598236 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.314267 | `communication_email_send` | ❌ |
| 3 | 0.206931 | `foundry_agents_query-and-evaluate` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.592519 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.314134 | `communication_email_send` | ❌ |
| 3 | 0.206916 | `foundry_agents_query-and-evaluate` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.201142 | `foundry_openai_chat-completions-create` | ❌ |
| 5 | 0.187824 | `confidentialledger_entries_append` | ❌ |

---

<<<<<<< HEAD
## Test 125
=======
## Test 120
=======
| 1 | 0.598211 | `communication_sms_send` | ✅ **EXPECTED** |
| 2 | 0.314134 | `communication_email_send` | ❌ |
| 3 | 0.206814 | `foundry_agents_query-and-evaluate` | ❌ |
| 4 | 0.187824 | `confidentialledger_entries_append` | ❌ |
| 5 | 0.181824 | `foundry_openai_chat-completions-create` | ❌ |

---

## Test 130
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append an entry to my ledger <ledger_name> with data {"key": "value"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.511241 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.295319 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.291757 | `appconfig_kv_set` | ❌ |
| 4 | 0.258741 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.250106 | `keyvault_certificate_import` | ❌ |

---

## Test 126
=======
<<<<<<< HEAD
| 1 | 0.510689 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.293736 | `confidentialledger_entries_get` | ❌ |
=======
| 1 | 0.510651 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.294885 | `confidentialledger_entries_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.292014 | `appconfig_kv_set` | ❌ |
| 4 | 0.258967 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.249704 | `keyvault_certificate_import` | ❌ |

---

<<<<<<< HEAD
## Test 121
=======
## Test 131
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write a tamper-proof entry to ledger <ledger_name> containing {"transaction": "data"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.602321 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.357401 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.211998 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.195461 | `keyvault_secret_create` | ❌ |
| 5 | 0.184070 | `keyvault_certificate_import` | ❌ |

---

## Test 127
=======
| 1 | 0.602257 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.356510 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.211990 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.195471 | `keyvault_secret_create` | ❌ |
| 5 | 0.183820 | `keyvault_certificate_import` | ❌ |

---

<<<<<<< HEAD
## Test 122
=======
## Test 132
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Append {"hello": "from mcp"} to my confidential ledger <ledger_name> in collection <collection_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.546786 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.452117 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.225013 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.215828 | `appconfig_kv_set` | ❌ |
| 5 | 0.203162 | `keyvault_certificate_import` | ❌ |

---

## Test 128
=======
<<<<<<< HEAD
| 1 | 0.546573 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.451031 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.224978 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.215862 | `appconfig_kv_set` | ❌ |
| 5 | 0.203109 | `keyvault_certificate_import` | ❌ |

---

## Test 123
=======
| 1 | 0.546675 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.452058 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.225145 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.215898 | `appconfig_kv_set` | ❌ |
| 5 | 0.211661 | `appservice_database_add` | ❌ |

---

## Test 133
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Create an immutable ledger entry in <ledger_name> with content {"audit": "log"}  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.496023 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.340187 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.218473 | `monitor_activitylog_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.496032 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.338270 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.218518 | `monitor_activitylog_list` | ❌ |
=======
| 1 | 0.496023 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.340187 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.218473 | `monitor_activitylog_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.215229 | `storage_blob_container_create` | ❌ |
| 5 | 0.204925 | `monitor_resource_log_query` | ❌ |

---

<<<<<<< HEAD
## Test 129
=======
<<<<<<< HEAD
## Test 124
=======
## Test 134
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `confidentialledger_entries_append`  
**Prompt:** Write an entry to confidential ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622138 | `confidentialledger_entries_append` | ✅ **EXPECTED** |
| 2 | 0.524777 | `confidentialledger_entries_get` | ❌ |
| 3 | 0.252508 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.240252 | `keyvault_secret_create` | ❌ |
| 5 | 0.186890 | `appconfig_kv_set` | ❌ |

---

<<<<<<< HEAD
## Test 130
=======
<<<<<<< HEAD
## Test 125
=======
## Test 135
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get entry from Confidential Ledger for transaction <transaction_id> on ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.707252 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.551953 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.245549 | `keyvault_secret_get` | ❌ |
| 4 | 0.231190 | `keyvault_key_get` | ❌ |
| 5 | 0.211839 | `loadtesting_testrun_get` | ❌ |

---

## Test 131
=======
<<<<<<< HEAD
| 1 | 0.706506 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.551901 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.245541 | `keyvault_secret_get` | ❌ |
| 4 | 0.229943 | `keyvault_key_get` | ❌ |
| 5 | 0.212658 | `loadtesting_testrun_get` | ❌ |

---

## Test 126
=======
| 1 | 0.707252 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.551953 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.245541 | `keyvault_secret_get` | ❌ |
| 4 | 0.229943 | `keyvault_key_get` | ❌ |
| 5 | 0.211925 | `loadtesting_testrun_get` | ❌ |

---

## Test 136
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `confidentialledger_entries_get`  
**Prompt:** Get transaction <transaction_id> from ledger <ledger_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.509714 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.416580 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.223959 | `loadtesting_testrun_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.510283 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.416550 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.224523 | `loadtesting_testrun_get` | ❌ |
=======
| 1 | 0.509714 | `confidentialledger_entries_get` | ✅ **EXPECTED** |
| 2 | 0.416580 | `confidentialledger_entries_append` | ❌ |
| 3 | 0.224029 | `loadtesting_testrun_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.218412 | `monitor_resource_log_query` | ❌ |
| 5 | 0.217671 | `loadtesting_testrun_list` | ❌ |

---

<<<<<<< HEAD
## Test 132
=======
<<<<<<< HEAD
## Test 127
=======
## Test 137
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cosmos_account_list`  
**Prompt:** List all cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.818340 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.668480 | `cosmos_database_list` | ❌ |
| 3 | 0.636009 | `subscription_list` | ❌ |
| 4 | 0.615268 | `cosmos_database_container_list` | ❌ |
<<<<<<< HEAD
| 5 | 0.601467 | `kusto_cluster_list` | ❌ |

---

<<<<<<< HEAD
## Test 133
=======
## Test 128
=======
| 5 | 0.601388 | `kusto_cluster_list` | ❌ |

---

## Test 138
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me my cosmosdb accounts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.665422 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605325 | `cosmos_database_list` | ❌ |
| 3 | 0.571573 | `cosmos_database_container_list` | ❌ |
| 4 | 0.549420 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.503865 | `storage_account_get` | ❌ |

---

## Test 134
=======
| 1 | 0.665440 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.605357 | `cosmos_database_list` | ❌ |
| 3 | 0.571613 | `cosmos_database_container_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.549476 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.504032 | `storage_account_get` | ❌ |

---

## Test 129
=======
| 4 | 0.549447 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.503850 | `storage_account_get` | ❌ |

---

## Test 139
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cosmos_account_list`  
**Prompt:** Show me the cosmosdb accounts in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.752494 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.607165 | `subscription_list` | ❌ |
| 3 | 0.605125 | `cosmos_database_list` | ❌ |
| 4 | 0.566249 | `cosmos_database_container_list` | ❌ |
| 5 | 0.563922 | `cosmos_database_container_item_query` | ❌ |

---

<<<<<<< HEAD
## Test 135
=======
## Test 130
=======
| 1 | 0.752501 | `cosmos_account_list` | ✅ **EXPECTED** |
| 2 | 0.607201 | `subscription_list` | ❌ |
| 3 | 0.605125 | `cosmos_database_list` | ❌ |
| 4 | 0.566249 | `cosmos_database_container_list` | ❌ |
| 5 | 0.563921 | `cosmos_database_container_item_query` | ❌ |

---

## Test 140
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cosmos_database_container_item_query`  
**Prompt:** Show me the items that contain the word <search_term> in the container <container_name> in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.658701 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 1 | 0.658738 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
=======
| 1 | 0.658701 | `cosmos_database_container_item_query` | ✅ **EXPECTED** |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.605253 | `cosmos_database_container_list` | ❌ |
| 3 | 0.488353 | `storage_blob_container_get` | ❌ |
| 4 | 0.477874 | `cosmos_database_list` | ❌ |
| 5 | 0.447777 | `cosmos_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 136
=======
<<<<<<< HEAD
## Test 131
=======
## Test 141
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** List all the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.852875 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.680991 | `cosmos_database_list` | ❌ |
| 3 | 0.680758 | `cosmos_database_container_item_query` | ❌ |
| 4 | 0.632634 | `storage_blob_container_get` | ❌ |
| 5 | 0.630588 | `cosmos_account_list` | ❌ |

---

## Test 137
=======
| 1 | 0.852832 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.681044 | `cosmos_database_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.680794 | `cosmos_database_container_item_query` | ❌ |
=======
| 3 | 0.680762 | `cosmos_database_container_item_query` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.632335 | `storage_blob_container_get` | ❌ |
| 5 | 0.630597 | `cosmos_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 132
=======
## Test 142
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cosmos_database_container_list`  
**Prompt:** Show me the containers in the database <database_name> for the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.789395 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.648126 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.614220 | `cosmos_database_list` | ❌ |
| 4 | 0.591350 | `storage_blob_container_get` | ❌ |
| 5 | 0.562062 | `cosmos_account_list` | ❌ |

---

## Test 138
=======
<<<<<<< HEAD
| 1 | 0.789413 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.648207 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.614278 | `cosmos_database_list` | ❌ |
| 4 | 0.591387 | `storage_blob_container_get` | ❌ |
| 5 | 0.562096 | `cosmos_account_list` | ❌ |

---

## Test 133
=======
| 1 | 0.789395 | `cosmos_database_container_list` | ✅ **EXPECTED** |
| 2 | 0.648126 | `cosmos_database_container_item_query` | ❌ |
| 3 | 0.614220 | `cosmos_database_list` | ❌ |
| 4 | 0.591361 | `storage_blob_container_get` | ❌ |
| 5 | 0.562033 | `cosmos_account_list` | ❌ |

---

## Test 143
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cosmos_database_list`  
**Prompt:** List all the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815683 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.668468 | `cosmos_account_list` | ❌ |
| 3 | 0.665298 | `cosmos_database_container_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.606433 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.582804 | `kusto_database_list` | ❌ |

---

## Test 139
=======
<<<<<<< HEAD
| 4 | 0.606414 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.583507 | `kusto_database_list` | ❌ |

---

## Test 134
=======
| 4 | 0.606433 | `cosmos_database_container_item_query` | ❌ |
| 5 | 0.583097 | `kusto_database_list` | ❌ |

---

## Test 144
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cosmos_database_list`  
**Prompt:** Show me the databases in the cosmosdb account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.749370 | `cosmos_database_list` | ✅ **EXPECTED** |
| 2 | 0.624759 | `cosmos_database_container_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.614572 | `cosmos_account_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.579919 | `cosmos_database_container_item_query` | ❌ |
=======
| 4 | 0.579913 | `cosmos_database_container_item_query` | ❌ |
=======
| 3 | 0.614554 | `cosmos_account_list` | ❌ |
| 4 | 0.579919 | `cosmos_database_container_item_query` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.538479 | `mysql_database_list` | ❌ |

---

<<<<<<< HEAD
## Test 140
=======
<<<<<<< HEAD
## Test 135
=======
## Test 145
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_cluster_get`  
**Prompt:** Show me the details of the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590264 | `kusto_cluster_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.463832 | `kusto_cluster_list` | ❌ |
| 3 | 0.428159 | `kusto_query` | ❌ |
<<<<<<< HEAD
| 4 | 0.425909 | `kusto_database_list` | ❌ |
=======
| 4 | 0.425688 | `kusto_database_list` | ❌ |
=======
| 2 | 0.463623 | `kusto_cluster_list` | ❌ |
| 3 | 0.428159 | `kusto_query` | ❌ |
| 4 | 0.425469 | `kusto_database_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.422577 | `kusto_table_schema` | ❌ |

---

<<<<<<< HEAD
## Test 141
=======
<<<<<<< HEAD
## Test 136
=======
## Test 146
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** List all Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.793744 | `kusto_cluster_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.630451 | `kusto_database_list` | ❌ |
=======
| 2 | 0.630504 | `kusto_database_list` | ❌ |
=======
| 1 | 0.793453 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.630261 | `kusto_database_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.573395 | `kusto_cluster_get` | ❌ |
| 4 | 0.525025 | `aks_cluster_get` | ❌ |
| 5 | 0.509397 | `grafana_list` | ❌ |

---

<<<<<<< HEAD
## Test 142
=======
<<<<<<< HEAD
## Test 137
=======
## Test 147
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me my Data Explorer clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.531307 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.465277 | `kusto_cluster_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.432311 | `kusto_database_list` | ❌ |
=======
| 3 | 0.432320 | `kusto_database_list` | ❌ |
=======
| 1 | 0.530932 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.465277 | `kusto_cluster_get` | ❌ |
| 3 | 0.432552 | `kusto_database_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.369596 | `aks_cluster_get` | ❌ |
| 5 | 0.363119 | `kusto_table_schema` | ❌ |

---

<<<<<<< HEAD
## Test 143
=======
<<<<<<< HEAD
## Test 138
=======
## Test 148
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_cluster_list`  
**Prompt:** Show me the Data Explorer clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.701484 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.571191 | `kusto_cluster_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.548734 | `kusto_database_list` | ❌ |
=======
| 3 | 0.548690 | `kusto_database_list` | ❌ |
=======
| 1 | 0.701232 | `kusto_cluster_list` | ✅ **EXPECTED** |
| 2 | 0.571191 | `kusto_cluster_get` | ❌ |
| 3 | 0.548589 | `kusto_database_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.498909 | `aks_cluster_get` | ❌ |
| 5 | 0.474201 | `redis_list` | ❌ |

---

<<<<<<< HEAD
## Test 144
=======
<<<<<<< HEAD
## Test 139
=======
## Test 149
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_database_list`  
**Prompt:** List all databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.676656 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.560592 | `kusto_cluster_list` | ❌ |
| 3 | 0.556795 | `kusto_table_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.677042 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.560592 | `kusto_cluster_list` | ❌ |
| 3 | 0.556688 | `kusto_table_list` | ❌ |
=======
| 1 | 0.676699 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.560388 | `kusto_cluster_list` | ❌ |
| 3 | 0.556795 | `kusto_table_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.553218 | `postgres_database_list` | ❌ |
| 5 | 0.549673 | `cosmos_database_list` | ❌ |

---

<<<<<<< HEAD
## Test 145
=======
<<<<<<< HEAD
## Test 140
=======
## Test 150
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_database_list`  
**Prompt:** Show me the databases in the Data Explorer cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.623242 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.509952 | `kusto_cluster_list` | ❌ |
| 3 | 0.507073 | `kusto_table_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.623528 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.509953 | `kusto_cluster_list` | ❌ |
| 3 | 0.506997 | `kusto_table_list` | ❌ |
=======
| 1 | 0.623401 | `kusto_database_list` | ✅ **EXPECTED** |
| 2 | 0.509763 | `kusto_cluster_list` | ❌ |
| 3 | 0.507073 | `kusto_table_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.497144 | `cosmos_database_list` | ❌ |
| 5 | 0.491400 | `mysql_database_list` | ❌ |

---

<<<<<<< HEAD
## Test 146
=======
<<<<<<< HEAD
## Test 141
=======
## Test 151
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_query`  
**Prompt:** Show me all items that contain the word <search_term> in the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423660 | `kusto_query` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.409485 | `postgres_database_query` | ❌ |
| 3 | 0.408178 | `kusto_table_schema` | ❌ |
| 4 | 0.407740 | `kusto_sample` | ❌ |
| 5 | 0.403989 | `kusto_cluster_list` | ❌ |

---

## Test 147
=======
| 2 | 0.409534 | `postgres_database_query` | ❌ |
| 3 | 0.408178 | `kusto_table_schema` | ❌ |
| 4 | 0.407741 | `kusto_sample` | ❌ |
<<<<<<< HEAD
| 5 | 0.403990 | `kusto_cluster_list` | ❌ |

---

## Test 142
=======
| 5 | 0.403800 | `kusto_cluster_list` | ❌ |

---

## Test 152
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_sample`  
**Prompt:** Show me a data sample from the Data Explorer table <table_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.595554 | `kusto_sample` | ✅ **EXPECTED** |
| 2 | 0.510233 | `kusto_table_schema` | ❌ |
<<<<<<< HEAD
| 3 | 0.424212 | `kusto_table_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.424221 | `kusto_table_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.400924 | `kusto_cluster_list` | ❌ |
=======
| 3 | 0.424212 | `kusto_table_list` | ❌ |
| 4 | 0.400719 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.399525 | `kusto_cluster_get` | ❌ |

---

<<<<<<< HEAD
## Test 148
=======
<<<<<<< HEAD
## Test 143
=======
## Test 153
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_table_list`  
**Prompt:** List all tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.679642 | `kusto_table_list` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 1 | 0.679655 | `kusto_table_list` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.585237 | `postgres_table_list` | ❌ |
| 3 | 0.580964 | `kusto_database_list` | ❌ |
| 4 | 0.556724 | `mysql_table_list` | ❌ |
| 5 | 0.550005 | `monitor_table_list` | ❌ |

---

<<<<<<< HEAD
## Test 149
=======
## Test 144
=======
| 1 | 0.679642 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.585237 | `postgres_table_list` | ❌ |
| 3 | 0.581015 | `kusto_database_list` | ❌ |
| 4 | 0.556724 | `mysql_table_list` | ❌ |
| 5 | 0.549762 | `monitor_table_list` | ❌ |

---

## Test 154
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_table_list`  
**Prompt:** Show me the tables in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.619252 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.554332 | `kusto_table_schema` | ❌ |
| 3 | 0.527431 | `kusto_database_list` | ❌ |
| 4 | 0.524691 | `mysql_table_list` | ❌ |
=======
| 1 | 0.619269 | `kusto_table_list` | ✅ **EXPECTED** |
| 2 | 0.554333 | `kusto_table_schema` | ❌ |
<<<<<<< HEAD
| 3 | 0.527616 | `kusto_database_list` | ❌ |
| 4 | 0.524607 | `mysql_table_list` | ❌ |
=======
| 3 | 0.527570 | `kusto_database_list` | ❌ |
| 4 | 0.524691 | `mysql_table_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.523432 | `postgres_table_list` | ❌ |

---

<<<<<<< HEAD
## Test 150
=======
<<<<<<< HEAD
## Test 145
=======
## Test 155
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `kusto_table_schema`  
**Prompt:** Show me the schema for table <table_name> in the Data Explorer database <database_name> in cluster <cluster_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.666980 | `kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564204 | `postgres_table_schema_get` | ❌ |
| 3 | 0.528301 | `mysql_table_schema_get` | ❌ |
| 4 | 0.490892 | `kusto_sample` | ❌ |
| 5 | 0.489745 | `kusto_table_list` | ❌ |

---

## Test 151
=======
<<<<<<< HEAD
| 1 | 0.667033 | `kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564282 | `postgres_table_schema_get` | ❌ |
| 3 | 0.527921 | `mysql_table_schema_get` | ❌ |
| 4 | 0.490939 | `kusto_sample` | ❌ |
| 5 | 0.489722 | `kusto_table_list` | ❌ |

---

## Test 146
=======
| 1 | 0.667095 | `kusto_table_schema` | ✅ **EXPECTED** |
| 2 | 0.564717 | `postgres_table_schema_get` | ❌ |
| 3 | 0.528210 | `mysql_table_schema_get` | ❌ |
| 4 | 0.490775 | `kusto_sample` | ❌ |
| 5 | 0.489814 | `kusto_table_list` | ❌ |

---

## Test 156
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_database_list`  
**Prompt:** List all MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.633991 | `postgres_database_list` | ❌ |
| 2 | 0.623359 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534434 | `mysql_table_list` | ❌ |
| 4 | 0.498902 | `mysql_server_list` | ❌ |
| 5 | 0.490102 | `sql_db_list` | ❌ |

---

## Test 152
=======
<<<<<<< HEAD
| 1 | 0.633973 | `postgres_database_list` | ❌ |
| 2 | 0.623333 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534537 | `mysql_table_list` | ❌ |
| 4 | 0.498854 | `mysql_server_list` | ❌ |
| 5 | 0.490179 | `sql_db_list` | ❌ |

---

## Test 147
=======
| 1 | 0.634056 | `postgres_database_list` | ❌ |
| 2 | 0.623421 | `mysql_database_list` | ✅ **EXPECTED** |
| 3 | 0.534457 | `mysql_table_list` | ❌ |
| 4 | 0.498918 | `mysql_server_list` | ❌ |
| 5 | 0.490148 | `sql_db_list` | ❌ |

---

## Test 157
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_database_list`  
**Prompt:** Show me the MySQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588121 | `mysql_database_list` | ✅ **EXPECTED** |
| 2 | 0.574089 | `postgres_database_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.483855 | `mysql_table_list` | ❌ |
| 4 | 0.463244 | `mysql_server_list` | ❌ |
| 5 | 0.444547 | `sql_db_list` | ❌ |

---

## Test 153
=======
<<<<<<< HEAD
| 3 | 0.483938 | `mysql_table_list` | ❌ |
| 4 | 0.463238 | `mysql_server_list` | ❌ |
| 5 | 0.444622 | `sql_db_list` | ❌ |

---

## Test 148
=======
| 3 | 0.483855 | `mysql_table_list` | ❌ |
| 4 | 0.463244 | `mysql_server_list` | ❌ |
| 5 | 0.444547 | `sql_db_list` | ❌ |

---

## Test 158
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.476423 | `mysql_table_list` | ❌ |
| 2 | 0.455770 | `mysql_database_list` | ❌ |
| 3 | 0.432703 | `mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419859 | `mysql_server_list` | ❌ |
| 5 | 0.409655 | `mysql_table_schema_get` | ❌ |

---

## Test 154
=======
<<<<<<< HEAD
| 1 | 0.476539 | `mysql_table_list` | ❌ |
| 2 | 0.455770 | `mysql_database_list` | ❌ |
| 3 | 0.433392 | `mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419938 | `mysql_server_list` | ❌ |
=======
| 1 | 0.476423 | `mysql_table_list` | ❌ |
| 2 | 0.455770 | `mysql_database_list` | ❌ |
| 3 | 0.433202 | `mysql_database_query` | ✅ **EXPECTED** |
| 4 | 0.419859 | `mysql_server_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.409445 | `mysql_table_schema_get` | ❌ |

---

<<<<<<< HEAD
## Test 149
=======
## Test 159
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_server_config_get`  
**Prompt:** Show me the configuration of MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.531964 | `postgres_server_config_get` | ❌ |
| 2 | 0.517385 | `mysql_server_param_set` | ❌ |
| 3 | 0.489870 | `mysql_server_config_get` | ✅ **EXPECTED** |
| 4 | 0.476944 | `mysql_server_param_get` | ❌ |
| 5 | 0.426840 | `mysql_table_schema_get` | ❌ |

---

<<<<<<< HEAD
## Test 155
=======
<<<<<<< HEAD
## Test 150
=======
## Test 160
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_server_list`  
**Prompt:** List all MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.678473 | `postgres_server_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.678536 | `postgres_server_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.558177 | `mysql_database_list` | ❌ |
| 3 | 0.554818 | `mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.513706 | `kusto_cluster_list` | ❌ |
| 5 | 0.501199 | `mysql_table_list` | ❌ |

---

<<<<<<< HEAD
## Test 156
=======
## Test 151
=======
| 1 | 0.678472 | `postgres_server_list` | ❌ |
| 2 | 0.558177 | `mysql_database_list` | ❌ |
| 3 | 0.554817 | `mysql_server_list` | ✅ **EXPECTED** |
| 4 | 0.513750 | `kusto_cluster_list` | ❌ |
| 5 | 0.501199 | `mysql_table_list` | ❌ |

---

## Test 161
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me my MySQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.478518 | `mysql_database_list` | ❌ |
<<<<<<< HEAD
| 2 | 0.474586 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `postgres_server_list` | ❌ |
| 4 | 0.412380 | `mysql_table_list` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.474630 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435692 | `postgres_server_list` | ❌ |
| 4 | 0.412417 | `mysql_table_list` | ❌ |
=======
| 2 | 0.474586 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.435642 | `postgres_server_list` | ❌ |
| 4 | 0.412380 | `mysql_table_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.389993 | `postgres_database_list` | ❌ |

---

<<<<<<< HEAD
## Test 157
=======
<<<<<<< HEAD
## Test 152
=======
## Test 162
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_server_list`  
**Prompt:** Show me the MySQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.636435 | `postgres_server_list` | ❌ |
| 2 | 0.534266 | `mysql_server_list` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 1 | 0.636471 | `postgres_server_list` | ❌ |
| 2 | 0.534277 | `mysql_server_list` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.530210 | `mysql_database_list` | ❌ |
| 4 | 0.475052 | `kusto_cluster_list` | ❌ |
=======
| 1 | 0.636435 | `postgres_server_list` | ❌ |
| 2 | 0.534266 | `mysql_server_list` | ✅ **EXPECTED** |
| 3 | 0.530210 | `mysql_database_list` | ❌ |
| 4 | 0.475138 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.470468 | `redis_list` | ❌ |

---

<<<<<<< HEAD
## Test 158
=======
<<<<<<< HEAD
## Test 153
=======
## Test 163
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_server_param_get`  
**Prompt:** Show me the value of connection timeout in seconds in my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.495071 | `mysql_server_param_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.437857 | `mysql_server_param_set` | ❌ |
| 3 | 0.333041 | `mysql_database_query` | ❌ |
| 4 | 0.313364 | `mysql_table_schema_get` | ❌ |
| 5 | 0.310856 | `postgres_server_param_get` | ❌ |

---

## Test 159
=======
| 2 | 0.438075 | `mysql_server_param_set` | ❌ |
| 3 | 0.333906 | `mysql_database_query` | ❌ |
| 4 | 0.313150 | `mysql_table_schema_get` | ❌ |
| 5 | 0.310834 | `postgres_server_param_get` | ❌ |

---

<<<<<<< HEAD
## Test 154
=======
## Test 164
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_server_param_set`  
**Prompt:** Set connection timeout to 20 seconds for my MySQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.449612 | `mysql_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.381144 | `mysql_server_param_get` | ❌ |
| 3 | 0.303499 | `postgres_server_param_set` | ❌ |
<<<<<<< HEAD
| 4 | 0.298661 | `mysql_database_query` | ❌ |
| 5 | 0.254180 | `mysql_server_list` | ❌ |

---

## Test 160
=======
<<<<<<< HEAD
| 4 | 0.298911 | `mysql_database_query` | ❌ |
| 5 | 0.254206 | `mysql_server_list` | ❌ |

---

## Test 155
=======
| 4 | 0.299246 | `mysql_database_query` | ❌ |
| 5 | 0.277569 | `appservice_database_add` | ❌ |

---

## Test 165
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_table_list`  
**Prompt:** List all tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.633542 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573851 | `postgres_table_list` | ❌ |
| 3 | 0.550878 | `postgres_database_list` | ❌ |
| 4 | 0.546988 | `mysql_database_list` | ❌ |
| 5 | 0.511879 | `kusto_table_list` | ❌ |

---

## Test 161
=======
<<<<<<< HEAD
| 1 | 0.633547 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573844 | `postgres_table_list` | ❌ |
| 3 | 0.550898 | `postgres_database_list` | ❌ |
| 4 | 0.546963 | `mysql_database_list` | ❌ |
| 5 | 0.511906 | `kusto_table_list` | ❌ |

---

## Test 156
=======
| 1 | 0.633542 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.573851 | `postgres_table_list` | ❌ |
| 3 | 0.550878 | `postgres_database_list` | ❌ |
| 4 | 0.546987 | `mysql_database_list` | ❌ |
| 5 | 0.511879 | `kusto_table_list` | ❌ |

---

## Test 166
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_table_list`  
**Prompt:** Show me the tables in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609131 | `mysql_table_list` | ✅ **EXPECTED** |
| 2 | 0.526236 | `postgres_table_list` | ❌ |
| 3 | 0.525709 | `mysql_database_list` | ❌ |
| 4 | 0.507532 | `mysql_table_schema_get` | ❌ |
| 5 | 0.498050 | `postgres_database_list` | ❌ |

---

<<<<<<< HEAD
## Test 162
=======
<<<<<<< HEAD
## Test 157
=======
## Test 167
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `mysql_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the MySQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630824 | `mysql_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.558306 | `postgres_table_schema_get` | ❌ |
| 3 | 0.545025 | `mysql_table_list` | ❌ |
| 4 | 0.517419 | `kusto_table_schema` | ❌ |
| 5 | 0.457739 | `mysql_database_list` | ❌ |

---

<<<<<<< HEAD
## Test 163
=======
<<<<<<< HEAD
## Test 158
=======
## Test 168
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_database_list`  
**Prompt:** List all PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.815470 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.643680 | `postgres_table_list` | ❌ |
| 3 | 0.622824 | `postgres_server_list` | ❌ |
| 4 | 0.542912 | `postgres_server_config_get` | ❌ |
| 5 | 0.490950 | `postgres_server_param_get` | ❌ |

---

<<<<<<< HEAD
## Test 164
=======
<<<<<<< HEAD
## Test 159
=======
## Test 169
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_database_list`  
**Prompt:** Show me the PostgreSQL databases in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.760033 | `postgres_database_list` | ✅ **EXPECTED** |
| 2 | 0.589784 | `postgres_server_list` | ❌ |
| 3 | 0.585891 | `postgres_table_list` | ❌ |
| 4 | 0.552660 | `postgres_server_config_get` | ❌ |
| 5 | 0.495685 | `postgres_server_param_get` | ❌ |

---

<<<<<<< HEAD
## Test 165
=======
<<<<<<< HEAD
## Test 160
=======
## Test 170
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_database_query`  
**Prompt:** Show me all items that contain the word <search_term> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.546211 | `postgres_database_list` | ❌ |
<<<<<<< HEAD
| 2 | 0.523223 | `postgres_database_query` | ✅ **EXPECTED** |
| 3 | 0.503267 | `postgres_table_list` | ❌ |
| 4 | 0.466599 | `postgres_server_list` | ❌ |
| 5 | 0.403963 | `postgres_server_param_get` | ❌ |

---

## Test 166
=======
<<<<<<< HEAD
| 2 | 0.523142 | `postgres_database_query` | ✅ **EXPECTED** |
| 3 | 0.503267 | `postgres_table_list` | ❌ |
| 4 | 0.466608 | `postgres_server_list` | ❌ |
=======
| 2 | 0.523122 | `postgres_database_query` | ✅ **EXPECTED** |
| 3 | 0.503267 | `postgres_table_list` | ❌ |
| 4 | 0.466599 | `postgres_server_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.403969 | `postgres_server_param_get` | ❌ |

---

<<<<<<< HEAD
## Test 161
=======
## Test 171
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_server_config_get`  
**Prompt:** Show me the configuration of PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756593 | `postgres_server_config_get` | ✅ **EXPECTED** |
| 2 | 0.615429 | `postgres_server_param_set` | ❌ |
| 3 | 0.599487 | `postgres_server_param_get` | ❌ |
| 4 | 0.535050 | `postgres_database_list` | ❌ |
| 5 | 0.518574 | `postgres_server_list` | ❌ |

---

<<<<<<< HEAD
## Test 167
=======
<<<<<<< HEAD
## Test 162
=======
## Test 172
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_server_list`  
**Prompt:** List all PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.900023 | `postgres_server_list` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 1 | 0.900052 | `postgres_server_list` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.640733 | `postgres_database_list` | ❌ |
| 3 | 0.565914 | `postgres_table_list` | ❌ |
| 4 | 0.538997 | `postgres_server_config_get` | ❌ |
| 5 | 0.534239 | `kusto_cluster_list` | ❌ |

---

<<<<<<< HEAD
## Test 168
=======
## Test 163
=======
| 1 | 0.900023 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.640733 | `postgres_database_list` | ❌ |
| 3 | 0.565914 | `postgres_table_list` | ❌ |
| 4 | 0.538997 | `postgres_server_config_get` | ❌ |
| 5 | 0.534345 | `kusto_cluster_list` | ❌ |

---

## Test 173
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me my PostgreSQL servers  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.674327 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.607062 | `postgres_database_list` | ❌ |
| 3 | 0.576348 | `postgres_server_config_get` | ❌ |
| 4 | 0.522995 | `postgres_table_list` | ❌ |
| 5 | 0.506254 | `postgres_server_param_get` | ❌ |

---

<<<<<<< HEAD
## Test 169
=======
<<<<<<< HEAD
## Test 164
=======
## Test 174
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_server_list`  
**Prompt:** Show me the PostgreSQL servers in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.832155 | `postgres_server_list` | ✅ **EXPECTED** |
| 2 | 0.579232 | `postgres_database_list` | ❌ |
| 3 | 0.531804 | `postgres_server_config_get` | ❌ |
| 4 | 0.514445 | `postgres_table_list` | ❌ |
| 5 | 0.505978 | `postgres_server_param_get` | ❌ |

---

<<<<<<< HEAD
## Test 170
=======
<<<<<<< HEAD
## Test 165
=======
## Test 175
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_server_param_get`  
**Prompt:** Show me if the parameter my PostgreSQL server <server> has replication enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.594770 | `postgres_server_param_get` | ✅ **EXPECTED** |
| 2 | 0.552678 | `postgres_server_param_set` | ❌ |
| 3 | 0.539671 | `postgres_server_config_get` | ❌ |
| 4 | 0.489693 | `postgres_server_list` | ❌ |
| 5 | 0.451871 | `postgres_database_list` | ❌ |

---

<<<<<<< HEAD
## Test 171
=======
<<<<<<< HEAD
## Test 166
=======
## Test 176
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_server_param_set`  
**Prompt:** Enable replication for my PostgreSQL server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579909 | `postgres_server_param_set` | ✅ **EXPECTED** |
| 2 | 0.488496 | `postgres_server_config_get` | ❌ |
| 3 | 0.469810 | `postgres_server_list` | ❌ |
| 4 | 0.447051 | `postgres_server_param_get` | ❌ |
| 5 | 0.440716 | `postgres_database_list` | ❌ |

---

<<<<<<< HEAD
## Test 172
=======
<<<<<<< HEAD
## Test 167
=======
## Test 177
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_table_list`  
**Prompt:** List all tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.789934 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.750592 | `postgres_database_list` | ❌ |
| 3 | 0.574975 | `postgres_server_list` | ❌ |
| 4 | 0.519816 | `postgres_table_schema_get` | ❌ |
| 5 | 0.501361 | `postgres_server_config_get` | ❌ |

---

<<<<<<< HEAD
## Test 173
=======
<<<<<<< HEAD
## Test 168
=======
## Test 178
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_table_list`  
**Prompt:** Show me the tables in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.736083 | `postgres_table_list` | ✅ **EXPECTED** |
| 2 | 0.690112 | `postgres_database_list` | ❌ |
| 3 | 0.558357 | `postgres_table_schema_get` | ❌ |
| 4 | 0.543331 | `postgres_server_list` | ❌ |
| 5 | 0.521570 | `postgres_server_config_get` | ❌ |

---

<<<<<<< HEAD
## Test 174
=======
<<<<<<< HEAD
## Test 169
=======
## Test 179
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `postgres_table_schema_get`  
**Prompt:** Show me the schema of table <table> in the PostgreSQL database <database> in server <server>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.714916 | `postgres_table_schema_get` | ✅ **EXPECTED** |
| 2 | 0.597892 | `postgres_table_list` | ❌ |
| 3 | 0.574251 | `postgres_database_list` | ❌ |
| 4 | 0.508090 | `postgres_server_config_get` | ❌ |
| 5 | 0.502593 | `kusto_table_schema` | ❌ |

---

<<<<<<< HEAD
## Test 175
=======
<<<<<<< HEAD
## Test 170
=======
## Test 180
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `deploy_app_logs_get`  
**Prompt:** Show me the log of the application deployed by azd  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.711844 | `deploy_app_logs_get` | ✅ **EXPECTED** |
| 2 | 0.471692 | `deploy_plan_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.451639 | `monitor_activitylog_list` | ❌ |
| 4 | 0.404892 | `deploy_pipeline_guidance_get` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.451653 | `monitor_activitylog_list` | ❌ |
=======
| 3 | 0.451638 | `monitor_activitylog_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.404890 | `deploy_pipeline_guidance_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.401388 | `monitor_resource_log_query` | ❌ |

---

<<<<<<< HEAD
## Test 176
=======
<<<<<<< HEAD
## Test 171
=======
## Test 181
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `deploy_architecture_diagram_generate`  
**Prompt:** Generate the azure architecture diagram for this application  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.680599 | `deploy_architecture_diagram_generate` | ✅ **EXPECTED** |
| 2 | 0.562485 | `deploy_plan_get` | ❌ |
| 3 | 0.497326 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.489325 | `cloudarchitect_design` | ❌ |
| 5 | 0.435899 | `deploy_iac_rules_get` | ❌ |

---

<<<<<<< HEAD
## Test 177
=======
<<<<<<< HEAD
## Test 172
=======
## Test 182
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `deploy_iac_rules_get`  
**Prompt:** Show me the rules to generate bicep scripts  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529092 | `deploy_iac_rules_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.480324 | `bicepschema_get` | ❌ |
=======
| 2 | 0.479903 | `bicepschema_get` | ❌ |
<<<<<<< HEAD
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.391965 | `get_bestpractices_get` | ❌ |
| 4 | 0.383210 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.375561 | `extension_cli_generate` | ❌ |

---

<<<<<<< HEAD
## Test 178
=======
## Test 173
=======
| 3 | 0.394509 | `get_bestpractices_get` | ❌ |
| 4 | 0.383210 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.375561 | `extension_cli_generate` | ❌ |

---

## Test 183
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `deploy_pipeline_guidance_get`  
**Prompt:** How can I create a CI/CD pipeline to deploy this app to Azure?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.638588 | `deploy_pipeline_guidance_get` | ✅ **EXPECTED** |
| 2 | 0.499242 | `deploy_plan_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.448917 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.385670 | `deploy_app_logs_get` | ❌ |
| 5 | 0.382240 | `get_bestpractices_get` | ❌ |

---

## Test 179
=======
| 3 | 0.448918 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.385940 | `get_bestpractices_get` | ❌ |
| 5 | 0.385920 | `deploy_app_logs_get` | ❌ |

---

<<<<<<< HEAD
## Test 174
=======
## Test 184
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `deploy_plan_get`  
**Prompt:** Create a plan to deploy this application to azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688055 | `deploy_plan_get` | ✅ **EXPECTED** |
| 2 | 0.587963 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.499385 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.498575 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.448912 | `loadtesting_test_create` | ❌ |

---

<<<<<<< HEAD
## Test 180
=======
<<<<<<< HEAD
## Test 175
=======
## Test 185
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish an event to Event Grid topic <topic_name> using <event_schema> with the following data <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.755353 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.482544 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.465759 | `eventgrid_topic_list` | ❌ |
| 4 | 0.360686 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.355213 | `servicebus_topic_details` | ❌ |

---

## Test 181
=======
<<<<<<< HEAD
| 1 | 0.755366 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.482575 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.465432 | `eventgrid_topic_list` | ❌ |
| 4 | 0.360845 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.354313 | `servicebus_topic_details` | ❌ |

---

## Test 176
=======
| 1 | 0.755380 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.483021 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.466031 | `eventgrid_topic_list` | ❌ |
| 4 | 0.360676 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.355599 | `servicebus_topic_details` | ❌ |

---

## Test 186
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Publish event to my Event Grid topic <topic_name> with the following events <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.654648 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.524134 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.509777 | `eventgrid_topic_list` | ❌ |
| 4 | 0.373438 | `servicebus_topic_details` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.654647 | `eventgrid_events_publish` | ✅ **EXPECTED** |
=======
| 1 | 0.654668 | `eventgrid_events_publish` | ✅ **EXPECTED** |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.524503 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.510039 | `eventgrid_topic_list` | ❌ |
| 4 | 0.373718 | `servicebus_topic_details` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.359908 | `eventhubs_eventhub_update` | ❌ |

---

<<<<<<< HEAD
## Test 182
=======
<<<<<<< HEAD
## Test 177
=======
## Test 187
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_events_publish`  
**Prompt:** Send an event to Event Grid topic <topic_name> in resource group <resource_group_name> with <event_data>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.600274 | `eventgrid_events_publish` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.521041 | `eventgrid_topic_list` | ❌ |
| 3 | 0.504642 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.411129 | `eventhubs_eventhub_consumergroup_update` | ❌ |
=======
| 2 | 0.521240 | `eventgrid_topic_list` | ❌ |
| 3 | 0.504808 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.411390 | `eventhubs_eventhub_consumergroup_update` | ❌ |
=======
| 1 | 0.600303 | `eventgrid_events_publish` | ✅ **EXPECTED** |
| 2 | 0.521240 | `eventgrid_topic_list` | ❌ |
| 3 | 0.504808 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.411130 | `eventhubs_eventhub_consumergroup_update` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.389439 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

<<<<<<< HEAD
## Test 183
=======
<<<<<<< HEAD
## Test 178
=======
## Test 188
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.769921 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.745048 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.561862 | `kusto_cluster_list` | ❌ |
| 4 | 0.543887 | `search_service_list` | ❌ |
=======
| 1 | 0.770140 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.745470 | `eventgrid_subscription_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.561862 | `kusto_cluster_list` | ❌ |
=======
| 3 | 0.561858 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.545540 | `search_service_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.526123 | `subscription_list` | ❌ |

---

<<<<<<< HEAD
## Test 184
=======
<<<<<<< HEAD
## Test 179
=======
## Test 189
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** Show me the Event Grid topics in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.738040 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.736919 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.492592 | `kusto_cluster_list` | ❌ |
| 4 | 0.480252 | `subscription_list` | ❌ |
| 5 | 0.473459 | `search_service_list` | ❌ |

---

## Test 185
=======
| 1 | 0.738258 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.737486 | `eventgrid_subscription_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.492592 | `kusto_cluster_list` | ❌ |
| 4 | 0.480252 | `subscription_list` | ❌ |
=======
| 3 | 0.492527 | `kusto_cluster_list` | ❌ |
| 4 | 0.480287 | `subscription_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.475119 | `search_service_list` | ❌ |

---

<<<<<<< HEAD
## Test 180
=======
## Test 190
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.769840 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.720426 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.535369 | `kusto_cluster_list` | ❌ |
| 4 | 0.513921 | `search_service_list` | ❌ |
| 5 | 0.495939 | `subscription_list` | ❌ |

---

## Test 186
=======
| 1 | 0.770140 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.721362 | `eventgrid_subscription_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.535326 | `kusto_cluster_list` | ❌ |
=======
| 3 | 0.535427 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.514248 | `search_service_list` | ❌ |
| 5 | 0.495952 | `subscription_list` | ❌ |

---

<<<<<<< HEAD
## Test 181
=======
## Test 191
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_topic_list`  
**Prompt:** List all Event Grid topics in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.758562 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.704062 | `eventgrid_subscription_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.758816 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.704462 | `eventgrid_subscription_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.609175 | `group_list` | ❌ |
| 4 | 0.544809 | `monitor_webtests_list` | ❌ |
| 5 | 0.524209 | `eventhubs_namespace_get` | ❌ |

---

<<<<<<< HEAD
## Test 187
=======
## Test 182
=======
| 1 | 0.758786 | `eventgrid_topic_list` | ✅ **EXPECTED** |
| 2 | 0.704443 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.609074 | `group_list` | ❌ |
| 4 | 0.536981 | `monitor_webtests_list` | ❌ |
| 5 | 0.524359 | `eventhubs_namespace_get` | ❌ |

---

## Test 192
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show me all Event Grid subscriptions for topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.768696 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.720373 | `eventgrid_topic_list` | ❌ |
| 3 | 0.498398 | `servicebus_topic_details` | ❌ |
| 4 | 0.486216 | `servicebus_topic_subscription_details` | ❌ |
<<<<<<< HEAD
| 5 | 0.486162 | `eventgrid_events_publish` | ❌ |

---

<<<<<<< HEAD
## Test 188
=======
## Test 183
=======
| 5 | 0.486132 | `eventgrid_events_publish` | ❌ |

---

## Test 193
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.717676 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.709586 | `eventgrid_topic_list` | ❌ |
| 3 | 0.539977 | `servicebus_topic_subscription_details` | ❌ |
<<<<<<< HEAD
| 4 | 0.529084 | `servicebus_topic_details` | ❌ |
=======
| 4 | 0.529286 | `servicebus_topic_details` | ❌ |
<<<<<<< HEAD
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.477876 | `eventgrid_events_publish` | ❌ |

---

<<<<<<< HEAD
## Test 189
=======
## Test 184
=======
| 5 | 0.477848 | `eventgrid_events_publish` | ❌ |

---

## Test 194
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for topic <topic_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.746672 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.745851 | `eventgrid_topic_list` | ❌ |
| 3 | 0.535463 | `monitor_webtests_list` | ❌ |
| 4 | 0.524802 | `group_list` | ❌ |
| 5 | 0.502884 | `servicebus_topic_details` | ❌ |

---

## Test 190
=======
<<<<<<< HEAD
| 1 | 0.746815 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.746174 | `eventgrid_topic_list` | ❌ |
| 3 | 0.535731 | `monitor_webtests_list` | ❌ |
| 4 | 0.524919 | `group_list` | ❌ |
| 5 | 0.503158 | `servicebus_topic_details` | ❌ |

---

## Test 185
=======
| 1 | 0.746335 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.745666 | `eventgrid_topic_list` | ❌ |
| 3 | 0.528105 | `monitor_webtests_list` | ❌ |
| 4 | 0.524883 | `group_list` | ❌ |
| 5 | 0.502820 | `servicebus_topic_details` | ❌ |

---

## Test 195
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show all Event Grid subscriptions in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.736844 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.659612 | `eventgrid_topic_list` | ❌ |
| 3 | 0.569255 | `subscription_list` | ❌ |
| 4 | 0.537922 | `kusto_cluster_list` | ❌ |
| 5 | 0.517276 | `search_service_list` | ❌ |

---

## Test 191
=======
| 1 | 0.736436 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.659727 | `eventgrid_topic_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.569256 | `subscription_list` | ❌ |
| 4 | 0.537922 | `kusto_cluster_list` | ❌ |
=======
| 3 | 0.569254 | `subscription_list` | ❌ |
| 4 | 0.537909 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.518857 | `search_service_list` | ❌ |

---

<<<<<<< HEAD
## Test 186
=======
## Test 196
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List all Event Grid subscriptions in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.684586 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.656227 | `eventgrid_topic_list` | ❌ |
| 3 | 0.542362 | `subscription_list` | ❌ |
| 4 | 0.521053 | `kusto_cluster_list` | ❌ |
| 5 | 0.510115 | `group_list` | ❌ |

---

## Test 192
=======
<<<<<<< HEAD
| 1 | 0.684444 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.656183 | `eventgrid_topic_list` | ❌ |
| 3 | 0.542320 | `subscription_list` | ❌ |
| 4 | 0.521015 | `kusto_cluster_list` | ❌ |
| 5 | 0.510024 | `group_list` | ❌ |

---

## Test 187
=======
| 1 | 0.684543 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.656277 | `eventgrid_topic_list` | ❌ |
| 3 | 0.542388 | `subscription_list` | ❌ |
| 4 | 0.521119 | `kusto_cluster_list` | ❌ |
| 5 | 0.510115 | `group_list` | ❌ |

---

## Test 197
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** Show Event Grid subscriptions in resource group <resource_group_name> in subscription <subscription>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.696332 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.691623 | `eventgrid_topic_list` | ❌ |
| 3 | 0.557573 | `group_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.510684 | `monitor_webtests_list` | ❌ |
| 5 | 0.504984 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 193
=======
<<<<<<< HEAD
| 4 | 0.510814 | `monitor_webtests_list` | ❌ |
| 5 | 0.505497 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 188
=======
| 4 | 0.504984 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.503099 | `monitor_webtests_list` | ❌ |

---

## Test 198
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventgrid_subscription_list`  
**Prompt:** List Event Grid subscriptions for subscription <subscription> in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710457 | `eventgrid_subscription_list` | ✅ **EXPECTED** |
| 2 | 0.642001 | `eventgrid_topic_list` | ❌ |
| 3 | 0.506618 | `subscription_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.476396 | `search_service_list` | ❌ |
=======
| 4 | 0.476763 | `search_service_list` | ❌ |
<<<<<<< HEAD
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.475782 | `kusto_cluster_list` | ❌ |

---

<<<<<<< HEAD
## Test 194
=======
## Test 189
=======
| 5 | 0.475718 | `kusto_cluster_list` | ❌ |

---

## Test 199
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_eventhub_consumergroup_delete`  
**Prompt:** Delete my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.766928 | `eventhubs_eventhub_consumergroup_delete` | ✅ **EXPECTED** |
| 2 | 0.675842 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.641112 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.633788 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.605465 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 195
=======
<<<<<<< HEAD
| 1 | 0.766896 | `eventhubs_eventhub_consumergroup_delete` | ✅ **EXPECTED** |
| 2 | 0.675127 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.641111 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.633848 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.605802 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 190
=======
| 1 | 0.767014 | `eventhubs_eventhub_consumergroup_delete` | ✅ **EXPECTED** |
| 2 | 0.675937 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.641200 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.631867 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.605622 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 200
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** List all consumer groups in my event hub <event_hub_name> in namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.738475 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.634517 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 3 | 0.626486 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.634345 | `eventhubs_eventhub_consumergroup_update` | ❌ |
=======
| 2 | 0.634517 | `eventhubs_eventhub_consumergroup_update` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.626485 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.606619 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.593098 | `eventhubs_eventhub_get` | ❌ |

---

<<<<<<< HEAD
## Test 196
=======
<<<<<<< HEAD
## Test 191
=======
## Test 201
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_eventhub_consumergroup_get`  
**Prompt:** Get the details of my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712861 | `eventhubs_eventhub_consumergroup_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.637170 | `eventhubs_eventhub_consumergroup_update` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.637418 | `eventhubs_eventhub_consumergroup_update` | ❌ |
=======
| 2 | 0.637170 | `eventhubs_eventhub_consumergroup_update` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.625913 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.576800 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.529940 | `eventhubs_eventhub_get` | ❌ |

---

<<<<<<< HEAD
## Test 197
=======
<<<<<<< HEAD
## Test 192
=======
## Test 202
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Create a new consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.756873 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.688248 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.669384 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.553692 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.544512 | `eventhubs_namespace_get` | ❌ |

---

## Test 198
=======
<<<<<<< HEAD
| 1 | 0.757520 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
=======
| 1 | 0.757614 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.688923 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 3 | 0.670026 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.554314 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.545003 | `eventhubs_namespace_get` | ❌ |

---

<<<<<<< HEAD
## Test 193
=======
## Test 203
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_eventhub_consumergroup_update`  
**Prompt:** Update my consumer group <consumer_group_name> in my event hub <event_hub_name>, namespace <namespace_name>, and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.739158 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.655927 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 3 | 0.642524 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.552602 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.524106 | `eventhubs_namespace_delete` | ❌ |

---

## Test 199
=======
<<<<<<< HEAD
| 1 | 0.739615 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.655951 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 3 | 0.642701 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.552830 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.524428 | `eventhubs_namespace_delete` | ❌ |

---

## Test 194
=======
| 1 | 0.738818 | `eventhubs_eventhub_consumergroup_update` | ✅ **EXPECTED** |
| 2 | 0.655610 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 3 | 0.642206 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.552216 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.523137 | `eventhubs_namespace_get` | ❌ |

---

## Test 204
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_eventhub_delete`  
**Prompt:** Delete my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.699266 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.688646 | `eventhubs_eventhub_delete` | ✅ **EXPECTED** |
| 3 | 0.627721 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.578653 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.552963 | `eventhubs_eventhub_get` | ❌ |

---

## Test 200
=======
<<<<<<< HEAD
| 1 | 0.699621 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.689171 | `eventhubs_eventhub_delete` | ✅ **EXPECTED** |
| 3 | 0.627887 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.579273 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.553715 | `eventhubs_eventhub_get` | ❌ |

---

## Test 195
=======
| 1 | 0.697894 | `eventhubs_namespace_delete` | ❌ |
| 2 | 0.688471 | `eventhubs_eventhub_delete` | ✅ **EXPECTED** |
| 3 | 0.627661 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.578662 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.552931 | `eventhubs_eventhub_get` | ❌ |

---

## Test 205
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** List all Event Hubs in my namespace <namespace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.773277 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 2 | 0.687596 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.578709 | `eventhubs_eventhub_update` | ❌ |
| 4 | 0.561587 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.545481 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

## Test 201
=======
| 1 | 0.773231 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 2 | 0.687582 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.578689 | `eventhubs_eventhub_update` | ❌ |
| 4 | 0.560155 | `eventhubs_namespace_delete` | ❌ |
| 5 | 0.545475 | `eventhubs_eventhub_consumergroup_get` | ❌ |

---

<<<<<<< HEAD
## Test 196
=======
## Test 206
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_eventhub_get`  
**Prompt:** Get the details of my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.638112 | `eventhubs_namespace_get` | ❌ |
| 2 | 0.627528 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 3 | 0.570964 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.527503 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.521930 | `eventhubs_namespace_delete` | ❌ |

---

## Test 202
=======
<<<<<<< HEAD
| 1 | 0.638030 | `eventhubs_namespace_get` | ❌ |
| 2 | 0.627606 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 3 | 0.570898 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.527564 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.521837 | `eventhubs_namespace_delete` | ❌ |

---

## Test 197
=======
| 1 | 0.638173 | `eventhubs_namespace_get` | ❌ |
| 2 | 0.627712 | `eventhubs_eventhub_get` | ✅ **EXPECTED** |
| 3 | 0.571001 | `eventhubs_eventhub_consumergroup_get` | ❌ |
| 4 | 0.527639 | `eventhubs_eventhub_update` | ❌ |
| 5 | 0.521101 | `eventhubs_namespace_delete` | ❌ |

---

## Test 207
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Create a new event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.645976 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.605856 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.574389 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.571676 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.557550 | `eventhubs_namespace_delete` | ❌ |

---

## Test 203
=======
<<<<<<< HEAD
| 1 | 0.645723 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.605716 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.574303 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.571748 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.557530 | `eventhubs_namespace_delete` | ❌ |

---

## Test 198
=======
| 1 | 0.645976 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.605856 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.574389 | `eventhubs_eventhub_get` | ❌ |
| 4 | 0.571676 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.557073 | `eventhubs_namespace_delete` | ❌ |

---

## Test 208
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_eventhub_update`  
**Prompt:** Update my event hub <event_hub_name> in my namespace <namespace_name> and resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.655283 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.571661 | `eventhubs_eventhub_delete` | ❌ |
| 3 | 0.568605 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 4 | 0.568396 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.565977 | `eventhubs_namespace_delete` | ❌ |

---

## Test 204
=======
<<<<<<< HEAD
| 1 | 0.655261 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.571762 | `eventhubs_eventhub_delete` | ❌ |
| 3 | 0.569417 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 4 | 0.568279 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.565852 | `eventhubs_namespace_delete` | ❌ |

---

## Test 199
=======
| 1 | 0.655104 | `eventhubs_eventhub_update` | ✅ **EXPECTED** |
| 2 | 0.571580 | `eventhubs_eventhub_delete` | ❌ |
| 3 | 0.568796 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 4 | 0.568526 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.564849 | `eventhubs_namespace_delete` | ❌ |

---

## Test 209
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_namespace_delete`  
**Prompt:** Delete my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.623995 | `eventhubs_namespace_delete` | ✅ **EXPECTED** |
| 2 | 0.525810 | `eventhubs_namespace_update` | ❌ |
=======
| 1 | 0.626113 | `eventhubs_namespace_delete` | ✅ **EXPECTED** |
| 2 | 0.525446 | `eventhubs_namespace_update` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.505082 | `eventhubs_eventhub_consumergroup_delete` | ❌ |
| 4 | 0.449841 | `eventhubs_namespace_get` | ❌ |
| 5 | 0.435037 | `workbooks_delete` | ❌ |

---

<<<<<<< HEAD
## Test 205
=======
<<<<<<< HEAD
## Test 200
=======
## Test 210
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** List all Event Hubs namespaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.659838 | `eventhubs_eventhub_get` | ❌ |
| 2 | 0.658827 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 3 | 0.607372 | `kusto_cluster_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.557150 | `eventgrid_topic_list` | ❌ |
| 5 | 0.556016 | `eventgrid_subscription_list` | ❌ |

---

## Test 206
=======
=======
| 3 | 0.607365 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.557200 | `eventgrid_topic_list` | ❌ |
| 5 | 0.556126 | `eventgrid_subscription_list` | ❌ |

---

<<<<<<< HEAD
## Test 201
=======
## Test 211
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_namespace_get`  
**Prompt:** Get the details of my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.509749 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.509432 | `monitor_webtests_get` | ❌ |
| 3 | 0.497399 | `servicebus_queue_details` | ❌ |
| 4 | 0.490015 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.470455 | `functionapp_get` | ❌ |

---

## Test 207
=======
<<<<<<< HEAD
| 1 | 0.510078 | `monitor_webtests_get` | ❌ |
| 2 | 0.509993 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 3 | 0.497527 | `servicebus_queue_details` | ❌ |
| 4 | 0.490095 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.470636 | `functionapp_get` | ❌ |

---

## Test 202
=======
| 1 | 0.509749 | `eventhubs_namespace_get` | ✅ **EXPECTED** |
| 2 | 0.509431 | `monitor_webtests_get` | ❌ |
| 3 | 0.497399 | `servicebus_queue_details` | ❌ |
| 4 | 0.490055 | `eventhubs_namespace_update` | ❌ |
| 5 | 0.470455 | `functionapp_get` | ❌ |

---

## Test 212
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Create an new namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.610313 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.466721 | `eventhubs_namespace_get` | ❌ |
| 3 | 0.461181 | `eventhubs_namespace_delete` | ❌ |
| 4 | 0.449724 | `workbooks_create` | ❌ |
| 5 | 0.438492 | `eventhubs_eventhub_consumergroup_update` | ❌ |

---

<<<<<<< HEAD
## Test 208
=======
<<<<<<< HEAD
## Test 203
=======
## Test 213
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `eventhubs_namespace_update`  
**Prompt:** Update my namespace <namespace_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.622219 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.474098 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.448723 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.436549 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.372490 | `sql_db_rename` | ❌ |

---

## Test 209
=======
| 1 | 0.622338 | `eventhubs_namespace_update` | ✅ **EXPECTED** |
| 2 | 0.476290 | `eventhubs_namespace_delete` | ❌ |
| 3 | 0.448723 | `eventhubs_namespace_get` | ❌ |
| 4 | 0.436549 | `eventhubs_eventhub_consumergroup_update` | ❌ |
| 5 | 0.372632 | `sql_db_rename` | ❌ |

---

<<<<<<< HEAD
## Test 204
=======
## Test 214
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** Describe the function app <function_app_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.660116 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.451226 | `deploy_app_logs_get` | ❌ |
| 3 | 0.450457 | `applens_resource_diagnose` | ❌ |
<<<<<<< HEAD
| 4 | 0.390048 | `mysql_server_list` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.390107 | `mysql_server_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.380314 | `get_bestpractices_get` | ❌ |

---

<<<<<<< HEAD
## Test 210
=======
## Test 205
=======
| 4 | 0.390048 | `mysql_server_list` | ❌ |
| 5 | 0.380262 | `get_bestpractices_get` | ❌ |

---

## Test 215
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** Get configuration for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.607276 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.447400 | `mysql_server_config_get` | ❌ |
| 3 | 0.424765 | `appconfig_account_list` | ❌ |
| 4 | 0.411267 | `appconfig_kv_get` | ❌ |
| 5 | 0.400002 | `deploy_app_logs_get` | ❌ |

---

<<<<<<< HEAD
## Test 211
=======
<<<<<<< HEAD
## Test 206
=======
## Test 216
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** Get function app status for <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622384 | `functionapp_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.413523 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.390708 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383293 | `deploy_app_logs_get` | ❌ |
| 5 | 0.360665 | `storage_account_get` | ❌ |

---

## Test 212
=======
<<<<<<< HEAD
| 2 | 0.413481 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.390766 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383533 | `deploy_app_logs_get` | ❌ |
| 5 | 0.360677 | `storage_account_get` | ❌ |

---

## Test 207
=======
| 2 | 0.411718 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.390708 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.383533 | `deploy_app_logs_get` | ❌ |
| 5 | 0.360764 | `storage_account_get` | ❌ |

---

## Test 217
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** Get information about my function app <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.690933 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.441937 | `foundry_resource_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.432317 | `resourcehealth_availability-status_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.432458 | `resourcehealth_availability-status_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.431821 | `applens_resource_diagnose` | ❌ |
| 5 | 0.429077 | `storage_account_get` | ❌ |

---

<<<<<<< HEAD
## Test 213
=======
## Test 208
=======
| 3 | 0.432317 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.431821 | `applens_resource_diagnose` | ❌ |
| 5 | 0.429120 | `storage_account_get` | ❌ |

---

## Test 218
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** Retrieve host name and status of function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592791 | `functionapp_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.417779 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.409487 | `deploy_app_logs_get` | ❌ |
| 4 | 0.399953 | `storage_account_get` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.417817 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.409712 | `deploy_app_logs_get` | ❌ |
| 4 | 0.399896 | `storage_account_get` | ❌ |
=======
| 2 | 0.417634 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.409712 | `deploy_app_logs_get` | ❌ |
| 4 | 0.400049 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.392237 | `applens_resource_diagnose` | ❌ |

---

<<<<<<< HEAD
## Test 214
=======
<<<<<<< HEAD
## Test 209
=======
## Test 219
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** Show function app details for <function_app_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687356 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.449033 | `deploy_app_logs_get` | ❌ |
| 3 | 0.428689 | `applens_resource_diagnose` | ❌ |
| 4 | 0.424686 | `foundry_resource_get` | ❌ |
<<<<<<< HEAD
| 5 | 0.391781 | `monitor_webtests_get` | ❌ |

---

## Test 215
=======
<<<<<<< HEAD
| 5 | 0.392451 | `monitor_webtests_get` | ❌ |

---

## Test 210
=======
| 5 | 0.391781 | `monitor_webtests_get` | ❌ |

---

## Test 220
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me the details for the function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.644882 | `functionapp_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.429692 | `deploy_app_logs_get` | ❌ |
| 3 | 0.421082 | `storage_account_get` | ❌ |
| 4 | 0.403261 | `signalr_runtime_get` | ❌ |
=======
| 2 | 0.430189 | `deploy_app_logs_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.421127 | `storage_account_get` | ❌ |
=======
| 3 | 0.421155 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.403311 | `signalr_runtime_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.391615 | `foundry_resource_get` | ❌ |

---

<<<<<<< HEAD
## Test 216
=======
<<<<<<< HEAD
## Test 211
=======
## Test 221
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** Show plan and region for function app <function_app_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.554980 | `functionapp_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.426921 | `quota_usage_check` | ❌ |
| 3 | 0.424062 | `deploy_app_logs_get` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.426976 | `quota_usage_check` | ❌ |
=======
| 2 | 0.426703 | `quota_usage_check` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.424610 | `deploy_app_logs_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.408011 | `deploy_plan_get` | ❌ |
| 5 | 0.381629 | `deploy_architecture_diagram_generate` | ❌ |

---

<<<<<<< HEAD
## Test 217
=======
<<<<<<< HEAD
## Test 212
=======
## Test 222
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** What is the status of function app <function_app_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.565797 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.403246 | `deploy_app_logs_get` | ❌ |
| 3 | 0.384159 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.369868 | `applens_resource_diagnose` | ❌ |
<<<<<<< HEAD
| 5 | 0.354912 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 218
=======
<<<<<<< HEAD
| 5 | 0.355044 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 213
=======
| 5 | 0.352966 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 223
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** List all function apps in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646561 | `functionapp_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.557549 | `search_service_list` | ❌ |
| 3 | 0.534936 | `subscription_list` | ❌ |
=======
| 2 | 0.559382 | `search_service_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.534935 | `subscription_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.529031 | `kusto_cluster_list` | ❌ |
| 5 | 0.516618 | `cosmos_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 219
=======
## Test 214
=======
| 3 | 0.534930 | `subscription_list` | ❌ |
| 4 | 0.528892 | `kusto_cluster_list` | ❌ |
| 5 | 0.516664 | `cosmos_account_list` | ❌ |

---

## Test 224
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** Show me my Azure function apps  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.560249 | `functionapp_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.464637 | `deploy_app_logs_get` | ❌ |
| 3 | 0.411323 | `get_bestpractices_get` | ❌ |
| 4 | 0.410461 | `search_service_list` | ❌ |
=======
| 2 | 0.464985 | `deploy_app_logs_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.412646 | `search_service_list` | ❌ |
| 4 | 0.411323 | `get_bestpractices_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.398503 | `extension_cli_install` | ❌ |

---

<<<<<<< HEAD
## Test 220
=======
## Test 215
=======
| 3 | 0.436167 | `foundry_agents_list` | ❌ |
| 4 | 0.413594 | `get_bestpractices_get` | ❌ |
| 5 | 0.412646 | `search_service_list` | ❌ |

---

## Test 225
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `functionapp_get`  
**Prompt:** What function apps do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433675 | `functionapp_get` | ✅ **EXPECTED** |
| 2 | 0.346031 | `deploy_app_logs_get` | ❌ |
| 3 | 0.337966 | `applens_resource_diagnose` | ❌ |
| 4 | 0.316594 | `extension_cli_install` | ❌ |
| 5 | 0.286490 | `get_bestpractices_get` | ❌ |

---

<<<<<<< HEAD
## Test 221
=======
<<<<<<< HEAD
## Test 216
=======
## Test 226
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Get the account settings for my key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.604780 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.532196 | `storage_account_get` | ❌ |
| 3 | 0.496042 | `keyvault_key_get` | ❌ |
| 4 | 0.452367 | `appconfig_kv_set` | ❌ |
| 5 | 0.448265 | `keyvault_secret_get` | ❌ |

---

## Test 222
=======
<<<<<<< HEAD
| 1 | 0.604797 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.532029 | `storage_account_get` | ❌ |
=======
| 1 | 0.604780 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.532169 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.496629 | `keyvault_key_get` | ❌ |
| 4 | 0.452366 | `appconfig_kv_set` | ❌ |
| 5 | 0.448039 | `keyvault_secret_get` | ❌ |

---

<<<<<<< HEAD
## Test 217
=======
## Test 227
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** Show me the account settings for managed HSM keyvault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.671370 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.455561 | `storage_account_get` | ❌ |
| 3 | 0.440966 | `keyvault_key_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.671368 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.455516 | `storage_account_get` | ❌ |
=======
| 1 | 0.671370 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.455526 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.441225 | `keyvault_key_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.404666 | `appconfig_kv_set` | ❌ |
| 5 | 0.395449 | `keyvault_secret_get` | ❌ |

---

<<<<<<< HEAD
## Test 223
=======
<<<<<<< HEAD
## Test 218
=======
## Test 228
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_admin_settings_get`  
**Prompt:** What's the value of the <setting_name> setting in my key vault with name <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.505709 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
| 2 | 0.496565 | `appconfig_kv_set` | ❌ |
| 3 | 0.420067 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.419642 | `keyvault_key_get` | ❌ |
| 5 | 0.410219 | `keyvault_secret_get` | ❌ |

---

## Test 224
=======
<<<<<<< HEAD
| 1 | 0.505731 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
=======
| 1 | 0.505750 | `keyvault_admin_settings_get` | ✅ **EXPECTED** |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.496540 | `appconfig_kv_set` | ❌ |
| 3 | 0.420145 | `appconfig_kv_lock_set` | ❌ |
| 4 | 0.419126 | `keyvault_key_get` | ❌ |
| 5 | 0.410215 | `keyvault_secret_get` | ❌ |

---

<<<<<<< HEAD
## Test 219
=======
## Test 229
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Create a new certificate called <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.627727 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570319 | `keyvault_certificate_import` | ❌ |
| 3 | 0.540199 | `keyvault_key_create` | ❌ |
| 4 | 0.519218 | `keyvault_certificate_get` | ❌ |
| 5 | 0.500027 | `keyvault_certificate_list` | ❌ |

---

## Test 225
=======
<<<<<<< HEAD
| 1 | 0.627882 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570708 | `keyvault_certificate_import` | ❌ |
| 3 | 0.540476 | `keyvault_key_create` | ❌ |
| 4 | 0.519268 | `keyvault_certificate_get` | ❌ |
| 5 | 0.500093 | `keyvault_certificate_list` | ❌ |

---

## Test 220
=======
| 1 | 0.627727 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.570398 | `keyvault_certificate_import` | ❌ |
| 3 | 0.540199 | `keyvault_key_create` | ❌ |
| 4 | 0.519218 | `keyvault_certificate_get` | ❌ |
| 5 | 0.500027 | `keyvault_certificate_list` | ❌ |

---

## Test 230
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Generate a certificate named <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.599548 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561717 | `keyvault_certificate_import` | ❌ |
| 3 | 0.521910 | `keyvault_certificate_get` | ❌ |
| 4 | 0.501291 | `keyvault_key_create` | ❌ |
| 5 | 0.496516 | `keyvault_certificate_list` | ❌ |

---

## Test 226
=======
| 1 | 0.599990 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.561458 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522706 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502128 | `keyvault_key_create` | ❌ |
| 5 | 0.497145 | `keyvault_certificate_list` | ❌ |

---

<<<<<<< HEAD
## Test 221
=======
## Test 231
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Request creation of certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.573998 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527759 | `keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `keyvault_certificate_get` | ❌ |
| 4 | 0.481548 | `keyvault_key_create` | ❌ |
| 5 | 0.469601 | `keyvault_certificate_list` | ❌ |

---

## Test 227
=======
<<<<<<< HEAD
| 1 | 0.574040 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527743 | `keyvault_certificate_import` | ❌ |
| 3 | 0.498226 | `keyvault_certificate_get` | ❌ |
| 4 | 0.481666 | `keyvault_key_create` | ❌ |
| 5 | 0.469651 | `keyvault_certificate_list` | ❌ |

---

## Test 222
=======
| 1 | 0.573998 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.527813 | `keyvault_certificate_import` | ❌ |
| 3 | 0.498278 | `keyvault_certificate_get` | ❌ |
| 4 | 0.481548 | `keyvault_key_create` | ❌ |
| 5 | 0.469601 | `keyvault_certificate_list` | ❌ |

---

## Test 232
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Provision a new key vault certificate <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.591697 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.562234 | `keyvault_certificate_import` | ❌ |
| 3 | 0.522147 | `keyvault_certificate_get` | ❌ |
| 4 | 0.502529 | `keyvault_key_create` | ❌ |
| 5 | 0.479992 | `keyvault_certificate_list` | ❌ |

---

<<<<<<< HEAD
## Test 228
=======
<<<<<<< HEAD
## Test 223
=======
## Test 233
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_create`  
**Prompt:** Issue a certificate <certificate_name> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.622788 | `keyvault_certificate_create` | ✅ **EXPECTED** |
| 2 | 0.558533 | `keyvault_certificate_import` | ❌ |
| 3 | 0.534503 | `keyvault_certificate_get` | ❌ |
| 4 | 0.521316 | `keyvault_certificate_list` | ❌ |
| 5 | 0.465056 | `keyvault_key_create` | ❌ |

---

<<<<<<< HEAD
## Test 229
=======
<<<<<<< HEAD
## Test 224
=======
## Test 234
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.600625 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528405 | `keyvault_certificate_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.519037 | `keyvault_certificate_import` | ❌ |
=======
| 3 | 0.518919 | `keyvault_certificate_import` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.499293 | `keyvault_certificate_create` | ❌ |
| 5 | 0.487691 | `keyvault_key_get` | ❌ |

---

<<<<<<< HEAD
## Test 230
=======
<<<<<<< HEAD
## Test 225
=======
## Test 235
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Show me the details of the certificate <certificate_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646098 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.563263 | `keyvault_key_get` | ❌ |
| 3 | 0.514499 | `keyvault_secret_get` | ❌ |
| 4 | 0.509446 | `keyvault_certificate_list` | ❌ |
<<<<<<< HEAD
| 5 | 0.507738 | `keyvault_certificate_import` | ❌ |

---

## Test 231
=======
| 5 | 0.507630 | `keyvault_certificate_import` | ❌ |

---

<<<<<<< HEAD
## Test 226
=======
## Test 236
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Get the certificate <certificate_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609523 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.515570 | `keyvault_certificate_list` | ❌ |
| 3 | 0.511197 | `keyvault_certificate_create` | ❌ |
<<<<<<< HEAD
| 4 | 0.507768 | `keyvault_certificate_import` | ❌ |
| 5 | 0.475674 | `keyvault_key_get` | ❌ |

---

## Test 232
=======
| 4 | 0.507693 | `keyvault_certificate_import` | ❌ |
| 5 | 0.474394 | `keyvault_key_get` | ❌ |

---

<<<<<<< HEAD
## Test 227
=======
## Test 237
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Display the certificate details for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.647669 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.528243 | `keyvault_key_get` | ❌ |
| 3 | 0.521556 | `keyvault_certificate_list` | ❌ |
| 4 | 0.509796 | `keyvault_certificate_import` | ❌ |
| 5 | 0.502403 | `keyvault_secret_get` | ❌ |

---

<<<<<<< HEAD
## Test 233
=======
## Test 228
=======
| 1 | 0.647626 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527284 | `keyvault_key_get` | ❌ |
| 3 | 0.521689 | `keyvault_certificate_list` | ❌ |
| 4 | 0.509907 | `keyvault_certificate_import` | ❌ |
| 5 | 0.501942 | `keyvault_secret_get` | ❌ |

---

## Test 238
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_get`  
**Prompt:** Retrieve certificate metadata for <certificate_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.595959 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527404 | `keyvault_certificate_list` | ❌ |
| 3 | 0.519059 | `keyvault_certificate_import` | ❌ |
| 4 | 0.501138 | `keyvault_certificate_create` | ❌ |
| 5 | 0.465429 | `keyvault_key_get` | ❌ |

---

## Test 234
=======
<<<<<<< HEAD
| 1 | 0.595902 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527167 | `keyvault_certificate_list` | ❌ |
| 3 | 0.518836 | `keyvault_certificate_import` | ❌ |
| 4 | 0.500932 | `keyvault_certificate_create` | ❌ |
| 5 | 0.465265 | `keyvault_key_get` | ❌ |

---

## Test 229
=======
| 1 | 0.595959 | `keyvault_certificate_get` | ✅ **EXPECTED** |
| 2 | 0.527404 | `keyvault_certificate_list` | ❌ |
| 3 | 0.518970 | `keyvault_certificate_import` | ❌ |
| 4 | 0.501138 | `keyvault_certificate_create` | ❌ |
| 5 | 0.465174 | `keyvault_key_get` | ❌ |

---

## Test 239
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import the certificate in file <file_path> into the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.585481 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420747 | `keyvault_certificate_get` | ❌ |
| 3 | 0.402595 | `keyvault_certificate_create` | ❌ |
| 4 | 0.399342 | `keyvault_certificate_list` | ❌ |
| 5 | 0.352905 | `keyvault_key_create` | ❌ |

---

## Test 235
=======
<<<<<<< HEAD
| 1 | 0.585549 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420798 | `keyvault_certificate_get` | ❌ |
| 3 | 0.402853 | `keyvault_certificate_create` | ❌ |
| 4 | 0.399353 | `keyvault_certificate_list` | ❌ |
| 5 | 0.353196 | `keyvault_key_create` | ❌ |

---

## Test 230
=======
| 1 | 0.585374 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.420747 | `keyvault_certificate_get` | ❌ |
| 3 | 0.402595 | `keyvault_certificate_create` | ❌ |
| 4 | 0.399342 | `keyvault_certificate_list` | ❌ |
| 5 | 0.352905 | `keyvault_key_create` | ❌ |

---

## Test 240
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Import a certificate into the key vault <key_vault_account_name> using the name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.622125 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.504314 | `keyvault_certificate_get` | ❌ |
| 3 | 0.498847 | `keyvault_certificate_create` | ❌ |
| 4 | 0.448105 | `keyvault_certificate_list` | ❌ |
| 5 | 0.419811 | `keyvault_key_create` | ❌ |

---

<<<<<<< HEAD
## Test 236
=======
## Test 231
=======
| 1 | 0.622168 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.504306 | `keyvault_certificate_get` | ❌ |
| 3 | 0.498841 | `keyvault_certificate_create` | ❌ |
| 4 | 0.448114 | `keyvault_certificate_list` | ❌ |
| 5 | 0.419794 | `keyvault_key_create` | ❌ |

---

## Test 241
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Upload certificate file <file_path> to key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.595707 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.453929 | `keyvault_certificate_create` | ❌ |
| 3 | 0.452551 | `keyvault_certificate_get` | ❌ |
| 4 | 0.418203 | `keyvault_certificate_list` | ❌ |
| 5 | 0.413377 | `keyvault_key_create` | ❌ |

---

<<<<<<< HEAD
## Test 237
=======
## Test 232
=======
| 1 | 0.594990 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.453726 | `keyvault_certificate_create` | ❌ |
| 3 | 0.452165 | `keyvault_certificate_get` | ❌ |
| 4 | 0.418142 | `keyvault_certificate_list` | ❌ |
| 5 | 0.413240 | `keyvault_key_create` | ❌ |

---

## Test 242
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Load certificate <certificate_name> from file <file_path> into vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619385 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.517804 | `keyvault_certificate_get` | ❌ |
| 3 | 0.480815 | `keyvault_certificate_create` | ❌ |
| 4 | 0.444386 | `keyvault_certificate_list` | ❌ |
| 5 | 0.381873 | `keyvault_key_create` | ❌ |

---

<<<<<<< HEAD
## Test 238
=======
<<<<<<< HEAD
## Test 233
=======
## Test 243
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_import`  
**Prompt:** Add existing certificate file <file_path> to the key vault <key_vault_account_name> with name <certificate_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.595418 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452490 | `keyvault_certificate_create` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.595417 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452489 | `keyvault_certificate_create` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.441616 | `keyvault_certificate_get` | ❌ |
| 4 | 0.408018 | `keyvault_key_create` | ❌ |
| 5 | 0.392244 | `keyvault_secret_create` | ❌ |

---

<<<<<<< HEAD
## Test 239
=======
## Test 234
=======
| 1 | 0.595426 | `keyvault_certificate_import` | ✅ **EXPECTED** |
| 2 | 0.452531 | `keyvault_certificate_create` | ❌ |
| 3 | 0.441676 | `keyvault_certificate_get` | ❌ |
| 4 | 0.408033 | `keyvault_key_create` | ❌ |
| 5 | 0.392316 | `keyvault_secret_create` | ❌ |

---

## Test 244
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List all certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.726124 | `keyvault_certificate_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.583110 | `keyvault_key_list` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.583138 | `keyvault_key_list` | ❌ |
=======
| 2 | 0.583079 | `keyvault_key_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.531988 | `keyvault_secret_list` | ❌ |
| 4 | 0.515236 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485792 | `keyvault_certificate_create` | ❌ |

---

<<<<<<< HEAD
## Test 240
=======
<<<<<<< HEAD
## Test 235
=======
## Test 245
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show me the certificates in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615541 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.522453 | `keyvault_certificate_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.475156 | `keyvault_key_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.475197 | `keyvault_key_list` | ❌ |
=======
| 3 | 0.475142 | `keyvault_key_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.460973 | `keyvault_certificate_create` | ❌ |
| 5 | 0.449381 | `keyvault_key_get` | ❌ |

---

<<<<<<< HEAD
## Test 241
=======
<<<<<<< HEAD
## Test 236
=======
## Test 246
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** What certificates are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624710 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.519739 | `keyvault_certificate_get` | ❌ |
| 3 | 0.510048 | `keyvault_certificate_create` | ❌ |
<<<<<<< HEAD
| 4 | 0.505534 | `keyvault_certificate_import` | ❌ |
| 5 | 0.497356 | `keyvault_key_list` | ❌ |

---

<<<<<<< HEAD
## Test 242
=======
## Test 237
=======
| 4 | 0.505367 | `keyvault_certificate_import` | ❌ |
| 5 | 0.497322 | `keyvault_key_list` | ❌ |

---

## Test 247
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** List certificate names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.672622 | `keyvault_certificate_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.553990 | `keyvault_key_list` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.554016 | `keyvault_key_list` | ❌ |
=======
| 2 | 0.553960 | `keyvault_key_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.511905 | `keyvault_secret_list` | ❌ |
| 4 | 0.507062 | `keyvault_certificate_get` | ❌ |
| 5 | 0.492357 | `keyvault_certificate_create` | ❌ |

---

<<<<<<< HEAD
## Test 243
=======
<<<<<<< HEAD
## Test 238
=======
## Test 248
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Enumerate certificates in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.747408 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.594216 | `keyvault_key_list` | ❌ |
=======
| 1 | 0.747407 | `keyvault_certificate_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.594268 | `keyvault_key_list` | ❌ |
=======
| 2 | 0.594121 | `keyvault_key_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.558771 | `keyvault_secret_list` | ❌ |
| 4 | 0.515568 | `keyvault_certificate_get` | ❌ |
| 5 | 0.490876 | `keyvault_certificate_create` | ❌ |

---

<<<<<<< HEAD
## Test 244
=======
<<<<<<< HEAD
## Test 239
=======
## Test 249
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_certificate_list`  
**Prompt:** Show certificate names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.639711 | `keyvault_certificate_list` | ✅ **EXPECTED** |
| 2 | 0.512475 | `keyvault_certificate_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.507572 | `keyvault_key_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.507603 | `keyvault_key_list` | ❌ |
=======
| 3 | 0.507562 | `keyvault_key_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.482583 | `keyvault_certificate_create` | ❌ |
| 5 | 0.464725 | `keyvault_secret_list` | ❌ |

---

<<<<<<< HEAD
## Test 245
=======
<<<<<<< HEAD
## Test 240
=======
## Test 250
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create a new key called <key_name> with the RSA type in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.661466 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456580 | `keyvault_secret_create` | ❌ |
| 3 | 0.451790 | `keyvault_certificate_create` | ❌ |
| 4 | 0.429614 | `keyvault_certificate_import` | ❌ |
| 5 | 0.399469 | `keyvault_key_get` | ❌ |

---

<<<<<<< HEAD
## Test 246
=======
## Test 241
=======
| 1 | 0.661548 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.456628 | `keyvault_secret_create` | ❌ |
| 3 | 0.451826 | `keyvault_certificate_create` | ❌ |
| 4 | 0.429537 | `keyvault_certificate_import` | ❌ |
| 5 | 0.399324 | `keyvault_key_get` | ❌ |

---

## Test 251
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Generate a key <key_name> with type <key_type> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.641070 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428964 | `keyvault_key_get` | ❌ |
| 3 | 0.422763 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420045 | `keyvault_secret_create` | ❌ |
| 5 | 0.405644 | `appconfig_kv_set` | ❌ |

---

## Test 247
=======
<<<<<<< HEAD
| 1 | 0.641022 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428461 | `keyvault_key_get` | ❌ |
| 3 | 0.422686 | `keyvault_certificate_create` | ❌ |
| 4 | 0.419964 | `keyvault_secret_create` | ❌ |
| 5 | 0.405612 | `appconfig_kv_set` | ❌ |

---

## Test 242
=======
| 1 | 0.641070 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.428502 | `keyvault_key_get` | ❌ |
| 3 | 0.422763 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420045 | `keyvault_secret_create` | ❌ |
| 5 | 0.405644 | `appconfig_kv_set` | ❌ |

---

## Test 252
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an oct key in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.547493 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.463557 | `keyvault_secret_create` | ❌ |
| 3 | 0.447410 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420793 | `keyvault_key_get` | ❌ |
| 5 | 0.404350 | `keyvault_certificate_import` | ❌ |

---

## Test 248
=======
<<<<<<< HEAD
| 1 | 0.548424 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.464221 | `keyvault_secret_create` | ❌ |
| 3 | 0.448379 | `keyvault_certificate_create` | ❌ |
| 4 | 0.421467 | `keyvault_key_get` | ❌ |
| 5 | 0.405195 | `keyvault_certificate_import` | ❌ |

---

## Test 243
=======
| 1 | 0.547493 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.463557 | `keyvault_secret_create` | ❌ |
| 3 | 0.447410 | `keyvault_certificate_create` | ❌ |
| 4 | 0.420366 | `keyvault_key_get` | ❌ |
| 5 | 0.404180 | `keyvault_certificate_import` | ❌ |

---

## Test 253
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an RSA key in the vault <key_vault_account_name> with name <key_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.641369 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.501636 | `keyvault_secret_create` | ❌ |
| 3 | 0.491735 | `keyvault_certificate_create` | ❌ |
| 4 | 0.464557 | `keyvault_certificate_import` | ❌ |
| 5 | 0.451505 | `keyvault_key_get` | ❌ |

---

<<<<<<< HEAD
## Test 249
=======
## Test 244
=======
| 1 | 0.640853 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.500742 | `keyvault_secret_create` | ❌ |
| 3 | 0.491071 | `keyvault_certificate_create` | ❌ |
| 4 | 0.463536 | `keyvault_certificate_import` | ❌ |
| 5 | 0.450448 | `keyvault_key_get` | ❌ |

---

## Test 254
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_create`  
**Prompt:** Create an EC key with name <key_name> in the vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.571793 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443085 | `keyvault_certificate_create` | ❌ |
| 3 | 0.434697 | `keyvault_secret_create` | ❌ |
| 4 | 0.421997 | `keyvault_key_get` | ❌ |
| 5 | 0.400514 | `keyvault_certificate_import` | ❌ |

---

## Test 250
=======
| 1 | 0.571718 | `keyvault_key_create` | ✅ **EXPECTED** |
| 2 | 0.443369 | `keyvault_certificate_create` | ❌ |
| 3 | 0.434675 | `keyvault_secret_create` | ❌ |
| 4 | 0.421721 | `keyvault_key_get` | ❌ |
<<<<<<< HEAD
| 5 | 0.400533 | `keyvault_certificate_import` | ❌ |

---

## Test 245
=======
| 5 | 0.400433 | `keyvault_certificate_import` | ❌ |

---

## Test 255
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.550225 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.468243 | `keyvault_secret_get` | ❌ |
| 3 | 0.452816 | `keyvault_key_create` | ❌ |
<<<<<<< HEAD
| 4 | 0.439969 | `keyvault_key_list` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.440015 | `keyvault_key_list` | ❌ |
=======
| 4 | 0.439941 | `keyvault_key_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.426545 | `keyvault_certificate_get` | ❌ |

---

<<<<<<< HEAD
## Test 251
=======
<<<<<<< HEAD
## Test 246
=======
## Test 256
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Show me the details of the key <key_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.629372 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.532872 | `keyvault_secret_get` | ❌ |
| 3 | 0.512278 | `storage_account_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.629552 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.532651 | `keyvault_secret_get` | ❌ |
| 3 | 0.512106 | `storage_account_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.495957 | `keyvault_certificate_get` | ❌ |
| 5 | 0.456992 | `keyvault_key_create` | ❌ |

---

<<<<<<< HEAD
## Test 252
=======
## Test 247
=======
| 1 | 0.629579 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.532628 | `keyvault_secret_get` | ❌ |
| 3 | 0.512235 | `storage_account_get` | ❌ |
| 4 | 0.496014 | `keyvault_certificate_get` | ❌ |
| 5 | 0.457056 | `keyvault_key_create` | ❌ |

---

## Test 257
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Get the key <key_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.485492 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.443182 | `keyvault_key_create` | ❌ |
| 3 | 0.409356 | `keyvault_secret_get` | ❌ |
| 4 | 0.395491 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.383519 | `appconfig_kv_lock_set` | ❌ |

---

<<<<<<< HEAD
## Test 253
=======
<<<<<<< HEAD
## Test 248
=======
## Test 258
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Display the key details for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.590297 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.488574 | `keyvault_secret_get` | ❌ |
| 3 | 0.476498 | `storage_account_get` | ❌ |
=======
| 1 | 0.590303 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.488213 | `keyvault_secret_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.476278 | `storage_account_get` | ❌ |
=======
| 3 | 0.476529 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.460796 | `keyvault_certificate_get` | ❌ |
| 5 | 0.436511 | `keyvault_admin_settings_get` | ❌ |

---

<<<<<<< HEAD
## Test 254
=======
<<<<<<< HEAD
## Test 249
=======
## Test 259
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_get`  
**Prompt:** Retrieve key metadata for <key_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.518346 | `keyvault_key_get` | ✅ **EXPECTED** |
| 2 | 0.432950 | `storage_account_get` | ❌ |
| 3 | 0.432742 | `keyvault_admin_settings_get` | ❌ |
=======
| 1 | 0.518886 | `keyvault_key_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.432731 | `keyvault_admin_settings_get` | ❌ |
| 3 | 0.432677 | `storage_account_get` | ❌ |
=======
| 2 | 0.432980 | `storage_account_get` | ❌ |
| 3 | 0.432742 | `keyvault_admin_settings_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.429131 | `keyvault_key_create` | ❌ |
| 5 | 0.422731 | `keyvault_secret_get` | ❌ |

---

<<<<<<< HEAD
## Test 255
=======
<<<<<<< HEAD
## Test 250
=======
## Test 260
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List all keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.701448 | `keyvault_key_list` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 1 | 0.701474 | `keyvault_key_list` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.601513 | `keyvault_certificate_list` | ❌ |
| 3 | 0.587427 | `keyvault_secret_list` | ❌ |
| 4 | 0.498767 | `cosmos_account_list` | ❌ |
| 5 | 0.480129 | `keyvault_admin_settings_get` | ❌ |

---

<<<<<<< HEAD
## Test 256
=======
## Test 251
=======
| 1 | 0.701420 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.601513 | `keyvault_certificate_list` | ❌ |
| 3 | 0.587427 | `keyvault_secret_list` | ❌ |
| 4 | 0.498750 | `cosmos_account_list` | ❌ |
| 5 | 0.480129 | `keyvault_admin_settings_get` | ❌ |

---

## Test 261
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show me the keys in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.549453 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.507865 | `keyvault_key_get` | ❌ |
| 3 | 0.475507 | `keyvault_certificate_list` | ❌ |
| 4 | 0.472465 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.455936 | `keyvault_secret_get` | ❌ |

---

## Test 257
=======
<<<<<<< HEAD
| 1 | 0.549498 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.506815 | `keyvault_key_get` | ❌ |
| 3 | 0.475507 | `keyvault_certificate_list` | ❌ |
| 4 | 0.472457 | `keyvault_admin_settings_get` | ❌ |
=======
| 1 | 0.549442 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.506815 | `keyvault_key_get` | ❌ |
| 3 | 0.475507 | `keyvault_certificate_list` | ❌ |
| 4 | 0.472465 | `keyvault_admin_settings_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.455683 | `keyvault_secret_get` | ❌ |

---

<<<<<<< HEAD
## Test 252
=======
## Test 262
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_list`  
**Prompt:** What keys are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.581970 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.502245 | `keyvault_admin_settings_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.582010 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.502252 | `keyvault_admin_settings_get` | ❌ |
=======
| 1 | 0.581948 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.502245 | `keyvault_admin_settings_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.501481 | `keyvault_certificate_list` | ❌ |
| 4 | 0.477451 | `keyvault_key_get` | ❌ |
| 5 | 0.472414 | `keyvault_secret_list` | ❌ |

---

<<<<<<< HEAD
## Test 258
=======
<<<<<<< HEAD
## Test 253
=======
## Test 263
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_list`  
**Prompt:** List key names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.641314 | `keyvault_key_list` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 1 | 0.641339 | `keyvault_key_list` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.559550 | `keyvault_certificate_list` | ❌ |
| 3 | 0.553553 | `keyvault_secret_list` | ❌ |
| 4 | 0.486377 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475992 | `cosmos_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 259
=======
## Test 254
=======
| 1 | 0.641210 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.559476 | `keyvault_certificate_list` | ❌ |
| 3 | 0.553501 | `keyvault_secret_list` | ❌ |
| 4 | 0.486377 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.475945 | `cosmos_account_list` | ❌ |

---

## Test 264
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Enumerate keys in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.723266 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611366 | `keyvault_certificate_list` | ❌ |
| 3 | 0.611185 | `keyvault_secret_list` | ❌ |
| 4 | 0.473886 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.443322 | `keyvault_key_get` | ❌ |

---

## Test 260
=======
<<<<<<< HEAD
| 1 | 0.723318 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611366 | `keyvault_certificate_list` | ❌ |
| 3 | 0.611185 | `keyvault_secret_list` | ❌ |
| 4 | 0.473874 | `keyvault_admin_settings_get` | ❌ |
=======
| 1 | 0.723171 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.611366 | `keyvault_certificate_list` | ❌ |
| 3 | 0.611185 | `keyvault_secret_list` | ❌ |
| 4 | 0.473886 | `keyvault_admin_settings_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.441881 | `keyvault_key_get` | ❌ |

---

<<<<<<< HEAD
## Test 255
=======
## Test 265
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_key_list`  
**Prompt:** Show key names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.570444 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501953 | `keyvault_key_get` | ❌ |
| 3 | 0.500103 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496817 | `storage_account_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.570489 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `keyvault_key_get` | ❌ |
| 3 | 0.500103 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496907 | `storage_account_get` | ❌ |
=======
| 1 | 0.570418 | `keyvault_key_list` | ✅ **EXPECTED** |
| 2 | 0.501073 | `keyvault_key_get` | ❌ |
| 3 | 0.500103 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496837 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.490367 | `keyvault_secret_list` | ❌ |

---

<<<<<<< HEAD
## Test 261
=======
<<<<<<< HEAD
## Test 256
=======
## Test 266
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Create a new secret called <secret_name> with value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678482 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.553018 | `keyvault_key_create` | ❌ |
| 3 | 0.512602 | `keyvault_secret_get` | ❌ |
| 4 | 0.475097 | `keyvault_certificate_create` | ❌ |
| 5 | 0.461437 | `appconfig_kv_set` | ❌ |

---

<<<<<<< HEAD
## Test 262
=======
<<<<<<< HEAD
## Test 257
=======
## Test 267
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Set a secret named <secret_name> with value <secret_value> in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.663094 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.519306 | `keyvault_secret_get` | ❌ |
| 3 | 0.512233 | `appconfig_kv_set` | ❌ |
| 4 | 0.458502 | `keyvault_key_create` | ❌ |
| 5 | 0.429785 | `appconfig_kv_lock_set` | ❌ |

---

<<<<<<< HEAD
## Test 263
=======
<<<<<<< HEAD
## Test 258
=======
## Test 268
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Store secret <secret_name> value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.639897 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.509526 | `keyvault_secret_get` | ❌ |
| 3 | 0.485203 | `appconfig_kv_set` | ❌ |
| 4 | 0.484680 | `keyvault_key_create` | ❌ |
| 5 | 0.448995 | `appconfig_kv_lock_set` | ❌ |

---

<<<<<<< HEAD
## Test 264
=======
## Test 259
=======
| 1 | 0.639804 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.509509 | `keyvault_secret_get` | ❌ |
| 3 | 0.485174 | `appconfig_kv_set` | ❌ |
| 4 | 0.484391 | `keyvault_key_create` | ❌ |
| 5 | 0.449001 | `appconfig_kv_lock_set` | ❌ |

---

## Test 269
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Add a new version of secret <secret_name> with value <secret_value> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.675145 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.499276 | `keyvault_secret_get` | ❌ |
| 3 | 0.498228 | `keyvault_key_create` | ❌ |
| 4 | 0.479174 | `keyvault_certificate_import` | ❌ |
| 5 | 0.458574 | `appconfig_kv_set` | ❌ |

---

## Test 265
=======
<<<<<<< HEAD
| 1 | 0.675147 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.499602 | `keyvault_secret_get` | ❌ |
| 3 | 0.498196 | `keyvault_key_create` | ❌ |
| 4 | 0.479173 | `keyvault_certificate_import` | ❌ |
| 5 | 0.458587 | `appconfig_kv_set` | ❌ |

---

## Test 260
=======
| 1 | 0.675145 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.499612 | `keyvault_secret_get` | ❌ |
| 3 | 0.498228 | `keyvault_key_create` | ❌ |
| 4 | 0.478700 | `keyvault_certificate_import` | ❌ |
| 5 | 0.458574 | `appconfig_kv_set` | ❌ |

---

## Test 270
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_create`  
**Prompt:** Update secret <secret_name> to value <secret_value> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.571597 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.513012 | `keyvault_secret_get` | ❌ |
| 3 | 0.441198 | `appconfig_kv_set` | ❌ |
| 4 | 0.417911 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.408739 | `keyvault_key_get` | ❌ |

---

## Test 266
=======
<<<<<<< HEAD
| 1 | 0.571716 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.513963 | `keyvault_secret_get` | ❌ |
| 3 | 0.441281 | `appconfig_kv_set` | ❌ |
| 4 | 0.417998 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.408505 | `keyvault_key_get` | ❌ |

---

## Test 261
=======
| 1 | 0.571612 | `keyvault_secret_create` | ✅ **EXPECTED** |
| 2 | 0.513767 | `keyvault_secret_get` | ❌ |
| 3 | 0.441223 | `appconfig_kv_set` | ❌ |
| 4 | 0.417943 | `appconfig_kv_lock_set` | ❌ |
| 5 | 0.408242 | `keyvault_key_get` | ❌ |

---

## Test 271
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.602686 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.505620 | `keyvault_key_get` | ❌ |
| 3 | 0.501397 | `keyvault_secret_create` | ❌ |
| 4 | 0.478769 | `keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `keyvault_certificate_get` | ❌ |

---

## Test 267
=======
<<<<<<< HEAD
| 1 | 0.605040 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504063 | `keyvault_key_get` | ❌ |
| 3 | 0.502826 | `keyvault_secret_create` | ❌ |
| 4 | 0.479767 | `keyvault_secret_list` | ❌ |
| 5 | 0.440063 | `keyvault_certificate_get` | ❌ |

---

## Test 262
=======
| 1 | 0.602769 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.504212 | `keyvault_key_get` | ❌ |
| 3 | 0.501397 | `keyvault_secret_create` | ❌ |
| 4 | 0.478769 | `keyvault_secret_list` | ❌ |
| 5 | 0.439521 | `keyvault_certificate_get` | ❌ |

---

## Test 272
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Show me the details of the secret <secret_name> in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.653920 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.567036 | `keyvault_key_get` | ❌ |
| 3 | 0.517547 | `storage_account_get` | ❌ |
=======
| 1 | 0.653871 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.566786 | `keyvault_key_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.517355 | `storage_account_get` | ❌ |
=======
| 3 | 0.517561 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.496050 | `keyvault_certificate_get` | ❌ |
| 5 | 0.485249 | `keyvault_secret_list` | ❌ |

---

<<<<<<< HEAD
## Test 268
=======
<<<<<<< HEAD
## Test 263
=======
## Test 273
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Get the secret <secret_name> from vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.578261 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.493543 | `keyvault_key_get` | ❌ |
| 3 | 0.488705 | `keyvault_secret_create` | ❌ |
| 4 | 0.443676 | `keyvault_secret_list` | ❌ |
| 5 | 0.424167 | `keyvault_admin_settings_get` | ❌ |

---

<<<<<<< HEAD
## Test 269
=======
<<<<<<< HEAD
## Test 264
=======
## Test 274
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Display the secret details for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.649423 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.548102 | `keyvault_key_get` | ❌ |
| 3 | 0.497402 | `storage_account_get` | ❌ |
=======
| 1 | 0.649267 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.546992 | `keyvault_key_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.497258 | `storage_account_get` | ❌ |
=======
| 3 | 0.497410 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.492583 | `keyvault_certificate_get` | ❌ |
| 5 | 0.491597 | `keyvault_secret_list` | ❌ |

---

<<<<<<< HEAD
## Test 270
=======
<<<<<<< HEAD
## Test 265
=======
## Test 275
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_get`  
**Prompt:** Retrieve secret metadata for <secret_name> in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.577338 | `keyvault_secret_get` | ✅ **EXPECTED** |
| 2 | 0.475492 | `keyvault_key_get` | ❌ |
| 3 | 0.466890 | `keyvault_secret_create` | ❌ |
| 4 | 0.447602 | `keyvault_secret_list` | ❌ |
<<<<<<< HEAD
| 5 | 0.439583 | `storage_account_get` | ❌ |

---

## Test 271
=======
<<<<<<< HEAD
| 5 | 0.439381 | `storage_account_get` | ❌ |

---

## Test 266
=======
| 5 | 0.439597 | `storage_account_get` | ❌ |

---

## Test 276
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List all secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.701227 | `keyvault_secret_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.563736 | `keyvault_key_list` | ❌ |
| 3 | 0.538337 | `keyvault_certificate_list` | ❌ |
| 4 | 0.499888 | `keyvault_secret_get` | ❌ |
| 5 | 0.455500 | `cosmos_account_list` | ❌ |

---

## Test 272
=======
<<<<<<< HEAD
| 2 | 0.563760 | `keyvault_key_list` | ❌ |
=======
| 2 | 0.563694 | `keyvault_key_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.538337 | `keyvault_certificate_list` | ❌ |
| 4 | 0.499642 | `keyvault_secret_get` | ❌ |
| 5 | 0.455469 | `cosmos_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 267
=======
## Test 277
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show me the secrets in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555681 | `keyvault_secret_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.544015 | `keyvault_secret_get` | ❌ |
| 3 | 0.498713 | `keyvault_key_get` | ❌ |
| 4 | 0.464661 | `keyvault_key_list` | ❌ |
| 5 | 0.453130 | `keyvault_admin_settings_get` | ❌ |

---

## Test 273
=======
| 2 | 0.543861 | `keyvault_secret_get` | ❌ |
| 3 | 0.497525 | `keyvault_key_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.464705 | `keyvault_key_list` | ❌ |
| 5 | 0.453107 | `keyvault_admin_settings_get` | ❌ |

---

## Test 268
=======
| 4 | 0.464652 | `keyvault_key_list` | ❌ |
| 5 | 0.453130 | `keyvault_admin_settings_get` | ❌ |

---

## Test 278
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** What secrets are in the key vault <key_vault_account_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572540 | `keyvault_secret_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.529389 | `keyvault_secret_get` | ❌ |
| 3 | 0.493761 | `keyvault_key_list` | ❌ |
| 4 | 0.487620 | `keyvault_admin_settings_get` | ❌ |
| 5 | 0.476109 | `keyvault_key_get` | ❌ |

---

## Test 274
=======
| 2 | 0.529258 | `keyvault_secret_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.493797 | `keyvault_key_list` | ❌ |
| 4 | 0.487611 | `keyvault_admin_settings_get` | ❌ |
=======
| 3 | 0.493728 | `keyvault_key_list` | ❌ |
| 4 | 0.487620 | `keyvault_admin_settings_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.475273 | `keyvault_key_get` | ❌ |

---

<<<<<<< HEAD
## Test 269
=======
## Test 279
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** List secrets names in vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624290 | `keyvault_secret_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.559681 | `keyvault_key_list` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.559700 | `keyvault_key_list` | ❌ |
=======
| 2 | 0.559622 | `keyvault_key_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.517516 | `keyvault_certificate_list` | ❌ |
| 4 | 0.479771 | `keyvault_secret_get` | ❌ |
| 5 | 0.453295 | `storage_blob_container_get` | ❌ |

---

<<<<<<< HEAD
## Test 275
=======
<<<<<<< HEAD
## Test 270
=======
## Test 280
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Enumerate secrets in key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.742358 | `keyvault_secret_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.601183 | `keyvault_key_list` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.601234 | `keyvault_key_list` | ❌ |
=======
| 2 | 0.601079 | `keyvault_key_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.567827 | `keyvault_certificate_list` | ❌ |
| 4 | 0.496363 | `keyvault_secret_get` | ❌ |
| 5 | 0.437560 | `keyvault_admin_settings_get` | ❌ |

---

<<<<<<< HEAD
## Test 276
=======
<<<<<<< HEAD
## Test 271
=======
## Test 281
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `keyvault_secret_list`  
**Prompt:** Show secrets names in the key vault <key_vault_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.567110 | `keyvault_secret_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.522600 | `keyvault_secret_get` | ❌ |
| 3 | 0.476309 | `keyvault_key_list` | ❌ |
| 4 | 0.462711 | `keyvault_key_get` | ❌ |
| 5 | 0.462677 | `keyvault_secret_create` | ❌ |

---

## Test 277
=======
| 2 | 0.522398 | `keyvault_secret_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.476354 | `keyvault_key_list` | ❌ |
=======
| 3 | 0.476288 | `keyvault_key_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.462676 | `keyvault_secret_create` | ❌ |
| 5 | 0.461326 | `keyvault_key_get` | ❌ |

---

<<<<<<< HEAD
## Test 272
=======
## Test 282
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Get the configuration of AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588300 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.544302 | `aks_nodepool_get` | ❌ |
| 3 | 0.517279 | `kusto_cluster_get` | ❌ |
| 4 | 0.481416 | `mysql_server_config_get` | ❌ |
| 5 | 0.430976 | `postgres_server_config_get` | ❌ |

---

<<<<<<< HEAD
## Test 278
=======
<<<<<<< HEAD
## Test 273
=======
## Test 283
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the details of AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.621759 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.575626 | `aks_nodepool_get` | ❌ |
| 3 | 0.567870 | `kusto_cluster_get` | ❌ |
| 4 | 0.461466 | `sql_db_show` | ❌ |
| 5 | 0.444327 | `monitor_webtests_get` | ❌ |

---

## Test 279
=======
<<<<<<< HEAD
| 1 | 0.621536 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.575434 | `aks_nodepool_get` | ❌ |
| 3 | 0.567416 | `kusto_cluster_get` | ❌ |
| 4 | 0.461358 | `sql_db_show` | ❌ |
| 5 | 0.445310 | `monitor_webtests_get` | ❌ |

---

## Test 274
=======
| 1 | 0.621759 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.575625 | `aks_nodepool_get` | ❌ |
| 3 | 0.567870 | `kusto_cluster_get` | ❌ |
| 4 | 0.461466 | `sql_db_show` | ❌ |
| 5 | 0.444327 | `monitor_webtests_get` | ❌ |

---

## Test 284
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me the network configuration for AKS cluster <cluster-name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.522525 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.483220 | `aks_nodepool_get` | ❌ |
| 3 | 0.434684 | `kusto_cluster_get` | ❌ |
| 4 | 0.380301 | `mysql_server_config_get` | ❌ |
<<<<<<< HEAD
| 5 | 0.366689 | `kusto_cluster_list` | ❌ |

---

<<<<<<< HEAD
## Test 280
=======
## Test 275
=======
| 5 | 0.366594 | `kusto_cluster_list` | ❌ |

---

## Test 285
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What are the details of my AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.588634 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.550555 | `aks_nodepool_get` | ❌ |
| 3 | 0.527511 | `kusto_cluster_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.445722 | `storage_account_get` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.445813 | `storage_account_get` | ❌ |
=======
| 4 | 0.445833 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.435597 | `foundry_resource_get` | ❌ |

---

<<<<<<< HEAD
## Test 281
=======
<<<<<<< HEAD
## Test 276
=======
## Test 286
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_cluster_get`  
**Prompt:** List all AKS clusters in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.756471 | `aks_cluster_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.749416 | `kusto_cluster_list` | ❌ |
| 3 | 0.590166 | `aks_nodepool_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.568635 | `kusto_database_list` | ❌ |
| 5 | 0.560522 | `search_service_list` | ❌ |

---

## Test 282
=======
| 4 | 0.568440 | `kusto_database_list` | ❌ |
=======
| 2 | 0.749293 | `kusto_cluster_list` | ❌ |
| 3 | 0.590166 | `aks_nodepool_get` | ❌ |
| 4 | 0.568301 | `kusto_database_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.562043 | `search_service_list` | ❌ |

---

<<<<<<< HEAD
## Test 277
=======
## Test 287
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_cluster_get`  
**Prompt:** Show me my Azure Kubernetes Service clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.612123 | `aks_cluster_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.586661 | `kusto_cluster_list` | ❌ |
| 3 | 0.507757 | `aks_nodepool_get` | ❌ |
| 4 | 0.489724 | `kusto_cluster_get` | ❌ |
| 5 | 0.462950 | `kusto_database_list` | ❌ |

---

<<<<<<< HEAD
## Test 283
=======
## Test 278
=======
| 2 | 0.586466 | `kusto_cluster_list` | ❌ |
| 3 | 0.507757 | `aks_nodepool_get` | ❌ |
| 4 | 0.489724 | `kusto_cluster_get` | ❌ |
| 5 | 0.462718 | `kusto_database_list` | ❌ |

---

## Test 288
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_cluster_get`  
**Prompt:** What AKS clusters do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.628470 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563211 | `aks_nodepool_get` | ❌ |
| 3 | 0.526840 | `kusto_cluster_list` | ❌ |
| 4 | 0.426233 | `kusto_cluster_get` | ❌ |
| 5 | 0.409379 | `kusto_database_list` | ❌ |

---

## Test 284
=======
| 1 | 0.628429 | `aks_cluster_get` | ✅ **EXPECTED** |
| 2 | 0.563189 | `aks_nodepool_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.526756 | `kusto_cluster_list` | ❌ |
| 4 | 0.426157 | `kusto_cluster_get` | ❌ |
| 5 | 0.409163 | `kusto_database_list` | ❌ |

---

## Test 279
=======
| 3 | 0.526670 | `kusto_cluster_list` | ❌ |
| 4 | 0.426157 | `kusto_cluster_get` | ❌ |
| 5 | 0.409404 | `kusto_database_list` | ❌ |

---

## Test 289
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Get details for nodepool <nodepool-name> in AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.728569 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.516573 | `kusto_cluster_get` | ❌ |
| 3 | 0.509314 | `aks_cluster_get` | ❌ |
| 4 | 0.468516 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463185 | `sql_elastic-pool_list` | ❌ |

---

## Test 285
=======
<<<<<<< HEAD
| 1 | 0.729136 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.517116 | `kusto_cluster_get` | ❌ |
| 3 | 0.510014 | `aks_cluster_get` | ❌ |
| 4 | 0.468597 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463489 | `sql_elastic-pool_list` | ❌ |

---

## Test 280
=======
| 1 | 0.728937 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.517021 | `kusto_cluster_get` | ❌ |
| 3 | 0.509820 | `aks_cluster_get` | ❌ |
| 4 | 0.468392 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.463192 | `sql_elastic-pool_list` | ❌ |

---

## Test 290
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the configuration for nodepool <nodepool-name> in AKS cluster <cluster-name> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.654106 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.458596 | `sql_elastic-pool_list` | ❌ |
| 3 | 0.446035 | `aks_cluster_get` | ❌ |
| 4 | 0.440273 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.413758 | `kusto_cluster_get` | ❌ |

---

<<<<<<< HEAD
## Test 286
=======
## Test 281
=======
| 1 | 0.654031 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.458651 | `sql_elastic-pool_list` | ❌ |
| 3 | 0.445952 | `aks_cluster_get` | ❌ |
| 4 | 0.440187 | `virtualdesktop_hostpool_list` | ❌ |
| 5 | 0.413711 | `kusto_cluster_get` | ❌ |

---

## Test 291
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What is the setup of nodepool <nodepool-name> for AKS cluster <cluster-name> in <resource-group>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592806 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.402556 | `aks_cluster_get` | ❌ |
| 3 | 0.385218 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.383045 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.355090 | `kusto_cluster_get` | ❌ |

---

<<<<<<< HEAD
## Test 287
=======
<<<<<<< HEAD
## Test 282
=======
## Test 292
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** List nodepools for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.692231 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.519037 | `aks_cluster_get` | ❌ |
| 3 | 0.506720 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500749 | `kusto_cluster_list` | ❌ |
| 5 | 0.487707 | `sql_elastic-pool_list` | ❌ |

---

## Test 288
=======
<<<<<<< HEAD
| 1 | 0.692264 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.519034 | `aks_cluster_get` | ❌ |
| 3 | 0.506649 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500705 | `kusto_cluster_list` | ❌ |
| 5 | 0.487723 | `sql_elastic-pool_list` | ❌ |

---

## Test 283
=======
| 1 | 0.692231 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.519037 | `aks_cluster_get` | ❌ |
| 3 | 0.506624 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.500514 | `kusto_cluster_list` | ❌ |
| 5 | 0.487707 | `sql_elastic-pool_list` | ❌ |

---

## Test 293
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** Show me the nodepool list for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732132 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.561829 | `aks_cluster_get` | ❌ |
| 3 | 0.510269 | `sql_elastic-pool_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.509840 | `virtualdesktop_hostpool_list` | ❌ |
=======
| 4 | 0.509732 | `virtualdesktop_hostpool_list` | ❌ |
<<<<<<< HEAD
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.486700 | `kusto_cluster_list` | ❌ |

---

<<<<<<< HEAD
## Test 289
=======
## Test 284
=======
| 5 | 0.486544 | `kusto_cluster_list` | ❌ |

---

## Test 294
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `aks_nodepool_get`  
**Prompt:** What nodepools do I have for AKS cluster <cluster-name> in <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629358 | `aks_nodepool_get` | ✅ **EXPECTED** |
| 2 | 0.456911 | `aks_cluster_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.443940 | `virtualdesktop_hostpool_list` | ❌ |
=======
| 3 | 0.443902 | `virtualdesktop_hostpool_list` | ❌ |
<<<<<<< HEAD
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.433006 | `kusto_cluster_list` | ❌ |
=======
| 4 | 0.432757 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.425448 | `sql_elastic-pool_list` | ❌ |

---

<<<<<<< HEAD
## Test 290
=======
<<<<<<< HEAD
## Test 285
=======
## Test 295
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `loadtesting_test_create`  
**Prompt:** Create a basic URL test using the following endpoint URL <test-url> that runs for 30 minutes with 45 virtual users. The test name is <sample-name> with the test id <test-id> and the load testing resource is <load-test-resource> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.579172 | `loadtesting_test_create` | ✅ **EXPECTED** |
| 2 | 0.520449 | `loadtesting_testresource_create` | ❌ |
| 3 | 0.513419 | `loadtesting_testrun_create` | ❌ |
| 4 | 0.473951 | `monitor_webtests_create` | ❌ |
| 5 | 0.461959 | `loadtesting_testresource_list` | ❌ |

---

<<<<<<< HEAD
## Test 291
=======
<<<<<<< HEAD
## Test 286
=======
## Test 296
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `loadtesting_test_get`  
**Prompt:** Get the load test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.626226 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.619944 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 3 | 0.594666 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.590698 | `monitor_webtests_get` | ❌ |
| 5 | 0.536024 | `monitor_webtests_list` | ❌ |

---

## Test 292
=======
<<<<<<< HEAD
| 1 | 0.626213 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.620147 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 3 | 0.594630 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.591112 | `monitor_webtests_get` | ❌ |
| 5 | 0.535891 | `monitor_webtests_list` | ❌ |

---

## Test 287
=======
| 1 | 0.626271 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.620094 | `loadtesting_test_get` | ✅ **EXPECTED** |
| 3 | 0.594881 | `loadtesting_testresource_create` | ❌ |
| 4 | 0.590679 | `monitor_webtests_get` | ❌ |
| 5 | 0.537187 | `monitor_webtests_list` | ❌ |

---

## Test 297
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `loadtesting_testresource_create`  
**Prompt:** Create a load test resource <load-test-resource-name> in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.645537 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.618773 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.541696 | `loadtesting_test_create` | ❌ |
| 4 | 0.539771 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.526684 | `monitor_webtests_list` | ❌ |

---

## Test 293
=======
<<<<<<< HEAD
| 1 | 0.645750 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.618984 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.541950 | `loadtesting_test_create` | ❌ |
| 4 | 0.539866 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.526644 | `monitor_webtests_list` | ❌ |

---

## Test 288
=======
| 1 | 0.645537 | `loadtesting_testresource_create` | ✅ **EXPECTED** |
| 2 | 0.618773 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.541746 | `loadtesting_test_create` | ❌ |
| 4 | 0.539771 | `loadtesting_testrun_create` | ❌ |
| 5 | 0.525628 | `monitor_webtests_list` | ❌ |

---

## Test 298
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `loadtesting_testresource_list`  
**Prompt:** List all load testing resources in the resource group <resource-group> in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.794326 | `loadtesting_testresource_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.653165 | `monitor_webtests_list` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.653137 | `monitor_webtests_list` | ❌ |
=======
| 2 | 0.651533 | `monitor_webtests_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.577408 | `group_list` | ❌ |
| 4 | 0.575172 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.565565 | `datadog_monitoredresources_list` | ❌ |

---

<<<<<<< HEAD
## Test 294
=======
<<<<<<< HEAD
## Test 289
=======
## Test 299
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `loadtesting_testrun_create`  
**Prompt:** Create a test run using the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>. Use the name of test run <display-name> and description as <description>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.688976 | `loadtesting_testrun_create` | ✅ **EXPECTED** |
| 2 | 0.594879 | `loadtesting_testrun_update` | ❌ |
| 3 | 0.558566 | `loadtesting_test_create` | ❌ |
| 4 | 0.547102 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.496224 | `loadtesting_testresource_list` | ❌ |

---

<<<<<<< HEAD
## Test 295
=======
<<<<<<< HEAD
## Test 290
=======
## Test 300
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `loadtesting_testrun_get`  
**Prompt:** Get the load test run with id <testrun-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.619146 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.601927 | `loadtesting_test_get` | ❌ |
| 3 | 0.597430 | `loadtesting_testresource_create` | ❌ |
<<<<<<< HEAD
| 4 | 0.577532 | `monitor_webtests_get` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.577924 | `monitor_webtests_get` | ❌ |
=======
| 4 | 0.577532 | `monitor_webtests_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.565996 | `loadtesting_testrun_list` | ❌ |

---

<<<<<<< HEAD
## Test 296
=======
<<<<<<< HEAD
## Test 291
=======
## Test 301
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `loadtesting_testrun_list`  
**Prompt:** Get all the load test runs for the test with id <test-id> in the load test resource <test-resource> in resource group <resource-group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.669307 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.640644 | `loadtesting_testrun_list` | ✅ **EXPECTED** |
| 3 | 0.600977 | `loadtesting_test_get` | ❌ |
| 4 | 0.577403 | `loadtesting_testresource_create` | ❌ |
| 5 | 0.569287 | `monitor_webtests_list` | ❌ |

---

## Test 297
=======
| 1 | 0.669180 | `loadtesting_testresource_list` | ❌ |
| 2 | 0.640360 | `loadtesting_testrun_list` | ✅ **EXPECTED** |
| 3 | 0.601075 | `loadtesting_test_get` | ❌ |
| 4 | 0.577460 | `loadtesting_testresource_create` | ❌ |
<<<<<<< HEAD
| 5 | 0.569963 | `monitor_webtests_get` | ❌ |

---

## Test 292
=======
| 5 | 0.569424 | `monitor_webtests_get` | ❌ |

---

## Test 302
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `loadtesting_testrun_update`  
**Prompt:** Update a test run display name as <display-name> for the id <testrun-id> for test <test-id> in the load testing resource <load-testing-resource> in resource group <resource-group>.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.706747 | `loadtesting_testrun_update` | ✅ **EXPECTED** |
| 2 | 0.514428 | `loadtesting_testrun_create` | ❌ |
<<<<<<< HEAD
| 3 | 0.486977 | `monitor_webtests_update` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.486980 | `monitor_webtests_update` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.470337 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.468374 | `monitor_webtests_get` | ❌ |

---

<<<<<<< HEAD
## Test 298
=======
## Test 293
=======
| 3 | 0.487022 | `monitor_webtests_update` | ❌ |
| 4 | 0.470337 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.468374 | `monitor_webtests_get` | ❌ |

---

## Test 303
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `grafana_list`  
**Prompt:** List all Azure Managed Grafana in one subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.599427 | `kusto_cluster_list` | ❌ |
=======
| 1 | 0.599428 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.578892 | `grafana_list` | ✅ **EXPECTED** |
| 3 | 0.550372 | `subscription_list` | ❌ |
| 4 | 0.549957 | `search_service_list` | ❌ |
| 5 | 0.531259 | `redis_list` | ❌ |

---

<<<<<<< HEAD
## Test 299
=======
<<<<<<< HEAD
## Test 294
=======
## Test 304
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `managedlustre_fs_create`  
**Prompt:** Create an Azure Managed Lustre filesystem with name <filesystem_name>, size <filesystem_size>, SKU <sku>, and subnet <subnet_id> for availability zone <zone> in location <location>. Maintenance should occur on <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.726553 | `managedlustre_fs_create` | ✅ **EXPECTED** |
| 2 | 0.616164 | `managedlustre_fs_list` | ❌ |
| 3 | 0.605701 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.598215 | `managedlustre_fs_update` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.728113 | `managedlustre_fs_create` | ✅ **EXPECTED** |
| 2 | 0.615874 | `managedlustre_fs_list` | ❌ |
| 3 | 0.605775 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.598255 | `managedlustre_fs_update` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.557720 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

<<<<<<< HEAD
## Test 300
=======
## Test 295
=======
| 1 | 0.728113 | `managedlustre_filesystem_create` | ❌ |
| 2 | 0.616164 | `managedlustre_filesystem_list` | ❌ |
| 3 | 0.605775 | `managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.598255 | `managedlustre_filesystem_update` | ❌ |
| 5 | 0.557720 | `managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 305
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.750675 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.631730 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.579855 | `managedlustre_fs_create` | ❌ |
| 4 | 0.562377 | `kusto_cluster_list` | ❌ |
| 5 | 0.512086 | `search_service_list` | ❌ |

---

## Test 301
=======
<<<<<<< HEAD
| 1 | 0.750302 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.631770 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.582660 | `managedlustre_fs_create` | ❌ |
| 4 | 0.562377 | `kusto_cluster_list` | ❌ |
=======
| 1 | 0.750675 | `managedlustre_filesystem_list` | ❌ |
| 2 | 0.631770 | `managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.582660 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.562520 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.513156 | `search_service_list` | ❌ |

---

<<<<<<< HEAD
## Test 296
=======
## Test 306
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `managedlustre_fs_list`  
**Prompt:** List the Azure Managed Lustre filesystems in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.743903 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.613164 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.563081 | `managedlustre_fs_create` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.743639 | `managedlustre_fs_list` | ✅ **EXPECTED** |
| 2 | 0.613217 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.565856 | `managedlustre_fs_create` | ❌ |
=======
| 1 | 0.743903 | `managedlustre_filesystem_list` | ❌ |
| 2 | 0.613217 | `managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.565856 | `managedlustre_filesystem_create` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.519986 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.515433 | `loadtesting_testresource_list` | ❌ |

---

<<<<<<< HEAD
## Test 302
=======
<<<<<<< HEAD
## Test 297
=======
## Test 307
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `managedlustre_fs_sku_get`  
**Prompt:** List the Azure Managed Lustre SKUs available in location <location>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.827360 | `managedlustre_fs_sku_get` | ✅ **EXPECTED** |
| 2 | 0.613674 | `managedlustre_fs_list` | ❌ |
| 3 | 0.511625 | `managedlustre_fs_create` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.827381 | `managedlustre_fs_sku_get` | ✅ **EXPECTED** |
| 2 | 0.613245 | `managedlustre_fs_list` | ❌ |
| 3 | 0.513242 | `managedlustre_fs_create` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.496242 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 5 | 0.470241 | `kusto_cluster_list` | ❌ |

---

<<<<<<< HEAD
## Test 303
=======
## Test 298
=======
| 1 | 0.827381 | `managedlustre_filesystem_sku_get` | ❌ |
| 2 | 0.613674 | `managedlustre_filesystem_list` | ❌ |
| 3 | 0.513242 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.496242 | `managedlustre_filesystem_subnetsize_validate` | ❌ |
| 5 | 0.470347 | `kusto_cluster_list` | ❌ |

---

## Test 308
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `managedlustre_fs_subnetsize_ask`  
**Prompt:** Tell me how many IP addresses I need for an Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.739766 | `managedlustre_fs_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.651598 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 3 | 0.594536 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.559498 | `managedlustre_fs_list` | ❌ |
| 5 | 0.533351 | `managedlustre_fs_create` | ❌ |

---

## Test 304
=======
<<<<<<< HEAD
| 1 | 0.739679 | `managedlustre_fs_subnetsize_ask` | ✅ **EXPECTED** |
| 2 | 0.651615 | `managedlustre_fs_subnetsize_validate` | ❌ |
| 3 | 0.594695 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.559034 | `managedlustre_fs_list` | ❌ |
| 5 | 0.533796 | `managedlustre_fs_create` | ❌ |

---

## Test 299
=======
| 1 | 0.739721 | `managedlustre_filesystem_subnetsize_ask` | ❌ |
| 2 | 0.651551 | `managedlustre_filesystem_subnetsize_validate` | ❌ |
| 3 | 0.594559 | `managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.559415 | `managedlustre_filesystem_list` | ❌ |
| 5 | 0.533625 | `managedlustre_filesystem_create` | ❌ |

---

## Test 309
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `managedlustre_fs_subnetsize_validate`  
**Prompt:** Validate if the network <subnet_id> can host Azure Managed Lustre filesystem of size <filesystem_size> using the SKU <sku>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.879240 | `managedlustre_fs_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.622368 | `managedlustre_fs_subnetsize_ask` | ❌ |
| 3 | 0.542555 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.516032 | `managedlustre_fs_create` | ❌ |
| 5 | 0.480796 | `managedlustre_fs_list` | ❌ |

---

## Test 305
=======
<<<<<<< HEAD
| 1 | 0.879541 | `managedlustre_fs_subnetsize_validate` | ✅ **EXPECTED** |
| 2 | 0.622603 | `managedlustre_fs_subnetsize_ask` | ❌ |
| 3 | 0.542788 | `managedlustre_fs_sku_get` | ❌ |
| 4 | 0.515947 | `managedlustre_fs_create` | ❌ |
| 5 | 0.480673 | `managedlustre_fs_list` | ❌ |

---

## Test 300
=======
| 1 | 0.879453 | `managedlustre_filesystem_subnetsize_validate` | ❌ |
| 2 | 0.622511 | `managedlustre_filesystem_subnetsize_ask` | ❌ |
| 3 | 0.542894 | `managedlustre_filesystem_sku_get` | ❌ |
| 4 | 0.516028 | `managedlustre_filesystem_create` | ❌ |
| 5 | 0.480920 | `managedlustre_filesystem_list` | ❌ |

---

## Test 310
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `managedlustre_fs_update`  
**Prompt:** Update the maintenance window of the Azure Managed Lustre filesystem <filesystem_name> to <maintenance_window_day> at <maintenance_window_time>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.738895 | `managedlustre_fs_update` | ✅ **EXPECTED** |
| 2 | 0.525980 | `managedlustre_fs_create` | ❌ |
| 3 | 0.487193 | `managedlustre_fs_list` | ❌ |
| 4 | 0.385318 | `managedlustre_fs_sku_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.739000 | `managedlustre_fs_update` | ✅ **EXPECTED** |
| 2 | 0.527525 | `managedlustre_fs_create` | ❌ |
| 3 | 0.487003 | `managedlustre_fs_list` | ❌ |
| 4 | 0.385349 | `managedlustre_fs_sku_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.344891 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

<<<<<<< HEAD
## Test 306
=======
## Test 301
=======
| 1 | 0.739000 | `managedlustre_filesystem_update` | ❌ |
| 2 | 0.527525 | `managedlustre_filesystem_create` | ❌ |
| 3 | 0.487193 | `managedlustre_filesystem_list` | ❌ |
| 4 | 0.385349 | `managedlustre_filesystem_sku_get` | ❌ |
| 5 | 0.344891 | `managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 311
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `marketplace_product_get`  
**Prompt:** Get details about marketplace product <product_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.570164 | `marketplace_product_get` | ✅ **EXPECTED** |
| 2 | 0.499208 | `marketplace_product_list` | ❌ |
| 3 | 0.353280 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.333304 | `servicebus_topic_details` | ❌ |
| 5 | 0.330949 | `servicebus_queue_details` | ❌ |

---

## Test 307
=======
<<<<<<< HEAD
| 1 | 0.570028 | `marketplace_product_get` | ✅ **EXPECTED** |
=======
| 1 | 0.570109 | `marketplace_product_get` | ✅ **EXPECTED** |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.499184 | `marketplace_product_list` | ❌ |
| 3 | 0.353256 | `servicebus_topic_subscription_details` | ❌ |
| 4 | 0.333160 | `servicebus_topic_details` | ❌ |
| 5 | 0.330935 | `servicebus_queue_details` | ❌ |

---

<<<<<<< HEAD
## Test 302
=======
## Test 312
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Search for Microsoft products in the marketplace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.607950 | `marketplace_product_list` | ✅ **EXPECTED** |
| 2 | 0.443177 | `marketplace_product_get` | ❌ |
| 3 | 0.341360 | `search_service_list` | ❌ |
| 4 | 0.330544 | `foundry_models_list` | ❌ |
| 5 | 0.328671 | `managedlustre_fs_sku_get` | ❌ |

---

## Test 308
=======
| 1 | 0.607916 | `marketplace_product_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.443178 | `marketplace_product_get` | ❌ |
=======
| 2 | 0.443109 | `marketplace_product_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.343549 | `search_service_list` | ❌ |
| 4 | 0.330500 | `foundry_models_list` | ❌ |
| 5 | 0.328676 | `managedlustre_fs_sku_get` | ❌ |

---

<<<<<<< HEAD
## Test 303
=======
## Test 313
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `marketplace_product_list`  
**Prompt:** Show me marketplace products from publisher <publisher_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.537726 | `marketplace_product_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.385167 | `marketplace_product_get` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.385198 | `marketplace_product_get` | ❌ |
=======
| 2 | 0.385111 | `marketplace_product_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.308769 | `foundry_models_list` | ❌ |
| 4 | 0.288006 | `redis_list` | ❌ |
| 5 | 0.260421 | `managedlustre_fs_sku_get` | ❌ |

---

<<<<<<< HEAD
## Test 309

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Get best practices for building AI applications in Azure  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.675775 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.555579 | `get_bestpractices_get` | ❌ |
| 3 | 0.501210 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.480026 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.477592 | `cloudarchitect_design` | ❌ |

---

## Test 310

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Show me the best practices for Azure AI Foundry agents code generation  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.699440 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.603487 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.534202 | `get_bestpractices_get` | ❌ |
| 4 | 0.520202 | `foundry_agents_list` | ❌ |
| 5 | 0.508727 | `azureterraformbestpractices_get` | ❌ |

---

## Test 311

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Get guidance for building agents with Azure AI Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.635165 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.593029 | `foundry_agents_get-sdk-sample` | ❌ |
| 3 | 0.553580 | `foundry_agents_list` | ❌ |
| 4 | 0.534256 | `foundry_agents_create` | ❌ |
| 5 | 0.513217 | `foundry_agents_connect` | ❌ |

---

## Test 312

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Create an AI app that helps me to manage travel queries.  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.417629 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.343844 | `foundry_threads_create` | ❌ |
| 3 | 0.327503 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.320532 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.311958 | `foundry_agents_connect` | ❌ |

---

## Test 313

**Expected Tool:** `azureaibestpractices_get`  
**Prompt:** Create an AI app that helps me to manage travel queries in Azure AI Foundry  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.517931 | `azureaibestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.478747 | `foundry_openai_embeddings-create` | ❌ |
| 3 | 0.469654 | `foundry_openai_chat-completions-create` | ❌ |
| 4 | 0.466216 | `foundry_openai_create-completion` | ❌ |
| 5 | 0.456719 | `foundry_resource_get` | ❌ |

---

## Test 314
=======
<<<<<<< HEAD
## Test 304
=======
## Test 314
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.656395 | `azureaibestpractices_get` | ❌ |
| 2 | 0.646844 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 3 | 0.635406 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.586907 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.531457 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 315
=======
<<<<<<< HEAD
| 1 | 0.646857 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.635437 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.586894 | `deploy_iac_rules_get` | ❌ |
=======
| 1 | 0.651264 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.635406 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.586907 | `deploy_iac_rules_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.531727 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.490235 | `deploy_plan_get` | ❌ |

---

<<<<<<< HEAD
## Test 305
=======
## Test 315
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.600903 | `get_bestpractices_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.548542 | `azureterraformbestpractices_get` | ❌ |
=======
| 2 | 0.548655 | `azureterraformbestpractices_get` | ❌ |
=======
| 1 | 0.602216 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.548542 | `azureterraformbestpractices_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.541091 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.516852 | `deploy_plan_get` | ❌ |
| 5 | 0.516203 | `deploy_pipeline_guidance_get` | ❌ |

---

<<<<<<< HEAD
## Test 316
=======
<<<<<<< HEAD
## Test 306
=======
## Test 316
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.625259 | `get_bestpractices_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.594323 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.539715 | `azureaibestpractices_get` | ❌ |
| 4 | 0.518643 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.465370 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 317
=======
| 2 | 0.594455 | `azureterraformbestpractices_get` | ❌ |
=======
| 1 | 0.624689 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.594323 | `azureterraformbestpractices_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.518643 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.465572 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.450629 | `cloudarchitect_design` | ❌ |

---

<<<<<<< HEAD
## Test 307
=======
## Test 317
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions code generation best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.624273 | `get_bestpractices_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.587474 | `azureaibestpractices_get` | ❌ |
| 3 | 0.570488 | `azureterraformbestpractices_get` | ❌ |
| 4 | 0.522998 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.493745 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 318
=======
| 2 | 0.570547 | `azureterraformbestpractices_get` | ❌ |
=======
| 1 | 0.629031 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.570488 | `azureterraformbestpractices_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.522998 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.493998 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.467377 | `extension_cli_install` | ❌ |

---

<<<<<<< HEAD
## Test 308
=======
## Test 318
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions deployment best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.581850 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.497056 | `deploy_pipeline_guidance_get` | ❌ |
=======
| 1 | 0.584392 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.497350 | `deploy_pipeline_guidance_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.495659 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.486886 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.474511 | `deploy_plan_get` | ❌ |

---

<<<<<<< HEAD
## Test 319
=======
<<<<<<< HEAD
## Test 309
=======
## Test 319
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Functions best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.610986 | `get_bestpractices_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.532790 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.518386 | `azureaibestpractices_get` | ❌ |
| 4 | 0.487322 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.457812 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 320
=======
| 2 | 0.532921 | `azureterraformbestpractices_get` | ❌ |
=======
| 1 | 0.612552 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.532790 | `azureterraformbestpractices_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.487322 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.458060 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.448034 | `extension_cli_install` | ❌ |

---

<<<<<<< HEAD
## Test 310
=======
## Test 320
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** Get the latest Azure Static Web Apps best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.557862 | `get_bestpractices_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.513262 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.510399 | `azureaibestpractices_get` | ❌ |
| 4 | 0.505123 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.483482 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 321
=======
| 2 | 0.513385 | `azureterraformbestpractices_get` | ❌ |
=======
| 1 | 0.559184 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.513262 | `azureterraformbestpractices_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.505123 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.483705 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.421581 | `cloudarchitect_design` | ❌ |

---

<<<<<<< HEAD
## Test 311
=======
## Test 321
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** What are azure function best practices?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.582541 | `get_bestpractices_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.500368 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.475018 | `azureaibestpractices_get` | ❌ |
| 4 | 0.472112 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.432959 | `deploy_pipeline_guidance_get` | ❌ |

---

## Test 322
=======
| 2 | 0.500479 | `azureterraformbestpractices_get` | ❌ |
=======
| 1 | 0.584536 | `get_bestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.500368 | `azureterraformbestpractices_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.472112 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.433134 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.432087 | `cloudarchitect_design` | ❌ |

---

<<<<<<< HEAD
## Test 312
=======
## Test 322
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `get_bestpractices_get`  
**Prompt:** configure azure mcp in coding agent for my repo  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.488855 | `deploy_plan_get` | ❌ |
| 2 | 0.460745 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.390270 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.370753 | `azureaibestpractices_get` | ❌ |
| 5 | 0.370298 | `azureterraformbestpractices_get` | ❌ |

---

<<<<<<< HEAD
## Test 323
=======
## Test 313
=======
| 1 | 0.488915 | `deploy_plan_get` | ❌ |
| 2 | 0.460980 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.390340 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.370368 | `azureterraformbestpractices_get` | ❌ |
| 5 | 0.369284 | `extension_cli_install` | ❌ |

---

## Test 323
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_activitylog_list`  
**Prompt:** List the activity logs of the last month for <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.537893 | `monitor_activitylog_list` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 1 | 0.537916 | `monitor_activitylog_list` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.506212 | `monitor_resource_log_query` | ❌ |
| 3 | 0.371728 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.363798 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.344629 | `datadog_monitoredresources_list` | ❌ |

---

<<<<<<< HEAD
## Test 324
=======
## Test 314
=======
| 1 | 0.537780 | `monitor_activitylog_list` | ✅ **EXPECTED** |
| 2 | 0.506270 | `monitor_resource_log_query` | ❌ |
| 3 | 0.371737 | `monitor_workspace_log_query` | ❌ |
| 4 | 0.363731 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.344620 | `datadog_monitoredresources_list` | ❌ |

---

## Test 324
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_healthmodels_entity_get`  
**Prompt:** Show me the health status of entity <entity_id> using the health model <health_model_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.660947 | `monitor_healthmodels_entity_get` | ✅ **EXPECTED** |
| 2 | 0.608665 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.351697 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.328321 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.288127 | `foundry_models_deployments_list` | ❌ |

---

<<<<<<< HEAD
## Test 325
=======
## Test 315
=======
| 1 | 0.660947 | `monitor_healthmodels_entity_gethealth` | ❌ |
| 2 | 0.603153 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.355116 | `foundry_openai_models-list` | ❌ |
| 4 | 0.351697 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.328321 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 325
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Get metric definitions for <resource_type> <resource_name> from the namespace  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.592640 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424141 | `monitor_metrics_query` | ❌ |
| 3 | 0.368006 | `bicepschema_get` | ❌ |
| 4 | 0.332369 | `monitor_table_type_list` | ❌ |
| 5 | 0.325634 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 326
=======
<<<<<<< HEAD
| 1 | 0.592676 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424006 | `monitor_metrics_query` | ❌ |
| 3 | 0.368319 | `bicepschema_get` | ❌ |
| 4 | 0.332356 | `monitor_table_type_list` | ❌ |
| 5 | 0.324986 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 316
=======
| 1 | 0.592640 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.424141 | `monitor_metrics_query` | ❌ |
| 3 | 0.368319 | `bicepschema_get` | ❌ |
| 4 | 0.332356 | `monitor_table_type_list` | ❌ |
| 5 | 0.323083 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 326
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** Show me all available metrics and their definitions for storage account <account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.607600 | `storage_account_get` | ❌ |
| 2 | 0.587736 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.544043 | `storage_blob_container_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.607537 | `storage_account_get` | ❌ |
| 2 | 0.587640 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.544781 | `storage_blob_container_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.495829 | `storage_blob_get` | ❌ |
| 5 | 0.473421 | `managedlustre_fs_list` | ❌ |

---

<<<<<<< HEAD
## Test 327
=======
## Test 317
=======
| 1 | 0.607575 | `storage_account_get` | ❌ |
| 2 | 0.587736 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 3 | 0.544781 | `storage_blob_container_get` | ❌ |
| 4 | 0.495829 | `storage_blob_get` | ❌ |
| 5 | 0.473421 | `managedlustre_filesystem_list` | ❌ |

---

## Test 327
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_metrics_definitions`  
**Prompt:** What metric definitions are available for the Application Insights resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.633173 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495513 | `monitor_metrics_query` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.633132 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495439 | `monitor_metrics_query` | ❌ |
=======
| 1 | 0.633173 | `monitor_metrics_definitions` | ✅ **EXPECTED** |
| 2 | 0.495513 | `monitor_metrics_query` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.433945 | `monitor_resource_log_query` | ❌ |
| 4 | 0.392960 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.388569 | `bicepschema_get` | ❌ |

---

<<<<<<< HEAD
## Test 328
=======
<<<<<<< HEAD
## Test 318
=======
## Test 328
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Analyze the performance trends and response times for Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.555377 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.527530 | `monitor_resource_log_query` | ❌ |
| 3 | 0.464743 | `applens_resource_diagnose` | ❌ |
| 4 | 0.420462 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.413282 | `applicationinsights_recommendation_list` | ❌ |

---

<<<<<<< HEAD
## Test 329
=======
<<<<<<< HEAD
## Test 319
=======
## Test 329
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Check the availability metrics for my Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.557830 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.476671 | `monitor_resource_log_query` | ❌ |
<<<<<<< HEAD
| 3 | 0.460611 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.456360 | `quota_usage_check` | ❌ |
| 5 | 0.438233 | `monitor_metrics_definitions` | ❌ |

---

## Test 330
=======
<<<<<<< HEAD
| 3 | 0.460351 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.456321 | `quota_usage_check` | ❌ |
| 5 | 0.438171 | `monitor_metrics_definitions` | ❌ |

---

## Test 320
=======
| 3 | 0.460611 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.455904 | `quota_usage_check` | ❌ |
| 5 | 0.438233 | `monitor_metrics_definitions` | ❌ |

---

## Test 330
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Get the <aggregation_type> <metric_name> metric for <resource_type> <resource_name> over the last <time_period> with intervals  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.461249 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390029 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.338557 | `monitor_resource_log_query` | ❌ |
| 4 | 0.335118 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.306338 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 331
=======
<<<<<<< HEAD
| 1 | 0.461138 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.389998 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.338392 | `monitor_resource_log_query` | ❌ |
| 4 | 0.334417 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.306224 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 321
=======
| 1 | 0.461249 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.390029 | `monitor_metrics_definitions` | ❌ |
| 3 | 0.338557 | `monitor_resource_log_query` | ❌ |
| 4 | 0.330533 | `resourcehealth_availability-status_get` | ❌ |
| 5 | 0.306338 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 331
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Investigate error rates and failed requests for Application Insights resource <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.496878 | `monitor_resource_log_query` | ❌ |
<<<<<<< HEAD
| 2 | 0.492138 | `monitor_metrics_query` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 2 | 0.491782 | `monitor_metrics_query` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.448148 | `applens_resource_diagnose` | ❌ |
| 4 | 0.412184 | `resourcehealth_health-events_list` | ❌ |
| 5 | 0.397853 | `quota_usage_check` | ❌ |

---

<<<<<<< HEAD
## Test 332
=======
## Test 322
=======
| 2 | 0.492138 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 3 | 0.448148 | `applens_resource_diagnose` | ❌ |
| 4 | 0.412184 | `resourcehealth_service-health-events_list` | ❌ |
| 5 | 0.397335 | `quota_usage_check` | ❌ |

---

## Test 332
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** Query the <metric_name> metric for <resource_type> <resource_name> for the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.525890 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.405838 | `monitor_resource_log_query` | ❌ |
| 3 | 0.384811 | `monitor_metrics_definitions` | ❌ |
| 4 | 0.347228 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.330657 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 333
=======
| 1 | 0.525326 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.406185 | `monitor_resource_log_query` | ❌ |
<<<<<<< HEAD
| 3 | 0.384524 | `monitor_metrics_definitions` | ❌ |
| 4 | 0.347723 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.330713 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 323
=======
| 3 | 0.384482 | `monitor_metrics_definitions` | ❌ |
| 4 | 0.347723 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.325967 | `resourcehealth_availability-status_get` | ❌ |

---

## Test 333
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_metrics_query`  
**Prompt:** What's the request per second rate for my Application Insights resource <resource_name> over the last <time_period>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.480140 | `monitor_metrics_query` | ✅ **EXPECTED** |
| 2 | 0.444779 | `monitor_resource_log_query` | ❌ |
| 3 | 0.388382 | `applens_resource_diagnose` | ❌ |
<<<<<<< HEAD
| 4 | 0.363672 | `quota_usage_check` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.363640 | `quota_usage_check` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.350076 | `resourcehealth_health-events_list` | ❌ |

---

<<<<<<< HEAD
## Test 334
=======
## Test 324
=======
| 4 | 0.363412 | `quota_usage_check` | ❌ |
| 5 | 0.350076 | `resourcehealth_service-health-events_list` | ❌ |

---

## Test 334
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_resource_log_query`  
**Prompt:** Show me the logs for the past hour for the resource <resource_name> in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.687852 | `monitor_resource_log_query` | ✅ **EXPECTED** |
| 2 | 0.621919 | `monitor_workspace_log_query` | ❌ |
<<<<<<< HEAD
| 3 | 0.598393 | `monitor_activitylog_list` | ❌ |
| 4 | 0.485528 | `deploy_app_logs_get` | ❌ |
| 5 | 0.469703 | `monitor_metrics_query` | ❌ |

---

## Test 335
=======
<<<<<<< HEAD
| 3 | 0.598436 | `monitor_activitylog_list` | ❌ |
=======
| 3 | 0.598393 | `monitor_activitylog_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.485633 | `deploy_app_logs_get` | ❌ |
| 5 | 0.470119 | `monitor_metrics_query` | ❌ |

---

<<<<<<< HEAD
## Test 325
=======
## Test 335
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_table_list`  
**Prompt:** List all tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.851075 | `monitor_table_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.725693 | `monitor_table_type_list` | ❌ |
| 3 | 0.620451 | `monitor_workspace_list` | ❌ |
| 4 | 0.541928 | `kusto_table_list` | ❌ |
=======
| 2 | 0.725738 | `monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `monitor_workspace_list` | ❌ |
| 4 | 0.541959 | `kusto_table_list` | ❌ |
=======
| 1 | 0.850522 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.725738 | `monitor_table_type_list` | ❌ |
| 3 | 0.620445 | `monitor_workspace_list` | ❌ |
| 4 | 0.541928 | `kusto_table_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.539481 | `monitor_workspace_log_query` | ❌ |

---

<<<<<<< HEAD
## Test 336
=======
<<<<<<< HEAD
## Test 326
=======
## Test 336
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_table_list`  
**Prompt:** Show me the tables in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.798459 | `monitor_table_list` | ✅ **EXPECTED** |
| 2 | 0.701092 | `monitor_table_type_list` | ❌ |
| 3 | 0.600003 | `monitor_workspace_list` | ❌ |
| 4 | 0.542820 | `monitor_workspace_log_query` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.798460 | `monitor_table_list` | ✅ **EXPECTED** |
=======
| 1 | 0.798109 | `monitor_table_list` | ✅ **EXPECTED** |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.701122 | `monitor_table_type_list` | ❌ |
| 3 | 0.599917 | `monitor_workspace_list` | ❌ |
| 4 | 0.542821 | `monitor_workspace_log_query` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.502882 | `monitor_resource_log_query` | ❌ |

---

<<<<<<< HEAD
## Test 337
=======
<<<<<<< HEAD
## Test 327
=======
## Test 337
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** List all available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.881468 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.765694 | `monitor_table_list` | ❌ |
| 3 | 0.570092 | `monitor_workspace_list` | ❌ |
| 4 | 0.504683 | `mysql_table_list` | ❌ |
=======
| 1 | 0.881524 | `monitor_table_type_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.765702 | `monitor_table_list` | ❌ |
| 3 | 0.569921 | `monitor_workspace_list` | ❌ |
| 4 | 0.504789 | `mysql_table_list` | ❌ |
=======
| 2 | 0.765548 | `monitor_table_list` | ❌ |
| 3 | 0.569921 | `monitor_workspace_list` | ❌ |
| 4 | 0.504683 | `mysql_table_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.497622 | `monitor_workspace_log_query` | ❌ |

---

<<<<<<< HEAD
## Test 338
=======
<<<<<<< HEAD
## Test 328
=======
## Test 338
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_table_type_list`  
**Prompt:** Show me the available table types in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.843110 | `monitor_table_type_list` | ✅ **EXPECTED** |
| 2 | 0.736831 | `monitor_table_list` | ❌ |
| 3 | 0.576934 | `monitor_workspace_list` | ❌ |
=======
| 1 | 0.843138 | `monitor_table_type_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.736837 | `monitor_table_list` | ❌ |
=======
| 2 | 0.736830 | `monitor_table_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.576731 | `monitor_workspace_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.509598 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.481189 | `mysql_table_list` | ❌ |

---

<<<<<<< HEAD
## Test 339
=======
<<<<<<< HEAD
## Test 329
=======
## Test 339
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_webtests_create`  
**Prompt:** Create a new Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.651084 | `monitor_webtests_create` | ✅ **EXPECTED** |
| 2 | 0.570105 | `monitor_webtests_list` | ❌ |
| 3 | 0.550426 | `monitor_webtests_update` | ❌ |
| 4 | 0.533477 | `monitor_webtests_get` | ❌ |
| 5 | 0.482251 | `loadtesting_testresource_create` | ❌ |

---

## Test 340
=======
<<<<<<< HEAD
| 1 | 0.650749 | `monitor_webtests_create` | ✅ **EXPECTED** |
| 2 | 0.569999 | `monitor_webtests_list` | ❌ |
| 3 | 0.550088 | `monitor_webtests_update` | ❌ |
| 4 | 0.533466 | `monitor_webtests_get` | ❌ |
| 5 | 0.482122 | `loadtesting_testresource_create` | ❌ |

---

## Test 330
=======
| 1 | 0.650734 | `monitor_webtests_create` | ✅ **EXPECTED** |
| 2 | 0.572163 | `monitor_webtests_list` | ❌ |
| 3 | 0.550075 | `monitor_webtests_update` | ❌ |
| 4 | 0.533352 | `monitor_webtests_get` | ❌ |
| 5 | 0.482145 | `loadtesting_testresource_create` | ❌ |

---

## Test 340
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_webtests_get`  
**Prompt:** Get Web Test details for <webtest_resource_name> in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.758910 | `monitor_webtests_get` | ✅ **EXPECTED** |
| 2 | 0.725360 | `monitor_webtests_list` | ❌ |
| 3 | 0.583663 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.562785 | `monitor_webtests_update` | ❌ |
| 5 | 0.530432 | `monitor_webtests_create` | ❌ |

---

## Test 341
=======
<<<<<<< HEAD
| 1 | 0.759380 | `monitor_webtests_get` | ✅ **EXPECTED** |
| 2 | 0.725337 | `monitor_webtests_list` | ❌ |
| 3 | 0.583816 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.562797 | `monitor_webtests_update` | ❌ |
| 5 | 0.530557 | `monitor_webtests_create` | ❌ |

---

## Test 331
=======
| 1 | 0.759062 | `monitor_webtests_get` | ✅ **EXPECTED** |
| 2 | 0.726138 | `monitor_webtests_list` | ❌ |
| 3 | 0.583770 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.562773 | `monitor_webtests_update` | ❌ |
| 5 | 0.530496 | `monitor_webtests_create` | ❌ |

---

## Test 341
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.730616 | `monitor_webtests_list` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 1 | 0.730568 | `monitor_webtests_list` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.610160 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.547708 | `grafana_list` | ❌ |
| 4 | 0.520828 | `redis_list` | ❌ |
| 5 | 0.496166 | `monitor_webtests_get` | ❌ |

---

<<<<<<< HEAD
## Test 342
=======
## Test 332
=======
| 1 | 0.732801 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.610160 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.547708 | `grafana_list` | ❌ |
| 4 | 0.520829 | `redis_list` | ❌ |
| 5 | 0.496166 | `monitor_webtests_get` | ❌ |

---

## Test 342
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_webtests_list`  
**Prompt:** List all Web Test resources in my subscription in <resource_group>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.793807 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.675965 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.584429 | `monitor_webtests_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.793702 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.675965 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.584942 | `monitor_webtests_get` | ❌ |
=======
| 1 | 0.793581 | `monitor_webtests_list` | ✅ **EXPECTED** |
| 2 | 0.675965 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.584429 | `monitor_webtests_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.573602 | `group_list` | ❌ |
| 5 | 0.546088 | `resourcehealth_availability-status_list` | ❌ |

---

<<<<<<< HEAD
## Test 343
=======
<<<<<<< HEAD
## Test 333
=======
## Test 343
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_webtests_update`  
**Prompt:** Update an existing Standard Web Test with name <webtest_resource_name> in my subscription in <resource_group> in a given <appinsights_component>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.686427 | `monitor_webtests_update` | ✅ **EXPECTED** |
| 2 | 0.558816 | `monitor_webtests_get` | ❌ |
| 3 | 0.557828 | `monitor_webtests_create` | ❌ |
| 4 | 0.553372 | `monitor_webtests_list` | ❌ |
| 5 | 0.509192 | `loadtesting_testrun_update` | ❌ |

---

## Test 344
=======
<<<<<<< HEAD
| 1 | 0.686449 | `monitor_webtests_update` | ✅ **EXPECTED** |
| 2 | 0.559199 | `monitor_webtests_get` | ❌ |
| 3 | 0.558234 | `monitor_webtests_create` | ❌ |
| 4 | 0.553545 | `monitor_webtests_list` | ❌ |
| 5 | 0.508736 | `loadtesting_testrun_update` | ❌ |

---

## Test 334
=======
| 1 | 0.686466 | `monitor_webtests_update` | ✅ **EXPECTED** |
| 2 | 0.559612 | `monitor_webtests_get` | ❌ |
| 3 | 0.558102 | `monitor_webtests_create` | ❌ |
| 4 | 0.555899 | `monitor_webtests_list` | ❌ |
| 5 | 0.509033 | `loadtesting_testrun_update` | ❌ |

---

## Test 344
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** List all Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.813871 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.680201 | `grafana_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.660127 | `monitor_table_list` | ❌ |
| 4 | 0.610623 | `kusto_cluster_list` | ❌ |
| 5 | 0.599636 | `search_service_list` | ❌ |

---

## Test 345
=======
<<<<<<< HEAD
| 3 | 0.660135 | `monitor_table_list` | ❌ |
| 4 | 0.610623 | `kusto_cluster_list` | ❌ |
=======
| 3 | 0.659287 | `monitor_table_list` | ❌ |
| 4 | 0.610480 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.600802 | `search_service_list` | ❌ |

---

<<<<<<< HEAD
## Test 335
=======
## Test 345
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me my Log Analytics workspaces  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.656159 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.585355 | `monitor_table_list` | ❌ |
| 3 | 0.531036 | `monitor_table_type_list` | ❌ |
| 4 | 0.518275 | `grafana_list` | ❌ |
| 5 | 0.506663 | `monitor_workspace_log_query` | ❌ |

---

## Test 346
=======
<<<<<<< HEAD
| 1 | 0.656194 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.585436 | `monitor_table_list` | ❌ |
| 3 | 0.531083 | `monitor_table_type_list` | ❌ |
| 4 | 0.518254 | `grafana_list` | ❌ |
| 5 | 0.506772 | `monitor_workspace_log_query` | ❌ |

---

## Test 336
=======
| 1 | 0.656153 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.584651 | `monitor_table_list` | ❌ |
| 3 | 0.531025 | `monitor_table_type_list` | ❌ |
| 4 | 0.518275 | `grafana_list` | ❌ |
| 5 | 0.506663 | `monitor_workspace_log_query` | ❌ |

---

## Test 346
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_workspace_list`  
**Prompt:** Show me the Log Analytics workspaces in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.732964 | `monitor_workspace_list` | ✅ **EXPECTED** |
| 2 | 0.601481 | `grafana_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.580244 | `monitor_table_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.580261 | `monitor_table_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.523782 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.522749 | `kusto_cluster_list` | ❌ |

---

<<<<<<< HEAD
## Test 347
=======
## Test 337
=======
| 3 | 0.579582 | `monitor_table_list` | ❌ |
| 4 | 0.523782 | `monitor_workspace_log_query` | ❌ |
| 5 | 0.522605 | `kusto_cluster_list` | ❌ |

---

## Test 347
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `monitor_workspace_log_query`  
**Prompt:** Show me the logs for the past hour in the Log Analytics workspace <workspace_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.610115 | `monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.587614 | `monitor_resource_log_query` | ❌ |
| 3 | 0.527733 | `monitor_activitylog_list` | ❌ |
| 4 | 0.498148 | `deploy_app_logs_get` | ❌ |
| 5 | 0.485982 | `monitor_table_list` | ❌ |

---

<<<<<<< HEAD
## Test 348
=======
## Test 338
=======
| 1 | 0.610116 | `monitor_workspace_log_query` | ✅ **EXPECTED** |
| 2 | 0.587644 | `monitor_resource_log_query` | ❌ |
| 3 | 0.527761 | `monitor_activitylog_list` | ❌ |
| 4 | 0.498255 | `deploy_app_logs_get` | ❌ |
| 5 | 0.485667 | `monitor_table_list` | ❌ |

---

## Test 348
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** List all monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.668828 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.454270 | `redis_list` | ❌ |
| 3 | 0.413661 | `loadtesting_testresource_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.413173 | `monitor_metrics_query` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.413208 | `monitor_metrics_query` | ❌ |
=======
| 4 | 0.413173 | `monitor_metrics_query` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.401731 | `grafana_list` | ❌ |

---

<<<<<<< HEAD
## Test 349
=======
<<<<<<< HEAD
## Test 339
=======
## Test 349
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `datadog_monitoredresources_list`  
**Prompt:** Show me the monitored resources in the Datadog resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624066 | `datadog_monitoredresources_list` | ✅ **EXPECTED** |
| 2 | 0.443481 | `monitor_metrics_query` | ❌ |
| 3 | 0.440052 | `redis_list` | ❌ |
| 4 | 0.424391 | `monitor_resource_log_query` | ❌ |
| 5 | 0.385122 | `loadtesting_testresource_list` | ❌ |

---

<<<<<<< HEAD
## Test 350
=======
<<<<<<< HEAD
## Test 340
=======
## Test 350
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `extension_azqr`  
**Prompt:** Check my Azure subscription for any compliance issues or recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.533403 | `quota_usage_check` | ❌ |
| 2 | 0.481143 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.476826 | `extension_azqr` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 1 | 0.533406 | `quota_usage_check` | ❌ |
| 2 | 0.481236 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.476761 | `extension_azqr` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.471547 | `subscription_list` | ❌ |
=======
| 1 | 0.533164 | `quota_usage_check` | ❌ |
| 2 | 0.481143 | `azureterraformbestpractices_get` | ❌ |
| 3 | 0.476826 | `extension_azqr` | ✅ **EXPECTED** |
| 4 | 0.471499 | `subscription_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.468404 | `applens_resource_diagnose` | ❌ |

---

<<<<<<< HEAD
## Test 351
=======
<<<<<<< HEAD
## Test 341
=======
## Test 351
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `extension_azqr`  
**Prompt:** Provide compliance recommendations for my current Azure subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.532792 | `azureterraformbestpractices_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.532869 | `azureterraformbestpractices_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.492863 | `get_bestpractices_get` | ❌ |
=======
| 1 | 0.532792 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.492602 | `get_bestpractices_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.476164 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.473365 | `deploy_iac_rules_get` | ❌ |
| 5 | 0.468491 | `azureaibestpractices_get` | ❌ |

---

<<<<<<< HEAD
## Test 352
=======
<<<<<<< HEAD
## Test 342
=======
## Test 352
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `extension_azqr`  
**Prompt:** Scan my Azure subscription for compliance recommendations  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.536917 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.516910 | `extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.514947 | `applicationinsights_recommendation_list` | ❌ |
| 4 | 0.504918 | `quota_usage_check` | ❌ |
| 5 | 0.494808 | `deploy_plan_get` | ❌ |

---

## Test 353
=======
| 1 | 0.536984 | `azureterraformbestpractices_get` | ❌ |
| 2 | 0.516810 | `extension_azqr` | ✅ **EXPECTED** |
| 3 | 0.514978 | `applicationinsights_recommendation_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.504929 | `quota_usage_check` | ❌ |
=======
| 4 | 0.504673 | `quota_usage_check` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.494872 | `deploy_plan_get` | ❌ |

---

<<<<<<< HEAD
## Test 343
=======
## Test 353
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `quota_region_availability_list`  
**Prompt:** Show me the available regions for these resource types <resource_types>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.590878 | `quota_region_availability_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.413662 | `quota_usage_check` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.413577 | `quota_usage_check` | ❌ |
=======
| 2 | 0.413274 | `quota_usage_check` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.391332 | `redis_list` | ❌ |
| 4 | 0.372940 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.369915 | `managedlustre_fs_sku_get` | ❌ |

---

<<<<<<< HEAD
## Test 354
=======
<<<<<<< HEAD
## Test 344
=======
## Test 354
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `quota_usage_check`  
**Prompt:** Check usage information for <resource_type> in region <region>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.609711 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.376819 | `resourcehealth_availability-status_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.609607 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `quota_region_availability_list` | ❌ |
| 3 | 0.384500 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.376368 | `resourcehealth_availability-status_get` | ❌ |
=======
| 1 | 0.609244 | `quota_usage_check` | ✅ **EXPECTED** |
| 2 | 0.491058 | `quota_region_availability_list` | ❌ |
| 3 | 0.384350 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.374248 | `resourcehealth_availability-status_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.371407 | `redis_list` | ❌ |

---

<<<<<<< HEAD
## Test 355
=======
<<<<<<< HEAD
## Test 345
=======
## Test 355
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `role_assignment_list`  
**Prompt:** List all available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.645258 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.539757 | `subscription_list` | ❌ |
| 3 | 0.483988 | `group_list` | ❌ |
| 4 | 0.478700 | `grafana_list` | ❌ |
| 5 | 0.471431 | `cosmos_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 356
=======
<<<<<<< HEAD
## Test 346
=======
## Test 356
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `role_assignment_list`  
**Prompt:** Show me the available role assignments in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.609704 | `role_assignment_list` | ✅ **EXPECTED** |
| 2 | 0.514697 | `subscription_list` | ❌ |
| 3 | 0.456956 | `grafana_list` | ❌ |
| 4 | 0.449753 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.445149 | `redis_list` | ❌ |

---

<<<<<<< HEAD
## Test 357
=======
<<<<<<< HEAD
## Test 347
=======
## Test 357
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `redis_list`  
**Prompt:** List all Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.810504 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.587836 | `grafana_list` | ❌ |
| 3 | 0.512954 | `kusto_cluster_list` | ❌ |
| 4 | 0.508532 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.501218 | `postgres_server_list` | ❌ |

---

## Test 358
=======
<<<<<<< HEAD
| 1 | 0.810487 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.587872 | `grafana_list` | ❌ |
| 3 | 0.512995 | `kusto_cluster_list` | ❌ |
| 4 | 0.508555 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.501183 | `postgres_server_list` | ❌ |

---

## Test 348
=======
| 1 | 0.810504 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.587836 | `grafana_list` | ❌ |
| 3 | 0.512970 | `kusto_cluster_list` | ❌ |
| 4 | 0.508531 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.501218 | `postgres_server_list` | ❌ |

---

## Test 358
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.685128 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.374327 | `grafana_list` | ❌ |
| 3 | 0.364197 | `datadog_monitoredresources_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.359659 | `mysql_server_list` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.359709 | `mysql_server_list` | ❌ |
=======
| 4 | 0.359659 | `mysql_server_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.331502 | `mysql_database_list` | ❌ |

---

<<<<<<< HEAD
## Test 359
=======
<<<<<<< HEAD
## Test 349
=======
## Test 359
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `redis_list`  
**Prompt:** Show me the Redis resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.781228 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.539177 | `grafana_list` | ❌ |
| 3 | 0.449276 | `datadog_monitoredresources_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.449014 | `postgres_server_list` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.448989 | `postgres_server_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.442854 | `kusto_cluster_list` | ❌ |

---

<<<<<<< HEAD
## Test 360
=======
## Test 350
=======
| 4 | 0.449014 | `postgres_server_list` | ❌ |
| 5 | 0.442860 | `kusto_cluster_list` | ❌ |

---

## Test 360
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `redis_list`  
**Prompt:** Show me my Redis caches  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.572767 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.316630 | `mysql_database_list` | ❌ |
| 3 | 0.301786 | `postgres_database_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.286513 | `mysql_server_list` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.286570 | `mysql_server_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.273014 | `kusto_cluster_list` | ❌ |

---

<<<<<<< HEAD
## Test 361
=======
## Test 351
=======
| 4 | 0.286513 | `mysql_server_list` | ❌ |
| 5 | 0.272972 | `kusto_cluster_list` | ❌ |

---

## Test 361
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `redis_list`  
**Prompt:** Get Redis clusters  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.478070 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.456308 | `kusto_cluster_list` | ❌ |
| 3 | 0.384630 | `kusto_cluster_get` | ❌ |
| 4 | 0.359935 | `kusto_database_list` | ❌ |
| 5 | 0.343305 | `aks_cluster_get` | ❌ |

---

## Test 362
=======
<<<<<<< HEAD
| 1 | 0.478109 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.456382 | `kusto_cluster_list` | ❌ |
| 3 | 0.384637 | `kusto_cluster_get` | ❌ |
| 4 | 0.359466 | `kusto_database_list` | ❌ |
| 5 | 0.343367 | `aks_cluster_get` | ❌ |

---

## Test 352
=======
| 1 | 0.478070 | `redis_list` | ✅ **EXPECTED** |
| 2 | 0.456311 | `kusto_cluster_list` | ❌ |
| 3 | 0.384630 | `kusto_cluster_get` | ❌ |
| 4 | 0.359797 | `kusto_database_list` | ❌ |
| 5 | 0.343305 | `aks_cluster_get` | ❌ |

---

## Test 362
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `group_list`  
**Prompt:** List all resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.755935 | `group_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.566552 | `workbooks_list` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.566497 | `workbooks_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.564566 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.552633 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.549477 | `monitor_webtests_list` | ❌ |

---

<<<<<<< HEAD
## Test 363
=======
## Test 353
=======
| 2 | 0.566552 | `workbooks_list` | ❌ |
| 3 | 0.564566 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.552633 | `datadog_monitoredresources_list` | ❌ |
| 5 | 0.546156 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 363
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `group_list`  
**Prompt:** Show me my resource groups  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.529504 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.464690 | `redis_list` | ❌ |
| 3 | 0.463685 | `datadog_monitoredresources_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.462391 | `mysql_server_list` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.462388 | `mysql_server_list` | ❌ |
=======
| 4 | 0.462391 | `mysql_server_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.460280 | `loadtesting_testresource_list` | ❌ |

---

<<<<<<< HEAD
## Test 364
=======
<<<<<<< HEAD
## Test 354
=======
## Test 364
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `group_list`  
**Prompt:** Show me the resource groups in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.665772 | `group_list` | ✅ **EXPECTED** |
| 2 | 0.532656 | `datadog_monitoredresources_list` | ❌ |
| 3 | 0.532505 | `redis_list` | ❌ |
| 4 | 0.532015 | `eventgrid_topic_list` | ❌ |
| 5 | 0.531920 | `resourcehealth_availability-status_list` | ❌ |

---

<<<<<<< HEAD
## Test 365
=======
<<<<<<< HEAD
## Test 355
=======
## Test 365
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Get the availability status for resource <resource_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.556926 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.378030 | `quota_usage_check` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.556629 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538277 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.377966 | `quota_usage_check` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.373112 | `monitor_healthmodels_entity_get` | ❌ |
| 5 | 0.349981 | `datadog_monitoredresources_list` | ❌ |

---

<<<<<<< HEAD
## Test 366
=======
## Test 356
=======
| 1 | 0.555432 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 2 | 0.538273 | `resourcehealth_availability-status_list` | ❌ |
| 3 | 0.404305 | `foundry_openai_models-list` | ❌ |
| 4 | 0.377586 | `quota_usage_check` | ❌ |
| 5 | 0.373112 | `monitor_healthmodels_entity_gethealth` | ❌ |

---

## Test 366
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** Show me the health status of the storage account <storage_account_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.576591 | `storage_account_get` | ❌ |
| 2 | 0.564706 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.555636 | `storage_blob_container_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.576617 | `storage_account_get` | ❌ |
| 2 | 0.564128 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.556167 | `storage_blob_container_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.487207 | `storage_blob_get` | ❌ |
| 5 | 0.466885 | `resourcehealth_availability-status_list` | ❌ |

---

<<<<<<< HEAD
## Test 367
=======
## Test 357
=======
| 1 | 0.576591 | `storage_account_get` | ❌ |
| 2 | 0.566633 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.556167 | `storage_blob_container_get` | ❌ |
| 4 | 0.487207 | `storage_blob_get` | ❌ |
| 5 | 0.466885 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 367
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_availability-status_get`  
**Prompt:** What is the availability status of virtual machine <vm_name> in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.577398 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.502794 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424939 | `mysql_server_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.577529 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.501568 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424957 | `mysql_server_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.412025 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.393479 | `managedlustre_fs_list` | ❌ |

---

<<<<<<< HEAD
## Test 368
=======
## Test 358
=======
| 1 | 0.577398 | `resourcehealth_availability-status_list` | ❌ |
| 2 | 0.502457 | `resourcehealth_availability-status_get` | ✅ **EXPECTED** |
| 3 | 0.424939 | `mysql_server_list` | ❌ |
| 4 | 0.413484 | `foundry_openai_models-list` | ❌ |
| 5 | 0.412025 | `loadtesting_testresource_list` | ❌ |

---

## Test 368
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** List availability status for all resources in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.737219 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.585501 | `redis_list` | ❌ |
| 3 | 0.549914 | `loadtesting_testresource_list` | ❌ |
| 4 | 0.548549 | `grafana_list` | ❌ |
| 5 | 0.544514 | `subscription_list` | ❌ |

---

<<<<<<< HEAD
## Test 369
=======
<<<<<<< HEAD
## Test 359
=======
## Test 369
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** Show me the health status of all my Azure resources  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.644982 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.544917 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.509740 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.508766 | `quota_usage_check` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.644908 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.545208 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.509740 | `resourcehealth_health-events_list` | ❌ |
| 4 | 0.508703 | `quota_usage_check` | ❌ |
=======
| 1 | 0.644982 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.546520 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.509740 | `resourcehealth_service-health-events_list` | ❌ |
| 4 | 0.508252 | `quota_usage_check` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.505776 | `redis_list` | ❌ |

---

<<<<<<< HEAD
## Test 370
=======
<<<<<<< HEAD
## Test 360
=======
## Test 370
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_availability-status_list`  
**Prompt:** What resources in resource group <resource_group_name> have health issues?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.596890 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.550812 | `resourcehealth_availability-status_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.596817 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.549900 | `resourcehealth_availability-status_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.496640 | `resourcehealth_health-events_list` | ❌ |
=======
| 1 | 0.596890 | `resourcehealth_availability-status_list` | ✅ **EXPECTED** |
| 2 | 0.551332 | `resourcehealth_availability-status_get` | ❌ |
| 3 | 0.496640 | `resourcehealth_service-health-events_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.441921 | `applens_resource_diagnose` | ❌ |
| 5 | 0.433614 | `loadtesting_testresource_list` | ❌ |

---

<<<<<<< HEAD
## Test 371
=======
<<<<<<< HEAD
## Test 361
=======
## Test 371
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List all service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.690720 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.553485 | `search_service_list` | ❌ |
| 3 | 0.534169 | `eventgrid_topic_list` | ❌ |
| 4 | 0.529200 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.518372 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 372
=======
<<<<<<< HEAD
| 1 | 0.690719 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
=======
| 1 | 0.690719 | `resourcehealth_service-health-events_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.554895 | `search_service_list` | ❌ |
| 3 | 0.534250 | `eventgrid_topic_list` | ❌ |
| 4 | 0.529761 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.518595 | `resourcehealth_availability-status_list` | ❌ |

---

<<<<<<< HEAD
## Test 362
=======
## Test 372
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me Azure service health events for subscription <subscription_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.686448 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.534707 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.513302 | `search_service_list` | ❌ |
| 4 | 0.513237 | `eventgrid_topic_list` | ❌ |
=======
=======
| 1 | 0.686448 | `resourcehealth_service-health-events_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.534556 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.513815 | `search_service_list` | ❌ |
| 4 | 0.513259 | `eventgrid_topic_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.501121 | `subscription_list` | ❌ |

---

<<<<<<< HEAD
## Test 373
=======
<<<<<<< HEAD
## Test 363
=======
## Test 373
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** What service issues have occurred in the last 30 days?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.450841 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.267663 | `applens_resource_diagnose` | ❌ |
| 3 | 0.245720 | `cloudarchitect_design` | ❌ |
| 4 | 0.216847 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211043 | `search_service_list` | ❌ |

---

## Test 374
=======
<<<<<<< HEAD
| 1 | 0.450909 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
| 2 | 0.267752 | `applens_resource_diagnose` | ❌ |
| 3 | 0.245709 | `cloudarchitect_design` | ❌ |
| 4 | 0.217130 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211900 | `search_service_list` | ❌ |

---

## Test 364
=======
| 1 | 0.450841 | `resourcehealth_service-health-events_list` | ❌ |
| 2 | 0.267663 | `applens_resource_diagnose` | ❌ |
| 3 | 0.245720 | `cloudarchitect_design` | ❌ |
| 4 | 0.216847 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.211842 | `search_service_list` | ❌ |

---

## Test 374
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** List active service health events in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.685391 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.527255 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.523975 | `eventgrid_topic_list` | ❌ |
| 4 | 0.518668 | `search_service_list` | ❌ |
| 5 | 0.502064 | `resourcehealth_availability-status_list` | ❌ |

---

## Test 375
=======
=======
| 1 | 0.685391 | `resourcehealth_service-health-events_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.527905 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.524063 | `eventgrid_topic_list` | ❌ |
| 4 | 0.520197 | `search_service_list` | ❌ |
| 5 | 0.502345 | `resourcehealth_availability-status_list` | ❌ |

---

<<<<<<< HEAD
## Test 365
=======
## Test 375
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `resourcehealth_health-events_list`  
**Prompt:** Show me planned maintenance events for my Azure services  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.565851 | `resourcehealth_health-events_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.436322 | `search_service_list` | ❌ |
| 3 | 0.404191 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.402493 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.398050 | `quota_usage_check` | ❌ |

---

## Test 376
=======
=======
| 1 | 0.565851 | `resourcehealth_service-health-events_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.437868 | `search_service_list` | ❌ |
| 3 | 0.403665 | `eventgrid_subscription_list` | ❌ |
| 4 | 0.402532 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.398084 | `quota_usage_check` | ❌ |

---

<<<<<<< HEAD
## Test 366
=======
## Test 376
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `servicebus_queue_details`  
**Prompt:** Show me the details of service bus <service_bus_name> queue <queue_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642876 | `servicebus_queue_details` | ✅ **EXPECTED** |
| 2 | 0.460932 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.437000 | `servicebus_topic_details` | ❌ |
| 4 | 0.385812 | `search_knowledge_base_get` | ❌ |
<<<<<<< HEAD
| 5 | 0.384139 | `storage_account_get` | ❌ |

---

## Test 377
=======
<<<<<<< HEAD
| 5 | 0.384133 | `storage_account_get` | ❌ |

---

## Test 367
=======
| 5 | 0.384187 | `storage_account_get` | ❌ |

---

## Test 377
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `servicebus_topic_details`  
**Prompt:** Show me the details of service bus <service_bus_name> topic <topic_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.642762 | `servicebus_topic_details` | ✅ **EXPECTED** |
| 2 | 0.571860 | `servicebus_topic_subscription_details` | ❌ |
| 3 | 0.483976 | `servicebus_queue_details` | ❌ |
| 4 | 0.482735 | `eventgrid_topic_list` | ❌ |
| 5 | 0.457603 | `eventgrid_subscription_list` | ❌ |

---

<<<<<<< HEAD
## Test 378
=======
<<<<<<< HEAD
## Test 368
=======
## Test 378
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `servicebus_topic_subscription_details`  
**Prompt:** Show me the details of service bus <service_bus_name> subscription <subscription_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.633187 | `servicebus_topic_subscription_details` | ✅ **EXPECTED** |
| 2 | 0.517516 | `servicebus_topic_details` | ❌ |
| 3 | 0.494515 | `servicebus_queue_details` | ❌ |
| 4 | 0.493776 | `eventgrid_topic_list` | ❌ |
| 5 | 0.471876 | `eventgrid_subscription_list` | ❌ |

---

<<<<<<< HEAD
## Test 379
=======
<<<<<<< HEAD
## Test 369
=======
## Test 379
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the details of SignalR <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.532742 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.355028 | `redis_list` | ❌ |
| 3 | 0.329804 | `foundry_resource_get` | ❌ |
| 4 | 0.319981 | `sql_server_show` | ❌ |
| 5 | 0.304420 | `servicebus_queue_details` | ❌ |

---

<<<<<<< HEAD
## Test 380
=======
<<<<<<< HEAD
## Test 370
=======
## Test 380
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show me the network information of SignalR runtime <signalr_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.573540 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.337342 | `sql_server_show` | ❌ |
| 3 | 0.306559 | `foundry_resource_get` | ❌ |
| 4 | 0.305021 | `redis_list` | ❌ |
| 5 | 0.301114 | `servicebus_topic_details` | ❌ |

---

<<<<<<< HEAD
## Test 381
=======
<<<<<<< HEAD
## Test 371
=======
## Test 381
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Describe the SignalR runtime <signalr_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.710281 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.411396 | `loadtesting_testresource_list` | ❌ |
| 3 | 0.410606 | `foundry_resource_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.399412 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.382028 | `sql_server_list` | ❌ |

---

## Test 382
=======
<<<<<<< HEAD
| 4 | 0.399745 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.382472 | `sql_server_list` | ❌ |

---

## Test 372
=======
| 4 | 0.399412 | `resourcehealth_availability-status_list` | ❌ |
| 5 | 0.382152 | `sql_server_list` | ❌ |

---

## Test 382
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Get information about my SignalR runtime <signalr_name> in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.715701 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.458894 | `foundry_resource_get` | ❌ |
| 3 | 0.431212 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.430721 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.417313 | `functionapp_get` | ❌ |

---

## Test 383
=======
<<<<<<< HEAD
| 1 | 0.715913 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.459979 | `foundry_resource_get` | ❌ |
| 3 | 0.431800 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.431393 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.417497 | `functionapp_get` | ❌ |

---

## Test 373
=======
| 1 | 0.715937 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.459543 | `foundry_resource_get` | ❌ |
| 3 | 0.431534 | `resourcehealth_availability-status_list` | ❌ |
| 4 | 0.430926 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.417653 | `functionapp_get` | ❌ |

---

## Test 383
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** Show all the SignalRs information in <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563883 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.501077 | `redis_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.494478 | `resourcehealth_availability-status_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.494808 | `resourcehealth_availability-status_list` | ❌ |
=======
| 3 | 0.494478 | `resourcehealth_availability-status_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.481428 | `loadtesting_testresource_list` | ❌ |
| 5 | 0.462090 | `mysql_server_list` | ❌ |

---

<<<<<<< HEAD
## Test 384
=======
<<<<<<< HEAD
## Test 374
=======
## Test 384
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `signalr_runtime_get`  
**Prompt:** List all SignalRs in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.530514 | `signalr_runtime_get` | ✅ **EXPECTED** |
| 2 | 0.507654 | `postgres_server_list` | ❌ |
| 3 | 0.495157 | `redis_list` | ❌ |
<<<<<<< HEAD
| 4 | 0.494498 | `kusto_cluster_list` | ❌ |
| 5 | 0.487906 | `subscription_list` | ❌ |

---

<<<<<<< HEAD
## Test 385
=======
## Test 375
=======
| 4 | 0.494513 | `kusto_cluster_list` | ❌ |
| 5 | 0.487856 | `subscription_list` | ❌ |

---

## Test 385
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new SQL database named <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.516780 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.470892 | `sql_server_create` | ❌ |
| 3 | 0.420389 | `sql_db_rename` | ❌ |
| 4 | 0.408515 | `sql_db_delete` | ❌ |
| 5 | 0.404860 | `sql_server_delete` | ❌ |

---

<<<<<<< HEAD
## Test 386
=======
<<<<<<< HEAD
## Test 376
=======
## Test 386
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a SQL database <database_name> with Basic tier in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.571760 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.459672 | `sql_server_create` | ❌ |
<<<<<<< HEAD
| 3 | 0.437525 | `sql_server_delete` | ❌ |
=======
| 3 | 0.437526 | `sql_server_delete` | ❌ |
<<<<<<< HEAD
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.420843 | `sql_db_show` | ❌ |
| 5 | 0.417661 | `sql_db_delete` | ❌ |

---

<<<<<<< HEAD
## Test 387
=======
## Test 377
=======
| 4 | 0.424021 | `appservice_database_add` | ❌ |
| 5 | 0.420843 | `sql_db_show` | ❌ |

---

## Test 387
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_create`  
**Prompt:** Create a new database called <database_name> on SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.604472 | `sql_db_create` | ✅ **EXPECTED** |
| 2 | 0.545906 | `sql_server_create` | ❌ |
| 3 | 0.503938 | `sql_db_rename` | ❌ |
| 4 | 0.494377 | `sql_db_show` | ❌ |
<<<<<<< HEAD
| 5 | 0.473975 | `sql_db_list` | ❌ |

---

## Test 388
=======
<<<<<<< HEAD
| 5 | 0.473859 | `sql_db_list` | ❌ |

---

## Test 378
=======
| 5 | 0.473975 | `sql_db_list` | ❌ |

---

## Test 388
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the SQL database <database_name> from server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.568196 | `sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.567412 | `sql_server_delete` | ❌ |
| 3 | 0.391436 | `sql_db_rename` | ❌ |
| 4 | 0.386721 | `sql_server_firewall-rule_delete` | ❌ |
| 5 | 0.364776 | `sql_db_show` | ❌ |

---

<<<<<<< HEAD
## Test 389
=======
<<<<<<< HEAD
## Test 379
=======
## Test 389
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_delete`  
**Prompt:** Remove database <database_name> from SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.567513 | `sql_server_delete` | ❌ |
| 2 | 0.543440 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.500756 | `sql_db_show` | ❌ |
| 4 | 0.481023 | `sql_db_rename` | ❌ |
| 5 | 0.478729 | `sql_db_list` | ❌ |

---

## Test 390
=======
<<<<<<< HEAD
| 1 | 0.567481 | `sql_server_delete` | ❌ |
| 2 | 0.543378 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.500746 | `sql_db_show` | ❌ |
| 4 | 0.480981 | `sql_db_rename` | ❌ |
| 5 | 0.478583 | `sql_db_list` | ❌ |

---

## Test 380
=======
| 1 | 0.567513 | `sql_server_delete` | ❌ |
| 2 | 0.543440 | `sql_db_delete` | ✅ **EXPECTED** |
| 3 | 0.500756 | `sql_db_show` | ❌ |
| 4 | 0.481083 | `sql_db_rename` | ❌ |
| 5 | 0.478729 | `sql_db_list` | ❌ |

---

## Test 390
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_delete`  
**Prompt:** Delete the database called <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.509916 | `sql_db_delete` | ✅ **EXPECTED** |
| 2 | 0.490893 | `sql_server_delete` | ❌ |
| 3 | 0.364494 | `postgres_database_list` | ❌ |
| 4 | 0.355416 | `mysql_database_list` | ❌ |
| 5 | 0.347703 | `sql_db_rename` | ❌ |

---

<<<<<<< HEAD
## Test 391
=======
<<<<<<< HEAD
## Test 381
=======
## Test 391
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_list`  
**Prompt:** List all databases in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.643138 | `sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639644 | `mysql_database_list` | ❌ |
| 3 | 0.609116 | `postgres_database_list` | ❌ |
| 4 | 0.602872 | `cosmos_database_list` | ❌ |
| 5 | 0.569464 | `kusto_database_list` | ❌ |

---

## Test 392
=======
<<<<<<< HEAD
| 1 | 0.643202 | `sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639694 | `mysql_database_list` | ❌ |
| 3 | 0.609178 | `postgres_database_list` | ❌ |
| 4 | 0.602890 | `cosmos_database_list` | ❌ |
| 5 | 0.570103 | `kusto_database_list` | ❌ |

---

## Test 382
=======
| 1 | 0.643186 | `sql_db_list` | ✅ **EXPECTED** |
| 2 | 0.639694 | `mysql_database_list` | ❌ |
| 3 | 0.609178 | `postgres_database_list` | ❌ |
| 4 | 0.602890 | `cosmos_database_list` | ❌ |
| 5 | 0.569739 | `kusto_database_list` | ❌ |

---

## Test 392
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_list`  
**Prompt:** Show me all the databases configuration details in the Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.617746 | `sql_server_show` | ❌ |
<<<<<<< HEAD
| 2 | 0.609322 | `sql_db_list` | ✅ **EXPECTED** |
=======
<<<<<<< HEAD
| 2 | 0.609291 | `sql_db_list` | ✅ **EXPECTED** |
=======
| 2 | 0.609322 | `sql_db_list` | ✅ **EXPECTED** |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.557353 | `mysql_database_list` | ❌ |
| 4 | 0.553488 | `mysql_server_config_get` | ❌ |
| 5 | 0.524274 | `sql_db_show` | ❌ |

---

<<<<<<< HEAD
## Test 393
=======
<<<<<<< HEAD
## Test 383
=======
## Test 393
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename the SQL database <database_name> on server <server_name> to <new_database_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.593251 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.425282 | `sql_server_delete` | ❌ |
| 3 | 0.416207 | `sql_db_delete` | ❌ |
| 4 | 0.396947 | `sql_db_create` | ❌ |
| 5 | 0.346018 | `sql_db_show` | ❌ |

---

## Test 394
=======
<<<<<<< HEAD
| 1 | 0.593308 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.425296 | `sql_server_delete` | ❌ |
| 3 | 0.416187 | `sql_db_delete` | ❌ |
| 4 | 0.396109 | `sql_db_create` | ❌ |
| 5 | 0.345991 | `sql_db_show` | ❌ |

---

## Test 384
=======
| 1 | 0.593348 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.425282 | `sql_server_delete` | ❌ |
| 3 | 0.416207 | `sql_db_delete` | ❌ |
| 4 | 0.396947 | `sql_db_create` | ❌ |
| 5 | 0.346018 | `sql_db_show` | ❌ |

---

## Test 394
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_rename`  
**Prompt:** Rename my Azure SQL database <database_name> to <new_database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.711257 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.516770 | `sql_server_delete` | ❌ |
| 3 | 0.506834 | `sql_db_delete` | ❌ |
| 4 | 0.501963 | `sql_db_create` | ❌ |
| 5 | 0.434094 | `sql_server_show` | ❌ |

---

## Test 395
=======
<<<<<<< HEAD
| 1 | 0.710788 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.516432 | `sql_server_delete` | ❌ |
| 3 | 0.506388 | `sql_db_delete` | ❌ |
| 4 | 0.500926 | `sql_db_create` | ❌ |
| 5 | 0.434133 | `sql_server_show` | ❌ |

---

## Test 385
=======
| 1 | 0.710925 | `sql_db_rename` | ✅ **EXPECTED** |
| 2 | 0.516662 | `sql_server_delete` | ❌ |
| 3 | 0.506572 | `sql_db_delete` | ❌ |
| 4 | 0.501347 | `sql_db_create` | ❌ |
| 5 | 0.433966 | `sql_server_show` | ❌ |

---

## Test 395
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_show`  
**Prompt:** Get the configuration details for the SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.610991 | `sql_server_show` | ❌ |
| 2 | 0.593150 | `postgres_server_config_get` | ❌ |
| 3 | 0.530422 | `mysql_server_config_get` | ❌ |
| 4 | 0.528136 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465693 | `sql_db_list` | ❌ |

---

## Test 396
=======
<<<<<<< HEAD
| 1 | 0.611215 | `sql_server_show` | ❌ |
| 2 | 0.593200 | `postgres_server_config_get` | ❌ |
| 3 | 0.530520 | `mysql_server_config_get` | ❌ |
| 4 | 0.528378 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465779 | `sql_db_list` | ❌ |

---

## Test 386
=======
| 1 | 0.610991 | `sql_server_show` | ❌ |
| 2 | 0.593150 | `postgres_server_config_get` | ❌ |
| 3 | 0.530422 | `mysql_server_config_get` | ❌ |
| 4 | 0.528136 | `sql_db_show` | ✅ **EXPECTED** |
| 5 | 0.465693 | `sql_db_list` | ❌ |

---

## Test 396
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_show`  
**Prompt:** Show me the details of SQL database <database_name> in server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.530095 | `sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503681 | `sql_server_show` | ❌ |
| 3 | 0.440073 | `sql_db_list` | ❌ |
| 4 | 0.439076 | `mysql_table_schema_get` | ❌ |
| 5 | 0.432919 | `mysql_database_list` | ❌ |

---

## Test 397
=======
<<<<<<< HEAD
| 1 | 0.530071 | `sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503602 | `sql_server_show` | ❌ |
| 3 | 0.439895 | `sql_db_list` | ❌ |
| 4 | 0.438615 | `mysql_table_schema_get` | ❌ |
| 5 | 0.432907 | `mysql_database_list` | ❌ |

---

## Test 387
=======
| 1 | 0.530095 | `sql_db_show` | ✅ **EXPECTED** |
| 2 | 0.503681 | `sql_server_show` | ❌ |
| 3 | 0.440073 | `sql_db_list` | ❌ |
| 4 | 0.438622 | `mysql_table_schema_get` | ❌ |
| 5 | 0.432919 | `mysql_database_list` | ❌ |

---

## Test 397
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_update`  
**Prompt:** Update the performance tier of SQL database <database_name> on server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.603271 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467571 | `sql_db_create` | ❌ |
| 3 | 0.440442 | `sql_db_rename` | ❌ |
| 4 | 0.427621 | `sql_db_show` | ❌ |
| 5 | 0.413941 | `sql_server_delete` | ❌ |

---

## Test 398
=======
<<<<<<< HEAD
| 1 | 0.603537 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467332 | `sql_db_create` | ❌ |
| 3 | 0.440688 | `sql_db_rename` | ❌ |
| 4 | 0.427542 | `sql_db_show` | ❌ |
| 5 | 0.414267 | `sql_server_delete` | ❌ |

---

## Test 388
=======
| 1 | 0.603360 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.467590 | `sql_db_create` | ❌ |
| 3 | 0.440550 | `sql_db_rename` | ❌ |
| 4 | 0.427654 | `sql_db_show` | ❌ |
| 5 | 0.414041 | `sql_server_delete` | ❌ |

---

## Test 398
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_db_update`  
**Prompt:** Scale SQL database <database_name> on server <server_name> to use <sku_name> SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.550449 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.418358 | `sql_server_delete` | ❌ |
| 3 | 0.401817 | `sql_db_list` | ❌ |
| 4 | 0.395508 | `sql_db_rename` | ❌ |
| 5 | 0.394770 | `sql_db_show` | ❌ |

---

## Test 399
=======
<<<<<<< HEAD
| 1 | 0.550501 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.418334 | `sql_server_delete` | ❌ |
| 3 | 0.401717 | `sql_db_list` | ❌ |
| 4 | 0.395462 | `sql_db_rename` | ❌ |
| 5 | 0.394705 | `sql_db_show` | ❌ |

---

## Test 389
=======
| 1 | 0.550556 | `sql_db_update` | ✅ **EXPECTED** |
| 2 | 0.418358 | `sql_server_delete` | ❌ |
| 3 | 0.401817 | `sql_db_list` | ❌ |
| 4 | 0.395518 | `sql_db_rename` | ❌ |
| 5 | 0.394770 | `sql_db_show` | ❌ |

---

## Test 399
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** List all elastic pools in SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.678124 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.502376 | `sql_db_list` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.502382 | `sql_db_list` | ❌ |
=======
| 2 | 0.502376 | `sql_db_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.498367 | `mysql_database_list` | ❌ |
| 4 | 0.485249 | `aks_nodepool_get` | ❌ |
| 5 | 0.479044 | `sql_server_show` | ❌ |

---

<<<<<<< HEAD
## Test 400
=======
<<<<<<< HEAD
## Test 390
=======
## Test 400
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** Show me the elastic pools configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.606425 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `sql_server_show` | ❌ |
| 3 | 0.457164 | `sql_db_list` | ❌ |
| 4 | 0.450743 | `aks_nodepool_get` | ❌ |
| 5 | 0.432816 | `mysql_database_list` | ❌ |

---

## Test 401
=======
<<<<<<< HEAD
| 1 | 0.606478 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502977 | `sql_server_show` | ❌ |
| 3 | 0.457262 | `sql_db_list` | ❌ |
| 4 | 0.450790 | `aks_nodepool_get` | ❌ |
| 5 | 0.432867 | `mysql_database_list` | ❌ |

---

## Test 391
=======
| 1 | 0.606425 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.502877 | `sql_server_show` | ❌ |
| 3 | 0.457163 | `sql_db_list` | ❌ |
| 4 | 0.450743 | `aks_nodepool_get` | ❌ |
| 5 | 0.432816 | `mysql_database_list` | ❌ |

---

## Test 401
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_elastic-pool_list`  
**Prompt:** What elastic pools are available in my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.592709 | `sql_elastic-pool_list` | ✅ **EXPECTED** |
| 2 | 0.420325 | `mysql_database_list` | ❌ |
| 3 | 0.407169 | `aks_nodepool_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.402616 | `mysql_server_list` | ❌ |
| 5 | 0.397670 | `sql_db_list` | ❌ |

---

## Test 402
=======
<<<<<<< HEAD
| 4 | 0.402602 | `mysql_server_list` | ❌ |
| 5 | 0.397708 | `sql_db_list` | ❌ |

---

## Test 392
=======
| 4 | 0.402616 | `mysql_server_list` | ❌ |
| 5 | 0.397670 | `sql_db_list` | ❌ |

---

## Test 402
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_create`  
**Prompt:** Create a new Azure SQL server named <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.682605 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563707 | `sql_db_create` | ❌ |
| 3 | 0.529198 | `sql_server_list` | ❌ |
| 4 | 0.482102 | `storage_account_create` | ❌ |
| 5 | 0.474180 | `sql_db_rename` | ❌ |

---

## Test 403
=======
<<<<<<< HEAD
| 1 | 0.682198 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563307 | `sql_db_create` | ❌ |
| 3 | 0.529314 | `sql_server_list` | ❌ |
| 4 | 0.481645 | `storage_account_create` | ❌ |
| 5 | 0.473844 | `sql_db_rename` | ❌ |

---

## Test 393
=======
| 1 | 0.682812 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.563994 | `sql_db_create` | ❌ |
| 3 | 0.529755 | `sql_server_list` | ❌ |
| 4 | 0.482437 | `storage_account_create` | ❌ |
| 5 | 0.474643 | `sql_db_rename` | ❌ |

---

## Test 403
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_create`  
**Prompt:** Create an Azure SQL server with name <server_name> in location <location> with admin user <admin_user>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.618354 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510222 | `sql_db_create` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.618244 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510507 | `sql_db_create` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.472462 | `sql_server_show` | ❌ |
| 4 | 0.441267 | `sql_server_delete` | ❌ |
| 5 | 0.400941 | `sql_db_rename` | ❌ |

---

<<<<<<< HEAD
## Test 404
=======
## Test 394
=======
| 1 | 0.618309 | `sql_server_create` | ✅ **EXPECTED** |
| 2 | 0.510169 | `sql_db_create` | ❌ |
| 3 | 0.472463 | `sql_server_show` | ❌ |
| 4 | 0.441174 | `sql_server_delete` | ❌ |
| 5 | 0.400939 | `sql_db_rename` | ❌ |

---

## Test 404
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_create`  
**Prompt:** Set up a new SQL server called <server_name> in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589818 | `sql_server_create` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.501403 | `sql_db_create` | ❌ |
| 3 | 0.497890 | `sql_server_list` | ❌ |
| 4 | 0.461147 | `sql_db_rename` | ❌ |
| 5 | 0.442934 | `mysql_server_list` | ❌ |

---

## Test 405
=======
<<<<<<< HEAD
| 2 | 0.500874 | `sql_db_create` | ❌ |
| 3 | 0.498255 | `sql_server_list` | ❌ |
| 4 | 0.461181 | `sql_db_rename` | ❌ |
| 5 | 0.442984 | `mysql_server_list` | ❌ |

---

## Test 395
=======
| 2 | 0.501403 | `sql_db_create` | ❌ |
| 3 | 0.498298 | `sql_server_list` | ❌ |
| 4 | 0.461181 | `sql_db_rename` | ❌ |
| 5 | 0.442934 | `mysql_server_list` | ❌ |

---

## Test 405
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete the Azure SQL server <server_name> from resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.656593 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.548064 | `sql_db_delete` | ❌ |
<<<<<<< HEAD
| 3 | 0.518037 | `sql_server_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.518306 | `sql_server_list` | ❌ |
=======
| 3 | 0.518201 | `sql_server_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.495550 | `sql_server_create` | ❌ |
| 5 | 0.483132 | `workbooks_delete` | ❌ |

---

<<<<<<< HEAD
## Test 406
=======
<<<<<<< HEAD
## Test 396
=======
## Test 406
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_delete`  
**Prompt:** Remove the SQL server <server_name> from my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.615073 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.393885 | `postgres_server_list` | ❌ |
| 3 | 0.379760 | `sql_db_delete` | ❌ |
| 4 | 0.376660 | `sql_server_show` | ❌ |
<<<<<<< HEAD
| 5 | 0.350103 | `sql_server_list` | ❌ |

---

## Test 407
=======
<<<<<<< HEAD
| 5 | 0.350384 | `sql_server_list` | ❌ |

---

## Test 397
=======
| 5 | 0.350173 | `sql_server_list` | ❌ |

---

## Test 407
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_delete`  
**Prompt:** Delete SQL server <server_name> permanently  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.624310 | `sql_server_delete` | ✅ **EXPECTED** |
| 2 | 0.454892 | `sql_db_delete` | ❌ |
| 3 | 0.362561 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.341503 | `sql_server_show` | ❌ |
<<<<<<< HEAD
| 5 | 0.318758 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 408
=======
<<<<<<< HEAD
| 5 | 0.319013 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 398
=======
| 5 | 0.318758 | `eventhubs_eventhub_delete` | ❌ |

---

## Test 408
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** List Microsoft Entra ID administrators for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.783479 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.456051 | `sql_server_show` | ❌ |
<<<<<<< HEAD
| 3 | 0.434868 | `sql_server_list` | ❌ |
| 4 | 0.401854 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376055 | `sql_db_list` | ❌ |

---

## Test 409
=======
<<<<<<< HEAD
| 3 | 0.434565 | `sql_server_list` | ❌ |
| 4 | 0.401908 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.375977 | `sql_db_list` | ❌ |

---

## Test 399
=======
| 3 | 0.434776 | `sql_server_list` | ❌ |
| 4 | 0.401880 | `sql_server_firewall-rule_list` | ❌ |
| 5 | 0.376055 | `sql_db_list` | ❌ |

---

## Test 409
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** Show me the Entra ID administrators configured for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713306 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.413144 | `sql_server_show` | ❌ |
<<<<<<< HEAD
| 3 | 0.368082 | `sql_server_list` | ❌ |
| 4 | 0.315966 | `sql_db_list` | ❌ |
| 5 | 0.311085 | `postgres_server_list` | ❌ |

---

## Test 410
=======
<<<<<<< HEAD
| 3 | 0.367692 | `sql_server_list` | ❌ |
| 4 | 0.315939 | `sql_db_list` | ❌ |
| 5 | 0.311071 | `postgres_server_list` | ❌ |

---

## Test 400
=======
| 3 | 0.368018 | `sql_server_list` | ❌ |
| 4 | 0.315966 | `sql_db_list` | ❌ |
| 5 | 0.311085 | `postgres_server_list` | ❌ |

---

## Test 410
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_entra-admin_list`  
**Prompt:** What Microsoft Entra ID administrators are set up for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.646419 | `sql_server_entra-admin_list` | ✅ **EXPECTED** |
| 2 | 0.356025 | `sql_server_show` | ❌ |
<<<<<<< HEAD
| 3 | 0.322155 | `sql_server_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.322084 | `sql_server_list` | ❌ |
=======
| 3 | 0.322362 | `sql_server_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.307823 | `sql_server_create` | ❌ |
| 5 | 0.269788 | `sql_server_delete` | ❌ |

---

<<<<<<< HEAD
## Test 411
=======
<<<<<<< HEAD
## Test 401
=======
## Test 411
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a firewall rule for my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.635467 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.532658 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.522133 | `sql_server_firewall-rule_delete` | ❌ |
=======
| 1 | 0.635466 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.532712 | `sql_server_firewall-rule_list` | ❌ |
=======
| 2 | 0.532682 | `sql_server_firewall-rule_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.522184 | `sql_server_firewall-rule_delete` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.448822 | `sql_server_create` | ❌ |
| 5 | 0.440845 | `sql_server_delete` | ❌ |

---

<<<<<<< HEAD
## Test 412
=======
<<<<<<< HEAD
## Test 402
=======
## Test 412
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Add a firewall rule to allow access from IP range <start_ip> to <end_ip> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.670392 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533587 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503740 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316700 | `sql_server_list` | ❌ |
| 5 | 0.302273 | `sql_server_delete` | ❌ |

---

## Test 413
=======
<<<<<<< HEAD
| 1 | 0.670233 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533669 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503500 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316954 | `sql_server_list` | ❌ |
| 5 | 0.302510 | `sql_server_delete` | ❌ |

---

## Test 403
=======
| 1 | 0.670189 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.533532 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.503648 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.316667 | `sql_server_list` | ❌ |
| 5 | 0.302362 | `sql_server_delete` | ❌ |

---

## Test 413
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_firewall-rule_create`  
**Prompt:** Create a new firewall rule named <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.685125 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
| 2 | 0.574393 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539643 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428987 | `sql_server_create` | ❌ |
| 5 | 0.395244 | `sql_db_create` | ❌ |

---

## Test 414
=======
| 1 | 0.685107 | `sql_server_firewall-rule_create` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.574336 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539577 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428919 | `sql_server_create` | ❌ |
| 5 | 0.394446 | `sql_db_create` | ❌ |

---

## Test 404
=======
| 2 | 0.574310 | `sql_server_firewall-rule_list` | ❌ |
| 3 | 0.539577 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.428919 | `sql_server_create` | ❌ |
| 5 | 0.395165 | `sql_db_create` | ❌ |

---

## Test 414
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Delete a firewall rule from my Azure SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.691498 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.584379 | `sql_server_delete` | ❌ |
<<<<<<< HEAD
| 3 | 0.543780 | `sql_server_firewall-rule_list` | ❌ |
=======
| 3 | 0.543839 | `sql_server_firewall-rule_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.540333 | `sql_server_firewall-rule_create` | ❌ |
| 5 | 0.498444 | `sql_db_delete` | ❌ |

---

<<<<<<< HEAD
## Test 415
=======
<<<<<<< HEAD
## Test 405
=======
## Test 415
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Remove the firewall rule <rule_name> from SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.670233 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574296 | `sql_server_firewall-rule_list` | ❌ |
=======
| 1 | 0.670179 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.574321 | `sql_server_firewall-rule_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.530419 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.488418 | `sql_server_delete` | ❌ |
| 5 | 0.360381 | `sql_db_delete` | ❌ |

---

<<<<<<< HEAD
## Test 416
=======
<<<<<<< HEAD
## Test 406
=======
## Test 416
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_firewall-rule_delete`  
**Prompt:** Delete firewall rule <rule_name> for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.671298 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601174 | `sql_server_firewall-rule_list` | ❌ |
=======
| 1 | 0.671212 | `sql_server_firewall-rule_delete` | ✅ **EXPECTED** |
| 2 | 0.601217 | `sql_server_firewall-rule_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.577330 | `sql_server_firewall-rule_create` | ❌ |
| 4 | 0.499272 | `sql_server_delete` | ❌ |
| 5 | 0.378586 | `sql_db_delete` | ❌ |

---

<<<<<<< HEAD
## Test 417
=======
<<<<<<< HEAD
## Test 407
=======
## Test 417
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** List all firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.729336 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
=======
| 1 | 0.729320 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.549667 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.513187 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.468812 | `sql_server_show` | ❌ |
<<<<<<< HEAD
| 5 | 0.418817 | `sql_server_list` | ❌ |

---

## Test 418
=======
<<<<<<< HEAD
| 5 | 0.418869 | `sql_server_list` | ❌ |

---

## Test 408
=======
| 5 | 0.418738 | `sql_server_list` | ❌ |

---

## Test 418
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** Show me the firewall rules for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.630671 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
| 2 | 0.524126 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.476792 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.410680 | `sql_server_show` | ❌ |
<<<<<<< HEAD
| 5 | 0.348100 | `sql_server_list` | ❌ |

---

## Test 419
=======
<<<<<<< HEAD
| 5 | 0.348249 | `sql_server_list` | ❌ |

---

## Test 409
=======
| 5 | 0.348096 | `sql_server_list` | ❌ |

---

## Test 419
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_firewall-rule_list`  
**Prompt:** What firewall rules are configured for my SQL server <server_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.630460 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
=======
| 1 | 0.630494 | `sql_server_firewall-rule_list` | ✅ **EXPECTED** |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 2 | 0.532454 | `sql_server_firewall-rule_create` | ❌ |
| 3 | 0.473596 | `sql_server_firewall-rule_delete` | ❌ |
| 4 | 0.412957 | `sql_server_show` | ❌ |
<<<<<<< HEAD
| 5 | 0.350513 | `sql_server_list` | ❌ |

---

## Test 420
=======
<<<<<<< HEAD
| 5 | 0.350545 | `sql_server_list` | ❌ |

---

## Test 410
=======
| 5 | 0.350474 | `sql_server_list` | ❌ |

---

## Test 420
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_list`  
**Prompt:** List all Azure SQL servers in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.694404 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596686 | `mysql_server_list` | ❌ |
| 3 | 0.578238 | `sql_db_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.694268 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596720 | `mysql_server_list` | ❌ |
| 3 | 0.578135 | `sql_db_list` | ❌ |
=======
| 1 | 0.694306 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.596686 | `mysql_server_list` | ❌ |
| 3 | 0.578239 | `sql_db_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.515851 | `sql_elastic-pool_list` | ❌ |
| 5 | 0.509789 | `sql_db_show` | ❌ |

---

<<<<<<< HEAD
## Test 421
=======
<<<<<<< HEAD
## Test 411
=======
## Test 421
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_list`  
**Prompt:** Show me every SQL server available in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.618218 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593837 | `mysql_server_list` | ❌ |
| 3 | 0.542398 | `sql_db_list` | ❌ |
| 4 | 0.507404 | `resourcehealth_availability-status_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.618206 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593874 | `mysql_server_list` | ❌ |
| 3 | 0.542307 | `sql_db_list` | ❌ |
| 4 | 0.507683 | `resourcehealth_availability-status_list` | ❌ |
=======
| 1 | 0.618222 | `sql_server_list` | ✅ **EXPECTED** |
| 2 | 0.593837 | `mysql_server_list` | ❌ |
| 3 | 0.542398 | `sql_db_list` | ❌ |
| 4 | 0.507404 | `resourcehealth_availability-status_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.496200 | `group_list` | ❌ |

---

<<<<<<< HEAD
## Test 422
=======
<<<<<<< HEAD
## Test 412
=======
## Test 422
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_show`  
**Prompt:** Show me the details of Azure SQL server <server_name> in resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.629672 | `sql_db_show` | ❌ |
| 2 | 0.595184 | `sql_server_show` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 3 | 0.587728 | `sql_server_list` | ❌ |
| 4 | 0.559893 | `mysql_server_list` | ❌ |
| 5 | 0.540218 | `sql_db_list` | ❌ |

---

## Test 423
=======
<<<<<<< HEAD
| 3 | 0.587826 | `sql_server_list` | ❌ |
| 4 | 0.559936 | `mysql_server_list` | ❌ |
| 5 | 0.540037 | `sql_db_list` | ❌ |

---

## Test 413
=======
| 3 | 0.587806 | `sql_server_list` | ❌ |
| 4 | 0.559893 | `mysql_server_list` | ❌ |
| 5 | 0.540218 | `sql_db_list` | ❌ |

---

## Test 423
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_show`  
**Prompt:** Get the configuration details for SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.658817 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.610507 | `postgres_server_config_get` | ❌ |
| 3 | 0.538034 | `mysql_server_config_get` | ❌ |
| 4 | 0.471541 | `sql_db_show` | ❌ |
| 5 | 0.445432 | `postgres_server_param_get` | ❌ |

---

<<<<<<< HEAD
## Test 424
=======
<<<<<<< HEAD
## Test 414
=======
## Test 424
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `sql_server_show`  
**Prompt:** Display the properties of SQL server <server_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.563143 | `sql_server_show` | ✅ **EXPECTED** |
| 2 | 0.392532 | `postgres_server_config_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.380035 | `postgres_server_param_get` | ❌ |
| 4 | 0.372102 | `sql_server_firewall-rule_list` | ❌ |
=======
| 3 | 0.380021 | `postgres_server_param_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.372194 | `sql_server_firewall-rule_list` | ❌ |
=======
| 4 | 0.372172 | `sql_server_firewall-rule_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.370539 | `sql_db_show` | ❌ |

---

<<<<<<< HEAD
## Test 425
=======
<<<<<<< HEAD
## Test 415
=======
## Test 425
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account called testaccount123 in East US region  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.533552 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.438046 | `storage_blob_container_create` | ❌ |
<<<<<<< HEAD
| 3 | 0.418191 | `storage_account_get` | ❌ |
| 4 | 0.413950 | `storage_blob_container_get` | ❌ |
| 5 | 0.373651 | `managedlustre_fs_create` | ❌ |

---

## Test 426
=======
<<<<<<< HEAD
| 3 | 0.418002 | `storage_account_get` | ❌ |
=======
| 3 | 0.418134 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.414518 | `storage_blob_container_get` | ❌ |
| 5 | 0.370957 | `managedlustre_fs_create` | ❌ |

---

<<<<<<< HEAD
## Test 416
=======
## Test 426
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a storage account with premium performance and LRS replication  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.500638 | `storage_account_create` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.484584 | `managedlustre_fs_create` | ❌ |
| 3 | 0.407222 | `storage_account_get` | ❌ |
=======
<<<<<<< HEAD
| 2 | 0.483202 | `managedlustre_fs_create` | ❌ |
| 3 | 0.407182 | `storage_account_get` | ❌ |
=======
| 2 | 0.483202 | `managedlustre_filesystem_create` | ❌ |
| 3 | 0.407200 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.406804 | `storage_blob_container_create` | ❌ |
| 5 | 0.400134 | `managedlustre_fs_sku_get` | ❌ |

---

<<<<<<< HEAD
## Test 427
=======
<<<<<<< HEAD
## Test 417
=======
## Test 427
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_account_create`  
**Prompt:** Create a new storage account with Data Lake Storage Gen2 enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.589002 | `storage_account_create` | ✅ **EXPECTED** |
| 2 | 0.538023 | `managedlustre_fs_create` | ❌ |
| 3 | 0.509731 | `storage_blob_container_create` | ❌ |
<<<<<<< HEAD
| 4 | 0.462519 | `storage_account_get` | ❌ |
| 5 | 0.447156 | `sql_db_create` | ❌ |

---

## Test 428
=======
<<<<<<< HEAD
| 4 | 0.462494 | `storage_account_get` | ❌ |
| 5 | 0.447560 | `sql_db_create` | ❌ |

---

## Test 418
=======
| 4 | 0.462480 | `storage_account_get` | ❌ |
| 5 | 0.447156 | `sql_db_create` | ❌ |

---

## Test 428
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the details for my storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.673750 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.607762 | `storage_blob_container_get` | ❌ |
| 3 | 0.556457 | `storage_blob_get` | ❌ |
| 4 | 0.483435 | `storage_account_create` | ❌ |
| 5 | 0.439236 | `cosmos_account_list` | ❌ |

---

## Test 429
=======
<<<<<<< HEAD
| 1 | 0.673569 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.608073 | `storage_blob_container_get` | ❌ |
| 3 | 0.556407 | `storage_blob_get` | ❌ |
| 4 | 0.483573 | `storage_account_create` | ❌ |
| 5 | 0.439109 | `cosmos_account_list` | ❌ |

---

## Test 419
=======
| 1 | 0.673754 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.608256 | `storage_blob_container_get` | ❌ |
| 3 | 0.556457 | `storage_blob_get` | ❌ |
| 4 | 0.483435 | `storage_account_create` | ❌ |
| 5 | 0.439187 | `cosmos_account_list` | ❌ |

---

## Test 429
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_account_get`  
**Prompt:** Get details about the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.692687 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.577173 | `storage_blob_container_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.692473 | `storage_account_get` | ✅ **EXPECTED** |
=======
| 1 | 0.692698 | `storage_account_get` | ✅ **EXPECTED** |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 2 | 0.577547 | `storage_blob_container_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.529205 | `storage_blob_get` | ❌ |
| 4 | 0.518215 | `storage_account_create` | ❌ |
| 5 | 0.448506 | `storage_blob_container_create` | ❌ |

---

<<<<<<< HEAD
## Test 430
=======
<<<<<<< HEAD
## Test 420
=======
## Test 430
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_account_get`  
**Prompt:** List all storage accounts in my subscription including their location and SKU  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.649215 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557093 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.549448 | `storage_blob_container_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.649393 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557016 | `managedlustre_fs_sku_get` | ❌ |
| 3 | 0.550148 | `storage_blob_container_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.547577 | `subscription_list` | ❌ |
| 5 | 0.536909 | `cosmos_account_list` | ❌ |

---

<<<<<<< HEAD
## Test 431
=======
## Test 421
=======
| 1 | 0.649191 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.557016 | `managedlustre_filesystem_sku_get` | ❌ |
| 3 | 0.550148 | `storage_blob_container_get` | ❌ |
| 4 | 0.547647 | `subscription_list` | ❌ |
| 5 | 0.536912 | `cosmos_account_list` | ❌ |

---

## Test 431
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me my storage accounts with whether hierarchical namespace (HNS) is enabled  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.556860 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.481664 | `storage_blob_container_get` | ❌ |
| 3 | 0.461284 | `managedlustre_fs_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.557064 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.482418 | `storage_blob_container_get` | ❌ |
| 3 | 0.461308 | `managedlustre_fs_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.421642 | `cosmos_account_list` | ❌ |
=======
| 1 | 0.556930 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.482418 | `storage_blob_container_get` | ❌ |
| 3 | 0.461284 | `managedlustre_filesystem_list` | ❌ |
| 4 | 0.421671 | `cosmos_account_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.410587 | `storage_blob_get` | ❌ |

---

<<<<<<< HEAD
## Test 432
=======
<<<<<<< HEAD
## Test 422
=======
## Test 432
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_account_get`  
**Prompt:** Show me the storage accounts in my subscription and include HTTPS-only and public blob access settings  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.619462 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.555677 | `storage_blob_container_get` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.619639 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.556436 | `storage_blob_container_get` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 3 | 0.518229 | `storage_blob_get` | ❌ |
| 4 | 0.473598 | `cosmos_account_list` | ❌ |
| 5 | 0.465527 | `subscription_list` | ❌ |

---

<<<<<<< HEAD
## Test 433
=======
## Test 423
=======
| 1 | 0.619491 | `storage_account_get` | ✅ **EXPECTED** |
| 2 | 0.556436 | `storage_blob_container_get` | ❌ |
| 3 | 0.518229 | `storage_blob_get` | ❌ |
| 4 | 0.473662 | `cosmos_account_list` | ❌ |
| 5 | 0.465571 | `subscription_list` | ❌ |

---

## Test 433
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the storage container mycontainer in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.649793 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.585556 | `storage_blob_container_get` | ❌ |
| 3 | 0.524779 | `storage_account_create` | ❌ |
| 4 | 0.496679 | `storage_blob_get` | ❌ |
| 5 | 0.447784 | `cosmos_database_container_list` | ❌ |

---

<<<<<<< HEAD
## Test 434
=======
<<<<<<< HEAD
## Test 424
=======
## Test 434
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create the container using blob public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.682161 | `storage_blob_container_create` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.590826 | `storage_blob_container_get` | ❌ |
| 3 | 0.559264 | `storage_blob_get` | ❌ |
| 4 | 0.500625 | `storage_account_create` | ❌ |
| 5 | 0.420514 | `storage_account_get` | ❌ |

---

## Test 435
=======
| 2 | 0.590160 | `storage_blob_container_get` | ❌ |
| 3 | 0.559263 | `storage_blob_get` | ❌ |
| 4 | 0.500624 | `storage_account_create` | ❌ |
<<<<<<< HEAD
| 5 | 0.420434 | `storage_account_get` | ❌ |

---

## Test 425
=======
| 5 | 0.420516 | `storage_account_get` | ❌ |

---

## Test 435
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_container_create`  
**Prompt:** Create a new blob container named documents with container public access in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.625397 | `storage_blob_container_create` | ✅ **EXPECTED** |
| 2 | 0.544024 | `storage_blob_container_get` | ❌ |
| 3 | 0.497804 | `storage_blob_get` | ❌ |
| 4 | 0.463198 | `storage_account_create` | ❌ |
| 5 | 0.435099 | `cosmos_database_container_list` | ❌ |

---

<<<<<<< HEAD
## Test 436
=======
<<<<<<< HEAD
## Test 426
=======
## Test 436
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the properties of the storage container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.703348 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.623681 | `storage_blob_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.577921 | `storage_account_get` | ❌ |
| 4 | 0.549804 | `storage_blob_container_create` | ❌ |
| 5 | 0.523289 | `cosmos_database_container_list` | ❌ |

---

## Test 437
=======
<<<<<<< HEAD
| 3 | 0.577740 | `storage_account_get` | ❌ |
=======
| 3 | 0.577904 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.549803 | `storage_blob_container_create` | ❌ |
| 5 | 0.523288 | `cosmos_database_container_list` | ❌ |

---

<<<<<<< HEAD
## Test 427
=======
## Test 437
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** List all blob containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.712012 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.680802 | `storage_blob_get` | ❌ |
| 3 | 0.613933 | `cosmos_database_container_list` | ❌ |
| 4 | 0.556319 | `storage_blob_container_create` | ❌ |
| 5 | 0.518266 | `storage_account_get` | ❌ |

---

<<<<<<< HEAD
## Test 438
=======
<<<<<<< HEAD
## Test 428
=======
## Test 438
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_container_get`  
**Prompt:** Show me the containers in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.713080 | `storage_blob_container_get` | ✅ **EXPECTED** |
| 2 | 0.592373 | `cosmos_database_container_list` | ❌ |
| 3 | 0.586169 | `storage_blob_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.523322 | `storage_account_get` | ❌ |
| 5 | 0.487520 | `storage_blob_container_create` | ❌ |

---

## Test 439
=======
<<<<<<< HEAD
| 4 | 0.523353 | `storage_account_get` | ❌ |
=======
| 4 | 0.523293 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.487521 | `storage_blob_container_create` | ❌ |

---

<<<<<<< HEAD
## Test 429
=======
## Test 439
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the properties for blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.700963 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.648279 | `storage_blob_container_get` | ❌ |
| 3 | 0.540987 | `storage_blob_container_create` | ❌ |
| 4 | 0.527363 | `storage_account_get` | ❌ |
| 5 | 0.477959 | `cosmos_database_container_list` | ❌ |

---

## Test 440
=======
<<<<<<< HEAD
| 1 | 0.700969 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.647029 | `storage_blob_container_get` | ❌ |
| 3 | 0.541060 | `storage_blob_container_create` | ❌ |
| 4 | 0.527327 | `storage_account_get` | ❌ |
| 5 | 0.477993 | `cosmos_database_container_list` | ❌ |

---

## Test 430
=======
| 1 | 0.700973 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.646973 | `storage_blob_container_get` | ❌ |
| 3 | 0.541019 | `storage_blob_container_create` | ❌ |
| 4 | 0.527428 | `storage_account_get` | ❌ |
| 5 | 0.477946 | `cosmos_database_container_list` | ❌ |

---

## Test 440
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_get`  
**Prompt:** Get the details about blob <blob> in the container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.694997 | `storage_blob_get` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.633397 | `storage_blob_container_get` | ❌ |
| 3 | 0.589151 | `storage_blob_container_create` | ❌ |
| 4 | 0.580226 | `storage_account_get` | ❌ |
=======
| 2 | 0.631161 | `storage_blob_container_get` | ❌ |
| 3 | 0.589152 | `storage_blob_container_create` | ❌ |
<<<<<<< HEAD
| 4 | 0.579989 | `storage_account_get` | ❌ |
=======
| 4 | 0.580235 | `storage_account_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.457038 | `storage_account_create` | ❌ |

---

<<<<<<< HEAD
## Test 441
=======
<<<<<<< HEAD
## Test 431
=======
## Test 441
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_get`  
**Prompt:** List all blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.733586 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.702342 | `storage_blob_container_get` | ❌ |
| 3 | 0.605993 | `storage_blob_container_create` | ❌ |
| 4 | 0.579070 | `cosmos_database_container_list` | ❌ |
<<<<<<< HEAD
| 5 | 0.506639 | `cosmos_database_container_item_query` | ❌ |

---

## Test 442
=======
<<<<<<< HEAD
| 5 | 0.506792 | `cosmos_database_container_item_query` | ❌ |

---

## Test 432
=======
| 5 | 0.506639 | `cosmos_database_container_item_query` | ❌ |

---

## Test 442
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_get`  
**Prompt:** Show me the blobs in the blob container <container> in the storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.704426 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.666342 | `storage_blob_container_get` | ❌ |
| 3 | 0.561557 | `storage_blob_container_create` | ❌ |
| 4 | 0.533515 | `cosmos_database_container_list` | ❌ |
| 5 | 0.484018 | `storage_account_get` | ❌ |

---

<<<<<<< HEAD
## Test 443
=======
## Test 433
=======
| 1 | 0.704413 | `storage_blob_get` | ✅ **EXPECTED** |
| 2 | 0.664877 | `storage_blob_container_get` | ❌ |
| 3 | 0.561546 | `storage_blob_container_create` | ❌ |
| 4 | 0.533442 | `cosmos_database_container_list` | ❌ |
| 5 | 0.483914 | `storage_account_get` | ❌ |

---

## Test 443
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `storage_blob_upload`  
**Prompt:** Upload file <local-file-path> to storage blob <blob> in container <container> in storage account <account>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.566278 | `storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.525685 | `storage_blob_container_create` | ❌ |
| 3 | 0.517524 | `storage_blob_get` | ❌ |
| 4 | 0.474395 | `storage_blob_container_get` | ❌ |
| 5 | 0.382007 | `storage_account_create` | ❌ |

---

## Test 444
=======
<<<<<<< HEAD
| 1 | 0.566280 | `storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.525689 | `storage_blob_container_create` | ❌ |
| 3 | 0.517628 | `storage_blob_get` | ❌ |
| 4 | 0.473667 | `storage_blob_container_get` | ❌ |
| 5 | 0.382148 | `storage_account_create` | ❌ |

---

## Test 434
=======
| 1 | 0.566287 | `storage_blob_upload` | ✅ **EXPECTED** |
| 2 | 0.525674 | `storage_blob_container_create` | ❌ |
| 3 | 0.517616 | `storage_blob_get` | ❌ |
| 4 | 0.473645 | `storage_blob_container_get` | ❌ |
| 5 | 0.382123 | `storage_account_create` | ❌ |

---

## Test 444
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `subscription_list`  
**Prompt:** List all subscriptions for my account  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.654048 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512964 | `cosmos_account_list` | ❌ |
| 3 | 0.471653 | `postgres_server_list` | ❌ |
| 4 | 0.469023 | `kusto_cluster_list` | ❌ |
=======
| 1 | 0.654071 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.512954 | `cosmos_account_list` | ❌ |
| 3 | 0.471653 | `postgres_server_list` | ❌ |
| 4 | 0.469085 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.461078 | `redis_list` | ❌ |

---

<<<<<<< HEAD
## Test 445
=======
<<<<<<< HEAD
## Test 435
=======
## Test 445
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `subscription_list`  
**Prompt:** Show me my subscriptions  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.458834 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.407101 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.393662 | `eventgrid_topic_list` | ❌ |
| 4 | 0.391555 | `redis_list` | ❌ |
| 5 | 0.381238 | `postgres_server_list` | ❌ |

---

<<<<<<< HEAD
## Test 446
=======
<<<<<<< HEAD
## Test 436
=======
## Test 446
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `subscription_list`  
**Prompt:** What is my current subscription?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.433242 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.319579 | `marketplace_product_list` | ❌ |
<<<<<<< HEAD
| 3 | 0.315547 | `marketplace_product_get` | ❌ |
| 4 | 0.293009 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.289280 | `eventgrid_topic_list` | ❌ |

---

## Test 447
=======
<<<<<<< HEAD
| 3 | 0.315354 | `marketplace_product_get` | ❌ |
=======
| 3 | 0.315474 | `marketplace_product_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.293772 | `eventgrid_subscription_list` | ❌ |
| 5 | 0.289334 | `eventgrid_topic_list` | ❌ |

---

<<<<<<< HEAD
## Test 437
=======
## Test 447
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `subscription_list`  
**Prompt:** What subscriptions do I have?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.477657 | `subscription_list` | ✅ **EXPECTED** |
| 2 | 0.356775 | `eventgrid_subscription_list` | ❌ |
| 3 | 0.354286 | `marketplace_product_list` | ❌ |
| 4 | 0.344549 | `redis_list` | ❌ |
| 5 | 0.340764 | `eventgrid_topic_list` | ❌ |

---

<<<<<<< HEAD
## Test 448
=======
<<<<<<< HEAD
## Test 438
=======
## Test 448
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Fetch the Azure Terraform best practices  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686886 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.625270 | `deploy_iac_rules_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.605048 | `get_bestpractices_get` | ❌ |
| 4 | 0.482745 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.468390 | `azureaibestpractices_get` | ❌ |

---

## Test 449
=======
| 3 | 0.605599 | `get_bestpractices_get` | ❌ |
| 4 | 0.482936 | `deploy_pipeline_guidance_get` | ❌ |
| 5 | 0.466199 | `deploy_plan_get` | ❌ |

---

<<<<<<< HEAD
## Test 439
=======
## Test 449
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `azureterraformbestpractices_get`  
**Prompt:** Show me the Azure Terraform best practices and generate code sample to get a secret from Azure Key Vault  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.581316 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `get_bestpractices_get` | ❌ |
| 3 | 0.510005 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.473943 | `keyvault_secret_get` | ❌ |
| 5 | 0.451726 | `azureaibestpractices_get` | ❌ |

---

## Test 450
=======
<<<<<<< HEAD
| 1 | 0.581332 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.512141 | `get_bestpractices_get` | ❌ |
=======
| 1 | 0.581316 | `azureterraformbestpractices_get` | ✅ **EXPECTED** |
| 2 | 0.515758 | `get_bestpractices_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.510004 | `deploy_iac_rules_get` | ❌ |
| 4 | 0.473596 | `keyvault_secret_get` | ❌ |
| 5 | 0.444297 | `deploy_pipeline_guidance_get` | ❌ |

---

<<<<<<< HEAD
## Test 440
=======
## Test 450
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `virtualdesktop_hostpool_list`  
**Prompt:** List all host pools in my subscription  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.711905 | `virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
| 2 | 0.659763 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.620665 | `kusto_cluster_list` | ❌ |
| 4 | 0.546744 | `search_service_list` | ❌ |
| 5 | 0.536423 | `virtualdesktop_hostpool_host_user-list` | ❌ |

---

## Test 451
=======
| 1 | 0.711969 | `virtualdesktop_hostpool_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.659763 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.620666 | `kusto_cluster_list` | ❌ |
=======
| 2 | 0.659732 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.620507 | `kusto_cluster_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 4 | 0.548888 | `search_service_list` | ❌ |
| 5 | 0.535777 | `virtualdesktop_hostpool_host_user-list` | ❌ |

---

<<<<<<< HEAD
## Test 441
=======
## Test 451
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `virtualdesktop_hostpool_host_list`  
**Prompt:** List all session hosts in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.727054 | `virtualdesktop_hostpool_host_list` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.715572 | `virtualdesktop_hostpool_host_user-list` | ❌ |
| 3 | 0.573350 | `virtualdesktop_hostpool_list` | ❌ |
=======
| 2 | 0.714553 | `virtualdesktop_hostpool_host_user-list` | ❌ |
=======
| 1 | 0.726933 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 2 | 0.714469 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 3 | 0.573352 | `virtualdesktop_hostpool_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.438659 | `aks_nodepool_get` | ❌ |
| 5 | 0.393721 | `sql_elastic-pool_list` | ❌ |

---

<<<<<<< HEAD
## Test 452
=======
<<<<<<< HEAD
## Test 442
=======
## Test 452
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `virtualdesktop_hostpool_host_user-list`  
**Prompt:** List all user sessions on session host <sessionhost_name> in host pool <hostpool_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
<<<<<<< HEAD
| 1 | 0.813311 | `virtualdesktop_hostpool_host_user-list` | ✅ **EXPECTED** |
| 2 | 0.659213 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.501113 | `virtualdesktop_hostpool_list` | ❌ |
=======
<<<<<<< HEAD
| 1 | 0.812787 | `virtualdesktop_hostpool_host_user-list` | ✅ **EXPECTED** |
| 2 | 0.659212 | `virtualdesktop_hostpool_host_list` | ❌ |
| 3 | 0.501167 | `virtualdesktop_hostpool_list` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.357561 | `aks_nodepool_get` | ❌ |
| 5 | 0.336576 | `monitor_workspace_list` | ❌ |

---

<<<<<<< HEAD
## Test 453
=======
## Test 443
=======
| 1 | 0.812628 | `virtualdesktop_hostpool_sessionhost_usersession-list` | ❌ |
| 2 | 0.658986 | `virtualdesktop_hostpool_sessionhost_list` | ❌ |
| 3 | 0.501050 | `virtualdesktop_hostpool_list` | ❌ |
| 4 | 0.357450 | `aks_nodepool_get` | ❌ |
| 5 | 0.336389 | `monitor_workspace_list` | ❌ |

---

## Test 453
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `workbooks_create`  
**Prompt:** Create a new workbook named <workbook_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.552212 | `workbooks_create` | ✅ **EXPECTED** |
| 2 | 0.417950 | `workbooks_update` | ❌ |
| 3 | 0.361364 | `workbooks_delete` | ❌ |
| 4 | 0.329077 | `workbooks_show` | ❌ |
| 5 | 0.328113 | `workbooks_list` | ❌ |

---

<<<<<<< HEAD
## Test 454
=======
<<<<<<< HEAD
## Test 444
=======
## Test 454
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `workbooks_delete`  
**Prompt:** Delete the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.621310 | `workbooks_delete` | ✅ **EXPECTED** |
| 2 | 0.498506 | `workbooks_show` | ❌ |
| 3 | 0.432454 | `workbooks_create` | ❌ |
<<<<<<< HEAD
| 4 | 0.425569 | `workbooks_list` | ❌ |
=======
<<<<<<< HEAD
| 4 | 0.425484 | `workbooks_list` | ❌ |
=======
| 4 | 0.425569 | `workbooks_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.421897 | `workbooks_update` | ❌ |

---

<<<<<<< HEAD
## Test 455
=======
<<<<<<< HEAD
## Test 445
=======
## Test 455
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `workbooks_list`  
**Prompt:** List all workbooks in my resource group <resource_group_name>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.772404 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.562476 | `workbooks_create` | ❌ |
| 3 | 0.516733 | `grafana_list` | ❌ |
| 4 | 0.493962 | `workbooks_show` | ❌ |
| 5 | 0.488522 | `group_list` | ❌ |

---

<<<<<<< HEAD
## Test 456
=======
<<<<<<< HEAD
## Test 446
=======
## Test 456
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `workbooks_list`  
**Prompt:** What workbooks do I have in resource group <resource_group_name>?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.708612 | `workbooks_list` | ✅ **EXPECTED** |
| 2 | 0.570260 | `workbooks_create` | ❌ |
| 3 | 0.499633 | `workbooks_show` | ❌ |
| 4 | 0.485504 | `workbooks_delete` | ❌ |
| 5 | 0.472378 | `grafana_list` | ❌ |

---

<<<<<<< HEAD
## Test 457
=======
<<<<<<< HEAD
## Test 447
=======
## Test 457
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `workbooks_show`  
**Prompt:** Get information about the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.686095 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.498390 | `workbooks_create` | ❌ |
<<<<<<< HEAD
| 3 | 0.494708 | `workbooks_list` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.494492 | `workbooks_list` | ❌ |
=======
| 3 | 0.494708 | `workbooks_list` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 4 | 0.463156 | `workbooks_update` | ❌ |
| 5 | 0.452348 | `workbooks_delete` | ❌ |

---

<<<<<<< HEAD
## Test 458
=======
<<<<<<< HEAD
## Test 448
=======
## Test 458
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `workbooks_show`  
**Prompt:** Show me the workbook with resource ID <workbook_resource_id>  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.581575 | `workbooks_show` | ✅ **EXPECTED** |
| 2 | 0.500475 | `workbooks_list` | ❌ |
| 3 | 0.468996 | `workbooks_create` | ❌ |
| 4 | 0.466266 | `workbooks_update` | ❌ |
| 5 | 0.455311 | `workbooks_delete` | ❌ |

---

<<<<<<< HEAD
## Test 459
=======
<<<<<<< HEAD
## Test 449
=======
## Test 459
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `workbooks_update`  
**Prompt:** Update the workbook <workbook_resource_id> with a new text step  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.586347 | `workbooks_update` | ✅ **EXPECTED** |
| 2 | 0.382651 | `workbooks_create` | ❌ |
| 3 | 0.349689 | `workbooks_delete` | ❌ |
| 4 | 0.347778 | `workbooks_show` | ❌ |
| 5 | 0.292904 | `loadtesting_testrun_update` | ❌ |

---

<<<<<<< HEAD
## Test 460
=======
<<<<<<< HEAD
## Test 450
=======
## Test 460
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `bicepschema_get`  
**Prompt:** How can I use Bicep to create an Azure OpenAI service?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.543803 | `bicepschema_get` | ✅ **EXPECTED** |
| 2 | 0.485970 | `foundry_models_deploy` | ❌ |
| 3 | 0.485889 | `deploy_iac_rules_get` | ❌ |
<<<<<<< HEAD
| 4 | 0.468898 | `azureaibestpractices_get` | ❌ |
| 5 | 0.453412 | `foundry_openai_embeddings-create` | ❌ |

---

## Test 461
=======
<<<<<<< HEAD
| 4 | 0.453282 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.448373 | `get_bestpractices_get` | ❌ |

---

## Test 451
=======
| 4 | 0.462146 | `foundry_openai_embeddings-create` | ❌ |
| 5 | 0.449694 | `get_bestpractices_get` | ❌ |

---

## Test 461
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Please help me design an architecture for a large-scale file upload, storage, and retrieval service  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.502125 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.290902 | `storage_blob_upload` | ❌ |
<<<<<<< HEAD
| 3 | 0.260101 | `managedlustre_fs_create` | ❌ |
| 4 | 0.254991 | `deploy_architecture_diagram_generate` | ❌ |
=======
<<<<<<< HEAD
| 3 | 0.259162 | `managedlustre_fs_create` | ❌ |
| 4 | 0.254853 | `deploy_architecture_diagram_generate` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)
| 5 | 0.245034 | `managedlustre_fs_subnetsize_validate` | ❌ |

---

<<<<<<< HEAD
## Test 462
=======
## Test 452
=======
| 3 | 0.259162 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.254991 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.245034 | `managedlustre_filesystem_subnetsize_validate` | ❌ |

---

## Test 462
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** Help me design an Azure cloud service that will serve as an ATM for users  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.508153 | `cloudarchitect_design` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.377941 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.341316 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.336385 | `azureaibestpractices_get` | ❌ |
| 5 | 0.328747 | `get_bestpractices_get` | ❌ |

---

## Test 463
=======
<<<<<<< HEAD
| 2 | 0.377584 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.341462 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.328747 | `get_bestpractices_get` | ❌ |
=======
| 2 | 0.377941 | `deploy_architecture_diagram_generate` | ❌ |
| 3 | 0.341462 | `deploy_pipeline_guidance_get` | ❌ |
| 4 | 0.331626 | `get_bestpractices_get` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.321855 | `deploy_plan_get` | ❌ |

---

<<<<<<< HEAD
## Test 453
=======
## Test 463
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** I want to design a cloud app for ordering groceries  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.423577 | `cloudarchitect_design` | ✅ **EXPECTED** |
| 2 | 0.271869 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.265972 | `deploy_architecture_diagram_generate` | ❌ |
| 4 | 0.242581 | `deploy_plan_get` | ❌ |
| 5 | 0.241197 | `azureaibestpractices_get` | ❌ |

---

<<<<<<< HEAD
## Test 464
=======
<<<<<<< HEAD
## Test 454
=======
## Test 464
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

**Expected Tool:** `cloudarchitect_design`  
**Prompt:** How can I design a cloud service in Azure that will store and present videos for users?  

### Results

| Rank | Score | Tool | Status |
|------|-------|------|--------|
| 1 | 0.534690 | `cloudarchitect_design` | ✅ **EXPECTED** |
<<<<<<< HEAD
| 2 | 0.369872 | `deploy_pipeline_guidance_get` | ❌ |
| 3 | 0.357808 | `managedlustre_fs_create` | ❌ |
| 4 | 0.352797 | `deploy_architecture_diagram_generate` | ❌ |
| 5 | 0.324217 | `azureaibestpractices_get` | ❌ |
=======
| 2 | 0.369969 | `deploy_pipeline_guidance_get` | ❌ |
<<<<<<< HEAD
| 3 | 0.356331 | `managedlustre_fs_create` | ❌ |
| 4 | 0.352914 | `deploy_architecture_diagram_generate` | ❌ |
=======
| 3 | 0.356331 | `managedlustre_filesystem_create` | ❌ |
| 4 | 0.352797 | `deploy_architecture_diagram_generate` | ❌ |
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
| 5 | 0.323920 | `storage_blob_upload` | ❌ |
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

---

## Summary

<<<<<<< HEAD
**Total Prompts Tested:** 464  
**Analysis Execution Time:** 186.7791311s  
=======
<<<<<<< HEAD
**Total Prompts Tested:** 454  
**Analysis Execution Time:** 61.2275421s  
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

### Success Rate Metrics

**Top Choice Success:** 92.2% (428/464 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 3.2% (15/464 tests)  
**🎯 High Confidence (≥0.7):** 22.8% (106/464 tests)  
**✅ Good Confidence (≥0.6):** 62.3% (289/464 tests)  
**👍 Fair Confidence (≥0.5):** 92.2% (428/464 tests)  
**👌 Acceptable Confidence (≥0.4):** 99.6% (462/464 tests)  
**❌ Low Confidence (<0.4):** 0.4% (2/464 tests)  

#### Top Choice + Confidence Combinations

<<<<<<< HEAD
**💪 Top Choice + Very High Confidence (≥0.8):** 3.2% (15/464 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 22.8% (106/464 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 60.3% (280/464 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 86.9% (403/464 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 92.2% (428/464 tests)  
=======
**💪 Top Choice + Very High Confidence (≥0.8):** 3.3% (15/454 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 23.3% (106/454 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 60.6% (275/454 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 86.8% (394/454 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 92.1% (418/454 tests)  
=======
**Total Prompts Tested:** 464  
**Analysis Execution Time:** 123.7654249s  

### Success Rate Metrics

**Top Choice Success:** 89.2% (414/464 tests)  

#### Confidence Level Distribution

**💪 Very High Confidence (≥0.8):** 2.6% (12/464 tests)  
**🎯 High Confidence (≥0.7):** 19.6% (91/464 tests)  
**✅ Good Confidence (≥0.6):** 57.8% (268/464 tests)  
**👍 Fair Confidence (≥0.5):** 88.8% (412/464 tests)  
**👌 Acceptable Confidence (≥0.4):** 96.3% (447/464 tests)  
**❌ Low Confidence (<0.4):** 3.7% (17/464 tests)  

#### Top Choice + Confidence Combinations

**💪 Top Choice + Very High Confidence (≥0.8):** 2.6% (12/464 tests)  
**🎯 Top Choice + High Confidence (≥0.7):** 19.6% (91/464 tests)  
**✅ Top Choice + Good Confidence (≥0.6):** 55.8% (259/464 tests)  
**👍 Top Choice + Fair Confidence (≥0.5):** 83.8% (389/464 tests)  
**👌 Top Choice + Acceptable Confidence (≥0.4):** 89.2% (414/464 tests)  
>>>>>>> 84ad4f44 (update prompts and tool description evaluator)
>>>>>>> 58ab8585 (update prompts and tool description evaluator)

### Success Rate Analysis

🟡 **Good** - The tool selection system is performing well.

⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).

